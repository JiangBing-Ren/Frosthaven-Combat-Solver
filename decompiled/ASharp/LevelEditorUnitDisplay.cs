using System;
using System.Collections.Generic;
using System.Linq;
using GLOOM;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorUnitDisplay : MonoBehaviour
{
	private LevelEditorUnitsPanel m_ParentUnitsPanel;

	[Header("Display Toggled Items")]
	public LayoutElement MoveButtonElement;

	public LayoutElement DeleteButtonElement;

	public LayoutElement ToggleButtonElement;

	public TextMeshProUGUI ToggleButtonText;

	public GameObject UnitDetailsOptionsPanel;

	public LevelEditorAttackModifiersPanel AttackModiferPanel;

	public LevelEditorAbilityDeckPanel AbilityDeckPanel;

	public LevelEditorItemLoadoutPanel ItemLoadoutPanel;

	public LevelEditorTrapConditionsPanel TrapConditionsPanel;

	public LevelEditorChestConditionsPanel ChestConditionsPanel;

	public LevelEditorSpawnerPanel SpawnerPanel;

	public LevelEditorApparanceOverridePanel PropApparancePanel;

	public LevelEditorPropWithHealthPanel PropWithHealthPanel;

	public LevelEditorGenericListPanel PressurePlateLinkedDoorPanel;

	public LevelEditorGenericListPanel PressurePlateLinkedSpawnerPanel;

	public LevelEditorStatIsBasedOnXPanel StatBasedOnXPanel;

	public LevelEditorAIFocusOverridePanel AIFocusOverridePanel;

	public LevelEditorObjectiveFilterPanel ObjectiveFilterPanel;

	[Header("Mutable UI Items")]
	public TextMeshProUGUI UnitTitle;

	public LayoutElement MonsterStandeeIDElement;

	public TMP_Text MonsterStandeeIDText;

	public LayoutElement LevelInputElement;

	public TMP_InputField LevelInput;

	public LayoutElement HealthInputElement;

	public TMP_InputField HealthInput;

	public LayoutElement DamageInputElement;

	public TMP_InputField DamageInput;

	public LayoutElement AdjacentRangeInputElement;

	public TMP_InputField AdjacentRangeInput;

	public LayoutElement MaxHealthInputElement;

	public TMP_InputField MaxHealthInput;

	public LayoutElement MinInitialMaxHealthInputElement;

	public TMP_InputField MinInitialMaxHealthInput;

	public LayoutElement GoldInputElement;

	public TMP_InputField GoldInput;

	public LayoutElement XPInputElement;

	public TMP_InputField XPInput;

	public LayoutElement PropCustomLocInputElement;

	public TMP_InputField PropCustomLocInput;

	public LayoutElement PropCustomDescriptionLocInputElement;

	public TMP_InputField PropCustomDescriptionLocInput;

	public LayoutElement CustomLocInputElement;

	public TMP_InputField CustomLocInput;

	public TextMeshProUGUI CustomLocLabel;

	public Toggle RevealedToggle;

	public LayoutElement AllyNeutralToggleElement;

	public Toggle AllyToggle;

	public Toggle Enemy2Toggle;

	public Toggle NeutralToggle;

	public Toggle EnemyToggle;

	public LayoutElement TreasureTableElement;

	public TMP_Dropdown TreasureTableDropdown;

	public LayoutElement PressurePlateSpawnedActImmediatelyElement;

	public Toggle PressurePlateSpawnedActImmediatelyToggle;

	public LayoutElement PressurePlateTypeElement;

	public TMP_Dropdown PressurePlateTypeDropdown;

	public LayoutElement PressurePlateCountElement;

	public TMP_InputField PressurePlateCountInput;

	public LayoutElement DoorTypeElement;

	public TMP_Dropdown DoorTypeDropdown;

	public LayoutElement IgnoreEndOfTurnLootingToggleElement;

	public Toggle IgnoreEndOfTurnLootingToggle;

	public LayoutElement PropHasHealthToggleElement;

	public Toggle PropHasHealthToggle;

	public LayoutElement DoorHasExtraLockToggleElement;

	public Toggle DoorHasExtraLockToggle;

	public LayoutElement DoorApparanceLockTypeElement;

	public TMP_Dropdown DoorApparanceLockTypeDropDown;

	public LayoutElement OverrideDisallowMoveOrDestroyElement;

	public Toggle OverrideDisallowMoveOrDestroyToggle;

	public TextMeshProUGUI PropActivatedLabel;

	public LayoutElement PropActivatedToggleElement;

	public Toggle PropActivatedToggle;

	public LayoutElement TreatDifficultTerrainAsTrapToggleElement;

	public Toggle TreatDifficultTerrainAsTrapToggle;

	public LayoutElement LinkedPortalElement;

	public TMP_Dropdown LinkedPortalDropdown;

	public LayoutElement ObstacleIgnoreFlyOrJumpElement;

	public LayoutElement ObstacleBlocksLOSElement;

	public Toggle ObstacleIgnoreFlyOrJumpToggle;

	public Toggle ObstacleBlocksLOSToggle;

	public GameObject PropLinkButtonsPanel;

	public Toggle PropActivatesOnEndTurnToggle;

	public LayoutElement PropActivatesOnEndTurnElement;

	public GameObject PropRotateButton;

	public GameObject BlockingObjectForMonsterCurrentHealthOverride;

	public GameObject BlockingObjectForMonsterMaxHealthOverride;

	[Header("Per Party Size Config")]
	public LayoutElement PerPartySizeConfigElement;

	public TMP_Dropdown PartySizeOneConfigDropDown;

	public TMP_Dropdown PartySizeTwoConfigDropDown;

	public TMP_Dropdown PartySizeThreeConfigDropDown;

	public TMP_Dropdown PartySizeFourConfigDropDown;

	private LevelEditorUnitListItem m_ItemDisplayingFor;

	private LevelEditorUnitsPanel.UnitType m_TypeDisplayed;

	private CPlayerActor m_DisplayedPlayer;

	private PlayerState m_DisplayedPlayerState;

	private CEnemyActor m_DisplayedEnemy;

	private EnemyState m_DisplayedEnemyState;

	private CObjectProp m_DisplayedProp;

	private CSpawner m_DisplayedSpawner;

	private List<CObjectDoor.EDoorType> m_SwappableDoorTypes = new List<CObjectDoor.EDoorType>();

	private List<string> m_TreasureTableOptions = new List<string>();

	private List<string> m_LinkedPortalOptions = new List<string>();

	private void Awake()
	{
		m_ParentUnitsPanel = GetComponentInParent<LevelEditorUnitsPanel>();
	}

	private void ResetObject()
	{
		m_DisplayedPlayer = null;
		m_DisplayedPlayerState = null;
		m_DisplayedEnemy = null;
		m_DisplayedEnemyState = null;
		m_DisplayedProp = null;
		m_DisplayedSpawner = null;
	}

	public void SetListItemToDisplay(LevelEditorUnitListItem itemToDisplay)
	{
		if ((bool)m_ItemDisplayingFor)
		{
			m_ItemDisplayingFor.SetSelectedState(selected: false);
		}
		m_ItemDisplayingFor = itemToDisplay;
		m_ItemDisplayingFor.SetSelectedState(selected: true);
		switch (itemToDisplay.type)
		{
		case LevelEditorUnitsPanel.UnitType.Player:
			SetObjectToDisplay(itemToDisplay.playerObj);
			break;
		case LevelEditorUnitsPanel.UnitType.Enemy:
			SetObjectToDisplay(itemToDisplay.enemyObj);
			break;
		case LevelEditorUnitsPanel.UnitType.Objects:
			SetObjectToDisplay(itemToDisplay.objectObj);
			break;
		case LevelEditorUnitsPanel.UnitType.Prop:
			SetObjectToDisplay(itemToDisplay.propObj);
			break;
		case LevelEditorUnitsPanel.UnitType.Spawner:
			SetObjectToDisplay(itemToDisplay.spawnerObj);
			break;
		}
	}

	public void SetObjectToDisplay(CPlayerActor playerActor)
	{
		ResetObject();
		m_TypeDisplayed = LevelEditorUnitsPanel.UnitType.Player;
		m_DisplayedPlayer = playerActor;
		m_DisplayedPlayerState = ScenarioManager.CurrentScenarioState.Players.Single((PlayerState playerState) => playerState.ActorGuid == playerActor.ActorGuid);
		UpdateUIForDisplay();
		AttackModiferPanel.DisplayAttackModifiersForPlayer(playerActor);
		AbilityDeckPanel.DisplayAbilityDeckForPlayer(playerActor);
		ItemLoadoutPanel.DisplayItemLoadoutForPlayer(playerActor);
		OnLocateUnitPressed();
	}

	public void SetObjectToDisplay(CEnemyActor enemyActor)
	{
		ResetObject();
		m_TypeDisplayed = LevelEditorUnitsPanel.UnitType.Enemy;
		m_DisplayedEnemy = enemyActor;
		m_DisplayedEnemyState = ScenarioManager.CurrentScenarioState.Monsters.Find((EnemyState enemyState) => enemyState.ActorGuid == enemyActor.ActorGuid);
		if (m_DisplayedEnemyState == null)
		{
			m_DisplayedEnemyState = ScenarioManager.CurrentScenarioState.AllyMonsters.Find((EnemyState enemyState) => enemyState.ActorGuid == enemyActor.ActorGuid);
		}
		if (m_DisplayedEnemyState == null)
		{
			m_DisplayedEnemyState = ScenarioManager.CurrentScenarioState.Enemy2Monsters.Find((EnemyState enemyState) => enemyState.ActorGuid == enemyActor.ActorGuid);
		}
		if (m_DisplayedEnemyState == null)
		{
			m_DisplayedEnemyState = ScenarioManager.CurrentScenarioState.NeutralMonsters.Find((EnemyState enemyState) => enemyState.ActorGuid == enemyActor.ActorGuid);
		}
		UpdateUIForDisplay();
		AttackModiferPanel.DisplayAttackModifiersForEnemies();
		AbilityDeckPanel.DisplayAbilityDeckForEnemyClass(enemyActor);
		OnLocateUnitPressed();
	}

	public void SetObjectToDisplay(CObjectActor objectActor)
	{
		ResetObject();
		m_TypeDisplayed = LevelEditorUnitsPanel.UnitType.Objects;
		m_DisplayedEnemy = objectActor;
		m_DisplayedEnemyState = ScenarioManager.CurrentScenarioState.Objects.Single((ObjectState enemyState) => enemyState.ActorGuid == objectActor.ActorGuid);
		UpdateUIForDisplay();
		AttackModiferPanel.DisplayAttackModifiersForEnemies();
		AbilityDeckPanel.DisplayAbilityDeckForEnemyClass(objectActor);
		OnLocateUnitPressed();
	}

	public void SetObjectToDisplay(CObjectProp prop)
	{
		ResetObject();
		m_TypeDisplayed = LevelEditorUnitsPanel.UnitType.Prop;
		m_DisplayedProp = prop;
		UpdateUIForDisplay();
		OnLocateUnitPressed();
	}

	public void SetObjectToDisplay(CSpawner spawner)
	{
		ResetObject();
		m_TypeDisplayed = LevelEditorUnitsPanel.UnitType.Spawner;
		m_DisplayedSpawner = spawner;
		UpdateUIForDisplay();
		SpawnerPanel.DisplayForSpawner(spawner);
		OnLocateUnitPressed();
	}

	private void OnDoorPressurePlateTypeChanged(int type)
	{
		PressurePlateCountElement.gameObject.SetActive(type == 2);
	}

	public void UpdateUIForDisplay()
	{
		if (m_DisplayedPlayer == null && m_DisplayedEnemy == null && m_DisplayedProp == null && m_DisplayedSpawner == null)
		{
			return;
		}
		if (SaveData.Instance.Global.CurrentEditorLevelData.PartySpawnType == ELevelPartyChoiceType.LoadAdventureParty)
		{
			BlockingObjectForMonsterCurrentHealthOverride.SetActive(SaveData.Instance.Global.CurrentEditorLevelData.SetEnemyHealthToMaxOnPlay);
			BlockingObjectForMonsterMaxHealthOverride.SetActive(SaveData.Instance.Global.CurrentEditorLevelData.EnemyMaxHealthBasedOnPartyLevel);
		}
		else
		{
			BlockingObjectForMonsterCurrentHealthOverride.SetActive(value: false);
			BlockingObjectForMonsterMaxHealthOverride.SetActive(value: false);
		}
		if (PropLinkButtonsPanel != null)
		{
			PropLinkButtonsPanel.SetActive(value: false);
		}
		if (PropRotateButton != null)
		{
			PropRotateButton.SetActive(value: false);
		}
		SpawnerPanel.gameObject.SetActive(value: false);
		DamageInputElement.gameObject.SetActive(value: false);
		AdjacentRangeInputElement.gameObject.SetActive(value: false);
		PropCustomLocInputElement.gameObject.SetActive(value: false);
		PropCustomDescriptionLocInputElement?.gameObject.SetActive(value: false);
		CustomLocInputElement.gameObject.SetActive(value: false);
		UnitDetailsOptionsPanel.SetActive(value: true);
		ObjectiveFilterPanel?.gameObject.SetActive(value: false);
		PressurePlateTypeElement.gameObject.SetActive(value: false);
		PressurePlateSpawnedActImmediatelyElement.gameObject.SetActive(value: false);
		DoorTypeElement.gameObject.SetActive(value: false);
		MonsterStandeeIDElement.gameObject.SetActive(value: false);
		LevelInputElement.gameObject.SetActive(value: false);
		HealthInputElement.gameObject.SetActive(value: false);
		MinInitialMaxHealthInputElement.gameObject.SetActive(value: false);
		MaxHealthInputElement.gameObject.SetActive(value: false);
		PropActivatesOnEndTurnElement.gameObject.SetActive(value: false);
		switch (m_TypeDisplayed)
		{
		case LevelEditorUnitsPanel.UnitType.Player:
			UnitTitle.text = LocalizationManager.GetTranslation(m_DisplayedPlayer.ActorLocKey()) + "\n(" + m_DisplayedPlayer.ActorGuid + ")";
			CharacterClassManager.Classes.SingleOrDefault((CCharacterClass x) => x.ID == m_DisplayedPlayer.Class.ID);
			LevelInputElement.gameObject.SetActive(value: true);
			LevelInput.text = m_DisplayedPlayer.Level.ToString();
			MaxHealthInputElement.gameObject.SetActive(value: true);
			if (m_DisplayedPlayer.MaxHealth != m_DisplayedPlayer.GetMaxHealthForCharacterLevel())
			{
				MaxHealthInput.text = m_DisplayedPlayer.MaxHealth.ToString();
			}
			else
			{
				MaxHealthInput.text = string.Empty;
			}
			HealthInputElement.gameObject.SetActive(value: true);
			if (m_DisplayedPlayer.Health != m_DisplayedPlayer.MaxHealth)
			{
				HealthInput.text = m_DisplayedPlayer.Health.ToString();
			}
			else
			{
				HealthInput.text = string.Empty;
			}
			RevealedToggle.isOn = m_DisplayedPlayerState.IsRevealed;
			AllyNeutralToggleElement?.gameObject.SetActive(value: false);
			GoldInputElement.gameObject.SetActive(value: false);
			XPInputElement.gameObject.SetActive(value: false);
			TreasureTableElement.gameObject.SetActive(value: false);
			LinkedPortalElement?.gameObject.SetActive(value: false);
			ToggleButtonElement.gameObject.SetActive(value: false);
			TrapConditionsPanel?.gameObject.SetActive(value: false);
			ChestConditionsPanel?.gameObject.SetActive(value: false);
			PropApparancePanel?.gameObject.SetActive(value: false);
			IgnoreEndOfTurnLootingToggleElement?.gameObject.SetActive(value: false);
			PropHasHealthToggleElement?.gameObject.SetActive(value: false);
			DoorHasExtraLockToggleElement?.gameObject.SetActive(value: false);
			DoorApparanceLockTypeElement?.gameObject.SetActive(value: false);
			PressurePlateLinkedDoorPanel?.gameObject.SetActive(value: false);
			PressurePlateLinkedSpawnerPanel?.gameObject.SetActive(value: false);
			StatBasedOnXPanel?.gameObject.SetActive(value: false);
			AIFocusOverridePanel?.gameObject.SetActive(value: false);
			OverrideDisallowMoveOrDestroyElement?.gameObject.SetActive(value: false);
			PropActivatedToggleElement?.gameObject.SetActive(value: false);
			TreatDifficultTerrainAsTrapToggleElement?.gameObject.SetActive(value: false);
			ObjectiveFilterPanel?.gameObject.SetActive(value: false);
			ObstacleIgnoreFlyOrJumpElement?.gameObject.SetActive(value: false);
			ObstacleBlocksLOSElement?.gameObject.SetActive(value: false);
			PropActivatesOnEndTurnElement?.gameObject.SetActive(value: false);
			break;
		case LevelEditorUnitsPanel.UnitType.Enemy:
		case LevelEditorUnitsPanel.UnitType.Objects:
		{
			MonsterStandeeIDElement.gameObject.SetActive(value: true);
			LevelInputElement.gameObject.SetActive(value: true);
			ItemLoadoutPanel.gameObject.SetActive(value: false);
			UnitTitle.text = LocalizationManager.GetTranslation(m_DisplayedEnemy.ActorLocKey()) + "\n(" + m_DisplayedEnemy.ActorGuid + ")";
			if (m_DisplayedEnemy.ID != 0)
			{
				MonsterStandeeIDText.text = m_DisplayedEnemy.ID.ToString();
			}
			else
			{
				int nextID = m_DisplayedEnemy.MonsterClass.GetNextID();
				if (nextID != -1)
				{
					MonsterStandeeIDText.text = nextID.ToString();
				}
				else
				{
					Log.Error("Maximum number of enemies of this class has been exceeded!");
					MonsterStandeeIDText.text = string.Empty;
				}
			}
			CMonsterClass cMonsterClass = MonsterClassManager.MonsterAndObjectClasses.SingleOrDefault((CMonsterClass x) => x.ID == m_DisplayedEnemy.MonsterClass.ID);
			MaxHealthInputElement.gameObject.SetActive(value: true);
			if (m_DisplayedEnemy.MaxHealth != cMonsterClass.Health())
			{
				MaxHealthInput.text = m_DisplayedEnemy.MaxHealth.ToString();
			}
			else
			{
				MaxHealthInput.text = string.Empty;
			}
			MinInitialMaxHealthInputElement.gameObject.SetActive(value: true);
			if (m_DisplayedEnemy.MinInitialMaxHealth != -1)
			{
				MinInitialMaxHealthInput.text = m_DisplayedEnemy.MinInitialMaxHealth.ToString();
			}
			else
			{
				MinInitialMaxHealthInput.text = string.Empty;
			}
			HealthInputElement.gameObject.SetActive(value: true);
			if (m_DisplayedEnemy.Health != m_DisplayedEnemy.MaxHealth)
			{
				HealthInput.text = m_DisplayedEnemy.Health.ToString();
			}
			else
			{
				HealthInput.text = string.Empty;
			}
			LevelInput.text = m_DisplayedEnemy.Level.ToString();
			RevealedToggle.isOn = m_DisplayedEnemyState.IsRevealed;
			AllyNeutralToggleElement?.gameObject.SetActive(value: true);
			AllyToggle.isOn = m_DisplayedEnemy.Type == CActor.EType.Ally;
			NeutralToggle.isOn = m_DisplayedEnemy.Type == CActor.EType.Neutral;
			Enemy2Toggle.isOn = m_DisplayedEnemy.Type == CActor.EType.Enemy2;
			EnemyToggle.isOn = m_DisplayedEnemy.Type == CActor.EType.Enemy;
			PartySizeOneConfigDropDown.options.Clear();
			PartySizeTwoConfigDropDown.options.Clear();
			PartySizeThreeConfigDropDown.options.Clear();
			PartySizeFourConfigDropDown.options.Clear();
			PartySizeOneConfigDropDown.AddOptions(EnemyState.PerPartySizeConfigOptions.Select((ScenarioManager.EPerPartySizeConfig t) => t.ToString()).ToList());
			PartySizeTwoConfigDropDown.AddOptions(EnemyState.PerPartySizeConfigOptions.Select((ScenarioManager.EPerPartySizeConfig t) => t.ToString()).ToList());
			PartySizeThreeConfigDropDown.AddOptions(EnemyState.PerPartySizeConfigOptions.Select((ScenarioManager.EPerPartySizeConfig t) => t.ToString()).ToList());
			PartySizeFourConfigDropDown.AddOptions(EnemyState.PerPartySizeConfigOptions.Select((ScenarioManager.EPerPartySizeConfig t) => t.ToString()).ToList());
			PartySizeOneConfigDropDown.value = (int)m_DisplayedEnemyState.GetConfigForPartySize(1);
			PartySizeTwoConfigDropDown.value = (int)m_DisplayedEnemyState.GetConfigForPartySize(2);
			PartySizeThreeConfigDropDown.value = (int)m_DisplayedEnemyState.GetConfigForPartySize(3);
			PartySizeFourConfigDropDown.value = (int)m_DisplayedEnemyState.GetConfigForPartySize(4);
			GoldInputElement.gameObject.SetActive(value: false);
			XPInputElement.gameObject.SetActive(value: false);
			TreasureTableElement.gameObject.SetActive(value: false);
			LinkedPortalElement?.gameObject.SetActive(value: false);
			ToggleButtonElement.gameObject.SetActive(value: false);
			TrapConditionsPanel?.gameObject.SetActive(value: false);
			ChestConditionsPanel?.gameObject.SetActive(value: false);
			PropApparancePanel?.gameObject.SetActive(value: false);
			IgnoreEndOfTurnLootingToggleElement?.gameObject.SetActive(value: false);
			PropHasHealthToggleElement?.gameObject.SetActive(value: false);
			DoorHasExtraLockToggleElement?.gameObject.SetActive(value: false);
			DoorApparanceLockTypeElement?.gameObject.SetActive(value: false);
			PropWithHealthPanel?.gameObject.SetActive(value: false);
			PressurePlateLinkedDoorPanel?.gameObject.SetActive(value: false);
			PressurePlateLinkedSpawnerPanel?.gameObject.SetActive(value: false);
			StatBasedOnXPanel?.gameObject.SetActive(value: true);
			StatBasedOnXPanel?.SetShowingForMonsterClass(m_DisplayedEnemy.Class.ID);
			AIFocusOverridePanel?.gameObject.SetActive(value: true);
			AIFocusOverridePanel?.SetShowingForActorState(m_DisplayedEnemyState);
			OverrideDisallowMoveOrDestroyElement?.gameObject.SetActive(value: false);
			PropActivatedToggleElement?.gameObject.SetActive(value: false);
			TreatDifficultTerrainAsTrapToggleElement?.gameObject.SetActive(value: false);
			ObjectiveFilterPanel?.gameObject.SetActive(value: false);
			ObstacleIgnoreFlyOrJumpElement?.gameObject.SetActive(value: false);
			ObstacleBlocksLOSElement?.gameObject.SetActive(value: false);
			PropActivatesOnEndTurnElement?.gameObject.SetActive(value: false);
			break;
		}
		case LevelEditorUnitsPanel.UnitType.Prop:
		{
			UnitTitle.text = "[" + (m_DisplayedProp.Activated ? "ACTIVATED" : "UNACTIVATED") + "]\n" + m_DisplayedProp.InstanceName + "\nIndex X:" + m_DisplayedProp.ArrayIndex.X + "|Y:" + m_DisplayedProp.ArrayIndex.Y;
			AllyNeutralToggleElement?.gameObject.SetActive(value: false);
			PressurePlateLinkedDoorPanel?.gameObject.SetActive(value: false);
			PressurePlateLinkedSpawnerPanel?.gameObject.SetActive(value: false);
			AttackModiferPanel.gameObject.SetActive(value: false);
			AbilityDeckPanel.gameObject.SetActive(value: false);
			ObjectiveFilterPanel?.gameObject.SetActive(value: false);
			if (PropLinkButtonsPanel != null)
			{
				PropLinkButtonsPanel.SetActive(value: true);
			}
			if (PropRotateButton != null)
			{
				PropRotateButton.SetActive(value: true);
			}
			bool flag = true;
			bool flag2 = true;
			CObjectProp displayedProp = m_DisplayedProp;
			CObjectChest chest = displayedProp as CObjectChest;
			if (chest != null)
			{
				GoldInputElement.gameObject.SetActive(value: true);
				XPInputElement.gameObject.SetActive(value: true);
				GoldInput.text = chest.Gold.ToString();
				XPInput.text = chest.XP.ToString();
				TreasureTableElement.gameObject.SetActive(value: true);
				ItemLoadoutPanel.gameObject.SetActive(value: true);
				ItemLoadoutPanel.DisplayItemLoadoutForObjectProp(m_DisplayedProp);
				TreasureTableDropdown.options.Clear();
				m_TreasureTableOptions = ScenarioRuleClient.SRLYML.TreasureTables.Select((TreasureTable x) => x.Name).ToList();
				m_TreasureTableOptions.Insert(0, "NONE");
				TreasureTableDropdown.AddOptions(m_TreasureTableOptions);
				TMP_Dropdown treasureTableDropdown = TreasureTableDropdown;
				List<string> chestTreasureTablesID = chest.ChestTreasureTablesID;
				treasureTableDropdown.value = ((chestTreasureTablesID != null && chestTreasureTablesID.Count > 0) ? TreasureTableDropdown.options.FindIndex((TMP_Dropdown.OptionData x) => x.text == chest.ChestTreasureTablesID[0]) : 0);
				DamageInputElement.gameObject.SetActive(value: true);
				if (chest.DamageValue > 0)
				{
					DamageInput.text = chest.DamageValue.ToString();
				}
				else
				{
					DamageInput.text = string.Empty;
				}
				ChestConditionsPanel?.DisplayConditionsForChest(m_DisplayedProp as CObjectChest);
				ChestConditionsPanel?.gameObject.SetActive(value: true);
				flag = true;
				flag2 = false;
			}
			else
			{
				GoldInputElement.gameObject.SetActive(value: false);
				XPInputElement.gameObject.SetActive(value: false);
				TreasureTableElement.gameObject.SetActive(value: false);
				ItemLoadoutPanel.gameObject.SetActive(value: false);
				DamageInputElement.gameObject.SetActive(value: false);
				AdjacentRangeInputElement.gameObject.SetActive(value: false);
				ChestConditionsPanel?.gameObject.SetActive(value: false);
			}
			displayedProp = m_DisplayedProp;
			CObjectDoor doorProp = displayedProp as CObjectDoor;
			if (doorProp != null)
			{
				UnitTitle.text = "[" + doorProp.DoorType.ToString() + "]\n" + doorProp.InstanceName + "\nIndex X:" + doorProp.ArrayIndex.X + "|Y:" + doorProp.ArrayIndex.Y;
				MoveButtonElement.gameObject.SetActive(value: false);
				DeleteButtonElement.gameObject.SetActive(value: false);
				ToggleButtonText.text = "Rotate";
				ToggleButtonElement.gameObject.SetActive(!doorProp.IsDungeonEntrance);
				flag = true;
				flag2 = false;
				DoorHasExtraLockToggleElement?.gameObject.SetActive(value: true);
				DoorHasExtraLockToggle?.SetIsOnWithoutNotify(doorProp.DoorHasExtraLock);
				DoorApparanceLockTypeDropDown.options.Clear();
				DoorApparanceLockTypeDropDown.AddOptions(CObjectDoor.LockTypes.Select((CObjectDoor.ELockType t) => t.ToString()).ToList());
				DoorApparanceLockTypeDropDown.SetValueWithoutNotify(CObjectDoor.LockTypes.IndexOf(doorProp.LockType));
				DoorApparanceLockTypeElement?.gameObject.SetActive(value: true);
				PropActivatedLabel?.SetText("Door opened state\n<i>(Revealing rooms opens doors by default)</i>");
				PropActivatedToggle?.SetIsOnWithoutNotify(doorProp.Activated);
				PropActivatedToggleElement?.gameObject.SetActive(value: true);
				PressurePlateTypeElement.gameObject.SetActive(value: true);
				DoorTypeElement.gameObject.SetActive(!doorProp.IsDungeonEntrance);
				PressurePlateTypeDropdown.options.Clear();
				PressurePlateTypeDropdown.AddOptions(CObjectDoor.DoorPressurePlateLockTypes.Select((CObjectDoor.EDoorPressurePlateLockType t) => t.ToString()).ToList());
				PressurePlateTypeDropdown.value = PressurePlateTypeDropdown.options.FindIndex((TMP_Dropdown.OptionData x) => x.text == doorProp.DoorPressurePlateLockType.ToString());
				PressurePlateTypeDropdown.onValueChanged.RemoveListener(OnDoorPressurePlateTypeChanged);
				PressurePlateTypeDropdown.onValueChanged.AddListener(OnDoorPressurePlateTypeChanged);
				PressurePlateCountElement.gameObject.SetActive(doorProp.DoorPressurePlateLockType == CObjectDoor.EDoorPressurePlateLockType.XActivatedToOpen);
				PressurePlateCountInput.text = doorProp.DoorPressurePlateLockCount.ToString();
				DoorTypeDropdown.options.Clear();
				m_SwappableDoorTypes = CObjectDoor.SwappableTypes(doorProp.DoorType).ToList();
				DoorTypeDropdown.AddOptions(m_SwappableDoorTypes.Select((CObjectDoor.EDoorType t) => t.ToString()).ToList());
				DoorTypeDropdown.SetValueWithoutNotify(m_SwappableDoorTypes.IndexOf(doorProp.DoorType));
			}
			else
			{
				MoveButtonElement.gameObject.SetActive(value: true);
				DeleteButtonElement.gameObject.SetActive(value: true);
				ToggleButtonElement.gameObject.SetActive(value: false);
				PressurePlateCountElement.gameObject.SetActive(value: false);
				DoorHasExtraLockToggleElement?.gameObject.SetActive(value: false);
				DoorApparanceLockTypeElement?.gameObject.SetActive(value: false);
				PropActivatedToggleElement?.gameObject.SetActive(value: false);
			}
			if (m_DisplayedProp is CObjectTrap cObjectTrap)
			{
				DamageInputElement.gameObject.SetActive(value: true);
				if (cObjectTrap.HasCustomDamageSet)
				{
					DamageInput.text = cObjectTrap.DamageValue.ToString();
				}
				else
				{
					DamageInput.text = string.Empty;
				}
				AdjacentRangeInputElement.gameObject.SetActive(value: true);
				AdjacentRangeInput.text = cObjectTrap.AdjacentRange.ToString();
				TrapConditionsPanel?.DisplayConditionsForTrap(m_DisplayedProp as CObjectTrap);
				TrapConditionsPanel?.gameObject.SetActive(value: true);
				flag = true;
				flag2 = false;
			}
			else
			{
				TrapConditionsPanel?.gameObject.SetActive(value: false);
			}
			displayedProp = m_DisplayedProp;
			CObjectPressurePlate pressurePlate = displayedProp as CObjectPressurePlate;
			if (pressurePlate != null)
			{
				PressurePlateLinkedDoorPanel?.gameObject.SetActive(value: true);
				PopulatePressurePlateLinkedDoor(pressurePlate);
				PressurePlateLinkedSpawnerPanel?.gameObject.SetActive(value: true);
				PopulatePressurePlateLinkedSpawner(pressurePlate);
				PressurePlateTypeElement.gameObject.SetActive(value: true);
				PressurePlateTypeDropdown.options.Clear();
				PressurePlateTypeDropdown.AddOptions(CObjectPressurePlate.PressurePlateTypes.Select((CObjectPressurePlate.EPressurePlateType t) => t.ToString()).ToList());
				PressurePlateTypeDropdown.value = PressurePlateTypeDropdown.options.FindIndex((TMP_Dropdown.OptionData x) => x.text == pressurePlate.PressurePlateType.ToString());
				PropActivatesOnEndTurnToggle?.SetIsOnWithoutNotify(pressurePlate.ActivatesOnEndTurn);
				PropActivatesOnEndTurnElement?.gameObject.SetActive(value: true);
				GameObject gameObject = PressurePlateSpawnedActImmediatelyElement.gameObject;
				List<string> linkedSpawnerGuids = pressurePlate.LinkedSpawnerGuids;
				gameObject.SetActive(linkedSpawnerGuids != null && linkedSpawnerGuids.Count > 0);
				PressurePlateSpawnedActImmediatelyToggle.isOn = pressurePlate.SpawnedActImmediately;
				flag = true;
				flag2 = false;
			}
			displayedProp = m_DisplayedProp;
			CObjectPortal portal = displayedProp as CObjectPortal;
			if (portal != null)
			{
				LinkedPortalElement.gameObject.SetActive(value: true);
				LinkedPortalDropdown.options.Clear();
				m_LinkedPortalOptions = (from x in ScenarioManager.CurrentScenarioState.PortalProps
					select x.PropGuid into y
					where y != portal.PropGuid
					select y).ToList();
				m_LinkedPortalOptions.Insert(0, "NONE");
				LinkedPortalDropdown.AddOptions(m_LinkedPortalOptions);
				LinkedPortalDropdown.value = ((portal.LinkedPortalGuid != null) ? LinkedPortalDropdown.options.FindIndex((TMP_Dropdown.OptionData x) => x.text == portal.LinkedPortalGuid) : 0);
				ObjectiveFilterPanel?.gameObject.SetActive(value: true);
				ObjectiveFilterPanel.SetShowing(portal.PortalUsageFilter, "\"Portal Usage\" Filter");
			}
			else
			{
				LinkedPortalElement?.gameObject.SetActive(value: false);
			}
			if (m_DisplayedProp is CObjectObstacle cObjectObstacle)
			{
				ObstacleIgnoreFlyOrJumpElement?.gameObject.SetActive(value: true);
				ObstacleBlocksLOSElement?.gameObject.SetActive(value: true);
				ObstacleIgnoreFlyOrJumpToggle?.SetIsOnWithoutNotify(cObjectObstacle.IgnoresFlyAndJump);
				ObstacleBlocksLOSToggle?.SetIsOnWithoutNotify(cObjectObstacle.BlockLOS);
			}
			else
			{
				ObstacleIgnoreFlyOrJumpElement?.gameObject.SetActive(value: false);
				ObstacleBlocksLOSElement?.gameObject.SetActive(value: false);
			}
			TreatDifficultTerrainAsTrapToggleElement?.gameObject.SetActive(value: false);
			if (m_DisplayedProp is CObjectDifficultTerrain cObjectDifficultTerrain)
			{
				TreatDifficultTerrainAsTrapToggleElement?.gameObject.SetActive(value: true);
				TreatDifficultTerrainAsTrapToggle?.gameObject.SetActive(value: true);
				TreatDifficultTerrainAsTrapToggle?.SetIsOnWithoutNotify(cObjectDifficultTerrain.TreatAsTrap);
				if (cObjectDifficultTerrain.TreatAsTrap)
				{
					ObjectiveFilterPanel?.gameObject.SetActive(value: true);
					if (cObjectDifficultTerrain.TreatAsTrapFilter == null)
					{
						cObjectDifficultTerrain.TreatAsTrapFilter = new CObjectiveFilter();
					}
					ObjectiveFilterPanel.SetShowing(cObjectDifficultTerrain.TreatAsTrapFilter, "\"Treat as Trap\" Filter");
				}
			}
			if (flag)
			{
				PropApparancePanel?.gameObject.SetActive(value: true);
				PropApparancePanel?.SetPropDisplayed(m_DisplayedProp);
			}
			else
			{
				PropApparancePanel?.gameObject.SetActive(value: false);
			}
			if (flag2)
			{
				OverrideDisallowMoveOrDestroyElement?.gameObject.SetActive(value: true);
				OverrideDisallowMoveOrDestroyToggle?.SetIsOnWithoutNotify(m_DisplayedProp.OverrideDisallowDestroyAndMove);
			}
			else
			{
				OverrideDisallowMoveOrDestroyElement?.gameObject.SetActive(value: false);
			}
			PartySizeOneConfigDropDown.options.Clear();
			PartySizeTwoConfigDropDown.options.Clear();
			PartySizeThreeConfigDropDown.options.Clear();
			PartySizeFourConfigDropDown.options.Clear();
			PartySizeOneConfigDropDown.AddOptions(CObjectProp.PerPartySizeConfigOptions.Select((ScenarioManager.EPerPartySizeConfig t) => t.ToString()).ToList());
			PartySizeTwoConfigDropDown.AddOptions(CObjectProp.PerPartySizeConfigOptions.Select((ScenarioManager.EPerPartySizeConfig t) => t.ToString()).ToList());
			PartySizeThreeConfigDropDown.AddOptions(CObjectProp.PerPartySizeConfigOptions.Select((ScenarioManager.EPerPartySizeConfig t) => t.ToString()).ToList());
			PartySizeFourConfigDropDown.AddOptions(CObjectProp.PerPartySizeConfigOptions.Select((ScenarioManager.EPerPartySizeConfig t) => t.ToString()).ToList());
			PartySizeOneConfigDropDown.value = (int)m_DisplayedProp.GetConfigForPartySize(1);
			PartySizeTwoConfigDropDown.value = (int)m_DisplayedProp.GetConfigForPartySize(2);
			PartySizeThreeConfigDropDown.value = (int)m_DisplayedProp.GetConfigForPartySize(3);
			PartySizeFourConfigDropDown.value = (int)m_DisplayedProp.GetConfigForPartySize(4);
			IgnoreEndOfTurnLootingToggle?.SetIsOnWithoutNotify(m_DisplayedProp.IgnoreEndOfTurnLooting);
			IgnoreEndOfTurnLootingToggleElement?.gameObject.SetActive(m_DisplayedProp.IsLootable);
			if (m_DisplayedProp.IsLootable)
			{
				CustomLocInputElement?.gameObject.SetActive(value: true);
				ObjectiveFilterPanel?.gameObject.SetActive(value: true);
				if (m_DisplayedProp.CanLootFilter == null)
				{
					m_DisplayedProp.CanLootFilter = new CObjectiveFilter();
				}
				ObjectiveFilterPanel?.SetShowing(m_DisplayedProp.CanLootFilter, "\"Can Loot\" Filter");
				CustomLocLabel?.SetText("\"Can Loot\" Loc");
				CustomLocInput.SetTextWithoutNotify(m_DisplayedProp.CanLootLocKey);
				PropCustomLocInputElement?.gameObject.SetActive(value: true);
				PropCustomLocInput.SetTextWithoutNotify(m_DisplayedProp.PropCustomLocKey);
				PropCustomDescriptionLocInputElement?.gameObject.SetActive(value: true);
				PropCustomDescriptionLocInput.SetTextWithoutNotify(m_DisplayedProp.PropCustomDescriptionLocKey);
			}
			else if (m_DisplayedProp.ObjectType == ScenarioManager.ObjectImportType.Obstacle || m_DisplayedProp is CObjectDifficultTerrain || m_DisplayedProp.ObjectType == ScenarioManager.ObjectImportType.TerrainVisualEffect || m_DisplayedProp.ObjectType == ScenarioManager.ObjectImportType.PressurePlate || m_DisplayedProp.ObjectType == ScenarioManager.ObjectImportType.GenericProp)
			{
				PropCustomLocInputElement?.gameObject.SetActive(value: true);
				PropCustomLocInput.SetTextWithoutNotify(m_DisplayedProp.PropCustomLocKey);
				PropCustomDescriptionLocInputElement?.gameObject.SetActive(value: true);
				PropCustomDescriptionLocInput.SetTextWithoutNotify(m_DisplayedProp.PropCustomDescriptionLocKey);
			}
			PropHasHealthToggle?.SetIsOnWithoutNotify(m_DisplayedProp.PropHealthDetails != null && m_DisplayedProp.PropHealthDetails.HasHealth);
			PropHasHealthToggleElement?.gameObject.SetActive(value: true);
			PropWithHealthPanel?.gameObject.SetActive(PropHasHealthToggle.isOn);
			PropWithHealthPanel?.SetPropDisplayed(m_DisplayedProp);
			StatBasedOnXPanel?.gameObject.SetActive(PropHasHealthToggle.isOn);
			StatBasedOnXPanel?.SetShowingForPropHealth(m_DisplayedProp);
			AIFocusOverridePanel?.gameObject.SetActive(value: false);
			break;
		}
		case LevelEditorUnitsPanel.UnitType.Spawner:
			UnitTitle.text = m_DisplayedSpawner.SpawnerGuid;
			AllyNeutralToggleElement?.gameObject.SetActive(value: false);
			UnitDetailsOptionsPanel.SetActive(value: false);
			MoveButtonElement.gameObject.SetActive(value: true);
			DeleteButtonElement.gameObject.SetActive(value: true);
			ToggleButtonElement.gameObject.SetActive(value: false);
			GoldInputElement.gameObject.SetActive(value: false);
			XPInputElement.gameObject.SetActive(value: false);
			TreasureTableElement.gameObject.SetActive(value: false);
			LinkedPortalElement?.gameObject.SetActive(value: false);
			SpawnerPanel.gameObject.SetActive(value: true);
			SpawnerPanel.SetupButtonCallbacks(OnLocateUnitPressed, OnMoveUnitPressed, OnDeleteUnitPressed, OnApplyToUnitPressed, OnToggleUnitPressed);
			ItemLoadoutPanel.gameObject.SetActive(value: false);
			AttackModiferPanel.gameObject.SetActive(value: false);
			AbilityDeckPanel.gameObject.SetActive(value: false);
			ChestConditionsPanel?.gameObject.SetActive(value: false);
			TrapConditionsPanel?.gameObject.SetActive(value: false);
			PropApparancePanel?.gameObject.SetActive(value: false);
			IgnoreEndOfTurnLootingToggleElement?.gameObject.SetActive(value: false);
			PropHasHealthToggleElement?.gameObject.SetActive(value: false);
			DoorHasExtraLockToggleElement?.gameObject.SetActive(value: false);
			DoorApparanceLockTypeElement?.gameObject.SetActive(value: false);
			PropWithHealthPanel?.gameObject.SetActive(value: false);
			PressurePlateLinkedDoorPanel?.gameObject.SetActive(value: false);
			PressurePlateLinkedSpawnerPanel?.gameObject.SetActive(value: false);
			StatBasedOnXPanel?.gameObject.SetActive(value: false);
			AIFocusOverridePanel?.gameObject.SetActive(value: true);
			AIFocusOverridePanel?.SetShowingForSpawner(m_DisplayedSpawner);
			OverrideDisallowMoveOrDestroyElement?.gameObject.SetActive(value: false);
			PropActivatedToggleElement?.gameObject.SetActive(value: false);
			TreatDifficultTerrainAsTrapToggleElement?.gameObject.SetActive(value: false);
			ObjectiveFilterPanel?.gameObject.SetActive(value: false);
			ObstacleIgnoreFlyOrJumpElement?.gameObject.SetActive(value: false);
			ObstacleBlocksLOSElement?.gameObject.SetActive(value: false);
			PropActivatesOnEndTurnElement?.gameObject.SetActive(value: false);
			break;
		}
	}

	public void OnPressRotateProp()
	{
		CObjectProp displayedProp = m_DisplayedProp;
		if (displayedProp != null)
		{
			LevelEditorController.s_Instance.TryRotateProp(displayedProp);
			UpdateUIForDisplay();
		}
	}

	private void MoveMonsterToFrom(CActor.EType type, List<CEnemyActor> toScenarioInitialList, List<CEnemyActor> toScenarioList, List<EnemyState> toStateList, List<GameObject> toClientList)
	{
		if (toStateList.Contains(m_DisplayedEnemyState))
		{
			return;
		}
		GameObject gameObject = Choreographer.s_Choreographer.m_ClientAllyMonsters.Find((GameObject x) => ActorBehaviour.GetActorBehaviour(x).Actor == m_DisplayedEnemy);
		if (gameObject == null)
		{
			gameObject = Choreographer.s_Choreographer.m_ClientNeutralMonsters.Find((GameObject x) => ActorBehaviour.GetActorBehaviour(x).Actor == m_DisplayedEnemy);
		}
		if (gameObject == null)
		{
			gameObject = Choreographer.s_Choreographer.m_ClientEnemy2Monsters.Find((GameObject x) => ActorBehaviour.GetActorBehaviour(x).Actor == m_DisplayedEnemy);
		}
		if (gameObject == null)
		{
			gameObject = Choreographer.s_Choreographer.m_ClientEnemies.Find((GameObject x) => ActorBehaviour.GetActorBehaviour(x).Actor == m_DisplayedEnemy);
		}
		toScenarioInitialList.Add(m_DisplayedEnemy);
		toScenarioList.Add(m_DisplayedEnemy);
		toStateList.Add(m_DisplayedEnemyState);
		toClientList.Add(gameObject);
		if (type != CActor.EType.Ally)
		{
			ScenarioManager.Scenario.InitialAllyMonsters.RemoveAll((CEnemyActor x) => x.ActorGuid == m_DisplayedEnemy.ActorGuid);
			ScenarioManager.Scenario.AllyMonsters.Remove(m_DisplayedEnemy);
			ScenarioManager.CurrentScenarioState.AllyMonsters.Remove(m_DisplayedEnemyState);
			Choreographer.s_Choreographer.m_ClientAllyMonsters.Remove(gameObject);
		}
		if (type != CActor.EType.Enemy2)
		{
			ScenarioManager.Scenario.InitialEnemy2Monsters.RemoveAll((CEnemyActor x) => x.ActorGuid == m_DisplayedEnemy.ActorGuid);
			ScenarioManager.Scenario.Enemy2Monsters.Remove(m_DisplayedEnemy);
			ScenarioManager.CurrentScenarioState.Enemy2Monsters.Remove(m_DisplayedEnemyState);
			Choreographer.s_Choreographer.m_ClientEnemy2Monsters.Remove(gameObject);
		}
		if (type != CActor.EType.Neutral)
		{
			ScenarioManager.Scenario.InitialNeutralMonsters.RemoveAll((CEnemyActor x) => x.ActorGuid == m_DisplayedEnemy.ActorGuid);
			ScenarioManager.Scenario.NeutralMonsters.Remove(m_DisplayedEnemy);
			ScenarioManager.CurrentScenarioState.NeutralMonsters.Remove(m_DisplayedEnemyState);
			Choreographer.s_Choreographer.m_ClientNeutralMonsters.Remove(gameObject);
		}
		if (type != CActor.EType.Enemy)
		{
			ScenarioManager.Scenario.InitialEnemies.RemoveAll((CEnemyActor x) => x.ActorGuid == m_DisplayedEnemy.ActorGuid);
			ScenarioManager.Scenario.Enemies.Remove(m_DisplayedEnemy);
			ScenarioManager.CurrentScenarioState.Monsters.Remove(m_DisplayedEnemyState);
			Choreographer.s_Choreographer.m_ClientEnemies.Remove(gameObject);
		}
	}

	public void OnApplyToUnitPressed()
	{
		switch (m_TypeDisplayed)
		{
		case LevelEditorUnitsPanel.UnitType.Player:
			CharacterClassManager.Classes.SingleOrDefault((CCharacterClass x) => x.ID == m_DisplayedPlayer.Class.ID);
			m_DisplayedPlayer.SetPlayerLevel(string.IsNullOrEmpty(LevelInput.text) ? 1 : int.Parse(LevelInput.text));
			m_DisplayedPlayer.MaxHealth = (string.IsNullOrEmpty(MaxHealthInput.text) ? m_DisplayedPlayer.GetMaxHealthForCharacterLevel() : int.Parse(MaxHealthInput.text));
			m_DisplayedPlayer.Health = (string.IsNullOrEmpty(HealthInput.text) ? m_DisplayedPlayer.MaxHealth : Mathf.Min(int.Parse(HealthInput.text), m_DisplayedPlayer.MaxHealth));
			ActorBehaviour.UpdateHealth(Choreographer.s_Choreographer.FindClientPlayer(m_DisplayedPlayer));
			LevelEditorController.s_Instance.SetPlayerRevealed(m_DisplayedPlayerState, RevealedToggle.isOn);
			break;
		case LevelEditorUnitsPanel.UnitType.Enemy:
		case LevelEditorUnitsPanel.UnitType.Objects:
		{
			CMonsterClass cMonsterClass = MonsterClassManager.MonsterAndObjectClasses.SingleOrDefault((CMonsterClass x) => x.ID == m_DisplayedEnemy.MonsterClass.ID);
			int num = cMonsterClass.Health();
			List<CEnemyActor> list = new List<CEnemyActor>();
			foreach (CActor allActor in ScenarioManager.Scenario.AllActors)
			{
				if (allActor.Class == cMonsterClass)
				{
					CEnemyActor cEnemyActor = allActor as CEnemyActor;
					if (cEnemyActor.MaxHealth == num)
					{
						list.Add(cEnemyActor);
					}
				}
			}
			cMonsterClass.SetMonsterStatLevel(string.IsNullOrEmpty(LevelInput.text) ? 1 : int.Parse(LevelInput.text));
			num = cMonsterClass.Health();
			foreach (CEnemyActor item in list)
			{
				item.MaxHealth = num;
				item.Health = item.MaxHealth;
				ActorBehaviour.UpdateHealth(Choreographer.s_Choreographer.FindClientEnemy(item));
			}
			m_DisplayedEnemy.MinInitialMaxHealth = (string.IsNullOrEmpty(MinInitialMaxHealthInput.text) ? (-1) : int.Parse(MinInitialMaxHealthInput.text));
			m_DisplayedEnemy.MaxHealth = (string.IsNullOrEmpty(MaxHealthInput.text) ? num : int.Parse(MaxHealthInput.text));
			m_DisplayedEnemy.Health = (string.IsNullOrEmpty(HealthInput.text) ? m_DisplayedEnemy.MaxHealth : int.Parse(HealthInput.text));
			ActorBehaviour.UpdateHealth(Choreographer.s_Choreographer.FindClientEnemy(m_DisplayedEnemy));
			LevelEditorController.s_Instance.SetEnemyRevealed(m_DisplayedEnemyState, RevealedToggle.isOn);
			if (AllyToggle.isOn)
			{
				CEnemyActor displayedEnemy = m_DisplayedEnemy;
				CActor.EType type = (m_DisplayedEnemy.OriginalType = CActor.EType.Ally);
				displayedEnemy.Type = type;
			}
			else if (Enemy2Toggle.isOn)
			{
				CEnemyActor displayedEnemy2 = m_DisplayedEnemy;
				CActor.EType type = (m_DisplayedEnemy.OriginalType = CActor.EType.Enemy2);
				displayedEnemy2.Type = type;
			}
			else if (NeutralToggle.isOn)
			{
				CEnemyActor displayedEnemy3 = m_DisplayedEnemy;
				CActor.EType type = (m_DisplayedEnemy.OriginalType = CActor.EType.Neutral);
				displayedEnemy3.Type = type;
			}
			else
			{
				CEnemyActor displayedEnemy4 = m_DisplayedEnemy;
				CActor.EType type = (m_DisplayedEnemy.OriginalType = CActor.EType.Enemy);
				displayedEnemy4.Type = type;
			}
			if (m_TypeDisplayed == LevelEditorUnitsPanel.UnitType.Enemy)
			{
				if (AllyToggle.isOn)
				{
					MoveMonsterToFrom(CActor.EType.Ally, ScenarioManager.Scenario.InitialAllyMonsters, ScenarioManager.Scenario.AllyMonsters, ScenarioManager.CurrentScenarioState.AllyMonsters, Choreographer.s_Choreographer.m_ClientAllyMonsters);
				}
				else if (Enemy2Toggle.isOn)
				{
					MoveMonsterToFrom(CActor.EType.Enemy2, ScenarioManager.Scenario.InitialEnemy2Monsters, ScenarioManager.Scenario.Enemy2Monsters, ScenarioManager.CurrentScenarioState.Enemy2Monsters, Choreographer.s_Choreographer.m_ClientEnemy2Monsters);
				}
				else if (NeutralToggle.isOn)
				{
					MoveMonsterToFrom(CActor.EType.Neutral, ScenarioManager.Scenario.InitialNeutralMonsters, ScenarioManager.Scenario.NeutralMonsters, ScenarioManager.CurrentScenarioState.NeutralMonsters, Choreographer.s_Choreographer.m_ClientNeutralMonsters);
				}
				else
				{
					MoveMonsterToFrom(CActor.EType.Enemy, ScenarioManager.Scenario.InitialEnemies, ScenarioManager.Scenario.Enemies, ScenarioManager.CurrentScenarioState.Monsters, Choreographer.s_Choreographer.m_ClientEnemies);
				}
			}
			m_DisplayedEnemyState.SetConfigForPartySize(1, (ScenarioManager.EPerPartySizeConfig)PartySizeOneConfigDropDown.value);
			m_DisplayedEnemyState.SetConfigForPartySize(2, (ScenarioManager.EPerPartySizeConfig)PartySizeTwoConfigDropDown.value);
			m_DisplayedEnemyState.SetConfigForPartySize(3, (ScenarioManager.EPerPartySizeConfig)PartySizeThreeConfigDropDown.value);
			m_DisplayedEnemyState.SetConfigForPartySize(4, (ScenarioManager.EPerPartySizeConfig)PartySizeFourConfigDropDown.value);
			break;
		}
		case LevelEditorUnitsPanel.UnitType.Prop:
			if (m_DisplayedProp is CObjectTrap cObjectTrap)
			{
				if (!string.IsNullOrEmpty(DamageInput.text))
				{
					cObjectTrap.SetCustomDamageValue(int.Parse(DamageInput.text));
				}
				else
				{
					cObjectTrap.ClearCustomDamage();
				}
				cObjectTrap.AdjacentRange = int.Parse(AdjacentRangeInput.text);
				cObjectTrap.AdjacentDamageValue = ((!string.IsNullOrEmpty(DamageInput.text)) ? int.Parse(DamageInput.text) : 0);
			}
			if (m_DisplayedProp is CObjectChest cObjectChest)
			{
				cObjectChest.Gold = ((!string.IsNullOrEmpty(GoldInput.text)) ? int.Parse(GoldInput.text) : 0);
				cObjectChest.XP = ((!string.IsNullOrEmpty(XPInput.text)) ? int.Parse(XPInput.text) : 0);
				cObjectChest.DamageValue = ((!string.IsNullOrEmpty(DamageInput.text)) ? int.Parse(DamageInput.text) : 0);
			}
			if (m_DisplayedProp is CObjectDoor cObjectDoor)
			{
				cObjectDoor.SetHasExtraLock(DoorHasExtraLockToggle.isOn);
				cObjectDoor.SetDoorApparanceLockType(CObjectDoor.LockTypes[DoorApparanceLockTypeDropDown.value]);
				cObjectDoor.SetDoorPressurePlateLockType(CObjectDoor.DoorPressurePlateLockTypes[PressurePlateTypeDropdown.value]);
				cObjectDoor.SetDoorPressurePlateLockCount(int.Parse(PressurePlateCountInput.text));
				LevelEditorController.s_Instance.SetDoorType(cObjectDoor, m_SwappableDoorTypes[DoorTypeDropdown.value]);
				LevelEditorController.s_Instance.SetDoorOpen(PropActivatedToggle?.isOn ?? cObjectDoor.Activated, cObjectDoor);
			}
			if (m_DisplayedProp is CObjectDifficultTerrain cObjectDifficultTerrain)
			{
				cObjectDifficultTerrain.TreatAsTrap = TreatDifficultTerrainAsTrapToggle != null && TreatDifficultTerrainAsTrapToggle.isOn;
				ObjectiveFilterPanel?.Apply();
				cObjectDifficultTerrain.TreatAsTrapFilter = ObjectiveFilterPanel?.FilterBeingShown.Copy();
			}
			if (m_DisplayedProp is CObjectPortal cObjectPortal)
			{
				ObjectiveFilterPanel?.Apply();
				cObjectPortal.PortalUsageFilter = ObjectiveFilterPanel?.FilterBeingShown.Copy();
			}
			if (m_DisplayedProp is CObjectPressurePlate cObjectPressurePlate)
			{
				cObjectPressurePlate.SetPressurePlateType(CObjectPressurePlate.PressurePlateTypes[PressurePlateTypeDropdown.value]);
				cObjectPressurePlate.SetActivatesOnEndTurn(PropActivatesOnEndTurnToggle.isOn);
				cObjectPressurePlate.SetSpawnedActImmediately(PressurePlateSpawnedActImmediatelyToggle.isOn);
			}
			if (m_DisplayedProp is CObjectObstacle cObjectObstacle)
			{
				cObjectObstacle.SetIgnoreFlyAndJump(ObstacleIgnoreFlyOrJumpToggle?.isOn ?? false);
				cObjectObstacle.SetBlockLOS(ObstacleBlocksLOSToggle?.isOn ?? false);
			}
			if (m_DisplayedProp.IsLootable)
			{
				m_DisplayedProp.SetIgnoreEndOfTurnLooting(IgnoreEndOfTurnLootingToggle.isOn);
				ObjectiveFilterPanel?.Apply();
				m_DisplayedProp.CanLootFilter = ObjectiveFilterPanel?.FilterBeingShown.Copy();
				m_DisplayedProp.CanLootLocKey = CustomLocInput.text;
			}
			m_DisplayedProp.PropCustomLocKey = PropCustomLocInput.text;
			m_DisplayedProp.PropCustomDescriptionLocKey = PropCustomDescriptionLocInput.text;
			m_DisplayedProp.SetConfigForPartySize(1, (ScenarioManager.EPerPartySizeConfig)PartySizeOneConfigDropDown.value);
			m_DisplayedProp.SetConfigForPartySize(2, (ScenarioManager.EPerPartySizeConfig)PartySizeTwoConfigDropDown.value);
			m_DisplayedProp.SetConfigForPartySize(3, (ScenarioManager.EPerPartySizeConfig)PartySizeThreeConfigDropDown.value);
			m_DisplayedProp.SetConfigForPartySize(4, (ScenarioManager.EPerPartySizeConfig)PartySizeFourConfigDropDown.value);
			if (m_DisplayedProp.PropHealthDetails != null)
			{
				m_DisplayedProp.PropHealthDetails.HasHealth = PropHasHealthToggle.isOn;
				PropWithHealthPanel.ApplyToProp();
			}
			m_DisplayedProp.SetOverrideDisallowMoveOrDestroy(OverrideDisallowMoveOrDestroyToggle?.isOn ?? false);
			break;
		case LevelEditorUnitsPanel.UnitType.Spawner:
			SpawnerPanel.SaveValuesToSpawner();
			break;
		default:
			Log.Error($"Apply feature for unitType:{m_TypeDisplayed} not implemented.");
			break;
		}
		UpdateUIForDisplay();
	}

	public void OnLinkPropsPressed()
	{
		m_ParentUnitsPanel.ListItemLinkPressed(m_ItemDisplayingFor);
	}

	public void OnUnlinkPropsPressed()
	{
		m_ParentUnitsPanel.ListItemUnlinkPressed(m_ItemDisplayingFor);
	}

	public void OnLocateUnitPressed()
	{
		switch (m_TypeDisplayed)
		{
		case LevelEditorUnitsPanel.UnitType.Player:
			LevelEditorController.s_Instance.ShowLocationIndicator(Choreographer.s_Choreographer.FindClientPlayer(m_DisplayedPlayer).transform.position);
			break;
		case LevelEditorUnitsPanel.UnitType.Enemy:
			LevelEditorController.s_Instance.ShowLocationIndicator(Choreographer.s_Choreographer.FindClientEnemy(m_DisplayedEnemy).transform.position);
			break;
		case LevelEditorUnitsPanel.UnitType.Objects:
			LevelEditorController.s_Instance.ShowLocationIndicator(Choreographer.s_Choreographer.FindClientObjectActor(m_DisplayedEnemy).transform.position);
			break;
		case LevelEditorUnitsPanel.UnitType.Prop:
			if (m_DisplayedProp != null && m_DisplayedProp.Position != null)
			{
				LevelEditorController.s_Instance.ShowLocationIndicator(GloomUtility.CVToV(m_DisplayedProp.Position));
			}
			break;
		case LevelEditorUnitsPanel.UnitType.Spawner:
			if (m_DisplayedSpawner.ArrayIndex != null)
			{
				CClientTile cClientTile = CAutoTileClick.TileIndexToClientTile(m_DisplayedSpawner.ArrayIndex);
				LevelEditorController.s_Instance.ShowLocationIndicator(cClientTile.m_GameObject.transform.position);
			}
			break;
		}
	}

	public void OnMoveUnitPressed()
	{
		m_ParentUnitsPanel.ListItemMovePressed(m_ItemDisplayingFor);
	}

	public void OnDeleteUnitPressed()
	{
		if (m_TypeDisplayed == LevelEditorUnitsPanel.UnitType.Prop && m_DisplayedProp != null && m_DisplayedProp is CObjectPressurePlate cObjectPressurePlate)
		{
			List<string> linkedDoorGuids = cObjectPressurePlate.LinkedDoorGuids.ToList();
			int i;
			for (i = 0; i < linkedDoorGuids.Count; i++)
			{
				CObjectProp cObjectProp = ScenarioManager.CurrentScenarioState.DoorProps.FirstOrDefault((CObjectProp p) => p.PropGuid == linkedDoorGuids[i]);
				if (cObjectProp != null && cObjectProp is CObjectDoor doorProp)
				{
					cObjectPressurePlate.UnlinkDoor(doorProp, updateDoorLockApparance: false);
				}
			}
		}
		ItemLoadoutPanel?.gameObject.SetActive(value: false);
		m_ParentUnitsPanel.ListItemRemovePressed(m_ItemDisplayingFor);
	}

	public void OnToggleUnitPressed()
	{
		if (m_TypeDisplayed == LevelEditorUnitsPanel.UnitType.Prop)
		{
			if (m_DisplayedProp is CObjectDoor doorPropToRotate)
			{
				LevelEditorController.s_Instance.TryRotateDoor(doorPropToRotate);
				UpdateUIForDisplay();
			}
		}
		else if (m_TypeDisplayed == LevelEditorUnitsPanel.UnitType.Spawner && m_DisplayedSpawner is CInteractableSpawner { Prop: not null } cInteractableSpawner)
		{
			LevelEditorController.s_Instance.TryRotateProp(cInteractableSpawner.Prop);
			UpdateUIForDisplay();
		}
	}

	public void OnChangeTreasureTableDropdown()
	{
		if (m_DisplayedProp is CObjectChest cObjectChest)
		{
			string text = ((TreasureTableDropdown.value == 0) ? string.Empty : TreasureTableDropdown.options[TreasureTableDropdown.value].text);
			if (string.IsNullOrEmpty(text))
			{
				cObjectChest.ChestTreasureTablesID = null;
				return;
			}
			cObjectChest.ChestTreasureTablesID = new List<string> { text };
			return;
		}
		throw new Exception("OnChangeTreasureTableDropdown triggered while not displaying a treasure chest");
	}

	public void OnChangeLinkedPortalDropdown()
	{
		if (m_DisplayedProp is CObjectPortal cObjectPortal)
		{
			string text = ((LinkedPortalDropdown.value == 0) ? string.Empty : LinkedPortalDropdown.options[LinkedPortalDropdown.value].text);
			if (string.IsNullOrEmpty(text))
			{
				cObjectPortal.LinkedPortalGuid = null;
			}
			else
			{
				cObjectPortal.LinkedPortalGuid = text;
			}
			return;
		}
		throw new Exception("OnChangeLinkedPortalDropdown triggered while not displaying a portal");
	}

	public void OnPropWithHealthToggleChanged(bool value)
	{
		if (value)
		{
			if (m_DisplayedProp != null)
			{
				PropWithHealthPanel?.gameObject.SetActive(value: true);
				PropWithHealthPanel?.SetPropDisplayed(m_DisplayedProp);
				StatBasedOnXPanel?.gameObject.SetActive(value: true);
				StatBasedOnXPanel?.SetShowingForPropHealth(m_DisplayedProp);
			}
		}
		else
		{
			PropWithHealthPanel?.gameObject.SetActive(value: false);
		}
	}

	public void OnApparanceDoorLockTypeChanged()
	{
		if (m_DisplayedProp == null || !(m_DisplayedProp is CObjectDoor cObjectDoor))
		{
			return;
		}
		GameObject gameObject = GameObject.Find(cObjectDoor.InstanceName);
		if (gameObject != null)
		{
			UnityGameEditorDoorProp component = gameObject.GetComponent<UnityGameEditorDoorProp>();
			if (component != null)
			{
				component.m_LockType = CObjectDoor.LockTypes[DoorApparanceLockTypeDropDown.value];
				component.GetComponent<IProceduralContent>()?.RebuildContent();
			}
		}
	}

	public void OnTreatAsTrapChanged(bool value)
	{
		if (value)
		{
			if (m_DisplayedProp != null && m_DisplayedProp is CObjectDifficultTerrain cObjectDifficultTerrain)
			{
				ObjectiveFilterPanel?.gameObject.SetActive(value: true);
				if (cObjectDifficultTerrain.TreatAsTrapFilter == null)
				{
					cObjectDifficultTerrain.TreatAsTrapFilter = new CObjectiveFilter();
				}
				ObjectiveFilterPanel?.SetShowing(cObjectDifficultTerrain.TreatAsTrapFilter, "\"Treat as Trap\" Filter");
			}
		}
		else
		{
			ObjectiveFilterPanel?.gameObject.SetActive(value: false);
		}
	}

	private void PopulatePressurePlateLinkedDoor(CObjectPressurePlate pressurePlate)
	{
		PressurePlateLinkedDoorPanel?.SetupDelegateActions(OnPressurePlateDoorItemAdded, OnPressurePlateDoorItemRemoved, OnPressurePlateDoorItemPressed);
		List<string> itemsThatCanBeAdded = (from d in ScenarioManager.CurrentScenarioState.DoorProps.Where((CObjectProp d) => d is CObjectDoor { IsDungeonEntrance: false } cObjectDoor && !cObjectDoor.IsDungeonExit).ToList()
			select d.PropGuid).ToList();
		PressurePlateLinkedDoorPanel.SetupItemsAvailableToAdd(itemsThatCanBeAdded);
		PressurePlateLinkedDoorPanel.RefreshUIWithItems(pressurePlate.LinkedDoorGuids.ToList());
	}

	private void OnPressurePlateDoorItemAdded(string doorGUID)
	{
		if (m_DisplayedProp != null && m_DisplayedProp is CObjectPressurePlate cObjectPressurePlate)
		{
			CObjectDoor cObjectDoor = (CObjectDoor)ScenarioManager.CurrentScenarioState.DoorProps.SingleOrDefault((CObjectProp y) => y.PropGuid == doorGUID);
			if (cObjectDoor != null)
			{
				cObjectPressurePlate.LinkDoor(cObjectDoor, updateDoorLockApparance: false);
			}
			PressurePlateLinkedDoorPanel.RefreshUIWithItems(cObjectPressurePlate.LinkedDoorGuids.ToList());
		}
	}

	private void OnPressurePlateDoorItemRemoved(string doorGUID, int doorIndex)
	{
		if (m_DisplayedProp != null && m_DisplayedProp is CObjectPressurePlate cObjectPressurePlate)
		{
			CObjectDoor cObjectDoor = (CObjectDoor)ScenarioManager.CurrentScenarioState.DoorProps.SingleOrDefault((CObjectProp y) => y.PropGuid == doorGUID);
			if (cObjectDoor != null)
			{
				cObjectPressurePlate.UnlinkDoor(cObjectDoor, updateDoorLockApparance: false);
			}
			PressurePlateLinkedDoorPanel.RefreshUIWithItems(cObjectPressurePlate.LinkedDoorGuids.ToList());
		}
	}

	private void OnPressurePlateDoorItemPressed(string doorGUID, int doorIndex)
	{
		if (m_DisplayedProp != null && m_DisplayedProp is CObjectPressurePlate)
		{
			CObjectDoor cObjectDoor = (CObjectDoor)ScenarioManager.CurrentScenarioState.DoorProps.SingleOrDefault((CObjectProp y) => y.PropGuid == doorGUID);
			if (cObjectDoor != null)
			{
				LevelEditorController.s_Instance.ShowLocationIndicator(GloomUtility.CVToV(cObjectDoor.Position));
			}
		}
	}

	private void PopulatePressurePlateLinkedSpawner(CObjectPressurePlate pressurePlate)
	{
		PressurePlateLinkedSpawnerPanel?.SetupDelegateActions(OnPressurePlateSpawnerItemAdded, OnPressurePlateSpawnerItemRemoved, OnPressurePlateSpawnerItemPressed);
		List<string> itemsThatCanBeAdded = (from d in ScenarioManager.CurrentScenarioState.Spawners.ToList()
			select d.SpawnerGuid).ToList();
		PressurePlateLinkedSpawnerPanel.SetupItemsAvailableToAdd(itemsThatCanBeAdded);
		PressurePlateLinkedSpawnerPanel.RefreshUIWithItems(pressurePlate.LinkedSpawnerGuids.ToList());
	}

	private void OnPressurePlateSpawnerItemAdded(string spawnerGUID)
	{
		if (m_DisplayedProp != null && m_DisplayedProp is CObjectPressurePlate cObjectPressurePlate)
		{
			CSpawner cSpawner = ScenarioManager.CurrentScenarioState.Spawners.SingleOrDefault((CSpawner y) => y.SpawnerGuid == spawnerGUID);
			if (cSpawner != null)
			{
				cObjectPressurePlate.LinkSpawner(cSpawner);
			}
			PressurePlateLinkedSpawnerPanel.RefreshUIWithItems(cObjectPressurePlate.LinkedSpawnerGuids.ToList());
		}
	}

	private void OnPressurePlateSpawnerItemRemoved(string spawnerGUID, int spawnerIndex)
	{
		if (m_DisplayedProp != null && m_DisplayedProp is CObjectPressurePlate cObjectPressurePlate)
		{
			CSpawner cSpawner = ScenarioManager.CurrentScenarioState.Spawners.SingleOrDefault((CSpawner y) => y.SpawnerGuid == spawnerGUID);
			if (cSpawner != null)
			{
				cObjectPressurePlate.UnlinkSpawner(cSpawner);
			}
			PressurePlateLinkedSpawnerPanel.RefreshUIWithItems(cObjectPressurePlate.LinkedSpawnerGuids.ToList());
		}
	}

	private void OnPressurePlateSpawnerItemPressed(string spawnerGUID, int spawnerIndex)
	{
		if (m_DisplayedProp != null && m_DisplayedProp is CObjectPressurePlate)
		{
			CSpawner cSpawner = ScenarioManager.CurrentScenarioState.Spawners.SingleOrDefault((CSpawner y) => y.SpawnerGuid == spawnerGUID);
			if (cSpawner != null)
			{
				Vector3 locationToShowAt = ((cSpawner.Prop != null) ? GloomUtility.CVToV(cSpawner.Prop.Position) : ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[cSpawner.ArrayIndex.X, cSpawner.ArrayIndex.Y].m_GameObject.transform.position);
				LevelEditorController.s_Instance.ShowLocationIndicator(locationToShowAt);
			}
		}
	}
}
