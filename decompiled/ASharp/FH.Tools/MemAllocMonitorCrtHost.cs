using System;
using System.Collections;
using SETUtil;
using UnityEngine;
using UnityEngine.Profiling;

namespace FH.Tools;

public class MemAllocMonitorCrtHost : MonoBehaviour
{
	private IEnumerator Crt(int seconds, Action<string> outFeedback)
	{
		DateTime time = DateTime.Now;
		while (DateTime.Now - time < TimeSpan.FromSeconds(seconds))
		{
			long totalAllocatedMemoryLong = Profiler.GetTotalAllocatedMemoryLong();
			string text = string.Format("[MemoryMonitor] Mem KiB: {0} ({1} GiB)", totalAllocatedMemoryLong / 1024, ((float)totalAllocatedMemoryLong / 1024f / 1024f / 1024f).ToString("0.0"));
			Debug.Log(text);
			outFeedback?.Invoke(text);
			yield return new WaitForSeconds(1f);
		}
		SceneUtil.SmartDestroy(gameObject);
	}

	internal void Run(int forSeconds, Action<string> outFeedback)
	{
		StartCoroutine(Crt(forSeconds, outFeedback));
	}
}
