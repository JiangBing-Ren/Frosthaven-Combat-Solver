using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIRulesetInventory : MonoBehaviour
{
	[Serializable]
	public class RulesetEvent : UnityEvent<IRuleset>
	{
	}

	[SerializeField]
	private List<UIRulesetSlot> rulesetSlotPool;

	[SerializeField]
	private ScrollRect rulesetsScroll;

	[SerializeField]
	private ToggleGroup rulesetsToggleGroup;

	public RulesetEvent OnSelectedRuleset = new RulesetEvent();

	public RulesetEvent OnDeselectedRuleset = new RulesetEvent();

	public RulesetEvent OnHoveredRuleset = new RulesetEvent();

	public RulesetEvent OnUnhoveredRuleset = new RulesetEvent();

	private Dictionary<IRuleset, UIRulesetSlot> assignedRulesets = new Dictionary<IRuleset, UIRulesetSlot>();

	private IRuleset selectedRuleset;

	public IRuleset SelectedRuleset => selectedRuleset;

	public void Setup(List<IRuleset> rulesets)
	{
		selectedRuleset = null;
		assignedRulesets.Clear();
		for (int i = 0; i < rulesets.Count; i++)
		{
			Add(rulesets[i], highlight: false);
		}
		for (int j = rulesets.Count; j < rulesetSlotPool.Count; j++)
		{
			rulesetSlotPool[j].gameObject.SetActive(value: false);
		}
		rulesetsScroll.verticalNormalizedPosition = 1f;
	}

	private void OnHovered(IRuleset ruleset, bool hovered)
	{
		if (selectedRuleset == null)
		{
			if (hovered)
			{
				OnHoveredRuleset.Invoke(ruleset);
			}
			else
			{
				OnUnhoveredRuleset.Invoke(ruleset);
			}
		}
	}

	private void OnToggled(IRuleset ruleset, bool toggled)
	{
		if (toggled)
		{
			if (ruleset != selectedRuleset)
			{
				selectedRuleset = ruleset;
				OnSelectedRuleset.Invoke(selectedRuleset);
			}
		}
		else if (ruleset == selectedRuleset)
		{
			selectedRuleset = null;
			OnDeselectedRuleset.Invoke(selectedRuleset);
			if (assignedRulesets[ruleset].IsHovered)
			{
				OnHovered(ruleset, hovered: true);
			}
		}
	}

	public void ClearSelectedRuleset()
	{
		if (selectedRuleset != null)
		{
			assignedRulesets[selectedRuleset].SetSelected(selected: false);
		}
		selectedRuleset = null;
	}

	private UIRulesetSlot GetSlot()
	{
		if (assignedRulesets.Count < rulesetSlotPool.Count)
		{
			return rulesetSlotPool[assignedRulesets.Count];
		}
		UIRulesetSlot uIRulesetSlot = UnityEngine.Object.Instantiate(rulesetSlotPool[0], rulesetsScroll.content);
		rulesetSlotPool.Add(uIRulesetSlot);
		return uIRulesetSlot;
	}

	public void Add(IRuleset ruleset, bool highlight = true)
	{
		if (!assignedRulesets.ContainsKey(ruleset))
		{
			UIRulesetSlot slot = GetSlot();
			assignedRulesets[ruleset] = slot;
			slot.SetRuleset(ruleset, rulesetsToggleGroup, OnToggled, OnHovered);
			slot.gameObject.SetActive(value: true);
			if (highlight)
			{
				slot.Highlight();
			}
		}
	}

	public void Remove(IRuleset ruleset)
	{
		if (assignedRulesets.ContainsKey(ruleset))
		{
			if (selectedRuleset == ruleset)
			{
				selectedRuleset = null;
			}
			UIRulesetSlot uIRulesetSlot = assignedRulesets[ruleset];
			uIRulesetSlot.gameObject.SetActive(value: false);
			assignedRulesets.Remove(ruleset);
			rulesetSlotPool.Remove(uIRulesetSlot);
			rulesetSlotPool.Add(uIRulesetSlot);
			uIRulesetSlot.transform.SetAsLastSibling();
		}
	}

	public void Replace(IRuleset oldRuleset, IRuleset newRuleset)
	{
		if (oldRuleset == selectedRuleset)
		{
			selectedRuleset = newRuleset;
		}
		UIRulesetSlot uIRulesetSlot = assignedRulesets[oldRuleset];
		assignedRulesets.Remove(oldRuleset);
		assignedRulesets[newRuleset] = uIRulesetSlot;
		uIRulesetSlot.SetRuleset(newRuleset, rulesetsToggleGroup, OnToggled, OnHovered);
		uIRulesetSlot.SetSelected(selected: true);
	}
}
