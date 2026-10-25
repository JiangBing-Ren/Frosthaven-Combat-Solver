using System;
using FH.LevelComponents.ProcGen;
using UnityEngine;

public class TallCharacter : MonoBehaviour, ICharacterOccluder, IOccluder<bool>
{
	private Action onDestroy;

	private const float Size = 1f;

	public Vector2 Front
	{
		get
		{
			Transform transform = base.transform;
			Vector3 vector = transform.position + transform.forward * 1f;
			return new Vector2(vector.x, vector.z);
		}
	}

	public Vector2 Back
	{
		get
		{
			Transform transform = base.transform;
			Vector3 vector = transform.position - transform.forward * 1f;
			return new Vector2(vector.x, vector.z);
		}
	}

	public Vector2 Left
	{
		get
		{
			Transform transform = base.transform;
			Vector3 vector = transform.position - transform.right * 1f;
			return new Vector2(vector.x, vector.z);
		}
	}

	public Vector2 Right
	{
		get
		{
			Transform transform = base.transform;
			Vector3 vector = transform.position + transform.right * 1f;
			return new Vector2(vector.x, vector.z);
		}
	}

	public void SetOnDestroyCallback(Action callback)
	{
		onDestroy = callback;
	}

	public void OnUpdatedOcclusionCheck(bool occlusionResult)
	{
	}

	private void OnDestroy()
	{
		onDestroy?.Invoke();
	}
}
