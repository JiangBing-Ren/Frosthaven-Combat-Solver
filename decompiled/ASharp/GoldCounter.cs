using System.Collections.Generic;
using System.Linq;
using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoldCounter : Counter
{
	[SerializeField]
	protected UITextTooltipTarget tooltip;

	[SerializeField]
	private TextMeshProUGUI textHighlight;

	[SerializeField]
	private TextMeshProUGUI textWarning;

	[SerializeField]
	private List<Graphic> elementsToColor;

	private List<Color> _defaultElementsColors;

	private string _tooltipFormat;

	private List<Color> DefaultElementsColors
	{
		get
		{
			if (_defaultElementsColors == null)
			{
				_defaultElementsColors = elementsToColor.Select((Graphic it) => it.color).ToList();
			}
			return _defaultElementsColors;
		}
	}

	private string TooltipFormat
	{
		get
		{
			if (string.IsNullOrEmpty(_tooltipFormat))
			{
				_tooltipFormat = LocalizationManager.GetTranslation("GUI_PartyGold_Tooltip");
			}
			return _tooltipFormat;
		}
	}

	protected override void UpdateCount(int count)
	{
		base.UpdateCount(count);
		TextMeshProUGUI textMeshProUGUI = textHighlight;
		string text = (textWarning.text = base.text.text);
		textMeshProUGUI.text = text;
	}

	public override void CountTo(int count)
	{
		tooltip?.SetText(string.Format(TooltipFormat, count), refreshTooltip: true);
		base.CountTo(count);
	}

	public override void SetCount(int count)
	{
		tooltip?.SetText(string.Format(TooltipFormat, count), refreshTooltip: true);
		base.SetCount(count);
	}

	protected override void ResetEffects()
	{
		base.ResetEffects();
		for (int i = 0; i < elementsToColor.Count; i++)
		{
			elementsToColor[i].color = DefaultElementsColors[i];
		}
	}

	protected override void ShowIncrease()
	{
		base.ShowIncrease();
		for (int i = 0; i < elementsToColor.Count; i++)
		{
			elementsToColor[i].color = increaseColor;
		}
	}

	protected override void ShowDecrease()
	{
		base.ShowDecrease();
		for (int i = 0; i < elementsToColor.Count; i++)
		{
			elementsToColor[i].color = UIInfoTools.Instance.warningColor;
		}
	}
}
