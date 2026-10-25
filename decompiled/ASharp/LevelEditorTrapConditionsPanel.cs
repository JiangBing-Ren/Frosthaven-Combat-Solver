using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorTrapConditionsPanel : MonoBehaviour
{
	public TextMeshProUGUI ListTitle;

	public LayoutGroup NegativeConditionItemParent;

	public GameObject NegativeConditionItemPrefab;

	public TMP_Dropdown NegativeConditionDropdown;

	private CObjectTrap m_TrapShowingFor;

	private List<LevelEditorClassListItem> m_NegativeConditionItems;

	private List<string> m_AllPossibleConditions;

	public void DisplayConditionsForTrap(CObjectTrap trapToShow)
	{
		ListTitle.text = "Trap Conditions:";
		m_TrapShowingFor = trapToShow;
		FillList();
	}

	public void AddNegativeConditionForTrap(CCondition.ENegativeCondition conditionToAdd)
	{
		LevelEditorClassListItem component = Object.Instantiate(NegativeConditionItemPrefab, NegativeConditionItemParent.transform).GetComponent<LevelEditorClassListItem>();
		component.Init(conditionToAdd.ToString());
		component.OnRemoveButtonPressedAction = ConditionDeleted;
		m_NegativeConditionItems.Add(component);
	}

	public void ConditionDeleted(LevelEditorClassListItem conditionItemToRemove)
	{
		m_TrapShowingFor.Conditions.RemoveAll((CCondition.ENegativeCondition c) => c.ToString() == conditionItemToRemove.ClassName);
		GameObject obj = conditionItemToRemove.gameObject;
		m_NegativeConditionItems.Remove(conditionItemToRemove);
		Object.Destroy(obj);
		RefreshAddDropDown();
	}

	private void ClearList()
	{
		if (m_NegativeConditionItems == null)
		{
			m_NegativeConditionItems = new List<LevelEditorClassListItem>();
			return;
		}
		for (int i = 0; i < m_NegativeConditionItems.Count; i++)
		{
			Object.Destroy(m_NegativeConditionItems[i].gameObject);
		}
		m_NegativeConditionItems.Clear();
	}

	private void FillList()
	{
		ClearList();
		foreach (CCondition.ENegativeCondition condition in m_TrapShowingFor.Conditions)
		{
			AddNegativeConditionForTrap(condition);
		}
		RefreshAddDropDown();
	}

	private void RefreshAddDropDown()
	{
		NegativeConditionDropdown.ClearOptions();
		NegativeConditionDropdown.options.Add(new TMP_Dropdown.OptionData("<SELECT ITEM TO ADD>"));
		m_AllPossibleConditions = CCondition.NegativeConditions.Select((CCondition.ENegativeCondition s) => s.ToString()).ToList();
		NegativeConditionDropdown.AddOptions(m_AllPossibleConditions);
		NegativeConditionDropdown.value = 0;
	}

	public void AddConditionPressed()
	{
		int dropDownIndexToUse = NegativeConditionDropdown.value - 1;
		if (m_AllPossibleConditions != null && dropDownIndexToUse >= 0 && dropDownIndexToUse < m_AllPossibleConditions.Count)
		{
			CCondition.ENegativeCondition eNegativeCondition = CCondition.NegativeConditions.FirstOrDefault((CCondition.ENegativeCondition c) => c.ToString() == m_AllPossibleConditions[dropDownIndexToUse]);
			if (eNegativeCondition != CCondition.ENegativeCondition.NA && !m_TrapShowingFor.Conditions.Contains(eNegativeCondition))
			{
				m_TrapShowingFor.Conditions.Add(eNegativeCondition);
				m_TrapShowingFor.AdjacentConditions.Add(eNegativeCondition);
				FillList();
			}
		}
	}
}
