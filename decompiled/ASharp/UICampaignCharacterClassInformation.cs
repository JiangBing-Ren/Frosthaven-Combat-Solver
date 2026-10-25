using MapRuleLibrary.Party;
using TMPro;
using UnityEngine;

public class UICampaignCharacterClassInformation : UICharacterClassInformation
{
	[SerializeField]
	private TextMeshProUGUI gold;

	public override void Display(CMapCharacter characterData)
	{
		base.Display(characterData);
		if (gold != null)
		{
			gold.text = characterData.CharacterGold.ToString();
		}
	}
}
