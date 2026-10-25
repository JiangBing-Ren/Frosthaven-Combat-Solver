using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using AStar;

namespace ScenarioRuleLibrary;

[Serializable]
public class CScenarioModifierPhaseInAndTeleport : CScenarioModifier
{
	private int NextTeleportTileIndex;

	public List<TileIndex> TeleportTileIndexes { get; set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("TeleportTileIndexes", TeleportTileIndexes);
		info.AddValue("NextTeleportTileIndex", NextTeleportTileIndex);
	}

	public CScenarioModifierPhaseInAndTeleport(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "TeleportTileIndexes"))
				{
					if (name == "NextTeleportTileIndex")
					{
						NextTeleportTileIndex = info.GetInt32("NextTeleportTileIndex");
					}
				}
				else
				{
					TeleportTileIndexes = (List<TileIndex>)info.GetValue("TeleportTileIndexes", typeof(List<TileIndex>));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CScenarioModifierPhaseInAndTeleport entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CScenarioModifierPhaseInAndTeleport(string name, int id, int activationRound, EScenarioModifierTriggerPhase triggerPhase, bool applyToEachActorOnce, CObjectiveFilter scenarioModFilter, bool isPositive, List<TileIndex> teleportTiles, EScenarioModifierActivationType scenarioModifierActivationType = EScenarioModifierActivationType.None, string scenarioModifierActivationID = null, string customLocKey = null, string customTriggerLocKey = null, string eventId = null, List<CAbility.EAbilityType> afterAbilityTypes = null, List<CAbility.EAttackType> afterAttackTypes = null, bool isHidden = false, bool isDeactivated = false, bool applyOnceTotal = false, bool cancelAllActiveBonusesOnDeactivation = false, EScenarioModifierRoomOpenBehaviour roomOpenBehaviour = EScenarioModifierRoomOpenBehaviour.None, List<string> roomMapGuids = null)
		: base(name, id, activationRound, applyToEachActorOnce, EScenarioModifierType.PhaseInAndTeleport, triggerPhase, scenarioModifierActivationType, scenarioModifierActivationID, scenarioModFilter, isPositive, null, customLocKey, customTriggerLocKey, eventId, afterAbilityTypes, afterAttackTypes, isHidden, isDeactivated, applyOnceTotal, cancelAllActiveBonusesOnDeactivation, roomOpenBehaviour, roomMapGuids)
	{
		TeleportTileIndexes = teleportTiles;
		NextTeleportTileIndex = 0;
	}

	public override void PerformScenarioModifierInRound(int roundCount, bool forceActivate = false)
	{
		if (!IsActiveInRound(roundCount, forceActivate))
		{
			return;
		}
		foreach (CActor allActor in ScenarioManager.Scenario.AllActors)
		{
			PerformScenarioModifier(roundCount, allActor, ScenarioManager.CurrentScenarioState.Players.Count, forceActivate);
		}
	}

	public override void PerformScenarioModifier(int roundCount, CActor currentActor = null, int partySize = 2, bool forceActivate = false)
	{
		if (!IsActiveInRound(roundCount, forceActivate) || (!IsPositive && IgnoreNegativeScenarioEffects(currentActor)) || (ApplyOnceTotal && HasBeenAppliedOnce) || AppliedToActorGUIDs.Contains(currentActor.ActorGuid))
		{
			return;
		}
		bool flag = false;
		if (ScenarioModifierFilter != null && currentActor != null)
		{
			ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.SingleOrDefault((ActorState x) => x.ActorGuid == currentActor.ActorGuid);
			if (actorState != null && currentActor.PhasedOut && ScenarioModifierFilter.IsValidTarget(actorState))
			{
				flag = true;
			}
		}
		if (!flag)
		{
			return;
		}
		if (ApplyToEachActorOnce)
		{
			AppliedToActorGUIDs.Add(currentActor.ActorGuid);
		}
		HasBeenAppliedOnce = true;
		Point arrayIndex = currentActor.ArrayIndex;
		TileIndex tileIndex = TeleportTileIndexes[NextTeleportTileIndex];
		NextTeleportTileIndex++;
		if (NextTeleportTileIndex > TeleportTileIndexes.Count - 1)
		{
			NextTeleportTileIndex = 0;
		}
		CTile cTile = ScenarioManager.Tiles[tileIndex.X, tileIndex.Y];
		CTile propTile = null;
		if (!ScenarioManager.PathFinder.Nodes[cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y].Walkable || ScenarioManager.Scenario.FindActorAt(cTile.m_ArrayIndex) != null || CObjectProp.FindPropWithPathingBlocker(cTile.m_ArrayIndex, ref propTile) != null)
		{
			for (int num = 0; num < 5; num++)
			{
				List<CTile> allUnblockedTilesFromOrigin = ScenarioManager.GetAllUnblockedTilesFromOrigin(cTile, num + 1);
				allUnblockedTilesFromOrigin.Remove(cTile);
				List<CTile> list = new List<CTile>();
				foreach (CTile item in allUnblockedTilesFromOrigin)
				{
					if (ScenarioManager.PathFinder.Nodes[item.m_ArrayIndex.X, item.m_ArrayIndex.Y].Walkable && ScenarioManager.Scenario.FindActorAt(item.m_ArrayIndex) == null && CObjectProp.FindPropWithPathingBlocker(item.m_ArrayIndex, ref propTile) == null)
					{
						list.Add(item);
					}
				}
				if (list.Count > 0)
				{
					cTile = list[ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(list.Count)];
					break;
				}
			}
		}
		currentActor.ArrayIndex = cTile.m_ArrayIndex;
		currentActor.PhasedOut = false;
		CActorHasTeleported_MessageData message = new CActorHasTeleported_MessageData(currentActor)
		{
			m_EndLocation = cTile.m_ArrayIndex,
			m_StartLocation = arrayIndex,
			m_ActorTeleported = currentActor,
			m_TeleportAbility = null
		};
		ScenarioRuleClient.MessageHandler(message);
		foreach (CObjectProp prop in cTile.m_Props)
		{
			prop.Activate(currentActor);
		}
	}
}
