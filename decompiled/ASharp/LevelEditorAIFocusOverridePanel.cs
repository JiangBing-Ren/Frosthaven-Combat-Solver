using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorAIFocusOverridePanel : MonoBehaviour
{
	public TMP_Dropdown OverrideTypeDropDown;

	public TMP_Dropdown TargetTypeDropDown;

	public TMP_Dropdown TargetGUIDDropDown;

	public TMP_Dropdown TargetClassDropDown;

	public Toggle IsDisabledToggle;

	public Toggle DisallowDoorwaysToggle;

	public LayoutElement TargetTypeDropDownElement;

	public LayoutElement TargetDropDownElement;

	private CAIFocusOverrideDetails m_AIFocusOverrideShowingFor;

	public void SetShowingForActorState(ActorState actorState)
	{
		if (actorState == null)
		{
			m_AIFocusOverrideShowingFor = null;
			return;
		}
		if (actorState.AIFocusOverride == null)
		{
			actorState.SetAIFocusOverride(new CAIFocusOverrideDetails());
		}
		m_AIFocusOverrideShowingFor = actorState.AIFocusOverride;
		ConfigureDropDownVisibility();
		OverrideTypeDropDown.ClearOptions();
		OverrideTypeDropDown.AddOptions(CAIFocusOverrideDetails.OverrideTypes.Select((CAIFocusOverrideDetails.EOverrideType t) => t.ToString()).ToList());
		OverrideTypeDropDown.SetValueWithoutNotify(Mathf.Max(0, CAIFocusOverrideDetails.OverrideTypes.IndexOf(m_AIFocusOverrideShowingFor.OverrideType)));
		TargetTypeDropDown.ClearOptions();
		TargetTypeDropDown.AddOptions(CAIFocusOverrideDetails.OverrideTargetTypes.Select((CAIFocusOverrideDetails.EOverrideTargetType t) => t.ToString()).ToList());
		TargetTypeDropDown.SetValueWithoutNotify(Mathf.Max(0, CAIFocusOverrideDetails.OverrideTargetTypes.IndexOf(m_AIFocusOverrideShowingFor.OverrideTargetType)));
		IsDisabledToggle.SetIsOnWithoutNotify(m_AIFocusOverrideShowingFor.IsDisabled);
		DisallowDoorwaysToggle.SetIsOnWithoutNotify(m_AIFocusOverrideShowingFor.DisallowDoorways);
		ConfigureTargetGUIDDropDown();
		ConfigureTargetClassDropDown();
	}

	public void SetShowingForSpawner(CSpawner spawner)
	{
		if (spawner == null)
		{
			m_AIFocusOverrideShowingFor = null;
			return;
		}
		if (spawner.AIFocusOverride == null)
		{
			spawner.SetAIFocusOverride(new CAIFocusOverrideDetails());
		}
		m_AIFocusOverrideShowingFor = spawner.AIFocusOverride;
		ConfigureDropDownVisibility();
		OverrideTypeDropDown.ClearOptions();
		OverrideTypeDropDown.AddOptions(CAIFocusOverrideDetails.OverrideTypes.Select((CAIFocusOverrideDetails.EOverrideType t) => t.ToString()).ToList());
		OverrideTypeDropDown.SetValueWithoutNotify(Mathf.Max(0, CAIFocusOverrideDetails.OverrideTypes.IndexOf(m_AIFocusOverrideShowingFor.OverrideType)));
		TargetTypeDropDown.ClearOptions();
		TargetTypeDropDown.AddOptions(CAIFocusOverrideDetails.OverrideTargetTypes.Select((CAIFocusOverrideDetails.EOverrideTargetType t) => t.ToString()).ToList());
		TargetTypeDropDown.SetValueWithoutNotify(Mathf.Max(0, CAIFocusOverrideDetails.OverrideTargetTypes.IndexOf(m_AIFocusOverrideShowingFor.OverrideTargetType)));
		IsDisabledToggle.SetIsOnWithoutNotify(m_AIFocusOverrideShowingFor.IsDisabled);
		DisallowDoorwaysToggle.SetIsOnWithoutNotify(m_AIFocusOverrideShowingFor.DisallowDoorways);
		ConfigureTargetGUIDDropDown();
		ConfigureTargetClassDropDown();
	}

	private void ConfigureTargetGUIDDropDown()
	{
		TargetGUIDDropDown.ClearOptions();
		TargetGUIDDropDown.AddOptions(new List<string> { "None" });
		switch (m_AIFocusOverrideShowingFor.OverrideTargetType)
		{
		case CAIFocusOverrideDetails.EOverrideTargetType.Actor:
			TargetGUIDDropDown.AddOptions(ScenarioManager.CurrentScenarioState.ActorStates.Select((ActorState a) => a.ActorGuid).ToList());
			break;
		case CAIFocusOverrideDetails.EOverrideTargetType.Prop:
			TargetGUIDDropDown.AddOptions(ScenarioManager.CurrentScenarioState.Props.Select((CObjectProp p) => p.PropGuid).ToList());
			TargetGUIDDropDown.AddOptions(ScenarioManager.CurrentScenarioState.ActivatedProps.Select((CObjectProp p) => p.PropGuid).ToList());
			break;
		}
		int num = TargetGUIDDropDown.options.FindIndex((TMP_Dropdown.OptionData o) => o.text == m_AIFocusOverrideShowingFor.TargetGUID);
		if (num < 0)
		{
			if (TargetGUIDDropDown.options.Count > 0)
			{
				m_AIFocusOverrideShowingFor.TargetGUID = TargetGUIDDropDown.options[0].text;
			}
			num = 0;
		}
		TargetGUIDDropDown.SetValueWithoutNotify(num);
	}

	private void ConfigureTargetClassDropDown()
	{
		TargetClassDropDown.ClearOptions();
		TargetClassDropDown.AddOptions(new List<string> { "None" });
		if (m_AIFocusOverrideShowingFor.OverrideTargetType == CAIFocusOverrideDetails.EOverrideTargetType.Actor)
		{
			TargetClassDropDown.AddOptions(CharacterClassManager.Classes.Select((CCharacterClass x) => x.ID).ToList());
		}
		int num = TargetClassDropDown.options.FindIndex((TMP_Dropdown.OptionData o) => o.text == m_AIFocusOverrideShowingFor.TargetClassID);
		if (num < 0)
		{
			if (TargetClassDropDown.options.Count > 0)
			{
				m_AIFocusOverrideShowingFor.TargetClassID = TargetClassDropDown.options[0].text;
			}
			num = 0;
		}
		TargetClassDropDown.SetValueWithoutNotify(num);
	}

	private void ConfigureDropDownVisibility()
	{
		TargetTypeDropDownElement.gameObject.SetActive(m_AIFocusOverrideShowingFor.OverrideType != CAIFocusOverrideDetails.EOverrideType.None);
		TargetDropDownElement.gameObject.SetActive(m_AIFocusOverrideShowingFor.OverrideType != CAIFocusOverrideDetails.EOverrideType.None && m_AIFocusOverrideShowingFor.OverrideTargetType != CAIFocusOverrideDetails.EOverrideTargetType.None);
	}

	public void OnOverrideTypeDropDownValueChanged()
	{
		m_AIFocusOverrideShowingFor.OverrideType = CAIFocusOverrideDetails.OverrideTypes[OverrideTypeDropDown.value];
		ConfigureDropDownVisibility();
	}

	public void OnTargetTypeDropdownValueChanged()
	{
		m_AIFocusOverrideShowingFor.OverrideTargetType = CAIFocusOverrideDetails.OverrideTargetTypes[TargetTypeDropDown.value];
		ConfigureDropDownVisibility();
		ConfigureTargetGUIDDropDown();
		ConfigureTargetClassDropDown();
	}

	public void OnTargetDropDownValueChanged()
	{
		m_AIFocusOverrideShowingFor.TargetGUID = TargetGUIDDropDown.options[TargetGUIDDropDown.value].text;
	}

	public void OnTargetClassDropDownValueChanged()
	{
		m_AIFocusOverrideShowingFor.TargetClassID = TargetClassDropDown.options[TargetClassDropDown.value].text;
	}

	public void OnIsDisabledToggleValueChanged()
	{
		m_AIFocusOverrideShowingFor.IsDisabled = IsDisabledToggle.isOn;
	}

	public void OnDisallowDoorwaysToggleValueChanged()
	{
		m_AIFocusOverrideShowingFor.DisallowDoorways = DisallowDoorwaysToggle.isOn;
	}
}
