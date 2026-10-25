using UnityEngine;
using UnityEngine.AI;

namespace Chronos;

public class RigidbodyTimeline3D : RigidbodyTimeline<Rigidbody, RigidbodyTimeline3D.Snapshot>
{
	public struct Snapshot
	{
		public Vector3 position;

		public Quaternion rotation;

		public Vector3 velocity;

		public Vector3 angularVelocity;

		public float drag;

		public float angularDrag;

		public float lastPositiveTimeScale;

		public static Snapshot Lerp(Snapshot from, Snapshot to, float t)
		{
			return new Snapshot
			{
				position = Vector3.Lerp(from.position, to.position, t),
				rotation = Quaternion.Lerp(from.rotation, to.rotation, t),
				velocity = Vector3.Lerp(from.velocity, to.velocity, t),
				angularVelocity = Vector3.Lerp(from.angularVelocity, to.angularVelocity, t),
				drag = Mathf.Lerp(from.drag, to.drag, t),
				angularDrag = Mathf.Lerp(from.angularDrag, to.angularDrag, t),
				lastPositiveTimeScale = Mathf.Lerp(from.lastPositiveTimeScale, to.lastPositiveTimeScale, t)
			};
		}
	}

	private bool _isKinematic;

	protected bool reallyUsesGravity
	{
		get
		{
			return component.useGravity;
		}
		set
		{
			component.useGravity = value;
		}
	}

	private bool isReallyKinematic
	{
		get
		{
			return component.isKinematic;
		}
		set
		{
			component.isKinematic = value;
		}
	}

	protected override bool isReallyManual
	{
		get
		{
			return isReallyKinematic;
		}
		set
		{
			isReallyKinematic = value;
		}
	}

	protected override float realMass
	{
		get
		{
			return component.mass;
		}
		set
		{
			component.mass = value;
		}
	}

	protected override Vector3 realVelocity
	{
		get
		{
			return component.velocity;
		}
		set
		{
			component.velocity = value;
		}
	}

	protected override Vector3 realAngularVelocity
	{
		get
		{
			return component.angularVelocity;
		}
		set
		{
			component.angularVelocity = value;
		}
	}

	protected override float realDrag
	{
		get
		{
			return component.drag;
		}
		set
		{
			component.drag = value;
		}
	}

	protected override float realAngularDrag
	{
		get
		{
			return component.angularDrag;
		}
		set
		{
			component.angularDrag = value;
		}
	}

	public bool isKinematic
	{
		get
		{
			return _isKinematic;
		}
		set
		{
			_isKinematic = value;
			if (timeline.timeScale > 0f)
			{
				isReallyKinematic = value;
			}
		}
	}

	protected override bool isManual => isKinematic;

	public bool useGravity { get; set; }

	public Vector3 velocity
	{
		get
		{
			if (timeline.timeScale == 0f)
			{
				return zeroSnapshot.velocity;
			}
			return realVelocity / timeline.timeScale;
		}
		set
		{
			if (AssertForwardProperty("velocity", Severity.Ignore))
			{
				realVelocity = value * timeline.timeScale;
			}
		}
	}

	public Vector3 angularVelocity
	{
		get
		{
			if (timeline.timeScale == 0f)
			{
				return zeroSnapshot.angularVelocity;
			}
			return realAngularVelocity / timeline.timeScale;
		}
		set
		{
			if (AssertForwardProperty("angularVelocity", Severity.Ignore))
			{
				realAngularVelocity = value * timeline.timeScale;
			}
		}
	}

	public RigidbodyTimeline3D(Timeline timeline, Rigidbody component)
		: base(timeline, component)
	{
	}

	public override void CopyProperties(Rigidbody source)
	{
		isKinematic = source.isKinematic;
		useGravity = source.useGravity;
		source.useGravity = false;
	}

	public override void FixedUpdate()
	{
		if (useGravity && !component.isKinematic && timeline.timeScale > 0f)
		{
			velocity += Physics.gravity * timeline.fixedDeltaTime;
		}
	}

	protected override Snapshot LerpSnapshots(Snapshot from, Snapshot to, float t)
	{
		return Snapshot.Lerp(from, to, t);
	}

	private NavMeshAgent GetNavMeshAgentOverride()
	{
		if (timeline.navMeshAgent == null || timeline.navMeshAgent.component == null || !timeline.navMeshAgent.component.enabled)
		{
			return null;
		}
		return timeline.navMeshAgent.component;
	}

	protected override Snapshot CopySnapshot()
	{
		NavMeshAgent navMeshAgentOverride = GetNavMeshAgentOverride();
		return new Snapshot
		{
			position = component.transform.position,
			rotation = component.transform.rotation,
			velocity = ((navMeshAgentOverride == null) ? component.velocity : navMeshAgentOverride.velocity),
			angularVelocity = component.angularVelocity,
			drag = component.drag,
			angularDrag = component.angularDrag,
			lastPositiveTimeScale = lastPositiveTimeScale
		};
	}

	protected override void ApplySnapshot(Snapshot snapshot)
	{
		NavMeshAgent navMeshAgentOverride = GetNavMeshAgentOverride();
		if (navMeshAgentOverride == null)
		{
			component.transform.position = snapshot.position;
		}
		else
		{
			navMeshAgentOverride.Warp(snapshot.position);
		}
		component.transform.rotation = snapshot.rotation;
		if (timeline.timeScale > 0f)
		{
			if (navMeshAgentOverride == null)
			{
				component.velocity = snapshot.velocity;
			}
			else
			{
				navMeshAgentOverride.velocity = snapshot.velocity;
			}
			component.angularVelocity = snapshot.angularVelocity;
		}
		component.drag = snapshot.drag;
		component.angularDrag = snapshot.angularDrag;
		lastPositiveTimeScale = snapshot.lastPositiveTimeScale;
	}

	protected override void WakeUp()
	{
		component.WakeUp();
	}

	protected override bool IsSleeping()
	{
		return component.IsSleeping();
	}

	public void AddForce(Vector3 force, ForceMode mode = ForceMode.Force)
	{
		if (AssertForwardForce(Severity.Ignore))
		{
			component.AddForce(AdjustForce(force), mode);
		}
	}

	public void AddRelativeForce(Vector3 force, ForceMode mode = ForceMode.Force)
	{
		if (AssertForwardForce(Severity.Ignore))
		{
			component.AddRelativeForce(AdjustForce(force), mode);
		}
	}

	public void AddForceAtPosition(Vector3 force, Vector3 position, ForceMode mode = ForceMode.Force)
	{
		if (AssertForwardForce(Severity.Ignore))
		{
			component.AddForceAtPosition(AdjustForce(force), position, mode);
		}
	}

	public void AddExplosionForce(float explosionForce, Vector3 explosionPosition, float explosionRadius, float upwardsModifier = 0f, ForceMode mode = ForceMode.Force)
	{
		if (AssertForwardForce(Severity.Ignore))
		{
			component.AddExplosionForce(AdjustForce(explosionForce), explosionPosition, explosionRadius, upwardsModifier, mode);
		}
	}

	public void AddTorque(Vector3 torque, ForceMode mode = ForceMode.Force)
	{
		if (AssertForwardForce(Severity.Ignore))
		{
			component.AddTorque(AdjustForce(torque), mode);
		}
	}

	public void AddRelativeTorque(Vector3 torque, ForceMode mode = ForceMode.Force)
	{
		if (AssertForwardForce(Severity.Ignore))
		{
			component.AddRelativeTorque(AdjustForce(torque), mode);
		}
	}
}
