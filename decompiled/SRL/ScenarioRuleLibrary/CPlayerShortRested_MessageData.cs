namespace ScenarioRuleLibrary;

public class CPlayerShortRested_MessageData : CMessageData
{
	public CPlayerActor m_Player;

	public CAbilityCard m_AbilityCard;

	public bool m_SyncMultiplayer = true;

	public CPlayerShortRested_MessageData(CPlayerActor player, CAbilityCard abilityCard, CActor actorSpawningMessage)
		: base(MessageType.PlayerShortRested, actorSpawningMessage)
	{
		m_Player = player;
		m_AbilityCard = abilityCard;
	}
}
