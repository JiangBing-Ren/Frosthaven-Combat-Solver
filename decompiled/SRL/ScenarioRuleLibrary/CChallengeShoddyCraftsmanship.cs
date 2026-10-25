using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CChallengeShoddyCraftsmanship : CChallenge
{
	public override uint ID => 1507u;

	public override string Name => "shoddy_craftsmanship";

	public override IEnumerable<Trigger> GetTriggers()
	{
		yield return Trigger.OnItemSpent;
	}

	protected override void Perform(TriggerParams triggerParams = null)
	{
		base.Perform(triggerParams);
		CItem item = triggerParams.item;
		CPlayerActor cPlayerActor = triggerParams.actor as CPlayerActor;
		if (item != null)
		{
			cPlayerActor?.Inventory.ConsumeItem(item);
		}
	}
}
