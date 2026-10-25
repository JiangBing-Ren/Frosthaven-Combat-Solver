using System;
using UnityEngine;

[Serializable]
public class LocationConfigUI
{
	public string location;

	public Vector3 locationMarkerOffset;

	[Header("Box collider")]
	[SerializeField]
	private Vector3 locationColliderCenter;

	[SerializeField]
	private bool overrideSizeCollider;

	[SerializeField]
	private Vector3 locationColliderSize;

	public Vector3? ColliderSize
	{
		get
		{
			if (!overrideSizeCollider)
			{
				return null;
			}
			return locationColliderSize;
		}
	}

	public Vector3 ColliderCenter => locationColliderCenter;
}
