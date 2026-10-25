using TMPro;
using UnityEngine;

namespace FHGUI.Elements;

public class UIJewellerRow : MonoBehaviour
{
	public struct JewellerRowData
	{
		public enum State
		{
			None,
			Locked,
			Active
		}

		public uint PercentageDiscount;

		public string TooltipText;

		public State JewelerState;

		public uint UnlockLevel;
	}

	[SerializeField]
	private TMP_Text _discountPercentage;

	[SerializeField]
	private UITextTooltipTarget _tooltip;

	[SerializeField]
	private TextLocalizedListener _discountDescription;

	[SerializeField]
	private UIBuildingLockRequirement _lockedState;

	[SerializeField]
	private TMP_Text _unlockText;

	private JewellerRowData? _data;

	public void Init(JewellerRowData data, uint currentLevel)
	{
		_data = data;
		_discountPercentage.text = $"{data.PercentageDiscount}%";
		_tooltip.tooltipText = data.TooltipText;
		_discountDescription.SetParam("0", data.PercentageDiscount);
		_lockedState.Show(data.UnlockLevel, currentLevel);
		_unlockText.text = data.UnlockLevel.ToString();
	}

	private void OnDisable()
	{
		_data = null;
	}
}
