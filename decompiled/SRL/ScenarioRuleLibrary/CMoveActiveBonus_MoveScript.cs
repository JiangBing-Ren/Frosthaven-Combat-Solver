using System.Collections.Generic;
using AStar;

namespace ScenarioRuleLibrary;

public class CMoveActiveBonus_MoveScript : CBespokeBehaviour
{
	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.MoveScript;

	public CMoveActiveBonus_MoveScript(CActor actor, CAbilityMove ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnBehaviourTriggered()
	{
	}

	public override bool IsHiddenInUI()
	{
		return true;
	}

	public override void OnAbilityEnded(CAbility endedAbility)
	{
		CAbility ability = m_ActiveBonus.Ability;
		if (GetScenarioScriptAtRuntime(ability, out var script))
		{
			CScenarioModifier sm = ability.TargetingActor.FindCardWithAbility(ability) as CScenarioModifier;
			script.TickAbilityEnded(endedAbility, sm);
		}
		else
		{
			if (!IsCardsScript(ability))
			{
				return;
			}
			foreach (ScenarioScript scenarioScript in ScenarioManager.CurrentScenarioState.ScenarioScripts)
			{
				if (scenarioScript.IsCardScript)
				{
					scenarioScript.TickAbilityEnded(endedAbility, null);
				}
			}
		}
	}

	public override void OnAbilityUndone(CAbility endedAbility)
	{
		CAbility ability = m_ActiveBonus.Ability;
		if (GetScenarioScriptAtRuntime(ability, out var script))
		{
			CScenarioModifier sm = ability.TargetingActor.FindCardWithAbility(ability) as CScenarioModifier;
			script.TickAbilityUndone(endedAbility, sm);
		}
		else
		{
			if (!IsCardsScript(ability))
			{
				return;
			}
			foreach (ScenarioScript scenarioScript in ScenarioManager.CurrentScenarioState.ScenarioScripts)
			{
				if (scenarioScript.IsCardScript)
				{
					scenarioScript.TickAbilityUndone(endedAbility, null);
				}
			}
		}
	}

	public override void OnAbilityPerformStarted(CAbility ability)
	{
		CAbility ability2 = m_ActiveBonus.Ability;
		if (GetScenarioScriptAtRuntime(ability2, out var script))
		{
			CScenarioModifier sm = ability2.TargetingActor.FindCardWithAbility(ability2) as CScenarioModifier;
			script.TickAbilityPerformStarted(ability, sm);
		}
		else
		{
			if (!IsCardsScript(ability2))
			{
				return;
			}
			foreach (ScenarioScript scenarioScript in ScenarioManager.CurrentScenarioState.ScenarioScripts)
			{
				if (scenarioScript.IsCardScript)
				{
					scenarioScript.TickAbilityPerformStarted(ability, null);
				}
			}
		}
	}

	public override void OnAbilityStarted(CAbility startedAbility)
	{
		CAbility ability = m_ActiveBonus.Ability;
		if (GetScenarioScriptAtRuntime(ability, out var script))
		{
			CScenarioModifier sm = ability.TargetingActor.FindCardWithAbility(ability) as CScenarioModifier;
			script.TickAbilityStarted(startedAbility, sm);
		}
		else
		{
			if (!IsCardsScript(ability))
			{
				return;
			}
			foreach (ScenarioScript scenarioScript in ScenarioManager.CurrentScenarioState.ScenarioScripts)
			{
				if (scenarioScript.IsCardScript)
				{
					scenarioScript.TickAbilityStarted(startedAbility, null);
				}
			}
		}
	}

	public override int ReferenceStrength(CAbility ability, CActor target)
	{
		return 0;
	}

	public override int ReferenceStrengthScalar(CAbility ability, CActor target)
	{
		return 1;
	}

	public override int ReferenceXP(CAbility ability, CActor target)
	{
		return 0;
	}

	public override void OnMoving(CAbilityMove ability)
	{
	}

	public override void OnMoved(CAbility ability, CActor movedActor, List<CActor> actorsCarried, bool newActorCarried, int moveHexes, bool finalMovement, int difficultTerrainTilesEntered, int hazardousTerrainTilesEntered, int thisMoveHexes, Point prevPosition)
	{
		if (moveHexes == 0 || thisMoveHexes == 0)
		{
			return;
		}
		CAbility ability2 = m_ActiveBonus.Ability;
		if (GetScenarioScriptAtRuntime(ability2, out var script))
		{
			CScenarioModifier sm = ability2.TargetingActor.FindCardWithAbility(ability2) as CScenarioModifier;
			script.TickReposition(ability, movedActor, sm);
		}
		else
		{
			if (!IsCardsScript(ability2))
			{
				return;
			}
			foreach (ScenarioScript scenarioScript in ScenarioManager.CurrentScenarioState.ScenarioScripts)
			{
				if (scenarioScript.IsCardScript)
				{
					scenarioScript.TickReposition(ability, movedActor, null);
				}
			}
		}
	}

	public override void OnAttacking(CAbilityAttack ability, CActor target)
	{
		base.OnAttacking(ability, target);
		CAbility ability2 = m_ActiveBonus.Ability;
		if (GetScenarioScriptAtRuntime(ability2, out var script))
		{
			CScenarioModifier sm = ability2.TargetingActor.FindCardWithAbility(ability2) as CScenarioModifier;
			script.TickAttacking(ability, target, sm);
		}
		else
		{
			if (!IsCardsScript(ability2))
			{
				return;
			}
			foreach (ScenarioScript scenarioScript in ScenarioManager.CurrentScenarioState.ScenarioScripts)
			{
				if (scenarioScript.IsCardScript)
				{
					scenarioScript.TickAttacking(ability, target, null);
				}
			}
		}
	}

	public override void OnBeingAttacked(CAbilityAttack ability, int modifiedStrength)
	{
		base.OnBeingAttacked(ability, modifiedStrength);
		CAbility ability2 = m_ActiveBonus.Ability;
		if (GetScenarioScriptAtRuntime(ability2, out var script))
		{
			CScenarioModifier sm = ability2.TargetingActor.FindCardWithAbility(ability2) as CScenarioModifier;
			script.TickBeingAttacked(ability, m_Actor, modifiedStrength, sm);
		}
		else
		{
			if (!IsCardsScript(ability2))
			{
				return;
			}
			foreach (ScenarioScript scenarioScript in ScenarioManager.CurrentScenarioState.ScenarioScripts)
			{
				if (scenarioScript.IsCardScript)
				{
					scenarioScript.TickBeingAttacked(ability, m_Actor, modifiedStrength, null);
				}
			}
		}
	}

	public override void OnTakenDamage(int damageTaken, CAbility damagingAbility, int damageReducedByShields, int actualDamage, CActor damageSourceActor)
	{
		base.OnTakenDamage(damageTaken, damagingAbility, damageReducedByShields, actualDamage, damageSourceActor);
		CAbility ability = m_ActiveBonus.Ability;
		if (GetScenarioScriptAtRuntime(ability, out var script))
		{
			CScenarioModifier sm = ability.TargetingActor.FindCardWithAbility(ability) as CScenarioModifier;
			script.TickTakenDamage(damageTaken, damageReducedByShields, actualDamage, damagingAbility, m_Actor, sm, damageSourceActor);
		}
		else
		{
			if (!IsCardsScript(ability))
			{
				return;
			}
			foreach (ScenarioScript scenarioScript in ScenarioManager.CurrentScenarioState.ScenarioScripts)
			{
				if (scenarioScript.IsCardScript)
				{
					scenarioScript.TickTakenDamage(damageTaken, damageReducedByShields, actualDamage, damagingAbility, m_Actor, null, damageSourceActor);
				}
			}
		}
	}

	public override void OnDeath(CActor actor, CAbility causeOfDeathAbility)
	{
		base.OnDeath(actor, causeOfDeathAbility);
		CAbility ability = m_ActiveBonus.Ability;
		if (GetScenarioScriptAtRuntime(ability, out var script))
		{
			CScenarioModifier sm = ability.TargetingActor.FindCardWithAbility(ability) as CScenarioModifier;
			script.TickDeath(causeOfDeathAbility, actor, sm);
		}
		else
		{
			if (!IsCardsScript(ability))
			{
				return;
			}
			foreach (ScenarioScript scenarioScript in ScenarioManager.CurrentScenarioState.ScenarioScripts)
			{
				if (scenarioScript.IsCardScript)
				{
					scenarioScript.TickDeath(causeOfDeathAbility, actor, null);
				}
			}
		}
	}

	private bool GetScenarioScriptAtRuntime(CAbility ability, out ScenarioScript script)
	{
		if (ScenarioScript.GetName(ability.ActiveBonusData, out var scriptName))
		{
			return ScenarioManager.CurrentScenarioState.FindScenarioScript(scriptName, out script);
		}
		script = null;
		return false;
	}

	private bool IsCardsScript(CAbility ab)
	{
		if (!ScenarioScript.GetName(ab.ActiveBonusData, out var scriptName))
		{
			return false;
		}
		if (scriptName != "CardsScript")
		{
			return false;
		}
		return true;
	}
}
