using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;

[Serializable]
public class AutoTestData : CCustomLevelData
{
	public ScenarioState ExpectedResultingScenarioState;

	public int ChoreographerStepCountUntilResult;

	public DateTime ExpectedStateTimeStamp;

	public CAutoLog RecordedUIActions;

	public float CustomTimeout;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ExpectedResultingScenarioState", ExpectedResultingScenarioState);
		info.AddValue("ChoreographerStepCountUntilResult", ChoreographerStepCountUntilResult);
		info.AddValue("ExpectedStateTimeStamp", ExpectedStateTimeStamp);
		info.AddValue("RecordedUIActions", RecordedUIActions);
		info.AddValue("CustomTimeout", CustomTimeout);
	}

	public AutoTestData(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "ExpectedResultingScenarioState":
					ExpectedResultingScenarioState = (ScenarioState)info.GetValue("ExpectedResultingScenarioState", typeof(ScenarioState));
					break;
				case "ChoreographerStepCountUntilResult":
					ChoreographerStepCountUntilResult = info.GetInt32("ChoreographerStepCountUntilResult");
					break;
				case "ExpectedStateTimeStamp":
					ExpectedStateTimeStamp = (DateTime)info.GetValue("ExpectedStateTimeStamp", typeof(DateTime));
					break;
				case "RecordedUIActions":
					RecordedUIActions = (CAutoLog)info.GetValue("RecordedUIActions", typeof(CAutoLog));
					break;
				case "CustomTimeout":
					CustomTimeout = info.GetSingle("CustomTimeout");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "Exception while trying to deserialize AutoTestData entry " + item.Name);
				throw ex;
			}
		}
		if (CustomTimeout == 0f)
		{
			CustomTimeout = 300f;
		}
	}

	public AutoTestData(CCustomLevelData data)
		: base(data)
	{
		ChoreographerStepCountUntilResult = 0;
		ExpectedStateTimeStamp = DateTime.Now;
		RecordedUIActions = new CAutoLog();
		CustomTimeout = 300f;
	}

	public static async Task<string> ConvertAutotestToJSON(string filePath)
	{
		AutoTestData autoTestData;
		using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read))
		{
			SaveData.Instance.CurrentFileBeingDeserialized = filePath;
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			BinaryFormatter binaryFormatter2 = binaryFormatter;
			binaryFormatter2.Binder = new SerializationBinding(await SaveData.Instance.AssemblyInfoSaveDir);
			autoTestData = binaryFormatter.Deserialize(fs) as AutoTestData;
			fs.Close();
		}
		if (autoTestData != null)
		{
			return JsonConvert.SerializeObject(autoTestData, Formatting.Indented);
		}
		return string.Empty;
	}
}
