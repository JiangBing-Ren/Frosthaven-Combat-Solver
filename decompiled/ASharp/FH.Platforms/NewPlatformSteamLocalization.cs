using Steamworks;

namespace FH.Platforms;

public class NewPlatformSteamLocalization : NewPlatformLocalization
{
	public override string GetPlatformLocale()
	{
		return SteamApps.GameLanguage;
	}

	public override string GetDebugInfo()
	{
		return "Available Languages: " + SteamApps.AvailableLanguages.ToStringPretty();
	}
}
