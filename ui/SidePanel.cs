using Spectre.Console;
using System.Text;
using Brew;
using CoffeeBrewer;

namespace UI;

public class SidePanel
{

	public Layout GetSidePanel(BrewLog brewLog, int selectedIndex)
	{
		Layout sidePanel = new Layout("Root");

		string timeDisplay = $"You spent [red]{brewLog.GetTimeForAllEntriesFormatted()}[/] working today!";
		Panel topDisplayPanel = new Panel(timeDisplay);

		Table logTable = new Table();
		logTable.AddColumn(new TableColumn("Assignments"));
		logTable.Border(TableBorder.None);
		logTable.HideHeaders();

		logTable.AddRow(topDisplayPanel);
		logTable.AddEmptyRow();

		for (int i = 0; i < brewLog.SortedEntries.Count(); i++)
		{
			logTable.AddRow((StudyLogPanel(brewLog.SortedEntries[i], i == selectedIndex)));
		}

		sidePanel["Root"].Update(new Align(logTable, HorizontalAlignment.Center));
		return sidePanel;

	}

	private Panel StudyLogPanel(BrewEntry entry, bool selected = false)
	{
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

public class MainPanel()
{

	public void DrawFrame(BrewLog brewLog)
	{

		Layout mainLayout = GenerateBaseMainLayout(brewLog);
		AnsiConsole.Live(mainLayout).Start(ctx =>
		{
			bool listenToKeypresses = true;
			int selectedIndex = 0;
			mainLayout["Side"].Update(new SidePanel().GetSidePanel(brewLog, selectedIndex));
			ctx.Refresh();
			while (listenToKeypresses)
			{
				var keyPressed = Console.ReadKey(true);
				switch (keyPressed.Key)
				{
					case ConsoleKey.J:
						selectedIndex++;
						if (selectedIndex >= brewLog.Entries.Count()) selectedIndex = 0;
						break;
					case ConsoleKey.K:
						selectedIndex--;
						if (selectedIndex < 0) selectedIndex = brewLog.Entries.Count() - 1;
						break;
					case ConsoleKey.E:
						Environment.Exit(0);
						break;
					case ConsoleKey.A:
						listenToKeypresses = false;
						break;
					default:
						AnsiConsole.Clear();
						break;
				}
				mainLayout["Side"].Update(new SidePanel().GetSidePanel(brewLog, selectedIndex));
				ctx.Refresh();
			}
		});

		Prompts.PromptAndAddNewEntry(brewLog);

	}

	public void DrawTimerFrame(BrewLog brewLog, BrewEntry entryToDisplay)
	{

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

				const int TIME_BETWEEN_REFRESH_MS = 1000;

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
				brewLog.AddEntry(entryToDisplay);
			});
		Program.mainPanel.DrawFrame(brewLog);
	}

	private Panel GetCoffeeInformationDisplay(BrewEntry entryToDisplay, int timeLeft)
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

	private Panel GetKeybindingsInformationDisplay()
	{
		var panelText =
			new StringBuilder()
			.AppendLine("i: increment the timer by 10 seconds")
			.AppendLine("s: you have finished the assignment early");
		return new Panel(panelText.ToString());
	}

	private Layout GenerateBaseMainLayout(BrewLog brewLog)
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

		mainLayout["Side"].Update(new SidePanel().GetSidePanel(brewLog, -1));

		//Canvas coffeeDrawing = new Canvas(32, 32);
		//DrawCoffee(coffeeDrawing, ((mainLayout.Size ?? 0 / 2), 0));
		//mainLayout["Image"].Update(Align.Center(coffeeDrawing));

		return mainLayout;
	}

	static void DrawCoffee(Canvas canvas, (int, int) padding)
	{
		foreach ((int x, int y) in CoffeeImage.coffeeImage.Keys)
		{
			canvas.SetPixel(x, y, CoffeeImage.coffeeImage[(x + padding.Item1, y + padding.Item2)]);
		}
	}
}
