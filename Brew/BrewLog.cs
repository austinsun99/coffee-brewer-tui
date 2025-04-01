namespace Brew;

public class BrewLog
{
	private HashSet<Topic> topics;
	private List<BrewEntry> entries;
	public List<BrewEntry> Entries { get => entries; }
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
		return $"{seconds / 3600} hours, {seconds % 60} minutes";
	}

	public HashSet<Topic> Topics { get => topics; }

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
				_ => throw new Exception("Time Interval is null"),
			};

			if (entryFitsTimeIntreval) brewEntries.Add(brewEntry);

		}

		return brewEntries;

	}

	private bool EntryFitsTimeInterval()
	{
		return false;
	}

}

public enum TimeInterval
{
	Day,
	Week,
	Month,
	Year,
}

public struct Topic
{

	public string name;

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
	public string name;
	public DateTime startTime;
	public int lengthSeconds;
	public Topic[] topics;

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
