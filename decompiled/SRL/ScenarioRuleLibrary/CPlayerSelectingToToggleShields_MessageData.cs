namespace ScenarioRuleLibrary;

public class CPlayerSelectingToToggleShields_MessageData : CMessageData
{
	public CActor m_ActorBeingAttacked;

	public CPlayerActor m_ActorToShowCardsFor;

	public int m_ModifiedStrength;

	public CAttackSummary.TargetSummary m_TargetSummary;

	public CAbility m_DamagingAbility;

	public CAbility.EAbilityType m_DamageType;

	public CPlayerSelectingToToggleShields_MessageData(CActor actorSpawningMessage)
		: base(MessageType.PlayerSelectingToToggleShields, actorSpawningMessage)
	{
	}
}
