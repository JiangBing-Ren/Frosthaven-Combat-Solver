using System.Collections.Generic;
using System.Linq;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using UnityEngine;

public class BattleGoalContainer : MonoBehaviour
{
	[SerializeField]
	private List<UIScenarioPlayerBattleGoal> playerBattlePool = new List<UIScenarioPlayerBattleGoal>();

	private Dictionary<CPlayerActor, List<UIScenarioPlayerBattleGoal>> assignedBattleGoals = new Dictionary<CPlayerActor, List<UIScenarioPlayerBattleGoal>>();

	public void Init(List<CBattleGoalState> battleGoals)
	{
		assignedBattleGoals.Clear();
		if (battleGoals == null)
		{
			HelperTools.NormalizePool(ref playerBattlePool, playerBattlePool[0].gameObject, playerBattlePool[0].transform.parent, 0);
			return;
		}
		HelperTools.NormalizePool(ref playerBattlePool, playerBattlePool[0].gameObject, playerBattlePool[0].transform.parent, battleGoals.Count, (UIScenarioPlayerBattleGoal ui) =>
		{
			ui.gameObject.SetActive(value: false);
		});
		if (ScenarioManager.Scenario.AllPlayers.Count <= 0)
		{
			return;
		}
		foreach (CPlayerActor item in ScenarioManager.Scenario.AllPlayers.Where((CPlayerActor x) => battleGoals.Any((CBattleGoalState y) => y.BattleGoalConditionState.OverrideCharacterID == x.CharacterClass.CharacterID)))
		{
			assignedBattleGoals[item] = new List<UIScenarioPlayerBattleGoal>();
		}
		int i;
		for (i = 0; i < battleGoals.Count; i++)
		{
			CPlayerActor cPlayerActor = ScenarioManager.Scenario.AllPlayers.First((CPlayerActor it) => it.CharacterClass.CharacterID == battleGoals[i].BattleGoalConditionState.OverrideCharacterID);
			playerBattlePool[i].Init(cPlayerActor.CharacterClass.CharacterModel, battleGoals[i], cPlayerActor.CharacterClass.CharacterYML.CustomCharacterConfig);
			assignedBattleGoals[cPlayerActor].Add(playerBattlePool[i]);
			battleGoals[i].BattleGoalConditionState.UpdateGoal(battleGoals[i].BattleGoalConditionState.OverrideCharacterID);
		}
	}

	public void Show(CPlayerActor playerActor)
	{
		foreach (KeyValuePair<CPlayerActor, List<UIScenarioPlayerBattleGoal>> assignedBattleGoal in assignedBattleGoals)
		{
			if (assignedBattleGoal.Key != playerActor || (FFSNetwork.IsOnline && !playerActor.IsUnderMyControl()))
			{
				Hide(assignedBattleGoal.Key);
				continue;
			}
			foreach (UIScenarioPlayerBattleGoal item in assignedBattleGoals[playerActor])
			{
				item.Show();
			}
		}
	}

	public void Hide(CPlayerActor playerActor)
	{
		if (playerActor == null || !assignedBattleGoals.ContainsKey(playerActor))
		{
			return;
		}
		foreach (UIScenarioPlayerBattleGoal item in assignedBattleGoals[playerActor])
		{
			item.Hide();
		}
	}

	public void Hide()
	{
		foreach (List<UIScenarioPlayerBattleGoal> value in assignedBattleGoals.Values)
		{
			foreach (UIScenarioPlayerBattleGoal item in value)
			{
				item.Hide();
			}
		}
	}

	public void UpdateProgress(CPlayerActor playerActor)
	{
		if (!assignedBattleGoals.ContainsKey(playerActor) || (FFSNetwork.IsOnline && !playerActor.IsUnderMyControl()))
		{
			return;
		}
		foreach (UIScenarioPlayerBattleGoal item in assignedBattleGoals[playerActor])
		{
			item.UpdateProgress();
		}
	}

	public void UpdateProgress()
	{
		foreach (KeyValuePair<CPlayerActor, List<UIScenarioPlayerBattleGoal>> item in assignedBattleGoals.Where((KeyValuePair<CPlayerActor, List<UIScenarioPlayerBattleGoal>> it) => !FFSNetwork.IsOnline || it.Key.IsUnderMyControl()))
		{
			foreach (UIScenarioPlayerBattleGoal item2 in item.Value)
			{
				item2.UpdateProgress();
			}
		}
	}

	public bool AllComplete(CPlayerActor playerActor)
	{
		if (playerActor == null)
		{
			return false;
		}
		if (assignedBattleGoals.ContainsKey(playerActor) && (!FFSNetwork.IsOnline || playerActor.IsUnderMyControl()))
		{
			bool result = true;
			foreach (UIScenarioPlayerBattleGoal item in assignedBattleGoals[playerActor])
			{
				if (!item.IsComplete())
				{
					result = false;
					break;
				}
			}
			return result;
		}
		return false;
	}

	public bool AllFailed(CPlayerActor playerActor)
	{
		if (playerActor == null)
		{
			return false;
		}
		if (assignedBattleGoals.ContainsKey(playerActor) && (!FFSNetwork.IsOnline || playerActor.IsUnderMyControl()))
		{
			bool result = true;
			foreach (UIScenarioPlayerBattleGoal item in assignedBattleGoals[playerActor])
			{
				if (!item.IsFailed())
				{
					result = false;
					break;
				}
			}
			return result;
		}
		return false;
	}
}
