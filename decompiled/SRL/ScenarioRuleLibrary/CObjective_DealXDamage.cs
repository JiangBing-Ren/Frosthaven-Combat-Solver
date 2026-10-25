using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjective_DealXDamage : CObjective
{
	public List<int> TargetDamage { get; private set; }

	public int CurrentDamageDealt { get; private set; }

	public override bool RemovesFromUIOnComplete => false;

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
						return "GUI_OBJECTIVE_DEAL_X_DAMAGE_WIN_FILTERED";
					}
					if (Result == EObjectiveResult.Lose)
					{
						return "GUI_OBJECTIVE_DEAL_X_DAMAGE_LOSE_FILTERED";
					}
				}
				else
				{
					if (Result == EObjectiveResult.Win)
					{
						return "GUI_OBJECTIVE_DEAL_X_DAMAGE_WIN";
					}
					if (Result == EObjectiveResult.Lose)
					{
						return "GUI_OBJECTIVE_DEAL_X_DAMAGE_LOSE";
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
		info.AddValue("TargetDamage", TargetDamage);
		info.AddValue("CurrentDamageDealt", CurrentDamageDealt);
	}

	public CObjective_DealXDamage(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "TargetDamage"))
				{
					if (name == "CurrentDamageDealt")
					{
						CurrentDamageDealt = info.GetInt32("CurrentDamageDealt");
					}
				}
				else
				{
					TargetDamage = (List<int>)info.GetValue("TargetDamage", typeof(List<int>));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CObjective_DealXDamage entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CObjective_DealXDamage(EObjectiveResult result, CObjectiveFilter objectiveFilter, List<int> targetDamage, bool hideCounter, bool activeFromStart = true, int? activateOnRound = null, string customLoc = null, string customTileHoverLoc = null, string eventIdentifier = null, bool isHidden = false, bool isOptional = false, bool winDespiteExhaustion = false, bool enoughToWinAlone = false, Dictionary<string, bool> requiredObjectiveStates = null)
		: base(EObjectiveType.DealXDamage, result, objectiveFilter, hideCounter, activeFromStart, activateOnRound, customLoc, customTileHoverLoc, eventIdentifier, isHidden, isOptional, winDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates)
	{
		TargetDamage = targetDamage;
		CurrentDamageDealt = 0;
	}

	public override bool CheckObjectiveComplete(int partySize, bool isEndOfRound = false)
	{
		int index = Math.Max(0, partySize - 1);
		IsComplete = CurrentDamageDealt >= TargetDamage[index] && CheckOtherObjectiveStatesRequirement();
		return IsComplete;
	}

	public override float GetObjectiveProgress(int partySize, out int total, out int current)
	{
		int index = Math.Max(0, partySize - 1);
		total = TargetDamage[index];
		current = Math.Min(CurrentDamageDealt, TargetDamage[index]);
		return Math.Min(1f, (float)CurrentDamageDealt / (float)TargetDamage[index]);
	}

	public void SetActorDamaged(ActorState actorBeingDamaged, int damageReceived)
	{
		if (ObjectiveFilter.IsValidTarget(actorBeingDamaged))
		{
			CurrentDamageDealt += damageReceived;
		}
	}
}
