using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CAbilityLoseCards : CAbilityTargeting
{
	public CAbilityLoseCards()
		: base(EAbilityType.LoseCards)
	{
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_NumberTargetsRemaining = NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		SetHasStarted(val: true);
	}

	public override bool ApplyToActor(CActor actor)
	{
		AbilityHasHappened = false;
		if (base.ApplyToActor(actor) && actor.Type == CActor.EType.Player && ((CPlayerActor)actor).CharacterClass.HandAbilityCards.Count + ((CPlayerActor)actor).CharacterClass.DiscardedAbilityCards.Count > 0)
		{
			if (Strength == int.MaxValue)
			{
				AbilityHasHappened = true;
				CPlayerActor cPlayerActor = (CPlayerActor)actor;
				for (int num = cPlayerActor.CharacterClass.HandAbilityCards.Count - 1; num >= 0; num--)
				{
					cPlayerActor.CharacterClass.MoveAbilityCard(cPlayerActor.CharacterClass.HandAbilityCards[num], cPlayerActor.CharacterClass.HandAbilityCards, cPlayerActor.CharacterClass.LostAbilityCards, "HandAbilityCards", "LostAbilityCards");
				}
				CLoseCards_MessageData cLoseCards_MessageData = new CLoseCards_MessageData(AnimOverload, TargetingActor);
				cLoseCards_MessageData.m_ActorLosingCards = actor;
				cLoseCards_MessageData.m_Ability = this;
				ScenarioRuleClient.MessageHandler(cLoseCards_MessageData);
				return true;
			}
			_ = (CPlayerActor)actor;
			CSelectLoseCards_MessageData cSelectLoseCards_MessageData = new CSelectLoseCards_MessageData(AnimOverload, TargetingActor);
			cSelectLoseCards_MessageData.m_ActorLosingCards = actor;
			cSelectLoseCards_MessageData.m_Ability = this;
			ScenarioRuleClient.MessageHandler(cSelectLoseCards_MessageData);
			return true;
		}
		return false;
	}

	public void OnCardsSelected(List<CAbilityCard> selectedCards, CActor actor)
	{
		GameState.PlayerSelectedCardsToLose(actor as CPlayerActor, selectedCards);
		AbilityHasHappened = true;
	}

	public override bool IsPositive()
	{
		return false;
	}
}
