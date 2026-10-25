using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorEventTriggerPanel : MonoBehaviour
{
	[Header("Config")]
	public bool IsDismissTrigger;

	[Header("UI Elements")]
	public Toggle EventIsButtonPressToggle;

	[Space]
	public LayoutElement MainTypeToggleGroupElement;

	public Toggle IsSEventTypeToggle;

	public Toggle IsUIEventTypeToggle;

	[Space]
	public LayoutElement EventTypeElement;

	public TMP_Dropdown EventTypeDropDown;

	[Space]
	public LayoutElement EventSubTypeElement;

	public TextMeshProUGUI EventSubTypeLabel;

	public TMP_Dropdown EventSubTypeDropDown;

	[Space]
	public LayoutElement EventActorElement;

	public TMP_Dropdown EventActorDropDown;

	[Space]
	public LayoutElement EventContextTypeElement;

	public TextMeshProUGUI EventContextTypeLabel;

	public TMP_Dropdown EventContextTypeDropDown;

	[Space]
	public LayoutElement EventContextSubTypeElement;

	public TextMeshProUGUI EventContextSubTypeLabel;

	public TMP_Dropdown EventContextSubTypeDropDown;

	[Space]
	public LayoutElement EventRoundElement;

	public InputField EventRoundInput;

	[Space]
	public LayoutElement EventContextualElement;

	public TextMeshProUGUI EventContextualLabel;

	public InputField EventContextualInput;

	[Space]
	public LayoutElement EventContextualDropDownElement;

	public TextMeshProUGUI EventContextualDropDownLabel;

	public TMP_Dropdown EventContextualDropDownInput;

	[Space]
	public LayoutElement EventContextualIndexElement;

	public TextMeshProUGUI EventContextualIndexLabel;

	public InputField EventContextualIndexInput;

	[Space]
	public InputField EventPlayedMessageInput;

	public InputField EventNotPlayedMessageInput;

	public ToggleButton EventPlayedMessageLogicToggle;

	public ToggleButton EventNotPlayedMessageLogicToggle;

	[Space]
	public LayoutElement TileIndexElement;

	public TextMeshProUGUI TileIndexParameterLabel;

	[Space]
	public LevelEditorObjectiveFilterPanel FilterPanel;

	[Space]
	public LayoutElement EventAliveActorElement;

	public TMP_Dropdown EventAliveActorDropDown;

	[HideInInspector]
	public CLevelTrigger CurrentTrigger;

	private ESEType[] EventTypes = (ESEType[])Enum.GetValues(typeof(ESEType));

	private ESESubTypeInternal[] InternalSubTypes = (ESESubTypeInternal[])Enum.GetValues(typeof(ESESubTypeInternal));

	private ESESubTypePhase[] PhaseSubTypes = (ESESubTypePhase[])Enum.GetValues(typeof(ESESubTypePhase));

	private ESESubTypeAbility[] AbilitySubTypes = (ESESubTypeAbility[])Enum.GetValues(typeof(ESESubTypeAbility));

	private ESESubTypeActor[] ActorSubTypes = (ESESubTypeActor[])Enum.GetValues(typeof(ESESubTypeActor));

	private ESESubTypeObjectProp[] ObjectPropSubTypes = (ESESubTypeObjectProp[])Enum.GetValues(typeof(ESESubTypeObjectProp));

	private ESESubTypeAction[] ActionSubTypes = (ESESubTypeAction[])Enum.GetValues(typeof(ESESubTypeAction));

	private ESESubTypeItem[] ItemSubTypes = (ESESubTypeItem[])Enum.GetValues(typeof(ESESubTypeItem));

	private CPhase.PhaseType[] PhaseTypes = (CPhase.PhaseType[])Enum.GetValues(typeof(CPhase.PhaseType));

	private CAbility.EAbilityType[] AbilityTypes = (CAbility.EAbilityType[])Enum.GetValues(typeof(CAbility.EAbilityType));

	private CAbilityMove.EMoveState[] AbilityMoveStates = (CAbilityMove.EMoveState[])Enum.GetValues(typeof(CAbilityMove.EMoveState));

	private CAbilityAttack.EAttackState[] AbilityAttackStates = (CAbilityAttack.EAttackState[])Enum.GetValues(typeof(CAbilityAttack.EAttackState));

	private UIEvent.EUIEventType[] UIEventTypes = (UIEvent.EUIEventType[])Enum.GetValues(typeof(UIEvent.EUIEventType));

	private List<CActor> m_ActorOptions = new List<CActor>();

	private List<CObjective> m_ObjectiveOptions = new List<CObjective>();

	private List<CObjectProp> m_PropOptions = new List<CObjectProp>();

	private bool m_PreventToggleEvents;

	public void InitForMessageTrigger(CLevelTrigger trigger)
	{
		CurrentTrigger = trigger;
		if (CurrentTrigger != null)
		{
			m_PreventToggleEvents = true;
			if (IsDismissTrigger)
			{
				EventIsButtonPressToggle.isOn = trigger.IsTriggeredByDismiss;
			}
			else
			{
				EventPlayedMessageInput.text = trigger.EventTriggerPlayedMessageReq;
				EventNotPlayedMessageInput.text = trigger.EventTriggerNotPlayedMessageReq;
				EventPlayedMessageLogicToggle.IsOn = trigger.EventTriggerPlayedMessageReqOR;
				EventNotPlayedMessageLogicToggle.IsOn = trigger.EventTriggerNotPlayedMessageReqOR;
			}
			if (trigger.EventFilter == null)
			{
				trigger.EventFilter = new CObjectiveFilter();
			}
			FilterPanel?.SetShowing(trigger.EventFilter, "Event trigger Filter");
			IsSEventTypeToggle.isOn = !CurrentTrigger.IsUIEventTypeTrigger;
			IsUIEventTypeToggle.isOn = CurrentTrigger.IsUIEventTypeTrigger;
			m_PreventToggleEvents = false;
			FillEventTypeDropDown();
			FillActorNameDropDown();
			EventRoundInput.text = CurrentTrigger.EventTriggerRound.ToString();
			EventContextualInput.text = CurrentTrigger.EventTriggerContextId;
			EventContextualIndexInput.text = CurrentTrigger.EventTriggerContextIndex.ToString();
			OnTypeDropDownValueChanged(EventTypeDropDown.value);
		}
	}

	private void FillEventTypeDropDown()
	{
		EventTypeDropDown.options.Clear();
		if (!CurrentTrigger.IsUIEventTypeTrigger)
		{
			EventTypeDropDown.AddOptions(EventTypes.Select((ESEType s) => s.ToString()).ToList());
			EventTypeDropDown.value = Mathf.Clamp(CurrentTrigger.EventTriggerTypeInt, 0, EventTypes.Length - 1);
		}
		else
		{
			EventTypeDropDown.AddOptions(UIEventTypes.Select((UIEvent.EUIEventType s) => s.ToString()).ToList());
			EventTypeDropDown.value = Mathf.Clamp(CurrentTrigger.EventTriggerTypeInt, 0, UIEventTypes.Length - 1);
		}
	}

	public void FillActorNameDropDown()
	{
		EventActorDropDown.options.Clear();
		EventAliveActorDropDown.options.Clear();
		if (CurrentTrigger != null)
		{
			m_ActorOptions.Clear();
			m_ActorOptions.AddRange(ScenarioManager.Scenario.PlayerActors);
			m_ActorOptions.AddRange(ScenarioManager.Scenario.AllAliveMonsters);
			List<string> list = new List<string> { "NONE" };
			list.AddRange(m_ActorOptions.Select(LevelEditorUtils.FormatActorOption));
			EventActorDropDown.AddOptions(list);
			EventAliveActorDropDown.AddOptions(list);
			int b = m_ActorOptions.FindIndex((CActor a) => a.GetPrefabName() == CurrentTrigger.EventTriggerActorName && a.ActorGuid == CurrentTrigger.EventTriggerActorGuid) + 1;
			EventActorDropDown.value = Mathf.Max(0, b);
			b = m_ActorOptions.FindIndex((CActor a) => a.ActorGuid == CurrentTrigger.EventAliveActorGUID) + 1;
			EventAliveActorDropDown.value = Mathf.Max(0, b);
		}
	}

	private void FillContextTextOrDropDown(UIEvent.EUIEventType eventType)
	{
		switch (eventType)
		{
		case UIEvent.EUIEventType.RoomRevealed:
		{
			EventContextualDropDownInput.options.Clear();
			List<string> list = ScenarioManager.CurrentScenarioState.Maps.Select((CMap m) => m.MapGuid).ToList();
			EventContextualDropDownInput.AddOptions(list);
			int num2 = list.IndexOf(CurrentTrigger.EventTriggerContextId);
			EventContextualDropDownInput.value = ((num2 != -1) ? num2 : 0);
			break;
		}
		case UIEvent.EUIEventType.ObjectiveCompleted:
		case UIEvent.EUIEventType.ObjectiveActivated:
		{
			m_ObjectiveOptions.Clear();
			m_ObjectiveOptions.AddRange(ScenarioManager.CurrentScenarioState.LoseObjectives.Where((CObjective o) => !string.IsNullOrEmpty(o.EventIdentifier)).ToList());
			m_ObjectiveOptions.AddRange(ScenarioManager.CurrentScenarioState.WinObjectives.Where((CObjective o) => !string.IsNullOrEmpty(o.EventIdentifier)).ToList());
			EventContextualDropDownInput.options.Clear();
			EventContextualDropDownInput.AddOptions(m_ObjectiveOptions.Select((CObjective o) => o.EventIdentifier).ToList());
			int num = m_ObjectiveOptions.FindIndex((CObjective o) => o.EventIdentifier == CurrentTrigger.EventTriggerContextId);
			EventContextualDropDownInput.value = ((num != -1) ? num : 0);
			break;
		}
		case UIEvent.EUIEventType.PropActivated:
		case UIEvent.EUIEventType.PropDestroyed:
		case UIEvent.EUIEventType.PropDeactivated:
			m_PropOptions.Clear();
			m_PropOptions.AddRange(ScenarioManager.CurrentScenarioState.Props);
			m_PropOptions.AddRange(ScenarioManager.CurrentScenarioState.ActivatedProps);
			LevelEditorUtils.SetFormattedOptions(EventContextualDropDownInput, m_PropOptions, CurrentTrigger.EventTriggerContextId);
			break;
		case UIEvent.EUIEventType.DoorUnlocked:
			m_PropOptions.Clear();
			m_PropOptions.AddRange(ScenarioManager.CurrentScenarioState.DoorProps);
			LevelEditorUtils.SetFormattedOptions(EventContextualDropDownInput, m_PropOptions, CurrentTrigger.EventTriggerContextId);
			break;
		default:
			EventContextualDropDownInput.options.Clear();
			break;
		}
	}

	public void FillAbilityContextSubTypeDropDown()
	{
		EventContextSubTypeDropDown.options.Clear();
		CAbility.EAbilityType eAbilityType = AbilityTypes[EventContextTypeDropDown.value];
		if (eAbilityType == CAbility.EAbilityType.Move || eAbilityType == CAbility.EAbilityType.Attack)
		{
			EventContextSubTypeElement.gameObject.SetActive(value: true);
			if (eAbilityType == CAbility.EAbilityType.Move)
			{
				EventContextSubTypeLabel.text = "Move State";
				EventContextSubTypeDropDown.AddOptions(AbilityMoveStates.Select((CAbilityMove.EMoveState s) => s.ToString()).ToList());
			}
			else
			{
				EventContextSubTypeLabel.text = "Attack State";
				EventContextSubTypeDropDown.AddOptions(AbilityAttackStates.Select((CAbilityAttack.EAttackState s) => s.ToString()).ToList());
			}
			EventContextSubTypeDropDown.value = CurrentTrigger.EventTriggerContextSubTypeInt;
		}
		else
		{
			EventContextSubTypeElement.gameObject.SetActive(value: false);
		}
	}

	public void OnTypeDropDownValueChanged(int value)
	{
		if (CurrentTrigger.IsTriggeredByDismiss)
		{
			MainTypeToggleGroupElement.gameObject.SetActive(value: false);
			EventTypeElement.gameObject.SetActive(value: false);
			EventSubTypeElement.gameObject.SetActive(value: false);
			EventActorElement.gameObject.SetActive(value: false);
			EventContextTypeElement.gameObject.SetActive(value: false);
			EventContextSubTypeElement.gameObject.SetActive(value: false);
			EventRoundElement.gameObject.SetActive(value: false);
			EventContextualElement.gameObject.SetActive(value: false);
			EventContextualIndexElement.gameObject.SetActive(value: false);
			TileIndexElement.gameObject.SetActive(value: false);
			EventAliveActorElement.gameObject.SetActive(value: false);
		}
		else
		{
			MainTypeToggleGroupElement.gameObject.SetActive(value: true);
			EventTypeElement.gameObject.SetActive(value: true);
			EventActorElement.gameObject.SetActive(value: true);
			EventAliveActorElement.gameObject.SetActive(value: true);
			EventContextualDropDownElement.gameObject.SetActive(value: false);
			if (!CurrentTrigger.IsUIEventTypeTrigger)
			{
				ESEType eventType = EventTypes[value];
				DisplayForSEventType(eventType);
			}
			else
			{
				UIEvent.EUIEventType eventType2 = UIEventTypes[value];
				DisplayForUIEventType(eventType2);
			}
		}
	}

	public void OnContextTypeDropDownValueChanged(int value)
	{
		if (EventTypeDropDown.value >= 0 && EventTypeDropDown.value < EventTypes.Length && EventTypes[EventTypeDropDown.value] == ESEType.Ability)
		{
			FillAbilityContextSubTypeDropDown();
		}
	}

	private void DisplayForSEventType(ESEType eventType)
	{
		EventRoundElement.gameObject.SetActive(value: true);
		TileIndexElement.gameObject.SetActive(value: false);
		EventContextualIndexElement.gameObject.SetActive(value: false);
		FilterPanel?.gameObject.SetActive(value: false);
		EventContextTypeLabel.color = Color.white;
		EventContextTypeLabel.fontStyle = FontStyles.Normal;
		EventSubTypeDropDown.onValueChanged.RemoveAllListeners();
		switch (eventType)
		{
		case ESEType.CustomSEvent:
			EventSubTypeElement.gameObject.SetActive(value: false);
			EventContextTypeElement.gameObject.SetActive(value: false);
			EventContextSubTypeElement.gameObject.SetActive(value: false);
			EventRoundElement.gameObject.SetActive(value: false);
			EventContextualElement.gameObject.SetActive(value: true);
			break;
		case ESEType.Internal:
			EventSubTypeDropDown.options.Clear();
			EventSubTypeDropDown.AddOptions(InternalSubTypes.Select((ESESubTypeInternal s) => s.ToString()).ToList());
			EventSubTypeDropDown.value = CurrentTrigger.EventTriggerSubTypeInt;
			EventSubTypeElement.gameObject.SetActive(value: true);
			EventContextTypeElement.gameObject.SetActive(value: false);
			EventContextualElement.gameObject.SetActive(value: false);
			EventContextSubTypeElement.gameObject.SetActive(value: false);
			break;
		case ESEType.Phase:
			EventSubTypeDropDown.options.Clear();
			EventSubTypeDropDown.AddOptions(PhaseSubTypes.Select((ESESubTypePhase s) => s.ToString()).ToList());
			EventSubTypeDropDown.value = CurrentTrigger.EventTriggerSubTypeInt;
			EventContextTypeDropDown.options.Clear();
			EventContextTypeDropDown.AddOptions(PhaseTypes.Select((CPhase.PhaseType s) => s.ToString()).ToList());
			EventContextTypeDropDown.value = CurrentTrigger.EventTriggerContextTypeInt;
			EventContextTypeLabel.text = "Phase";
			if (EventContextTypeDropDown.value == 0)
			{
				EventContextTypeDropDown.value = 9;
			}
			EventSubTypeElement.gameObject.SetActive(value: true);
			EventContextTypeElement.gameObject.SetActive(value: true);
			EventContextualElement.gameObject.SetActive(value: false);
			EventContextSubTypeElement.gameObject.SetActive(value: false);
			break;
		case ESEType.Actor:
			EventSubTypeDropDown.options.Clear();
			EventSubTypeDropDown.AddOptions(ActorSubTypes.Select((ESESubTypeActor s) => s.ToString()).ToList());
			EventSubTypeDropDown.value = CurrentTrigger.EventTriggerSubTypeInt;
			EventSubTypeElement.gameObject.SetActive(value: true);
			EventContextTypeElement.gameObject.SetActive(value: false);
			EventContextualElement.gameObject.SetActive(value: false);
			EventContextSubTypeElement.gameObject.SetActive(value: false);
			break;
		case ESEType.ObjectProp:
			EventSubTypeDropDown.options.Clear();
			EventSubTypeDropDown.AddOptions(ObjectPropSubTypes.Select((ESESubTypeObjectProp s) => s.ToString()).ToList());
			EventSubTypeDropDown.value = CurrentTrigger.EventTriggerSubTypeInt;
			EventSubTypeDropDown.onValueChanged.AddListener((int x) =>
			{
				RefreshPropObjectEventSubType();
			});
			EventContextualElement.gameObject.SetActive(value: false);
			EventContextSubTypeElement.gameObject.SetActive(value: false);
			EventSubTypeElement.gameObject.SetActive(value: true);
			RefreshPropObjectEventSubType();
			break;
		case ESEType.Item:
			EventSubTypeDropDown.options.Clear();
			EventSubTypeDropDown.AddOptions(ItemSubTypes.Select((ESESubTypeItem s) => s.ToString()).ToList());
			EventSubTypeDropDown.value = CurrentTrigger.EventTriggerSubTypeInt;
			EventContextualLabel.text = "Item";
			EventSubTypeElement.gameObject.SetActive(value: true);
			EventContextTypeElement.gameObject.SetActive(value: false);
			EventContextualElement.gameObject.SetActive(value: true);
			EventContextSubTypeElement.gameObject.SetActive(value: false);
			break;
		case ESEType.Action:
			EventSubTypeDropDown.options.Clear();
			EventSubTypeDropDown.AddOptions(ActionSubTypes.Select((ESESubTypeAction s) => s.ToString()).ToList());
			EventSubTypeDropDown.value = CurrentTrigger.EventTriggerSubTypeInt;
			EventSubTypeElement.gameObject.SetActive(value: true);
			EventContextTypeElement.gameObject.SetActive(value: false);
			EventContextualElement.gameObject.SetActive(value: false);
			EventContextSubTypeElement.gameObject.SetActive(value: false);
			break;
		case ESEType.Ability:
			EventSubTypeDropDown.options.Clear();
			EventSubTypeDropDown.AddOptions(AbilitySubTypes.Select((ESESubTypeAbility s) => s.ToString()).ToList());
			EventSubTypeDropDown.value = CurrentTrigger.EventTriggerSubTypeInt;
			EventContextTypeDropDown.options.Clear();
			EventContextTypeDropDown.AddOptions(AbilityTypes.Select((CAbility.EAbilityType s) => s.ToString()).ToList());
			EventContextTypeDropDown.value = CurrentTrigger.EventTriggerContextTypeInt;
			EventContextTypeLabel.text = "Ability type";
			FillAbilityContextSubTypeDropDown();
			EventSubTypeElement.gameObject.SetActive(value: true);
			EventContextTypeElement.gameObject.SetActive(value: true);
			EventContextualElement.gameObject.SetActive(value: false);
			break;
		case ESEType.AttackModifier:
			EventSubTypeDropDown.value = 0;
			EventSubTypeDropDown.options.Clear();
			EventSubTypeElement.gameObject.SetActive(value: false);
			EventContextTypeElement.gameObject.SetActive(value: false);
			EventContextualElement.gameObject.SetActive(value: false);
			EventContextSubTypeElement.gameObject.SetActive(value: false);
			break;
		default:
			EventSubTypeDropDown.value = 0;
			EventSubTypeDropDown.options.Clear();
			EventSubTypeElement.gameObject.SetActive(value: false);
			EventContextTypeElement.gameObject.SetActive(value: false);
			EventContextualElement.gameObject.SetActive(value: false);
			EventContextSubTypeElement.gameObject.SetActive(value: false);
			break;
		}
	}

	private void RefreshPropObjectEventSubType()
	{
		m_PropOptions.Clear();
		if (EventSubTypeDropDown.value != 4)
		{
			m_PropOptions.AddRange(ScenarioManager.CurrentScenarioState.Props);
			EventContextTypeLabel.text = "Prop GUID";
			EventContextTypeLabel.fontStyle = FontStyles.Underline;
			EventContextTypeLabel.color = new Color(1f, 0.753f, 0f);
			EventContextTypeElement.gameObject.SetActive(value: true);
		}
		else
		{
			EventContextTypeElement.gameObject.SetActive(value: false);
		}
		LevelEditorUtils.SetFormattedOptions(EventContextTypeDropDown, m_PropOptions, CurrentTrigger.EventTriggerPropGuid);
	}

	private void DisplayForUIEventType(UIEvent.EUIEventType eventType)
	{
		EventContextTypeDropDown.options.Clear();
		EventContextTypeDropDown.AddOptions(PhaseTypes.Select((CPhase.PhaseType s) => s.ToString()).ToList());
		EventContextTypeDropDown.value = CurrentTrigger.EventTriggerContextTypeInt;
		EventContextTypeLabel.text = "Phase";
		EventContextTypeLabel.color = Color.white;
		EventContextTypeLabel.fontStyle = FontStyles.Normal;
		if (EventContextTypeDropDown.value == 0)
		{
			EventContextTypeDropDown.value = 9;
		}
		EventContextTypeElement.gameObject.SetActive(value: true);
		EventSubTypeElement.gameObject.SetActive(value: false);
		EventRoundElement.gameObject.SetActive(value: true);
		EventContextSubTypeElement.gameObject.SetActive(value: false);
		FilterPanel?.gameObject.SetActive(value: false);
		switch (eventType)
		{
		case UIEvent.EUIEventType.AbilityCardSelected:
		case UIEvent.EUIEventType.CardTopHalfSelected:
		case UIEvent.EUIEventType.CardBottomHalfSelected:
			EventContextualLabel.text = "Ability";
			EventContextualElement.gameObject.SetActive(value: true);
			TileIndexElement.gameObject.SetActive(value: false);
			EventContextualIndexElement.gameObject.SetActive(value: false);
			break;
		case UIEvent.EUIEventType.TileSelectedForAbility:
			TileIndexParameterLabel.text = ((CurrentTrigger.EventTriggerTile == null) ? "Not Assigned" : ("X:" + CurrentTrigger.EventTriggerTile.X + "\nY:" + CurrentTrigger.EventTriggerTile.Y));
			EventContextualLabel.text = "Ability";
			EventContextualElement.gameObject.SetActive(value: true);
			TileIndexElement.gameObject.SetActive(value: true);
			EventContextualIndexElement.gameObject.SetActive(value: false);
			break;
		case UIEvent.EUIEventType.LevelMessageDismissed:
			EventContextualLabel.text = "Message Name";
			EventContextualElement.gameObject.SetActive(value: true);
			TileIndexElement.gameObject.SetActive(value: false);
			EventContextualIndexElement.gameObject.SetActive(value: false);
			FilterPanel?.gameObject.SetActive(value: false);
			break;
		case UIEvent.EUIEventType.WindowOpened:
		case UIEvent.EUIEventType.WindowClosed:
			EventContextualLabel.text = "Window Name";
			EventContextualElement.gameObject.SetActive(value: true);
			TileIndexElement.gameObject.SetActive(value: false);
			EventContextualIndexElement.gameObject.SetActive(value: false);
			FilterPanel?.gameObject.SetActive(value: false);
			break;
		case UIEvent.EUIEventType.ConsumeElementCardTop:
		case UIEvent.EUIEventType.ConsumeElementCardBottom:
			EventContextualLabel.text = "Ability Card Name";
			EventContextualIndexLabel.text = "Consume button index";
			EventContextualElement.gameObject.SetActive(value: true);
			TileIndexElement.gameObject.SetActive(value: false);
			EventContextualIndexElement.gameObject.SetActive(value: true);
			break;
		case UIEvent.EUIEventType.RoomRevealed:
			EventContextualElement.gameObject.SetActive(value: false);
			EventContextualDropDownElement.gameObject.SetActive(value: true);
			EventContextualDropDownLabel.text = "Map GUID";
			TileIndexElement.gameObject.SetActive(value: false);
			EventContextualIndexElement.gameObject.SetActive(value: false);
			FillContextTextOrDropDown(eventType);
			break;
		case UIEvent.EUIEventType.ScenarioWon:
		case UIEvent.EUIEventType.ScenarioLost:
			EventContextualElement.gameObject.SetActive(value: false);
			TileIndexElement.gameObject.SetActive(value: false);
			EventContextualIndexElement.gameObject.SetActive(value: false);
			break;
		case UIEvent.EUIEventType.ActorEndedTurnOnTile:
		case UIEvent.EUIEventType.ActorDidntEndTurnOnTile:
			TileIndexParameterLabel.text = ((CurrentTrigger.EventTriggerTile == null) ? "Not Assigned" : ("X:" + CurrentTrigger.EventTriggerTile.X + "\nY:" + CurrentTrigger.EventTriggerTile.Y));
			EventContextualElement.gameObject.SetActive(value: false);
			TileIndexElement.gameObject.SetActive(value: true);
			EventContextualIndexElement.gameObject.SetActive(value: false);
			FilterPanel?.gameObject.SetActive(value: true);
			break;
		case UIEvent.EUIEventType.ObjectiveCompleted:
		case UIEvent.EUIEventType.ObjectiveActivated:
			EventContextualElement.gameObject.SetActive(value: false);
			EventContextualDropDownElement.gameObject.SetActive(value: true);
			EventContextualDropDownLabel.text = "Objective";
			TileIndexElement.gameObject.SetActive(value: false);
			EventContextualIndexElement.gameObject.SetActive(value: false);
			FillContextTextOrDropDown(eventType);
			break;
		case UIEvent.EUIEventType.AllActorsDeadFiltered:
			EventContextualElement.gameObject.SetActive(value: false);
			EventSubTypeElement.gameObject.SetActive(value: false);
			EventContextTypeElement.gameObject.SetActive(value: false);
			EventContextSubTypeElement.gameObject.SetActive(value: false);
			FilterPanel?.gameObject.SetActive(value: true);
			break;
		case UIEvent.EUIEventType.PropActivated:
		case UIEvent.EUIEventType.PropDestroyed:
		case UIEvent.EUIEventType.PropDeactivated:
			EventContextualElement.gameObject.SetActive(value: false);
			EventContextualDropDownElement.gameObject.SetActive(value: true);
			EventContextualDropDownLabel.text = "Prop GUID";
			TileIndexElement.gameObject.SetActive(value: false);
			EventContextualIndexElement.gameObject.SetActive(value: false);
			FillContextTextOrDropDown(eventType);
			break;
		case UIEvent.EUIEventType.DoorUnlocked:
			EventContextualElement.gameObject.SetActive(value: false);
			EventContextualDropDownElement.gameObject.SetActive(value: true);
			EventContextualDropDownLabel.text = "Door GUID";
			TileIndexElement.gameObject.SetActive(value: false);
			EventContextualIndexElement.gameObject.SetActive(value: false);
			FillContextTextOrDropDown(eventType);
			break;
		default:
			EventContextualElement.gameObject.SetActive(value: false);
			TileIndexElement.gameObject.SetActive(value: false);
			EventContextualIndexElement.gameObject.SetActive(value: false);
			break;
		}
	}

	public void SaveValuesToTrigger()
	{
		if (CurrentTrigger == null)
		{
			return;
		}
		if (IsDismissTrigger)
		{
			CurrentTrigger.IsTriggeredByDismiss = EventIsButtonPressToggle.isOn;
		}
		else
		{
			CurrentTrigger.EventTriggerPlayedMessageReq = (string.IsNullOrEmpty(EventPlayedMessageInput.text) ? string.Empty : EventPlayedMessageInput.text);
			CurrentTrigger.EventTriggerNotPlayedMessageReq = (string.IsNullOrEmpty(EventNotPlayedMessageInput.text) ? string.Empty : EventNotPlayedMessageInput.text);
			CurrentTrigger.EventTriggerPlayedMessageReqOR = EventPlayedMessageLogicToggle.IsOn;
			CurrentTrigger.EventTriggerNotPlayedMessageReqOR = EventNotPlayedMessageLogicToggle.IsOn;
		}
		CurrentTrigger.EventTriggerTypeInt = EventTypeDropDown.value;
		CurrentTrigger.EventTriggerSubTypeInt = EventSubTypeDropDown.value;
		CurrentTrigger.EventTriggerActorName = ((EventActorDropDown.value == 0) ? string.Empty : m_ActorOptions[EventActorDropDown.value - 1].GetPrefabName());
		CurrentTrigger.EventTriggerActorGuid = ((EventActorDropDown.value == 0) ? string.Empty : m_ActorOptions[EventActorDropDown.value - 1].ActorGuid);
		if (EventContextTypeDropDown.options.Count > 0 && EventContextTypeDropDown.options[EventContextTypeDropDown.value].text.Contains("-"))
		{
			CurrentTrigger.EventTriggerPropGuid = LevelEditorUtils.ExtractGuidFromFormattedOption(EventContextTypeDropDown.options[EventContextTypeDropDown.value].text);
			CurrentTrigger.EventTriggerContextTypeInt = -1;
		}
		else
		{
			CurrentTrigger.EventTriggerPropGuid = "";
			CurrentTrigger.EventTriggerContextTypeInt = EventContextTypeDropDown.value;
		}
		CurrentTrigger.EventTriggerContextSubTypeInt = EventContextSubTypeDropDown.value;
		CurrentTrigger.EventTriggerRound = ((!string.IsNullOrEmpty(EventRoundInput.text)) ? int.Parse(EventRoundInput.text) : 0);
		CurrentTrigger.EventAliveActorGUID = ((EventAliveActorDropDown.value == 0) ? string.Empty : m_ActorOptions[EventAliveActorDropDown.value - 1].ActorGuid);
		if ((CurrentTrigger.IsUIEventTypeTrigger && UIEventTypes[CurrentTrigger.EventTriggerTypeInt] == UIEvent.EUIEventType.RoomRevealed) || (CurrentTrigger.IsUIEventTypeTrigger && UIEventTypes[CurrentTrigger.EventTriggerTypeInt] == UIEvent.EUIEventType.ObjectiveCompleted) || (CurrentTrigger.IsUIEventTypeTrigger && UIEventTypes[CurrentTrigger.EventTriggerTypeInt] == UIEvent.EUIEventType.ObjectiveActivated) || (CurrentTrigger.IsUIEventTypeTrigger && UIEventTypes[CurrentTrigger.EventTriggerTypeInt] == UIEvent.EUIEventType.PropActivated) || (CurrentTrigger.IsUIEventTypeTrigger && UIEventTypes[CurrentTrigger.EventTriggerTypeInt] == UIEvent.EUIEventType.PropDestroyed) || (CurrentTrigger.IsUIEventTypeTrigger && UIEventTypes[CurrentTrigger.EventTriggerTypeInt] == UIEvent.EUIEventType.PropDeactivated) || (CurrentTrigger.IsUIEventTypeTrigger && UIEventTypes[CurrentTrigger.EventTriggerTypeInt] == UIEvent.EUIEventType.DoorUnlocked))
		{
			if (EventContextualDropDownInput.options.Count > 0 && EventContextualDropDownInput.options.Count > EventContextualDropDownInput.value)
			{
				CurrentTrigger.EventTriggerContextId = LevelEditorUtils.ExtractGuidFromFormattedOption(EventContextualDropDownInput.options[EventContextualDropDownInput.value].text);
			}
		}
		else
		{
			CurrentTrigger.EventTriggerContextId = EventContextualInput.text;
		}
		CurrentTrigger.EventTriggerContextIndex = ((!string.IsNullOrEmpty(EventContextualIndexInput.text)) ? int.Parse(EventContextualIndexInput.text) : 0);
		FilterPanel?.Apply();
		CurrentTrigger.EventFilter = FilterPanel?.FilterBeingShown ?? new CObjectiveFilter();
	}

	public void IsDismissButtonToggle(bool value)
	{
		if (CurrentTrigger != null && !m_PreventToggleEvents)
		{
			CurrentTrigger.IsTriggeredByDismiss = value;
			OnTypeDropDownValueChanged(EventTypeDropDown.value);
		}
	}

	public void MainEventTypeToggleChanged()
	{
		if (CurrentTrigger != null && !m_PreventToggleEvents)
		{
			CurrentTrigger.IsUIEventTypeTrigger = !IsSEventTypeToggle.isOn;
			FillEventTypeDropDown();
			OnTypeDropDownValueChanged(EventTypeDropDown.value);
		}
	}

	public void SelectTileForUIEventPressed()
	{
		LevelEditorController.SelectTile(TileSelected);
	}

	public void TileSelected(CClientTile tileSelected)
	{
		CurrentTrigger.EventTriggerTile = new TileIndex(tileSelected.m_Tile.m_ArrayIndex);
		TileIndexParameterLabel.text = ((CurrentTrigger.EventTriggerTile == null) ? "Not Assigned" : ("X:" + CurrentTrigger.EventTriggerTile.X + "\nY:" + CurrentTrigger.EventTriggerTile.Y));
	}
}
