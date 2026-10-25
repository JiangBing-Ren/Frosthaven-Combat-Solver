using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityAddModifierToMonsterDeck : CAbility
{
	public enum EAddModifierToMonsterDeckState
	{
		PlayerConfirming,
		AddingModifiers,
		AddingModifiersDone
	}

	private EAddModifierToMonsterDeckState m_State;

	[JsonProperty]
	private List<string> m_ModifierCardNamesToAdd;

	private List<string> m_CombatLogStrings = new List<string>();

	private bool isPositive;

	public List<string> ModifierCardNamesToAdd
	{
		get
		{
			return m_ModifierCardNamesToAdd;
		}
		set
		{
			m_ModifierCardNamesToAdd = value.ToList();
		}
	}

	public CAbilityAddModifierToMonsterDeck(List<string> addModifiers)
	{
		m_ModifierCardNamesToAdd = addModifiers;
		CheckSkippability();
	}

	public override void Start(CActor targetingActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetingActor, filterActor, controllingActor);
		m_ActorsToTarget = new List<CActor> { TargetingActor };
		CheckSkippability();
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
		case EAddModifierToMonsterDeckState.PlayerConfirming:
		{
			if (MiscAbilityData.AutotriggerAbility.HasValue && MiscAbilityData.AutotriggerAbility.Value)
			{
				PhaseManager.StepComplete();
				break;
			}
			CActorIsSelectingTargetingFocus_MessageData cActorIsSelectingTargetingFocus_MessageData = new CActorIsSelectingTargetingFocus_MessageData(TargetingActor);
			cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility = this;
			cActorIsSelectingTargetingFocus_MessageData.m_IsPositive = true;
			ScenarioRuleClient.MessageHandler(cActorIsSelectingTargetingFocus_MessageData);
			break;
		}
		case EAddModifierToMonsterDeckState.AddingModifiers:
			if (ScenarioManager.Scenario.Enemies.Count > 0)
			{
				CEnemyActor cEnemyActor = ScenarioManager.Scenario.Enemies[0];
				if (AbilityFilter.IsValidTarget(cEnemyActor, TargetingActor, isTargetedAbility: false, useTargetOriginalType: false, true, skipUntargetableCheck: true))
				{
					for (int i = 0; i < Strength; i++)
					{
						MonsterClassManager.AddAdditionalModifierCards(cEnemyActor, ModifierCardNamesToAdd);
					}
				}
			}
			if (ScenarioManager.Scenario.Enemy2Monsters.Count > 0)
			{
				CEnemyActor cEnemyActor2 = ScenarioManager.Scenario.Enemy2Monsters[0];
				if (AbilityFilter.IsValidTarget(cEnemyActor2, TargetingActor, isTargetedAbility: false, useTargetOriginalType: false, true, skipUntargetableCheck: true))
				{
					for (int j = 0; j < Strength; j++)
					{
						MonsterClassManager.AddAdditionalModifierCards(cEnemyActor2, ModifierCardNamesToAdd);
					}
				}
			}
			if (ScenarioManager.Scenario.AllyMonsters.Count > 0)
			{
				CEnemyActor cEnemyActor3 = ScenarioManager.Scenario.AllyMonsters[0];
				if (AbilityFilter.IsValidTarget(cEnemyActor3, TargetingActor, isTargetedAbility: false, useTargetOriginalType: false, true, skipUntargetableCheck: true))
				{
					for (int k = 0; k < Strength; k++)
					{
						MonsterClassManager.AddAdditionalModifierCards(cEnemyActor3, ModifierCardNamesToAdd);
					}
				}
			}
			if (ScenarioManager.Scenario.NeutralMonsters.Count > 0)
			{
				CEnemyActor cEnemyActor4 = ScenarioManager.Scenario.NeutralMonsters[0];
				if (AbilityFilter.IsValidTarget(cEnemyActor4, TargetingActor, isTargetedAbility: false, useTargetOriginalType: false, true, skipUntargetableCheck: true))
				{
					for (int l = 0; l < Strength; l++)
					{
						MonsterClassManager.AddAdditionalModifierCards(cEnemyActor4, ModifierCardNamesToAdd);
					}
				}
			}
			if (ScenarioManager.Scenario.Enemies.Where((CEnemyActor x) => x.MonsterClass.Boss).ToList().Count > 0)
			{
				CEnemyActor cEnemyActor5 = ScenarioManager.Scenario.Enemies.Where((CEnemyActor x) => x.MonsterClass.Boss).ToList()[0];
				if (AbilityFilter.IsValidTarget(cEnemyActor5, TargetingActor, isTargetedAbility: false, useTargetOriginalType: false, true, skipUntargetableCheck: true))
				{
					for (int num = 0; num < Strength; num++)
					{
						MonsterClassManager.AddAdditionalModifierCards(cEnemyActor5, ModifierCardNamesToAdd);
					}
				}
			}
			foreach (string modifierName in ModifierCardNamesToAdd)
			{
				AttackModifierYMLData attackModifierYMLData = ScenarioRuleClient.SRLYML.AttackModifiers.FirstOrDefault((AttackModifierYMLData f) => f.Name == modifierName);
				m_CombatLogStrings.Add(attackModifierYMLData.Card.AttackModifierLogString());
			}
			PhaseManager.NextStep();
			break;
		}
		return false;
		IL_0089:
		if (TargetingActor.Type == CActor.EType.Player)
		{
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				CPlayerIsSleeping_MessageData message = new CPlayerIsSleeping_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message);
			}
			else
			{
				CPlayerIsStunned_MessageData message2 = new CPlayerIsStunned_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message2);
			}
		}
		else
		{
			PhaseManager.NextStep();
		}
		return true;
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (!dontMoveState)
		{
			m_State++;
		}
		return m_State == EAddModifierToMonsterDeckState.AddingModifiersDone;
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityAddModifierToMonsterDeck(new SEventAbility.SEventAbilityBaseData(TargetingActor, subTypeAbility, this), m_State, m_CombatLogStrings));
	}

	private void CheckSkippability()
	{
		if (m_ModifierCardNamesToAdd.Any((string x) => x.Contains("Bless") || x.Contains("_P") || x.Contains("_T2")))
		{
			isPositive = false;
			SetCanSkip(canSkip: false);
		}
	}

	public override string GetDescription()
	{
		return "AddModifierToMonsterDeck";
	}

	public override bool CanClearTargets()
	{
		return false;
	}

	public override bool CanReceiveTileSelection()
	{
		return false;
	}

	public bool HasPassedState(EAddModifierToMonsterDeckState addModifierToMonsterDeckState)
	{
		return m_State > addModifierToMonsterDeckState;
	}

	public override bool EnoughTargetsSelected()
	{
		return true;
	}

	public override bool IsCurrentlyTargetingActors()
	{
		return false;
	}

	public override bool IsPositive()
	{
		return isPositive;
	}
}
