using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjective_BringItemsToArea : CObjective
{
	public List<int> TargetNumberOfItems { get; private set; }

	public List<TileIndex> AreaPositions { get; private set; }

	public int CurrentItemsAmount { get; private set; }

	public override bool RemovesFromUIOnComplete => false;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("TargetNumberOfItems", TargetNumberOfItems);
		info.AddValue("AreaPositions", AreaPositions);
		info.AddValue("CurrentItemsAmount", CurrentItemsAmount);
	}

	public CObjective_BringItemsToArea(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "TargetNumberOfItems":
					TargetNumberOfItems = (List<int>)info.GetValue("TargetNumberOfItems", typeof(List<int>));
					break;
				case "AreaPositions":
					AreaPositions = (List<TileIndex>)info.GetValue("AreaPositions", typeof(List<TileIndex>));
					break;
				case "CurrentItemsAmount":
					CurrentItemsAmount = info.GetInt32("CurrentItemsAmount");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CObjective_ActorsEscaped entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CObjective_BringItemsToArea(EObjectiveResult result, CObjectiveFilter objectiveFilter, List<int> targetNumberOfEscapees, List<TileIndex> areaPositions, bool hideCounter, bool activeFromStart = true, int? activateOnRound = null, string customLoc = null, string customTileHoverLoc = null, string eventIdentifier = null, bool isHidden = false, bool isOptional = false, bool winDespiteExhaustion = false, bool enoughToWinAlone = false, Dictionary<string, bool> requiredObjectiveStates = null)
		: base(EObjectiveType.BringItemsToArea, result, objectiveFilter, hideCounter, activeFromStart, activateOnRound, customLoc, customTileHoverLoc, eventIdentifier, isHidden, isOptional, winDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates)
	{
		TargetNumberOfItems = targetNumberOfEscapees;
		AreaPositions = areaPositions;
		CurrentItemsAmount = 0;
	}

	public override bool CheckObjectiveComplete(int partySize, bool isEndOfRound = false)
	{
		int num = 0;
		foreach (CObjectProp destroyedProp in ScenarioManager.CurrentScenarioState.DestroyedProps)
		{
			if (destroyedProp.PropType == EPropType.QuestItem)
			{
				num++;
			}
		}
		CurrentItemsAmount = num;
		int index = Math.Max(0, partySize - 1);
		IsComplete = num >= TargetNumberOfItems[index] && CheckOtherObjectiveStatesRequirement();
		return IsComplete;
	}

	public override float GetObjectiveProgress(int partySize, out int total, out int current)
	{
		int index = Math.Max(0, partySize - 1);
		total = TargetNumberOfItems[index];
		current = Math.Min(CurrentItemsAmount, TargetNumberOfItems[index]);
		return Math.Min(1f, (float)CurrentItemsAmount / (float)TargetNumberOfItems[index]);
	}
}
