namespace ScenarioRuleLibrary;

public class CChangeCharacterModelActiveBonus : CActiveBonus
{
	private CActor m_PlayerActor;

	public CAbilityChangeCharacterModel ChangeCharacterModelAbility { get; set; }

	public CChangeCharacterModelActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? iD, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining)
	{
		ChangeCharacterModelAbility = (CAbilityChangeCharacterModel)ability;
		m_PlayerActor = actor;
		actor.OverrideCharacterModel = ChangeCharacterModelAbility.CharacterModel.ToString();
		if (ChangeCharacterModelAbility.OverrideType.HasValue)
		{
			actor.OverrideCharacterModelActorType = ChangeCharacterModelAbility.OverrideType.Value;
		}
	}

	public override void Finish()
	{
		m_PlayerActor.OverrideCharacterModel = null;
		m_PlayerActor.OverrideCharacterModelActorType = null;
		CChangeCharacterModel_MessageData cChangeCharacterModel_MessageData = new CChangeCharacterModel_MessageData(m_PlayerActor);
		cChangeCharacterModel_MessageData.m_ChangeCharacterAbility = ChangeCharacterModelAbility;
		cChangeCharacterModel_MessageData.AnimOverload = Ability.ActiveBonusData.ActiveBonusAnimOverload;
		ScenarioRuleClient.MessageHandler(cChangeCharacterModel_MessageData);
		base.Finish();
	}
}
