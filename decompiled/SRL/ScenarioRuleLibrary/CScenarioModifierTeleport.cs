using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CScenarioModifierTeleport : CScenarioModifier
{
	public List<TileIndex> TeleportTileIndexes { get; set; }

	public bool MovesOtherThingsOffTile { get; set; }

	public bool OpenDoorsToTeleportedLocation { get; set; }

	public int NextTeleportTileIndex { get; set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("TeleportTileIndexes", TeleportTileIndexes);
		info.AddValue("MovesOtherThingsOffTile", MovesOtherThingsOffTile);
		info.AddValue("OpenDoorsToTeleportedLocation", OpenDoorsToTeleportedLocation);
		info.AddValue("NextTeleportTileIndex", NextTeleportTileIndex);
	}

	public CScenarioModifierTeleport(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "TeleportTileIndexes":
					TeleportTileIndexes = (List<TileIndex>)info.GetValue("TeleportTileIndexes", typeof(List<TileIndex>));
					break;
				case "MovesOtherThingsOffTile":
					MovesOtherThingsOffTile = info.GetBoolean("MovesOtherThingsOffTile");
					break;
				case "OpenDoorsToTeleportedLocation":
					OpenDoorsToTeleportedLocation = info.GetBoolean("OpenDoorsToTeleportedLocation");
					break;
				case "NextTeleportTileIndex":
					NextTeleportTileIndex = info.GetInt32("NextTeleportTileIndex");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CScenarioModifierTeleport entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CScenarioModifierTeleport(string name, int id, int activationRound, EScenarioModifierTriggerPhase triggerPhase, bool applyToEachActorOnce, CObjectiveFilter scenarioModFilter, bool isPositive, List<TileIndex> teleportTiles, bool movesOtherThingsOffTile, bool openDoorsToTeleportedLocation, EScenarioModifierActivationType scenarioModifierActivationType = EScenarioModifierActivationType.None, string scenarioModifierActivationID = null, string customLocKey = null, string customTriggerLocKey = null, string eventId = null, List<CAbility.EAbilityType> afterAbilityTypes = null, List<CAbility.EAttackType> afterAttackTypes = null, bool isHidden = false, bool isDeactivated = false, bool applyOnceTotal = false, bool cancelAllActiveBonusesOnDeactivation = false, EScenarioModifierRoomOpenBehaviour roomOpenBehaviour = EScenarioModifierRoomOpenBehaviour.None, List<string> roomMapGuids = null)
		: base(name, id, activationRound, applyToEachActorOnce, EScenarioModifierType.Teleport, triggerPhase, scenarioModifierActivationType, scenarioModifierActivationID, scenarioModFilter, isPositive, null, customLocKey, customTriggerLocKey, eventId, afterAbilityTypes, afterAttackTypes, isHidden, isDeactivated, applyOnceTotal, cancelAllActiveBonusesOnDeactivation, roomOpenBehaviour, roomMapGuids)
	{
		TeleportTileIndexes = teleportTiles;
		MovesOtherThingsOffTile = movesOtherThingsOffTile;
		OpenDoorsToTeleportedLocation = openDoorsToTeleportedLocation;
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
			if (actorState != null && ScenarioModifierFilter.IsValidTarget(actorState))
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
		_ = currentActor.ArrayIndex;
		TileIndex tileIndex = TeleportTileIndexes[NextTeleportTileIndex];
		NextTeleportTileIndex++;
		if (NextTeleportTileIndex > TeleportTileIndexes.Count - 1)
		{
			NextTeleportTileIndex = 0;
		}
		CTile item = ScenarioManager.Tiles[tileIndex.X, tileIndex.Y];
		CAbilityTeleport cAbilityTeleport = (CAbilityTeleport)CAbilityTeleport.CreateDefaultTeleport(new CAbilityTeleport.TeleportData
		{
			ShouldOpenDoorsToTeleportedLocation = OpenDoorsToTeleportedLocation,
			MoveOtherThingsOffTiles = MovesOtherThingsOffTile
		});
		cAbilityTeleport.InlineSubAbilityTiles.Add(item);
		cAbilityTeleport.IsInlineSubAbility = true;
		cAbilityTeleport.SetCanSkip(canSkip: false);
		List<CAbility> list = new List<CAbility> { cAbilityTeleport };
		if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction)
		{
			cPhaseAction.StackNextAbilities(list, currentActor, killAfter: false, stackToNextCurrent: true);
			return;
		}
		for (int num = 0; num < list.Count; num++)
		{
			GameState.PendingScenarioModifierAbilities.Add(new Tuple<CAbility, CActor>(list[num], currentActor));
		}
	}
}
