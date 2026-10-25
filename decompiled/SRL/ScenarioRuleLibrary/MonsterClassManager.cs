using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Libs.Threading;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class MonsterClassManager
{
	private static List<CMonsterClass> s_Classes = new List<CMonsterClass>();

	private static List<CObjectClass> s_ObjectClasses = new List<CObjectClass>();

	private static CMonsterAttackModifierDeck s_EnemyMonsterAttackModifierDeck = new CMonsterAttackModifierDeck();

	private static CMonsterAttackModifierDeck s_AlliedMonsterAttackModifierDeck = new CMonsterAttackModifierDeck();

	private static CMonsterAttackModifierDeck s_Enemy2MonsterAttackModifierDeck = new CMonsterAttackModifierDeck();

	private static CMonsterAttackModifierDeck s_NeutralMonsterAttackModifierDeck = new CMonsterAttackModifierDeck();

	public static List<CMonsterClass> Classes => s_Classes;

	public static List<CObjectClass> ObjectClasses => s_ObjectClasses;

	public static List<CMonsterClass> MonsterAndObjectClasses => s_Classes.Concat(s_ObjectClasses).ToList();

	public static IEnumerable<CMonsterClass> MonsterAndObjectClassesWithInitiative => Classes.Concat(ObjectClasses.Where((CObjectClass o) => o.AbilityCardsPool.Any()));

	public static CMonsterAttackModifierDeck EnemyMonsterAttackModifierDeck => s_EnemyMonsterAttackModifierDeck;

	public static CMonsterAttackModifierDeck AlliedMonsterAttackModifierDeck => s_AlliedMonsterAttackModifierDeck;

	public static CMonsterAttackModifierDeck Enemy2MonsterAttackModifierDeck => s_Enemy2MonsterAttackModifierDeck;

	public static CMonsterAttackModifierDeck NeutralMonsterAttackModifierDeck => s_NeutralMonsterAttackModifierDeck;

	public static int MonsterBlessCount => EnemyMonsterAttackModifierDeck.AttackModifierCards.Count((AttackModifierYMLData x) => x.IsBless) + AlliedMonsterAttackModifierDeck.AttackModifierCards.Count((AttackModifierYMLData x) => x.IsBless) + Enemy2MonsterAttackModifierDeck.AttackModifierCards.Count((AttackModifierYMLData x) => x.IsBless) + NeutralMonsterAttackModifierDeck.AttackModifierCards.Count((AttackModifierYMLData x) => x.IsBless);

	public static List<CMonsterAbilityCard> LoadMonsterCards(MonsterYMLData monster)
	{
		List<CMonsterAbilityCard> list = new List<CMonsterAbilityCard>();
		if (monster.AbilityCards != null)
		{
			foreach (MonsterCardYMLData abilityCard in monster.AbilityCards)
			{
				if (abilityCard != null && abilityCard.CardAction != null)
				{
					list.Add(new CMonsterAbilityCard(abilityCard.ID, abilityCard.Initiative, abilityCard.CardAction, abilityCard.Shuffle, monster.ID, abilityCard));
				}
				else
				{
					Log.ErrorWithReport("Invalid monster card in file " + abilityCard.FileName);
				}
			}
		}
		return list;
	}

	public static void Load()
	{
		CSRLYML.IsLoading = true;
		s_Classes.Clear();
		s_ObjectClasses.Clear();
		EnemyMonsterAttackModifierDeck.LoadCardPool();
		AlliedMonsterAttackModifierDeck.LoadCardPool();
		Enemy2MonsterAttackModifierDeck.LoadCardPool();
		NeutralMonsterAttackModifierDeck.LoadCardPool();
		if (DataLoadingFlags.HandleAbilitiesOnMainThread)
		{
			LoadMonsters();
		}
		else
		{
			LoadMonstersMT();
		}
		ValidateMonsterClassManager();
		CSRLYML.IsLoading = false;
	}

	private static void LoadMonstersMT()
	{
		ConcurrentQueue<CMonsterClass> monsterClasses = new ConcurrentQueue<CMonsterClass>();
		ConcurrentQueue<CObjectClass> objectClasses = new ConcurrentQueue<CObjectClass>();
		monsterClasses.Clear();
		JobSystem.ExecuteComplexJobs(ScenarioRuleClient.SRLYML.Monsters.Where((MonsterYMLData b) => string.IsNullOrEmpty(b.NonEliteVariant)).ToList(), (MonsterYMLData monster) =>
		{
			LoadMonster(monster, monsterClasses, objectClasses);
		});
		s_Classes.AddRange(monsterClasses);
		s_ObjectClasses.AddRange(objectClasses);
		monsterClasses.Clear();
		objectClasses.Clear();
		JobSystem.ExecuteComplexJobs(ScenarioRuleClient.SRLYML.Monsters.Where((MonsterYMLData b) => !string.IsNullOrEmpty(b.NonEliteVariant)).ToList(), (MonsterYMLData monster) =>
		{
			LoadMonster(monster, monsterClasses, objectClasses);
		});
		s_Classes.AddRange(monsterClasses);
		s_ObjectClasses.AddRange(objectClasses);
		s_Classes.Sort((CMonsterClass a, CMonsterClass b) => a.ID.CompareTo(b.ID));
		s_ObjectClasses.Sort((CObjectClass a, CObjectClass b) => a.ID.CompareTo(b.ID));
	}

	private static void LoadMonsters()
	{
		foreach (MonsterYMLData item in ScenarioRuleClient.SRLYML.Monsters.OrderBy((MonsterYMLData b) => !string.IsNullOrEmpty(b.NonEliteVariant)).ToList())
		{
			List<CMonsterAbilityCard> abilityCards = LoadMonsterCards(item);
			if (item.MonsterType == EMonsterType.Monster || item.MonsterType == EMonsterType.Boss)
			{
				s_Classes.Add(new CMonsterClass(item, abilityCards));
			}
			else if (item.MonsterType == EMonsterType.Object)
			{
				s_ObjectClasses.Add(new CObjectClass(item, abilityCards));
			}
			else
			{
				Log.ErrorWithReport("Invalid Monster Type! " + item.FileName);
			}
		}
		s_Classes.Sort((CMonsterClass a, CMonsterClass b) => a.ID.CompareTo(b.ID));
		s_ObjectClasses.Sort((CObjectClass a, CObjectClass b) => a.ID.CompareTo(b.ID));
	}

	private static void LoadMonster(MonsterYMLData monster, ConcurrentQueue<CMonsterClass> monsterClasses, ConcurrentQueue<CObjectClass> objectClasses)
	{
		List<CMonsterAbilityCard> abilityCards = LoadMonsterCards(monster);
		if (monster.MonsterType == EMonsterType.Monster || monster.MonsterType == EMonsterType.Boss)
		{
			monsterClasses.Enqueue(new CMonsterClass(monster, abilityCards));
		}
		else if (monster.MonsterType == EMonsterType.Object)
		{
			objectClasses.Enqueue(new CObjectClass(monster, abilityCards));
		}
		else
		{
			Log.ErrorWithReport("Invalid Monster Type! " + monster.FileName);
		}
	}

	public static void Unload()
	{
		s_Classes.Clear();
		s_ObjectClasses.Clear();
		s_EnemyMonsterAttackModifierDeck = new CMonsterAttackModifierDeck();
		s_AlliedMonsterAttackModifierDeck = new CMonsterAttackModifierDeck();
		s_Enemy2MonsterAttackModifierDeck = new CMonsterAttackModifierDeck();
		s_NeutralMonsterAttackModifierDeck = new CMonsterAttackModifierDeck();
	}

	public static void ValidateMonsterClassManager()
	{
	}

	public static void SelectRoundAbilityCards(bool reset = false)
	{
		foreach (CMonsterClass item in ScenarioManager.Scenario.AllAliveMonsters.Select((CEnemyActor m) => m.MonsterClass).Distinct())
		{
			if (reset)
			{
				item.ResetAbilityDeck();
			}
			item.SelectRoundAbilityCard();
		}
		foreach (CObjectClass objectClass in s_ObjectClasses)
		{
			if (ScenarioManager.Scenario.Objects.Find((CObjectActor x) => x.MonsterClass == objectClass) != null)
			{
				if (reset)
				{
					objectClass.ResetAbilityDeck();
				}
				objectClass.SelectRoundAbilityCard();
			}
		}
	}

	public static void DiscardRoundAbilityCards()
	{
		foreach (CMonsterClass monsterClass in s_Classes)
		{
			if (ScenarioManager.Scenario.AllMonsters.Find((CEnemyActor x) => x.MonsterClass == monsterClass) != null)
			{
				monsterClass.CardsPoolClass.DiscardRoundAbilityCard();
			}
		}
		foreach (CObjectClass objectMonsterClass in s_ObjectClasses)
		{
			if (ScenarioManager.Scenario.AllObjects.Find((CObjectActor x) => x.MonsterClass == objectMonsterClass) != null)
			{
				objectMonsterClass.CardsPoolClass.DiscardRoundAbilityCard();
			}
		}
	}

	public static List<AttackModifierYMLData> DrawAttackModifierCards(CActor actor, int attackStrength, EAdvantageStatuses advStatus, out List<AttackModifierYMLData> notUsed, CAbilityAttack attackAbility, CActor currentTarget, out List<AttackModifierYMLData> cardsDrawnTotal, CAttackSummary.TargetSummary targetSummary)
	{
		cardsDrawnTotal = new List<AttackModifierYMLData>();
		if (actor is CEnemyActor enemyActor)
		{
			return GetAttackModifiersDeck(enemyActor).DrawAttackModifierCards(actor, attackStrength, advStatus, out notUsed, attackAbility, currentTarget, out cardsDrawnTotal, targetSummary);
		}
		notUsed = new List<AttackModifierYMLData>();
		return null;
	}

	public static void CheckAllAttackModifierDecksForShuffle()
	{
		EnemyMonsterAttackModifierDeck.CheckAttackModifierCardShuffle();
		AlliedMonsterAttackModifierDeck.CheckAttackModifierCardShuffle();
		Enemy2MonsterAttackModifierDeck.CheckAttackModifierCardShuffle();
		NeutralMonsterAttackModifierDeck.CheckAttackModifierCardShuffle();
	}

	public static bool IsDeckEmpty(CEnemyActor checkActor)
	{
		return GetAttackModifiersDeck(checkActor).AttackModifierCards.Count <= 0;
	}

	public static void CheckAttackModifierCardShuffle(CEnemyActor forceShuffleForActor)
	{
		if (GameState.ShuffleAttackModsEnabledForMonsters)
		{
			GetAttackModifiersDeck(forceShuffleForActor).CheckAttackModifierCardShuffle(force: true);
		}
	}

	public static CMonsterClass Find(string classID)
	{
		return MonsterAndObjectClasses.FirstOrDefault((CMonsterClass x) => x.ID == classID);
	}

	public static CObjectClass FindObjectClass(string classID)
	{
		return s_ObjectClasses.SingleOrDefault((CObjectClass x) => x.ID == classID);
	}

	public static IEnumerable<CActiveBonus> FindAllActiveBonuses()
	{
		foreach (CEnemyActor allMonster in ScenarioManager.Scenario.AllMonsters)
		{
			foreach (CActiveBonus item in allMonster.MonsterClass.FindActiveBonuses(allMonster))
			{
				yield return item;
			}
		}
	}

	public static List<CActiveBonus> FindAllActiveBonuses(CActor actor)
	{
		List<CActiveBonus> list = new List<CActiveBonus>();
		if (actor is CEnemyActor cEnemyActor)
		{
			list.AddRange(cEnemyActor.MonsterClass.FindActiveBonuses(actor));
		}
		return list;
	}

	public static List<CActiveBonus> FindAllActiveBonuses(CAbility.EAbilityType type, CActor actor)
	{
		List<CMonsterClass> list = new List<CMonsterClass>();
		List<CActiveBonus> list2 = new List<CActiveBonus>();
		foreach (CActor allActor in ScenarioManager.Scenario.AllActors)
		{
			CMonsterClass cMonsterClass = allActor.Class as CMonsterClass;
			if (cMonsterClass == null)
			{
				continue;
			}
			if (cMonsterClass.NonEliteVariant != null)
			{
				cMonsterClass = cMonsterClass.NonEliteVariant;
			}
			if (!list.Contains(cMonsterClass))
			{
				list.Add(cMonsterClass);
				List<CActiveBonus> list3 = cMonsterClass.FindActiveBonuses(type, actor);
				if (list3 != null)
				{
					list2.AddRange(list3);
				}
			}
		}
		return list2;
	}

	public static List<CActiveBonus> FindAllActiveBonuses(CActiveBonus.EActiveBonusDurationType durationType, CActor actor)
	{
		List<CActiveBonus> list = new List<CActiveBonus>();
		if (actor is CEnemyActor cEnemyActor)
		{
			list.AddRange(cEnemyActor.MonsterClass.FindActiveBonuses(durationType, actor, includeIfCasterOfAura: true));
		}
		return list;
	}

	public static void SetAllMonsterStatLevels(int level, List<CStatBasedOnXOverrideDetails> statBasedOnXOverrides = null)
	{
		foreach (CMonsterClass monsterClass in MonsterAndObjectClasses)
		{
			List<AbilityData.StatIsBasedOnXData> additionalStatBasedOnXData = null;
			if (statBasedOnXOverrides != null && statBasedOnXOverrides.Any((CStatBasedOnXOverrideDetails s) => s.AssociatedClassID == monsterClass.ID))
			{
				additionalStatBasedOnXData = (from o in statBasedOnXOverrides
					where o.AssociatedClassID == monsterClass.ID
					select o.OverrideData).ToList();
			}
			monsterClass.SetMonsterStatLevel(level, 0, additionalStatBasedOnXData);
		}
	}

	public static CClass FindActiveBonusClass(CActiveBonus activeBonus)
	{
		new List<CActiveBonus>();
		foreach (CMonsterClass monsterAndObjectClass in MonsterAndObjectClasses)
		{
			if (monsterAndObjectClass.NonEliteVariant == null && monsterAndObjectClass.HasActiveBonus(activeBonus))
			{
				return monsterAndObjectClass;
			}
		}
		return null;
	}

	public static void Reset()
	{
		foreach (CMonsterClass monsterAndObjectClass in MonsterAndObjectClasses)
		{
			monsterAndObjectClass.Reset();
		}
		if (ScenarioManager.CurrentScenarioState != null)
		{
			ResetAttackModifiers();
		}
	}

	public static void ResetAttackModifiers()
	{
		EnemyMonsterAttackModifierDeck.ResetAttackModifiers(ScenarioManager.CurrentScenarioState.EnemyClassManager?.EnemyAttackModifierDeck);
		AlliedMonsterAttackModifierDeck.ResetAttackModifiers(ScenarioManager.CurrentScenarioState.EnemyClassManager?.AlliedMonsterAttackModifierDeck);
		Enemy2MonsterAttackModifierDeck.ResetAttackModifiers(ScenarioManager.CurrentScenarioState.EnemyClassManager?.Enemy2MonsterAttackModifierDeck);
		NeutralMonsterAttackModifierDeck.ResetAttackModifiers(ScenarioManager.CurrentScenarioState.EnemyClassManager?.NeutralMonsterAttackModifierDeck);
	}

	public static void RandomizeAllMonsterAvailableAttackModifierCardsOrder()
	{
		EnemyMonsterAttackModifierDeck.RandomizeAvailableAttackModifierCardsOrder();
		AlliedMonsterAttackModifierDeck.RandomizeAvailableAttackModifierCardsOrder();
		Enemy2MonsterAttackModifierDeck.RandomizeAvailableAttackModifierCardsOrder();
		NeutralMonsterAttackModifierDeck.RandomizeAvailableAttackModifierCardsOrder();
	}

	public static void AddAdditionalModifierCards(CEnemyActor enemyActor, List<string> cardNames)
	{
		GetAttackModifiersDeck(enemyActor)?.AddAdditionalModifierCards(cardNames);
	}

	public static void AddAdditionalModifierCardsToEnemies(List<string> cardNames)
	{
		EnemyMonsterAttackModifierDeck.AddAdditionalModifierCards(cardNames);
		Enemy2MonsterAttackModifierDeck.AddAdditionalModifierCards(cardNames);
	}

	public static void RemoveModifierCards(CEnemyActor enemyActor, List<string> cardNames)
	{
		CMonsterAttackModifierDeck attackModifiersDeck = GetAttackModifiersDeck(enemyActor);
		if (attackModifiersDeck != null)
		{
			RemoveModifierCards(attackModifiersDeck, cardNames);
		}
	}

	public static void RemoveModifierCards(CMonsterAttackModifierDeck deck, List<string> cardNames)
	{
		deck.RemoveModifierCards(cardNames);
	}

	public static void RemoveAllModifierCards(CMonsterAttackModifierDeck deck, List<string> cardNames)
	{
		deck.RemoveAllModifierCards(cardNames);
	}

	public static string DebugSetAttackMod(CEnemyActor enemyActor)
	{
		CMonsterAttackModifierDeck attackModifiersDeck = GetAttackModifiersDeck(enemyActor);
		if (attackModifiersDeck == null)
		{
			return string.Empty;
		}
		return attackModifiersDeck.DebugSetAttackMod();
	}

	public static CMonsterAttackModifierDeck GetAttackModifiersDeck(CEnemyActor enemyActor)
	{
		string deckID;
		return GetAttackModifiersDeck(enemyActor, out deckID);
	}

	public static CMonsterAttackModifierDeck GetAttackModifiersDeck(CEnemyActor enemyActor, out string deckID)
	{
		switch (enemyActor.OriginalType)
		{
		case CActor.EType.Ally:
			deckID = "Ally";
			return AlliedMonsterAttackModifierDeck;
		case CActor.EType.Neutral:
			deckID = "Neutral";
			return NeutralMonsterAttackModifierDeck;
		case CActor.EType.Enemy:
			deckID = "Enemy";
			return EnemyMonsterAttackModifierDeck;
		case CActor.EType.Enemy2:
			deckID = "Enemy2";
			return Enemy2MonsterAttackModifierDeck;
		default:
			deckID = null;
			return null;
		}
	}

	public static int GetCurseCardCount(CEnemyActor enemyActor)
	{
		return GetCurseCards(enemyActor)?.Count ?? 0;
	}

	public static List<AttackModifierYMLData> GetCurseCards(CEnemyActor enemyActor)
	{
		return GetAttackModifiersDeck(enemyActor)?.AttackModifierCards.Where((AttackModifierYMLData x) => x.IsCurse).ToList();
	}

	public static int RemoveAllCurseCards(CEnemyActor enemyActor)
	{
		CMonsterAttackModifierDeck attackModifiersDeck = GetAttackModifiersDeck(enemyActor);
		if (attackModifiersDeck == null)
		{
			return 0;
		}
		List<AttackModifierYMLData> curseCards = GetCurseCards(enemyActor);
		foreach (AttackModifierYMLData item in curseCards)
		{
			attackModifiersDeck.AttackModifierCards.Remove(item);
		}
		return curseCards.Count;
	}

	public static void AddCurseCard(CEnemyActor enemyActor)
	{
		CMonsterAttackModifierDeck attackModifiersDeck = GetAttackModifiersDeck(enemyActor);
		if (attackModifiersDeck != null)
		{
			AddCurseCard(attackModifiersDeck);
		}
	}

	public static void AddCurseCard(CMonsterAttackModifierDeck deck)
	{
		CClass.AddCurseCardToDeck(deck.AttackModifierCards);
	}

	public static void AddBlessCardToEnemies()
	{
		CClass.AddBlessCardToDeck(EnemyMonsterAttackModifierDeck.AttackModifierCards);
		CClass.AddBlessCardToDeck(Enemy2MonsterAttackModifierDeck.AttackModifierCards);
	}

	public static void AddBlessCard(CEnemyActor enemyActor)
	{
		CMonsterAttackModifierDeck attackModifiersDeck = GetAttackModifiersDeck(enemyActor);
		if (attackModifiersDeck != null)
		{
			CClass.AddBlessCardToDeck(attackModifiersDeck.AttackModifierCards);
		}
	}
}
