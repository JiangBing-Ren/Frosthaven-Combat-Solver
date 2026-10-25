using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Chronos;

public abstract class RecorderTimeline<TComponent, TSnapshot> : ComponentTimeline<TComponent>, IRecorder where TComponent : Component
{
	public delegate TSnapshot SnapshotModifier(TSnapshot snapshot, float time);

	protected List<TSnapshot> snapshots;

	protected List<float> times;

	protected int capacity;

	protected float recordingTimer;

	protected float lastTimeScale = 1f;

	protected bool canRewind;

	protected TSnapshot laterSnapshot;

	protected float laterTime;

	protected TSnapshot earlierSnapshot;

	protected float earlierTime;

	protected TSnapshot interpolatedSnapshot;

	public bool exhaustedRewind => !canRewind;

	public float availableRewindDuration
	{
		get
		{
			if (exhaustedRewind || times.Count == 0)
			{
				return 0f;
			}
			return Mathf.Max(0f, timeline.time - times[0]);
		}
	}

	public RecorderTimeline(Timeline timeline, TComponent component)
		: base(timeline, component)
	{
		snapshots = new List<TSnapshot>();
		times = new List<float>();
	}

	public override void OnStartOrReEnable()
	{
		Reset();
	}

	public override void Update()
	{
		float timeScale = timeline.timeScale;
		if (lastTimeScale >= 0f && timeScale < 0f)
		{
			laterSnapshot = CopySnapshot();
			laterTime = timeline.time;
			interpolatedSnapshot = laterSnapshot;
			canRewind = TryFindEarlierSnapshot(pop: false);
		}
		if (timeScale > 0f)
		{
			Progress();
		}
		else if (timeScale < 0f)
		{
			Rewind();
		}
		lastTimeScale = timeScale;
	}

	protected void Progress()
	{
		if (recordingTimer >= timeline.recordingInterval)
		{
			Record();
			recordingTimer = 0f;
		}
		recordingTimer += timeline.deltaTime;
	}

	public void Record()
	{
		if (timeline.rewindable)
		{
			if (snapshots.Count == capacity)
			{
				snapshots.RemoveAt(0);
				times.RemoveAt(0);
			}
			snapshots.Add(CopySnapshot());
			times.Add(timeline.time);
			canRewind = true;
		}
	}

	protected void Rewind()
	{
		if (!canRewind)
		{
			return;
		}
		if (timeline.time <= earlierTime)
		{
			canRewind = TryFindEarlierSnapshot(pop: true);
			if (!canRewind)
			{
				interpolatedSnapshot = earlierSnapshot;
				ApplySnapshot(interpolatedSnapshot);
				timeline.SendMessage("OnExhaustRewind", SendMessageOptions.DontRequireReceiver);
				return;
			}
		}
		float t = (laterTime - timeline.time) / (laterTime - earlierTime);
		interpolatedSnapshot = LerpSnapshots(laterSnapshot, earlierSnapshot, t);
		ApplySnapshot(interpolatedSnapshot);
	}

	protected void OnExhaustRewind()
	{
		if (Timekeeper.instance.debug)
		{
			Debug.LogWarning("Reached rewind limit.");
		}
	}

	protected abstract TSnapshot LerpSnapshots(TSnapshot from, TSnapshot to, float t);

	protected abstract TSnapshot CopySnapshot();

	protected abstract void ApplySnapshot(TSnapshot snapshot);

	protected bool TryFindEarlierSnapshot(bool pop)
	{
		if (pop)
		{
			if (snapshots.Count < 1)
			{
				return false;
			}
			laterSnapshot = snapshots[snapshots.Count - 1];
			laterTime = times[times.Count - 1];
			snapshots.RemoveAt(snapshots.Count - 1);
			times.RemoveAt(times.Count - 1);
		}
		if (snapshots.Count < 1)
		{
			return false;
		}
		earlierSnapshot = snapshots[snapshots.Count - 1];
		earlierTime = times[times.Count - 1];
		return true;
	}

	public override void Reset()
	{
		lastTimeScale = 1f;
		if (timeline.recordingDuration < timeline.recordingInterval)
		{
			throw new ChronosException("The recording duration must be longer than or equal to interval.");
		}
		if (timeline.recordingInterval <= 0f)
		{
			throw new ChronosException("The recording interval must be positive.");
		}
		snapshots.Clear();
		times.Clear();
		capacity = Mathf.CeilToInt(timeline.recordingDuration / timeline.recordingInterval);
		snapshots.Capacity = capacity;
		times.Capacity = capacity;
		recordingTimer = 0f;
		Record();
	}

	public virtual void ModifySnapshots(SnapshotModifier modifier)
	{
		for (int i = 0; i < snapshots.Count; i++)
		{
			snapshots[i] = modifier(snapshots[i], times[i]);
		}
	}

	internal static int EstimateMemoryUsage(float duration, float interval)
	{
		int num = Marshal.SizeOf(typeof(TSnapshot));
		int num2 = Mathf.CeilToInt(duration / interval);
		int num3;
		for (num3 = 1; num3 < num2; num3 *= 2)
		{
		}
		int size = IntPtr.Size;
		return num * num2 + size * num3;
	}

	public int EstimateMemoryUsage()
	{
		return EstimateMemoryUsage(timeline.recordingDuration, timeline.recordingInterval);
	}
}
