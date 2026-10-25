using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class PlayerState : ActorState, ISerializable
{
	public string CharacterName { get; private set; }

	public int ScenarioGold { get; private set; }

	public int ScenarioXP { get; private set; }

	public LootState ScenarioLoot { get; private set; }

	public LootState ScenarioLootFromLootCards { get; private set; }

	public AbilityDeckState AbilityDeck { get; private set; }

	public AttackModifierDeckState AttackModifierDeck { get; private set; }

	public int CompletedMasteries { get; set; }

	public List<CItem> Items { get; private set; }

	public int MaxSmallItems { get; set; }

	public bool HiddenAtStart { get; set; }

	public bool IsLongRestSelected { get; set; }

	public bool IsLongRestActionSelected { get; set; }

	public bool IsLongResting { get; private set; }

	public bool HasLongRested { get; set; }

	public bool IsShortResting { get; private set; }

	public bool HasShortRested { get; set; }

	public bool HasImprovedShortRested { get; set; }

	public bool HasQueuedPreRestBonuses { get; set; }

	public bool ShortRestCardRedrawn { get; set; }

	public int ShortRestCardBurned { get; set; }

	public bool ImprovedShortRestState { get; set; }

	public uint ImprovedShortRestDrawCountState { get; set; }

	public string CompanionSummonGuid { get; set; }

	public int TimeTokens { get; set; }

	public int TrophyTokens { get; set; }

	public int ResonanceTokens { get; set; }

	public CPlayerActor.EAgility Agility { get; set; }

	public CPlayerActor.EForm Form { get; set; }

	public CPlayerActor.EForm PrevForm { get; set; }

	public CPlayerActor.EForm PendingForm { get; set; }

	public CPlayerActor.EPressure Pressure { get; set; }

	public int NextSummonID { get; set; }

	[StateDump(Skip = true)]
	public List<string> CheckPerkBonuses { get; set; }

	public CPlayerActor Player
	{
		get
		{
			try
			{
				return ScenarioManager.Scenario.AllPlayers.SingleOrDefault((CPlayerActor s) => s.ActorGuid == ActorGuid);
			}
			catch (Exception)
			{
				Log.ErrorWithReport("More than one matching player actor in Scenario.AllPlayers with ActorGuid: " + ActorGuid);
				return null;
			}
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("CharacterName", CharacterName);
		info.AddValue("ScenarioGold", ScenarioGold);
		info.AddValue("ScenarioXP", ScenarioXP);
		info.AddValue("ScenarioLoot", ScenarioLoot);
		info.AddValue("ScenarioLootFromLootCards", ScenarioLootFromLootCards);
		info.AddValue("AbilityDeck", AbilityDeck);
		info.AddValue("AttackModifierDeck", AttackModifierDeck);
		info.AddValue("Items", Items);
		info.AddValue("HiddenAtStart", HiddenAtStart);
		info.AddValue("IsLongRestSelected", IsLongRestSelected);
		info.AddValue("IsLongRestActionSelected", IsLongRestActionSelected);
		info.AddValue("IsLongResting", IsLongResting);
		info.AddValue("HasLongRested", HasLongRested);
		info.AddValue("IsShortResting", IsShortResting);
		info.AddValue("HasShortRested", HasShortRested);
		info.AddValue("HasImprovedShortRested", HasImprovedShortRested);
		info.AddValue("HasQueuedPreRestBonuses", HasQueuedPreRestBonuses);
		info.AddValue("ShortRestCardRedrawn", ShortRestCardRedrawn);
		info.AddValue("ShortRestCardBurned", ShortRestCardBurned);
		info.AddValue("ImprovedShortRestState", ImprovedShortRestState);
		info.AddValue("ImprovedShortRestDrawCountState", ImprovedShortRestDrawCountState);
		info.AddValue("CompanionSummonGuid", CompanionSummonGuid);
		info.AddValue("TimeTokens", TimeTokens);
		info.AddValue("TrophyTokens", TrophyTokens);
		info.AddValue("ResonanceTokens", ResonanceTokens);
		info.AddValue("Agility", Agility.ToString());
		info.AddValue("Form", Form.ToString());
		info.AddValue("PrevForm", PrevForm.ToString());
		info.AddValue("PendingForm", PendingForm.ToString());
		info.AddValue("NextSummonID", NextSummonID.ToString());
		info.AddValue("MaxSmallItems", MaxSmallItems);
		info.AddValue("CompletedMasteries", CompletedMasteries);
		info.AddValue("Pressure", Pressure.ToString());
		info.AddValue("CheckPerkBonuses", CheckPerkBonuses);
	}

	public PlayerState(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "CharacterName":
					CharacterName = info.GetString("CharacterName");
					break;
				case "ScenarioGold":
					ScenarioGold = info.GetInt32("ScenarioGold");
					break;
				case "ScenarioXP":
					ScenarioXP = info.GetInt32("ScenarioXP");
					break;
				case "ScenarioLoot":
					if (item.ObjectType == typeof(byte[]))
					{
						byte[] oldData2 = (byte[])info.GetValue("ScenarioLoot", typeof(byte[]));
						ScenarioLoot = new LootState();
						ScenarioLoot.MigrateFromOldData(oldData2);
					}
					else
					{
						ScenarioLoot = (LootState)info.GetValue("ScenarioLoot", typeof(LootState));
					}
					break;
				case "ScenarioLootFromLootCards":
					if (item.ObjectType == typeof(byte[]))
					{
						byte[] oldData = (byte[])info.GetValue("ScenarioLootFromLootCards", typeof(byte[]));
						ScenarioLootFromLootCards = new LootState();
						ScenarioLootFromLootCards.MigrateFromOldData(oldData);
					}
					else
					{
						ScenarioLootFromLootCards = (LootState)info.GetValue("ScenarioLootFromLootCards", typeof(LootState));
					}
					break;
				case "AbilityDeck":
					AbilityDeck = (AbilityDeckState)info.GetValue("AbilityDeck", typeof(AbilityDeckState));
					break;
				case "AttackModifierDeck":
					AttackModifierDeck = (AttackModifierDeckState)info.GetValue("AttackModifierDeck", typeof(AttackModifierDeckState));
					break;
				case "Items":
					Items = (List<CItem>)info.GetValue("Items", typeof(List<CItem>));
					break;
				case "HiddenAtStart":
					HiddenAtStart = info.GetBoolean("HiddenAtStart");
					break;
				case "IsLongRestSelected":
					IsLongRestSelected = info.GetBoolean("IsLongRestSelected");
					break;
				case "IsLongRestActionSelected":
					IsLongRestActionSelected = info.GetBoolean("IsLongRestActionSelected");
					break;
				case "IsLongResting":
					IsLongResting = info.GetBoolean("IsLongResting");
					break;
				case "HasLongRested":
					HasLongRested = info.GetBoolean("HasLongRested");
					break;
				case "IsShortResting":
					IsShortResting = info.GetBoolean("IsShortResting");
					break;
				case "HasShortRested":
					HasShortRested = info.GetBoolean("HasShortRested");
					break;
				case "HasImprovedShortRested":
					HasImprovedShortRested = info.GetBoolean("HasImprovedShortRested");
					break;
				case "HasQueuedPreRestBonuses":
					HasQueuedPreRestBonuses = info.GetBoolean("HasQueuedPreRestBonuses");
					break;
				case "ShortRestCardRedrawn":
					ShortRestCardRedrawn = info.GetBoolean("ShortRestCardRedrawn");
					break;
				case "ShortRestCardBurned":
					ShortRestCardBurned = info.GetInt32("ShortRestCardBurned");
					break;
				case "ImprovedShortRestState":
					ImprovedShortRestState = info.GetBoolean("ImprovedShortRestState");
					break;
				case "ImprovedShortRestDrawCountState":
					ImprovedShortRestDrawCountState = info.GetUInt32("ImprovedShortRestDrawCountState");
					break;
				case "CompanionSummonGuid":
					CompanionSummonGuid = info.GetString("CompanionSummonGuid");
					break;
				case "TimeTokens":
					TimeTokens = info.GetInt32("TimeTokens");
					break;
				case "TrophyTokens":
					TrophyTokens = info.GetInt32("TrophyTokens");
					break;
				case "ResonanceTokens":
					ResonanceTokens = info.GetInt32("ResonanceTokens");
					break;
				case "Agility":
				{
					string value5 = info.GetString("Agility");
					Agility = Enum.Parse<CPlayerActor.EAgility>(value5);
					break;
				}
				case "Form":
				{
					string value4 = info.GetString("Form");
					Form = Enum.Parse<CPlayerActor.EForm>(value4);
					break;
				}
				case "PrevForm":
				{
					string value3 = info.GetString("PrevForm");
					PrevForm = Enum.Parse<CPlayerActor.EForm>(value3);
					break;
				}
				case "PendingForm":
				{
					string value2 = info.GetString("PendingForm");
					PendingForm = Enum.Parse<CPlayerActor.EForm>(value2);
					break;
				}
				case "NextSummonID":
					NextSummonID = info.GetInt32("NextSummonID");
					break;
				case "CheckPerkBonuses":
					CheckPerkBonuses = (List<string>)info.GetValue("CheckPerkBonuses", typeof(List<string>));
					break;
				case "MaxSmallItems":
					MaxSmallItems = info.GetInt32("MaxSmallItems");
					break;
				case "CompletedMasteries":
					CompletedMasteries = info.GetInt32("CompletedMasteries");
					break;
				case "Pressure":
				{
					string value = info.GetString("Pressure");
					Pressure = Enum.Parse<CPlayerActor.EPressure>(value);
					break;
				}
				}
				if (CompanionSummonGuid == null)
				{
					CompanionSummonGuid = string.Empty;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize PlayerState entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw;
			}
		}
	}

	public override void SaveFields(CActor actor)
	{
		if (actor is CPlayerActor cPlayerActor)
		{
			base.SaveFields(actor);
			CCharacterClass cCharacterClass = cPlayerActor.Class as CCharacterClass;
			CharacterName = cPlayerActor.CharacterName;
			PhasedOut = cPlayerActor.PhasedOut;
			ScenarioGold = cPlayerActor.Gold;
			ScenarioXP = cPlayerActor.XP;
			ScenarioLoot = new LootState();
			ScenarioLoot.AccumulateLootFrom(cPlayerActor.TotalLoot);
			ScenarioLootFromLootCards = new LootState();
			ScenarioLootFromLootCards.AccumulateLootFrom(cPlayerActor.LootFromLootCards);
			IsLongRestSelected = cPlayerActor.IsLongRestSelected;
			IsLongRestActionSelected = cPlayerActor.IsLongRestActionSelected;
			IsLongResting = cCharacterClass.LongRest;
			HasLongRested = cCharacterClass.HasLongRested;
			IsShortResting = cCharacterClass.ShortRest;
			HasShortRested = cCharacterClass.HasShortRested;
			HasImprovedShortRested = cCharacterClass.HasImprovedShortRested;
			HasQueuedPreRestBonuses = cCharacterClass.HasQueuedPreRestBonuses;
			ShortRestCardRedrawn = cCharacterClass.ShortRestCardRedrawn;
			ShortRestCardBurned = cCharacterClass.ShortRestCardBurned;
			ImprovedShortRestState = cCharacterClass.ImprovedShortRest;
			ImprovedShortRestDrawCountState = cCharacterClass.ImprovedShortRestDrawCount;
			CompanionSummonGuid = ((cPlayerActor.CompanionSummon != null) ? cPlayerActor.CompanionSummon.ActorGuid : string.Empty);
			TimeTokens = cPlayerActor.GetTimeTokens();
			TrophyTokens = cPlayerActor.GetTrophyTokens();
			ResonanceTokens = cPlayerActor.GetResonanceTokens();
			Agility = cPlayerActor.Agility;
			Form = cPlayerActor.Form;
			PrevForm = cPlayerActor.PrevForm;
			PendingForm = cPlayerActor.PendingForm;
			NextSummonID = cPlayerActor.NextSummonID;
			Pressure = cPlayerActor.Pressure;
		}
	}

	public void Save(bool initial, bool forceSave = false, CPlayerActor currentPlayer = null)
	{
		if (!IsRevealed && !forceSave)
		{
			return;
		}
		if (currentPlayer == null)
		{
			currentPlayer = ((initial && !IsDead) ? ScenarioManager.Scenario.InitialPlayers.Single((CPlayerActor s) => s.ActorGuid == ActorGuid) : Player);
		}
		CCharacterClass playerClass = currentPlayer.Class as CCharacterClass;
		AbilityDeck.Save(currentPlayer.Class);
		if (AttackModifierDeck == null)
		{
			AttackModifierDeck = new AttackModifierDeckState(playerClass);
		}
		else
		{
			AttackModifierDeck.SaveCharacter(playerClass);
		}
		SaveFields(currentPlayer);
		if (!initial)
		{
			Items = currentPlayer.Inventory.AllItems.ToList();
		}
		CarriedQuestProps = currentPlayer.CarriedQuestItems.ToList();
	}

	public void Load(bool forceLoad = false)
	{
		if (IsRevealed | forceLoad)
		{
			AttackModifierDeck.Load(Player.Class as CCharacterClass);
			Player.LoadFields(this);
		}
	}

	public PlayerState(string classID, int chosenModelIndex, string actorGuid, string mapGuid, TileIndex location, int health, int maxHealth, int level, List<PositiveConditionPair> posCons, List<NegativeConditionPair> negCons, bool playedThisRound, CActor.ECauseOfDeath causeOfDeath, int augmentSlots, string characterName, int gold, int xp, LootState loot, LootState lootFromLootCards, bool isLongResting, bool isShortResting, AbilityDeckState deck, AttackModifierDeckState attackModDeck, List<CItem> items, int timeTokens, CPlayerActor.EAgility agility, CPlayerActor.EForm form, CPlayerActor.EForm prevForm, CPlayerActor.EForm pendingForm, CPlayerActor.EPressure pressure, int nextSummonID, int maxSmallitems, int trophyTokens, int completedMasteries, int resonanceTokens)
		: base(classID, chosenModelIndex, actorGuid, mapGuid, location, health, maxHealth, level, posCons, negCons, playedThisRound, causeOfDeath, augmentSlots)
	{
		CharacterName = characterName;
		ScenarioGold = gold;
		ScenarioXP = xp;
		ScenarioLoot = loot;
		ScenarioLootFromLootCards = lootFromLootCards;
		IsLongResting = isLongResting;
		IsShortResting = isShortResting;
		AbilityDeck = deck;
		AttackModifierDeck = attackModDeck;
		Items = items;
		CompanionSummonGuid = string.Empty;
		TimeTokens = timeTokens;
		TrophyTokens = trophyTokens;
		ResonanceTokens = resonanceTokens;
		Agility = agility;
		Form = form;
		PrevForm = prevForm;
		PendingForm = pendingForm;
		NextSummonID = nextSummonID;
		MaxSmallItems = maxSmallitems;
		CompletedMasteries = completedMasteries;
		Pressure = pressure;
		CheckPerkBonuses = null;
	}

	public PlayerState(CPlayerActor playerActor, string mapGuid)
		: base(playerActor.Class.ID, playerActor.ChosenModelIndex, playerActor.ActorGuid, mapGuid, new TileIndex(playerActor.ArrayIndex), playerActor.Health, playerActor.MaxHealth, playerActor.Level, playerActor.Tokens.CheckPositiveTokens.ToList(), playerActor.Tokens.CheckNegativeTokens.ToList(), playerActor.PlayedThisRound, playerActor.CauseOfDeath, playerActor.AugmentSlots)
	{
		ScenarioGold = playerActor.Gold;
		ScenarioXP = playerActor.XP;
		ScenarioLoot = new LootState();
		ScenarioLoot.AccumulateLootFrom(playerActor.TotalLoot);
		ScenarioLootFromLootCards = new LootState();
		ScenarioLootFromLootCards.AccumulateLootFrom(playerActor.LootFromLootCards);
		Items = playerActor.Inventory.AllItems.ToList();
		IsRevealed = true;
		HiddenAtStart = false;
		ImprovedShortRestState = false;
		ImprovedShortRestDrawCountState = 0u;
		CompanionSummonGuid = string.Empty;
		TimeTokens = playerActor.GetTimeTokens();
		TrophyTokens = playerActor.GetTrophyTokens();
		ResonanceTokens = playerActor.GetResonanceTokens();
		InitiativeLockedDuringRound = playerActor.InitiativeLockedDuringRound;
		Agility = playerActor.Agility;
		Form = playerActor.Form;
		PrevForm = playerActor.PrevForm;
		PendingForm = playerActor.PendingForm;
		NextSummonID = playerActor.NextSummonID;
		MaxSmallItems = playerActor.Inventory.SmallItemMax;
		CompletedMasteries = playerActor.CompletedMasteries;
		Pressure = playerActor.Pressure;
		CheckPerkBonuses = null;
	}

	public void LoadAbiltyDeck(bool forceLoad = false)
	{
		if (IsRevealed | forceLoad)
		{
			AbilityDeck.Load(Player.Class);
		}
	}

	public static List<Tuple<int, string>> Compare(PlayerState state1, PlayerState state2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		try
		{
			list.AddRange(ActorState.Compare(state1, state2, isMPCompare));
			if (state1.ScenarioGold != state2.ScenarioGold)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 501, "Player State ScenarioGold does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3]
					{
						"Actor ID",
						state1.Actor?.ID.ToString(),
						state1.Actor?.ID.ToString()
					},
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"ScenarioGold",
						state1.ScenarioGold.ToString(),
						state2.ScenarioGold.ToString()
					}
				});
			}
			if (state1.ScenarioXP != state2.ScenarioXP)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 502, "Player State ScenarioXP does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3]
					{
						"Actor ID",
						state1.Actor?.ID.ToString(),
						state1.Actor?.ID.ToString()
					},
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"ScenarioXP",
						state1.ScenarioXP.ToString(),
						state2.ScenarioXP.ToString()
					}
				});
			}
			if (state1.IsLongResting != state2.IsLongResting)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 503, "Player State IsLongResting does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3]
					{
						"Actor ID",
						state1.Actor?.ID.ToString(),
						state1.Actor?.ID.ToString()
					},
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"IsLongResting",
						state1.IsLongResting.ToString(),
						state2.IsLongResting.ToString()
					}
				});
			}
			if (state1.HiddenAtStart != state2.HiddenAtStart)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 504, "Player State HiddenAtStart does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3]
					{
						"Actor ID",
						state1.Actor?.ID.ToString(),
						state1.Actor?.ID.ToString()
					},
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"HiddenAtStart",
						state1.HiddenAtStart.ToString(),
						state2.HiddenAtStart.ToString()
					}
				});
			}
			if (state1.ImprovedShortRestState != state2.ImprovedShortRestState)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 505, "Player State ImprovedShortRestState does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3]
					{
						"Actor ID",
						state1.Actor?.ID.ToString(),
						state1.Actor?.ID.ToString()
					},
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"ImprovedShortRestState",
						state1.ImprovedShortRestState.ToString(),
						state2.ImprovedShortRestState.ToString()
					}
				});
			}
			if (state1.CompanionSummonGuid != state2.CompanionSummonGuid)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 506, "Player State CompanionSummonGuid does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3]
					{
						"Actor ID",
						state1.Actor?.ID.ToString(),
						state1.Actor?.ID.ToString()
					},
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3] { "CompanionSummonGuid", state1.CompanionSummonGuid, state2.CompanionSummonGuid }
				});
			}
			if (state1.TimeTokens != state2.TimeTokens)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 507, "Player State TimeTokens does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3]
					{
						"Actor ID",
						state1.Actor?.ID.ToString(),
						state1.Actor?.ID.ToString()
					},
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"TimeTokens",
						state1.TimeTokens.ToString(),
						state2.TimeTokens.ToString()
					}
				});
			}
			if (state1.TrophyTokens != state2.TrophyTokens)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 507, "Player State TrophyTokens does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3]
					{
						"Actor ID",
						state1.Actor?.ID.ToString(),
						state1.Actor?.ID.ToString()
					},
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"TrophyTokens",
						state1.TrophyTokens.ToString(),
						state2.TrophyTokens.ToString()
					}
				});
			}
			if (state1.ResonanceTokens != state2.ResonanceTokens)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 507, "Player State ResonanceTokens does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3]
					{
						"Actor ID",
						state1.Actor?.ID.ToString(),
						state1.Actor?.ID.ToString()
					},
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"ResonanceTokens",
						state1.ResonanceTokens.ToString(),
						state2.ResonanceTokens.ToString()
					}
				});
			}
			if (state1.Agility != state2.Agility)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 508, "Player State Agility does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3]
					{
						"Actor ID",
						state1.Actor?.ID.ToString(),
						state1.Actor?.ID.ToString()
					},
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Agility",
						state1.Agility.ToString(),
						state2.Agility.ToString()
					}
				});
			}
			if (state1.Form != state2.Form)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 509, "Player State Form does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3]
					{
						"Actor ID",
						state1.Actor?.ID.ToString(),
						state1.Actor?.ID.ToString()
					},
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Form",
						state1.Form.ToString(),
						state2.Form.ToString()
					}
				});
			}
			if (state1.PrevForm != state2.PrevForm)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 510, "Player State PrevForm does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3]
					{
						"Actor ID",
						state1.Actor?.ID.ToString(),
						state1.Actor?.ID.ToString()
					},
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"PrevForm",
						state1.PrevForm.ToString(),
						state2.PrevForm.ToString()
					}
				});
			}
			if (state1.PendingForm != state2.PendingForm)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 511, "Player State PendingForm does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3]
					{
						"Actor ID",
						state1.Actor?.ID.ToString(),
						state1.Actor?.ID.ToString()
					},
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"PendingForm",
						state1.PendingForm.ToString(),
						state2.PendingForm.ToString()
					}
				});
			}
			if (state1.NextSummonID != state2.NextSummonID)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 512, "Player State NextSummonID does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3]
					{
						"Actor ID",
						state1.Actor?.ID.ToString(),
						state1.Actor?.ID.ToString()
					},
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"NextSummonID",
						state1.NextSummonID.ToString(),
						state2.NextSummonID.ToString()
					}
				});
			}
			if (state1.Pressure != state2.Pressure)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 508, "Player State Pressure does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3]
					{
						"Actor ID",
						state1.Actor?.ID.ToString(),
						state1.Actor?.ID.ToString()
					},
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Pressure",
						state1.Pressure.ToString(),
						state2.Pressure.ToString()
					}
				});
			}
			list.AddRange(AbilityDeckState.Compare(state1.AbilityDeck, state2.AbilityDeck, state1.ActorGuid, state1.ClassID, isMPCompare, state1.Actor?.ID.ToString()));
			list.AddRange(AttackModifierDeckState.Compare(state1.AttackModifierDeck, state2.AttackModifierDeck, state1.ActorGuid, state1.ClassID, state1.Actor?.ID.ToString(), isMPCompare));
			switch (StateShared.CheckNullsMatch(state1.Items, state2.Items))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 513, "Player State Items null state does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3]
					{
						"Actor ID",
						state1.Actor?.ID.ToString(),
						state1.Actor?.ID.ToString()
					},
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Items",
						(state1.Items == null) ? "is null" : "is not null",
						(state2.Items == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (state1.Items.Count != state2.Items.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 514, "Player State Items Count does not match.", new List<string[]>
					{
						new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
						new string[3]
						{
							"Actor ID",
							state1.Actor?.ID.ToString(),
							state1.Actor?.ID.ToString()
						},
						new string[3] { "Class ID", state1.ClassID, state2.ClassID },
						new string[3]
						{
							"Items Count",
							state1.Items.Count.ToString(),
							state2.Items.Count.ToString()
						}
					});
					break;
				}
				bool flag = false;
				foreach (CItem item in state1.Items)
				{
					if (!state2.Items.Exists((CItem e) => e.ItemGuid == item.ItemGuid))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 515, "Player State Items in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing item from " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
							new string[3]
							{
								"Actor ID",
								state1.Actor?.ID.ToString(),
								state1.Actor?.ID.ToString()
							},
							new string[3] { "Class ID", state1.ClassID, state2.ClassID },
							new string[3] { "Items GUID", item.ItemGuid, "Missing" },
							new string[3]
							{
								"Items Name",
								item.YMLData.Name,
								"Missing"
							}
						});
						flag = true;
					}
				}
				foreach (CItem item2 in state2.Items)
				{
					if (!state1.Items.Exists((CItem e) => e.ItemGuid == item2.ItemGuid))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 516, "Player State Items in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing item from " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
							new string[3]
							{
								"Actor ID",
								state1.Actor?.ID.ToString(),
								state1.Actor?.ID.ToString()
							},
							new string[3] { "Class ID", state1.ClassID, state2.ClassID },
							new string[3] { "Items GUID", "Missing", item2.ItemGuid },
							new string[3]
							{
								"Items Name",
								"Missing",
								item2.YMLData.Name
							}
						});
						flag = true;
					}
				}
				if (flag)
				{
					break;
				}
				foreach (CItem item3 in state1.Items)
				{
					try
					{
						CItem item4 = state2.Items.Single((CItem s) => s.ItemGuid == item3.ItemGuid);
						list.AddRange(CItem.Compare(item3, item4, "Player", state1.ActorGuid, state1.ClassID, isMPCompare));
					}
					catch (Exception ex)
					{
						list.Add(new Tuple<int, string>(517, "Exception during Item State compare.\n" + ex.Message + "\n" + ex.StackTrace));
					}
				}
				break;
			}
			}
		}
		catch (Exception ex2)
		{
			list.Add(new Tuple<int, string>(599, "Exception during Player State compare.\n" + ex2.Message + "\n" + ex2.StackTrace));
		}
		return list;
	}
}
