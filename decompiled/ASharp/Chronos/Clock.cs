using System;
using UnityEngine;

namespace Chronos;

[HelpURL("http://ludiq.io/chronos/documentation#Clock")]
public abstract class Clock : MonoBehaviour
{
	private bool enabledOnce;

	protected bool isLerping;

	protected float lerpStart;

	protected float lerpEnd;

	protected float lerpFrom;

	protected float lerpTo;

	[SerializeField]
	private float _localTimeScale = 1f;

	[SerializeField]
	private bool _paused;

	[SerializeField]
	[GlobalClock]
	private string _parentKey;

	private GlobalClock _parent;

	[SerializeField]
	private ClockBlend _parentBlend;

	public float localTimeScale
	{
		get
		{
			return _localTimeScale;
		}
		set
		{
			_localTimeScale = value;
			ComputeTimeScale();
		}
	}

	public float timeScale { get; protected set; }

	public float time { get; protected set; }

	public float unscaledTime { get; protected set; }

	public float deltaTime { get; protected set; }

	public float unscaledDeltaTime { get; protected set; }

	public float fixedDeltaTime { get; protected set; }

	public float startTime { get; protected set; }

	public bool paused
	{
		get
		{
			return _paused;
		}
		set
		{
			_paused = value;
		}
	}

	public GlobalClock parent
	{
		get
		{
			return _parent;
		}
		set
		{
			if (_parent != null)
			{
				_parent.Unregister(this);
			}
			if (value != null)
			{
				if (value == this)
				{
					throw new ChronosException("Global clock parent cannot be itself.");
				}
				_parentKey = value.key;
				_parent = value;
				_parent.Register(this);
			}
			else
			{
				_parentKey = null;
				_parent = null;
			}
		}
	}

	public ClockBlend parentBlend
	{
		get
		{
			return _parentBlend;
		}
		set
		{
			_parentBlend = value;
		}
	}

	public TimeState state => Timekeeper.GetTimeState(timeScale);

	protected virtual void Awake()
	{
	}

	private void Start()
	{
		OnStartOrReEnable();
	}

	private void OnEnable()
	{
		if (enabledOnce)
		{
			OnStartOrReEnable();
		}
		else
		{
			enabledOnce = true;
		}
	}

	protected virtual void OnStartOrReEnable()
	{
		if (string.IsNullOrEmpty(_parentKey))
		{
			parent = null;
		}
		else
		{
			if (!Timekeeper.instance.HasClock(_parentKey))
			{
				throw new ChronosException($"Missing parent clock: '{_parentKey}'.");
			}
			parent = Timekeeper.instance.Clock(_parentKey);
		}
		startTime = Time.unscaledTime;
		if (parent != null)
		{
			parent.Register(this);
			parent.ComputeTimeScale();
		}
		ComputeTimeScale();
	}

	protected virtual void Update()
	{
		if (isLerping)
		{
			_localTimeScale = Mathf.Lerp(lerpFrom, lerpTo, (unscaledTime - lerpStart) / (lerpEnd - lerpStart));
			if (unscaledTime >= lerpEnd)
			{
				isLerping = false;
			}
		}
		ComputeTimeScale();
		unscaledDeltaTime = Timekeeper.unscaledDeltaTime;
		deltaTime = unscaledDeltaTime * timeScale;
		fixedDeltaTime = Time.fixedDeltaTime;
		time += deltaTime;
		unscaledTime += unscaledDeltaTime;
	}

	protected virtual void OnDisable()
	{
		if (parent != null)
		{
			parent.Unregister(this);
		}
	}

	public virtual void ComputeTimeScale()
	{
		if (paused)
		{
			timeScale = 0f;
		}
		else if (parent == null)
		{
			timeScale = localTimeScale;
		}
		else if (parentBlend == ClockBlend.Multiplicative)
		{
			timeScale = parent.timeScale * localTimeScale;
		}
		else
		{
			timeScale = parent.timeScale + localTimeScale;
		}
	}

	public void LerpTimeScale(float timeScale, float duration, bool steady = false)
	{
		if (duration < 0f)
		{
			throw new ArgumentException("Duration must be positive.", "duration");
		}
		if (duration == 0f)
		{
			localTimeScale = timeScale;
			isLerping = false;
			return;
		}
		float num = 1f;
		if (steady)
		{
			num = Mathf.Abs(localTimeScale - timeScale);
		}
		if (num != 0f)
		{
			lerpFrom = localTimeScale;
			lerpStart = unscaledTime;
			lerpTo = timeScale;
			lerpEnd = lerpStart + duration * num;
			isLerping = true;
		}
	}
}
