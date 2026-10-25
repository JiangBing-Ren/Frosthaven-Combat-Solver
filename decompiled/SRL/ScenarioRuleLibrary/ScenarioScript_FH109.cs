using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AStar;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH109 : ScenarioScript
{
	public class LaserInfo
	{
		public CObjectActor laserSpire;

		public bool laserActive;

		public bool startedRotating;

		public bool finishedRotating;
	}

	private const string _VFXPropName = "LaserSpireBeam";

	private Dictionary<Point, int> _initialLaserRotations = new Dictionary<Point, int>
	{
		{
			new Point(11, 5),
			1
		},
		{
			new Point(7, 3),
			1
		},
		{
			new Point(6, 17),
			2
		},
		{
			new Point(10, 19),
			4
		},
		{
			new Point(4, 6),
			1
		},
		{
			new Point(14, 16),
			5
		}
	};

	private Dictionary<CObjectActor, int> _laserRotations = new Dictionary<CObjectActor, int>();

	private List<CObjectActor> _startedLasers = new List<CObjectActor>();

	[StateDump(Skip = true)]
	public Dictionary<CObjectActor, LaserInfo> LaserInfos = new Dictionary<CObjectActor, LaserInfo>();

	public override bool ShouldSortModifierTextsInOrderOfModifiers()
	{
		return true;
	}

	private void GatherLasers()
	{
		foreach (CObjectActor item in ScenarioManager.Scenario.Objects.Where((CObjectActor x) => x.Class.ID == "LaserSpireID"))
		{
			if (!LaserInfos.TryGetValue(item, out var value))
			{
				value = new LaserInfo
				{
					laserSpire = item,
					laserActive = _startedLasers.Contains(item),
					startedRotating = false,
					finishedRotating = false
				};
				LaserInfos[item] = value;
			}
		}
	}

	protected override bool Initialize()
	{
		return true;
	}

	protected override string SerializeInternal()
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		foreach (KeyValuePair<CObjectActor, int> laserRotation in _laserRotations)
		{
			dictionary.Add(laserRotation.Key.ActorGuid, laserRotation.Value);
		}
		bool flag = _startedLasers.Any();
		string text = JsonConvert.SerializeObject(dictionary);
		string text2 = JsonConvert.SerializeObject(flag);
		string text3 = JsonConvert.SerializeObject(new string[2] { text, text2 });
		Trace("[ffa000]Serialize: " + text3 + "[-]", null, 72);
		return text3;
	}

	protected override void DeserializeInternal(string s)
	{
		Trace("[ffa000]Deserialize " + s + "[-]", null, 78);
		if (string.IsNullOrEmpty(s))
		{
			return;
		}
		string[]? array = JsonConvert.DeserializeObject<string[]>(s);
		Dictionary<string, int> dictionary = JsonConvert.DeserializeObject<Dictionary<string, int>>(array[0]);
		bool flag = JsonConvert.DeserializeObject<bool>(array[1]);
		foreach (KeyValuePair<string, int> item in dictionary)
		{
			CObjectActor key = (CObjectActor)ScenarioManager.Scenario.FindActor(item.Key);
			_laserRotations.Add(key, item.Value);
		}
		if (flag)
		{
			_startedLasers.AddRange(_laserRotations.Keys.Where((CObjectActor x) => x.PlayedThisRound));
			_startedLasers.Sort((CObjectActor l, CObjectActor r) => l.ActorGuid.CompareTo(r.ActorGuid));
			foreach (CObjectActor startedLaser in _startedLasers)
			{
				foreach (CTile item2 in GetTilesAffectedByLaser(startedLaser))
				{
					ScenarioManager.PathFinder.GetNode(item2.m_ArrayIndex).IsSuperNegativeHex = true;
				}
			}
		}
		foreach (CObjectActor @object in ScenarioManager.Scenario.Objects)
		{
			if (@object.Class.ID == "LaserSpireID")
			{
				CSetActiveActorHealthBar_MessageData message = new CSetActiveActorHealthBar_MessageData(@object, active: false);
				ScenarioRuleClient.MessageHandler(message);
			}
		}
		GatherLasers();
	}

	private void InitLaserSpire(CObjectActor laserSpire, int rotation)
	{
		CSetActiveActorHealthBar_MessageData message = new CSetActiveActorHealthBar_MessageData(laserSpire, active: false);
		ScenarioRuleClient.MessageHandler(message);
		_laserRotations[laserSpire] = rotation;
		TryRotateLaser(laserSpire, rotation, initial: true);
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		if (scenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.OnActorSpawnedOrRevealed)
		{
			TickActorSpawnedOrRevealed();
		}
		if (scenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.EndRound)
		{
			StopAllLasers();
		}
	}

	private void TickActorSpawnedOrRevealed()
	{
		int value;
		if (_tickActor.Class.ID.StartsWith("SteelAutomaton"))
		{
			_tickActor.OverrideCharacterModel = "CentralProcessor";
			_tickActor.CustomLocKey = "FH_Scenario_Campaign_109_SCENARIO_OBJECT_4";
		}
		else if (_tickActor is CObjectActor cObjectActor && !_laserRotations.ContainsKey(cObjectActor) && _initialLaserRotations.TryGetValue(cObjectActor.ArrayIndex, out value))
		{
			CRevealRooms_MessageData message = new CRevealRooms_MessageData();
			ScenarioRuleClient.MessageHandler(message);
			InitLaserSpire(cObjectActor, value);
		}
	}

	protected override void TickAbilityEndedInternal()
	{
		if (!(_tickAbility is CAbilitySummon cAbilitySummon))
		{
			return;
		}
		foreach (CActor summonedActor in cAbilitySummon.SummonedActors)
		{
			TryDamageByAllLasers(summonedActor);
		}
	}

	protected override void OnDoorOpenedInternal(CObjectDoor door, CActor actor, CActor creditActor, bool visualOnly)
	{
		foreach (CActor item in ScenarioManager.Scenario.AllAliveActors.Where((CActor a) => !(a is CObjectActor)))
		{
			if (actor != item)
			{
				TryDamageByAllLasers(item, door);
			}
		}
		CObjectProp[] props = ScenarioManager.CurrentScenarioState.Props.Where((CObjectProp p) => p.PrefabName == "LaserSpireBeam").ToArray();
		CRefreshProps_VFX_LOS_MessageData message = new CRefreshProps_VFX_LOS_MessageData
		{
			m_Props = props
		};
		ScenarioRuleClient.MessageHandler(message);
	}

	protected override void TickBeingAttackedInternal(CAbilityAttack ability, int modifiedStrength)
	{
		CObjectActor laserSpire = _tickActor as CObjectActor;
		if (laserSpire == null || !_laserRotations.ContainsKey(laserSpire))
		{
			return;
		}
		Point point = Point.OddRToAxial(laserSpire.ArrayIndex);
		Point point2 = Point.OddRToAxial(ability.TargetingActor.ArrayIndex);
		CVector3 cVector = new CVector3(point.X, point.Y, 0f);
		CVector3 cVector2 = new CVector3(point2.X, point2.Y, 0f);
		int laserRot = _laserRotations[laserSpire];
		CVector3 normalized = (cVector - cVector2).Normalized;
		Point point3 = Point.RotationToAxialDirection(laserRot * 60);
		CVector3 normalized2 = new CVector3(point3.X, point3.Y, 0f).Normalized;
		CVector3 b = new CVector3(normalized2.Y, 0f - normalized2.X, 0f);
		float num = CVector3.Dot(normalized, b);
		if (Math.Abs(num) < 0.0001f)
		{
			if (CVector3.Dot(normalized2, normalized) > 0.9f)
			{
				return;
			}
			List<CTile> validTiles = new List<CTile>
			{
				ScenarioManager.GetTile(GetBarrelPoint(laserSpire, WrapValue(laserRot + 1, 6))),
				ScenarioManager.GetTile(GetBarrelPoint(laserSpire, WrapValue(laserRot - 1, 6)))
			}.Where((CTile t) => t.IsEmpty).ToList();
			if (validTiles.Count == 2)
			{
				ScenarioScript.PickHexes("FH_Scenario_Campaign_109_SCENARIO_MODIFIER_7", ability.TargetingActor, validTiles, (CAbilityNullHex _, List<CTile> selected) =>
				{
					TryRotateLaser(laserSpire, (selected[0] == validTiles[0]) ? WrapValue(laserRot + 1, 6) : WrapValue(laserRot - 1, 6));
				}, 1, animateActor: false, null, canSkip: false);
			}
			else if (validTiles.Count == 1)
			{
				bool flag = validTiles[0].m_ArrayIndex == GetBarrelPoint(laserSpire, WrapValue(laserRot + 1, 6));
				TryRotateLaser(laserSpire, flag ? WrapValue(laserRot + 1, 6) : WrapValue(laserRot - 1, 6));
			}
		}
		else if (num > 0f)
		{
			TryAddRotation(laserSpire, 1);
		}
		else
		{
			TryAddRotation(laserSpire, -1);
		}
	}

	public void TryAddRotation(CObjectActor laserSpire, int rotation)
	{
		TryRotateLaser(laserSpire, WrapValue(_laserRotations[laserSpire] + rotation, 6));
	}

	private int WrapValue(int value, int size)
	{
		for (value %= size; value < 0; value += size)
		{
		}
		return value;
	}

	private bool TryRotateLaser(CObjectActor laserSpire, int nextRotation, bool initial = false)
	{
		Point barrelPoint = GetBarrelPoint(laserSpire, nextRotation);
		if (laserSpire.Tile.m_HexMap.Revealed && !ScenarioManager.Tiles[barrelPoint.X, barrelPoint.Y].IsEmpty)
		{
			return false;
		}
		bool flag = _startedLasers.Contains(laserSpire);
		if (flag)
		{
			StopLaser(laserSpire);
		}
		if (!LaserInfos.TryGetValue(laserSpire, out var value))
		{
			value = new LaserInfo
			{
				laserSpire = laserSpire
			};
			LaserInfos[laserSpire] = value;
		}
		value.laserActive = flag;
		value.startedRotating = !initial;
		value.finishedRotating = false;
		CObjectObstacle cObjectObstacle = (CObjectObstacle)laserSpire.AttachedProp;
		cObjectObstacle.SetIsBlocker(isBlocker: false);
		cObjectObstacle.PathingBlockers.RemoveAll((TileIndex ti) => ti.X != laserSpire.ArrayIndex.X || ti.Y != laserSpire.ArrayIndex.Y);
		cObjectObstacle.PathingBlockers.Add(new TileIndex(barrelPoint));
		cObjectObstacle.SetIsBlocker(isBlocker: true);
		_laserRotations[laserSpire] = nextRotation;
		if (ScenarioRuleClient.IsWorkerThread && value.startedRotating)
		{
			GameState.ThreadIsSleeping = true;
			while (!value.finishedRotating)
			{
				Thread.Sleep(100);
			}
			GameState.ThreadIsSleeping = false;
		}
		if (flag)
		{
			StartLaser(laserSpire);
		}
		return true;
	}

	private static Point GetBarrelPoint(CObjectActor laserSpire, int rotationIndex)
	{
		return Point.AxialToOddR(Point.OddRToAxial(laserSpire.ArrayIndex) + Point.RotationToAxialDirection(rotationIndex * 60));
	}

	public override Point GetActorIteractionTile(CActor actor)
	{
		if (!(actor is CObjectActor cObjectActor) || !_laserRotations.TryGetValue(cObjectActor, out var value))
		{
			return default;
		}
		return GetBarrelPoint(cObjectActor, value);
	}

	public void StartLaser(CObjectActor laserSpire)
	{
		if (_startedLasers.Contains(laserSpire))
		{
			return;
		}
		int num = _laserRotations[laserSpire];
		Point barrelPoint = GetBarrelPoint(laserSpire, num);
		CVector3 rotation = new CVector3(0f, num * 60 - 90, 0f);
		CObjectProp prop = new CObjectProp("LaserSpireBeam", ScenarioManager.ObjectImportType.VFXProp, new TileIndex(barrelPoint), null, rotation, null, laserSpire.Tile.m_HexMap.MapGuid);
		ScenarioManager.GetTile(barrelPoint).SpawnProp(prop);
		_startedLasers.Add(laserSpire);
		_startedLasers.Sort((CObjectActor l, CObjectActor r) => l.ActorGuid.CompareTo(r.ActorGuid));
		if (!LaserInfos.TryGetValue(laserSpire, out var value))
		{
			value = new LaserInfo
			{
				laserSpire = laserSpire
			};
			LaserInfos[laserSpire] = value;
		}
		value.laserActive = true;
		foreach (CTile item in GetTilesAffectedByLaser(laserSpire))
		{
			ScenarioManager.PathFinder.GetNode(item.m_ArrayIndex).IsSuperNegativeHex = true;
		}
		foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
		{
			if (!(allAliveActor is CObjectActor) && IsAffectedByLaser(laserSpire, allAliveActor.Tile))
			{
				DealScenarioDamage(allAliveActor, ScenarioManager.HazardousTerrainDamage, isTrapDamage: false, isTerrainDamage: true);
			}
		}
	}

	public void StopLaser(CObjectActor laserSpire)
	{
		if (!_startedLasers.Contains(laserSpire))
		{
			return;
		}
		foreach (CTile item in GetTilesAffectedByLaser(laserSpire))
		{
			ScenarioManager.PathFinder.GetNode(item.m_ArrayIndex).IsSuperNegativeHex = false;
		}
		Point barrelPoint = GetBarrelPoint(laserSpire, _laserRotations[laserSpire]);
		CObjectProp prop = GetProp(barrelPoint.X, barrelPoint.Y, "LaserSpireBeam");
		if (prop != null)
		{
			prop.DestroyProp();
			Trace($"Destroyed prop {prop.PrefabName} at {prop.PropTile.m_ArrayIndex}", null, 380);
		}
		_startedLasers.Remove(laserSpire);
		if (!LaserInfos.TryGetValue(laserSpire, out var value))
		{
			value = new LaserInfo
			{
				laserSpire = laserSpire
			};
			LaserInfos[laserSpire] = value;
		}
		value.laserActive = false;
	}

	private void StopAllLasers()
	{
		if (!OncePerRound())
		{
			return;
		}
		foreach (CObjectActor item in _startedLasers.ToList())
		{
			StopLaser(item);
		}
	}

	protected override void TickRepositionInternal()
	{
		TryDamageByAllLasers(_tickActor);
	}

	private void TryDamageByAllLasers(CActor actor, CObjectDoor affectsBeyondDoor = null)
	{
		int num = _startedLasers.Count((CObjectActor l) => IsAffectedByLaser(l, actor.Tile, affectsBeyondDoor));
		if (num > 0)
		{
			DealScenarioDamage(actor, ScenarioManager.HazardousTerrainDamage * num, isTrapDamage: false, isTerrainDamage: true);
		}
	}

	private bool IsAffectedByLaser(CObjectActor laserSpire, CTile tile, CObjectDoor affectsBeyondDoor = null)
	{
		if (!_startedLasers.Contains(laserSpire))
		{
			return false;
		}
		Point point = Point.RotationToAxialDirection(_laserRotations[laserSpire] * 60);
		Point point2 = Point.OddRToAxial(laserSpire.ArrayIndex);
		Point point3 = point2 + point;
		Point point4 = Point.OddRToAxial(tile.m_ArrayIndex);
		if (point3.Y != point2.Y)
		{
			float num = (float)(point3.X - point2.X) / (float)(point3.Y - point2.Y);
			float num2 = (float)(point4.X - point2.X) / (float)(point4.Y - point2.Y);
			if (Math.Abs(num - num2) > 1E-06f)
			{
				return false;
			}
		}
		else if (point4.Y != point2.Y || Math.Sign(point4.X - point2.X) != Math.Sign(point3.X - point2.X))
		{
			return false;
		}
		CTile tile2 = laserSpire.Tile;
		while (tile2 != tile)
		{
			tile2 = ScenarioManager.GetTile(Point.AxialToOddR(Point.OddRToAxial(tile2.m_ArrayIndex) + point));
			if (tile2 == null)
			{
				return false;
			}
			if (affectsBeyondDoor != null && tile2 == affectsBeyondDoor.PropTile)
			{
				affectsBeyondDoor = null;
			}
			bool flag = tile2.FindDoor() != null && !tile2.FindDoor().DoorIsOpen;
			if (!tile2.IsWalkable | flag)
			{
				return false;
			}
		}
		return affectsBeyondDoor == null;
	}

	private IEnumerable<CTile> GetTilesAffectedByLaser(CObjectActor laserSpire)
	{
		Point dir = Point.RotationToAxialDirection(_laserRotations[laserSpire] * 60);
		CTile hitTile = laserSpire.Tile;
		while (true)
		{
			Point point = Point.AxialToOddR(Point.OddRToAxial(hitTile.m_ArrayIndex) + dir);
			hitTile = ScenarioManager.GetTile(point);
			if (hitTile == null)
			{
				break;
			}
			bool flag = hitTile.FindDoor() != null && !hitTile.FindDoor().DoorIsOpen;
			if (!hitTile.IsWalkable | flag)
			{
				break;
			}
			yield return hitTile;
		}
	}
}
