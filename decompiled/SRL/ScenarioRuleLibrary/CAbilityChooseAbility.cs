using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class CAbilityChooseAbility : CAbilityTargeting
{
	[JsonProperty]
	public List<CAbility> ChooseAbilities { get; set; }

	public List<CAbility> ApplicableAbilities { get; private set; }

	private CAbility _chosenAbility { get; set; }

	public CAbility ChosenAbility { get; private set; }

	public CBaseCard ParentBaseCard { get; set; }

	public bool HasChosenAbility { get; set; }

	public CAbilityChooseAbility(List<CAbility> chooseAbilities, CBaseCard parentBaseCard = null)
		: base(EAbilityType.Choose)
	{
		ChooseAbilities = ((chooseAbilities != null) ? chooseAbilities : new List<CAbility>());
		ParentBaseCard = parentBaseCard;
		HasChosenAbility = false;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		ChosenAbility = null;
		if (AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true))
		{
			m_ActorsToTarget.Add(targetActor);
			m_State = TargetingState.PreProcessingBeforeActorApplies;
		}
		CChosenAbilityChanged_MessageData cChosenAbilityChanged_MessageData = new CChosenAbilityChanged_MessageData(TargetingActor);
		cChosenAbilityChanged_MessageData.m_ChooseAbility = this;
		ScenarioRuleClient.MessageHandler(cChosenAbilityChanged_MessageData);
		SetHasStarted(val: true);
	}

	public override void Restart()
	{
		base.Restart();
		ChosenAbility = null;
		if (AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true))
		{
			m_ActorsToTarget.Add(TargetingActor);
			m_State = TargetingState.PreProcessingBeforeActorApplies;
		}
		CChosenAbilityChanged_MessageData cChosenAbilityChanged_MessageData = new CChosenAbilityChanged_MessageData(TargetingActor);
		cChosenAbilityChanged_MessageData.m_ChooseAbility = this;
		ScenarioRuleClient.MessageHandler(cChosenAbilityChanged_MessageData);
	}

	public override bool PreProcessBeforeActorApplies()
	{
		FilterChooseAbilities(m_ActorsToTarget[0]);
		if (ApplicableAbilities.Count == 0)
		{
			string text = GetMiscVar("CancelReason").GetString();
			if (!string.IsNullOrEmpty(text))
			{
				SetCancelReason(text);
				SetCancelAbilityFlag(cancel: true);
				Perform();
				return false;
			}
		}
		CShowSelectAbility_MessageData message = new CShowSelectAbility_MessageData(TargetingActor)
		{
			m_ChooseAbility = this,
			m_IsFirstAbility = (PhaseManager.CurrentPhaseAction.FirstPhaseAbility == PhaseManager.CurrentPhaseAction.CurrentPhaseAbility)
		};
		ScenarioRuleClient.MessageHandler(message);
		base.PreProcessBeforeActorApplies();
		return false;
	}

	public bool HasValidChooseAbilities(CActor actor)
	{
		FilterChooseAbilities(actor);
		return ApplicableAbilities.Count > 0;
	}

	public override bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		base.ActorIsApplying(actorApplying, actorsAppliedTo);
		if (MiscAbilityData.HasCondition == true)
		{
			CActorIsApplyingConditionActiveBonus_MessageData message = new CActorIsApplyingConditionActiveBonus_MessageData(AnimOverload, actorApplying)
			{
				m_Ability = this,
				m_ActorsAppliedTo = actorsAppliedTo
			};
			ScenarioRuleClient.MessageHandler(message);
			return false;
		}
		return true;
	}

	public override bool Perform()
	{
		if (ChosenAbility == null && !CanSkip && m_State == TargetingState.ActorIsApplying)
		{
			return false;
		}
		return base.Perform();
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (_chosenAbility != null)
		{
			AbilityHasHappened = true;
			DoAbility(actor, toggledOn: true);
			ResetChosenAbility(fromClick: false);
		}
		if (m_PositiveConditions.Count > 0)
		{
			ProcessPositiveStatusEffects(actor);
		}
		return true;
	}

	protected void FilterChooseAbilities(CActor actor)
	{
		ApplicableAbilities = new List<CAbility>();
		foreach (CAbility chooseAbility in ChooseAbilities)
		{
			chooseAbility.AddParentContext(VarsContext);
			TValue miscVar = GetMiscVar("IsValidChooseAbility");
			if (!miscVar.isUnknown)
			{
				CVars cVars = new CVars();
				CVarsContext context = new CVarsContext(cVars, VarsContext);
				cVars.Set("Ability", new TValue(chooseAbility));
				cVars.Set("Target", new TValue(actor));
				cVars.Set("TargetingActor", new TValue(TargetingActor));
				if (!miscVar.GetBool(context, defVal: true))
				{
					continue;
				}
			}
			if (!chooseAbility.MeetsStartAbilityRequirements(actor))
			{
				continue;
			}
			bool flag = false;
			if (MiscAbilityData.HasCondition != true)
			{
				flag = true;
			}
			else
			{
				foreach (CCondition.ENegativeCondition allNegativeCondition in chooseAbility.GetAllNegativeConditions())
				{
					if (actor.Tokens.HasKey(allNegativeCondition))
					{
						flag = true;
					}
					if (allNegativeCondition == CCondition.ENegativeCondition.Curse && actor.Class.GetCurseCards().Count > 0)
					{
						flag = true;
					}
				}
				foreach (CCondition.EPositiveCondition allPositiveCondition in chooseAbility.GetAllPositiveConditions())
				{
					if (actor.Tokens.HasKey(allPositiveCondition))
					{
						flag = true;
					}
					if (allPositiveCondition == CCondition.EPositiveCondition.Bless && actor.Class.GetBlessCard().Count > 0)
					{
						flag = true;
					}
				}
			}
			if (flag)
			{
				ApplicableAbilities.Add(chooseAbility);
			}
		}
	}

	public void DoAbility(CActor currentActor, bool toggledOn)
	{
		if (GetMiscVar("DontPerformChosenAbility").GetBool())
		{
			return;
		}
		if (toggledOn)
		{
			if (MiscAbilityData.HasCondition == true)
			{
				if (_chosenAbility.Vars != null && _chosenAbility.Vars.Count > 0)
				{
					_chosenAbility.Start(TargetingActor, TargetingActor);
				}
				CAbilityRemoveConditions.RemoveConditions(TargetingActor, currentActor, _chosenAbility.GetAllNegativeConditions(), _chosenAbility.PositiveConditions.Keys.ToList(), this, AnimOverload);
			}
			else
			{
				List<CAbility> inlineSubAbilities = new List<CAbility> { CAbility.CopyAbility(_chosenAbility, generateNewID: false, fullCopy: false, copyCurrentOverrides: false, applyCurrentStatIsBasedOnX: false, VarsContext) };
				(PhaseManager.CurrentPhase as CPhaseAction).StackInlineSubAbilities(inlineSubAbilities, currentActor, performNow: false, stopPlayerSkipping: false, true, stopPlayerUndo: true, null, ignorePerformNow: true);
			}
		}
		else if (!toggledOn)
		{
			(PhaseManager.CurrentPhase as CPhaseAction).UnstackAbility(_chosenAbility);
		}
	}

	public void AbilityChosen(int abilityIndex)
	{
		if (abilityIndex < ApplicableAbilities.Count && abilityIndex >= 0)
		{
			_chosenAbility = ApplicableAbilities[abilityIndex];
		}
		else
		{
			_chosenAbility = null;
		}
		if (_chosenAbility != null && _chosenAbility is CAbilityAdjustInitiative { ActiveBonusData: var activeBonusData } cAbilityAdjustInitiative && activeBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.SetInitiative) && TargetingActor is CPlayerActor playerActor)
		{
			CAdjustInitiativeActiveBonus_SetInitiative.ApplyInitiative(cAbilityAdjustInitiative.Strength, playerActor);
		}
		if (ChosenAbility != _chosenAbility)
		{
			ChosenAbility = _chosenAbility;
			CChosenAbilityChanged_MessageData cChosenAbilityChanged_MessageData = new CChosenAbilityChanged_MessageData(TargetingActor);
			cChosenAbilityChanged_MessageData.m_ChooseAbility = this;
			ScenarioRuleClient.MessageHandler(cChosenAbilityChanged_MessageData);
		}
	}

	public void ResetChosenAbility(bool fromClick = true)
	{
		if (fromClick && _chosenAbility != null && _chosenAbility is CAbilityAdjustInitiative { ActiveBonusData: var activeBonusData } && activeBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.SetInitiative) && TargetingActor is CPlayerActor playerActor)
		{
			CAdjustInitiativeActiveBonus_SetInitiative.ResetInitiative(playerActor);
		}
		_chosenAbility = null;
		if (fromClick)
		{
			ChosenAbility = null;
		}
		CChosenAbilityChanged_MessageData cChosenAbilityChanged_MessageData = new CChosenAbilityChanged_MessageData(TargetingActor);
		cChosenAbilityChanged_MessageData.m_ChooseAbility = this;
		ScenarioRuleClient.MessageHandler(cChosenAbilityChanged_MessageData);
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbility(new SEventAbility.SEventAbilityBaseData(TargetingActor, subTypeAbility, this)));
	}

	public override bool IsPositive()
	{
		return !GetMiscVar("IsNegative").GetBool();
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (!dontMoveState)
		{
			m_State++;
		}
		return m_State >= TargetingState.Done;
	}

	public override bool ShouldPreviewPotentialDamage()
	{
		if (State != TargetingState.PreProcessingBeforeActorApplies)
		{
			return false;
		}
		return base.ShouldPreviewPotentialDamage();
	}
}
