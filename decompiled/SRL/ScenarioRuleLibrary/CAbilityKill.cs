using System.Collections.Generic;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityKill : CAbilityTargeting
{
	public CAbilityKill()
		: base(EAbilityType.Kill)
	{
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		if (AreaEffect != null)
		{
			TilesInRange = GameState.GetTilesInRange(targetActor, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: true, null, ignorePathLength: false, ignoreBlockedWithActor: true);
			m_ValidTilesInAreaEffect = CAreaEffect.GetValidTiles(TargetingActor, ScenarioManager.Tiles[targetActor.ArrayIndex.X, targetActor.ArrayIndex.Y], AreaEffect, 0f, flipped: false, AreaEffectFlipAxis, GameState.GetTilesInRange(targetActor, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: true), ref m_ValidTilesInAreaEffectIncludingBlocked, ref m_FlagsOfValidTiles, IgnoreAreaEffectLos, IgnoreDoorLocks);
		}
		if (UseSubAbilityTargeting)
		{
			for (int num = ValidActorsInRange.Count - 1; num >= 0; num--)
			{
				CActor cActor = ValidActorsInRange[num];
				if (cActor != null && cActor.IsDead)
				{
					ValidActorsInRange.Remove(cActor);
				}
			}
		}
		SetHasStarted(val: true);
	}

	public override bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		base.ActorIsApplying(actorApplying, actorsAppliedTo);
		CActorIsKilling_MessageData message = new CActorIsKilling_MessageData(AnimOverload, actorApplying)
		{
			m_ActorsAppliedTo = actorsAppliedTo,
			m_KillAbility = this
		};
		ScenarioRuleClient.MessageHandler(message);
		return false;
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (ScenarioManager.Scenario.HasActor(actor) && base.ApplyToActor(actor))
		{
			AbilityHasHappened = true;
			List<CActiveBonus> list = new List<CActiveBonus>();
			GameState.ShowToggleActiveBonusSelection(actor as CPlayerActor, actor, this, EAbilityType.None, CActiveBonus.EActiveBonusBehaviourType.GenericPreventKillAbility);
			CGenericActiveBonus.FireEvent(CActiveBonus.EActiveBonusBehaviourType.GenericPreventKillAbility, this, actor, actor, list);
			if (list.Count > 0)
			{
				return false;
			}
			actor.Health = 0;
			bool actorWasAsleep = actor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
			AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
			if (miscAbilityData != null && miscAbilityData.NoGoldDrop == true)
			{
				actor.NoGoldDrop = true;
			}
			GameState.KillActor(TargetingActor, actor, CActor.ECauseOfDeath.KillAbility, out var _, this, actorWasAsleep, null, GetVar("InstantlyDeletePrefab").GetBool());
		}
		return true;
	}

	public override bool IsPositive()
	{
		return false;
	}

	public override void RemoveImmuneActorsFromList(ref List<CActor> actorList, bool onlyCheck = true)
	{
		if (!TargetingActor.PierceInvulnerability)
		{
			actorList.RemoveAll((CActor x) => x.Invulnerable);
		}
		base.RemoveImmuneActorsFromList(ref actorList, onlyCheck);
	}
}
