using System;
using ScenarioRuleLibrary;
using UnityEngine;

namespace FHGUI;

public class MonsterStatConditionalElement : MonsterCardConditionalElement
{
	[SerializeField]
	private string _abilityKey;

	protected override bool CanShow(MonsterCardUIContext context)
	{
		int result = 0;
		try
		{
			if (!int.TryParse(AbilityCardActionLocalizedListener.LocalizeAbility("*" + _abilityKey + "*", context.CardYML.Name, context.CardYML.CardAction, new CardLayoutRow.PropertyLookupContext
			{
				Class = context.MonsterClass,
				Actor = context.EnemyForBaseStats,
				OverrideLevel = context.OverrideLevel
			}, (CAbility ability, int amount, string subtype, CAbilityOverride overrideAbility, int? overrideLevel) => MonsterRoundCardUI.GetStatValues(context.MonsterClass, ability, amount, subtype, overrideAbility, context.EnemyForBaseStats, overrideLevel)), out result))
			{
				return false;
			}
		}
		catch (Exception)
		{
			Log.GUIWarning($"Cannot calculate ability{_abilityKey} for monster card condition in card {context.CardYML.ID}");
		}
		return result > 0;
	}
}
