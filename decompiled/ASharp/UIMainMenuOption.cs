using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMainMenuOption : UIMenuOptionToggle
{
	[SerializeField]
	protected TextMeshProUGUI text;

	[SerializeField]
	private Color defaultTextColor;

	[SerializeField]
	private Color higlightedTextColor;

	[SerializeField]
	private TMP_ColorGradient defaultColorGradient;

	[SerializeField]
	private TMP_ColorGradient highlightedColorGradient;

	[SerializeField]
	private UITextTooltipTarget tooltip;

	[SerializeField]
	protected Image focusMask;

	[SerializeField]
	protected List<Graphic> interactionMasks;

	[Header("Locked")]
	[SerializeField]
	private GameObject lockedMask;

	[SerializeField]
	private Color lockedTextColor;

	[SerializeField]
	private TMP_ColorGradient lockedColorGradient;

	private bool isFocused;

	public bool IsInteractable
	{
		get
		{
			return toggle.interactable;
		}
		set
		{
			if (value != toggle.interactable)
			{
				toggle.interactable = value;
				lockedMask.SetActive(!value);
				RefreshHighlight();
				for (int i = 0; i < interactionMasks.Count; i++)
				{
					interactionMasks[i].material = (toggle.interactable ? null : UIInfoTools.Instance.greyedOutMaterial);
				}
				if (!toggle.interactable && isSelected)
				{
					Deselect();
				}
			}
		}
	}

	public override void Init(Action onSelected, Action onDeselected = null, Action<bool> onHovered = null, bool isSelected = false)
	{
		base.Init(onSelected, onDeselected, onHovered, isSelected);
		IsInteractable = true;
		SetTooltip(enable: false);
		SetFocused(isFocused: true);
	}

	protected override void RefreshHighlight(bool isHighlighted)
	{
		base.RefreshHighlight(isHighlighted && toggle.interactable);
		text.enableVertexGradient = !isHighlighted;
		if (defaultColorGradient != null)
		{
			text.enableVertexGradient = true;
			TextMeshProUGUI textMeshProUGUI = text;
			TMP_ColorGradient colorGradientPreset;
			if (toggle.interactable)
			{
				colorGradientPreset = (isHighlighted ? highlightedColorGradient : defaultColorGradient);
			}
			else
			{
				colorGradientPreset = lockedColorGradient;
			}
			textMeshProUGUI.colorGradientPreset = colorGradientPreset;
		}
		else
		{
			TextMeshProUGUI textMeshProUGUI2 = text;
			Color color;
			if (toggle.interactable)
			{
				color = (isHighlighted ? higlightedTextColor : defaultTextColor);
			}
			else
			{
				color = lockedTextColor;
			}
			textMeshProUGUI2.color = color;
		}
		focusMask.enabled = !isFocused && !isHighlighted;
	}

	public void SetTooltip(bool enable, string tooltipText = null)
	{
		if (!(tooltip == null))
		{
			if (tooltipText.IsNOTNullOrEmpty())
			{
				tooltip.SetText(tooltipText);
			}
			tooltip.enabled = enable;
		}
	}

	public void SetFocused(bool isFocused)
	{
		this.isFocused = isFocused;
		focusMask.enabled = !isFocused && !isSelected && !isHovered;
	}
}
