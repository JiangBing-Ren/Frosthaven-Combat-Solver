using System;
using UnityEngine;

namespace FH.LevelComponents.ProcGen;

public class NaMitkoPlo4kite : MonoBehaviour
{
	[Serializable]
	public class Definition
	{
		[Header("Contains the following in the name:")]
		public string LevelName;

		[Header("Or:")]
		public string RoomName;

		public bool InWalls = true;

		public GameObject EnableVariant;
	}

	[SerializeField]
	private GameObject _default;

	[SerializeField]
	private Definition[] _definitions;

	public void OnEnable()
	{
		if (!(Choreographer.s_Choreographer == null) && Choreographer.s_Choreographer.m_CurrentState != null)
		{
			Init();
		}
	}

	public void Init()
	{
		if (Choreographer.s_Choreographer == null || Choreographer.s_Choreographer.m_CurrentState == null)
		{
			return;
		}
		if (_default == null)
		{
			if (transform.childCount == 0)
			{
				Debug.LogError("Object " + name + " has no children, but children were expected.");
				return;
			}
			Transform child = transform.GetChild(0);
			_default = child.gameObject;
		}
		string bakingID = Choreographer.s_Choreographer.m_CurrentState.BakingID;
		ProceduralMapTile componentInParent = transform.GetComponentInParent<ProceduralMapTile>();
		string text = ((componentInParent != null) ? componentInParent.name : "");
		bool flag = false;
		bool flag2 = transform.parent.parent.name.Contains("Wall") || GetComponentInParent<ProceduralWall>() != null;
		Definition[] definitions = _definitions;
		foreach (Definition definition in definitions)
		{
			if ((bool)definition.EnableVariant)
			{
				definition.EnableVariant.SetActive(value: false);
				if ((!string.IsNullOrEmpty(definition.LevelName) || !string.IsNullOrEmpty(definition.RoomName)) && ((!string.IsNullOrEmpty(definition.LevelName) && bakingID.Contains(definition.LevelName)) || (!string.IsNullOrEmpty(definition.RoomName) && text.Contains(definition.RoomName))) && ((flag2 && definition.InWalls) || (!definition.InWalls && !flag2)))
				{
					flag = true;
					definition.EnableVariant.SetActive(value: true);
					break;
				}
			}
		}
		_default.SetActive(!flag);
	}
}
