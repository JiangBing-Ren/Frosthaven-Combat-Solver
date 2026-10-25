using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIHallOfRevelryRow : MonoBehaviour
{
	public struct HallOfRevelryData
	{
		public uint RequiredLevel;

		public Sprite Icon;

		public string TooltipText;
	}

	[SerializeField]
	private UITextTooltipTarget _tooltip;

	[SerializeField]
	private Image _favorIcon;

	[Header("Locked")]
	[SerializeField]
	private UIBuildingLockRequirement _lockedState;

	[SerializeField]
	private TMP_Text _levelText;

	private HallOfRevelryData? _data;

	public void Init(HallOfRevelryData data, uint currentLevel)
	{
		_data = data;
		_favorIcon.sprite = data.Icon;
		_levelText.text = data.RequiredLevel.ToString();
		_tooltip.tooltipText = data.TooltipText;
		_lockedState.Show(data.RequiredLevel, currentLevel);
	}

	private void OnDisable()
	{
		_data = null;
	}
}
