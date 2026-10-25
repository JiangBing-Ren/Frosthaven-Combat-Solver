using UnityEngine;

namespace FH.Platforms;

public class NewPlatformStandaloneLocalization : NewPlatformLocalization
{
	public override string GetPlatformLocale()
	{
		return Application.systemLanguage.ToString();
	}

	public override string GetDebugInfo()
	{
		return "Uses Application.systemLanguage for deciding the locale.";
	}
}
