using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using AStar;
using SharedLibrary;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjectHazardousTerrain : CObjectProp, ISerializable
{
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
	}

	public CObjectHazardousTerrain(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public CObjectHazardousTerrain(string name, ScenarioManager.ObjectImportType type, TileIndex arrayIndex, CVector3 position, CVector3 rotation, CActor owner, string mapGuid)
		: base(name, type, arrayIndex, position, rotation, owner, mapGuid)
	{
	}

	public CObjectHazardousTerrain(string name, ScenarioManager.ObjectImportType type, string mapGuid)
		: base(name, type, null, null, null, null, mapGuid)
	{
	}

	public CObjectHazardousTerrain(SharedLibrary.Random scenarioGenerationRNG, string name, ScenarioManager.ObjectImportType type, string mapGuid)
		: base(scenarioGenerationRNG, name, type, null, null, null, null, mapGuid)
	{
	}

	public override bool Activate(CActor actor, CActor creditActor = null, bool actorIsJumping = false, bool actorIsFlying = false)
	{
		if (!Activated)
		{
			ActorActivated = ((actor == null) ? string.Empty : actor.ActorGuid);
			if (actor == null)
			{
				return false;
			}
			int num = Damage(actor);
			string characterID = "";
			string overrideInternalActor = "";
			if (PhaseManager.CurrentPhase is CPhaseAction { CurrentPhaseAbility: var currentPhaseAbility } && currentPhaseAbility?.m_BaseCard is CAbilityCard cAbilityCard && !string.IsNullOrEmpty(cAbilityCard?.ClassID))
			{
				characterID = cAbilityCard.ClassID;
				overrideInternalActor = cAbilityCard.ClassID;
			}
			SEventObjectPropTrap sEventObjectPropTrap = null;
			if (actor != null)
			{
				if (actor is CPlayerActor cPlayerActor)
				{
					characterID = cPlayerActor.CharacterClass.ID;
				}
				if (actor is CHeroSummonActor { Summoner: var summoner })
				{
					characterID = summoner?.CharacterClass.ID;
				}
				if (actor.OriginalType == CActor.EType.Enemy || actor.OriginalType == CActor.EType.Enemy2)
				{
					CActor turnActor = GameState.TurnActor;
					if (turnActor != null && turnActor.Type == CActor.EType.Player)
					{
						characterID = GameState.TurnActor.Class.ID;
						overrideInternalActor = GameState.TurnActor.Class.ID;
					}
				}
				int num2 = num;
				if (actor.Tokens.HasKey(CCondition.ENegativeCondition.Brittle))
				{
					num2 *= 2;
				}
				if (actor.Tokens.HasKey(CCondition.EPositiveCondition.Ward))
				{
					num2 /= 2;
				}
				sEventObjectPropTrap = new SEventObjectPropTrap(num2, characterID, "Activate", ESESubTypeObjectProp.Activated, ObjectType, PropGuid, PrefabName, m_OwnerGuid, "", overrideInternalActor, ArrayIndex.ToPoint());
			}
			int strength = 1;
			CAbilityMove.GetMoveBonuses(actor, out var _, out var fly, out var _, out var ignoreHazardousTerrain, ref strength);
			if (fly | ignoreHazardousTerrain)
			{
				return false;
			}
			if ((actor.Type == CActor.EType.Player || actor.Type == CActor.EType.HeroSummon || actor.Type == CActor.EType.Ally) && !ScenarioRuleClient.IsWorkerThread)
			{
				ScenarioRuleClient.AddSRLQueueMessage(new CSRLHazardousTerrainMessage(this, actor), processImmediately: false);
				return false;
			}
			SEventLogMessageHandler.AddEventLogMessage(sEventObjectPropTrap);
			if (ScenarioManager.Scenario.HasActor(actor))
			{
				CActivateProp_MessageData message = new CActivateProp_MessageData(actor)
				{
					m_Prop = this,
					m_InitialLoad = false,
					m_PropEvent = sEventObjectPropTrap
				};
				ScenarioRuleClient.MessageHandler(message);
				int health = actor.Health;
				bool actorWasAsleep = actor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
				CActor killer = null;
				CAbility causeOfDeathAbility = null;
				GameState.ResolveKillCredit(ref killer, actor, CActor.ECauseOfDeath.HazardousTerrain, ref causeOfDeathAbility);
				GameState.ActorBeenDamaged(actor, num, out var triggeredPreventDamage, out var _, checkIfPlayerCanAvoidDamage: true, killer, null, CAbility.EAbilityType.None, 0, isTrapDamage: false, isTerrainDamage: true);
				if ((!(actor.Class is CCharacterClass) || GameState.PlayerSelectedToAvoidDamage == GameState.EAvoidDamageOption.None) && GameState.ActorHealthCheck(actor, actor, isTrap: false, isTerrain: true, actorWasAsleep) && !triggeredPreventDamage)
				{
					CActorBeenDamaged_MessageData message2 = new CActorBeenDamaged_MessageData(actor)
					{
						m_ActorBeingDamaged = actor,
						m_DamageAbility = null,
						m_ActorOriginalHealth = health,
						m_ActorWasAsleep = actorWasAsleep
					};
					ScenarioRuleClient.MessageHandler(message2);
				}
				if (PushDoubleHazardousDamageAndReplaceHazardousWithOneHexObstacle(actor, out var currentPushAbility) && !actor.IsDead && !PropTile.LivingActors.Any((CActor x) => x != actor) && !CAbility.WillTileBlockTerrain(PropTile, currentPushAbility.TargetingActor, null))
				{
					DestroyProp();
					TileIndex tileIndex = new TileIndex(PropTile.m_ArrayIndex);
					CObjectObstacle prop = new CObjectObstacle("PyroclastObstacle", ScenarioManager.ObjectImportType.Obstacle, tileIndex, null, null, new List<TileIndex> { tileIndex }, null, PropTile.m_HexMap.MapGuid, ignoresFlyAndJump: false);
					PropTile.SpawnProp(prop);
					CustomAbility.PlayVFX("PyroclastTerrainFormationVFX", PropTile.m_ArrayIndex);
				}
			}
			Activated = false;
		}
		return false;
	}

	public override bool WillActivationDamageActor(CActor actor)
	{
		int num = Damage(actor);
		int strength = 1;
		CAbilityMove.GetMoveBonuses(actor, out var _, out var fly, out var _, out var ignoreHazardousTerrain, ref strength);
		if (fly | ignoreHazardousTerrain)
		{
			num = 0;
		}
		return num > 0;
	}

	private bool PushDoubleHazardousDamageAndReplaceHazardousWithOneHexObstacle(CActor actor, out CAbilityPush currentPushAbility)
	{
		currentPushAbility = PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability as CAbilityPush;
		List<Point> list = currentPushAbility?.GetAbilityPath();
		if (currentPushAbility != null && currentPushAbility.CurrentTarget == actor && currentPushAbility.AdditionalPushEffect.Equals(CAbilityPush.EAdditionalPushEffect.DoubleHazardousDamageAndReplaceHazardousWithOneHexObstacle) && list != null && list.Count > 1)
		{
			return PropTile.m_ArrayIndex != list[list.Count - 1];
		}
		return false;
	}

	public override bool WillActivationKillActor(CActor actor)
	{
		int incomingDamage = Damage(actor);
		int strength = 1;
		CAbilityMove.GetMoveBonuses(actor, out var _, out var fly, out var _, out var ignoreHazardousTerrain, ref strength);
		if (fly | ignoreHazardousTerrain)
		{
			incomingDamage = 0;
		}
		return actor.WillActorDie(incomingDamage);
	}

	private int Damage(CActor actor = null)
	{
		int num = GetTerrainDamage();
		if (PushDoubleHazardousDamageAndReplaceHazardousWithOneHexObstacle(actor, out var _))
		{
			num *= 2;
		}
		return num;
	}

	public static int GetTerrainDamage()
	{
		return ScenarioManager.HazardousTerrainDamage;
	}

	public static List<Tuple<int, string>> Compare(CObjectHazardousTerrain hazard1, CObjectHazardousTerrain hazard2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		list.AddRange(CObjectProp.Compare(hazard1, hazard2, isMPCompare));
		return list;
	}

	public override bool WillActivate(CActor actor, bool actorIsJumping, bool actorIsFlying)
	{
		if (Owner != null && actor.FindApplicableActiveBonuses().Any((CActiveBonus bonus) => bonus.Caster == Owner && bonus.Ability.GetMiscVar("IgnoreOwnHazardousProps").GetBool()))
		{
			return false;
		}
		return base.WillActivate(actor, actorIsJumping, actorIsFlying);
	}

	public override bool AutomaticActivate(CActor actor, bool actorIsJumping = false, bool actorIsFlying = false)
	{
		if (!WillActivate(actor, actorIsJumping, actorIsFlying))
		{
			return false;
		}
		return base.AutomaticActivate(actor, actorIsJumping, actorIsFlying);
	}
}
