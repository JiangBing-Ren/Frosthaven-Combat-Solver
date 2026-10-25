namespace FH.Platforms;

public class NewPlatformPS5Localization : NewPlatformLocalization
{
	public override string GetPlatformLocale()
	{
		return string.Empty;
	}

	public override string GetDebugInfo()
	{
		return "Localization not implemented for this platform.";
	}
}
