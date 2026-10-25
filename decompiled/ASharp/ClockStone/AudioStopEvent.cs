using System;

namespace ClockStone;

[Serializable]
public class AudioStopEvent
{
	public enum Level
	{
		Global,
		GameObject
	}

	public Level NotificationLevel;

	[AudioEventName]
	public string AudioEvent;
}
