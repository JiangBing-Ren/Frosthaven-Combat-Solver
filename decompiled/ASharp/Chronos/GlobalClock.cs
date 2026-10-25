using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chronos;

[AddComponentMenu("Time/Global Clock")]
[HelpURL("http://ludiq.io/chronos/documentation#GlobalClock")]
public class GlobalClock : Clock
{
	protected HashSet<Clock> clocks;

	protected HashSet<Timeline> timelines;

	[SerializeField]
	private string _key;

	public string key
	{
		get
		{
			return _key;
		}
		internal set
		{
			_key = value;
		}
	}

	public GlobalClock()
	{
		clocks = new HashSet<Clock>();
		timelines = new HashSet<Timeline>();
	}

	internal virtual void Register(Timeline timeline)
	{
		if (timeline == null)
		{
			throw new ArgumentNullException("timeline");
		}
		timelines.Add(timeline);
	}

	internal virtual void Unregister(Timeline timeline)
	{
		if (timeline == null)
		{
			throw new ArgumentNullException("timeline");
		}
		timelines.Remove(timeline);
	}

	internal virtual void Register(Clock clock)
	{
		if (clock == null)
		{
			throw new ArgumentNullException("clock");
		}
		clocks.Add(clock);
	}

	internal virtual void Unregister(Clock clock)
	{
		if (clock == null)
		{
			throw new ArgumentNullException("clock");
		}
		clocks.Remove(clock);
	}
}
