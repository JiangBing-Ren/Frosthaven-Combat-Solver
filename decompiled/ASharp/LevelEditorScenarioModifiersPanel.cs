using System;
using System.Collections.Generic;
using System.Linq;
using MapRuleLibrary.Client;
using MapRuleLibrary.YML.PersonalQuests;
using MapRuleLibrary.YML.Quest;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorScenarioModifiersPanel : MonoBehaviour
{
	public LayoutGroup ScenarioModifierItemListParent;

	public GameObject ScenarioModifierListItemPrefab;

	[Header("Display panel components")]
	public GameObject ModifierDisplayPanel;

	public TextMeshProUGUI ModifierTitle;

	public TMP_InputField ActivationRoundInput;

	public TMP_InputField EventIdInput;

	public TMP_InputField CustomLocField;

	public TMP_InputField CustomTriggerLocField;

	public TMP_Dropdown ModifierTypeDropDown;

	public TMP_Dropdown ModifierTriggerPhaseDropDown;

	public LevelEditorEnumFlagsDropDown ModifierOpenRoomBehaviourFlagDropdown;

	public TMP_Dropdown ModifierActivationTypeDropDown;

	public TMP_Dropdown ModifierActivationIDDropdown;

	public Toggle ApplyToEachActorOnceToggle;

	public TMP_Dropdown ModifierScenarioAbilityIdDropDown;

	public TMP_Dropdown ModifierTriggerAbilityStackingTypeDropDown;

	public TMP_Dropdown ModifierConditionDecTrigger;

	public TMP_Dropdown ModifierAfterAbilityTypeDropdown;

	public TMP_Dropdown ModifierAfterAttackTypeDropdown;

	public Toggle IsHiddenToggle;

	public Toggle StartsDeactivatedToggle;

	public Toggle IsNotNegativeToggle;

	public Toggle OnlyApplyOnceTotalToggle;

	public Toggle CancelAllActiveBonusesOnDeactivationToggle;

	public TextMeshProUGUI ApplyButtonText;

	public Button DeleteButton;

	public TextMeshProUGUI StatusText;

	public TMP_Dropdown ModifierSpawnerGUIDDropdown;

	public TMP_InputField ModifierCheckIntervalInput;

	public TMP_InputField ModifierCheckIntervalOffsetInput;

	public TMP_Dropdown ModifierMapGUIDDropdown;

	[Header("Objective Filter")]
	public LevelEditorObjectiveFilterPanel ObjectiveFilterPanel;

	[Header("Elements to Disable/Enable")]
	public GameObject ModifierScenarioAbilityIdWrapper;

	public GameObject ModifierConditionDecTriggerWrapper;

	public GameObject ModifierAfterAbilityTypeWrapper;

	public GameObject ModifierAfterAttackTypeWrapper;

	public GameObject ModifierTriggerAbilityStackingTypeWrapper;

	public GameObject ModifierActivationIdWrapper;

	public GameObject ModifierTriggerCheckIntervalWrapper;

	public GameObject ModifierTriggerCheckIntervalOffsetWrapper;

	public LevelEditorConditionsComponent ConditionsComponent;

	public LevelEditorElementStatesComponent ElementStatesComponent;

	public LevelEditorAttackModifiersPanel AttackModifiersPanel;

	public LevelEditorTeleportSettingsComponent TeleportSettingsPanel;

	public LevelEditorAbilityPerTileComponent AbilitiesPerTilePanel;

	public LevelEditorGenericTileListPanel InLineSubAbilityTilesPanel;

	public LevelEditorGenericListPanel MapPanel;

	public LevelEditorMoveActorInDirectionSettingsComponent MoveActorInDirectionSettingsPanel;

	public GameObject ModifierDestroyRoomInstaKillActorsContainer;

	public Toggle ModifierDestroyRoomInstaKillActors;

	[Header("Prefabs To Spawn")]
	public GameObject InlineRemovalListItemPrefab;

	private ScenarioState m_CurrentScenarioState;

	private LevelEditorScenarioModifierListItem m_CurrentItem;

	private List<LevelEditorScenarioModifierListItem> m_ScenarioModifierListItems = new List<LevelEditorScenarioModifierListItem>();

	private bool m_ButtonModeAdd;

	private List<string> m_AbilityIds = new List<string>();

	private List<string> m_CurrentMapGUIDS = new List<string>();

	private void Awake()
	{
		ModifierTypeDropDown.options.Clear();
		ModifierTypeDropDown.AddOptions(CScenarioModifier.ScenarioModifierTypes.Select((EScenarioModifierType s) => s.ToString()).ToList());
		ModifierTypeDropDown.onValueChanged.AddListener(OnScenarioModifierTypeDropDownChanged);
		ModifierTriggerPhaseDropDown.options.Clear();
		ModifierTriggerPhaseDropDown.AddOptions(CScenarioModifier.ScenarioModifierTriggerPhases.Select((EScenarioModifierTriggerPhase s) => s.ToString()).ToList());
		ModifierTriggerPhaseDropDown.onValueChanged.AddListener(OnScenarioModifierTriggerPhaseDropDownChanged);
		ModifierOpenRoomBehaviourFlagDropdown.SetEnumFlagType<EScenarioModifierRoomOpenBehaviour>();
		ModifierActivationTypeDropDown.options.Clear();
		ModifierActivationTypeDropDown.AddOptions(CScenarioModifier.ScenarioModifierActivationTypes.Select((EScenarioModifierActivationType s) => s.ToString()).ToList());
		ModifierActivationTypeDropDown.onValueChanged.AddListener(OnScenarioModifierActivationTypeDropDownChanged);
		ModifierActivationIdWrapper.SetActive(value: false);
		ModifierActivationIDDropdown.options.Clear();
		ApplyToEachActorOnceToggle.SetIsOnWithoutNotify(value: false);
		IsHiddenToggle.SetIsOnWithoutNotify(value: false);
		StartsDeactivatedToggle.SetIsOnWithoutNotify(value: false);
		IsNotNegativeToggle.SetIsOnWithoutNotify(value: false);
		OnlyApplyOnceTotalToggle.SetIsOnWithoutNotify(value: false);
		CancelAllActiveBonusesOnDeactivationToggle.SetIsOnWithoutNotify(value: false);
		ModifierConditionDecTrigger.options.Clear();
		ModifierConditionDecTrigger.AddOptions(CAbilityCondition.ConditionDecTriggers.Select((EConditionDecTrigger s) => s.ToString()).ToList());
		m_AbilityIds.Add("NONE");
		List<string> list = ScenarioRuleClient.SRLYML.ScenarioAbilities.Select((ScenarioAbilitiesYMLData a) => a.ScenarioAbilityID).ToList();
		list.Sort();
		list.Remove("CardsScript");
		m_AbilityIds.AddRange(list);
		ModifierScenarioAbilityIdDropDown.options.Clear();
		ModifierScenarioAbilityIdDropDown.AddOptions(m_AbilityIds);
		ModifierTriggerAbilityStackingTypeDropDown.options.Clear();
		ModifierTriggerAbilityStackingTypeDropDown.AddOptions(CScenarioModifierTriggerAbility.TriggerAbilityStackingTypes.Select((CScenarioModifierTriggerAbility.ETriggerAbilityStackingType s) => s.ToString()).ToList());
		ModifierAfterAbilityTypeDropdown.options.Clear();
		ModifierAfterAbilityTypeDropdown.AddOptions(CAbility.AbilityTypes.Select((CAbility.EAbilityType s) => s.ToString()).ToList());
		ModifierAfterAbilityTypeDropdown.onValueChanged.AddListener(OnScenarioModifierAfterAbilityTypeDropDownChanged);
		ModifierAfterAttackTypeDropdown.options.Clear();
		ModifierAfterAttackTypeDropdown.AddOptions(CAbility.AttackTypes.Select((CAbility.EAttackType s) => s.ToString()).ToList());
		ModifierSpawnerGUIDDropdown.options.Clear();
		ModifierSpawnerGUIDDropdown.AddOptions(ScenarioManager.CurrentScenarioState.Spawners.Select((CSpawner s) => s.SpawnerGuid).ToList());
		ModifierMapGUIDDropdown.options.Clear();
		ModifierMapGUIDDropdown.AddOptions(new List<string> { "NONE" });
		ModifierMapGUIDDropdown.AddOptions(ScenarioManager.CurrentScenarioState.Maps.Select((CMap s) => s.MapGuid).ToList());
		ModifierDestroyRoomInstaKillActorsContainer.gameObject.SetActive(value: false);
		ModifierDestroyRoomInstaKillActors.SetIsOnWithoutNotify(value: false);
		MapPanel.SetupDelegateActions(OnButtonAddMapToListPressed, OnButtonMapListItemDeletePressed);
		ModifierDisplayPanel.SetActive(value: false);
	}

	private void OnEnable()
	{
		RefreshUIFromCurrentScenarioState(ScenarioManager.CurrentScenarioState);
	}

	private LevelEditorScenarioModifierListItem AddNewListItemForScenarioModifier(int atIndex, CScenarioModifier modifierToAddFor)
	{
		GameObject gameObject = null;
		gameObject = UnityEngine.Object.Instantiate(ScenarioModifierListItemPrefab, ScenarioModifierItemListParent.transform);
		LevelEditorScenarioModifierListItem levelEditorScenarioModifierListItem = null;
		if (gameObject != null)
		{
			levelEditorScenarioModifierListItem = gameObject.GetComponent<LevelEditorScenarioModifierListItem>();
			levelEditorScenarioModifierListItem.InitForScenarioModifier(modifierToAddFor, atIndex);
			levelEditorScenarioModifierListItem.ButtonPressedAction = EditModifier;
			m_ScenarioModifierListItems.Add(levelEditorScenarioModifierListItem);
		}
		else
		{
			Log.Error("Modifier list gameobject could not be created");
		}
		return levelEditorScenarioModifierListItem;
	}

	public void EditModifier(LevelEditorScenarioModifierListItem itemEdited)
	{
		ModifierDisplayPanel.SetActive(value: true);
		if ((bool)m_CurrentItem)
		{
			m_CurrentItem.SetSelectedState(selected: false);
		}
		m_CurrentItem = itemEdited;
		m_CurrentItem.SetSelectedState(selected: true);
		m_ButtonModeAdd = false;
		StatusText.text = "-";
		ApplyButtonText.text = "APPLY";
		DeleteButton.gameObject.SetActive(value: true);
		ModifierTitle.text = $"Modifier #{m_CurrentItem.modifierIndex} - {m_CurrentItem.ModifierEditorDisplayName}";
		ActivationRoundInput.text = m_CurrentItem.Modifier.ActivationRound.ToString();
		ModifierTypeDropDown.value = (int)m_CurrentItem.Modifier.ScenarioModifierType;
		ModifierTriggerPhaseDropDown.value = (int)m_CurrentItem.Modifier.ScenarioModifierTriggerPhase;
		ModifierOpenRoomBehaviourFlagDropdown.SetCurrentValue<EScenarioModifierRoomOpenBehaviour>(m_CurrentItem.Modifier.RoomOpenBehaviour.ToString());
		ModifierActivationTypeDropDown.value = (int)m_CurrentItem.Modifier.ScenarioModifierActivationType;
		OnScenarioModifierActivationTypeDropDownChanged(ModifierActivationTypeDropDown.value);
		bool flag = false;
		for (int i = 0; i < ModifierActivationIDDropdown.options.Count; i++)
		{
			if (ModifierActivationIDDropdown.options[i].text == m_CurrentItem.Modifier.ScenarioModifierActivationID)
			{
				ModifierActivationIDDropdown.value = i;
				flag = true;
				break;
			}
		}
		ModifierActivationIDDropdown.value = (flag ? ModifierActivationIDDropdown.value : 0);
		ApplyToEachActorOnceToggle.SetIsOnWithoutNotify(m_CurrentItem.Modifier.ApplyToEachActorOnce);
		IsHiddenToggle.SetIsOnWithoutNotify(m_CurrentItem.Modifier.IsHidden);
		StartsDeactivatedToggle.SetIsOnWithoutNotify(m_CurrentItem.Modifier.Deactivated);
		IsNotNegativeToggle.SetIsOnWithoutNotify(m_CurrentItem.Modifier.IsPositive);
		OnlyApplyOnceTotalToggle.SetIsOnWithoutNotify(m_CurrentItem.Modifier.ApplyOnceTotal);
		CancelAllActiveBonusesOnDeactivationToggle.SetIsOnWithoutNotify(m_CurrentItem.Modifier.CancelAllActiveBonusesOnDeactivation);
		bool flag2 = false;
		for (int j = 0; j < ModifierScenarioAbilityIdDropDown.options.Count; j++)
		{
			if (ModifierScenarioAbilityIdDropDown.options[j].text == m_CurrentItem.Modifier.ScenarioAbilityID)
			{
				ModifierScenarioAbilityIdDropDown.value = j;
				flag2 = true;
				break;
			}
		}
		ModifierScenarioAbilityIdDropDown.value = (flag2 ? ModifierScenarioAbilityIdDropDown.value : 0);
		ModifierAfterAbilityTypeDropdown.value = (int)((m_CurrentItem.Modifier.AfterAbilityTypes != null) ? m_CurrentItem.Modifier.AfterAbilityTypes[0] : CAbility.EAbilityType.None);
		ModifierAfterAttackTypeDropdown.value = (int)((m_CurrentItem.Modifier.AfterAttackTypes != null) ? m_CurrentItem.Modifier.AfterAttackTypes[0] : CAbility.EAttackType.None);
		ObjectiveFilterPanel.SetShowing(m_CurrentItem.Modifier.ScenarioModifierFilter);
		CustomLocField.text = m_CurrentItem.Modifier.CustomLocKey;
		CustomTriggerLocField.text = m_CurrentItem.Modifier.CustomTriggerLocKey;
		EventIdInput.text = m_CurrentItem.Modifier.EventIdentifier;
		m_CurrentMapGUIDS = ((m_CurrentItem.Modifier.TriggerAndActivationMapGUIDs != null) ? m_CurrentItem.Modifier.TriggerAndActivationMapGUIDs.ToList() : new List<string>());
		MapPanel.SetupItemsAvailableToAdd(ScenarioManager.CurrentScenarioState.Maps.Select((CMap x) => x.MapGuid).ToList());
		MapPanel.RefreshUIWithItems(m_CurrentMapGUIDS.ToList());
		if (m_CurrentItem.Modifier is CScenarioModifierAddConditionsToAbilities cScenarioModifierAddConditionsToAbilities)
		{
			ConditionsComponent.SetConditions(cScenarioModifierAddConditionsToAbilities.PositiveConditions, cScenarioModifierAddConditionsToAbilities.NegativeConditions);
		}
		else if (m_CurrentItem.Modifier is CScenarioModifierApplyConditionToActor cScenarioModifierApplyConditionToActor)
		{
			ConditionsComponent.SetConditions(cScenarioModifierApplyConditionToActor.PositiveConditions, cScenarioModifierApplyConditionToActor.NegativeConditions);
			ModifierConditionDecTrigger.value = (int)cScenarioModifierApplyConditionToActor.ConditionDecrementTrigger;
		}
		else if (m_CurrentItem.Modifier is CScenarioModifierSetElements cScenarioModifierSetElements)
		{
			ModifierCheckIntervalInput.text = cScenarioModifierSetElements.CheckRoundInterval.ToString();
			ElementStatesComponent.SetStates(cScenarioModifierSetElements.StrongElements, cScenarioModifierSetElements.WaningElements, cScenarioModifierSetElements.InertElements);
		}
		else if (m_CurrentItem.Modifier is CScenarioModifierTriggerAbility cScenarioModifierTriggerAbility)
		{
			InLineSubAbilityTilesPanel.SetTileList(cScenarioModifierTriggerAbility.InlineSubAbilityTileIndexes);
			bool flag3 = false;
			for (int num = 0; num < ModifierTriggerAbilityStackingTypeDropDown.options.Count; num++)
			{
				if (ModifierTriggerAbilityStackingTypeDropDown.options[num].text == cScenarioModifierTriggerAbility.TriggerAbilityStackingType.ToString())
				{
					ModifierTriggerAbilityStackingTypeDropDown.value = num;
					flag3 = true;
					break;
				}
			}
			ModifierTriggerAbilityStackingTypeDropDown.value = (flag3 ? ModifierTriggerAbilityStackingTypeDropDown.value : 0);
		}
		else if (m_CurrentItem.Modifier is CScenarioModifierForceSpawnerToSpawnIfActorsNotInRooms cScenarioModifierForceSpawnerToSpawnIfActorsNotInRooms)
		{
			ModifierCheckIntervalInput.text = cScenarioModifierForceSpawnerToSpawnIfActorsNotInRooms.CheckRoundInterval.ToString();
			ModifierSpawnerGUIDDropdown.options.Clear();
			ModifierSpawnerGUIDDropdown.AddOptions(ScenarioManager.CurrentScenarioState.Spawners.Select((CSpawner s) => s.SpawnerGuid).ToList());
			bool flag4 = false;
			for (int num2 = 0; num2 < ModifierSpawnerGUIDDropdown.options.Count; num2++)
			{
				if (ModifierSpawnerGUIDDropdown.options[num2].text == cScenarioModifierForceSpawnerToSpawnIfActorsNotInRooms.SpawnerGUID)
				{
					ModifierSpawnerGUIDDropdown.value = num2;
					flag4 = true;
					break;
				}
			}
			ModifierSpawnerGUIDDropdown.value = (flag4 ? ModifierSpawnerGUIDDropdown.value : 0);
		}
		else if (m_CurrentItem.Modifier is CScenarioModifierMovePropsInSequence cScenarioModifierMovePropsInSequence)
		{
			InLineSubAbilityTilesPanel.SetTileList(cScenarioModifierMovePropsInSequence.SequenceTiles.ToList());
		}
		else if (m_CurrentItem.Modifier is CScenarioModifierDestroyRoom cScenarioModifierDestroyRoom)
		{
			ModifierDestroyRoomInstaKillActors.SetIsOnWithoutNotify(cScenarioModifierDestroyRoom.InstaKillActors);
			ModifierMapGUIDDropdown.options.Clear();
			ModifierMapGUIDDropdown.AddOptions(new List<string> { "NONE" });
			ModifierMapGUIDDropdown.AddOptions(ScenarioManager.CurrentScenarioState.Maps.Select((CMap s) => s.MapGuid).ToList());
			bool flag5 = false;
			for (int num3 = 0; num3 < ModifierMapGUIDDropdown.options.Count; num3++)
			{
				if (ModifierMapGUIDDropdown.options[num3].text == cScenarioModifierDestroyRoom.MapGuid)
				{
					ModifierMapGUIDDropdown.value = num3;
					flag5 = true;
					break;
				}
			}
			ModifierMapGUIDDropdown.value = (flag5 ? ModifierMapGUIDDropdown.value : 0);
		}
		TeleportSettingsPanel.SetScenarioModifier(m_CurrentItem.Modifier);
		MoveActorInDirectionSettingsPanel.SetScenarioModifier(m_CurrentItem.Modifier);
		AbilitiesPerTilePanel.SetScenarioModifier(m_CurrentItem.Modifier);
		ExposeParametersBasedOnScenarioModifierType(m_CurrentItem.Modifier.ScenarioModifierType);
		ExposeParametersBasedOnTriggerType(m_CurrentItem.Modifier.ScenarioModifierTriggerPhase, m_CurrentItem.Modifier.ScenarioModifierType);
		ExposeParametersBasedOnAbilityType((m_CurrentItem.Modifier.AfterAbilityTypes != null) ? m_CurrentItem.Modifier.AfterAbilityTypes[0] : CAbility.EAbilityType.None);
		ModifierActivationIdWrapper.SetActive(m_CurrentItem.Modifier.ScenarioModifierActivationType != EScenarioModifierActivationType.None);
	}

	private void ExposeParametersBasedOnScenarioModifierType(EScenarioModifierType typeToExposeFor)
	{
		ModifierScenarioAbilityIdWrapper.SetActive(value: false);
		ModifierConditionDecTriggerWrapper.SetActive(value: false);
		ConditionsComponent.gameObject.SetActive(value: false);
		ElementStatesComponent.gameObject.SetActive(value: false);
		AttackModifiersPanel.gameObject.SetActive(value: false);
		TeleportSettingsPanel.gameObject.SetActive(value: false);
		AbilitiesPerTilePanel.gameObject.SetActive(value: false);
		MoveActorInDirectionSettingsPanel.gameObject.SetActive(value: false);
		InLineSubAbilityTilesPanel.gameObject.SetActive(value: false);
		ModifierTriggerAbilityStackingTypeWrapper.gameObject.SetActive(value: false);
		ModifierTriggerCheckIntervalWrapper.gameObject.SetActive(value: false);
		ModifierTriggerCheckIntervalOffsetWrapper.gameObject.SetActive(value: false);
		ModifierDestroyRoomInstaKillActorsContainer.gameObject.SetActive(value: false);
		MapPanel.gameObject.SetActive(value: true);
		MapPanel.SetupItemsAvailableToAdd(ScenarioManager.CurrentScenarioState.Maps.Select((CMap x) => x.MapGuid).ToList());
		MapPanel.RefreshUIWithItems(m_CurrentMapGUIDS.ToList());
		switch (typeToExposeFor)
		{
		case EScenarioModifierType.DestroyRoom:
			ModifierDestroyRoomInstaKillActorsContainer.gameObject.SetActive(value: true);
			break;
		case EScenarioModifierType.SetElements:
			ModifierTriggerCheckIntervalWrapper.gameObject.SetActive(value: true);
			ModifierTriggerCheckIntervalOffsetWrapper.gameObject.SetActive(value: true);
			ElementStatesComponent.gameObject.SetActive(value: true);
			break;
		case EScenarioModifierType.TriggerAbility:
			ModifierScenarioAbilityIdWrapper.gameObject.SetActive(value: true);
			AbilitiesPerTilePanel.gameObject.SetActive(value: true);
			InLineSubAbilityTilesPanel.gameObject.SetActive(value: true);
			ModifierTriggerAbilityStackingTypeWrapper.gameObject.SetActive(value: true);
			break;
		case EScenarioModifierType.AddConditionsToAbilities:
			ConditionsComponent.gameObject.SetActive(value: true);
			break;
		case EScenarioModifierType.ApplyConditionToActor:
			ConditionsComponent.gameObject.SetActive(value: true);
			ModifierConditionDecTriggerWrapper.SetActive(value: true);
			break;
		case EScenarioModifierType.ApplyActiveBonusToActor:
			ModifierScenarioAbilityIdWrapper.gameObject.SetActive(value: true);
			AbilitiesPerTilePanel.gameObject.SetActive(value: true);
			break;
		case EScenarioModifierType.AddModifierCards:
		{
			AttackModifiersPanel.gameObject.SetActive(value: true);
			CScenarioModifierAddModifierCards cScenarioModifierAddModifierCards = ((m_CurrentItem != null) ? ((CScenarioModifierAddModifierCards)m_CurrentItem.Modifier) : null);
			AttackModifiersPanel.DisplayAttackModifiersForScenarioModifier((cScenarioModifierAddModifierCards != null) ? cScenarioModifierAddModifierCards.ModifierCardNames : new List<string>());
			break;
		}
		case EScenarioModifierType.PhaseInAndTeleport:
		case EScenarioModifierType.Teleport:
		case EScenarioModifierType.OverrideCompanionSummonTiles:
			TeleportSettingsPanel.gameObject.SetActive(value: true);
			TeleportSettingsPanel.UpdateTileIndexPanel();
			break;
		case EScenarioModifierType.ActivateClosestAI:
			ModifierScenarioAbilityIdWrapper.gameObject.SetActive(value: true);
			AbilitiesPerTilePanel.gameObject.SetActive(value: true);
			break;
		case EScenarioModifierType.MoveActorsInDirection:
			MoveActorInDirectionSettingsPanel.gameObject.SetActive(value: true);
			MoveActorInDirectionSettingsPanel.UpdateDirectionsPanel();
			break;
		case EScenarioModifierType.MovePropsInSequence:
			InLineSubAbilityTilesPanel.gameObject.SetActive(value: true);
			break;
		case EScenarioModifierType.ForceSpawnerToSpawnIfActorsNotInRooms:
			ModifierTriggerCheckIntervalWrapper.gameObject.SetActive(value: true);
			ModifierTriggerCheckIntervalOffsetWrapper.gameObject.SetActive(value: true);
			break;
		default:
			Log.Error($"UI for Type {typeToExposeFor} not implemented yet");
			break;
		case EScenarioModifierType.None:
		case EScenarioModifierType.PhaseOut:
		case EScenarioModifierType.ToggleActorDeactivated:
		case EScenarioModifierType.MovePropsToNearestPlayer:
		case EScenarioModifierType.ActorsCreateGraves:
		case EScenarioModifierType.MakeTrapsObstacles:
		case EScenarioModifierType.ActorsDontDropCoins:
		case EScenarioModifierType.DisableRest:
			break;
		}
	}

	private void ExposeParametersBasedOnTriggerType(EScenarioModifierTriggerPhase triggerType, EScenarioModifierType typeToExposeFor)
	{
		ModifierAfterAbilityTypeWrapper.SetActive(value: false);
		switch (triggerType)
		{
		case EScenarioModifierTriggerPhase.None:
			if (typeToExposeFor == EScenarioModifierType.AddConditionsToAbilities)
			{
				ModifierAfterAbilityTypeWrapper.SetActive(value: true);
			}
			break;
		case EScenarioModifierTriggerPhase.AfterAbility:
			ModifierAfterAbilityTypeWrapper.SetActive(value: true);
			break;
		}
	}

	private void ExposeParametersBasedOnAbilityType(CAbility.EAbilityType abilityType)
	{
		ModifierAfterAttackTypeWrapper.SetActive(value: false);
		if (abilityType == CAbility.EAbilityType.Attack)
		{
			ModifierAfterAttackTypeWrapper.SetActive(value: true);
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
		foreach (LevelEditorScenarioModifierListItem scenarioModifierListItem in m_ScenarioModifierListItems)
		{
			UnityEngine.Object.Destroy(scenarioModifierListItem.gameObject);
		}
		m_ScenarioModifierListItems.Clear();
		for (int i = 0; i < m_CurrentScenarioState.ScenarioModifiers.Count; i++)
		{
			AddNewListItemForScenarioModifier(i, m_CurrentScenarioState.ScenarioModifiers[i]);
		}
		TeleportSettingsPanel.UpdateTileIndexPanel();
	}

	public bool ValidateCurrentUIForMessage(out string completionMessage)
	{
		EScenarioModifierType value = (EScenarioModifierType)ModifierTypeDropDown.value;
		if (value == EScenarioModifierType.None)
		{
			completionMessage = "ScenarioModifier type must not be None";
			return false;
		}
		int result = 0;
		if (!int.TryParse(ActivationRoundInput.text, out result))
		{
			completionMessage = "ActivationRound needs to be a whole number";
			return false;
		}
		if (value == EScenarioModifierType.ActivateClosestAI && (AbilitiesPerTilePanel.TileIndexes == null || AbilitiesPerTilePanel.TileIndexes.Count == 0))
		{
			completionMessage = "Need at least one tile to check";
			return false;
		}
		completionMessage = "Validation succeeded";
		return true;
	}

	public void OnButtonAdd()
	{
		if ((bool)m_CurrentItem)
		{
			m_CurrentItem.SetSelectedState(selected: false);
		}
		m_CurrentItem = null;
		m_ButtonModeAdd = true;
		ApplyButtonText.text = "ADD NEW";
		DeleteButton.gameObject.SetActive(value: false);
		ModifierTitle.text = "MODIFIER TO ADD";
		ActivationRoundInput.text = "0";
		ModifierTypeDropDown.value = 0;
		ModifierCheckIntervalInput.text = "0";
		CustomLocField.text = string.Empty;
		EventIdInput.text = string.Empty;
		ModifierActivationTypeDropDown.value = 0;
		ModifierActivationIdWrapper.SetActive(value: false);
		ModifierActivationIDDropdown.value = 0;
		ModifierScenarioAbilityIdDropDown.value = 0;
		ModifierConditionDecTrigger.value = 0;
		ModifierSpawnerGUIDDropdown.value = 0;
		ModifierMapGUIDDropdown.value = 0;
		ApplyToEachActorOnceToggle.SetIsOnWithoutNotify(value: false);
		IsHiddenToggle.SetIsOnWithoutNotify(value: false);
		StartsDeactivatedToggle.SetIsOnWithoutNotify(value: false);
		IsNotNegativeToggle.SetIsOnWithoutNotify(value: false);
		OnlyApplyOnceTotalToggle.SetIsOnWithoutNotify(value: false);
		CancelAllActiveBonusesOnDeactivationToggle.SetIsOnWithoutNotify(value: false);
		ObjectiveFilterPanel.SetShowing(new CObjectiveFilter());
		ExposeParametersBasedOnScenarioModifierType((EScenarioModifierType)ModifierTypeDropDown.value);
		ExposeParametersBasedOnTriggerType((EScenarioModifierTriggerPhase)ModifierTriggerPhaseDropDown.value, (EScenarioModifierType)ModifierTypeDropDown.value);
		ExposeParametersBasedOnAbilityType((CAbility.EAbilityType)ModifierAfterAbilityTypeDropdown.value);
		ModifierDisplayPanel.SetActive(value: true);
		ConditionsComponent.Clear();
		ElementStatesComponent.Clear();
		TeleportSettingsPanel.SetScenarioModifier(null);
		TeleportSettingsPanel.UpdateTileIndexPanel();
		AbilitiesPerTilePanel.SetScenarioModifier(null);
		MoveActorInDirectionSettingsPanel.SetScenarioModifier(null);
		MoveActorInDirectionSettingsPanel.UpdateDirectionsPanel();
	}

	public void OnButtonApplyPressed()
	{
		if (ValidateCurrentUIForMessage(out var completionMessage))
		{
			EScenarioModifierType value = (EScenarioModifierType)ModifierTypeDropDown.value;
			int num = 0;
			int checkRoundInterval = 0;
			int checkRoundIntervalOffset = 0;
			try
			{
				num = int.Parse(ActivationRoundInput.text);
				checkRoundInterval = int.Parse(ModifierCheckIntervalInput.text);
				checkRoundIntervalOffset = int.Parse(ModifierCheckIntervalOffsetInput.text);
			}
			catch
			{
			}
			ObjectiveFilterPanel.Apply();
			CScenarioModifier cScenarioModifier = null;
			int count = ScenarioManager.CurrentScenarioState.ScenarioModifiers.Count;
			string text = $"SM {count}";
			EScenarioModifierTriggerPhase value2 = (EScenarioModifierTriggerPhase)ModifierTriggerPhaseDropDown.value;
			EScenarioModifierRoomOpenBehaviour currentFlagEnum = ModifierOpenRoomBehaviourFlagDropdown.GetCurrentFlagEnum<EScenarioModifierRoomOpenBehaviour>();
			EScenarioModifierActivationType value3 = (EScenarioModifierActivationType)ModifierActivationTypeDropDown.value;
			string scenarioModifierActivationID = ((ModifierActivationIDDropdown.options.Count > 0) ? ModifierActivationIDDropdown.options[ModifierActivationIDDropdown.value].text : string.Empty);
			CAbility.EAbilityType value4 = (CAbility.EAbilityType)ModifierAfterAbilityTypeDropdown.value;
			CAbility.EAttackType value5 = (CAbility.EAttackType)ModifierAfterAttackTypeDropdown.value;
			string spawnerGUID = ((ModifierSpawnerGUIDDropdown.options.Count > 0) ? ModifierSpawnerGUIDDropdown.options[ModifierSpawnerGUIDDropdown.value].text : string.Empty);
			string mapGUID = ((ModifierMapGUIDDropdown.options.Count > 0 && ModifierMapGUIDDropdown.value > 0) ? ModifierMapGUIDDropdown.options[ModifierMapGUIDDropdown.value].text : string.Empty);
			bool isOn = ApplyToEachActorOnceToggle.isOn;
			bool isOn2 = OnlyApplyOnceTotalToggle.isOn;
			bool isOn3 = IsHiddenToggle.isOn;
			bool isOn4 = StartsDeactivatedToggle.isOn;
			bool isOn5 = IsNotNegativeToggle.isOn;
			bool isOn6 = CancelAllActiveBonusesOnDeactivationToggle.isOn;
			string text2 = CustomLocField.text;
			string text3 = CustomTriggerLocField.text;
			string text4 = EventIdInput.text;
			switch (value)
			{
			case EScenarioModifierType.SetElements:
			{
				ElementStatesComponent.GetStates(out var strongElements, out var waningElements, out var inertElements);
				cScenarioModifier = new CScenarioModifierSetElements(text, count, num, value2, isOn, ObjectiveFilterPanel.FilterBeingShown, isOn5, strongElements, waningElements, inertElements, checkRoundInterval, checkRoundIntervalOffset, value3, scenarioModifierActivationID, text2, text3, text4, new List<CAbility.EAbilityType> { value4 }, new List<CAbility.EAttackType> { value5 }, isOn3, isOn4, isOn2, isOn6, currentFlagEnum, m_CurrentMapGUIDS.ToList());
				break;
			}
			case EScenarioModifierType.TriggerAbility:
			{
				CScenarioModifierTriggerAbility.ETriggerAbilityStackingType value6 = (CScenarioModifierTriggerAbility.ETriggerAbilityStackingType)ModifierTriggerAbilityStackingTypeDropDown.value;
				int activationRound = num;
				bool applyToEachActorOnce = isOn;
				CObjectiveFilter filterBeingShown = ObjectiveFilterPanel.FilterBeingShown;
				bool isPositive = isOn5;
				string scenarioAbilityID = m_AbilityIds[ModifierScenarioAbilityIdDropDown.value];
				Dictionary<TileIndex, List<string>> abilityIdByLocation = AbilitiesPerTilePanel.AbilityIdByLocation;
				List<TileIndex> tileIndexes = InLineSubAbilityTilesPanel.TileIndexes;
				string customLocKey = text2;
				string customTriggerLocKey = text3;
				string eventId = text4;
				List<CAbility.EAbilityType> afterAbilityTypes = new List<CAbility.EAbilityType> { value4 };
				List<CAbility.EAttackType> afterAttackTypes = new List<CAbility.EAttackType> { value5 };
				bool isHidden = isOn3;
				bool isDeactivated = isOn4;
				bool applyOnceTotal = isOn2;
				bool cancelAllActiveBonusesOnDeactivation = isOn6;
				EScenarioModifierRoomOpenBehaviour roomOpenBehaviour = currentFlagEnum;
				List<string> roomMapGuids = m_CurrentMapGUIDS.ToList();
				cScenarioModifier = new CScenarioModifierTriggerAbility(text, count, activationRound, value2, applyToEachActorOnce, filterBeingShown, isPositive, scenarioAbilityID, value6, abilityIdByLocation, tileIndexes, value3, scenarioModifierActivationID, null, customLocKey, customTriggerLocKey, eventId, afterAbilityTypes, afterAttackTypes, isHidden, isDeactivated, applyOnceTotal, cancelAllActiveBonusesOnDeactivation, roomOpenBehaviour, roomMapGuids);
				break;
			}
			case EScenarioModifierType.AddConditionsToAbilities:
			{
				ConditionsComponent.GetConditions(out var positiveConditions, out var negativeConditions);
				cScenarioModifier = new CScenarioModifierAddConditionsToAbilities(text, count, num, value2, isOn, ObjectiveFilterPanel.FilterBeingShown, isOn5, positiveConditions, negativeConditions, value3, scenarioModifierActivationID, text2, text3, text4, new List<CAbility.EAbilityType> { value4 }, new List<CAbility.EAttackType> { value5 }, isOn3, isOn4, isOn2, isOn6, currentFlagEnum, m_CurrentMapGUIDS.ToList());
				break;
			}
			case EScenarioModifierType.ApplyConditionToActor:
			{
				ConditionsComponent.GetConditions(out var positiveConditions2, out var negativeConditions2);
				cScenarioModifier = new CScenarioModifierApplyConditionToActor(text, count, num, value2, isOn, ObjectiveFilterPanel.FilterBeingShown, isOn5, positiveConditions2, negativeConditions2, value3, scenarioModifierActivationID, (EConditionDecTrigger)ModifierConditionDecTrigger.value, text2, text3, text4, new List<CAbility.EAbilityType> { value4 }, new List<CAbility.EAttackType> { value5 }, isOn3, isOn4, isOn2, isOn6, currentFlagEnum, m_CurrentMapGUIDS.ToList());
				break;
			}
			case EScenarioModifierType.ApplyActiveBonusToActor:
				cScenarioModifier = new CScenarioModifierApplyActiveBonusToActor(text, count, num, value2, isOn, ObjectiveFilterPanel.FilterBeingShown, isOn5, m_AbilityIds[ModifierScenarioAbilityIdDropDown.value], AbilitiesPerTilePanel.AbilityIdByLocation, value3, scenarioModifierActivationID, text2, text3, text4, new List<CAbility.EAbilityType> { value4 }, new List<CAbility.EAttackType> { value5 }, isOn3, isOn4, isOn2, isOn6, currentFlagEnum, m_CurrentMapGUIDS.ToList());
				break;
			case EScenarioModifierType.AddModifierCards:
				cScenarioModifier = new CScenarioModifierAddModifierCards(text, count, num, value2, isOn, ObjectiveFilterPanel.FilterBeingShown, isOn5, AttackModifiersPanel.ScenarioModifierAttackModifierCardNames, value3, scenarioModifierActivationID, text2, text3, text4, new List<CAbility.EAbilityType> { value4 }, new List<CAbility.EAttackType> { value5 }, isOn3, isOn4, isOn2, isOn6, currentFlagEnum, m_CurrentMapGUIDS.ToList());
				break;
			case EScenarioModifierType.PhaseOut:
				cScenarioModifier = new CScenarioModifierPhaseOut(text, count, num, value2, isOn, ObjectiveFilterPanel.FilterBeingShown, isOn5, value3, scenarioModifierActivationID, text2, text3, text4, new List<CAbility.EAbilityType> { value4 }, new List<CAbility.EAttackType> { value5 }, isOn3, isOn4, isOn2, isOn6, currentFlagEnum, m_CurrentMapGUIDS.ToList());
				break;
			case EScenarioModifierType.PhaseInAndTeleport:
				cScenarioModifier = new CScenarioModifierPhaseInAndTeleport(text, count, num, value2, isOn, ObjectiveFilterPanel.FilterBeingShown, isOn5, TeleportSettingsPanel.TileIndexes.ToList(), value3, scenarioModifierActivationID, text2, text3, text4, new List<CAbility.EAbilityType> { value4 }, new List<CAbility.EAttackType> { value5 }, isOn3, isOn4, isOn2, isOn6, currentFlagEnum, m_CurrentMapGUIDS.ToList());
				break;
			case EScenarioModifierType.Teleport:
				cScenarioModifier = new CScenarioModifierTeleport(text, count, num, value2, isOn, ObjectiveFilterPanel.FilterBeingShown, isOn5, TeleportSettingsPanel.TileIndexes.ToList(), TeleportSettingsPanel.TeleportTakeTilePriority.isOn, TeleportSettingsPanel.OpenDoorsToTeleportedLocation.isOn, value3, scenarioModifierActivationID, text2, text3, text4, new List<CAbility.EAbilityType> { value4 }, new List<CAbility.EAttackType> { value5 }, isOn3, isOn4, isOn2, isOn6, currentFlagEnum, m_CurrentMapGUIDS.ToList());
				break;
			case EScenarioModifierType.ActivateClosestAI:
				cScenarioModifier = new CScenarioModifierActivateClosestAI(text, count, num, value2, isOn, ObjectiveFilterPanel.FilterBeingShown, isOn5, m_AbilityIds[ModifierScenarioAbilityIdDropDown.value], AbilitiesPerTilePanel.AbilityIdByLocation, value3, scenarioModifierActivationID, text2, text3, text4, new List<CAbility.EAbilityType> { value4 }, new List<CAbility.EAttackType> { value5 }, isOn3, isOn4, isOn2, isOn6, currentFlagEnum, m_CurrentMapGUIDS.ToList());
				break;
			case EScenarioModifierType.ToggleActorDeactivated:
				cScenarioModifier = new CScenarioModifierToggleActorDeactivated(text, count, num, value2, isOn, ObjectiveFilterPanel.FilterBeingShown, isOn5, value3, scenarioModifierActivationID, text2, text3, text4, new List<CAbility.EAbilityType> { value4 }, new List<CAbility.EAttackType> { value5 }, isOn3, isOn4, isOn2, isOn6, currentFlagEnum, m_CurrentMapGUIDS.ToList());
				break;
			case EScenarioModifierType.MoveActorsInDirection:
				cScenarioModifier = new CScenarioModifierMoveActorsInDirections(text, count, num, value2, isOn, ObjectiveFilterPanel.FilterBeingShown, isOn5, MoveActorInDirectionSettingsPanel.Directions.ToList(), MoveActorInDirectionSettingsPanel.ShouldTakeDamageIfMovementBlockedToggle.isOn, MoveActorInDirectionSettingsPanel.ShouldTakeDamageIfMovementBlockedToggle.isOn ? int.Parse(MoveActorInDirectionSettingsPanel.DamageToTakeInputField.text) : 0, MoveActorInDirectionSettingsPanel.PreventMovementIfBehindObstacleToggle.isOn, mapGUID, value3, scenarioModifierActivationID, text2, text3, text4, new List<CAbility.EAbilityType> { value4 }, new List<CAbility.EAttackType> { value5 }, isOn3, isOn4, isOn2, isOn6, currentFlagEnum, m_CurrentMapGUIDS.ToList());
				break;
			case EScenarioModifierType.ForceSpawnerToSpawnIfActorsNotInRooms:
				cScenarioModifier = new CScenarioModifierForceSpawnerToSpawnIfActorsNotInRooms(text, count, num, value2, isOn, ObjectiveFilterPanel.FilterBeingShown, isOn5, spawnerGUID, checkRoundInterval, checkRoundIntervalOffset, value3, scenarioModifierActivationID, text2, text3, text4, new List<CAbility.EAbilityType> { value4 }, new List<CAbility.EAttackType> { value5 }, isOn3, isOn4, isOn2, isOn6, currentFlagEnum, m_CurrentMapGUIDS.ToList());
				break;
			case EScenarioModifierType.MovePropsInSequence:
				cScenarioModifier = new CScenarioModifierMovePropsInSequence(text, count, num, value2, isOn, ObjectiveFilterPanel.FilterBeingShown, isOn5, InLineSubAbilityTilesPanel.TileIndexes.ToList(), value3, scenarioModifierActivationID, text2, text3, text4, new List<CAbility.EAbilityType> { value4 }, new List<CAbility.EAttackType> { value5 }, isOn3, isOn4, isOn2, isOn6, currentFlagEnum, m_CurrentMapGUIDS.ToList());
				break;
			case EScenarioModifierType.MovePropsToNearestPlayer:
				cScenarioModifier = new CScenarioModifierMovePropsToNearestPlayer(text, count, num, value2, isOn, ObjectiveFilterPanel.FilterBeingShown, isOn5, value3, scenarioModifierActivationID, text2, text3, text4, new List<CAbility.EAbilityType> { value4 }, new List<CAbility.EAttackType> { value5 }, isOn3, isOn4, isOn2, isOn6, currentFlagEnum, m_CurrentMapGUIDS.ToList());
				break;
			case EScenarioModifierType.DestroyRoom:
				cScenarioModifier = new CScenarioModifierDestroyRoom(text, count, num, value2, isOn, ObjectiveFilterPanel.FilterBeingShown, isOn5, mapGUID, 10, value3, scenarioModifierActivationID, text2, text3, text4, new List<CAbility.EAbilityType> { value4 }, new List<CAbility.EAttackType> { value5 }, isOn3, isOn4, isOn2, isOn6, currentFlagEnum, m_CurrentMapGUIDS.ToList(), ModifierDestroyRoomInstaKillActors.isOn);
				break;
			case EScenarioModifierType.ActorsCreateGraves:
				cScenarioModifier = new CScenarioModifierActorsCreateGraves(text, count, num, value2, isOn, ObjectiveFilterPanel.FilterBeingShown, isOn5, value3, scenarioModifierActivationID, text2, text3, text4, new List<CAbility.EAbilityType> { value4 }, new List<CAbility.EAttackType> { value5 }, isOn3, isOn4, isOn2, isOn6, currentFlagEnum, m_CurrentMapGUIDS.ToList());
				break;
			case EScenarioModifierType.OverrideCompanionSummonTiles:
				cScenarioModifier = new CScenarioModifierOverrideCompanionSummonTiles(text, count, num, value2, isOn, ObjectiveFilterPanel.FilterBeingShown, isOn5, TeleportSettingsPanel.TileIndexes.ToList(), value3, scenarioModifierActivationID, text2, text3, text4, new List<CAbility.EAbilityType> { value4 }, new List<CAbility.EAttackType> { value5 }, isOn3, isOn4, isOn2, isOn6, currentFlagEnum, m_CurrentMapGUIDS.ToList());
				break;
			case EScenarioModifierType.MakeTrapsObstacles:
				cScenarioModifier = new CScenarioModifierMakeTrapsObstacles(text, count, num, value2, isOn, ObjectiveFilterPanel.FilterBeingShown, isOn5, value3, scenarioModifierActivationID, text2, text3, text4, new List<CAbility.EAbilityType> { value4 }, new List<CAbility.EAttackType> { value5 }, isOn3, isOn4, isOn2, isOn6, currentFlagEnum, m_CurrentMapGUIDS.ToList());
				break;
			case EScenarioModifierType.ActorsDontDropCoins:
				cScenarioModifier = new CScenarioModifierActorsDontDropCoins(text, count, num, value2, isOn, ObjectiveFilterPanel.FilterBeingShown, isOn5, value3, scenarioModifierActivationID, text2, text3, text4, new List<CAbility.EAbilityType> { value4 }, new List<CAbility.EAttackType> { value5 }, isOn3, isOn4, isOn2, isOn6, currentFlagEnum, m_CurrentMapGUIDS.ToList());
				break;
			case EScenarioModifierType.DisableRest:
				cScenarioModifier = new CScenarioModifier(text, count, num, isOn, value, value2, value3, scenarioModifierActivationID, null, isOn5, null, text2, text3, text4, new List<CAbility.EAbilityType> { value4 }, new List<CAbility.EAttackType> { value5 }, isOn3, isOn4, isOn2, isOn6, currentFlagEnum, m_CurrentMapGUIDS.ToList());
				break;
			default:
				throw new NotImplementedException($"Apply not implemented for EScenarioModifierType:{value}");
			}
			if (m_ButtonModeAdd)
			{
				int num2 = 0;
				num2 = LevelEditorController.AddScenarioModifierToScenario(cScenarioModifier);
				LevelEditorScenarioModifierListItem itemEdited = AddNewListItemForScenarioModifier(num2, cScenarioModifier);
				EditModifier(itemEdited);
				StatusText.text = "<color=green> - Successfully added Scenario Modifier - </color>";
			}
			else
			{
				LevelEditorController.ReplaceScenarioModifierInScenarioAtIndex(m_CurrentItem.modifierIndex, cScenarioModifier);
				m_CurrentItem.RefreshDisplayName();
				StatusText.text = "<color=green> - Successfully edited Scenario Modifier - </color>";
			}
		}
		else
		{
			StatusText.text = "<color=red> - " + completionMessage + " - </color>";
			Log.Error("Modifier validation failed: " + completionMessage);
		}
	}

	public void OnButtonDeletePressed()
	{
		if (m_CurrentItem != null)
		{
			LevelEditorController.DeleteScenarioModifierFromScenario(m_CurrentItem.modifierIndex);
			m_ScenarioModifierListItems.Remove(m_CurrentItem);
			UnityEngine.Object.Destroy(m_CurrentItem.gameObject);
			m_CurrentItem = null;
			ModifierDisplayPanel.SetActive(value: false);
			RefreshUIFromCurrentScenarioState();
		}
	}

	public void OnMoveUpPressed()
	{
		if (!(m_CurrentItem == null) && m_CurrentItem.modifierIndex != 0)
		{
			MoveCurrentItem(-1);
		}
	}

	public void OnMoveDownPressed()
	{
		if (!(m_CurrentItem == null) && m_CurrentItem.modifierIndex < m_CurrentScenarioState.ScenarioModifiers.Count - 1)
		{
			MoveCurrentItem(1);
		}
	}

	private void MoveCurrentItem(int amount)
	{
		int num = m_CurrentItem.modifierIndex + amount;
		List<CScenarioModifier> scenarioModifiers = m_CurrentScenarioState.ScenarioModifiers;
		int index = num;
		List<CScenarioModifier> scenarioModifiers2 = m_CurrentScenarioState.ScenarioModifiers;
		int modifierIndex = m_CurrentItem.modifierIndex;
		CScenarioModifier cScenarioModifier = m_CurrentScenarioState.ScenarioModifiers[m_CurrentItem.modifierIndex];
		CScenarioModifier cScenarioModifier2 = m_CurrentScenarioState.ScenarioModifiers[num];
		CScenarioModifier cScenarioModifier3 = (scenarioModifiers[index] = cScenarioModifier);
		cScenarioModifier3 = (scenarioModifiers2[modifierIndex] = cScenarioModifier2);
		RefreshUIFromCurrentScenarioState();
		m_CurrentItem = null;
		EditModifier(m_ScenarioModifierListItems[num]);
	}

	public void OnScenarioModifierTypeDropDownChanged(int value)
	{
		ExposeParametersBasedOnScenarioModifierType((EScenarioModifierType)value);
	}

	public void OnScenarioModifierTriggerPhaseDropDownChanged(int value)
	{
		ExposeParametersBasedOnTriggerType((EScenarioModifierTriggerPhase)value, (EScenarioModifierType)ModifierTypeDropDown.value);
	}

	public void OnScenarioModifierAfterAbilityTypeDropDownChanged(int value)
	{
		ExposeParametersBasedOnAbilityType((CAbility.EAbilityType)value);
	}

	public void OnButtonAddMapToListPressed(string mapSelected)
	{
		m_CurrentMapGUIDS.Add(mapSelected);
		MapPanel.RefreshUIWithItems(m_CurrentMapGUIDS.ToList());
	}

	public void OnButtonMapListItemDeletePressed(string mapGuidToRemove, int indexToRemove)
	{
		m_CurrentMapGUIDS.RemoveAt(indexToRemove);
		MapPanel.RefreshUIWithItems(m_CurrentMapGUIDS.ToList());
	}

	public void OnScenarioModifierActivationTypeDropDownChanged(int value)
	{
		List<string> list = new List<string>();
		list.Add("NONE");
		switch ((EScenarioModifierActivationType)ModifierActivationTypeDropDown.value)
		{
		case EScenarioModifierActivationType.None:
			ModifierActivationIdWrapper.SetActive(value: false);
			break;
		case EScenarioModifierActivationType.QuestCompleted:
			ModifierActivationIdWrapper.SetActive(value: true);
			list.AddRange(MapRuleLibraryClient.MRLYML.Quests.Select((CQuest a) => a.ID).ToList());
			break;
		case EScenarioModifierActivationType.PersonalQuestOwnerInParty:
			ModifierActivationIdWrapper.SetActive(value: true);
			list.AddRange(MapRuleLibraryClient.MRLYML.PersonalQuests.Select((PersonalQuestYMLData a) => a.ID).ToList());
			break;
		}
		ModifierActivationIDDropdown.options.Clear();
		ModifierActivationIDDropdown.AddOptions(list);
	}
}
