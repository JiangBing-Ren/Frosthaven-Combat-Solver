using System;
using System.Linq;
using System.Text;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;

namespace FHGUI;

public class MonsterCardAbilityRowUI : MonoBehaviour
{
	[SerializeField]
	private string _abilityName;

	[Space]
	[SerializeField]
	private AbilityCardActionLocalizedListener _mainAction;

	[Space]
	[SerializeField]
	private TMP_Text _target;

	[SerializeField]
	private TMP_Text _range;

	[SerializeField]
	private TMP_Text Conditions;

	private StringBuilder _stringBuilder;

	public void Init(MonsterCardUIContext context)
	{
		CAbilityCard.GetActionAbilities(context.CardYML.CardAction).FirstOrDefault((CAbility a) => a.Name == _abilityName);
		InitInnateConditions(context.MonsterClass);
	}

	private void InitInnateConditions(CMonsterClass monsterClass)
	{
		if (_stringBuilder == null)
		{
			_stringBuilder = new StringBuilder();
		}
		foreach (CCondition.ENegativeCondition condition in monsterClass.CurrentMonsterStat.Conditions)
		{
			switch (condition)
			{
			case CCondition.ENegativeCondition.NA:
			case CCondition.ENegativeCondition.Poison:
			case CCondition.ENegativeCondition.Wound:
			case CCondition.ENegativeCondition.Immobilize:
			case CCondition.ENegativeCondition.Disarm:
			case CCondition.ENegativeCondition.Stun:
			case CCondition.ENegativeCondition.Muddle:
			case CCondition.ENegativeCondition.Curse:
			case CCondition.ENegativeCondition.Disadvantage:
			case CCondition.ENegativeCondition.StopFlying:
			case CCondition.ENegativeCondition.Sleep:
			case CCondition.ENegativeCondition.Bane:
			case CCondition.ENegativeCondition.Brittle:
			case CCondition.ENegativeCondition.Impair:
				continue;
			}
			throw new ArgumentOutOfRangeException();
		}
	}
}
