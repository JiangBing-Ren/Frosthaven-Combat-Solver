using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorStatIsBasedOnXPanel : MonoBehaviour
{
	[Header("Entry List")]
	public LevelEditorGenericListPanel ItemListPanel;

	[Header("Details Panel")]
	public GameObject NeedSelectionBlocker;

	public TextMeshProUGUI TitleLabel;

	public TMP_Dropdown StatDropDown;

	public TMP_Dropdown BasedOnXTypeDropDown;

	public TMP_InputField XInputField;

	public TMP_InputField YInputField;

	public TMP_InputField MinScenarioLevelField;

	[Header("Layout Elements to toggle")]
	public LayoutElement ListItemPanel;

	public LayoutElement StatDropDownElement;

	public LayoutElement XInputFieldElement;

	public LayoutElement YInputFieldElement;

	public LayoutElement MinScenarioLevelFieldElement;

	private List<EMonsterBaseStats> m_AvailableStats;

	private List<Tuple<CAbility.EStatIsBasedOnXType, string>> m_AvailableExpressionTypes;

	private List<CStatBasedOnXOverrideDetails> m_OverridesBeingShown;

	private CStatBasedOnXOverrideDetails m_OverrideBeingEdited;

	private PropHealthDetails m_PropHealthDetailsBeingShown;

	public string IDShowingFor { get; private set; }

	private bool m_IsCurrentlyEditingProp
	{
		get
		{
			if (string.IsNullOrEmpty(IDShowingFor))
			{
				return m_PropHealthDetailsBeingShown != null;
			}
			return false;
		}
	}

	public void SetShowingForMonsterClass(string IDToShow)
	{
		IDShowingFor = IDToShow;
		m_PropHealthDetailsBeingShown = null;
		SetupDropDowns();
		TitleLabel.text = $"Stat for {IDToShow}";
		m_OverridesBeingShown = SaveData.Instance.Global.CurrentEditorLevelData.GetStatBasedOnXEntriedForClass(IDToShow);
		ItemListPanel.RefreshUIWithItems(m_OverridesBeingShown.Select((CStatBasedOnXOverrideDetails o) => o.OverrideData.BaseStatType.ToString()).ToList());
		ItemListPanel.SetupDelegateActions(OnAddOverridePressed, OnOverrideDeleted, OnOverridePressed);
		ListItemPanel.gameObject.SetActive(value: true);
		if (m_OverrideBeingEdited != null && m_OverridesBeingShown.Contains(m_OverrideBeingEdited))
		{
			SetOverrideSelected(m_OverrideBeingEdited);
		}
		else
		{
			SetOverrideSelected(null);
		}
	}

	public void SetShowingForPropHealth(CObjectProp propShowing)
	{
		IDShowingFor = string.Empty;
		m_PropHealthDetailsBeingShown = propShowing.PropHealthDetails;
		SetupDropDowns(showOnlyHealth: true);
		TitleLabel.text = $"Health Stat for {propShowing.PropGuid}";
		m_OverridesBeingShown = new List<CStatBasedOnXOverrideDetails>();
		ItemListPanel.RefreshUIWithItems(new List<string>());
		ItemListPanel.SetupDelegateActions();
		ListItemPanel.gameObject.SetActive(value: false);
		SetPropHealthShown(m_PropHealthDetailsBeingShown);
	}

	private void SetupDropDowns(bool showOnlyHealth = false)
	{
		if (showOnlyHealth)
		{
			m_AvailableStats = new List<EMonsterBaseStats>
			{
				EMonsterBaseStats.None,
				EMonsterBaseStats.Health
			};
		}
		else
		{
			m_AvailableStats = MonsterYMLData.MonsterBaseStatsEnums.ToList();
		}
		StatDropDown.SetValueWithoutNotify(0);
		StatDropDown.ClearOptions();
		StatDropDown.AddOptions(m_AvailableStats.Select((EMonsterBaseStats s) => s.ToString()).ToList());
		List<CAbility.EStatIsBasedOnXType> list = CAbility.StatIsBasedOnXTypesForBaseStats.ToList();
		list.Insert(0, CAbility.EStatIsBasedOnXType.None);
		m_AvailableExpressionTypes = new List<Tuple<CAbility.EStatIsBasedOnXType, string>>();
		for (int num = 0; num < list.Count; num++)
		{
			m_AvailableExpressionTypes.Add(new Tuple<CAbility.EStatIsBasedOnXType, string>(list[num], AbilityData.StatIsBasedOnXData.GetExpressionStringForType(list[num])));
		}
		BasedOnXTypeDropDown.SetValueWithoutNotify(0);
		BasedOnXTypeDropDown.ClearOptions();
		BasedOnXTypeDropDown.AddOptions(m_AvailableExpressionTypes.Select((Tuple<CAbility.EStatIsBasedOnXType, string> e) => e.Item2).ToList());
	}

	private void SetOverrideSelected(CStatBasedOnXOverrideDetails overrideSelected)
	{
		m_OverrideBeingEdited = overrideSelected;
		MinScenarioLevelFieldElement.gameObject.SetActive(value: false);
		if (m_OverrideBeingEdited?.OverrideData != null && m_OverrideBeingEdited.OverrideData.BasedOn.ToString().Contains("Level"))
		{
			MinScenarioLevelFieldElement.gameObject.SetActive(value: true);
			if (m_OverrideBeingEdited.OverrideData.MinLValue != 0)
			{
				MinScenarioLevelField.text = m_OverrideBeingEdited.OverrideData.MinLValue.ToString();
			}
			else
			{
				MinScenarioLevelField.text = string.Empty;
			}
		}
		if (m_OverrideBeingEdited == null)
		{
			NeedSelectionBlocker.gameObject.SetActive(value: true);
			return;
		}
		NeedSelectionBlocker.gameObject.SetActive(value: false);
		RefreshDisplayLayoutForExpressionType(overrideSelected.OverrideData.BasedOn);
		StatDropDownElement.gameObject.SetActive(value: true);
		int valueWithoutNotify = m_AvailableStats.FindIndex((EMonsterBaseStats b) => overrideSelected.OverrideData.BaseStatType == b);
		int valueWithoutNotify2 = m_AvailableExpressionTypes.FindIndex((Tuple<CAbility.EStatIsBasedOnXType, string> e) => overrideSelected.OverrideData.BasedOn == e.Item1);
		StatDropDown.SetValueWithoutNotify(valueWithoutNotify);
		BasedOnXTypeDropDown.SetValueWithoutNotify(valueWithoutNotify2);
		switch (overrideSelected.OverrideData.BasedOn)
		{
		case CAbility.EStatIsBasedOnXType.InitialPlayerCharacterCount:
		case CAbility.EStatIsBasedOnXType.XAddedToCharactersTimesLevel:
		case CAbility.EStatIsBasedOnXType.CharactersTimesLevelPlusX:
		case CAbility.EStatIsBasedOnXType.XPlusCharactersPlusLevel:
		case CAbility.EStatIsBasedOnXType.XPlusLevel:
		case CAbility.EStatIsBasedOnXType.LevelAddedToXTimesCharacters:
		case CAbility.EStatIsBasedOnXType.XAddedToInitialPlayerCharacterCount:
		case CAbility.EStatIsBasedOnXType.CharactersPlusLevelOverX:
		case CAbility.EStatIsBasedOnXType.CharactersPlusLevelTimesX:
			XInputField.text = overrideSelected.OverrideData.Multiplier.ToString();
			break;
		case CAbility.EStatIsBasedOnXType.XAddedToYTimesLevel:
		case CAbility.EStatIsBasedOnXType.XAddedToLevelTimesCharactersOverY:
		case CAbility.EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountPlusLevelMinusY:
		case CAbility.EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountPlusLevelMinusYTimesRound:
		case CAbility.EStatIsBasedOnXType.XPlusCharactersPlusLevelTimesY:
		case CAbility.EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountMinusY:
		case CAbility.EStatIsBasedOnXType.XAddedToLevelThenTimesCharactersOverY:
		case CAbility.EStatIsBasedOnXType.LevelTimesXPlusY:
		case CAbility.EStatIsBasedOnXType.CharactersAddedToXOver2ThenTimesLevelPlusY:
		case CAbility.EStatIsBasedOnXType.LevelPlusXTimesCharactersTimesY:
		case CAbility.EStatIsBasedOnXType.LevelPlusYTimesX:
		case CAbility.EStatIsBasedOnXType.XPlusYCharactersTimes2PlusLevel:
		case CAbility.EStatIsBasedOnXType.LevelTimesXPlusYTimesCharacters:
		case CAbility.EStatIsBasedOnXType.YPlusCharactersTimesLevelPlusX:
			XInputField.text = overrideSelected.OverrideData.Multiplier.ToString();
			YInputField.text = overrideSelected.OverrideData.SecondVariable.ToString();
			break;
		}
	}

	private void SetPropHealthShown(PropHealthDetails propHealth)
	{
		if (m_PropHealthDetailsBeingShown == null)
		{
			NeedSelectionBlocker.gameObject.SetActive(value: true);
			return;
		}
		NeedSelectionBlocker.gameObject.SetActive(value: false);
		RefreshDisplayLayoutForExpressionType(propHealth.HealthBasedOnXType);
		StatDropDownElement.gameObject.SetActive(value: false);
		int valueWithoutNotify = m_AvailableExpressionTypes.FindIndex((Tuple<CAbility.EStatIsBasedOnXType, string> e) => propHealth.HealthBasedOnXType == e.Item1);
		BasedOnXTypeDropDown.SetValueWithoutNotify(valueWithoutNotify);
		MinScenarioLevelFieldElement.gameObject.SetActive(value: false);
		if (propHealth.HealthBasedOnXType.ToString().Contains("Level"))
		{
			MinScenarioLevelFieldElement.gameObject.SetActive(value: true);
			if (propHealth.MinScenarioLevel != 0)
			{
				MinScenarioLevelField.text = propHealth.MinScenarioLevel.ToString();
			}
			else
			{
				MinScenarioLevelField.text = string.Empty;
			}
		}
		switch (propHealth.HealthBasedOnXType)
		{
		case CAbility.EStatIsBasedOnXType.InitialPlayerCharacterCount:
		case CAbility.EStatIsBasedOnXType.XAddedToCharactersTimesLevel:
		case CAbility.EStatIsBasedOnXType.CharactersTimesLevelPlusX:
		case CAbility.EStatIsBasedOnXType.XPlusCharactersPlusLevel:
		case CAbility.EStatIsBasedOnXType.XPlusLevel:
		case CAbility.EStatIsBasedOnXType.LevelAddedToXTimesCharacters:
		case CAbility.EStatIsBasedOnXType.XAddedToInitialPlayerCharacterCount:
		case CAbility.EStatIsBasedOnXType.CharactersPlusLevelOverX:
		case CAbility.EStatIsBasedOnXType.CharactersPlusLevelTimesX:
			XInputField.text = propHealth.HealthBasedOnXVariable.ToString();
			break;
		case CAbility.EStatIsBasedOnXType.XAddedToYTimesLevel:
		case CAbility.EStatIsBasedOnXType.XAddedToLevelTimesCharactersOverY:
		case CAbility.EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountPlusLevelMinusY:
		case CAbility.EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountPlusLevelMinusYTimesRound:
		case CAbility.EStatIsBasedOnXType.XPlusCharactersPlusLevelTimesY:
		case CAbility.EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountMinusY:
		case CAbility.EStatIsBasedOnXType.XAddedToLevelThenTimesCharactersOverY:
		case CAbility.EStatIsBasedOnXType.LevelTimesXPlusY:
		case CAbility.EStatIsBasedOnXType.CharactersAddedToXOver2ThenTimesLevelPlusY:
		case CAbility.EStatIsBasedOnXType.LevelPlusXTimesCharactersTimesY:
		case CAbility.EStatIsBasedOnXType.LevelPlusYTimesX:
		case CAbility.EStatIsBasedOnXType.XPlusYCharactersTimes2PlusLevel:
		case CAbility.EStatIsBasedOnXType.LevelTimesXPlusYTimesCharacters:
		case CAbility.EStatIsBasedOnXType.YPlusCharactersTimesLevelPlusX:
			XInputField.text = propHealth.HealthBasedOnXVariable.ToString();
			YInputField.text = propHealth.HealthBasedOnYVariable.ToString();
			break;
		}
	}

	private void RefreshDisplayLayoutForExpressionType(CAbility.EStatIsBasedOnXType expressionType)
	{
		switch (expressionType)
		{
		case CAbility.EStatIsBasedOnXType.InitialPlayerCharacterCount:
		case CAbility.EStatIsBasedOnXType.XAddedToCharactersTimesLevel:
		case CAbility.EStatIsBasedOnXType.CharactersTimesLevelPlusX:
		case CAbility.EStatIsBasedOnXType.XPlusCharactersPlusLevel:
		case CAbility.EStatIsBasedOnXType.XPlusLevel:
		case CAbility.EStatIsBasedOnXType.LevelAddedToXTimesCharacters:
		case CAbility.EStatIsBasedOnXType.XAddedToInitialPlayerCharacterCount:
		case CAbility.EStatIsBasedOnXType.CharactersPlusLevelOverX:
		case CAbility.EStatIsBasedOnXType.CharactersPlusLevelTimesX:
			XInputFieldElement.gameObject.SetActive(value: true);
			YInputFieldElement.gameObject.SetActive(value: false);
			break;
		case CAbility.EStatIsBasedOnXType.XAddedToYTimesLevel:
		case CAbility.EStatIsBasedOnXType.XAddedToLevelTimesCharactersOverY:
		case CAbility.EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountPlusLevelMinusY:
		case CAbility.EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountPlusLevelMinusYTimesRound:
		case CAbility.EStatIsBasedOnXType.XPlusCharactersPlusLevelTimesY:
		case CAbility.EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountMinusY:
		case CAbility.EStatIsBasedOnXType.XAddedToLevelThenTimesCharactersOverY:
		case CAbility.EStatIsBasedOnXType.LevelTimesXPlusY:
		case CAbility.EStatIsBasedOnXType.CharactersAddedToXOver2ThenTimesLevelPlusY:
		case CAbility.EStatIsBasedOnXType.LevelPlusXTimesCharactersTimesY:
		case CAbility.EStatIsBasedOnXType.LevelPlusYTimesX:
		case CAbility.EStatIsBasedOnXType.XPlusYCharactersTimes2PlusLevel:
		case CAbility.EStatIsBasedOnXType.LevelTimesXPlusYTimesCharacters:
		case CAbility.EStatIsBasedOnXType.YPlusCharactersTimesLevelPlusX:
			XInputFieldElement.gameObject.SetActive(value: true);
			YInputFieldElement.gameObject.SetActive(value: true);
			break;
		default:
			XInputFieldElement.gameObject.SetActive(value: false);
			YInputFieldElement.gameObject.SetActive(value: false);
			break;
		}
	}

	public void OnStatTypeDropDownChanged()
	{
		EMonsterBaseStats baseStatType = m_AvailableStats[StatDropDown.value];
		if (!m_IsCurrentlyEditingProp)
		{
			m_OverrideBeingEdited.OverrideData.BaseStatType = baseStatType;
			SetShowingForMonsterClass(IDShowingFor);
		}
	}

	public void OnExpressionTypeDropDownChanged()
	{
		CAbility.EStatIsBasedOnXType item = m_AvailableExpressionTypes[BasedOnXTypeDropDown.value].Item1;
		if (m_IsCurrentlyEditingProp)
		{
			m_PropHealthDetailsBeingShown.HealthBasedOnXType = item;
			SetPropHealthShown(m_PropHealthDetailsBeingShown);
		}
		else
		{
			m_OverrideBeingEdited.OverrideData.BasedOn = item;
			SetOverrideSelected(m_OverrideBeingEdited);
		}
	}

	public void OnXInputFinishedEditing()
	{
		switch (m_IsCurrentlyEditingProp ? m_PropHealthDetailsBeingShown.HealthBasedOnXType : m_OverrideBeingEdited.OverrideData.BasedOn)
		{
		case CAbility.EStatIsBasedOnXType.InitialPlayerCharacterCount:
		case CAbility.EStatIsBasedOnXType.XAddedToCharactersTimesLevel:
		case CAbility.EStatIsBasedOnXType.CharactersTimesLevelPlusX:
		case CAbility.EStatIsBasedOnXType.XAddedToYTimesLevel:
		case CAbility.EStatIsBasedOnXType.XPlusCharactersPlusLevel:
		case CAbility.EStatIsBasedOnXType.XPlusLevel:
		case CAbility.EStatIsBasedOnXType.XAddedToLevelTimesCharactersOverY:
		case CAbility.EStatIsBasedOnXType.LevelAddedToXTimesCharacters:
		case CAbility.EStatIsBasedOnXType.XAddedToInitialPlayerCharacterCount:
		case CAbility.EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountPlusLevelMinusY:
		case CAbility.EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountPlusLevelMinusYTimesRound:
		case CAbility.EStatIsBasedOnXType.CharactersPlusLevelOverX:
		case CAbility.EStatIsBasedOnXType.XPlusCharactersPlusLevelTimesY:
		case CAbility.EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountMinusY:
		case CAbility.EStatIsBasedOnXType.XAddedToLevelThenTimesCharactersOverY:
		case CAbility.EStatIsBasedOnXType.LevelTimesXPlusY:
		case CAbility.EStatIsBasedOnXType.CharactersPlusLevelTimesX:
		case CAbility.EStatIsBasedOnXType.CharactersAddedToXOver2ThenTimesLevelPlusY:
		case CAbility.EStatIsBasedOnXType.LevelPlusXTimesCharactersTimesY:
		case CAbility.EStatIsBasedOnXType.LevelPlusYTimesX:
		case CAbility.EStatIsBasedOnXType.XPlusYCharactersTimes2PlusLevel:
		case CAbility.EStatIsBasedOnXType.LevelTimesXPlusYTimesCharacters:
		case CAbility.EStatIsBasedOnXType.YPlusCharactersTimesLevelPlusX:
		{
			float result = 1f;
			if (float.TryParse(XInputField.text, out result))
			{
				if (m_IsCurrentlyEditingProp)
				{
					m_PropHealthDetailsBeingShown.HealthBasedOnXVariable = result;
				}
				else
				{
					m_OverrideBeingEdited.OverrideData.Multiplier = result;
				}
				break;
			}
			Log.ErrorFormat("Cannot convert {0} to float, try making it a decimal number", XInputField.text);
			if (m_IsCurrentlyEditingProp)
			{
				XInputField.text = m_PropHealthDetailsBeingShown.HealthBasedOnXVariable.ToString();
			}
			else
			{
				XInputField.text = m_OverrideBeingEdited.OverrideData.Multiplier.ToString();
			}
			break;
		}
		}
	}

	public void OnMinScenarioLevelFinishedEditing()
	{
		int result = 0;
		if (int.TryParse(MinScenarioLevelField.text, out result))
		{
			if (m_IsCurrentlyEditingProp)
			{
				m_PropHealthDetailsBeingShown.MinScenarioLevel = result;
			}
			else
			{
				m_OverrideBeingEdited.OverrideData.MinLValue = result;
			}
		}
	}

	public void OnYInputFinishedEditing()
	{
		switch (m_IsCurrentlyEditingProp ? m_PropHealthDetailsBeingShown.HealthBasedOnXType : m_OverrideBeingEdited.OverrideData.BasedOn)
		{
		case CAbility.EStatIsBasedOnXType.XAddedToYTimesLevel:
		case CAbility.EStatIsBasedOnXType.XAddedToLevelTimesCharactersOverY:
		case CAbility.EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountPlusLevelMinusY:
		case CAbility.EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountPlusLevelMinusYTimesRound:
		case CAbility.EStatIsBasedOnXType.XPlusCharactersPlusLevelTimesY:
		case CAbility.EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountMinusY:
		case CAbility.EStatIsBasedOnXType.XAddedToLevelThenTimesCharactersOverY:
		case CAbility.EStatIsBasedOnXType.LevelTimesXPlusY:
		case CAbility.EStatIsBasedOnXType.CharactersAddedToXOver2ThenTimesLevelPlusY:
		case CAbility.EStatIsBasedOnXType.LevelPlusXTimesCharactersTimesY:
		case CAbility.EStatIsBasedOnXType.LevelPlusYTimesX:
		case CAbility.EStatIsBasedOnXType.XPlusYCharactersTimes2PlusLevel:
		case CAbility.EStatIsBasedOnXType.LevelTimesXPlusYTimesCharacters:
		case CAbility.EStatIsBasedOnXType.YPlusCharactersTimesLevelPlusX:
		{
			float result = 1f;
			if (float.TryParse(YInputField.text, out result))
			{
				if (m_IsCurrentlyEditingProp)
				{
					m_PropHealthDetailsBeingShown.HealthBasedOnYVariable = result;
				}
				else
				{
					m_OverrideBeingEdited.OverrideData.SecondVariable = result;
				}
				break;
			}
			Log.ErrorFormat("Cannot convert {0} to float, try making it a decimal number", XInputField.text);
			if (m_IsCurrentlyEditingProp)
			{
				YInputField.text = m_PropHealthDetailsBeingShown.HealthBasedOnYVariable.ToString();
			}
			else
			{
				YInputField.text = m_OverrideBeingEdited.OverrideData.SecondVariable.ToString();
			}
			break;
		}
		}
	}

	public void OnAddOverridePressed(string unusedItemStringParameter)
	{
		CStatBasedOnXOverrideDetails cStatBasedOnXOverrideDetails = new CStatBasedOnXOverrideDetails(IDShowingFor);
		SaveData.Instance.Global.CurrentEditorLevelData.StatBasedOnXOverrideList.Add(cStatBasedOnXOverrideDetails);
		SetOverrideSelected(cStatBasedOnXOverrideDetails);
		SetShowingForMonsterClass(IDShowingFor);
	}

	public void OnOverrideDeleted(string unusedItemStringParameter, int itemIndex)
	{
		CStatBasedOnXOverrideDetails item = m_OverridesBeingShown[itemIndex];
		SaveData.Instance.Global.CurrentEditorLevelData.StatBasedOnXOverrideList.Remove(item);
		SetShowingForMonsterClass(IDShowingFor);
	}

	public void OnOverridePressed(string unusedItemStringParameter, int itemIndex)
	{
		CStatBasedOnXOverrideDetails overrideSelected = m_OverridesBeingShown[itemIndex];
		SetOverrideSelected(overrideSelected);
	}
}
