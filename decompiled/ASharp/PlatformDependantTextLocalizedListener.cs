public class PlatformDependantTextLocalizedListener : TextLocalizedListener
{
	private const string PLATFORM_NAME_STEAM = "Steam";

	private const string PLATFORM_NAME_EPIC = "Epic";

	protected override void OnLanguageChanged()
	{
		base.OnLanguageChanged();
		text.text = text.text.Replace("Epic", "Steam");
	}
}
