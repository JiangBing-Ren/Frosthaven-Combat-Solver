namespace ClockStone;

public class StopAllAudio : AudioTriggerBase
{
	public float fadeOut;

	protected override void _OnEventTriggered()
	{
		AudioController.StopAll(fadeOut);
	}
}
