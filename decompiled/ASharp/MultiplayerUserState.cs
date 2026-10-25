using System.Collections.Generic;
using FFSNet;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerUserState : UIMultiplayerUser
{
	[SerializeField]
	private CanvasGroup avatarCanvasGroup;

	[SerializeField]
	private List<Graphic> statusGraphics;

	[SerializeField]
	private TextLocalizedListener statusText;

	[SerializeField]
	private float connectingFade = 0.5f;

	[SerializeField]
	private bool hideConnectionStatusForClient;

	[SerializeField]
	private LoopAnimator connectingAnimation;

	private bool? isOnline;

	public void Show(NetworkPlayer player, bool isOnline)
	{
		base.Show(player);
		RefreshOnline(isOnline);
	}

	public override void Show(NetworkPlayer player)
	{
		Show(player, isOnline: true);
	}

	public override void Hide()
	{
		base.Hide();
		CleanState();
	}

	public void RefreshOnline(bool isOnline)
	{
		if (this.isOnline == isOnline)
		{
			return;
		}
		this.isOnline = isOnline;
		if (hideConnectionStatusForClient && player.IsClient)
		{
			if (avatarCanvasGroup != null)
			{
				avatarCanvasGroup.alpha = 1f;
			}
			username.SetAlpha(1f);
			for (int i = 0; i < statusGraphics.Count; i++)
			{
				statusGraphics[i].SetAlpha(0f);
			}
			return;
		}
		Color color = (isOnline ? UIInfoTools.Instance.playerOnlineColor : UIInfoTools.Instance.playerConnectingColor);
		for (int j = 0; j < statusGraphics.Count; j++)
		{
			statusGraphics[j].color = color;
		}
		if ((bool)username)
		{
			username.SetAlpha(isOnline ? 1f : connectingFade);
		}
		if (avatarCanvasGroup != null)
		{
			avatarCanvasGroup.alpha = (isOnline ? 1f : connectingFade);
		}
		if (statusText != null)
		{
			statusText.SetTextKey(isOnline ? "GUI_PLAYER_ONLINE" : "GUI_PLAYER_CONNECTING");
		}
		if (connectingAnimation != null)
		{
			if (isOnline)
			{
				connectingAnimation.StopLoop(resetToInitial: true);
			}
			else
			{
				connectingAnimation.StartLoop();
			}
		}
	}

	private void CancelConnectingAnimation()
	{
		if (connectingAnimation != null)
		{
			connectingAnimation.StopLoop(resetToInitial: true);
		}
	}

	public void CleanState()
	{
		isOnline = null;
		CancelConnectingAnimation();
	}

	private void OnDisable()
	{
		CancelConnectingAnimation();
	}
}
