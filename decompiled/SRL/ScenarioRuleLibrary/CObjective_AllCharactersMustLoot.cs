using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjective_AllCharactersMustLoot : CObjective
{
	public int CurrentCharactersLooted { get; private set; }

	public override string LocKey
	{
		get
		{
			if (string.IsNullOrEmpty(CustomLocKey))
			{
				if (Result == EObjectiveResult.Win)
				{
					return "GUI_OBJECTIVE_ALL_CHARACTERS_MUST_LOOT_WIN";
				}
				if (Result == EObjectiveResult.Lose)
				{
					return "GUI_OBJECTIVE_ALL_CHARACTERS_MUST_LOOT_LOSE";
				}
				return string.Empty;
			}
			return CustomLocKey;
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("CurrentCharactersLooted", CurrentCharactersLooted);
	}

	public CObjective_AllCharactersMustLoot(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				if (item.Name == "CurrentCharactersLooted")
				{
					CurrentCharactersLooted = info.GetInt32("CurrentCharactersLooted");
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CObjective_AllCharactersMustLoot entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CObjective_AllCharactersMustLoot(EObjectiveResult result, CObjectiveFilter objectiveFilter, bool hideCounter, bool activeFromStart = true, int? activateOnRound = null, string customLoc = null, string customTileHoverLoc = null, string eventIdentifier = null, bool isHidden = false, bool isOptional = false, bool winDespiteExhaustion = false, bool enoughToWinAlone = false, Dictionary<string, bool> requiredObjectiveStates = null)
		: base(EObjectiveType.AllCharactersMustLoot, result, objectiveFilter, hideCounter, activeFromStart, activateOnRound, customLoc, customTileHoverLoc, eventIdentifier, isHidden, isOptional, winDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates)
	{
	}

	public override bool CheckObjectiveComplete(int partySize, bool isEndOfRound = false)
	{
		List<CObjectProp> list = ScenarioManager.CurrentScenarioState.Props.Where((CObjectProp p) => p.IsLootable && p.GetConfigForPartySize(partySize) != ScenarioManager.EPerPartySizeConfig.Hidden && ObjectiveFilter.IsValidLootTarget(p.ObjectType)).ToList();
		List<CObjectProp> source = ScenarioManager.CurrentScenarioState.ActivatedProps.Where((CObjectProp p) => p.IsLootable && ObjectiveFilter.IsValidLootTarget(p.ObjectType)).ToList();
		int num = 0;
		bool flag = true;
		foreach (PlayerState player in ScenarioManager.CurrentScenarioState.Players)
		{
			bool flag2 = false;
			flag2 = ScenarioManager.Scenario.AllPlayers.FirstOrDefault((CPlayerActor p) => p.ActorGuid == player.ActorGuid)?.IsDeadForObjectives ?? player.IsDeadForObjectives;
			if (!source.Any((CObjectProp p) => p.ActorActivated == player.ActorGuid))
			{
				flag = false;
				if (flag2)
				{
					UnableToComplete = true;
					break;
				}
			}
			else
			{
				num++;
			}
		}
		CurrentCharactersLooted = num;
		if (!flag && list.Count == 0)
		{
			UnableToComplete = true;
		}
		IsComplete = flag && CheckOtherObjectiveStatesRequirement();
		return IsComplete;
	}

	public override float GetObjectiveProgress(int partySize, out int total, out int current)
	{
		current = CurrentCharactersLooted;
		total = ScenarioManager.CurrentScenarioState.Players.Count;
		return Math.Min(1f, (float)current / (float)total);
	}

	public override int GetObjectiveCompletionValue(int partySize)
	{
		if (ScenarioManager.CurrentScenarioState == null)
		{
			return 0;
		}
		return ScenarioManager.CurrentScenarioState.Players.Count;
	}
}
