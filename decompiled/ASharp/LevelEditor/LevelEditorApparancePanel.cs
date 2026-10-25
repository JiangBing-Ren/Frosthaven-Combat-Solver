using System.Collections.Generic;
using System.Linq;
using FH.LevelComponents.ProcGen;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.UI;

namespace LevelEditor;

public class LevelEditorApparancePanel : MonoBehaviour
{
	public LevelEditorApparanceOverridePanel ApparanceOverridePanel;

	private ScenarioState _scenarioState;

	public Text bakedText;

	private void OnEnable()
	{
		InitUI(ScenarioManager.CurrentScenarioState);
	}

	public void InitUI(ScenarioState scenarioState)
	{
		_scenarioState = scenarioState;
		ApparanceOverridePanel?.SetScenarioDisplayed();
		string text = _scenarioState?.BakingID;
		bakedText.text = (string.IsNullOrEmpty(text) ? "Apparance Only" : ("Baked:" + text));
	}

	public void OnBakePressed()
	{
	}

	public void OnProcGenPressed()
	{
	}

	public void OnResetAllChildrenToInherit()
	{
		int objectCount;
		foreach (CMap map in ScenarioManager.CurrentScenarioState.Maps)
		{
			GameObject gameObject = GameObject.Find(map.MapInstanceName);
			if (gameObject != null)
			{
				CApparanceOverrideDetails cApparanceOverrideDetails = new CApparanceOverrideDetails(map.MapGuid);
				ProceduralStyle component = gameObject.GetComponent<ProceduralStyle>();
				if (component != null)
				{
					cApparanceOverrideDetails.OverrideSeed = component.Seed;
					component.Biome = ScenarioPossibleRoom.EBiome.Inherit;
					component.SubBiome = ScenarioPossibleRoom.ESubBiome.Inherit;
					component.Theme = ScenarioPossibleRoom.ETheme.Inherit;
					component.SubTheme = ScenarioPossibleRoom.ESubTheme.Inherit;
					component.Tone = ScenarioPossibleRoom.ETone.Inherit;
					component.FetchObjectHierarchyValues(out var objectIndex, out objectCount);
					cApparanceOverrideDetails.OverrideSiblingIndex = objectIndex;
					component.ForceValidate();
				}
				LevelEditorController.s_Instance.AddOrReplaceApparanceOverride(cApparanceOverrideDetails);
				ProceduralWall[] componentsInChildren = gameObject.GetComponentsInChildren<ProceduralWall>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					CApparanceOverrideDetails cApparanceOverrideDetails2 = new CApparanceOverrideDetails(CApparanceOverrideDetails.GetWallGUID(map, i));
					ProceduralStyle component2 = componentsInChildren[i].GetComponent<ProceduralStyle>();
					component2.Biome = ScenarioPossibleRoom.EBiome.Inherit;
					component2.SubBiome = ScenarioPossibleRoom.ESubBiome.Inherit;
					component2.Theme = ScenarioPossibleRoom.ETheme.Inherit;
					component2.SubTheme = ScenarioPossibleRoom.ESubTheme.Inherit;
					component2.Tone = ScenarioPossibleRoom.ETone.Inherit;
					component2.FetchObjectHierarchyValues(out var objectIndex2, out objectCount);
					cApparanceOverrideDetails2.OverrideSiblingIndex = objectIndex2;
					component2.ForceValidate();
					LevelEditorController.s_Instance.AddOrReplaceApparanceOverride(cApparanceOverrideDetails2);
				}
			}
		}
		List<UnityGameEditorObject> list = new List<UnityGameEditorObject>();
		foreach (GameObject item in UnityGameEditorRuntime.FindUnityGameObjects(LevelEditorController.MapSceneRoot, ScenarioManager.ObjectImportType.Obstacle))
		{
			UnityGameEditorObject component3 = item.GetComponent<UnityGameEditorObject>();
			if (component3 != null)
			{
				list.Add(component3);
			}
		}
		foreach (GameObject item2 in UnityGameEditorRuntime.FindUnityGameObjects(LevelEditorController.MapSceneRoot, ScenarioManager.ObjectImportType.Door))
		{
			UnityGameEditorObject component4 = item2.GetComponent<UnityGameEditorObject>();
			if (component4 != null)
			{
				list.Add(component4);
			}
		}
		foreach (CObjectProp prop in ScenarioManager.CurrentScenarioState.Props)
		{
			CApparanceOverrideDetails cApparanceOverrideDetails3 = new CApparanceOverrideDetails(prop.PropGuid);
			UnityGameEditorObject unityGameEditorObject = list.FirstOrDefault((UnityGameEditorObject p) => p != null && p.PropObject != null && p.PropObject.PropGuid == prop.PropGuid);
			if (unityGameEditorObject != null)
			{
				ProceduralStyle component5 = unityGameEditorObject.GetComponent<ProceduralStyle>();
				if (component5 != null)
				{
					cApparanceOverrideDetails3.OverrideSeed = component5.Seed;
					component5.Biome = ScenarioPossibleRoom.EBiome.Inherit;
					component5.SubBiome = ScenarioPossibleRoom.ESubBiome.Inherit;
					component5.Theme = ScenarioPossibleRoom.ETheme.Inherit;
					component5.SubTheme = ScenarioPossibleRoom.ESubTheme.Inherit;
					component5.Tone = ScenarioPossibleRoom.ETone.Inherit;
					component5.FetchObjectHierarchyValues(out var objectIndex3, out objectCount);
					cApparanceOverrideDetails3.OverrideSiblingIndex = objectIndex3;
					component5.ForceValidate();
				}
				LevelEditorController.s_Instance.AddOrReplaceApparanceOverride(cApparanceOverrideDetails3);
			}
		}
	}

	public void EnsureAllStylesHaveAnOverride(bool overrideExistingWithCurrentStyleSettings = false)
	{
		foreach (CMap map in ScenarioManager.CurrentScenarioState.Maps)
		{
			GameObject gameObject = GameObject.Find(map.MapInstanceName);
			if (gameObject != null)
			{
				ProceduralStyle component = gameObject.GetComponent<ProceduralStyle>();
				if (component != null)
				{
					LevelEditorController.s_Instance.EnsureApparanceOverrideExistsForGuidAndStyle(map.MapGuid, component, overrideExistingWithCurrentStyleSettings);
				}
				ProceduralWall[] componentsInChildren = gameObject.GetComponentsInChildren<ProceduralWall>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					LevelEditorController.s_Instance.EnsureApparanceOverrideExistsForGuidAndStyle(CApparanceOverrideDetails.GetWallGUID(map, i), componentsInChildren[i].GetComponent<ProceduralStyle>(), overrideExistingWithCurrentStyleSettings);
				}
			}
		}
		List<UnityGameEditorObject> list = new List<UnityGameEditorObject>();
		foreach (GameObject item in UnityGameEditorRuntime.FindUnityGameObjects(LevelEditorController.MapSceneRoot, ScenarioManager.ObjectImportType.Obstacle))
		{
			UnityGameEditorObject component2 = item.GetComponent<UnityGameEditorObject>();
			if (component2 != null)
			{
				list.Add(component2);
			}
		}
		foreach (GameObject item2 in UnityGameEditorRuntime.FindUnityGameObjects(LevelEditorController.MapSceneRoot, ScenarioManager.ObjectImportType.Resource))
		{
			UnityGameEditorObject component3 = item2.GetComponent<UnityGameEditorObject>();
			if (component3 != null)
			{
				list.Add(component3);
			}
		}
		foreach (GameObject item3 in UnityGameEditorRuntime.FindUnityGameObjects(LevelEditorController.MapSceneRoot, ScenarioManager.ObjectImportType.Door))
		{
			UnityGameEditorObject component4 = item3.GetComponent<UnityGameEditorObject>();
			if (component4 != null)
			{
				list.Add(component4);
			}
		}
		foreach (CObjectProp prop in ScenarioManager.CurrentScenarioState.Props)
		{
			UnityGameEditorObject unityGameEditorObject = list.FirstOrDefault((UnityGameEditorObject p) => p != null && p.PropObject != null && p.PropObject.PropGuid == prop.PropGuid);
			if (unityGameEditorObject != null)
			{
				ProceduralStyle component5 = unityGameEditorObject.GetComponent<ProceduralStyle>();
				if (component5 != null)
				{
					LevelEditorController.s_Instance.EnsureApparanceOverrideExistsForGuidAndStyle(prop.PropGuid, component5, overrideExistingWithCurrentStyleSettings);
				}
			}
		}
	}

	public void EnsureAllOverrideConsistency()
	{
		EnsureAllStylesHaveAnOverride(overrideExistingWithCurrentStyleSettings: true);
	}
}
