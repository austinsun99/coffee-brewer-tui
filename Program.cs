using Spectre.Console;
using UI;

using Brew;

namespace CoffeeBrewer;

class Program
{

	static BrewLog brewLog = new BrewLog();
	static bool applicationIsOpen;

	static void Main(string[] args)
	{

		AnsiConsole.Clear();

		Topic math = new Topic("Math");
		Topic studying = new Topic("Studying");
		Topic rust = new Topic("Rust");
		Topic piano = new Topic("Piano");
		Topic cs = new Topic("Computer Science");

		BrewEntry csProject = new BrewEntry("CS Project", DateTime.Now, 20, new Topic[] { cs, studying });

		brewLog.AddEntry(new BrewEntry("Integral Calculus", DateTime.Now, 20, new Topic[] { math, studying }));
		brewLog.AddEntry(new BrewEntry("Rust lifetimes", DateTime.Now, 30, new Topic[] { rust, studying }));
		brewLog.AddEntry(new BrewEntry("Piano", DateTime.Now, 50, new Topic[] { piano }));
		brewLog.AddEntry(csProject);


		MainPanel mainPanel = new MainPanel();
		mainPanel.DrawTimerFrame(brewLog, csProject);

		if (args == null || args.Length == 0) ;// mainPanel.DrawFrame(brewLog);
		else
			switch (args[0])
			{
				case "add":
					BrewEntry entry = PromptEntry();
					mainPanel.DrawTimerFrame(brewLog, entry);
					break;
			}
		Console.ReadKey();

	}

	static BrewEntry PromptEntry()
	{
		BrewEntry entry = Prompts.PromptBrewEntry(brewLog);
		return entry;
	}

}
