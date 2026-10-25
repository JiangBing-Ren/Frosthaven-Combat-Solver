namespace ScenarioRuleLibrary;

public class CAdjustInitiativeActiveBonus_SetInitiative : CBespokeBehaviour
{
	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.SetInitiative;

	public CAdjustInitiativeActiveBonus_SetInitiative(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnBehaviourTriggered()
	{
		base.OnBehaviourTriggered();
		if (m_Actor is CPlayerActor playerActor)
		{
			ResetInitiative(playerActor, refreshUI: false);
		}
	}

	public static void ApplyInitiative(int Strength, CPlayerActor playerActor, bool refreshUI = true)
	{
		playerActor.CharacterClass.UpdateSetInitiativePreview(Strength);
		if (refreshUI)
		{
			GameState.SortIntoInitiativeAndIDOrder();
			CUpdateInitiativeTrack_MessageData message = new CUpdateInitiativeTrack_MessageData(GameState.InternalCurrentActor);
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	public static void ResetInitiative(CPlayerActor playerActor, bool refreshUI = true)
	{
		playerActor.CharacterClass.ResetSetInitiative();
		if (refreshUI)
		{
			GameState.SortIntoInitiativeAndIDOrder();
			CUpdateInitiativeTrack_MessageData message = new CUpdateInitiativeTrack_MessageData(GameState.InternalCurrentActor);
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	public void OnToggleInitiativeBonus()
	{
		if (m_Actor is CPlayerActor playerActor)
		{
			ApplyInitiative(Strength, playerActor);
		}
	}

	public void OnUntoggleInitiativeBonus()
	{
		if (m_Actor is CPlayerActor playerActor)
		{
			ResetInitiative(playerActor);
		}
	}

	public override bool IsHiddenInUI()
	{
		return m_Ability.ActiveBonusData.IsToggleBonus;
	}
}
