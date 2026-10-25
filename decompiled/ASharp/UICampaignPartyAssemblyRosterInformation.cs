using GLOOM;
using MapRuleLibrary.Party;
using TMPro;
using UnityEngine;

public class UICampaignPartyAssemblyRosterInformation : UICharacterInformation
{
	[SerializeField]
	private TextMeshProUGUI characterName;

	public override void Display(CMapCharacter characterData)
	{
		characterName.text = (characterData.CharacterName.IsNullOrEmpty() ? LocalizationManager.GetTranslation(characterData.CharacterYMLData.LocKey) : characterData.CharacterName);
	}
}
