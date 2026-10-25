using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class CAbilityRecoverDiscardedCards : CAbilityTargeting
{
	[JsonProperty]
	public List<EAbilityType> RecoverCardsWithAbilityOfTypeFilter;

	public CAbilityRecoverDiscardedCards(List<EAbilityType> recoverCardsWithAbilityOfTypeFilter)
		: base(EAbilityType.RecoverDiscardedCards)
	{
		RecoverCardsWithAbilityOfTypeFilter = recoverCardsWithAbilityOfTypeFilter;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		if (Strength != int.MaxValue)
		{
			m_OneTargetAtATime = true;
		}
		base.Start(targetActor, filterActor, controllingActor);
		m_NumberTargetsRemaining = NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		CCharacterClass characterClass = TargetingActor.CharacterClass;
		if (characterClass != null)
		{
			characterClass.LastRecoveredAbilityCards = null;
		}
		SetHasStarted(val: true);
	}

	public override bool CanBePerformed(CActor performer = null)
	{
		if (ModifiedStrength() == 0)
		{
			InvalidTileReason = GetMiscVar("HelpTextWhenCancelled").GetString();
			return false;
		}
		InvalidTileReason = null;
		return base.CanBePerformed(performer);
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			AbilityHasHappened = true;
			if (actor.Type == CActor.EType.Player && HasCardsToRecover(((CPlayerActor)actor).CharacterClass.DiscardedAbilityCards))
			{
				if (Strength == int.MaxValue)
				{
					CPlayerActor cPlayerActor = (CPlayerActor)actor;
					foreach (CAbilityCard card in cPlayerActor.CharacterClass.DiscardedAbilityCards.ToList())
					{
						bool flag = false;
						if (RecoverCardsWithAbilityOfTypeFilter.Count > 0)
						{
							if (RecoverCardsWithAbilityOfTypeFilter.Any((EAbilityType a) => card.HasAbilityOfType(a)))
							{
								flag = true;
							}
						}
						else
						{
							flag = true;
						}
						if (flag)
						{
							GameState.RecoverDiscardedCards(card, cPlayerActor);
						}
					}
					CRecoverDiscardedCards_MessageData cRecoverDiscardedCards_MessageData = new CRecoverDiscardedCards_MessageData(AnimOverload, TargetingActor);
					cRecoverDiscardedCards_MessageData.m_ActorRecoveringLostCards = actor;
					cRecoverDiscardedCards_MessageData.m_Ability = this;
					ScenarioRuleClient.MessageHandler(cRecoverDiscardedCards_MessageData);
					return true;
				}
				CSelectRecoverCards_MessageData cSelectRecoverCards_MessageData = new CSelectRecoverCards_MessageData(AnimOverload, TargetingActor);
				cSelectRecoverCards_MessageData.m_ActorRecoveringLostCards = actor;
				cSelectRecoverCards_MessageData.m_Ability = this;
				cSelectRecoverCards_MessageData.m_MaxCardLevel = VarsContext.GetVar("CardLevel").GetInt(null, 99);
				ScenarioRuleClient.MessageHandler(cSelectRecoverCards_MessageData);
				return true;
			}
		}
		return false;
	}

	private bool HasCardsToRecover(List<CAbilityCard> cards)
	{
		if (Flags.HasFlag(EAbilityFlag.FrozenFistMechanic))
		{
			return true;
		}
		int level = VarsContext.GetVar("CardLevel").GetInt(null, 99);
		cards = cards.Where((CAbilityCard x) => x.Level <= level).ToList();
		if (RecoverCardsWithAbilityOfTypeFilter == null || RecoverCardsWithAbilityOfTypeFilter.Count == 0)
		{
			return cards.Count > 0;
		}
		return cards.Exists((CAbilityCard card) => RecoverCardsWithAbilityOfTypeFilter.Any(card.HasAbilityOfType));
	}

	public override bool IsPositive()
	{
		return true;
	}
}
