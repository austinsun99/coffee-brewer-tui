using System.Text.Json.Serialization;
namespace Brew;

public class BrewLog
{
	private HashSet<Topic> topics;
	public HashSet<Topic> Topics { get => topics; set => topics = value; }

	private List<BrewEntry> entries;
	public List<BrewEntry> Entries { get => entries; set => entries = value; }

	[JsonIgnore]
	public List<BrewEntry> SortedEntries
	{
		get => entries.OrderBy(e => e.startTime).ToList();
	}

	public int GetTimeForTopicsInMinutes(Topic[] topics) => GetTimeForTopicsInSeconds(topics) / 60;
	private int GetTimeForTopicsInHours(Topic[] topics) => GetTimeForTopicsInSeconds(topics) / 3600;
	public string GetTimeForTopicsFormatted(Topic[] topics) =>
		$"{GetTimeForTopicsInHours(topics)} hours {GetTimeForTopicsInMinutes(topics)} minutes";
	public string GetTimeForAllEntriesFormatted()
	{
		int seconds = 0;
		foreach (BrewEntry entry in entries)
			seconds += entry.lengthSeconds;
		return $"{seconds / 3600} hours, {(seconds % 3600) / 60} minutes";
	}

	public BrewLog()
	{
		entries = new List<BrewEntry>();
		topics = new HashSet<Topic>();
	}

	public int GetTimeForTopicsInSeconds(Topic[] topics)
	{
		int seconds = 0;
		List<BrewEntry> entries = EntriesWithTopic(topics);
		foreach (BrewEntry entry in entries)
		{
			Console.WriteLine(entry.name);
			seconds += entry.lengthSeconds;
		}
		return seconds;
	}

	public Topic[] GetTopicsFromNames(List<string> names)
	{
		List<Topic> topicsWithName = new List<Topic>();
		foreach (Topic t in topics)
		{
			if (names.Contains(t.name))
			{
				topicsWithName.Add(t);
			}
		}
		return topicsWithName.ToArray();
	}

	public string GetEntriesWithTopicAndDateRangeFormatted(Topic[] topics, TimeInterval timeInterval)
	{
		var entriesWithGivenTopic = EntriesWithTopic(topics);
		var entriesWithTopicAndWithinDate = new List<BrewEntry>();

		foreach (BrewEntry entry in entriesWithGivenTopic)
		{
			var cal = System.Globalization.DateTimeFormatInfo.CurrentInfo.Calendar;
			var d1 = DateTime.Now.Date.AddDays(-1 * (int)cal.GetDayOfWeek(DateTime.Now));
			var d2 = entry.endTime.Date.AddDays(-1 * (int)cal.GetDayOfWeek(entry.endTime));
			bool entryEndTimeSameWeek = d1 == d2;

			bool withinTimeRange = timeInterval switch
			{
				TimeInterval.Day => DateTime.Today == entry.endTime.Date,
				TimeInterval.Week => entryEndTimeSameWeek,
				TimeInterval.Month => DateTime.Now.Year == entry.endTime.Year
					&& DateTime.Now.Month == entry.endTime.Month,
				TimeInterval.Year => DateTime.Now.Year == entry.endTime.Year, 
				TimeInterval.All => true,
				_ => false,
			};

			if (withinTimeRange) entriesWithTopicAndWithinDate.Add(entry);

		}

		int totalSeconds = 0;
		foreach (var entry in entriesWithGivenTopic) {
			totalSeconds += entry.lengthSeconds;
		}

		string totalSecondsFormatted;
		if (totalSeconds < 60) totalSecondsFormatted = $"{totalSeconds} seconds";
		else if (totalSeconds < 3600) totalSecondsFormatted = $"{totalSeconds / 60} minutes, {totalSeconds % 60} seconds";
		else totalSecondsFormatted = $"{totalSeconds / 3600} hours, {(totalSeconds / 3600) % 60} minutes";

		var displayBuilder = new System.Text.StringBuilder();
		displayBuilder.Append($"You have spent {totalSecondsFormatted} working ");

		bool topicIsEmpty = topics == null || topics.Length == 0;
		if (!topicIsEmpty) {
			string topicsDisplay = string.Join(" ", topics.Select(e => e.name));
			displayBuilder.Append($"on: [blue]{topicsDisplay}[/] ");
		}

		string suffix = timeInterval switch {
			TimeInterval.Day => "today",
			TimeInterval.Week => "this week",
			TimeInterval.Month => $"in {DateTime.Now.ToString("MMMM")}",
			TimeInterval.Year => $"in {DateTime.Now.ToString("yyyy")}",
			TimeInterval.All => "in total",
			_ => ""
		};

		displayBuilder.Append(suffix);
		return displayBuilder.ToString();

	}

