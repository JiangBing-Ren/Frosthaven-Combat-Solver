using System.Collections.Generic;
using System.Linq;
using FHGUI.Elements;
using FHGUI.Scenario.Modules;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;

namespace FHGUI.Scenario.Services;

public class ActionsViewService : FHScenarioViewService
{
	public ActionsViewService(FHScenarioViewController view)
		: base(view)
	{
	}

	public static bool TryGetMovementButtonParam(CActor actor, out ButtonsBarParam resultButtonParam)
	{
		Waypoint getLastWaypoint = Waypoint.GetLastWaypoint;
		if (getLastWaypoint == null)
		{
			resultButtonParam = default;
			return false;
		}
		string text = ((!getLastWaypoint.WaypointIsADoor) ? (getLastWaypoint.Type switch
		{
			Waypoint.EWaypointType.Move => "GUI_CONFIRM_MOVEMENT", 
			Waypoint.EWaypointType.Push => "GUI_CONFIRM_PUSH", 
			Waypoint.EWaypointType.Pull => "GUI_CONFIRM_PULL", 
			Waypoint.EWaypointType.Teleport2 => "GUI_CONFIRM_TELEPORT2", 
			Waypoint.EWaypointType.Attack => "GUI_CONFIRM_ATTACK", 
			_ => "GUI_END_SELECTION", 
		}) : "GUI_OPEN_DOOR");
		string labelKey = text;
		ButtonsBarParam buttonsBarParam = new ButtonsBarParam(labelKey, InputFHManager.Controls.Generic.Confirm, ConfirmMovement)
		{
			Interactable = IsMovementInteractable(getLastWaypoint, actor)
		};
		resultButtonParam = buttonsBarParam;
		return true;
	}

	public static bool IsMovementInteractable(Waypoint waypoint, CActor actor)
	{
		bool flag = actor.IsUnderMyControl() && waypoint.CanEndMovementHere;
		if (flag)
		{
			flag = ((waypoint.Type == Waypoint.EWaypointType.Attack) ? (WorldspaceStarHexDisplay.Instance.CurrentNumberSelectedTargets() > 0) : WorldspaceStarHexDisplay.Instance.IsMovementPossible());
		}
		return flag;
	}

	public static void ConfirmMovement()
	{
		Waypoint getLastWaypoint = Waypoint.GetLastWaypoint;
		if (!(getLastWaypoint == null))
		{
			Choreographer.s_Choreographer.DisableTileSelection(active: true);
			if (getLastWaypoint.WaypointIsADoor)
			{
				getLastWaypoint.OpenDoor();
			}
			else
			{
				getLastWaypoint.OnPress();
			}
		}
	}

	public bool CheckAdvantageChoice(CMessageData message, ScenarioAttackSummaryModule modulesAttackSummary)
	{
		if (!GameState.ChooseAdvantageModifiers && !CAbilityAttack.ConfirmAttackAbilityBeforeFinishing)
		{
			return false;
		}
		switch (message.m_Type)
		{
		case CMessageData.MessageType.StartAbility:
		case CMessageData.MessageType.ActorIsSelectingAttackFocusTargets:
		case CMessageData.MessageType.ActorIsAttacking:
		case CMessageData.MessageType.ActorHasAttacked:
		case CMessageData.MessageType.EndAbility:
		case CMessageData.MessageType.ActionAbilityHasHappened:
		case CMessageData.MessageType.ClearAllActorEvents:
		case CMessageData.MessageType.TileSelectionFinished:
		case CMessageData.MessageType.ActorWantsAnAttackModifierConfirmation:
		case CMessageData.MessageType.ActorWantsAnActiveBonusConfirmation:
		case CMessageData.MessageType.ShowChooseAttackModifierAdvantage:
		case CMessageData.MessageType.ActorFinishedDrawingModifiers:
		case CMessageData.MessageType.StartedDrawingModifier:
		case CMessageData.MessageType.FinishedDrawingModifier:
		case CMessageData.MessageType.StackedInlineSubAbilities:
		case CMessageData.MessageType.StartedAbility:
			return GameState.CurrentAttackSummaries()?.Any() ?? false;
		default:
			return false;
		}
	}

