using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIOptionPicker : MonoBehaviour
{
	[SerializeField]
	private UIPickerSlot optionPrefab;

	[SerializeField]
	private List<UIPickerSlot> optionButtons;

	[SerializeField]
	private GameObject content;

	private Action<List<IOption>, bool> onClickCallback;

	private Action<IOption, bool> onClickElement;

	private int elementsToPick;

	private List<IOption> pickedElements;

	private Dictionary<string, UIPickerSlot> slots = new Dictionary<string, UIPickerSlot>();

	private Action<IOption, bool> onHovered;

	private Action onOpenPicker;

	private Action onClosePicker;

	public bool IsOpen => content.gameObject.activeSelf;

	public void Init(List<IOption> options, Action<List<IOption>, bool> onClick, int elementsToPick = 1, Action<IOption, bool> onClickElement = null, Action onShow = null, Action onHide = null, Action<IOption, bool> onHovered = null)
	{
		onClosePicker = onHide;
		onOpenPicker = onShow;
		onClickCallback = onClick;
		this.onClickElement = onClickElement;
		this.elementsToPick = elementsToPick;
		this.onHovered = onHovered;
		pickedElements = new List<IOption>();
		slots.Clear();
		HelperTools.NormalizePool(ref optionButtons, optionPrefab.gameObject, optionButtons[0].transform.parent, options.Count);
		for (int i = 0; i < options.Count; i++)
		{
			IOption option = options[i];
			slots[option.ID] = optionButtons[i];
			optionButtons[i].Init(option.GetPickerText(), option.GetPickerIcon(), () =>
			{
				Select(option);
			}, (bool hovered) =>
			{
				onHovered?.Invoke(option, hovered);
			});
		}
	}

	public void Show(GameObject holder = null)
	{
		pickedElements.Clear();
		if (!content.activeSelf)
		{
			content.SetActive(value: true);
			onOpenPicker?.Invoke();
			if (holder != null)
			{
				UIManager.Instance.HighlightElement(holder, fadeEverythingElse: false, lockUI: false);
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

	private void OnDisable()
	{
		if (!GloomUtility.IsAppQuitting)
		{
			pickedElements.Clear();
		}
	}

	public void Select(IOption option)
	{
		if (pickedElements.Contains(option))
		{
			pickedElements.Remove(option);
			slots[option.ID].SetSelected(selected: false);
			onClickElement?.Invoke(option, arg2: false);
			if (elementsToPick < 0)
			{
				onClickCallback(pickedElements.ToList(), arg2: true);
			}
		}
		else
		{
			slots[option.ID].SetSelected(selected: true);
			pickedElements.Add(option);
			onClickElement?.Invoke(option, arg2: true);
			if (elementsToPick < 0 || pickedElements.Count == elementsToPick)
			{
				onClickCallback(pickedElements.ToList(), arg2: false);
			}
		}
	}
}
