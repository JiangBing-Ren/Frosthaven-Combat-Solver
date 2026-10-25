namespace ClockStone;

public class StopAudio : AudioTriggerBase
{
	public string audioID;

	public float fadeOut;

	protected override void _OnEventTriggered()
	{
		AudioController.Stop(audioID, fadeOut);
	}
}
