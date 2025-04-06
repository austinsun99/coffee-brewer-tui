using Brew;
using System.Text.Json;

static class SaveLoad
{
	private const string SAVE_PATH = "save.json";
	public static void SaveBrewLog(BrewLog log)
	{
		var options = new JsonSerializerOptions { WriteIndented = true };
		string data = JsonSerializer.Serialize(log, options);

		using (var output = new StreamWriter(SAVE_PATH))
		{
			output.WriteLine(data);
		}
	}

	public static BrewLog LoadBrewLog()
	{
		var fileInfo = new FileInfo(SAVE_PATH);
		if (!fileInfo.Exists || fileInfo.Length == 0) return new BrewLog();
		using (var reader = new StreamReader(SAVE_PATH))
		{
			string text = reader.ReadToEnd();
			var brewLog = JsonSerializer.Deserialize<BrewLog>(text);

			if (brewLog == null)
			{ Console.WriteLine("BrewLog is null"); return new BrewLog(); }

			else Console.WriteLine("Brewlog is not null");

			foreach (var topic in brewLog.Topics)
			{
				Console.WriteLine($"Topic: {topic}");
			}
			foreach (var entry in brewLog.Entries)
			{
				Console.WriteLine($"Entry: {entry.name}, Unfocused time: {entry.unfocusedTimeSeconds}, Focused time: {entry.focusedTimeSeconds}");
			}
			return brewLog;
		}
	}
}
