using Spectre.Console;
using CoffeeBrewer;

namespace Brew;

public static class Prompts
{
    private const string ADD_NEW_TOPICS_CHOICE_TEXT = "[red]Add new topics[/]";

    public static void PromptAndAddNewEntry(BrewLog brewLog)
	{

		AnsiConsole.Clear();
		AnsiConsole.Write(new Rule("[purple]Coffee Brewer[/]"));

		List<string> topicsToDisplay = new List<string> { ADD_NEW_TOPICS_CHOICE_TEXT };
		topicsToDisplay.AddRange(brewLog.Topics.ToList().Select(t => t.name));

		string entryName = AnsiConsole.Prompt(new TextPrompt<string>("Enter the name of the activity:"));
		List<string> topics = AnsiConsole.Prompt(
			new MultiSelectionPrompt<string>()
				.Title("Tags")
				.Required()
				.PageSize(10)
				.MoreChoicesText("[grey](Move up and down to reveal more fruits)[/]")
				.InstructionsText(
					"[grey](Press [blue]<space>[/] to toggle a tag, " + 
					"[green]<enter>[/] to accept)[/]")
				.AddChoices(topicsToDisplay.ToArray()));

		if (topics.Remove(ADD_NEW_TOPICS_CHOICE_TEXT)) {
			List<Topic> addedTopics = PromptAddTopics().ToList();

			List<string> duplicateTopics = new List<string>();
			foreach (Topic t in addedTopics) {
				bool added = brewLog.Topics.Add(t);
				if (!added) {
					duplicateTopics.Add(t.name);
					addedTopics.Remove(t);
				}
			}
			topics.AddRange(addedTopics.Select(t => t.name));
			if (duplicateTopics.Count != 0) AnsiConsole.MarkupLineInterpolated($"Duplicate topics: [blue]{string.Join(" ", duplicateTopics)}[/]");
		}

		AnsiConsole.MarkupLineInterpolated($"You've selected the following tags: [blue]{string.Join(" ", topics)}[/]");

		int length = AnsiConsole.Prompt(new TextPrompt<int>("How long would you like to do this activity (in minutes):"));// * 60;
		Program.mainPanel.DrawTimerFrame(brewLog, new BrewEntry(entryName, DateTime.Now, length, brewLog.GetTopicsFromNames(topics)));
	}

	public static Topic[] PromptAddTopics()
	{

		List<Topic> topics = new List<Topic>();
		string topicsPrompt = 
			AnsiConsole.Prompt(new TextPrompt<string>("Create new tags (Separate tags with a space):"));

		foreach (string topic in topicsPrompt.Split(" ")) {
			topics.Add(new Topic(topic));
		}

		return topics.ToArray();
	}

}
