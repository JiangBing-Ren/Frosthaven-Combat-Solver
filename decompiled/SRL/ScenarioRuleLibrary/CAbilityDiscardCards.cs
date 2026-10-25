using System.Collections.Generic;
using System.Linq;

namespace ScenarioRuleLibrary;

public class CAbilityDiscardCards : CAbilityTargeting
{
	public CAbilityDiscardCards()
		: base(EAbilityType.DiscardCards)
	{
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_NumberTargetsRemaining = NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		if (AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true))
		{
			CPlayerActor cPlayerActor = targetActor as CPlayerActor;
			if (targetActor.Type != CActor.EType.Player || cPlayerActor.CharacterClass.HandAbilityCards.Count <= 0 || !HasValidCardsWithFilter(cPlayerActor.CharacterClass))
			{
				SetCancelAbilityFlag(cancel: true);
			}
		}
		SetHasStarted(val: true);
	}

	public CExpression CardFilterExpression()
	{
		if (MiscVars == null)
		{
			return null;
		}
		(string, TValue) tuple = MiscVars.SingleOrDefault(((string, TValue) x) => x.Item1 == "CardFilter");
		(string, TValue) tuple2 = tuple;
		if (tuple2.Item1 != null || tuple2.Item2 != default(TValue))
		{
			return tuple.Item2.objVal as CExpression;
		}
		return null;
	}

	private bool HasValidCardsWithFilter(CCharacterClass player)
	{
		CExpression cExpression = CardFilterExpression();
		if (cExpression != null)
		{
			bool flag = false;
			CVars cVars = new CVars();
			CVarsContext context = new CVarsContext(cVars);
			foreach (CAbilityCard handAbilityCard in player.HandAbilityCards)
			{
				cVars.Set("obj", new TValue(handAbilityCard));
				if (cExpression.Calc(context).GetBool())
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	public override bool ApplyToActor(CActor actor)
	{
		CCharacterClass cCharacterClass = (actor as CPlayerActor)?.CharacterClass;
		AbilityHasHappened = false;
		if (!HasValidCardsWithFilter(cCharacterClass))
		{
			return false;
		}
		if (base.ApplyToActor(actor) && actor.Type == CActor.EType.Player && cCharacterClass.HandAbilityCards.Count > 0)
		{
			if (Strength == int.MaxValue)
			{
				AbilityHasHappened = true;
				CPlayerActor cPlayerActor = (CPlayerActor)actor;
				for (int num = cPlayerActor.CharacterClass.HandAbilityCards.Count - 1; num >= 0; num--)
				{
					cPlayerActor.CharacterClass.MoveAbilityCard(cPlayerActor.CharacterClass.HandAbilityCards[num], cPlayerActor.CharacterClass.HandAbilityCards, cPlayerActor.CharacterClass.DiscardedAbilityCards, "HandAbilityCards", "DiscardedAbilityCards");
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
		GameState.PlayerSelectedCardsToDiscard(actor as CPlayerActor, selectedCards);
		AbilityHasHappened = true;
	}

	public override bool IsPositive()
	{
		return false;
	}
}