	public void AddEntry(BrewEntry entry)
	{
		entries.Add(entry);
		foreach (Topic t in entry.topics)
		{
			topics.Add(t);
		}
	}

	public List<BrewEntry> EntriesWithTopic(Topic[] topics)
	{
		List<BrewEntry> entriesWithTopic = new List<BrewEntry>();
		foreach (BrewEntry brewEntry in entries)
		{
			bool brewEntryContainsTopic = false;
			foreach (Topic t in brewEntry.topics)
				if (topics.Contains(t))
					brewEntryContainsTopic = true;

			if (brewEntryContainsTopic) entriesWithTopic.Add(brewEntry);
		}
		return entriesWithTopic;
	}

	public List<BrewEntry> GetEntriesWithTimeInterval(TimeInterval timeInterval)
	{

		List<BrewEntry> brewEntries = new List<BrewEntry>();

		foreach (BrewEntry brewEntry in brewEntries)
		{
			DateTime startTimeOfEntry = brewEntry.startTime;

			bool entryFitsTimeIntreval = timeInterval switch
			{
				TimeInterval.Day => startTimeOfEntry.Day == DateTime.Now.Day,
				TimeInterval.Week =>
					DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek) == startTimeOfEntry.AddDays(-(int)startTimeOfEntry.DayOfWeek),
				TimeInterval.Month => startTimeOfEntry.Month == DateTime.Now.Month,
				TimeInterval.Year => startTimeOfEntry.Year == DateTime.Now.Year,
				TimeInterval.All => true,
				_ => throw new Exception("Time Interval is null"),
			};

			if (entryFitsTimeIntreval) brewEntries.Add(brewEntry);

		}

		return brewEntries;

	}

}

public enum TimeInterval
{
	Day,
	Week,
	Month,
	Year,
	All,
}

public struct Topic
{

	[JsonInclude] public string name;

	public Topic(string _name)
	{
		name = _name;
	}

	public override bool Equals(object? obj)
	{
		if (obj == null || !(obj is Topic)) return false;

		Topic topic = (Topic)obj;
		return topic.name.Equals(this.name);
	}
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

}

public struct BrewEntry
{
	[JsonInclude] public string name;
	[JsonInclude] public DateTime startTime;
	[JsonInclude] public int lengthSeconds;
	[JsonInclude] public Topic[] topics;

	public DateTime endTime { get => startTime.AddSeconds(lengthSeconds); }

	public BrewEntry(string _name, DateTime _startTime, int _lengthSeconds, Topic[] _topic)
	{
		name = _name;
		startTime = _startTime;
		lengthSeconds = _lengthSeconds;
		topics = _topic;
	}

	public string GetLengthFormatted()
	{
		int seconds = lengthSeconds;
		int hours = seconds / 3600;
		int minutes = (seconds % 3600) / 60;
		return hours != 0 ? $"{hours} hours, {minutes} minutes" : $"{minutes} minutes";
	}

	public override string ToString()
	{
		string topicsAsString = String.Join(" ", topics.Select(t => t.name));
		return $"Brew Entry: {name}; started at {startTime.ToString("h m")} for {lengthSeconds} seconds.";
	}

}
