using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CAbilityRemoveActorFromMap : CAbilityTargeting
{
	public CAbilityRemoveActorFromMap()
		: base(EAbilityType.RemoveActorFromMap)
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
		CActorIsApplyingConditionActiveBonus_MessageData message = new CActorIsApplyingConditionActiveBonus_MessageData(AnimOverload, actorApplying)
		{
			m_Ability = this,
			m_ActorsAppliedTo = actorsAppliedTo
		};
		ScenarioRuleClient.MessageHandler(message);
		return false;
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (ScenarioManager.Scenario.HasActor(actor) && base.ApplyToActor(actor))
		{
			AbilityHasHappened = true;
			GameState.KillActor(TargetingActor, actor, CActor.ECauseOfDeath.ActorRemovedFromMap, out var _, this);
		}
		return true;
	}

	public override bool IsPositive()
	{
		return true;
	}
}
