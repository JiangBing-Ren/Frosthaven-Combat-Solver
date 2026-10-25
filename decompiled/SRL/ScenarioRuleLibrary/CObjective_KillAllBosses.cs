using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjective_KillAllBosses : CObjective
{
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
	}

	public CObjective_KillAllBosses(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public CObjective_KillAllBosses(EObjectiveResult result, CObjectiveFilter objectiveFilter, bool hideCounter, bool activeFromStart = true, int? activateOnRound = null, string customLoc = null, string customTileHoverLoc = null, string eventIdentifier = null, bool isHidden = false, bool isOptional = false, bool winDespiteExhaustion = false, bool enoughToWinAlone = false, Dictionary<string, bool> requiredObjectiveStates = null)
		: base(EObjectiveType.KillAllBosses, result, objectiveFilter, hideCounter, activeFromStart, activateOnRound, customLoc, customTileHoverLoc, eventIdentifier, isHidden, isOptional, winDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates)
	{
	}

	public override bool CheckObjectiveComplete(int partySize, bool isEndOfRound = false)
	{
		IsComplete = !ScenarioManager.CurrentScenarioState.Monsters.Any((EnemyState a) => !a.IsHiddenForCurrentPartySize && a.Enemy != null && !a.Enemy.IsDeadForObjectives && a.IsBoss) && CheckOtherObjectiveStatesRequirement();
		return IsComplete;
	}

	public override float GetObjectiveProgress(int partySize, out int total, out int current)
	{
		total = ScenarioManager.CurrentScenarioState.Monsters.Where((EnemyState w) => !w.IsHiddenForCurrentPartySize && w.IsBoss).Count();
		current = ScenarioManager.CurrentScenarioState.Monsters.Where((EnemyState w) => !w.IsHiddenForCurrentPartySize && w.IsBoss && (w.Enemy == null || w.Enemy.IsDeadForObjectives)).Count();
		return (float)current / (float)total;
	}
}
