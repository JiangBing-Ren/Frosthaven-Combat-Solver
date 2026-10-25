using System.Collections.Generic;
using System.Linq;

namespace ScenarioRuleLibrary;

public class CAbilityMoveActiveBonusTracker : CAbilityTargeting
{
	private bool _waitingForCardSelection;

	public CAbilityMoveActiveBonusTracker()
		: base(EAbilityType.MoveActiveBonusTracker)
	{
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_NumberTargetsRemaining = NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		if (!AutoTrigger && m_State == TargetingState.ActorIsSelectingTargetingFocus && AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true) && CanBePerformed(TargetingActor))
		{
			ActorsToTarget.Clear();
			ActorsToTarget.Add(TargetingActor);
			m_State = TargetingState.PreProcessingBeforeActorApplies;
		}
		SetHasStarted(val: true);
	}

	public override void OnActorIsApplying()
	{
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			bool flag = false;
			CPlayerActor cPlayerActor = (CPlayerActor)actor;
			foreach (CActiveBonus activeBonusesWithMovableTracker in GetActiveBonusesWithMovableTrackers(cPlayerActor, Strength))
			{
				foreach (CBaseCard activatedCard in cPlayerActor.CharacterClass.ActivatedCards)
				{
					if (activeBonusesWithMovableTracker.BaseCard == activatedCard)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
			if (flag)
			{
				_waitingForCardSelection = true;
				SetCanUndo(canUndo: true);
				CSelectCardsWithActiveBonusTracker_MessageData message = new CSelectCardsWithActiveBonusTracker_MessageData(AnimOverload, TargetingActor)
				{
					m_Actor = actor,
					m_Ability = this,
					m_FirstAbility = CPhaseAction.IsFirstAbility(this)
				};
				ScenarioRuleClient.MessageHandler(message);
				return true;
			}
		}
		return false;
	}

	public void OnTrackerUpdated(List<CAbilityCard> selectedCards, int moveStrength)
	{
		CPlayerActor cPlayerActor = TargetingActor as CPlayerActor;
		GameState.PlayerSelectedCardsToMoveActiveBonusTracker(cPlayerActor, selectedCards, moveStrength, this);
		CWaitForProgressChoreographer_MessageData message = new CWaitForProgressChoreographer_MessageData(null)
		{
			WaitActor = cPlayerActor,
			WaitTickFrame = 10000,
			ClearEvents = false
		};
		ScenarioRuleClient.MessageHandler(message);
		_waitingForCardSelection = false;
		AbilityHasHappened = true;
		if (TargetingActor.Type == CActor.EType.Player)
		{
			TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
			ScenarioRuleClient.FirstAbilityStarted();
		}
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		bool result = base.AbilityComplete(dontMoveState, out fullAbilityRestart);
		if (_waitingForCardSelection)
		{
			result = false;
		}
		return result;
	}

	public override bool IsPositive()
	{
		return true;
	}

	public override bool CanBePerformed(CActor performer = null)
	{
		CActor cActor = TargetingActor;
		if (cActor == null)
		{
			cActor = performer;
		}
		return GetActiveBonusesWithMovableTrackers(cActor, Strength).Any();
	}

	public static IEnumerable<CActiveBonus> GetActiveBonusesWithMovableTrackers(CActor actor, int moveStrength)
	{
		return from ab in GetActiveBonusesWithTrackers(actor)
			where ((moveStrength < 0) ? (ab.TrackerIndex > 0) : (ab.TrackerIndex >= 0)) && ab.TrackerIndex <= ab.TrackerMaxIndex
			select ab;
	}

	public static IEnumerable<CActiveBonus> GetActiveBonusesWithTrackers(CActor actor)
	{
		return from ab in CharacterClassManager.FindAllActiveBonuses()
			where ab.Actor == actor && ab.HasTracker && ab.BaseCard is CAbilityCard cAbilityCard && cAbilityCard.ClassID == actor.Class.ID && !ab.Ability.GetMiscVar("ImmuneMoveActiveBonusTracker").GetBool()
			select ab;
	}
}
