using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CRepeatAbilityActiveBonus : CActiveBonus
{
	public CRepeatAbilityActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? iD, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining)
	{
		AddBespokeBehaviour(new CRepeatAbilityBaseBehaviour(actor, ability, this));
	}

	public override void OnTurnEnded(CActor cur_turn_actor)
	{
		base.OnTurnEnded(cur_turn_actor);
		if (cur_turn_actor == Actor)
		{
			if (IsActorAgilityValid(Actor) || TrackerIndex == 0)
			{
				UpdateXPTracker();
			}
			if (HasTracker)
			{
				FinishIfNoRemaining();
			}
		}
	}

	public bool IsActorAgilityValid(CActor actor)
	{
		if (actor is CPlayerActor cPlayerActor)
		{
			return IsAgilityValid(cPlayerActor.CalcAgility());
		}
		return true;
	}

	public bool IsAgilityValid(CPlayerActor.EAgility agility)
	{
		if (agility == CPlayerActor.EAgility.Fast && (Ability.ActiveBonusData.Flags & AbilityData.EActiveBonusFlag.WhenFast) != 0)
		{
			return true;
		}
		if (agility == CPlayerActor.EAgility.Slow && (Ability.ActiveBonusData.Flags & AbilityData.EActiveBonusFlag.WhenSlow) != 0)
		{
			return true;
		}
		return false;
	}
}
