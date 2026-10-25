namespace ScenarioRuleLibrary;

public class CAdjustInitiativeActiveBonus_AdjustInitiative : CBespokeBehaviour
{
	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.AdjustInitiative;

	public CAdjustInitiativeActiveBonus_AdjustInitiative(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		if (m_Ability.ActiveBonusData.IsToggleBonus && !activeBonus.ToggledBonus)
		{
			m_Strength = 0;
		}
		else
		{
			m_Strength = m_Ability.Strength;
		}
	}

	public void ApplyInitiative(bool positive)
	{
		m_Strength = m_Ability.Strength * (positive ? 1 : (-1));
		if (m_Ability.ActiveBonusData.IsToggleBonus && ActiveBonus.ToggledBonus)
		{
			ApplyInitiativePreview(m_Strength, m_Actor);
		}
	}

	public void ResetInitiative()
	{
		if (m_Ability.ActiveBonusData.IsToggleBonus && !ActiveBonus.ToggledBonus)
		{
			ApplyInitiativePreview(-m_Strength, m_Actor);
		}
		m_Strength = 0;
	}

	public static void ApplyInitiativePreview(int m_Strength, CActor m_Actor)
	{
		CClass cClass = m_Actor.Class;
		if (cClass != null)
		{
			cClass.UpdateInitiativeBonusPreview(m_Strength);
			if (cClass is CMonsterClass { NonEliteVariant: var nonEliteVariant } cMonsterClass)
			{
				nonEliteVariant?.UpdateInitiativeBonusPreview(m_Strength);
				cMonsterClass.EliteVariant?.UpdateInitiativeBonusPreview(m_Strength);
			}
		}
	}

	public void OnToggleInitiativeBonus(bool positive)
	{
		ApplyInitiative(positive);
		GameState.SortIntoInitiativeAndIDOrder();
		CUpdateInitiativeTrack_MessageData message = new CUpdateInitiativeTrack_MessageData(GameState.InternalCurrentActor);
		ScenarioRuleClient.MessageHandler(message);
	}

	public void OnUntoggleInitiativeBonus()
	{
		ResetInitiative();
		GameState.SortIntoInitiativeAndIDOrder();
		CUpdateInitiativeTrack_MessageData message = new CUpdateInitiativeTrack_MessageData(GameState.InternalCurrentActor);
		ScenarioRuleClient.MessageHandler(message);
	}

	public override bool IsHiddenInUI()
	{
		return m_Ability.ActiveBonusData.IsToggleBonus;
	}
}
