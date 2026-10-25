using System;
using System.Collections.Generic;
using FHGUI.Common;
using FHGUI.DTO;
using FHGUI.Elements;
using UnityEngine;

namespace FHGUI.Modules;

public class CharacterGoalsModule : FHMapViewModule
{
	public delegate void GoalEventHandler(string goalId);

	public GameObject ContainerMasteries;

	private List<UIGoalSummary> _summaries = new List<UIGoalSummary>();

	protected override bool PlaySoundOnEnable => false;

	private void InitCache()
	{
		_summaries.Clear();
		_summaries.AddRange(ContainerMasteries.GetComponentsInChildren<UIGoalSummary>());
	}

	public void Init(List<GoalDTO> goals, GoalEventHandler onBattleGoalSelected = null)
	{
		if (_summaries.Count == 0)
		{
			InitCache();
		}
		SetGoals(goals, _summaries, ContainerMasteries, null);
		if (goals != null)
		{
			_ = goals.Count > 0;
		}
		else
			_ = 0;
	}

	private static void SetGoals(List<GoalDTO> goals, List<UIGoalSummary> rowList, GameObject container, GoalEventHandler onGoalSelected)
	{
		if (goals == null || goals.Count == 0)
		{
			container.SetActive(value: false);
			return;
		}
		container.SetActive(value: true);
		HelperTools.NormalizePool(ref rowList, rowList[0].gameObject, container.transform, goals.Count);
		for (int i = 0; i < goals.Count; i++)
		{
			string goalId = goals[i].Id;
			UIGoalSummary.Params obj = new UIGoalSummary.Params
			{
				DTO = goals[i],
				OnClickedCallback = ((onGoalSelected != null) ? ((Action)(() =>
				{
					onGoalSelected(goalId);
				})) : null)
			};
			rowList[i].Init(obj);
		}
	}

	protected override void OnShow()
	{
		base.OnShow();
		UIContextualTutorialModule.OnEvent("Masteries", this);
	}
}
