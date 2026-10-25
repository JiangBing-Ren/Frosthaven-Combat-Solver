using System.Collections.Generic;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class CChallengeDenseFog : CChallenge
{
	private List<string> actorsAppliedTo = new List<string>();

	public override uint ID => 1511u;

	public override string Name => "dense_fog";

	public override void Reset()
	{
		base.Reset();
		actorsAppliedTo.Clear();
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		if (!string.IsNullOrEmpty(storage))
		{
			actorsAppliedTo = JsonConvert.DeserializeObject<List<string>>(storage);
		}
	}

	public override IEnumerable<Trigger> GetTriggers()
	{
		yield return Trigger.OnSpawn;
	}

	protected override string SerializeInternal()
	{
		return JsonConvert.SerializeObject(actorsAppliedTo);
	}

	protected override void Perform(TriggerParams triggerParams = null)
	{
		CActor actor = triggerParams.actor;
		if ((actor is CPlayerActor || actor is CHeroSummonActor) && !actorsAppliedTo.Contains(actor.ActorGuid))
		{
			actorsAppliedTo.Add(actor.ActorGuid);
			CAbilityMove obj = CAbilityMove.CreateDefaultMove(0, isMonster: false, 0, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, isDefault: false) as CAbilityMove;
			obj.Name = "DenseFogDebuff";
			obj.ActiveBonusData.Duration = CActiveBonus.EActiveBonusDurationType.Persistent;
			obj.ActiveBonusData.Behaviour = CActiveBonus.EActiveBonusBehaviourType.BuffMove;
			obj.ActiveBonusData.Hidden = true;
			obj.Ability.SetMiscVar("MaxMovement", 4);
			obj.Start(actor, actor);
			obj.ApplyToActor(actor);
		}
	}
}
