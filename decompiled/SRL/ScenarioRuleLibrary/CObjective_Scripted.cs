using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjective_Scripted : CObjective
{
	public delegate List<CActor> CustomActorFilter();

	public delegate List<CObjectProp> CustomPropFilter();

	public int Total;

	public int Current;

	public bool ScriptedIsComplete;

	public bool ScriptedUnableToComplete;

	public bool ScriptedEnoughToWinAlone;

	public string ScriptedLocKey;

	public bool HideOnComplete;

	[NonSerialized]
	public CustomActorFilter ValidActors;

	[NonSerialized]
	public CustomPropFilter ValidProps;

	public override bool RemovesFromUIOnComplete => HideOnComplete;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("Total", Total);
		info.AddValue("Current", Current);
		info.AddValue("ScriptedIsComplete", ScriptedIsComplete);
		info.AddValue("ScriptedUnableToComplete", ScriptedUnableToComplete);
		info.AddValue("ScriptedEnoughToWinAlone", ScriptedEnoughToWinAlone);
		info.AddValue("ScriptedLocKey", ScriptedLocKey);
		info.AddValue("HideOnComplete", HideOnComplete);
	}

	public CObjective_Scripted(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				switch (name)
				{
				case "Total":
					Total = info.GetInt32(name);
					break;
				case "Current":
					Current = info.GetInt32(name);
					break;
				case "ScriptedIsComplete":
					ScriptedIsComplete = info.GetBoolean(name);
					break;
				case "ScriptedUnableToComplete":
					ScriptedUnableToComplete = info.GetBoolean(name);
					break;
				case "ScriptedEnoughToWinAlone":
					ScriptedEnoughToWinAlone = info.GetBoolean(name);
					break;
				case "ScriptedLocKey":
					ScriptedLocKey = info.GetString(name);
					break;
				case "HideOnComplete":
					HideOnComplete = info.GetBoolean(name);
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport($"Exception while trying to deserialize {this} entry {item.Name}\n{ex.Message}\n{ex.StackTrace}");
				throw ex;
			}
		}
	}

	public CObjective_Scripted(EObjectiveResult result, CObjectiveFilter objectiveFilter, bool hideCounter, bool activeFromStart = true, int? activateOnRound = null, string customLoc = null, string customTileHoverLoc = null, string eventIdentifier = null, bool isHidden = false, bool isOptional = false, bool winDespiteExhaustion = false, bool enoughToWinAlone = false, Dictionary<string, bool> requiredObjectiveStates = null)
		: base(EObjectiveType.Scripted, result, objectiveFilter, hideCounter, activeFromStart, activateOnRound, customLoc, customTileHoverLoc, eventIdentifier, isHidden, isOptional, winDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates)
	{
	}

	public override bool CheckObjectiveComplete(int partySize, bool isEndOfRound = false)
	{
		IsComplete = ScriptedIsComplete;
		UnableToComplete = ScriptedUnableToComplete;
		EnoughToWinAlone = ScriptedEnoughToWinAlone;
		return IsComplete;
	}

	public override float GetObjectiveProgress(int partySize, out int total, out int current)
	{
		total = Total;
		current = Current;
		if (Total <= 0)
		{
			return 0f;
		}
		return (float)Current / (float)Total;
	}

	public bool IsInitialized()
	{
		if (Total == Current && string.IsNullOrEmpty(ScriptedLocKey) && !ScriptedIsComplete)
		{
			return ScriptedUnableToComplete;
		}
		return true;
	}
}
