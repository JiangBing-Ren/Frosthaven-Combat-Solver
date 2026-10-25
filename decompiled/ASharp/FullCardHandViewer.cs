using System;
using System.Collections.Generic;
using System.Linq;
using DevLocker.GFrame.Input.UIScope;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FullCardHandViewer : Singleton<FullCardHandViewer>
{
	[SerializeField]
	private ScrollRect cardScroll;

	[SerializeField]
	private CanvasGroup cardsInteraction;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private Button closeBackgroundButton;

	[SerializeField]
	private SelectionControllerScopeElement _selectionController;

	[SerializeField]
	private UINavigationGroup _navigationGroup;

	public Vector3 FullCardScaleFactor = new Vector3(0.792f, 0.792f, 0.792f);

	private Action onClose;

	public RectTransform CardContainer => cardScroll.content;

	protected override void AwakeInit()
	{
		closeButton.onClick.AddListener(Hide);
		closeBackgroundButton.onClick.AddListener(Hide);
	}

	public void Hide()
	{
		if (gameObject.activeSelf)
		{
			gameObject.SetActive(value: false);
			ClearNavigation();
			onClose?.Invoke();
		}
	}

	public void Show(Action onClose = null)
	{
		this.onClose = onClose;
		RefreshNavigation();
		cardScroll.ScrollToTop();
		gameObject.SetActive(value: true);
	}

	public void SetCardsInteractable(bool interactable)
	{
		cardsInteraction.interactable = interactable;
	}

	private void ClearNavigation()
	{
		_selectionController.StartSelections.Clear();
		_navigationGroup.Exclude.Clear();
		_navigationGroup.RefreshNavigationClearCaches();
	}

	public void RefreshNavigation()
	{
		ClearNavigation();
		AbilityCardUI[] componentsInChildren = GetComponentsInChildren<AbilityCardUI>(includeInactive: true);
		if (componentsInChildren.Length == 0)
		{
			return;
		}
		IEnumerable<Selectable> collection = componentsInChildren.SelectMany((AbilityCardUI cardUi) => from s in cardUi.GetComponentsInChildren<Selectable>()
			where s != cardUi.Selectable
			select s);
		_navigationGroup.Exclude.AddRange(collection);
		_selectionController.StartSelections = new List<Selectable> { componentsInChildren[0].Selectable };
		if (!(EventSystem.current.currentSelectedGameObject == null))
		{
			AbilityCardUI componentInParent = EventSystem.current.currentSelectedGameObject.GetComponentInParent<AbilityCardUI>();
			if (!(componentInParent == null))
			{
				_selectionController.StartSelections = new List<Selectable> { componentInParent.Selectable };
			}
		}
	}
}
