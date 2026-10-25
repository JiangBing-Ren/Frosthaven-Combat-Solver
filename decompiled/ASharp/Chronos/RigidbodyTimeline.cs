using UnityEngine;
using UnityEngine.AI;

namespace Chronos;

public abstract class RigidbodyTimeline<TComponent, TSnapshot> : RecorderTimeline<TComponent, TSnapshot>, IRigidbodyTimeline where TComponent : Component
{
	protected float lastPositiveTimeScale = 1f;

	protected TSnapshot zeroSnapshot;

	protected float zeroTime;

	protected Vector3 zeroDestination;

	protected abstract bool isReallyManual { get; set; }

	protected abstract float realMass { get; set; }

	protected abstract Vector3 realVelocity { get; set; }

	protected abstract Vector3 realAngularVelocity { get; set; }

	protected abstract float realDrag { get; set; }

	protected abstract float realAngularDrag { get; set; }

	protected abstract bool isManual { get; }

	public float mass
	{
		get
		{
			return realMass;
		}
		set
		{
			realMass = value;
		}
	}

	public float drag
	{
		get
		{
			return realDrag / timeline.timeScale;
		}
		set
		{
			if (AssertForwardProperty("drag", Severity.Warn))
			{
				realDrag = value * timeline.timeScale;
			}
		}
	}

	public float angularDrag
	{
		get
		{
			return realAngularDrag / timeline.timeScale;
		}
		set
		{
			if (AssertForwardProperty("angularDrag", Severity.Warn))
			{
				realAngularDrag = value * timeline.timeScale;
			}
		}
	}

	public RigidbodyTimeline(Timeline timeline, TComponent component)
		: base(timeline, component)
	{
	}

	public override void Update()
	{
		float timeScale = timeline.timeScale;
		if (lastTimeScale != 0f && timeScale == 0f)
		{
			if (lastTimeScale > 0f)
			{
				zeroSnapshot = CopySnapshot();
				NavMeshAgent navMeshAgent = component.GetComponent<NavMeshAgent>();
				if (navMeshAgent != null)
				{
					zeroDestination = navMeshAgent.destination;
				}
			}
			else if (lastTimeScale < 0f && timeline.rewindable)
			{
				zeroSnapshot = interpolatedSnapshot;
			}
			zeroTime = timeline.time;
		}
		if (lastTimeScale >= 0f && timeScale <= 0f)
		{
			if (timeScale < 0f)
			{
				laterSnapshot = CopySnapshot();
				laterTime = timeline.time;
				interpolatedSnapshot = laterSnapshot;
				canRewind = TryFindEarlierSnapshot(pop: false);
			}
			isReallyManual = true;
		}
		else if (lastTimeScale <= 0f && timeScale > 0f)
		{
			isReallyManual = isManual;
			if (lastTimeScale == 0f)
			{
				ApplySnapshot(zeroSnapshot);
				NavMeshAgent navMeshAgent2 = component.GetComponent<NavMeshAgent>();
				if (navMeshAgent2 != null)
				{
					navMeshAgent2.destination = zeroDestination;
				}
			}
			else if (lastTimeScale < 0f && timeline.rewindable)
			{
				ApplySnapshot(interpolatedSnapshot);
			}
			WakeUp();
			Record();
		}
		if (timeScale > 0f && timeScale != lastTimeScale && !isReallyManual)
		{
			float num = timeScale / lastPositiveTimeScale;
			realVelocity *= num;
			realAngularVelocity *= num;
			realDrag *= num;
			realAngularDrag *= num;
			WakeUp();
		}
		if (timeScale > 0f)
		{
			isReallyManual = isManual;
			Progress();
		}
		else if (timeScale < 0f)
		{
			Rewind();
		}
		lastTimeScale = timeScale;
		if (timeScale > 0f)
		{
			lastPositiveTimeScale = timeScale;
		}
	}

	public override void Reset()
	{
		lastPositiveTimeScale = 1f;
		base.Reset();
	}

	public override void ModifySnapshots(SnapshotModifier modifier)
	{
		base.ModifySnapshots(modifier);
		zeroSnapshot = modifier(zeroSnapshot, zeroTime);
	}

	protected abstract bool IsSleeping();

	protected abstract void WakeUp();

	protected virtual float AdjustForce(float force)
	{
		return force * timeline.timeScale;
	}

	protected virtual Vector2 AdjustForce(Vector2 force)
	{
		return force * timeline.timeScale;
	}

	protected virtual Vector3 AdjustForce(Vector3 force)
	{
		return force * timeline.timeScale;
	}

	protected bool AssertForwardProperty(string property, Severity severity)
	{
		if (timeline.timeScale <= 0f)
		{
			switch (severity)
			{
			case Severity.Error:
				throw new ChronosException("Cannot change the " + property + " of the rigidbody while time is paused or rewinding.");
			case Severity.Warn:
				Debug.LogWarning("Trying to change the " + property + " of the rigidbody while time is paused or rewinding, ignoring.");
				break;
			}
		}
		return timeline.timeScale > 0f;
	}

	protected bool AssertForwardForce(Severity severity)
	{
		if (timeline.timeScale <= 0f)
		{
			switch (severity)
			{
			case Severity.Error:
				throw new ChronosException("Cannot apply a force to the rigidbody while time is paused or rewinding.");
			case Severity.Warn:
				Debug.LogWarning("Trying to apply a force to the rigidbody while time is paused or rewinding, ignoring.");
				break;
			}
		}
		return timeline.timeScale > 0f;
	}
}
