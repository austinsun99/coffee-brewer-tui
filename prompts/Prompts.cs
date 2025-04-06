using Spectre.Console;
using UI;

namespace Brew;

public static class Prompts
{
	private const string ADD_NEW_TOPICS_CHOICE_TEXT = "[red]Add new topics[/]";

	public static void PromptAndAddNewEntry(BrewLog brewLog)
	{
		AnsiConsole.Clear();
		AnsiConsole.Write(new Rule("[purple]Coffee Brewer[/]"));

		List<string> topicsToDisplay = new List<string> { ADD_NEW_TOPICS_CHOICE_TEXT };
		topicsToDisplay.AddRange(brewLog.Topics);

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

			
		List<string> addedTopics = PromptAddTopics();
		if (topics.Remove(ADD_NEW_TOPICS_CHOICE_TEXT))
		{
			List<string> duplicateTopics = new List<string>();

			foreach (string t in addedTopics)
			{
				if (!brewLog.Topics.Add(t))
				{
					duplicateTopics.Add(t);
					addedTopics.Remove(t);
				}
			}
			topics.AddRange(addedTopics);
			if (duplicateTopics.Count != 0)
				AnsiConsole.MarkupLineInterpolated($"Duplicate topics: [blue]{string.Join(" ", duplicateTopics)}[/]");
		}

		AnsiConsole.MarkupLineInterpolated($"You've selected the following tags: [blue]{string.Join(" ", topics)}[/]");

		int length = AnsiConsole.Prompt(new TextPrompt<int>("How long would you like to do this activity (in minutes):")) * 60;
		MainPanel.DrawTimerFrame(brewLog, new BrewEntry(entryName, DateTime.Now, addedTopics.ToArray(), length, 0));
	}

	public static List<string> PromptAddTopics()
	{
		var topics = new List<string>();
		string topicsPrompt =
			AnsiConsole.Prompt(new TextPrompt<string>("Create new tags (Separate tags with a space):"));

		foreach (string topic in topicsPrompt.Split(" "))
		{
			topics.Add(topic);
		}

		return topics;
	}

	public static List<string> PromptSelectTopics(BrewLog brewLog)
	{
		var topicNames = AnsiConsole.Prompt(
		new MultiSelectionPrompt<string>()
			.Title("Select the [blue]topics[/] to filter by, or select none to select all")
			.NotRequired() // Not required to have a favorite fruit
			.PageSize(10)
			.MoreChoicesText("[grey](Move up and down to reveal more fruits)[/]")
			.InstructionsText(
				"[grey](Press [blue]<space>[/] to toggle a fruit, " +
				"[green]<enter>[/] to accept)[/]")
			.AddChoices(brewLog.Topics));
		return topicNames;
	}

}
