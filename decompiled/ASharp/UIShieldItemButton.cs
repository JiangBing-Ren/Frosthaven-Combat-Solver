using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;

public class UIShieldItemButton : MonoBehaviour
{
	[SerializeField]
	private TMP_Text shieldText;

	[SerializeField]
	private TMP_Text shieldAmount;

	[SerializeField]
	private GameObject shieldObject;

	public CItem m_Item;

	public int m_ItemIndex;

	public void SetupItemValues(CItem item, int index)
	{
		m_ItemIndex = index;
		m_Item = item;
		shieldAmount.text = item.YMLData.Data.ShieldValue.ToString();
	}

	public void ToggleShieldItem(bool toggledOn = true)
	{
	}
}
