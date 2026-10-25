using System.Collections.Generic;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityMergedMoveAttack : CAbilityMerged
{
	public enum MergedMoveAttackState
	{
		MoveAbilitySelectingMoveTile,
		AttackAbilitySelectAttackFocus,
		PreActorIsAttackingStates,
		StartAttackAndMove,
		ProcessingMove,
		FinishAttack,
		CheckForRemainingMove,
		MergedDone
	}

	[JsonProperty]
	private CAbilityMove m_MergedMoveAbility;

	[JsonProperty]
	private CAbilityAttack m_MergedAttackAbility;

	private CAbilityMove m_MergedMoveAbilityCopy;

	private CAbilityAttack m_MergedAttackAbilityCopy;

	private MergedMoveAttackState m_State;

	private bool m_StartedAttackAbility;

	public CAbilityMove MergedMoveAbility => m_MergedMoveAbility;

	public CAbilityAttack MergedAttackAbility => m_MergedAttackAbility;

	public CAbilityMove MergedMoveAbilityCopy => m_MergedMoveAbilityCopy;

	public CAbilityAttack MergedAttackAbilityCopy => m_MergedAttackAbilityCopy;

	public CAbilityMergedMoveAttack(CAbilityMove moveAbility, CAbilityAttack attackAbility)
		: base(moveAbility, attackAbility)
	{
		m_MergedMoveAbility = moveAbility;
		m_MergedAttackAbility = attackAbility;
	}

	public override void CopyMergedAbilities()
	{
		base.CopyMergedAbilities();
		m_MergedMoveAbilityCopy = (CAbilityMove)CAbility.CopyAbility(m_MergedMoveAbility, generateNewID: false, fullCopy: false, copyCurrentOverrides: false, applyCurrentStatIsBasedOnX: false, VarsContext);
		m_MergedAttackAbilityCopy = (CAbilityAttack)CAbility.CopyAbility(m_MergedAttackAbility, generateNewID: false, fullCopy: false, copyCurrentOverrides: false, applyCurrentStatIsBasedOnX: false, VarsContext);
		m_MergedMoveAbilityCopy.ParentAbility = this;
		m_MergedAttackAbilityCopy.ParentAbility = this;
		m_CopiedMergedAbilities.Add(m_MergedMoveAbilityCopy);
		m_CopiedMergedAbilities.Add(m_MergedAttackAbilityCopy);
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_StartedAttackAbility = false;
		m_State = MergedMoveAttackState.MoveAbilitySelectingMoveTile;
		m_MergedMoveAbilityCopy.Start(targetActor, filterActor, controllingActor);
		m_ActiveAbility = m_MergedMoveAbilityCopy;
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
		case MergedMoveAttackState.MoveAbilitySelectingMoveTile:
			if (m_ActiveAbility != m_MergedMoveAbilityCopy)
			{
				m_ActiveAbility = m_MergedMoveAbilityCopy;
			}
			if (!m_MergedMoveAbilityCopy.HasPassedState(CAbilityMove.EMoveState.ActorIsSelectingMoveTile) && !m_MergedMoveAbilityCopy.AbilityHasBeenCancelled)
			{
				CClearWaypointsAndTargets_MessageData message = new CClearWaypointsAndTargets_MessageData();
				ScenarioRuleClient.MessageHandler(message);
				m_MergedMoveAbilityCopy.Perform();
			}
			break;
		case MergedMoveAttackState.AttackAbilitySelectAttackFocus:
			if (!m_MergedAttackAbilityCopy.HasPassedState(CAbilityAttack.EAttackState.SelectAttackFocus) && !m_MergedAttackAbilityCopy.AbilityHasBeenCancelled)
			{
				m_MergedAttackAbilityCopy.Perform();
				break;
			}
			m_State++;
			Perform();
			break;
		case MergedMoveAttackState.PreActorIsAttackingStates:
			if (!m_MergedAttackAbilityCopy.HasPassedState(CAbilityAttack.EAttackState.CheckForDamageSelf) && !m_MergedAttackAbilityCopy.AbilityHasBeenCancelled)
			{
				m_MergedAttackAbilityCopy.Perform();
				break;
			}
			m_State++;
			Perform();
			break;
		case MergedMoveAttackState.StartAttackAndMove:
			if (m_ActiveAbility != m_MergedAttackAbilityCopy)
			{
				m_ActiveAbility = m_MergedAttackAbilityCopy;
			}
			m_State++;
			m_MergedMoveAbilityCopy.Perform();
			m_MergedAttackAbilityCopy.Perform();
			break;
		case MergedMoveAttackState.ProcessingMove:
			AbilityHasHappened = true;
			if (m_ActiveAbility != m_MergedMoveAbilityCopy)
			{
				m_ActiveAbility = m_MergedMoveAbilityCopy;
			}
			if (!m_MergedMoveAbilityCopy.HasPassedState(CAbilityMove.EMoveState.ActorHasMoved) && !m_MergedMoveAbilityCopy.AbilityHasBeenCancelled)
			{
				m_MergedMoveAbilityCopy.Perform();
				break;
			}
			m_State++;
			Perform();
			break;
		case MergedMoveAttackState.FinishAttack:
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
			Perform();
			break;
		case MergedMoveAttackState.CheckForRemainingMove:
			if (m_MergedMoveAbilityCopy.RemainingMovesAfterLimit > 0)
			{
				m_State = MergedMoveAttackState.MoveAbilitySelectingMoveTile;
				ScenarioRuleClient.MessageHandler(new CUnlockWaypointsForClearing(TargetingActor));
				m_MergedMoveAbilityCopy.Perform();
				Perform();
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
		if (m_State == MergedMoveAttackState.MoveAbilitySelectingMoveTile && !TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Immobilize) && !TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun) && !TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
		{
			m_MergedMoveAbilityCopy.TileSelected(selectedTile, optionalTileList);
			ScenarioRuleClient.MessageHandler(new CLockWaypointsFromClearing(TargetingActor));
			m_State++;
			if (m_ActiveAbility != m_MergedAttackAbilityCopy && !m_StartedAttackAbility)
			{
				m_MergedAttackAbilityCopy.Start(m_StartTargetActor, m_StartFilterActor);
				m_MergedAttackAbilityCopy.SetCanSkip(canSkip: false);
				m_ActiveAbility = m_MergedAttackAbilityCopy;
				if (!m_MergedAttackAbilityCopy.AbilityHasBeenCancelled)
				{
					CStartSecondMergedAbility_MessageData message = new CStartSecondMergedAbility_MessageData(TargetingActor)
					{
						m_Ability = this
					};
					ScenarioRuleClient.MessageHandler(message);
				}
				m_StartedAttackAbility = true;
			}
			else
			{
				m_MergedAttackAbilityCopy.Restart();
				m_ActiveAbility = m_MergedAttackAbilityCopy;
			}
			Perform();
		}
		else if (m_State == MergedMoveAttackState.AttackAbilitySelectAttackFocus)
		{
			m_MergedAttackAbilityCopy.TileSelected(selectedTile, optionalTileList);
		}
		else if (m_State == MergedMoveAttackState.ProcessingMove)
		{
			m_MergedMoveAbilityCopy.TileSelected(selectedTile, optionalTileList);
			Perform();
		}
		else
		{
			base.TileSelected(selectedTile, optionalTileList);
		}
	}

	public override void TileDeselected(CTile selectedTile, List<CTile> optionalTileList)
	{
		if (m_State == MergedMoveAttackState.MoveAbilitySelectingMoveTile)
		{
			m_MergedMoveAbilityCopy.TileDeselected(selectedTile, optionalTileList);
		}
		else if (m_State == MergedMoveAttackState.AttackAbilitySelectAttackFocus)
		{
			m_MergedAttackAbilityCopy.TileDeselected(selectedTile, optionalTileList);
		}
	}

	public override bool CanClearTargets()
	{
		return m_State == MergedMoveAttackState.MoveAbilitySelectingMoveTile;
	}

	public override bool CanReceiveTileSelection()
	{
		if ((m_State != MergedMoveAttackState.MoveAbilitySelectingMoveTile || TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Immobilize) || TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun) || TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep)) && m_State != MergedMoveAttackState.AttackAbilitySelectAttackFocus)
		{
			return m_State == MergedMoveAttackState.ProcessingMove;
		}
		return true;
	}

	public override bool RequiresWaypointSelection()
	{
		return m_State == MergedMoveAttackState.MoveAbilitySelectingMoveTile;
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		ActiveAbility.AbilityComplete(dontMoveState, out fullAbilityRestart);
		return m_State == MergedMoveAttackState.MergedDone;
	}

	public override void AbilityEnded()
	{
		base.AbilityEnded();
		if (m_MergedAttackAbilityCopy.TargetingActor != null)
		{
			m_MergedAttackAbilityCopy.AbilityEnded();
		}
		if (m_MergedMoveAbilityCopy.TargetingActor != null)
		{
			m_MergedMoveAbilityCopy.AbilityEnded();
		}
	}

	public override void Restart()
	{
		ScenarioRuleClient.MessageHandler(new CRestoreWaypoints(TargetingActor));
		m_ActiveAbility.Restart();
	}

	public override void InterruptAbility()
	{
		ScenarioRuleClient.MessageHandler(new CCacheWaypoints(TargetingActor));
	}

	public override string GetDescription()
	{
		return "MeredCreateAttack R: " + Range + " N: " + NumberTargets;
	}

	public override CAbility GetMergedWithAbility(CAbility ability)
	{
		if (!m_MergedMoveAbilityCopy.Equals(ability))
		{
			return m_MergedMoveAbilityCopy;
		}
		return m_MergedAttackAbilityCopy;
	}
}
