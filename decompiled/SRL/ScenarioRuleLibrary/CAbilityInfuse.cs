using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;

namespace ScenarioRuleLibrary;

public class CAbilityInfuse : CAbility
{
	public enum EInfuseState
	{
		ShowPicker,
		Infusing,
		Done
	}

	[JsonProperty]
	public List<ElementInfusionBoardManager.EElement> SelectedElementsToInfuse = new List<ElementInfusionBoardManager.EElement>();

	private EInfuseState m_State;

	[JsonProperty]
	public List<ElementInfusionBoardManager.EElement> ElementsToInfuse { get; set; }

	[JsonProperty]
	public bool ShowPicker { get; set; }

	public CBaseCard ParentBaseCard { get; set; }

	public bool HasInfusedAnElement { get; set; }

	public bool InfuseOnlyWaning => GetMiscVar("InfuseOnlyWaning").GetBool();

	private bool InfuseAsStrong
	{
		get
		{
			if (!GetMiscVar("InfuseAsStrong").GetBool())
			{
				if (TargetingActor != null)
				{
					return GameState.TurnActor != TargetingActor;
				}
				return false;
			}
			return true;
		}
	}

	public CAbilityInfuse(List<ElementInfusionBoardManager.EElement> elementsToInfuse, bool showPicker = false, CBaseCard parentBaseCard = null)
	{
		ElementsToInfuse = elementsToInfuse;
		ShowPicker = showPicker;
		ParentBaseCard = parentBaseCard;
		m_State = EInfuseState.Infusing;
		HasInfusedAnElement = false;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_ActorsToTarget.Clear();
		m_ActorsToTarget.Add(TargetingActor);
		m_ValidActorsInRange = new List<CActor>();
		m_ValidActorsInRange.Add(TargetingActor);
		m_State = EInfuseState.Infusing;
		if (ShowPicker && InfuseOnlyWaning)
		{
			ElementsToInfuse.RemoveAll((ElementInfusionBoardManager.EElement x) => ElementInfusionBoardManager.ElementColumn(x) != ElementInfusionBoardManager.EColumn.Waning);
			m_State = EInfuseState.ShowPicker;
		}
		else if (ShowPicker)
		{
			m_State = EInfuseState.ShowPicker;
		}
		else
		{
			m_CanUndo = false;
		}
		m_CanSkip = false;
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
		if (ElementAlreadyStrongAndInfuseIfNotStrongSet())
		{
			PhaseManager.NextStep();
			return true;
		}
		if (!ProcessIfDead)
		{
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				goto IL_0099;
			}
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
			{
				AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
				if (miscAbilityData == null || miscAbilityData.IgnoreStun != true)
				{
					goto IL_0099;
				}
			}
		}
		switch (m_State)
		{
		case EInfuseState.ShowPicker:
			m_CanSkip = MiscAbilityData == null || !MiscAbilityData.CanSkip.HasValue || MiscAbilityData.CanSkip.Value;
			if (ShowPicker)
			{
				if (ElementsToInfuse.Any((ElementInfusionBoardManager.EElement a) => a == ElementInfusionBoardManager.EElement.Any) || ElementsToInfuse.Count > 1)
				{
					CShowElementPicker_MessageData message = new CShowElementPicker_MessageData(TargetingActor)
					{
						m_InfuseAbility = this
					};
					ScenarioRuleClient.MessageHandler(message);
				}
				else if (TargetingActor.Type != CActor.EType.Player)
				{
					m_State = EInfuseState.Infusing;
					Perform();
				}
				else
				{
					CActorIsSelectingTargetingFocus_MessageData cActorIsSelectingTargetingFocus_MessageData = new CActorIsSelectingTargetingFocus_MessageData(TargetingActor);
					cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility = this;
					cActorIsSelectingTargetingFocus_MessageData.m_IsPositive = IsPositive();
					ScenarioRuleClient.MessageHandler(cActorIsSelectingTargetingFocus_MessageData);
				}
			}
			else
			{
				m_State = EInfuseState.Infusing;
				Perform();
			}
			break;
		case EInfuseState.Infusing:
			m_CanUndo = false;
			m_CanSkip = false;
			if (TargetingActor.Type == CActor.EType.Player)
			{
				TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
			}
			AbilityHasHappened = true;
			DoInfuse();
			PhaseManager.NextStep();
			return true;
		}
		return false;
		IL_0099:
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

