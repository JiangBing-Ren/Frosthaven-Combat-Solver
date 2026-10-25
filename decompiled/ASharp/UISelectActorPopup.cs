using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using FHGUI.Common;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;

public class UISelectActorPopup : MonoBehaviour
{
	public class Params
	{
		public IDistributePointsService Service;

		public Action<IDistributePointsActor> OnSelectActor;

		public Action<IDistributePointsActor, bool> OnHovered;

		public Action<IDistributePointsActor> OnConfirm;

		public string TitleText;

		public string DescriptionText;

		public string HintText;

		public string ButtonTextConfirm;

		public string ButtonTextCancel;

		public bool Interactable;
	}

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[SerializeField]
	private TextMeshProUGUI hintText;

	[SerializeField]
	private FHButton confirmButton;

	[SerializeField]
	private UISelectActorSlot slotPrefab;

	[SerializeField]
	private RectTransform slotsContainer;

	[SerializeField]
	private UIAbilityCardContainer abilityCardContainer;

	private List<UISelectActorSlot> slots = new List<UISelectActorSlot>();

	private Dictionary<IDistributePointsActor, UISelectActorSlot> assignedSlots = new Dictionary<IDistributePointsActor, UISelectActorSlot>();

	private IDistributePointsActor Selected;

	private Params Data;

	public bool IsShown => gameObject.activeSelf;

	private void Awake()
	{
		confirmButton.onClick.AddListener(HandleOnConfirm);
	}

	public void Show(Params @params)
	{
		Data = @params;
		gameObject.SetActive(value: true);
		if (Data.Service is DistributeSelectPlayerActorService distributeSelectPlayerActorService)
		{
			distributeSelectPlayerActorService.ResetPoints(networkAction: false);
		}
		Selected = null;
		Refresh();
		Data.OnSelectActor?.Invoke(null);
	}

	public void Refresh()
	{
		if (!IsShown || Data == null)
		{
			return;
		}
		if ((bool)titleText)
		{
			titleText.text = Data.TitleText;
		}
		if ((bool)descriptionText)
		{
			descriptionText.text = Data.DescriptionText;
		}
		if ((bool)hintText)
		{
			hintText.text = Data.HintText;
		}
		if ((bool)confirmButton)
		{
			confirmButton.interactable = Data.Interactable;
			confirmButton.SetNonLocalizedText((Selected != null) ? Data.ButtonTextConfirm : Data.ButtonTextCancel);
		}
		List<IDistributePointsActor> actors = Data.Service.GetActors();
		HelperTools.NormalizePool(ref slots, (slotPrefab == null) ? slots[0].gameObject : slotPrefab.gameObject, slotsContainer, actors.Count);
		assignedSlots.Clear();
		for (int i = 0; i < actors.Count; i++)
		{
			UISelectActorSlot uISelectActorSlot = slots[i];
			IDistributePointsActor actor = actors[i];
			uISelectActorSlot.Init(actor, i, !FFSNetwork.IsOnline || FFSNetwork.IsHost, OnSelected, (bool hovered) =>
			{
				OnHoveredSlot(actor, hovered);
			}, actor == Selected);
			assignedSlots[actor] = uISelectActorSlot;
		}
	}

	public void Hide()
	{
		Data = null;
		gameObject.SetActive(value: false);
	}

	private void OnHoveredSlot(IDistributePointsActor actor, bool hovered)
	{
		if (IsShown)
		{
			Data.OnHovered?.Invoke(actor, hovered);
		}
		abilityCardContainer.gameObject.SetActive(hovered);
		if (hovered)
		{
			List<CAbilityCard> cards = actor.Actor.CharacterClass.HandAbilityCards.Concat(actor.Actor.CharacterClass.DiscardedAbilityCards).Concat(actor.Actor.CharacterClass.PermanentlyLostAbilityCards).ToList();
			abilityCardContainer.Init(cards, actor.Actor as CPlayerActor);
		}
	}

	private void OnSelected(IDistributePointsActor actor)
	{
		if (Data.Service is DistributeSelectPlayerActorService distributeSelectPlayerActorService)
		{
			if (actor == null)
			{
				distributeSelectPlayerActorService.ResetPoints();
			}
			else
			{
				distributeSelectPlayerActorService.AddPoint(actor);
			}
		}
		Selected = actor;
		Refresh();
		Data.OnSelectActor?.Invoke(actor);
	}

	private void HandleOnConfirm()
	{
		Data.OnConfirm?.Invoke(Selected);
	}

	private void OnDisable()
	{
		abilityCardContainer.Clear();
	}

	public void ProxySelectActor(GameAction gameAction)
	{
		IDistributePointsActor actor = Data.Service.GetActor(gameAction);
		if (actor != null && assignedSlots.ContainsKey(actor))
		{
			Selected = (gameAction.SupplementaryDataBoolean ? actor : null);
			Refresh();
			return;
		}
		throw new Exception("Error redistributing damage. Actor returns null (Actor ID: " + gameAction.ActorID + ").");
	}
}
