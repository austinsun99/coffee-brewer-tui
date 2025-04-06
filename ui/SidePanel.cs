using Spectre.Console;
using System.Text;
using Brew;

namespace UI;

public static class SidePanel
{

	public static Layout GetSidePanel(BrewLog brewLog, int selectedIndex, TimeInterval interval, Topic[] selectedTopics)
	{
		Layout sidePanel = new Layout("Root");

		string timeDisplay =
			brewLog.GetEntriesWithTopicAndDateRangeFormatted(selectedTopics, interval);
		Panel topDisplayPanel = new Panel(timeDisplay);

		Table logTable = new Table();
		logTable.AddColumn(new TableColumn("Assignments"));
		logTable.Border(TableBorder.None);
		logTable.HideHeaders();

		logTable.AddRow(topDisplayPanel);
		logTable.AddEmptyRow();

		var filteredEntries = brewLog.GetEntriesWithTopicAndDateInterval(selectedTopics, interval);
		for (int i = 0; i < filteredEntries.Count(); i++)
		{
			logTable.AddRow((StudyLogPanel(filteredEntries[i], i == selectedIndex)));
		}

		sidePanel["Root"].Update(new Align(logTable, HorizontalAlignment.Center));
		return sidePanel;

	}

	private static Panel StudyLogPanel(BrewEntry entry, bool selected = false)
	{

		string topics = string.Join(" ", entry.topics.Select(t => $"[[{t.name}]]"));
		string entryTime = entry.GetLengthFormatted();

		topics += string.Join(" ", new string[Math.Max(0, entry.name.Length + 1 - topics.Length)]);
		entryTime += string.Join(" ", new string[Math.Max(0, entry.name.Length + 1 - entryTime.Length)]);

		var panelInfo = new StringBuilder();
		if (entry.topics.Length != 0) panelInfo.AppendLine($"[blue]{topics}[/]");

		panelInfo.AppendLine(entryTime);

		var panel = new Panel(panelInfo.ToString());
		panel.Header = new PanelHeader(entry.name);

		if (selected) panel.BorderColor(Color.Red);

		return panel;
	}

}

public static class MainPanel
{
	static TimeInterval interval;
	static Topic[] selectedTopics = new Topic[] { };

	public static void DrawFrame(BrewLog brewLog)
	{
		Console.CancelKeyPress += new ConsoleCancelEventHandler((
			object? sender,
			ConsoleCancelEventArgs args) => Save(brewLog));

		bool listenToKeypresses = true;
		var escapeFlag = DrawFrameEscapeFlag.None;

		Layout mainLayout = GenerateBaseMainLayout(brewLog);
		mainLayout["Top"].Update(GetKeybindingsInformationDisplay());
		mainLayout["Image"].Update(new FigletText("Coffee Brewer.").Centered().Color(Color.SandyBrown));
		AnsiConsole.Live(mainLayout).Start(ctx =>
		{
			int selectedIndex = 0;
			mainLayout["Side"].Update(SidePanel.GetSidePanel(brewLog, selectedIndex, interval, selectedTopics));
			ctx.Refresh();
			while (listenToKeypresses)
			{
				var keyPressed = Console.ReadKey(true);
				int numEntriesToShow = brewLog.GetEntriesWithTopicAndDateInterval(selectedTopics, interval).Count();
				switch (keyPressed.Key)
				{
					case ConsoleKey.J:
						selectedIndex++;
						if (selectedIndex >= numEntriesToShow) selectedIndex = 0;
						break;
					case ConsoleKey.K:
						selectedIndex--;
						if (selectedIndex < 0) selectedIndex = numEntriesToShow;
						break;
					case ConsoleKey.E:
						Save(brewLog);
						break;
					case ConsoleKey.A:
						escapeFlag = DrawFrameEscapeFlag.AddEntry;
						listenToKeypresses = false;
						break;
					case ConsoleKey.B:
						escapeFlag = DrawFrameEscapeFlag.FilterByTopic;
						listenToKeypresses = false;
						break;
					case ConsoleKey.X:
						if (brewLog.Entries.Count() == 0) break;
						brewLog.Entries.Remove(brewLog.SortedEntries[selectedIndex]);
						if (selectedIndex >= brewLog.SortedEntries.Count()) selectedIndex--;
						break;
					case ConsoleKey.F:
						interval++;
						if ((int)interval >= Enum.GetNames(typeof(TimeInterval)).Length) interval = 0;
						break;
					default:
						AnsiConsole.Clear();
						break;
				}
				mainLayout["Side"].Update(SidePanel.GetSidePanel(brewLog, selectedIndex, interval, selectedTopics));
				ctx.Refresh();
			}
		});

		switch (escapeFlag)
		{
			case DrawFrameEscapeFlag.AddEntry:
				Prompts.PromptAndAddNewEntry(brewLog);
				break;
			case DrawFrameEscapeFlag.FilterByTopic:
				AnsiConsole.Clear();
				selectedTopics = Prompts.PromptSelectTopics(brewLog);
				DrawFrame(brewLog);
				break;
			default:
				break;
		}

	}

