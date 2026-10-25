using System.Collections.Generic;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class CChallengeSwarmingTactics : CChallenge
{
	private List<string> actorsAppliedTo = new List<string>();

	public override uint ID => 1521u;

	public override string Name => "swarming_tactics";

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
		if (triggerParams.actor is CEnemyActor cEnemyActor && (cEnemyActor.Type == CActor.EType.Enemy || cEnemyActor.Type == CActor.EType.Enemy2) && !actorsAppliedTo.Contains(cEnemyActor.ActorGuid))
		{
			actorsAppliedTo.Add(cEnemyActor.ActorGuid);
			CAbilityMove obj = CAbilityMove.CreateDefaultMove(0, isMonster: true, 0, jump: true, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, isDefault: false) as CAbilityMove;
			obj.Name = "SwarmingTacticsJumpBuff";
			obj.ActiveBonusData.Duration = CActiveBonus.EActiveBonusDurationType.Persistent;
			obj.ActiveBonusData.Behaviour = CActiveBonus.EActiveBonusBehaviourType.BuffMove;
			obj.ActiveBonusData.Hidden = true;
			obj.Start(cEnemyActor, cEnemyActor);
			obj.ApplyToActor(cEnemyActor);
		}
	}
}
