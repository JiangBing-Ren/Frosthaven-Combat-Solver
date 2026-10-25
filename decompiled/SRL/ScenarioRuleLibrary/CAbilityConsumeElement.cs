using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityConsumeElement : CAbility
{
	public enum EConsumeState
	{
		ActorSelectingTargetingFocus,
		Consuming,
		ShowPicker,
		Done
	}

	private EConsumeState m_State;

	[JsonProperty]
	public List<ElementInfusionBoardManager.EElement> ElementsToConsume { get; set; }

	[JsonProperty]
	public bool ShowPicker { get; set; }

	public CBaseCard ParentBaseCard { get; set; }

	public CAbilityConsumeElement(List<ElementInfusionBoardManager.EElement> elementsToConsume, bool showPicker = false, CBaseCard parentBaseCard = null)
	{
		ElementsToConsume = elementsToConsume;
		ShowPicker = showPicker;
		ParentBaseCard = parentBaseCard;
		m_State = EConsumeState.Consuming;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_ActorsToTarget.Clear();
		m_ActorsToTarget.Add(TargetingActor);
		m_ValidActorsInRange = new List<CActor>();
		m_ValidActorsInRange.Add(TargetingActor);
		m_State = EConsumeState.ActorSelectingTargetingFocus;
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
		case EConsumeState.ActorSelectingTargetingFocus:
		{
			CActorIsSelectingTargetingFocus_MessageData cActorIsSelectingTargetingFocus_MessageData = new CActorIsSelectingTargetingFocus_MessageData(TargetingActor);
			cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility = this;
			cActorIsSelectingTargetingFocus_MessageData.m_IsPositive = true;
			ScenarioRuleClient.MessageHandler(cActorIsSelectingTargetingFocus_MessageData);
			break;
		}
		case EConsumeState.Consuming:
			m_CanUndo = false;
			if (TargetingActor.Type == CActor.EType.Player)
			{
				TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
			}
			AbilityHasHappened = true;
			DoConsume();
			return true;
		case EConsumeState.ShowPicker:
			if (ShowPicker && ElementsToConsume.Any((ElementInfusionBoardManager.EElement a) => a == ElementInfusionBoardManager.EElement.Any))
			{
				CShowElementPicker_MessageData message = new CShowElementPicker_MessageData(TargetingActor)
				{
					m_ConsumeAbility = this
				};
				ScenarioRuleClient.MessageHandler(message);
			}
			else
			{
				PhaseManager.NextStep();
			}
			return true;
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

	public void DoConsume()
	{
		if (TargetingActor.Type == CActor.EType.Player)
		{
			foreach (ElementInfusionBoardManager.EElement item in ElementsToConsume.FindAll((ElementInfusionBoardManager.EElement x) => !x.Equals(ElementInfusionBoardManager.EElement.Any)))
			{
				ElementInfusionBoardManager.Consume(item, TargetingActor);
			}
		}
		else
		{
			foreach (ElementInfusionBoardManager.EElement item2 in ElementsToConsume)
			{
				ElementInfusionBoardManager.Consume(item2, TargetingActor);
			}
		}
		CElementsInfused_MessageData message = new CElementsInfused_MessageData(AnimOverload, TargetingActor)
		{
			m_ChangedElements = ElementsToConsume,
			m_ConsumeAbility = this,
			m_IsConsume = true
		};
		ScenarioRuleClient.MessageHandler(message);
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			return m_State == EConsumeState.ActorSelectingTargetingFocus;
		}
		return false;
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (!dontMoveState)
		{
			m_State++;
		}
		if (m_State != EConsumeState.Done)
		{
			if (m_State == EConsumeState.ShowPicker)
			{
				return !ShowPicker;
			}
			return false;
		}
		return true;
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		Enum.TryParse<CAbilityInfuse.EInfuseState>(m_State.ToString(), out var result);
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityInfuse(new SEventAbility.SEventAbilityBaseData(TargetingActor, subTypeAbility, this), result, ElementsToConsume));
	}

	public override bool IsPositive()
	{
		return true;
	}
}