	public static void DrawTimerFrame(BrewLog brewLog, BrewEntry entryToDisplay)
	{

		Console.CancelKeyPress += new ConsoleCancelEventHandler((
			object? sender,
			ConsoleCancelEventArgs args) => Save(brewLog));

		const int TIME_BETWEEN_REFRESH_MS = 1000;
		Layout mainLayout = GenerateBaseMainLayout(brewLog);

		Panel keybindingsInfo = GetKeybindingsInformationDisplayTimer();

		mainLayout["Top"]
				.Update(keybindingsInfo);

		mainLayout["Image"]
			.Ratio(5)
			.SplitRows(
					new Layout("CoffeeInfoDisplay"),
					new Layout("BrewingText"),
					new Layout("CoffeeImage"),
					new Layout("CoffeeTimer"));

		mainLayout["CoffeeInfoDisplay"].Ratio(1)
			.Update(Align.Center(GetCoffeeInformationPanel(entryToDisplay), VerticalAlignment.Middle));
		mainLayout["CoffeeImage"].Ratio(5);
		mainLayout["BrewingText"].Ratio(3);
		mainLayout["CoffeeTimer"].Ratio(1);

		AnsiConsole.Live(mainLayout)
			.Start(ctx =>
			{

				int brewDots = 1;
				int secondsUntilActivityFinishes = entryToDisplay.lengthSeconds;
				var activityTimeStopwatch = new System.Diagnostics.Stopwatch();
				activityTimeStopwatch.Start();

				var tokenSource = new CancellationTokenSource();

				Func<int> GetTimeLeft = ()
					=> secondsUntilActivityFinishes - (int)activityTimeStopwatch.Elapsed.TotalSeconds;

				Action UpdateTimeLeft = ()
				=>
				{
					string timerText = $"{GetTimeLeft() / 60} minutes, {GetTimeLeft() % 60} seconds left until coffee is done brewing";
					mainLayout["CoffeeTimer"].Update(
							Align.Center(new Panel(timerText), VerticalAlignment.Middle));
				};

				Task t = Task.Factory.StartNew(() =>
				{
					while (secondsUntilActivityFinishes > 0)
					{
						tokenSource.Token.ThrowIfCancellationRequested();
						var keyPressed = Console.ReadKey(true);
						switch (keyPressed.Key)
						{
							case ConsoleKey.I:
								secondsUntilActivityFinishes += 10;
								break;
							case ConsoleKey.S:
								secondsUntilActivityFinishes = 0;
								break;
						}
						UpdateTimeLeft();
						ctx.Refresh();
					}
				}, tokenSource.Token);

				while (activityTimeStopwatch.Elapsed.TotalSeconds < secondsUntilActivityFinishes)
				{
					mainLayout["CoffeeImage"].Update(Align.Center(CoffeeImage.CoffeeASCII()));

					string brewingText = "Brewing" + new string('.', brewDots);
					mainLayout["BrewingText"].Update(Align.Center(
								new FigletText(brewingText).Color(Color.RosyBrown)));
					brewDots++;
					if (brewDots >= 4) brewDots = 1;

					UpdateTimeLeft();

					ctx.Refresh();
					Thread.Sleep(TIME_BETWEEN_REFRESH_MS);
				}

				tokenSource.Cancel();
				tokenSource.Dispose();
				activityTimeStopwatch.Stop();
				TimeSpan activityTotalTime = activityTimeStopwatch.Elapsed;

				entryToDisplay.lengthSeconds = (int)activityTotalTime.TotalSeconds;
			});

		brewLog.AddEntry(entryToDisplay);
		DrawFrame(brewLog);
	}

