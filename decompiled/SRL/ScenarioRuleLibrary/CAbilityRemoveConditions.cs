using System.Collections.Generic;
using System.Linq;

namespace ScenarioRuleLibrary;

public class CAbilityRemoveConditions : CAbilityTargeting
{
	public const int STRENGTH_ALL = -1;

	public new Dictionary<CCondition.ENegativeCondition, CAbility> OriginalNegativeConditions;

	public new Dictionary<CCondition.EPositiveCondition, CAbility> OriginalPositiveConditions;

	public CAbilityRemoveConditions()
		: base(EAbilityType.RemoveConditions)
	{
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		if (OriginalNegativeConditions == null)
		{
			OriginalNegativeConditions = new Dictionary<CCondition.ENegativeCondition, CAbility>();
			foreach (KeyValuePair<CCondition.ENegativeCondition, CAbility> negativeCondition in m_NegativeConditions)
			{
				OriginalNegativeConditions.Add(negativeCondition.Key, negativeCondition.Value);
			}
		}
		if (OriginalPositiveConditions == null)
		{
			OriginalPositiveConditions = new Dictionary<CCondition.EPositiveCondition, CAbility>();
			foreach (KeyValuePair<CCondition.EPositiveCondition, CAbility> positiveCondition in m_PositiveConditions)
			{
				OriginalPositiveConditions.Add(positiveCondition.Key, positiveCondition.Value);
			}
		}
		SetHasStarted(val: true);
	}

	public override bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		base.ActorIsApplying(actorApplying, actorsAppliedTo);
		if (Strength != -1)
		{
			FilterConditions(actorsAppliedTo[0]);
		}
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
			RemoveConditions(TargetingActor, actor, m_NegativeConditions.Keys.ToList(), m_PositiveConditions.Keys.ToList(), this, AnimOverload);
		}
		return true;
	}

	public static void RemoveConditions(CActor applyingActor, CActor actor, List<CCondition.ENegativeCondition> negativeConditions, List<CCondition.EPositiveCondition> positiveConditions, CAbility ability, string animOverload = "None")
	{
		if (negativeConditions != null && negativeConditions.Count > 0)
		{
			foreach (CCondition.ENegativeCondition negativeCondition in negativeConditions)
			{
				if (negativeCondition == CCondition.ENegativeCondition.Curse)
				{
					actor.Class.RemoveCurses(actor);
				}
				if (actor.RemoveNegativeConditionToken(negativeCondition, EConditionDecTrigger.None, applyingActor, new List<EConditionDecTrigger>
				{
					EConditionDecTrigger.WhenAnimatedClaymoreEnds,
					EConditionDecTrigger.WhenFrozenFistMechanicActiveBonusEnds
				}))
				{
					CActorIsRemovingCondition_MessageData message = new CActorIsRemovingCondition_MessageData(animOverload, applyingActor)
					{
						m_Ability = ability,
						m_ActorAppliedTo = actor,
						m_NegativeCondition = negativeCondition
					};
					ScenarioRuleClient.MessageHandler(message);
				}
			}
		}
		if (positiveConditions == null || positiveConditions.Count <= 0)
		{
			return;
		}
		foreach (CCondition.EPositiveCondition positiveCondition in positiveConditions)
		{
			if (positiveCondition == CCondition.EPositiveCondition.Bless)
			{
				actor.Class.RemoveBlesses(actor);
			}
			actor.RemovePositiveConditionToken(positiveCondition, EConditionDecTrigger.None, new List<EConditionDecTrigger>
			{
				EConditionDecTrigger.WhenAnimatedClaymoreEnds,
				EConditionDecTrigger.WhenFrozenFistMechanicActiveBonusEnds
			});
		}
	}

	protected void FilterConditions(CActor actor)
	{
		m_NegativeConditions.Clear();
		m_PositiveConditions.Clear();
		bool flag = false;
		int num = 0;
		foreach (KeyValuePair<CCondition.ENegativeCondition, CAbility> originalNegativeCondition in OriginalNegativeConditions)
		{
			if (num < Strength)
			{
				if (actor.Tokens.HasKey(originalNegativeCondition.Key))
				{
					flag = true;
				}
				if (originalNegativeCondition.Key == CCondition.ENegativeCondition.Curse && actor.Class.GetCurseCards().Count > 0)
				{
					flag = true;
				}
				if (flag)
				{
					m_NegativeConditions.Add(originalNegativeCondition.Key, originalNegativeCondition.Value);
					num++;
				}
			}
		}
		foreach (KeyValuePair<CCondition.EPositiveCondition, CAbility> originalPositiveCondition in OriginalPositiveConditions)
		{
			if (num < Strength)
			{
				if (actor.Tokens.HasKey(originalPositiveCondition.Key))
				{
					flag = true;
				}
				if (originalPositiveCondition.Key == CCondition.EPositiveCondition.Bless && actor.Class.GetBlessCard().Count > 0)
				{
					flag = true;
				}
				if (flag)
				{
					m_PositiveConditions.Add(originalPositiveCondition.Key, originalPositiveCondition.Value);
					num++;
				}
			}
		}
	}

	public override bool IsPositive()
	{
		return true;
	}
}
