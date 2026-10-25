using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjective_DamageQuota : CObjective
{
	public float DamagePercent { get; private set; }

	public List<int> CurrentDamageAmounts { get; private set; }

	public override bool RemovesFromUIOnComplete => false;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("CurrentDamageAmounts", CurrentDamageAmounts);
	}

	public CObjective_DamageQuota(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				if (item.Name == "CurrentDamageAmounts")
				{
					CurrentDamageAmounts = (List<int>)info.GetValue("CurrentDamageAmounts", typeof(List<int>));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CObjective_ActorsEscaped entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CObjective_DamageQuota(EObjectiveResult result, CObjectiveFilter objectiveFilter, List<int> currentDamageAmounts, bool hideCounter, bool activeFromStart = true, int? activateOnRound = null, string customLoc = null, string customTileHoverLoc = null, string eventIdentifier = null, bool isHidden = false, bool isOptional = false, bool winDespiteExhaustion = false, bool enoughToWinAlone = false, Dictionary<string, bool> requiredObjectiveStates = null)
		: base(EObjectiveType.DamageQuota, result, objectiveFilter, hideCounter, activeFromStart, activateOnRound, customLoc, customTileHoverLoc, eventIdentifier, isHidden, isOptional, winDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates)
	{
		CurrentDamageAmounts = currentDamageAmounts;
		for (int i = 0; i < objectiveFilter.FilterActorGUIDs.Count; i++)
		{
			currentDamageAmounts.Add(0);
		}
	}

	public override bool CheckObjectiveComplete(int partySize, bool isEndOfRound = false)
	{
		IsComplete = false;
		List<EnemyState> list = ScenarioManager.CurrentScenarioState.AllEnemyStates.Where((EnemyState a) => a != null && ObjectiveFilter.FilterActorGUIDs.Contains(a.ActorGuid)).ToList();
		if (!list.Any((EnemyState t) => !t.IsHiddenForCurrentPartySize && t.IsDeadForObjectives))
		{
			IsComplete = true;
			for (int num = 0; num < list.Count; num++)
			{
				EnemyState enemyState = list[num];
				if (!enemyState.IsHiddenForCurrentPartySize && CurrentDamageAmounts[num] >= enemyState.MaxHealth / 2)
				{
					IsComplete = false;
					return false;
				}
			}
		}
		return IsComplete;
	}

	public override float GetObjectiveProgress(int partySize, out int total, out int current)
	{
		Math.Max(0, partySize - 1);
		total = 0;
		current = 0;
		return 0f;
	}

	public void SetActorDamaged(ActorState actorBeingDamaged, CActor damageSource, int damageReceived)
	{
		if (damageSource != null && damageSource is CPlayerActor)
		{
			int num = ObjectiveFilter.FilterActorGUIDs.IndexOf(actorBeingDamaged.ActorGuid);
			if (num >= 0)
			{
				CurrentDamageAmounts[num] += damageReceived;
			}
		}
	}
}
