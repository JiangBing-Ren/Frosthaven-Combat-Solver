using System.Collections.Generic;
using System.Linq;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using MapRuleLibrary.YML.Quest;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICampaignReward : UIReward
{
	[SerializeField]
	private List<Image> glows;

	[SerializeField]
	private Color positiveGlowColor;

	[SerializeField]
	protected GUIAnimator glowAnimator;

	[SerializeField]
	private string positiveRewardAudioItem = "PlaySound_UIPingRewardPositive";

	[SerializeField]
	private string negativeRewardAudioItem = "PlaySound_UIPingRewardNegative";

	[SerializeField]
	private TextMeshProUGUI extraAmountText;

	[SerializeField]
	private string itemRewardTextLoc;

	private Color? textColor;

	public override void ShowReward(Reward reward)
	{
		base.ShowReward(reward);
		switch (reward.Type)
		{
		case ETreasureType.PerkPoint:
			rewardInformation.text = string.Format("<font=\"MarcellusSC-Regular SDF\"><size=+2><color=#{1}><cspace=1>{0}</cspace></color></size></font>", LocalizationManager.GetTranslation("GUI_QUEST_REWARD_PERKPOINT_Unlocked"), itemNameColor.ToHex());
			break;
		case ETreasureType.EnhancementSlots:
			rewardInformation.text = string.Format(LocalizationManager.GetTranslation("GUI_QUEST_REWARD_EnchantmentPoints"), $"+{reward.Amount}");
			break;
		}
		if (reward.GiveToCharacterID.IsNOTNullOrEmpty() && reward.GiveToCharacterID != "party" && reward.GiveToCharacterID != "NoneID")
		{
			CPlayerActor cPlayerActor = ScenarioManager.Scenario.PlayerActors.SingleOrDefault((CPlayerActor x) => x.Class.ID == reward.GiveToCharacterID);
			string text = string.Empty;
			if (AdventureState.MapState?.MapParty?.SelectedCharactersArray != null)
			{
				text = AdventureState.MapState.MapParty.SelectedCharacters.FirstOrDefault((CMapCharacter it) => it?.CharacterID == reward.GiveToCharacterID)?.CharacterName;
			}
			if (string.IsNullOrEmpty(text))
			{
				text = LocalizationManager.GetTranslation(cPlayerActor.CharacterClass.LocKey);
			}
			string text2 = string.Format("<font=\"MarcellusSC-Regular SDF\"><size=+2><color=#{1}><cspace=1>{0}</cspace></color></size></font>", text, itemNameColor.ToHex());
			if (reward.Type == ETreasureType.Condition)
			{
				rewardInformation.text = string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_PLAYER_WILL_RECIEVE_CONDITION"), text2, LocalizationManager.GetTranslation(reward.Conditions.Single().ToString()));
			}
			else if (reward.Type == ETreasureType.Damage)
			{
				rewardInformation.text = string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_WILL_SUFFER_DAMAGE"), text2, reward.Amount);
			}
			else if (itemRewardTextLoc.IsNullOrEmpty())
			{
				rewardInformation.text = text2 + " " + rewardInformation.text;
			}
			else
			{
				rewardInformation.text = string.Format(LocalizationManager.GetTranslation(itemRewardTextLoc), text2, rewardInformation.text);
			}
		}
		SetupExtraAmount(reward);
		if (!textColor.HasValue)
		{
			textColor = rewardInformation.color;
		}
		if (reward.IsNegative())
		{
			for (int num = 0; num < glows.Count; num++)
			{
				glows[num].color = UIInfoTools.Instance.warningColor;
			}
			rewardInformation.color = UIInfoTools.Instance.warningColor;
			AudioControllerUtils.PlaySound(negativeRewardAudioItem);
		}
		else
		{
			for (int num2 = 0; num2 < glows.Count; num2++)
			{
				glows[num2].color = positiveGlowColor;
			}
			rewardInformation.color = textColor.Value;
			AudioControllerUtils.PlaySound(positiveRewardAudioItem);
		}
		glowAnimator?.Play(fromStart: true);
	}

	protected override string GenerateUnlockQuestDescription(CQuest quest)
	{
		if (LocalizationManager.TryGetTranslation(quest.LocalisedRewardKey, out var translation))
		{
			return translation;
		}
		return string.Format(LocalizationManager.GetTranslation("GUI_QUEST_REWARD_Unlocked"), string.Format("<font=\"MarcellusSC-Regular SDF\"><size=+2><color=#{1}><cspace=1>{0}</cspace></color></size></font>", LocalizationManager.GetTranslation(quest.LocalisedNameKey), itemNameColor.ToHex()));
	}

	private void SetupExtraAmount(Reward reward)
	{
		ETreasureType type = reward.Type;
		if (type == ETreasureType.Item || type == ETreasureType.UnlockProsperityItem)
		{
			if (reward.Amount > 1)
			{
				extraAmountText.text = $"x{reward.Amount}";
				extraAmountText.gameObject.SetActive(value: true);
			}
			else
			{
				extraAmountText.gameObject.SetActive(value: false);
			}
		}
		else
		{
			extraAmountText.gameObject.SetActive(value: false);
		}
	}

	private void OnDisable()
	{
		StopAnimations();
	}

	public virtual void StopAnimations()
	{
		glowAnimator?.Stop();
	}
}
