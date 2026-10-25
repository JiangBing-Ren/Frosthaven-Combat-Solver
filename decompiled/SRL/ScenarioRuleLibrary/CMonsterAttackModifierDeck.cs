using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CMonsterAttackModifierDeck
{
	private List<AttackModifierYMLData> m_AttackModifierCardsPool = new List<AttackModifierYMLData>();

	private List<AttackModifierYMLData> m_AttackModifierCards = new List<AttackModifierYMLData>();

	private List<AttackModifierYMLData> m_DiscardedAttackModifierCards = new List<AttackModifierYMLData>();

	private bool m_ShuffleAttackModifierCards;

	public List<AttackModifierYMLData> AttackModifierCards => m_AttackModifierCards;

	public List<AttackModifierYMLData> AttackModifierCardsPool => m_AttackModifierCardsPool;

	public List<AttackModifierYMLData> DiscardedAttackModifierCards => m_DiscardedAttackModifierCards;

	public void LoadCardPool(IEnumerable<AttackModifierYMLData> cardPool = null)
	{
		if (cardPool != null)
		{
			m_AttackModifierCardsPool = cardPool.ToList();
		}
		else if (ScenarioManager.HouseRulesSettings.HasFlag(StateShared.EHouseRulesFlag.ReducedRandomness))
		{
			m_AttackModifierCardsPool = ScenarioRuleClient.SRLYML.AttackModifierDecks.Single((AttackModifierDeckYMLData s) => s.IsMonsterDeck && s.IsReducedRandomnessDeck).GetAllAttackModifiers.ToList();
		}
		else
		{
			m_AttackModifierCardsPool = ScenarioRuleClient.SRLYML.AttackModifierDecks.Single((AttackModifierDeckYMLData s) => s.IsMonsterDeck && !s.IsReducedRandomnessDeck).GetAllAttackModifiers.ToList();
		}
	}

	public List<AttackModifierYMLData> DrawAttackModifierCards(CActor actor, int attackStrength, EAdvantageStatuses advStatus, out List<AttackModifierYMLData> notUsed, CAbilityAttack attackAbility, CActor currentTarget, out List<AttackModifierYMLData> cardsDrawnTotal, CAttackSummary.TargetSummary targetSummary)
	{
		if (m_AttackModifierCards.Count == 0 && m_DiscardedAttackModifierCards.Count == 0)
		{
			ResetAttackModifiers(null);
		}
		List<AttackModifierYMLData> result = CActor.DrawAttackModifierCards(actor, attackStrength, advStatus, m_AttackModifierCards, m_DiscardedAttackModifierCards, out var shuffle, out notUsed, attackAbility, currentTarget, out cardsDrawnTotal, targetSummary);
		m_ShuffleAttackModifierCards |= shuffle;
		return result;
	}

	public void CheckAttackModifierCardShuffle(bool force = false)
	{
		if (m_ShuffleAttackModifierCards | force)
		{
			if (GameState.ShuffleAttackModsEnabledForMonsters)
			{
				CActor.ShuffleAttackModifierCards(m_AttackModifierCards, m_DiscardedAttackModifierCards);
			}
			m_ShuffleAttackModifierCards = false;
		}
	}

	public void SetPoolAndLoadCards(AttackModifierDeckState deckState, IEnumerable<AttackModifierYMLData> cardPool = null, bool shuffle = false, bool forceResetAvailable = false)
	{
		ResetAttackModifiers(deckState, cardPool, forceResetAvailable);
		if (!forceResetAvailable)
		{
			LoadAttackModifierDeck(deckState);
		}
		if (shuffle)
		{
			RandomizeAvailableAttackModifierCardsOrder();
		}
	}

	public void ResetAttackModifiers(AttackModifierDeckState deckState, IEnumerable<AttackModifierYMLData> cardPool = null, bool forceResetAvailable = false)
	{
		LoadCardPool(cardPool);
		AttackModifierCards.Clear();
		m_DiscardedAttackModifierCards.Clear();
		m_ShuffleAttackModifierCards = false;
		if (!((ScenarioManager.CurrentScenarioState.EnemyClassManager == null || deckState?.AttackModifierCardsAvailable == null) | forceResetAvailable))
		{
			return;
		}
		foreach (AttackModifierYMLData item in m_AttackModifierCardsPool)
		{
			m_AttackModifierCards.Insert(ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(0, m_AttackModifierCards.Count), item);
		}
	}

	public void LoadAttackModifierDeck(AttackModifierDeckState deckState, bool allowShuffle = true)
	{
		if (deckState.AttackModifierCardsPool != null && deckState.AttackModifierCardsPool.Count > 0)
		{
			m_AttackModifierCardsPool.Clear();
			CClass.AddModifierCards(deckState.AttackModifierCardsPool, ref m_AttackModifierCardsPool, allowShuffle: false);
		}
		if (deckState.AttackModifierCardsAvailable != null)
		{
			m_AttackModifierCards.Clear();
			CClass.AddModifierCards(deckState.AttackModifierCardsAvailable, ref m_AttackModifierCards, allowShuffle: false);
		}
		if (deckState.AttackModifierCardsDiscarded != null)
		{
			m_DiscardedAttackModifierCards.Clear();
			CClass.AddModifierCards(deckState.AttackModifierCardsDiscarded, ref m_DiscardedAttackModifierCards, allowShuffle: false);
		}
	}

	public void RandomizeAvailableAttackModifierCardsOrder()
	{
		m_AttackModifierCards.Shuffle(ScenarioManager.CurrentScenarioState.ScenarioRNG);
	}

	public void AddAdditionalModifierCards(List<string> cardNames)
	{
		CClass.AddModifierCards(cardNames, ref m_AttackModifierCards);
	}

	public void RemoveModifierCards(List<string> cardNames)
	{
		CClass.RemoveModifierCards(cardNames, ref m_AttackModifierCards);
	}

	public void RemoveAllModifierCards(List<string> cardNames)
	{
		CClass.RemoveAllModifierCards(cardNames, ref m_AttackModifierCards);
	}

	public string DebugSetAttackMod()
	{
		AttackModifierYMLData item = AttackModifierCards[0];
		AttackModifierCards.Remove(item);
		AttackModifierCards.Add(item);
		return "Next: " + AttackModifierCards[0].Name;
	}

	public void SetModifierPool(IEnumerable<AttackModifierYMLData> mods, bool removeFromActiveCards)
	{
		AttackModifierCardsPool.Clear();
		AttackModifierCardsPool.AddRange(mods);
		if (removeFromActiveCards)
		{
			AttackModifierCards.Clear();
			AttackModifierCards.AddRange(mods);
		}
	}
}
