using System;
using System.Globalization;
using System.Threading.Tasks;
using FH.ApparanceBaking;
using LevelEditor;
using Libs.GameConsole;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorUI : MonoBehaviour
{
	[Header("Main Components")]
	public Canvas ThisCanvas;

	public TextMeshProUGUI MenuTitle;

	public GameObject MainPanel;

	public LevelEditorConfirmDialog ConfirmationDialog;

	[Header("CardUI")]
	public GameObject AbilityCardUI;

	public MonsterBaseUI MonsterCardUI;

	private RectTransform m_MainPanelRect;

	[Header("Bar Menu Panels")]
	public LevelEditorStandardDataPanel LevelDataPanel;

	public LevelEditorRoomEditingPanel RoomPanel;

	public LevelEditorUnitsPanel PlayerPanel;

	public LevelEditorUnitsPanel SpanwerPanel;

	public LevelEditorUnitsPanel EnemyPanel;

	public LevelEditorUnitsPanel ObjectsPanel;

	public LevelEditorUnitsPanel PropPanel;

	public LevelEditorApparancePanel ApparancePanel;

	public LevelEditorObjectivesPanel ObjectivesPanel;

	public LevelEditorMessagesPanel MessagesPanel;

	public LevelEditorEventsPanel EventsPanel;

	public LevelEditorAutoTestDataPanel AutoTestDataPanel;

	public LevelEditorTabTogglesController TabTogglesController;

	[Header("Panel Toggles")]
	public Toggle LevelDataPanelToggle;

	public Toggle RoomPanelToggle;

	public Toggle CharacterPanelToggle;

	public Toggle EnemyPanelToggle;

	public Toggle PropPanelToggle;

	public Toggle ApparancePanelToggle;

	public Toggle ObjectivePanelToggle;

	public Toggle EventsPanelToggle;

	public Toggle MessagesPanelToggle;

	public Toggle TutorialPanelToggle;

	public Toggle AutoTestPanelToggle;

	private CanvasGroup _mainPanelCanvasGroup;

	[ConsoleVariable(Alias = "fade_editor_ui", Persistent = false)]
	public static bool FadeEditorUI = true;

	private void Awake()
	{
		MenuTitle.text = "";
		m_MainPanelRect = MainPanel.GetComponent<RectTransform>();
		_mainPanelCanvasGroup = MainPanel.GetComponent<CanvasGroup>();
	}

	private void Start()
	{
		InitUI();
	}

	private void Update()
	{
	}

	private async Task InitUI()
	{
		ThisCanvas.worldCamera = UIManager.Instance.UICamera;
		ApparancePanel.InitUI(ScenarioManager.CurrentScenarioState);
		LevelDataPanel.InitUIForLoadedLevel();
		MessagesPanel.RefreshUIWithLoadedLevel();
		EventsPanel.RefreshUIWithLoadedLevel();
		if (LevelEditorController.s_Instance.AutoTestNeedsSaving)
		{
			AutoTestPanelToggle.isOn = true;
		}
		else if (ScenarioManager.CurrentScenarioState.Maps == null || ScenarioManager.CurrentScenarioState.Maps.Count == 0)
		{
			RoomPanelToggle.isOn = true;
		}
		await AutoTestDataPanel.InitFromAutoTest(SaveData.Instance.Global.CurrentEditorAutoTestData);
		ApparanceQuickReload[] array = UnityEngine.Object.FindObjectsOfType<ApparanceQuickReload>(includeInactive: true);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Init();
		}
		LevelEditorApparanceDirtyUI[] array2 = UnityEngine.Object.FindObjectsOfType<LevelEditorApparanceDirtyUI>(includeInactive: true);
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].Init();
		}
	}

	public void ObjectBeingPlaced(string UnitID, int currentTileNumber = 1, int totalNumberOfTiles = 1)
	{
		SetMinimised(isMinimised: true);
		if (totalNumberOfTiles > 1)
		{
			MenuTitle.text = "Select an empty tile to place " + UnitID + " (" + currentTileNumber + " of " + totalNumberOfTiles + ")";
		}
		else
		{
			MenuTitle.text = "Select an empty tile to place " + UnitID;
		}
	}

	public void ObjectBeingLinked(string UnitID, int currentTileNumber = 1, int totalNumberOfTiles = 1)
	{
		SetMinimised(isMinimised: true);
		if (totalNumberOfTiles > 1)
		{
			MenuTitle.text = "Select a prop to link " + UnitID + " (" + currentTileNumber + " of " + totalNumberOfTiles + ")";
		}
		else
		{
			MenuTitle.text = "Select a prop to link " + UnitID;
		}
	}

	public void ObjectBeingUnlinked(string UnitID, int currentTileNumber = 1, int totalNumberOfTiles = 1)
	{
		SetMinimised(isMinimised: true);
		if (totalNumberOfTiles > 1)
		{
			MenuTitle.text = "Select a prop to unlink " + UnitID + " (" + currentTileNumber + " of " + totalNumberOfTiles + ")";
		}
		else
		{
			MenuTitle.text = "Select a prop to unlink " + UnitID;
		}
	}

	public void RoomBeingPlaced(string roomType)
	{
		SetMinimised(isMinimised: true);
		MenuTitle.text = "Left click to place a room of type: " + roomType + ", right click to undo";
	}

	public void DoorBeingPlaced(string doorType)
	{
		SetMinimised(isMinimised: true);
		MenuTitle.text = "Left click to place a door of type: " + doorType + ", right click to undo";
	}

	public void TileBeingSelected()
	{
		SetMinimised(isMinimised: true);
		MenuTitle.text = "Select a tile";
	}

	public void FinishedPlacingUnit(string UnitName, CClientTile tilePlacedIn, LevelEditorUnitsPanel.UnitType function)
	{
		switch (function)
		{
		case LevelEditorUnitsPanel.UnitType.Enemy:
			EnemyPanel.SuccessfullyPlacedUnitInHex(UnitName, tilePlacedIn);
			break;
		case LevelEditorUnitsPanel.UnitType.Objects:
			ObjectsPanel.SuccessfullyPlacedUnitInHex(UnitName, tilePlacedIn);
			break;
		case LevelEditorUnitsPanel.UnitType.Player:
			PlayerPanel.SuccessfullyPlacedUnitInHex(UnitName, tilePlacedIn);
			break;
		case LevelEditorUnitsPanel.UnitType.Prop:
			PropPanel.SuccessfullyPlacedUnitInHex(UnitName, tilePlacedIn);
			break;
		case LevelEditorUnitsPanel.UnitType.Spawner:
			SpanwerPanel.SuccessfullyPlacedUnitInHex(UnitName, tilePlacedIn);
			break;
		}
		MessagesPanel.RefreshTriggerPanelsAfterUnitListChange();
		FinishedAction();
	}

	public void FinishDeletingUnit()
	{
		MessagesPanel.RefreshTriggerPanelsAfterUnitListChange();
	}

	public void FinishedMovingUnit()
	{
		FinishedAction();
	}

	public void FailedToEditObject(LevelEditorUnitsPanel.UnitType function)
	{
		switch (function)
		{
		case LevelEditorUnitsPanel.UnitType.Enemy:
			EnemyPanel.FailedToPlaceUnitInHex();
			break;
		case LevelEditorUnitsPanel.UnitType.Objects:
			ObjectsPanel.FailedToPlaceUnitInHex();
			break;
		case LevelEditorUnitsPanel.UnitType.Player:
			PlayerPanel.FailedToPlaceUnitInHex();
			break;
		case LevelEditorUnitsPanel.UnitType.Spawner:
			SpanwerPanel.FailedToPlaceUnitInHex();
			break;
		case LevelEditorUnitsPanel.UnitType.Prop:
			PropPanel.FailedToPlaceUnitInHex();
			break;
		}
		FinishedAction();
	}

	public void FinishedAction()
	{
		SetMinimised(isMinimised: false);
		MenuTitle.text = "";
	}

	public void SetMinimised(bool isMinimised)
	{
		if (isMinimised)
		{
			m_MainPanelRect.anchorMax = new Vector2(1f, 0.2f);
		}
		else
		{
			m_MainPanelRect.anchorMax = new Vector2(1f, 0.35f);
		}
	}

	public static string FormatFHName(string displayName)
	{
		if (!displayName.StartsWith("FH/"))
		{
			return displayName;
		}
		TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
		displayName = displayName.Replace("FH/", "").Replace("character_ability_cards_", "", StringComparison.OrdinalIgnoreCase).Replace("monster_ability_cards_", "", StringComparison.OrdinalIgnoreCase)
			.Replace("item_cards_", "", StringComparison.OrdinalIgnoreCase)
			.Replace("boss_stat_cards_", "", StringComparison.OrdinalIgnoreCase)
			.Replace("boss_ability_cards_", "", StringComparison.OrdinalIgnoreCase)
			.Replace("pet_cards_", "", StringComparison.OrdinalIgnoreCase)
			.Replace("_", " ");
		string[] names = Enum.GetNames(typeof(ECharacter));
		foreach (string oldValue in names)
		{
			displayName = displayName.Replace(oldValue, "", StringComparison.OrdinalIgnoreCase);
		}
		displayName = displayName.Replace("  ", " ");
		displayName = textInfo.ToTitleCase(displayName);
		if (displayName.EndsWith(" 1"))
		{
			displayName = displayName.TrimEnd(' ', '1');
		}
		return displayName;
	}
}
