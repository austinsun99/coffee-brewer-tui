using Spectre.Console;
using UI;

using Brew;

namespace CoffeeBrewer;

class Program
{

	static BrewLog brewLog = new BrewLog();
	static bool applicationIsOpen;
	public static MainPanel mainPanel = new MainPanel();

	static void Main(string[] args)
	{

		AnsiConsole.Clear();

		Topic math = new Topic("Math");
		Topic studying = new Topic("Studying");
		Topic rust = new Topic("Rust");
		Topic piano = new Topic("Piano");
		Topic cs = new Topic("Computer Science");

		BrewEntry csProject = new BrewEntry("CS Project", DateTime.Now, 2400, new Topic[] { cs, studying });

		brewLog.AddEntry(new BrewEntry("Integral Calculus", DateTime.Now, 2400, new Topic[] { math, studying }));
		brewLog.AddEntry(new BrewEntry("Rust lifetimes", DateTime.Now, 4800, new Topic[] { rust, studying }));
		brewLog.AddEntry(new BrewEntry("Piano", DateTime.Now, 3400, new Topic[] { piano }));
		brewLog.AddEntry(csProject);

		mainPanel.DrawFrame(brewLog);
	}
}
