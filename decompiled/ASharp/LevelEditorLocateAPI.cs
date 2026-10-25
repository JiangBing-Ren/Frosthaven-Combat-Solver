using System.Linq;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LevelEditorLocateAPI : MonoBehaviour
{
	public Button Button;

	public TMP_Dropdown DropDownSource;

	private void Reset()
	{
		Button = GetComponent<Button>();
	}

	private void Awake()
	{
		if ((bool)Button && (bool)DropDownSource)
		{
			Button.onClick.AddListener(() =>
			{
				TryLocate(DropDownSource);
			});
		}
	}

	public void TryLocate(TMP_Dropdown dropdown)
	{
		if (dropdown.value >= dropdown.options.Count || dropdown.value < 0)
		{
			return;
		}
		string guid = LevelEditorUtils.ExtractGuidFromFormattedOption(dropdown.options[dropdown.value].text);
		CActor cActor = ScenarioManager.Scenario.AllActors.FirstOrDefault((CActor a) => a.ActorGuid == guid);
		if (cActor != null)
		{
			LevelEditorController.s_Instance.ShowLocationIndicator(Choreographer.s_Choreographer.FindClientActorGameObject(cActor).transform.position);
			return;
		}
		CObjectActor cObjectActor = ScenarioManager.Scenario.AllObjects.FirstOrDefault((CObjectActor o) => o.ActorGuid == guid);
		if (cObjectActor != null)
		{
			LevelEditorController.s_Instance.ShowLocationIndicator(Choreographer.s_Choreographer.FindClientActorGameObject(cObjectActor).transform.position);
			return;
		}
		CObjectProp cObjectProp = ScenarioManager.CurrentScenarioState.Props.FirstOrDefault((CObjectProp o) => o.PropGuid == guid);
		if (cObjectProp != null && cObjectProp.Position != null)
		{
			LevelEditorController.s_Instance.ShowLocationIndicator(GloomUtility.CVToV(cObjectProp.Position));
		}
		CSpawner cSpawner = ScenarioManager.CurrentScenarioState.Spawners.FirstOrDefault((CSpawner o) => o.SpawnerGuid == guid);
		if (cSpawner != null && cSpawner.ArrayIndex != null)
		{
			CClientTile cClientTile = CAutoTileClick.TileIndexToClientTile(cSpawner.ArrayIndex);
			LevelEditorController.s_Instance.ShowLocationIndicator(cClientTile.m_GameObject.transform.position);
		}
	}
}
