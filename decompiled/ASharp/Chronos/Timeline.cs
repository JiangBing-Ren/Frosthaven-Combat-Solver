using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Chronos;

[AddComponentMenu("Time/Timeline")]
[DisallowMultipleComponent]
[HelpURL("http://ludiq.io/chronos/documentation#Timeline")]
public class Timeline : TimelineEffector
{
	protected internal const float DefaultRecordingDuration = 30f;

	protected internal float lastTimeScale;

	protected Queue<float> previousDeltaTimes;

	protected List<Occurrence> occurrences;

	protected HashSet<Occurrence> handledOccurrences;

	protected Occurrence nextForwardOccurrence;

	protected Occurrence nextBackwardOccurrence;

	protected internal List<IAreaClock> areaClocks;

	protected internal List<TimelineChild> children;

	[SerializeField]
	private TimelineMode _mode;

	[SerializeField]
	[GlobalClock]
	private string _globalClockKey;

	private Clock _clock;

	public static int smoothingDeltas = 5;

	[SerializeField]
	[FormerlySerializedAs("_recordTransform")]
	private bool _rewindable = true;

	[SerializeField]
	private float _recordingDuration = 30f;

	protected override Timeline timeline => this;

	public TimelineMode mode
	{
		get
		{
			return _mode;
		}
		set
		{
			_mode = value;
			_clock = null;
		}
	}

	public string globalClockKey
	{
		get
		{
			return _globalClockKey;
		}
		set
		{
			_globalClockKey = value;
			_clock = null;
		}
	}

	public Clock clock
	{
		get
		{
			if (_clock == null)
			{
				_clock = FindClock();
			}
			return _clock;
		}
	}

	public float timeScale { get; protected set; }

	public float deltaTime { get; protected set; }

	public float fixedDeltaTime { get; protected set; }

	public float smoothDeltaTime => (deltaTime + previousDeltaTimes.Sum()) / (float)(previousDeltaTimes.Count + 1);

	public float time { get; protected internal set; }

	public float unscaledTime { get; protected set; }

	public TimeState state => Timekeeper.GetTimeState(timeScale);

	public bool rewindable
	{
		get
		{
			return _rewindable;
		}
		set
		{
			_rewindable = value;
			if (_particleSystem != null)
			{
				CacheComponents();
			}
		}
	}

	public float recordingDuration
	{
		get
		{
			return _recordingDuration;
		}
		set
		{
			_recordingDuration = value;
			if (recorder != null)
			{
				recorder.Reset();
			}
		}
	}

	public float availableRewindDuration => transform.availableRewindDuration;

	public Timeline()
	{
		children = new List<TimelineChild>();
		areaClocks = new List<IAreaClock>();
		occurrences = new List<Occurrence>();
		handledOccurrences = new HashSet<Occurrence>();
		previousDeltaTimes = new Queue<float>();
		timeScale = (lastTimeScale = 1f);
	}

	protected override void OnStartOrReEnable()
	{
		timeScale = (lastTimeScale = clock.timeScale);
		base.OnStartOrReEnable();
	}

	protected override void Update()
	{
		TriggerEvents();
		lastTimeScale = timeScale;
		timeScale = clock.timeScale;
		for (int i = 0; i < areaClocks.Count; i++)
		{
			IAreaClock areaClock = areaClocks[i];
			if (areaClock != null)
			{
				float num = areaClock.TimeScale(this);
				if (areaClock.innerBlend == ClockBlend.Multiplicative)
				{
					timeScale *= num;
				}
				else
				{
					timeScale += num;
				}
			}
		}
		if (!rewindable)
		{
			timeScale = Mathf.Max(0f, timeScale);
		}
		if (timeScale != lastTimeScale)
		{
			for (int j = 0; j < components.Count; j++)
			{
				components[j].AdjustProperties();
			}
			for (int k = 0; k < children.Count; k++)
			{
				for (int l = 0; l < children[k].components.Count; l++)
				{
					children[k].components[l].AdjustProperties();
				}
			}
		}
		float unscaledDeltaTime = Timekeeper.unscaledDeltaTime;
		deltaTime = unscaledDeltaTime * timeScale;
		fixedDeltaTime = Time.fixedDeltaTime * timeScale;
		time += deltaTime;
		unscaledTime += unscaledDeltaTime;
		RecordSmoothing();
		base.Update();
		if (timeScale > 0f)
		{
			TriggerForwardOccurrences();
		}
		else if (timeScale < 0f)
		{
			TriggerBackwardOccurrences();
		}
	}

