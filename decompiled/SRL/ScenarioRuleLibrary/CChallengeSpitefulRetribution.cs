using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class CChallengeSpitefulRetribution : CChallenge
{
	private List<string> actorsAppliedTo = new List<string>();

	public override uint ID => 1525u;

	public override string Name => "spiteful_retribution";

	public override void Reset()
	{
		base.Reset();
		actorsAppliedTo.Clear();
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		if (!string.IsNullOrEmpty(storage))
		{
			actorsAppliedTo = JsonConvert.DeserializeObject<List<string>>(storage);
		}
	}

	public override IEnumerable<Trigger> GetTriggers()
	{
		yield return Trigger.OnSpawn;
	}

	protected override string SerializeInternal()
	{
		return JsonConvert.SerializeObject(actorsAppliedTo);
	}

	protected override void Perform(TriggerParams triggerParams = null)
	{
		if (triggerParams.actor is CEnemyActor cEnemyActor && (cEnemyActor.Type == CActor.EType.Enemy || cEnemyActor.Type == CActor.EType.Enemy2) && !actorsAppliedTo.Contains(cEnemyActor.ActorGuid))
		{
			actorsAppliedTo.Add(cEnemyActor.ActorGuid);
			CAbilityAddCondition obj = CAbility.CreateAbility(CAbility.EAbilityType.AddCondition, 1, useSpecialBaseStat: false, 1, 1, 0, CAbility.EAbilityFlag.None, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), null, null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, null, null, null, null, null, null, null, null, null, multiPassAttack: false, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, null, textOnly: false, noLayoutSpace: false, showRange: false, showTarget: false, showArea: false, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, null, null, CAbility.EAbilityTargeting.All, null, null, null, null, null, isMonster: false, null, null, isSubAbility: false, isInlineSubAbility: false, 0, 0, isTargetedAbility: true, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, null, null, 0, null, null, skipAnim: false, 0, EConditionDecTrigger.None, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, null, 0, null, null, null, null, CAbility.EAttackType.None, null, null, CAbility.ECardAction.None, null, null, CAbilityExtraTurn.EExtraTurnType.None, null, null, null, null, null, null, null, null, immuneToSelfDamage: false, null, null, null, null, null, null, ECharacter.None, CAbilityFilter.EFilterTile.None, null, isDefault: false, isItemAbility: false, showElementPicker: false, isModifierAbility: true, null, null, null, null, exploitVulnerability: false, null, null, int.MaxValue) as CAbilityAddCondition;
			obj.Name = "SpitefulRetributionStrengthen";
			obj.ActiveBonusData.Duration = CActiveBonus.EActiveBonusDurationType.Persistent;
			obj.ActiveBonusData.Behaviour = CActiveBonus.EActiveBonusBehaviourType.ApplyConditionOnLoseCondition;
			obj.ActiveBonusData.ValidNegativeConditionTypes = CCondition.NegativeConditions.ToList();
			obj.ActiveBonusData.AbilityData = CAbilityNullTargeting.CreateDefaultNullTargeting(1, 1, isMonster: true);
			obj.ActiveBonusData.AbilityData.AddPositiveCondition(CCondition.EPositiveCondition.Strengthen);
			obj.ActiveBonusData.Hidden = true;
			obj.Start(cEnemyActor, cEnemyActor);
			obj.ApplyToActor(cEnemyActor);
		}
	}
}
