namespace ScenarioRuleLibrary;

public class CActorBeenDamaged_MessageData : CMessageData
{
	public CActor m_ActorBeingDamaged;

	public CAbilityDamage m_DamageAbility;

	public int m_ActorOriginalHealth;

	public bool m_ActorWasAsleep;

	public int? m_ActualDamage;

	public string m_DamageSourceLocKey;

	public string m_OverrideLogMessageLocKey;

	public string m_OverrideAnimState;

	public bool m_skipHitAnimation;

	public bool m_TreatAsScenarioEffect;

	public bool m_TreatAsScenarioRule;

	public bool m_TreatAsEventEffect;

	public CActorBeenDamaged_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorBeenDamaged, actorSpawningMessage)
	{
	}
}
