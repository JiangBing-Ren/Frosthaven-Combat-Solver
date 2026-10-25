namespace ScenarioRuleLibrary;

public class CardScript_Geminate0182 : CardScript_Geminate
{
	protected override void TickAbilityStartedInternal()
	{
		ExtraTurnFormChange(CPlayerActor.EForm.Range);
	}
}
