using System.Collections.Generic;
using FHGUI.Common;
using FHGUI.Scenario;
using FHGUI.Scenario.DTO;
using FHGUI.Scenario.Services;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using UnityEngine;

public class ScenarioInfoModule : Singleton<ScenarioInfoModule>
{
	[SerializeField]
	private MissionObjectiveContainer missionObjectiveContainer;

	[SerializeField]
	private ScenarioModifierContainer scenarioModifierContainer;

	[SerializeField]
	private BattleGoalContainer battleGoalContainer;

	[SerializeField]
	private BattleGoalContainer masteriesContainer;

	[SerializeField]
	private FHButton ButtonScenarioRules;

	[SerializeField]
	private GameObject NewScenarioRulesNotification;

	[SerializeField]
	private FHTabButton TabButtonQuestGoals;

	[SerializeField]
	private FHTabButton TabButtonBattleGoals;

	[SerializeField]
	private GameObject TabButtonBattleGoalsFailed;

	[SerializeField]
	private GameObject TabButtonBattleGoalsCompleted;

	[SerializeField]
	private FHTabButton TabButtonMasteries;

	private bool m_Dirty;

	public MissionObjectiveContainer MissionObjectiveContainer => missionObjectiveContainer;

	public ScenarioModifierContainer ScenarioModifierContainer => scenarioModifierContainer;

	protected override void AwakeInit()
	{
		base.AwakeInit();
		ButtonScenarioRules.onClick.AddListener(HandleOnOpenScenarioRules);
		NewScenarioRulesNotification.gameObject.SetActive(value: false);
	}

	public void InitScenario(string scenarioID, List<CObjective> winObjectives, List<CObjective> loseObjectives, List<CScenarioModifier> scenarioModifiers, List<CBattleGoalState> battleGoalStates, List<CBattleGoalState> masteriesStates)
	{
		missionObjectiveContainer.Init(winObjectives, loseObjectives);
		scenarioModifierContainer.Init(scenarioID, scenarioModifiers);
		scenarioModifierContainer.gameObject.SetActive(!ScenarioMechanicViewService.HasSpecialRulesData());
		battleGoalContainer.Init(battleGoalStates);
		masteriesContainer.Init(masteriesStates);
		TabButtonQuestGoals.interactable = (winObjectives != null && winObjectives.Count > 0) || (loseObjectives != null && loseObjectives.Count > 0);
		TabButtonBattleGoals.interactable = battleGoalStates != null && battleGoalStates.Count > 0;
		TabButtonMasteries.interactable = masteriesStates != null && masteriesStates.Count > 0;
	}

	public void OnGoldValueChanged()
	{
	}

	public void ShowBattleGoals(CPlayerActor playerActor)
	{
		battleGoalContainer.Show(playerActor);
		masteriesContainer.Show(playerActor);
		UpdateTabButtonStates(playerActor);
	}

	private void UpdateTabButtonStates(CPlayerActor playerActor)
	{
		if (TabButtonBattleGoalsFailed != null)
		{
			TabButtonBattleGoalsFailed.SetActive(battleGoalContainer.AllFailed(playerActor));
		}
		if (TabButtonBattleGoalsCompleted != null)
		{
			TabButtonBattleGoalsCompleted.SetActive(battleGoalContainer.AllComplete(playerActor));
		}
	}

	public void HideBattleGoals()
	{
		battleGoalContainer.Hide();
		masteriesContainer.Hide();
	}

	public void HideBattleGoals(CPlayerActor playerActor)
	{
		battleGoalContainer.Hide(playerActor);
		masteriesContainer.Hide(playerActor);
	}

	public void UpdateProgress()
	{
		m_Dirty = false;
		battleGoalContainer.UpdateProgress();
		masteriesContainer.UpdateProgress();
		UpdateTabButtonStates(Singleton<CardsHandManager>.Instance?.CurrentHand?.PlayerActor);
	}

	public void UpdateNewRuleNotification(bool active)
	{
		NewScenarioRulesNotification.gameObject.SetActive(active);
	}

	private void HandleOnOpenScenarioRules()
	{
		ScenarioMechanicsInfoModule scenarioMechanicsInfoModule = Singleton<FHScenarioViewController>.InstanceFast?.ScenarioMechanics;
		if (scenarioMechanicsInfoModule == null)
		{
			return;
		}
		ScenarioMechanicsInfoModule.Params data = Singleton<FHScenarioViewController>.InstanceFast.GetService<ScenarioMechanicViewService>().GetData(scenarioMechanicsInfoModule.MechanicsData);
		scenarioMechanicsInfoModule.Init(data);
		foreach (ScenarioMechanicDTO scenarioRule in data.ScenarioRules)
		{
			if (!scenarioRule.Seen)
			{
				Singleton<FHScenarioViewController>.InstanceFast.GetService<ScenarioMechanicViewService>().SetMechanicsAsShown(scenarioRule.ID);
			}
		}
		UpdateNewRuleNotification(active: false);
	}

	private void LateUpdate()
	{
		if (m_Dirty)
		{
			UpdateProgress();
		}
	}

	public void MarkDirty()
	{
		m_Dirty = true;
	}
}
