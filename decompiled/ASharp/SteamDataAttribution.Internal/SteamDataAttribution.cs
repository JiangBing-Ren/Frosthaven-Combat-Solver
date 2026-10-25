using UnityEngine;

namespace SteamDataAttribution.Internal;

public class SteamDataAttribution
{
	public const string PLAYER_PREF_KEY = "steamDataSuiteSuccess";

	public static void OnRuntimeMethodLoad()
	{
		SteamDataAttributionConfig[] array = Resources.LoadAll<SteamDataAttributionConfig>("");
		if (array.Length == 0)
		{
			Debug.LogError("SteamDataAttribution Error: Unable to find any Config-files. Please create a new file in your Resources-folder");
			return;
		}
		if (!array[0].Enabled)
		{
			Debug.LogWarning("SteamDataAttribution Disabled!");
			return;
		}
		GameObject gameObject = new GameObject("SteamDataAttributionHelper (hidden)")
		{
			hideFlags = HideFlags.HideAndDontSave
		};
		Object.DontDestroyOnLoad(gameObject);
		gameObject.AddComponent<SteamDataAttributionHelper>().SendForm(array[0]);
	}
}
