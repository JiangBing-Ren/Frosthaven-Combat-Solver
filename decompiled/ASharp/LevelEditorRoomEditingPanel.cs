using System.Collections.Generic;
using System.Linq;
using FH.ApparanceBaking;
using FH.LevelComponents.ProcGen;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorRoomEditingPanel : MonoBehaviour
{
	[Header("Panel Objects")]
	public GameObject ScenarioEditingPanel;

	public GameObject RoomDisplayPanel;

	public GameObject DoorDisplayPanel;

	public GameObject RoomListPanel;

	public GameObject DoorListPanel;

	public GameObject RoomShapePanel;

	[Header("Scenario Editing Panel")]
	public Button CompleteRoomPlacementButton;

	public Button CompleteDoorPlacementButton;

	[Header("Room List")]
	public Transform RoomContent;

	public GameObject RoomItemPrefab;

	public ConvenientDropdown AddRoomTypeDropDown;

	[Header("Door List")]
	public Transform DoorContent;

	public GameObject DoorItemPrefab;

	public TMP_Dropdown AddDoorTypeDropDown;

	[Header("Room Display Panel")]
	public TMP_InputField RoomNameField;

	public TextMeshProUGUI RoomTypeText;

	public Button DeleteButton;

	public Toggle RevealToggle;

	public TextMeshProUGUI StatusText;

	[Header("Door Display Panel")]
	public TextMeshProUGUI DoorTypeText;

	public Button DeleteDoorButton;

	public Button RotateDoorButton;

	[Header("Wall Display List")]
	public LevelEditorGenericListPanel WallPanel;

	[Header("Apparance Panel")]
	public LevelEditorApparanceOverridePanel ApparanceOverridePanel;

	[Header("Misc")]
	public GameObject ControlsBlockingObject;

	private List<LevelEditorRoomListItem> m_RoomItems = new List<LevelEditorRoomListItem>();

	private LevelEditorRoomListItem m_CurrentRoomItem;

	private List<LevelEditorDoorListItem> m_DoorItems = new List<LevelEditorDoorListItem>();

	private LevelEditorDoorListItem m_CurrentDoorItem;

	private List<ProceduralWall> m_CurrentWallItems = new List<ProceduralWall>();

	private bool m_BlockToggleEvents;

	[SerializeField]
	private ApparanceQuickReload ApparanceQuickReload;

	private void Awake()
	{
		if (!ApparanceQuickReload)
		{
			ApparanceQuickReload = Object.FindObjectOfType<ApparanceQuickReload>();
		}
		AddRoomTypeDropDown.options.Clear();
		AddRoomTypeDropDown.AddOptions((from s in CMap.MapTypes
			where s != EMapType.None
			select s.ToString()).ToList());
		AddDoorTypeDropDown.options.Clear();
		AddDoorTypeDropDown.AddOptions(ApparanceLayer.GetRealDoorTypeNames());
		CompleteRoomPlacementButton.onClick.AddListener(() =>
		{
			LevelEditorController.s_Instance.OnFinishedPlacingRooms();
			RefreshDisplayedPanelsByEditingState();
		});
		CompleteDoorPlacementButton.onClick.AddListener(() =>
		{
			LevelEditorController.s_Instance.OnBuildTileMap();
			RefreshDisplayedPanelsByEditingState();
			LevelEditorController.s_Instance.m_LevelEditorUIInstance.ApparancePanel.EnsureAllStylesHaveAnOverride();
			ApparanceQuickReload.FullReload();
		});
	}

	private void OnEnable()
	{
		WallPanel?.gameObject.SetActive(value: false);
		RefreshDisplayedPanelsByEditingState();
		RefreshRoomUIWithCurrentState();
		RefreshDoorUIWithCurrentState();
	}

	private void RefreshDisplayedPanelsByEditingState()
	{
		switch (LevelEditorController.s_Instance.CurrentLevelEditingState)
		{
		case LevelEditorController.ELevelEditingState.RoomPlacement:
			ScenarioEditingPanel.SetActive(value: true);
			RoomDisplayPanel.SetActive(value: true);
			DoorDisplayPanel.SetActive(value: false);
			RoomListPanel.SetActive(value: true);
			DoorListPanel.SetActive(value: false);
			RoomShapePanel.SetActive(value: true);
			WallPanel?.gameObject.SetActive(value: false);
			CompleteRoomPlacementButton.gameObject.SetActive(value: true);
			CompleteDoorPlacementButton.gameObject.SetActive(value: false);
			ControlsBlockingObject.SetActive(value: false);
			break;
		case LevelEditorController.ELevelEditingState.DoorPlacement:
			ScenarioEditingPanel.SetActive(value: true);
			RoomDisplayPanel.SetActive(value: false);
			DoorDisplayPanel.SetActive(value: true);
			RoomListPanel.SetActive(value: false);
			DoorListPanel.SetActive(value: true);
			RoomShapePanel.SetActive(value: false);
			WallPanel?.gameObject.SetActive(value: false);
			CompleteRoomPlacementButton.gameObject.SetActive(value: false);
			CompleteDoorPlacementButton.gameObject.SetActive(value: true);
			ControlsBlockingObject.SetActive(value: false);
			break;
		case LevelEditorController.ELevelEditingState.PropActorPlacement:
			ScenarioEditingPanel.SetActive(value: false);
			RoomDisplayPanel.SetActive(value: false);
			DoorDisplayPanel.SetActive(value: false);
			RoomListPanel.SetActive(value: true);
			DoorListPanel.SetActive(value: false);
			RoomShapePanel.SetActive(value: false);
			CompleteRoomPlacementButton.gameObject.SetActive(value: false);
			CompleteDoorPlacementButton.gameObject.SetActive(value: false);
			DeleteButton.gameObject.SetActive(value: false);
			ControlsBlockingObject.SetActive(value: true);
			break;
		}
	}

	public void RefreshRoomUIWithCurrentState()
	{
		foreach (LevelEditorRoomListItem roomItem in m_RoomItems)
		{
			Object.Destroy(roomItem.gameObject);
		}
		m_RoomItems.Clear();
		foreach (CMap map in ScenarioManager.CurrentScenarioState.Maps)
		{
			AddRoomItemToUI(map);
		}
		RoomDisplayPanel.SetActive(value: false);
		ApparanceOverridePanel?.gameObject.SetActive(value: false);
	}

	private LevelEditorRoomListItem AddRoomItemToUI(CMap mapToAddFor)
	{
		LevelEditorRoomListItem component = Object.Instantiate(RoomItemPrefab, RoomContent).GetComponent<LevelEditorRoomListItem>();
		component.Init(mapToAddFor, OnEditRoomButtonPressed);
		m_RoomItems.Add(component);
		return component;
	}

	public void RefreshDoorUIWithCurrentState()
	{
		foreach (LevelEditorDoorListItem doorItem in m_DoorItems)
		{
			Object.Destroy(doorItem.gameObject);
		}
		m_DoorItems.Clear();
		foreach (CObjectDoor doorProp in ScenarioManager.CurrentScenarioState.DoorProps)
		{
			AddDoorItemToUI(doorProp);
		}
		RoomDisplayPanel.SetActive(value: false);
		ApparanceOverridePanel?.gameObject.SetActive(value: false);
	}

	private LevelEditorDoorListItem AddDoorItemToUI(CObjectDoor doorToAddFor)
	{
		LevelEditorDoorListItem component = Object.Instantiate(DoorItemPrefab, DoorContent).GetComponent<LevelEditorDoorListItem>();
		component.Init(doorToAddFor, OnEditDoorButtonPressed);
		m_DoorItems.Add(component);
		return component;
	}

	private void SetupWallListForRoom(CMap map)
	{
		GameObject gameObject = GameObject.Find(map.MapInstanceName);
		m_CurrentWallItems = gameObject.GetComponentsInChildren<ProceduralWall>().ToList();
		WallPanel?.RefreshUIWithItems(m_CurrentWallItems.Select((ProceduralWall w) => w.gameObject.name).ToList());
		WallPanel?.SetupDelegateActions(null, null, OnWallItemPressed);
	}

	public void OnEditRoomButtonPressed(LevelEditorRoomListItem viewItem)
	{
		m_CurrentRoomItem = viewItem;
		StatusText.text = m_CurrentRoomItem.MapItem.MapGuid;
		RoomNameField.text = m_CurrentRoomItem.MapItem.RoomName;
		RoomTypeText.text = m_CurrentRoomItem.MapItem.MapType.ToString();
		m_BlockToggleEvents = true;
		RevealToggle.isOn = m_CurrentRoomItem.MapItem.Revealed;
		m_BlockToggleEvents = false;
		if (string.IsNullOrEmpty(LevelEditorController.s_Instance.CanRoomBeDeleted(m_CurrentRoomItem.MapItem)))
		{
			DeleteButton.interactable = true;
			StatusText.text = m_CurrentRoomItem.MapItem.MapGuid;
		}
		else
		{
			DeleteButton.interactable = false;
			StatusText.text = m_CurrentRoomItem.MapItem.MapGuid;
		}
		RoomDisplayPanel.SetActive(value: true);
		ApparanceOverridePanel?.gameObject.SetActive(value: true);
		ApparanceOverridePanel?.SetRoomDisplayed(m_CurrentRoomItem.MapItem);
		RoomShapePanel.SetActive(value: false);
		WallPanel?.gameObject.SetActive(LevelEditorController.s_Instance.CurrentLevelEditingState == LevelEditorController.ELevelEditingState.PropActorPlacement);
		SetupWallListForRoom(m_CurrentRoomItem.MapItem);
		GameObject gameObject = GameObject.Find(m_CurrentRoomItem.MapItem.MapInstanceName);
		if (gameObject != null)
		{
			LevelEditorController.s_Instance.ShowLocationIndicator(gameObject.transform.position);
		}
	}

	public void OnEditDoorButtonPressed(LevelEditorDoorListItem viewItem)
	{
		m_CurrentDoorItem = viewItem;
		StatusText.text = m_CurrentDoorItem.DoorItem.StartingMapGuid;
		DoorTypeText.text = m_CurrentDoorItem.DoorItem.DoorType.ToString();
		if (LevelEditorController.s_Instance.CanDoorBeDeleted())
		{
			DeleteButton.interactable = true;
			StatusText.text = m_CurrentDoorItem.DoorItem.StartingMapGuid;
		}
		else
		{
			DeleteButton.interactable = false;
			StatusText.text = m_CurrentDoorItem.DoorItem.StartingMapGuid;
		}
		ApparanceOverridePanel?.gameObject.SetActive(value: false);
		WallPanel?.gameObject.SetActive(value: false);
		DoorDisplayPanel.SetActive(value: true);
	}

	private void OnWallItemPressed(string itemDescPressed, int indexPressed)
	{
		ProceduralWall proceduralWall = m_CurrentWallItems[indexPressed];
		Vector3 locationToShowAt = (proceduralWall.LeftCorner.Position + proceduralWall.RightCorner.Position) / 2f;
		LevelEditorController.s_Instance.ShowLocationIndicator(locationToShowAt);
		ApparanceOverridePanel?.gameObject.SetActive(value: true);
		ApparanceOverridePanel?.SetWallDisplayed(m_CurrentRoomItem.MapItem, indexPressed);
	}

	public void OnButtonAddNewRoomClicked()
	{
		EMapType mapType = CMap.MapTypes.SingleOrDefault((EMapType s) => s.ToString() == AddRoomTypeDropDown.options[AddRoomTypeDropDown.value].text);
		LevelEditorController.s_Instance.AddRoomOfType(mapType);
	}

	public void OnButtonDeleteRoomClicked()
	{
		LevelEditorController.s_Instance.TryDeleteRoom(m_CurrentRoomItem.MapItem);
	}

	public void OnRoomToggleVisibilityClicked()
	{
		LevelEditorController.s_Instance.ToggleRoomVisibility(m_CurrentRoomItem.MapItem);
	}

	public void OnButtonAddNewDoorClicked()
	{
		CObjectDoor.EDoorType doorType = CObjectDoor.DoorTypes.SingleOrDefault((CObjectDoor.EDoorType s) => s.ToString() == AddDoorTypeDropDown.options[AddDoorTypeDropDown.value].text);
		LevelEditorController.s_Instance.AddDoorOfType(doorType);
	}

	public void OnButtonDeleteDoorClicked()
	{
		LevelEditorController.s_Instance.TryDeleteDoor(m_CurrentDoorItem.DoorItem);
	}

	public void OnButtonRotateDoorClicked()
	{
		LevelEditorController.s_Instance.TryRotateDoor(m_CurrentDoorItem.DoorItem);
	}

	public void OnRevealToggled(bool toggleValue)
	{
		if (!m_BlockToggleEvents && m_CurrentRoomItem != null)
		{
			LevelEditorController.s_Instance.SetRoomRevealed(m_CurrentRoomItem.MapItem, toggleValue);
		}
	}

	public void OnRoomNameChanged(string newValue)
	{
		bool flag = true;
		foreach (CMap map in ScenarioManager.CurrentScenarioState.Maps)
		{
			if (map != m_CurrentRoomItem.MapItem && map.RoomName == RoomNameField.text)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			m_CurrentRoomItem.MapItem.RoomName = RoomNameField.text;
			foreach (string childMapGuid in m_CurrentRoomItem.MapItem.Children)
			{
				ScenarioManager.CurrentScenarioState.Maps.Single((CMap x) => x.MapGuid == childMapGuid).ParentName = m_CurrentRoomItem.MapItem.RoomName;
			}
			RefreshRoomUIWithCurrentState();
			OnEditRoomButtonPressed(m_CurrentRoomItem);
		}
		else
		{
			RoomNameField.text = m_CurrentRoomItem.MapItem.RoomName;
		}
	}
}
