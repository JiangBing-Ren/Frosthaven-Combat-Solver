using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CAbilityCapture : CAbilityTargeting
{
	public CAbilityCapture()
		: base(EAbilityType.Capture)
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
		RemoveImmuneActorsFromList(ref m_ValidActorsInRange, onlyCheck: false);
		SetHasStarted(val: true);
	}

	public override bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		base.ActorIsApplying(actorApplying, actorsAppliedTo);
		CActorIsCapturing_MessageData message = new CActorIsCapturing_MessageData(AnimOverload, actorApplying)
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
			actor.Health = 0;
			bool actorWasAsleep = actor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
			actor.NoGoldDrop = true;
			GameState.KillActor(TargetingActor, actor, CActor.ECauseOfDeath.KillAbility, out var _, this, actorWasAsleep);
			ScenarioManager.CurrentScenarioState.CapturedEnemies.Add(((uint)(actor as CEnemyActor).PetCard.ID, TargetingActor.Class.ID));
		}
		return true;
	}

	public override bool IsPositive()
	{
		return false;
	}

	public override void RemoveImmuneActorsFromList(ref List<CActor> actorList, bool onlyCheck = true)
	{
		actorList.RemoveAll((CActor x) => (x as CEnemyActor)?.PetCard == null || x.Health > Strength);
		base.RemoveImmuneActorsFromList(ref actorList, onlyCheck);
	}
}
