using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

namespace FH.Misc;

public class RespectFrameTime : IDisposable
{
	private readonly Stopwatch _stopwatch;

	private readonly float _maxMilliseconds;

	public RespectFrameTime(string name, float maxMilliseconds = 100f)
	{
		_stopwatch = Stopwatch.StartNew();
		_maxMilliseconds = maxMilliseconds;
	}

	public void Dispose()
	{
	}

	public void ResetTimer()
	{
		_stopwatch.Restart();
	}

	public bool ShouldWait()
	{
		if (Application.isPlaying)
		{
			return (float)_stopwatch.ElapsedMilliseconds >= _maxMilliseconds;
		}
		return false;
	}

	public IEnumerator WaitUntilNextFrameIfNeeded(string description = null)
	{
		if (Application.isPlaying && (float)_stopwatch.ElapsedMilliseconds >= _maxMilliseconds)
		{
			yield return null;
			_stopwatch.Restart();
		}
	}
}
