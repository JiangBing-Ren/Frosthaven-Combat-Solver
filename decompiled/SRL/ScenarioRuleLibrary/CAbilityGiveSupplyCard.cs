using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class CAbilityGiveSupplyCard : CAbilityTargeting
{
	[JsonProperty]
	public List<int> SupplyCardIDs;

	private int m_SupplyCardsGiven;

	private CPlayerActor m_PlayerActor;

	public CAbilityGiveSupplyCard(List<int> supplyCardIDs)
		: base(EAbilityType.GiveSupplyCard)
	{
		SupplyCardIDs = supplyCardIDs.ToList();
	}

	public override bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		base.ActorIsApplying(actorApplying, actorsAppliedTo);
		m_SupplyCardsGiven = 0;
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
		if (base.ApplyToActor(actor))
		{
			AbilityHasHappened = true;
			TargetingActor.FindCardWithAbility(this);
			CPlayerActor cPlayerActor = (CPlayerActor)TargetingActor;
			if (actor is CPlayerActor playerActor)
			{
				m_PlayerActor = playerActor;
			}
			List<CAbilityCard> list = new List<CAbilityCard>();
			foreach (CAbilityCard supplyCard in cPlayerActor.CharacterClass.SupplyCards)
			{
				if (!SupplyCardIDs.Contains(supplyCard.ID))
				{
					continue;
				}
				if (m_PlayerActor == null)
				{
					foreach (CAbility item2 in supplyCard.Copy().GetTopActionAbilities().ToList())
					{
						if (item2 is CAbilityTargeting)
						{
							item2.TargetingActor = TargetingActor;
							item2.ApplyToActor(actor);
						}
					}
				}
				else
				{
					CAbilityCard item = supplyCard.Copy();
					list.Add(item);
					m_PlayerActor.CharacterClass.HandAbilityCards.Add(item);
					CharacterClassManager.AllAbilityCardInstances.Add(item);
					m_SupplyCardsGiven++;
				}
			}
			if (m_PlayerActor != null)
			{
				CSupplyCardsGiven_MessageData cSupplyCardsGiven_MessageData = new CSupplyCardsGiven_MessageData(TargetingActor);
				cSupplyCardsGiven_MessageData.m_ActorGivenCards = m_PlayerActor;
				cSupplyCardsGiven_MessageData.m_SupplyCardsGiven = list;
				ScenarioRuleClient.MessageHandler(cSupplyCardsGiven_MessageData);
			}
			if (m_PositiveConditions.Count > 0)
			{
				ProcessPositiveStatusEffects(actor);
			}
		}
		return true;
	}

	public override bool IsPositive()
	{
		return true;
	}

	public static List<string> GetSupplyCardNames(CAbilityGiveSupplyCard ability)
	{
		List<string> list = new List<string>();
		if (ability.TargetingActor.Class is CCharacterClass cCharacterClass)
		{
			foreach (CAbilityCard supplyCard in cCharacterClass.SupplyCards)
			{
				if (ability.SupplyCardIDs.Contains(supplyCard.ID))
				{
					list.Add(supplyCard.Name);
				}
			}
		}
		return list;
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		m_SupplyCardsGiven = -1;
		base.AbilityEnded();
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		if (m_SupplyCardsGiven < 0)
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
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityTargeting(new SEventAbility.SEventAbilityBaseData(TargetingActor, subTypeAbility, this, defaultAction: false, 1, isBuff: false, secondaryActorClassID, secondaryActorType, secondaryActorIsSummon), m_State, m_SupplyCardsGiven));
	}
}
