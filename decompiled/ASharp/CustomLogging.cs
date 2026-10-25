using Apparance.Net;
using UnityEngine;

[ExecuteAlways]
public class CustomLogging : MonoBehaviour, ILogging
{
	private void Start()
	{
		ApparanceEngine.Instance.LogMessage("Testing default logging handler.");
		Debug.Log("Replacing Apparance Logging with Custom handler.");
		ApparanceEngine.SetCustomLogging(this, replace: true);
		ApparanceEngine.Instance.LogMessage("Testing custom logging handler.");
	}

	public void LogMessage(string message)
	{
		Debug.Log("CUSTOM MESSAGE: " + message);
	}

	public void LogWarning(string message)
	{
		Debug.LogWarning("CUSTOM WARNING: " + message);
	}

	public void LogError(string message)
	{
		Debug.LogError("CUSTOM ERROR: " + message);
	}
}
