using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using AStar;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

[Serializable]
public class CScenarioModifierActivateClosestAI : CScenarioModifier
{
	public int HighestRoundProcessed;

	public int HighestRoundActivated;

	public string CurrentChosenActorGUID;

	public TileIndex CurrentChosenTargetTile;

	public Dictionary<TileIndex, List<string>> TileSpecificAbilityIds { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("TileSpecificAbilityIds", TileSpecificAbilityIds);
		info.AddValue("HighestRoundProcessed", HighestRoundProcessed);
		info.AddValue("CurrentChosenActorGUID", CurrentChosenActorGUID);
		info.AddValue("CurrentChosenTargetTile", CurrentChosenTargetTile);
		base.GetObjectData(info, context);
	}

	public CScenarioModifierActivateClosestAI(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "TileSpecificAbilityIds":
					TileSpecificAbilityIds = (Dictionary<TileIndex, List<string>>)info.GetValue("TileSpecificAbilityIds", typeof(Dictionary<TileIndex, List<string>>));
					break;
				case "HighestRoundProcessed":
					HighestRoundProcessed = info.GetInt32("HighestRoundProcessed");
					break;
				case "CurrentChosenActorGUID":
					CurrentChosenActorGUID = info.GetString("CurrentChosenActorGUID");
					break;
				case "CurrentChosenTargetTile":
					CurrentChosenTargetTile = (TileIndex)info.GetValue("CurrentChosenTargetTile", typeof(TileIndex));
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CScenarioModifierActivateClosestAI entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CScenarioModifierActivateClosestAI(string name, int id, int activationRound, EScenarioModifierTriggerPhase triggerPhase, bool applyToEachActorOnce, CObjectiveFilter scenarioModFilter, bool isPositive, string scenarioAbilityID, Dictionary<TileIndex, List<string>> abilityIdsByLocation, EScenarioModifierActivationType scenarioModifierActivationType = EScenarioModifierActivationType.None, string scenarioModifierActivationID = null, string customLocKey = null, string customTriggerLocKey = null, string eventId = null, List<CAbility.EAbilityType> afterAbilityTypes = null, List<CAbility.EAttackType> afterAttackTypes = null, bool isHidden = false, bool isDeactivated = false, bool applyOnceTotal = false, bool cancelAllActiveBonusesOnDeactivation = false, EScenarioModifierRoomOpenBehaviour roomOpenBehaviour = EScenarioModifierRoomOpenBehaviour.None, List<string> roomMapGuids = null)
		: base(name, id, activationRound, applyToEachActorOnce, EScenarioModifierType.ActivateClosestAI, triggerPhase, scenarioModifierActivationType, scenarioModifierActivationID, scenarioModFilter, isPositive, scenarioAbilityID, customLocKey, customTriggerLocKey, eventId, afterAbilityTypes, afterAttackTypes, isHidden, isDeactivated, applyOnceTotal, cancelAllActiveBonusesOnDeactivation, roomOpenBehaviour, roomMapGuids)
	{
		TileSpecificAbilityIds = abilityIdsByLocation?.ToDictionary((KeyValuePair<TileIndex, List<string>> kv) => kv.Key, (KeyValuePair<TileIndex, List<string>> kv) => kv.Value) ?? null;
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
		if (!IsActiveInRound(roundCount, forceActivate) || currentActor == null || (ApplyOnceTotal && HasBeenAppliedOnce))
		{
			return;
		}
		if (HighestRoundProcessed < roundCount)
		{
			HighestRoundProcessed = roundCount;
			CurrentChosenActorGUID = string.Empty;
			CurrentChosenTargetTile = null;
		}
		if (HighestRoundActivated >= roundCount || (!IsPositive && IgnoreNegativeScenarioEffects(currentActor)) || AppliedToActorGUIDs.Contains(currentActor.ActorGuid))
		{
			return;
		}
		bool flag = false;
		if (ScenarioModifierFilter != null)
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
		bool flag2 = true;
		if (!string.IsNullOrEmpty(CurrentChosenActorGUID))
		{
			flag2 = ScenarioManager.Scenario.AllAliveActors.FirstOrDefault((CActor a) => a.ActorGuid == CurrentChosenActorGUID)?.IsDead ?? true;
		}
		if (flag2)
		{
			List<CActor> list = new List<CActor>();
			List<ActorState> list2 = new List<ActorState>();
			foreach (ActorState actorState2 in ScenarioManager.CurrentScenarioState.ActorStates)
			{
				if (!actorState2.IsDead && actorState2.AIFocusOverride != null && actorState2.AIFocusOverride.OverrideTargetType != CAIFocusOverrideDetails.EOverrideTargetType.None && ScenarioModifierFilter.IsValidTarget(actorState2))
				{
					CActor cActor = ScenarioManager.Scenario.AllActors.FirstOrDefault((CActor a) => a.ActorGuid == actorState2.ActorGuid);
					if (cActor != null && !cActor.IsDead)
					{
						list.Add(cActor);
						list2.Add(actorState2);
					}
				}
			}
			if (list.Count > 0)
			{
				List<Tuple<GameState.CRangeSortedActor, TileIndex>> rangeIndexedActors = new List<Tuple<GameState.CRangeSortedActor, TileIndex>>();
				foreach (CActor actor in list)
				{
					foreach (TileIndex key in TileSpecificAbilityIds.Keys)
					{
						List<Point> list3 = ScenarioManager.PathFinder.FindPath(currentActor, new Point(key), actor.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out var foundPath, ignoreDoorLocks: false);
						if (!foundPath)
						{
							continue;
						}
						GameState.CRangeSortedActor existingActorOfSame = rangeIndexedActors.FirstOrDefault((Tuple<GameState.CRangeSortedActor, TileIndex> r) => r.Item1.m_Actor.ActorGuid == actor.ActorGuid)?.Item1;
						if (existingActorOfSame != null)
						{
							if (existingActorOfSame.m_Range < list3.Count)
							{
								rangeIndexedActors.RemoveAll((Tuple<GameState.CRangeSortedActor, TileIndex> r) => r.Item1 == existingActorOfSame);
								rangeIndexedActors.Add(new Tuple<GameState.CRangeSortedActor, TileIndex>(new GameState.CRangeSortedActor(actor, list3.Count), key));
							}
						}
						else
						{
							rangeIndexedActors.Add(new Tuple<GameState.CRangeSortedActor, TileIndex>(new GameState.CRangeSortedActor(actor, list3.Count), key));
						}
					}
				}
				rangeIndexedActors.Sort((Tuple<GameState.CRangeSortedActor, TileIndex> x, Tuple<GameState.CRangeSortedActor, TileIndex> y) => GameState.s_ActorInitiativeComparer.Compare(x.Item1.m_Actor, y.Item1.m_Actor));
				rangeIndexedActors.Sort((Tuple<GameState.CRangeSortedActor, TileIndex> x, Tuple<GameState.CRangeSortedActor, TileIndex> y) => x.Item1.m_Range.CompareTo(y.Item1.m_Range));
				if (rangeIndexedActors.Count > 0)
				{
					CurrentChosenActorGUID = rangeIndexedActors[0].Item1.m_Actor.ActorGuid;
					CurrentChosenTargetTile = rangeIndexedActors[0].Item2;
				}
				int i;
				for (i = 0; i < rangeIndexedActors.Count; i++)
				{
					ActorState actorState3 = list2.FirstOrDefault((ActorState a) => a.ActorGuid == rangeIndexedActors[i].Item1.m_Actor.ActorGuid);
					if (i == 0)
					{
						actorState3.AIFocusOverride.IsDisabled = false;
						List<CAbility> list4 = TriggerAbilities(currentActor, ScenarioManager.CurrentScenarioState.Players.Count);
						if (list4 != null)
						{
							list[i].MindControlDuration = CAbilityControlActor.EControlDurationType.ControlForOneTurn;
							list[i].MindControlIsFromEnemy = true;
							list[i].MindControlAbility = null;
							GameState.QueueOverrideActorForOneTurn(list[i], list[i].Type, list4.ToList(), this);
						}
					}
					else
					{
						actorState3.AIFocusOverride.IsDisabled = true;
					}
				}
			}
		}
		ActorState actorState4 = ScenarioManager.CurrentScenarioState.ActorStates.FirstOrDefault((ActorState a) => a.ActorGuid == currentActor.ActorGuid);
		if (CurrentChosenActorGUID == currentActor.ActorGuid)
		{
			actorState4.AIFocusOverride.IsDisabled = false;
			actorState4.AIFocusOverride.ForcedUseTileIndex = CurrentChosenTargetTile;
			HighestRoundActivated = roundCount;
			if (!TileSpecificAbilityIds.TryGetValue(CurrentChosenTargetTile, out var abilityIds) || abilityIds.Count < partySize)
			{
				return;
			}
			List<CAbility> list5 = ScenarioRuleClient.SRLYML.ScenarioAbilities.SingleOrDefault((ScenarioAbilitiesYMLData a) => a.ScenarioAbilityID == abilityIds[partySize - 1])?.ScenarioAbilities.ToList();
			if (list5 != null)
			{
				currentActor.MindControlDuration = CAbilityControlActor.EControlDurationType.ControlForOneTurn;
				currentActor.MindControlIsFromEnemy = true;
				currentActor.MindControlAbility = null;
				GameState.QueueOverrideActorForOneTurn(currentActor, currentActor.Type, list5.ToList(), this);
				if (!AppliedToActorGUIDs.Contains(currentActor.ActorGuid))
				{
					AppliedToActorGUIDs.Add(currentActor.ActorGuid);
				}
				HasBeenAppliedOnce = true;
			}
		}
		else
		{
			actorState4.AIFocusOverride.IsDisabled = true;
			actorState4.AIFocusOverride.ForcedUseTileIndex = null;
		}
	}

	public override CAbility TriggerAbility(CActor actorPerformingAbility = null, int partySize = 2, bool copyAbility = true)
	{
		if (actorPerformingAbility != null && TileSpecificAbilityIds != null)
		{
			TileIndex key = new TileIndex(actorPerformingAbility.ArrayIndex);
			if (TileSpecificAbilityIds.TryGetValue(key, out var abilityIds) && abilityIds.Count >= partySize)
			{
				CAbility cAbility = ScenarioRuleClient.SRLYML.ScenarioAbilities.SingleOrDefault((ScenarioAbilitiesYMLData a) => a.ScenarioAbilityID == abilityIds[partySize - 1])?.ScenarioAbilities[0];
				if (copyAbility)
				{
					cAbility = CAbility.CopyAbility(cAbility, generateNewID: false);
				}
				return cAbility;
			}
		}
		return base.TriggerAbility(actorPerformingAbility, partySize, copyAbility);
	}

	public override List<CAbility> TriggerAbilities(CActor actorPerformingAbility = null, int partySize = 2, bool copyAbilities = true)
	{
		if (actorPerformingAbility != null && TileSpecificAbilityIds != null)
		{
			TileIndex key = new TileIndex(actorPerformingAbility.ArrayIndex);
			if (TileSpecificAbilityIds.TryGetValue(key, out var abilityIds) && abilityIds.Count >= partySize)
			{
				List<CAbility> list = ScenarioRuleClient.SRLYML.ScenarioAbilities.SingleOrDefault((ScenarioAbilitiesYMLData a) => a.ScenarioAbilityID == abilityIds[partySize - 1])?.ScenarioAbilities.ToList();
				if (copyAbilities)
				{
					list = list.Select((CAbility x) => CAbility.CopyAbility(x, generateNewID: false)).ToList();
				}
				return list;
			}
		}
		return base.TriggerAbilities(actorPerformingAbility, partySize, copyAbilities);
	}

	public override List<CAbility> AllListedTriggerAbilities(bool copyAbilities = true)
	{
		List<string> list = new List<string>();
		if (!string.IsNullOrEmpty(ScenarioAbilityID))
		{
			list.Add(ScenarioAbilityID);
		}
		if (TileSpecificAbilityIds != null)
		{
			foreach (TileIndex key in TileSpecificAbilityIds.Keys)
			{
				foreach (string item in TileSpecificAbilityIds[key])
				{
					if (!list.Contains(item))
					{
						list.Add(item);
					}
				}
			}
		}
		List<CAbility> list2 = new List<CAbility>();
		foreach (string abilityID in list)
		{
			ScenarioAbilitiesYMLData scenarioAbilitiesYMLData = ScenarioRuleClient.SRLYML.ScenarioAbilities.SingleOrDefault((ScenarioAbilitiesYMLData a) => a.ScenarioAbilityID == abilityID);
			if (scenarioAbilitiesYMLData == null)
			{
				continue;
			}
			foreach (CAbility scenarioAbility in scenarioAbilitiesYMLData.ScenarioAbilities)
			{
				CAbility cAbility = scenarioAbility;
				if (copyAbilities)
				{
					cAbility = CAbility.CopyAbility(cAbility, generateNewID: false);
				}
				list2.Add(cAbility);
			}
		}
		return list2 ?? new List<CAbility>();
	}
}
