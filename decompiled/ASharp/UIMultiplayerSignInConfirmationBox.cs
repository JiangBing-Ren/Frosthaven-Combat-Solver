using System;
using UnityEngine;
using UnityEngine.UI;

public class UIMultiplayerSignInConfirmationBox : Singleton<UIMultiplayerSignInConfirmationBox>
{
	[SerializeField]
	private Button confirmButton;

	[SerializeField]
	private Button cancelButton;

	[SerializeField]
	private UIWindow confirmationBox;

	protected override void AwakeInit()
	{
		cancelButton.onClick.AddListener(Hide);
	}

	public void ShowConfirmation(Action onActionConfirmed)
	{
		ResetConfirmationBox();
		gameObject.SetActive(value: true);
		confirmationBox.Show();
		confirmButton.onClick.AddListener(() =>
		{
			confirmationBox.onTransitionComplete.AddListener((UIWindow window, UIWindow.VisualState state) =>
			{
				if (state == UIWindow.VisualState.Hidden)
				{
					onActionConfirmed?.Invoke();
				}
			});
			Hide();
		});
		confirmationBox.onTransitionComplete.AddListener((UIWindow window, UIWindow.VisualState state) =>
		{
			if (state == UIWindow.VisualState.Hidden)
			{
				gameObject.SetActive(value: false);
			}
		});
	}

	public void Hide()
	{
		confirmationBox.Hide();
	}

	private void ResetConfirmationBox()
	{
		confirmButton.onClick.RemoveAllListeners();
		confirmationBox.onTransitionComplete.RemoveAllListeners();
	}
}
