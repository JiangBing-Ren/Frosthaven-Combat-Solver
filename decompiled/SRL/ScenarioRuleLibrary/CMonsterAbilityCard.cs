using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

[DebuggerDisplay("{ID} : {ClassID}")]
public class CMonsterAbilityCard : CBaseAbilityCard
{
	private CAction m_Action;

	private bool m_Shuffle;

	private MonsterCardYMLData m_monsterCardYML;

	public CAction Action => m_Action;

	public bool Shuffle => m_Shuffle;

	public MonsterCardYMLData GetMonsterCardYML => m_monsterCardYML;

	public CMonsterAbilityCard(int id, int initiative, CAction action, bool shuffle, string classID, MonsterCardYMLData monsterCardYML)
		: base(initiative, id, classID, ECardType.MonsterAbility, id.ToString())
	{
		m_Action = action;
		m_Shuffle = shuffle;
		m_monsterCardYML = monsterCardYML;
	}

	public override ActionType GetAbilityActionType(CAbility ability)
	{
		return ActionType.TopAction;
	}

	public override void Reset()
	{
		base.Reset();
		Action.Reset();
	}

	public CAbility FindAbilityOnCard(string name)
	{
		return GetAllAbilities(this).FirstOrDefault((CAbility a) => a.Name == name);
	}

	public static IEnumerable<CAbility> GetAllAbilities(CMonsterAbilityCard card)
	{
		foreach (CAbility actionAbility in CAbilityCard.GetActionAbilities(card.Action))
		{
			yield return actionAbility;
		}
	}
}
