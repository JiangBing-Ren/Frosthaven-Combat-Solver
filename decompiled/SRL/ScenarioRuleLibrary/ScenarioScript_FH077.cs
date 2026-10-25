using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH077 : ScenarioScript
{
	public enum Stage
	{
		BossOverTreeQuartersHP,
		BossOverHalfHP,
		BossOverOneQuarterHP,
		BossDying
	}

	[Serializable]
	public class Storage77
	{
		public Stage currentStage;

		public bool BossImmune;
	}

	public readonly List<Point> bossAdjacentTiles = new List<Point>
	{
		new Point(5, 16),
		new Point(6, 16),
		new Point(6, 15),
		new Point(7, 14),
		new Point(6, 13),
		new Point(5, 13),
		new Point(4, 13),
		new Point(4, 14),
		new Point(4, 15)
	};

	public readonly List<Point> glowingOrbAdjacentTiles = new List<Point>
	{
		new Point(4, 5),
		new Point(5, 5),
		new Point(6, 4),
		new Point(5, 3),
		new Point(4, 3),
		new Point(4, 4)
	};

	private List<CSpawner> windDemonSpawners = new List<CSpawner>();

	private List<CSpawner> nightDemonSpawners = new List<CSpawner>();

	private List<CSpawner> frostDemonSpawners = new List<CSpawner>();

	private CScenarioModifier scenarioAbilityHolderAttackBuff1;

	private CScenarioModifier scenarioAbilityHolderAttackBuff2;

	private CScenarioModifier scenarioModifierRoomLOSText;

	private CScenarioModifier scenarioModifierPortalText;

	private CScenarioModifier scenarioModifierBossImmunityText;

	private CObjective scenarioObjectiveKillAllDemons;

	private CEnemyActor boss;

	private CMap spiritRealmRoom;

	private Storage77 serializedProperties;

	private Stage currentStage
	{
		get
		{
			return serializedProperties.currentStage;
		}
		set
		{
			serializedProperties.currentStage = value;
		}
	}

	protected override string SerializeInternal()
	{
		serializedProperties.BossImmune = boss?.IsImmuneToDamage ?? false;
		return JsonConvert.SerializeObject(serializedProperties);
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		if (!string.IsNullOrEmpty(storage))
		{
			serializedProperties = (Storage77)JsonConvert.DeserializeObject(storage, typeof(Storage77));
			RefreshNavLinks(null);
			if (boss != null)
			{
				boss.IsImmuneToDamage = serializedProperties.BossImmune;
			}
		}
		UpdateEnvironmentVFX();
	}

	protected override bool Initialize()
	{
		serializedProperties = new Storage77();
		boss = ScenarioManager.Scenario.AllEnemies.Where((CEnemyActor x) => x.MonsterClass.Boss).ToList()[0];
		boss.MultiHexArrayIndexes = new List<Point>
		{
			new Point(5, 15),
			new Point(6, 14),
			new Point(5, 14)
		};
		spiritRealmRoom = ScenarioManager.CurrentScenarioState.Maps.Find((CMap map) => map.RoomName == "Room_2");
		scenarioAbilityHolderAttackBuff1 = GetScenarioModifier("scenarioAbilityHolderAttackBuff1");
		scenarioAbilityHolderAttackBuff2 = GetScenarioModifier("scenarioAbilityHolderAttackBuff2");
		scenarioModifierRoomLOSText = GetScenarioModifier("scenarioModifierRoomLOSText");
		scenarioModifierPortalText = GetScenarioModifier("scenarioModifierPortalText");
		scenarioModifierBossImmunityText = GetScenarioModifier("scenarioModifierBossImmunityText");
		scenarioObjectiveKillAllDemons = GetObjective("KillAllDemons");
		windDemonSpawners.Add(ScenarioScript._css.Spawners[0]);
		windDemonSpawners.Add(ScenarioScript._css.Spawners[1]);
		windDemonSpawners.Add(ScenarioScript._css.Spawners[2]);
		nightDemonSpawners.Add(ScenarioScript._css.Spawners[3]);
		nightDemonSpawners.Add(ScenarioScript._css.Spawners[4]);
		nightDemonSpawners.Add(ScenarioScript._css.Spawners[5]);
		frostDemonSpawners.Add(ScenarioScript._css.Spawners[6]);
		frostDemonSpawners.Add(ScenarioScript._css.Spawners[7]);
		frostDemonSpawners.Add(ScenarioScript._css.Spawners[8]);
		foreach (CMap map in ScenarioManager.CurrentScenarioState.Maps)
		{
			map.CanTeleportToFromAnotherMap = false;
		}
		return base.Initialize();
	}

	protected override void TickTakenDamageInternal(int damageTaken, int damageReducedByShields, int actualDamage)
	{
		if (actualDamage != 0 && _tickActor == boss)
		{
			Stage stage = GetStage(boss.Health);
			Trace($"Attacking the boss, HP: {boss.Health}/{boss.MaxHealth}, Stage: {currentStage} -> {stage}", null, 109);
			if (stage > currentStage)
			{
				ProcessNextStage();
			}
		}
	}

	protected override void TickDeathInternal()
	{
		if (_tickActor != boss && _tickActor.Type == CActor.EType.Enemy && _tickActor.Class.ID.Contains("Demon") && ScenarioManager.Scenario.Enemies.Where((CEnemyActor e) => e.MonsterClass.ID.Contains("Demon") && !e.IsDead).Count() == 0)
		{
			EndConsumeAbility();
			boss.IsImmuneToDamage = false;
			RemoveTileNavLinks();
			TeleportPlayersAndSummonsToBossRoom();
			ClearGlowingOrbRoom();
			RemoveScenarioAbilityFromPlayers(scenarioAbilityHolderAttackBuff1);
			RemoveScenarioAbilityFromPlayers(scenarioAbilityHolderAttackBuff2);
			ScenarioManager.ToggleTileHighlights(bossAdjacentTiles, enabled: false);
			ScenarioManager.ToggleTileHighlights(glowingOrbAdjacentTiles, enabled: false);
			SetDemonsPhaseTextsHidden(hidden: true);
			UpdateEnvironmentVFX();
			spiritRealmRoom.Destroyed = true;
			if (currentStage == Stage.BossOverHalfHP)
			{
				GameState.TriggerCampaignSection("FS_077_The_Fish_Kings_Ascension_Special_4_1");
			}
		}
	}

	private void ProcessNextStage()
	{
		if (currentStage != Stage.BossDying)
		{
			currentStage++;
			if (!spiritRealmRoom.Revealed)
			{
				spiritRealmRoom.Reveal(initial: true);
			}
			spiritRealmRoom.Destroyed = false;
			boss.IsImmuneToDamage = true;
			ReturnBossHPToTreshold();
			SpawnDemons();
			AddAttackBuffs();
			SetDemonsPhaseTextsHidden(hidden: false);
			UpdateEnvironmentVFX();
			RefreshNavLinks(null);
			if (currentStage == Stage.BossOverHalfHP)
			{
				GameState.TriggerCampaignSection("FS_077_The_Fish_Kings_Ascension_Special_1_1");
			}
			else if (currentStage == Stage.BossOverOneQuarterHP)
			{
				GameState.TriggerCampaignSection("FS_077_The_Fish_Kings_Ascension_Special_2_1");
			}
			else if (currentStage == Stage.BossDying)
			{
				GameState.TriggerCampaignSection("FS_077_The_Fish_Kings_Ascension_Special_3_1");
			}
			StartConsumeAbility();
		}
	}

	private Stage GetStage(int bossHealth)
	{
		if (bossHealth > GetBossHPThreshold(Stage.BossOverTreeQuartersHP))
		{
			return Stage.BossOverTreeQuartersHP;
		}
		if (bossHealth > GetBossHPThreshold(Stage.BossOverHalfHP))
		{
			return Stage.BossOverHalfHP;
		}
		if (bossHealth > GetBossHPThreshold(Stage.BossOverOneQuarterHP))
		{
			return Stage.BossOverOneQuarterHP;
		}
		return Stage.BossDying;
	}

	private void ReturnBossHPToTreshold()
	{
		int bossHPThreshold = GetBossHPThreshold(currentStage - 1);
		boss.Health = bossHPThreshold;
		CRefreshActorHealth_MessageData message = new CRefreshActorHealth_MessageData(boss, boss, boss.Health);
		ScenarioRuleClient.MessageHandler(message);
	}

	private int GetBossHPThreshold(Stage stage)
	{
		return stage switch
		{
			Stage.BossOverTreeQuartersHP => (int)MathF.Round((float)boss.MaxHealth * 0.75f), 
			Stage.BossOverHalfHP => (int)MathF.Round((float)boss.MaxHealth * 0.5f), 
			Stage.BossOverOneQuarterHP => (int)MathF.Round((float)boss.MaxHealth * 0.25f), 
			_ => 0, 
		};
	}

	private void SetDemonsPhaseTextsHidden(bool hidden)
	{
		scenarioModifierBossImmunityText.SetHiddenState(hidden);
		scenarioModifierPortalText.SetHiddenState(hidden);
		scenarioObjectiveKillAllDemons.SetActivation(!hidden);
	}

	private void CreateTileNavLinks()
	{
		List<(Point, Point)> list = new List<(Point, Point)>();
		foreach (Point bossAdjacentTile in bossAdjacentTiles)
		{
			CNode node = ScenarioManager.PathFinder.GetNode(bossAdjacentTile);
			foreach (Point glowingOrbAdjacentTile in glowingOrbAdjacentTiles)
			{
				CNode node2 = ScenarioManager.PathFinder.GetNode(glowingOrbAdjacentTile);
				CNavLink item = new CNavLink(1, node2);
				if (!node.NavLinks.Contains(item))
				{
					node.NavLinks.Add(item);
				}
				list.Add((bossAdjacentTile, glowingOrbAdjacentTile));
			}
		}
		ScenarioManager.ToggleTileLinks(list, enabled: true);
	}

	private void RemoveTileNavLinks()
	{
		List<(Point, Point)> list = new List<(Point, Point)>();
		foreach (Point bossAdjacentTile in bossAdjacentTiles)
		{
			CNode node = ScenarioManager.PathFinder.GetNode(bossAdjacentTile);
			foreach (Point glowingOrbAdjacentTile in glowingOrbAdjacentTiles)
			{
				CNode bNode = ScenarioManager.PathFinder.GetNode(glowingOrbAdjacentTile);
				node.NavLinks.RemoveAll((CNavLink nl) => nl.DestinationNode == bNode);
				list.Add((bossAdjacentTile, glowingOrbAdjacentTile));
			}
		}
		ScenarioManager.ToggleTileLinks(list, enabled: false);
	}

	private void SpawnDemons()
	{
		if (currentStage == Stage.BossOverHalfHP)
		{
			SpawnDemons(windDemonSpawners);
		}
		else if (currentStage == Stage.BossOverOneQuarterHP)
		{
			SpawnDemons(nightDemonSpawners);
		}
		else if (currentStage == Stage.BossDying)
		{
			SpawnDemons(frostDemonSpawners);
		}
		else
		{
			Error($"Invalid demons spawn stage: {currentStage}", null, 257);
		}
	}

	private void AddAttackBuffs()
	{
		if (currentStage == Stage.BossOverOneQuarterHP)
		{
			AddScenarioAbilityToPlayers(scenarioAbilityHolderAttackBuff1);
		}
		else if (currentStage == Stage.BossDying)
		{
			AddScenarioAbilityToPlayers(scenarioAbilityHolderAttackBuff2);
		}
	}

	private void SpawnDemons(List<CSpawner> demonSpawners)
	{
		foreach (CSpawner demonSpawner in demonSpawners)
		{
			demonSpawner.SpawnUnit(ScenarioScript.PartySize, treatAsSummon: false, initial: false, startRound: false, forceUseAnyEntry: false, addIntoInitiativeOrder: true);
		}
	}

	private void StartConsumeAbility()
	{
		CAbility cAbility = null;
		if (currentStage == Stage.BossOverHalfHP)
		{
			cAbility = GetScenarioAbilityByID("ScenarioAbility_FH77_DamageWindDemons");
		}
		else if (currentStage == Stage.BossOverOneQuarterHP)
		{
			cAbility = GetScenarioAbilityByID("ScenarioAbility_FH77_DamageNightDemons");
		}
		else if (currentStage == Stage.BossDying)
		{
			cAbility = GetScenarioAbilityByID("ScenarioAbility_FH77_DamageFrostDemons");
		}
		if (cAbility == null)
		{
			return;
		}
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			CAbility cAbility2 = CAbility.CopyAbility(cAbility, generateNewID: true);
			cAbility2.Start(playerActor, playerActor);
			cAbility2.ApplyToActor(playerActor);
		}
	}

	public static void EndConsumeAbility()
	{
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			playerActor.FindApplicableActiveBonuses().FirstOrDefault((CActiveBonus x) => x.Ability.Name == "FH77DamageDemonsAbility")?.Finish();
			playerActor.CharacterClass.CheckForFinishedActiveBonuses(playerActor);
		}
	}

	private ElementInfusionBoardManager.EElement GetStageConsumeElement()
	{
		if (currentStage == Stage.BossOverHalfHP)
		{
			return ElementInfusionBoardManager.EElement.Earth;
		}
		if (currentStage == Stage.BossOverOneQuarterHP)
		{
			return ElementInfusionBoardManager.EElement.Light;
		}
		return ElementInfusionBoardManager.EElement.Fire;
	}

	private void DamageDemonHostAbility(CAbilityNullHex ability, List<CTile> selectedTiles)
	{
		if (selectedTiles.Count != 0)
		{
			ElementInfusionBoardManager.Consume(GetStageConsumeElement(), GameState.CurrentActor);
			CActor actorToDamage = selectedTiles[0].Actors.FirstOrDefault();
			DealScenarioDamage(actorToDamage, ScenarioManager.TrapDamage, isTrapDamage: true);
		}
	}

	private void TeleportPlayersAndSummonsToBossRoom()
	{
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			if (playerActor.Tile.m_HexMap != spiritRealmRoom)
			{
				continue;
			}
			bool flag = false;
			foreach (CTile startingTile in ScenarioManager.StartingTiles)
			{
				if (IsFreeTile(startingTile))
				{
					TeleportActor(playerActor, startingTile);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				CTile cTile2 = ScenarioManager.StartingTiles[0];
				if (GetNearestFreeTile(cTile2.m_ArrayIndex.X, cTile2.m_ArrayIndex.Y, out var nearestTile))
				{
					TeleportActor(playerActor, nearestTile);
				}
			}
		}
		foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
		{
			if (allAliveActor is CHeroSummonActor cHeroSummonActor && cHeroSummonActor.Tile.m_HexMap == spiritRealmRoom && GetNearestFreeTile(cHeroSummonActor.Summoner.ArrayIndex.X, cHeroSummonActor.Summoner.ArrayIndex.Y, out var nearestTile2))
			{
				TeleportActor(allAliveActor, nearestTile2);
			}
		}
	}

	private void ClearGlowingOrbRoom()
	{
		foreach (CActor item in ScenarioManager.Scenario.AllActors.Where((CActor x) => x.Tile.m_HexMap == spiritRealmRoom).ToList())
		{
			GameState.KillActor(item, item, CActor.ECauseOfDeath.ActorRemovedFromMap, out var _);
		}
		CTile[,] tiles = ScenarioManager.Tiles;
		foreach (CTile cTile in tiles)
		{
			if (cTile == null || cTile.m_HexMap != spiritRealmRoom)
			{
				continue;
			}
			for (int num3 = cTile.m_Props.Count() - 1; num3 >= 0; num3--)
			{
				CObjectProp cObjectProp = cTile.m_Props[num3];
				if (!(cObjectProp.PropCustomLocKey == "FH_77_Elemental_Portal_T"))
				{
					cObjectProp.DestroyProp();
				}
			}
		}
	}

	private void AddScenarioAbilityToPlayers(CScenarioModifier holder)
	{
		holder.SetHiddenState(shouldSetHidden: false);
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			AddScenarioAbilityToActor(holder, playerActor);
		}
	}

	private void RemoveScenarioAbilityFromPlayers(CScenarioModifier holder)
	{
		holder.SetHiddenState(shouldSetHidden: true);
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			RemoveScenarioAbilityFromActor(holder, playerActor);
		}
	}

	protected void UpdateEnvironmentVFX()
	{
		if (currentStage == Stage.BossOverTreeQuartersHP)
		{
			UpdateEnvironmentVFX("StateWDI");
		}
		else if (currentStage == Stage.BossOverHalfHP)
		{
			if (!boss.IsImmuneToDamage)
			{
				UpdateEnvironmentVFX("StateDI");
			}
			else
			{
				UpdateEnvironmentVFX("State75");
			}
		}
		else if (currentStage == Stage.BossOverOneQuarterHP)
		{
			if (!boss.IsImmuneToDamage)
			{
				UpdateEnvironmentVFX("StateI");
			}
			else
			{
				UpdateEnvironmentVFX("State50");
			}
		}
		else if (!boss.IsImmuneToDamage)
		{
			UpdateEnvironmentVFX("StateFinal");
		}
		else
		{
			UpdateEnvironmentVFX("State25");
		}
	}

	protected void UpdateEnvironmentVFX(string VFXState)
	{
		CObjectProp environmentVFXProp = GetEnvironmentVFXProp();
		CChangePropVisualState_MessageData cChangePropVisualState_MessageData = new CChangePropVisualState_MessageData();
		cChangePropVisualState_MessageData.m_prop = environmentVFXProp;
		cChangePropVisualState_MessageData.m_stateName = VFXState;
		cChangePropVisualState_MessageData.m_delay = 0.001f;
		ScenarioRuleClient.MessageHandler(cChangePropVisualState_MessageData);
	}

	protected CObjectProp GetEnvironmentVFXProp()
	{
		CTile cTile = ScenarioManager.Tiles[2, 12];
		return cTile?.m_Props?.Find((CObjectProp p) => p.PrefabName == "S77EnvironmentalVFX") ?? SpawnVFXProp(cTile, "S77EnvironmentalVFX");
	}

	protected override void TickAbilityStartedInternal()
	{
		base.TickAbilityStartedInternal();
		RefreshNavLinks(_tickAbility);
	}

	protected override void TickAbilityEndedInternal()
	{
		base.TickAbilityEndedInternal();
		RefreshNavLinks(null);
	}

	private void RefreshNavLinks(CAbility ability)
	{
		if (currentStage > Stage.BossOverTreeQuartersHP && spiritRealmRoom.RevealedAndNotDestroyed)
		{
			if (ability == null || ability is CAbilityMove)
			{
				CreateTileNavLinks();
			}
			else
			{
				RemoveTileNavLinks();
			}
			ScenarioManager.ToggleTileHighlights(bossAdjacentTiles, enabled: true);
			ScenarioManager.ToggleTileHighlights(glowingOrbAdjacentTiles, enabled: true);
		}
		else
		{
			RemoveTileNavLinks();
			ScenarioManager.ToggleTileHighlights(bossAdjacentTiles, enabled: false);
			ScenarioManager.ToggleTileHighlights(glowingOrbAdjacentTiles, enabled: false);
		}
	}

	public override bool IsTeleportValid(CTile a, CTile b)
	{
		if (a.m_HexMap == spiritRealmRoom && b.m_HexMap != spiritRealmRoom)
		{
			return false;
		}
		if (b.m_HexMap == spiritRealmRoom && a.m_HexMap != spiritRealmRoom)
		{
			return false;
		}
		return base.IsTeleportValid(a, b);
	}
}
