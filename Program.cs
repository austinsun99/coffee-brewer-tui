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
		brewLog.AddEntry(new BrewEntry("Now", DateTime.Now, 30, new Topic[0]));
		brewLog.AddEntry(new BrewEntry("Today", DateTime.Now.Date, 30, new Topic[0]));
		brewLog.AddEntry(new BrewEntry("Yesterday", DateTime.Now.AddDays(-1), 30, new Topic[0]));
		brewLog.AddEntry(new BrewEntry("Last Week", DateTime.Now.AddDays(-7.01), 30, new Topic[0]));
		brewLog.AddEntry(new BrewEntry("Last Month", DateTime.Now.AddMonths(-1), 30, new Topic[0]));
		brewLog.AddEntry(new BrewEntry("Last Year", DateTime.Now.AddYears(-1), 30, new Topic[0]));
		MainPanel.DrawFrame(brewLog);
	}
}
