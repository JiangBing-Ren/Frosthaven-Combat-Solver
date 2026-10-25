using System.Collections.Generic;
using FHGUI.Elements;
using FHGUI.Scenario.Modules;
using FHGUI.Scenario.Services;
using ScenarioRuleLibrary;
using UnityEngine.Scripting;

namespace FHGUI.Scenario.States;

public class CharactersSelectedCardsViewState : FHScenarioPhaseViewState
{
	public override CPhase.PhaseType PhaseType => CPhase.PhaseType.MonsterClassesSelectAbilityCards;

	[Preserve]
	private ActionBoxViewModule ActionsBox => View.Modules.ActionsBox;

	private NetworkViewService NetworkService => View.GetService<NetworkViewService>();

	public override void Update()
	{
		List<ButtonsBarParam> list = new List<ButtonsBarParam>();
		if (NetworkService.IsHost)
		{
			ActionsBox.ClearInfoText();
			list.Add(new ButtonsBarParam("GUI_CONTINUE", InputFHManager.Controls.Generic.Confirm, () =>
			{
				NetworkService.ConfirmAction();
			}));
		}
		else
		{
			ActionsBox.SetInfoText("GUI_WAITING_PARTY_LEADER");
		}
		ActionsBox.SetButtons(list.ToArray());
	}
}
