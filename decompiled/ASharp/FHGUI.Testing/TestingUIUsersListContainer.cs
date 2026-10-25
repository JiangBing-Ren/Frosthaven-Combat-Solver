using System.Collections.Generic;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Modules;
using UnityEngine;
using UnityEngine.Serialization;

namespace FHGUI.Testing;

public class TestingUIUsersListContainer : MonoBehaviour
{
	[FormerlySerializedAs("PetsListContainer")]
	public UIScenarioBriefingSuggestibleList suggestibleListContainer;

	public List<SuggestibleSlotDTO> PetDtos = new List<SuggestibleSlotDTO>();

	public UIMultiplayerUser.Data UserData;

	private UIScenarioBriefingSuggestibleList.ListParams _listParams;

	private UISuggestibleSlotInteractionEvents _interactionEvents;

	private void Start()
	{
		_interactionEvents = new UISuggestibleSlotInteractionEvents
		{
			OnHover = null,
			OnClick = OnClick,
			OnDoubleClick = null,
			OnRightClick = null
		};
		_listParams = new UIScenarioBriefingSuggestibleList.ListParams
		{
			InteractionEvents = _interactionEvents
		};
		Refresh();
	}

	private void OnClick(SuggestibleSlotDTO target)
	{
		target.IsSelected = !target.IsSelected;
		target.SuggestingUsers = (target.IsSelected ? new List<UIMultiplayerUser.Data> { UserData } : null);
		Refresh();
	}

	[ContextMenu("Refresh")]
	private void Refresh()
	{
		suggestibleListContainer.Init(_listParams, PetDtos);
	}
}
