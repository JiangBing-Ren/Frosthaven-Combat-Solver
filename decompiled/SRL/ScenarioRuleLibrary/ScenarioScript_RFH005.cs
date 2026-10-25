using System.Collections.Generic;
using System.Linq;

namespace ScenarioRuleLibrary;

public class ScenarioScript_RFH005 : ScenarioScript
{
	public override bool IsActorConsideringElementAsStrong(CActor actor, ElementInfusionBoardManager.EElement element)
	{
		lock (ScenarioManager.Scenario.m_Objects)
		{
			foreach (CObjectActor item in ScenarioManager.Scenario.m_Objects.ToList())
			{
				if (item.FindApplicableActiveBonuses(CAbility.EAbilityType.NullTargeting).Any((CActiveBonus x) => x.Ability.GetMiscVar("TreatElementAsStrong").GetString() == element.ToString()))
				{
					return true;
				}
			}
		}
		return base.IsActorConsideringElementAsStrong(actor, element);
	}

	protected override List<object> GetConclusionParametersInternal()
	{
		int num = ScenarioManager.Scenario.AllPlayers.Sum((CPlayerActor x) => x.GetCharacterResourceAmount("LumberPile"));
		return new List<object> { num };
	}

	public override bool CanActorLootResources(CActor actor)
	{
		if (actor is CHeroSummonActor)
		{
			return true;
		}
		return base.CanActorLootResources(actor);
	}
}
