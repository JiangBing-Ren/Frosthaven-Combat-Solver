namespace ScenarioRuleLibrary;

public class CPlayEnvAudio_MessageData : CMessageData
{
	public string m_RoomId;

	public CPlayEnvAudio_MessageData(string roomId)
		: base(MessageType.PlayEnvAudio, null)
	{
		m_RoomId = roomId;
	}
}
