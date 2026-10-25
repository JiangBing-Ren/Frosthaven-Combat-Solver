using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityRedistributeDamage : CAbility
{
	public enum ERedistributeState
	{
		None,
		ShowRedistributeUI,
		ActorIsRedistributing,
		ApplyRedistributions,
		RedistributeDone
	}

	private ERedistributeState m_State;

	private List<Tuple<CActor, int>> m_HealthChanges;

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_State = ERedistributeState.ShowRedistributeUI;
		LogEvent(ESESubTypeAbility.AbilityStart);
		SetHasStarted(val: true);
	}

	public override bool Perform()
	{
		base.Perform();
		if (GameState.WaitingForMercenarySpecialMechanicSlotChoice)
		{
			return true;
		}
		LogEvent(ESESubTypeAbility.AbilityPerform);
		if (ShowCancelledHelpText())
		{
			return true;
		}
		if (!ProcessIfDead)
		{
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				goto IL_0089;
			}
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
			{
				AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
				if (miscAbilityData == null || miscAbilityData.IgnoreStun != true)
				{
					goto IL_0089;
				}
			}
		}
		switch (m_State)
		{
		case ERedistributeState.ShowRedistributeUI:
		{
			m_ActorsToTarget.Add(TargetingActor);
			m_ActorsToTarget.AddRange(ScenarioManager.Scenario.HeroSummons);
			CShowRedistributeDamageUI_MessageData cShowRedistributeDamageUI_MessageData = new CShowRedistributeDamageUI_MessageData(TargetingActor);
			cShowRedistributeDamageUI_MessageData.m_ActorsToRedistributeBetween = m_ActorsToTarget;
			cShowRedistributeDamageUI_MessageData.m_RedistributeDamageAbility = this;
			ScenarioRuleClient.MessageHandler(cShowRedistributeDamageUI_MessageData);
			break;
		}
		case ERedistributeState.ActorIsRedistributing:
		{
			TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
			ScenarioRuleClient.FirstAbilityStarted();
			AbilityHasHappened = true;
			m_CanUndo = false;
			CActorIsRedistributingDamage_MessageData cActorIsRedistributingDamage_MessageData = new CActorIsRedistributingDamage_MessageData(TargetingActor);
			cActorIsRedistributingDamage_MessageData.m_ActorsRedistributingDamageTo = m_HealthChanges.Select((Tuple<CActor, int> x) => x.Item1).ToList();
			cActorIsRedistributingDamage_MessageData.m_RedistributeDamageAbility = this;
			ScenarioRuleClient.MessageHandler(cActorIsRedistributingDamage_MessageData);
			break;
		}
		case ERedistributeState.ApplyRedistributions:
			foreach (Tuple<CActor, int> healthChange in m_HealthChanges)
			{
				CActor item = healthChange.Item1;
				item.Health = healthChange.Item2;
				GameState.ActorHealthCheck(TargetingActor, item);
				CRefreshActorHealth_MessageData message = new CRefreshActorHealth_MessageData(item, item, item.Health);
				ScenarioRuleClient.MessageHandler(message);
			}
			PhaseManager.StepComplete();
			break;
		}
		return false;
		IL_0089:
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

	public void StoreHealthChanges(List<Tuple<CActor, int>> healthChanges)
	{
		m_HealthChanges = healthChanges;
	}

	public int GetHealthChangeForActor(CActor actor)
	{
		if (m_HealthChanges == null)
		{
			return 0;
		}
		return m_HealthChanges.FirstOrDefault((Tuple<CActor, int> it) => it.Item1 == actor)?.Item2 ?? 0;
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (!dontMoveState)
		{
			m_State++;
		}
		return m_State == ERedistributeState.RedistributeDone;
	}

	public override void Undo()
	{
		base.Undo();
		CHideRedistributeDamageUI_MessageData message = new CHideRedistributeDamageUI_MessageData(TargetingActor);
		ScenarioRuleClient.MessageHandler(message);
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityRedistributeDamage(new SEventAbility.SEventAbilityBaseData(TargetingActor, subTypeAbility, this), m_State));
	}

	public override bool IsPositive()
	{
		return true;
	}

	public override bool CanReceiveTileSelection()
	{
		return false;
	}
}
