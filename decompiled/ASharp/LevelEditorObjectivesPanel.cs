using System;
using System.Collections.Generic;
using System.Linq;
using GLOOM;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorObjectivesPanel : MonoBehaviour
{
	public LayoutGroup WinObjectiveItemListParent;

	public LayoutGroup LoseObjectiveItemListParent;

	public GameObject ObjectiveListItemPrefab;

	[Header("Display panel components")]
	public GameObject ObjectiveDisplayPanel;

	public TextMeshProUGUI ObjectiveTitle;

	public TMP_Dropdown ObjectiveTypeDropDown;

	public TMP_InputField ObjectiveCustomLocInput;

	public TMP_InputField ObjectiveCustomTileHoverLocInput;

	public TMP_Dropdown ObjectiveActorDropDown;

	public TMP_InputField ObjectiveActivationRoundInput;

	public TMP_InputField ObjectiveRoundInput;

	public TMP_InputField ObjectiveEventIdentifier;

	public Toggle ObjectiveActiveFromStartToggle;

	public Toggle ObjectiveHiddenToggle;

	public Toggle ObjectiveOptionalToggle;

	public Toggle ObjectiveWinsDespiteExhaustionToggle;

	public Toggle ObjectiveEnoughToWinAloneToggle;

	public Toggle ObjectiveCountFromActivationRound;

	public Toggle ObjectiveHideCounter;

	public TMP_InputField ObjectiveAmountInputPartySize1;

	public TMP_InputField ObjectiveAmountInputPartySize2;

	public TMP_InputField ObjectiveAmountInputPartySize3;

	public TMP_InputField ObjectiveAmountInputPartySize4;

	public TextMeshProUGUI ApplyButtonText;

	public Button DeleteButton;

	public TextMeshProUGUI StatusText;

	[Header("Custom Objective Trigger")]
	public LevelEditorEventTriggerPanel CustomTriggerPanel;

	[Header("Objective Filter")]
	public LevelEditorObjectiveFilterPanel ObjectiveFilterPanel;

	[Header("Objective Dependency Panel")]
	public LevelEditorObjectiveDependencyPanel ObjectiveDependencyPanel;

	[Header("Elements to Disable/Enable")]
	public GameObject ObjectiveCustomLocWrapper;

	public GameObject ObjectiveActivationRoundWrapper;

	public GameObject ObjectiveRoundWrapper;

	public GameObject ObjectiveActorWrapper;

	public GameObject ObjectiveAmountWrapper;

	public GameObject ObjectiveAmountLabelsWrapper;

	public GameObject ObjectiveOptionalToggleWrapper;

	public GameObject ObjectiveWinsDespiteExhaustionToggleWrapper;

	public GameObject ObjectiveEnoughToWinAloneToggleWrapper;

	public GameObject ObjectiveCountFromActivationRoundWrapper;

	[Header("Objective Tile List")]
	public LayoutElement ObjectiveTileListElement;

	public GameObject ObjectiveTileListItemPrefab;

	public Transform ObjectiveTileListParentTransform;

	[Header("Objective Map List")]
	public LayoutElement ObjectiveMapListElement;

	public TMP_Dropdown ObjectiveMapListDropDown;

	public GameObject ObjectiveMapListItemPrefab;

	public Transform ObjectiveMapListParentTransform;

	public LevelEditorGenericListPanel ObjectiveMapListPanel;

	private ScenarioState m_CurrentScenarioState;

	private LevelEditorObjectiveListItem m_CurrentItem;

	private List<LevelEditorObjectiveListItem> m_WinObjectiveListItems = new List<LevelEditorObjectiveListItem>();

	private List<LevelEditorObjectiveListItem> m_LoseObjectiveListItems = new List<LevelEditorObjectiveListItem>();

	private bool m_ButtonModeAdd;

	private List<CActor> m_ActorOptions = new List<CActor>();

	private List<TileIndex> m_CurrentTileList;

	private List<LevelEditorListItemInlineButtons> m_CurrentTileListItems;

	private List<CMap> m_MapOptions = new List<CMap>();

	private List<string> m_CurrentMapGuidList;

	private List<LevelEditorListItemInlineButtons> m_CurrentMapListItems;

	private EObjectiveResult m_CurrentObjectiveResult;

	private void Awake()
	{
		EObjectiveType[] source = (EObjectiveType[])Enum.GetValues(typeof(EObjectiveType));
		_ = (EObjectiveResult[])Enum.GetValues(typeof(EObjectiveResult));
		ObjectiveTypeDropDown.options.Clear();
		ObjectiveTypeDropDown.AddOptions(source.Select((EObjectiveType s) => s.ToString()).ToList());
		ObjectiveDisplayPanel.SetActive(value: false);
	}

	private void OnEnable()
	{
		RefreshUIFromCurrentScenarioState(ScenarioManager.CurrentScenarioState);
	}

	private LevelEditorObjectiveListItem AddNewListItemForObjective(int atIndex, CObjective objectiveToAddFor, EObjectiveResult resultColumn)
	{
		GameObject gameObject = null;
		EObjectiveResult eObjectiveResult = objectiveToAddFor.Result;
		if (eObjectiveResult == EObjectiveResult.None)
		{
			eObjectiveResult = resultColumn;
		}
		switch (eObjectiveResult)
		{
		case EObjectiveResult.Win:
			gameObject = UnityEngine.Object.Instantiate(ObjectiveListItemPrefab, WinObjectiveItemListParent.transform);
			break;
		case EObjectiveResult.Lose:
			gameObject = UnityEngine.Object.Instantiate(ObjectiveListItemPrefab, LoseObjectiveItemListParent.transform);
			break;
		}
		LevelEditorObjectiveListItem levelEditorObjectiveListItem = null;
		if (gameObject != null)
		{
			levelEditorObjectiveListItem = gameObject.GetComponent<LevelEditorObjectiveListItem>();
			levelEditorObjectiveListItem.InitForObjective(objectiveToAddFor, atIndex, eObjectiveResult);
			levelEditorObjectiveListItem.ButtonPressedAction = EditObjective;
			switch (eObjectiveResult)
			{
			case EObjectiveResult.Win:
				m_WinObjectiveListItems.Add(levelEditorObjectiveListItem);
				break;
			case EObjectiveResult.Lose:
				m_LoseObjectiveListItems.Add(levelEditorObjectiveListItem);
				break;
			default:
				Log.Error("Objective to add new list item for has no result");
				break;
			}
		}
		else
		{
			Log.Error("Objective list gameobject could not be created");
		}
		return levelEditorObjectiveListItem;
	}

	public void EditObjective(LevelEditorObjectiveListItem itemEdited)
	{
		ObjectiveDisplayPanel.SetActive(value: true);
		if ((bool)m_CurrentItem)
		{
			m_CurrentItem.SetSelectedState(selected: false);
		}
		m_CurrentItem = itemEdited;
		m_CurrentItem.SetSelectedState(selected: true);
		m_ButtonModeAdd = false;
		m_CurrentObjectiveResult = itemEdited.CurrentObjectiveResult;
		ObjectiveOptionalToggleWrapper.SetActive(m_CurrentObjectiveResult == EObjectiveResult.Win);
		ObjectiveWinsDespiteExhaustionToggleWrapper.SetActive(m_CurrentObjectiveResult == EObjectiveResult.Win);
		ObjectiveEnoughToWinAloneToggleWrapper.SetActive(m_CurrentObjectiveResult == EObjectiveResult.Win);
		StatusText.text = "-";
		ApplyButtonText.text = "APPLY";
		DeleteButton.gameObject.SetActive(value: true);
		ObjectiveTitle.text = $"{m_CurrentItem.CurrentObjective.Result.ToString()} Objective #{m_CurrentItem.objectiveIndex}";
		ObjectiveTypeDropDown.value = (int)m_CurrentItem.CurrentObjective.ObjectiveType;
		ObjectiveCustomLocInput.text = m_CurrentItem.CurrentObjective.CustomLocKey;
		ObjectiveCustomTileHoverLocInput.text = m_CurrentItem.CurrentObjective.CustomTileHoverLocKey;
		ObjectiveRoundInput.text = ((m_CurrentItem.CurrentObjective is CObjective_ReachRound cObjective_ReachRound) ? cObjective_ReachRound.ReachRoundNumber.ToString() : "");
		ObjectiveEventIdentifier.text = m_CurrentItem.CurrentObjective.EventIdentifier;
		ObjectiveOptionalToggle.SetIsOnWithoutNotify(m_CurrentItem.CurrentObjective.IsOptional);
		ObjectiveWinsDespiteExhaustionToggle.SetIsOnWithoutNotify(m_CurrentItem.CurrentObjective.WinsDespiteExhaustion);
		ObjectiveEnoughToWinAloneToggle.SetIsOnWithoutNotify(m_CurrentItem.CurrentObjective.EnoughToWinAlone);
		ObjectiveActiveFromStartToggle.SetIsOnWithoutNotify(m_CurrentItem.CurrentObjective.IsActiveFromStart);
		if (m_CurrentItem.CurrentObjective.IsActiveFromStart)
		{
			ObjectiveActivationRoundWrapper.SetActive(value: false);
		}
		else
		{
			ObjectiveActivationRoundWrapper.SetActive(value: true);
			ObjectiveActivationRoundInput.text = (m_CurrentItem.CurrentObjective.ActivateOnRound.HasValue ? m_CurrentItem.CurrentObjective.ActivateOnRound.ToString() : "NULL");
		}
		ObjectiveHiddenToggle.SetIsOnWithoutNotify(m_CurrentItem.CurrentObjective.IsHidden);
		ObjectiveHideCounter.SetIsOnWithoutNotify(m_CurrentItem.CurrentObjective.HideCounter);
		ObjectiveFilterPanel.SetShowing(m_CurrentItem.CurrentObjective.ObjectiveFilter);
		ObjectiveDependencyPanel.SetupForObjective(m_CurrentItem.CurrentObjective);
		ObjectiveMapListPanel.SetupDelegateActions(OnButtonAddMapToListPressed);
		FillActorNameDropDown();
		FillMapNameDropDown();
		RefreshTileList();
		RefreshMapList();
		if (m_CurrentItem.CurrentObjective is CObjective_XCharactersDie cObjective_XCharactersDie)
		{
			ObjectiveAmountInputPartySize1.text = cObjective_XCharactersDie.KillAmount[0].ToString();
			ObjectiveAmountInputPartySize2.text = cObjective_XCharactersDie.KillAmount[1].ToString();
			ObjectiveAmountInputPartySize3.text = cObjective_XCharactersDie.KillAmount[2].ToString();
			ObjectiveAmountInputPartySize4.text = cObjective_XCharactersDie.KillAmount[3].ToString();
		}
		else if (m_CurrentItem.CurrentObjective is CObjective_LootX cObjective_LootX)
		{
			ObjectiveAmountInputPartySize1.text = cObjective_LootX.LootAmount[0].ToString();
			ObjectiveAmountInputPartySize2.text = cObjective_LootX.LootAmount[1].ToString();
			ObjectiveAmountInputPartySize3.text = cObjective_LootX.LootAmount[2].ToString();
			ObjectiveAmountInputPartySize4.text = cObjective_LootX.LootAmount[3].ToString();
		}
		else if (m_CurrentItem.CurrentObjective is CObjective_DestroyXObjects cObjective_DestroyXObjects)
		{
			ObjectiveAmountInputPartySize1.text = cObjective_DestroyXObjects.DestroyAmount[0].ToString();
			ObjectiveAmountInputPartySize2.text = cObjective_DestroyXObjects.DestroyAmount[1].ToString();
			ObjectiveAmountInputPartySize3.text = cObjective_DestroyXObjects.DestroyAmount[2].ToString();
			ObjectiveAmountInputPartySize4.text = cObjective_DestroyXObjects.DestroyAmount[3].ToString();
		}
		else if (m_CurrentItem.CurrentObjective is CObjective_ActorReachPosition cObjective_ActorReachPosition)
		{
			ObjectiveAmountInputPartySize1.text = cObjective_ActorReachPosition.ReachPositionAmounts[0].ToString();
			ObjectiveAmountInputPartySize2.text = cObjective_ActorReachPosition.ReachPositionAmounts[1].ToString();
			ObjectiveAmountInputPartySize3.text = cObjective_ActorReachPosition.ReachPositionAmounts[2].ToString();
			ObjectiveAmountInputPartySize4.text = cObjective_ActorReachPosition.ReachPositionAmounts[3].ToString();
		}
		else if (m_CurrentItem.CurrentObjective is CObjective_AnyActorReachPosition cObjective_AnyActorReachPosition)
		{
			ObjectiveAmountInputPartySize1.text = cObjective_AnyActorReachPosition.ReachPositionAmounts[0].ToString();
			ObjectiveAmountInputPartySize2.text = cObjective_AnyActorReachPosition.ReachPositionAmounts[1].ToString();
			ObjectiveAmountInputPartySize3.text = cObjective_AnyActorReachPosition.ReachPositionAmounts[2].ToString();
			ObjectiveAmountInputPartySize4.text = cObjective_AnyActorReachPosition.ReachPositionAmounts[3].ToString();
		}
		else if (m_CurrentItem.CurrentObjective is CObjective_ActivatePressurePlateX cObjective_ActivatePressurePlateX)
		{
			ObjectiveAmountInputPartySize1.text = cObjective_ActivatePressurePlateX.NumberToActivate[0].ToString();
			ObjectiveAmountInputPartySize2.text = cObjective_ActivatePressurePlateX.NumberToActivate[1].ToString();
			ObjectiveAmountInputPartySize3.text = cObjective_ActivatePressurePlateX.NumberToActivate[2].ToString();
			ObjectiveAmountInputPartySize4.text = cObjective_ActivatePressurePlateX.NumberToActivate[3].ToString();
		}
		else if (m_CurrentItem.CurrentObjective is CObjective_ActivateXProps cObjective_ActivateXProps)
		{
			ObjectiveAmountInputPartySize1.text = cObjective_ActivateXProps.ActivateAmount[0].ToString();
			ObjectiveAmountInputPartySize2.text = cObjective_ActivateXProps.ActivateAmount[1].ToString();
			ObjectiveAmountInputPartySize3.text = cObjective_ActivateXProps.ActivateAmount[2].ToString();
			ObjectiveAmountInputPartySize4.text = cObjective_ActivateXProps.ActivateAmount[3].ToString();
		}
		else if (m_CurrentItem.CurrentObjective is CObjective_DeactivateXSpawners cObjective_DeactivateXSpawners)
		{
			ObjectiveAmountInputPartySize1.text = cObjective_DeactivateXSpawners.NumberToDeactivate[0].ToString();
			ObjectiveAmountInputPartySize2.text = cObjective_DeactivateXSpawners.NumberToDeactivate[1].ToString();
			ObjectiveAmountInputPartySize3.text = cObjective_DeactivateXSpawners.NumberToDeactivate[2].ToString();
			ObjectiveAmountInputPartySize4.text = cObjective_DeactivateXSpawners.NumberToDeactivate[3].ToString();
		}
		else if (m_CurrentItem.CurrentObjective is CObjective_ActivateXSpawners cObjective_ActivateXSpawners)
		{
			ObjectiveAmountInputPartySize1.text = cObjective_ActivateXSpawners.NumberToActivate[0].ToString();
			ObjectiveAmountInputPartySize2.text = cObjective_ActivateXSpawners.NumberToActivate[1].ToString();
			ObjectiveAmountInputPartySize3.text = cObjective_ActivateXSpawners.NumberToActivate[2].ToString();
			ObjectiveAmountInputPartySize4.text = cObjective_ActivateXSpawners.NumberToActivate[3].ToString();
		}
		else if (m_CurrentItem.CurrentObjective is CObjective_ActorsEscaped cObjective_ActorsEscaped)
		{
			ObjectiveAmountInputPartySize1.text = cObjective_ActorsEscaped.TargetNumberOfEscapees[0].ToString();
			ObjectiveAmountInputPartySize2.text = cObjective_ActorsEscaped.TargetNumberOfEscapees[1].ToString();
			ObjectiveAmountInputPartySize3.text = cObjective_ActorsEscaped.TargetNumberOfEscapees[2].ToString();
			ObjectiveAmountInputPartySize4.text = cObjective_ActorsEscaped.TargetNumberOfEscapees[3].ToString();
		}
		else if (m_CurrentItem.CurrentObjective is CObjective_BringItemsToArea cObjective_BringItemsToArea)
		{
			ObjectiveAmountInputPartySize1.text = cObjective_BringItemsToArea.TargetNumberOfItems[0].ToString();
			ObjectiveAmountInputPartySize2.text = cObjective_BringItemsToArea.TargetNumberOfItems[1].ToString();
			ObjectiveAmountInputPartySize3.text = cObjective_BringItemsToArea.TargetNumberOfItems[2].ToString();
			ObjectiveAmountInputPartySize4.text = cObjective_BringItemsToArea.TargetNumberOfItems[3].ToString();
		}
		else if (m_CurrentItem.CurrentObjective is CObjective_DealXDamage cObjective_DealXDamage)
		{
			ObjectiveAmountInputPartySize1.text = cObjective_DealXDamage.TargetDamage[0].ToString();
			ObjectiveAmountInputPartySize2.text = cObjective_DealXDamage.TargetDamage[1].ToString();
			ObjectiveAmountInputPartySize3.text = cObjective_DealXDamage.TargetDamage[2].ToString();
			ObjectiveAmountInputPartySize4.text = cObjective_DealXDamage.TargetDamage[3].ToString();
		}
		else
		{
			ObjectiveAmountInputPartySize1.text = "";
			ObjectiveAmountInputPartySize2.text = "";
			ObjectiveAmountInputPartySize3.text = "";
			ObjectiveAmountInputPartySize4.text = "";
		}
		if (m_CurrentItem.CurrentObjective is CObjective_CustomTrigger cObjective_CustomTrigger)
		{
			CustomTriggerPanel.InitForMessageTrigger(cObjective_CustomTrigger.CustomTrigger);
		}
		ExposeParametersBasedOnObjectiveType(m_CurrentItem.CurrentObjective.ObjectiveType);
	}

	private void ExposeParametersBasedOnObjectiveType(EObjectiveType typeToExposeFor)
	{
		switch (typeToExposeFor)
		{
		case EObjectiveType.None:
		case EObjectiveType.KillAllEnemies:
		case EObjectiveType.KillAllBosses:
			ObjectiveRoundWrapper.SetActive(value: false);
			ObjectiveActorWrapper.SetActive(value: false);
			ObjectiveAmountWrapper.SetActive(value: false);
			ObjectiveAmountLabelsWrapper.SetActive(value: false);
			ObjectiveCountFromActivationRoundWrapper.SetActive(value: false);
			ObjectiveFilterPanel.gameObject.SetActive(value: true);
			CustomTriggerPanel.gameObject.SetActive(value: false);
			ObjectiveTileListElement.gameObject.SetActive(value: false);
			ObjectiveMapListElement.gameObject.SetActive(value: false);
			break;
		case EObjectiveType.ReachRound:
			ObjectiveRoundWrapper.SetActive(value: true);
			ObjectiveActorWrapper.SetActive(value: false);
			ObjectiveAmountWrapper.SetActive(value: false);
			ObjectiveAmountLabelsWrapper.SetActive(value: false);
			ObjectiveCountFromActivationRoundWrapper.SetActive(value: true);
			ObjectiveFilterPanel.gameObject.SetActive(value: true);
			CustomTriggerPanel.gameObject.SetActive(value: false);
			ObjectiveTileListElement.gameObject.SetActive(value: false);
			ObjectiveMapListElement.gameObject.SetActive(value: false);
			break;
		case EObjectiveType.ActorReachPosition:
		case EObjectiveType.AnyActorReachPosition:
		case EObjectiveType.ActorsEscaped:
		case EObjectiveType.BringItemsToArea:
			ObjectiveRoundWrapper.SetActive(value: false);
			ObjectiveActorWrapper.SetActive(value: false);
			ObjectiveAmountWrapper.SetActive(value: true);
			ObjectiveAmountLabelsWrapper.SetActive(value: true);
			ObjectiveCountFromActivationRoundWrapper.SetActive(value: false);
			ObjectiveFilterPanel.gameObject.SetActive(value: true);
			CustomTriggerPanel.gameObject.SetActive(value: false);
			ObjectiveTileListElement.gameObject.SetActive(value: true);
			ObjectiveMapListElement.gameObject.SetActive(value: false);
			break;
		case EObjectiveType.XCharactersDie:
		case EObjectiveType.LootX:
		case EObjectiveType.DestroyXObjects:
		case EObjectiveType.ActivateXPressurePlates:
		case EObjectiveType.DeactivateXSpawners:
		case EObjectiveType.DealXDamage:
		case EObjectiveType.ActivateXSpawners:
		case EObjectiveType.XActorsHealToMax:
		case EObjectiveType.ActivateXProps:
			ObjectiveRoundWrapper.SetActive(value: false);
			ObjectiveActorWrapper.SetActive(value: false);
			ObjectiveAmountWrapper.SetActive(value: true);
			ObjectiveAmountLabelsWrapper.SetActive(value: true);
			ObjectiveCountFromActivationRoundWrapper.SetActive(value: false);
			ObjectiveFilterPanel.gameObject.SetActive(value: true);
			CustomTriggerPanel.gameObject.SetActive(value: false);
			ObjectiveTileListElement.gameObject.SetActive(value: false);
			ObjectiveMapListElement.gameObject.SetActive(value: false);
			break;
		case EObjectiveType.CustomTrigger:
			ObjectiveRoundWrapper.SetActive(value: false);
			ObjectiveActorWrapper.SetActive(value: false);
			ObjectiveAmountWrapper.SetActive(value: false);
			ObjectiveAmountLabelsWrapper.SetActive(value: false);
			ObjectiveCountFromActivationRoundWrapper.SetActive(value: false);
			ObjectiveFilterPanel.gameObject.SetActive(value: false);
			if (!m_ButtonModeAdd)
			{
				CustomTriggerPanel.gameObject.SetActive(value: true);
			}
			ObjectiveTileListElement.gameObject.SetActive(value: false);
			ObjectiveMapListElement.gameObject.SetActive(value: false);
			break;
		case EObjectiveType.AllCharactersMustLoot:
			ObjectiveRoundWrapper.SetActive(value: false);
			ObjectiveActorWrapper.SetActive(value: false);
			ObjectiveAmountWrapper.SetActive(value: false);
			ObjectiveAmountLabelsWrapper.SetActive(value: false);
			ObjectiveCountFromActivationRoundWrapper.SetActive(value: false);
			ObjectiveFilterPanel.gameObject.SetActive(value: true);
			CustomTriggerPanel.gameObject.SetActive(value: false);
			ObjectiveTileListElement.gameObject.SetActive(value: false);
			ObjectiveMapListElement.gameObject.SetActive(value: false);
			break;
		case EObjectiveType.RevealAllRooms:
			ObjectiveRoundWrapper.SetActive(value: false);
			ObjectiveActorWrapper.SetActive(value: false);
			ObjectiveAmountWrapper.SetActive(value: false);
			ObjectiveAmountLabelsWrapper.SetActive(value: false);
			ObjectiveCountFromActivationRoundWrapper.SetActive(value: false);
			ObjectiveFilterPanel.gameObject.SetActive(value: false);
			CustomTriggerPanel.gameObject.SetActive(value: false);
			ObjectiveTileListElement.gameObject.SetActive(value: false);
			ObjectiveMapListElement.gameObject.SetActive(value: false);
			break;
		case EObjectiveType.ActorsNotInAllRooms:
			ObjectiveRoundWrapper.SetActive(value: false);
			ObjectiveActorWrapper.SetActive(value: false);
			ObjectiveAmountWrapper.SetActive(value: false);
			ObjectiveAmountLabelsWrapper.SetActive(value: false);
			ObjectiveCountFromActivationRoundWrapper.SetActive(value: false);
			ObjectiveFilterPanel.gameObject.SetActive(value: true);
			CustomTriggerPanel.gameObject.SetActive(value: false);
			ObjectiveTileListElement.gameObject.SetActive(value: false);
			ObjectiveMapListElement.gameObject.SetActive(value: true);
			break;
		case EObjectiveType.DamageQuota:
		case EObjectiveType.Scripted:
			break;
		}
	}

	public void RefreshUIFromCurrentScenarioState(ScenarioState scenarioState = null)
	{
		if (scenarioState == null)
		{
			scenarioState = ScenarioManager.CurrentScenarioState;
			if (scenarioState == null)
			{
				return;
			}
		}
		m_CurrentScenarioState = scenarioState;
		foreach (LevelEditorObjectiveListItem winObjectiveListItem in m_WinObjectiveListItems)
		{
			UnityEngine.Object.Destroy(winObjectiveListItem.gameObject);
		}
		m_WinObjectiveListItems.Clear();
		foreach (LevelEditorObjectiveListItem loseObjectiveListItem in m_LoseObjectiveListItems)
		{
			UnityEngine.Object.Destroy(loseObjectiveListItem.gameObject);
		}
		m_LoseObjectiveListItems.Clear();
		for (int i = 0; i < m_CurrentScenarioState.WinObjectives.Count; i++)
		{
			AddNewListItemForObjective(i, m_CurrentScenarioState.WinObjectives[i], EObjectiveResult.Win);
		}
		for (int j = 0; j < m_CurrentScenarioState.LoseObjectives.Count; j++)
		{
			AddNewListItemForObjective(j, m_CurrentScenarioState.LoseObjectives[j], EObjectiveResult.Lose);
		}
	}

	public void FillActorNameDropDown()
	{
		ObjectiveActorDropDown.options.Clear();
		m_ActorOptions.Clear();
		m_ActorOptions.AddRange(ScenarioManager.Scenario.PlayerActors);
		m_ActorOptions.AddRange(ScenarioManager.Scenario.Enemies);
		List<string> list = new List<string> { "NONE" };
		list.AddRange(m_ActorOptions.Select((CActor a) => LocalizationManager.GetTranslation(a.Class.LocKey) + " - [" + a.ActorGuid + "]"));
		ObjectiveActorDropDown.AddOptions(list);
		ObjectiveActorDropDown.value = 0;
	}

	public void FillMapNameDropDown()
	{
		ObjectiveMapListDropDown.options.Clear();
		m_MapOptions.Clear();
		m_MapOptions.AddRange(ScenarioManager.Scenario.Maps);
		List<string> list = new List<string> { "NONE" };
		list.AddRange(m_MapOptions.Select((CMap a) => a.MapType.ToString() + " - [" + a.MapGuid + "]"));
		ObjectiveMapListDropDown.AddOptions(list);
		ObjectiveMapListDropDown.value = 0;
	}

	public void RefreshTileList(bool getTilesFromObjective = true)
	{
		if (m_CurrentTileList == null)
		{
			m_CurrentTileList = new List<TileIndex>();
		}
		if (getTilesFromObjective && m_CurrentItem != null)
		{
			m_CurrentTileList.Clear();
			if (m_CurrentItem.CurrentObjective is CObjective_ActorReachPosition cObjective_ActorReachPosition)
			{
				m_CurrentTileList = cObjective_ActorReachPosition.ActorTargetPositions?.ToList() ?? new List<TileIndex>();
			}
			else if (m_CurrentItem.CurrentObjective is CObjective_AnyActorReachPosition cObjective_AnyActorReachPosition)
			{
				m_CurrentTileList = cObjective_AnyActorReachPosition.ActorTargetPositions?.ToList() ?? new List<TileIndex>();
			}
			else if (m_CurrentItem.CurrentObjective is CObjective_ActorsEscaped cObjective_ActorsEscaped)
			{
				m_CurrentTileList = cObjective_ActorsEscaped.EscapePositions?.ToList() ?? new List<TileIndex>();
			}
			else if (m_CurrentItem.CurrentObjective is CObjective_BringItemsToArea cObjective_BringItemsToArea)
			{
				m_CurrentTileList = cObjective_BringItemsToArea.AreaPositions?.ToList() ?? new List<TileIndex>();
			}
		}
		if (m_CurrentTileListItems == null)
		{
			m_CurrentTileListItems = new List<LevelEditorListItemInlineButtons>();
		}
		else
		{
			for (int i = 0; i < m_CurrentTileListItems.Count; i++)
			{
				UnityEngine.Object.Destroy(m_CurrentTileListItems[i].gameObject);
			}
			m_CurrentTileListItems.Clear();
		}
		for (int j = 0; j < m_CurrentTileList.Count; j++)
		{
			LevelEditorListItemInlineButtons component = UnityEngine.Object.Instantiate(ObjectiveTileListItemPrefab, ObjectiveTileListParentTransform).GetComponent<LevelEditorListItemInlineButtons>();
			component.SetupListItem($"X:{m_CurrentTileList[j].X}, Y:{m_CurrentTileList[j].Y}", j, OnTileListItemDeletePressed, TileItemSelected);
			m_CurrentTileListItems.Add(component);
		}
	}

	public void RefreshMapList(bool getMapsFromObjective = true)
	{
		if (m_CurrentMapGuidList == null)
		{
			m_CurrentMapGuidList = new List<string>();
		}
		if (getMapsFromObjective && m_CurrentItem != null)
		{
			m_CurrentMapGuidList.Clear();
			if (m_CurrentItem.CurrentObjective is CObjective_ActorsNotInAllRooms cObjective_ActorsNotInAllRooms)
			{
				m_CurrentMapGuidList = cObjective_ActorsNotInAllRooms.RoomMapGUIDs?.ToList() ?? new List<string>();
			}
		}
		if (m_CurrentMapListItems == null)
		{
			m_CurrentMapListItems = new List<LevelEditorListItemInlineButtons>();
		}
		else
		{
			for (int i = 0; i < m_CurrentMapListItems.Count; i++)
			{
				UnityEngine.Object.Destroy(m_CurrentMapListItems[i].gameObject);
			}
			m_CurrentMapListItems.Clear();
		}
		for (int j = 0; j < m_CurrentMapGuidList.Count; j++)
		{
			LevelEditorListItemInlineButtons component = UnityEngine.Object.Instantiate(ObjectiveMapListItemPrefab, ObjectiveMapListParentTransform).GetComponent<LevelEditorListItemInlineButtons>();
			component.SetupListItem(m_CurrentMapGuidList[j], j, OnMapListItemDeletePressed, MapItemSelected);
			m_CurrentMapListItems.Add(component);
		}
	}

	public bool ValidateCurrentUIForMessage(out string completionMessage)
	{
		switch ((EObjectiveType)ObjectiveTypeDropDown.value)
		{
		case EObjectiveType.None:
			completionMessage = "Objective type must not be None";
			return false;
		case EObjectiveType.ReachRound:
		{
			if (!int.TryParse(ObjectiveRoundInput.text, out var _))
			{
				completionMessage = "Invalid round value, must be integer for this objective type";
				return false;
			}
			break;
		}
		case EObjectiveType.ActorReachPosition:
		case EObjectiveType.AnyActorReachPosition:
		{
			if (m_CurrentTileList == null || m_CurrentTileList.Count == 0)
			{
				completionMessage = "No Tiles Selected, This objective type requires at least one tile selected";
				return false;
			}
			bool flag = true;
			completionMessage = "";
			if (!int.TryParse(ObjectiveAmountInputPartySize1.text, out var _))
			{
				completionMessage = "Invalid objective amount set for party size 1, must be valid for this objective type (Amount = Required Actors in area)";
				flag = false;
			}
			if (!int.TryParse(ObjectiveAmountInputPartySize2.text, out var _))
			{
				completionMessage = "Invalid objective amount set for party size 2, must be valid for this objective type (Amount = Required Actors in area)";
				flag = false;
			}
			if (!int.TryParse(ObjectiveAmountInputPartySize3.text, out var _))
			{
				completionMessage = "Invalid objective amount set for party size 3, must be valid for this objective type (Amount = Required Actors in area)";
				flag = false;
			}
			if (!int.TryParse(ObjectiveAmountInputPartySize4.text, out var _))
			{
				completionMessage = "Invalid objective amount set for party size 4, must be valid for this objective type (Amount = Required Actors in area)";
				flag = false;
			}
			if (!flag)
			{
				return false;
			}
			break;
		}
		case EObjectiveType.XCharactersDie:
		case EObjectiveType.LootX:
		case EObjectiveType.DestroyXObjects:
		case EObjectiveType.ActivateXPressurePlates:
		case EObjectiveType.DeactivateXSpawners:
		case EObjectiveType.ActorsEscaped:
		case EObjectiveType.DealXDamage:
		case EObjectiveType.ActivateXSpawners:
		case EObjectiveType.XActorsHealToMax:
		case EObjectiveType.ActivateXProps:
		case EObjectiveType.BringItemsToArea:
		{
			bool flag2 = true;
			completionMessage = "";
			if (!int.TryParse(ObjectiveAmountInputPartySize1.text, out var _))
			{
				completionMessage = "Invalid objective amount set for party size 1, must be valid for this objective type";
				flag2 = false;
			}
			if (!int.TryParse(ObjectiveAmountInputPartySize2.text, out var _))
			{
				completionMessage = "Invalid objective amount set for party size 2, must be valid for this objective type";
				flag2 = false;
			}
			if (!int.TryParse(ObjectiveAmountInputPartySize3.text, out var _))
			{
				completionMessage = "Invalid objective amount set for party size 3, must be valid for this objective type";
				flag2 = false;
			}
			if (!int.TryParse(ObjectiveAmountInputPartySize4.text, out var _))
			{
				completionMessage = "Invalid objective amount set for party size 4, must be valid for this objective type";
				flag2 = false;
			}
			if (!flag2)
			{
				return false;
			}
			break;
		}
		case EObjectiveType.ActorsNotInAllRooms:
			if (m_CurrentMapGuidList == null || m_CurrentMapGuidList.Count == 0)
			{
				completionMessage = "No Maps Selected, This objective type requires at least one map selected";
				return false;
			}
			break;
		}
		completionMessage = "Validation succeeded";
		return true;
	}

	public void OnButtonAddObjectivePressed(int objectiveResult)
	{
		if ((bool)m_CurrentItem)
		{
			m_CurrentItem.SetSelectedState(selected: false);
		}
		m_CurrentItem = null;
		m_ButtonModeAdd = true;
		ApplyButtonText.text = "ADD NEW";
		DeleteButton.gameObject.SetActive(value: false);
		m_CurrentObjectiveResult = (EObjectiveResult)objectiveResult;
		ObjectiveTitle.text = "OBJECTIVE TO ADD";
		ObjectiveTypeDropDown.value = 0;
		ObjectiveCustomLocInput.text = string.Empty;
		ObjectiveCustomTileHoverLocInput.text = string.Empty;
		ObjectiveRoundInput.text = "";
		ObjectiveEventIdentifier.text = "";
		ObjectiveActiveFromStartToggle.SetIsOnWithoutNotify(value: true);
		ObjectiveActivationRoundWrapper.SetActive(value: false);
		ObjectiveActivationRoundInput.text = "NULL";
		ObjectiveHiddenToggle.SetIsOnWithoutNotify(value: false);
		ObjectiveHideCounter.SetIsOnWithoutNotify(value: false);
		ObjectiveWinsDespiteExhaustionToggle.SetIsOnWithoutNotify(value: false);
		ObjectiveEnoughToWinAloneToggle.SetIsOnWithoutNotify(value: false);
		ObjectiveAmountInputPartySize1.text = "";
		ObjectiveAmountInputPartySize2.text = "";
		ObjectiveAmountInputPartySize3.text = "";
		ObjectiveAmountInputPartySize4.text = "";
		ObjectiveFilterPanel.SetShowing(new CObjectiveFilter());
		ObjectiveDependencyPanel.SetupForObjective(null);
		ObjectiveMapListPanel.SetupDelegateActions(OnButtonAddMapToListPressed);
		FillActorNameDropDown();
		FillMapNameDropDown();
		RefreshTileList();
		RefreshMapList();
		ExposeParametersBasedOnObjectiveType((EObjectiveType)ObjectiveTypeDropDown.value);
		ObjectiveDisplayPanel.SetActive(value: true);
	}

	public async void OnButtonApplyPressed()
	{
		if (ValidateCurrentUIForMessage(out var completionMessage))
		{
			EObjectiveType value = (EObjectiveType)ObjectiveTypeDropDown.value;
			EObjectiveResult currentResult = CObjective.ObjectiveResults.SingleOrDefault((EObjectiveResult s) => s == m_CurrentObjectiveResult);
			bool activeFromStart = ObjectiveActiveFromStartToggle.isOn;
			bool isOn = ObjectiveCountFromActivationRound.isOn;
			bool isOptional = currentResult == EObjectiveResult.Win && ObjectiveOptionalToggle.isOn;
			bool winsDespiteExhaustion = currentResult == EObjectiveResult.Win && ObjectiveWinsDespiteExhaustionToggle.isOn;
			bool enoughToWinAlone = currentResult == EObjectiveResult.Win && ObjectiveEnoughToWinAloneToggle.isOn;
			Dictionary<string, bool> requiredObjectiveStates = ObjectiveDependencyPanel.DependenciesBeingShown;
			bool hideCounter = ObjectiveHideCounter.isOn;
			int? activationRound = null;
			try
			{
				activationRound = int.Parse(ObjectiveActivationRoundInput.text);
			}
			catch
			{
			}
			ObjectiveFilterPanel.Apply();
			CObjective cObjective = null;
			switch (value)
			{
			case EObjectiveType.ActorReachPosition:
			{
				List<int> reachPositionAmounts = new List<int>
				{
					int.Parse(ObjectiveAmountInputPartySize1.text),
					int.Parse(ObjectiveAmountInputPartySize2.text),
					int.Parse(ObjectiveAmountInputPartySize3.text),
					int.Parse(ObjectiveAmountInputPartySize4.text)
				};
				cObjective = new CObjective_ActorReachPosition(currentResult, ObjectiveFilterPanel.FilterBeingShown, reachPositionAmounts, m_CurrentTileList.ToList(), hideCounter, activeFromStart, activationRound, ObjectiveCustomLocInput.text, ObjectiveCustomTileHoverLocInput.text, ObjectiveEventIdentifier.text, ObjectiveHiddenToggle.isOn, isOptional, winsDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates);
				break;
			}
			case EObjectiveType.ReachRound:
				cObjective = new CObjective_ReachRound(currentResult, ObjectiveFilterPanel.FilterBeingShown, int.Parse(ObjectiveRoundInput.text), isOn, hideCounter, activeFromStart, activationRound, ObjectiveCustomLocInput.text, ObjectiveCustomTileHoverLocInput.text, ObjectiveEventIdentifier.text, ObjectiveHiddenToggle.isOn, isOptional, winsDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates);
				break;
			case EObjectiveType.None:
				cObjective = new CObjective(value, currentResult, ObjectiveFilterPanel.FilterBeingShown, hideCounter, activeFromStart, activationRound, ObjectiveCustomLocInput.text, ObjectiveCustomTileHoverLocInput.text, ObjectiveEventIdentifier.text, ObjectiveHiddenToggle.isOn, isOptional, winsDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates);
				break;
			case EObjectiveType.KillAllEnemies:
				cObjective = new CObjective_KillAllEnemies(currentResult, ObjectiveFilterPanel.FilterBeingShown, hideCounter, activeFromStart, activationRound, ObjectiveCustomLocInput.text, ObjectiveCustomTileHoverLocInput.text, ObjectiveEventIdentifier.text, ObjectiveHiddenToggle.isOn, isOptional, winsDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates);
				break;
			case EObjectiveType.KillAllBosses:
				cObjective = new CObjective_KillAllBosses(currentResult, ObjectiveFilterPanel.FilterBeingShown, hideCounter, activeFromStart, activationRound, ObjectiveCustomLocInput.text, ObjectiveCustomTileHoverLocInput.text, ObjectiveEventIdentifier.text, ObjectiveHiddenToggle.isOn, isOptional, winsDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates);
				break;
			case EObjectiveType.XCharactersDie:
			{
				List<int> killAmount = new List<int>
				{
					int.Parse(ObjectiveAmountInputPartySize1.text),
					int.Parse(ObjectiveAmountInputPartySize2.text),
					int.Parse(ObjectiveAmountInputPartySize3.text),
					int.Parse(ObjectiveAmountInputPartySize4.text)
				};
				cObjective = new CObjective_XCharactersDie(currentResult, ObjectiveFilterPanel.FilterBeingShown, killAmount, hideCounter, activeFromStart, activationRound, ObjectiveCustomLocInput.text, ObjectiveCustomTileHoverLocInput.text, ObjectiveEventIdentifier.text, ObjectiveHiddenToggle.isOn, isOptional, winsDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates);
				break;
			}
			case EObjectiveType.LootX:
			{
				List<int> lootAmount = new List<int>
				{
					int.Parse(ObjectiveAmountInputPartySize1.text),
					int.Parse(ObjectiveAmountInputPartySize2.text),
					int.Parse(ObjectiveAmountInputPartySize3.text),
					int.Parse(ObjectiveAmountInputPartySize4.text)
				};
				cObjective = new CObjective_LootX(currentResult, ObjectiveFilterPanel.FilterBeingShown, lootAmount, hideCounter, activeFromStart, activationRound, ObjectiveCustomLocInput.text, ObjectiveCustomTileHoverLocInput.text, ObjectiveEventIdentifier.text, ObjectiveHiddenToggle.isOn, isOptional, winsDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates);
				break;
			}
			case EObjectiveType.DestroyXObjects:
			{
				List<int> destroyAmount = new List<int>
				{
					int.Parse(ObjectiveAmountInputPartySize1.text),
					int.Parse(ObjectiveAmountInputPartySize2.text),
					int.Parse(ObjectiveAmountInputPartySize3.text),
					int.Parse(ObjectiveAmountInputPartySize4.text)
				};
				cObjective = new CObjective_DestroyXObjects(currentResult, ObjectiveFilterPanel.FilterBeingShown, destroyAmount, hideCounter, activeFromStart, activationRound, ObjectiveCustomLocInput.text, ObjectiveCustomTileHoverLocInput.text, ObjectiveEventIdentifier.text, ObjectiveHiddenToggle.isOn, isOptional, winsDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates);
				break;
			}
			case EObjectiveType.ActivateXProps:
			{
				List<int> activateAmount = new List<int>
				{
					int.Parse(ObjectiveAmountInputPartySize1.text),
					int.Parse(ObjectiveAmountInputPartySize2.text),
					int.Parse(ObjectiveAmountInputPartySize3.text),
					int.Parse(ObjectiveAmountInputPartySize4.text)
				};
				cObjective = new CObjective_ActivateXProps(currentResult, ObjectiveFilterPanel.FilterBeingShown, activateAmount, hideCounter, activeFromStart, activationRound, ObjectiveCustomLocInput.text, ObjectiveCustomTileHoverLocInput.text, ObjectiveEventIdentifier.text, ObjectiveHiddenToggle.isOn, isOptional, winsDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates);
				break;
			}
			case EObjectiveType.ActivateXPressurePlates:
			{
				List<int> numberToActivate3 = new List<int>
				{
					int.Parse(ObjectiveAmountInputPartySize1.text),
					int.Parse(ObjectiveAmountInputPartySize2.text),
					int.Parse(ObjectiveAmountInputPartySize3.text),
					int.Parse(ObjectiveAmountInputPartySize4.text)
				};
				cObjective = new CObjective_ActivatePressurePlateX(currentResult, ObjectiveFilterPanel.FilterBeingShown, numberToActivate3, hideCounter, activeFromStart, activationRound, ObjectiveCustomLocInput.text, ObjectiveCustomTileHoverLocInput.text, ObjectiveEventIdentifier.text, ObjectiveHiddenToggle.isOn, isOptional, winsDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates);
				break;
			}
			case EObjectiveType.RevealAllRooms:
				cObjective = new CObjective_RevealAllRooms(currentResult, ObjectiveFilterPanel.FilterBeingShown, hideCounter, activeFromStart, activationRound, ObjectiveCustomLocInput.text, ObjectiveCustomTileHoverLocInput.text, ObjectiveEventIdentifier.text, ObjectiveHiddenToggle.isOn, isOptional, winsDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates);
				break;
			case EObjectiveType.AllCharactersMustLoot:
				cObjective = new CObjective_AllCharactersMustLoot(currentResult, ObjectiveFilterPanel.FilterBeingShown, hideCounter, activeFromStart, activationRound, ObjectiveCustomLocInput.text, ObjectiveCustomTileHoverLocInput.text, ObjectiveEventIdentifier.text, ObjectiveHiddenToggle.isOn, isOptional, winsDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates);
				break;
			case EObjectiveType.AnyActorReachPosition:
			{
				List<int> reachPositionAmounts2 = new List<int>
				{
					int.Parse(ObjectiveAmountInputPartySize1.text),
					int.Parse(ObjectiveAmountInputPartySize2.text),
					int.Parse(ObjectiveAmountInputPartySize3.text),
					int.Parse(ObjectiveAmountInputPartySize4.text)
				};
				cObjective = new CObjective_AnyActorReachPosition(currentResult, ObjectiveFilterPanel.FilterBeingShown, reachPositionAmounts2, m_CurrentTileList.ToList(), hideCounter, activeFromStart, activationRound, ObjectiveCustomLocInput.text, ObjectiveCustomTileHoverLocInput.text, ObjectiveEventIdentifier.text, ObjectiveHiddenToggle.isOn, isOptional, winsDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates);
				break;
			}
			case EObjectiveType.DeactivateXSpawners:
			{
				List<int> numberToActivate2 = new List<int>
				{
					int.Parse(ObjectiveAmountInputPartySize1.text),
					int.Parse(ObjectiveAmountInputPartySize2.text),
					int.Parse(ObjectiveAmountInputPartySize3.text),
					int.Parse(ObjectiveAmountInputPartySize4.text)
				};
				cObjective = new CObjective_DeactivateXSpawners(currentResult, ObjectiveFilterPanel.FilterBeingShown, numberToActivate2, hideCounter, activeFromStart, activationRound, ObjectiveCustomLocInput.text, ObjectiveCustomTileHoverLocInput.text, ObjectiveEventIdentifier.text, ObjectiveHiddenToggle.isOn, isOptional, winsDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates);
				break;
			}
			case EObjectiveType.ActivateXSpawners:
			{
				List<int> numberToActivate = new List<int>
				{
					int.Parse(ObjectiveAmountInputPartySize1.text),
					int.Parse(ObjectiveAmountInputPartySize2.text),
					int.Parse(ObjectiveAmountInputPartySize3.text),
					int.Parse(ObjectiveAmountInputPartySize4.text)
				};
				cObjective = new CObjective_ActivateXSpawners(currentResult, ObjectiveFilterPanel.FilterBeingShown, numberToActivate, hideCounter, activeFromStart, activationRound, ObjectiveCustomLocInput.text, ObjectiveCustomTileHoverLocInput.text, ObjectiveEventIdentifier.text, ObjectiveHiddenToggle.isOn, isOptional, winsDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates);
				break;
			}
			case EObjectiveType.ActorsEscaped:
			{
				List<int> targetNumberOfEscapees2 = new List<int>
				{
					int.Parse(ObjectiveAmountInputPartySize1.text),
					int.Parse(ObjectiveAmountInputPartySize2.text),
					int.Parse(ObjectiveAmountInputPartySize3.text),
					int.Parse(ObjectiveAmountInputPartySize4.text)
				};
				cObjective = new CObjective_ActorsEscaped(currentResult, ObjectiveFilterPanel.FilterBeingShown, targetNumberOfEscapees2, m_CurrentTileList.ToList(), hideCounter, activeFromStart, activationRound, ObjectiveCustomLocInput.text, ObjectiveCustomTileHoverLocInput.text, ObjectiveEventIdentifier.text, ObjectiveHiddenToggle.isOn, isOptional, winsDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates);
				break;
			}
			case EObjectiveType.BringItemsToArea:
			{
				List<int> targetNumberOfEscapees = new List<int>
				{
					int.Parse(ObjectiveAmountInputPartySize1.text),
					int.Parse(ObjectiveAmountInputPartySize2.text),
					int.Parse(ObjectiveAmountInputPartySize3.text),
					int.Parse(ObjectiveAmountInputPartySize4.text)
				};
				cObjective = new CObjective_BringItemsToArea(currentResult, ObjectiveFilterPanel.FilterBeingShown, targetNumberOfEscapees, m_CurrentTileList.ToList(), hideCounter, activeFromStart, activationRound, ObjectiveCustomLocInput.text, ObjectiveCustomTileHoverLocInput.text, ObjectiveEventIdentifier.text, ObjectiveHiddenToggle.isOn, isOptional, winsDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates);
				break;
			}
			case EObjectiveType.DealXDamage:
			{
				List<int> targetDamage = new List<int>
				{
					int.Parse(ObjectiveAmountInputPartySize1.text),
					int.Parse(ObjectiveAmountInputPartySize2.text),
					int.Parse(ObjectiveAmountInputPartySize3.text),
					int.Parse(ObjectiveAmountInputPartySize4.text)
				};
				cObjective = new CObjective_DealXDamage(currentResult, ObjectiveFilterPanel.FilterBeingShown, targetDamage, hideCounter, activeFromStart, activationRound, ObjectiveCustomLocInput.text, ObjectiveCustomTileHoverLocInput.text, ObjectiveEventIdentifier.text, ObjectiveHiddenToggle.isOn, isOptional, winsDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates);
				break;
			}
			case EObjectiveType.DamageQuota:
				cObjective = new CObjective_DamageQuota(currentResult, ObjectiveFilterPanel.FilterBeingShown, new List<int>(), hideCounter, activeFromStart, activationRound, ObjectiveCustomLocInput.text, ObjectiveCustomTileHoverLocInput.text, ObjectiveEventIdentifier.text, ObjectiveHiddenToggle.isOn, isOptional, winsDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates);
				break;
			case EObjectiveType.ActorsNotInAllRooms:
				cObjective = new CObjective_ActorsNotInAllRooms(currentResult, ObjectiveFilterPanel.FilterBeingShown, m_CurrentMapGuidList.ToList(), hideCounter, activeFromStart, activationRound, ObjectiveCustomLocInput.text, ObjectiveCustomTileHoverLocInput.text, ObjectiveEventIdentifier.text, ObjectiveHiddenToggle.isOn, isOptional, winsDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates);
				break;
			case EObjectiveType.XActorsHealToMax:
			{
				List<int> actorAmount = new List<int>
				{
					int.Parse(ObjectiveAmountInputPartySize1.text),
					int.Parse(ObjectiveAmountInputPartySize2.text),
					int.Parse(ObjectiveAmountInputPartySize3.text),
					int.Parse(ObjectiveAmountInputPartySize4.text)
				};
				cObjective = new CObjective_XActorsHealToMax(currentResult, ObjectiveFilterPanel.FilterBeingShown, actorAmount, hideCounter, activeFromStart, activationRound, ObjectiveCustomLocInput.text, ObjectiveCustomTileHoverLocInput.text, ObjectiveEventIdentifier.text, ObjectiveHiddenToggle.isOn, isOptional, winsDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates);
				break;
			}
			case EObjectiveType.CustomTrigger:
				CustomTriggerPanel.SaveValuesToTrigger();
				cObjective = new CObjective_CustomTrigger(customTrigger: (!m_ButtonModeAdd && CustomTriggerPanel.CurrentTrigger != null) ? (await CustomTriggerPanel.CurrentTrigger.DeepCopySerializableObject<CLevelTrigger>()) : new CLevelTrigger(), result: currentResult, objectiveFilter: ObjectiveFilterPanel.FilterBeingShown, hideCounter: hideCounter, activeFromStart: activeFromStart, activateOnRound: activationRound, customLoc: ObjectiveCustomLocInput.text, customTileHoverLoc: ObjectiveCustomTileHoverLocInput.text, eventIdentifier: ObjectiveEventIdentifier.text, isHidden: ObjectiveHiddenToggle.isOn, isOptional: isOptional, winDespiteExhaustion: winsDespiteExhaustion, enoughToWinAlone: enoughToWinAlone, requiredObjectiveStates: requiredObjectiveStates);
				break;
			case EObjectiveType.Scripted:
				cObjective = new CObjective_Scripted(currentResult, ObjectiveFilterPanel.FilterBeingShown, hideCounter, activeFromStart, activationRound, ObjectiveCustomLocInput.text, ObjectiveCustomTileHoverLocInput.text, ObjectiveEventIdentifier.text, ObjectiveHiddenToggle.isOn, isOptional, winsDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates);
				break;
			}
			if (m_ButtonModeAdd)
			{
				int atIndex = 0;
				switch (currentResult)
				{
				case EObjectiveResult.Win:
					atIndex = LevelEditorController.AddWinObjectiveToScenario(cObjective);
					break;
				case EObjectiveResult.Lose:
					atIndex = LevelEditorController.AddLoseObjectiveToScenario(cObjective);
					break;
				}
				LevelEditorObjectiveListItem itemEdited = AddNewListItemForObjective(atIndex, cObjective, currentResult);
				EditObjective(itemEdited);
				StatusText.text = "<color=green> - Successfully added objective - </color>";
			}
			else
			{
				switch (currentResult)
				{
				case EObjectiveResult.Win:
					LevelEditorController.ReplaceWinObjectiveInScenarioAtIndex(m_CurrentItem.objectiveIndex, cObjective);
					break;
				case EObjectiveResult.Lose:
					LevelEditorController.ReplaceLoseObjectiveInScenarioAtIndex(m_CurrentItem.objectiveIndex, cObjective);
					break;
				}
				m_CurrentItem.RefreshDisplayName();
				StatusText.text = "<color=green> - Successfully edited objective - </color>";
			}
		}
		else
		{
			StatusText.text = "<color=red> - " + completionMessage + " - </color>";
			Log.Error("Objective validation failed: " + completionMessage);
		}
	}

	public void OnButtonDeletePressed()
	{
		if (m_CurrentItem != null)
		{
			if (m_CurrentItem.CurrentObjective.Result == EObjectiveResult.Win)
			{
				LevelEditorController.DeleteWinObjectiveFromScenario(m_CurrentItem.objectiveIndex);
				m_WinObjectiveListItems.Remove(m_CurrentItem);
			}
			else if (m_CurrentItem.CurrentObjective.Result == EObjectiveResult.Lose)
			{
				LevelEditorController.DeleteLoseObjectiveFromScenario(m_CurrentItem.objectiveIndex);
				m_LoseObjectiveListItems.Remove(m_CurrentItem);
			}
			UnityEngine.Object.Destroy(m_CurrentItem.gameObject);
			m_CurrentItem = null;
			ObjectiveDisplayPanel.SetActive(value: false);
			RefreshUIFromCurrentScenarioState();
		}
	}

	public void OnMoveUpPressed()
	{
		if (!(m_CurrentItem == null) && m_CurrentItem.objectiveIndex != 0)
		{
			MoveCurrentItem(-1);
		}
	}

	public void OnMoveDownPressed()
	{
		if (!(m_CurrentItem == null) && (m_CurrentItem.CurrentObjective.Result != EObjectiveResult.Win || m_CurrentItem.objectiveIndex < m_CurrentScenarioState.WinObjectives.Count - 1) && (m_CurrentItem.CurrentObjective.Result != EObjectiveResult.Lose || m_CurrentItem.objectiveIndex < m_CurrentScenarioState.LoseObjectives.Count - 1))
		{
			MoveCurrentItem(1);
		}
	}

	private void MoveCurrentItem(int amount)
	{
		int num = m_CurrentItem.objectiveIndex + amount;
		bool flag = m_CurrentItem.CurrentObjective.Result == EObjectiveResult.Win;
		List<CObjective> list2;
		List<CObjective> list = (list2 = (flag ? m_CurrentScenarioState.WinObjectives : m_CurrentScenarioState.LoseObjectives));
		int index = num;
		List<CObjective> list3 = list;
		int objectiveIndex = m_CurrentItem.objectiveIndex;
		CObjective cObjective = list[m_CurrentItem.objectiveIndex];
		CObjective cObjective2 = list[num];
		CObjective cObjective3 = (list2[index] = cObjective);
		cObjective3 = (list3[objectiveIndex] = cObjective2);
		RefreshUIFromCurrentScenarioState();
		m_CurrentItem = null;
		EditObjective(flag ? m_WinObjectiveListItems[num] : m_LoseObjectiveListItems[num]);
	}

	public void OnObjectiveTypeDropDownChanged()
	{
		ExposeParametersBasedOnObjectiveType((EObjectiveType)ObjectiveTypeDropDown.value);
	}

	public void OnActiveFromStartChanged()
	{
		if (ObjectiveActiveFromStartToggle.isOn)
		{
			ObjectiveActivationRoundWrapper.SetActive(value: false);
			return;
		}
		ObjectiveActivationRoundWrapper.SetActive(value: true);
		ObjectiveActivationRoundInput.text = "NULL";
	}

	public void OnButtonAddTileToListPressed()
	{
		LevelEditorController.SelectTile(OnTileSelectedToAddToList);
	}

	public void OnButtonAddMapToListPressed(string mapSelected)
	{
		if (ObjectiveMapListDropDown.value > 0)
		{
			CMap cMap = m_MapOptions[ObjectiveMapListDropDown.value - 1];
			m_CurrentMapGuidList.Add(cMap.MapGuid);
			RefreshMapList(getMapsFromObjective: false);
		}
	}

	public void OnTileSelectedToAddToList(CClientTile tileToAdd)
	{
		if (!m_CurrentTileList.Any((TileIndex t) => t.X == tileToAdd.m_Tile.m_ArrayIndex.X && t.Y == tileToAdd.m_Tile.m_ArrayIndex.Y))
		{
			m_CurrentTileList.Add(new TileIndex(tileToAdd.m_Tile.m_ArrayIndex));
			RefreshTileList(getTilesFromObjective: false);
		}
	}

	public void TileItemSelected(LevelEditorListItemInlineButtons tileItemSelected)
	{
		TileIndex tileIndex = m_CurrentTileList[tileItemSelected.ItemIndex];
		CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[tileIndex.X, tileIndex.Y];
		LevelEditorController.s_Instance.ShowLocationIndicator(cClientTile.m_GameObject.transform.position);
	}

	public void OnTileListItemDeletePressed(LevelEditorListItemInlineButtons itemToDelete)
	{
		m_CurrentTileList.RemoveAt(itemToDelete.ItemIndex);
		RefreshTileList(getTilesFromObjective: false);
	}

	public void MapItemSelected(LevelEditorListItemInlineButtons mapItemSelected)
	{
		string mapGuid = m_CurrentMapGuidList[mapItemSelected.ItemIndex];
		CMap cMap = ScenarioManager.CurrentScenarioState.Maps.SingleOrDefault((CMap x) => x.MapGuid == mapGuid);
		LevelEditorController.s_Instance.ShowLocationIndicator(new Vector3(cMap.Centre.X, cMap.Centre.Y, cMap.Centre.Z));
	}

	public void OnMapListItemDeletePressed(LevelEditorListItemInlineButtons itemToDelete)
	{
		m_CurrentMapGuidList.RemoveAt(itemToDelete.ItemIndex);
		RefreshMapList(getMapsFromObjective: false);
	}
}
