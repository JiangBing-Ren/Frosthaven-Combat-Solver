using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventActorFinishedScenario : SEventActor
{
	public enum EScenarioResult
	{
		None,
		Win,
		Lose,
		Resign
	}

	public EScenarioResult ScenarioResult { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ScenarioResult", ScenarioResult);
	}

	public SEventActorFinishedScenario(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				if (item.Name == "ScenarioResult")
				{
					ScenarioResult = (EScenarioResult)info.GetValue("ScenarioResult", typeof(EScenarioResult));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventActorFinishedScenario entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventActorFinishedScenario(EScenarioResult scenarioResult, SEventActorBaseData baseData)
		: base(baseData)
	{
		ScenarioResult = scenarioResult;
	}
}
