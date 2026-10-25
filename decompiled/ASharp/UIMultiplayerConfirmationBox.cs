using System;
using FFSNet;
using FHGUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMultiplayerConfirmationBox : MonoBehaviour
{
	[SerializeField]
	private UIWindow confirmationBox;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private FHButton confirmButton;

	[SerializeField]
	private FHButton cancelButton;

	[SerializeField]
	private TMP_Text informationText;

	public void ShowConfirmation(NetworkPlayer player, Action onActionConfirmed)
	{
		ResetConfirmationBox();
		icon.overrideSprite = player.Avatar;
		icon.color = UIInfoTools.GetPlaceholderPlayerColor(player);
		informationText.text = FHUILocFormatting.PopulateParams("GUI_MULTIPLAYER_KICK_PLAYER_DESCRIPTION", player.Username);
		confirmButton.onClick.AddListener(() =>
		{
			onActionConfirmed?.Invoke();
			Hide();
		});
		cancelButton.onClick.AddListener(Hide);
		confirmationBox.Show();
	}

	public void Hide()
	{
		confirmationBox.Hide();
		ResetConfirmationBox();
	}

	private void ResetConfirmationBox()
	{
		confirmButton.onClick.RemoveAllListeners();
		cancelButton.onClick.RemoveAllListeners();
		confirmationBox.onHidden.RemoveAllListeners();
		confirmationBox.onTransitionComplete.RemoveAllListeners();
	}
}
