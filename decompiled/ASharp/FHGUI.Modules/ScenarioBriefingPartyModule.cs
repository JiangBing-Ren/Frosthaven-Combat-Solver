using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Elements;
using UnityEngine;

namespace FHGUI.Modules;

public class ScenarioBriefingPartyModule : ListContainerBase<UIScenarioBriefingPartySlot, ScenarioBriefingPartyModule.ListParams, ScenarioBriefingPartyModule.BriefingSlotData>, IUIViewModule
{
	public class ListParams
	{
		public bool ShowSelectedGoal;

		public bool ShowGoalsList;

		public UIPartySlot.SlotEventHandler OnSlotClicked;

		public UIPartySlot.SlotToHintedActions SlotHintedActions;

		public UIScenarioBriefingPartySlot.BattleGoalSelectionHandler OnSelectedGoalClicked;

		public UIScenarioBriefingPartySlot.BattleGoalSelectionHandler OnGoalListElementClicked;
	}

	public class BriefingSlotData
	{
		public PartySlotDTO PartySlotDto;

		public GoalDTO SelectedGoalDto;

		public List<GoalDTO> GoalDtos;

		public IEnumerable<SpriteWithTooltipDTO> Warrnings;

		public string InfoLiteral;

		public BriefingSlotData Clone()
		{
			BriefingSlotData briefingSlotData = (BriefingSlotData)MemberwiseClone();
			briefingSlotData.GoalDtos = GoalDtos.ToList();
			briefingSlotData.Warrnings = Warrnings;
			return briefingSlotData;
		}
	}

	public bool ShowEmptySlots = true;

	public GameObject GameObject => gameObject;

	protected override void InitList(ListParams listParams, IList<BriefingSlotData> listElementParams)
	{
		if (!ShowEmptySlots && listElementParams != null)
		{
			for (int num = listElementParams.Count - 1; num >= 0; num--)
			{
				if (listElementParams[num].PartySlotDto == null || listElementParams[num].PartySlotDto.IsEmpty)
				{
					listElementParams.RemoveAt(num);
				}
			}
		}
		base.InitList(listParams, listElementParams);
	}

	protected override void InitElement(UIScenarioBriefingPartySlot partySlotComponent, BriefingSlotData slotData, ListParams listParams)
	{
		UIScenarioBriefingPartySlot.BriefingSlotParams slotParams = new UIScenarioBriefingPartySlot.BriefingSlotParams
		{
			PartySlotDto = slotData.PartySlotDto,
			OnSlotClicked = listParams.OnSlotClicked,
			SlotHintedActions = listParams.SlotHintedActions,
			IsGoalsModuleActive = listParams.ShowGoalsList,
			SelectedGoalDto = (listParams.ShowSelectedGoal ? slotData.SelectedGoalDto : null),
			OnSelectedGoalClicked = listParams.OnSelectedGoalClicked,
			InfoLiteral = slotData.InfoLiteral,
			BattleGoalDtos = slotData.GoalDtos,
			Warrnings = slotData.Warrnings,
			OnGoalListElementClicked = listParams.OnGoalListElementClicked
		};
		partySlotComponent.Init(slotParams);
	}
}
