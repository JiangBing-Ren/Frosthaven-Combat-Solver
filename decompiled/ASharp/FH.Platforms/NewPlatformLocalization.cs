using System.Collections.Generic;
using Libs.GameConsole;

namespace FH.Platforms;

public abstract class NewPlatformLocalization
{
	private static Dictionary<string, string> _localeTransformNameToISO639 = new Dictionary<string, string>
	{
		{ "english", "en" },
		{ "french", "fr" },
		{ "german", "de" },
		{ "italian", "it" },
		{ "russian", "ru" },
		{ "spanish", "es" }
	};

	public static string TryTransformNameToISO639(string fullLocaleName)
	{
		if (_localeTransformNameToISO639.TryGetValue(fullLocaleName.ToLower(), out var value))
		{
			return value;
		}
		return "en";
	}

	public abstract string GetPlatformLocale();

	public abstract string GetDebugInfo();

	[ConsoleCommand(Command = "locale_debug_platform")]
	public static async void ConsoleDebugLocale(IConsole console)
	{
		console.WriteLine("[NewPlatform] Current Platform Locale is: " + await PlatformLayer.Instance.NewPlatformLayer.GetLocale() + ".");
		string debugInfo = PlatformLayer.Instance.NewPlatformLayer.CurrentPlatform.Localization.GetDebugInfo();
		console.WriteLine("[New Platform] Debug Info:\n" + debugInfo);
	}
}
