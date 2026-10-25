using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FFSNet;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using SharedLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class StateDumper
{
	public class AgilityDumper : IStateDumper
	{
		public string ToStateDumpString(StateDump dump, object obj)
		{
			if (!(obj is CPlayerActor.EAgility eAgility))
			{
				return obj.ToString();
			}
			if (PhaseManager.IsInCardsSelectionPhase)
			{
				return null;
			}
			return eAgility.ToString();
		}

		public void DumpMembers(StateDump dump, object obj)
		{
		}
	}

	public class ItemSlotStateDumper : IStateDumper
	{
		public string ToStateDumpString(StateDump dump, object obj)
		{
			if (!(obj is CItem.EItemSlotState eItemSlotState))
			{
				return obj.ToString();
			}
			if (PhaseManager.CurrentPhase is CPhaseShortRest && GameState.ThreadIsSleeping && eItemSlotState == CItem.EItemSlotState.Useable)
			{
				eItemSlotState = CItem.EItemSlotState.Equipped;
			}
			else if (NormalizeUseableForFastLoad && eItemSlotState == CItem.EItemSlotState.Useable)
			{
				eItemSlotState = CItem.EItemSlotState.Equipped;
			}
			return eItemSlotState.ToString();
		}

		public void DumpMembers(StateDump dump, object obj)
		{
		}
	}

	public class Vector3StateDumper : IStateDumper
	{
		public string ToStateDumpString(StateDump dump, object obj)
		{
			if (obj is CVector3 cVector)
			{
				return cVector.ToStateDumpString();
			}
			return obj.ToString();
		}

		public void DumpMembers(StateDump dump, object obj)
		{
		}
	}

	public class UnityObjectDumper : IStateDumper
	{
		public string ToStateDumpString(StateDump dump, object obj)
		{
			string text = Resolve(obj);
			if (string.IsNullOrEmpty(text))
			{
				return obj.GetType().Name ?? "";
			}
			return obj.GetType().Name + "(" + text + ")";
		}

		public void DumpMembers(StateDump dump, object obj)
		{
			if (Resolve(obj) == null)
			{
				dump.DumpMembersUsingReflection(obj);
			}
		}

		private string Resolve(object obj)
		{
			if (!(obj is GameObject { name: var text } gameObject))
			{
				if (!(obj is TextMeshProUGUI textMeshProUGUI))
				{
					if (!(obj is Button button))
					{
						if (!(obj is UIFX_MaterialFX_Control))
						{
							if (!(obj is UITextTooltipTarget))
							{
								if (!(obj is UIItemModifiersTooltip))
								{
									if (!(obj is CardActionHighlight))
									{
										if (!(obj is CardEffects))
										{
											if (!(obj is ItemCardEffects))
											{
												if (!(obj is MiniCardEffects))
												{
													if (!(obj is MiniAbilityCardIndicator))
													{
														if (obj is MonoBehaviour monoBehaviour)
														{
															if (!monoBehaviour.gameObject.activeInHierarchy)
															{
																return "Hidden";
															}
															return null;
														}
														Type type = obj.GetType();
														if (type.Assembly != typeof(Choreographer).Assembly)
														{
															return "";
														}
														if (type.FullName.StartsWith("DOTween", StringComparison.Ordinal))
														{
															return "";
														}
														return null;
													}
													return "";
												}
												return "";
											}
											return "";
										}
										return "";
									}
									return "";
								}
								return "";
							}
							return "";
						}
						return "";
					}
					if (!button.gameObject.activeInHierarchy)
					{
						return "Hidden";
					}
					if (!button.interactable)
					{
						return "Disabled";
					}
					return "Enabled";
				}
				return "\"" + textMeshProUGUI.text + "\"";
			}
			Transform parent = gameObject.transform.parent;
			while (parent != null)
			{
				text = parent.name + "/" + text;
				parent = parent.parent;
			}
			return text;
		}
	}

	public class RandomDumper : IStateDumper
	{
		public string ToStateDumpString(StateDump dump, object obj)
		{
			if (!(obj is SharedLibrary.Random random))
			{
				return TValue.ToString(obj);
			}
			SharedLibrary.Random random2 = random.Save().Restore();
			int num = random2.Next();
			int num2 = random2.Next();
			int num3 = random2.Next();
			return $"Random({num}, {num2}, {num3})";
		}

		public void DumpMembers(StateDump dump, object obj)
		{
		}
	}

	public static bool NormalizeUseableForFastLoad = false;

	public static readonly string DelimiterLine = "================================";

	public static readonly string Delimiter = "\n\n" + DelimiterLine + "\n\n";

	private static bool s_CustomDumpersInitted;

	public static string GenerateFullStateDump()
	{
		try
		{
			InitCustomDumpers();
			Stopwatch stopwatch = Stopwatch.StartNew();
			StateDump stateDump = new StateDump();
			stateDump.Add("LevelName", SaveData.Instance.Global.CurrentCustomLevelData?.Name);
			stateDump.Add("Scenario.PlayerActors", ScenarioManager.Scenario.PlayerActors);
			stateDump.Add("Scenario.ExhaustedPlayers", ScenarioManager.Scenario.ExhaustedPlayers);
			stateDump.Add("Scenario.Enemies", ScenarioManager.Scenario.Enemies);
			stateDump.Add("Scenario.DeadEnemies", ScenarioManager.Scenario.DeadEnemies);
			stateDump.Add("Scenario.HeroSummons", ScenarioManager.Scenario.HeroSummons);
			stateDump.Add("Scenario.DeadHeroSummons", ScenarioManager.Scenario.DeadHeroSummons);
			stateDump.Add("Scenario.AllyMonsters", ScenarioManager.Scenario.AllyMonsters);
			stateDump.Add("Scenario.DeadAllyMonsters", ScenarioManager.Scenario.DeadAllyMonsters);
			stateDump.Add("Scenario.Enemy2Monsters", ScenarioManager.Scenario.Enemy2Monsters);
			stateDump.Add("Scenario.DeadEnemy2Monsters", ScenarioManager.Scenario.DeadEnemy2Monsters);
			stateDump.Add("Scenario.NeutralMonsters", ScenarioManager.Scenario.NeutralMonsters);
			stateDump.Add("Scenario.DeadNeutralMonsters", ScenarioManager.Scenario.DeadNeutralMonsters);
			stateDump.Add("Scenario.Objects", ScenarioManager.Scenario.Objects);
			stateDump.Add("Scenario.DeadObjects", ScenarioManager.Scenario.DeadObjects);
			stateDump.Add("CurrentScenarioState.Props", ScenarioManager.CurrentScenarioState.Props);
			stateDump.Add("CurrentScenarioState.Spawners", ScenarioManager.CurrentScenarioState.Spawners);
			stateDump.Add("Scenario", ScenarioManager.Scenario);
			stateDump.Add(typeof(CActiveBonus));
			stateDump.Add(typeof(CGenericActiveBonus));
			stateDump.Add(typeof(ScenarioManager));
			stateDump.Add(typeof(GameState));
			stateDump.Add(typeof(PhaseManager));
			stateDump.Add(typeof(ElementInfusionBoardManager));
			stateDump.Add("CurrentScenarioState", ScenarioManager.CurrentScenarioState);
			stateDump.Add("s_Choreographer", Choreographer.s_Choreographer);
			string result = stateDump.Finish();
			Log.Info($"GenerateFullStateDump took {stopwatch.ElapsedMilliseconds}ms");
			return result;
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "Error generating state dump");
			return null;
		}
	}

	public static string GenerateImportantStateDump()
	{
		try
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			StateDump stateDump = new StateDump();
			stateDump.AddValue("LevelName", SaveData.Instance.Global.CurrentCustomLevelData?.Name);
			ImportantDumpParty(stateDump);
			stateDump.AddValue("Round", GameState.RoundCount);
			ImportantDumpPhase(stateDump);
			if (GameState.s_DesyncVar > 1)
			{
				stateDump.AddValue("DesyncVar", GameState.s_DesyncVar);
			}
			ImportantDumpObjects(stateDump, "Heroes", ScenarioManager.Scenario.PlayerActors, ImportantDumpActor);
			ImportantDumpObjects(stateDump, "HeroSummons", ScenarioManager.Scenario.HeroSummons, ImportantDumpActor);
			ImportantDumpObjects(stateDump, "Enemies", ScenarioManager.Scenario.Enemies, ImportantDumpActor);
			ImportantDumpObjects(stateDump, "AllyMonsters", ScenarioManager.Scenario.AllyMonsters, ImportantDumpActor);
			ImportantDumpObjects(stateDump, "Enemy2Monsters", ScenarioManager.Scenario.Enemy2Monsters, ImportantDumpActor);
			ImportantDumpObjects(stateDump, "NeutralMonsters", ScenarioManager.Scenario.NeutralMonsters, ImportantDumpActor);
			ImportantDumpObjects(stateDump, "Objects", ScenarioManager.Scenario.Objects, ImportantDumpActor);
			ImportantDumpObjects(stateDump, "Props", ScenarioManager.CurrentScenarioState.Props, ImportantDumpProp);
			if (ScenarioManager.Scenario.Enemies.Count > 0)
			{
				ImportantDumpObjects(stateDump, "EnemyMonsterAttackModifiers", MonsterClassManager.EnemyMonsterAttackModifierDeck.AttackModifierCards, ImportantDumpAttackModifier);
			}
			if (ScenarioManager.Scenario.AllyMonsters.Count > 0)
			{
				ImportantDumpObjects(stateDump, "AlliedMonsterAttackModifiers", MonsterClassManager.AlliedMonsterAttackModifierDeck.AttackModifierCards, ImportantDumpAttackModifier);
			}
			if (ScenarioManager.Scenario.Enemy2Monsters.Count > 0)
			{
				ImportantDumpObjects(stateDump, "Enemy2MonsterAttackModifiers", MonsterClassManager.Enemy2MonsterAttackModifierDeck.AttackModifierCards, ImportantDumpAttackModifier);
			}
			if (ScenarioManager.Scenario.NeutralMonsters.Count > 0)
			{
				ImportantDumpObjects(stateDump, "NeutralMonsterAttackModifiers", MonsterClassManager.NeutralMonsterAttackModifierDeck.AttackModifierCards, ImportantDumpAttackModifier);
			}
			ImportantDumpElements(stateDump);
			string result = stateDump.Finish();
			Log.Info($"GenerateImportantStateDump took {stopwatch.ElapsedMilliseconds}ms");
			return result;
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "Error generating important state dump");
			return null;
		}
	}

	private static void ImportantDumpParty(StateDump dump)
	{
		dump.StartNewLine("Party");
		int num = 0;
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			if (num > 0)
			{
				dump.Append(", ");
			}
			num++;
			dump.Append(ActorID(playerActor));
		}
		foreach (CPlayerActor exhaustedPlayer in ScenarioManager.Scenario.ExhaustedPlayers)
		{
			if (num > 0)
			{
				dump.Append(", ");
			}
			num++;
			dump.Append("[exhausted] ");
			dump.Append(ActorID(exhaustedPlayer));
		}
	}

	private static void ImportantDumpControllers(StateDump dump)
	{
		if (!FFSNetwork.IsOnline)
		{
			return;
		}
		dump.AddValue("Players", PlayerRegistry.AllPlayers.Count);
		dump.OpenSection("Players");
		foreach (NetworkPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			dump.StartNewLine(allPlayer.PlayerToken.ToStateDumpString(dump));
			int num = 0;
			foreach (NetworkControllable myControllable in allPlayer.MyControllables)
			{
				string text = StripID(myControllable.GetCharacter);
				if (!string.IsNullOrEmpty(text))
				{
					if (num != 0)
					{
						dump.Append(", ");
					}
					num++;
					dump.Append(text);
				}
			}
		}
		dump.CloseSection();
	}

	private static void ImportantDumpElements(StateDump dump)
	{
		bool flag = false;
		ElementInfusionBoardManager.EColumn[] getElementColumn = ElementInfusionBoardManager.GetElementColumn;
		for (int i = 0; i < getElementColumn.Length; i++)
		{
			ElementInfusionBoardManager.EColumn eColumn = getElementColumn[i];
			if (eColumn != ElementInfusionBoardManager.EColumn.Inert)
			{
				if (!flag)
				{
					flag = true;
					dump.StartNewLine("Elements");
					dump.OpenSection("Elements");
				}
				ElementInfusionBoardManager.EElement eElement = (ElementInfusionBoardManager.EElement)i;
				string key = eElement.ToString();
				dump.AddValue(key, eColumn.ToString());
			}
		}
		if (flag)
		{
			dump.CloseSection();
		}
	}

	private static void ImportantDumpPhase(StateDump dump)
	{
		dump.AddValue("Phase", PhaseManager.PhaseType.ToString());
		switch (PhaseManager.PhaseType)
		{
		case CPhase.PhaseType.ShortRest:
			dump.Append(" (" + ActorID(GameState.s_ShortRestingPlayer) + ")");
			break;
		default:
			dump.Append(" (" + ActorID(GameState.TurnActor) + ")");
			break;
		case CPhase.PhaseType.SelectAbilityCardsOrLongRest:
			break;
		}
	}

	private static void ImportantDumpObjects(StateDump dump, string key, IList list, Action<StateDump, string, int, object> dumper)
	{
		if (list == null)
		{
			return;
		}
		int count = list.Count;
		if (count > 0)
		{
			dump.AddValue(key, count);
			dump.OpenSection(key);
			for (int i = 0; i < count; i++)
			{
				object arg = list[i];
				dumper(dump, key, i, arg);
			}
			dump.CloseSection();
		}
	}

	private static void ImportantDumpActor(StateDump dump, string key, int index, object obj)
	{
		CActor cActor = (CActor)obj;
		key = $"[{index}]";
		string text = ActorID(cActor);
		dump.AddValue(key, text);
		dump.OpenSection(text);
		dump.AddValue("Health", cActor.Health);
		dump.AddValue("Position", cActor.ArrayIndex.ToString());
		int? initiativeLockedDuringRound = cActor.InitiativeLockedDuringRound;
		if (initiativeLockedDuringRound.HasValue && initiativeLockedDuringRound.Value != 0)
		{
			dump.AddValue("Initiative", initiativeLockedDuringRound.Value);
		}
		ImportantDumpTokens(dump, cActor);
		if (!(cActor is CPlayerActor { CharacterClass: var characterClass } cPlayerActor))
		{
			if (!(cActor is CEnemyActor { MonsterClass: var monsterClass }))
			{
				if (cActor is CHeroSummonActor)
				{
				}
			}
			else if (monsterClass.RoundAbilityCard != null)
			{
				dump.AddValue("RoundAbilityCard", monsterClass.RoundAbilityCard?.Name);
			}
		}
		else
		{
			ImportantDumpObjects(dump, "ActivatedCards", characterClass.ActivatedAbilityCards, ImportantDumpAbilityCard);
			ImportantDumpObjects(dump, "RoundAbilityCards", characterClass.RoundAbilityCards, ImportantDumpAbilityCard);
			ImportantDumpObjects(dump, "HandAbilityCards", characterClass.HandAbilityCards, ImportantDumpAbilityCard);
			ImportantDumpObjects(dump, "DiscardedAbilityCards", characterClass.DiscardedAbilityCards, ImportantDumpAbilityCard);
			ImportantDumpObjects(dump, "LostAbilityCards", characterClass.LostAbilityCards, ImportantDumpAbilityCard);
			ImportantDumpObjects(dump, "AttackModifierCards", characterClass.AttackModifierCards, ImportantDumpAttackModifier);
			ImportantDumpObjects(dump, "Items", cPlayerActor.Inventory.AllItems, ImportantDumpItem);
			ImportantDumpObjects(dump, "Perks", cPlayerActor.CharacterClass.Perks, ImportantDumpPerk);
		}
		dump.CloseSection();
	}

	private static void ImportantDumpProp(StateDump dump, string key, int index, object obj)
	{
		CObjectProp cObjectProp = (CObjectProp)obj;
		key = $"[{index}]";
		string value = $"{cObjectProp.PrefabName} at {cObjectProp.ArrayIndex}";
		dump.AddValue(key, value);
		if (cObjectProp is CObjectDoor cObjectDoor)
		{
			if (cObjectDoor.DoorIsOpen)
			{
				dump.Append(", Open");
			}
			if (cObjectDoor.DoorIsLocked)
			{
				dump.Append(", Locked");
			}
			if (cObjectDoor.IsDungeonEntrance)
			{
				dump.Append(", Entrance");
			}
			if (cObjectDoor.IsDungeonExit)
			{
				dump.Append(", Exit");
			}
		}
	}

	private static void ImportantDumpTokens(StateDump dump, CActor actor)
	{
		if (actor?.Tokens == null)
		{
			return;
		}
		List<CCondition.EPositiveCondition> allPositiveConditions = actor.Tokens.GetAllPositiveConditions();
		List<CCondition.ENegativeCondition> allNegativeConditions = actor.Tokens.GetAllNegativeConditions();
		if (allPositiveConditions.Count == 0 && allNegativeConditions.Count == 0)
		{
			return;
		}
		List<string> list = allPositiveConditions.Select((CCondition.EPositiveCondition x) => x.ToString()).Concat(allNegativeConditions.Select((CCondition.ENegativeCondition x) => x.ToString())).ToList();
		dump.StartNewLine("Conditions");
		for (int num = 0; num < list.Count; num++)
		{
			string data = list[num];
			if (num > 0)
			{
				dump.Append(", ");
			}
			dump.Append(data);
		}
	}

	private static void ImportantDumpAbilityCard(StateDump dump, string key, int index, object obj)
	{
		CAbilityCard cAbilityCard = (CAbilityCard)obj;
		key = $"[{index}]";
		string name = cAbilityCard.Name;
		dump.AddValue(key, name);
	}

	private static void ImportantDumpAttackModifier(StateDump dump, string key, int index, object obj)
	{
		AttackModifierYMLData attackModifierYMLData = (AttackModifierYMLData)obj;
		key = $"[{index}]";
		string name = attackModifierYMLData.Name;
		dump.AddValue(key, name);
	}

	private static void ImportantDumpItem(StateDump dump, string key, int index, object obj)
	{
		CItem cItem = (CItem)obj;
		key = $"[{index}]";
		string value = cItem.Name ?? "";
		dump.AddValue(key, value);
	}

	private static void ImportantDumpPerk(StateDump dump, string key, int index, object obj)
	{
		PerksYMLData perksYMLData = (PerksYMLData)obj;
		key = $"[{index}]";
		string name = perksYMLData.Name;
		dump.AddValue(key, name);
	}

	private static string ActorID(CActor actor)
	{
		if (actor != null)
		{
			if (!(actor is CPlayerActor cPlayerActor))
			{
				if (!(actor is CEnemyActor cEnemyActor))
				{
					if (actor is CHeroSummonActor cHeroSummonActor)
					{
						return $"{StripID(cHeroSummonActor.Class.ID)}({cHeroSummonActor.StandeeID})";
					}
					return StripID(actor.Class.ID);
				}
				return $"{StripID(cEnemyActor.Class.ID)}({cEnemyActor.StandeeID})";
			}
			return StripID(cPlayerActor.CharacterClass.ID);
		}
		return "null";
	}

	private static string StripID(string id)
	{
		if (id == null)
		{
			return null;
		}
		if (id.EndsWith("ID", StringComparison.Ordinal))
		{
			return id.Substring(0, id.Length - 2);
		}
		return id;
	}

	public static string CombineStateDumps(string importantDump, string fullDump)
	{
		if (string.IsNullOrEmpty(fullDump))
		{
			return importantDump;
		}
		return importantDump + Delimiter + fullDump;
	}

	public static void SaveStateDump(string fileName)
	{
	}

	public static void SaveStateDump(string stateDump, string fileName)
	{
	}

	public static void SaveStateDump(byte[] stateDump, string fileName)
	{
	}

	private static void InitCustomDumpers()
	{
		if (!s_CustomDumpersInitted)
		{
			s_CustomDumpersInitted = true;
			StateDump.AddCustomDumper(typeof(UnityEngine.Object), new UnityObjectDumper());
			StateDump.AddCustomDumper(typeof(SharedLibrary.Random), new RandomDumper());
			StateDump.AddCustomDumper(typeof(CItem.EItemSlotState), new ItemSlotStateDumper());
			StateDump.AddCustomDumper(typeof(CPlayerActor.EAgility), new AgilityDumper());
			StateDump.AddCustomDumper(typeof(CVector3), new Vector3StateDumper());
		}
	}
}
