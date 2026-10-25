using System.Collections.Generic;
using System.Linq;
using GLOOM;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICharacterClassInformation : UICharacterInformation
{
	[SerializeField]
	private TextMeshProUGUI characterName;

	[SerializeField]
	private TextMeshProUGUI health;

	[SerializeField]
	private TextMeshProUGUI abilities;

	[SerializeField]
	private List<Image> characterColorMasks;

	[SerializeField]
	private TextMeshProUGUI items;

	[SerializeField]
	private TextMeshProUGUI perks;

	[SerializeField]
	private TextMeshProUGUI enhancements;

	[SerializeField]
	private UIAdventureCharacterXPInformation xpInformation;

	[SerializeField]
	private TextLocalizedListener characterDescription;

	[SerializeField]
	private TextLocalizedListener difficulty;

	[SerializeField]
	private TextLocalizedListener role;

	[SerializeField]
	private TextLocalizedListener strengths;

	[SerializeField]
	private TextLocalizedListener weaknesses;

	public override void Display(CMapCharacter characterData)
	{
		if (characterData.CharacterName.IsNOTNullOrEmpty())
		{
			characterName.text = characterData.CharacterName;
		}
		else
		{
			characterName.text = LocalizationManager.GetTranslation(CharacterClassManager.Classes.Single((CCharacterClass s) => s.ID == characterData.CharacterID).LocKey);
		}
		CharacterYMLData characterYMLData = characterData.CharacterYMLData;
		Color characterColor = UIInfoTools.Instance.GetCharacterColor(characterYMLData.Model, characterYMLData.CustomCharacterConfig);
		foreach (Image characterColorMask in characterColorMasks)
		{
			characterColorMask.color = characterColor;
		}
		health.text = characterData.HealthTable[characterData.Level].ToString();
		abilities.text = characterData.OwnedAbilityCardIDs.Count.ToString();
		if (enhancements != null)
		{
			enhancements.text = characterData.Enhancements.Count((CEnhancement it) => it.Enhancement != EEnhancement.NoEnhancement).ToString();
		}
		if (items != null)
		{
			items.text = characterData.CheckEquippedItems.Count.ToString();
		}
		if (perks != null)
		{
			perks.text = characterData.Perks.Count((CharacterPerk it) => it.IsActive).ToString();
		}
		if (xpInformation != null)
		{
			xpInformation.Setup(characterData);
		}
		if (characterDescription != null)
		{
			characterDescription.SetTextKey(characterYMLData.Adventure_Description);
		}
		if (difficulty != null)
		{
			difficulty.SetTextKey(characterYMLData.Difficulty);
		}
		if (role != null)
		{
			role.SetTextKey(characterYMLData.Role);
		}
		if (strengths != null)
		{
			strengths.SetTextKey(characterYMLData.Strengths);
		}
		if (weaknesses != null)
		{
			weaknesses.SetTextKey(characterYMLData.Weaknesses);
		}
	}
}
