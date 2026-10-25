using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjective_KillAllEnemies : CObjective
{
	[StateDump(Skip = true)]
	public bool CheckedEnemies { get; private set; }

	public int CurrentKills { get; private set; }

	public int ValidTargets { get; private set; }

	public override bool RemovesFromUIOnComplete => false;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("CurrentKills", CurrentKills);
		info.AddValue("ValidTargets", ValidTargets);
	}

	public CObjective_KillAllEnemies(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "CurrentKills"))
				{
					if (name == "ValidTargets")
					{
						ValidTargets = info.GetInt32("ValidTargets");
					}
				}
				else
				{
					CurrentKills = info.GetInt32("CurrentKills");
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CObjective entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CObjective_KillAllEnemies(EObjectiveResult result, CObjectiveFilter objectiveFilter, bool hideCounter, bool activeFromStart = true, int? activateOnRound = null, string customLoc = null, string customTileHoverLoc = null, string eventIdentifier = null, bool isHidden = false, bool isOptional = false, bool winDespiteExhaustion = false, bool enoughToWinAlone = false, Dictionary<string, bool> requiredObjectiveStates = null)
		: base(EObjectiveType.KillAllEnemies, result, objectiveFilter, hideCounter, activeFromStart, activateOnRound, customLoc, customTileHoverLoc, eventIdentifier, isHidden, isOptional, winDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates)
	{
	}

	public override bool CheckObjectiveComplete(int partySize, bool isEndOfRound = false)
	{
		int num = 0;
		int num2 = 0;
		bool flag = ObjectiveFilter.FilterEnemyType.HasFlag(CAbilityFilter.EFilterEnemy.Object);
		foreach (EnemyState allEnemyState in ScenarioManager.CurrentScenarioState.AllEnemyStates)
		{
			if (!allEnemyState.IsHiddenForCurrentPartySize && !CActor.AreActorsAllied(CActor.EType.Player, allEnemyState.Type, null, allEnemyState.Actor) && ObjectiveFilter.IsValidTarget(allEnemyState) && (flag || !(allEnemyState is ObjectState) || !(allEnemyState.ClassID == "PropDummyObject")) && (!flag || !(allEnemyState is ObjectState { IsAttachedToProp: false, Health: >0 })))
			{
				num++;
				CActor enemy = allEnemyState.Enemy;
				if (enemy != null && enemy.IsDeadForObjectives)
				{
					num2++;
				}
			}
		}
		if (flag)
		{
			CTile[,] tiles = ScenarioManager.Tiles;
			foreach (CTile cTile in tiles)
			{
				if (cTile == null)
				{
					continue;
				}
				foreach (CObjectProp prop in cTile.m_Props)
				{
					if (prop.PropHealthDetails != null && prop.PropHealthDetails.HasHealth && prop.GetConfigForPartySize(ScenarioManager.CurrentScenarioState.Players.Count) != ScenarioManager.EPerPartySizeConfig.Hidden && !prop.PropActorHasBeenAssigned)
					{
						num++;
					}
				}
			}
		}
		foreach (CSpawner spawner in ScenarioManager.CurrentScenarioState.Spawners)
		{
			if (!spawner.SpawnerData.ShouldCountTowardsKillAllEnemies || spawner.GetConfigForPartySize(ScenarioManager.CurrentScenarioState.Players?.Count ?? 0) == ScenarioManager.EPerPartySizeConfig.Hidden)
			{
				continue;
			}
			if (spawner.SpawnerData.LoopSpawnPattern)
			{
				if (!spawner.HasSpawned)
				{
					num++;
				}
			}
			else
			{
				num += spawner.TotalMonstersLeftToSpawn();
			}
		}
		CurrentKills = num2;
		ValidTargets = num;
		CheckedEnemies = true;
		IsComplete = num2 >= num && CheckOtherObjectiveStatesRequirement();
		return IsComplete;
	}

	public override float GetObjectiveProgress(int partySize, out int total, out int current)
	{
		if (!CheckedEnemies)
		{
			CheckObjectiveComplete(partySize);
		}
		total = ValidTargets;
		current = CurrentKills;
		return Math.Min(1f, (float)CurrentKills / (float)ValidTargets);
	}
}