	protected override void OnDisable()
	{
		ReleaseFromAll();
		base.OnDisable();
	}

	public override void Reset()
	{
		base.Reset();
		timeScale = (lastTimeScale = 1f);
		previousDeltaTimes.Clear();
		occurrences.Clear();
		handledOccurrences.Clear();
		nextForwardOccurrence = null;
		nextBackwardOccurrence = null;
	}

	protected virtual Clock FindClock()
	{
		if (mode == TimelineMode.Local)
		{
			LocalClock component = GetComponent<LocalClock>();
			if (component == null)
			{
				throw new ChronosException($"Missing local clock for timeline.");
			}
			return component;
		}
		if (mode == TimelineMode.Global)
		{
			GlobalClock globalClock = _clock as GlobalClock;
			if (globalClock != null)
			{
				globalClock.Unregister(this);
			}
			if (!Timekeeper.instance.HasClock(globalClockKey))
			{
				throw new ChronosException($"Missing global clock for timeline: '{globalClockKey}'.");
			}
			GlobalClock globalClock2 = Timekeeper.instance.Clock(globalClockKey);
			globalClock2.Register(this);
			return globalClock2;
		}
		throw new ChronosException($"Unknown timeline mode: '{mode}'.");
	}

	public virtual void ReleaseFrom(IAreaClock areaClock)
	{
		areaClock.Release(this);
	}

	public virtual void ReleaseFromAll()
	{
		IAreaClock[] array = areaClocks.Where((IAreaClock ac) => ac != null).ToArray();
		for (int num = 0; num < array.Length; num++)
		{
			array[num].Release(this);
		}
		areaClocks.Clear();
	}

	protected virtual void TriggerEvents()
	{
		if (lastTimeScale != 0f && timeScale == 0f)
		{
			SendMessage("OnStartPause", SendMessageOptions.DontRequireReceiver);
		}
		if (lastTimeScale == 0f && timeScale != 0f)
		{
			SendMessage("OnStopPause", SendMessageOptions.DontRequireReceiver);
		}
		if (lastTimeScale >= 0f && timeScale < 0f)
		{
			SendMessage("OnStartRewind", SendMessageOptions.DontRequireReceiver);
		}
		if (lastTimeScale < 0f && timeScale >= 0f)
		{
			SendMessage("OnStopRewind", SendMessageOptions.DontRequireReceiver);
		}
		if ((lastTimeScale <= 0f || lastTimeScale >= 1f) && timeScale > 0f && timeScale < 1f)
		{
			SendMessage("OnStartSlowDown", SendMessageOptions.DontRequireReceiver);
		}
		if (lastTimeScale > 0f && lastTimeScale < 1f && (timeScale <= 0f || timeScale >= 1f))
		{
			SendMessage("OnStopSlowDown", SendMessageOptions.DontRequireReceiver);
		}
		if (lastTimeScale <= 1f && timeScale > 1f)
		{
			SendMessage("OnStartFastForward", SendMessageOptions.DontRequireReceiver);
		}
		if (lastTimeScale > 1f && timeScale <= 1f)
		{
			SendMessage("OnStopFastForward", SendMessageOptions.DontRequireReceiver);
		}
	}

