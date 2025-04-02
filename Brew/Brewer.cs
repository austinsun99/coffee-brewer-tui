using System.Timers;

namespace Brew;

public delegate void OnBrewFinish();

public class Brewer
{

	private bool brewing = false;

	private BrewLog brewLog;
	public BrewLog BrewLog { get => brewLog; }

	private OnBrewFinish OnBrewFinish;
	private System.Timers.Timer brewTimer;

	public Brewer(OnBrewFinish _OnBrewFinish)
	{
		OnBrewFinish = _OnBrewFinish;
		brewTimer = new System.Timers.Timer();
		brewLog = new BrewLog();
	}

	public void StartBrew(float time, BrewEntry brewEntry)
	{

		if (brewing) return;

		brewing = true;

		brewTimer = new System.Timers.Timer(time);

		brewTimer.Elapsed += (object? sender, ElapsedEventArgs e) =>
		{
			brewing = false;
			brewLog.Entries.Add(brewEntry);
			OnBrewFinish();
		};

		brewTimer.AutoReset = false;
		brewTimer.Enabled = true;
	}

}
