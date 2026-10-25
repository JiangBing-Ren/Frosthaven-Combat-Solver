namespace ScenarioRuleLibrary;

public class CAdjustInitiativeActiveBonus_FocusInitiative : CBespokeBehaviour
{
	private bool active;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.FocusInitiative;

	public CAdjustInitiativeActiveBonus_FocusInitiative(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public int SetFocusInitiative()
	{
		if (!m_Ability.ActiveBonusData.IsToggleBonus)
		{
			return m_Strength;
		}
		if (active)
		{
			return m_Strength;
		}
		return m_Actor.Initiative();
	}

	public void OnToggleInitiativeBonus(bool active)
	{
		this.active = active;
		CUpdateInitiativeTrack_MessageData message = new CUpdateInitiativeTrack_MessageData(GameState.InternalCurrentActor);
		ScenarioRuleClient.MessageHandler(message);
	}
}
