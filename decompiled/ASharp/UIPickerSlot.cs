using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPickerSlot : MonoBehaviour
{
	[SerializeField]
	private ExtendedButton button;

	[SerializeField]
	private Image highlightBackground;

	[SerializeField]
	private Sprite highlightedBackground;

	[SerializeField]
	private float highlightedBackgroundAlpha = 1f;

	[SerializeField]
	private Sprite unhighlightedBackground;

	[SerializeField]
	private float unhighlightedBackgroundAlpha = 0.4f;

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private Image image;

	private Action onSelected;

	private Action<bool> onHovered;

	private bool isHovered;

	private bool isSelected;

	public bool IsSelected => isSelected;

	public Selectable Selectable => button;

	private void Awake()
	{
		button.onClick.AddListener(OnClick);
		button.onMouseEnter.AddListener(() =>
		{
			OnHover(hover: true);
		});
		button.onMouseExit.AddListener(() =>
		{
			OnHover(hover: false);
		});
	}

	public void Init(string optionText = null, Sprite icon = null, Action onSelected = null, Action<bool> onHovered = null)
	{
		this.onSelected = onSelected;
		this.onHovered = onHovered;
		if (optionText.IsNOTNullOrEmpty() && icon == null)
		{
			text.text = optionText;
			text.enabled = true;
		}
		else
		{
			text.enabled = false;
		}
		if (image != null)
		{
			if (icon != null)
			{
				image.sprite = icon;
				image.enabled = true;
			}
			else
			{
				image.enabled = false;
			}
		}
		isHovered = false;
		SetSelected(selected: false);
		gameObject.SetActive(value: true);
	}

	public void SetInteractable(bool interactable)
	{
		button.interactable = interactable;
	}

	private void OnClick()
	{
		onSelected?.Invoke();
	}

	private void OnHover(bool hover)
	{
		isHovered = hover;
		RefreshHighlight();
		onHovered?.Invoke(hover);
	}

	public void SetSelected(bool selected)
	{
		isSelected = selected;
		RefreshHighlight();
	}

	private void RefreshHighlight()
	{
		bool flag = isHovered || isSelected;
		highlightBackground.sprite = (flag ? highlightedBackground : unhighlightedBackground);
		highlightBackground.SetAlpha(flag ? highlightedBackgroundAlpha : unhighlightedBackgroundAlpha);
	}

	public void Hide()
	{
		isSelected = false;
		isHovered = false;
		gameObject.SetActive(value: false);
	}
}
