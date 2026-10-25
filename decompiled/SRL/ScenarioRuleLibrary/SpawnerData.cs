using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SpawnerData : ISerializable
{
	[Serializable]
	public enum ESpawnerTriggerType
	{
		None,
		StartRound,
		EndRound,
		Both,
		StartTurn,
		EndTurn,
		TriggeredExternally
	}

	[Serializable]
	public enum ESpawnerActivationType
	{
		None,
		ScenarioStart,
		RoomOpen,
		RoomRevealed,
		ActivatedExternally,
		PressurePlateTriggered
	}

	[Serializable]
	public enum ESpawnerEntryDifficulty
	{
		Default,
		Easy,
		Normal,
		Hard,
		Insane
	}

	public static readonly ESpawnerTriggerType[] SpawnerTriggerTypes = (ESpawnerTriggerType[])Enum.GetValues(typeof(ESpawnerTriggerType));

	public static readonly ESpawnerActivationType[] SpawnerActivationTypes = (ESpawnerActivationType[])Enum.GetValues(typeof(ESpawnerActivationType));

	public static readonly ESpawnerEntryDifficulty[] SpawnerEntryDifficulties = (ESpawnerEntryDifficulty[])Enum.GetValues(typeof(ESpawnerEntryDifficulty));

	public ESpawnerTriggerType SpawnerTriggerType { get; set; }

	public ESpawnerActivationType SpawnerActivationType { get; set; }

	public int SpawnStartRound { get; set; }

	public bool LoopSpawnPattern { get; set; }

	public bool ShouldCountTowardsKillAllEnemies { get; set; }

	public List<int> SpawnRoundInterval { get; set; }

	public Dictionary<string, List<SpawnRoundEntry>> SpawnRoundEntryDictionary { get; set; }

	public string SpawnerHoverNameLoc { get; set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("SpawnerTriggerType", SpawnerTriggerType);
		info.AddValue("SpawnerActivationType", SpawnerActivationType);
		info.AddValue("SpawnStartRound", SpawnStartRound);
		info.AddValue("LoopSpawnPattern", LoopSpawnPattern);
		info.AddValue("ShouldCountTowardsKillAllEnemies", ShouldCountTowardsKillAllEnemies);
		info.AddValue("SpawnRoundInterval", SpawnRoundInterval);
		info.AddValue("SpawnRoundEntryDictionary", SpawnRoundEntryDictionary);
		info.AddValue("SpawnerHoverNameLoc", SpawnerHoverNameLoc);
	}

	public SpawnerData(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "SpawnerTriggerType":
					SpawnerTriggerType = (ESpawnerTriggerType)info.GetValue("SpawnerTriggerType", typeof(ESpawnerTriggerType));
					break;
				case "SpawnerActivationType":
					SpawnerActivationType = (ESpawnerActivationType)info.GetValue("SpawnerActivationType", typeof(ESpawnerActivationType));
					break;
				case "SpawnStartRound":
					SpawnStartRound = info.GetInt32("SpawnStartRound");
					break;
				case "LoopSpawnPattern":
					LoopSpawnPattern = info.GetBoolean("LoopSpawnPattern");
					break;
				case "ShouldCountTowardsKillAllEnemies":
					ShouldCountTowardsKillAllEnemies = info.GetBoolean("ShouldCountTowardsKillAllEnemies");
					break;
				case "SpawnRoundInterval":
					SpawnRoundInterval = (List<int>)info.GetValue("SpawnRoundInterval", typeof(List<int>));
					break;
				case "SpawnRoundEntryDictionary":
					SpawnRoundEntryDictionary = (Dictionary<string, List<SpawnRoundEntry>>)info.GetValue("SpawnRoundEntryDictionary", typeof(Dictionary<string, List<SpawnRoundEntry>>));
					break;
				case "SpawnRoundEntries":
				{
					List<SpawnRoundEntry> value = (List<SpawnRoundEntry>)info.GetValue("SpawnRoundEntries", typeof(List<SpawnRoundEntry>));
					SpawnRoundEntryDictionary = new Dictionary<string, List<SpawnRoundEntry>>();
					SpawnRoundEntryDictionary.Add("Default", value);
					Log.Warning("Spawner Data contained old SpawnRoundEntries data");
					break;
				}
				case "SpawnerHoverNameLoc":
					SpawnerHoverNameLoc = info.GetString("SpawnerHoverNameLoc");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SpawnerData entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SpawnerData(ESpawnerTriggerType spawnerTriggerType, ESpawnerActivationType spawnerActivationType, int spawnStartRound, bool loopSpawnPattern, bool shouldCountTowardsKillAllEnemies, List<int> spawnRoundInterval, Dictionary<string, List<SpawnRoundEntry>> spawnRoundEntryDictionary, string spawnerHoverNameLoc)
	{
		SpawnerTriggerType = spawnerTriggerType;
		SpawnerActivationType = spawnerActivationType;
		SpawnStartRound = spawnStartRound;
		LoopSpawnPattern = loopSpawnPattern;
		ShouldCountTowardsKillAllEnemies = shouldCountTowardsKillAllEnemies;
		SpawnRoundInterval = spawnRoundInterval;
		SpawnRoundEntryDictionary = spawnRoundEntryDictionary;
		SpawnerHoverNameLoc = spawnerHoverNameLoc;
	}

	public SpawnerData Clone()
	{
		SpawnerData spawnerData = new SpawnerData(SpawnerTriggerType, SpawnerActivationType, SpawnStartRound, LoopSpawnPattern, ShouldCountTowardsKillAllEnemies, SpawnRoundInterval.ToList(), new Dictionary<string, List<SpawnRoundEntry>>(SpawnRoundEntryDictionary.Count), SpawnerHoverNameLoc);
		foreach (KeyValuePair<string, List<SpawnRoundEntry>> item in SpawnRoundEntryDictionary)
		{
			spawnerData.SpawnRoundEntryDictionary.Add(item.Key, item.Value.ToList());
		}
		return spawnerData;
	}

	public static List<Tuple<int, string>> Compare(SpawnerData state1, SpawnerData state2, string spawnerGUID, TileIndex spawnerArrayIndex, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		try
		{
			if (state1.SpawnerTriggerType != state2.SpawnerTriggerType)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2501, "SpawnerData SpawnerTriggerType does not match.", new List<string[]>
				{
					new string[3] { "Spawner GUID", spawnerGUID, spawnerGUID },
					new string[3]
					{
						"Array Index",
						spawnerArrayIndex.ToString(),
						spawnerArrayIndex.ToString()
					},
					new string[3]
					{
						"SpawnerTriggerType",
						state1.SpawnerTriggerType.ToString(),
						state2.SpawnerTriggerType.ToString()
					}
				});
			}
			if (state1.SpawnerActivationType != state2.SpawnerActivationType)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2502, "SpawnerData SpawnerActivationType does not match.", new List<string[]>
				{
					new string[3] { "Spawner GUID", spawnerGUID, spawnerGUID },
					new string[3]
					{
						"Array Index",
						spawnerArrayIndex.ToString(),
						spawnerArrayIndex.ToString()
					},
					new string[3]
					{
						"SpawnerActivationType",
						state1.SpawnerActivationType.ToString(),
						state2.SpawnerActivationType.ToString()
					}
				});
			}
			if (state1.SpawnStartRound != state2.SpawnStartRound)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2503, "SpawnerData SpawnStartRound does not match.", new List<string[]>
				{
					new string[3] { "Spawner GUID", spawnerGUID, spawnerGUID },
					new string[3]
					{
						"Array Index",
						spawnerArrayIndex.ToString(),
						spawnerArrayIndex.ToString()
					},
					new string[3]
					{
						"SpawnStartRound",
						state1.SpawnStartRound.ToString(),
						state2.SpawnStartRound.ToString()
					}
				});
			}
			if (state1.LoopSpawnPattern != state2.LoopSpawnPattern)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2504, "SpawnerData LoopSpawnPattern does not match.", new List<string[]>
				{
					new string[3] { "Spawner GUID", spawnerGUID, spawnerGUID },
					new string[3]
					{
						"Array Index",
						spawnerArrayIndex.ToString(),
						spawnerArrayIndex.ToString()
					},
					new string[3]
					{
						"LoopSpawnPattern",
						state1.LoopSpawnPattern.ToString(),
						state2.LoopSpawnPattern.ToString()
					}
				});
			}
			if (state1.ShouldCountTowardsKillAllEnemies != state2.ShouldCountTowardsKillAllEnemies)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2510, "SpawnerData ShouldCountTowardsKillAllEnemies does not match.", new List<string[]>
				{
					new string[3] { "Spawner GUID", spawnerGUID, spawnerGUID },
					new string[3]
					{
						"Array Index",
						spawnerArrayIndex.ToString(),
						spawnerArrayIndex.ToString()
					},
					new string[3]
					{
						"ShouldCountTowardsKillAllEnemies",
						state1.ShouldCountTowardsKillAllEnemies.ToString(),
						state2.ShouldCountTowardsKillAllEnemies.ToString()
					}
				});
			}
			if (state1.SpawnerHoverNameLoc != state2.SpawnerHoverNameLoc)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2511, "SpawnerData SpawnerHoverNameLoc does not match.", new List<string[]>
				{
					new string[3] { "Spawner GUID", spawnerGUID, spawnerGUID },
					new string[3]
					{
						"Array Index",
						spawnerArrayIndex.ToString(),
						spawnerArrayIndex.ToString()
					},
					new string[3] { "SpawnerHoverNameLoc", state1.SpawnerHoverNameLoc, state2.SpawnerHoverNameLoc }
				});
			}
			if (state1.SpawnStartRound != state2.SpawnStartRound)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2503, "SpawnerData SpawnStartRound does not match.", new List<string[]>
				{
					new string[3] { "Spawner GUID", spawnerGUID, spawnerGUID },
					new string[3]
					{
						"Array Index",
						spawnerArrayIndex.ToString(),
						spawnerArrayIndex.ToString()
					},
					new string[3]
					{
						"SpawnStartRound",
						state1.SpawnStartRound.ToString(),
						state2.SpawnStartRound.ToString()
					}
				});
			}
			switch (StateShared.CheckNullsMatch(state1.SpawnRoundInterval, state2.SpawnRoundInterval))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 2505, "SpawnerData SpawnRoundInterval Null state does not match.", new List<string[]>
				{
					new string[3] { "Spawner GUID", spawnerGUID, spawnerGUID },
					new string[3]
					{
						"Array Index",
						spawnerArrayIndex.ToString(),
						spawnerArrayIndex.ToString()
					},
					new string[3]
					{
						"SpawnRoundInterval",
						(state1.SpawnRoundInterval == null) ? "is null" : "is not null",
						(state2.SpawnRoundInterval == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
				if (state1.SpawnRoundInterval.Count != state2.SpawnRoundInterval.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 2506, "SpawnerData SpawnRoundInterval Count does not match.", new List<string[]>
					{
						new string[3] { "Spawner GUID", spawnerGUID, spawnerGUID },
						new string[3]
						{
							"Array Index",
							spawnerArrayIndex.ToString(),
							spawnerArrayIndex.ToString()
						},
						new string[3]
						{
							"SpawnRoundInterval Count",
							state1.SpawnRoundInterval.Count.ToString(),
							state2.SpawnRoundInterval.Count.ToString()
						}
					});
					break;
				}
				foreach (int item in state1.SpawnRoundInterval)
				{
					if (!state2.SpawnRoundInterval.Contains(item))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 2507, "SpawnerData SpawnRoundInterval in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Spawner GUID", spawnerGUID, spawnerGUID },
							new string[3]
							{
								"Array Index",
								spawnerArrayIndex.ToString(),
								spawnerArrayIndex.ToString()
							},
							new string[3]
							{
								"SpawnRoundInterval",
								item.ToString(),
								"Missing"
							}
						});
					}
				}
				break;
			}
			switch (StateShared.CheckNullsMatch(state1.SpawnRoundEntryDictionary, state2.SpawnRoundEntryDictionary))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 2506, "SpawnerData SpawnRoundEntryDictionary null state does not match.", new List<string[]>
				{
					new string[3] { "Spawner GUID", spawnerGUID, spawnerGUID },
					new string[3]
					{
						"Array Index",
						spawnerArrayIndex.ToString(),
						spawnerArrayIndex.ToString()
					},
					new string[3]
					{
						"SpawnRoundEntryDictionary",
						(state1.SpawnRoundEntryDictionary == null) ? "is null" : "is not null",
						(state2.SpawnRoundEntryDictionary == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (state1.SpawnRoundEntryDictionary.Count != state2.SpawnRoundEntryDictionary.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 2507, "SpawnerData total SpawnRoundEntryDictionary Count does not match.", new List<string[]>
					{
						new string[3] { "Spawner GUID", spawnerGUID, spawnerGUID },
						new string[3]
						{
							"Array Index",
							spawnerArrayIndex.ToString(),
							spawnerArrayIndex.ToString()
						},
						new string[3]
						{
							"SpawnRoundEntryDictionary Count",
							state1.SpawnRoundEntryDictionary.Count.ToString(),
							state2.SpawnRoundEntryDictionary.Count.ToString()
						}
					});
					break;
				}
				bool flag = false;
				foreach (KeyValuePair<string, List<SpawnRoundEntry>> item2 in state1.SpawnRoundEntryDictionary)
				{
					if (!state2.SpawnRoundEntryDictionary.ContainsKey(item2.Key))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 2508, "SpawnerData SpawnRoundEntryDictionary in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing a key that is in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Spawner GUID", spawnerGUID, spawnerGUID },
							new string[3]
							{
								"Array Index",
								spawnerArrayIndex.ToString(),
								spawnerArrayIndex.ToString()
							},
							new string[3] { "SpawnRoundEntryDictionary Key", item2.Key, "Missing" }
						});
						flag = true;
					}
				}
				foreach (KeyValuePair<string, List<SpawnRoundEntry>> item3 in state2.SpawnRoundEntryDictionary)
				{
					if (!state1.SpawnRoundEntryDictionary.ContainsKey(item3.Key))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 2508, "SpawnerData SpawnRoundEntryDictionary in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing a key that is in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Spawner GUID", spawnerGUID, spawnerGUID },
							new string[3]
							{
								"Array Index",
								spawnerArrayIndex.ToString(),
								spawnerArrayIndex.ToString()
							},
							new string[3] { "SpawnRoundEntryDictionary Key", "Missing", item3.Key }
						});
						flag = true;
					}
				}
				if (flag)
				{
					break;
				}
				foreach (KeyValuePair<string, List<SpawnRoundEntry>> item4 in state1.SpawnRoundEntryDictionary)
				{
					List<SpawnRoundEntry> value = item4.Value;
					List<SpawnRoundEntry> list2 = state2.SpawnRoundEntryDictionary[item4.Key];
					switch (StateShared.CheckNullsMatch(value, list2))
					{
					case StateShared.ENullStatus.Mismatch:
						ScenarioState.LogMismatch(list, isMPCompare, 2509, "SpawnerData SpawnRoundEntryDictionary Value null state does not match.", new List<string[]>
						{
							new string[3] { "Spawner GUID", spawnerGUID, spawnerGUID },
							new string[3]
							{
								"Array Index",
								spawnerArrayIndex.ToString(),
								spawnerArrayIndex.ToString()
							},
							new string[3] { "Key", item4.Key, item4.Key },
							new string[3]
							{
								"Value",
								(value == null) ? "is null" : "is not null",
								(list2 == null) ? "is null" : "is not null"
							}
						});
						break;
					case StateShared.ENullStatus.BothNotNull:
						if (value.Count != list2.Count)
						{
							ScenarioState.LogMismatch(list, isMPCompare, 2509, "SpawnerData SpawnRoundEntryDictionary Value Count does not match.", new List<string[]>
							{
								new string[3] { "Spawner GUID", spawnerGUID, spawnerGUID },
								new string[3]
								{
									"Array Index",
									spawnerArrayIndex.ToString(),
									spawnerArrayIndex.ToString()
								},
								new string[3] { "Key", item4.Key, item4.Key },
								new string[3]
								{
									"Value Count",
									value.Count.ToString(),
									list2.Count.ToString()
								}
							});
						}
						else
						{
							for (int i = 0; i < value.Count; i++)
							{
								list.AddRange(SpawnRoundEntry.Compare(value[i], list2[i], spawnerGUID, spawnerArrayIndex, isMPCompare));
							}
						}
						break;
					}
				}
				break;
			}
			}
		}
		catch (Exception ex)
		{
			list.Add(new Tuple<int, string>(2599, "Exception during SpawnerData compare.\n" + ex.Message + "\n" + ex.StackTrace));
		}
		return list;
	}
}
