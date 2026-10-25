using System;
using MapRuleLibrary.Party;
using UnityEngine;

public class UIBattleGoalProgress : MonoBehaviour
{
	[SerializeField]
	protected TextLocalizedListener title;

	[SerializeField]
	protected TextLocalizedListener description;

	[SerializeField]
	protected ImageProgressBar progressBar;

	protected CBattleGoalState m_BattleGoal;

	public virtual void SetBattleGoal(CBattleGoalState battleGoal)
	{
		m_BattleGoal = battleGoal;
		title.SetTextKey(battleGoal.BattleGoal.LocalisedName);
		description.SetTextKey(battleGoal.BattleGoal.LocalisedDescription);
		UpdateProgress(playAnimation: false);
	}

	protected void UpdateProgress(int total, int current, bool playAnimation = true, Action onFinishedAnimation = null)
	{
		if (playAnimation)
		{
			progressBar.PlayProgressTo(current, total, onFinishedAnimation);
			return;
		}
		progressBar.SetAmount(current, total);
		onFinishedAnimation?.Invoke();
	}

	public virtual void UpdateProgress(bool playAnimation = true)
	{
		int totalConditionsAndTargets = m_BattleGoal.BattleGoalConditionState.TotalConditionsAndTargets;
		int currentProgress = m_BattleGoal.BattleGoalConditionState.CurrentProgress;
		UpdateProgress(totalConditionsAndTargets, currentProgress);
	}
}
