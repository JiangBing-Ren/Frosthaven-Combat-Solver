using System.Collections.Generic;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityMergedDisarmTrapDestroyObstacle : CAbilityMerged
{
	public enum MergedDisarmTrapDestroyObstacleState
	{
		DestroyObstacleAbilitySelectObstacles,
		DisarmAbilitySelectTraps,
		PreDestroyObstacles,
		DisarmAndDestroy,
		FinishDisarm,
		MergedDone
	}

	[JsonProperty]
	private CAbilityDisarmTrap m_MergedDisarmTrapAbility;

	[JsonProperty]
	private CAbilityDestroyObstacle m_MergedDestroyObstacleAbility;

	private CAbilityDisarmTrap m_MergedDisarmTrapAbilityCopy;

	private CAbilityDestroyObstacle m_MergedDestroyObstacleAbilityCopy;

	private MergedDisarmTrapDestroyObstacleState m_State;

	public CAbilityDisarmTrap MergedDisarmTrapAbility => m_MergedDisarmTrapAbility;

	public CAbilityDestroyObstacle MergedDestroyObstacleAbility => m_MergedDestroyObstacleAbility;

	public CAbilityDisarmTrap MergedDisarmTrapAbilityCopy => m_MergedDisarmTrapAbilityCopy;

	public CAbilityDestroyObstacle MergedDestroyObstacleAbilityCopy => m_MergedDestroyObstacleAbilityCopy;

	public CAbilityMergedDisarmTrapDestroyObstacle(CAbilityDisarmTrap disarmAbility, CAbilityDestroyObstacle destroyObstacleAbility)
		: base(disarmAbility, destroyObstacleAbility)
	{
		m_MergedDisarmTrapAbility = disarmAbility;
		m_MergedDestroyObstacleAbility = destroyObstacleAbility;
	}

	public override void CopyMergedAbilities()
	{
		base.CopyMergedAbilities();
		m_MergedDisarmTrapAbilityCopy = (CAbilityDisarmTrap)CAbility.CopyAbility(m_MergedDisarmTrapAbility, generateNewID: false, fullCopy: false, copyCurrentOverrides: false, applyCurrentStatIsBasedOnX: false, VarsContext);
		m_MergedDestroyObstacleAbilityCopy = (CAbilityDestroyObstacle)CAbility.CopyAbility(m_MergedDestroyObstacleAbility, generateNewID: false, fullCopy: false, copyCurrentOverrides: false, applyCurrentStatIsBasedOnX: false, VarsContext);
		m_MergedDisarmTrapAbilityCopy.ParentAbility = this;
		m_MergedDestroyObstacleAbilityCopy.ParentAbility = this;
		m_CopiedMergedAbilities.Add(m_MergedDisarmTrapAbilityCopy);
		m_CopiedMergedAbilities.Add(m_MergedDestroyObstacleAbilityCopy);
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_State = MergedDisarmTrapDestroyObstacleState.DestroyObstacleAbilitySelectObstacles;
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
		if (m_MergedDestroyObstacleAbilityCopy.AbilityHasBeenCancelled && m_MergedDisarmTrapAbilityCopy.AbilityHasBeenCancelled)
		{
			PhaseManager.NextStep();
			return true;
		}
		switch (m_State)
		{
		case MergedDisarmTrapDestroyObstacleState.DestroyObstacleAbilitySelectObstacles:
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
		case MergedDisarmTrapDestroyObstacleState.DisarmAbilitySelectTraps:
			if (m_ActiveAbility != m_MergedDisarmTrapAbilityCopy)
			{
				m_MergedDisarmTrapAbilityCopy.Start(m_StartTargetActor, m_StartFilterActor);
				m_ActiveAbility = m_MergedDisarmTrapAbilityCopy;
				if (!m_MergedDisarmTrapAbilityCopy.AbilityHasBeenCancelled)
				{
					CStartSecondMergedAbility_MessageData message = new CStartSecondMergedAbility_MessageData(TargetingActor)
					{
						m_Ability = this
					};
					ScenarioRuleClient.MessageHandler(message);
				}
				ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			}
			if (!m_MergedDisarmTrapAbilityCopy.HasPassedState(CAbilityDisarmTrap.DisarmTrapState.SelectTrapPosition) && !m_MergedDisarmTrapAbilityCopy.AbilityHasBeenCancelled)
			{
				m_MergedDisarmTrapAbilityCopy.Perform();
				break;
			}
			m_State++;
			Perform();
			break;
		case MergedDisarmTrapDestroyObstacleState.PreDestroyObstacles:
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
		case MergedDisarmTrapDestroyObstacleState.DisarmAndDestroy:
			if (!m_MergedDestroyObstacleAbilityCopy.AbilityHasBeenCancelled)
			{
				m_MergedDestroyObstacleAbilityCopy.Perform();
			}
			if (!m_MergedDisarmTrapAbilityCopy.AbilityHasBeenCancelled)
			{
				m_MergedDisarmTrapAbilityCopy.Perform();
			}
			break;
		case MergedDisarmTrapDestroyObstacleState.FinishDisarm:
			AbilityHasHappened = true;
			if (m_ActiveAbility != m_MergedDisarmTrapAbilityCopy)
			{
				m_ActiveAbility = m_MergedDisarmTrapAbilityCopy;
			}
			if (!m_MergedDisarmTrapAbilityCopy.HasPassedState(CAbilityDisarmTrap.DisarmTrapState.AffectingActorWithTrapEffects) && !m_MergedDisarmTrapAbilityCopy.AbilityHasBeenCancelled)
			{
				m_MergedDisarmTrapAbilityCopy.Perform();
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
		if (m_State == MergedDisarmTrapDestroyObstacleState.DestroyObstacleAbilitySelectObstacles)
		{
			m_MergedDestroyObstacleAbilityCopy.TileSelected(selectedTile, optionalTileList);
		}
		else if (m_State == MergedDisarmTrapDestroyObstacleState.DisarmAbilitySelectTraps)
		{
			m_MergedDisarmTrapAbilityCopy.TileSelected(selectedTile, optionalTileList);
		}
	}

	public override void TileDeselected(CTile selectedTile, List<CTile> optionalTileList)
	{
		if (m_State == MergedDisarmTrapDestroyObstacleState.DestroyObstacleAbilitySelectObstacles)
		{
			m_MergedDestroyObstacleAbilityCopy.TileDeselected(selectedTile, optionalTileList);
		}
		else if (m_State == MergedDisarmTrapDestroyObstacleState.DisarmAbilitySelectTraps)
		{
			m_MergedDisarmTrapAbilityCopy.TileDeselected(selectedTile, optionalTileList);
		}
	}

	public override bool CanClearTargets()
	{
		if (m_State != MergedDisarmTrapDestroyObstacleState.DestroyObstacleAbilitySelectObstacles)
		{
			return m_State == MergedDisarmTrapDestroyObstacleState.DisarmAbilitySelectTraps;
		}
		return true;
	}

	public override bool CanReceiveTileSelection()
	{
		if (m_State != MergedDisarmTrapDestroyObstacleState.DestroyObstacleAbilitySelectObstacles)
		{
			return m_State == MergedDisarmTrapDestroyObstacleState.DisarmAbilitySelectTraps;
		}
		return true;
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		if (m_State == MergedDisarmTrapDestroyObstacleState.DisarmAndDestroy)
		{
			m_MergedDisarmTrapAbilityCopy.AbilityComplete(dontMoveState, out fullAbilityRestart);
			m_MergedDestroyObstacleAbilityCopy.AbilityComplete(dontMoveState, out fullAbilityRestart);
			m_State++;
		}
		else
		{
			ActiveAbility.AbilityComplete(dontMoveState, out fullAbilityRestart);
		}
		return m_State == MergedDisarmTrapDestroyObstacleState.MergedDone;
	}

	public override void AbilityEnded()
	{
		base.AbilityEnded();
		if (m_MergedDestroyObstacleAbilityCopy.TargetingActor != null)
		{
			m_MergedDestroyObstacleAbilityCopy.AbilityEnded();
		}
		if (m_MergedDisarmTrapAbilityCopy.TargetingActor != null)
		{
			m_MergedDisarmTrapAbilityCopy.AbilityEnded();
		}
	}

	public override string GetDescription()
	{
		return "MeredCreateAttack R: " + Range + " N: " + NumberTargets;
	}

	public override CAbility GetMergedWithAbility(CAbility ability)
	{
		if (!m_MergedDisarmTrapAbilityCopy.Equals(ability))
		{
			return m_MergedDisarmTrapAbilityCopy;
		}
		return m_MergedDestroyObstacleAbilityCopy;
	}
}
