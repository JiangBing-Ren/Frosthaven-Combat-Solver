using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CTransferDamageActiveBonus : CActiveBonus
{
	public CTransferDamageActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
		foreach (EActiveBonusBehaviourType behaviour in ability.ActiveBonusData.Behaviours)
		{
			if (behaviour == EActiveBonusBehaviourType.TransferDamage)
			{
				AddBespokeBehaviour(new CTransferDamageActiveBonus_TransferDamage(actor, caster, ability, this));
			}
		}
	}

	public override bool TriggerTransferDamage(int damageTransfered, CActor damageSource, CActor actorDamaged, CAbility damagingAbility)
	{
		if (damageTransfered == 0)
		{
			return false;
		}
		if (Ability.MiscVars == null)
		{
			return false;
		}
		CAbilityFilter cAbilityFilter = Ability.GetMiscVar("TransferFilter").GetObject<CAbilityFilter>();
		if (cAbilityFilter == null)
		{
			return false;
		}
		int range = Ability.GetMiscVar("TransferRange").GetInt();
		bool ignoreLOS = Ability.GetMiscVar("TransferIgnoreLOS").GetBool();
		List<CActor> actorsInRange = GameState.GetActorsInRange(actorDamaged, actorDamaged, range, null, new CAbilityFilterContainer(new List<CAbilityFilter> { cAbilityFilter }), null, null, isTargetedAbility: false, null, true, originalTargetType: false, skipUntargetableCheck: false, ignoreLOS);
		if (actorsInRange == null)
		{
			return false;
		}
		CActor cActor = null;
		int num = int.MaxValue;
		foreach (CActor item in actorsInRange)
		{
			int tileDistance = ScenarioManager.GetTileDistance(actorDamaged.ArrayIndex, item.ArrayIndex);
			if (cActor == null || tileDistance < num)
			{
				cActor = item;
				num = tileDistance;
			}
			else if (tileDistance == num && item.Health < cActor.Health)
			{
				cActor = item;
			}
		}
		if (cActor == null)
		{
			return false;
		}
		CRedirectedAttack_MessageData message = new CRedirectedAttack_MessageData(actorDamaged)
		{
			m_Target = cActor,
			m_OriginalTarget = actorDamaged,
			m_ProjectileName = Ability.GetMiscVar("ProjectileName").GetString()
		};
		ScenarioRuleClient.MessageHandler(message);
		int health = cActor.Health;
		GameState.ActorBeenDamaged(cActor, damageTransfered, checkIfPlayerCanAvoidDamage: true, actorDamaged, Ability);
		if (GameState.ActorHealthCheck(actorDamaged, cActor))
		{
			CActorBeenDamaged_MessageData message2 = new CActorBeenDamaged_MessageData(cActor)
			{
				m_ActorBeingDamaged = cActor,
				m_DamageAbility = null,
				m_ActorOriginalHealth = health,
				m_ActualDamage = damageTransfered
			};
			ScenarioRuleClient.MessageHandler(message2);
		}
		UpdateXPTracker();
		FinishIfNoRemaining();
		return true;
	}
}
