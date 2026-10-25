using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH083 : BossHasMonsterCardExtraTurnScenarioScript
{
	[Serializable]
	public class Storage83
	{
		public int _waterLevel;

		public bool _barrelDestroyed;

		public string BossHasMonsterCardExtraTurnScenarioScriptSerialization;
	}

	private List<Point> _newWaterCoordinates = new List<Point>
	{
		new Point(6, 8),
		new Point(5, 7),
		new Point(5, 6),
		new Point(4, 5),
		new Point(4, 4),
		new Point(3, 3),
		new Point(3, 2)
	};

	private CObjective_Scripted _loseObjective;

	private Storage83 serializedProperties;

	protected override string BossClassID => "GiantPiranhaPigID";

	protected override string BossCardsClassID => "GiantPiranhaPigBossCardsID";

	protected override string MonsterClassID => "PiranhaPigID";

	protected override string MonsterEliteClassID => "PiranhaPigEliteID";

	private int _waterLevel
	{
		get
		{
			return serializedProperties._waterLevel;
		}
		set
		{
			serializedProperties._waterLevel = value;
		}
	}

	private bool _barrelDestroyed
	{
		get
		{
			return serializedProperties._barrelDestroyed;
		}
		set
		{
			serializedProperties._barrelDestroyed = value;
		}
	}

	protected override bool Initialize()
	{
		serializedProperties = new Storage83();
		_loseObjective = (CObjective_Scripted)GetObjective("LoseObjective");
		return base.Initialize();
	}

	protected override string SerializeInternal()
	{
		serializedProperties.BossHasMonsterCardExtraTurnScenarioScriptSerialization = base.SerializeInternal();
		return JsonConvert.SerializeObject(serializedProperties);
	}

	protected override void DeserializeInternal(string storage)
	{
		if (string.IsNullOrEmpty(storage))
		{
			return;
		}
		try
		{
			serializedProperties = (Storage83)JsonConvert.DeserializeObject(storage, typeof(Storage83));
			base.DeserializeInternal(serializedProperties.BossHasMonsterCardExtraTurnScenarioScriptSerialization);
		}
		catch (Exception)
		{
		}
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartTurn:
			if (_tickActor is CPlayerActor cPlayerActor)
			{
				CActor boss = FindBoss();
				if (cPlayerActor.Tile.m_Props.Find((CObjectProp prop) => prop is CObjectDifficultTerrain && prop.ObjectType == ScenarioManager.ObjectImportType.TerrainWater && prop.Owner == boss) != null)
				{
					GameState.ActorBeenDamaged(cPlayerActor, ScenarioManager.HazardousTerrainDamage, checkIfPlayerCanAvoidDamage: true);
				}
			}
			break;
		case EScenarioModifierTriggerPhase.EndTurn:
			TickPhaseEndTurn();
			_loseObjective.ScriptedIsComplete = _barrelDestroyed;
			_loseObjective.CheckObjectiveComplete(ScenarioScript.PartySize);
			CheckObjectivesComplete();
			break;
		case EScenarioModifierTriggerPhase.StartRound:
			TickPhaseStartRound();
			break;
		case EScenarioModifierTriggerPhase.StartScenario:
		{
			CActor cActor = FindBoss();
			{
				foreach (CObjectProp item in ScenarioManager.CurrentScenarioState.Props.Where((CObjectProp x) => x.PropType == EPropType.TerrainWater))
				{
					item.OwnerGUID = cActor.ActorGuid;
				}
				break;
			}
		}
		case EScenarioModifierTriggerPhase.AfterSelectMonsterClassAbilityCards:
			TickAfterSelectMonsterClassAbilityCards();
			break;
		case EScenarioModifierTriggerPhase.AfterAbility:
			if (_tickActor.Class.ID == "GiantPiranhaPigID")
			{
				if ((_tickActor.LastAbilityPerformed.Flags & CAbility.EAbilityFlag.Special2) != 0)
				{
					MoveTerrain();
				}
				else if ((_tickActor.LastAbilityPerformed.Flags & CAbility.EAbilityFlag.Special1) != 0)
				{
					MoveTerrain();
					MoveTerrain();
				}
			}
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 142);
			break;
		case EScenarioModifierTriggerPhase.EndRound:
		case EScenarioModifierTriggerPhase.OnRoomRevealed:
			break;
		}
	}

	private CActor FindBoss()
	{
		return ScenarioManager.Scenario.Enemies.FirstOrDefault((CEnemyActor x) => x.Class.ID == BossClassID);
	}

	private void MoveTerrain()
	{
		if (_waterLevel > 10)
		{
			_barrelDestroyed = true;
			return;
		}
		CActor targetingActor = FindBoss();
		foreach (Point newWaterCoordinate in _newWaterCoordinates)
		{
			CTile cTile = ScenarioManager.Tiles[newWaterCoordinate.X + _waterLevel, newWaterCoordinate.Y];
			if (cTile.m_Props.Count > 0)
			{
				foreach (CObjectProp item in cTile.m_Props.ToList())
				{
					if (item.ObjectType != ScenarioManager.ObjectImportType.Shadow && item.PropType != EPropType.LootToken)
					{
						item.DestroyProp();
					}
				}
			}
			List<CActor> list = cTile.LivingActors.ToList();
			if (list.Count > 0)
			{
				CActor cActor = list.Find((CActor actor) => actor.Class.ID == "AncientArtilleryID" || actor.Class.ID == "AncientArtilleryEliteID");
				if (cActor != null)
				{
					GameState.KillActor(_tickActor, cActor, CActor.ECauseOfDeath.HazardousTerrain, out var _);
				}
			}
			ScenarioRuleClient.MessageHandler(new CTryPlayVfxOnTile_MessageData(null)
			{
				m_VFXPropType = "PiranhaPigTerrainSpawnVFX",
				m_ArrayIndex = cTile.m_ArrayIndex
			});
			CAbilityCreateTerrain.SpawnTerrain(ScenarioManager.ObjectImportType.TerrainWater, cTile, null, targetingActor);
		}
		_waterLevel++;
		if (_waterLevel > 10)
		{
			_barrelDestroyed = true;
		}
	}

	protected override void TickRepositionInternal()
	{
		if (_tickActor.Class.ID == "AncientArtilleryID" || _tickActor.Class.ID == "AncientArtilleryEliteID")
		{
			CActor boss = FindBoss();
			if (_tickActor.Tile.m_Props.Count > 0 && _tickActor.Tile.m_Props.Find((CObjectProp prop) => prop.ObjectType == ScenarioManager.ObjectImportType.TerrainWater && prop.Owner == boss) != null)
			{
				GameState.KillActor(_tickActor, _tickActor, CActor.ECauseOfDeath.HazardousTerrain, out var _);
			}
		}
	}
}
