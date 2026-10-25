namespace ScenarioRuleLibrary;

public class CPlayerImprovedShortRested_MessageData : CMessageData
{
	public int m_ActorOriginalHealth;

	public CAbilityCard m_AbilityCard;

	public bool m_SyncMultiplayer = true;

	public CPlayerImprovedShortRested_MessageData(int actorOriginalHealth, CAbilityCard abilityCard, CActor actorSpawningMessage)
		: base(MessageType.PlayerImprovedShortRested, actorSpawningMessage)
	{
		m_ActorOriginalHealth = actorOriginalHealth;
		m_AbilityCard = abilityCard;
	}
}
