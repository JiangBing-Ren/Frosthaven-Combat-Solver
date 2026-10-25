using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.Elements;
using ScenarioRuleLibrary;

namespace FHGUI.Common;

public class UIActiveAbilityListContainer : ListContainerBase<UIActiveAbility, object, ActiveAbilityDTO>
{
	protected override void InitElement(UIActiveAbility listElement, ActiveAbilityDTO activeAbility, object listParams)
	{
		listElement.Initialize(activeAbility);
		if (!(activeAbility.Bonus?.Ability is CAbilityAddActiveBonus { AddAbility: CAbilityChooseAbility addAbility }) || !addAbility.GetMiscVar("ShowMonsterCards").GetBool())
		{
			return;
		}
		string monsterClassKey = addAbility.GetMiscVar("MonsterClassID").GetString();
		CMonsterClass cMonsterClass = MonsterClassManager.Classes.Find((CMonsterClass x) => x.NonEliteVariant == null && x.ID.StartsWith(monsterClassKey, StringComparison.InvariantCultureIgnoreCase));
		List<CMonsterAbilityCard> list = new List<CMonsterAbilityCard>();
		foreach (CAbility chooseAbility in addAbility.ChooseAbilities)
		{
			int id = chooseAbility.GetMiscVar("CardIdToShow").GetInt();
			CMonsterAbilityCard cMonsterAbilityCard = cMonsterClass.AbilityCardsPool.FirstOrDefault((CMonsterAbilityCard x) => x.ID == id);
			if (cMonsterAbilityCard != null)
			{
				list.Add(cMonsterAbilityCard);
			}
		}
		listElement.ShowMonsterCards(list, cMonsterClass);
	}
}
