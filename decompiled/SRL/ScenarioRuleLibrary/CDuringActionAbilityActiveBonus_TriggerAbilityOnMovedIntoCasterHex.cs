using System.Collections.Generic;
using AStar;

namespace ScenarioRuleLibrary;

public class CDuringActionAbilityActiveBonus_TriggerAbilityOnMovedIntoCasterHex : CBespokeBehaviour
{
	private CAbilityAddActiveBonus m_AbilityAddActiveBonus;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.DuringActionAbilityOnMovedIntoCasterHex;

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnMovedIntoCasterHex(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		m_AbilityAddActiveBonus = ability as CAbilityAddActiveBonus;
	}

	public override void OnMoved(CAbility moveAbility, CActor movedActor, List<CActor> actorsCarried, bool newActorCarried, int moveHexes, bool finalMovement, int difficultTerrainTilesEntered, int hazardousTerrainTilesEntered, int thisMoveHexes, Point prevPosition)
	{
		if (thisMoveHexes <= 0 || finalMovement || m_ActiveBonus.Actor.IsDead || !IsValidAbilityType(moveAbility) || !IsValidTarget(moveAbility, movedActor) || !m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(movedActor))
		{
			return;
		}
		if (movedActor != m_ActiveBonus.Caster)
		{
			if (movedActor.ArrayIndex.Equals(m_ActiveBonus.Caster.ArrayIndex))
			{
				DoAbility(movedActor);
			}
		}
		else
		{
			if (!m_ActiveBonusData.IsAura)
			{
				return;
			}
			foreach (CActor item in m_ActiveBonus.ValidActorsInRangeOfAura)
			{
				if (moveHexes > 0 && item != m_ActiveBonus.Caster && (!actorsCarried.Contains(item) | newActorCarried) && item.ArrayIndex.Equals(m_ActiveBonus.Caster.ArrayIndex))
				{
					DoAbility(item);
				}
			}
		}
	}

	private void DoAbility(CActor actor)
	{
		CActor cActor = (m_ActiveBonusData.TriggerOnCaster ? m_ActiveBonus.Caster : actor);
		CAbility cAbility = CAbility.CopyAbility(m_AbilityAddActiveBonus.AddAbility, generateNewID: false);
		if (m_ActiveBonus.SetupTriggerContext(this, cAbility, null, actor, cActor))
		{
			cAbility.CanUndoOverride = false;
			if (cAbility.IsInlineSubAbility)
			{
				cAbility.InlineSubAbilityTiles.Add(ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y]);
			}
			List<CAbility> nextAbilities = new List<CAbility> { cAbility };
			if (GameState.InternalCurrentActor != cActor)
			{
				GameState.OverrideCurrentActorForOneAction(cActor);
			}
			(PhaseManager.CurrentPhase as CPhaseAction).StackNextAbilities(nextAbilities, cActor, killAfter: false, stackToNextCurrent: true);
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			m_ActiveBonus.RestrictActiveBonus(cActor);
			OnBehaviourTriggered();
			if (m_ActiveBonus.HasTracker)
			{
				m_ActiveBonus.UpdateXPTracker();
				m_ActiveBonus.FinishIfNoRemaining();
			}
		}
	}

	protected override bool IsValidAbilityType(CAbility conditionAbility)
	{
		if (m_ActiveBonusData.ValidAbilityTypes.Count <= 0)
		{
			if (conditionAbility.AbilityType != CAbility.EAbilityType.Move && conditionAbility.AbilityType != CAbility.EAbilityType.Push && conditionAbility.AbilityType != CAbility.EAbilityType.Pull && conditionAbility.AbilityType != CAbility.EAbilityType.Teleport2)
			{
				return conditionAbility.AbilityType == CAbility.EAbilityType.Fear;
			}
			return true;
		}
		return m_ActiveBonusData.ValidAbilityTypes.Contains(conditionAbility.AbilityType);
	}
}
