using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Libs.GameConsole;
using Libs.Utils;
using SharedLibrary;

namespace ScenarioRuleLibrary;

public class CLootDeck
{
	private static LootCard[] _allCards = null;

	public static LootCard[] AdditionalLootCards = null;

	public static readonly LootCard[] DefaultCards = new LootCard[59]
	{
		new LootCard
		{
			Type = LootCard.EType.None
		},
		new LootCard
		{
			Type = LootCard.EType.Money,
			Amount = LootCard.EAmount.Plus1,
			ID = 1361
		},
		new LootCard
		{
			Type = LootCard.EType.Money,
			Amount = LootCard.EAmount.Plus1,
			ID = 1362
		},
		new LootCard
		{
			Type = LootCard.EType.Money,
			Amount = LootCard.EAmount.Plus1,
			ID = 1363
		},
		new LootCard
		{
			Type = LootCard.EType.Money,
			Amount = LootCard.EAmount.Plus1,
			ID = 1364
		},
		new LootCard
		{
			Type = LootCard.EType.Money,
			Amount = LootCard.EAmount.Plus1,
			ID = 1365
		},
		new LootCard
		{
			Type = LootCard.EType.Money,
			Amount = LootCard.EAmount.Plus1,
			ID = 1366
		},
		new LootCard
		{
			Type = LootCard.EType.Money,
			Amount = LootCard.EAmount.Plus1,
			ID = 1367
		},
		new LootCard
		{
			Type = LootCard.EType.Money,
			Amount = LootCard.EAmount.Plus1,
			ID = 1368
		},
		new LootCard
		{
			Type = LootCard.EType.Money,
			Amount = LootCard.EAmount.Plus1,
			ID = 1369
		},
		new LootCard
		{
			Type = LootCard.EType.Money,
			Amount = LootCard.EAmount.Plus1,
			ID = 1370
		},
		new LootCard
		{
			Type = LootCard.EType.Money,
			Amount = LootCard.EAmount.Plus1,
			ID = 1371
		},
		new LootCard
		{
			Type = LootCard.EType.Money,
			Amount = LootCard.EAmount.Plus1,
			ID = 1372
		},
		new LootCard
		{
			Type = LootCard.EType.Money,
			Amount = LootCard.EAmount.Plus2,
			ID = 1373
		},
		new LootCard
		{
			Type = LootCard.EType.Money,
			Amount = LootCard.EAmount.Plus2,
			ID = 1374
		},
		new LootCard
		{
			Type = LootCard.EType.Money,
			Amount = LootCard.EAmount.Plus2,
			ID = 1375
		},
		new LootCard
		{
			Type = LootCard.EType.Money,
			Amount = LootCard.EAmount.Plus2,
			ID = 1376
		},
		new LootCard
		{
			Type = LootCard.EType.Money,
			Amount = LootCard.EAmount.Plus2,
			ID = 1377
		},
		new LootCard
		{
			Type = LootCard.EType.Money,
			Amount = LootCard.EAmount.Plus2,
			ID = 1378
		},
		new LootCard
		{
			Type = LootCard.EType.Money,
			Amount = LootCard.EAmount.Plus3,
			ID = 1379
		},
		new LootCard
		{
			Type = LootCard.EType.Money,
			Amount = LootCard.EAmount.Plus3,
			ID = 1380
		},
		new LootCard
		{
			Type = LootCard.EType.Arrowvine,
			Amount = LootCard.EAmount.Plus1,
			ID = 1381
		},
		new LootCard
		{
			Type = LootCard.EType.Arrowvine,
			Amount = LootCard.EAmount.Plus1,
			ID = 1382
		},
		new LootCard
		{
			Type = LootCard.EType.Axenut,
			Amount = LootCard.EAmount.Plus1,
			ID = 1383
		},
		new LootCard
		{
			Type = LootCard.EType.Axenut,
			Amount = LootCard.EAmount.Plus1,
			ID = 1384
		},
		new LootCard
		{
			Type = LootCard.EType.Corpsecap,
			Amount = LootCard.EAmount.Plus1,
			ID = 1385
		},
		new LootCard
		{
			Type = LootCard.EType.Corpsecap,
			Amount = LootCard.EAmount.Plus1,
			ID = 1386
		},
		new LootCard
		{
			Type = LootCard.EType.Flamefruit,
			Amount = LootCard.EAmount.Plus1,
			ID = 1387
		},
		new LootCard
		{
			Type = LootCard.EType.Flamefruit,
			Amount = LootCard.EAmount.Plus1,
			ID = 1388
		},
		new LootCard
		{
			Type = LootCard.EType.Rockroot,
			Amount = LootCard.EAmount.Plus1,
			ID = 1389
		},
		new LootCard
		{
			Type = LootCard.EType.Rockroot,
			Amount = LootCard.EAmount.Plus1,
			ID = 1390
		},
		new LootCard
		{
			Type = LootCard.EType.Snowthistle,
			Amount = LootCard.EAmount.Plus1,
			ID = 1391
		},
		new LootCard
		{
			Type = LootCard.EType.Snowthistle,
			Amount = LootCard.EAmount.Plus1,
			ID = 1392
		},
		new LootCard
		{
			Type = LootCard.EType.Hide,
			Amount = LootCard.EAmount.Plus1if4_Plus2if23,
			ID = 1393
		},
		new LootCard
		{
			Type = LootCard.EType.Hide,
			Amount = LootCard.EAmount.Plus1if4_Plus2if23,
			ID = 1394
		},
		new LootCard
		{
			Type = LootCard.EType.Hide,
			Amount = LootCard.EAmount.Plus1if4_Plus2if23,
			ID = 1395
		},
		new LootCard
		{
			Type = LootCard.EType.Hide,
			Amount = LootCard.EAmount.Plus1if4_Plus2if23,
			ID = 1395
		},
		new LootCard
		{
			Type = LootCard.EType.Hide,
			Amount = LootCard.EAmount.Plus1if34_Plus2if2,
			ID = 1396
		},
		new LootCard
		{
			Type = LootCard.EType.Hide,
			Amount = LootCard.EAmount.Plus1if34_Plus2if2,
			ID = 1397
		},
		new LootCard
		{
			Type = LootCard.EType.Hide,
			Amount = LootCard.EAmount.Plus1if34_Plus2if2,
			ID = 1398
		},
		new LootCard
		{
			Type = LootCard.EType.Hide,
			Amount = LootCard.EAmount.Plus1,
			ID = 1399
		},
		new LootCard
		{
			Type = LootCard.EType.Hide,
			Amount = LootCard.EAmount.Plus1,
			ID = 1400
		},
		new LootCard
		{
			Type = LootCard.EType.Lumber,
			Amount = LootCard.EAmount.Plus1if4_Plus2if23,
			ID = 1401
		},
		new LootCard
		{
			Type = LootCard.EType.Lumber,
			Amount = LootCard.EAmount.Plus1if4_Plus2if23,
			ID = 1402
		},
		new LootCard
		{
			Type = LootCard.EType.Lumber,
			Amount = LootCard.EAmount.Plus1if4_Plus2if23,
			ID = 1403
		},
		new LootCard
		{
			Type = LootCard.EType.Lumber,
			Amount = LootCard.EAmount.Plus1if34_Plus2if2,
			ID = 1404
		},
		new LootCard
		{
			Type = LootCard.EType.Lumber,
			Amount = LootCard.EAmount.Plus1if34_Plus2if2,
			ID = 1405
		},
		new LootCard
		{
			Type = LootCard.EType.Lumber,
			Amount = LootCard.EAmount.Plus1if34_Plus2if2,
			ID = 1406
		},
		new LootCard
		{
			Type = LootCard.EType.Lumber,
			Amount = LootCard.EAmount.Plus1,
			ID = 1407
		},
		new LootCard
		{
			Type = LootCard.EType.Lumber,
			Amount = LootCard.EAmount.Plus1,
			ID = 1408
		},
		new LootCard
		{
			Type = LootCard.EType.Metal,
			Amount = LootCard.EAmount.Plus1if4_Plus2if23,
			ID = 1409
		},
		new LootCard
		{
			Type = LootCard.EType.Metal,
			Amount = LootCard.EAmount.Plus1if4_Plus2if23,
			ID = 1410
		},
		new LootCard
		{
			Type = LootCard.EType.Metal,
			Amount = LootCard.EAmount.Plus1if4_Plus2if23,
			ID = 1411
		},
		new LootCard
		{
			Type = LootCard.EType.Metal,
			Amount = LootCard.EAmount.Plus1if34_Plus2if2,
			ID = 1412
		},
		new LootCard
		{
			Type = LootCard.EType.Metal,
			Amount = LootCard.EAmount.Plus1if34_Plus2if2,
			ID = 1413
		},
		new LootCard
		{
			Type = LootCard.EType.Metal,
			Amount = LootCard.EAmount.Plus1if34_Plus2if2,
			ID = 1414
		},
		new LootCard
		{
			Type = LootCard.EType.Metal,
			Amount = LootCard.EAmount.Plus1,
			ID = 1415
		},
		new LootCard
		{
			Type = LootCard.EType.Metal,
			Amount = LootCard.EAmount.Plus1,
			ID = 1416
		},
		new LootCard
		{
			Type = LootCard.EType.RandomItem,
			Amount = LootCard.EAmount.Plus1,
			ID = 1417
		}
	};

