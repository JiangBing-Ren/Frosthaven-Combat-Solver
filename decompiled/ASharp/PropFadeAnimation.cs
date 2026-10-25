using System;
using System.Collections.Generic;
using System.Linq;
using Libs.Utils;
using SETUtil.Materials;
using ScenarioRuleLibrary;
using UnityEngine;

public class PropFadeAnimation : MonoBehaviour
{
	private enum SystemStatus
	{
		Idle,
		Starting,
		Running,
		Stopping
	}

	private class ActorData
	{
		internal enum ActorStatus
		{
			WaitingForEntry,
			Entering,
			Inside,
			Leaving,
			Outside
		}

		public ActorStatus Status;

		public float Influence;
	}

	private readonly Dictionary<Transform, ActorData> _overlappingActors = new Dictionary<Transform, ActorData>();

	private MeshRenderer[] _childMeshes;

	private SystemStatus _status;

	private Vector3 _calculatedPosition = Vector3.zero;

	private float _calculatedRadius;

	private float _totalInfluence;

	private Vector3[] _relatedTiles;

	private float _farthestActorSqrdDistance;

	private static readonly int OverlapFadePosition = Shader.PropertyToID("_OverlapFade_Position");

	private static readonly int OverlapFadeStrength = Shader.PropertyToID("_OverlapFade_Strength");

	private static readonly int OverlapFadeInverseRadius = Shader.PropertyToID("_OverlapFade_InverseRadius");

	private const float LERP_SPEED = 10f;

	private const float ACTOR_RADIUS_SINGLE = 1.8f;

	private const float SQRD_ACTOR_RADIUS_SINGLE = 3.2399998f;

	private const float ENTER_ACTOR_DISTANCE_MODIFIER = 8f;

	private const float EXIT_ACTOR_DISTANCE_MODIFIER = 0.3f;

	public Action<PropFadeAnimation> OnSystemStopped;

	private HashSet<Material> _materialInstances = new HashSet<Material>();

	private void Awake()
	{
		_childMeshes = GetComponentsInChildren<MeshRenderer>();
		MeshRenderer[] childMeshes = _childMeshes;
		foreach (MeshRenderer renderer in childMeshes)
		{
			_materialInstances.AddRange(renderer.GetMaterialClonesSlow());
		}
	}

	public void SetPropData(CObjectProp prop)
	{
		if (prop is CObjectObstacle cObjectObstacle)
		{
			_relatedTiles = new Vector3[cObjectObstacle.PathingBlockers.Count];
			for (int i = 0; i < cObjectObstacle.PathingBlockers.Count; i++)
			{
				TileIndex tileIndex = cObjectObstacle.PathingBlockers[i];
				_relatedTiles[i] = GloomUtility.CVToV(ScenarioManager.GetTile(tileIndex.X, tileIndex.Y).m_Hex.Position);
			}
		}
		else
		{
			_relatedTiles = new Vector3[1];
			_relatedTiles[0] = GloomUtility.CVToV(prop.Position);
		}
	}

	public void AddOverlappingActor(Transform actor)
	{
		if (!_overlappingActors.ContainsKey(actor))
		{
			if (_overlappingActors.Count == 0)
			{
				_calculatedPosition = actor.position;
			}
			_overlappingActors.Add(actor, new ActorData());
			SystemStatus status = _status;
			if (status == SystemStatus.Idle || status == SystemStatus.Stopping)
			{
				_status = SystemStatus.Starting;
				enabled = true;
			}
		}
	}

	public void RemoveOverlappingActor(Transform actor)
	{
		if (_overlappingActors.ContainsKey(actor))
		{
			_overlappingActors.Remove(actor);
			if (_overlappingActors.Count < 1)
			{
				_status = SystemStatus.Stopping;
			}
		}
	}

	private void Update()
	{
		if (_status == SystemStatus.Idle)
		{
			enabled = false;
			return;
		}
		UpdateActorsStatus();
		if (_status == SystemStatus.Starting && _totalInfluence >= 1f)
		{
			_status = SystemStatus.Running;
		}
		SystemStatus status = _status;
		if (status == SystemStatus.Starting || status == SystemStatus.Running)
		{
			CalculatePositionAndRadius();
			UpdateMaterials(_calculatedPosition, _calculatedRadius, Mathf.Min(_totalInfluence, 1f));
			return;
		}
		_totalInfluence = Mathf.Lerp(_totalInfluence, 0f, Time.deltaTime * 10f);
		if (_totalInfluence < 0.01f)
		{
			_totalInfluence = 0f;
			_status = SystemStatus.Idle;
			enabled = false;
			OnSystemStopped?.Invoke(this);
		}
		UpdateMaterials(_totalInfluence);
	}

