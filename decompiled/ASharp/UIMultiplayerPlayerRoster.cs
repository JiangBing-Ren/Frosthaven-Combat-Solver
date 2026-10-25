using System;
using System.Collections.Generic;
using FFSNet;
using FHGUI;
using UnityEngine;

public class UIMultiplayerPlayerRoster : MonoBehaviour
{
	public struct Params
	{
		public List<NetworkPlayer> Players;

		public Action OnInvite;

		public Action<NetworkPlayer> OnKick;
	}

	[Header("Components")]
	[SerializeField]
	private List<UIMultiplayerPlayerSlot> slots;

	public Params Data { get; private set; }

	public void Init(Params @params)
	{
		Data = @params;
		for (int i = 0; i < 4; i++)
		{
			NetworkPlayer networkPlayer = ((@params.Players != null && @params.Players.Count > i) ? @params.Players[i] : null);
			slots[i].Init(new UIMultiplayerPlayerSlot.Params
			{
				Player = networkPlayer,
				Actions = GetInteractions(networkPlayer),
				IsPrimaryActionPositive = (networkPlayer == null)
			});
		}
		IEnumerable<FHInputEventContainer.Params> GetInteractions(NetworkPlayer player)
		{
			if (player == null)
			{
				yield return new FHInputEventContainer.Params
				{
					ActionIcon = null,
					HitLocalizedOverride = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick, "GUI_CONFIRMATION_INVITE_FRIENDS_TITLE"),
					InteractionType = FHInputEventElement.UserInteractionType.LeftClick,
					ActionCallback = () =>
					{
						Data.OnInvite?.Invoke();
					}
				};
			}
			if (player != null && player.IsClient)
			{
				yield return new FHInputEventContainer.Params
				{
					ActionIcon = null,
					HitLocalizedOverride = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.RightClick, "GUI_MULTIPLAYER_KICK_PLAYER_TITLE"),
					InteractionType = FHInputEventElement.UserInteractionType.RightClick,
					ActionCallback = () =>
					{
						Data.OnKick?.Invoke(player);
					}
				};
			}
		}
	}
}
