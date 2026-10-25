public class PauseAllAudio : AudioTriggerBase
{
	public enum PauseType
	{
		All,
		MusicOnly,
		AmbienceOnly,
		Category
	}

	public PauseType pauseType;

	public float fadeOut;

	public string categoryName;

	protected override void _OnEventTriggered()
	{
		switch (pauseType)
		{
		case PauseType.All:
			AudioController.PauseAll(fadeOut);
			break;
		case PauseType.MusicOnly:
			AudioController.PauseMusic(fadeOut);
			break;
		case PauseType.AmbienceOnly:
			AudioController.PauseAmbienceSound(fadeOut);
			break;
		case PauseType.Category:
			AudioController.PauseCategory(categoryName, fadeOut);
			break;
		}
	}
}
