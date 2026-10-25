using System.Linq;
using FH.LevelComponents.ProcGen;
using ScenarioRuleLibrary;
using UnityEngine;

public class TerrainLamp : MonoBehaviour
{
	public void Init()
	{
		if (!ProceduralMapTile.IsInLevelEditorMode)
		{
			Light[] componentsInChildren = GetComponentsInChildren<Light>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].transform.SetParent(transform.parent);
			}
			Choreographer.s_Choreographer.m_CurrentState.Props.First((CObjectProp a) => a.InstanceName == name).DestroyProp();
		}
	}
}
