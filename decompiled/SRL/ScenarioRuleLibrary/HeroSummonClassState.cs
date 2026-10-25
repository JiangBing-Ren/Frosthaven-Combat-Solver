using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class HeroSummonClassState : ISerializable
{
	public string ClassID { get; private set; }

	public List<int> AvailableIDs { get; private set; }

	public CHeroSummonClass HeroSummonClass => CharacterClassManager.FindHeroSummonClass(ClassID);

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("ClassID", ClassID);
		info.AddValue("AvailableIDs", AvailableIDs);
	}

	public HeroSummonClassState(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "ClassID"))
				{
					if (name == "AvailableIDs")
					{
						AvailableIDs = (List<int>)info.GetValue("AvailableIDs", typeof(List<int>));
					}
				}
				else
				{
					ClassID = info.GetString("ClassID");
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize HeroSummonClassState entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public HeroSummonClassState(string classID)
	{
		ClassID = classID;
	}

	public void Save()
	{
		AvailableIDs = HeroSummonClass.AvailableIDs.ToList();
	}

	public void Load()
	{
		HeroSummonClass.LoadAvailableIDs(AvailableIDs);
	}

	public static List<Tuple<int, string>> Compare(HeroSummonClassState state1, HeroSummonClassState state2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		try
		{
			if (state1.ClassID != state2.ClassID)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 3001, "Hero Summon Class State ID does not match.", new List<string[]> { new string[3] { "Class ID", state1.ClassID, state2.ClassID } });
			}
			switch (StateShared.CheckNullsMatch(state1.AvailableIDs, state2.AvailableIDs))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 3003, "Hero Summon Class State AvailableIDs null state does not match.", new List<string[]>
				{
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"AvailableIDs",
						(state1.AvailableIDs == null) ? "is null" : "is not null",
						(state2.AvailableIDs == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (state1.AvailableIDs.Count != state2.AvailableIDs.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 3004, "Hero Summon Class State total AvailableIDs Count does not match.", new List<string[]>
					{
						new string[3] { "Class ID", state1.ClassID, state2.ClassID },
						new string[3]
						{
							"AvailableIDs Count",
							state1.AvailableIDs.Count.ToString(),
							state2.AvailableIDs.Count.ToString()
						}
					});
					break;
				}
				bool flag = false;
				foreach (int id in state1.AvailableIDs)
				{
					if (state1.AvailableIDs.Where((int w) => w == id).Count() > 1)
					{
						ScenarioState.LogMismatch(list, isMPCompare, 3005, "Hero Summon Class State AvailableIDs in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " contains duplicate ID values.", new List<string[]>
						{
							new string[3] { "Class ID", state1.ClassID, state2.ClassID },
							new string[3]
							{
								"Duplicate AvailableID",
								"ID " + id + " Count: " + state1.AvailableIDs.Where((int w) => w == id).Count(),
								"NA"
							}
						});
					}
					flag = true;
				}
				foreach (int id2 in state2.AvailableIDs)
				{
					if (state2.AvailableIDs.Where((int w) => w == id2).Count() > 1)
					{
						ScenarioState.LogMismatch(list, isMPCompare, 3005, "Hero Summon Class State AvailableIDs in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " contains duplicate ID values.", new List<string[]>
						{
							new string[3] { "Class ID", state1.ClassID, state2.ClassID },
							new string[3]
							{
								"Duplicate AvailableID",
								"NA",
								"ID " + id2 + " Count: " + state2.AvailableIDs.Where((int w) => w == id2).Count()
							}
						});
					}
					flag = true;
				}
				if (flag)
				{
					break;
				}
				foreach (int availableID in state1.AvailableIDs)
				{
					if (!state2.AvailableIDs.Contains(availableID))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 3006, "Hero Summon Class State AvailableIDs in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing a value contained in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Class ID", state1.ClassID, state2.ClassID },
							new string[3]
							{
								"AvailableID",
								availableID.ToString(),
								"Missing"
							}
						});
					}
				}
				foreach (int availableID2 in state2.AvailableIDs)
				{
					if (!state1.AvailableIDs.Contains(availableID2))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 3006, "Hero Summon Class State AvailableIDs in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing a value contained in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Class ID", state1.ClassID, state2.ClassID },
							new string[3]
							{
								"AvailableID",
								"Missing",
								availableID2.ToString()
							}
						});
					}
				}
				break;
			}
			}
		}
		catch (Exception ex)
		{
			list.Add(new Tuple<int, string>(3099, "Exception during Hero Summon Class State compare.\n" + ex.Message + "\n" + ex.StackTrace));
		}
		return list;
	}
}
