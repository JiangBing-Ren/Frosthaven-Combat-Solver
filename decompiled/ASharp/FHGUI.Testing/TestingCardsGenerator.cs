using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;

namespace FHGUI.Testing;

public class TestingCardsGenerator : MonoBehaviour
{
	public List<Vector2Int> FromToId = new List<Vector2Int>
	{
		new Vector2Int(61, 89),
		new Vector2Int(32, 60),
		new Vector2Int(120, 150)
	};

	private void Awake()
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Restart();
		int num = 0;
		foreach (Vector2Int item in FromToId)
		{
			int id;
			for (id = item.x; id <= item.y; id++)
			{
				AbilityCardYMLData abilityCardYMLData = ScenarioRuleClient.SRLYML.AbilityCards.FirstOrDefault((AbilityCardYMLData ac) => ac.ID == id);
				if (abilityCardYMLData != null)
				{
					PersistentData.CreateAbilityCard(abilityCardYMLData, out var newAbilityCard, out var _);
					ObjectPool.RecycleCard(id, ObjectPool.ECardType.Ability, newAbilityCard);
					num++;
				}
			}
		}
		foreach (MonsterCardYMLData monsterCard in ScenarioRuleClient.SRLYML.MonsterCards)
		{
			PersistentData.CreateMonsterCard(monsterCard, out var monsterCardGO);
			ObjectPool.RecycleCard(monsterCard.ID, ObjectPool.ECardType.Monster, monsterCardGO);
		}
		stopwatch.Stop();
		Log.Info($"Took {stopwatch.Elapsed.TotalSeconds} seconds to generate {num} cards");
	}
}