	protected virtual void RecordSmoothing()
	{
		if (deltaTime != 0f)
		{
			previousDeltaTimes.Enqueue(deltaTime);
		}
		if (previousDeltaTimes.Count > smoothingDeltas)
		{
			previousDeltaTimes.Dequeue();
		}
	}

	protected void TriggerForwardOccurrences()
	{
		handledOccurrences.Clear();
		while (nextForwardOccurrence != null && nextForwardOccurrence.time <= time)
		{
			nextForwardOccurrence.Forward();
			handledOccurrences.Add(nextForwardOccurrence);
			nextBackwardOccurrence = nextForwardOccurrence;
			nextForwardOccurrence = OccurrenceAfter(nextForwardOccurrence.time, handledOccurrences);
		}
	}

	protected void TriggerBackwardOccurrences()
	{
		handledOccurrences.Clear();
		while (nextBackwardOccurrence != null && nextBackwardOccurrence.time >= time)
		{
			nextBackwardOccurrence.Backward();
			if (nextBackwardOccurrence.repeatable)
			{
				handledOccurrences.Add(nextBackwardOccurrence);
				nextForwardOccurrence = nextBackwardOccurrence;
			}
			else
			{
				occurrences.Remove(nextBackwardOccurrence);
			}
			nextBackwardOccurrence = OccurrenceBefore(nextBackwardOccurrence.time, handledOccurrences);
		}
	}

	protected Occurrence OccurrenceAfter(float time, params Occurrence[] ignored)
	{
		return OccurrenceAfter(time, (IEnumerable<Occurrence>)ignored);
	}

	protected Occurrence OccurrenceAfter(float time, IEnumerable<Occurrence> ignored)
	{
		Occurrence occurrence = null;
		for (int i = 0; i < occurrences.Count; i++)
		{
			Occurrence occurrence2 = occurrences[i];
			if (occurrence2.time >= time && !ignored.Contains(occurrence2) && (occurrence == null || occurrence2.time < occurrence.time))
			{
				occurrence = occurrence2;
			}
		}
		return occurrence;
	}

	protected Occurrence OccurrenceBefore(float time, params Occurrence[] ignored)
	{
		return OccurrenceBefore(time, (IEnumerable<Occurrence>)ignored);
	}

	protected Occurrence OccurrenceBefore(float time, IEnumerable<Occurrence> ignored)
	{
		Occurrence occurrence = null;
		for (int i = 0; i < occurrences.Count; i++)
		{
			Occurrence occurrence2 = occurrences[i];
			if (occurrence2.time <= time && !ignored.Contains(occurrence2) && (occurrence == null || occurrence2.time > occurrence.time))
			{
				occurrence = occurrence2;
			}
		}
		return occurrence;
	}

	protected virtual void PlaceOccurence(Occurrence occurrence, float time)
	{
		if (time == this.time)
		{
			if (timeScale >= 0f)
			{
				occurrence.Forward();
				nextBackwardOccurrence = occurrence;
			}
			else
			{
				occurrence.Backward();
				nextForwardOccurrence = occurrence;
			}
		}
		else if (time > this.time)
		{
			if (nextForwardOccurrence == null || nextForwardOccurrence.time > time)
			{
				nextForwardOccurrence = occurrence;
			}
		}
		else if (time < this.time && (nextBackwardOccurrence == null || nextBackwardOccurrence.time < time))
		{
			nextBackwardOccurrence = occurrence;
		}
	}

	public virtual Occurrence Schedule(float time, bool repeatable, Occurrence occurrence)
	{
		occurrence.time = time;
		occurrence.repeatable = repeatable;
		occurrences.Add(occurrence);
		PlaceOccurence(occurrence, time);
		return occurrence;
	}

	public Occurrence Do(bool repeatable, Occurrence occurrence)
	{
		return Schedule(time, repeatable, occurrence);
	}

	public Occurrence Plan(float delay, bool repeatable, Occurrence occurrence)
	{
		if (delay <= 0f)
		{
			throw new ChronosException("Planned occurrences must be in the future.");
		}
		return Schedule(time + delay, repeatable, occurrence);
	}

