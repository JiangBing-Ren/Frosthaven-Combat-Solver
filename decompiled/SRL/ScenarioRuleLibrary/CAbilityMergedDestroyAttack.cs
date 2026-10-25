using System.Collections.Generic;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityMergedDestroyAttack : CAbilityMerged
{
	public enum MergedDestroyAttackState
	{
		DestroyAbilitySelectObstaclePositions,
		AttackAbilitySelectAttackFocus,
		PreDestroyObstacles,
		PreActorIsAttacking,
		StartAttackAndDestroyObstacle,
		FinishAttack,
		MergedDone
	}

	[JsonProperty]
	private CAbilityDestroyObstacle m_MergedDestroyObstacleAbility;

	[JsonProperty]
	private CAbilityAttack m_MergedAttackAbility;

	private CAbilityDestroyObstacle m_MergedDestroyObstacleAbilityCopy;

	private CAbilityAttack m_MergedAttackAbilityCopy;

	private MergedDestroyAttackState m_State;

	public CAbilityDestroyObstacle MergedDestroyObstacleAbility => m_MergedDestroyObstacleAbility;

	public CAbilityAttack MergedAttackAbility => m_MergedAttackAbility;

	public CAbilityDestroyObstacle MergedDestroyObstacleAbilityCopy => m_MergedDestroyObstacleAbilityCopy;

	public CAbilityAttack MergedAttackAbilityCopy => m_MergedAttackAbilityCopy;

	public CAbilityMergedDestroyAttack(CAbilityDestroyObstacle destroyObstacleAbility, CAbilityAttack attackAbility)
		: base(destroyObstacleAbility, attackAbility)
	{
		m_MergedDestroyObstacleAbility = destroyObstacleAbility;
		m_MergedAttackAbility = attackAbility;
	}

	public override void CopyMergedAbilities()
	{
		base.CopyMergedAbilities();
		m_MergedDestroyObstacleAbilityCopy = (CAbilityDestroyObstacle)CAbility.CopyAbility(m_MergedDestroyObstacleAbility, generateNewID: false, fullCopy: false, copyCurrentOverrides: false, applyCurrentStatIsBasedOnX: false, VarsContext);
		m_MergedAttackAbilityCopy = (CAbilityAttack)CAbility.CopyAbility(m_MergedAttackAbility, generateNewID: false, fullCopy: false, copyCurrentOverrides: false, applyCurrentStatIsBasedOnX: false, VarsContext);
		m_MergedDestroyObstacleAbilityCopy.ParentAbility = this;
		m_MergedAttackAbilityCopy.ParentAbility = this;
		m_CopiedMergedAbilities.Add(m_MergedDestroyObstacleAbilityCopy);
		m_CopiedMergedAbilities.Add(m_MergedAttackAbilityCopy);
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_State = MergedDestroyAttackState.DestroyAbilitySelectObstaclePositions;
		m_MergedDestroyObstacleAbilityCopy.Start(targetActor, filterActor, controllingActor);
		m_ActiveAbility = m_MergedDestroyObstacleAbilityCopy;
		SetHasStarted(val: true);
	}

	public override bool Perform()
	{
		base.Perform();
		if (GameState.WaitingForMercenarySpecialMechanicSlotChoice)
		{
			return true;
		}
		if (ShowCancelledHelpText())
		{
			return true;
		}
		if (!ProcessIfDead)
		{
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				goto IL_0082;
			}
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
			{
				AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
				if (miscAbilityData == null || miscAbilityData.IgnoreStun != true)
				{
					goto IL_0082;
				}
			}
		}
		switch (m_State)
		{
		case MergedDestroyAttackState.DestroyAbilitySelectObstaclePositions:
			if (m_ActiveAbility != m_MergedDestroyObstacleAbilityCopy)
			{
				m_ActiveAbility = m_MergedDestroyObstacleAbilityCopy;
			}
			if (!m_MergedDestroyObstacleAbilityCopy.HasPassedState(CAbilityDestroyObstacle.DestroyObstacleState.SelectObstaclesPosition) && !m_MergedDestroyObstacleAbilityCopy.AbilityHasBeenCancelled)
			{
				m_MergedDestroyObstacleAbilityCopy.Perform();
				break;
			}
			m_State++;
			Perform();
			break;
		case MergedDestroyAttackState.AttackAbilitySelectAttackFocus:
			if (m_ActiveAbility != m_MergedAttackAbilityCopy)
			{
				m_MergedAttackAbilityCopy.Start(m_StartTargetActor, m_StartFilterActor);
				m_ActiveAbility = m_MergedAttackAbilityCopy;
			}
			if (!m_MergedAttackAbilityCopy.HasPassedState(CAbilityAttack.EAttackState.SelectAttackFocus) && !m_MergedAttackAbilityCopy.AbilityHasBeenCancelled)
			{
				m_MergedAttackAbilityCopy.Perform();
				if (!m_MergedAttackAbilityCopy.AbilityHasBeenCancelled)
				{
					CStartSecondMergedAbility_MessageData message = new CStartSecondMergedAbility_MessageData(TargetingActor)
					{
						m_Ability = this
					};
					ScenarioRuleClient.MessageHandler(message);
				}
			}
			else
			{
				m_CanUndo = false;
				m_State++;
				Perform();
			}
			break;
		case MergedDestroyAttackState.PreDestroyObstacles:
			if (m_ActiveAbility != m_MergedDestroyObstacleAbilityCopy)
			{
				m_ActiveAbility = m_MergedDestroyObstacleAbilityCopy;
			}
			if (!m_MergedDestroyObstacleAbilityCopy.HasPassedState(CAbilityDestroyObstacle.DestroyObstacleState.PreDestroyObstacleProcessing) && !m_MergedDestroyObstacleAbilityCopy.AbilityHasBeenCancelled)
			{
				m_MergedDestroyObstacleAbilityCopy.Perform();
				break;
			}
			m_State++;
			Perform();
			break;
		case MergedDestroyAttackState.PreActorIsAttacking:
			if (m_ActiveAbility != m_MergedAttackAbilityCopy)
			{
				m_ActiveAbility = m_MergedAttackAbilityCopy;
			}
			if (!m_MergedAttackAbilityCopy.HasPassedState(CAbilityAttack.EAttackState.UpdateAttackFocusAfterAttackEffectInlineSubAbility) && !m_MergedAttackAbilityCopy.AbilityHasBeenCancelled)
			{
				m_MergedAttackAbilityCopy.Perform();
				break;
			}
			m_State++;
			Perform();
			break;
		case MergedDestroyAttackState.StartAttackAndDestroyObstacle:
			if (m_MergedAttackAbilityCopy.AbilityHasBeenCancelled)
			{
				m_MergedDestroyObstacleAbilityCopy.AnimOverload = m_MergedAttackAbilityCopy.AnimOverload;
			}
			if (!m_MergedDestroyObstacleAbilityCopy.AbilityHasBeenCancelled)
			{
				m_MergedDestroyObstacleAbilityCopy.Perform();
			}
			if (!m_MergedAttackAbilityCopy.AbilityHasBeenCancelled)
			{
				m_MergedAttackAbilityCopy.Perform();
			}
			break;
		case MergedDestroyAttackState.FinishAttack:
			AbilityHasHappened = true;
			if (m_ActiveAbility != m_MergedAttackAbilityCopy)
			{
				m_ActiveAbility = m_MergedAttackAbilityCopy;
			}
			if (!m_MergedAttackAbilityCopy.HasPassedState(CAbilityAttack.EAttackState.FinaliseAttack) && !m_MergedAttackAbilityCopy.AbilityHasBeenCancelled)
			{
				m_MergedAttackAbilityCopy.Perform();
				break;
			}
			m_State++;
			PhaseManager.StepComplete();
			break;
		}
		return false;
		IL_0082:
		if (TargetingActor.Type == CActor.EType.Player)
		{
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				CPlayerIsSleeping_MessageData message2 = new CPlayerIsSleeping_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message2);
			}
			else
			{
				CPlayerIsStunned_MessageData message3 = new CPlayerIsStunned_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message3);
			}
		}
		else
		{
			PhaseManager.NextStep();
		}
		return true;
	}

	public override void TileSelected(CTile selectedTile, List<CTile> optionalTileList)
	{
		if (m_State == MergedDestroyAttackState.AttackAbilitySelectAttackFocus || m_State == MergedDestroyAttackState.PreActorIsAttacking)
		{
			m_MergedAttackAbilityCopy.TileSelected(selectedTile, optionalTileList);
		}
		else if (m_State == MergedDestroyAttackState.DestroyAbilitySelectObstaclePositions)
		{
			m_MergedDestroyObstacleAbilityCopy.TileSelected(selectedTile, optionalTileList);
		}
	}

	public override void TileDeselected(CTile selectedTile, List<CTile> optionalTileList)
	{
		if (m_State == MergedDestroyAttackState.AttackAbilitySelectAttackFocus || m_State == MergedDestroyAttackState.PreActorIsAttacking)
		{
			m_MergedAttackAbilityCopy.TileDeselected(selectedTile, optionalTileList);
		}
		else if (m_State == MergedDestroyAttackState.DestroyAbilitySelectObstaclePositions)
		{
			m_MergedDestroyObstacleAbilityCopy.TileDeselected(selectedTile, optionalTileList);
		}
	}

	public override bool CanClearTargets()
	{
		if (m_State != MergedDestroyAttackState.AttackAbilitySelectAttackFocus && m_State != MergedDestroyAttackState.PreActorIsAttacking)
		{
			return m_State == MergedDestroyAttackState.DestroyAbilitySelectObstaclePositions;
		}
		return true;
	}

	public override bool CanReceiveTileSelection()
	{
		if (m_State != MergedDestroyAttackState.AttackAbilitySelectAttackFocus && m_State != MergedDestroyAttackState.DestroyAbilitySelectObstaclePositions)
		{
			return m_State == MergedDestroyAttackState.PreActorIsAttacking;
		}
		return true;
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		if (m_State == MergedDestroyAttackState.StartAttackAndDestroyObstacle)
		{
			m_MergedAttackAbilityCopy.AbilityComplete(dontMoveState, out fullAbilityRestart);
			m_MergedDestroyObstacleAbilityCopy.AbilityComplete(dontMoveState, out fullAbilityRestart);
			m_State++;
		}
		else
		{
			ActiveAbility.AbilityComplete(dontMoveState, out fullAbilityRestart);
		}
		return m_State == MergedDestroyAttackState.MergedDone;
	}

	public override void AbilityEnded()
	{
		base.AbilityEnded();
		if (m_MergedAttackAbilityCopy.TargetingActor != null)
		{
			m_MergedAttackAbilityCopy.AbilityEnded();
		}
		if (m_MergedDestroyObstacleAbilityCopy.TargetingActor != null)
		{
			m_MergedDestroyObstacleAbilityCopy.AbilityEnded();
		}
	}

	public override string GetDescription()
	{
		return "MeredCreateAttack R: " + Range + " N: " + NumberTargets;
	}

	public override CAbility GetMergedWithAbility(CAbility ability)
	{
		if (!m_MergedDestroyObstacleAbilityCopy.Equals(ability))
		{
			return m_MergedDestroyObstacleAbilityCopy;
		}
		return m_MergedAttackAbilityCopy;
	}
}
