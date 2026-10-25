using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GLOOM.MainMenu;

[RequireComponent(typeof(UIWindow))]
public class UIMainMenuSubptionsPanel : MonoBehaviour
{
	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private UIMainMenuSuboption optionButtonPrefab;

	[SerializeField]
	private ScrollRect optionsScroll;

	[SerializeField]
	private ToggleGroup toggleGroup;

	[SerializeField]
	private List<UIMainMenuSuboption> optionsPool;

	[SerializeField]
	private CanvasGroup canvasGroupInteraction;

	[SerializeField]
	private VerticalPointerUI verticalPointer;

	private Action onHidden;

	private void Awake()
	{
		window.onTransitionComplete.AddListener((UIWindow _, UIWindow.VisualState state) =>
		{
			if (state == UIWindow.VisualState.Hidden && window.HasGoneToStartingState)
			{
				onHidden();
			}
		});
	}

	public void SetInteractable(bool interactable)
	{
		if (window.IsOpen)
		{
			canvasGroupInteraction.blocksRaycasts = interactable;
		}
	}

	public void Show(List<MenuSuboption> options, RectTransform targetPointer, Action onHidden)
	{
		this.onHidden = onHidden;
		verticalPointer.PointAt(targetPointer);
		HelperTools.NormalizePool(ref optionsPool, optionButtonPrefab.gameObject, optionsScroll.content, options.Count);
		optionsScroll.ScrollToTop();
		for (int i = 0; i < options.Count; i++)
		{
			MenuSuboption option = options[i];
			UIMainMenuSuboption slot = optionsPool[i];
			slot.Init(option, toggleGroup, () =>
			{
				OnSelectSlot(option);
			}, () =>
			{
				OnDeselectSlot(option);
			}, option.OnHovered);
			option.RegisterOnUpdatedInteraction((bool interact) =>
			{
				slot.IsInteractable = interact;
			});
			option.RegisterOnToggledSelect(slot.ToggleSelect);
		}
		window.Show();
	}

	private void OnDeselectSlot(MenuSuboption option)
	{
		SetFocused(isFocused: true);
		option.OnDeselected();
	}

	private void OnSelectSlot(MenuSuboption option)
	{
		option.OnSelected();
		SetFocused(isFocused: false);
	}

	public void DeselectCurrentSlot()
	{
		toggleGroup.SetAllTogglesOff();
	}

	private void SetFocused(bool isFocused)
	{
		for (int i = 0; i < optionsPool.Count && optionsPool[i].gameObject.activeSelf; i++)
		{
			optionsPool[i].SetFocused(isFocused);
		}
	}

	public void Hide()
	{
		Hide(instant: true);
	}

	public void Hide(bool instant)
	{
		if (window.IsOpen)
		{
			DeselectCurrentSlot();
			window.Hide(instant);
		}
	}
}
