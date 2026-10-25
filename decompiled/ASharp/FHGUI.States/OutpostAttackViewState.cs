using System;
using FHGUI.Modules;
using FHGUI.Services;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class OutpostAttackViewState : FHMapViewState
{
	public Action OnAttackResolved;

	private int m_SelectedAttackStepIndex = -1;

	private int m_SelectedModifierIndex = -1;

	private bool m_AttackSoundPlayed;

	[Preserve]
	private MainHeaderModule MainHeader => View.Modules.MainHeaderModule;

	[Preserve]
	private OutpostAttackModalModule OutpostAttackModalModule => GetModule<OutpostAttackModalModule>();

	private HeaderViewService HeaderService => View.GetService<HeaderViewService>();

	private OutpostEventsService EventService => View.GetService<OutpostEventsService>();

	public override void OnShow()
	{
		if (!GameController.AttackController.CanPerformAttack)
		{
			PopThisState();
			OnAttackResolved?.Invoke();
			return;
		}
		if (OutpostAttackModalModule.InTransition)
		{
			OutpostAttackModalModule.OnTransitionFinished = () =>
			{
				View.RefreshView();
			};
			return;
		}
		OutpostAttackModalModule.Params obj = new OutpostAttackModalModule.Params
		{
			EventData = EventService.GetAttackEvent(EventService.GetCurrentUsedSoldiersCount()),
			HistoryData = EventService.GetAttackEventHistory(),
			TownGuardDeck = EventService.GetTownGuardDeck(),
			IsPartyLeader = View.IsPartyLeader,
			SoldiersToSpend = EventService.GetCurrentUsedSoldiersCount(),
			AvailableSoldiers = View.GameController.AttackController.AvailableSoliders,
			CanRepair = EventService.CanRepairDamagedBuilding(),
			ModifySoldiersActions = EventService.GetSolderInteractions(),
			HasOperationalBarracks = EventService.HasOperationalBarracks(),
			RepairBuildingWithMaterialsCallback = EventService.RepairBuildingWithMaterials,
			RepairBuildingWithMoraleCallback = EventService.RepairBuildingWithMorale,
			NextActionCallback = EventService.NextAttack,
			PerformAttackCallback = EventService.PerformAttack,
			OnModifierChoiceCallback = (int s) =>
			{
				m_SelectedModifierIndex = -1;
				EventService.SelectModifier(s);
			},
			OnModifierSelectedCallback = SelectModifier,
			SelectedModiferIndex = m_SelectedModifierIndex,
			SelectedStep = m_SelectedAttackStepIndex,
			OnSlotPicked = (int s) =>
			{
				m_SelectedAttackStepIndex = s;
			}
		};
		if (!m_AttackSoundPlayed)
		{
			m_AttackSoundPlayed = true;
			AudioControllerUtils.PlaySound(UIInfoTools.Instance?.outpostAttackAudioItem);
		}
		OutpostAttackModalModule.Init(obj);
		HeaderService.SetupHeader(MainHeader, HeaderViewService.HeaderFunctionality.None);
		View.OutpostSceneController.OpenOutpostMap();
	}

	private void SelectModifier(int modiferIndex)
	{
		m_SelectedModifierIndex = modiferIndex;
		View.RefreshView();
	}
}
