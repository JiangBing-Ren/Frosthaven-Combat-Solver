using System.Collections.Generic;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

internal class CAbilityMergedMoveObstacleAttack : CAbilityMerged
{
	public enum MergedCreateAttackState
	{
		MoveAbilitySelectMoveTiles,
		AttackAbilitySelectAttackFocus,
		PreMovingObstacles,
		PreActorIsAttacking,
		RemoveObstacle,
		StartAttackAndCreateObstacle,
		FinishAttack,
		FinishMove,
		MergedDone
	}

	[JsonProperty]
	private CAbilityMoveObstacle m_MergedMoveObstacleAbility;

	[JsonProperty]
	private CAbilityAttack m_MergedAttackAbility;

	private CAbilityMoveObstacle m_MergedMoveObstacleAbilityCopy;

	private CAbilityAttack m_MergedAttackAbilityCopy;

	private MergedCreateAttackState m_State;

	public CAbilityMoveObstacle MergedMoveObstacleAbility => m_MergedMoveObstacleAbility;

	public CAbilityAttack MergedAttackAbility => m_MergedAttackAbility;

	public CAbilityMoveObstacle MergedMoveObstacleAbilityCopy => m_MergedMoveObstacleAbilityCopy;

	public CAbilityAttack MergedAttackAbilityCopy => m_MergedAttackAbilityCopy;

	public CAbilityMergedMoveObstacleAttack(CAbilityMoveObstacle moveObstacleAbility, CAbilityAttack attackAbility)
		: base(moveObstacleAbility, attackAbility)
	{
		m_MergedMoveObstacleAbility = moveObstacleAbility;
		m_MergedAttackAbility = attackAbility;
	}

	public override void CopyMergedAbilities()
	{
		base.CopyMergedAbilities();
		m_MergedMoveObstacleAbilityCopy = (CAbilityMoveObstacle)CAbility.CopyAbility(m_MergedMoveObstacleAbility, generateNewID: false, fullCopy: false, copyCurrentOverrides: false, applyCurrentStatIsBasedOnX: false, VarsContext);
		m_MergedAttackAbilityCopy = (CAbilityAttack)CAbility.CopyAbility(m_MergedAttackAbility, generateNewID: false, fullCopy: false, copyCurrentOverrides: false, applyCurrentStatIsBasedOnX: false, VarsContext);
		m_MergedMoveObstacleAbilityCopy.ParentAbility = this;
		m_MergedAttackAbilityCopy.ParentAbility = this;
		m_CopiedMergedAbilities.Add(m_MergedMoveObstacleAbilityCopy);
		m_CopiedMergedAbilities.Add(m_MergedAttackAbilityCopy);
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_State = MergedCreateAttackState.MoveAbilitySelectMoveTiles;
		m_MergedMoveObstacleAbilityCopy.Start(targetActor, filterActor, controllingActor);
		m_ActiveAbility = m_MergedMoveObstacleAbilityCopy;
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
		case MergedCreateAttackState.MoveAbilitySelectMoveTiles:
			if (m_ActiveAbility != m_MergedMoveObstacleAbilityCopy)
			{
				m_ActiveAbility = m_MergedMoveObstacleAbilityCopy;
			}
			if (!m_MergedMoveObstacleAbilityCopy.HasPassedState(CAbilityMoveObstacle.MoveObstacleState.SelectMoveToTile) && !m_MergedMoveObstacleAbilityCopy.AbilityHasBeenCancelled)
			{
				m_MergedMoveObstacleAbilityCopy.Perform();
				break;
			}
			m_State++;
			Perform();
			break;
		case MergedCreateAttackState.AttackAbilitySelectAttackFocus:
			if (m_ActiveAbility != m_MergedAttackAbilityCopy)
			{
				m_MergedAttackAbilityCopy.Start(m_StartTargetActor, m_StartFilterActor);
				m_ActiveAbility = m_MergedAttackAbilityCopy;
				if (!m_MergedAttackAbilityCopy.AbilityHasBeenCancelled)
				{
					CStartSecondMergedAbility_MessageData message = new CStartSecondMergedAbility_MessageData(TargetingActor)
					{
						m_Ability = this
					};
					ScenarioRuleClient.MessageHandler(message);
				}
			}
			if (!m_MergedAttackAbilityCopy.HasPassedState(CAbilityAttack.EAttackState.SelectAttackFocus) && !m_MergedAttackAbilityCopy.AbilityHasBeenCancelled)
			{
				m_MergedAttackAbilityCopy.Perform();
				break;
			}
			m_CanUndo = false;
			m_State++;
			Perform();
			break;
		case MergedCreateAttackState.PreMovingObstacles:
			if (m_ActiveAbility != m_MergedMoveObstacleAbilityCopy)
			{
				m_ActiveAbility = m_MergedMoveObstacleAbilityCopy;
			}
			if (!m_MergedMoveObstacleAbilityCopy.HasPassedState(CAbilityMoveObstacle.MoveObstacleState.PreMovingObstacles) && !m_MergedMoveObstacleAbilityCopy.AbilityHasBeenCancelled)
			{
				m_MergedMoveObstacleAbilityCopy.Perform();
				break;
			}
			m_State++;
			Perform();
			break;
		case MergedCreateAttackState.PreActorIsAttacking:
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
		case MergedCreateAttackState.RemoveObstacle:
			m_CanUndo = false;
			if (m_ActiveAbility != m_MergedMoveObstacleAbilityCopy)
			{
				m_ActiveAbility = m_MergedMoveObstacleAbilityCopy;
			}
			if (!m_MergedMoveObstacleAbilityCopy.HasPassedState(CAbilityMoveObstacle.MoveObstacleState.RemoveObstacle) && !m_MergedMoveObstacleAbilityCopy.AbilityHasBeenCancelled)
			{
				m_MergedMoveObstacleAbilityCopy.Perform();
				break;
			}
			m_State++;
			Perform();
			break;
		case MergedCreateAttackState.StartAttackAndCreateObstacle:
			if (!m_MergedMoveObstacleAbilityCopy.AbilityHasBeenCancelled)
			{
				m_MergedMoveObstacleAbilityCopy.Perform();
			}
			if (!m_MergedAttackAbilityCopy.AbilityHasBeenCancelled)
			{
				m_MergedAttackAbilityCopy.Perform();
			}
			break;
		case MergedCreateAttackState.FinishAttack:
		{
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
			m_MergedMoveObstacleAbilityCopy.AbilityComplete(dontMoveState: false, out var _);
			Perform();
			break;
		}
		case MergedCreateAttackState.FinishMove:
			if (m_ActiveAbility != m_MergedMoveObstacleAbilityCopy)
			{
				m_ActiveAbility = m_MergedMoveObstacleAbilityCopy;
			}
			if (!m_MergedMoveObstacleAbilityCopy.HasPassedState(CAbilityMoveObstacle.MoveObstacleState.MovedObstacle) && !m_MergedMoveObstacleAbilityCopy.AbilityHasBeenCancelled)
			{
				m_MergedMoveObstacleAbilityCopy.Perform();
			}
			else
			{
				PhaseManager.NextStep();
			}
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
		if (m_State == MergedCreateAttackState.AttackAbilitySelectAttackFocus)
		{
			m_MergedAttackAbilityCopy.TileSelected(selectedTile, optionalTileList);
		}
		else if (m_State == MergedCreateAttackState.MoveAbilitySelectMoveTiles)
		{
			m_MergedMoveObstacleAbilityCopy.TileSelected(selectedTile, optionalTileList);
		}
	}

