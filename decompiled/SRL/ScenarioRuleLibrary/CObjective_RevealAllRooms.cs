using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjective_RevealAllRooms : CObjective
{
	public override string LocKey
	{
		get
		{
			if (string.IsNullOrEmpty(CustomLocKey))
			{
				if (Result == EObjectiveResult.Win)
				{
					return "GUI_OBJECTIVE_REVEAL_ALL_ROOMS_WIN";
				}
				if (Result == EObjectiveResult.Lose)
				{
					return "GUI_OBJECTIVE_REVEAL_ALL_ROOMS_LOSE";
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
	}

	public CObjective_RevealAllRooms(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public CObjective_RevealAllRooms(EObjectiveResult result, CObjectiveFilter objectiveFilter, bool hideCounter, bool activeFromStart = true, int? activateOnRound = null, string customLoc = null, string customTileHoverLoc = null, string eventIdentifier = null, bool isHidden = false, bool isOptional = false, bool winDespiteExhaustion = false, bool enoughToWinAlone = false, Dictionary<string, bool> requiredObjectiveStates = null)
		: base(EObjectiveType.RevealAllRooms, result, objectiveFilter, hideCounter, activeFromStart, activateOnRound, customLoc, customTileHoverLoc, eventIdentifier, isHidden, isOptional, winDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates)
	{
	}

	public override bool CheckObjectiveComplete(int partySize, bool isEndOfRound = false)
	{
		bool flag = true;
		foreach (CMap map in ScenarioManager.CurrentScenarioState.Maps)
		{
			if (!map.Revealed)
			{
				flag = false;
				break;
			}
		}
		IsComplete = flag && CheckOtherObjectiveStatesRequirement();
		return IsComplete;
	}

	public override float GetObjectiveProgress(int partySize, out int total, out int current)
	{
		current = ScenarioManager.CurrentScenarioState.Maps.Where((CMap m) => m.Revealed).Count();
		total = ScenarioManager.CurrentScenarioState.Maps.Count;
		return Math.Min(1f, (float)current / (float)total);
	}

	public override int GetObjectiveCompletionValue(int partySize)
	{
		return 1;
	}
}
