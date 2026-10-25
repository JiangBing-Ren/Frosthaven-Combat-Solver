using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjective_ActorsEscaped : CObjective
{
	public List<int> TargetNumberOfEscapees { get; private set; }

	public List<TileIndex> EscapePositions { get; private set; }

	public int CurrentEscapedAmount { get; private set; }

	public override bool RemovesFromUIOnComplete => false;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("TargetNumberOfEscapees", TargetNumberOfEscapees);
		info.AddValue("EscapePositions", EscapePositions);
		info.AddValue("CurrentEscapedAmount", CurrentEscapedAmount);
	}

	public CObjective_ActorsEscaped(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "TargetNumberOfEscapees":
					TargetNumberOfEscapees = (List<int>)info.GetValue("TargetNumberOfEscapees", typeof(List<int>));
					break;
				case "EscapePositions":
					EscapePositions = (List<TileIndex>)info.GetValue("EscapePositions", typeof(List<TileIndex>));
					break;
				case "CurrentEscapedAmount":
					CurrentEscapedAmount = info.GetInt32("CurrentEscapedAmount");
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

	public CObjective_ActorsEscaped(EObjectiveResult result, CObjectiveFilter objectiveFilter, List<int> targetNumberOfEscapees, List<TileIndex> actorEscapePositions, bool hideCounter, bool activeFromStart = true, int? activateOnRound = null, string customLoc = null, string customTileHoverLoc = null, string eventIdentifier = null, bool isHidden = false, bool isOptional = false, bool winDespiteExhaustion = false, bool enoughToWinAlone = false, Dictionary<string, bool> requiredObjectiveStates = null)
		: base(EObjectiveType.ActorsEscaped, result, objectiveFilter, hideCounter, activeFromStart, activateOnRound, customLoc, customTileHoverLoc, eventIdentifier, isHidden, isOptional, winDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates)
	{
		TargetNumberOfEscapees = targetNumberOfEscapees;
		EscapePositions = actorEscapePositions;
		CurrentEscapedAmount = 0;
	}

	public override bool CheckObjectiveComplete(int partySize, bool isEndOfRound = false)
	{
		int num = 0;
		foreach (ActorState actorState in ScenarioManager.CurrentScenarioState.ActorStates)
		{
			if (actorState is HeroSummonState && !ObjectiveFilter.FilterActorType.HasFlag(CAbilityFilter.EFilterActorType.Summon))
			{
				continue;
			}
			CActor scenarioActor = ScenarioManager.Scenario.FindActor(actorState.ActorGuid);
			if (scenarioActor != null)
			{
				if (ObjectiveFilter.IsValidTarget(actorState))
				{
					if (scenarioActor.CauseOfDeath == CActor.ECauseOfDeath.ActorRemovedFromMap && EscapePositions.Any((TileIndex position) => position.X == scenarioActor.ArrayIndex.X && position.Y == scenarioActor.ArrayIndex.Y))
					{
						num++;
					}
					else if (scenarioActor.IsDeadForObjectives && scenarioActor is CPlayerActor)
					{
						UnableToComplete = true;
					}
				}
			}
			else if (actorState.CauseOfDeath == CActor.ECauseOfDeath.ActorRemovedFromMap && EscapePositions.Any((TileIndex position) => position.X == actorState.Location.X && position.Y == actorState.Location.Y))
			{
				num++;
			}
		}
		CurrentEscapedAmount = num;
		int index = Math.Max(0, partySize - 1);
		IsComplete = num >= TargetNumberOfEscapees[index] && CheckOtherObjectiveStatesRequirement();
		return IsComplete;
	}

	public override float GetObjectiveProgress(int partySize, out int total, out int current)
	{
		int index = Math.Max(0, partySize - 1);
		total = TargetNumberOfEscapees[index];
		current = Math.Min(CurrentEscapedAmount, TargetNumberOfEscapees[index]);
		return Math.Min(1f, (float)CurrentEscapedAmount / (float)TargetNumberOfEscapees[index]);
	}
}
