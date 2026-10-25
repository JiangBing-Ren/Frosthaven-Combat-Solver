namespace ScenarioRuleLibrary;

public class CardScript_Geminate : ScenarioScript
{
	public CPlayerActor.EForm ScriptedFormForUI;

	public CActor TickActor => _tickActor;

	protected void ExtraTurnFormChange(CPlayerActor.EForm form)
	{
		if (_tickActor.Class.ID != "GeminateID")
		{
			ScriptedFormForUI = CPlayerActor.EForm.None;
			return;
		}
		if (!_tickAbilityBelongsToCard)
		{
			ScriptedFormForUI = CPlayerActor.EForm.None;
			return;
		}
		if (_tickAbility.AbilityType != CAbility.EAbilityType.ExtraTurn)
		{
			ScriptedFormForUI = CPlayerActor.EForm.None;
			return;
		}
		Trace("Change the Geminate form in the UI...", null, 34);
		ScriptedFormForUI = form;
	}
}
