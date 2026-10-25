using System;
using System.Collections.Generic;
using System.Linq;
using AStar;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH110 : ScenarioScript
{
	private Dictionary<CObjectProp, Point[]> _coilAffectedHexes = new Dictionary<CObjectProp, Point[]>();

	private List<CObjectProp[]> _coilPairs = new List<CObjectProp[]>();

	private CObjectProp[] _activeCoilsPair;

	private const string _VFXPropName = "ElectricityRockColumnCoil";

	private CObjective_Scripted _destroyCoilsObjective;

	protected override bool Initialize()
	{
		GatherCoilHexes();
		UpdateActiveCoils();
		_destroyCoilsObjective = (CObjective_Scripted)GetObjective("Destroy Pillars");
		_destroyCoilsObjective.Total = _coilAffectedHexes.Count;
		return true;
	}

	private void GatherCoilHexes()
	{
		_coilAffectedHexes.Add(GetCoilProp(6, 7), new Point[6]
		{
			new Point(6, 6),
			new Point(5, 5),
			new Point(7, 6),
			new Point(7, 5),
			new Point(5, 7),
			new Point(4, 7)
		});
		_coilAffectedHexes.Add(GetCoilProp(8, 4), new Point[6]
		{
			new Point(7, 4),
			new Point(6, 4),
			new Point(7, 6),
			new Point(7, 5),
			new Point(7, 3),
			new Point(7, 2)
		});
		_coilAffectedHexes.Add(GetCoilProp(6, 1), new Point[6]
		{
			new Point(6, 2),
			new Point(5, 3),
			new Point(5, 1),
			new Point(4, 1),
			new Point(7, 3),
			new Point(7, 2)
		});
		_coilAffectedHexes.Add(GetCoilProp(3, 1), new Point[6]
		{
			new Point(4, 2),
			new Point(4, 3),
			new Point(5, 1),
			new Point(4, 1),
			new Point(3, 2),
			new Point(2, 3)
		});
		_coilAffectedHexes.Add(GetCoilProp(2, 4), new Point[6]
		{
			new Point(3, 4),
			new Point(4, 4),
			new Point(2, 5),
			new Point(3, 6),
			new Point(3, 2),
			new Point(2, 3)
		});
		_coilAffectedHexes.Add(GetCoilProp(3, 7), new Point[6]
		{
			new Point(4, 6),
			new Point(4, 5),
			new Point(2, 5),
			new Point(3, 6),
			new Point(4, 7),
			new Point(5, 7)
		});
		_coilPairs.Add(new CObjectProp[2]
		{
			GetCoilProp(6, 7),
			GetCoilProp(3, 1)
		});
		_coilPairs.Add(new CObjectProp[2]
		{
			GetCoilProp(2, 4),
			GetCoilProp(8, 4)
		});
		_coilPairs.Add(new CObjectProp[2]
		{
			GetCoilProp(3, 7),
			GetCoilProp(6, 1)
		});
		static CObjectProp GetCoilProp(int x, int y)
		{
			return ScenarioManager.CurrentScenarioState.Props.Concat(ScenarioManager.CurrentScenarioState.DestroyedProps).FirstOrDefault((CObjectProp p) => p.ArrayIndex.X == x && p.ArrayIndex.Y == y && p.SpecificPropType == ESpecificPropType.RockColumn);
		}
	}

	private bool IsRuinedMachine(CActor actor)
	{
		if (!actor.Class.ID.Equals("RuinedMachineID"))
		{
			return actor.Class.ID.Equals("RuinedMachineEliteID");
		}
		return true;
	}

	private bool IsElectrified(CTile tile)
	{
		CObjectProp[] activeCoilsPair = _activeCoilsPair;
		foreach (CObjectProp cObjectProp in activeCoilsPair)
		{
			if (!cObjectProp.IsDestroyed && _coilAffectedHexes[cObjectProp].Contains(tile.m_ArrayIndex))
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateActiveCoils()
	{
		if (_coilAffectedHexes.Keys.Count((CObjectProp p) => p.IsDestroyed) >= 4)
		{
			_activeCoilsPair = (from p in _coilPairs.SelectMany((CObjectProp[] pair) => pair)
				where !p.IsDestroyed
				select p).ToArray();
		}
		else
		{
			_activeCoilsPair = _coilPairs[(ScenarioScript.RoundNumber - 1) % 3];
		}
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		if (scenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.StartRound)
		{
			TickStartRound();
		}
		if (scenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.StartTurn)
		{
			TickStartTurn();
		}
	}

	private void TickStartRound()
	{
		if (!OncePerRound())
		{
			return;
		}
		UpdateActiveCoils();
		foreach (CObjectProp key in _coilAffectedHexes.Keys)
		{
			CObjectProp cObjectProp2 = key.PropTile.m_Props.FirstOrDefault((CObjectProp p) => p.PrefabName == "ElectricityRockColumnCoil");
			if (cObjectProp2 != null)
			{
				cObjectProp2.DestroyProp();
				Trace($"Destroyed prop {cObjectProp2.PrefabName} at {key.PropTile.m_ArrayIndex}", null, 105);
			}
		}
		CObjectProp[] activeCoilsPair = _activeCoilsPair;
		foreach (CObjectProp cObjectProp3 in activeCoilsPair)
		{
			if (!cObjectProp3.IsDestroyed)
			{
				CVector3 position = cObjectProp3.Position;
				CVector3 cVector = ScenarioManager.Tiles[5, 4].m_Hex.Position - position;
				float num2 = (float)(Math.Atan2(cVector.Z, cVector.X) * (180.0 / Math.PI));
				CVector3 rotation = new CVector3(0f, 0f - num2 + 90f, 0f);
				CObjectProp prop = new CObjectProp("ElectricityRockColumnCoil", ScenarioManager.ObjectImportType.VFXProp, cObjectProp3.ArrayIndex, null, rotation, null, cObjectProp3.PropTile.m_HexMap.MapGuid);
				cObjectProp3.PropTile.SpawnProp(prop);
			}
		}
	}

	private void TickStartTurn()
	{
		TryElectrifyActor(_tickActor);
		foreach (Point item in _coilAffectedHexes.Values.SelectMany((Point[] list) => list))
		{
			ScenarioManager.PathFinder.GetNode(item).IsNegativeHex = _tickActor is CHeroSummonActor;
		}
	}

	protected override void TickRepositionInternal()
	{
		TryElectrifyActor(_tickActor, _tickAbility as CAbilityMove);
	}

	protected override void TickDeathInternal()
	{
		if (!IsRuinedMachine(_tickActor))
		{
			return;
		}
		List<CTile> list = new List<CTile>();
		GameState.GetAdjacentTiles(_tickActor.ArrayIndex, list);
		CObjectProp cObjectProp = list.SelectMany((CTile t) => t.m_Props).FirstOrDefault((CObjectProp p) => _coilAffectedHexes.ContainsKey(p) && !p.IsDestroyed);
		if (cObjectProp == null)
		{
			return;
		}
		cObjectProp.DestroyProp();
		CObjectProp cObjectProp2 = cObjectProp.PropTile.m_Props.FirstOrDefault((CObjectProp p) => p.PrefabName == "ElectricityRockColumnCoil");
		if (cObjectProp2 != null)
		{
			cObjectProp2.DestroyProp();
			Trace($"Destroyed prop {cObjectProp2.PrefabName} at {cObjectProp.PropTile.m_ArrayIndex}", null, 161);
		}
		_destroyCoilsObjective.Current = _coilAffectedHexes.Keys.Count((CObjectProp c) => c.IsDestroyed);
		if (_destroyCoilsObjective.Current == 4)
		{
			CObjectDoor cObjectDoor = (CObjectDoor)ScenarioManager.CurrentScenarioState.DoorProps.First();
			if (!cObjectDoor.DoorIsOpen)
			{
				cObjectDoor.Activate(_tickActor);
			}
		}
		if (_destroyCoilsObjective.Current == _coilAffectedHexes.Count)
		{
			_destroyCoilsObjective.ScriptedIsComplete = true;
		}
		UpdateActiveCoils();
	}

	private void TryElectrifyActor(CActor actor, CAbilityMove moveAbility = null)
	{
		if (!IsElectrified(actor.Tile))
		{
			return;
		}
		if (actor is CPlayerActor || actor is CHeroSummonActor)
		{
			if (actor.FlyingAfterConditions || (moveAbility != null && (moveAbility.Jump || moveAbility.Fly)))
			{
				return;
			}
			DealScenarioDamage(actor, ScenarioManager.HazardousTerrainDamage, isTrapDamage: false, isTerrainDamage: true);
		}
		if (actor is CEnemyActor && IsRuinedMachine(actor))
		{
			ScenarioScript.HealTo(actor, actor.Health + 1, removeAllTokens: true);
		}
	}
}
