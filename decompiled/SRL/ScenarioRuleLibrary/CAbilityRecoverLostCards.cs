using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class CAbilityRecoverLostCards : CAbilityTargeting
{
	[JsonProperty]
	public List<EAbilityType> RecoverCardsWithAbilityOfTypeFilter;

	public int StartLostCards;

	public CPlayerActor playerActor;

	public CAbilityRecoverLostCards(List<EAbilityType> recoverCardsWithAbilityOfTypeFilter)
		: base(EAbilityType.RecoverLostCards)
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
		playerActor = null;
		CCharacterClass characterClass = TargetingActor.CharacterClass;
		if (characterClass != null)
		{
			characterClass.LastRecoveredAbilityCards = null;
		}
		SetHasStarted(val: true);
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			AbilityHasHappened = true;
			if (actor.Type == CActor.EType.Player && HasCardsToRecover(((CPlayerActor)actor).CharacterClass.LostAbilityCards))
			{
				playerActor = (CPlayerActor)actor;
				StartLostCards = playerActor.CharacterClass.LostAbilityCards.Count;
				if (Strength == int.MaxValue)
				{
					foreach (CAbilityCard card in playerActor.CharacterClass.LostAbilityCards.ToList())
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
							GameState.RecoverLostCards(card, playerActor);
						}
					}
					CRecoverLostCards_MessageData cRecoverLostCards_MessageData = new CRecoverLostCards_MessageData(AnimOverload, TargetingActor);
					cRecoverLostCards_MessageData.m_ActorRecoveringLostCards = actor;
					cRecoverLostCards_MessageData.m_Ability = this;
					ScenarioRuleClient.MessageHandler(cRecoverLostCards_MessageData);
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
		if (RecoverCardsWithAbilityOfTypeFilter == null || RecoverCardsWithAbilityOfTypeFilter.Count == 0)
		{
			return cards.Count > 0;
		}
		return cards.Exists((CAbilityCard card) => RecoverCardsWithAbilityOfTypeFilter.Any(card.HasAbilityOfType));
	}

	public override void AbilityEnded()
	{
		if (playerActor != null)
		{
			StartLostCards = Math.Max(0, StartLostCards - playerActor.CharacterClass.LostAbilityCards.Count);
		}
		LogEvent(ESESubTypeAbility.AbilityEnded);
		StartLostCards = -1;
		base.AbilityEnded();
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		if (StartLostCards < 0)
		{
			return;
		}
		string secondaryActorClassID = "";
		CActor.EType secondaryActorType = CActor.EType.Unknown;
		bool secondaryActorIsSummon = false;
		CActor actor = null;
		if (ActorsTargeted.Count > 0)
		{
			actor = ActorsTargeted[ActorsTargeted.Count - 1];
			secondaryActorClassID = actor.Class.ID;
			secondaryActorType = actor.Type;
			if (actor.Type == CActor.EType.Enemy)
			{
				CEnemyActor cEnemyActor = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == actor.ActorGuid);
				if (cEnemyActor != null)
				{
					secondaryActorIsSummon = cEnemyActor.IsSummon;
				}
			}
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityTargeting(new SEventAbility.SEventAbilityBaseData(TargetingActor, subTypeAbility, this, defaultAction: false, 1, isBuff: false, secondaryActorClassID, secondaryActorType, secondaryActorIsSummon), m_State, StartLostCards));
	}

	public override bool IsPositive()
	{
		return true;
	}
}
