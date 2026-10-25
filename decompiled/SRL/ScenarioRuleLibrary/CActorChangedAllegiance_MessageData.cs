namespace ScenarioRuleLibrary;

public class CActorChangedAllegiance_MessageData : CMessageData
{
	public CAbilityChangeAllegiance m_ChangeAllegianceAbility;

	public CActor.EType m_FromType;

	public CActor.EType m_ToType;

	public CActorChangedAllegiance_MessageData(CAbilityChangeAllegiance changeAllegianceAbility, CActor actor, CActor.EType fromType, CActor.EType toType)
		: base(MessageType.ActorChangedAllegiance, actor)
	{
		m_ChangeAllegianceAbility = changeAllegianceAbility;
		m_FromType = fromType;
		m_ToType = toType;
	}
}
