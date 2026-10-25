using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CScenarioModifierMovePropsInSequence : CScenarioModifier
{
	public List<TileIndex> SequenceTiles { get; set; }

	public int NextTileIndex { get; set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("SequenceTiles", SequenceTiles);
		info.AddValue("NextTileIndex", NextTileIndex);
	}

	public CScenarioModifierMovePropsInSequence(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "SequenceTiles"))
				{
					if (name == "NextTileIndex")
					{
						NextTileIndex = info.GetInt32("NextTileIndex");
					}
				}
				else
				{
					SequenceTiles = (List<TileIndex>)info.GetValue("SequenceTiles", typeof(List<TileIndex>));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CScenarioModifierMoveTrapsInSequence entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CScenarioModifierMovePropsInSequence(string name, int id, int activationRound, EScenarioModifierTriggerPhase triggerPhase, bool applyToEachActorOnce, CObjectiveFilter scenarioModFilter, bool isPositive, List<TileIndex> sequenceTiles, EScenarioModifierActivationType scenarioModifierActivationType = EScenarioModifierActivationType.None, string scenarioModifierActivationID = null, string customLocKey = null, string customTriggerLocKey = null, string eventId = null, List<CAbility.EAbilityType> afterAbilityTypes = null, List<CAbility.EAttackType> afterAttackTypes = null, bool isHidden = false, bool isDeactivated = false, bool applyOnceTotal = false, bool cancelAllActiveBonusesOnDeactivation = false, EScenarioModifierRoomOpenBehaviour roomOpenBehaviour = EScenarioModifierRoomOpenBehaviour.None, List<string> roomMapGuids = null)
		: base(name, id, activationRound, applyToEachActorOnce, EScenarioModifierType.MovePropsInSequence, triggerPhase, scenarioModifierActivationType, scenarioModifierActivationID, scenarioModFilter, isPositive, null, customLocKey, customTriggerLocKey, eventId, afterAbilityTypes, afterAttackTypes, isHidden, isDeactivated, applyOnceTotal, cancelAllActiveBonusesOnDeactivation, roomOpenBehaviour, roomMapGuids)
	{
		SequenceTiles = sequenceTiles;
	}

	public override void PerformScenarioModifierInRound(int roundCount, bool forceActivate = false)
	{
		if (!IsActiveInRound(roundCount, forceActivate))
		{
			return;
		}
		foreach (CObjectProp item in ScenarioManager.CurrentScenarioState.Props.ToList())
		{
			PerformScenarioModifier(roundCount, item, ScenarioManager.CurrentScenarioState.Players.Count, forceActivate);
		}
		if (ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.StartRound || ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.EndRound)
		{
			NextTileIndex++;
			if (NextTileIndex > SequenceTiles.Count - 1)
			{
				NextTileIndex = 0;
			}
		}
	}

	public override void PerformScenarioModifier(int roundCount, CObjectProp currentProp = null, int partySize = 2, bool forceActivate = false)
	{
		if (!currentProp.StartingMap.Revealed || !IsActiveInRound(roundCount, forceActivate) || (ApplyOnceTotal && HasBeenAppliedOnce) || AppliedToPropGUIDs.Contains(currentProp.PropGuid))
		{
			return;
		}
		bool flag = true;
		if (ScenarioModifierFilter != null)
		{
			flag = ScenarioModifierFilter.IsValidProp(currentProp);
		}
		if (!flag)
		{
			return;
		}
		if (ApplyToEachActorOnce)
		{
			AppliedToPropGUIDs.Add(currentProp.PropGuid);
		}
		HasBeenAppliedOnce = true;
		TileIndex tileIndex = SequenceTiles[NextTileIndex];
		if (ScenarioModifierTriggerPhase != EScenarioModifierTriggerPhase.StartRound && ScenarioModifierTriggerPhase != EScenarioModifierTriggerPhase.EndRound)
		{
			NextTileIndex++;
			if (NextTileIndex > SequenceTiles.Count - 1)
			{
				NextTileIndex = 0;
			}
		}
		CTile adjacentTile = ScenarioManager.GetAdjacentTile(tileIndex.X, tileIndex.Y, ScenarioManager.EAdjacentPosition.ECenter);
		CTile propTile = null;
		if (adjacentTile == null || !ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y].Walkable || CObjectProp.FindPropWithPathingBlocker(adjacentTile.m_ArrayIndex, ref propTile) != null)
		{
			return;
		}
		ScenarioManager.Tiles[currentProp.ArrayIndex.X, currentProp.ArrayIndex.Y].DelProp(currentProp);
		currentProp.SetLocation(tileIndex, null, currentProp.Rotation);
		currentProp.StartingMapGuid = adjacentTile.m_HexMap.MapGuid;
		adjacentTile.AddProp(currentProp);
		CMoveProp_MessageData message = new CMoveProp_MessageData
		{
			m_MoveProp = currentProp,
			m_MoveToTile = adjacentTile,
			m_MoveSpeed = 2f
		};
		ScenarioRuleClient.MessageHandler(message);
		foreach (CActor item in ScenarioManager.Scenario.FindActorsAt(adjacentTile.m_ArrayIndex))
		{
			currentProp.Activate(item);
			currentProp.DestroyProp();
		}
	}
}
