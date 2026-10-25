using System;
using System.Collections.Generic;
using UnityEngine;

public class RFX4_TransformMotion : MonoBehaviour
{
	public enum RFX4_SimulationSpace
	{
		Local,
		World
	}

	public class RFX4_CollisionInfo : EventArgs
	{
		public RaycastHit Hit;
	}

	public float Distance = 30f;

	public float Speed = 1f;

	public float Dampeen;

	public float MinSpeed = 1f;

	public float TimeDelay;

	public LayerMask CollidesWith = -1;

	public GameObject[] EffectsOnCollision;

	public float CollisionOffset;

	public float DestroyTimeDelay = 5f;

	public bool CollisionEffectInWorldSpace = true;

	public GameObject[] DeactivatedObjectsOnCollision;

	[HideInInspector]
	public float HUE = -1f;

	[HideInInspector]
	public List<GameObject> CollidedInstances;

	private Vector3 startPositionLocal;

	private Transform t;

	private Vector3 oldPos;

	private bool isCollided;

	private bool isOutDistance;

	private Quaternion startQuaternion;

	private float currentSpeed;

	private float currentDelay;

	private const float RayCastTolerance = 0.3f;

	private bool isInitialized;

	private bool dropFirstFrameForFixUnityBugWithParticles;

	public event EventHandler<RFX4_CollisionInfo> CollisionEnter;

	private void Start()
	{
		t = transform;
		startQuaternion = t.rotation;
		startPositionLocal = t.localPosition;
		oldPos = t.TransformPoint(startPositionLocal);
		Initialize();
		isInitialized = true;
	}

	private void OnEnable()
	{
		if (isInitialized)
		{
			Initialize();
		}
	}

	private void OnDisable()
	{
		if (isInitialized)
		{
			Initialize();
		}
	}

	private void Initialize()
	{
		isCollided = false;
		isOutDistance = false;
		currentSpeed = Speed;
		currentDelay = 0f;
		startQuaternion = t.rotation;
		t.localPosition = startPositionLocal;
		oldPos = t.TransformPoint(startPositionLocal);
		OnCollisionDeactivateBehaviour(active: true);
		dropFirstFrameForFixUnityBugWithParticles = true;
	}

	private void Update()
	{
		if (!dropFirstFrameForFixUnityBugWithParticles)
		{
			UpdateWorldPosition();
		}
		else
		{
			dropFirstFrameForFixUnityBugWithParticles = false;
		}
	}

	private void UpdateWorldPosition()
	{
		currentDelay += Time.deltaTime;
		if (!(currentDelay < TimeDelay))
		{
			Vector3 vector = Vector3.zero;
			Vector3 vector2 = Vector3.zero;
			if (!isCollided && !isOutDistance)
			{
				currentSpeed = Mathf.Clamp(currentSpeed - Speed * Dampeen * Time.deltaTime, MinSpeed, Speed);
				Vector3 vector3 = Vector3.forward * currentSpeed * Time.deltaTime;
				vector = t.localRotation * vector3;
				vector2 = startQuaternion * vector3;
			}
			float magnitude = (t.localPosition + vector - startPositionLocal).magnitude;
			if (!isCollided && Physics.Raycast(t.position, t.forward, out var hitInfo, 10f, CollidesWith) && vector.magnitude + 0.3f > hitInfo.distance)
			{
				isCollided = true;
				t.position = hitInfo.point;
				oldPos = t.position;
				OnCollisionBehaviour(hitInfo);
				OnCollisionDeactivateBehaviour(active: false);
			}
			else if (!isOutDistance && magnitude > Distance)
			{
				isOutDistance = true;
				t.localPosition = startPositionLocal + t.localRotation * Vector3.forward * Distance;
				oldPos = t.position;
			}
			else
			{
				t.position = oldPos + vector2;
				oldPos = t.position;
			}
		}
	}

	private void OnCollisionBehaviour(RaycastHit hit)
	{
		CollisionEnter?.Invoke(this, new RFX4_CollisionInfo
		{
			Hit = hit
		});
		CollidedInstances.Clear();
		GameObject[] effectsOnCollision = EffectsOnCollision;
		for (int i = 0; i < effectsOnCollision.Length; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(effectsOnCollision[i], hit.point + hit.normal * CollisionOffset, default);
			CollidedInstances.Add(gameObject);
			if (HUE > -0.9f)
			{
				RFX4_ColorHelper.ChangeObjectColorByHUE(gameObject, HUE);
			}
			gameObject.transform.LookAt(hit.point + hit.normal + hit.normal * CollisionOffset);
			if (!CollisionEffectInWorldSpace)
			{
				gameObject.transform.parent = transform;
			}
			UnityEngine.Object.Destroy(gameObject, DestroyTimeDelay);
		}
	}

	private void OnCollisionDeactivateBehaviour(bool active)
	{
		GameObject[] deactivatedObjectsOnCollision = DeactivatedObjectsOnCollision;
		for (int i = 0; i < deactivatedObjectsOnCollision.Length; i++)
		{
			deactivatedObjectsOnCollision[i].SetActive(active);
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (!Application.isPlaying)
		{
			t = transform;
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(t.position, t.position + t.forward * Distance);
		}
	}
}
