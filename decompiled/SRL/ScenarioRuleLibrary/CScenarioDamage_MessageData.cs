namespace ScenarioRuleLibrary;

public class CScenarioDamage_MessageData : CMessageData
{
	public int m_DamageAmount;

	public string m_DamageSourceLocKey;

	public string m_OverrideLogMessageLocKey;

	public bool m_TreatAsScenarioEffect;

	public bool m_TreatAsScenarioRule;

	public bool m_TreatAsEventEffect;

	public CScenarioDamage_MessageData(CActor actor, int damageAmount, string damageSourceLocKey, string overrideLogMessageLocKey)
		: base(MessageType.ScenarioDamage, actor)
	{
		m_DamageAmount = damageAmount;
		m_DamageSourceLocKey = damageSourceLocKey;
		m_OverrideLogMessageLocKey = overrideLogMessageLocKey;
	}
}
