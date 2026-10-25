using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using UnityEngine;

public class LevelEditorConditionsComponent : MonoBehaviour
{
	public LevelEditorItemListFromComboComponent PositiveDropDown;

	public LevelEditorItemListFromComboComponent NegativeDropDown;

	private List<CCondition.ENegativeCondition> neg;

	private List<CCondition.EPositiveCondition> pos;

	private void Awake()
	{
		PositiveDropDown.Setup((int i) => CCondition.PositiveConditions[i].ToString(), () => pos.Select((CCondition.EPositiveCondition p) => CCondition.PositiveConditions.IndexOf(p)).ToList(), CCondition.PositiveConditions.Select((CCondition.EPositiveCondition s) => s.ToString()).ToList());
		NegativeDropDown.Setup((int i) => CCondition.NegativeConditions[i].ToString(), () => neg.Select((CCondition.ENegativeCondition n) => CCondition.NegativeConditions.IndexOf(n)).ToList(), CCondition.NegativeConditions.Select((CCondition.ENegativeCondition s) => s.ToString()).ToList());
	}

	public void SetConditions(List<CCondition.EPositiveCondition> positiveConditions, List<CCondition.ENegativeCondition> negativeConditions)
	{
		pos = positiveConditions;
		neg = negativeConditions;
		RefreshUI();
	}

	public void GetConditions(out List<CCondition.EPositiveCondition> positiveConditions, out List<CCondition.ENegativeCondition> negativeConditions)
	{
		pos = (from i in PositiveDropDown.GetItems()
			select (CCondition.EPositiveCondition)Enum.Parse(typeof(CCondition.EPositiveCondition), i)).ToList();
		neg = (from i in NegativeDropDown.GetItems()
			select (CCondition.ENegativeCondition)Enum.Parse(typeof(CCondition.ENegativeCondition), i)).ToList();
		positiveConditions = pos;
		negativeConditions = neg;
	}

	public void Clear()
	{
		SetConditions(new List<CCondition.EPositiveCondition>(), new List<CCondition.ENegativeCondition>());
	}

	private void RefreshUI()
	{
		PositiveDropDown.RefreshUi();
		NegativeDropDown.RefreshUi();
	}
}
