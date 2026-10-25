using UnityEngine;

namespace Chronos;

[AddComponentMenu("Time/Local Clock")]
[DisallowMultipleComponent]
[HelpURL("http://ludiq.io/chronos/documentation#LocalClock")]
public class LocalClock : Clock
{
	protected Timeline timeline;

	protected override void Awake()
	{
		base.Awake();
		CacheComponents();
	}

	public virtual void CacheComponents()
	{
		timeline = GetComponent<Timeline>();
		if (timeline == null)
		{
			throw new ChronosException($"Missing timeline for local clock.");
		}
	}
}
