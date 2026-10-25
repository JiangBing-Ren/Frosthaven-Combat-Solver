using UnityEngine;

namespace Chronos;

[AddComponentMenu("Time/Timeline Child")]
[DisallowMultipleComponent]
[HelpURL("http://ludiq.io/chronos/documentation#Timeline")]
public class TimelineChild : TimelineEffector
{
	public Timeline parent { get; private set; }

	protected override Timeline timeline => parent;

	protected override void Awake()
	{
		CacheParent();
		base.Awake();
	}

	public void CacheParent()
	{
		Timeline timeline = parent;
		parent = GetComponentInParent<Timeline>();
		if (parent == null)
		{
			throw new ChronosException("Missing parent timeline for timeline child.");
		}
		if (timeline != null)
		{
			timeline.children.Remove(this);
		}
		parent.children.Add(this);
	}
}
