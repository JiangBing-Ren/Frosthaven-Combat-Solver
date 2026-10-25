public class UIGuildmasterPartyStats : UIPartyStats
{
	public override void UpdateLevel()
	{
		levelText.text = mapParty.PartyLevel.ToString();
	}
}
