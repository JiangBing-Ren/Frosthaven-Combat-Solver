using System;
using System.Linq;
using FHGUI.DTO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class OutpostInfoStatElement : MonoBehaviour
{
	[Serializable]
	private struct ColorValuePair
	{
		public int Value;

		public Color Color;
	}

	[SerializeField]
	private Slider _slider;

	[SerializeField]
	private Image _coloredBackground;

	[SerializeField]
	private TMP_Text _valueLabel;

	[SerializeField]
	private ColorValuePair[] _ascendingBackgroundColorValues = Array.Empty<ColorValuePair>();

	[SerializeField]
	private ColorValuePair[] _ascendingLabelColorValues = Array.Empty<ColorValuePair>();

	public void Init(int value)
	{
		_slider.value = 0f;
		_valueLabel.text = value.ToString();
		RefreshColors(value);
	}

	public void Init(RangedValueDTO rangedValue)
	{
		_slider.value = rangedValue.Progress;
		_valueLabel.text = rangedValue.ToDisplayString();
		RefreshColors(rangedValue.Value);
	}

	private void RefreshColors(int value)
	{
		foreach (ColorValuePair item in Enumerable.Reverse(_ascendingLabelColorValues))
		{
			if (value <= item.Value)
			{
				_valueLabel.color = item.Color;
				continue;
			}
			break;
		}
		foreach (ColorValuePair item2 in Enumerable.Reverse(_ascendingBackgroundColorValues))
		{
			if (value <= item2.Value)
			{
				_coloredBackground.color = item2.Color;
				continue;
			}
			break;
		}
	}
}
