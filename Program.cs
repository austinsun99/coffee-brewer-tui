﻿using Spectre.Console;
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
		brewLog.AddEntry(new BrewEntry("Last Month", DateTime.Now, new string[0], 3000, 4000));
		var entry = new BrewEntry("Last Year", DateTime.Now.AddYears(-1).AddDays(5), new string[0], 30, 40);
		brewLog.AddEntry(entry);
		MainPanel.DrawTimerFrame(brewLog, entry);
	}
}
