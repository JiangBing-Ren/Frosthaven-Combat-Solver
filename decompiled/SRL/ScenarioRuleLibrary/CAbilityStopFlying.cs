using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CAbilityStopFlying : CAbilityCondition
{
	public CAbilityStopFlying()
		: base(EAbilityType.StopFlying, int.MaxValue, EConditionDecTrigger.Never)
	{
	}

	public override bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		LogIsApplying(actorsAppliedTo);
		CActorIsApplyingConditionActiveBonus_MessageData message = new CActorIsApplyingConditionActiveBonus_MessageData(AnimOverload, actorApplying)
		{
			m_Ability = this,
			m_ActorsAppliedTo = actorsAppliedTo
		};
		ScenarioRuleClient.MessageHandler(message);
		return false;
	}

	protected override bool LogApplyToAfterApplied()
	{
		return false;
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			LogApplyToActor(actor);
			AbilityHasHappened = true;
			actor.ApplyCondition(TargetingActor, CCondition.ENegativeCondition.StopFlying, 0, EConditionDecTrigger.Never, "", IsMapCondition, canGoOverCurseLimit: false, isCauseByAnotherCondition: false, -1, TempDisabledConditions);
			if (m_NegativeConditions.Count > 0)
			{
				ProcessNegativeStatusEffects(actor);
			}
			if (!TempDisabledConditions)
			{
				CheckPostStopFlying(actor);
			}
			return true;
		}
		return false;
	}

	public static void CheckPostStopFlying(CActor actor)
	{
		CTile cTile = ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y];
		if (CAbilityFilter.IsValidTile(cTile, CAbilityFilter.EFilterTile.Obstacle))
		{
			CTile propTile = null;
			List<CTile> list = new List<CTile>();
			for (int i = 0; i < 5; i++)
			{
				List<CTile> allUnblockedTilesFromOriginNoDoors = ScenarioManager.GetAllUnblockedTilesFromOriginNoDoors(cTile, i + 1);
				List<CTile> list2 = new List<CTile>();
				foreach (CTile item in allUnblockedTilesFromOriginNoDoors)
				{
					if (!ScenarioManager.PathFinder.Nodes[item.m_ArrayIndex.X, item.m_ArrayIndex.Y].Blocked && ScenarioManager.PathFinder.Nodes[item.m_ArrayIndex.X, item.m_ArrayIndex.Y].Walkable && !CAbilityFilter.IsValidTile(item, CAbilityFilter.EFilterTile.Actor, includeInitial: true) && CObjectProp.FindPropWithPathingBlocker(item.m_ArrayIndex, ref propTile) == null)
					{
						list2.Add(item);
					}
				}
				if (list2.Count > 0)
				{
					list.AddRange(list2);
					break;
				}
			}
			if (list.Count > 0)
			{
				CTile cTile2 = list[ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(list.Count)];
				actor.ArrayIndex = cTile2.m_ArrayIndex;
				CActorHasPushed_MessageData cActorHasPushed_MessageData = new CActorHasPushed_MessageData(actor);
				cActorHasPushed_MessageData.m_PushAbility = null;
				cActorHasPushed_MessageData.m_ActorBeingPushed = actor;
				ScenarioRuleClient.MessageHandler(cActorHasPushed_MessageData);
				TriggerTrapsAndHazards(actor, cTile2);
			}
		}
		else
		{
			TriggerTrapsAndHazards(actor, cTile);
		}
	}

	public static void TriggerTrapsAndHazards(CActor actor, CTile actorTile)
	{
		for (int num = actorTile.m_Props.Count - 1; num >= 0; num--)
		{
			CObjectProp cObjectProp = actorTile.m_Props[num];
			if (!CObjectProp.TryChooseToSkipTrap(actor, cObjectProp) && (cObjectProp.WillActivationDamageActor(actor) || cObjectProp is CObjectTrap) && (!actor.IgnoreHazardousTerrain || (cObjectProp.ObjectType != ScenarioManager.ObjectImportType.TerrainHotCoals && cObjectProp.ObjectType != ScenarioManager.ObjectImportType.TerrainThorns)))
			{
				cObjectProp.AutomaticActivate(actor);
			}
		}
	}

	public override bool IsPositive()
	{
		return false;
	}
}
