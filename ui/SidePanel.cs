using Spectre.Console;
using System.Text;
using Brew;
using CoffeeBrewer;

namespace UI;

public class SidePanel
{

	public Layout GetSidePanel(BrewLog brewLog)
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

		foreach (BrewEntry entry in brewLog.SortedEntries)
		{
			logTable.AddRow((StudyLogPanel(entry)));
		}

		sidePanel["Root"].Update(new Align(logTable, HorizontalAlignment.Center));
		return sidePanel;

	}

	private Panel StudyLogPanel(BrewEntry entry)
	{
		StringBuilder panelInfo = new StringBuilder();

		string topics = string.Join(" ", entry.topics.Select(t => $"[[{t.name}]]"));

		panelInfo.AppendLine($"[blue]{topics}[/]");
		panelInfo.AppendLine(entry.GetLengthFormatted());

		var panel = new Panel(panelInfo.ToString());
		panel.Header = new PanelHeader(entry.name);

		return panel;
	}

}

public class MainPanel()
{

	public void DrawFrame(BrewLog brewLog) => AnsiConsole.Write(GenerateBaseMainLayout(brewLog));
	public void DrawTimerFrame(BrewLog brewLog, BrewEntry entryToDisplay)
	{

		Layout mainLayout = GenerateBaseMainLayout(brewLog);

		Panel coffeeInfo = new Panel("New Coffee");
		mainLayout["Top"].Update(coffeeInfo);

		AnsiConsole.Live(mainLayout)
			.Start(ctx =>
			{

				int secondsUntilActivityFinishes = entryToDisplay.lengthSeconds;

				var activityTimeStopwatch = new System.Diagnostics.Stopwatch();
				activityTimeStopwatch.Start();

				Task.Factory.StartNew(() =>
				{
					while (true)
					{
						var keyPressed = Console.ReadKey();
						switch (keyPressed.Key) {
							case ConsoleKey.E:
								secondsUntilActivityFinishes += 10;
								break;
								
						}
					}
				});

				const int TIME_BETWEEN_REFRESH_MS = 1000;

				while (activityTimeStopwatch.Elapsed.TotalSeconds < secondsUntilActivityFinishes)
				{
					int timeLeft = secondsUntilActivityFinishes - (int)activityTimeStopwatch.Elapsed.TotalSeconds;
					coffeeInfo = new Panel($"{timeLeft / 60} minutes, {timeLeft % 60} seconds left until coffee is done brewing");
					mainLayout["Top"].Update(coffeeInfo);

					ctx.Refresh();
					Thread.Sleep(TIME_BETWEEN_REFRESH_MS);
				}

				activityTimeStopwatch.Stop();
				TimeSpan activityTotalTime = activityTimeStopwatch.Elapsed;
				
				entryToDisplay.lengthSeconds = (int)activityTotalTime.TotalSeconds;
				brewLog.AddEntry(entryToDisplay);
				mainLayout["Side"].Update(new SidePanel().GetSidePanel(brewLog));
				ctx.Refresh();
			});
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
				new Layout("Image"),
				new Layout("Bottom")
				);
		mainLayout["Top"].Ratio(1);
		mainLayout["Bottom"].Ratio(1);
		mainLayout["Image"].Ratio(5);

		mainLayout["Side"].Update(new SidePanel().GetSidePanel(brewLog));

		Canvas coffeeDrawing = new Canvas(32, 32);
		DrawCoffee(coffeeDrawing, ((mainLayout.Size ?? 0 / 2), 0));
		mainLayout["Image"].Update(Align.Center(coffeeDrawing));

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
