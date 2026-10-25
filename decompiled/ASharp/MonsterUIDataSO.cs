using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;

public class MonsterUIDataSO : ScriptableObject
{
	[Serializable]
	public struct MonsterCardPrefabData
	{
		public int ID;

		public MonsterRoundCardUI Card;
	}

	[SerializeField]
	private MonsterConfigUI[] _monsters;

	[SerializeField]
	private List<MonsterRoundCardUI> _cards;

	[Tooltip("There are duplicates of other cards but with custom ids")]
	[SerializeField]
	private List<MonsterCardPrefabData> _alternativeCards;

	[Header("Card prefab bases")]
	public MonsterRoundCardUI BasePrefab;

	private Dictionary<string, MonsterConfigUI> _monsterConfigMapper;

	private Dictionary<int, MonsterRoundCardUI> _monsterCardPrefabMapper;

	public Dictionary<string, MonsterConfigUI> MonsterConfigs
	{
		get
		{
			if (_monsterConfigMapper == null)
			{
				BuildConfigMapper();
			}
			return _monsterConfigMapper;
		}
	}

	public Dictionary<int, MonsterRoundCardUI> MonsterCards
	{
		get
		{
			if (_monsterCardPrefabMapper == null)
			{
				BuildCardMapper();
			}
			return _monsterCardPrefabMapper;
		}
	}

	public int GetAlternativeCardID(int cardID, out MonsterRoundCardUI card)
	{
		card = _alternativeCards.FirstOrDefault((MonsterCardPrefabData c) => c.ID == cardID).Card;
		if (!(card == null))
		{
			return card.CardID;
		}
		return -1;
	}

	private void OnEnable()
	{
		BuildConfigMapper();
		BuildCardMapper();
	}

	private void OnDisable()
	{
		_monsterConfigMapper = null;
		_monsterCardPrefabMapper = null;
	}

	public MonsterConfigUI GetConfig(string monsterID)
	{
		MonsterConfigs.TryGetValue(monsterID, out var value);
		return value;
	}

	public MonsterConfigUI GetConfig(string monsterID, string modelName)
	{
		if (!modelName.IsNOTNullOrEmpty())
		{
			return GetConfig(monsterID);
		}
		return GetConfig(modelName) ?? GetConfig(monsterID);
	}

	public MonsterConfigUI GetConfig(MonsterYMLData monster)
	{
		return GetConfig(monster.ID);
	}

	public MonsterConfigUI GetConfig(CMonsterClass monster)
	{
		return GetConfig(monster.ID);
	}

	public IEnumerable<MonsterRoundCardUI> GetMonsterCards(MonsterYMLData monster)
	{
		if (monster == null)
		{
			yield break;
		}
		foreach (MonsterCardYMLData abilityCard in monster.AbilityCards)
		{
			MonsterCards.TryGetValue(abilityCard.ID, out var value);
			if (value != null)
			{
				yield return value;
			}
		}
	}

	public IEnumerable<MonsterRoundCardUI> GetMonsterCards(CMonsterClass monster)
	{
		return GetMonsterCards(monster.MonsterYML);
	}

	public MonsterRoundCardUI GetMonsterCardPrefab(int cardID)
	{
		MonsterCards.TryGetValue(cardID, out var value);
		return value;
	}

	private void BuildConfigMapper()
	{
		_monsterConfigMapper = _monsters.ToDictionary((MonsterConfigUI config) => config.ID, (MonsterConfigUI config) => config);
		foreach (MonsterConfigUI item in _monsters.Where((MonsterConfigUI c) =>
		{
			MonsterConfigUI.ModelVariant[] variants2 = c.Variants;
			return variants2 != null && variants2.Length > 0;
		}))
		{
			MonsterConfigUI.ModelVariant[] variants = item.Variants;
			for (int num = 0; num < variants.Length; num++)
			{
				MonsterConfigUI.ModelVariant modelVariant = variants[num];
				if (!_monsterConfigMapper.ContainsKey(modelVariant.ModelName))
				{
					_monsterConfigMapper[modelVariant.ModelName] = modelVariant.ConfigUI;
					continue;
				}
				Log.GUIWarning("Duplicate Variant key for model " + modelVariant.ModelName + " found!\nFirst config: [" + _monsterConfigMapper[modelVariant.ModelName].name + "]", item);
			}
		}
	}

	private void BuildCardMapper()
	{
		_monsterCardPrefabMapper = _cards.ToDictionary((MonsterRoundCardUI card) => card.CardID, (MonsterRoundCardUI card) => card);
		foreach (MonsterCardPrefabData alternativeCard in _alternativeCards)
		{
			if (_monsterCardPrefabMapper.ContainsKey(alternativeCard.ID))
			{
				_monsterCardPrefabMapper[alternativeCard.ID] = alternativeCard.Card;
			}
			else
			{
				_monsterCardPrefabMapper.TryAdd(alternativeCard.ID, alternativeCard.Card);
			}
		}
	}
}
