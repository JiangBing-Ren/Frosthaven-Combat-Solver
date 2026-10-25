using System.Collections.Generic;
using System.Linq;

namespace ScenarioRuleLibrary;

public class CPhaseSelectMonsterClassAbilityCards : CPhase
{
	public bool HasShownCards;

	public bool HasPromptedItems;

	public bool HasDoneAbilitiesAfterItems;

	public CPhaseSelectMonsterClassAbilityCards()
	{
		m_PhaseType = PhaseType.MonsterClassesSelectAbilityCards;
	}

	protected override void OnNextStep(bool passing = false)
	{
		if (HasShownCards)
		{
			GameState.NextPhase();
			return;
		}
		if (GameState.CurrentActionInitiator == GameState.EActionInitiator.ActionsTriggeredOutsideActionPhase)
		{
			GameState.CurrentActionInitiator = GameState.EActionInitiator.None;
		}
		foreach (CScenarioModifier item in ScenarioManager.CurrentScenarioState.ScenarioModifiers.Where((CScenarioModifier m) => m.ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.BeforeSelectMonsterClassAbilityCards))
		{
			item.PerformScenarioModifierInRound(ScenarioManager.CurrentScenarioState.RoundNumber);
		}
		List<CMonsterClass> list = new List<CMonsterClass>();
		foreach (CMonsterClass monsterClass in MonsterClassManager.Classes)
		{
			if (ScenarioManager.Scenario.AllAliveMonsters.Find((CEnemyActor x) => x.MonsterClass == monsterClass) != null)
			{
				list.Add(monsterClass);
			}
		}
		foreach (CObjectClass objectClass in MonsterClassManager.ObjectClasses)
		{
			if (ScenarioManager.Scenario.Objects.Find((CObjectActor x) => x.MonsterClass == objectClass) != null)
			{
				list.Add(objectClass);
			}
		}
		CMonsterClassesToSelectAbilityCards_MessageData cMonsterClassesToSelectAbilityCards_MessageData = new CMonsterClassesToSelectAbilityCards_MessageData(null);
		cMonsterClassesToSelectAbilityCards_MessageData.m_MonsterClasses = list;
		ScenarioRuleClient.MessageHandler(cMonsterClassesToSelectAbilityCards_MessageData);
		HasShownCards = true;
	}

	public bool PromptBeforeEnd()
	{
		List<(CAbility, CActor)> list = new List<(CAbility, CActor)>();
		if (!HasPromptedItems)
		{
			HasPromptedItems = true;
			foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
			{
				List<CItem> usedItems = new List<CItem>();
				GameState.ShowItemSelection(playerActor, playerActor, null, CItem.EItemTrigger.AfterMonsterCardsRevealed, onlyInActionPhase: false, killAfter: false, null, usedItems);
			}
			if (PhaseManager.CurrentPhaseAction != null)
			{
				return true;
			}
		}
		if (!HasDoneAbilitiesAfterItems)
		{
			HasDoneAbilitiesAfterItems = true;
			foreach (CPlayerActor playerActor2 in ScenarioManager.Scenario.PlayerActors)
			{
				foreach (CGenericActiveBonus item in playerActor2.FindApplicableActiveBonuses(CAbility.EAbilityType.AddActiveBonus, CActiveBonus.EActiveBonusBehaviourType.GenericAfterRevealMonsterCards))
				{
					if (item.GetBehaviour<CGenericActiveBonus_BespokeBehaviour>().PrepAbilityForEvent(CActiveBonus.EActiveBonusBehaviourType.GenericAfterRevealMonsterCards, null, playerActor2, null, out var abilityToAdd, out var actorToUse))
					{
						item.RestrictActiveBonus(playerActor2, manualRestrict: true);
						list.Add((abilityToAdd, actorToUse));
					}
				}
			}
			if (list.Count > 0)
			{
				PhaseManager.StartAbilities(list);
				return true;
			}
		}
		return false;
	}

	protected override void OnEndPhase()
	{
		foreach (CScenarioModifier item in ScenarioManager.CurrentScenarioState.ScenarioModifiers.Where((CScenarioModifier m) => m.ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.AfterSelectMonsterClassAbilityCards))
		{
			item.PerformScenarioModifierInRound(ScenarioManager.CurrentScenarioState.RoundNumber);
		}
		CMonsterClassesHaveSelectedAbilityCards_MessageData message = new CMonsterClassesHaveSelectedAbilityCards_MessageData(null);
		ScenarioRuleClient.MessageHandler(message);
	}
}
