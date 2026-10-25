using System.Collections.Generic;
using ScenarioRuleLibrary;
using UnityEngine;

[StateDump(UseToString = true)]
public class CClientTile
{
	public GameObject m_GameObject;

	public TileBehaviour m_TileBehaviour;

	public GameObject m_GameObject1;

	public TileBehaviour m_TileBehaviour1;

	public CTile m_Tile;

	public List<GameObject> m_Items = new List<GameObject>();

	public override string ToString()
	{
		if (m_Tile == null)
		{
			return GetType().Name + "(null)";
		}
		return $"{GetType().Name}({m_Tile.m_ArrayIndex})";
	}
}