	private static byte[] _numCardsPerType;

	public static LootCard[] AllCards
	{
		get
		{
			if (_allCards == null || _allCards.Length == 0)
			{
				return DefaultCards;
			}
			return _allCards;
		}
		set
		{
			_allCards = value;
		}
	}

	public static byte[] numCardsPerType
	{
		get
		{
			if (_numCardsPerType == null)
			{
				_numCardsPerType = new byte[LootCard.AllTypes.Length];
				LootCard[] allCards = AllCards;
				for (int i = 0; i < allCards.Length; i++)
				{
					LootCard lootCard = allCards[i];
					numCardsPerType[(int)lootCard.Type]++;
				}
			}
			return _numCardsPerType;
		}
	}

	public static byte[] RandomScenarioLootTable(SharedLibrary.Random rng)
	{
		byte[] array = new byte[LootCard.AllTypes.Length];
		LootCard.EType[] allTypes = LootCard.AllTypes;
		foreach (LootCard.EType eType in allTypes)
		{
			rng.Next();
			array[(int)eType] = (byte)(rng.Next() % numCardsPerType[(int)eType]);
		}
		return array;
	}

	public static void Generate(List<byte> deck, SharedLibrary.Random rng, byte[] scenarioLootTable)
	{
		deck.Clear();
		List<byte>[] array = new List<byte>[LootCard.AllTypes.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new List<byte>();
		}
		for (int j = 1; j < AllCards.Length; j++)
		{
			LootCard card = AllCards[j];
			if (AdditionalLootCards == null || !AdditionalLootCards.Any((LootCard x) => x.ID == card.ID))
			{
				array[(int)card.Type].Add((byte)j);
			}
		}
		for (int num = 0; num < array.Length; num++)
		{
			array[num].Shuffle(rng);
		}
		LootCard.EType[] allTypes = LootCard.AllTypes;
		foreach (LootCard.EType eType in allTypes)
		{
			if (eType != LootCard.EType.None)
			{
				int num3 = (int)eType;
				int num4 = scenarioLootTable[num3];
				for (int num5 = 0; num5 < num4; num5++)
				{
					deck.Add(array[num3][num5]);
				}
			}
		}
		if (AdditionalLootCards != null)
		{
			LootCard[] additionalLootCards = AdditionalLootCards;
			for (int num2 = 0; num2 < additionalLootCards.Length; num2++)
			{
				LootCard card2 = additionalLootCards[num2];
				deck.Add((byte)AllCards.FindIndex((LootCard x) => x.ID == card2.ID));
			}
		}
		deck.Shuffle(rng);
		deck.Add(0);
	}

