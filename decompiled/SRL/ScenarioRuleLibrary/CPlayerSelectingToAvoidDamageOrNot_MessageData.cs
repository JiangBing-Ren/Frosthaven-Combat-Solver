namespace ScenarioRuleLibrary;

public class CPlayerSelectingToAvoidDamageOrNot_MessageData : CMessageData
{
	public CActor m_ActorBeingAttacked;

	public CPlayerActor m_ActorToShowCardsFor;

	public int m_ModifiedStrength;

	public bool m_IsDirectDamage;

	public CAttackSummary.TargetSummary m_TargetSummary;

	public CAbility m_DamagingAbility;

	public bool m_AllowBurnCards;

	public CAbility.EAbilityType m_DamageType;

	public CPlayerSelectingToAvoidDamageOrNot_MessageData(CActor actorSpawningMessage)
		: base(MessageType.PlayerSelectingToAvoidDamageOrNot, actorSpawningMessage)
	{
	}
}
