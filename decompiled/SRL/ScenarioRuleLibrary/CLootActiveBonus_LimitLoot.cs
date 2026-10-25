using System.Linq;

namespace ScenarioRuleLibrary;

public class CLootActiveBonus_LimitLoot : CBespokeBehaviour
{
	private CAbilityLoot m_LootAbility;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.LimitLoot;

	public CLootActiveBonus_LimitLoot(CActor actor, CAbilityLoot ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		ability.Strength = ability.Range;
		m_LootAbility = ability;
	}

	public bool CanLootObject(CActor actorLooting, ScenarioManager.ObjectImportType objectType)
	{
		if (m_LootAbility.AbilityLootData.LootableObjectImportTypes.Contains(objectType))
		{
			int num = 0;
			foreach (CObjectProp item in ScenarioManager.CurrentScenarioState.ActivatedProps.Where((CObjectProp p) => p.IsLootable && p.ObjectType == objectType).ToList())
			{
				if (item.ActorActivated == actorLooting.ActorGuid)
				{
					num++;
				}
			}
			if (num >= m_LootAbility.Strength)
			{
				return false;
			}
		}
		return true;
	}
}
