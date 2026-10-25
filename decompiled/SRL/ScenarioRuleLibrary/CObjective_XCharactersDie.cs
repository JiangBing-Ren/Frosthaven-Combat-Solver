using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjective_XCharactersDie : CObjective
{
	public List<int> KillAmount { get; private set; }

	public int CurrentKills { get; private set; }

	public override string LocKey
	{
		get
		{
			if (string.IsNullOrEmpty(CustomLocKey))
			{
				if (ObjectiveFilter.FilterHasValues)
				{
					if (Result == EObjectiveResult.Win)
					{
						return "GUI_OBJECTIVE_KILL_X_WIN_FILTERED";
					}
					if (Result == EObjectiveResult.Lose)
					{
						return "GUI_OBJECTIVE_KILL_X_LOSE_FILTERED";
					}
				}
				else
				{
					if (Result == EObjectiveResult.Win)
					{
						return "GUI_OBJECTIVE_KILL_X_WIN";
					}
					if (Result == EObjectiveResult.Lose)
					{
						return "GUI_OBJECTIVE_KILL_X_LOSE";
					}
				}
				return string.Empty;
			}
			return CustomLocKey;
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("KillAmount", KillAmount);
	}

	public CObjective_XCharactersDie(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item2 in info)
		{
			try
			{
				if (item2.Name == "KillAmount")
				{
					try
					{
						KillAmount = (List<int>)info.GetValue("KillAmount", typeof(List<int>));
					}
					catch
					{
						int item = (int)info.GetValue("KillAmount", typeof(int));
						KillAmount = new List<int> { item, item, item, item };
					}
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CObjective_XCharactersDie entry " + item2.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CObjective_XCharactersDie(EObjectiveResult result, CObjectiveFilter objectiveFilter, List<int> killAmount, bool hideCounter, bool activeFromStart = true, int? activateOnRound = null, string customLoc = null, string customTileHoverLoc = null, string eventIdentifier = null, bool isHidden = false, bool isOptional = false, bool winDespiteExhaustion = false, bool enoughToWinAlone = false, Dictionary<string, bool> requiredObjectiveStates = null)
		: base(EObjectiveType.XCharactersDie, result, objectiveFilter, hideCounter, activeFromStart, activateOnRound, customLoc, customTileHoverLoc, eventIdentifier, isHidden, isOptional, winDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates)
	{
		KillAmount = killAmount;
		CurrentKills = 0;
	}

	public override bool CheckObjectiveComplete(int partySize, bool isEndOfRound = false)
	{
		int num = 0;
		foreach (HeroSummonState heroSummon2 in ScenarioManager.CurrentScenarioState.HeroSummons)
		{
			CActor heroSummon = heroSummon2.HeroSummon;
			if (heroSummon != null && heroSummon.IsDeadForObjectives && ObjectiveFilter.IsValidTarget(heroSummon2))
			{
				num++;
			}
		}
		foreach (PlayerState player2 in ScenarioManager.CurrentScenarioState.Players)
		{
			CActor player = player2.Player;
			if (player != null && player.IsDeadForObjectives && ObjectiveFilter.IsValidTarget(player2))
			{
				num++;
			}
		}
		foreach (EnemyState allEnemyState in ScenarioManager.CurrentScenarioState.AllEnemyStates)
		{
			if (!allEnemyState.IsHiddenForCurrentPartySize)
			{
				CActor enemy = allEnemyState.Enemy;
				if (enemy != null && enemy.IsDeadForObjectives && ObjectiveFilter.IsValidTarget(allEnemyState))
				{
					num++;
				}
			}
		}
		CurrentKills = num;
		int index = Math.Max(0, partySize - 1);
		IsComplete = num >= KillAmount[index] && CheckOtherObjectiveStatesRequirement();
		return IsComplete;
	}

	public override float GetObjectiveProgress(int partySize, out int total, out int current)
	{
		int index = Math.Max(0, partySize - 1);
		total = KillAmount[index];
		current = Math.Min(CurrentKills, KillAmount[index]);
		return Math.Min(1f, (float)CurrentKills / (float)KillAmount[index]);
	}

	public override int GetObjectiveCompletionValue(int partySize)
	{
		int index = Math.Max(0, partySize - 1);
		return KillAmount[index];
	}
}
