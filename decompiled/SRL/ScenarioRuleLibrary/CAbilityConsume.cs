using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityConsume : CAbility
{
	public enum EConsumeState
	{
		ActorSelectingConsumes,
		Consuming,
		Done
	}

	[JsonProperty]
	public int TrophiesToConsume;

	[JsonProperty]
	public int ResonanceToConsume;

	[JsonProperty]
	public List<ElementInfusionBoardManager.EElement> ElementsToConsume = new List<ElementInfusionBoardManager.EElement>();

	[JsonProperty]
	public List<AbilityConsume> Consumes = new List<AbilityConsume>();

	private EConsumeState m_State;

	[JsonProperty]
	public bool ShowPicker { get; set; }

	[StateDump(Skip = true)]
	public bool WaitingForUI { get; set; }

	public CAbilityConsume(List<ElementInfusionBoardManager.EElement> elementsToConsume, bool showPicker = false, List<AbilityConsume> consumes = null)
	{
		ElementsToConsume.AddRange(elementsToConsume);
		m_State = EConsumeState.ActorSelectingConsumes;
		ShowPicker = showPicker;
		Consumes = consumes;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		if (AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true))
		{
			m_ActorsToTarget.Clear();
			m_ActorsToTarget.Add(targetActor);
			m_ValidActorsInRange = new List<CActor>();
			m_ValidActorsInRange.Add(targetActor);
			m_State = EConsumeState.ActorSelectingConsumes;
		}
		SetHasStarted(val: true);
	}

	private bool DoConsume(ElementInfusionBoardManager.EElement el)
	{
		bool flag = false;
		if (TargetingActor is CEnemyActor actor)
		{
			List<ElementInfusionBoardManager.EElement> enemyConsumedElements = ElementInfusionBoardManager.GetEnemyConsumedElements(actor);
			if (enemyConsumedElements != null)
			{
				flag = enemyConsumedElements.Contains(el);
			}
		}
		ElementInfusionBoardManager.Consume(el, TargetingActor);
		return !flag;
	}

	public override bool Perform()
	{
		base.Perform();
		if (GameState.WaitingForMercenarySpecialMechanicSlotChoice)
		{
			return true;
		}
		LogEvent(ESESubTypeAbility.AbilityPerform);
		List<ElementInfusionBoardManager.EElement> availableElements = ElementInfusionBoardManager.GetAvailableElements(TargetingActor);
		if (DontMoveAbilityState.HasValue && DontMoveAbilityState.Value && WaitingForUI)
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
				goto IL_00d1;
			}
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
			{
				AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
				if (miscAbilityData == null || miscAbilityData.IgnoreStun != true)
				{
					goto IL_00d1;
				}
			}
		}
		switch (m_State)
		{
		case EConsumeState.ActorSelectingConsumes:
		{
			CActor actorSpawningMessage = TargetingActor;
			if (TargetingActor.Type == CActor.EType.Player)
			{
				if (TargetingActor.OriginalType != CActor.EType.Player)
				{
					actorSpawningMessage = ControllingActor;
				}
				if (!CanConsume(out var missingTrophy, out var missingResonance, out var missingElements))
				{
					if (missingTrophy)
					{
						HelpBoxTooltipLocKey = "GUI_TOOLTIP_NO_TROPHY_TOKENS";
					}
					else if (missingResonance)
					{
						HelpBoxTooltipLocKey = "GUI_TOOLTIP_NO_RESONANCE_TOKENS";
					}
					else if (missingElements)
					{
						HelpBoxTooltipLocKey = "GUI_TOOLTIP_ELEMENT_INERT";
					}
					else
					{
						HelpBoxTooltipLocKey = "";
					}
				}
				else
				{
					HelpBoxTooltipLocKey = "";
				}
				CShowConsumes_MessageData message2 = new CShowConsumes_MessageData(actorSpawningMessage)
				{
					m_Ability = this,
					m_IsFirstAbility = (PhaseManager.CurrentPhaseAction != null && PhaseManager.CurrentPhaseAction.CurrentPhaseAbility == PhaseManager.CurrentPhaseAction.FirstPhaseAbility)
				};
				ScenarioRuleClient.MessageHandler(message2);
				break;
			}
			foreach (AbilityConsume consume in Consumes)
			{
				if (consume.ConsumeData.Elements[0] == ElementInfusionBoardManager.EElement.Any)
				{
					ElementsToConsume.Add(availableElements[0]);
				}
				else
				{
					ElementsToConsume.Add(consume.ConsumeData.Elements[0]);
				}
			}
			if (!ElementsToConsume.All((ElementInfusionBoardManager.EElement eElement) => availableElements.Count > 0 && (eElement == ElementInfusionBoardManager.EElement.Any || availableElements.Contains(eElement))))
			{
				PhaseManager.NextStep();
				return true;
			}
			m_State = EConsumeState.Consuming;
			Perform();
			break;
		}
		case EConsumeState.Consuming:
		{
			m_CanUndo = false;
			if (TargetingActor.Type == CActor.EType.Player)
			{
				TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
			}
			AbilityHasHappened = true;
			List<ElementInfusionBoardManager.EElement> list = ElementsToConsume?.FindAll((ElementInfusionBoardManager.EElement x) => !x.Equals(ElementInfusionBoardManager.EElement.Any));
			if (ElementsToConsume.Count > 0)
			{
				List<ElementInfusionBoardManager.EElement> list2 = new List<ElementInfusionBoardManager.EElement>();
				while (list.Count > 0)
				{
					ElementInfusionBoardManager.EElement element = list[0];
					list.RemoveAt(0);
					if (DoConsume(element))
					{
						list2.Add(element);
					}
					AbilityConsume abilityConsume = Consumes.Find((AbilityConsume consume) => TrophiesToConsume <= consume.ConsumeData.Trohpies && ResonanceToConsume <= consume.ConsumeData.Resonance && (consume.ConsumeData.Elements.Contains(element) || consume.ConsumeData.Elements.Contains(ElementInfusionBoardManager.EElement.Any)));
					foreach (ElementInfusionBoardManager.EElement item in abilityConsume.ConsumeData.Elements.FindAll((ElementInfusionBoardManager.EElement x) => !x.Equals(ElementInfusionBoardManager.EElement.Any)))
					{
						if (list.Contains(item))
						{
							if (DoConsume(item))
							{
								list2.Add(item);
							}
							list.Remove(item);
						}
					}
					StartConsumeAbility(abilityConsume);
				}
				CElementsConsumed_MessageData message = new CElementsConsumed_MessageData(AnimOverload, TargetingActor)
				{
					m_ChangedElements = list2,
					m_Consume = this
				};
				ScenarioRuleClient.MessageHandler(message);
			}
			if (TrophiesToConsume > 0)
			{
				CPlayerActor obj = TargetingActor as CPlayerActor;
				obj.SetTrophyTokens(obj.GetTrophyTokens() - TrophiesToConsume);
				if (list.Count == 0)
				{
					AbilityConsume consumeAbility = Consumes.Find((AbilityConsume consume) => TrophiesToConsume <= consume.ConsumeData.Trohpies && consume.ConsumeData.Elements.Count() == 0);
					StartConsumeAbility(consumeAbility);
				}
			}
			if (ResonanceToConsume > 0)
			{
				CPlayerActor obj2 = TargetingActor as CPlayerActor;
				obj2.SetResonanceTokens(obj2.GetResonanceTokens() - ResonanceToConsume);
				if (list.Count == 0)
				{
					AbilityConsume consumeAbility2 = Consumes.Find((AbilityConsume consume) => ResonanceToConsume <= consume.ConsumeData.Resonance && consume.ConsumeData.Elements.Count() == 0);
					StartConsumeAbility(consumeAbility2);
				}
			}
			PhaseManager.NextStep();
			return true;
		}
		}
		return false;
		IL_00d1:
		if (TargetingActor.Type == CActor.EType.Player)
		{
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				CPlayerIsSleeping_MessageData message3 = new CPlayerIsSleeping_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message3);
			}
			else
			{
				CPlayerIsStunned_MessageData message4 = new CPlayerIsStunned_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message4);
			}
		}
		else
		{
			PhaseManager.NextStep();
		}
		return true;
	}

	protected void StartConsumeAbility(AbilityConsume consumeAbility)
	{
		if (consumeAbility == null)
		{
			return;
		}
		if (PhaseManager.CurrentPhaseAction != null && consumeAbility.ConsumeData.XP > 0)
		{
			CAbility cAbility = CAbilityXP.CreateDefaultXP(consumeAbility.ConsumeData.XP, 1, 1, isMonster: false, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self));
			cAbility.MiscAbilityData.AutotriggerAbility = true;
			PhaseManager.CurrentPhaseAction.InjectAbility(cAbility, TargetingActor, GetAbilityBaseCard());
		}
		for (int num = consumeAbility.ConsumeData.AugmentationOps.Count - 1; num >= 0; num--)
		{
			CAbility ability = CAbility.CopyAbility(consumeAbility.ConsumeData.AugmentationOps[num].Ability, generateNewID: false);
			if (IsControlAbility)
			{
				if (PhaseManager.Phase is CPhaseAction cPhaseAction && !cPhaseAction.CurrentPhaseAbilities.Any((CPhaseAction.CPhaseAbility phaseAbility) => phaseAbility.m_Ability == ability))
				{
					ability.AddParentContext(VarsContext);
					ability.SetCancelAbilityFlag(cancel: false);
					cPhaseAction.StackNextAbilities(new List<CAbility> { ability }, TargetingActor, killAfter: false, stackToNextCurrent: true, copyCurrentActionID: true, interruptCurrent: false, addDirectlyToCurrentAbilities: false, forcedInterrupt: false, interruptRegardlessOfStrength: false, dontCreateCopies: false, null, GetAction()?.ID);
				}
			}
			else if (PhaseManager.Phase is CPhaseAction cPhaseAction2 && !cPhaseAction2.RemainingPhaseAbilities.Any((CPhaseAction.CPhaseAbility phaseAbility) => phaseAbility.m_Ability == ability))
			{
				ability.AddParentContext(VarsContext);
				ability.SetCancelAbilityFlag(cancel: false);
				cPhaseAction2.StackNextAbilities(new List<CAbility> { ability }, TargetingActor, killAfter: false, stackToNextCurrent: true, copyCurrentActionID: false, interruptCurrent: false, addDirectlyToCurrentAbilities: false, forcedInterrupt: false, interruptRegardlessOfStrength: false, dontCreateCopies: false, null, GetAction()?.ID);
			}
		}
	}

	public bool CanConsume()
	{
		bool missingTrophy;
		bool missingResonance;
		bool missingElements;
		return CanConsume(out missingTrophy, out missingResonance, out missingElements);
	}

	public bool CanConsume(out bool missingTrophy, out bool missingResonance, out bool missingElements)
	{
		List<ElementInfusionBoardManager.EElement> availableElements = ElementInfusionBoardManager.GetAvailableElements(TargetingActor);
		missingTrophy = (missingResonance = (missingElements = false));
		foreach (AbilityConsume consume in Consumes)
		{
			bool flag = true;
			bool flag2 = true;
			bool flag3 = true;
			bool flag4 = true;
			if (consume.ConsumeData.Trohpies > 0)
			{
				flag3 = true;
				flag2 = true;
				flag = TargetingActor is CPlayerActor cPlayerActor && cPlayerActor.UsesTrophyTokens() && consume.ConsumeData.Trohpies <= cPlayerActor.GetTrophyTokens();
				if (!flag)
				{
					missingTrophy = true;
				}
			}
			else if (consume.ConsumeData.Resonance > 0)
			{
				flag3 = true;
				flag = true;
				flag2 = TargetingActor is CPlayerActor cPlayerActor2 && cPlayerActor2.UsesResonanceTokens() && consume.ConsumeData.Resonance <= cPlayerActor2.GetResonanceTokens();
				if (!flag2)
				{
					missingResonance = true;
				}
			}
			else
			{
				flag3 = false;
			}
			if (consume.ConsumeData.Elements.Count > 0)
			{
				flag3 = ((!ShowPicker) ? consume.ConsumeData.Elements.All((ElementInfusionBoardManager.EElement element) => availableElements.Count > 0 && (element == ElementInfusionBoardManager.EElement.Any || availableElements.Contains(element))) : consume.ConsumeData.Elements.Any((ElementInfusionBoardManager.EElement element) => availableElements.Count > 0 && (element == ElementInfusionBoardManager.EElement.Any || availableElements.Contains(element))));
				if (!flag3)
				{
					missingElements = true;
				}
			}
			if (MiscVars != null)
			{
				TValue miscVar = GetMiscVar("CanConsume");
				if (miscVar.isValid)
				{
					TValue tValue = miscVar.Calc(new CVarsContext(this));
					if (tValue.isBool && !tValue.GetBool())
					{
						flag4 = false;
					}
				}
			}
			if (flag & flag3 & flag4 & flag2)
			{
				return true;
			}
		}
		return false;
	}

	public static bool CanConsume(List<object> Consumes)
	{
		return ElementInfusionBoardManager.GetAvailableElements().Any((ElementInfusionBoardManager.EElement consume) => Consumes.Contains(consume));
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			AbilityHasHappened = true;
			if (CActor.IsSameType(actor, TargetingActor))
			{
				if (m_PositiveConditions.Count > 0)
				{
					ProcessPositiveStatusEffects(actor);
				}
			}
			else if (m_NegativeConditions.Count > 0)
			{
				ProcessNegativeStatusEffects(actor);
			}
		}
		return true;
	}

	public override bool IsPositive()
	{
		if (m_PositiveConditions.Count <= 0)
		{
			return GetMiscVar("IsPositive").GetBool();
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
		return m_State == EConsumeState.Done;
	}

	public override bool CanReceiveTileSelection()
	{
		return false;
	}
}
