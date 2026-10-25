using System;
using System.Collections.Generic;
using FFSNet;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDistributePointsPopup : Singleton<UIDistributePointsPopup>
{
	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private Image titleImage;

	[SerializeField]
	private UIDistributePointsSlot slotPrefab;

	[SerializeField]
	private RectTransform slotsContainer;

	[SerializeField]
	private List<UIDistributePointsSlot> slots = new List<UIDistributePointsSlot>();

	private Dictionary<IDistributePointsActor, UIDistributePointsSlot> assignedSlots = new Dictionary<IDistributePointsActor, UIDistributePointsSlot>();

	private IDistributePointsService service;

	private Action<IDistributePointsActor, int> onUpdatedPoints;

	private Action<IDistributePointsActor, bool> onHovered;

	public List<UIDistributePointsSlot> Slots => slots;

	public bool IsShown => gameObject.activeSelf;

	public void Show(IDistributePointsService service, Action<IDistributePointsActor, int> onUpdatedPoints = null, Action<IDistributePointsActor, bool> onHovered = null, string optionsText = null, bool isRewards = false)
	{
		this.service = service;
		this.onUpdatedPoints = onUpdatedPoints;
		this.onHovered = onHovered;
		titleImage.sprite = service.GetTitleIcon();
		List<IDistributePointsActor> actors = service.GetActors();
		HelperTools.NormalizePool(ref slots, (slotPrefab == null) ? slots[0].gameObject : slotPrefab.gameObject, slotsContainer, actors.Count);
		assignedSlots.Clear();
		for (int i = 0; i < actors.Count; i++)
		{
			UIDistributePointsSlot uIDistributePointsSlot = slots[i];
			IDistributePointsActor actor = actors[i];
			uIDistributePointsSlot.Init(actor, service.GetMaxPoints(actor), service.GetCurrentPoints(actor), () =>
			{
				AddHealthPoint(actor);
			}, () =>
			{
				RemoveHealthPoint(actor);
			}, (bool hovered) =>
			{
				OnHoveredSlot(actor, hovered);
			}, i, isRewards, optionsText);
			assignedSlots[actor] = uIDistributePointsSlot;
		}
		RefreshAssignedPoints();
		gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		service = null;
		gameObject.SetActive(value: false);
	}

	private void OnHoveredSlot(IDistributePointsActor actor, bool hovered)
	{
		if (IsShown)
		{
			onHovered?.Invoke(actor, hovered);
		}
	}

	private void RemoveHealthPoint(IDistributePointsActor actor)
	{
		service.RemovePoint(actor);
		Refresh();
		onUpdatedPoints?.Invoke(actor, service.GetAssignedPoints(actor));
	}

	private void AddHealthPoint(IDistributePointsActor actor)
	{
		service.AddPoint(actor);
		Refresh();
		onUpdatedPoints?.Invoke(actor, service.GetAssignedPoints(actor));
	}

	public void Refresh()
	{
		if (IsShown)
		{
			RefreshAssignedPoints();
		}
	}

	private void RefreshAssignedPoints()
	{
		titleText.text = service.GetTitleText();
		foreach (KeyValuePair<IDistributePointsActor, UIDistributePointsSlot> assignedSlot in assignedSlots)
		{
			assignedSlot.Value.RefreshAssignedPoints(service.GetAssignedPoints(assignedSlot.Key));
			bool flag = service.CanAddPointsTo(assignedSlot.Key);
			bool flag2 = service.CanRemovePointsFrom(assignedSlot.Key);
			assignedSlot.Value.EnableAddPoints(flag);
			assignedSlot.Value.EnableRemovePoints(flag2);
			if (assignedSlot.Value.Counter is UIDistributePointsToggle uIDistributePointsToggle)
			{
				uIDistributePointsToggle.Interactable = flag | flag2;
			}
		}
	}

	public void ProxyRedistributeHealth(GameAction gameAction)
	{
		IDistributePointsActor actor = service.GetActor(gameAction);
		if (actor != null && assignedSlots.ContainsKey(actor))
		{
			if (gameAction.SupplementaryDataBoolean)
			{
				AddHealthPoint(actor);
			}
			else
			{
				RemoveHealthPoint(actor);
			}
			return;
		}
		throw new Exception("Error redistributing damage. Actor returns null (Actor ID: " + gameAction.ActorID + ").");
	}
}
