using System.Collections.Generic;
using AStar;

namespace ScenarioRuleLibrary;

public class CPreventDamageActiveBonus_PreventAndRetaliate : CBespokeBehaviour
{
	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.PreventAndRetaliate;

	public CPreventDamageActiveBonus_PreventAndRetaliate(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnPreventDamageTriggered(int preventedDamage, CActor damageSource, CActor actorDamaged, CAbility damagingAbility, CActiveBonus preventedByBonus)
	{
		if (preventedByBonus != ActiveBonus)
		{
			return;
		}
		CAbilityRetaliate obj = m_Ability.ActiveBonusData.AbilityData as CAbilityRetaliate;
		int strength = obj.Strength;
		int retaliateRange = obj.RetaliateRange;
		if (damageSource != null && damagingAbility != null && strength > 0 && retaliateRange > 0)
		{
			List<Point> list = ScenarioManager.PathFinder.FindPath(damageSource, damageSource.ArrayIndex, actorDamaged.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out var foundPath, ignoreDoorLocks: false);
			if (foundPath && list.Count <= retaliateRange)
			{
				CAbilityRetaliate cAbilityRetaliate = new CAbilityRetaliate(retaliateRange)
				{
					Strength = strength
				};
				if (!m_ActiveBonus.SetupTriggerContext(this, cAbilityRetaliate, damagingAbility, damageSource, actorDamaged, ("PreventedDamage", preventedDamage), ("RetaliateStrength", strength), ("RetaliateRange", retaliateRange)))
				{
					return;
				}
				CRetaliate_MessageData message = new CRetaliate_MessageData("", actorDamaged)
				{
					m_RetaliateAbility = cAbilityRetaliate,
					m_ActorAppliedTo = actorDamaged
				};
				ScenarioRuleClient.MessageHandler(message);
				damageSource.ApplyRetaliateToAttack(actorDamaged, damagingAbility, strength);
			}
		}
		OnBehaviourTriggered();
	}
}
