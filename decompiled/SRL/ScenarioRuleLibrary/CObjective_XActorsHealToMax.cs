using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjective_XActorsHealToMax : CObjective
{
	public List<int> ActorAmount { get; private set; }

	public int CurrentActorsFullHealth { get; private set; }

	public override string LocKey
	{
		get
		{
			if (string.IsNullOrEmpty(CustomLocKey))
			{
				return string.Empty;
			}
			return CustomLocKey;
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ActorAmount", ActorAmount);
	}

	public CObjective_XActorsHealToMax(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				if (item.Name == "ActorAmount")
				{
					ActorAmount = (List<int>)info.GetValue("ActorAmount", typeof(List<int>));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CObjective_XActorsHealToMax entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CObjective_XActorsHealToMax(EObjectiveResult result, CObjectiveFilter objectiveFilter, List<int> actorAmount, bool hideCounter, bool activeFromStart = true, int? activateOnRound = null, string customLoc = null, string customTileHoverLoc = null, string eventIdentifier = null, bool isHidden = false, bool isOptional = false, bool winDespiteExhaustion = false, bool enoughToWinAlone = false, Dictionary<string, bool> requiredObjectiveStates = null)
		: base(EObjectiveType.XActorsHealToMax, result, objectiveFilter, hideCounter, activeFromStart, activateOnRound, customLoc, customTileHoverLoc, eventIdentifier, isHidden, isOptional, winDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates)
	{
		ActorAmount = actorAmount;
		CurrentActorsFullHealth = 0;
	}

	public override bool CheckObjectiveComplete(int partySize, bool isEndOfRound = false)
	{
		int num = 0;
		foreach (HeroSummonState heroSummon2 in ScenarioManager.CurrentScenarioState.HeroSummons)
		{
			CActor heroSummon = heroSummon2.HeroSummon;
			if (heroSummon != null && heroSummon.Health >= heroSummon.OriginalMaxHealth && ObjectiveFilter.IsValidTarget(heroSummon2))
			{
				num++;
			}
		}
		foreach (PlayerState player2 in ScenarioManager.CurrentScenarioState.Players)
		{
			CActor player = player2.Player;
			if (player != null && player.Health >= player.OriginalMaxHealth && ObjectiveFilter.IsValidTarget(player2))
			{
				num++;
			}
		}
		foreach (EnemyState allEnemyState in ScenarioManager.CurrentScenarioState.AllEnemyStates)
		{
			if (!allEnemyState.IsHiddenForCurrentPartySize)
			{
				CActor enemy = allEnemyState.Enemy;
				if (enemy != null && enemy.Health >= enemy.OriginalMaxHealth && ObjectiveFilter.IsValidTarget(allEnemyState))
				{
					num++;
				}
			}
		}
		CurrentActorsFullHealth = num;
		int index = Math.Max(0, partySize - 1);
		IsComplete = num >= ActorAmount[index] && CheckOtherObjectiveStatesRequirement();
		return IsComplete;
	}

	public override float GetObjectiveProgress(int partySize, out int total, out int current)
	{
		int index = Math.Max(0, partySize - 1);
		total = ActorAmount[index];
		current = Math.Min(CurrentActorsFullHealth, ActorAmount[index]);
		return Math.Min(1f, (float)CurrentActorsFullHealth / (float)ActorAmount[index]);
	}

	public override int GetObjectiveCompletionValue(int partySize)
	{
		int index = Math.Max(0, partySize - 1);
		return ActorAmount[index];
	}
}
