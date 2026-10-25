using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Libs.Threading;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CharacterClassManager
{
	private static List<CCharacterClass> s_Classes = new List<CCharacterClass>();

	private static List<CHeroSummonClass> s_HeroSummonClasses = new List<CHeroSummonClass>();

	private static AbilityCardYMLData s_LongRestLayout = null;

	public static List<CCharacterClass> PerksClass;

	public static List<CCharacterClass> Classes => s_Classes;

	public static List<CHeroSummonClass> HeroSummonClasses => s_HeroSummonClasses;

	public static List<CAbilityCard> AllAbilityCards { get; private set; }

	public static List<CAbilityCard> AllAbilityCardsUnmodifiedForUI { get; private set; }

	public static List<CAbilityCard> AllAbilityCardInstances { get; private set; }

	public static AbilityCardYMLData LongRestLayout => s_LongRestLayout;

	public static void Load()
	{
		CSRLYML.IsLoading = true;
		s_Classes.Clear();
		AllAbilityCards = new List<CAbilityCard>();
		AllAbilityCardsUnmodifiedForUI = new List<CAbilityCard>();
		AllAbilityCardInstances = new List<CAbilityCard>();
		s_LongRestLayout = ScenarioRuleClient.SRLYML.AbilityCards.SingleOrDefault((AbilityCardYMLData x) => x.ID == -1);
		if (DataLoadingFlags.HandleAbilitiesOnMainThread)
		{
			LoaddAllCharacters();
		}
		else
		{
			LoaddAllCharactersMT();
		}
		ValidateCharacterClassManager();
		s_HeroSummonClasses.Clear();
		foreach (HeroSummonYMLData heroSummon in ScenarioRuleClient.SRLYML.HeroSummons)
		{
			s_HeroSummonClasses.Add(new CHeroSummonClass(heroSummon.ID, heroSummon.Model, heroSummon.LocKey, heroSummon));
		}
		CSRLYML.IsLoading = false;
	}

	private static void LoaddAllCharactersMT()
	{
		List<CharacterYMLData> elements = ScenarioRuleClient.SRLYML.Characters.ToList();
		ConcurrentQueue<CCharacterClass> characterClasses = new ConcurrentQueue<CCharacterClass>();
		ConcurrentBag<CAbilityCard> allAbilityCards = new ConcurrentBag<CAbilityCard>();
		ConcurrentBag<CAbilityCard> allAbilityCardInstances = new ConcurrentBag<CAbilityCard>();
		JobSystem.ExecuteComplexJobs(elements, (CharacterYMLData character) =>
		{
			LoadCharacterClass(character, characterClasses, allAbilityCards, allAbilityCardInstances);
		});
		s_Classes.AddRange(characterClasses);
		s_Classes.Sort((CCharacterClass left, CCharacterClass right) => left.CharacterID.CompareTo(right.CharacterID));
		AllAbilityCards.AddRange(allAbilityCards);
		AllAbilityCards.Sort((CAbilityCard left, CAbilityCard right) => left.ID.CompareTo(right.ID));
		AllAbilityCardsUnmodifiedForUI.AddRange(allAbilityCards.Select((CAbilityCard x) => x.Copy()));
		AllAbilityCardInstances.AddRange(allAbilityCardInstances);
		AllAbilityCardInstances.Sort((CAbilityCard left, CAbilityCard right) => left.ID.CompareTo(right.ID));
	}

	private static void LoaddAllCharacters()
	{
		foreach (CharacterYMLData character in ScenarioRuleClient.SRLYML.Characters)
		{
			List<CAbilityCard> list = new List<CAbilityCard>();
			foreach (AbilityCardYMLData item in from s in ScenarioRuleClient.SRLYML.AbilityCards
				where s.CharacterID == character.ID
				orderby s.ID
				select s)
			{
				if (item != null && item.TopActionCardData != null && item.BottomActionCardData != null)
				{
					list.Add(new CAbilityCard(item.Initiative, item.ID, new CAction(null, null, CAbilityMove.CreateDefaultMove(GameState.s_DefaultMoveStrength, isMonster: false), 0), new CAction(null, null, CAbilityAttack.CreateDefaultAttack(2, 1, 1, isMonster: false), 0), item.TopActionCardData, item.BottomActionCardData, item, character.ID));
				}
				else
				{
					Log.ErrorWithReport("Ability card " + item.FileName + " unable to load properly");
				}
			}
			if (list.Count > 0)
			{
				s_Classes.Add(new CCharacterClass(character, list, 1));
				AllAbilityCards.AddRange(list);
				AllAbilityCardsUnmodifiedForUI.AddRange(list.Select((CAbilityCard x) => x.Copy()));
				AllAbilityCardInstances.AddRange(list);
				continue;
			}
			throw new Exception("Character '" + character.ID + "' has no ability cards");
		}
	}

	private static void LoadCharacterClass(CharacterYMLData character, ConcurrentQueue<CCharacterClass> characterClasses, ConcurrentBag<CAbilityCard> allAbilityCards, ConcurrentBag<CAbilityCard> allAbilityCardInstances)
	{
		List<CAbilityCard> list = new List<CAbilityCard>();
		foreach (AbilityCardYMLData item in from s in ScenarioRuleClient.SRLYML.AbilityCards
			where s.CharacterID == character.ID
			orderby s.ID
			select s)
		{
			if (item != null && item.TopActionCardData != null && item.BottomActionCardData != null)
			{
				list.Add(new CAbilityCard(item.Initiative, item.ID, new CAction(null, null, CAbilityMove.CreateDefaultMove(GameState.s_DefaultMoveStrength, isMonster: false), 0), new CAction(null, null, CAbilityAttack.CreateDefaultAttack(2, 1, 1, isMonster: false), 0), item.TopActionCardData, item.BottomActionCardData, item, character.ID));
			}
			else
			{
				Log.ErrorWithReport("Ability card " + item.FileName + " unable to load properly");
			}
		}
		if (list.Count > 0)
		{
			characterClasses.Enqueue(new CCharacterClass(character, list, 1));
			list.ForEach((CAbilityCard c) =>
			{
				allAbilityCards.Add(c);
				allAbilityCardInstances.Add(c);
			});
			return;
		}
		throw new Exception("Character '" + character.ID + "' has no ability cards");
	}

	public static void Unload()
	{
		s_Classes.Clear();
		s_HeroSummonClasses.Clear();
		AllAbilityCards = new List<CAbilityCard>();
		AllAbilityCardsUnmodifiedForUI = new List<CAbilityCard>();
		AllAbilityCardInstances = new List<CAbilityCard>();
		s_LongRestLayout = null;
	}

	public static void ValidateCharacterClassManager()
	{
	}

	public static CCharacterClass Find(string classID)
	{
		return s_Classes.Find((CCharacterClass x) => x.ID.ToLower() == classID.ToLower());
	}

	public static CHeroSummonClass FindHeroSummonClass(string classID)
	{
		return s_HeroSummonClasses.Find((CHeroSummonClass x) => x.ID.ToLower() == classID.ToLower());
	}

	public static IEnumerable<CActiveBonus> FindAllActiveBonuses(CActor actor = null, bool important = false)
	{
		GameState.CheckForFinishedActiveBonuses(important);
		IEnumerable<CClass> enumerable = ScenarioManager.Scenario.AllActorsThreadSafe.Select((CActor x) =>
		{
			if (x.Class is CMonsterClass { CardsPoolClass: var cMonsterClass2 } cMonsterClass)
			{
				if (cMonsterClass2 == null)
				{
					cMonsterClass2 = cMonsterClass;
				}
				return cMonsterClass2;
			}
			return x.Class;
		}).Distinct();
		foreach (CClass item in enumerable)
		{
			foreach (CActiveBonus item2 in item.FindActiveBonuses(actor))
			{
				yield return item2;
			}
		}
		foreach (CScenarioModifier scenarioModifier in ScenarioManager.CurrentScenarioState.ScenarioModifiers)
		{
			foreach (CActiveBonus item3 in scenarioModifier.GetActiveBonusesForActor(actor))
			{
				yield return item3;
			}
		}
	}

	public static List<CActiveBonus> FindAllSongActiveBonuses(CActor actor)
	{
		List<CActiveBonus> list = new List<CActiveBonus>();
		CPlayerActor cPlayerActor = ScenarioManager.Scenario.PlayerActors.SingleOrDefault((CPlayerActor x) => x.CharacterClass.CharacterModel == ECharacter.Soothsinger);
		if (cPlayerActor != null && actor != null)
		{
			list.AddRange(cPlayerActor.CharacterClass.FindAllSongActiveBonuses(actor, cPlayerActor));
		}
		return list;
	}

	public static List<CActiveBonus> FindAllDoomActiveBonuses(CActor actor = null, bool checkDeadActor = false)
	{
		List<CActiveBonus> list = new List<CActiveBonus>();
		CPlayerActor cPlayerActor = ScenarioManager.Scenario.PlayerActors.SingleOrDefault((CPlayerActor x) => x.CharacterClass.CharacterModel == ECharacter.Doomstalker);
		if (cPlayerActor != null)
		{
			list.AddRange(cPlayerActor.CharacterClass.FindAllDoomActiveBonuses(actor));
		}
		if (checkDeadActor)
		{
			cPlayerActor = ScenarioManager.Scenario.ExhaustedPlayers.SingleOrDefault((CPlayerActor x) => x.CharacterClass.CharacterModel == ECharacter.Doomstalker);
			if (cPlayerActor != null)
			{
				list.AddRange(cPlayerActor.CharacterClass.FindAllDoomActiveBonuses(actor));
			}
		}
		return list;
	}

	public static List<CActiveBonus> FindAllActiveBonusAuras(CActor actor)
	{
		List<CActiveBonus> allActiveBonuses = new List<CActiveBonus>();
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			allActiveBonuses.AddRange(playerActor.CharacterClass.FindActiveBonusAuras(actor));
		}
		foreach (CPlayerActor exhaustedPlayer in ScenarioManager.Scenario.ExhaustedPlayers)
		{
			allActiveBonuses.AddRange(exhaustedPlayer.CharacterClass.FindActiveBonusAuras(actor));
		}
		foreach (CHeroSummonActor heroSummon in ScenarioManager.Scenario.HeroSummons)
		{
			allActiveBonuses.AddRange(heroSummon.BaseCard.ActiveBonuses.Where((CActiveBonus x) => x.IsAura && x.ValidActorsInRangeOfAura.Contains(actor) && !allActiveBonuses.Contains(x)));
		}
		foreach (CEnemyActor allAliveMonster in ScenarioManager.Scenario.AllAliveMonsters)
		{
			allActiveBonuses.AddRange(from x in allAliveMonster.MonsterClass.FindActiveBonusAuras(actor)
				where !allActiveBonuses.Contains(x)
				select x);
		}
		foreach (CObjectActor @object in ScenarioManager.Scenario.Objects)
		{
			allActiveBonuses.AddRange(from x in @object.MonsterClass.FindActiveBonusAuras(actor)
				where !allActiveBonuses.Contains(x)
				select x);
		}
		foreach (CScenarioModifier scenarioModifier in ScenarioManager.CurrentScenarioState.ScenarioModifiers)
		{
			allActiveBonuses.AddRange(scenarioModifier.ActiveBonuses.Where((CActiveBonus x) => x.IsAura && (x.Actor == actor || x.ValidActorsInRangeOfAura.Contains(actor))));
		}
		return allActiveBonuses;
	}

	public static List<CActiveBonus> FindAllActiveBonuses(CAbility.EAbilityType type, CActor actor)
	{
		List<CActiveBonus> allActiveBonuses = new List<CActiveBonus>();
		for (int i = 0; i < ScenarioManager.Scenario.PlayerActors.Count; i++)
		{
			allActiveBonuses.AddRange(ScenarioManager.Scenario.PlayerActors[i].CharacterClass.FindActiveBonuses(type, actor));
		}
		for (int j = 0; j < ScenarioManager.Scenario.ExhaustedPlayers.Count; j++)
		{
			allActiveBonuses.AddRange(ScenarioManager.Scenario.ExhaustedPlayers[j].CharacterClass.FindActiveBonuses(type, actor));
		}
		for (int k = 0; k < ScenarioManager.Scenario.HeroSummons.Count; k++)
		{
			allActiveBonuses.AddRange(ScenarioManager.Scenario.HeroSummons[k].BaseCard.ActiveBonuses.Where((CActiveBonus x) => x.Type() == type && (x.Actor == actor || (x.IsAura && x.ValidActorsInRangeOfAura.Contains(actor) && !allActiveBonuses.Contains(x)))));
		}
		for (int num = 0; num < ScenarioManager.Scenario.AllAliveMonsters.Count; num++)
		{
			allActiveBonuses.AddRange(from x in ScenarioManager.Scenario.AllAliveMonsters[num].MonsterClass.FindActiveBonuses(type, actor)
				where !allActiveBonuses.Contains(x)
				select x);
		}
		for (int num2 = 0; num2 < ScenarioManager.Scenario.Objects.Count; num2++)
		{
			allActiveBonuses.AddRange(from x in ScenarioManager.Scenario.Objects[num2].MonsterClass.FindActiveBonuses(type, actor)
				where !allActiveBonuses.Contains(x)
				select x);
		}
		for (int num3 = 0; num3 < ScenarioManager.CurrentScenarioState.ScenarioModifiers.Count; num3++)
		{
			allActiveBonuses.AddRange(ScenarioManager.CurrentScenarioState.ScenarioModifiers[num3].ActiveBonuses.Where((CActiveBonus x) => x.Type() == type && ((!x.IsAura) ? (x.Actor == actor) : x.ValidActorsInRangeOfAura.Contains(actor))));
		}
		return allActiveBonuses;
	}

	public static List<CActiveBonus> FindAllActiveBonuses(CActiveBonus.EActiveBonusDurationType durationType, CActor actor)
	{
		List<CActiveBonus> allActiveBonuses = new List<CActiveBonus>();
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			allActiveBonuses.AddRange(playerActor.CharacterClass.FindActiveBonuses(durationType, actor));
		}
		foreach (CPlayerActor exhaustedPlayer in ScenarioManager.Scenario.ExhaustedPlayers)
		{
			allActiveBonuses.AddRange(exhaustedPlayer.CharacterClass.FindActiveBonuses(durationType, actor));
		}
		foreach (CHeroSummonActor heroSummon in ScenarioManager.Scenario.HeroSummons)
		{
			allActiveBonuses.AddRange(heroSummon.BaseCard.ActiveBonuses.Where((CActiveBonus x) => x.Duration == durationType && x.IsAura && x.ValidActorsInRangeOfAura.Contains(actor) && !allActiveBonuses.Contains(x)));
		}
		foreach (CEnemyActor allAliveMonster in ScenarioManager.Scenario.AllAliveMonsters)
		{
			allActiveBonuses.AddRange(from x in allAliveMonster.MonsterClass.FindActiveBonuses(durationType, actor)
				where !allActiveBonuses.Contains(x)
				select x);
		}
		foreach (CObjectActor @object in ScenarioManager.Scenario.Objects)
		{
			allActiveBonuses.AddRange(from x in @object.MonsterClass.FindActiveBonuses(durationType, actor)
				where !allActiveBonuses.Contains(x)
				select x);
		}
		foreach (CScenarioModifier scenarioModifier in ScenarioManager.CurrentScenarioState.ScenarioModifiers)
		{
			allActiveBonuses.AddRange(scenarioModifier.ActiveBonuses.Where((CActiveBonus x) => x.Duration == durationType && ((!x.IsAura) ? (x.Actor == actor) : x.ValidActorsInRangeOfAura.Contains(actor))));
		}
		return allActiveBonuses;
	}

	public static void SetPerks(List<CharacterPerk> perks)
	{
		foreach (CharacterPerk perk in perks)
		{
			CCharacterClass cCharacterClass = s_Classes.SingleOrDefault((CCharacterClass s) => s.ID == perk.CharacterID);
			if (cCharacterClass != null)
			{
				cCharacterClass.ApplyPerk(perk.Perk);
			}
			else
			{
				Log.ErrorWithReport("Unable to create character perk.  Class not found " + perk.CharacterID);
			}
		}
	}

	public static void AddPerks(List<CharacterPerk> perks)
	{
		foreach (CharacterPerk perk in perks)
		{
			CCharacterClass cCharacterClass = s_Classes.SingleOrDefault((CCharacterClass s) => s.ID == perk.CharacterID);
			if (cCharacterClass != null)
			{
				cCharacterClass.AddPerk(perk.Perk);
			}
			else
			{
				Log.ErrorWithReport("Unable to create character perk.  Class not found " + perk.CharacterID);
			}
		}
	}

	public static void SetPerksComplete(string characterID)
	{
		foreach (CMap item in ScenarioManager.CurrentScenarioState.Maps.Where((CMap w) => w.Revealed || w.Players.Any((PlayerState p) => p.IsRevealed && !p.HiddenAtStart) || w.Monsters.Any((EnemyState m) => m.IsRevealed) || w.HeroSummons.Any((HeroSummonState h) => h.IsRevealed)))
		{
			foreach (PlayerState playerState in item.Players.Where((PlayerState p) => !p.HiddenAtStart))
			{
				CCharacterClass cCharacterClass = Find(playerState.ClassID);
				if (cCharacterClass.ID == characterID && ScenarioManager.Scenario.InitialPlayers.SingleOrDefault((CPlayerActor s) => s.ActorGuid == playerState.ActorGuid) != null)
				{
					cCharacterClass.ApplyItemModifiersToList(playerState.Items);
				}
			}
		}
	}

	public static string GetCharacterIDFromModelInstanceID(int modelInstanceID)
	{
		if (modelInstanceID == 0)
		{
			Log.Warning("Default modelInstanceID was provided. Returning null as the CharacterID.");
			return null;
		}
		int num = 0;
		while (modelInstanceID > 1000)
		{
			num++;
			modelInstanceID -= 1000;
		}
		List<CCharacterClass> list = Classes.Where((CCharacterClass w) => w.CharacterModel == (ECharacter)modelInstanceID).ToList();
		if (num < list.Count)
		{
			return list[num].CharacterID;
		}
		Log.ErrorWithReport("Invalid modelInstanceID '" + modelInstanceID + "' sent to GetCharacterIDFromModelInstanceID");
		return null;
	}

	public static int GetModelInstanceIDFromCharacterID(string characterID)
	{
		if (string.IsNullOrEmpty(characterID))
		{
			Log.Warning("Default CharacterID was provided. Returning 0 as the ModelInstanceID.");
			return 0;
		}
		CCharacterClass cCharacterClass = Classes.SingleOrDefault((CCharacterClass s) => s.CharacterID == characterID);
		if (cCharacterClass == null)
		{
			Log.ErrorWithReport("Invalid characterID '" + characterID + "' sent to GetModelInstanceIDFromCharacterID");
			return 0;
		}
		return cCharacterClass.ModelInstanceID;
	}

	public static void Reset()
	{
		AllAbilityCardInstances = AllAbilityCards.ToList();
		AllAbilityCardsUnmodifiedForUI = AllAbilityCards.Select((CAbilityCard x) => x.Copy()).ToList();
		foreach (CCharacterClass s_Class in s_Classes)
		{
			s_Class.Reset();
		}
		foreach (CHeroSummonClass s_HeroSummonClass in s_HeroSummonClasses)
		{
			s_HeroSummonClass.Reset();
		}
	}
}
