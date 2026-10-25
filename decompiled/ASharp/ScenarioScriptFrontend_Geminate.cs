using ScenarioRuleLibrary;

internal class ScenarioScriptFrontend_Geminate : ScenarioScriptFrontend
{
	private CardScript_Geminate _cardScript => (CardScript_Geminate)_script;

	protected void OverrideUIForm()
	{
		if (!(_cardScript.TickActor is CPlayerActor cPlayerActor) || cPlayerActor.Class.ID != "GeminateID")
		{
			return;
		}
		CardsHandUI hand = Singleton<CardsHandManager>.Instance.GetHand(cPlayerActor);
		if (hand.currentMode == CardHandMode.CardsSelection)
		{
			CPlayerActor.EForm scriptedFormForUI = _cardScript.ScriptedFormForUI;
			if (scriptedFormForUI != CPlayerActor.EForm.None && hand.TryRestrictToForm(scriptedFormForUI))
			{
				_cardScript.Trace($"Setting Geminate Cards UI form to {scriptedFormForUI}", null, 29);
			}
		}
	}
}
