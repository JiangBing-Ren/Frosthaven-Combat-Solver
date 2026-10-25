using UnityEngine;

public class LevelEditorTabTogglesController : MonoBehaviour
{
	public GameObject LevelDataTab;

	public GameObject RoomEditTab;

	public GameObject CharactersTab;

	public GameObject SpawnersTab;

	public GameObject MonstersTab;

	public GameObject ObjectsTab;

	public GameObject PropsTab;

	public GameObject ElementInfusionsTab;

	public GameObject ApparanceTab;

	public GameObject ScenarioModifiersTab;

	public GameObject ObjectivesTab;

	public GameObject EventsTab;

	public GameObject MessagesTab;

	public GameObject AutotestTab;

	public void RefreshAvailableToggles(LevelEditorController.ELevelEditingState levelEditingState)
	{
		switch (levelEditingState)
		{
		case LevelEditorController.ELevelEditingState.RoomPlacement:
		case LevelEditorController.ELevelEditingState.DoorPlacement:
			LevelDataTab.SetActive(value: true);
			RoomEditTab.SetActive(value: true);
			CharactersTab.SetActive(value: false);
			SpawnersTab.SetActive(value: false);
			MonstersTab.SetActive(value: false);
			ObjectsTab.SetActive(value: false);
			PropsTab.SetActive(value: false);
			ElementInfusionsTab.SetActive(value: false);
			ApparanceTab.SetActive(value: false);
			ScenarioModifiersTab.SetActive(value: false);
			ObjectivesTab.SetActive(value: false);
			EventsTab.SetActive(value: false);
			MessagesTab.SetActive(value: false);
			AutotestTab.SetActive(value: false);
			break;
		case LevelEditorController.ELevelEditingState.PropActorPlacement:
			LevelDataTab.SetActive(value: true);
			RoomEditTab.SetActive(value: true);
			CharactersTab.SetActive(value: true);
			SpawnersTab.SetActive(value: true);
			MonstersTab.SetActive(value: true);
			ObjectsTab.SetActive(value: true);
			PropsTab.SetActive(value: true);
			ElementInfusionsTab.SetActive(value: true);
			ApparanceTab.SetActive(value: true);
			ScenarioModifiersTab.SetActive(value: true);
			ObjectivesTab.SetActive(value: true);
			EventsTab.SetActive(value: true);
			MessagesTab.SetActive(value: true);
			AutotestTab.SetActive(value: false);
			break;
		}
	}
}