	private static Panel GetCoffeeInformationPanel(BrewEntry entryToDisplay)
	{
		string topicsFormatted = string.Join(" ", entryToDisplay.topics.Select(t => $"[[{t.name}]]"));
		string ingredientsFormatted = entryToDisplay.topics.Length == 0 ?
			" " : $"Ingredients in coffee: {topicsFormatted}";

		var panelText =
			new StringBuilder()
			.AppendLine()
			.AppendFormat("{0}", entryToDisplay.name);

		if (entryToDisplay.topics.Length != 0)
		{
			panelText
				.AppendLine()
				.Append("[blue]").AppendFormat(ingredientsFormatted).Append("[/]");
		}

		Panel coffeeInfo = new Panel(panelText.ToString());
		coffeeInfo.Border = BoxBorder.None;
		return coffeeInfo;
	}

	private static Panel GetKeybindingsInformationDisplayTimer()
	{
		var panelText = new StringBuilder();

		Action<string, string> AddKeybinding = (string keys, string description) =>
		{
			panelText.AppendLine($"[purple]{keys}[/]: {description}");
		};

		AddKeybinding("i", "increment the timer by 10 seconds");
		AddKeybinding("s", "you have finished the assignment early"); ;

		var panel = new Panel(Align.Left(
			new Markup(panelText.ToString()),
			VerticalAlignment.Bottom));
		panel.Border = BoxBorder.None;
		panel.Expand = true;
		return panel;
	}

	private static Panel GetKeybindingsInformationDisplay()
	{
		var panelText = new StringBuilder();

		Action<string, string> AddKeybinding = (string keys, string description) =>
		{
			panelText.AppendLine($"[purple]{keys}[/]: {description}");
		};

		AddKeybinding("j, k", "scroll (up, down) between the entries on the side panel");
		AddKeybinding("a", "add an entry");
		AddKeybinding("e", "save and exit");
		AddKeybinding("b", "filter by topic");
		AddKeybinding("x", "delete selected entry");
		AddKeybinding("f", "cycle between filtered time intervals"); ;

		var panel = new Panel(Align.Left(
			new Markup(panelText.ToString()),
			VerticalAlignment.Bottom));
		panel.Border = BoxBorder.None;
		panel.Expand = true;
		return panel;

	}

	private static Layout GenerateBaseMainLayout(BrewLog brewLog)
	{

		Layout mainLayout = new Layout("Root");

		mainLayout.SplitColumns(
			new Layout("Main"),
			new Layout("Side"));
		mainLayout["Side"].Ratio(1);
		mainLayout["Main"].Ratio(2);

		mainLayout["Main"].SplitRows(
				new Layout("Image"),
				new Layout("Top")
				);
		mainLayout["Top"].Ratio(1);
		mainLayout["Image"].Ratio(4);

		mainLayout["Side"].Update(SidePanel.GetSidePanel(brewLog, -1, interval, new Topic[] { }));

		return mainLayout;
	}

	private static void Save(BrewLog brewLog)
	{
		SaveLoad.SaveBrewLog(brewLog);
		Environment.Exit(0);
	}
	private enum DrawFrameEscapeFlag
	{
		None,
		AddEntry,
		FilterByTopic,
	}
}