	public void UpdateAttackModifiersModule(ScenarioAttackSummaryModule modulesAttackSummary)
	{
		IEnumerable<CAttackSummary.TargetSummary> enumerable = GameState.CurrentAttackSummaries();
		if (enumerable == null || !enumerable.Any())
		{
			modulesAttackSummary.Hide();
			return;
		}
		if (!GameState.ChooseAdvantageModifiers && !CAbilityAttack.ConfirmAttackAbilityBeforeFinishing)
		{
			modulesAttackSummary.Hide();
			return;
		}
		CActor controllingActor = GameState.GetControllingActor(GameState.CurrentActor);
		if (!(GameState.CurrentActor is CPlayerActor) && controllingActor?.PlayerActor == null)
		{
			modulesAttackSummary.Hide();
			return;
		}
		ScenarioAttackSummaryModule.Params obj = new ScenarioAttackSummaryModule.Params
		{
			Summaries = new List<UIScenarioAttackSummaryElement.SummaryData>()
		};
		foreach (CAttackSummary.TargetSummary item2 in GameState.CurrentAttackSummaries())
		{
			if (item2.NoDrawing())
			{
				continue;
			}
			UIScenarioAttackSummaryElement.SummaryData item = new UIScenarioAttackSummaryElement.SummaryData
			{
				Summary = item2
			};
			if (item2.State == CAttackSummary.TargetSummary.SummaryState.DrawingModifiers)
			{
				item.Selected = GameState.AttackModifierCardsSelectedToUseAfterUnfreezingThread;
				if (GameState.IsSelectingAdvantageModifier)
				{
					item.ModiferDrawn = item2.DrawnAttackMods.Where((AttackModifierYMLData mod) =>
					{
						List<AttackModifierYMLData> currentlyChoosingModifiers = GameState.CurrentlyChoosingModifiers;
						return currentlyChoosingModifiers != null && !currentlyChoosingModifiers.Contains(mod);
					}).ToList();
					item.ModiferChoice = GameState.CurrentlyChoosingModifiers;
				}
				else
				{
					item.ModiferDrawn = item2.DrawnAttackMods;
				}
			}
			else if (item2.State == CAttackSummary.TargetSummary.SummaryState.FinishedDrawingModifiers)
			{
				if (item2.OverallAdvantage == EAdvantageStatuses.Advantage)
				{
					List<AttackModifierYMLData> modiferChoice = item2.DrawnAttackMods.TakeLast(2).ToList();
					item.ModiferDrawn = item2.UsedAttackMods.Take(item2.UsedAttackMods.Count - 1).ToList();
					item.ModiferChoice = modiferChoice;
					item.ModiferNotUsed = item2.NotUsedAttackMods;
				}
				else if (item2.OverallAdvantage == EAdvantageStatuses.Disadvantage)
				{
					item.ModiferDrawn = item2.DrawnAttackMods.Take(item2.DrawnAttackMods.Count - 2).ToList();
					item.ModiferChoice = item2.DrawnAttackMods.TakeLast(2).ToList();
					item.ModiferNotUsed = item2.NotUsedAttackMods;
				}
				else
				{
					item.ModiferDrawn = item2.DrawnAttackMods;
					item.ModiferNotUsed = item2.NotUsedAttackMods;
				}
				CAbility cAbility = PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability;
				if (cAbility != null)
				{
					CAttackSummary.TargetSummary targetSummary = cAbility.VarsContext.GetVar("AbilityCameFromSummary").GetObject<CAttackSummary.TargetSummary>();
					CAbilityAttack.AttackModifierAbilityInfo attackModifierAbilityInfo = cAbility.VarsContext.GetVar("AttackSummaryModInfo").GetObject<CAbilityAttack.AttackModifierAbilityInfo>();
					if (targetSummary == item2 && attackModifierAbilityInfo?.Card != null)
					{
						item.ActiveAbilityModifiers = new List<AttackModifierYMLData> { attackModifierAbilityInfo.Card };
					}
				}
			}
			obj.Summaries.Add(item);
		}
		if (obj.Summaries.Count != 0)
		{
			modulesAttackSummary.Init(obj);
			modulesAttackSummary.Show();
		}
	}
}
