using System;
using DG.Tweening;
using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMultiplayerInvitePlayerSlot : MonoBehaviour
{
	[Header("Highlight")]
	[SerializeField]
	private Image highlightImage;

	[SerializeField]
	private Color highlightColor;

	[SerializeField]
	private float unhighlightDuration = 0.1f;

	[Header("Components")]
	[SerializeField]
	private Image inviteMask;

	[SerializeField]
	private Image invitedMask;

	[SerializeField]
	protected ExtendedButton button;

	[SerializeField]
	protected TextMeshProUGUI username;

	[SerializeField]
	protected RawImage userAvatar;

	[SerializeField]
	protected TextMeshProUGUI statusText;

	[SerializeField]
	protected Graphic[] statusGraphics;

	[SerializeField]
	private UITextTooltipTarget tooltip;

	private Action onSelected;

	private Tween unhighlightAnimation;

	protected virtual void Awake()
	{
		button.onClick.AddListener(() =>
		{
			onSelected?.Invoke();
		});
		button.onMouseEnter.AddListener(() =>
		{
			RefreshHighlight(isHighlighted: true);
		});
		button.onMouseExit.AddListener(() =>
		{
			RefreshHighlight(isHighlighted: false);
		});
	}

	public void SetPlayer(IInvitePlayer player, Action onSelected)
	{
		this.onSelected = onSelected;
		username.text = player.Username;
		userAvatar.texture = player.Avatar;
		TextMeshProUGUI textMeshProUGUI = statusText;
		string text;
		if (player.IsOnline)
		{
			text = (player.CurrentlyPlayingGameName.IsNullOrEmpty() ? LocalizationManager.GetTranslation("GUI_PLAYER_ONLINE") : (LocalizationManager.GetTranslation("GUI_PLAYING_GAME") + " " + player.CurrentlyPlayingGameName));
		}
		else
		{
			text = LocalizationManager.GetTranslation("GUI_PLAYER_OFFLINE");
		}
		textMeshProUGUI.text = text;
		Color color = (player.IsOnline ? UIInfoTools.Instance.playerOnlineColor : UIInfoTools.Instance.playerConnectingColor);
		for (int i = 0; i < statusGraphics.Length; i++)
		{
			statusGraphics[i].color = color;
		}
		SetInvited(player.IsInvited);
	}

	private void RefreshHighlight(bool isHighlighted)
	{
		if (isHighlighted)
		{
			CancelHighlightAnimations();
			highlightImage.color = highlightColor;
		}
		else if (unhighlightAnimation == null)
		{
			unhighlightAnimation = highlightImage.DOFade(0f, unhighlightDuration).SetUpdate(isIndependentUpdate: true).OnComplete(() =>
			{
				unhighlightAnimation = null;
			});
		}
	}

	private void CancelHighlightAnimations()
	{
		if (unhighlightAnimation != null)
		{
			unhighlightAnimation.Kill();
			unhighlightAnimation = null;
			highlightImage.SetAlpha(0f);
		}
	}

	protected void OnDisable()
	{
		if (!GloomUtility.IsAppQuitting)
		{
			CancelHighlightAnimations();
		}
	}

	public void SetInvited(bool selected)
	{
		invitedMask.enabled = selected;
		inviteMask.enabled = !selected;
		tooltip.SetText(LocalizationManager.GetTranslation(selected ? "GUI_MP_ALREADY_INVITED" : "GUI_MP_SEND_INVITE"), refreshTooltip: true);
	}
}
