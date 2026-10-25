using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ExtendedButton))]
public class UIEnhancementButtonHighlight : MonoBehaviour
{
	private enum HighlightState
	{
		PREVIEW,
		SELECTED,
		SELECTABLE,
		INVALID
	}

	[SerializeField]
	private Image frame;

	[SerializeField]
	private CanvasGroup fillImage;

	[SerializeField]
	private string shaderProperty = "_RectFormat";

	[SerializeField]
	private float opacitySelected = 0.35f;

	[SerializeField]
	private float opacityHovered = 0.2f;

	[SerializeField]
	private Color invalidFrameColor;

	[SerializeField]
	private Color validFrameColor;

	private RectTransform rectTransform;

	private EnhancementButtonBase ability;

	private bool enable;

	private ExtendedButton button;

	private Action<EnhancementButtonBase> onSelectedAbility;

	private HighlightState state;

	private Image fillImageIcon;

	private void Awake()
	{
		frame.material = new Material(frame.material);
		rectTransform = GetComponent<RectTransform>();
		button = GetComponent<ExtendedButton>();
		button.onClick.AddListener(SelectAbility);
		button.onSelected.AddListener(SelectAbility);
		fillImageIcon = fillImage.GetComponent<Image>();
	}

	private void SelectAbility()
	{
		onSelectedAbility?.Invoke(ability);
	}

	public void HighlightPreview(RectTransform target, EnhancementButtonBase ability)
	{
		state = HighlightState.PREVIEW;
		Highlight(target, ability);
	}

	public void HighlightSelectable(RectTransform target, EnhancementButtonBase ability, Action<EnhancementButtonBase> onSelect)
	{
		state = HighlightState.SELECTABLE;
		Highlight(target, ability, onSelect);
	}

	public void HighlightSelected(RectTransform target, EnhancementButtonBase ability)
	{
		state = HighlightState.SELECTED;
		Highlight(target, ability);
	}

	public void HighlightInvalid(RectTransform target, EnhancementButtonBase ability, Action<EnhancementButtonBase> onSelect)
	{
		state = HighlightState.INVALID;
		Highlight(target, ability, onSelect);
	}

	private void Highlight(RectTransform target, EnhancementButtonBase ability, Action<EnhancementButtonBase> onSelect = null)
	{
		this.ability = ability;
		Image image = frame;
		Color color = (fillImageIcon.color = ((state == HighlightState.INVALID) ? invalidFrameColor : validFrameColor));
		image.color = color;
		onSelectedAbility = onSelect;
		rectTransform.pivot = target.pivot;
		rectTransform.sizeDelta = target.rect.size;
		transform.position = target.transform.position;
		frame.material.SetVector(shaderProperty, target.rect.size);
		fillImage.alpha = ((state == HighlightState.SELECTED) ? opacitySelected : 0f);
		button.interactable = onSelect != null;
	}

	public void OnHovered(bool hovered)
	{
		switch (state)
		{
		case HighlightState.SELECTED:
			fillImage.alpha = opacitySelected;
			break;
		case HighlightState.PREVIEW:
			fillImage.alpha = 0f;
			break;
		case HighlightState.SELECTABLE:
		case HighlightState.INVALID:
			fillImage.alpha = (hovered ? opacityHovered : 0f);
			break;
		}
	}

	public void SetMode(bool isBuy)
	{
		Image image = frame;
		Color color = (fillImageIcon.color = (isBuy ? UIInfoTools.Instance.buyEnhancementColor : UIInfoTools.Instance.sellEnhancementColor));
		image.color = color;
	}
}
