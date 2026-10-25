namespace ClockStone;

public class UnpauseAllAudio : AudioTriggerBase
{
	public enum PauseType
	{
		All,
		MusicOnly,
		AmbienceOnly,
		Category
	}

	public PauseType pauseType;

	public float fadeIn;

	public string categoryName;

	protected override void _OnEventTriggered()
	{
		switch (pauseType)
		{
		case PauseType.All:
			AudioController.UnpauseAll(fadeIn);
			break;
		case PauseType.MusicOnly:
			AudioController.UnpauseMusic(fadeIn);
			break;
		case PauseType.AmbienceOnly:
			AudioController.UnpauseAmbienceSound(fadeIn);
			break;
		case PauseType.Category:
			AudioController.UnpauseCategory(categoryName, fadeIn);
			break;
		}
	}
}
