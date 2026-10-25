namespace ScenarioRuleLibrary;

public class CApplyToActorAddSong_MessageData : CMessageData
{
	public CAbilityAddSong m_AddSongAbility;

	public CActor m_Target;

	public CApplyToActorAddSong_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ApplyToActorAddSong, actorSpawningMessage)
	{
	}
}
