using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjective_ActivateXProps : CObjective
{
	public List<int> ActivateAmount { get; private set; }

	public int ActivatedProps { get; private set; }

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
						return "GUI_OBJECTIVE_ACTIVATE_X_PROPS_WIN_FILTERED";
					}
					if (Result == EObjectiveResult.Lose)
					{
						return "GUI_OBJECTIVE_ACTIVATE_X_PROPS_LOSE_FILTERED";
					}
				}
				else
				{
					if (Result == EObjectiveResult.Win)
					{
						return "GUI_OBJECTIVE_ACTIVATE_X_PROPS_WIN";
					}
					if (Result == EObjectiveResult.Lose)
					{
						return "GUI_OBJECTIVE_ACTIVATE_X_PROPS_LOSE";
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
		info.AddValue("ActivateAmount", ActivateAmount);
	}

	public CObjective_ActivateXProps(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "ActivateAmount"))
				{
					if (name == "ActivatedProps")
					{
						ActivatedProps = info.GetInt32("ActivatedProps");
					}
				}
				else
				{
					ActivateAmount = (List<int>)info.GetValue("ActivateAmount", typeof(List<int>));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CObjective_ActivateXProps entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CObjective_ActivateXProps(EObjectiveResult result, CObjectiveFilter objectiveFilter, List<int> activateAmount, bool hideCounter, bool activeFromStart = true, int? activateOnRound = null, string customLoc = null, string customTileHoverLoc = null, string eventIdentifier = null, bool isHidden = false, bool isOptional = false, bool winDespiteExhaustion = false, bool enoughToWinAlone = false, Dictionary<string, bool> requiredObjectiveStates = null)
		: base(EObjectiveType.ActivateXProps, result, objectiveFilter, hideCounter, activeFromStart, activateOnRound, customLoc, customTileHoverLoc, eventIdentifier, isHidden, isOptional, winDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates)
	{
		ActivateAmount = activateAmount;
	}

	public override bool CheckObjectiveComplete(int partySize, bool isEndOfRound = false)
	{
		int num = 0;
		foreach (CObjectProp activatedProp in ScenarioManager.CurrentScenarioState.ActivatedProps)
		{
			if (ObjectiveFilter.IsValidProp(activatedProp))
			{
				num++;
			}
		}
		ActivatedProps = num;
		int index = Math.Max(0, partySize - 1);
		IsComplete = num >= ActivateAmount[index] && CheckOtherObjectiveStatesRequirement();
		return IsComplete;
	}

	public override float GetObjectiveProgress(int partySize, out int total, out int current)
	{
		int index = Math.Max(0, partySize - 1);
		total = ActivateAmount[index];
		current = Math.Min(ActivatedProps, ActivateAmount[index]);
		return Math.Min(1f, (float)ActivatedProps / (float)ActivateAmount[index]);
	}

	public override int GetObjectiveCompletionValue(int partySize)
	{
		int index = Math.Max(0, partySize - 1);
		return ActivateAmount[index];
	}
}
