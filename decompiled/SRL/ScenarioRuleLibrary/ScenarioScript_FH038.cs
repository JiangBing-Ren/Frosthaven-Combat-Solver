using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;
using ScenarioRuleLibrary.CustomLevels;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH038 : ScenarioScript
{
	[Serializable]
	public class Storage38
	{
		public Point _currentTeleportTile;
	}

	private List<CEnemyActor> _emissaries;

	private List<Point> _altarTiles;

	private CMap _spiritRealm1;

	private CMap _spiritRealm2;

	private CScenarioModifier _mirrorDamageModifier;

	private CScenarioModifier _toggleActiveBonusModifier;

	private CScenarioModifier _changeModelModifier;

	private CScenarioModifier _preventDamageModifier;

	private CObjective _spiritRealmObjective;

	private CObjective _mainWinObjective;

	private List<(Point, Point)> _teleportLinks;

	[StateDump(Skip = true)]
	private int skipInteractChecks;

	private Storage38 serializedProperties;

	private bool _isSpiritRealmVisible
	{
		get
		{
			if (_spiritRealm1 != null)
			{
				return _spiritRealm1.Revealed;
			}
			return false;
		}
	}

	private CTile _currentTeleportTile
	{
		get
		{
			if (serializedProperties._currentTeleportTile == default(Point))
			{
				return null;
			}
			return ScenarioManager.GetTile(serializedProperties._currentTeleportTile);
		}
		set
		{
			serializedProperties._currentTeleportTile = value?.m_ArrayIndex ?? default(Point);
		}
	}

	protected override string SerializeInternal()
	{
		return JsonConvert.SerializeObject(serializedProperties);
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		_currentTeleportTile = null;
		if (!string.IsNullOrEmpty(storage))
		{
			serializedProperties = (Storage38)JsonConvert.DeserializeObject(storage, typeof(Storage38));
		}
		if (_isSpiritRealmVisible)
		{
			if (_currentTeleportTile != null)
			{
				UpdateTeleportLinks(_currentTeleportTile);
			}
			else
			{
				UpdateTeleportLinks();
			}
		}
		foreach (CEnemyActor emissary in _emissaries)
		{
			CActor mirrorActor = emissary.GetMirrorActor(includeMirroredClone: true);
			if (mirrorActor != null)
			{
				if (mirrorActor.FindApplicableActiveBonuses(CAbility.EAbilityType.PreventDamage).Count == 0)
				{
					ApplyScenarioActiveBonusToTickActor(_preventDamageModifier, mirrorActor, mirrorActor);
				}
				AddMindControlImmunity(mirrorActor);
			}
		}
		ScenarioManager.ToggleTileHighlights(_altarTiles, enabled: true);
	}

	protected override bool AllowTriggerMessageInternal(SEvent ev, CLevelMessage message)
	{
		if (ev.Type != ESEType.Ability)
		{
			return false;
		}
		Trace("Checking message trigger for " + message.MessageName + " " + message.TitleKey + "...", null, 76);
		if (!_isSpiritRealmVisible && _emissaries.All((CEnemyActor emissary) => _altarTiles.Contains(emissary.ArrayIndex)) && message.MessageName.Contains("FS038_1"))
		{
			Trace("...success", null, 80);
			return true;
		}
		Trace("...failed", null, 84);
		return false;
	}

	protected override bool Initialize()
	{
		serializedProperties = new Storage38();
		_emissaries = ScenarioManager.Scenario.AllAllyMonsters.Where((CEnemyActor monster) => monster.MonsterClass.ID == "EmissaryID").ToList();
		foreach (CEnemyActor emissary in _emissaries)
		{
			if (emissary.ChosenModelIndex == 0)
			{
				emissary.CustomLocKey = "FH/Barduu";
			}
			else
			{
				emissary.CustomLocKey = "FH/Grundel";
			}
		}
		_altarTiles = (from tile in ScenarioManager.GetAllAdjacentTiles(18, 16)
			select tile.m_ArrayIndex).ToList();
		_spiritRealm1 = ScenarioManager.CurrentScenarioState.Maps.Find((CMap map) => map.RoomName == "SpiritRealm1");
		_spiritRealm2 = ScenarioManager.CurrentScenarioState.Maps.Find((CMap map) => map.RoomName == "SpiritRealm2");
		_toggleActiveBonusModifier = GetScenarioModifier("TransferToSpiritPlane");
		_mirrorDamageModifier = GetScenarioModifier("MirrorDamage");
		_changeModelModifier = GetScenarioModifier("ChangeModel");
		_preventDamageModifier = GetScenarioModifier("PreventDamage");
		_mainWinObjective = GetObjective("AdjacentToOrb");
		_teleportLinks = new List<(Point, Point)>();
		_currentTeleportTile = null;
		return true;
	}

	protected override void TickPhaseInternal()
	{
		Log.SimpleInfo("TickPhaseInternal");
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartRound:
			TickPhaseStartRound();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 144);
			break;
		case EScenarioModifierTriggerPhase.EndRound:
		case EScenarioModifierTriggerPhase.StartTurn:
		case EScenarioModifierTriggerPhase.EndTurn:
		case EScenarioModifierTriggerPhase.StartScenario:
			break;
		}
	}

	protected void TickPhaseStartRound()
	{
		if (!(_tickActor.ActorGuid == _emissaries[0].ActorGuid))
		{
			return;
		}
		Log.Trace("Check if both emissaries are adjacent to altar");
		if (_isSpiritRealmVisible || !_emissaries.All((CEnemyActor emissary) => _altarTiles.Contains(emissary.ArrayIndex)))
		{
			return;
		}
		_changeModelModifier.SetHiddenState(shouldSetHidden: false);
		_toggleActiveBonusModifier.SetHiddenState(shouldSetHidden: false);
		_spiritRealm1.Reveal(initial: false);
		CRevealRooms_MessageData message = new CRevealRooms_MessageData();
		ScenarioRuleClient.MessageHandler(message);
		Log.Trace("Activate spawners on D and C");
		foreach (CSpawner spawner in ScenarioManager.CurrentScenarioState.Spawners)
		{
			spawner.SpawnerData.SpawnStartRound = ScenarioManager.CurrentScenarioState.RoundNumber + 1;
			spawner.SetActive(active: true, forceDontSpawn: true);
		}
		skipInteractChecks = 1000;
		TeleportActorToSpiritPlane(_emissaries[0], ScenarioManager.Tiles[12, 3]);
		ChangeEmissaryFocus(_emissaries[0]);
		TeleportActorToSpiritPlane(_emissaries[1], ScenarioManager.Tiles[13, 2]);
		ChangeEmissaryFocus(_emissaries[1]);
	}

	private void UpdateTeleportLinks()
	{
		if (GetSpiritRealmTeleportLocation(out var tile) && tile != _currentTeleportTile)
		{
			UpdateTeleportLinks(tile);
		}
	}

	private void UpdateTeleportLinks(CTile teleportTile)
	{
		_currentTeleportTile = teleportTile;
		if (_teleportLinks.Count > 0)
		{
			ScenarioManager.ToggleTileLinks(_teleportLinks, enabled: false);
			_teleportLinks.Clear();
		}
		foreach (Point altarTile in _altarTiles)
		{
			_teleportLinks.Add((altarTile, teleportTile.m_ArrayIndex));
		}
		ScenarioManager.ToggleTileLinks(_teleportLinks, enabled: true);
	}

	private void SpawnDamageRedirection(CActor actor, CTile tile)
	{
		CActor cActor = SpawnDamageRedirectionBase(actor, tile, copyBonuses: false);
		if (_emissaries.Contains(actor))
		{
			ApplyScenarioActiveBonusToTickActor(_preventDamageModifier, cActor, cActor);
		}
		ApplyScenarioActiveBonusToTickActor(_mirrorDamageModifier, actor, cActor);
		ApplyScenarioActiveBonusToTickActor(_mirrorDamageModifier, cActor, actor);
		AddMindControlImmunity(cActor);
		cActor.CheckForCachedValuesAfterActiveBonusesUpdate();
	}

	private void AddMindControlImmunity(CActor actor)
	{
		if (!actor.ActiveBonuses.Any((CActiveBonus x) => x.Ability.Name == "Scenario38MirrorEntityMindControlImmunity"))
		{
			CAbility cAbility = CAbilityImmunityTo.CreateDefaultImmunityTo(new List<CAbility.EAbilityType> { CAbility.EAbilityType.ControlActor }, new List<CAbility.EAttackType>(), immuneToSelfDamage: false);
			cAbility.ActiveBonusData.Hidden = true;
			cAbility.ActiveBonusData.Duration = CActiveBonus.EActiveBonusDurationType.Persistent;
			cAbility.Name = "Scenario38MirrorEntityMindControlImmunity";
			cAbility.Start(actor, actor);
			cAbility.ApplyToActor(actor);
		}
	}

	private void ChangeEmissaryFocus(CEnemyActor emissary)
	{
		CObjectProp cObjectProp = ScenarioManager.CurrentScenarioState.Props.Find((CObjectProp prop) => prop.ArrayIndex.X == 2 && prop.ArrayIndex.Y == 1);
		ScenarioManager.CurrentScenarioState.AllyMonsters.Find((EnemyState actor) => actor.ActorGuid == emissary.ActorGuid).SetAIFocusOverride(new CAIFocusOverrideDetails
		{
			OverrideTargetType = CAIFocusOverrideDetails.EOverrideTargetType.Prop,
			OverrideType = CAIFocusOverrideDetails.EOverrideType.OverrideFocus,
			TargetGUID = cObjectProp.PropGuid
		});
	}

	protected override void TickRepositionInternal()
	{
		_mainWinObjective.CheckObjectiveComplete(ScenarioManager.CurrentScenarioState.Players.Count);
	}

	private bool GetSpiritRealmTeleportLocation(out CTile tile)
	{
		return GetNearestFreeTile(13, 2, out tile);
	}

	public override Point GetActorIteractionTile(CActor actor)
	{
		return new Point(18, 16);
	}

	public override bool IsActorInteracting(CActor actor)
	{
		if (actor.Type != CActor.EType.Ally)
		{
			return false;
		}
		skipInteractChecks = ((skipInteractChecks > 0) ? (skipInteractChecks - 1) : 0);
		if (!_altarTiles.Contains(actor.ArrayIndex) && skipInteractChecks <= 0)
		{
			return false;
		}
		return true;
	}

	public static bool CanPlayerTeleportToSpiritPlane(CActor actor)
	{
		if (!(ScenarioScript.current is ScenarioScript_FH038 scenarioScript_FH))
		{
			return false;
		}
		if (!scenarioScript_FH._isSpiritRealmVisible)
		{
			return false;
		}
		return scenarioScript_FH._altarTiles.Contains(actor.RealArrayIndex);
	}

	public static void TeleportActorToSpiritPlane(CActor actor, CTile nearestTile = null)
	{
		if (!(ScenarioScript.current is ScenarioScript_FH038 scenarioScript_FH))
		{
			return;
		}
		Log.Trace("Transport player to spirit realm");
		if (nearestTile != null || scenarioScript_FH.GetSpiritRealmTeleportLocation(out nearestTile))
		{
			CTile tile = actor.Tile;
			actor.ArrayIndex = nearestTile.m_ArrayIndex;
			CActorHasTeleported_MessageData message = new CActorHasTeleported_MessageData(actor)
			{
				m_EndLocation = nearestTile.m_ArrayIndex,
				m_StartLocation = actor.ArrayIndex,
				m_ActorTeleported = actor,
				m_TeleportAbility = null
			};
			ScenarioRuleClient.MessageHandler(message);
			scenarioScript_FH.SpawnDamageRedirection(actor, tile);
			if (scenarioScript_FH._isSpiritRealmVisible)
			{
				scenarioScript_FH.UpdateTeleportLinks();
			}
		}
	}

	public override bool IsTeleportValid(CTile a, CTile b)
	{
		if (a.m_HexMap == _spiritRealm1 || a.m_HexMap == _spiritRealm2)
		{
			if (b.m_HexMap != _spiritRealm1)
			{
				return b.m_HexMap == _spiritRealm2;
			}
			return true;
		}
		return base.IsTeleportValid(a, b);
	}
}
