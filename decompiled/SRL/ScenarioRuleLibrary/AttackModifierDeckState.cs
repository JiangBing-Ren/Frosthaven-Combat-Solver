using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

[Serializable]
public class AttackModifierDeckState : ISerializable
{
	public List<string> AttackModifierCardsPool { get; private set; }

	public List<string> AttackModifierCardsAvailable { get; private set; }

	public List<string> AttackModifierCardsDiscarded { get; private set; }

	public bool ShuffleAttackModifierCards { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("AttackModifierCardsPool", AttackModifierCardsPool);
		info.AddValue("AttackModifierCardsAvailable", AttackModifierCardsAvailable);
		info.AddValue("AttackModifierCardsDiscarded", AttackModifierCardsDiscarded);
		info.AddValue("ShuffleAttackModifierCards", ShuffleAttackModifierCards);
	}

	public AttackModifierDeckState(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "AttackModifierCardsPool":
					AttackModifierCardsPool = (List<string>)info.GetValue("AttackModifierCardsPool", typeof(List<string>));
					break;
				case "AttackModifierCardsAvailable":
					AttackModifierCardsAvailable = (List<string>)info.GetValue("AttackModifierCardsAvailable", typeof(List<string>));
					break;
				case "AttackModifierCardsDiscarded":
					AttackModifierCardsDiscarded = (List<string>)info.GetValue("AttackModifierCardsDiscarded", typeof(List<string>));
					break;
				case "ShuffleAttackModifierCards":
					ShuffleAttackModifierCards = info.GetBoolean("ShuffleAttackModifierCards");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize AttackModifierDeckState entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public AttackModifierDeckState()
	{
	}

	public AttackModifierDeckState Copy()
	{
		return new AttackModifierDeckState
		{
			AttackModifierCardsPool = ((AttackModifierCardsPool != null) ? AttackModifierCardsPool.ToList() : null),
			AttackModifierCardsAvailable = ((AttackModifierCardsAvailable != null) ? AttackModifierCardsAvailable.ToList() : new List<string>()),
			AttackModifierCardsDiscarded = ((AttackModifierCardsDiscarded != null) ? AttackModifierCardsDiscarded.ToList() : new List<string>()),
			ShuffleAttackModifierCards = ShuffleAttackModifierCards
		};
	}

	public AttackModifierDeckState(CCharacterClass playerClass)
	{
		AttackModifierCardsPool = playerClass.AttackModifierCardsPool.Select((AttackModifierYMLData s) => s.Name).ToList();
		AttackModifierCardsAvailable = playerClass.AttackModifierCardsPool.Select((AttackModifierYMLData s) => s.Name).ToList();
		AttackModifierCardsDiscarded = new List<string>();
		ShuffleAttackModifierCards = playerClass.m_ShuffleAttackModifierCards;
	}

	public void SaveCharacter(CCharacterClass playerClass)
	{
		AttackModifierCardsPool = playerClass.AttackModifierCardsPool.Select((AttackModifierYMLData s) => s.Name).ToList();
		AttackModifierCardsAvailable = playerClass.AttackModifierCards.Select((AttackModifierYMLData s) => s.Name).ToList();
		AttackModifierCardsDiscarded = playerClass.DiscardedAttackModifierCards.Select((AttackModifierYMLData s) => s.Name).ToList();
		ShuffleAttackModifierCards = playerClass.m_ShuffleAttackModifierCards;
	}

	public void SaveMonster(CMonsterAttackModifierDeck monsterAttackModifierDeck)
	{
		AttackModifierCardsPool = monsterAttackModifierDeck.AttackModifierCardsPool.Select((AttackModifierYMLData s) => s.Name).ToList();
		AttackModifierCardsAvailable = monsterAttackModifierDeck.AttackModifierCards.Select((AttackModifierYMLData s) => s.Name).ToList();
		AttackModifierCardsDiscarded = monsterAttackModifierDeck.DiscardedAttackModifierCards.Select((AttackModifierYMLData s) => s.Name).ToList();
	}

	public void Load(CCharacterClass playerClass)
	{
		playerClass.LoadAttackModifierDeck(this);
	}

	public static List<Tuple<int, string>> Compare(AttackModifierDeckState state1, AttackModifierDeckState state2, string actorGuid, string classID, string id, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		try
		{
			switch (StateShared.CheckNullsMatch(state1.AttackModifierCardsAvailable, state2.AttackModifierCardsAvailable))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 1101, "AttackModifierDeck AttackModifierCardsAvailable null state does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", actorGuid, actorGuid },
					new string[3] { "Actor ID", id, id },
					new string[3] { "Class ID", classID, classID },
					new string[3]
					{
						"AttackModifierCardsAvailable",
						(state1.AttackModifierCardsAvailable == null) ? "is null" : "is not null",
						(state2.AttackModifierCardsAvailable == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
				if (state1.AttackModifierCardsAvailable.Count != state2.AttackModifierCardsAvailable.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 1102, "AttackModifierDeck AttackModifierCardsAvailable Count does not match.", new List<string[]>
					{
						new string[3] { "Actor GUID", actorGuid, actorGuid },
						new string[3] { "Actor ID", id, id },
						new string[3] { "Class ID", classID, classID },
						new string[3]
						{
							"AttackModifierCardsAvailable Count",
							state1.AttackModifierCardsAvailable.Count.ToString(),
							state2.AttackModifierCardsAvailable.Count.ToString()
						}
					});
					break;
				}
				foreach (string item in state1.AttackModifierCardsAvailable)
				{
					if (!state2.AttackModifierCardsAvailable.Contains(item))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1103, "AttackModifierDeck AttackModifierCardsAvailable in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Actor GUID", actorGuid, actorGuid },
							new string[3] { "Actor ID", id, id },
							new string[3] { "Class ID", classID, classID },
							new string[3] { "AttackModifierCardsAvailable", item, "Missing" }
						});
					}
				}
				foreach (string item2 in state2.AttackModifierCardsAvailable)
				{
					if (!state1.AttackModifierCardsAvailable.Contains(item2))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1103, "AttackModifierDeck AttackModifierCardsAvailable in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Actor GUID", actorGuid, actorGuid },
							new string[3] { "Actor ID", id, id },
							new string[3] { "Class ID", classID, classID },
							new string[3] { "AttackModifierCardsAvailable", "Missing", item2 }
						});
					}
				}
				break;
			}
			switch (StateShared.CheckNullsMatch(state1.AttackModifierCardsDiscarded, state2.AttackModifierCardsDiscarded))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 1104, "AttackModifierDeck AttackModifierCardsDiscarded null state does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", actorGuid, actorGuid },
					new string[3] { "Actor ID", id, id },
					new string[3] { "Class ID", classID, classID },
					new string[3]
					{
						"AttackModifierCardsDiscarded",
						(state1.AttackModifierCardsDiscarded == null) ? "is null" : "is not null",
						(state2.AttackModifierCardsDiscarded == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
				if (state1.AttackModifierCardsDiscarded.Count != state2.AttackModifierCardsDiscarded.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 1105, "AttackModifierDeck AttackModifierCardsDiscarded Count does not match.", new List<string[]>
					{
						new string[3] { "Actor GUID", actorGuid, actorGuid },
						new string[3] { "Actor ID", id, id },
						new string[3] { "Class ID", classID, classID },
						new string[3]
						{
							"AttackModifierCardsDiscarded Count",
							state1.AttackModifierCardsDiscarded.Count.ToString(),
							state2.AttackModifierCardsDiscarded.Count.ToString()
						}
					});
					break;
				}
				foreach (string item3 in state1.AttackModifierCardsDiscarded)
				{
					if (!state2.AttackModifierCardsDiscarded.Contains(item3))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1106, "AttackModifierDeck AttackModifierCardsDiscarded in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Actor GUID", actorGuid, actorGuid },
							new string[3] { "Actor ID", id, id },
							new string[3] { "Class ID", classID, classID },
							new string[3] { "AttackModifierCardsDiscarded", item3, "Missing" }
						});
					}
				}
				foreach (string item4 in state2.AttackModifierCardsDiscarded)
				{
					if (!state1.AttackModifierCardsDiscarded.Contains(item4))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1106, "AttackModifierDeck AttackModifierCardsDiscarded in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Actor GUID", actorGuid, actorGuid },
							new string[3] { "Actor ID", id, id },
							new string[3] { "Class ID", classID, classID },
							new string[3] { "AttackModifierCardsDiscarded", "Missing", item4 }
						});
					}
				}
				break;
			}
		}
		catch (Exception ex)
		{
			list.Add(new Tuple<int, string>(599, "Exception during AttackModifierDeck State compare.\n" + ex.Message + "\n" + ex.StackTrace));
		}
		return list;
	}
}
