using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIBuildingResourceCostElement : MonoBehaviour
{
	[SerializeField]
	private Image _icon;

	[SerializeField]
	private TMP_Text _value;

	[SerializeField]
	private UITextTooltipTarget _tooltip;

	public Color AvailableColor = Color.white;

	public Color UnallocatedColor = Color.gray;

	public Color UnavailableColor = Color.red;

	public UITextTooltipTarget Tooltip => _tooltip;

	public void Init(uint value, uint current)
	{
		Init((int)value, (int)current, 0);
	}

	public void Init(uint value, uint current, bool showTotal)
	{
		Init((int)value, (int)current, 0, showTotal);
	}

	public void Init(uint value, uint current, uint available, bool showTotal)
	{
		Init((int)value, (int)current, (int)available, showTotal);
	}

	public void Init(int required, int current, int available, bool showTotal = false)
	{
		bool flag = required <= current;
		_value.text = (showTotal ? $"{current}/{required}" : required.ToString());
		TMP_Text value = _value;
		Color color;
		if (flag)
		{
			color = AvailableColor;
		}
		else
		{
			color = ((current + available >= required) ? UnallocatedColor : UnavailableColor);
		}
		value.color = color;
	}
}
