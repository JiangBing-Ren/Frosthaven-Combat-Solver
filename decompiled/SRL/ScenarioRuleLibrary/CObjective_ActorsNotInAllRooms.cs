using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjective_ActorsNotInAllRooms : CObjective
{
	private bool m_LastCheckedCompletionValue;

	public List<string> RoomMapGUIDs { get; private set; }

	public override bool RemovesFromUIOnComplete => false;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("RoomMapGUIDs", RoomMapGUIDs);
	}

	public CObjective_ActorsNotInAllRooms(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				if (item.Name == "RoomMapGUIDs")
				{
					RoomMapGUIDs = (List<string>)info.GetValue("RoomMapGUIDs", typeof(List<string>));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CObjective_ActorsNotInAllRooms entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CObjective_ActorsNotInAllRooms(EObjectiveResult result, CObjectiveFilter objectiveFilter, List<string> roomMapGUIDs, bool hideCounter, bool activeFromStart = true, int? activateOnRound = null, string customLoc = null, string customTileHoverLoc = null, string eventIdentifier = null, bool isHidden = false, bool isOptional = false, bool winDespiteExhaustion = false, bool enoughToWinAlone = false, Dictionary<string, bool> requiredObjectiveStates = null)
		: base(EObjectiveType.ActorsNotInAllRooms, result, objectiveFilter, hideCounter, activeFromStart, activateOnRound, customLoc, customTileHoverLoc, eventIdentifier, isHidden, isOptional, winDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates)
	{
		RoomMapGUIDs = roomMapGUIDs;
	}

	public override bool CheckObjectiveComplete(int partySize, bool isEndOfRound = false)
	{
		List<CMap> list = new List<CMap>();
		foreach (ActorState actorState in ScenarioManager.CurrentScenarioState.ActorStates)
		{
			if (!ObjectiveFilter.IsValidTarget(actorState))
			{
				continue;
			}
			CActor cActor = ScenarioManager.Scenario.FindActor(actorState.ActorGuid);
			if (cActor != null)
			{
				CTile cTile = ScenarioManager.Tiles[cActor.ArrayIndex.X, cActor.ArrayIndex.Y];
				if (cTile.m_HexMap != null && RoomMapGUIDs.Contains(cTile.m_HexMap.MapGuid) && !list.Contains(cTile.m_HexMap))
				{
					list.Add(cTile.m_HexMap);
				}
			}
		}
		m_LastCheckedCompletionValue = RoomMapGUIDs.Count != list.Count && CheckOtherObjectiveStatesRequirement();
		IsComplete = m_LastCheckedCompletionValue;
		return IsComplete;
	}

	public override float GetObjectiveProgress(int partySize, out int total, out int current)
	{
		total = 1;
		current = (m_LastCheckedCompletionValue ? 1 : 0);
		return m_LastCheckedCompletionValue ? 1 : 0;
	}
}
