using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjective_DeactivateXSpawners : CObjective
{
	public List<int> NumberToDeactivate { get; private set; }

	public int CurrentDeactivatedAmount { get; private set; }

	public override string LocKey
	{
		get
		{
			if (string.IsNullOrEmpty(CustomLocKey))
			{
				if (Result == EObjectiveResult.Win)
				{
					return "GUI_OBJECTIVE_DEACTIVATE_X_SPAWNERS_WIN";
				}
				if (Result == EObjectiveResult.Lose)
				{
					return "GUI_OBJECTIVE_DEACTIVATE_X_SPAWNERS_LOSE";
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
		info.AddValue("NumberToDeactivate", NumberToDeactivate);
		info.AddValue("CurrentDeactivatedAmount", CurrentDeactivatedAmount);
	}

	public CObjective_DeactivateXSpawners(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "NumberToDeactivate"))
				{
					if (name == "CurrentDeactivatedAmount")
					{
						CurrentDeactivatedAmount = info.GetInt32("CurrentDeactivatedAmount");
					}
				}
				else
				{
					NumberToDeactivate = (List<int>)info.GetValue("NumberToDeactivate", typeof(List<int>));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CObjective_DeactivateXSpawners entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CObjective_DeactivateXSpawners(EObjectiveResult result, CObjectiveFilter objectiveFilter, List<int> numberToActivate, bool hideCounter, bool activeFromStart = true, int? activateOnRound = null, string customLoc = null, string customTileHoverLoc = null, string eventIdentifier = null, bool isHidden = false, bool isOptional = false, bool winDespiteExhaustion = false, bool enoughToWinAlone = false, Dictionary<string, bool> requiredObjectiveStates = null)
		: base(EObjectiveType.DeactivateXSpawners, result, objectiveFilter, hideCounter, activeFromStart, activateOnRound, customLoc, customTileHoverLoc, eventIdentifier, isHidden, isOptional, winDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates)
	{
		NumberToDeactivate = numberToActivate;
		CurrentDeactivatedAmount = 0;
	}

	public override bool CheckObjectiveComplete(int partySize, bool isEndOfRound = false)
	{
		int num = 0;
		foreach (CInteractableSpawner item in ScenarioManager.CurrentScenarioState.Spawners.OfType<CInteractableSpawner>())
		{
			if (item.Deactivated)
			{
				num++;
			}
		}
		CurrentDeactivatedAmount = num;
		int index = Math.Max(0, partySize - 1);
		IsComplete = num >= NumberToDeactivate[index] && CheckOtherObjectiveStatesRequirement();
		return IsComplete;
	}

	public override float GetObjectiveProgress(int partySize, out int total, out int current)
	{
		int index = Math.Max(0, partySize - 1);
		total = NumberToDeactivate[index];
		current = Math.Min(CurrentDeactivatedAmount, total);
		return Math.Min(1f, (float)CurrentDeactivatedAmount / (float)NumberToDeactivate[index]);
	}

	public override int GetObjectiveCompletionValue(int partySize)
	{
		int index = Math.Max(0, partySize - 1);
		return NumberToDeactivate[index];
	}
}
