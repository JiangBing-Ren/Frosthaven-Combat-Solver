using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjective_DestroyXObjects : CObjective
{
	public List<int> DestroyAmount { get; private set; }

	public int CurrentObjectsDestroyed { get; private set; }

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
						return "GUI_OBJECTIVE_DESTROY_X_OBJECTS_WIN_FILTERED";
					}
					if (Result == EObjectiveResult.Lose)
					{
						return "GUI_OBJECTIVE_DESTROY_X_OBJECTS_LOSE_FILTERED";
					}
				}
				else
				{
					if (Result == EObjectiveResult.Win)
					{
						return "GUI_OBJECTIVE_DESTROY_X_OBJECTS_WIN";
					}
					if (Result == EObjectiveResult.Lose)
					{
						return "GUI_OBJECTIVE_DESTROY_X_OBJECTS_LOSE";
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
		info.AddValue("DestroyAmount", DestroyAmount);
		info.AddValue("CurrentObjectsDestroyed", CurrentObjectsDestroyed);
	}

	public CObjective_DestroyXObjects(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "DestroyAmount"))
				{
					if (name == "CurrentObjectsDestroyed")
					{
						CurrentObjectsDestroyed = info.GetInt32("CurrentObjectsDestroyed");
					}
				}
				else
				{
					DestroyAmount = (List<int>)info.GetValue("DestroyAmount", typeof(List<int>));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CObjective_DestroyXObjects entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CObjective_DestroyXObjects(EObjectiveResult result, CObjectiveFilter objectiveFilter, List<int> destroyAmount, bool hideCounter, bool activeFromStart = true, int? activateOnRound = null, string customLoc = null, string customTileHoverLoc = null, string eventIdentifier = null, bool isHidden = false, bool isOptional = false, bool winDespiteExhaustion = false, bool enoughToWinAlone = false, Dictionary<string, bool> requiredObjectiveStates = null)
		: base(EObjectiveType.DestroyXObjects, result, objectiveFilter, hideCounter, activeFromStart, activateOnRound, customLoc, customTileHoverLoc, eventIdentifier, isHidden, isOptional, winDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates)
	{
		DestroyAmount = destroyAmount;
	}

	public override bool CheckObjectiveComplete(int partySize, bool isEndOfRound = false)
	{
		int num = 0;
		foreach (ObjectState @object in ScenarioManager.CurrentScenarioState.Objects)
		{
			if (!@object.IsHiddenForCurrentPartySize && @object.Object != null && @object.Object.IsDeadForObjectives && ObjectiveFilter.IsValidTarget(@object))
			{
				num++;
			}
		}
		int index = Math.Max(0, partySize - 1);
		CurrentObjectsDestroyed = num;
		IsComplete = num >= DestroyAmount[index] && CheckOtherObjectiveStatesRequirement();
		return IsComplete;
	}

	public override float GetObjectiveProgress(int partySize, out int total, out int current)
	{
		int index = Math.Max(0, partySize - 1);
		total = DestroyAmount[index];
		current = Math.Min(CurrentObjectsDestroyed, DestroyAmount[index]);
		return Math.Min(1f, (float)CurrentObjectsDestroyed / (float)DestroyAmount[index]);
	}

	public override int GetObjectiveCompletionValue(int partySize)
	{
		int index = Math.Max(0, partySize - 1);
		return DestroyAmount[index];
	}
}
