using FHGUI.DTO;
using FHRL;
using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class ResourceElement : MonoBehaviour
{
	public record ResourceActionElementData
	{
		public OutpostMaterialType Material;

		public int Amount;

		public bool IsGold;

		public bool IsAvailable = true;
	}

	[SerializeField]
	private Image _icon;

	[SerializeField]
	private TMP_Text _label;

	[SerializeField]
	private Color _unavailableIconColor;

	[SerializeField]
	private Color _unavailableLabelColor;

	[SerializeField]
	private UITextTooltipTarget _tooltip;

	private Color? _defaultIconColor;

	private Color? _defaultLabelColor;

	public void Init(int value)
	{
		if (!_defaultIconColor.HasValue || !_defaultLabelColor.HasValue)
		{
			_defaultIconColor = _icon.color;
			_defaultLabelColor = _label.color;
		}
		_label.text = value.ToString();
		_icon.color = ((value <= 0) ? _unavailableIconColor : _defaultIconColor.Value);
		_label.color = ((value <= 0) ? _unavailableLabelColor : _defaultLabelColor.Value);
	}

	public void Init(int value, bool available)
	{
		if (!_defaultIconColor.HasValue || !_defaultLabelColor.HasValue)
		{
			_defaultIconColor = _icon.color;
			_defaultLabelColor = _label.color;
		}
		_label.text = value.ToString();
		_icon.color = (available ? _defaultIconColor.Value : _unavailableIconColor);
		_label.color = (available ? _defaultLabelColor.Value : _unavailableLabelColor);
	}

	public void Init(uint value, bool available)
	{
		Init((int)value, available);
	}

	public void Init(int value, int maxValue)
	{
		Init(value);
		_label.text = $"{value}/{maxValue}";
	}

	public void Init(RangedValueDTO value)
	{
		Init(value.Value, value.Max);
	}

	public void Init(uint value)
	{
		Init((int)value);
	}

	public void SetSprite(Sprite sprite)
	{
		_icon.sprite = sprite;
	}

	public void Init(ResourceActionElementData data)
	{
		_icon.sprite = (data.IsGold ? UIInfoTools.Instance.GetResourceConfig("Gold").IconStylized : UIInfoTools.Instance.GetResourceConfig(data.Material).IconStylized);
		Init(data.Amount, data.IsAvailable);
		if (_tooltip != null)
		{
			_tooltip.SetText(LocalizationManager.GetTranslation(data.IsGold ? "Gold" : FHUILocKeys.GetMaterialLocalizationKey(data.Material)));
		}
	}
}
