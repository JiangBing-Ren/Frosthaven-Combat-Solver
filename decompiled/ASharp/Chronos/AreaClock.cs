using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chronos;

[HelpURL("http://ludiq.io/chronos/documentation#AreaClock")]
public abstract class AreaClock<TCollider, TVector> : Clock, IAreaClock
{
	protected Dictionary<Timeline, Vector3> within;

	protected HashSet<Timeline> outOfBounds;

	protected TCollider _collider;

	[SerializeField]
	private AreaClockMode _mode;

	[SerializeField]
	private AnimationCurve _curve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

	[SerializeField]
	private TVector _center;

	[SerializeField]
	private float _padding = 0.5f;

	[SerializeField]
	private ClockBlend _innerBlend;

	public AreaClockMode mode
	{
		get
		{
			return _mode;
		}
		set
		{
			_mode = value;
		}
	}

	public AnimationCurve curve
	{
		get
		{
			return _curve;
		}
		set
		{
			_curve = value;
		}
	}

	public TVector center
	{
		get
		{
			return _center;
		}
		set
		{
			_center = value;
		}
	}

	public float padding
	{
		get
		{
			return _padding;
		}
		set
		{
			_padding = value;
		}
	}

	public ClockBlend innerBlend
	{
		get
		{
			return _innerBlend;
		}
		set
		{
			_innerBlend = value;
		}
	}

	public AreaClock()
	{
		within = new Dictionary<Timeline, Vector3>();
		outOfBounds = new HashSet<Timeline>();
	}

	protected override void Awake()
	{
		base.Awake();
		CacheComponents();
	}

	protected override void OnDisable()
	{
		ReleaseAll();
		base.OnDisable();
	}

	protected virtual void Capture(Timeline timeline, Vector3 entry)
	{
		if (timeline == null)
		{
			throw new ArgumentNullException("timeline");
		}
		if (!within.ContainsKey(timeline))
		{
			within.Add(timeline, entry);
		}
		if (!timeline.areaClocks.Contains(this))
		{
			timeline.areaClocks.Add(this);
		}
	}

	public virtual void Release(Timeline timeline)
	{
		if (timeline == null)
		{
			throw new ArgumentNullException("timeline");
		}
		if (within.ContainsKey(timeline))
		{
			within.Remove(timeline);
		}
		if (timeline.areaClocks.Contains(this))
		{
			timeline.areaClocks.Remove(this);
		}
	}

	public virtual void ReleaseAll()
	{
		foreach (Timeline item in within.Keys.Where((Timeline w) => w != null))
		{
			if (item.areaClocks.Contains(this))
			{
				item.areaClocks.Remove(this);
			}
		}
		within.Clear();
	}

	float IAreaClock.TimeScale(Timeline timeline)
	{
		if (timeline == null)
		{
			throw new ArgumentNullException("timeline");
		}
		if (mode == AreaClockMode.Instant)
		{
			return timeScale;
		}
		if (mode == AreaClockMode.PointToEdge)
		{
			Vector3 position = timeline.gameObject.transform.position;
			return PointToEdgeTimeScale(position);
		}
		if (mode == AreaClockMode.DistanceFromEntry)
		{
			Vector3 position2 = timeline.gameObject.transform.position;
			Vector3 entry = within[timeline];
			return DistanceFromEntryTimeScale(entry, position2);
		}
		throw new ChronosException("Unknown area clock mode.");
	}

	protected virtual float DistanceFromEntryTimeScale(Vector3 entry, Vector3 position)
	{
		if (padding < 0f)
		{
			throw new ChronosException("Area clock padding must be positive.");
		}
		entry = transform.TransformPoint(entry);
		Vector3 vector = position - entry;
		Vector3 normalized = vector.normalized;
		float magnitude = vector.magnitude;
		Vector3 end = entry + padding * normalized;
		float a = ((innerBlend == ClockBlend.Multiplicative) ? 1 : 0);
		if (magnitude < padding)
		{
			if (Timekeeper.instance.debug)
			{
				Debug.DrawLine(entry, end, Color.cyan);
				Debug.DrawLine(entry, position, Color.magenta);
			}
			return Mathf.Lerp(a, timeScale, curve.Evaluate(1f - magnitude / padding));
		}
		if (Timekeeper.instance.debug)
		{
			Debug.DrawLine(entry, position, Color.magenta);
		}
		return timeScale;
	}

	protected abstract float PointToEdgeTimeScale(Vector3 position);

	public abstract bool ContainsPoint(Vector3 point);

	public virtual void CacheComponents()
	{
	}
}
