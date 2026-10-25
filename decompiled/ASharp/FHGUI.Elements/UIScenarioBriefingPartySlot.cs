using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using UnityEngine;

namespace FHGUI.Elements;

public class UIScenarioBriefingPartySlot : MonoBehaviour
{
	public delegate void BattleGoalSelectionHandler(PartySlotDTO partySlotDto, GoalDTO goalDto);

	public class BriefingSlotParams
	{
		[Header("Character Slot")]
		public PartySlotDTO PartySlotDto;

		public UIPartySlot.SlotEventHandler OnSlotClicked;

		public UIPartySlot.SlotToHintedActions SlotHintedActions;

		[Header("Battle Goals")]
		public bool IsGoalsModuleActive;

		public GoalDTO SelectedGoalDto;

		public BattleGoalSelectionHandler OnSelectedGoalClicked;

		public string InfoLiteral;

		public List<GoalDTO> BattleGoalDtos;

		public BattleGoalSelectionHandler OnGoalListElementClicked;

		public IEnumerable<SpriteWithTooltipDTO> Warrnings;
	}

	[SerializeField]
	public UIPartySlot m_PartySlot;

	[SerializeField]
	private UIGoalSummary m_SelectedGoalSummary;

	[SerializeField]
	public UIGoalListContainer m_GoalListContainer;

	[SerializeField]
	private IconListContainer m_SetupWarnings;

	public void Init(BriefingSlotParams slotParams)
	{
		m_PartySlot.Init(slotParams.PartySlotDto, slotParams.OnSlotClicked, slotParams.SlotHintedActions?.Invoke(slotParams.PartySlotDto));
		if (slotParams.SelectedGoalDto != null)
		{
			m_SelectedGoalSummary.gameObject.SetActive(value: true);
			UIGoalSummary.Params obj = new UIGoalSummary.Params
			{
				DTO = slotParams.SelectedGoalDto,
				OnClickedCallback = () =>
				{
					slotParams.OnSelectedGoalClicked?.Invoke(slotParams.PartySlotDto, slotParams.SelectedGoalDto);
				},
				HintsLiteral = FHUILocFormatting.GetActionIconFormatLiteral(FHInputEventElement.UserInteractionType.LeftClick, "GUI_EDIT")
			};
			m_SelectedGoalSummary.Init(obj);
		}
		else
		{
			m_SelectedGoalSummary.gameObject.SetActive(value: false);
			m_SelectedGoalSummary.Clear();
		}
		m_GoalListContainer.gameObject.SetActive(slotParams.IsGoalsModuleActive);
		if (slotParams.IsGoalsModuleActive)
		{
			UIGoalListContainer.GoalListParams listParams = new UIGoalListContainer.GoalListParams
			{
				InfoLiteral = slotParams.InfoLiteral,
				OnListElementClicked = (GoalDTO goal) =>
				{
					slotParams.OnGoalListElementClicked?.Invoke(slotParams.PartySlotDto, goal);
				},
				InteractionHints = FHUILocFormatting.GetActionIconFormatLiteral(FHInputEventElement.UserInteractionType.LeftClick, "GUI_SELECT")
			};
			m_GoalListContainer.Init(listParams, slotParams.BattleGoalDtos);
			m_SetupWarnings?.Init(null, slotParams.Warrnings.ToList());
		}
	}
}