	public static bool DrawNext(List<byte> deck, out LootCard lc)
	{
		if (deck.Count == 0 || deck[0] == 0)
		{
			lc = default;
			return false;
		}
		lc = AllCards[deck[0]];
		deck.Add(deck[0]);
		deck.RemoveAt(0);
		return true;
	}

	public static byte GetDelimiterID(List<byte> deck)
	{
		return (byte)deck.FindIndex((byte x) => x == 0);
	}

	public static void AddCardBeforeDelimiter(List<byte> deck, LootCard lc)
	{
		int num = AllCards.FindIndex((LootCard x) => x == lc);
		byte delimiterID = GetDelimiterID(deck);
		deck.Insert(delimiterID, (byte)num);
	}

	public static void RemoveCardFromDeck(List<byte> deck, LootCard lc)
	{
		int num = AllCards.FindIndex((LootCard x) => x == lc);
		if (num > 0)
		{
			deck.Remove((byte)num);
		}
	}

	public static void DiscardCard(List<byte> deck, LootCard lc)
	{
		int num = AllCards.FindIndex((LootCard x) => x == lc);
		deck.Remove((byte)num);
		int num2 = deck.FindIndex((byte x) => x == 0);
		deck.Insert(num2 + 1, (byte)num);
	}

	public static int CalcLootAmount(LootCard lc, ScenarioState css)
	{
		int count = css.Players.Count;
		int num = 0;
		switch (lc.Amount)
		{
		case LootCard.EAmount.Plus1:
			num = 1;
			break;
		case LootCard.EAmount.Plus2:
			num = 2;
			break;
		case LootCard.EAmount.Plus3:
			num = 3;
			break;
		case LootCard.EAmount.Plus1if4_Plus2if23:
			num = ((count >= 4) ? 1 : 2);
			break;
		case LootCard.EAmount.Plus1if34_Plus2if2:
			num = ((count >= 3) ? 1 : 2);
			break;
		}
		return num + (int)lc.Bonus;
	}

