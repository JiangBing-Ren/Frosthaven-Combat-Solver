using System;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UIItemConfirmationBox : Singleton<UIItemConfirmationBox>
{
	[SerializeField]
	private UIWindow confirmationBox;

	[SerializeField]
	private RectTransform pictureHolder;

	[SerializeField]
	private ExtendedButton confirmButton;

	[SerializeField]
	private ExtendedButton cancelButton;

	[SerializeField]
	private TMP_Text titleText;

	[SerializeField]
	private TMP_Text informationText;

	private ItemCardUI itemCardUI;

	protected override void AwakeInit()
	{
		ResetConfirmationBox();
	}

	public void ShowConfirmation(string title, string information, CItem item, Action onActionConfirmed, string confirmActionKey = null, string cancelActionKey = null, string audioConfirmation = "", Action onActionCancelled = null)
	{
		ResetConfirmationBox();
		if (audioConfirmation.IsNOTNullOrEmpty())
		{
			confirmButton.mouseDownAudioItem = audioConfirmation;
		}
		itemCardUI = ObjectPool.SpawnCard(item.ID, ObjectPool.ECardType.Item, pictureHolder, resetLocalScale: true, resetToMiddle: true).GetComponent<ItemCardUI>();
		itemCardUI.item = item;
		titleText.text = title;
		informationText.text = information;
		if (confirmActionKey != null)
		{
			confirmButton.TextLanguageKey = confirmActionKey;
		}
		if (cancelActionKey != null)
		{
			cancelButton.TextLanguageKey = cancelActionKey;
		}
		confirmButton.onClick.AddListener(() =>
		{
			confirmationBox.onTransitionComplete.RemoveAllListeners();
			confirmationBox.onTransitionComplete.AddListener((UIWindow window, UIWindow.VisualState state) =>
			{
				if (state == UIWindow.VisualState.Hidden)
				{
					OnHidden();
					onActionConfirmed?.Invoke();
				}
			});
			Hide();
		});
		cancelButton.onClick.AddListener(Hide);
		if (onActionCancelled != null)
		{
			confirmationBox.onTransitionComplete.AddListener((UIWindow window, UIWindow.VisualState state) =>
			{
				if (state == UIWindow.VisualState.Hidden)
				{
					OnHidden();
					onActionCancelled();
				}
			});
		}
		confirmationBox.Show();
	}

	public bool IsConfirmingItem(CItem item)
	{
		if (confirmationBox.IsOpen && itemCardUI != null)
		{
			return itemCardUI.item == item;
		}
		return false;
	}

	public void Hide()
	{
		confirmationBox.Hide();
	}

	private void ResetConfirmationBox()
	{
		if (itemCardUI != null)
		{
			ObjectPool.RecycleCard(itemCardUI.CardID, ObjectPool.ECardType.Item, itemCardUI.gameObject);
			itemCardUI = null;
		}
		confirmButton.TextLanguageKey = "YES";
		cancelButton.TextLanguageKey = "NO";
		confirmButton.mouseDownAudioItem = null;
		confirmButton.onClick.RemoveAllListeners();
		cancelButton.onClick.RemoveAllListeners();
		confirmationBox.onHidden.RemoveAllListeners();
		confirmationBox.onTransitionComplete.RemoveAllListeners();
		confirmationBox.onTransitionComplete.AddListener((UIWindow window, UIWindow.VisualState state) =>
		{
			if (state == UIWindow.VisualState.Hidden)
			{
				OnHidden();
			}
		});
	}

	private void OnHidden()
	{
		if (itemCardUI != null)
		{
			ObjectPool.RecycleCard(itemCardUI.CardID, ObjectPool.ECardType.Item, itemCardUI.gameObject);
			itemCardUI = null;
		}
	}
}