	private bool CheckActorIsOnTiles(Vector3 actorPosition, float modifier)
	{
		Vector3[] relatedTiles = _relatedTiles;
		for (int i = 0; i < relatedTiles.Length; i++)
		{
			float num = Vector3.SqrMagnitude(relatedTiles[i] - actorPosition);
			_farthestActorSqrdDistance = Mathf.Max(_farthestActorSqrdDistance, num);
			if (num < 3.2399998f * modifier)
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateActorsStatus()
	{
		_farthestActorSqrdDistance = 0f;
		List<Transform> list = null;
		foreach (KeyValuePair<Transform, ActorData> overlappingActor in _overlappingActors)
		{
			float modifier = ((overlappingActor.Value.Status == ActorData.ActorStatus.WaitingForEntry) ? 8f : 0.3f);
			bool flag = CheckActorIsOnTiles(overlappingActor.Key.position, modifier);
			float num = overlappingActor.Value.Influence;
			switch (overlappingActor.Value.Status)
			{
			case ActorData.ActorStatus.WaitingForEntry:
				if (flag)
				{
					overlappingActor.Value.Status = ActorData.ActorStatus.Entering;
				}
				break;
			case ActorData.ActorStatus.Entering:
				num = Mathf.Lerp(num, 1f, Time.deltaTime * 10f);
				if (num >= 0.99f)
				{
					num = 1f;
					overlappingActor.Value.Status = ActorData.ActorStatus.Inside;
				}
				break;
			case ActorData.ActorStatus.Inside:
				if (!flag)
				{
					overlappingActor.Value.Status = ActorData.ActorStatus.Leaving;
				}
				break;
			case ActorData.ActorStatus.Leaving:
				if (flag)
				{
					overlappingActor.Value.Status = ActorData.ActorStatus.Entering;
				}
				num = Mathf.Lerp(overlappingActor.Value.Influence, 0f, Time.deltaTime * 10f);
				if (num <= 0.01f)
				{
					num = 0f;
					overlappingActor.Value.Status = ActorData.ActorStatus.Outside;
				}
				overlappingActor.Value.Influence = num;
				break;
			case ActorData.ActorStatus.Outside:
				if (flag)
				{
					overlappingActor.Value.Status = ActorData.ActorStatus.Entering;
					break;
				}
				if (list == null)
				{
					list = new List<Transform>();
				}
				list.Add(overlappingActor.Key);
				continue;
			}
			overlappingActor.Value.Influence = num;
		}
		if (list == null)
		{
			return;
		}
		foreach (Transform item in list)
		{
			RemoveOverlappingActor(item);
		}
	}

	private void CalculatePositionAndRadius()
	{
		Vector3 b = Vector3.zero;
		if (_overlappingActors.Count == 1)
		{
			KeyValuePair<Transform, ActorData> keyValuePair = _overlappingActors.First();
			b = keyValuePair.Key.position;
			_totalInfluence = keyValuePair.Value.Influence;
		}
		else
		{
			_totalInfluence = 0f;
			foreach (KeyValuePair<Transform, ActorData> overlappingActor in _overlappingActors)
			{
				b += overlappingActor.Key.position;
				_totalInfluence += overlappingActor.Value.Influence;
			}
			b /= (float)_overlappingActors.Count;
		}
		_calculatedPosition = Vector3.Lerp(_calculatedPosition, b, 10f * Time.deltaTime);
		_calculatedRadius = Mathf.Lerp(_calculatedRadius, 1.8f * _totalInfluence, 20f * Time.deltaTime);
	}

	private void UpdateMaterials(Vector3 position, float radius, float strength)
	{
		float value = 1f / radius;
		foreach (Material materialInstance in _materialInstances)
		{
			if (!(materialInstance == null))
			{
				materialInstance.SetVector(OverlapFadePosition, position);
				materialInstance.SetFloat(OverlapFadeInverseRadius, value);
				materialInstance.SetFloat(OverlapFadeStrength, strength);
			}
		}
	}

	private void UpdateMaterials(float strength)
	{
		foreach (Material materialInstance in _materialInstances)
		{
			if (!(materialInstance == null))
			{
				materialInstance.SetFloat(OverlapFadeStrength, strength);
			}
		}
	}

	private void OnDestroy()
	{
		_childMeshes = null;
		_materialInstances.Clear();
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawWireSphere(_calculatedPosition, _calculatedRadius * 0.5f);
	}
}
