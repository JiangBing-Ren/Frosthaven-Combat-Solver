using System;
using System.Collections.Generic;
using System.Linq;
using DevLocker.GFrame.Input.UIScope;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class CardsHandPreviewWindow : Singleton<CardsHandPreviewWindow>
{
	[SerializeField]
	private CardsHandPreviewUI previewPrefab;

	[SerializeField]
	private RectTransform previewsContainer;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private Button closeBackgroundButton;

	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private SelectionControllerScopeElement _selectionController;

	[Header("Settings")]
	[SerializeField]
	private int showFullPreviewOnLeftSizeThreshold = 3;

	private List<CardsHandPreviewUI> previews = new List<CardsHandPreviewUI>();

	private Action onCancel;

	private int currentHandFocused;

	public bool IsOpenByKey { get; private set; }

	protected override void AwakeInit()
	{
		closeButton.onClick.AddListener(Cancel);
		window.onHidden.AddListener(OnHidden);
	}

	protected override void AwakenedDestroy()
	{
		base.AwakenedDestroy();
		closeButton.onClick.RemoveAllListeners();
		closeBackgroundButton.onClick.RemoveAllListeners();
		window.onHidden.RemoveAllListeners();
		onCancel = null;
		previews.Clear();
		_selectionController.StartSelections.Clear();
		_selectionController.StartNavigationGroups.Clear();
	}

	public void Reset()
	{
		foreach (CardsHandPreviewUI preview in previews)
		{
			preview.RecycleCards();
			UnityEngine.Object.Destroy(preview.gameObject);
		}
		previews.Clear();
	}

	public void CreatePreview(CardsHandUI hand)
	{
		if (!hand.PlayerActor.IsDead)
		{
			CardsHandPreviewUI cardsHandPreviewUI = UnityEngine.Object.Instantiate(previewPrefab, previewsContainer);
			previews.Add(cardsHandPreviewUI);
			cardsHandPreviewUI.transform.SetAsFirstSibling();
			cardsHandPreviewUI.Init(hand, previews.Count > showFullPreviewOnLeftSizeThreshold);
			_selectionController.StartNavigationGroups.Add(cardsHandPreviewUI.CardsNavigationGroup);
		}
	}

	public void Show(bool cardsSelectable, Action onCancel = null, bool openByKey = false)
	{
		IsOpenByKey = openByKey;
		closeButton?.gameObject.SetActive(!openByKey);
		this.onCancel = onCancel;
		for (int i = 0; i < previews.Count; i++)
		{
			if (previews[i].PlayerActor.IsDead)
			{
				previews[i].Hide();
			}
			else
			{
				previews[i].Show(cardsSelectable && PhaseManager.IsInCardsSelectionPhase);
			}
		}
		window.Show();
		WireNavigation();
	}

	public void SetCardsSelectable(bool selectable)
	{
		if (!window.IsOpen)
		{
			return;
		}
		for (int i = 0; i < previews.Count; i++)
		{
			if (!previews[i].PlayerActor.IsDead)
			{
				previews[i].SetCardsSelectable(selectable && PhaseManager.IsInCardsSelectionPhase);
			}
		}
	}

	public void Cancel()
	{
		window.Hide();
	}

	public void Hide()
	{
		onCancel = null;
		window.Hide();
	}

	private void WireNavigation()
	{
		List<CardsHandPreviewUI> list = previews.Where((CardsHandPreviewUI p) => !p.PlayerActor.IsDead).ToList();
		if (list.Count != 0)
		{
			for (int num = 0; num < list.Count; num++)
			{
				int index = ((num > 0) ? (num - 1) : (list.Count - 1));
				int index2 = (num + 1) % list.Count;
				CardsHandPreviewUI cardsHandPreviewUI = list[index];
				CardsHandPreviewUI cardsHandPreviewUI2 = list[index2];
				list[num].InventoryNavigationGroup.WrapLeft.NavigationGroup = cardsHandPreviewUI.CardsNavigationGroup;
				list[num].InventoryNavigationGroup.WrapRight.NavigationGroup = list[num].CardsNavigationGroup;
				list[num].CardsNavigationGroup.WrapLeft.NavigationGroup = list[num].InventoryNavigationGroup;
				list[num].CardsNavigationGroup.WrapRight.NavigationGroup = cardsHandPreviewUI2.InventoryNavigationGroup;
			}
		}
	}

	private void OnHidden()
	{
		IsOpenByKey = false;
		for (int i = 0; i < previews.Count; i++)
		{
			previews[i].ResetView();
		}
		Singleton<HelpBox>.Instance.ClearOverrideController();
		onCancel?.Invoke();
	}

	public CardsHandPreviewUI FindHandPreview(CActor actor)
	{
		foreach (CardsHandPreviewUI preview in previews)
		{
			if (preview.PlayerActor == actor)
			{
				return preview;
			}
		}
		return null;
	}
}
