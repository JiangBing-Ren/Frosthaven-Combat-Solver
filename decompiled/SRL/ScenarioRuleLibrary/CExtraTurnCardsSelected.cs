namespace ScenarioRuleLibrary;

public class CExtraTurnCardsSelected : CMessageData
{
	public CExtraTurnCardsSelected(CActor actorSpawningMessage)
		: base(MessageType.ExtraTurnCardsSelected, actorSpawningMessage)
	{
	}
}
