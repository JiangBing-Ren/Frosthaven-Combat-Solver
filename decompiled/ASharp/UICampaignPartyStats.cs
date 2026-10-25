using GLOOM;
using MapRuleLibrary.Party;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICampaignPartyStats : UIPartyStats
{
	[Header("Reputation")]
	[SerializeField]
	private TextMeshProUGUI reputationText;

	[SerializeField]
	private ImageProgressBar reputationBar;

	[SerializeField]
	private UITextTooltipTarget reputationTooltip;

	[SerializeField]
	private Image reputationFill;

	[SerializeField]
	private Color positiveReputationColor;

	[SerializeField]
	private Color negativeReputationColor;

	[Header("Wealth")]
	[SerializeField]
	private TextMeshProUGUI wealthText;

	[SerializeField]
	private ImageProgressBar wealthBar;

	[SerializeField]
	private UITextTooltipTarget wealthTooltip;

	public override void Setup(CMapParty mapParty)
	{
		ClearEvents();
		base.Setup(mapParty);
		RefreshReputation(animate: false);
		RefreshWealth(animate: false);
	}

	public void RefreshReputation(bool animate)
	{
		string text = ((mapParty.ShopDiscount >= 0) ? $"+{mapParty.ShopDiscount}" : mapParty.ShopDiscount.ToString());
		string amountText = $"<rotate={((mapParty.Reputation < 0) ? 180 : 0)}><sprite name=\"ReputationLevel_Icon\"></rotate>{text}";
		Color color = ((mapParty.Reputation >= 0) ? positiveReputationColor : negativeReputationColor);
		reputationText.text = string.Format(string.Format("{0} {1}", LocalizationManager.GetTranslation("GUI_PartyReputation"), mapParty.Reputation));
		reputationTooltip.SetText(string.Format(LocalizationManager.GetTranslation("GUI_PartyReputation_Tooltip"), mapParty.Reputation, text, color.ToHex(), (mapParty.Reputation < 0) ? 180 : 0), refreshTooltip: true);
		reputationFill.color = color;
		if (animate)
		{
			reputationBar.PlayProgressTo(Mathf.InverseLerp(-20f, 20f, mapParty.Reputation), amountText);
		}
		else
		{
			reputationBar.SetAmount(Mathf.InverseLerp(-20f, 20f, mapParty.Reputation), amountText);
		}
	}

	public void RefreshWealth(bool animate)
	{
		RefreshWealth(mapParty.ProsperityXP, mapParty.ProsperityLevel, mapParty.ProsperityLevel, animate);
	}

	private void RefreshWealth(int prosperityXP, int prosperityLevel, int oldProsperityLevel, bool animate)
	{
		wealthText.text = string.Format(string.Format("{0} {1}", LocalizationManager.GetTranslation("GUI_PartyProsperity"), prosperityLevel));
		wealthTooltip.SetText(string.Format(LocalizationManager.GetTranslation("GUI_PartyProsperity_Tooltip"), prosperityLevel), refreshTooltip: true);
		wealthBar.SetAmount(1f, 1f);
		wealthBar.ShowProgressText(show: false);
	}

	private void OnDestroy()
	{
		ClearEvents();
	}

	private void ClearEvents()
	{
	}

	private void OnProsperityChanged(int newXP, int oldXP, int newLevel, int oldLevel)
	{
		RefreshWealth(newXP, newLevel, oldLevel, animate: true);
	}

	private void OnReputationChanged(int newvalue)
	{
		RefreshReputation(animate: true);
	}
}
