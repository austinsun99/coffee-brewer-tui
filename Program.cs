using Spectre.Console;
using UI;

using Brew;

namespace CoffeeBrewer;

class Program
{
	static void Main(string[] args)
	{
		AnsiConsole.Clear();
		//BrewLog brewLog = SaveLoad.LoadBrewLog();
		BrewLog brewLog = new BrewLog();
		brewLog.AddEntry(new BrewEntry("Last Month", DateTime.Now.AddMonths(-1).AddDays(10), 30, new Topic[0]));
		brewLog.AddEntry(new BrewEntry("Last Year", DateTime.Now.AddYears(-1).AddDays(5), 30, new Topic[0]));
		brewLog.AddEntry(new BrewEntry("Last Year", DateTime.Now.AddYears(-1).AddDays(5), 30, new Topic[0]));
		brewLog.AddEntry(new BrewEntry("Last Year", DateTime.Now.AddYears(-1).AddDays(5), 30, new Topic[0]));
		brewLog.AddEntry(new BrewEntry("Last Year", DateTime.Now.AddYears(-1).AddDays(5), 30, new Topic[0]));
		brewLog.AddEntry(new BrewEntry("Last Year", DateTime.Now.AddYears(-1).AddDays(5), 30, new Topic[0]));
		MainPanel.DrawTimerFrame(brewLog, new BrewEntry("Math Homework", DateTime.Now, 300, new Topic[] { new Topic("Math"), new Topic("Homework") }));
	}
}
