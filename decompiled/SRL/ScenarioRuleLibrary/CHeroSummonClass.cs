using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CHeroSummonClass : CClass
{
	protected const int c_MaxIDs = 16;

	protected List<int> m_AvailableIDs;

	private HeroSummonYMLData m_summonYML;

	public List<int> AvailableIDs => m_AvailableIDs;

	public HeroSummonYMLData SummonYML => m_summonYML;

	public CHeroSummonClass(string classID, string model, string locKey, HeroSummonYMLData summonData)
		: base(classID, model.ToString(), locKey)
	{
		ResetHeroSummonStandeeIDs();
		m_summonYML = summonData;
	}

	public bool IsValidClassName()
	{
		if (ScenarioRuleClient.SRLYML.HeroSummons.SingleOrDefault((HeroSummonYMLData s) => s.ID == ID) == null)
		{
			return false;
		}
		return true;
	}

	public override void Reset()
	{
		base.Reset();
		ResetHeroSummonStandeeIDs();
		TemporaryCards.Clear();
	}

	public int GetNextID()
	{
		if (m_AvailableIDs.Count > 0)
		{
			int index = ScenarioManager.CurrentScenarioState.EnemyIDRNG.Next(m_AvailableIDs.Count);
			int result = m_AvailableIDs[index];
			m_AvailableIDs.RemoveAt(index);
			return result;
		}
		return -1;
	}

	public void RecycleID(int id)
	{
		if (!m_AvailableIDs.Contains(id))
		{
			m_AvailableIDs.Add(id);
		}
	}

	public void ResetHeroSummonStandeeIDs()
	{
		if (m_AvailableIDs == null)
		{
			m_AvailableIDs = new List<int>();
		}
		else
		{
			m_AvailableIDs.Clear();
		}
		for (int i = 1; i <= 16; i++)
		{
			m_AvailableIDs.Add(i);
		}
	}

	public void LoadAvailableIDs(List<int> availableIDs)
	{
		if (availableIDs != null)
		{
			m_AvailableIDs = availableIDs.ToList();
		}
	}

	public override CBaseCard FindCardWithAbility(CAbility ability, CActor actor)
	{
		foreach (CScenarioModifier scenarioModifier in ScenarioManager.CurrentScenarioState.ScenarioModifiers)
		{
			foreach (CAbility item in scenarioModifier.AllListedTriggerAbilities(copyAbilities: false))
			{
				if (item != null && item.HasID(ability.ID))
				{
					return scenarioModifier;
				}
			}
		}
		foreach (CAbilityCard temporaryCard in TemporaryCards)
		{
			if (temporaryCard.HasAbility(ability))
			{
				return temporaryCard;
			}
		}
		foreach (AttackModifierYMLData attackModifier in ScenarioRuleClient.SRLYML.AttackModifiers)
		{
			if (attackModifier.Abilities.Any((CAbility a) => a.HasID(ability.ID)))
			{
				return attackModifier.Card;
			}
		}
		if (actor is CHeroSummonActor cHeroSummonActor)
		{
			if (cHeroSummonActor.SummonData.OnSummonAbilities != null && cHeroSummonActor.SummonData.OnSummonAbilities.Any((CAbility a) => a.HasID(ability.ID)))
			{
				return cHeroSummonActor.BaseCard;
			}
			if (cHeroSummonActor.SummonData.OnDeathAbilities != null && cHeroSummonActor.SummonData.OnDeathAbilities.Any((CAbility a) => a.HasID(ability.ID)))
			{
				return cHeroSummonActor.BaseCard;
			}
			if (cHeroSummonActor.SummonData.SpecialAbilities != null && cHeroSummonActor.SummonData.SpecialAbilities.Any((CAbility a) => a.HasID(ability.ID)))
			{
				return cHeroSummonActor.BaseCard;
			}
			if (ability is CAbilitySummon && cHeroSummonActor.Summoner != null)
			{
				foreach (CAbilityCard activatedAbilityCard in cHeroSummonActor.Summoner.CharacterClass.ActivatedAbilityCards)
				{
					foreach (CActiveBonus activeBonuse in activatedAbilityCard.ActiveBonuses)
					{
						if (activeBonuse.Ability == ability)
						{
							return activeBonuse.BaseCard;
						}
					}
				}
			}
		}
		else if (actor is CPlayerActor cPlayerActor)
		{
			return cPlayerActor.CharacterClass.FindCardWithAbility(ability, actor);
		}
		return base.FindCardWithAbility(ability, actor);
	}

	public override IEnumerable<CActiveBonus> FindActiveBonuses(CActor actor)
	{
		if (ScenarioManager.Scenario?.AllAliveActorsThreadSafe == null)
		{
			yield break;
		}
		CBaseAbilityCard cBaseAbilityCard = (from x in ScenarioManager.Scenario.AllAliveActorsThreadSafe
			where x.Class == this
			select x as CHeroSummonActor).FirstOrDefault((CHeroSummonActor x) => x != null && x.BaseCard != null)?.BaseCard;
		if (cBaseAbilityCard == null)
		{
			yield break;
		}
		lock (cBaseAbilityCard.ActiveBonuses)
		{
			if (actor == null)
			{
				foreach (CActiveBonus activeBonuse in cBaseAbilityCard.ActiveBonuses)
				{
					yield return activeBonuse;
				}
				yield break;
			}
			foreach (CActiveBonus item in cBaseAbilityCard.GetActiveBonusesForActor(actor))
			{
				yield return item;
			}
		}
	}
}
