using UnityEngine;

[CreateAssetMenu(fileName = "SteamDataAttributionConfig", menuName = "SteamDataAttribution/Config-file")]
public class SteamDataAttributionConfig : ScriptableObject
{
	[Tooltip("Easy way to disable it all together")]
	public bool Enabled = true;

	[Tooltip("The API key required to connect to the server. Looks like this: 2e96eedce46544d9806de9dc3401c8007")]
	public string LicenseKey = "";

	[Tooltip("Enable if you want to Log info when the connection with the server is succesfull/failed")]
	public bool DebugMode;
}
