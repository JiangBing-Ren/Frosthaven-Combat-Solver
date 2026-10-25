namespace ScenarioRuleLibrary;

public class CActorIsApplyingAddSong_MessageData : CMessageData
{
	public CAbilityAddSong m_AddSongAbility;

	public CActorIsApplyingAddSong_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.ActorIsApplyingAddSong, actorSpawningMessage, animOverload)
	{
	}
}