	public override void TileDeselected(CTile selectedTile, List<CTile> optionalTileList)
	{
		if (m_State == MergedCreateAttackState.AttackAbilitySelectAttackFocus)
		{
			m_MergedAttackAbilityCopy.TileDeselected(selectedTile, optionalTileList);
		}
		else if (m_State == MergedCreateAttackState.MoveAbilitySelectMoveTiles)
		{
			m_MergedMoveObstacleAbilityCopy.TileDeselected(selectedTile, optionalTileList);
		}
	}

	public override bool CanClearTargets()
	{
		if (m_State != MergedCreateAttackState.AttackAbilitySelectAttackFocus)
		{
			return m_State == MergedCreateAttackState.MoveAbilitySelectMoveTiles;
		}
		return true;
	}

	public override bool CanReceiveTileSelection()
	{
		if (m_State != MergedCreateAttackState.AttackAbilitySelectAttackFocus)
		{
			return m_State == MergedCreateAttackState.MoveAbilitySelectMoveTiles;
		}
		return true;
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		if (m_State == MergedCreateAttackState.StartAttackAndCreateObstacle)
		{
			m_MergedAttackAbilityCopy.AbilityComplete(dontMoveState, out fullAbilityRestart);
			m_MergedMoveObstacleAbilityCopy.AbilityComplete(dontMoveState, out fullAbilityRestart);
			m_State++;
		}
		else
		{
			ActiveAbility.AbilityComplete(dontMoveState, out fullAbilityRestart);
		}
		return m_State == MergedCreateAttackState.MergedDone;
	}

	public override void AbilityEnded()
	{
		base.AbilityEnded();
		if (m_MergedAttackAbilityCopy.TargetingActor != null)
		{
			m_MergedAttackAbilityCopy.AbilityEnded();
		}
		if (m_MergedMoveObstacleAbilityCopy.TargetingActor != null)
		{
			m_MergedMoveObstacleAbilityCopy.AbilityEnded();
		}
	}

	public override string GetDescription()
	{
		return "MeredCreateAttack R: " + Range + " N: " + NumberTargets;
	}

	public override CAbility GetMergedWithAbility(CAbility ability)
	{
		if (!m_MergedMoveObstacleAbilityCopy.Equals(ability))
		{
			return m_MergedMoveObstacleAbilityCopy;
		}
		return m_MergedAttackAbilityCopy;
	}
}
