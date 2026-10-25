using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SpawnRoundEntry : ISerializable
{
	public const string cGenericTrapSpawnClassID = "PROP_BearTrap";

	public List<string> SpawnClass;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("SpawnClass", SpawnClass);
	}

	public SpawnRoundEntry(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				if (item.Name == "SpawnClass")
				{
					SpawnClass = (List<string>)info.GetValue("SpawnClass", typeof(List<string>));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SpawnRoundEntry entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SpawnRoundEntry(List<string> spawnClass)
	{
		SpawnClass = spawnClass;
	}

	public static List<Tuple<int, string>> Compare(SpawnRoundEntry state1, SpawnRoundEntry state2, string spawnerGUID, TileIndex spawnerArrayIndex, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		try
		{
			switch (StateShared.CheckNullsMatch(state1.SpawnClass, state2.SpawnClass))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 2601, "SpawnRoundEntry SpawnClass Null state does not match.", new List<string[]>
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
						"SpawnClass",
						(state1.SpawnClass == null) ? "is null" : "is not null",
						(state2.SpawnClass == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
				if (state1.SpawnClass.Count != state2.SpawnClass.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 2602, "SpawnRoundEntry SpawnClass Count does not match.", new List<string[]>
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
							"SpawnClass Count",
							state1.SpawnClass.Count.ToString(),
							state2.SpawnClass.Count.ToString()
						}
					});
					break;
				}
				foreach (string item in state1.SpawnClass)
				{
					if (!state2.SpawnClass.Contains(item))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 2603, "SpawnRoundEntry SpawnClass in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Spawner GUID", spawnerGUID, spawnerGUID },
							new string[3]
							{
								"Array Index",
								spawnerArrayIndex.ToString(),
								spawnerArrayIndex.ToString()
							},
							new string[3] { "SpawnClass", item, "Missing" }
						});
					}
				}
				foreach (string item2 in state2.SpawnClass)
				{
					if (!state1.SpawnClass.Contains(item2))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 2603, "SpawnRoundEntry SpawnClass in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Spawner GUID", spawnerGUID, spawnerGUID },
							new string[3]
							{
								"Array Index",
								spawnerArrayIndex.ToString(),
								spawnerArrayIndex.ToString()
							},
							new string[3] { "SpawnClass", "Missing", item2 }
						});
					}
				}
				break;
			}
		}
		catch (Exception ex)
		{
			list.Add(new Tuple<int, string>(2699, "Exception during SpawnRoundEntry compare.\n" + ex.Message + "\n" + ex.StackTrace));
		}
		return list;
	}
}
