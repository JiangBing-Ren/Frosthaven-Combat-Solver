using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjective_LootX : CObjective
{
	public List<int> LootAmount { get; private set; }

	public int CurrentLootedAmount { get; private set; }

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
						return "GUI_OBJECTIVE_LOOT_X_WIN_FILTERED";
					}
					if (Result == EObjectiveResult.Lose)
					{
						return "GUI_OBJECTIVE_LOOT_X_LOSE_FILTERED";
					}
				}
				else
				{
					if (Result == EObjectiveResult.Win)
					{
						return "GUI_OBJECTIVE_LOOT_X_WIN";
					}
					if (Result == EObjectiveResult.Lose)
					{
						return "GUI_OBJECTIVE_LOOT_X_LOSE";
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
		info.AddValue("LootAmount", LootAmount);
		info.AddValue("CurrentLootedAmount", CurrentLootedAmount);
	}

	public CObjective_LootX(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item2 in info)
		{
			try
			{
				string name = item2.Name;
				if (!(name == "LootAmount"))
				{
					if (name == "CurrentLootedAmount")
					{
						CurrentLootedAmount = info.GetInt32("CurrentLootedAmount");
					}
					continue;
				}
				try
				{
					LootAmount = (List<int>)info.GetValue("LootAmount", typeof(List<int>));
				}
				catch
				{
					int item = (int)info.GetValue("LootAmount", typeof(int));
					LootAmount = new List<int> { item, item, item, item };
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CObjective_LootX entry " + item2.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CObjective_LootX(EObjectiveResult result, CObjectiveFilter objectiveFilter, List<int> lootAmount, bool hideCounter, bool activeFromStart = true, int? activateOnRound = null, string customLoc = null, string customTileHoverLoc = null, string eventIdentifier = null, bool isHidden = false, bool isOptional = false, bool winDespiteExhaustion = false, bool enoughToWinAlone = false, Dictionary<string, bool> requiredObjectiveStates = null)
		: base(EObjectiveType.LootX, result, objectiveFilter, hideCounter, activeFromStart, activateOnRound, customLoc, customTileHoverLoc, eventIdentifier, isHidden, isOptional, winDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates)
	{
		LootAmount = lootAmount;
		CurrentLootedAmount = 0;
	}

	public override bool CheckObjectiveComplete(int partySize, bool isEndOfRound = false)
	{
		int num = 0;
		foreach (CObjectProp activatedProp in ScenarioManager.CurrentScenarioState.ActivatedProps)
		{
			if (ObjectiveFilter.IsValidLootTarget(activatedProp.ObjectType) && ObjectiveFilter.IsValidProp(activatedProp))
			{
				num++;
			}
		}
		CurrentLootedAmount = num;
		int index = Math.Max(0, partySize - 1);
		IsComplete = CurrentLootedAmount >= LootAmount[index] && CheckOtherObjectiveStatesRequirement();
		return IsComplete;
	}

	public override float GetObjectiveProgress(int partySize, out int total, out int current)
	{
		int index = Math.Max(0, partySize - 1);
		total = LootAmount[index];
		current = Math.Min(CurrentLootedAmount, total);
		return Math.Min(1f, (float)CurrentLootedAmount / (float)LootAmount[index]);
	}

	public override int GetObjectiveCompletionValue(int partySize)
	{
		int index = Math.Max(0, partySize - 1);
		return LootAmount[index];
	}
}
