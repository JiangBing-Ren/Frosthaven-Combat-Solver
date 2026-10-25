using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjective_ReachRound : CObjective
{
	public int CustomRoundCounter = -1;

	public int ReachRoundNumber;

	public bool CountFromActivationRound { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("CustomRoundCounter", CustomRoundCounter);
		info.AddValue("ReachRoundNumber", ReachRoundNumber);
		info.AddValue("CountFromActivationRound", CountFromActivationRound);
	}

	public CObjective_ReachRound(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "CustomRoundCounter":
					CustomRoundCounter = info.GetInt32("CustomRoundCounter");
					break;
				case "ReachRoundNumber":
					ReachRoundNumber = info.GetInt32("ReachRoundNumber");
					break;
				case "CountFromActivationRound":
					CountFromActivationRound = info.GetBoolean("CountFromActivationRound");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CObjective_ReachRound entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CObjective_ReachRound(EObjectiveResult result, CObjectiveFilter objectiveFilter, int reachRoundNumber, bool countFromActivationRound, bool hideCounter, bool activeFromStart = true, int? activateOnRound = null, string customLoc = null, string customTileHoverLoc = null, string eventIdentifier = null, bool isHidden = false, bool isOptional = false, bool winDespiteExhaustion = false, bool enoughToWinAlone = false, Dictionary<string, bool> requiredObjectiveStates = null)
		: base(EObjectiveType.ReachRound, result, objectiveFilter, hideCounter, activeFromStart, activateOnRound, customLoc, customTileHoverLoc, eventIdentifier, isHidden, isOptional, winDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates)
	{
		CustomRoundCounter = -1;
		ReachRoundNumber = reachRoundNumber;
		CountFromActivationRound = countFromActivationRound;
	}

	public override bool CheckObjectiveComplete(int partySize, bool isEndOfRound = false)
	{
		if (CustomRoundCounter >= 0)
		{
			int num = ReachRoundNumber;
			if (CountFromActivationRound)
			{
				num += RoundActivatedOn;
			}
			IsComplete = CustomRoundCounter >= num && CheckOtherObjectiveStatesRequirement();
		}
		else
		{
			int num2 = (isEndOfRound ? ScenarioManager.CurrentScenarioState.RoundNumber : (ScenarioManager.CurrentScenarioState.RoundNumber - 1));
			int reachRoundNumber = ReachRoundNumber;
			reachRoundNumber += (CountFromActivationRound ? RoundActivatedOn : 0);
			IsComplete = num2 >= reachRoundNumber && CheckOtherObjectiveStatesRequirement();
		}
		return IsComplete;
	}

	public override float GetObjectiveProgress(int partySize, out int total, out int current)
	{
		if (CustomRoundCounter >= 0)
		{
			total = ReachRoundNumber;
			int num = (CountFromActivationRound ? RoundActivatedOn : 0);
			int num2 = CustomRoundCounter - num;
			current = Math.Min(num2, total);
			return Math.Min(1f, (float)num2 / (float)ReachRoundNumber);
		}
		total = ReachRoundNumber;
		int num3 = ((!CountFromActivationRound) ? 1 : RoundActivatedOn);
		int num4 = ScenarioManager.CurrentScenarioState.RoundNumber - num3;
		current = Math.Min(num4, total);
		return Math.Min(1f, (float)num4 / (float)ReachRoundNumber);
	}

	public override int GetObjectiveCompletionValue(int partySize)
	{
		return ReachRoundNumber;
	}
}
