using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorEventsPanel : MonoBehaviour
{
	[Header("Main Event Panel")]
	public GameObject EventDisplayPanel;

	[Space]
	public GameObject EventItemPrefab;

	public Transform EventListItemParent;

	[Space]
	public TMP_Dropdown EventTypeDropDown;

	[Space]
	public LayoutElement RepeatsElement;

	public Toggle RepeatsToggle;

	[Space]
	public LayoutElement ResourceElement;

	public TextMeshProUGUI ResourceTitle;

	public InputField ResourceInput;

	[Space]
	public LayoutElement Resource1DropDownElement;

	public TextMeshProUGUI Resource1DropDownTitle;

	public TMP_Dropdown Resource1DropDown;

	[Space]
	public LayoutElement Resource2Element;

	public TextMeshProUGUI Resource2Title;

	public InputField Resource2Input;

	[Space]
	public LayoutElement Resource2DropDownElement;

	public TextMeshProUGUI Resource2DropDownTitle;

	public TMP_Dropdown Resource2DropDown;

	[Space]
	public LayoutElement Resource3Element;

	public TextMeshProUGUI Resource3Title;

	public InputField Resource3Input;

	[Space]
	public LayoutElement FloatResourceElement;

	public TextMeshProUGUI FloatResourceTitle;

	public InputField FloatResourceInput;

	[Space]
	public LayoutElement CameraProfileElement;

	public LevelEditorCameraProfilePanel CameraProfilePanel;

	[Space]
	public Button ApplyButton;

	public Button DeleteButton;

	[Space]
	public LevelEditorEventTriggerPanel EventTrigger;

	[Space]
	public LayoutElement TeamDropDownElement;

	public TMP_Dropdown TeamDropDown;

	[Space]
	[Header("Per Party Size Config")]
	public GameObject PerPartySizeConfigGO;

	public TMP_Dropdown PartySizeOneConfigDropDown;

	public TMP_Dropdown PartySizeTwoConfigDropDown;

	public TMP_Dropdown PartySizeThreeConfigDropDown;

	public TMP_Dropdown PartySizeFourConfigDropDown;

	public const char PerPartySizeConfigDelimiter = '|';

	public const string OPTION_NONE = "None";

	private LevelEditorEventItem m_CurrentEventItem;

	private List<LevelEditorEventItem> m_EventItems = new List<LevelEditorEventItem>();

	private void Awake()
	{
		EventTypeDropDown.options.Clear();
		EventTypeDropDown.AddOptions(CLevelEvent.LevelEventTypes.Select((CLevelEvent.ELevelEventType s) => s.ToString()).ToList());
	}

	public void RefreshUIWithLoadedLevel()
	{
		foreach (LevelEditorEventItem eventItem in m_EventItems)
		{
			UnityEngine.Object.Destroy(eventItem.gameObject);
		}
		m_EventItems.Clear();
		if (SaveData.Instance.Global.CurrentEditorLevelData.LevelEvents != null)
		{
			for (int i = 0; i < SaveData.Instance.Global.CurrentEditorLevelData.LevelEvents.Count; i++)
			{
				AddEventItemToUI(SaveData.Instance.Global.CurrentEditorLevelData.LevelEvents[i], i);
			}
		}
	}

	private LevelEditorEventItem AddEventItemToUI(CLevelEvent eventToAddFor, int index)
	{
		LevelEditorEventItem component = UnityEngine.Object.Instantiate(EventItemPrefab, EventListItemParent).GetComponent<LevelEditorEventItem>();
		component.Init(eventToAddFor, index, EventItemClicked);
		m_EventItems.Add(component);
		return component;
	}

	private void ConfigureUIForEventType(CLevelEvent.ELevelEventType type)
	{
		RepeatsElement.gameObject.SetActive(value: false);
		ResourceElement.gameObject.SetActive(value: false);
		Resource2Element.gameObject.SetActive(value: false);
		Resource3Element.gameObject.SetActive(value: false);
		FloatResourceElement.gameObject.SetActive(value: false);
		CameraProfileElement.gameObject.SetActive(value: false);
		Resource1DropDownElement.gameObject.SetActive(value: false);
		Resource2DropDownElement.gameObject.SetActive(value: false);
		PerPartySizeConfigGO.gameObject.SetActive(value: false);
		TeamDropDownElement.gameObject.SetActive(value: false);
		switch (type)
		{
		case CLevelEvent.ELevelEventType.RevealCharacterAfterAnim:
			ResourceTitle.text = "Character to reveal:";
			Resource2Title.text = "Animation Prefab resource path:";
			FloatResourceTitle.text = "Reveal delay:";
			RepeatsElement.gameObject.SetActive(value: true);
			ResourceElement.gameObject.SetActive(value: true);
			Resource2Element.gameObject.SetActive(value: true);
			FloatResourceElement.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.SetShortRestCard:
			ResourceTitle.text = "Card to try draw on next ShortRest:";
			RepeatsElement.gameObject.SetActive(value: true);
			ResourceElement.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.SetCharacterSelectedRoundCards:
			ResourceTitle.text = "Character to set cards for:";
			Resource2Title.text = "First Card name:";
			Resource3Title.text = "Second Card name:";
			RepeatsElement.gameObject.SetActive(value: true);
			ResourceElement.gameObject.SetActive(value: true);
			Resource2Element.gameObject.SetActive(value: true);
			Resource3Element.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.SetCameraPosition:
			RepeatsElement.gameObject.SetActive(value: true);
			CameraProfileElement.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.EditSpawner:
			Resource1DropDownTitle.text = "Spawner To Edit:";
			Resource2DropDownTitle.text = "Spawner Edit Type:";
			RepeatsElement.gameObject.SetActive(value: true);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			Resource2DropDownElement.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.UnlockDoor:
			Resource1DropDownTitle.text = "Door to Unlock";
			Resource2DropDownTitle.text = "Should open door too:";
			RepeatsElement.gameObject.SetActive(value: true);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			Resource2DropDownElement.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.RevealRoom:
			Resource1DropDownTitle.text = "Room to reveal:";
			RepeatsElement.gameObject.SetActive(value: true);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.CloseDoor:
			Resource1DropDownTitle.text = "Door to close";
			Resource2DropDownTitle.text = "Should lock door too:";
			RepeatsElement.gameObject.SetActive(value: true);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			Resource2DropDownElement.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.DeactivateModifier:
			Resource1DropDownTitle.text = "Modifier to Deactivate";
			RepeatsElement.gameObject.SetActive(value: true);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.ActivateModifier:
			Resource1DropDownTitle.text = "Modifier to Activate";
			RepeatsElement.gameObject.SetActive(value: true);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.TriggerScenarioModifier:
		case CLevelEvent.ELevelEventType.TriggerScenarioModifierOnCurrentActor:
			Resource1DropDownTitle.text = "Modifier to Trigger";
			Resource2DropDownTitle.text = "Non-current Actor GUID Override";
			RepeatsElement.gameObject.SetActive(value: true);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			Resource2DropDownElement.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.DeactivateObjective:
			Resource1DropDownTitle.text = "Objective to Deactivate";
			RepeatsElement.gameObject.SetActive(value: true);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.ActivateObjective:
			Resource1DropDownTitle.text = "Objective to Activate";
			RepeatsElement.gameObject.SetActive(value: true);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.SpawnMonsterOnActor:
			Resource1DropDownTitle.text = "Monster Class to Spawn:";
			Resource2DropDownTitle.text = "Actor to Spawn On:";
			RepeatsElement.gameObject.SetActive(value: true);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			Resource2DropDownElement.gameObject.SetActive(value: true);
			PerPartySizeConfigGO.SetActive(value: true);
			TeamDropDownElement.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.SpawnPropOnActor:
			Resource1DropDownTitle.text = "Prop to Spawn:";
			Resource2DropDownTitle.text = "Actor to Spawn On:";
			RepeatsElement.gameObject.SetActive(value: true);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			Resource2DropDownElement.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.SpawnPropOnProp:
			Resource1DropDownTitle.text = "Prop to Spawn:";
			Resource2DropDownTitle.text = "Prop to Spawn On:";
			RepeatsElement.gameObject.SetActive(value: true);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			Resource2DropDownElement.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.SpawnMonsterOnProp:
			Resource1DropDownTitle.text = "Monster Class to Spawn:";
			Resource2DropDownTitle.text = "Prop to Spawn On:";
			RepeatsElement.gameObject.SetActive(value: true);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			Resource2DropDownElement.gameObject.SetActive(value: true);
			PerPartySizeConfigGO.gameObject.SetActive(value: true);
			TeamDropDownElement.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.SpawnMonsterOnLastDestroyedObstacle:
			Resource1DropDownTitle.text = "Monster Class to Spawn:";
			RepeatsElement.gameObject.SetActive(value: true);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			PerPartySizeConfigGO.gameObject.SetActive(value: true);
			TeamDropDownElement.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.SetModifierHiddenState:
			Resource1DropDownTitle.text = "Modifier to edit";
			Resource2DropDownTitle.text = "Hidden state to set:";
			RepeatsElement.gameObject.SetActive(value: true);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			Resource2DropDownElement.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.RemoveActiveBonusFromCurrentActor:
			ResourceTitle.text = "Active Name to Remove:";
			Resource2Title.text = "Active Name to Remove:";
			Resource3Title.text = "Active Name to Remove:";
			Resource1DropDownTitle.text = "Non-current Actor GUID Override";
			RepeatsElement.gameObject.SetActive(value: true);
			ResourceElement.gameObject.SetActive(value: true);
			Resource2Element.gameObject.SetActive(value: true);
			Resource3Element.gameObject.SetActive(value: true);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.RemoveDifficultTerrain:
			Resource1DropDownTitle.text = "Difficult terrain to remove";
			RepeatsElement.gameObject.SetActive(value: true);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.RemoveProp:
			Resource1DropDownTitle.text = "Prop to remove";
			RepeatsElement.gameObject.SetActive(value: true);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.LoseGoalChestRewardChoice:
			ResourceTitle.text = "Number of rewards to lose";
			RepeatsElement.gameObject.SetActive(value: true);
			ResourceElement.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.KillActor:
			Resource1DropDownTitle.text = "Actor to kill:";
			Resource1DropDownElement.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.KillPropWithHealth:
			Resource1DropDownTitle.text = "Prop to kill:";
			Resource1DropDownElement.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.SetActorAnimParameter:
			Resource1DropDownTitle.text = "Actor to animate";
			Resource2DropDownTitle.text = "Parameter to trigger:";
			RepeatsElement.gameObject.SetActive(value: true);
			Resource2Element.gameObject.SetActive(value: true);
			Resource1DropDownElement.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.TriggerPlayableDirectorOnGameObject:
			ResourceTitle.text = "GameObject name to trigger playable director on:";
			RepeatsElement.gameObject.SetActive(value: true);
			ResourceElement.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.ShowMessage:
			ResourceTitle.text = "Message name to be shown";
			ResourceElement.gameObject.SetActive(value: true);
			break;
		case CLevelEvent.ELevelEventType.LoseScenario:
		case CLevelEvent.ELevelEventType.WinScenario:
			break;
		}
	}

	public void ConfigureResourceDropDownsForItem(LevelEditorEventItem itemClicked)
	{
		switch (itemClicked.ItemEvent.EventType)
		{
		case CLevelEvent.ELevelEventType.EditSpawner:
		{
			Resource1DropDown?.ClearOptions();
			List<string> list13 = ScenarioManager.CurrentScenarioState.Spawners.Select((CSpawner t) => t.SpawnerGuid).ToList();
			Resource1DropDown.AddOptions(list13);
			int valueWithoutNotify13 = Mathf.Max(0, list13.IndexOf(itemClicked.ItemEvent.EventResource));
			Resource1DropDown.SetValueWithoutNotify(valueWithoutNotify13);
			Resource2DropDown?.ClearOptions();
			List<string> list14 = CLevelEvent.LevelEventSpawnerEditTypes.Select((CLevelEvent.ELevelEventSpawnerEditType t) => t.ToString()).ToList();
			Resource2DropDown.AddOptions(list14);
			int valueWithoutNotify14 = Mathf.Max(0, list14.IndexOf(itemClicked.ItemEvent.EventSecondResource));
			Resource2DropDown.SetValueWithoutNotify(valueWithoutNotify14);
			break;
		}
		case CLevelEvent.ELevelEventType.UnlockDoor:
		{
			LevelEditorUtils.SetFormattedOptions(Resource1DropDown, ScenarioManager.CurrentScenarioState.DoorProps, itemClicked.ItemEvent.EventResource);
			Resource2DropDown?.ClearOptions();
			List<string> list = new List<string> { "true", "false" };
			Resource2DropDown.AddOptions(list);
			int valueWithoutNotify = Mathf.Max(0, list.IndexOf(itemClicked.ItemEvent.EventSecondResource));
			Resource2DropDown.SetValueWithoutNotify(valueWithoutNotify);
			break;
		}
		case CLevelEvent.ELevelEventType.RevealRoom:
		{
			Resource1DropDown?.ClearOptions();
			List<string> list9 = ScenarioManager.CurrentScenarioState.Maps.Select((CMap t) => t.MapGuid).ToList();
			Resource1DropDown.AddOptions(list9);
			int valueWithoutNotify9 = Mathf.Max(0, list9.IndexOf(itemClicked.ItemEvent.EventResource));
			Resource1DropDown.SetValueWithoutNotify(valueWithoutNotify9);
			break;
		}
		case CLevelEvent.ELevelEventType.CloseDoor:
		{
			LevelEditorUtils.SetFormattedOptions(Resource1DropDown, ScenarioManager.CurrentScenarioState.DoorProps, itemClicked.ItemEvent.EventResource);
			Resource2DropDown?.ClearOptions();
			List<string> list4 = new List<string> { "true", "false" };
			Resource2DropDown.AddOptions(list4);
			int valueWithoutNotify4 = Mathf.Max(0, list4.IndexOf(itemClicked.ItemEvent.EventSecondResource));
			Resource2DropDown.SetValueWithoutNotify(valueWithoutNotify4);
			break;
		}
		case CLevelEvent.ELevelEventType.DeactivateModifier:
		case CLevelEvent.ELevelEventType.ActivateModifier:
		{
			Resource1DropDown?.ClearOptions();
			List<string> list8 = (from s in ScenarioManager.CurrentScenarioState.ScenarioModifiers
				where !string.IsNullOrEmpty(s.EventIdentifier)
				select s.EventIdentifier).ToList();
			Resource1DropDown.AddOptions(list8);
			int valueWithoutNotify8 = Mathf.Max(0, list8.IndexOf(itemClicked.ItemEvent.EventResource));
			Resource1DropDown.SetValueWithoutNotify(valueWithoutNotify8);
			break;
		}
		case CLevelEvent.ELevelEventType.TriggerScenarioModifier:
		case CLevelEvent.ELevelEventType.TriggerScenarioModifierOnCurrentActor:
		{
			Resource1DropDown?.ClearOptions();
			List<string> list6 = (from s in ScenarioManager.CurrentScenarioState.ScenarioModifiers
				where !string.IsNullOrEmpty(s.EventIdentifier)
				select s.EventIdentifier).ToList();
			Resource1DropDown.AddOptions(list6);
			int valueWithoutNotify6 = Mathf.Max(0, list6.IndexOf(itemClicked.ItemEvent.EventResource));
			Resource1DropDown.SetValueWithoutNotify(valueWithoutNotify6);
			LevelEditorUtils.SetFormattedOptions(Resource2DropDown, ScenarioManager.CurrentScenarioState.ActorStates.Select((ActorState a) => a.Actor), ScenarioManager.CurrentScenarioState.Props.Where((CObjectProp x) => x.PropHealthDetails?.HasHealth ?? false), itemClicked.ItemEvent.EventFourthResource, "None");
			break;
		}
		case CLevelEvent.ELevelEventType.DeactivateObjective:
		case CLevelEvent.ELevelEventType.ActivateObjective:
		{
			Resource1DropDown?.ClearOptions();
			List<string> list10 = (from s in ScenarioManager.CurrentScenarioState.AllObjectives
				where !string.IsNullOrEmpty(s.EventIdentifier)
				select s.EventIdentifier).ToList();
			Resource1DropDown.AddOptions(list10);
			int valueWithoutNotify10 = Mathf.Max(0, list10.IndexOf(itemClicked.ItemEvent.EventResource));
			Resource1DropDown.SetValueWithoutNotify(valueWithoutNotify10);
			break;
		}
		case CLevelEvent.ELevelEventType.SpawnMonsterOnActor:
		case CLevelEvent.ELevelEventType.SpawnMonsterOnProp:
		{
			Resource1DropDown?.ClearOptions();
			List<string> list7 = MonsterClassManager.MonsterAndObjectClasses.Select((CMonsterClass t) => t.ID).ToList();
			Resource1DropDown.AddOptions(list7);
			int valueWithoutNotify7 = Mathf.Max(0, list7.IndexOf(itemClicked.ItemEvent.EventResource));
			Resource1DropDown.SetValueWithoutNotify(valueWithoutNotify7);
			if (itemClicked.ItemEvent.EventType == CLevelEvent.ELevelEventType.SpawnMonsterOnActor)
			{
				LevelEditorUtils.SetFormattedOptions(Resource2DropDown, ScenarioManager.CurrentScenarioState.ActorStates, itemClicked.ItemEvent.EventSecondResource);
			}
			else
			{
				if (itemClicked.ItemEvent.EventType != CLevelEvent.ELevelEventType.SpawnMonsterOnProp)
				{
					break;
				}
				LevelEditorUtils.SetFormattedOptions(Resource2DropDown, ScenarioManager.CurrentScenarioState.Props, itemClicked.ItemEvent.EventSecondResource);
				PartySizeOneConfigDropDown.options.Clear();
				PartySizeTwoConfigDropDown.options.Clear();
				PartySizeThreeConfigDropDown.options.Clear();
				PartySizeFourConfigDropDown.options.Clear();
				PartySizeOneConfigDropDown.AddOptions(EnemyState.PerPartySizeConfigOptions.Select((ScenarioManager.EPerPartySizeConfig t) => t.ToString()).ToList());
				PartySizeTwoConfigDropDown.AddOptions(EnemyState.PerPartySizeConfigOptions.Select((ScenarioManager.EPerPartySizeConfig t) => t.ToString()).ToList());
				PartySizeThreeConfigDropDown.AddOptions(EnemyState.PerPartySizeConfigOptions.Select((ScenarioManager.EPerPartySizeConfig t) => t.ToString()).ToList());
				PartySizeFourConfigDropDown.AddOptions(EnemyState.PerPartySizeConfigOptions.Select((ScenarioManager.EPerPartySizeConfig t) => t.ToString()).ToList());
				if (itemClicked.ItemEvent.EventThirdResource.IsNOTNullOrEmpty())
				{
					List<ScenarioManager.EPerPartySizeConfig> perPartySizeConfigsFromResource2 = GetPerPartySizeConfigsFromResource(itemClicked.ItemEvent.EventThirdResource);
					if (perPartySizeConfigsFromResource2.Count == 4)
					{
						PartySizeOneConfigDropDown.value = (int)perPartySizeConfigsFromResource2[0];
						PartySizeTwoConfigDropDown.value = (int)perPartySizeConfigsFromResource2[1];
						PartySizeThreeConfigDropDown.value = (int)perPartySizeConfigsFromResource2[2];
						PartySizeFourConfigDropDown.value = (int)perPartySizeConfigsFromResource2[3];
					}
				}
				if (itemClicked.ItemEvent.EventFourthResource.IsNOTNullOrEmpty())
				{
					Enum.TryParse<CActor.EType>(itemClicked.ItemEvent.EventFourthResource, ignoreCase: true, out var teamValue2);
					TeamDropDown.value = TeamDropDown.options.FindIndex((TMP_Dropdown.OptionData op) => op.text.Trim().Equals(teamValue2.ToString(), StringComparison.OrdinalIgnoreCase));
				}
			}
			break;
		}
		case CLevelEvent.ELevelEventType.SpawnMonsterOnLastDestroyedObstacle:
		{
			Resource1DropDown?.ClearOptions();
			List<string> list5 = MonsterClassManager.MonsterAndObjectClasses.Select((CMonsterClass t) => t.ID).ToList();
			Resource1DropDown.AddOptions(list5);
			int valueWithoutNotify5 = Mathf.Max(0, list5.IndexOf(itemClicked.ItemEvent.EventResource));
			Resource1DropDown.SetValueWithoutNotify(valueWithoutNotify5);
			PartySizeOneConfigDropDown.options.Clear();
			PartySizeTwoConfigDropDown.options.Clear();
			PartySizeThreeConfigDropDown.options.Clear();
			PartySizeFourConfigDropDown.options.Clear();
			PartySizeOneConfigDropDown.AddOptions(EnemyState.PerPartySizeConfigOptions.Select((ScenarioManager.EPerPartySizeConfig t) => t.ToString()).ToList());
			PartySizeTwoConfigDropDown.AddOptions(EnemyState.PerPartySizeConfigOptions.Select((ScenarioManager.EPerPartySizeConfig t) => t.ToString()).ToList());
			PartySizeThreeConfigDropDown.AddOptions(EnemyState.PerPartySizeConfigOptions.Select((ScenarioManager.EPerPartySizeConfig t) => t.ToString()).ToList());
			PartySizeFourConfigDropDown.AddOptions(EnemyState.PerPartySizeConfigOptions.Select((ScenarioManager.EPerPartySizeConfig t) => t.ToString()).ToList());
			if (itemClicked.ItemEvent.EventThirdResource.IsNOTNullOrEmpty())
			{
				List<ScenarioManager.EPerPartySizeConfig> perPartySizeConfigsFromResource = GetPerPartySizeConfigsFromResource(itemClicked.ItemEvent.EventThirdResource);
				if (perPartySizeConfigsFromResource.Count == 4)
				{
					PartySizeOneConfigDropDown.value = (int)perPartySizeConfigsFromResource[0];
					PartySizeTwoConfigDropDown.value = (int)perPartySizeConfigsFromResource[1];
					PartySizeThreeConfigDropDown.value = (int)perPartySizeConfigsFromResource[2];
					PartySizeFourConfigDropDown.value = (int)perPartySizeConfigsFromResource[3];
				}
			}
			if (itemClicked.ItemEvent.EventFourthResource.IsNOTNullOrEmpty())
			{
				Enum.TryParse<CActor.EType>(itemClicked.ItemEvent.EventFourthResource, ignoreCase: true, out var teamValue);
				TeamDropDown.value = TeamDropDown.options.FindIndex((TMP_Dropdown.OptionData op) => op.text.Trim().Equals(teamValue.ToString(), StringComparison.OrdinalIgnoreCase));
			}
			break;
		}
		case CLevelEvent.ELevelEventType.SpawnPropOnActor:
		{
			Resource1DropDown?.ClearOptions();
			List<string> list11 = CObjectProp.PropTypes.Select((EPropType t) => t.ToString()).ToList();
			list11.AddRange(CObjectProp.SpecificPropTypes.Select((ESpecificPropType t) => t.ToString()).ToList());
			Resource1DropDown.AddOptions(list11);
			int valueWithoutNotify11 = Mathf.Max(0, list11.IndexOf(itemClicked.ItemEvent.EventResource));
			Resource1DropDown.SetValueWithoutNotify(valueWithoutNotify11);
			LevelEditorUtils.SetFormattedOptions(Resource2DropDown, ScenarioManager.CurrentScenarioState.ActorStates, itemClicked.ItemEvent.EventSecondResource);
			break;
		}
		case CLevelEvent.ELevelEventType.SpawnPropOnProp:
		{
			Resource1DropDown?.ClearOptions();
			List<string> list12 = CObjectProp.PropTypes.Select((EPropType t) => t.ToString()).ToList();
			list12.AddRange(CObjectProp.SpecificPropTypes.Select((ESpecificPropType t) => t.ToString()).ToList());
			Resource1DropDown.AddOptions(list12);
			int valueWithoutNotify12 = Mathf.Max(0, list12.IndexOf(itemClicked.ItemEvent.EventResource));
			Resource1DropDown.SetValueWithoutNotify(valueWithoutNotify12);
			LevelEditorUtils.SetFormattedOptions(Resource2DropDown, ScenarioManager.CurrentScenarioState.Props, itemClicked.ItemEvent.EventSecondResource);
			break;
		}
		case CLevelEvent.ELevelEventType.SetModifierHiddenState:
		{
			Resource1DropDown?.ClearOptions();
			List<string> list2 = (from s in ScenarioManager.CurrentScenarioState.ScenarioModifiers
				where !string.IsNullOrEmpty(s.EventIdentifier)
				select s.EventIdentifier).ToList();
			Resource1DropDown.AddOptions(list2);
			int valueWithoutNotify2 = Mathf.Max(0, list2.IndexOf(itemClicked.ItemEvent.EventResource));
			Resource1DropDown.SetValueWithoutNotify(valueWithoutNotify2);
			Resource2DropDown?.ClearOptions();
			List<string> list3 = new List<string> { "true", "false" };
			Resource2DropDown.AddOptions(list3);
			int valueWithoutNotify3 = Mathf.Max(0, list3.IndexOf(itemClicked.ItemEvent.EventSecondResource));
			Resource2DropDown.SetValueWithoutNotify(valueWithoutNotify3);
			break;
		}
		case CLevelEvent.ELevelEventType.RemoveDifficultTerrain:
			LevelEditorUtils.SetFormattedOptions(Resource1DropDown, ScenarioManager.CurrentScenarioState.Props.Where((CObjectProp p) => p is CObjectDifficultTerrain), itemClicked.ItemEvent.EventResource);
			break;
		case CLevelEvent.ELevelEventType.RemoveProp:
			LevelEditorUtils.SetFormattedOptions(Resource1DropDown, ScenarioManager.CurrentScenarioState.Props, itemClicked.ItemEvent.EventResource);
			break;
		case CLevelEvent.ELevelEventType.KillActor:
			LevelEditorUtils.SetFormattedOptions(Resource1DropDown, ScenarioManager.CurrentScenarioState.ActorStates, itemClicked.ItemEvent.EventResource);
			break;
		case CLevelEvent.ELevelEventType.KillPropWithHealth:
			LevelEditorUtils.SetFormattedOptions(Resource1DropDown, ScenarioManager.CurrentScenarioState.Props.Where((CObjectProp o) => o.PropHealthDetails?.HasHealth ?? false), itemClicked.ItemEvent.EventResource);
			break;
		case CLevelEvent.ELevelEventType.RemoveActiveBonusFromCurrentActor:
			LevelEditorUtils.SetFormattedOptions(Resource1DropDown, ScenarioManager.CurrentScenarioState.ActorStates.Select((ActorState a) => a.Actor), ScenarioManager.CurrentScenarioState.Props.Where((CObjectProp x) => x.PropHealthDetails?.HasHealth ?? false), itemClicked.ItemEvent.EventFourthResource, "None");
			break;
		case CLevelEvent.ELevelEventType.SetActorAnimParameter:
			LevelEditorUtils.SetFormattedOptions(Resource1DropDown, ScenarioManager.CurrentScenarioState.ActorStates, itemClicked.ItemEvent.EventResource);
			break;
		default:
			Resource1DropDown?.ClearOptions();
			Resource1DropDown?.SetValueWithoutNotify(0);
			Resource2DropDown?.ClearOptions();
			Resource2DropDown?.SetValueWithoutNotify(0);
			break;
		}
	}

	public static List<ScenarioManager.EPerPartySizeConfig> GetPerPartySizeConfigsFromResource(string resource)
	{
		List<ScenarioManager.EPerPartySizeConfig> list = new List<ScenarioManager.EPerPartySizeConfig>();
		List<string> list2 = resource.Split('|').ToList();
		if (list2.Count == 4)
		{
			foreach (string item in list2)
			{
				list.Add((ScenarioManager.EPerPartySizeConfig)Enum.Parse(typeof(ScenarioManager.EPerPartySizeConfig), item, ignoreCase: true));
			}
		}
		return list;
	}

	public void EventItemClicked(LevelEditorEventItem itemClicked)
	{
		if ((bool)m_CurrentEventItem)
		{
			m_CurrentEventItem.SetSelectedState(selected: false);
		}
		m_CurrentEventItem = itemClicked;
		m_CurrentEventItem.SetSelectedState(selected: true);
		EventTypeDropDown.SetValueWithoutNotify((int)m_CurrentEventItem.ItemEvent.EventType);
		RepeatsToggle.SetIsOnWithoutNotify(m_CurrentEventItem.ItemEvent.Repeats);
		ResourceInput.text = m_CurrentEventItem.ItemEvent.EventResource;
		Resource2Input.text = m_CurrentEventItem.ItemEvent.EventSecondResource;
		Resource3Input.text = m_CurrentEventItem.ItemEvent.EventThirdResource;
		FloatResourceInput.text = m_CurrentEventItem.ItemEvent.EventFloatResource.ToString();
		EventTrigger.InitForMessageTrigger(m_CurrentEventItem.ItemEvent.DisplayTrigger);
		CameraProfilePanel.InitForProfile(m_CurrentEventItem.ItemEvent.EventCameraProfile);
		ConfigureResourceDropDownsForItem(m_CurrentEventItem);
		ConfigureUIForEventType(m_CurrentEventItem.ItemEvent.EventType);
		EventDisplayPanel.SetActive(value: true);
	}

	public void OnAddEventPressed()
	{
		CLevelEvent cLevelEvent = new CLevelEvent();
		SaveData.Instance.Global.CurrentEditorLevelData.LevelEvents.Add(cLevelEvent);
		LevelEditorEventItem itemClicked = AddEventItemToUI(cLevelEvent, SaveData.Instance.Global.CurrentEditorLevelData.LevelEvents.Count - 1);
		EventItemClicked(itemClicked);
	}

	public void OnApplyPressed()
	{
		m_CurrentEventItem.ItemEvent.EventType = CLevelEvent.LevelEventTypes.SingleOrDefault((CLevelEvent.ELevelEventType s) => s.ToString() == EventTypeDropDown.options[EventTypeDropDown.value].text);
		m_CurrentEventItem.ItemEvent.Repeats = RepeatsToggle.isOn;
		m_CurrentEventItem.ItemEvent.EventThirdResource = Resource3Input.text;
		m_CurrentEventItem.ItemEvent.EventFloatResource = (string.IsNullOrEmpty(FloatResourceInput.text) ? 0f : Convert.ToSingle(FloatResourceInput.text));
		m_CurrentEventItem.Refresh();
		switch (m_CurrentEventItem.ItemEvent.EventType)
		{
		case CLevelEvent.ELevelEventType.EditSpawner:
		{
			string eventResource8 = string.Empty;
			if (Resource1DropDown.value < ScenarioManager.CurrentScenarioState.Spawners.Count)
			{
				eventResource8 = ScenarioManager.CurrentScenarioState.Spawners[Resource1DropDown.value].SpawnerGuid;
			}
			m_CurrentEventItem.ItemEvent.EventResource = eventResource8;
			string eventSecondResource3 = CLevelEvent.LevelEventSpawnerEditTypes[0].ToString();
			if (Resource2DropDown.value < CLevelEvent.LevelEventSpawnerEditTypes.Length)
			{
				eventSecondResource3 = CLevelEvent.LevelEventSpawnerEditTypes[Resource2DropDown.value].ToString();
			}
			m_CurrentEventItem.ItemEvent.EventSecondResource = eventSecondResource3;
			break;
		}
		case CLevelEvent.ELevelEventType.UnlockDoor:
		{
			string eventResource14 = string.Empty;
			if (Resource1DropDown.value < ScenarioManager.CurrentScenarioState.DoorProps.Count)
			{
				eventResource14 = ScenarioManager.CurrentScenarioState.DoorProps[Resource1DropDown.value].PropGuid;
			}
			m_CurrentEventItem.ItemEvent.EventResource = eventResource14;
			string eventSecondResource7 = string.Empty;
			if (Resource2DropDown.value < Resource2DropDown.options.Count)
			{
				eventSecondResource7 = Resource2DropDown.options[Resource2DropDown.value].text;
			}
			m_CurrentEventItem.ItemEvent.EventSecondResource = eventSecondResource7;
			break;
		}
		case CLevelEvent.ELevelEventType.RevealRoom:
		{
			string eventResource2 = string.Empty;
			if (Resource1DropDown.value < ScenarioManager.CurrentScenarioState.Maps.Count)
			{
				eventResource2 = ScenarioManager.CurrentScenarioState.Maps[Resource1DropDown.value].MapGuid;
			}
			m_CurrentEventItem.ItemEvent.EventResource = eventResource2;
			break;
		}
		case CLevelEvent.ELevelEventType.CloseDoor:
		{
			string eventResource13 = string.Empty;
			if (Resource1DropDown.value < ScenarioManager.CurrentScenarioState.DoorProps.Count)
			{
				eventResource13 = ScenarioManager.CurrentScenarioState.DoorProps[Resource1DropDown.value].PropGuid;
			}
			m_CurrentEventItem.ItemEvent.EventResource = eventResource13;
			string eventSecondResource6 = string.Empty;
			if (Resource2DropDown.value < Resource2DropDown.options.Count)
			{
				eventSecondResource6 = Resource2DropDown.options[Resource2DropDown.value].text;
			}
			m_CurrentEventItem.ItemEvent.EventSecondResource = eventSecondResource6;
			break;
		}
		case CLevelEvent.ELevelEventType.DeactivateModifier:
		case CLevelEvent.ELevelEventType.ActivateModifier:
		case CLevelEvent.ELevelEventType.DeactivateObjective:
		case CLevelEvent.ELevelEventType.ActivateObjective:
		{
			string eventResource9 = string.Empty;
			if (Resource1DropDown.value < Resource1DropDown.options.Count)
			{
				eventResource9 = Resource1DropDown.options[Resource1DropDown.value].text;
			}
			m_CurrentEventItem.ItemEvent.EventResource = eventResource9;
			break;
		}
		case CLevelEvent.ELevelEventType.TriggerScenarioModifier:
		case CLevelEvent.ELevelEventType.TriggerScenarioModifierOnCurrentActor:
		{
			string eventResource10 = string.Empty;
			if (Resource1DropDown.value < Resource1DropDown.options.Count)
			{
				eventResource10 = Resource1DropDown.options[Resource1DropDown.value].text;
			}
			m_CurrentEventItem.ItemEvent.EventResource = eventResource10;
			if (Resource2DropDown.gameObject.activeSelf)
			{
				m_CurrentEventItem.ItemEvent.EventFourthResource = LevelEditorUtils.ExtractGuidFromFormattedOption(Resource2DropDown.options[Resource2DropDown.value].text);
			}
			break;
		}
		case CLevelEvent.ELevelEventType.SpawnMonsterOnActor:
		case CLevelEvent.ELevelEventType.SpawnMonsterOnProp:
		{
			string eventResource11 = string.Empty;
			if (Resource1DropDown.value < MonsterClassManager.MonsterAndObjectClasses.Count)
			{
				eventResource11 = MonsterClassManager.MonsterAndObjectClasses[Resource1DropDown.value].ID;
			}
			m_CurrentEventItem.ItemEvent.EventResource = eventResource11;
			string eventSecondResource4 = string.Empty;
			if (m_CurrentEventItem.ItemEvent.EventType == CLevelEvent.ELevelEventType.SpawnMonsterOnActor)
			{
				if (Resource2DropDown.value < ScenarioManager.CurrentScenarioState.ActorStates.Count)
				{
					eventSecondResource4 = ScenarioManager.CurrentScenarioState.ActorStates[Resource2DropDown.value].ActorGuid;
				}
			}
			else if (m_CurrentEventItem.ItemEvent.EventType == CLevelEvent.ELevelEventType.SpawnMonsterOnProp && Resource2DropDown.value < ScenarioManager.CurrentScenarioState.Props.Count)
			{
				eventSecondResource4 = ScenarioManager.CurrentScenarioState.Props[Resource2DropDown.value].PropGuid;
			}
			m_CurrentEventItem.ItemEvent.EventSecondResource = eventSecondResource4;
			m_CurrentEventItem.ItemEvent.EventThirdResource = string.Join('|'.ToString(), (ScenarioManager.EPerPartySizeConfig)PartySizeOneConfigDropDown.value, (ScenarioManager.EPerPartySizeConfig)PartySizeTwoConfigDropDown.value, (ScenarioManager.EPerPartySizeConfig)PartySizeThreeConfigDropDown.value, (ScenarioManager.EPerPartySizeConfig)PartySizeFourConfigDropDown.value);
			Enum.TryParse<CActor.EType>(TeamDropDown.options[TeamDropDown.value].text, ignoreCase: true, out var result2);
			CLevelEvent itemEvent2 = m_CurrentEventItem.ItemEvent;
			int num = (int)result2;
			itemEvent2.EventFourthResource = num.ToString();
			break;
		}
		case CLevelEvent.ELevelEventType.SpawnMonsterOnLastDestroyedObstacle:
		{
			string eventResource5 = string.Empty;
			if (Resource1DropDown.value < MonsterClassManager.MonsterAndObjectClasses.Count)
			{
				eventResource5 = MonsterClassManager.MonsterAndObjectClasses[Resource1DropDown.value].ID;
			}
			m_CurrentEventItem.ItemEvent.EventResource = eventResource5;
			m_CurrentEventItem.ItemEvent.EventThirdResource = string.Join('|'.ToString(), (ScenarioManager.EPerPartySizeConfig)PartySizeOneConfigDropDown.value, (ScenarioManager.EPerPartySizeConfig)PartySizeTwoConfigDropDown.value, (ScenarioManager.EPerPartySizeConfig)PartySizeThreeConfigDropDown.value, (ScenarioManager.EPerPartySizeConfig)PartySizeFourConfigDropDown.value);
			Enum.TryParse<CActor.EType>(TeamDropDown.options[TeamDropDown.value].text, ignoreCase: true, out var result);
			CLevelEvent itemEvent = m_CurrentEventItem.ItemEvent;
			int num = (int)result;
			itemEvent.EventFourthResource = num.ToString();
			break;
		}
		case CLevelEvent.ELevelEventType.SpawnPropOnActor:
		{
			string eventResource3 = string.Empty;
			List<string> list = CObjectProp.PropTypes.Select((EPropType x) => x.ToString()).ToList();
			list.AddRange(CObjectProp.SpecificPropTypes.Select((ESpecificPropType x) => x.ToString()).ToList());
			if (Resource1DropDown.value < list.Count)
			{
				eventResource3 = list[Resource1DropDown.value];
			}
			m_CurrentEventItem.ItemEvent.EventResource = eventResource3;
			string eventSecondResource = string.Empty;
			if (Resource2DropDown.value < ScenarioManager.CurrentScenarioState.ActorStates.Count)
			{
				eventSecondResource = ScenarioManager.CurrentScenarioState.ActorStates[Resource2DropDown.value].ActorGuid;
			}
			m_CurrentEventItem.ItemEvent.EventSecondResource = eventSecondResource;
			break;
		}
		case CLevelEvent.ELevelEventType.SpawnPropOnProp:
		{
			string eventResource6 = string.Empty;
			List<string> list3 = CObjectProp.PropTypes.Select((EPropType x) => x.ToString()).ToList();
			list3.AddRange(CObjectProp.SpecificPropTypes.Select((ESpecificPropType x) => x.ToString()).ToList());
			if (Resource1DropDown.value < list3.Count)
			{
				eventResource6 = list3[Resource1DropDown.value];
			}
			m_CurrentEventItem.ItemEvent.EventResource = eventResource6;
			string eventSecondResource2 = string.Empty;
			if (Resource2DropDown.value < ScenarioManager.CurrentScenarioState.Props.Count)
			{
				eventSecondResource2 = ScenarioManager.CurrentScenarioState.Props[Resource2DropDown.value].PropGuid;
			}
			m_CurrentEventItem.ItemEvent.EventSecondResource = eventSecondResource2;
			break;
		}
		case CLevelEvent.ELevelEventType.SetModifierHiddenState:
		{
			string eventResource12 = string.Empty;
			if (Resource1DropDown.value < Resource1DropDown.options.Count)
			{
				eventResource12 = Resource1DropDown.options[Resource1DropDown.value].text;
			}
			m_CurrentEventItem.ItemEvent.EventResource = eventResource12;
			string eventSecondResource5 = string.Empty;
			if (Resource2DropDown.value < Resource2DropDown.options.Count)
			{
				eventSecondResource5 = Resource2DropDown.options[Resource2DropDown.value].text;
			}
			m_CurrentEventItem.ItemEvent.EventSecondResource = eventSecondResource5;
			break;
		}
		case CLevelEvent.ELevelEventType.RemoveDifficultTerrain:
		{
			string eventResource7 = string.Empty;
			List<CObjectProp> list4 = ScenarioManager.CurrentScenarioState.Props.Where((CObjectProp p) => p is CObjectDifficultTerrain).ToList();
			if (Resource1DropDown.value < list4.Count)
			{
				eventResource7 = list4[Resource1DropDown.value].PropGuid;
			}
			m_CurrentEventItem.ItemEvent.EventResource = eventResource7;
			break;
		}
		case CLevelEvent.ELevelEventType.RemoveProp:
		{
			string eventResource4 = string.Empty;
			List<CObjectProp> list2 = ScenarioManager.CurrentScenarioState.Props.ToList();
			if (Resource1DropDown.value < list2.Count)
			{
				eventResource4 = list2[Resource1DropDown.value].PropGuid;
			}
			m_CurrentEventItem.ItemEvent.EventResource = eventResource4;
			break;
		}
		case CLevelEvent.ELevelEventType.KillActor:
		{
			string eventResource = string.Empty;
			if (Resource1DropDown.value < ScenarioManager.CurrentScenarioState.ActorStates.Count)
			{
				eventResource = ScenarioManager.CurrentScenarioState.ActorStates[Resource1DropDown.value].ActorGuid;
			}
			m_CurrentEventItem.ItemEvent.EventResource = eventResource;
			break;
		}
		case CLevelEvent.ELevelEventType.KillPropWithHealth:
			m_CurrentEventItem.ItemEvent.EventResource = LevelEditorUtils.ExtractGuidFromFormattedOption(Resource1DropDown.options[Resource1DropDown.value].text);
			break;
		case CLevelEvent.ELevelEventType.RemoveActiveBonusFromCurrentActor:
			m_CurrentEventItem.ItemEvent.EventResource = ResourceInput.text;
			m_CurrentEventItem.ItemEvent.EventSecondResource = Resource2Input.text;
			m_CurrentEventItem.ItemEvent.EventThirdResource = Resource3Input.text;
			m_CurrentEventItem.ItemEvent.EventFourthResource = LevelEditorUtils.ExtractGuidFromFormattedOption(Resource1DropDown.options[Resource1DropDown.value].text);
			break;
		case CLevelEvent.ELevelEventType.SetActorAnimParameter:
			m_CurrentEventItem.ItemEvent.EventResource = LevelEditorUtils.ExtractGuidFromFormattedOption(Resource1DropDown.options[Resource1DropDown.value].text);
			m_CurrentEventItem.ItemEvent.EventSecondResource = Resource2Input.text;
			break;
		default:
			m_CurrentEventItem.ItemEvent.EventResource = ResourceInput.text;
			m_CurrentEventItem.ItemEvent.EventSecondResource = Resource2Input.text;
			m_CurrentEventItem.ItemEvent.EventThirdResource = Resource3Input.text;
			break;
		}
		CameraProfilePanel.OnApplyPressed();
		EventTrigger.SaveValuesToTrigger();
	}

	public void OnDeletePressed()
	{
		if (m_CurrentEventItem != null)
		{
			SaveData.Instance.Global.CurrentEditorLevelData.LevelEvents.RemoveAt(m_CurrentEventItem.ItemIndex);
			m_EventItems.Remove(m_CurrentEventItem);
			UnityEngine.Object.Destroy(m_CurrentEventItem.gameObject);
			m_CurrentEventItem = null;
			EventDisplayPanel.SetActive(value: false);
		}
	}

	public async void OnDuplicatePressed()
	{
		if (m_CurrentEventItem != null)
		{
			CLevelEvent cLevelEvent = await m_CurrentEventItem.ItemEvent.DeepCopySerializableObject<CLevelEvent>();
			SaveData.Instance.Global.CurrentEditorLevelData.LevelEvents.Add(cLevelEvent);
			LevelEditorEventItem itemClicked = AddEventItemToUI(cLevelEvent, SaveData.Instance.Global.CurrentEditorLevelData.LevelEvents.Count - 1);
			EventItemClicked(itemClicked);
		}
	}

	public void OnEventTypeDropDownChanged(int valueChangeTo)
	{
		m_CurrentEventItem.ItemEvent.EventType = CLevelEvent.LevelEventTypes[EventTypeDropDown.value];
		ConfigureUIForEventType(m_CurrentEventItem.ItemEvent.EventType);
		ConfigureResourceDropDownsForItem(m_CurrentEventItem);
	}
}
