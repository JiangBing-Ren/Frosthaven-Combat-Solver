using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.CustomLevels;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH057 : ScenarioScript
{
	public const string WindCarvingPrefabName = "WindCarving";

	public const string WindCarvingVFXPrefabName = "WindCarvingShieldVFX";

	public CActor boss;

	protected override bool Initialize()
	{
		boss = FindActorByClass("RenderID");
		DisableMonsterLootDrop();
		UpdateBossInvulnerability();
		UpdateInvulnerabilityVFX();
		return true;
	}

	protected override string SerializeInternal()
	{
		return base.SerializeInternal();
	}

	protected override void DeserializeInternal(string s)
	{
		UpdateModifiers();
		UpdateInvulnerabilityVFX();
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartTurn:
			TickStartTurn();
			break;
		case EScenarioModifierTriggerPhase.EndTurn:
			TickEndTurn();
			break;
		case EScenarioModifierTriggerPhase.EndRound:
			TickEndRound();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 51);
			break;
		case EScenarioModifierTriggerPhase.StartRound:
			break;
		}
	}

	private void TickStartTurn()
	{
	}

	private void TickEndTurn()
	{
		if (_tickActor.Type == CActor.EType.Player || IsDenpang(_tickActor))
		{
			CheckOfferPlaceWindCarving(_tickActor);
		}
	}

	private void TickEndRound()
	{
	}

	protected override void TickRepositionInternal()
	{
		if (_tickActor == boss)
		{
			UpdateBossInvulnerability();
		}
	}

	protected override void TickDeathInternal()
	{
	}

	public bool IsDenpang(CActor actor)
	{
		return actor?.Class?.ID == "DenpangS57ID";
	}

	public List<CObjectProp> GetWindCarvings()
	{
		return ScenarioManager.CurrentScenarioState.Props.Where((CObjectProp x) => x.PrefabName == "WindCarving").ToList();
	}

	private bool CanPlaceWindCarving(Point pos, List<CObjectProp> windCarvings)
	{
		foreach (CObjectProp windCarving in windCarvings)
		{
			Point pt = new Point(windCarving.ArrayIndex);
			if (Point.HexDist(pos, pt) <= 3)
			{
				return false;
			}
		}
		CTile propTile = ScenarioManager.GetTile(pos);
		if (CObjectProp.FindPropWithPathingBlocker(pos, ref propTile) != null)
		{
			return false;
		}
		return true;
	}

	private void CheckOfferPlaceWindCarving(CActor actor)
	{
		List<CObjectProp> windCarvings = GetWindCarvings();
		if (CanPlaceWindCarving(actor.ArrayIndex, windCarvings))
		{
			int num = ((ScenarioScript.PartySize == 4) ? 5 : 6);
			if (windCarvings.Count < num)
			{
				OfferPlaceWindCarving(actor);
			}
			else
			{
				OfferRemoveWindCarving(actor, windCarvings);
			}
		}
	}

	private void OfferPlaceWindCarving(CActor actor)
	{
		List<CTile> validTiles = new List<CTile> { ScenarioManager.GetTile(actor.ArrayIndex) };
		ScenarioScript.PickHexes("$FH/FH_Scenario_Campaign_057_Select_Hex_1$", actor, validTiles, PlaceWindCarvingCallback, 1, animateActor: true, null, canSkip: true, canUndo: true, recalculateValidTilesOnSelect: false, preSelectValidTiles: true, "UseItem", null, CAbilityFilter.EFilterTile.None, ignoreLos: false, ignoreDoorLocks: false, stackAsSeparateAbilities: true);
	}

	private void PlaceWindCarvingCallback(CAbilityNullHex ability, List<CTile> selectedTiles)
	{
		if (selectedTiles != null && selectedTiles.Count != 0)
		{
			CTile cTile = selectedTiles[0];
			PlaceWindCarvingAt(cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y);
		}
	}

	public void PlaceWindCarvingAt(int X, int Y)
	{
		CTile tile = ScenarioManager.GetTile(X, Y);
		CObjectProp cObjectProp = new CObjectProp("WindCarving", ScenarioManager.ObjectImportType.VFXProp, new TileIndex(X, Y), null, null, null, tile.m_HexMap.MapGuid);
		cObjectProp.PropHealthDetails = new PropHealthDetails();
		cObjectProp.PropHealthDetails.CustomLocKey = "FH_Scenario_Campaign_057_SCENARIO_OBJECT_1";
		cObjectProp.PropCustomLocKey = "FH_Scenario_Campaign_057_SCENARIO_OBJECT_1";
		cObjectProp.PropCustomDescriptionLocKey = "FH_57_Wind_Carving_D";
		tile.SpawnProp(cObjectProp);
		UpdateWindCarvingVFX(cObjectProp.ArrayIndex);
		UpdateBossInvulnerability();
	}

	private void OfferRemoveWindCarving(CActor actor, List<CObjectProp> windCarvings)
	{
		List<CTile> validTiles = windCarvings.Select((CObjectProp x) => x.PropTile).ToList();
		ScenarioScript.PickHexes("$FH/FH_Scenario_Campaign_057_Select_Hex_2$", actor, validTiles, RemoveWindCarvingCallback, 1, animateActor: true, null, canSkip: true, canUndo: true, recalculateValidTilesOnSelect: false, preSelectValidTiles: true, "UseItem", null, CAbilityFilter.EFilterTile.None, ignoreLos: false, ignoreDoorLocks: false, stackAsSeparateAbilities: true);
	}

	private void RemoveWindCarvingCallback(CAbilityNullHex ability, List<CTile> selectedTiles)
	{
		if (selectedTiles != null && selectedTiles.Count != 0)
		{
			CTile cTile = selectedTiles[0];
			RemoveWindCarvingAt(cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y);
			OfferPlaceWindCarving(ability.m_ExternalParams.Actor);
		}
	}

	public void RemoveWindCarvingAt(int X, int Y)
	{
		CTile tile = ScenarioManager.GetTile(X, Y);
		if (tile.m_Props != null)
		{
			CObjectProp cObjectProp = tile.m_Props.FirstOrDefault((CObjectProp x) => x.PrefabName == "WindCarving");
			if (cObjectProp != null)
			{
				cObjectProp.DestroyProp();
				RemoveWindCarvingVFX(cObjectProp.ArrayIndex);
				UpdateBossInvulnerability();
			}
		}
	}

	public override void OnSpawnerHasSpawned(CSpawner spawner, CActor actor)
	{
		base.OnSpawnerHasSpawned(spawner, actor);
		DisableMonsterLootDrop();
	}

	private void DisableMonsterLootDrop()
	{
		foreach (CEnemyActor enemy in ScenarioManager.Scenario.Enemies)
		{
			enemy.NoGoldDrop = true;
		}
	}

	private bool ShouldBossBeInvulnerable()
	{
		List<CObjectProp> windCarvings = GetWindCarvings();
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		foreach (CObjectProp item in windCarvings)
		{
			if (item.ArrayIndex.X == boss.ArrayIndex.X)
			{
				if (item.ArrayIndex.Y % 2 == 1 && boss.ArrayIndex.Y % 2 == 0)
				{
					num2++;
				}
				if (item.ArrayIndex.Y % 2 == 0 && boss.ArrayIndex.Y % 2 == 1)
				{
					num++;
				}
			}
			if (item.ArrayIndex.X < boss.ArrayIndex.X)
			{
				num++;
			}
			if (item.ArrayIndex.X > boss.ArrayIndex.X)
			{
				num2++;
			}
			if (item.ArrayIndex.Y < boss.ArrayIndex.Y)
			{
				num4++;
			}
			if (item.ArrayIndex.Y > boss.ArrayIndex.Y)
			{
				num3++;
			}
		}
		if (num >= 2 && num2 >= 2 && num3 >= 2)
		{
			return num4 < 2;
		}
		return true;
	}

	private void UpdateBossInvulnerability()
	{
		bool flag = ShouldBossBeInvulnerable();
		bool flag2 = boss.MonsterClass.CurrentMonsterStat.Invulnerable != flag;
		boss.MonsterClass.CurrentMonsterStat.Invulnerable = flag;
		if (flag2)
		{
			UpdateInvulnerabilityVFX();
		}
	}

	private void UpdateInvulnerabilityVFX()
	{
		ScenarioRuleClient.MessageHandler(new CRemoveCasterVFX_MessageData(boss)
		{
			m_VFXName = "RendererShield"
		});
		if (boss.MonsterClass.CurrentMonsterStat.Invulnerable)
		{
			ScenarioRuleClient.MessageHandler(new CAddCasterVFX_MessageData(boss)
			{
				m_VFXName = "RendererShield"
			});
		}
		foreach (CObjectProp windCarving in GetWindCarvings())
		{
			UpdateWindCarvingVFX(windCarving.ArrayIndex);
		}
	}

	private void UpdateWindCarvingVFX(TileIndex position)
	{
		if (!boss.MonsterClass.CurrentMonsterStat.Invulnerable)
		{
			CTile tile = ScenarioManager.GetTile(position.X, position.Y);
			if (tile.m_Props.FirstOrDefault((CObjectProp x) => x.PrefabName == "WindCarvingShieldVFX") == null)
			{
				CObjectProp prop = new CObjectProp("WindCarvingShieldVFX", ScenarioManager.ObjectImportType.VFXProp, position, null, null, null, tile.m_HexMap.MapGuid);
				tile.SpawnProp(prop);
			}
		}
		else
		{
			RemoveWindCarvingVFX(position);
		}
	}

	private void RemoveWindCarvingVFX(TileIndex position)
	{
		ScenarioManager.GetTile(position.X, position.Y).m_Props.FirstOrDefault((CObjectProp x) => x.PrefabName == "WindCarvingShieldVFX")?.DestroyProp();
	}

	public CTile GetFartherstWindCarvingTeleportTile()
	{
		CObjectProp furthestWindCarving;
		return GetFartherstWindCarvingTeleportTile(out furthestWindCarving);
	}

	public CObjectProp GetFartherstWindCarving()
	{
		GetFartherstWindCarvingTeleportTile(out var furthestWindCarving);
		return furthestWindCarving;
	}

	public CTile GetFartherstWindCarvingTeleportTile(out CObjectProp furthestWindCarving)
	{
		List<CObjectProp> windCarvings = GetWindCarvings();
		furthestWindCarving = null;
		int num = 0;
		CTile result = null;
		List<CTile> list = new List<CTile>();
		foreach (CObjectProp item in windCarvings)
		{
			Point point = new Point(item.ArrayIndex);
			int num2 = Point.HexDist(point, boss.ArrayIndex);
			if (num2 >= num)
			{
				GameState.GetAdjacentTiles(point, list);
				if (GetNearestFreeTile(boss.ArrayIndex.X, boss.ArrayIndex.Y, out var nearestTile, list))
				{
					furthestWindCarving = item;
					num = num2;
					result = nearestTile;
				}
			}
		}
		if (furthestWindCarving == null)
		{
			return null;
		}
		return result;
	}

	public void DestroyWindCarvingsAroundBoss()
	{
		foreach (CObjectProp windCarving in GetWindCarvings())
		{
			if (Point.HexDist(new Point(windCarving.ArrayIndex), boss.ArrayIndex) == 1)
			{
				RemoveWindCarvingAt(windCarving.ArrayIndex.X, windCarving.ArrayIndex.Y);
			}
		}
	}

	private void UpdateModifiers()
	{
		GetScenarioModifier("HintLessThan4Players").SetDeactivated(ScenarioScript.PartySize == 4);
		GetScenarioModifier("Hint4Players").SetDeactivated(ScenarioScript.PartySize != 4);
		GetScenarioModifier("HintDenpang").SetDeactivated(ScenarioScript.PartySize != 2);
	}

	protected override bool AllowTriggerMessageInternal(SEvent ev, CLevelMessage message)
	{
		_ = ev.Type;
		_ = 2;
		return false;
	}

	public override IEnumerable<(string, string)> GetSpecialRulesParamsString()
	{
		yield return ("max_wind_carving", ((ScenarioScript.PartySize == 4) ? 5 : 6).ToString());
	}
}
