using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

[Serializable]
public class CScenarioModifierTriggerAbility : CScenarioModifier
{
	public enum ETriggerAbilityStackingType
	{
		None,
		StackToPhaseAbilities,
		StackNextCurrentPhaseAbility
	}

	public static readonly ETriggerAbilityStackingType[] TriggerAbilityStackingTypes = (ETriggerAbilityStackingType[])Enum.GetValues(typeof(ETriggerAbilityStackingType));

	public ETriggerAbilityStackingType TriggerAbilityStackingType { get; private set; }

	public Dictionary<TileIndex, List<string>> TileSpecificAbilityIds { get; private set; }

	public List<TileIndex> InlineSubAbilityTileIndexes { get; private set; }

	public int? OverrideTriggerAbilityStrength { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("TriggerAbilityStackingType", TriggerAbilityStackingType);
		info.AddValue("TileSpecificAbilityIds", TileSpecificAbilityIds);
		info.AddValue("InlineSubAbilityTileIndexes", InlineSubAbilityTileIndexes);
		info.AddValue("OverrideTriggerAbilityStrength", OverrideTriggerAbilityStrength);
		base.GetObjectData(info, context);
	}

	public CScenarioModifierTriggerAbility(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "TriggerAbilityStackingType":
					TriggerAbilityStackingType = (ETriggerAbilityStackingType)info.GetValue("TriggerAbilityStackingType", typeof(ETriggerAbilityStackingType));
					break;
				case "TileSpecificAbilityIds":
					TileSpecificAbilityIds = (Dictionary<TileIndex, List<string>>)info.GetValue("TileSpecificAbilityIds", typeof(Dictionary<TileIndex, List<string>>));
					break;
				case "InlineSubAbilityTileIndexes":
					InlineSubAbilityTileIndexes = (List<TileIndex>)info.GetValue("InlineSubAbilityTileIndexes", typeof(List<TileIndex>));
					break;
				case "OverrideTriggerAbilityStrength":
					OverrideTriggerAbilityStrength = (int?)info.GetValue("OverrideTriggerAbilityStrength", typeof(int?));
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CScenarioModifierTriggerAbility entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CScenarioModifierTriggerAbility(string name, int id, int activationRound, EScenarioModifierTriggerPhase triggerPhase, bool applyToEachActorOnce, CObjectiveFilter scenarioModFilter, bool isPositive, string scenarioAbilityID, ETriggerAbilityStackingType triggerAbilityType, Dictionary<TileIndex, List<string>> abilityIdsByLocation, List<TileIndex> inlineSubAbilityTiles, EScenarioModifierActivationType scenarioModifierActivationType = EScenarioModifierActivationType.None, string scenarioModifierActivationID = null, int? overrideTriggerAbilityStrength = null, string customLocKey = null, string customTriggerLocKey = null, string eventId = null, List<CAbility.EAbilityType> afterAbilityTypes = null, List<CAbility.EAttackType> afterAttackTypes = null, bool isHidden = false, bool isDeactivated = false, bool applyOnceTotal = false, bool cancelAllActiveBonusesOnDeactivation = false, EScenarioModifierRoomOpenBehaviour roomOpenBehaviour = EScenarioModifierRoomOpenBehaviour.None, List<string> roomMapGuids = null)
		: base(name, id, activationRound, applyToEachActorOnce, EScenarioModifierType.TriggerAbility, triggerPhase, scenarioModifierActivationType, scenarioModifierActivationID, scenarioModFilter, isPositive, scenarioAbilityID, customLocKey, customTriggerLocKey, eventId, afterAbilityTypes, afterAttackTypes, isHidden, isDeactivated, applyOnceTotal, cancelAllActiveBonusesOnDeactivation, roomOpenBehaviour, roomMapGuids)
	{
		TriggerAbilityStackingType = triggerAbilityType;
		TileSpecificAbilityIds = abilityIdsByLocation?.ToDictionary((KeyValuePair<TileIndex, List<string>> kv) => kv.Key, (KeyValuePair<TileIndex, List<string>> kv) => kv.Value) ?? null;
		InlineSubAbilityTileIndexes = inlineSubAbilityTiles.ToList();
		OverrideTriggerAbilityStrength = overrideTriggerAbilityStrength;
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
		if (!IsActiveInRound(roundCount, forceActivate) || currentActor == null || (!IsPositive && IgnoreNegativeScenarioEffects(currentActor)) || (ApplyOnceTotal && HasBeenAppliedOnce) || AppliedToActorGUIDs.Contains(currentActor.ActorGuid))
		{
			return;
		}
		bool flag = false;
		if (ScenarioModifierFilter != null)
		{
			ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.ToList().SingleOrDefault((ActorState x) => x.ActorGuid == currentActor.ActorGuid);
			if (actorState != null && ScenarioModifierFilter.IsValidTarget(actorState))
			{
				flag = true;
			}
		}
		if (!flag)
		{
			return;
		}
		List<CAbility> list = TriggerAbilities(currentActor, ScenarioManager.CurrentScenarioState.Players.Count);
		if (list == null)
		{
			return;
		}
		if (currentActor is CEnemyActor cEnemyActor)
		{
			for (int num = 0; num < list.Count; num++)
			{
				CAbility ability = list[num];
				CAbility value = cEnemyActor.CloneAbilityAndApplyBaseStats(ability);
				list[num] = value;
			}
		}
		if (OverrideTriggerAbilityStrength.HasValue)
		{
			foreach (CAbility item in list)
			{
				item.Strength = OverrideTriggerAbilityStrength.Value;
			}
		}
		foreach (CAbility item2 in list)
		{
			item2.IsScenarioModifierAbility = true;
			item2.SetMiscVar("TreatAsEventEffect", TreatAsEventEffect);
			item2.SetMiscVar("TreatAsScenarioRule", TreatAsScenarioRule);
			if (!item2.IsInlineSubAbility)
			{
				continue;
			}
			foreach (TileIndex inlineSubAbilityTileIndex in InlineSubAbilityTileIndexes)
			{
				item2.InlineSubAbilityTiles.Add(ScenarioManager.Tiles[inlineSubAbilityTileIndex.X, inlineSubAbilityTileIndex.Y]);
			}
		}
		if (PhaseManager.CurrentPhase is CPhaseAction)
		{
			(PhaseManager.CurrentPhase as CPhaseAction).StackNextAbilities(list.ToList(), currentActor, killAfter: false, TriggerAbilityStackingType == ETriggerAbilityStackingType.StackNextCurrentPhaseAbility);
		}
		else
		{
			if (ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.StartScenario)
			{
				CAbility cAbility = CAbility.CreateAbility(CAbility.EAbilityType.Null, CAbilityFilterContainer.CreateDefaultFilter(), isMonster: false, isTargetedAbility: true);
				if (cAbility.MiscAbilityData == null)
				{
					cAbility.MiscAbilityData = new AbilityData.MiscAbilityData();
				}
				cAbility.MiscAbilityData.AutotriggerAbility = true;
				GameState.PendingScenarioModifierAbilities.Add(new Tuple<CAbility, CActor>(cAbility, currentActor));
			}
			for (int num2 = 0; num2 < list.Count; num2++)
			{
				GameState.PendingScenarioModifierAbilities.Add(new Tuple<CAbility, CActor>(list[num2], currentActor));
			}
		}
		if (ApplyToEachActorOnce)
		{
			AppliedToActorGUIDs.Add(currentActor.ActorGuid);
		}
		HasBeenAppliedOnce = true;
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
