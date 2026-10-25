using System;
using UnityEngine;

namespace FH;

public static class TelemetryAndEulaPolicy
{
	public static Action OnTelemetryEnabledChanged;

	public static bool IsTelemetryEnabled;

	public static bool IsEulaAccepted;

	public static void Cache()
	{
		IsTelemetryEnabled = PlayerPrefs.GetInt("TelemetryEnabled", 0) == 1;
		IsEulaAccepted = PlayerPrefs.GetInt("EulaAccepted", 0) == 1;
	}

	public static void SetTelemetryEnabled(bool enabled)
	{
		PlayerPrefs.SetInt("TelemetryEnabled", enabled ? 1 : 0);
		Cache();
		OnTelemetryEnabledChanged?.Invoke();
	}

	public static void SetEulaAccepted(bool accepted)
	{
		PlayerPrefs.SetInt("EulaAccepted", accepted ? 1 : 0);
		Cache();
	}
}
