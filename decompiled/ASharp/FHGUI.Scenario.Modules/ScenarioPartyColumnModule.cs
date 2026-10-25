using System;
using System.Collections.Generic;
using FHGUI.Elements;
using FHGUI.Modules;
using FHGUI.Scenario.DTO;
using FHGUI.Scenario.Elements;
using UnityEngine;

namespace FHGUI.Scenario.Modules;

public class ScenarioPartyColumnModule : ListContainerBase<ScenarioPartySlotDetailsElement, ScenarioPartyColumnModule.ListParams, ScenarioPartySlotDetailsDTO>, IUIViewModule
{
	public struct ListParams
	{
		public Action OnViewPartyClick;

		public UIPartySlot.SlotToHintedActions HintedActions;

		public bool IsHintsContainerShown;
	}

	[Header("Elements")]
	[SerializeField]
	private FHButton _viewPartyButton;

	[SerializeField]
	private GameObject _hintsContainer;

	private UIPartySlot.SlotToHintedActions _hintedActions;

	private UIPartySlotInteractionEvents _interactionEvents;

	private Action _onViewPartyClick;

	public GameObject GameObject => gameObject;

	private void Awake()
	{
		_viewPartyButton.onClick.AddListener(() =>
		{
			_onViewPartyClick?.Invoke();
		});
	}

	protected override void InitList(ListParams listParams, IList<ScenarioPartySlotDetailsDTO> listElementParams)
	{
		base.InitList(listParams, listElementParams);
		_onViewPartyClick = listParams.OnViewPartyClick;
		_hintedActions = listParams.HintedActions;
		_viewPartyButton.gameObject.SetActive(_onViewPartyClick != null);
		_hintsContainer.SetActive(listParams.IsHintsContainerShown && listParams.HintedActions != null);
	}

	protected override void InitElement(ScenarioPartySlotDetailsElement listElement, ScenarioPartySlotDetailsDTO data, ListParams listParams)
	{
		ScenarioPartySlotDetailsElement scenarioPartySlotDetailsElement = m_ListElements[data.PartySlotDto.SlotId];
		if (!scenarioPartySlotDetailsElement.gameObject.activeSelf)
		{
			Log.GUIError($"Setting data for {data.Character} on slot {data.PartySlotDto.SlotId} that is not active!", this);
		}
		scenarioPartySlotDetailsElement.Init(data, _hintedActions?.Invoke(data.PartySlotDto), _interactionEvents);
	}
}
