using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjective_AnyActorReachPosition : CObjective
{
	public List<int> ReachPositionAmounts { get; private set; }

	public List<TileIndex> ActorTargetPositions { get; private set; }

	public int CurrentReachedPosition { get; private set; }

	public override bool RemovesFromUIOnComplete => false;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ReachPositionAmounts", ReachPositionAmounts);
		info.AddValue("ActorTargetPositions", ActorTargetPositions);
		info.AddValue("CurrentReachedPosition", CurrentReachedPosition);
	}

	public CObjective_AnyActorReachPosition(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "ReachPositionAmounts":
					ReachPositionAmounts = (List<int>)info.GetValue("ReachPositionAmounts", typeof(List<int>));
					break;
				case "ActorTargetPositions":
					ActorTargetPositions = (List<TileIndex>)info.GetValue("ActorTargetPositions", typeof(List<TileIndex>));
					break;
				case "CurrentReachedPosition":
					CurrentReachedPosition = info.GetInt32("CurrentReachedPosition");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CObjective_ActorReachPosition entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CObjective_AnyActorReachPosition(EObjectiveResult result, CObjectiveFilter objectiveFilter, List<int> reachPositionAmounts, List<TileIndex> actorTargetPositions, bool hideCounter, bool activeFromStart = true, int? activateOnRound = null, string customLoc = null, string customTileHoverLoc = null, string eventIdentifier = null, bool isHidden = false, bool isOptional = false, bool winDespiteExhaustion = false, bool enoughToWinAlone = false, Dictionary<string, bool> requiredObjectiveStates = null)
		: base(EObjectiveType.AnyActorReachPosition, result, objectiveFilter, hideCounter, activeFromStart, activateOnRound, customLoc, customTileHoverLoc, eventIdentifier, isHidden, isOptional, winDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates)
	{
		ReachPositionAmounts = reachPositionAmounts;
		ActorTargetPositions = actorTargetPositions;
		CurrentReachedPosition = 0;
	}

	public override bool CheckObjectiveComplete(int partySize, bool isEndOfRound = false)
	{
		int num = 0;
		foreach (ActorState actorState in ScenarioManager.CurrentScenarioState.ActorStates)
		{
			CActor scenarioActor = ScenarioManager.Scenario.FindActor(actorState.ActorGuid);
			if (scenarioActor != null && ObjectiveFilter.IsValidTarget(actorState))
			{
				if (ActorTargetPositions.Any((TileIndex t) => t.X == scenarioActor.ArrayIndex.X && t.Y == scenarioActor.ArrayIndex.Y))
				{
					num++;
				}
				else if (scenarioActor.IsDeadForObjectives)
				{
					UnableToComplete = true;
				}
			}
		}
		CurrentReachedPosition = num;
		int index = Math.Max(0, partySize - 1);
		IsComplete = num >= ReachPositionAmounts[index] && CheckOtherObjectiveStatesRequirement();
		return IsComplete;
	}

	public override float GetObjectiveProgress(int partySize, out int total, out int current)
	{
		int index = Math.Max(0, partySize - 1);
		total = ReachPositionAmounts[index];
		current = Math.Min(CurrentReachedPosition, ReachPositionAmounts[index]);
		return Math.Min(1f, (float)CurrentReachedPosition / (float)ReachPositionAmounts[index]);
	}
}
