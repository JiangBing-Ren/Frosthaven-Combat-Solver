namespace ScenarioRuleLibrary;

public class CTransferDoomChoice_MessageData : CMessageData
{
	public CActor m_NewDoomTargetActor;

	public CAbility m_TransferDoomAbility;

	public CTransferDoomChoice_MessageData(CActor actorSpawningMessage)
		: base(MessageType.TransferDoomChoice, actorSpawningMessage)
	{
	}
}
