using System.Collections.Generic;
using FHGUI.Elements;
using FHGUI.Scenario.DTO;
using FHGUI.Scenario.Modules;
using FHGUI.Scenario.Services;
using ScenarioRuleLibrary;
using UnityEngine.Scripting;

namespace FHGUI.Scenario.States;

public class SelectAbilityCardsViewState : FHScenarioPhaseViewState
{
	public override CPhase.PhaseType PhaseType => CPhase.PhaseType.SelectAbilityCardsOrLongRest;

	[Preserve]
	private ActionBoxViewModule ActionsBox => View.Modules.ActionsBox;

	private PlayerViewService PlayerService => View.GetService<PlayerViewService>();

	private NetworkViewService NetworkService => View.GetService<NetworkViewService>();

	private ActorViewService ActorService => View.GetService<ActorViewService>();

	public override void Update()
	{
		List<ButtonsBarParam> list = new List<ButtonsBarParam>();
		if (!NetworkService.IsCurrentPlayerSpectating)
		{
			if (NetworkService.IsCurrentPlayerReady)
			{
				ActionsBox.SetInfoText("GUI_WAITING_PARTY_PLAYERS");
				ButtonsBarParam item = new ButtonsBarParam("GUI_CANCEL", InputFHManager.Controls.Generic.Confirm, CancelReady)
				{
					Interactable = true
				};
				list.Add(item);
				ActionsBox.ClearUsableItems();
			}
			else
			{
				ActionsBox.ClearInfoText();
				SetPlayerActions(list, PlayerService.SelectedPlayerActor.IsUnderMyControl());
				List<TacItemDTO> usableItems = ActorService.GetUsableItems(PlayerService.SelectedPlayerActor);
				ActionsBox.SetUsableItems(usableItems);
				ActionsBox.OnItemClicked = (TacItemDTO item2) =>
				{
					ActorService.UseItem(PlayerService.SelectedPlayerActor, item2);
				};
			}
		}
		else
		{
			ActionsBox.SetInfoText("GUI_WAITING_PARTY_PLAYERS");
			ActionsBox.ClearUsableItems();
		}
		ActionsBox.SetButtons(list.ToArray());
		PlayerService.GetPartyInfo(out var _, out var _);
	}

	private void SetPlayerActions(List<ButtonsBarParam> buttons, bool underMyControl)
	{
		CPlayerActor selectedPlayerActor = PlayerService.SelectedPlayerActor;
		if (selectedPlayerActor == null)
		{
			return;
		}
		ButtonsBarParam item = new ButtonsBarParam("GUI_END_SELECTION", InputFHManager.Controls.Generic.Confirm, CurrentPlayerReady)
		{
			Interactable = PlayerViewService.AreAllOwnedPlayerCardsHandSelectionReady()
		};
		buttons.Add(item);
		if (selectedPlayerActor.UsesTimeTokens() && selectedPlayerActor.Agility != CPlayerActor.EAgility.None)
		{
			string labelKey = "";
			if (selectedPlayerActor.Agility == CPlayerActor.EAgility.Fast)
			{
				labelKey = "@FAST";
			}
			else if (selectedPlayerActor.Agility == CPlayerActor.EAgility.Slow)
			{
				labelKey = "@SLOW";
			}
			item = new ButtonsBarParam(labelKey, InputFHManager.Controls.Generic.Confirm, SwitchBlinkbladeFastSlow)
			{
				Icon = UIInfoTools.Instance.GetAgilityIcon(selectedPlayerActor.Agility),
				Interactable = underMyControl
			};
			buttons.Add(item);
		}
		if (!underMyControl)
		{
			return;
		}
		if (!selectedPlayerActor.CharacterClass.LongRest)
		{
			if (!selectedPlayerActor.CharacterClass.ShortRest)
			{
				if (PlayerService.CanShortRest(selectedPlayerActor))
				{
					buttons.Add(new ButtonsBarParam("GUI_SHORT_REST", InputFHManager.Controls.Generic.Confirm, SelectShortRest));
				}
				if (PlayerService.CanUseLongRest(selectedPlayerActor))
				{
					buttons.Add(new ButtonsBarParam("GUI_LONG_REST", InputFHManager.Controls.Generic.Confirm, SelectLongRest));
				}
			}
			else if (PlayerService.CanShortRest(selectedPlayerActor))
			{
				buttons.Add(new ButtonsBarParam("GUI_CANCEL", InputFHManager.Controls.Generic.Confirm, CancelShortRest));
			}
		}
		else
		{
			buttons.Add(new ButtonsBarParam("GUI_CANCEL", InputFHManager.Controls.Generic.Confirm, CanceLongRest));
		}
	}

	private void CurrentPlayerReady()
	{
		NetworkService.SetCurrentPlayerReady(ready: true);
	}

	private void CancelReady()
	{
		NetworkService.SetCurrentPlayerReady(ready: false);
	}

	private void SelectShortRest()
	{
		PlayerService.SetUseShortRest(PlayerService.SelectedPlayerActor, isSelected: true);
	}

	private void CancelShortRest()
	{
		PlayerService.SetUseShortRest(PlayerService.SelectedPlayerActor, isSelected: false);
	}

	private void SelectLongRest()
	{
		PlayerService.SetUseLongRest(PlayerService.SelectedPlayerActor, useLongRest: true);
	}

	private void CanceLongRest()
	{
		PlayerService.SetUseLongRest(PlayerService.SelectedPlayerActor, useLongRest: false);
	}

	private void SwitchBlinkbladeFastSlow()
	{
		CPlayerActor selectedPlayerActor = PlayerService.SelectedPlayerActor;
		CPlayerActor.EAgility agility = selectedPlayerActor.Agility;
		agility = ((agility == CPlayerActor.EAgility.Slow) ? CPlayerActor.EAgility.Fast : CPlayerActor.EAgility.Slow);
		PlayerService.SetBlinkbladeAgility(selectedPlayerActor, agility);
	}
}