	public void DoInfuse()
	{
		List<ElementInfusionBoardManager.EElement> list = null;
		if (TargetingActor.Type == CActor.EType.Player || (TargetingActor is CHeroSummonActor { Summoner: not null } && ShowPicker))
		{
			bool flag = InfuseOnlyWaning && !ShowPicker;
			if (flag)
			{
				SelectedElementsToInfuse.Clear();
				SelectedElementsToInfuse.AddRange(ElementInfusionBoardManager.GetAvailableElements());
			}
			if (ShowPicker && SelectedElementsToInfuse.Count > 0)
			{
				list = ElementInfusionBoardManager.Infuse(SelectedElementsToInfuse, TargetingActor, InfuseAsStrong);
				HasInfusedAnElement = list.Count > 0;
			}
			else
			{
				SelectedElementsToInfuse.AddRange(ElementsToInfuse);
				if (flag)
				{
					SelectedElementsToInfuse.Clear();
					SelectedElementsToInfuse.AddRange(ElementInfusionBoardManager.GetAvailableElements());
				}
				List<ElementInfusionBoardManager.EElement> list2 = SelectedElementsToInfuse.FindAll((ElementInfusionBoardManager.EElement x) => !x.Equals(ElementInfusionBoardManager.EElement.Any));
				AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
				if (miscAbilityData != null && miscAbilityData.InfuseIfNotStrong == true)
				{
					list2 = list2.Where((ElementInfusionBoardManager.EElement x) => ElementInfusionBoardManager.ElementColumn(x) != ElementInfusionBoardManager.EColumn.Strong).ToList();
				}
				if (list2.Count > 0)
				{
					list = ElementInfusionBoardManager.Infuse(list2, TargetingActor, InfuseAsStrong);
					HasInfusedAnElement = list.Count > 0;
				}
				else if (SelectedElementsToInfuse.Count == 1 && SelectedElementsToInfuse[0].Equals(ElementInfusionBoardManager.EElement.Any))
				{
					ElementInfusionBoardManager.EElement[] array = ElementInfusionBoardManager.Elements.Where((ElementInfusionBoardManager.EElement x) => ElementInfusionBoardManager.ElementColumn(x) != ElementInfusionBoardManager.EColumn.Strong && x != ElementInfusionBoardManager.EElement.Any).ToArray();
					if (array.Length != 0)
					{
						int num = SharedClient.GlobalRNG.Next(0, array.Length);
						ElementInfusionBoardManager.Infuse(array[num], TargetingActor, endOfMonsterSet: false, InfuseAsStrong);
						HasInfusedAnElement = true;
					}
				}
				if ((Flags & EAbilityFlag.ModifiesTrophyTokens) != 0 && TargetingActor is CPlayerActor cPlayerActor)
				{
					cPlayerActor.SetTrophyTokens(cPlayerActor.GetTrophyTokens() + Tokens);
				}
			}
			if (flag)
			{
				ElementInfusionBoardManager.ProcessEndTurnInfusions();
			}
		}
		else
		{
			SelectedElementsToInfuse.AddRange(ElementsToInfuse);
			if (SelectedElementsToInfuse.Count > 0)
			{
				list = ElementInfusionBoardManager.Infuse(SelectedElementsToInfuse, TargetingActor, InfuseAsStrong);
				HasInfusedAnElement = list.Count > 0;
			}
		}
		if (HasInfusedAnElement || (!ShowPicker && SelectedElementsToInfuse.Any((ElementInfusionBoardManager.EElement a) => a == ElementInfusionBoardManager.EElement.Any)))
		{
			CElementsInfused_MessageData message = new CElementsInfused_MessageData(AnimOverload, TargetingActor)
			{
				m_ChangedElements = (list ?? SelectedElementsToInfuse),
				m_InfuseAbility = this,
				m_IsConsume = false
			};
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			return m_State == EInfuseState.Infusing;
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
		return m_State == EInfuseState.Done;
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityInfuse(new SEventAbility.SEventAbilityBaseData(TargetingActor, subTypeAbility, this), m_State, SelectedElementsToInfuse));
	}

	public bool ElementAlreadyStrongAndInfuseIfNotStrongSet()
	{
		AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
		if (miscAbilityData != null && miscAbilityData.InfuseIfNotStrong == true)
		{
			return ElementsToInfuse.All((ElementInfusionBoardManager.EElement x) => ElementInfusionBoardManager.ElementColumn(x) == ElementInfusionBoardManager.EColumn.Strong);
		}
		return false;
	}

	public override bool IsPositive()
	{
		return true;
	}
}
