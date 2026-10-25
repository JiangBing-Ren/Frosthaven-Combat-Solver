namespace ScenarioRuleLibrary;

public class CardScript_Geminate0181 : CardScript_Geminate
{
	protected override void TickAbilityStartedInternal()
	{
		ExtraTurnFormChange(CPlayerActor.EForm.Melee);
	}
}
