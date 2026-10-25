namespace ScenarioRuleLibrary;

public class CAddNewCards_MessageData : CMessageData
{
	public int[] ids;

	public CAddNewCards_MessageData(CActor actorSpawningMessage)
		: base(MessageType.AddNewCards, actorSpawningMessage, string.Empty)
	{
	}
}
