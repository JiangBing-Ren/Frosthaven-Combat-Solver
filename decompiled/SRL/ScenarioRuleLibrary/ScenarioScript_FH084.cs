using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Libs.GameConsole;
using Newtonsoft.Json;
using ScenarioRuleLibrary.CustomLevels;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH084 : ScenarioScript
{
	[Serializable]
	public class Storage84
	{
		public List<(string, int)> PlayerSlimes = new List<(string, int)>();
	}

	[ConsoleVariable(Alias = "ss084_debug_slimes")]
	public static int DebugSlimes_cvar = 1;

	private static string OOZE_SLIME_PREFAB_NAME = "OozeSlime";

	private static string OOZE_REMAINS_PREFAB_NAME = "OozeRemains";

	private static string SLIME_PROP_NAME_LOC_KEY = "FH_Scenario_Campaign_084_SCENARIO_OBJECT_1";

	private static string SLIME_PROP_DECK_LOC_KEY = "FH_84_Slime_D";

	private static string REMAINS_PROP_NAME_LOC_KEY = "FH_Scenario_Campaign_084_SCENARIO_OBJECT_2";

	private static string[] SHIELD_ABILITY_NAMES = new string[4] { "AScenarioShield1", "AScenarioShield2", "AScenarioShield3", "AScenarioShield4" };

	private static string[] SHIELD_MODIFIERS_NAMES = new string[4] { "ShieldAbility1", "ShieldAbility2", "ShieldAbility3", "ShieldAbility4" };

	private static string DISADVANTAGE_ABILITY_NAME = "ADisadvantage";

	private static string DISADVANTAGE_MODIFIER_NAME = "DisadvantageAbility";

	public Dictionary<CActor, int> PlayersSlimes = new Dictionary<CActor, int>();

	private Dictionary<(int, int), bool> NodesIsDifficultTerrain = new Dictionary<(int, int), bool>();

	private CSpawner oozeBossSpawner;

	private CSpawner oozeSpawner;

	private List<CEnemyActor> AliveOozes => ScenarioManager.Scenario.AllMonsters.FindAll((CEnemyActor a) => IsOoze(a) && !a.IsDead);

	private List<CEnemyActor> AliveNormalOozes => AliveOozes.Where((CEnemyActor x) => !x.IsElite()).ToList();

	private CActor AliveOozeBoss => ScenarioScript._css.Monsters.FirstOrDefault((EnemyState m) => m.IsBoss && !m.IsDead && !m.Actor.IsDead)?.Actor;

	private int MaxOozeCount => ScenarioScript.PartySize * 2;

	private List<CTile> ScenarioRoomTiles => GetRoomTiles("Room_1");

	private static bool IsOoze(CActor actor)
	{
		if (actor.Type == CActor.EType.Enemy)
		{
			return actor.MonsterClass.ID.StartsWith("Ooze");
		}
		return false;
	}

	private static bool IsOozeBoss(CActor actor)
	{
		if (actor.Type == CActor.EType.Enemy)
		{
			return actor.MonsterClass.ID == "ElderOozeID";
		}
		return false;
	}

	private CObjectProp GetSlime(CTile tile)
	{
		return tile.m_Props.Find((CObjectProp p) => p.PrefabName == OOZE_SLIME_PREFAB_NAME);
	}

	private CActor GetAliveOoze(CTile tile)
	{
		return AliveOozes.Find((CEnemyActor o) => o.Tile == tile);
	}

	private bool IsPlayerWithSlime(CActor actor)
	{
		if (actor is CPlayerActor)
		{
			return PlayersSlimes[actor] > 0;
		}
		return false;
	}

	public override bool ShouldSortModifierTextsInOrderOfModifiers()
	{
		return true;
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartTurn:
			TickStartTurn();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 69);
			break;
		case EScenarioModifierTriggerPhase.StartRound:
		case EScenarioModifierTriggerPhase.EndRound:
		case EScenarioModifierTriggerPhase.EndTurn:
		case EScenarioModifierTriggerPhase.StartScenario:
		case EScenarioModifierTriggerPhase.AfterAbility:
		case EScenarioModifierTriggerPhase.OnRoomRevealed:
			break;
		}
	}

	protected override string SerializeInternal()
	{
		Storage84 storage = new Storage84();
		foreach (KeyValuePair<CActor, int> playersSlime in PlayersSlimes)
		{
			storage.PlayerSlimes.Add((playersSlime.Key.ActorGuid, playersSlime.Value));
		}
		return JsonConvert.SerializeObject(storage);
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		UpdateElderOozeMinimumHealth();
		foreach (CSpawner spawner in ScenarioManager.CurrentScenarioState.Spawners)
		{
			spawner.SetActive(active: false);
		}
		if (string.IsNullOrEmpty(storage))
		{
			return;
		}
		PlayersSlimes.Clear();
		try
		{
			foreach (var player in ((Storage84)JsonConvert.DeserializeObject(storage, typeof(Storage84))).PlayerSlimes)
			{
				CActor cActor = ScenarioManager.Scenario.AllActors.SingleOrDefault((CActor x) => x.ActorGuid == player.Item1);
				if (cActor != null)
				{
					PlayersSlimes[cActor] = player.Item2;
				}
			}
		}
		catch (Exception)
		{
		}
	}

	protected override bool Initialize()
	{
		oozeBossSpawner = ScenarioScript._css.Spawners[0];
		oozeSpawner = ScenarioScript._css.Spawners[1];
		foreach (PlayerState player in ScenarioScript._css.Players)
		{
			PlayersSlimes.TryAdd(player.Actor, 0);
		}
		if (AliveOozeBoss != null)
		{
			AliveOozeBoss.CanBeDowned = true;
		}
		CheckSpecialRules();
		return true;
	}

	private void TickStartTurn()
	{
		if (IsPlayerWithSlime(_tickActor))
		{
			RemoveSlime(_tickActor);
		}
		CheckSpecialRules();
	}

	protected override void TickRepositionInternal()
	{
		CActor tickActor = _tickActor;
		if (IsOoze(tickActor) && !tickActor.IsElite)
		{
			foreach (CActor livingActor in _tickActor.Tile.LivingActors)
			{
				if (IsOozeBoss(livingActor))
				{
					HealUsingOoze(livingActor, tickActor);
				}
			}
		}
		else if (IsOozeBoss(tickActor))
		{
			foreach (CActor livingActor2 in tickActor.Tile.LivingActors)
			{
				if (IsOoze(livingActor2) && !livingActor2.IsElite)
				{
					HealUsingOoze(tickActor, livingActor2);
				}
			}
		}
		if (!tickActor.IsDead)
		{
			CObjectProp slime = GetSlime(tickActor.Tile);
			if (tickActor is CPlayerActor && slime != null)
			{
				AddSlime(tickActor);
				slime.DestroyProp(0f, sendMessageToClient: true, forceDestroy: true);
			}
			else if (slime != null && tickActor.Type == CActor.EType.Enemy)
			{
				HealUsingSlime(tickActor, slime);
			}
		}
		CheckSpecialRules();
	}

	protected override void TickAbilityStartedInternal()
	{
		if (IsPlayerWithSlime(_tickActor) && _tickAbility is CAbilityMove)
		{
			SaveIsDifficultTerrainSnapshot();
			SetAllTilesAsDifficultTerrain();
		}
	}

	public override void OnActorMoving(CActor movingActor, Point currentPosition, Point nextPosition)
	{
		CActor cActor = ScenarioManager.GetTile(nextPosition).LivingActors.ToList().Find((CActor a) => a != movingActor);
		if (IsOozeBoss(movingActor))
		{
			TryToPlaceSlime(ScenarioManager.GetTile(currentPosition));
		}
		if (cActor != null)
		{
			string text = null;
			if (IsOoze(movingActor) && IsOozeBoss(cActor) && !movingActor.IsElite)
			{
				text = "OozeToBossAssimilationVFXS84";
			}
			else if (IsOozeBoss(movingActor) && IsOoze(cActor) && !cActor.IsElite)
			{
				text = "BossToOozeAssimilationVFXS84";
			}
			if (!string.IsNullOrEmpty(text))
			{
				CAddCasterVFX_MessageData message = new CAddCasterVFX_MessageData(movingActor)
				{
					m_VFXName = text,
					m_delay = 0.001f
				};
				ScenarioRuleClient.MessageHandler(message);
			}
		}
	}

	protected override void TickAbilityEndedInternal()
	{
		if (_tickAbility is CAbilityHeal { ActorsTargeted: var actorsTargeted } cAbilityHeal && actorsTargeted.Exists((CActor at) => at is CPlayerActor))
		{
			foreach (CActor item in cAbilityHeal.ActorsTargeted)
			{
				if (item is CPlayerActor && PlayersSlimes[item] > 0)
				{
					RemoveSlime(item);
				}
			}
		}
		else if (IsPlayerWithSlime(_tickActor) && _tickAbility is CAbilityMove && NodesIsDifficultTerrain.Any())
		{
			RestoreIsDifficultTerrainSnapshot();
		}
		else if (IsOozeBoss(_tickActor) && (_tickAbility.Flags & CAbility.EAbilityFlag.Special1) != 0 && _tickAbility is CAbilityNull)
		{
			AddAllAdjacentPlayersSlime(_tickActor);
			Trace("Boss Special1 Null Ability - add slime to all adjacent characters", null, 220);
		}
		CheckSpecialRules();
	}

	protected override void TickAbilityUndoneInternal()
	{
		if (IsPlayerWithSlime(_tickActor) && _tickAbility is CAbilityMove && NodesIsDifficultTerrain.Any())
		{
			RestoreIsDifficultTerrainSnapshot();
		}
	}

	public static void LoseItem(CItem item, CActor actor)
	{
		CInventory inventory = actor.Inventory;
		inventory.ConsumeItem(item);
		if (inventory.IsItemActive(item))
		{
			inventory.CancelActiveBonusesOnRefreshingItems(item);
		}
	}

	protected override void TickDeathInternal()
	{
		if (IsOoze(_tickActor))
		{
			TryToPlaceSlime(_tickActor.Tile);
		}
		CheckSpecialRules();
	}

	protected override void TickTakenDamageInternal(int damageTaken, int damageReducedByShields, int actualDamage)
	{
		if (IsOozeBoss(_tickActor) && actualDamage > 0)
		{
			if (ValidateCanSpawnOoze() && GetNearestFreeTile(_tickActor.ArrayIndex.X, _tickActor.ArrayIndex.Y, out CTile nearestTile, (List<CTile>)null, (Func<int, int, bool>)((int x, int y) => ScenarioManager.GetTileDistance(x, y, _tickActor.ArrayIndex.X, _tickActor.ArrayIndex.Y) <= 3)))
			{
				CActor cActor = SpawnAtTileAndHp(oozeSpawner, nearestTile, actualDamage);
				CObjectProp slime = GetSlime(nearestTile);
				if (cActor != null && slime != null)
				{
					HealUsingSlime(cActor, slime);
				}
			}
			else
			{
				List<CEnemyActor> aliveNormalOozes = AliveNormalOozes;
				if (aliveNormalOozes.Count > 0)
				{
					CEnemyActor actor = aliveNormalOozes.OrderBy((CEnemyActor o) => o.Health).First();
					Heal(actor, actualDamage);
				}
			}
			UpdateElderOozeMinimumHealth();
		}
		CheckSpecialRules();
	}

	private void CheckSpecialRules()
	{
		if (AliveOozeBoss != null)
		{
			UpdateBossShieldBonus(AliveOozeBoss);
		}
		UpdateOozesFocus();
		UpdateOozesFocusNoGoldDrop();
		UpdateElderOozeMinimumHealth();
	}

	public static bool ValidateCanSpawnOoze()
	{
		if (!(ScenarioScript.current is ScenarioScript_FH084 scenarioScript_FH) || scenarioScript_FH.AliveOozes.Count < scenarioScript_FH.MaxOozeCount)
		{
			return true;
		}
		scenarioScript_FH.Trace($"Cant spawn Ooze. There's {scenarioScript_FH.AliveOozes.Count} alive oozes, max ooze count is {scenarioScript_FH.MaxOozeCount}.", null, 285);
		return false;
	}

	private void AddSlime(CActor actor)
	{
		bool flag = PlayersSlimes[actor] > 0;
		PlayersSlimes[actor]++;
		UpdatePlayerDisadvantageBonus(actor, IsPlayerWithSlime(actor));
		if (!flag && PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability is CAbilityMove cAbilityMove && cAbilityMove.TargetingActor == actor)
		{
			cAbilityMove.ForceInterrupt();
		}
	}

	private void RemoveSlime(CActor actor)
	{
		PlayersSlimes[actor]--;
		UpdatePlayerDisadvantageBonus(actor, IsPlayerWithSlime(actor));
	}

	private CActor SpawnAtTileAndHp(CSpawner spawner, CTile tile, int initialHp)
	{
		spawner.SetLocation(tile.m_Hex.ArrayIndex);
		spawner.SpawnUnit(ScenarioScript.PartySize, treatAsSummon: false, initial: false, startRound: false, forceUseAnyEntry: false, addIntoInitiativeOrder: true);
		CEnemyActor cEnemyActor = ScenarioManager.Scenario.AllMonsters.FirstOrDefault((CEnemyActor m) => m.Tile == tile && !m.IsDead);
		if (cEnemyActor == null)
		{
			Trace($"Can't find spawned monster at tile {tile.m_ArrayIndex.X},{tile.m_ArrayIndex.Y}!", null, 324);
			return null;
		}
		if (initialHp < cEnemyActor.MaxHealth)
		{
			cEnemyActor.Damaged(cEnemyActor.MaxHealth - initialHp, fromAttackAbility: false, null, null);
		}
		cEnemyActor.NoGoldDrop = true;
		return cEnemyActor;
	}

	private void TryToPlaceSlime(CTile tile)
	{
		if (!IsFreeTile(tile, checkEmpty: false))
		{
			Trace($"Can't place slime. Tile {tile.m_ArrayIndex.X},{tile.m_ArrayIndex.Y} is not free or has ooze remains.", null, 336);
		}
		else if (GetSlime(tile) != null)
		{
			Trace($"Can't place slime. There's existing slime on tile  {tile.m_ArrayIndex.X},{tile.m_ArrayIndex.Y} .", null, 341);
		}
		else
		{
			SpawnVFXProp(tile, OOZE_SLIME_PREFAB_NAME, SLIME_PROP_NAME_LOC_KEY, SLIME_PROP_DECK_LOC_KEY);
		}
	}

	private void UpdateBossShieldBonus(CActor actor)
	{
		if (actor.Type == CActor.EType.Enemy && actor.IsBoss)
		{
			int num = Math.Max(ScenarioScript.PartySize - AliveOozes.Count, 0);
			CActiveBonus cActiveBonus = actor.FindApplicableActiveBonuses(CAbility.EAbilityType.Shield).FirstOrDefault((CActiveBonus sb) => SHIELD_ABILITY_NAMES.Contains(sb.Ability.Name));
			if ((cActiveBonus?.Ability.Strength ?? 0) != num)
			{
				ChangeActorShieldBonus(actor, cActiveBonus, num);
			}
		}
	}

	private void ChangeActorShieldBonus(CActor actor, CActiveBonus currentScenarioBonus, int shieldStrength)
	{
		currentScenarioBonus?.Finish();
		if (shieldStrength > 0)
		{
			CAbility cAbility = GetScenarioModifier(SHIELD_MODIFIERS_NAMES[shieldStrength - 1]).TriggerAbility();
			cAbility.Start(actor, actor);
			cAbility.ApplyToActor(actor);
		}
	}

	private void UpdatePlayerDisadvantageBonus(CActor actor, bool hasDisadvantage)
	{
		CActiveBonus cActiveBonus = (from x in actor.FindApplicableActiveBonuses(CAbility.EAbilityType.Attack, CActiveBonus.EActiveBonusBehaviourType.BuffAttack)
			where x.Ability is CAbilityAttack { MiscAbilityData: not null } cAbilityAttack && cAbilityAttack.MiscAbilityData.AttackHasDisadvantage.HasValue && cAbilityAttack.MiscAbilityData.AttackHasDisadvantage.Value && x.Ability?.GetAbilityBaseCard() is CScenarioModifier cScenarioModifier && cScenarioModifier.ScenarioAbilityID == "ScenarioAbility_FH84_HasDisadvantageBuff"
			select x).FirstOrDefault();
		if (!hasDisadvantage && cActiveBonus != null)
		{
			cActiveBonus.Finish();
		}
		else if (hasDisadvantage && cActiveBonus == null)
		{
			ApplyScenarioAbilityToActor(actor, DISADVANTAGE_MODIFIER_NAME);
		}
		ScenarioRuleClient.MessageHandler(new CUpdateWorldspaceConditionsUI_MessageData(actor));
	}

	private void ApplyScenarioAbilityToActor(CActor actor, string scenarioModifierName)
	{
		CAbility cAbility = GetScenarioModifier(scenarioModifierName).TriggerAbility();
		cAbility.Start(actor, actor);
		cAbility.ApplyToActor(actor);
	}

	private void HealUsingSlime(CActor actor, CObjectProp slimeProp)
	{
		int strength = (int)Math.Ceiling((double)ScenarioScript._css.Level / 2.0);
		Heal(actor, strength);
		slimeProp.DestroyProp(0f, sendMessageToClient: true, forceDestroy: true);
	}

	private void HealUsingOoze(CActor actor, CActor ooze)
	{
		int health = ooze.Health;
		GameState.KillActor(ooze, ooze, CActor.ECauseOfDeath.Suicide, out var _, null, actorWasAsleep: false, null, instantlyDeletePrefab: true);
		Heal(actor, health);
	}

	private void Heal(CActor actor, int strength)
	{
		if (actor.IsDowned)
		{
			actor.SetDowned(downed: false);
		}
		int health = Math.Min(actor.Health + strength, actor.MaxHealth);
		ScenarioScript.HealTo(actor, health, removeAllTokens: false, ignoreTokens: false);
	}

	private void SaveIsDifficultTerrainSnapshot()
	{
		NodesIsDifficultTerrain = new Dictionary<(int, int), bool>();
		_ = ScenarioManager.Tiles;
		_ = ScenarioRoomTiles;
		foreach (CTile scenarioRoomTile in ScenarioRoomTiles)
		{
			NodesIsDifficultTerrain[(scenarioRoomTile.m_ArrayIndex.X, scenarioRoomTile.m_ArrayIndex.Y)] = ScenarioManager.PathFinder.Nodes[scenarioRoomTile.m_ArrayIndex.X, scenarioRoomTile.m_ArrayIndex.Y].IsDifficultTerrain;
		}
	}

	private void SetAllTilesAsDifficultTerrain()
	{
		foreach (CTile scenarioRoomTile in ScenarioRoomTiles)
		{
			ScenarioManager.PathFinder.Nodes[scenarioRoomTile.m_ArrayIndex.X, scenarioRoomTile.m_ArrayIndex.Y].IsDifficultTerrain = true;
		}
	}

	private void RestoreIsDifficultTerrainSnapshot()
	{
		foreach (CTile scenarioRoomTile in ScenarioRoomTiles)
		{
			ScenarioManager.PathFinder.Nodes[scenarioRoomTile.m_ArrayIndex.X, scenarioRoomTile.m_ArrayIndex.Y].IsDifficultTerrain = NodesIsDifficultTerrain[(scenarioRoomTile.m_ArrayIndex.X, scenarioRoomTile.m_ArrayIndex.Y)];
		}
		NodesIsDifficultTerrain.Clear();
	}

	private void UpdateOozesFocus()
	{
		List<CEnemyActor> aliveOozes = AliveOozes;
		foreach (EnemyState monster in ScenarioScript._css.Monsters)
		{
			if (!aliveOozes.Contains(monster.Actor))
			{
				continue;
			}
			if (monster.IsElite)
			{
				monster.SetAIFocusOverride(null);
				continue;
			}
			CAIFocusOverrideDetails cAIFocusOverrideDetails = monster.AIFocusOverride ?? new CAIFocusOverrideDetails();
			cAIFocusOverrideDetails.OverrideType = CAIFocusOverrideDetails.EOverrideType.OverrideFocus;
			if (AliveOozeBoss != null)
			{
				cAIFocusOverrideDetails.OverrideTargetType = CAIFocusOverrideDetails.EOverrideTargetType.Actor;
				cAIFocusOverrideDetails.TargetGUID = AliveOozeBoss.ActorGuid;
				cAIFocusOverrideDetails.ForcedUseTileIndex = new TileIndex(AliveOozeBoss.ArrayIndex);
			}
			else
			{
				Trace("Cant set ooze focus override! There's no alive ooze boss or ooze remains.", null, 453);
			}
			cAIFocusOverrideDetails.FocusBenign = true;
			monster.SetAIFocusOverride(cAIFocusOverrideDetails);
		}
	}

	private void UpdateOozesFocusNoGoldDrop()
	{
		CActor aliveOozeBoss = AliveOozeBoss;
		if (aliveOozeBoss != null)
		{
			aliveOozeBoss.NoGoldDrop = true;
		}
	}

	private void AddAllAdjacentPlayersSlime(CActor actor)
	{
		foreach (CActor allAdjacentActor in ScenarioManager.GetAllAdjacentActors(actor))
		{
			if (allAdjacentActor is CPlayerActor)
			{
				AddSlime(allAdjacentActor);
			}
		}
	}

	private void UpdateElderOozeMinimumHealth()
	{
		int spawningOozeMaxHealth = GetSpawningOozeMaxHealth();
		AliveOozeBoss.MinimumHealth = AliveOozeBoss.Health - spawningOozeMaxHealth;
	}

	public int GetSpawningOozeMaxHealth()
	{
		string spawnID = oozeSpawner.SpawnRoundEntries[0].SpawnClass[ScenarioScript.PartySize - 1];
		return MonsterClassManager.Classes.Find((CMonsterClass x) => x.ID == spawnID)?.Health() ?? 0;
	}

	public static int GetPlayerSlimeCount(CActor actor)
	{
		ScenarioScript_FH084 scenarioScript_FH = ScenarioScript.current as ScenarioScript_FH084;
		if (scenarioScript_FH?.PlayersSlimes == null)
		{
			return 0;
		}
		if (scenarioScript_FH.PlayersSlimes.TryGetValue(actor, out var value))
		{
			return value;
		}
		return 0;
	}

	public override IEnumerable<(string, string)> GetSpecialRulesParamsString()
	{
		yield return ("max_ooze_count", MaxOozeCount.ToString());
		yield return ("slime_heal_amount", ((int)Math.Ceiling((double)ScenarioScript._css.Level / 2.0)).ToString());
		yield return ("elder_ooze_damage_cap", GetSpawningOozeMaxHealth().ToString());
	}
}
