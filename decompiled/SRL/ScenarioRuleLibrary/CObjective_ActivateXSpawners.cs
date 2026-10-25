using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjective_ActivateXSpawners : CObjective
{
	public List<int> NumberToActivate { get; private set; }

	public int CurrentActivatedAmount { get; private set; }

	public override string LocKey
	{
		get
		{
			if (string.IsNullOrEmpty(CustomLocKey))
			{
				if (Result == EObjectiveResult.Win)
				{
					return "GUI_OBJECTIVE_ACTIVATE_X_SPAWNERS_WIN";
				}
				if (Result == EObjectiveResult.Lose)
				{
					return "GUI_OBJECTIVE_ACTIVATE_X_SPAWNERS_LOSE";
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
		info.AddValue("NumberToActivate", NumberToActivate);
		info.AddValue("CurrentActivatedAmount", CurrentActivatedAmount);
	}

	public CObjective_ActivateXSpawners(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "NumberToActivate"))
				{
					if (name == "CurrentActivatedAmount")
					{
						CurrentActivatedAmount = info.GetInt32("CurrentActivatedAmount");
					}
				}
				else
				{
					NumberToActivate = (List<int>)info.GetValue("NumberToActivate", typeof(List<int>));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CObjective_ActivateXSpawners entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CObjective_ActivateXSpawners(EObjectiveResult result, CObjectiveFilter objectiveFilter, List<int> numberToActivate, bool hideCounter, bool activeFromStart = true, int? activateOnRound = null, string customLoc = null, string customTileHoverLoc = null, string eventIdentifier = null, bool isHidden = false, bool isOptional = false, bool winDespiteExhaustion = false, bool enoughToWinAlone = false, Dictionary<string, bool> requiredObjectiveStates = null)
		: base(EObjectiveType.ActivateXSpawners, result, objectiveFilter, hideCounter, activeFromStart, activateOnRound, customLoc, customTileHoverLoc, eventIdentifier, isHidden, isOptional, winDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates)
	{
		NumberToActivate = numberToActivate;
		CurrentActivatedAmount = 0;
	}

	public override bool CheckObjectiveComplete(int partySize, bool isEndOfRound = false)
	{
		int num = 0;
		foreach (CInteractableSpawner item in ScenarioManager.CurrentScenarioState.Spawners.OfType<CInteractableSpawner>())
		{
			if (item.IsActive)
			{
				num++;
			}
		}
		CurrentActivatedAmount = num;
		int index = Math.Max(0, partySize - 1);
		IsComplete = num >= NumberToActivate[index] && CheckOtherObjectiveStatesRequirement();
		return IsComplete;
	}

	public override float GetObjectiveProgress(int partySize, out int total, out int current)
	{
		int index = Math.Max(0, partySize - 1);
		total = NumberToActivate[index];
		current = Math.Min(CurrentActivatedAmount, total);
		return Math.Min(1f, (float)CurrentActivatedAmount / (float)NumberToActivate[index]);
	}

	public override int GetObjectiveCompletionValue(int partySize)
	{
		int index = Math.Max(0, partySize - 1);
		return NumberToActivate[index];
	}
}