	[ConsoleCommand(Command = "set_loot_deck", Description = "[Money:3, RandomItem:1, ...] Sets the loot deck for the current scenario", PassAllArguments = true)]
	public static void SetLootDeckCommand(IConsole console, string args)
	{
		MatchCollection matchCollection = new Regex("([a-zA-Z]*):(\\d+)").Matches(args);
		if (!matchCollection.Any())
		{
			string text = string.Join(", ", LootCard.AllTypes.Skip(1));
			console.WriteLine("Valid card types are '" + text + "'");
			return;
		}
		List<byte> lootDeck = ScenarioManager.CurrentScenarioState.LootDeck;
		lootDeck.Clear();
		foreach (Match item in matchCollection)
		{
			string value = item.Groups[1].Value;
			if (!Enum.TryParse<LootCard.EType>(value, ignoreCase: true, out var result))
			{
				console.WriteLineWithStyle("Unknown card type '" + value + "'", CommandOutputStyle.Error);
				continue;
			}
			int num = int.Parse(item.Groups[2].Value);
			int num2 = -1;
			int i = 0;
			for (int num3 = AllCards.Length; i < num3; i++)
			{
				LootCard lootCard = AllCards[i];
				if (lootCard.Type == result && lootCard.Amount == LootCard.EAmount.Plus1)
				{
					num2 = i;
					break;
				}
			}
			if (num2 != -1)
			{
				for (int j = 0; j < num; j++)
				{
					lootDeck.Add((byte)num2);
				}
			}
		}
		lootDeck.Add(0);
	}
}
