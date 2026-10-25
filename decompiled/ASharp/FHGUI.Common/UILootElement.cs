using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Common;

public class UILootElement : MonoBehaviour
{
	[SerializeField]
	private Image _icon;

	[SerializeField]
	private UITextTooltipTarget _tooltip;

	[SerializeField]
	private TMP_Text _quantity;

	public void Init(LootDTO loot)
	{
		_icon.sprite = loot.Icon;
		_tooltip.SetText(loot.Tooltip);
		_quantity.text = loot.Quantity.ToString();
	}
}