	public Occurrence Memory(float delay, bool repeatable, Occurrence occurrence)
	{
		if (delay >= 0f)
		{
			throw new ChronosException("Memory occurrences must be in the past.");
		}
		return Schedule(time + delay, repeatable, occurrence);
	}

	public Occurrence Schedule<T>(float time, bool repeatable, ForwardFunc<T> forward, BackwardFunc<T> backward)
	{
		return Schedule(time, repeatable, new FuncOccurence<T>(forward, backward));
	}

	public Occurrence Do<T>(bool repeatable, ForwardFunc<T> forward, BackwardFunc<T> backward)
	{
		return Do(repeatable, new FuncOccurence<T>(forward, backward));
	}

	public Occurrence Plan<T>(float delay, bool repeatable, ForwardFunc<T> forward, BackwardFunc<T> backward)
	{
		return Plan(delay, repeatable, new FuncOccurence<T>(forward, backward));
	}

	public Occurrence Memory<T>(float delay, bool repeatable, ForwardFunc<T> forward, BackwardFunc<T> backward)
	{
		return Memory(delay, repeatable, new FuncOccurence<T>(forward, backward));
	}

	public Occurrence Schedule(float time, bool repeatable, ForwardAction forward, BackwardAction backward)
	{
		return Schedule(time, repeatable, new ActionOccurence(forward, backward));
	}

	public Occurrence Do(bool repeatable, ForwardAction forward, BackwardAction backward)
	{
		return Do(repeatable, new ActionOccurence(forward, backward));
	}

	public Occurrence Plan(float delay, bool repeatable, ForwardAction forward, BackwardAction backward)
	{
		return Plan(delay, repeatable, new ActionOccurence(forward, backward));
	}

	public Occurrence Memory(float delay, bool repeatable, ForwardAction forward, BackwardAction backward)
	{
		return Memory(delay, repeatable, new ActionOccurence(forward, backward));
	}

	public Occurrence Schedule(float time, ForwardAction forward)
	{
		return Schedule(time, repeatable: false, new ForwardActionOccurence(forward));
	}

	public Occurrence Plan(float delay, ForwardAction forward)
	{
		return Plan(delay, repeatable: false, new ForwardActionOccurence(forward));
	}

	public Occurrence Memory(float delay, ForwardAction forward)
	{
		return Memory(delay, repeatable: false, new ForwardActionOccurence(forward));
	}

	public void Cancel(Occurrence occurrence)
	{
		if (!occurrences.Contains(occurrence))
		{
			throw new ChronosException("Occurrence to cancel not found on timeline.");
		}
		if (occurrence == nextForwardOccurrence)
		{
			nextForwardOccurrence = OccurrenceAfter(occurrence.time, occurrence);
		}
		if (occurrence == nextBackwardOccurrence)
		{
			nextBackwardOccurrence = OccurrenceBefore(occurrence.time, occurrence);
		}
		occurrences.Remove(occurrence);
	}

	public bool TryCancel(Occurrence occurrence)
	{
		if (!occurrences.Contains(occurrence))
		{
			return false;
		}
		Cancel(occurrence);
		return true;
	}

	public void Reschedule(Occurrence occurrence, float time)
	{
		occurrence.time = time;
		PlaceOccurence(occurrence, time);
	}

	public void Postpone(Occurrence occurrence, float delay)
	{
		Reschedule(occurrence, time + delay);
	}

	public void Prepone(Occurrence occurrence, float delay)
	{
		Reschedule(occurrence, time - delay);
	}

	public Coroutine WaitForSeconds(float seconds)
	{
		return StartCoroutine(WaitingForSeconds(seconds));
	}

	protected IEnumerator WaitingForSeconds(float seconds)
	{
		float start = time;
		while (time < start + seconds)
		{
			yield return null;
		}
	}
}
