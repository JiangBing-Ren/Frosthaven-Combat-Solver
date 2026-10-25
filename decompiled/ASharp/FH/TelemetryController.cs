using System;
using Backtrace.Unity;
using SETUtil.Functional.Extend;
using UnityEngine;

namespace FH;

public class TelemetryController : MonoBehaviour
{
	private void Awake()
	{
		Upd();
		TelemetryAndEulaPolicy.OnTelemetryEnabledChanged = (Action)Delegate.Remove(TelemetryAndEulaPolicy.OnTelemetryEnabledChanged, new Action(Upd));
		TelemetryAndEulaPolicy.OnTelemetryEnabledChanged = (Action)Delegate.Combine(TelemetryAndEulaPolicy.OnTelemetryEnabledChanged, new Action(Upd));
	}

	private void Upd()
	{
		UnityEngine.Object.FindObjectOfType<BacktraceClient>().Then((BacktraceClient a) =>
		{
			a.enabled = TelemetryAndEulaPolicy.IsTelemetryEnabled;
		});
	}

	private void OnDestroy()
	{
		TelemetryAndEulaPolicy.OnTelemetryEnabledChanged = (Action)Delegate.Remove(TelemetryAndEulaPolicy.OnTelemetryEnabledChanged, new Action(Upd));
	}
}
