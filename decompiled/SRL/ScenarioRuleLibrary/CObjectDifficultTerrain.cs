using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using AStar;
using SharedLibrary;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjectDifficultTerrain : CObjectProp, ISerializable
{
	public bool TreatAsTrap { get; set; }

	public CObjectiveFilter TreatAsTrapFilter { get; set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("TreatAsTrap", TreatAsTrap);
		info.AddValue("TreatAsTrapFilter", TreatAsTrapFilter);
	}

	public CObjectDifficultTerrain(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "TreatAsTrap"))
				{
					if (name == "TreatAsTrapFilter")
					{
						TreatAsTrapFilter = (CObjectiveFilter)info.GetValue("TreatAsTrapFilter", typeof(CObjectiveFilter));
					}
				}
				else
				{
					TreatAsTrap = info.GetBoolean("TreatAsTrap");
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CObjectDifficultTerrain entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CObjectDifficultTerrain(string name, ScenarioManager.ObjectImportType type, TileIndex arrayIndex, CVector3 position, CVector3 rotation, CActor owner, string mapGuid, bool treatAsTrap = false, CObjectiveFilter treatAsTrapFilter = null)
		: base(name, type, arrayIndex, position, rotation, owner, mapGuid)
	{
		TreatAsTrap = treatAsTrap;
		TreatAsTrapFilter = treatAsTrapFilter;
		InitDifficultTerrain();
	}

	public CObjectDifficultTerrain(string name, ScenarioManager.ObjectImportType type, string mapGuid)
		: base(name, type, null, null, null, null, mapGuid)
	{
	}

	public CObjectDifficultTerrain(SharedLibrary.Random scenarioGenerationRNG, string name, ScenarioManager.ObjectImportType type, string mapGuid)
		: base(scenarioGenerationRNG, name, type, null, null, null, null, mapGuid)
	{
	}

	public override bool Activate(CActor actor, CActor creditActor = null, bool actorIsJumping = false, bool actorIsFlying = false)
	{
		if (!Activated)
		{
			ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.SingleOrDefault((ActorState x) => x.ActorGuid == actor.ActorGuid);
			if (TreatAsTrap && (TreatAsTrapFilter == null || TreatAsTrapFilter.IsValidTarget(actorState)))
			{
				int trapDamage = ScenarioManager.TrapDamage;
				if ((actor.Type == CActor.EType.Player || actor.Type == CActor.EType.HeroSummon || actor.Type == CActor.EType.Ally) && !ScenarioRuleClient.IsWorkerThread)
				{
					ScenarioRuleClient.AddSRLQueueMessage(new CSRLHazardousTerrainMessage(this, actor), processImmediately: false);
					return false;
				}
				if (ScenarioManager.Scenario.HasActor(actor))
				{
					int health = actor.Health;
					bool actorWasAsleep = actor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
					CActor killer = null;
					CAbility causeOfDeathAbility = null;
					GameState.ResolveKillCredit(ref killer, actor, CActor.ECauseOfDeath.Trap, ref causeOfDeathAbility);
					GameState.ActorBeenDamaged(actor, trapDamage, out var triggeredPreventDamage, out var _, checkIfPlayerCanAvoidDamage: true, killer, null, CAbility.EAbilityType.None, 0, isTrapDamage: false, isTerrainDamage: true);
					if ((!(actor.Class is CCharacterClass) || GameState.PlayerSelectedToAvoidDamage == GameState.EAvoidDamageOption.None) && GameState.ActorHealthCheck(actor, actor, isTrap: false, isTerrain: true, actorWasAsleep) && !triggeredPreventDamage)
					{
						CActorBeenDamaged_MessageData message = new CActorBeenDamaged_MessageData(actor)
						{
							m_ActorBeingDamaged = actor,
							m_DamageAbility = null,
							m_ActorOriginalHealth = health,
							m_ActorWasAsleep = actorWasAsleep
						};
						ScenarioRuleClient.MessageHandler(message);
					}
				}
			}
			ActorActivated = ((actor == null) ? string.Empty : actor.ActorGuid);
			CActivateProp_MessageData message2 = new CActivateProp_MessageData(actor)
			{
				m_Prop = this,
				m_InitialLoad = false
			};
			ScenarioRuleClient.MessageHandler(message2);
			Activated = false;
		}
		return false;
	}

	public override bool WillActivationDamageActor(CActor actor)
	{
		int num = ScenarioManager.TrapDamage;
		ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.SingleOrDefault((ActorState x) => x.ActorGuid == actor.ActorGuid);
		if (TreatAsTrap && (TreatAsTrapFilter == null || TreatAsTrapFilter.IsValidTarget(actorState)))
		{
			int strength = 1;
			CAbilityMove.GetMoveBonuses(actor, out var _, out var fly, out var _, out var ignoreHazardousTerrain, ref strength);
			if (fly | ignoreHazardousTerrain)
			{
				num = 0;
			}
		}
		else
		{
			num = 0;
		}
		return num > 0;
	}

	public override bool WillActivationKillActor(CActor actor)
	{
		ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.SingleOrDefault((ActorState x) => x.ActorGuid == actor.ActorGuid);
		if (TreatAsTrap && (TreatAsTrapFilter == null || TreatAsTrapFilter.IsValidTarget(actorState)))
		{
			int incomingDamage = ScenarioManager.TrapDamage;
			int strength = 1;
			CAbilityMove.GetMoveBonuses(actor, out var _, out var fly, out var _, out var ignoreHazardousTerrain, ref strength);
			if (fly | ignoreHazardousTerrain)
			{
				incomingDamage = 0;
			}
			return actor.WillActorDie(incomingDamage);
		}
		return false;
	}

	public void InitDifficultTerrain()
	{
		foreach (Point tile in GetTiles())
		{
			ScenarioManager.PathFinder.Nodes[tile.X, tile.Y].IsDifficultTerrain = true;
			if (ObjectType == ScenarioManager.ObjectImportType.TerrainWater)
			{
				ScenarioManager.PathFinder.Nodes[tile.X, tile.Y].IsWaterTerrain = true;
			}
		}
	}

	public override void DestroyProp(float spawnDelay = 0f, bool sendMessageToClient = true, bool forceDestroy = false, CAbility destroyedByAbility = null, bool checkLinkedProp = true)
	{
		if (IsDestroyed)
		{
			return;
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventObjectProp(ESESubTypeObjectProp.Destroyed, ObjectType, PropGuid, PrefabName, null, "Destroyed", m_OwnerGuid, "", new List<CMap>(), ArrayIndex.ToPoint()));
		RemoveDifficultTerrain();
		ScenarioManager.CurrentScenarioState.DestroyedProps.Add(this);
		if (destroyedByAbility != null)
		{
			SEventAbility.SEventAbilityBaseData baseData = new SEventAbility.SEventAbilityBaseData(destroyedByAbility.TargetingActor, ESESubTypeAbility.AbilityEnded, destroyedByAbility);
			SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityDestroyProp(PropType, SpecificPropType, baseData));
		}
		if (HasLinkedProps)
		{
			foreach (CObjectProp item in GetLinkedPropsTotal())
			{
				item.DestroyProp(0f, sendMessageToClient: true, forceDestroy: false, null, checkLinkedProp: false);
			}
		}
		if (PropType == EPropType.TerrainWater && (destroyedByAbility == null || !(destroyedByAbility is CAbilityMoveTerrain)))
		{
			CObjectProp.CheckRelocateWaterMonsters();
		}
	}

	public void RemoveDifficultTerrain(bool sendMessageToClient = true)
	{
		foreach (Point tile in GetTiles())
		{
			ScenarioManager.PathFinder.Nodes[tile.X, tile.Y].IsDifficultTerrain = false;
			ScenarioManager.PathFinder.Nodes[tile.X, tile.Y].IsWaterTerrain = false;
		}
		ScenarioManager.Tiles[ArrayIndex.X, ArrayIndex.Y].DelProp(PropGuid, recursiveTiles: true);
		ScenarioManager.CurrentScenarioState.Props.RemoveAll((CObjectProp p) => p.PropGuid == PropGuid);
		if (sendMessageToClient)
		{
			CDestroyProp_MessageData message = new CDestroyProp_MessageData
			{
				m_Prop = this,
				m_DestroyDelay = 0f,
				m_ForceDestroy = false,
				m_PropTile = PropTile
			};
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	public static List<Tuple<int, string>> Compare(CObjectDifficultTerrain difficult1, CObjectDifficultTerrain difficult2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		list.AddRange(CObjectProp.Compare(difficult1, difficult2, isMPCompare));
		return list;
	}
}
