using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AStar;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH130 : ScenarioScript
{
	public class IceMoveParams
	{
		public CTile firstTile;

		public CObjectProp iceProp;

		public List<CActor> actorsToMove = new List<CActor>();

		public List<CActor> actorsMightMove = new List<CActor>();

		public List<CObjectProp> propsToMove = new List<CObjectProp>();

		public List<CActor> actorsToDamageHazardous = new List<CActor>();

		public int dist;

		public int desiredDist;

		public bool isIceMovement;

		public CTile destTile => GetLeftNeighbor(firstTile, dist);
	}

	private List<CTile> bTiles = new List<CTile>();

	private List<CTile> initialIceTiles = new List<CTile>();

	private List<CTile> cTiles = new List<CTile>();

	private List<CTile> initialAgateTiles = new List<CTile>();

	private string IceProp = ESpecificPropType.FloatingIce.ToString();

	private const string resourceID = "FH_Scenario_Campaign_130_SCENARIO_OBJECT_1";

	private int agatesFrom129;

	public bool isPickingWhereToDropAgateFromDamage;

	private CObjective_Scripted CarrySunAgates;

	private CObjective_Scripted FailIfNotEnoughAgates;

	private List<CTile> TileRowsToMove = new List<CTile>();

	private List<IceMoveParams> movementBeyondCTiles = new List<IceMoveParams>();

	private List<int> PickedRowSpeedsThisRound = new List<int>();

	public bool startedIceRowMovement;

	public Dictionary<CObjectProp, IceMoveParams> movements = new Dictionary<CObjectProp, IceMoveParams>();

	[StateDump(Skip = true)]
	private bool? waterIsCurrentlyHazardous;

	private float tileWidth => CActor.s_LOSTileScalar.X;

	protected override bool Initialize()
	{
		CarrySunAgates = GetObjective<CObjective_Scripted>("CarrySunAgates");
		CarrySunAgates.Total = ScenarioScript.PartySize * 2;
		CarrySunAgates.CustomLocKeyArgs = new List<object> { CarrySunAgates.Total };
		FailIfNotEnoughAgates = GetObjective<CObjective_Scripted>("FailIfNotEnoughAgates");
		FailIfNotEnoughAgates.Total = 1;
		bTiles.Add(ScenarioManager.GetTile(9, 8));
		bTiles.Add(ScenarioManager.GetTile(9, 7));
		bTiles.Add(ScenarioManager.GetTile(9, 6));
		cTiles.Add(ScenarioManager.GetTile(2, 8));
		cTiles.Add(ScenarioManager.GetTile(2, 7));
		cTiles.Add(ScenarioManager.GetTile(2, 6));
		initialIceTiles.Add(ScenarioManager.GetTile(4, 7));
		initialIceTiles.Add(ScenarioManager.GetTile(6, 6));
		initialIceTiles.Add(ScenarioManager.GetTile(7, 8));
		initialAgateTiles.Add(ScenarioManager.GetTile(9, 10));
		initialAgateTiles.Add(ScenarioManager.GetTile(3, 5));
		initialAgateTiles.Add(ScenarioManager.GetTile(9, 4));
		initialAgateTiles.Add(ScenarioManager.GetTile(5, 8));
		initialAgateTiles.Add(ScenarioManager.GetTile(6, 1));
		initialAgateTiles.Add(ScenarioManager.GetTile(12, 6));
		initialAgateTiles.Add(ScenarioManager.GetTile(8, 6));
		initialAgateTiles.Add(ScenarioManager.GetTile(2, 3));
		initialAgateTiles.Add(ScenarioManager.GetTile(5, 3));
		initialAgateTiles.Add(ScenarioManager.GetTile(2, 10));
		initialAgateTiles.Add(ScenarioManager.GetTile(3, 1));
		initialAgateTiles.Add(ScenarioManager.GetTile(6, 9));
		FHScenarioSetup scenarioSetup = ScenarioScript._css.ScenarioSetup;
		if (scenarioSetup.ParseScenariosConclusionParameters(out var all))
		{
			if (scenarioSetup.GetConclusionParameters(129u, all, out var result))
			{
				agatesFrom129 = Convert.ToInt32(result[0]);
			}
			else
			{
				Error("Need remaining agates to be passed down from scenario 129 as Conclusion Parameters. Will use defaults....", null, 58);
				agatesFrom129 = 12;
			}
		}
		else
		{
			Log.Warning("Need remaining agates to be passed down from scenario 129 as Conclusion Parameters. Will use defaults....");
			agatesFrom129 = 12;
		}
		return base.Initialize();
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		InitWaterTiles(waterIsHazardous: false);
	}

	protected override int GetConclusionIndexInternal()
	{
		foreach (PlayerState player in ScenarioManager.CurrentScenarioState.Players)
		{
			if (player.ClassID == "FrozenFistID" || player.ClassID == "SnowdancerID")
			{
				return 1;
			}
		}
		return 0;
	}

	protected override List<object> GetConclusionParametersInternal()
	{
		int num = CarrySunAgates.Current;
		return new List<object> { num };
	}

	private IEnumerable<CObjectProp> GetPropsToCarry(CTile tile, CObjectProp iceProp)
	{
		return tile.m_Props.Where((CObjectProp x) => x == iceProp || IsValidPropForWaterRowMove(x));
	}

	private bool IsValidPropForWaterRowMove(CObjectProp prop)
	{
		if (prop.ObjectType != ScenarioManager.ObjectImportType.MoneyToken)
		{
			return prop.ObjectType == ScenarioManager.ObjectImportType.Resource;
		}
		return true;
	}

	private static CTile GetLeftNeighbor(CTile tile, int dist)
	{
		for (int i = 0; i < dist; i++)
		{
			tile = ScenarioManager.GetAdjacentTile(tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y, ScenarioManager.EAdjacentPosition.ELeft);
		}
		return tile;
	}

	protected override void TickPhaseInternal()
	{
		base.TickPhaseInternal();
		switch (_phase)
		{
		case EScenarioModifierTriggerPhase.StartScenario:
		{
			foreach (CTile initialIceTile in initialIceTiles)
			{
				SpawnIce(initialIceTile, null);
			}
			int num = 0;
			{
				foreach (CTile initialAgateTile in initialAgateTiles)
				{
					SpawnSunAgate(initialAgateTile);
					num++;
					if (num >= agatesFrom129)
					{
						break;
					}
				}
				break;
			}
		}
		case EScenarioModifierTriggerPhase.StartRound:
		{
			if (!OncePerRound())
			{
				break;
			}
			foreach (CTile bTile in bTiles)
			{
				if (!bTile.m_Props.Any((CObjectProp x) => x.PrefabName == IceProp))
				{
					SpawnIce(bTile, null);
				}
			}
			movements.Clear();
			PickedRowSpeedsThisRound.Clear();
			InitWaterTiles(waterIsHazardous: false, force: true);
			UpdateLootObjective();
			List<CTile> source = (from x in ScenarioManager.CurrentScenarioState.Props
				where x.PrefabName == IceProp
				select x.PropTile).ToList();
			TileRowsToMove.Clear();
			TileRowsToMove.AddRange(source.OrderBy((CTile x) => x.m_ArrayIndex.X));
			ChooseIceTileSpeed();
			break;
		}
		case EScenarioModifierTriggerPhase.EndTurnLoot:
		{
			UpdateLootObjective();
			CActor tickActor = _tickActor;
			if (tickActor.AllowEscape && ScenarioManager.CurrentScenarioState.IsOnEscapeZone(tickActor) && GameState.ShowGenericConfirmButton(tickActor, "FH_Scenario_Campaign_130_EscapeWarning", "GUI_ESCAPE", null, "FH_Scenario_Campaign_130_EscapeCancel", showHeader: false, new object[1] { GetSunAgates(tickActor as CPlayerActor).ToString() }))
			{
				TryEscapeMap();
			}
			break;
		}
		}
	}

	private void ChooseIceTileSpeed()
	{
		CPlayerActor hostCharacter = GameState.HostCharacter;
		if (hostCharacter == null)
		{
			return;
		}
		_ = from x in ScenarioManager.CurrentScenarioState.Props
			where x.PropType == EPropType.TerrainWater
			select x.PropTile;
		using (List<CTile>.Enumerator enumerator = TileRowsToMove.ToList().GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				CTile tile = enumerator.Current;
				_ = tile;
				new List<CTile>();
				List<CObjectProp> list = ScenarioManager.CurrentScenarioState.Props.Where((CObjectProp x) => x.PrefabName == IceProp && x.ArrayIndex.Y == tile.m_ArrayIndex.Y).ToList();
				foreach (CObjectProp item in list)
				{
					TileRowsToMove.Remove(item.PropTile);
				}
				CAbilityChooseAbility cAbilityChooseAbility = CAbility.CreateAbility(CAbility.EAbilityType.Choose, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), isMonster: false, isTargetedAbility: false) as CAbilityChooseAbility;
				cAbilityChooseAbility.Name = "ScenarioScript_FH130_IceRowSpeedChoice";
				int[] array = new int[3] { 1, 2, 3 };
				List<CAbility> list2 = new List<CAbility>();
				int[] array2 = array;
				for (int num = 0; num < array2.Length; num++)
				{
					int num2 = array2[num];
					if (!PickedRowSpeedsThisRound.Contains(num2))
					{
						string text = num2.ToString();
						string previewEffectText = text;
						int strength = num2;
						string name = text;
						CAbility cAbility = CAbility.CreateAbility(CAbility.EAbilityType.Null, strength, useSpecialBaseStat: false, 1, 1, 0, CAbility.EAbilityFlag.None, CAbilityFilterContainer.CreateDefaultFilter(), null, null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, null, null, null, null, null, name, null, null, null, multiPassAttack: false, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, null, textOnly: false, noLayoutSpace: false, showRange: false, showTarget: false, showArea: false, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, null, null, CAbility.EAbilityTargeting.All, null, null, null, null, null, isMonster: false, null, null, isSubAbility: false, isInlineSubAbility: false, 0, 0, isTargetedAbility: true, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, null, null, 0, null, null, skipAnim: false, 0, EConditionDecTrigger.None, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, null, 0, null, null, null, null, CAbility.EAttackType.None, null, null, CAbility.ECardAction.None, null, null, CAbilityExtraTurn.EExtraTurnType.None, null, null, null, null, null, null, null, null, immuneToSelfDamage: false, null, null, null, null, null, null, ECharacter.None, CAbilityFilter.EFilterTile.None, null, isDefault: false, isItemAbility: false, showElementPicker: false, isModifierAbility: false, null, null, previewEffectText, null, exploitVulnerability: false, null, null, int.MaxValue);
						if (cAbility.MiscAbilityData == null)
						{
							cAbility.MiscAbilityData = new AbilityData.MiscAbilityData();
						}
						cAbility.MiscAbilityData.AutotriggerAbility = true;
						list2.Add(cAbility);
					}
				}
				cAbilityChooseAbility.MiscAbilityData = new AbilityData.MiscAbilityData
				{
					HasCondition = false,
					CanSkip = false,
					CanUndo = false,
					IgnoreStun = true
				};
				cAbilityChooseAbility.ChooseAbilities = list2;
				cAbilityChooseAbility.SetMiscVar("DontPerformChosenAbility", true);
				List<object> val = ((IEnumerable<CObjectProp>)list).Select((Func<CObjectProp, object>)((CObjectProp x) => x.PropTile)).ToList();
				cAbilityChooseAbility.Vars = new List<(string, TValue)>
				{
					("IceTile", new TValue(tile)),
					("ForcePreviewTiles", new TValue(val))
				};
				cAbilityChooseAbility.PerformVars = new List<(string, List<(string, TValue)>)> { ("ApplyToActors", new List<(string, TValue)> { ("MoveIceTileRow", new TValue(CExpression.Parse("AbilityHasHappened || ScenarioScript.current.MoveIceTileRow(IceTile, ChosenAbility?.Strength || 0)"))) }) };
				cAbilityChooseAbility.HelpBoxTooltipLocKey = "$FH_Scenario_Campaign_130_SCENARIO_PICK_ICE_MOVE_HELPTEXT$";
				PhaseManager.StartAbilities(cAbilityChooseAbility, hostCharacter, stackInsteadOfInject: true);
				return;
			}
		}
		OnFinishAllIceRowsMovement();
	}

	private void OnFinishAllIceRowsMovement()
	{
		movements.Clear();
		PickedRowSpeedsThisRound.Clear();
		InitWaterTiles(waterIsHazardous: false, force: true);
		UpdateLootObjective();
	}

	private CObjectProp SpawnIce(CTile tile, CActor owner)
	{
		TileIndex arrayIndex = new TileIndex(tile.m_ArrayIndex);
		CObjectProp cObjectProp = new CObjectProp(IceProp, ScenarioManager.ObjectImportType.TerrainVisualEffect, arrayIndex, null, null, owner, tile.m_HexMap.MapGuid);
		tile.SpawnProp(cObjectProp);
		return cObjectProp;
	}

	private void InitWaterTiles(bool waterIsHazardous, bool force = false)
	{
		if (waterIsCurrentlyHazardous.HasValue && waterIsCurrentlyHazardous.Value == waterIsHazardous && !force)
		{
			return;
		}
		waterIsCurrentlyHazardous = waterIsHazardous;
		IEnumerable<CTile> enumerable = from x in ScenarioManager.CurrentScenarioState.Props
			where x.PropType == EPropType.TerrainWater
			select x.PropTile;
		IEnumerable<CTile> enumerable2 = from x in ScenarioManager.CurrentScenarioState.Props
			where x.PrefabName == IceProp
			select x.PropTile;
		foreach (CTile item in enumerable)
		{
			item.TreatAsWater = true;
			item.TreatAsHazardous = waterIsHazardous;
			item.TreatAsIce = false;
			CNode node = ScenarioManager.PathFinder.GetNode(item.m_ArrayIndex);
			node.IsDifficultTerrain = true;
			node.IsNegativeHex = waterIsHazardous;
		}
		foreach (CTile item2 in enumerable2)
		{
			item2.TreatAsWater = false;
			item2.TreatAsHazardous = false;
			item2.TreatAsIce = true;
			CNode node2 = ScenarioManager.PathFinder.GetNode(item2.m_ArrayIndex);
			node2.IsDifficultTerrain = false;
			node2.IsNegativeHex = false;
		}
	}

	private void UpdateLootObjective()
	{
		CarrySunAgates.Current = ScenarioManager.Scenario.AllPlayers.Sum((CPlayerActor x) => GetSunAgates(x));
		CarrySunAgates.ScriptedIsComplete = CarrySunAgates.Current >= CarrySunAgates.Total;
		CarrySunAgates.ScriptedUnableToComplete = false;
		FailIfNotEnoughAgates.Current = 0;
		FailIfNotEnoughAgates.ScriptedIsComplete = false;
		int num = ScenarioManager.CurrentScenarioState.Props.Count((CObjectProp x) => x.ObjectType == ScenarioManager.ObjectImportType.Resource);
		if (CarrySunAgates.Current + num < CarrySunAgates.Total || (ScenarioManager.Scenario.PlayerActors.Count == 0 && !CarrySunAgates.ScriptedIsComplete))
		{
			CarrySunAgates.ScriptedUnableToComplete = true;
			FailIfNotEnoughAgates.Current = 1;
			FailIfNotEnoughAgates.ScriptedIsComplete = true;
		}
		ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
	}

	protected override void TickRepositionInternal()
	{
		base.TickRepositionInternal();
		CActor tickActor = _tickActor;
		bool flag = _tickAbility is CAbilityMove cAbilityMove && cAbilityMove.Jump;
		bool flyingAfterConditions = tickActor.FlyingAfterConditions;
		if (tickActor.Tile.TreatAsHazardous && tickActor.Tile.IsWater && !flag && !flyingAfterConditions)
		{
			TryDealHazardousDamage(tickActor, null);
		}
	}

	private void TryDealHazardousDamage(CActor actor, string damageSourceLocKey, bool includeHeroSummons = false)
	{
		if (actor is CPlayerActor || (includeHeroSummons && actor is CHeroSummonActor))
		{
			DealScenarioDamage(actor, ScenarioManager.HazardousTerrainDamage, isTrapDamage: false, isTerrainDamage: false, damageSourceLocKey);
		}
	}

	private void TryDealTrapDamage(CActor actor, string damageSourceLocKey, bool includeHeroSummons = false)
	{
		if (actor is CPlayerActor || (includeHeroSummons && actor is CHeroSummonActor))
		{
			DealScenarioDamage(actor, ScenarioManager.TrapDamage, isTrapDamage: false, isTerrainDamage: false, damageSourceLocKey);
		}
	}

	private void SpawnSunAgate(CTile tile)
	{
		CharacterResourceData characterResourceData = ScenarioRuleClient.SRLYML.CharacterResources.SingleOrDefault((CharacterResourceData x) => x.ID == "FH_Scenario_Campaign_130_SCENARIO_OBJECT_1");
		CObjectResource cObjectResource = new CObjectResource(characterResourceData, characterResourceData.ResourceModel, ScenarioManager.ObjectImportType.Resource, new TileIndex(tile.m_ArrayIndex), null, null, null, tile.m_HexMap.MapGuid);
		cObjectResource.PropCustomLocKey = "FH_130_Sun_Agate_T";
		cObjectResource.PropCustomDescriptionLocKey = "FH_130_Sun_Agate_D";
		tile.SpawnProp(cObjectResource);
		IEnumerable<CTile> source = from x in ScenarioManager.CurrentScenarioState.Props
			where x.PropType == EPropType.TerrainWater
			select x.PropTile;
		if (source.Contains(tile) && movements.Any((KeyValuePair<CObjectProp, IceMoveParams> x) => x.Value.firstTile.m_ArrayIndex.Y == tile.m_ArrayIndex.Y))
		{
			int desiredDist = 0;
			foreach (IceMoveParams value in movements.Values)
			{
				CTile propTile = value.iceProp.PropTile;
				CTile destTile = value.destTile;
				desiredDist = value.desiredDist;
				if (tile.m_ArrayIndex.X <= propTile.m_ArrayIndex.X && tile.m_ArrayIndex.X > destTile.m_ArrayIndex.X && value.isIceMovement)
				{
					value.propsToMove.Add(cObjectResource);
					return;
				}
				if (destTile.m_ArrayIndex.X == tile.m_ArrayIndex.X)
				{
					return;
				}
			}
			IceMoveParams iceMoveParams2 = new IceMoveParams();
			iceMoveParams2.firstTile = cObjectResource.PropTile;
			iceMoveParams2.iceProp = cObjectResource;
			iceMoveParams2.desiredDist = desiredDist;
			iceMoveParams2.propsToMove.Add(cObjectResource);
			CTile cTile = cObjectResource.PropTile;
			for (int num = 0; num < iceMoveParams2.desiredDist; num++)
			{
				CTile cTile2 = cTile;
				cTile = ScenarioManager.GetAdjacentTile(cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y, ScenarioManager.EAdjacentPosition.ELeft);
				if (cTile == null || !source.Contains(cTile))
				{
					cTile = cTile2;
					break;
				}
				iceMoveParams2.dist = num + 1;
			}
			movements[cObjectResource] = iceMoveParams2;
		}
		UpdateLootObjective();
	}

	public override string PropKeyOverride(CObjectProp prop, out string[] formatParameters)
	{
		if (prop is CObjectResource)
		{
			int num = prop.PropTile.m_Props.Count((CObjectProp x) => x.ObjectType == ScenarioManager.ObjectImportType.Resource);
			formatParameters = new string[1] { num.ToString() };
			if (num == 1)
			{
				return "FH/FH_Scenario_Campaign_130_SCENARIO_OBJECT_1_Single";
			}
			return "FH/FH_Scenario_Campaign_130_SCENARIO_OBJECT_1_Multiple";
		}
		return base.PropKeyOverride(prop, out formatParameters);
	}

	protected override void TickTakenDamageInternal(int damageTaken, int damageReducedByShields, int actualDamage)
	{
		base.TickTakenDamageInternal(damageTaken, damageReducedByShields, actualDamage);
		if (damageTaken > 0)
		{
			CActor tickActor = _tickActor;
			if (tickActor.GetCharacterResourceAmount("FH_Scenario_Campaign_130_SCENARIO_OBJECT_1") > 0)
			{
				isPickingWhereToDropAgateFromDamage = true;
				CAbility cAbility = CAbility.CreateAbility(CAbility.EAbilityType.NullHex, new CAbilityFilterContainer(), isMonster: false, isTargetedAbility: true);
				cAbility.Name = "DropSunAgatesFromDamage";
				cAbility.MiscAbilityData = new AbilityData.MiscAbilityData();
				cAbility.MiscAbilityData.CanUndo = false;
				cAbility.MiscAbilityData.CanSkip = false;
				cAbility.MiscAbilityData.IgnoreStun = true;
				cAbility.RangeIsBase = true;
				cAbility.AnimOverload = "UseItem";
				cAbility.SetCanUndo(canUndo: false);
				cAbility.SetCanSkip(canSkip: false);
				cAbility.HelpBoxTooltipLocKey = "FH_Scenario_Campaign_130_SCENARIO_PICK_AGATE_DROP_HELPTEXT";
				cAbility.Targeting = CAbility.EAbilityTargeting.Custom;
				cAbility.TargetingVars = new List<(string, TValue)> { ("AddTilesInRange", new TValue(CExpression.Parse("ScenarioScript_FH130.ValidDropSunAgatePositions(TargetingActor)"))) };
				cAbility.PerformVars = new List<(string, List<(string, TValue)>)> { ("OnAbilityEnded", new List<(string, TValue)>
				{
					("DropAgates", new TValue(CExpression.Parse("ScenarioScript_FH130.DropSunAgate(obj, TilesSelected)"))),
					("ContinueIceRowMovement", new TValue(CExpression.Parse("ScenarioScript.current.ContinueIceRowMovement()")))
				}) };
				cAbility.SetMiscVar("InterruptCurrentAbility", true);
				PhaseManager.StartAbilities(cAbility, tickActor, stackInsteadOfInject: true, tickActor);
			}
		}
	}

	public static List<CTile> ValidDropSunAgatePositions(CActor actor)
	{
		ScenarioScript_FH130 scenarioScript_FH = ScenarioScript.current as ScenarioScript_FH130;
		List<CTile> tilesInRange = GameState.GetTilesInRange(actor.ArrayIndex, 1);
		tilesInRange.Remove(actor.Tile);
		tilesInRange.RemoveAll((CTile x) =>
		{
			CNode node = ScenarioManager.PathFinder.GetNode(x.m_ArrayIndex);
			return !node.Walkable || node.Blocked;
		});
		List<CTile> list = tilesInRange.Where((CTile x) => x.IsWater || x.IsIce).ToList();
		List<CTile> list2 = ((list.Count <= 0) ? tilesInRange : list);
		if (list2 == null || list2.Count == 0)
		{
			return new List<CTile>();
		}
		CTile closestCTile = scenarioScript_FH.cTiles.OrderBy((CTile x) => actor.GetDistanceTo(x)).FirstOrDefault();
		CTile cTile = list2.OrderBy((CTile x) => Point.HexDist(closestCTile.m_ArrayIndex, x.m_ArrayIndex)).FirstOrDefault();
		int distToClosestCTile = Point.HexDist(closestCTile.m_ArrayIndex, cTile.m_ArrayIndex);
		list2.RemoveAll((CTile x) => Point.HexDist(closestCTile.m_ArrayIndex, x.m_ArrayIndex) != distToClosestCTile);
		return list2;
	}

	public static void DropSunAgate(CAbility ability, List<CTile> selectedTiles)
	{
		ScenarioScript_FH130 scenarioScript_FH = ScenarioScript.current as ScenarioScript_FH130;
		CActor targetingActor = ability.TargetingActor;
		int num = 1;
		if (ability is CAbilityChooseAbility cAbilityChooseAbility)
		{
			num = cAbilityChooseAbility.ChosenAbility.Strength;
		}
		targetingActor.RemoveCharacterResource("FH_Scenario_Campaign_130_SCENARIO_OBJECT_1", num);
		for (int i = 0; i < num; i++)
		{
			scenarioScript_FH.SpawnSunAgate(selectedTiles[0]);
		}
		if (scenarioScript_FH.isPickingWhereToDropAgateFromDamage)
		{
			scenarioScript_FH.isPickingWhereToDropAgateFromDamage = false;
			return;
		}
		CActiveBonus cActiveBonus = ability.GetVar("CacheActiveBonus").GetObject<CActiveBonus>();
		if (cActiveBonus != null)
		{
			cActiveBonus.ToggleActiveBonusFromVars(targetingActor, markCostAbilityAsExecuted: true);
			CustomAbility.PayMovementPoints(0, targetingActor, cActiveBonus);
		}
	}

	public void MoveIceTileRow(CTile tile, int dist)
	{
		PickedRowSpeedsThisRound.Add(dist);
		CAbility cAbility = PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility.m_Ability;
		if (cAbility != null && cAbility.Name == "ScenarioScript_FH130_IceRowSpeedChoice")
		{
			cAbility.Vars.RemoveAll(((string, TValue) x) => x.Item1 == "ForcePreviewTiles");
			if (cAbility.VarsContext?.obj is CVars cVars)
			{
				cVars.Set("ForcePreviewTiles", TValue.Unknown);
			}
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
		}
		IEnumerable<CTile> waterTiles = from x in ScenarioManager.CurrentScenarioState.Props
			where x.PropType == EPropType.TerrainWater
			select x.PropTile;
		List<CObjectProp> icePropsInRow = ScenarioManager.CurrentScenarioState.Props.Where((CObjectProp x) => x.PrefabName == IceProp && x.ArrayIndex.Y == tile.m_ArrayIndex.Y).ToList();
		movements.Clear();
		movementBeyondCTiles.Clear();
		foreach (CObjectProp item in icePropsInRow)
		{
			IceMoveParams iceMoveParams = new IceMoveParams();
			iceMoveParams.firstTile = item.PropTile;
			iceMoveParams.iceProp = item;
			iceMoveParams.actorsToMove.AddRange(item.PropTile.LivingActors.Where((CActor x) => !(x is CEnemyActor)));
			iceMoveParams.propsToMove.AddRange(GetPropsToCarry(item.PropTile, item));
			iceMoveParams.isIceMovement = true;
			iceMoveParams.desiredDist = dist;
			movements[item] = iceMoveParams;
		}
		foreach (CObjectProp item2 in icePropsInRow.OrderBy((CObjectProp x) => x.ArrayIndex.X))
		{
			IceMoveParams iceMoveParams2 = movements[item2];
			int num = iceMoveParams2.desiredDist;
			CTile cTile = item2.PropTile;
			for (int num2 = 0; num2 < num; num2++)
			{
				CTile cTile2 = cTile;
				cTile = ScenarioManager.GetAdjacentTile(cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y, ScenarioManager.EAdjacentPosition.ELeft);
				if (cTile == null || !waterTiles.Contains(cTile))
				{
					num = num2 + 1;
					cTile = cTile2;
					movementBeyondCTiles.Add(iceMoveParams2);
					break;
				}
				CActor actorOnTile = cTile.LivingActor;
				if (actorOnTile != null && !movements.Values.Any((IceMoveParams x) => x.actorsToMove.Contains(actorOnTile) || x.actorsMightMove.Contains(actorOnTile)))
				{
					if (num2 == num - 1)
					{
						num++;
					}
					else if (!(actorOnTile is CEnemyActor))
					{
						iceMoveParams2.actorsMightMove.Add(actorOnTile);
					}
				}
				if (num2 < num - 1)
				{
					IEnumerable<CObjectProp> collection = from x in GetPropsToCarry(cTile, item2)
						where !movements.Any((KeyValuePair<CObjectProp, IceMoveParams> move) => move.Value.propsToMove.Contains(x))
						select x;
					iceMoveParams2.propsToMove.AddRange(collection);
				}
				iceMoveParams2.dist = num2 + 1;
			}
		}
		bool flag = false;
		do
		{
			flag = false;
			foreach (IceMoveParams movement in movements.Values.OrderByDescending((IceMoveParams x) => x.iceProp.PropTile.m_ArrayIndex.X))
			{
				CTile destTile = movement.destTile;
				if (movements.Values.Where((IceMoveParams x) => x != movement && x.destTile == destTile).Any())
				{
					CTile leftNeighbor = GetLeftNeighbor(destTile, 1);
					if (leftNeighbor != null && waterTiles.Contains(leftNeighbor))
					{
						movement.dist++;
						flag = true;
					}
				}
			}
		}
		while (flag);
		List<CObjectProp> list = ScenarioManager.CurrentScenarioState.Props.Where((CObjectProp x) => x.ArrayIndex.Y == tile.m_ArrayIndex.Y && !icePropsInRow.Contains(x) && IsValidPropForWaterRowMove(x) && waterTiles.Contains(x.PropTile)).ToList();
		foreach (CObjectProp otherProp in list)
		{
			if (!movements.Values.Any((IceMoveParams iceMoveParams4) => iceMoveParams4.destTile == otherProp.PropTile || iceMoveParams4.propsToMove.Contains(otherProp)))
			{
				IceMoveParams iceMoveParams3 = new IceMoveParams();
				iceMoveParams3.firstTile = otherProp.PropTile;
				iceMoveParams3.iceProp = otherProp;
				iceMoveParams3.desiredDist = dist;
				iceMoveParams3.propsToMove.Add(otherProp);
				movements[otherProp] = iceMoveParams3;
			}
		}
		foreach (CObjectProp item3 in list.OrderBy((CObjectProp x) => x.ArrayIndex.X))
		{
			if (!movements.TryGetValue(item3, out var value))
			{
				continue;
			}
			int desiredDist = value.desiredDist;
			CTile cTile3 = item3.PropTile;
			for (int num3 = 0; num3 < desiredDist; num3++)
			{
				CTile cTile4 = cTile3;
				cTile3 = ScenarioManager.GetAdjacentTile(cTile3.m_ArrayIndex.X, cTile3.m_ArrayIndex.Y, ScenarioManager.EAdjacentPosition.ELeft);
				if (cTile3 == null || !waterTiles.Contains(cTile3))
				{
					desiredDist = num3 + 1;
					cTile3 = cTile4;
					movementBeyondCTiles.Add(value);
					break;
				}
				value.dist = num3 + 1;
			}
		}
		startedIceRowMovement = true;
		ContinueIceRowMovement();
	}

	public void ContinueIceRowMovement()
	{
		GameState.ThreadIsSleeping = true;
		while (startedIceRowMovement)
		{
			Thread.Sleep(100);
			foreach (IceMoveParams value in movements.Values)
			{
				lock (value.actorsToDamageHazardous)
				{
					foreach (CActor actor in value.actorsToDamageHazardous)
					{
						if (!movementBeyondCTiles.Any((IceMoveParams x) => x.actorsToMove.Contains(actor)))
						{
							TryDealTrapDamage(actor, null, includeHeroSummons: true);
						}
					}
					value.actorsToDamageHazardous.Clear();
				}
			}
			if (isPickingWhereToDropAgateFromDamage)
			{
				break;
			}
		}
		GameState.ThreadIsSleeping = false;
	}

	private void CheckCTiles()
	{
		foreach (IceMoveParams movementBeyondCTile in movementBeyondCTiles)
		{
			CTile cTile = null;
			int num = 0;
			foreach (CObjectProp item in movementBeyondCTile.propsToMove)
			{
				if (item is CObjectResource || item.ObjectType == ScenarioManager.ObjectImportType.MoneyToken || item.PrefabName == IceProp)
				{
					if (cTile == null || cTile.m_ArrayIndex.X > item.ArrayIndex.X)
					{
						cTile = item.PropTile;
					}
					item.DestroyProp(1.5f);
					if (item is CObjectResource)
					{
						num++;
					}
				}
			}
			if (cTile != null)
			{
				ScenarioRuleClient.MessageHandler(new CTryPlayVfxOnTile_MessageData(null)
				{
					m_ArrayIndex = cTile.m_ArrayIndex,
					m_VFXPropType = EVFXPropType.WaterSplash.ToString()
				});
			}
			if (num > 0)
			{
				ScenarioRuleClient.MessageHandler(new CCombatLogMessage_MessageData
				{
					m_MessageKey = "COMBAT_LOG_SUN_AGATES_LOST",
					m_Params = new List<string> { num.ToString() }
				});
			}
			foreach (CActor actor in movementBeyondCTile.actorsToMove)
			{
				List<Point> list = new List<Point>();
				ScenarioManager.PathFinder.FloodFill_ToRange(actor, actor.ArrayIndex, 99, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: true, ignoreHazardousTerrain: true, 0, null, null, list, null, ignoreDoorLocks: false);
				CTile cTile2 = (from x in list
					where CAbilityFilter.IsTileEmpty(ScenarioManager.GetTile(x))
					orderby actor.GetDistanceTo(x)
					select ScenarioManager.GetTile(x)).FirstOrDefault();
				if (cTile2 != null)
				{
					CustomAbility.PushActorTo(actor, cTile2, actor.Tile, 0, choreographerWait: false);
					TryDealTrapDamage(actor, null);
				}
			}
		}
		movementBeyondCTiles.Clear();
	}

	public void FinishIceRowMovement()
	{
		new List<CActor>();
		new List<CObjectProp>();
		foreach (IceMoveParams item in from x in movements
			select x.Value into x
			orderby x.firstTile.m_ArrayIndex.X, x.dist
			select x)
		{
			CTile destTile = item.destTile;
			foreach (CObjectProp item2 in item.propsToMove)
			{
				item2.PropTile.DelProp(item2);
				destTile.AddProp(item2);
				item2.SetLocation(new TileIndex(destTile.m_ArrayIndex), null, item2.Rotation);
			}
			foreach (CActor item3 in item.actorsToMove)
			{
				item3.ArrayIndex = destTile.m_ArrayIndex;
			}
		}
		ChooseIceTileSpeed();
		CheckCTiles();
	}

	protected override void TickAbilityStartedInternal()
	{
		base.TickAbilityStartedInternal();
		CAbility tickAbility = _tickAbility;
		if (tickAbility.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA)
		{
			if (tickAbility.TargetingActor is CPlayerActor)
			{
				InitWaterTiles(tickAbility is CAbilityMove || tickAbility is CAbilityTeleport2);
			}
			else
			{
				InitWaterTiles(waterIsHazardous: false);
			}
		}
	}

	protected override void TickAbilityPerformStartedInternal()
	{
		base.TickAbilityPerformStartedInternal();
		CAbility tickAbility = _tickAbility;
		if (tickAbility.ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA)
		{
			return;
		}
		if (tickAbility is CAbilityPull || tickAbility is CAbilityPush)
		{
			if (tickAbility.ValidActorsInRange.Any((CActor x) => x is CPlayerActor))
			{
				InitWaterTiles(waterIsHazardous: true);
			}
		}
		else if (tickAbility is CAbilityMove && tickAbility.TargetingActor is CPlayerActor)
		{
			InitWaterTiles(waterIsHazardous: true);
		}
	}

	protected override void TickAbilityEndedInternal()
	{
		base.TickAbilityEndedInternal();
		CAbility tickAbility = _tickAbility;
		if (tickAbility.GetMiscVar("ContinueIceRowMovement").GetBool())
		{
			FinishIceRowMovement();
		}
		else if (tickAbility is CAbilityMove cAbilityMove)
		{
			bool num = cAbilityMove?.Jump ?? false;
			bool flyingAfterConditions = cAbilityMove.TargetingActor.FlyingAfterConditions;
			if (num && !flyingAfterConditions && cAbilityMove.TargetingActor.Tile.TreatAsHazardous)
			{
				TryDealHazardousDamage(cAbilityMove.TargetingActor, null);
			}
		}
		InitWaterTiles(waterIsHazardous: false);
		UpdateLootObjective();
	}

	protected override void TickAbilityUndoneInternal()
	{
		base.TickAbilityUndoneInternal();
		InitWaterTiles(waterIsHazardous: false);
	}

	public static int GetSunAgates(CPlayerActor actor)
	{
		return actor.GetCharacterResourceAmount("FH_Scenario_Campaign_130_SCENARIO_OBJECT_1");
	}

	private static void OverrideDropSunAgateAbility(CActor actor, CActiveBonus ActiveBonus)
	{
		if (!(actor is CPlayerActor actor2) || !(ActiveBonus.Ability is CAbilityAddActiveBonus cAbilityAddActiveBonus))
		{
			return;
		}
		int sunAgates = GetSunAgates(actor2);
		CAbilityChooseAbility cAbilityChooseAbility = CAbility.CreateAbility(CAbility.EAbilityType.Choose, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), isMonster: false, isTargetedAbility: false) as CAbilityChooseAbility;
		cAbilityChooseAbility.Name = "ScenarioScript_FH130_DropAgatesChoice";
		List<CAbility> list = new List<CAbility>();
		for (int i = 1; i <= sunAgates; i++)
		{
			string text = i.ToString();
			string previewEffectText = text;
			int strength = i;
			string name = text;
			CAbility cAbility = CAbility.CreateAbility(CAbility.EAbilityType.Null, strength, useSpecialBaseStat: false, 1, 1, 0, CAbility.EAbilityFlag.None, CAbilityFilterContainer.CreateDefaultFilter(), null, null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, null, null, null, null, null, name, null, null, null, multiPassAttack: false, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, null, textOnly: false, noLayoutSpace: false, showRange: false, showTarget: false, showArea: false, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, null, null, CAbility.EAbilityTargeting.All, null, null, null, null, null, isMonster: false, null, null, isSubAbility: false, isInlineSubAbility: false, 0, 0, isTargetedAbility: true, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, null, null, 0, null, null, skipAnim: false, 0, EConditionDecTrigger.None, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, null, 0, null, null, null, null, CAbility.EAttackType.None, null, null, CAbility.ECardAction.None, null, null, CAbilityExtraTurn.EExtraTurnType.None, null, null, null, null, null, null, null, null, immuneToSelfDamage: false, null, null, null, null, null, null, ECharacter.None, CAbilityFilter.EFilterTile.None, null, isDefault: false, isItemAbility: false, showElementPicker: false, isModifierAbility: false, null, null, previewEffectText, null, exploitVulnerability: false, null, null, int.MaxValue);
			if (cAbility.MiscAbilityData == null)
			{
				cAbility.MiscAbilityData = new AbilityData.MiscAbilityData();
			}
			cAbility.MiscAbilityData.AutotriggerAbility = true;
			list.Add(cAbility);
		}
		cAbilityChooseAbility.MiscAbilityData = new AbilityData.MiscAbilityData
		{
			HasCondition = false,
			CanSkip = false,
			CanUndo = false
		};
		cAbilityChooseAbility.ChooseAbilities = list;
		cAbilityChooseAbility.SetMiscVar("DontPerformChosenAbility", true);
		cAbilityChooseAbility.PerformVars = new List<(string, List<(string, TValue)>)> { ("ApplyToActors", new List<(string, TValue)> { ("DropSunAgate", new TValue(CExpression.Parse("ScenarioScript_FH130.DropSunAgate(obj, TilesSelected)"))) }) };
		cAbilityChooseAbility.HelpBoxTooltipLocKey = "FH_Scenario_Campaign_130_SCENARIO_SCENARIO_MODIFIER_2";
		cAbilityAddActiveBonus.AddAbility = cAbilityChooseAbility;
	}
}
