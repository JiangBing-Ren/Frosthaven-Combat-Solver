using UnityEngine;

namespace Chronos;

public class TrailRendererTimeline : ComponentTimeline<TrailRenderer>
{
	private float _time;

	public float time
	{
		get
		{
			return _time;
		}
		set
		{
			_time = value;
			AdjustProperties();
		}
	}

	public TrailRendererTimeline(Timeline timeline, TrailRenderer component)
		: base(timeline, component)
	{
	}

	public override void CopyProperties(TrailRenderer source)
	{
		_time = source.time;
	}

	public override void AdjustProperties(float timeScale)
	{
		component.time = time / Mathf.Abs(timeScale);
	}
}
