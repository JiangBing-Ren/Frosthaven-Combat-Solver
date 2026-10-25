using System.Collections.Generic;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityMergedKillCreate : CAbilityMerged
{
	public enum MergedKillCreateState
	{
		KillAbilitySelectAttackFocus,
		CreateAbilitySelectObstaclePositions,
		StartKillAndCreateObstacle,
		FinishKill,
		MergedDone
	}

	[JsonProperty]
	private CAbilityKill m_MergedKillAbility;

	[JsonProperty]
	private CAbilityCreate m_MergedCreateAbility;

	private CAbilityKill m_MergedKillAbilityCopy;

	private CAbilityCreate m_MergedCreateAbilityCopy;

	private MergedKillCreateState m_State;

	public CAbilityKill MergedKillAbility => m_MergedKillAbility;

	public CAbilityCreate MergedCreateAbility => m_MergedCreateAbility;

	public CAbilityKill MergedKillAbilityCopy => m_MergedKillAbilityCopy;

	public CAbilityCreate MergedCreateAbilityCopy => m_MergedCreateAbilityCopy;

	public CAbilityMergedKillCreate(CAbilityKill killAbility, CAbilityCreate createAbility)
		: base(killAbility, createAbility)
	{
		m_MergedCreateAbility = createAbility;
		m_MergedKillAbility = killAbility;
	}

	public override void CopyMergedAbilities()
	{
		base.CopyMergedAbilities();
		m_MergedKillAbilityCopy = (CAbilityKill)CAbility.CopyAbility(m_MergedKillAbility, generateNewID: false, fullCopy: false, copyCurrentOverrides: false, applyCurrentStatIsBasedOnX: false, VarsContext);
		m_MergedCreateAbilityCopy = (CAbilityCreate)CAbility.CopyAbility(m_MergedCreateAbility, generateNewID: false, fullCopy: false, copyCurrentOverrides: false, applyCurrentStatIsBasedOnX: false, VarsContext);
		m_MergedKillAbilityCopy.ParentAbility = this;
		m_MergedCreateAbilityCopy.ParentAbility = this;
		m_CopiedMergedAbilities.Add(m_MergedKillAbilityCopy);
		m_CopiedMergedAbilities.Add(m_MergedCreateAbilityCopy);
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_State = MergedKillCreateState.KillAbilitySelectAttackFocus;
		m_MergedKillAbilityCopy.Start(targetActor, filterActor, controllingActor);
		m_ActiveAbility = m_MergedKillAbilityCopy;
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
		if (m_MergedKillAbilityCopy.AbilityHasBeenCancelled)
		{
			PhaseManager.NextStep();
			return true;
		}
		switch (m_State)
		{
		case MergedKillCreateState.KillAbilitySelectAttackFocus:
			if (m_ActiveAbility != m_MergedKillAbilityCopy)
			{
				m_ActiveAbility = m_MergedKillAbilityCopy;
			}
			if (!m_MergedKillAbilityCopy.HasPassedState(CAbilityTargeting.TargetingState.ActorIsSelectingTargetingFocus) && !m_MergedKillAbilityCopy.AbilityHasBeenCancelled)
			{
				m_MergedKillAbilityCopy.Perform();
				break;
			}
			m_State++;
			Perform();
			break;
		case MergedKillCreateState.CreateAbilitySelectObstaclePositions:
			if (m_ActiveAbility != m_MergedCreateAbilityCopy)
			{
				m_MergedCreateAbilityCopy.Start(m_StartTargetActor, m_StartFilterActor);
				m_ActiveAbility = m_MergedCreateAbilityCopy;
				if (!m_MergedCreateAbilityCopy.AbilityHasBeenCancelled)
				{
					CStartSecondMergedAbility_MessageData message = new CStartSecondMergedAbility_MessageData(TargetingActor)
					{
						m_Ability = this
					};
					ScenarioRuleClient.MessageHandler(message);
				}
			}
			if (!m_MergedCreateAbilityCopy.HasPassedState(CAbilityCreate.CreateState.SelectObstaclePositions) && !m_MergedCreateAbilityCopy.AbilityHasBeenCancelled)
			{
				m_MergedCreateAbilityCopy.Perform();
				break;
			}
			m_State++;
			Perform();
			break;
		case MergedKillCreateState.StartKillAndCreateObstacle:
			if (m_ActiveAbility != m_MergedKillAbilityCopy)
			{
				m_ActiveAbility = m_MergedKillAbilityCopy;
			}
			m_State++;
			m_MergedKillAbilityCopy.Perform();
			m_MergedCreateAbilityCopy.Perform();
			break;
		case MergedKillCreateState.FinishKill:
			AbilityHasHappened = true;
			if (m_ActiveAbility != m_MergedKillAbilityCopy)
			{
				m_ActiveAbility = m_MergedKillAbilityCopy;
			}
			if (!m_MergedKillAbilityCopy.HasPassedState(CAbilityTargeting.TargetingState.ApplyToActors) && !m_MergedKillAbilityCopy.AbilityHasBeenCancelled)
			{
				m_MergedKillAbilityCopy.Perform();
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
		if (m_State == MergedKillCreateState.KillAbilitySelectAttackFocus)
		{
			m_MergedKillAbilityCopy.TileSelected(selectedTile, optionalTileList);
		}
		else if (m_State == MergedKillCreateState.CreateAbilitySelectObstaclePositions)
		{
			m_MergedCreateAbilityCopy.TileSelected(selectedTile, optionalTileList);
		}
	}

	public override void TileDeselected(CTile selectedTile, List<CTile> optionalTileList)
	{
		if (m_State == MergedKillCreateState.KillAbilitySelectAttackFocus)
		{
			m_MergedKillAbilityCopy.TileDeselected(selectedTile, optionalTileList);
		}
		else if (m_State == MergedKillCreateState.CreateAbilitySelectObstaclePositions)
		{
			m_MergedCreateAbilityCopy.TileDeselected(selectedTile, optionalTileList);
		}
	}

	public override bool CanClearTargets()
	{
		if (m_State != MergedKillCreateState.KillAbilitySelectAttackFocus)
		{
			return m_State == MergedKillCreateState.CreateAbilitySelectObstaclePositions;
		}
		return true;
	}

	public override bool CanReceiveTileSelection()
	{
		if (m_State != MergedKillCreateState.KillAbilitySelectAttackFocus)
		{
			return m_State == MergedKillCreateState.CreateAbilitySelectObstaclePositions;
		}
		return true;
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		ActiveAbility.AbilityComplete(dontMoveState, out fullAbilityRestart);
		return m_State == MergedKillCreateState.MergedDone;
	}

	public override void AbilityEnded()
	{
		base.AbilityEnded();
		if (m_MergedCreateAbilityCopy.TargetingActor != null)
		{
			m_MergedCreateAbilityCopy.AbilityEnded();
		}
		if (m_MergedKillAbilityCopy.TargetingActor != null)
		{
			m_MergedKillAbilityCopy.AbilityEnded();
		}
	}

	public override string GetDescription()
	{
		return "MergedKillCreate R: " + Range + " N: " + NumberTargets;
	}

	public override CAbility GetMergedWithAbility(CAbility ability)
	{
		if (!m_MergedCreateAbilityCopy.Equals(ability))
		{
			return m_MergedCreateAbilityCopy;
		}
		return m_MergedKillAbilityCopy;
	}
}
