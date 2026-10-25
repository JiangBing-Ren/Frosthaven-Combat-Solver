using System.Collections.Generic;
using System.Reflection;
using FH.YML;
using FHGUI.DTO.Outpost;
using FHGUI.Elements;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Testing;

public class TestNotifications : MonoBehaviour
{
	[Header("Toast")]
	public TMP_InputField ToastText;

	public Button ToastButton;

	public ToastNotificationsContainer ToastComponent;

	[Header("Event Log")]
	public Button OpenEventLog;

	public OutpostEventLogModule EventLogModule;

	public OutpostNotificationModule NotificationModule;

	[Header("Event Log Input")]
	public TMP_Dropdown ItemDropdown;

	public Button NewItemButon;

	public TMP_Dropdown CraftingDropdown;

	public Button NewCraftingButon;

	public TMP_Dropdown BuildingDropdown;

	public Button NewBuildingButon;

	public TMP_Dropdown SectionDropdown;

	public Button NewSectionButton;

	public uint CurrentWeek = 1u;

	private OutpostEventLogState _eventLogData = new OutpostEventLogState();

	private OutpostNotificationViewService _service;

	private List<OutpostNotificationDTO> _notifications = new List<OutpostNotificationDTO>();

	private List<string> _notificationPlayerData = new List<string>();

	private UIOutpostNotificationElement.OutpostNotificationParams _params;

	private List<OutpostEventLogState.EventLogData> Events;

	private MethodInfo _addToEventLog;

	private List<ItemDef> _items;

	private List<ItemDef> _blueprints;

	private List<BuildingTypeDef> _buildings;

	private List<CampaignSectionDef> _sections;

	public CampaignState _state;

	public DefRepository DefRepo => YMLDefRepository.Repo.DefRepository;
}
