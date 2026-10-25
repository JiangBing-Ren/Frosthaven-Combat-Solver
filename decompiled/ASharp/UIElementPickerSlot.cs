using System;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class UIElementPickerSlot : MonoBehaviour
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

	[Header("Element")]
	[SerializeField]
	private Image icon;

	[SerializeField]
	private Image highlightElement;

	private Action<ElementInfusionBoardManager.EElement> onSelected;

	private bool isHovered;

	private bool isSelected;

	[SerializeField]
	private ElementInfusionBoardManager.EElement element;

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
		Init(element);
	}

	public void Init(ElementInfusionBoardManager.EElement element)
	{
		this.element = element;
		icon.sprite = UIInfoTools.Instance.GetElementPickerSprite(element);
		highlightElement.color = UIInfoTools.Instance.GetElementHighlightColor(element, highlightElement.color.a);
		button.mouseDownAudioItem = UIInfoTools.Instance.GetSelectElementAudioItemIcon(element);
	}

	public void Show(Action<ElementInfusionBoardManager.EElement> onSelected)
	{
		this.onSelected = onSelected;
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
		onSelected?.Invoke(element);
	}

	private void OnHover(bool hover)
	{
		isHovered = hover;
		RefreshHighlight();
	}

	public void SetSelected(bool selected)
	{
		isSelected = selected;
		RefreshHighlight();
	}

	private void RefreshHighlight()
	{
		bool flag = isHovered || isSelected;
		highlightElement.enabled = flag;
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
