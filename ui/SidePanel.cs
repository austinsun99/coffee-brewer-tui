using Spectre.Console;
using System.Text;
using Brew;
using CoffeeBrewer;

namespace UI;

public static class SidePanel
{

	public static Layout GetSidePanel(BrewLog brewLog, int selectedIndex, TimeInterval interval)
	{
		Layout sidePanel = new Layout("Root");

		string timeDisplay =
			brewLog.GetEntriesWithTopicAndDateRangeFormatted(brewLog.Topics.ToArray(), interval);
		Panel topDisplayPanel = new Panel(timeDisplay);

		Table logTable = new Table();
		logTable.AddColumn(new TableColumn("Assignments"));
		logTable.Border(TableBorder.None);
		logTable.HideHeaders();

		logTable.AddRow(topDisplayPanel);
		logTable.AddEmptyRow();

		var filteredEntries = brewLog.GetEntriesWithTopicAndDateInterval(brewLog.Topics.ToArray(), interval);
		for (int i = 0; i < filteredEntries.Count(); i++)
		{
			logTable.AddRow((StudyLogPanel(filteredEntries[i], i == selectedIndex)));
		}

		sidePanel["Root"].Update(new Align(logTable, HorizontalAlignment.Center));
		return sidePanel;

	}

	private static Panel StudyLogPanel(BrewEntry entry, bool selected = false)
	{

		//TODO: Fix header being cut off
		string topics = string.Join(" ", entry.topics.Select(t => $"[[{t.name}]]"));
		StringBuilder panelInfo = new StringBuilder()
			.AppendLine($"[blue]{topics}[/]")
			.AppendLine(entry.GetLengthFormatted());

		var panel = new Panel(panelInfo.ToString());
		panel.Header = new PanelHeader(entry.name);

		if (selected) panel.BorderColor(Color.Red);

		return panel;
	}

}

public static class MainPanel
{
	static TimeInterval interval;
	public static void DrawFrame(BrewLog brewLog)
	{
		Console.CancelKeyPress += new ConsoleCancelEventHandler((
			object? sender,
			ConsoleCancelEventArgs args) => Save(brewLog));

		Layout mainLayout = GenerateBaseMainLayout(brewLog);
		AnsiConsole.Live(mainLayout).Start(ctx =>
		{
			bool listenToKeypresses = true;
			int selectedIndex = 0;
			mainLayout["Side"].Update(SidePanel.GetSidePanel(brewLog, selectedIndex, interval));
			ctx.Refresh();
			while (listenToKeypresses)
			{
				var keyPressed = Console.ReadKey(true);
				switch (keyPressed.Key)
				{
					case ConsoleKey.J:
						selectedIndex++;
						int v = brewLog.GetEntriesWithTopicAndDateInterval(brewLog.Topics.ToArray(), interval).Count();
						if (selectedIndex >= v) selectedIndex = 0;
						break;
					case ConsoleKey.K:
						selectedIndex--;
						if (selectedIndex < 0) selectedIndex = brewLog.Entries.Count() - 1;
						break;
					case ConsoleKey.E:
						Save(brewLog);
						break;
					case ConsoleKey.A:
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
				mainLayout["Side"].Update(SidePanel.GetSidePanel(brewLog, selectedIndex, interval));
				ctx.Refresh();
			}
		});

		Prompts.PromptAndAddNewEntry(brewLog);

	}

	public static void DrawTimerFrame(BrewLog brewLog, BrewEntry entryToDisplay)
	{

		Console.CancelKeyPress += new ConsoleCancelEventHandler((
			object? sender,
			ConsoleCancelEventArgs args) => Save(brewLog));

		const int TIME_BETWEEN_REFRESH_MS = 1000;
		Layout mainLayout = GenerateBaseMainLayout(brewLog);

		Panel coffeeInfo = GetCoffeeInformationDisplay(entryToDisplay, entryToDisplay.lengthSeconds);
		Panel keybindingsInfo = GetKeybindingsInformationDisplay();

		mainLayout["Top"]
			.SplitColumns(
				new Layout("CoffeeInfoDisplay")
				.Ratio(2)
				.Update(coffeeInfo),
				new Layout("KeybindingsInfoDisplay")
				.Ratio(1)
				.Update(keybindingsInfo));

		AnsiConsole.Live(mainLayout)
			.Start(ctx =>
			{

				int secondsUntilActivityFinishes = entryToDisplay.lengthSeconds;
				var activityTimeStopwatch = new System.Diagnostics.Stopwatch();
				activityTimeStopwatch.Start();

				var tokenSource = new CancellationTokenSource();

				Func<int> GetTimeLeft = ()
					=> secondsUntilActivityFinishes - (int)activityTimeStopwatch.Elapsed.TotalSeconds;

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
						coffeeInfo = GetCoffeeInformationDisplay(entryToDisplay, GetTimeLeft());
						mainLayout["CoffeeInfoDisplay"].Update(coffeeInfo);
						ctx.Refresh();
					}
				}, tokenSource.Token);

				//TODO: better formatting

				while (activityTimeStopwatch.Elapsed.TotalSeconds < secondsUntilActivityFinishes)
				{
					coffeeInfo = GetCoffeeInformationDisplay(entryToDisplay, GetTimeLeft());
					mainLayout["CoffeeInfoDisplay"].Update(coffeeInfo);
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

	private static Panel GetCoffeeInformationDisplay(BrewEntry entryToDisplay, int timeLeft)
	{
		string topicsFormatted = string.Join(" ", entryToDisplay.topics.Select(t => $"[[{t.name}]]"));
		var panelText =
			new StringBuilder()
			.AppendLine()
			.AppendFormat("Currently Brewing: {0}", entryToDisplay.name)
			.AppendLine()
			.Append("[blue]").AppendFormat("Ingredients in coffee: {0}", topicsFormatted).Append("[/]")
			.AppendLine()
			.AppendLine($"{timeLeft / 60} minutes, {timeLeft % 60} seconds left until coffee is done brewing");
		Panel coffeeInfo = new Panel(panelText.ToString());
		coffeeInfo.Border = BoxBorder.None;
		return coffeeInfo;
	}

	private static Panel GetKeybindingsInformationDisplay()
	{
		var panelText =
			new StringBuilder()
			.AppendLine("i: increment the timer by 10 seconds")
			.AppendLine("s: you have finished the assignment early");
		return new Panel(panelText.ToString());
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
				new Layout("Top"),
				new Layout("Image")
				);
		mainLayout["Top"].Ratio(2);
		mainLayout["Image"].Ratio(5);

		mainLayout["Side"].Update(SidePanel.GetSidePanel(brewLog, -1, interval));

		//Canvas coffeeDrawing = new Canvas(32, 32);
		//DrawCoffee(coffeeDrawing, ((mainLayout.Size ?? 0 / 2), 0));
		//mainLayout["Image"].Update(Align.Center(coffeeDrawing));

		return mainLayout;
	}

	private static void Save(BrewLog brewLog)
	{
		SaveLoad.SaveBrewLog(brewLog);
		Environment.Exit(0);
	}

	static void DrawCoffee(Canvas canvas, (int, int) padding)
	{
		foreach ((int x, int y) in CoffeeImage.coffeeImage.Keys)
		{
			canvas.SetPixel(x, y, CoffeeImage.coffeeImage[(x + padding.Item1, y + padding.Item2)]);
		}
	}
}
