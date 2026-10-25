using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class EnemyState : ActorState, ISerializable
{
	public static readonly ScenarioManager.EPerPartySizeConfig[] PerPartySizeConfigOptions = (ScenarioManager.EPerPartySizeConfig[])Enum.GetValues(typeof(ScenarioManager.EPerPartySizeConfig));

	public Dictionary<int, ScenarioManager.EPerPartySizeConfig> ConfigPerPartySize;

	public int ID { get; set; }

	public bool IsSummon { get; set; }

	public string SummonerGuid { get; set; }

	public CActor.EType Type { get; set; }

	public int MonsterGroupType { get; set; }

	public bool HasShiftedForm { get; set; }

	public bool IsElite => MonsterClassManager.Classes.Single((CMonsterClass s) => s.ID == ClassID).NonEliteVariant != null;

	public bool IsBoss => MonsterClassManager.Classes.Single((CMonsterClass s) => s.ID == ClassID).Boss;

	public CEnemyActor Enemy
	{
		get
		{
			CEnemyActor cEnemyActor = ScenarioManager.Scenario.AllEnemies.SingleOrDefault((CEnemyActor s) => s.ActorGuid == ActorGuid);
			if (cEnemyActor == null)
			{
				cEnemyActor = ScenarioManager.Scenario.AllAllyMonsters.SingleOrDefault((CEnemyActor s) => s.ActorGuid == ActorGuid);
			}
			if (cEnemyActor == null)
			{
				cEnemyActor = ScenarioManager.Scenario.AllNeutralMonsters.SingleOrDefault((CEnemyActor s) => s.ActorGuid == ActorGuid);
			}
			if (cEnemyActor == null)
			{
				cEnemyActor = ScenarioManager.Scenario.AllEnemy2Monsters.SingleOrDefault((CEnemyActor s) => s.ActorGuid == ActorGuid);
			}
			if (cEnemyActor == null)
			{
				cEnemyActor = ScenarioManager.Scenario.AllObjects.SingleOrDefault((CObjectActor s) => s.ActorGuid == ActorGuid);
			}
			return cEnemyActor;
		}
	}

	public bool IsHiddenForCurrentPartySize => GetConfigForPartySize(ScenarioManager.CurrentScenarioState.Players?.Count ?? 0) == ScenarioManager.EPerPartySizeConfig.Hidden;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ID", ID);
		info.AddValue("IsSummon", IsSummon);
		info.AddValue("SummonerGuid", SummonerGuid);
		info.AddValue("ConfigPerPartySize", ConfigPerPartySize);
		info.AddValue("Type", Type);
		info.AddValue("MonsterGroupType", MonsterGroupType);
		info.AddValue("HasShiftedForm", HasShiftedForm);
	}

	public EnemyState(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "ID":
					ID = info.GetInt32("ID");
					break;
				case "IsSummon":
					IsSummon = info.GetBoolean("IsSummon");
					break;
				case "SummonerGuid":
					SummonerGuid = info.GetString("SummonerGuid") ?? "";
					break;
				case "ConfigPerPartySize":
					ConfigPerPartySize = (Dictionary<int, ScenarioManager.EPerPartySizeConfig>)info.GetValue("ConfigPerPartySize", typeof(Dictionary<int, ScenarioManager.EPerPartySizeConfig>));
					break;
				case "Type":
					Type = (CActor.EType)info.GetValue("Type", typeof(CActor.EType));
					break;
				case "MonsterGroupType":
					MonsterGroupType = info.GetInt32("MonsterGroupType");
					break;
				case "HasShiftedForm":
					HasShiftedForm = info.GetBoolean("HasShiftedForm");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize EnemyState entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public override void SaveFields(CActor actor)
	{
		if (actor is CEnemyActor cEnemyActor)
		{
			base.SaveFields(actor);
			ClassID = actor.Class.ID;
			ID = cEnemyActor.ID;
			IsSummon = cEnemyActor.IsSummon;
			SummonerGuid = cEnemyActor.Summoner?.ActorGuid ?? cEnemyActor.PlayerSummoner?.ActorGuid ?? "";
			Type = cEnemyActor.OriginalType;
			MonsterGroupType = cEnemyActor.MonsterGroupType;
			HasShiftedForm = cEnemyActor.HasShiftedForm;
		}
	}

	public virtual void Save(bool initial, bool forceSave = false)
	{
		if (!IsRevealed && !forceSave)
		{
			return;
		}
		CEnemyActor cEnemyActor = null;
		if (initial)
		{
			cEnemyActor = ScenarioManager.Scenario.InitialEnemies.SingleOrDefault((CEnemyActor s) => s.ActorGuid == ActorGuid);
			if (cEnemyActor == null)
			{
				cEnemyActor = ScenarioManager.Scenario.InitialAllyMonsters.SingleOrDefault((CEnemyActor s) => s.ActorGuid == ActorGuid);
			}
			if (cEnemyActor == null)
			{
				cEnemyActor = ScenarioManager.Scenario.InitialEnemy2Monsters.SingleOrDefault((CEnemyActor s) => s.ActorGuid == ActorGuid);
			}
			if (cEnemyActor == null)
			{
				cEnemyActor = ScenarioManager.Scenario.InitialNeutralMonsters.SingleOrDefault((CEnemyActor s) => s.ActorGuid == ActorGuid);
			}
		}
		else
		{
			cEnemyActor = Enemy;
		}
		SaveFields(cEnemyActor);
	}

	public virtual void Load()
	{
		if (Enemy != null)
		{
			Enemy.LoadFields(this);
		}
		else
		{
			Log.Warning("Could not find Enemy");
		}
	}

	public EnemyState(string classID, int chosenModelIndex, string actorGuid, string mapGuid, TileIndex location, int health, int maxHealth, int level, List<PositiveConditionPair> posCons, List<NegativeConditionPair> negCons, bool playedThisRound, CActor.ECauseOfDeath causeOfDeath, bool isSummon, string summonerGuid, int augmentSlots, CActor.EType type)
		: base(classID, chosenModelIndex, actorGuid, mapGuid, location, health, maxHealth, level, posCons, negCons, playedThisRound, causeOfDeath, augmentSlots)
	{
		IsSummon = isSummon;
		SummonerGuid = summonerGuid ?? "";
		Type = type;
		ConfigPerPartySize = new Dictionary<int, ScenarioManager.EPerPartySizeConfig>
		{
			{
				1,
				ScenarioManager.EPerPartySizeConfig.Normal
			},
			{
				2,
				ScenarioManager.EPerPartySizeConfig.Normal
			},
			{
				3,
				ScenarioManager.EPerPartySizeConfig.Normal
			},
			{
				4,
				ScenarioManager.EPerPartySizeConfig.Normal
			}
		};
		PlaySpawnVFX();
	}

	public EnemyState(string classID, int chosenModelIndex, string mapGuid, CActor.EType type)
		: base(classID, chosenModelIndex, null, mapGuid, null, 0, 0, 0, null, null, playedThisRound: false, CActor.ECauseOfDeath.StillAlive, 1)
	{
		Type = type;
	}

	public EnemyState(CEnemyActor enemyActor, string mapGuid)
		: base(enemyActor.Class.ID, enemyActor.ChosenModelIndex, enemyActor.ActorGuid, mapGuid, new TileIndex(enemyActor.ArrayIndex), enemyActor.Health, enemyActor.MaxHealth, enemyActor.Level, enemyActor.Tokens.CheckPositiveTokens.ToList(), enemyActor.Tokens.CheckNegativeTokens.ToList(), enemyActor.PlayedThisRound, enemyActor.CauseOfDeath, enemyActor.AugmentSlots)
	{
		IsSummon = enemyActor.IsSummon;
		SummonerGuid = enemyActor.Summoner?.ActorGuid ?? "";
		IsRevealed = true;
		ConfigPerPartySize = new Dictionary<int, ScenarioManager.EPerPartySizeConfig>
		{
			{
				1,
				ScenarioManager.EPerPartySizeConfig.Normal
			},
			{
				2,
				ScenarioManager.EPerPartySizeConfig.Normal
			},
			{
				3,
				ScenarioManager.EPerPartySizeConfig.Normal
			},
			{
				4,
				ScenarioManager.EPerPartySizeConfig.Normal
			}
		};
		Type = enemyActor.Type;
		MonsterGroupType = enemyActor.MonsterGroupType;
		PlaySpawnVFX();
	}

	public void IncreaseToElite()
	{
		ClassID = MonsterClassManager.Classes.Single((CMonsterClass s) => s.NonEliteVariant != null && s.NonEliteVariant.ID == ClassID).ID;
	}

	public void SetConfigForPartySize(int partySize, ScenarioManager.EPerPartySizeConfig configToSet)
	{
		if (ConfigPerPartySize == null || ConfigPerPartySize.Count == 0)
		{
			ConfigPerPartySize = new Dictionary<int, ScenarioManager.EPerPartySizeConfig>
			{
				{
					1,
					ScenarioManager.EPerPartySizeConfig.Normal
				},
				{
					2,
					ScenarioManager.EPerPartySizeConfig.Normal
				},
				{
					3,
					ScenarioManager.EPerPartySizeConfig.Normal
				},
				{
					4,
					ScenarioManager.EPerPartySizeConfig.Normal
				}
			};
		}
		if (ConfigPerPartySize.ContainsKey(partySize))
		{
			ConfigPerPartySize[partySize] = configToSet;
		}
		else
		{
			ConfigPerPartySize.Add(partySize, configToSet);
		}
	}

	public ScenarioManager.EPerPartySizeConfig GetConfigForPartySize(int partySize)
	{
		if (partySize < 1)
		{
			partySize = 1;
		}
		if (partySize > 4)
		{
			partySize = 4;
		}
		if (ConfigPerPartySize == null || ConfigPerPartySize.Count == 0 || !ConfigPerPartySize.ContainsKey(partySize))
		{
			if (partySize == 1)
			{
				Dictionary<int, ScenarioManager.EPerPartySizeConfig> configPerPartySize = ConfigPerPartySize;
				if (configPerPartySize != null && configPerPartySize.ContainsKey(2))
				{
					return ConfigPerPartySize[2];
				}
			}
			return ScenarioManager.EPerPartySizeConfig.Normal;
		}
		return ConfigPerPartySize[partySize];
	}

	protected void PlaySpawnVFX()
	{
		if (!IsSummon)
		{
			ScenarioRuleClient.MessageHandler(new CTryPlayVfxOnTile_MessageData(null)
			{
				m_VFXPropType = "EnemySpawnEffect",
				m_ArrayIndex = Location.ToPoint()
			});
		}
	}

	public static List<Tuple<int, string>> Compare(EnemyState state1, EnemyState state2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		list.AddRange(ActorState.Compare(state1, state2, isMPCompare));
		try
		{
			if (state1.ID != state2.ID)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 601, "Enemy State ID does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Standee ID",
						state1.ID.ToString(),
						state2.ID.ToString()
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					}
				});
			}
			if (state1.IsSummon != state2.IsSummon)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 602, "Enemy State IsSummon does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Standee ID",
						state1.ID.ToString(),
						state2.ID.ToString()
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					},
					new string[3]
					{
						"IsSummon",
						state1.IsSummon.ToString(),
						state2.IsSummon.ToString()
					}
				});
			}
			if (state1.SummonerGuid != state2.SummonerGuid)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 603, "Enemy State SummonerGuid does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Standee ID",
						state1.ID.ToString(),
						state2.ID.ToString()
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					},
					new string[3] { "SummonerGuid", state1.SummonerGuid, state2.SummonerGuid }
				});
			}
			if (state1.Type != state2.Type)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 608, "Enemy State Type does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Standee ID",
						state1.ID.ToString(),
						state2.ID.ToString()
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					},
					new string[3]
					{
						"Type",
						state1.Type.ToString(),
						state2.Type.ToString()
					}
				});
			}
			switch (StateShared.CheckNullsMatch(state1.ConfigPerPartySize, state2.ConfigPerPartySize))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 604, "Enemy State ConfigPerPartySize null state does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Standee ID",
						state1.ID.ToString(),
						state2.ID.ToString()
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					},
					new string[3]
					{
						"ConfigPerPartySize",
						(state1.ConfigPerPartySize == null) ? "is null" : "is not null",
						(state2.ConfigPerPartySize == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (state1.ConfigPerPartySize.Count != state2.ConfigPerPartySize.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 605, "Enemy State total ConfigPerPartySize Count does not match.", new List<string[]>
					{
						new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
						new string[3] { "Class ID", state1.ClassID, state2.ClassID },
						new string[3]
						{
							"Standee ID",
							state1.ID.ToString(),
							state2.ID.ToString()
						},
						new string[3]
						{
							"Location",
							state1.Location.ToString(),
							state2.Location.ToString()
						},
						new string[3]
						{
							"ConfigPerPartySize Count",
							state1.ConfigPerPartySize.Count.ToString(),
							state2.ConfigPerPartySize.Count.ToString()
						}
					});
					break;
				}
				bool flag = false;
				foreach (KeyValuePair<int, ScenarioManager.EPerPartySizeConfig> item in state1.ConfigPerPartySize)
				{
					if (!state2.ConfigPerPartySize.ContainsKey(item.Key))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 606, "Enemy State ConfigPerPartySize in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing a key that is in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
							new string[3] { "Class ID", state1.ClassID, state2.ClassID },
							new string[3]
							{
								"Standee ID",
								state1.ID.ToString(),
								state2.ID.ToString()
							},
							new string[3]
							{
								"Location",
								state1.Location.ToString(),
								state2.Location.ToString()
							},
							new string[3]
							{
								"ConfigPerPartySize Key",
								item.Key.ToString(),
								"Missing"
							}
						});
						flag = true;
					}
				}
				foreach (KeyValuePair<int, ScenarioManager.EPerPartySizeConfig> item2 in state2.ConfigPerPartySize)
				{
					if (!state1.ConfigPerPartySize.ContainsKey(item2.Key))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 606, "Enemy State ConfigPerPartySize in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing a key that is in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
							new string[3] { "Class ID", state1.ClassID, state2.ClassID },
							new string[3]
							{
								"Standee ID",
								state1.ID.ToString(),
								state2.ID.ToString()
							},
							new string[3]
							{
								"Location",
								state1.Location.ToString(),
								state2.Location.ToString()
							},
							new string[3]
							{
								"ConfigPerPartySize Key",
								"Missing",
								item2.Key.ToString()
							}
						});
						flag = true;
					}
				}
				if (flag)
				{
					break;
				}
				foreach (KeyValuePair<int, ScenarioManager.EPerPartySizeConfig> item3 in state1.ConfigPerPartySize)
				{
					if (item3.Value != state2.ConfigPerPartySize[item3.Key])
					{
						ScenarioState.LogMismatch(list, isMPCompare, 607, "Enemy State ConfigPerPartySize has key with differing values.", new List<string[]>
						{
							new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
							new string[3] { "Class ID", state1.ClassID, state2.ClassID },
							new string[3]
							{
								"Standee ID",
								state1.ID.ToString(),
								state2.ID.ToString()
							},
							new string[3]
							{
								"Location",
								state1.Location.ToString(),
								state2.Location.ToString()
							},
							new string[3]
							{
								"ConfigPerPartySize Key",
								item3.Key.ToString(),
								item3.Key.ToString()
							},
							new string[3]
							{
								"ConfigPerPartySize Value",
								item3.Value.ToString(),
								state2.ConfigPerPartySize[item3.Key].ToString()
							}
						});
						flag = true;
					}
				}
				break;
			}
			}
		}
		catch (Exception ex)
		{
			list.Add(new Tuple<int, string>(699, "Exception during Enemy State compare.\n" + ex.Message + "\n" + ex.StackTrace));
		}
		return list;
	}
}
