using System.Linq;
using MapRuleLibrary.Adventure;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;

public static class RewardExtensions
{
	public static bool IsVisibleInUI(this Reward reward)
	{
		bool? flag = null;
		if (flag.HasValue)
		{
			return flag.Value;
		}
		if (reward.Type != ETreasureType.UnlockMerchant && reward.Type != ETreasureType.UnlockPartyUI && (reward.Type != ETreasureType.UnlockEnhancer || AdventureState.MapState.IsCampaign) && reward.Type != ETreasureType.UnlockTrainer && reward.Type != ETreasureType.UnlockTemple && reward.Type != ETreasureType.UnlockQuest && reward.Type != ETreasureType.CityEvent)
		{
			return reward.Type != ETreasureType.RoadEvent;
		}
		return false;
	}

	public static bool IsNegative(this Reward reward)
	{
		switch (reward.Type)
		{
		case ETreasureType.Damage:
		case ETreasureType.Discard:
		case ETreasureType.ConsumeItem:
		case ETreasureType.LoseItem:
			return true;
		case ETreasureType.Condition:
			if (reward.Conditions.SingleOrDefault().NegativeCondition != CCondition.ENegativeCondition.NA)
			{
				return true;
			}
			break;
		case ETreasureType.EnemyCondition:
			if (reward.EnemyConditions.SingleOrDefault().PositiveCondition != CCondition.EPositiveCondition.NA)
			{
				return true;
			}
			break;
		}
		if (reward.Amount != 0)
		{
			return reward.Amount < 0;
		}
		return false;
	}
}
