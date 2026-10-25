using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMultiplayerInvitePlayersWindow : MonoBehaviour
{
	[SerializeField]
	private ScrollRect scroll;

	[SerializeField]
	private UIMultiplayerInvitePlayerSlot slotPrefab;

	[SerializeField]
	private VerticalPointerUI pointerTo;

	[SerializeField]
	private List<UIMultiplayerInvitePlayerSlot> slots;

	[SerializeField]
	private UIWindow window;

	private Dictionary<IInvitePlayer, UIMultiplayerInvitePlayerSlot> assignedSlots = new Dictionary<IInvitePlayer, UIMultiplayerInvitePlayerSlot>();

	private INetworkInviteService inviteService = new DummyNetworkInviteService();

	private Action onClosed;

	private void Awake()
	{
		window.onHidden.AddListener(() =>
		{
			gameObject.SetActive(value: false);
			onClosed?.Invoke();
		});
	}

	public void Show(Action onClosed, RectTransform pointTo)
	{
		this.onClosed = onClosed;
		pointerTo.PointAt(pointTo);
		if (inviteService == null)
		{
			inviteService = new DummyNetworkInviteService();
		}
		Decorate(inviteService.GetPlayersToInvite());
		scroll.ScrollToTop();
		gameObject.SetActive(value: true);
		window.Show();
	}

	public void Hide()
	{
		assignedSlots.Clear();
		window.Hide();
	}

	private void Decorate(List<IInvitePlayer> invitePlayers)
	{
		assignedSlots.Clear();
		HelperTools.NormalizePool(ref slots, slotPrefab.gameObject, scroll.content, invitePlayers.Count);
		for (int i = 0; i < invitePlayers.Count; i++)
		{
			IInvitePlayer player = invitePlayers[i];
			slots[i].SetPlayer(player, () =>
			{
				InvitePlayer(player);
			});
			assignedSlots[player] = slots[i];
		}
	}

	private void InvitePlayer(IInvitePlayer player)
	{
		inviteService.SendInvite(player).Done(() =>
		{
			if (window.IsOpen)
			{
				assignedSlots[player].SetInvited(selected: true);
			}
		});
	}
}
