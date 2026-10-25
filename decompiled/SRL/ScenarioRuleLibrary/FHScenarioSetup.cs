using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using SharedLibrary;

namespace ScenarioRuleLibrary;

[Serializable]
public class FHScenarioSetup : ISerializable
{
	public class CPEntry
	{
		public uint Id;

		public List<object> ConclusionParameters;

		public uint ConclusionIndex;
	}

	public uint ScenarioID;

	public int NextRandomItemID;

	public bool RandomItemLooted;

	public bool IsWinter;

	public bool IsInPrologue;

	public bool IsInTutorial;

	public int Level;

	public bool WasCompleted;

	public uint NextRandomScenarioID;

	public uint NextRandomItemBlueprintID;

	public uint ExtraGoldOnLootCard;

	public bool MustHaveUnusedSmallItem;

	public List<string> UnlockedClassIDs;

	public uint GoldConversion = 1u;

	public uint GoldConversionNextLevel = 1u;

	public int TrapDamage = 2;

	public int HazardousTerrainDamage = 2;

	public bool HasBeenRestarted;

	public bool IsAlternativeLevel;

	public string ConclusionParametersJSON = "__INVALID__";

	public bool CanNotBeLost;

	public List<ElementInfusionBoardManager.EElement> WaningToInertDisabled;

	public List<string> AlreadyLootedChests = new List<string>();

	[StateDump(Skip = true)]
	public Dictionary<uint, string> CachedScenarioData = new Dictionary<uint, string>();

	public bool ParseScenariosConclusionParameters(out List<CPEntry> all)
	{
		bool result = true;
		try
		{
			all = JsonConvert.DeserializeObject<List<CPEntry>>(ConclusionParametersJSON);
		}
		catch
		{
			all = null;
			result = false;
		}
		all = ((all != null) ? all : new List<CPEntry>());
		return result;
	}

	public bool GetConclusionParameters(uint scenarioId, List<CPEntry> all, out List<object> result)
	{
		foreach (CPEntry item in all)
		{
			if (item.Id == scenarioId)
			{
				result = item.ConclusionParameters;
				return true;
			}
		}
		result = null;
		return false;
	}

	public bool GetConclusionIndex(uint scenarioId, List<CPEntry> all, out uint result)
	{
		foreach (CPEntry item in all)
		{
			if (item.Id == scenarioId)
			{
				result = item.ConclusionIndex;
				return true;
			}
		}
		result = 0u;
		return false;
	}

	public static string ConclusionParamsToJSON(List<CPEntry> all)
	{
		return JsonConvert.SerializeObject(all);
	}

	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		FieldInfo[] fields = GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			info.AddValue(fieldInfo.Name, fieldInfo.GetValue(this));
		}
	}

	private FieldInfo[] GetFields()
	{
		return GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
	}

	public FHScenarioSetup Copy()
	{
		return (FHScenarioSetup)MemberwiseClone();
	}

	public static FHScenarioSetup MakeDummy(SharedLibrary.Random rng)
	{
		List<int> list = new List<int>();
		for (int i = 1261; i < 1285; i++)
		{
			list.Add(i);
		}
		list.Shuffle(rng);
		return new FHScenarioSetup
		{
			NextRandomItemID = list.FirstOrDefault(),
			RandomItemLooted = false
		};
	}

	public FHScenarioSetup()
	{
	}

	public FHScenarioSetup(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry entry in info)
		{
			FieldInfo[] fields = GetFields();
			try
			{
				FieldInfo fieldInfo = fields.FirstOrDefault((FieldInfo f) => f.Name == entry.Name);
				if (fieldInfo != null)
				{
					object value = info.GetValue(fieldInfo.Name, fieldInfo.FieldType);
					fieldInfo.SetValue(this, value);
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize FHScenariSetup entry " + entry.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}
}
