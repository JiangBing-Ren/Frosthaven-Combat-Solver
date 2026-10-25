using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH049 : ScenarioScript
{
	[Serializable]
	public class OxygenInfo
	{
		public string ActorGUID;

		public string CounterGUID;

		[JsonIgnore]
		[StateDump(Skip = true)]
		private CActiveBonus _oxygenBonus;

		[JsonIgnore]
		public CActor Actor => ScenarioManager.FindActor(ActorGUID);

		[JsonIgnore]
		public CObjectProp Prop
		{
			get
			{
				if (CounterGUID != null)
				{
					return ScenarioManager.CurrentScenarioState.Props.SingleOrDefault((CObjectProp x) => x.PropGuid == CounterGUID);
				}
				return null;
			}
			set
			{
				CounterGUID = value?.PropGuid;
			}
		}

		public bool Valid => CounterGUID != null;

		[JsonIgnore]
		public CActiveBonus oxygenbonus
		{
			get
			{
				if (_oxygenBonus != null)
				{
					return _oxygenBonus;
				}
				if (Actor == null)
				{
					return null;
				}
				_oxygenBonus = (ScenarioScript.current as ScenarioScript_FH049)?.FindOxygenBonus(Actor);
				return _oxygenBonus;
			}
		}
	}

	[Serializable]
	public class Storage049
	{
		public List<string> actorsRestoredOxygenThisRound = new List<string>();

		public List<OxygenInfo> OxygenBonusesCache = new List<OxygenInfo>();
	}

	private CObjectObstacle bathySphere;

	private CObjective_Scripted _scenarioObjectiveBathysphereEscape;

	private CObjectDoor _firstDoor;

	private CObjectDoor _secondDoor;

	private static readonly Point FirstDoorTileIndex = new Point(4, 4);

	private static readonly Point SecondDoorTileIndex = new Point(4, 5);

	public Storage049 serializationData;

	public List<OxygenInfo> OxygenBonusesCache
	{
		get
		{
			return serializationData.OxygenBonusesCache;
		}
		set
		{
			serializationData.OxygenBonusesCache = value;
		}
	}

	private List<string> actorGuidsRestoredOxygenThisRound
	{
		get
		{
			return serializationData.actorsRestoredOxygenThisRound;
		}
		set
		{
			serializationData.actorsRestoredOxygenThisRound = value;
		}
	}

	public IEnumerable<Point> GetAirPocketTiles()
	{
		yield return new Point(15, 11);
		yield return new Point(11, 9);
		yield return new Point(15, 7);
		yield return new Point(6, 9);
		yield return new Point(1, 5);
		yield return new Point(6, 1);
	}

	public IEnumerable<Point> GetExitTiles()
	{
		yield return new Point(8, 3);
	}

	protected override string SerializeInternal()
	{
		return JsonConvert.SerializeObject(serializationData);
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		if (!string.IsNullOrEmpty(storage))
		{
			try
			{
				serializationData = JsonConvert.DeserializeObject<Storage049>(storage);
			}
			catch (Exception)
			{
				List<string> actorsRestoredOxygenThisRound = JsonConvert.DeserializeObject<List<string>>(storage);
				serializationData.actorsRestoredOxygenThisRound = actorsRestoredOxygenThisRound;
			}
		}
		ToggleBathysphereHighlights(active: true);
		InitOxygenVFX();
		foreach (OxygenInfo item in OxygenBonusesCache)
		{
			UpdateOxygenCounter(item.Actor as CPlayerActor);
		}
	}

	private void InitOxygenVFX()
	{
		if (OxygenBonusesCache != null && OxygenBonusesCache.Count != 0)
		{
			return;
		}
		OxygenBonusesCache = new List<OxygenInfo>();
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			InitOxygenVFX(playerActor);
		}
	}

	private void InitOxygenVFX(CPlayerActor player)
	{
		if (player != null && FindOxygenBonus(player) != null)
		{
			CObjectProp prop = SpawnVFXProp(player.Tile, EVFXPropType.NumberCounterActor.ToString());
			OxygenInfo item = new OxygenInfo
			{
				ActorGUID = player.ActorGuid,
				Prop = prop
			};
			OxygenBonusesCache.Add(item);
		}
	}

	protected override bool Initialize()
	{
		serializationData = new Storage049();
		_firstDoor = ScenarioScript.FindDoor(FirstDoorTileIndex.X, FirstDoorTileIndex.Y);
		_secondDoor = ScenarioScript.FindDoor(SecondDoorTileIndex.X, SecondDoorTileIndex.Y);
		_firstDoor.TwinDoors = new CObjectDoor[1] { _secondDoor };
		_secondDoor.TwinDoors = new CObjectDoor[1] { _firstDoor };
		_scenarioObjectiveBathysphereEscape = (CObjective_Scripted)GetObjective("ScenarioBathysphereWinObjective");
		bathySphere = ScenarioManager.CurrentScenarioState.Props.Concat(ScenarioManager.CurrentScenarioState.DestroyedProps).SingleOrDefault((CObjectProp x) => x.SpecificPropType == ESpecificPropType.Bathysphere) as CObjectObstacle;
		if (bathySphere?.RuntimeAttachedActor != null)
		{
			bathySphere.RuntimeAttachedActor.IsImmuneToDamage = true;
			bathySphere.RuntimeAttachedActor.ForcedUntargetable = true;
			bathySphere.VisualsForceLookAtRotateChild = "Bathysphere";
		}
		PutSnorkelsOn();
		return base.Initialize();
	}

	private CActiveBonus FindOxygenBonus(CActor actor)
	{
		return actor.FindApplicableActiveBonuses(CAbility.EAbilityType.AddActiveBonus).SingleOrDefault((CActiveBonus x) => x.Ability.Name == "OxygenAbility");
	}

	protected override void TickPhaseInternal()
	{
		base.TickPhaseInternal();
		switch (_scenarioModifier.ScenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartScenario:
			UnderwaterStart();
			InitOxygenVFX(_tickActor as CPlayerActor);
			UpdateOxygenCounter(_tickActor as CPlayerActor);
			break;
		case EScenarioModifierTriggerPhase.StartRound:
			actorGuidsRestoredOxygenThisRound.Clear();
			break;
		case EScenarioModifierTriggerPhase.OnActorSpawnedOrRevealed:
			PutSnorkelsOn(_tickActor);
			break;
		case EScenarioModifierTriggerPhase.EndTurn:
			TickPhaseEndTurn();
			break;
		case EScenarioModifierTriggerPhase.EndRound:
		case EScenarioModifierTriggerPhase.StartTurn:
		case EScenarioModifierTriggerPhase.AfterAbility:
			break;
		}
	}

	private void TickPhaseEndTurn()
	{
		CheckRestoreOxygen(_tickActor);
		if ((_tickActor.Type == CActor.EType.Ally || _tickActor.Type == CActor.EType.Player) && (_tickActor.Type != CActor.EType.Player || _tickActor.AllowEscape) && GetExitTiles().Contains(_tickActor.ArrayIndex))
		{
			if (IsPointNeighborToBathysphere(_tickActor.ArrayIndex))
			{
				bathySphere.DestroyProp();
				ToggleBathysphereHighlights(active: false);
			}
			EscapeMap();
			UpdateObjectives();
		}
	}

	protected override void TickAbilityEndedInternal()
	{
		base.TickAbilityEndedInternal();
		if (bathySphere != null && !bathySphere.IsDestroyed)
		{
			bathySphere.SetIsBlocker(isBlocker: true);
		}
	}

	public static bool IsTileValidForBathysphereMove(CTile tile)
	{
		if ((ScenarioScript.current as ScenarioScript_FH049).bathySphere.PathingBlockers.Any((TileIndex x) => x.ToPoint() == tile.m_ArrayIndex))
		{
			return true;
		}
		if (tile.LivingActor != null)
		{
			return false;
		}
		CNode node = ScenarioManager.PathFinder.GetNode(tile.m_ArrayIndex);
		if (!node.Walkable && !node.IsBridge)
		{
			return false;
		}
		List<CObjectProp> list = tile.m_Props.ToList();
		lock (list)
		{
			if (list.Any((CObjectProp x) => x is CObjectDoor cObjectDoor && !cObjectDoor.DoorIsOpen))
			{
				return false;
			}
		}
		return true;
	}

	protected override void TickRepositionInternal()
	{
		if (_scenarioModifier.ScenarioModifierTriggerPhase != EScenarioModifierTriggerPhase.OnActorSpawnedOrRevealed || !(_tickActor is CPlayerActor actor))
		{
			return;
		}
		CheckRestoreOxygen(_tickActor);
		CTile tile = _tickActor.Tile;
		foreach (OxygenInfo item in OxygenBonusesCache)
		{
			if (item.ActorGUID == _tickActor.ActorGuid)
			{
				CObjectProp prop = item.Prop;
				prop.PropTile.DelProp(prop);
				prop.SetLocation(new TileIndex(tile.m_ArrayIndex), null, prop.Rotation);
				tile.AddProp(prop);
				prop.SetNewStartingMapGuid(tile.m_HexMap.MapGuid);
			}
		}
		if (bathySphere != null && !bathySphere.IsDestroyed && IsPointNeighborToBathysphere(_tickActor.PrevArrayIndex))
		{
			OfferMoveBathysphere(actor);
		}
	}

	private void OfferMoveBathysphere(CPlayerActor actor)
	{
		List<CTile> list = new List<CTile>();
		new List<CTile>();
		CTile tile = ScenarioManager.GetTile(actor.PrevArrayIndex);
		CNode node = ScenarioManager.PathFinder.GetNode(tile.m_ArrayIndex);
		bathySphere.SetIsBlocker(isBlocker: false);
		if ((node.Walkable || node.IsBridge) && ScenarioManager.Scenario.PlayerActors.Any((CPlayerActor x) => x.GetDistanceTo(tile) <= 1) && tile.LivingActor == null)
		{
			CObjectDoor cObjectDoor = tile.FindDoor();
			if (cObjectDoor == null || cObjectDoor.DoorIsOpen)
			{
				list.Add(tile);
				CAreaEffect areaEffect = CObjectProp.GetAreaEffect(bathySphere.PrefabName, bathySphere.PropType.ToString());
				areaEffect.CanFlip = false;
				areaEffect.IgnoreBlocked = true;
				List<(string, TValue)> miscVars = new List<(string, TValue)> { ("IsValidTile", new TValue(CExpression.Parse("ScenarioScript_FH049.IsTileValidForBathysphereMove(obj)"))) };
				ScenarioScript.PickHexes("$FH/FH_Scenario_Campaign_049_MOVE_BATHYSPHERE_CONFIRMATION_DESC", actor, list, MoveBathysphereCallback, 1, animateActor: false, null, canSkip: true, canUndo: true, recalculateValidTilesOnSelect: false, preSelectValidTiles: false, "UseItem", areaEffect, CAbilityFilter.EFilterTile.Custom, ignoreLos: true, ignoreDoorLocks: false, stackAsSeparateAbilities: true, autoTrigger: false, miscVars, interruptCurrentAbility: true);
			}
		}
	}

	private void MoveBathysphereCallback(CAbilityNullHex ability, List<CTile> selectedTiles)
	{
		ToggleBathysphereHighlights(active: false);
		CTile propTile = bathySphere.PropTile;
		CVector3 rotation = bathySphere.Rotation;
		_ = bathySphere.PropCustomLocKey;
		bathySphere.SetIsBlocker(isBlocker: false);
		propTile.DelProp(bathySphere, recursiveTiles: true);
		CTile cTile = selectedTiles[0];
		CAbilityCreate.CalcRotationAndBlockers(selectedTiles, ability.AreaEffect, ability.AreaEffectAngle, ability.AreaEffectFlipped, out var rotation2, out var pathingBlockers);
		bathySphere.SetLocation(new TileIndex(cTile.m_ArrayIndex), null, rotation2);
		bathySphere.PathingBlockers.Clear();
		bathySphere.PathingBlockers.AddRange(pathingBlockers);
		bathySphere.StartingMapGuid = cTile.m_HexMap.MapGuid;
		bathySphere.SetIsBlocker(isBlocker: true);
		bathySphere.VisualsForceLookAt = ability.TargetingActor.ArrayIndex;
		cTile.AddProp(bathySphere);
		List<CObjectProp> list = new List<CObjectProp>();
		foreach (CTile tile in selectedTiles)
		{
			foreach (CObjectProp item in ScenarioManager.CurrentScenarioState.Props.Where((CObjectProp x) => x.GetTiles().Contains(tile.m_ArrayIndex)))
			{
				if (item != bathySphere && (item is CObjectObstacle || item is CObjectTrap || item is CObjectDifficultTerrain || item is CObjectHazardousTerrain || item is CObjectIceTerrain))
				{
					list.Add(item);
				}
			}
		}
		foreach (CObjectProp item2 in list)
		{
			item2.DestroyProp();
		}
		CMoveProp_MessageData message = new CMoveProp_MessageData
		{
			m_MoveProp = bathySphere,
			m_MoveToTile = cTile,
			m_MoveSpeed = 1f,
			m_MoveFromTile = propTile,
			m_MoveFromRotation = rotation,
			m_MoveToRotation = rotation2
		};
		ScenarioRuleClient.MessageHandler(message);
		ToggleBathysphereHighlights(active: true);
	}

	private void CheckRestoreOxygen(CActor actor)
	{
		if (GetAirPocketTiles().Contains(actor.ArrayIndex) && !actorGuidsRestoredOxygenThisRound.Contains(actor.ActorGuid))
		{
			CActiveBonus cActiveBonus = FindOxygenBonus(actor);
			if (cActiveBonus != null && cActiveBonus.TrackerIndex > 0)
			{
				cActiveBonus.TrackerIndex = 0;
				actorGuidsRestoredOxygenThisRound.Add(actor.ActorGuid);
				UpdateOxygenCounter(actor as CPlayerActor);
				CCombatLogMessage_MessageData message = new CCombatLogMessage_MessageData
				{
					m_Actors = new List<CActor> { actor },
					m_Params = new List<string> { "Actor_0" },
					m_MessageKey = "FH_Scenario_Campaign_049_RESTORED_OXYGEN"
				};
				ScenarioRuleClient.MessageHandler(message);
			}
		}
	}

	private bool IsPointNeighborToBathysphere(Point point)
	{
		if (bathySphere == null || bathySphere.IsDestroyed)
		{
			return false;
		}
		foreach (Point item in bathySphere.PathingBlockers.Select((TileIndex x) => x.ToPoint()))
		{
			if (Point.HexDist(item, point) <= 1)
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateObjectives()
	{
		_scenarioObjectiveBathysphereEscape.Current = ((!bathySphere.IsDestroyed && !ScenarioManager.Scenario.PlayerActors.Any((CPlayerActor x) => !x.IsDead)) ? 1 : 0);
		_scenarioObjectiveBathysphereEscape.Total = 1;
		_scenarioObjectiveBathysphereEscape.ScriptedIsComplete = _scenarioObjectiveBathysphereEscape.Current == _scenarioObjectiveBathysphereEscape.Total;
		CheckObjectivesComplete();
	}

	public override bool TreatTileAsNonFeatureless(CTile tile)
	{
		if (GetAirPocketTiles().Contains(tile.m_ArrayIndex))
		{
			return true;
		}
		return false;
	}

	private void ToggleBathysphereHighlights(bool active)
	{
		if (bathySphere != null)
		{
			if (bathySphere.IsDestroyed)
			{
				active = false;
			}
			ScenarioManager.ToggleTileHighlights(bathySphere.GetTiles(), active, new CVector4(1f, 1f, 0f, 1f));
		}
	}

	private void UpdateOxygenCounter(CPlayerActor actor)
	{
		if (actor == null)
		{
			return;
		}
		OxygenInfo oxygenInfo = OxygenBonusesCache.SingleOrDefault((OxygenInfo x) => x.Actor == actor);
		if (oxygenInfo == null)
		{
			return;
		}
		if (actor.IsDead)
		{
			if (oxygenInfo.Prop != null)
			{
				oxygenInfo.Prop.DestroyProp();
				oxygenInfo.Prop = null;
			}
		}
		else
		{
			CChangePropVisualState_MessageData message = new CChangePropVisualState_MessageData
			{
				m_prop = oxygenInfo.Prop,
				m_stateName = "State_" + oxygenInfo.oxygenbonus.Remaining
			};
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	public static void LogRemainingOxygen(CActor actor)
	{
		ScenarioScript_FH049 obj = ScenarioScript.current as ScenarioScript_FH049;
		CPlayerActor cPlayerActor = actor as CPlayerActor;
		obj.UpdateOxygenCounter(cPlayerActor);
		int remaining = obj.FindOxygenBonus(cPlayerActor).Remaining;
		if (remaining <= 3)
		{
			CCombatLogMessage_MessageData message = new CCombatLogMessage_MessageData
			{
				m_Actors = new List<CActor> { cPlayerActor },
				m_Params = new List<string>
				{
					"Actor_0",
					remaining.ToString()
				},
				m_MessageKey = "FH_Scenario_Campaign_049_REMAINING_OXYGEN"
			};
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	protected override void TickDeathInternal()
	{
		base.TickDeathInternal();
		UpdateOxygenCounter(_tickActor as CPlayerActor);
	}
}
