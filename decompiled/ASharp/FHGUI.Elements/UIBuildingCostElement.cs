using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

namespace FHGUI.Elements;

public class UIBuildingCostElement : MonoBehaviour
{
	[SerializeField]
	private Image _icon;

	[SerializeField]
	private TMP_Text _value;

	public void SetCost(int value)
	{
		_value.text = value.ToString();
	}
}
