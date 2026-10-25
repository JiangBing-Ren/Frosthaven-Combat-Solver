using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class UIElementPicker : MonoBehaviour
{
	[SerializeField]
	private UIElementPickerSlot[] elementButtons;

	[SerializeField]
	private GameObject content;

	private Action<List<ElementInfusionBoardManager.EElement>> onClickCallback;

	private Action<ElementInfusionBoardManager.EElement, bool> onClickElement;

	private List<ElementInfusionBoardManager.EElement> excludeElements;

	private bool isInfusion;

	private int elementsToPick;

	private List<ElementInfusionBoardManager.EElement> pickedElements;

	private Action onOpenPicker;

	private Action onClosePicker;

	public bool IsOpen => content.gameObject.activeSelf;

	public void Init(Action<ElementInfusionBoardManager.EElement> onClick, List<ElementInfusionBoardManager.EElement> excludeElements = null, bool isInfusion = false)
	{
		Init((List<ElementInfusionBoardManager.EElement> elements) =>
		{
			onClick(elements[0]);
		}, excludeElements, isInfusion);
	}

	public void Init(Action<List<ElementInfusionBoardManager.EElement>> onClick, List<ElementInfusionBoardManager.EElement> excludeElements = null, bool isInfusion = false, int elementsToPick = 1, Action<ElementInfusionBoardManager.EElement, bool> onClickElement = null, Action onShow = null, Action onHide = null, bool saveInfusionsState = true)
	{
		if (saveInfusionsState)
		{
			ElementInfusionBoardManager.SaveState();
			InfusionBoardUI.Instance?.SaveState();
		}
		onClosePicker = onHide;
		onOpenPicker = onShow;
		this.isInfusion = isInfusion;
		onClickCallback = onClick;
		this.onClickElement = onClickElement;
		this.excludeElements = excludeElements;
		this.elementsToPick = elementsToPick;
		pickedElements = new List<ElementInfusionBoardManager.EElement>();
		RefreshAvailable();
	}

	public void SetInteractable(ElementInfusionBoardManager.EElement element, bool interactable)
	{
		elementButtons[(int)element].SetInteractable(interactable && (!FFSNetwork.IsOnline || Choreographer.s_Choreographer.m_CurrentActor.IsUnderMyControl()));
	}

	public Selectable GetFirstSelectable()
	{
		return elementButtons.FirstOrDefault((UIElementPickerSlot it) => it.gameObject.activeSelf)?.Selectable;
	}

	public void Show(GameObject holder = null)
	{
		pickedElements.Clear();
		RefreshAvailable();
		if (!content.activeSelf)
		{
			Log.GUIInfo("show picker");
			content.SetActive(value: true);
			onOpenPicker?.Invoke();
			if (holder != null)
			{
				UIManager.Instance.HighlightElement(holder, fadeEverythingElse: false, lockUI: false);
			}
		}
	}

	public void RefreshAvailable()
	{
		for (int i = 0; i < elementButtons.Length; i++)
		{
			bool flag = (excludeElements == null || !excludeElements.Contains((ElementInfusionBoardManager.EElement)i)) && (isInfusion || (ElementInfusionBoardManager.ElementColumn((ElementInfusionBoardManager.EElement)i, Choreographer.s_Choreographer.CurrentPlayerActor) != ElementInfusionBoardManager.EColumn.Inert && !(InfusionBoardUI.Instance?.IsElementReserved((ElementInfusionBoardManager.EElement)i) ?? false)));
			elementButtons[i].SetInteractable(!flag || Choreographer.s_Choreographer.ThisPlayerHasTurnControl);
			if (flag)
			{
				elementButtons[i].Show(Select);
			}
			else
			{
				elementButtons[i].Hide();
			}
		}
	}

	public void Hide(GameObject holder)
	{
		if (content.activeSelf)
		{
			Hide();
			if (holder != null)
			{
				UIManager.Instance.UnhighlightElement(holder, unlockUI: false);
			}
		}
	}

	public void Hide()
	{
		if (content.activeSelf)
		{
			content.SetActive(value: false);
			onClosePicker?.Invoke();
		}
	}

	protected void OnDisable()
	{
		pickedElements.Clear();
	}

	public void Select(ElementInfusionBoardManager.EElement element)
	{
		if (pickedElements.Contains(element))
		{
			pickedElements.Remove(element);
			elementButtons[(int)element].SetSelected(selected: false);
			onClickElement?.Invoke(element, arg2: false);
			return;
		}
		elementButtons[(int)element].SetSelected(selected: true);
		pickedElements.Add(element);
		onClickElement?.Invoke(element, arg2: true);
		if (pickedElements.Count == elementsToPick)
		{
			onClickCallback(pickedElements.ToList());
		}
	}
}
