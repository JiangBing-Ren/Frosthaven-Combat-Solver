using System.Collections.Generic;
using FHGUI.Elements;
using FHGUI.Scenario.DTO;
using FHGUI.Scenario.Modules;
using FHGUI.Scenario.Services;
using ScenarioRuleLibrary;
using UnityEngine.InputSystem;
using UnityEngine.Scripting;

namespace FHGUI.Scenario.States;

public class ActionSelectionViewState : FHScenarioPhaseViewState
{
	public override CPhase.PhaseType PhaseType => CPhase.PhaseType.ActionSelection;

	[Preserve]
	private ActionBoxViewModule ActionsBox => View.Modules.ActionsBox;

	private NetworkViewService NetworkService => View.GetService<NetworkViewService>();

	private ActorViewService ActorService => View.GetService<ActorViewService>();

	private PlayerViewService PlayerService => View.GetService<PlayerViewService>();

	private ActionsViewService ActionsService => View.GetService<ActionsViewService>();

	public override void OnShow()
	{
	}

	public override void Update()
	{
		CActor currentActor = PlayerService.CurrentActor;
		List<ButtonsBarParam> list = new List<ButtonsBarParam>();
		SetPlayerActions(list, currentActor);
		ActionsBox.ClearInfoText();
		ActionsBox.SetButtons(list.ToArray());
		if (PlayerViewService.IsActorUnderMyControl(currentActor))
		{
			List<TacItemDTO> usableItems = ActorService.GetUsableItems(currentActor);
			ActionsBox.SetUsableItems(usableItems);
			ActionsBox.OnItemClicked = (TacItemDTO item) =>
			{
				ActorService.UseItem(currentActor, item);
			};
		}
		else
		{
			ActionsBox.ClearUsableItems();
		}
	}

	private void SetPlayerActions(List<ButtonsBarParam> buttons, CActor actor)
	{
		if (actor != null)
		{
			ProcessUndoButton(buttons, actor);
			ProcessReadyButton(buttons, actor);
			ProcessSkipButton(buttons, actor);
		}
	}

	private void ProcessReadyButton(List<ButtonsBarParam> buttons, CActor actor)
	{
		if (ActionsViewService.TryGetMovementButtonParam(actor, out var resultButtonParam))
		{
			buttons.Add(resultButtonParam);
		}
		else
		{
			TryAddLegacyButton(buttons, Choreographer.s_Choreographer.readyButton, InputFHManager.Controls.Generic.Confirm);
		}
	}

	private void ProcessUndoButton(List<ButtonsBarParam> buttons, CActor actor)
	{
		TryAddLegacyButton(buttons, Choreographer.s_Choreographer.m_UndoButton, InputFHManager.Controls.Scenario.ClearTargetsOrUndo);
	}

	private void ProcessSkipButton(List<ButtonsBarParam> buttons, CActor actor)
	{
		TryAddLegacyButton(buttons, Choreographer.s_Choreographer.m_SkipButton, InputFHManager.Controls.Scenario.SkipAction);
	}

	private static bool TryAddLegacyButton(List<ButtonsBarParam> buttons, ILegacyButton legacyButton, InputAction hotkey, bool tagLegacyButton = false)
	{
		if (!legacyButton.IsActiveAndEnabled)
		{
			return false;
		}
		string text = (tagLegacyButton ? ("!!!" + legacyButton.Text + "!!!") : legacyButton.Text);
		buttons.Add(new ButtonsBarParam
		{
			LabelKey = FHUILocFormatting.PrepareNonLocButtonText(text),
			Hotkey = hotkey,
			Callback = legacyButton.OnClick,
			Interactable = legacyButton.IsInteractable
		});
		return true;
	}
}
