using System.Text;
using GLOOM;
using MapRuleLibrary.Party;
using UnityEngine;
using UnityEngine.UI;

public class UICampaignAssemblyCharacterInformation : UICharacterInformation
{
	[SerializeField]
	private UITextTooltipTarget descriptionTooltip;

	[SerializeField]
	private Toggle personalQuestConcealToggle;

	[SerializeField]
	private UITextTooltipTarget personalQuestTooltip;

	private CMapCharacter characterData;

	private void Awake()
	{
		personalQuestConcealToggle.onValueChanged.AddListener(ConcealPersonalQuest);
	}

	public override void Display(CMapCharacter characterData)
	{
		if (this.characterData != characterData)
		{
			this.characterData = characterData;
			if (characterData.PersonalQuest != null)
			{
				personalQuestConcealToggle.SetValue(characterData.PersonalQuest.IsConcealed);
				RefreshConcelPersonalQuestTooltip(personalQuestConcealToggle.isOn);
			}
			StringBuilder stringBuilder = new StringBuilder("<size=18>");
			stringBuilder.AppendLine(LocalizationManager.GetTranslation(characterData.CharacterYMLData.Adventure_Description));
			stringBuilder.AppendFormat("\n<size=22><sprite name=\"Icon_Strengthen\" color=#{1}> <color=#{1}>{0}</color></size>\n", LocalizationManager.GetTranslation("GUI_DV_CHARACTER_STRENGTHS"), UIInfoTools.DefInstance.basicTextColor.ToHex());
			stringBuilder.AppendLine(LocalizationManager.GetTranslation(characterData.CharacterYMLData.Strengths));
			stringBuilder.AppendFormat("\n<size=22><sprite name=\"Skull_Icon\" color=#{1}> <color=#{1}>{0}</color></size>\n", LocalizationManager.GetTranslation("GUI_DV_CHARACTER_WEAKNESSES"), UIInfoTools.DefInstance.basicTextColor.ToHex());
			stringBuilder.AppendLine(LocalizationManager.GetTranslation(characterData.CharacterYMLData.Weaknesses));
			descriptionTooltip.SetText(stringBuilder.ToString());
		}
	}

	private void ConcealPersonalQuest(bool conceal)
	{
		characterData.PersonalQuest.Conceal(conceal);
		RefreshConcelPersonalQuestTooltip(conceal);
	}

	private void RefreshConcelPersonalQuestTooltip(bool conceal)
	{
		string text = string.Format("<size=+2><color=#{1}>{0}</color></size>", LocalizationManager.GetTranslation((!conceal) ? "GUI_PERSONAL_QUEST_CONCEAL_TOOLTIP_TITLE" : "GUI_PERSONAL_QUEST_SHOW_TOOLTIP_TITLE"), UIInfoTools.DefInstance.basicTextColor.ToHex());
		personalQuestTooltip.SetText(text, refreshTooltip: true, LocalizationManager.GetTranslation((!conceal) ? "GUI_PERSONAL_QUEST_CONCEAL_TOOLTIP" : "GUI_PERSONAL_QUEST_SHOW_TOOLTIP"));
	}
}
