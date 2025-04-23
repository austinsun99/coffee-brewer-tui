using System.Text.Json.Serialization;
namespace Brew;

public class BrewLog
{
	private HashSet<string> topics;
	public HashSet<string> Topics { get => topics; set => topics = value; }

	private List<BrewEntry> entries;
	public List<BrewEntry> Entries { get => entries; set => entries = value; }

	[JsonIgnore]
	public List<BrewEntry> SortedEntries
	{
		get => entries.OrderBy(e => e.startTime).Reverse().ToList();
	}

	public BrewLog()
	{
		entries = new List<BrewEntry>();
		topics = new HashSet<string>();
	}

	public void AddEntry(BrewEntry entry)
	{
		entries.Add(entry);
		foreach (string t in entry.topics)
		{
			topics.Add(t);
		}
	}

	public string TimeToFormatted(int totalSeconds)
	{
		string totalSecondsFormatted;
		if (totalSeconds < 60) totalSecondsFormatted = $"{totalSeconds} seconds";
		else if (totalSeconds < 3600) totalSecondsFormatted = $"{totalSeconds / 60} minutes, {totalSeconds % 60} seconds";
		else totalSecondsFormatted = $"{totalSeconds / 3600} hours, {(totalSeconds / 3600) % 60} minutes";
		return totalSecondsFormatted;
	}

	public int GetTotalMinutesForTimeInterval(TimeInterval timeInterval, bool focusedTime)
	{
		List<BrewEntry> entries = GetEntriesWithTimeInterval(Entries, timeInterval);
		int totalSeconds = 0;
		foreach (var entry in entries)
		{
			totalSeconds += focusedTime ? entry.focusedTimeSeconds : entry.unfocusedTimeSeconds;
		}
		return totalSeconds / 60;
	}

	public string GetEntriesWithTopicAndDateRangeFormatted(string[] topics, TimeInterval timeInterval)
	{
		var entriesWithGivenTopicAndDateRange = GetEntriesWithTopicAndTimeInterval(entries, topics, timeInterval);
		int totalSeconds = 0;
		foreach (var entry in entriesWithGivenTopicAndDateRange)
		{
			totalSeconds += entry.focusedTimeSeconds;
			totalSeconds += entry.unfocusedTimeSeconds;
		}

		string totalSecondsFormatted = TimeToFormatted(totalSeconds);

		var displayBuilder = new System.Text.StringBuilder();
		displayBuilder.Append($"You have spent [green]{totalSecondsFormatted}[/] working ");

		bool topicIsEmpty = topics.Length == 0;
		if (!topicIsEmpty)
		{
			string topicsDisplay = string.Join(" ", topics);
			displayBuilder.Append($"on: [blue]{topicsDisplay}[/] ");
		}

		string suffix = timeInterval switch
		{
			TimeInterval.Day => "today",
			TimeInterval.Week => "this week",
			TimeInterval.Month => $"in {DateTime.Now.ToString("MMMM")}",
			TimeInterval.Year => $"in {DateTime.Now.ToString("yyyy")}",
			TimeInterval.All => "in total",
			_ => ""
		};

		displayBuilder.Append($"[green]{suffix}[/]");
		return displayBuilder.ToString();

	}

	public List<BrewEntry> GetEntriesWithTopicAndTimeInterval(string[] topics, TimeInterval timeInterval)
	{
		return GetEntriesWithTopicAndTimeInterval(entries, topics, timeInterval);
	}

	public List<BrewEntry> GetEntriesWithTopicAndTimeInterval
		(List<BrewEntry> entries, string[] topics, TimeInterval timeInterval)
	{
		var entriesWithTopic = GetEntriesWithTopic(entries, topics);
		return GetEntriesWithTimeInterval(entriesWithTopic, timeInterval);
	}

	public List<BrewEntry> GetEntriesWithTimeInterval(List<BrewEntry> entries, TimeInterval timeInterval)
	{
		var entriesWithTopicAndWithinDate = new List<BrewEntry>();

		foreach (BrewEntry entry in entries)
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
		return entriesWithTopicAndWithinDate.OrderBy(e => e.startTime).Reverse().ToList();
	}

	public List<BrewEntry> GetEntriesWithTopic(List<BrewEntry> entries, string[] topics, bool includeNoTopics = true)
	{
		List<BrewEntry> entriesWithTopic = new List<BrewEntry>();
		foreach (BrewEntry brewEntry in entries)
		{
			bool brewEntryContainsTopic = false;

			if (topics.Length == 0 && includeNoTopics)
				brewEntryContainsTopic = true;
			foreach (string t in brewEntry.topics)
				if (topics.Any(t.Equals))
					brewEntryContainsTopic = true;

			if (brewEntryContainsTopic) entriesWithTopic.Add(brewEntry);
		}
		return entriesWithTopic;
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

public struct BrewEntry
{
	[JsonInclude] public string name;
	[JsonInclude] public DateTime startTime;
	[JsonInclude] public string[] topics;

	[JsonInclude] public int unfocusedTimeSeconds;
	[JsonInclude] public int focusedTimeSeconds;
	[JsonIgnore] public int totalTimeSeconds { get => unfocusedTimeSeconds + focusedTimeSeconds; }

	public DateTime endTime { get => startTime.AddSeconds(totalTimeSeconds); }
	public string FocusedTimeFormatted { get => GetLengthFormatted(unfocusedTimeSeconds); }
	public string UnfocusedTimeFormatted { get => GetLengthFormatted(unfocusedTimeSeconds); }
	public string TotalTimeFormatted { get => GetLengthFormatted(totalTimeSeconds); }

	public BrewEntry(string _name, DateTime _startTime, string[] _topic, int _unfocusedTimeSeconds, int _focusedTimeSeconds)
	{
		name = _name;
		startTime = _startTime;
		topics = _topic;
		unfocusedTimeSeconds = _unfocusedTimeSeconds;
		focusedTimeSeconds = _focusedTimeSeconds;
	}

	private string GetLengthFormatted(int totalSeconds)
	{
		int seconds = totalSeconds % 60;
		int hours = totalSeconds / 3600;
		int minutes = (totalSeconds % 3600) / 60;

		string result = "";

		if (minutes == 0) result = $"{seconds} seconds";
		else if (hours == 0) result = $"{minutes} minutes, {seconds} seconds";
		else result = $"{hours} hours, {minutes} minutes";

		return result;
	}
}
