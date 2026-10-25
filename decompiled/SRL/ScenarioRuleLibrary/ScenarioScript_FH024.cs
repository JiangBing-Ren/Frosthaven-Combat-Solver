using System.Collections.Generic;
using System.Linq;
using AStar;
using Libs.GameConsole;
using Newtonsoft.Json;
using ScenarioRuleLibrary.CustomLevels;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH024 : ScenarioScript
{
	[ConsoleVariable(Alias = "ss024_beam_force_effect")]
	public static int BeamForceEffect_cvar;

	[ConsoleVariable(Alias = "ss024_beam_trap_damage")]
	public static int BeamTrapDamage_cvar;

	[ConsoleVariable(Alias = "ss024_beam_skip_effects")]
	public static bool BeamSkipEffects_cvar;

	public static bool PrintBeamDeckOnMove_cvar;

	[ConsoleVariable(Alias = "ss024_show_chaotic_beams")]
	public static bool ShowChaoticBeams_cvar;

	[ConsoleVariable(Alias = "ss024_show_fx_deck")]
	public static int ShowFxDeck_cvar;

	public List<TileIndex> chaoticBeams;

	private List<int> _deck = new List<int>();

	private List<int> _deckSpent = new List<int>();

	private CScenarioModifier _smRoom1;

	private CScenarioModifier _smRoom2;

	private bool[] _specialRulesRevealed = new bool[2];

	[StateDump(Skip = true)]
	private CAbility _lastRepositionAbility;

	private List<CObjectProp> chaoticBeamProps;

	private List<CActor> enemyActorsSpawnedThisStep = new List<CActor>();

	private CObjective_ActorsEscaped _escapeObjective => ScenarioScript._css.WinObjectives.FirstOrDefault((CObjective o) => o.ObjectiveType == EObjectiveType.ActorsEscaped && o.EventIdentifier == "ActorsEscape") as CObjective_ActorsEscaped;

	private CObjective_ActorsEscaped _escapeObjectiveDummy => ScenarioScript._css.WinObjectives.FirstOrDefault((CObjective o) => o.ObjectiveType == EObjectiveType.ActorsEscaped && o.EventIdentifier != "ActorsEscape") as CObjective_ActorsEscaped;

	public List<int> Deck => _deck;

	public List<int> DeckSpend => _deckSpent;

	private int _trapDamage
	{
		get
		{
			if (BeamTrapDamage_cvar <= 0)
			{
				return ScenarioManager.TrapDamage;
			}
			return BeamTrapDamage_cvar;
		}
	}

	protected override bool Initialize()
	{
		ChaoticBeamInit();
		if (!IsLoading)
		{
			TryRefillDeck();
		}
		if (_escapeObjective == null)
		{
			Error("Needs escape objective.", null, 59);
			return false;
		}
		if (_escapeObjectiveDummy == null)
		{
			Error("Needs escape dummy objective.", null, 64);
			return false;
		}
		_smRoom1 = GetScenarioModifier("Room1");
		_smRoom2 = GetScenarioModifier("Room2");
		return true;
	}

	protected override void TickDeathInternal()
	{
		if (_tickActor.Type != CActor.EType.Enemy && _tickActor.Type != CActor.EType.Enemy2)
		{
			return;
		}
		ResetSpawnedEnemiesCache();
		UpdateEscapeObjective();
		Trace($"{_tickActor.Class.ID} is dead: {_tickActor.IsDead}", null, 82);
		Trace(_tickActor.Class.ID + " dies, try to apply chaotic effects and teleport...", null, 83);
		foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
		{
			if (HasChaoticBeam(allAliveActor.ArrayIndex))
			{
				CAbility cAbility = _lastRepositionAbility;
				if (cAbility?.TargetingActor != allAliveActor)
				{
					cAbility = null;
				}
				TryTeleportToNextRoom(cAbility, allAliveActor);
			}
		}
		UpdateChaoticBeamPropStates();
	}

	protected override void TickRepositionInternal()
	{
		_lastRepositionAbility = _tickAbility;
		ChaoticBeamStep(_tickAbility, _tickActor);
		UpdateEscapeObjective();
	}

	protected override void TickAbilityEndedInternal()
	{
		base.TickAbilityEndedInternal();
		if (_tickAbility.AbilityInterruptCopy == null)
		{
			ResetSpawnedEnemiesCache();
		}
	}

	protected override void TickPhaseInternal()
	{
		_lastRepositionAbility = null;
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartScenario:
			_smRoom1.SetHiddenState(shouldSetHidden: true);
			_smRoom2.SetHiddenState(shouldSetHidden: true);
			break;
		case EScenarioModifierTriggerPhase.StartTurn:
			TickPhaseStartTurn();
			break;
		case EScenarioModifierTriggerPhase.EndTurn:
			TryEscapeMap();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 149);
			break;
		case EScenarioModifierTriggerPhase.StartRound:
		case EScenarioModifierTriggerPhase.EndRound:
			break;
		}
	}

	protected override string SerializeInternal()
	{
		string text = JsonConvert.SerializeObject(_deck);
		string text2 = JsonConvert.SerializeObject(_deckSpent);
		string text3 = JsonConvert.SerializeObject(_specialRulesRevealed);
		string text4 = JsonConvert.SerializeObject(new string[3] { text, text2, text3 });
		Trace("[ffa000]Serialize: " + text4 + "[-]", null, 158);
		return text4;
	}

	protected override void DeserializeInternal(string s)
	{
		Trace("[ffa000]Deserialize " + s + "[-]", null, 163);
		if (!string.IsNullOrEmpty(s))
		{
			string[] array = JsonConvert.DeserializeObject<string[]>(s);
			_deck = JsonConvert.DeserializeObject<List<int>>(array[0]);
			_deckSpent = JsonConvert.DeserializeObject<List<int>>(array[1]);
			_specialRulesRevealed = JsonConvert.DeserializeObject<bool[]>(array[2]);
		}
		UpdateEscapeObjective();
	}

	private bool AllMonstersInRoomAreDead(CActor actor)
	{
		string roomName = actor.Tile.m_HexMap?.RoomName ?? actor.Tile.m_Hex2Map?.RoomName;
		return !GetEnemyActorsInRoom(roomName).Any();
	}

	private void TickPhaseStartTurn()
	{
		ResetSpawnedEnemiesCache();
		UpdateEscapeObjective();
		if (_tickActor is CHeroSummonActor && TeleportValid(_tickActor, out var _, out var _))
		{
			Trace($"{_tickActor.Class.ID} tries to focus on a beam tile... num beam tiles: {chaoticBeams.Count}", null, 192);
			if (GetNearestFreeTile(_tickActorTile.m_ArrayIndex.X, _tickActorTile.m_ArrayIndex.Y, out var nearestTile, chaoticBeams) && nearestTile.m_Props.Count > 0)
			{
				CAIFocusOverrideDetails.SetAIFocusOverrideOnTileOrNearestEmpty(_tickActor, nearestTile).TargetGUID = nearestTile.m_Props[0].PropGuid;
				Trace($"...success {_tickActor.Class.ID} focuses on tile {nearestTile.m_ArrayIndex}", null, 197);
			}
			else
			{
				Trace("... failed", null, 199);
			}
		}
	}

	private void GainRegenerate(CActor actor)
	{
		GainPos<CAbilityRegenerate>(actor, CAbility.EAbilityType.Regenerate, CCondition.EPositiveCondition.Regenerate);
	}

	private void GainStrengthen(CActor actor)
	{
		GainPos<CAbilityStrengthen>(actor, CAbility.EAbilityType.Strengthen, CCondition.EPositiveCondition.Strengthen);
	}

	private void GainWard(CActor actor)
	{
		GainPos<CAbilityWard>(actor, CAbility.EAbilityType.Ward, CCondition.EPositiveCondition.Ward);
	}

	private void GainBlessTwice(CActor actor)
	{
		GainPos<CAbilityBless>(actor, CAbility.EAbilityType.Bless, CCondition.EPositiveCondition.Bless);
		GainPos<CAbilityBless>(actor, CAbility.EAbilityType.Bless, CCondition.EPositiveCondition.Bless);
	}

	private void PerformHeal2PlusLSelf(CActor actor)
	{
		CAbilityHeal obj = CAbility.CreateAbility(CAbility.EAbilityType.Heal, CAbilityFilterContainer.CreateDefaultFilter(), actor.Type == CActor.EType.Enemy, isTargetedAbility: true, 2 + ScenarioScript._css.Level) as CAbilityHeal;
		obj.TargetingActor = actor;
		obj.ApplyToActor(actor);
	}

	private void GainWound(CActor actor)
	{
		GainNeg<CAbilityWound>(actor, CAbility.EAbilityType.Wound, CCondition.ENegativeCondition.Wound);
	}

	private void GainBrittle(CActor actor)
	{
		GainNeg<CAbilityBrittle>(actor, CAbility.EAbilityType.Brittle, CCondition.ENegativeCondition.Brittle);
	}

	private void GainMuddle(CActor actor)
	{
		GainNeg<CAbilityMuddle>(actor, CAbility.EAbilityType.Muddle, CCondition.ENegativeCondition.Muddle);
	}

	private bool GetClosestMonster(CActor inActor, out CActor nearestMonster)
	{
		CTile cTile = ScenarioManager.Tiles[inActor.ArrayIndex.X, inActor.ArrayIndex.Y];
		nearestMonster = null;
		int num = 9999999;
		foreach (CEnemyActor enemy in ScenarioManager.Scenario.Enemies)
		{
			CTile cTile2 = ScenarioManager.Tiles[enemy.ArrayIndex.X, enemy.ArrayIndex.Y];
			int tileDistance = ScenarioManager.GetTileDistance(cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y, cTile2.m_ArrayIndex.X, cTile2.m_ArrayIndex.Y);
			if (tileDistance < num)
			{
				num = tileDistance;
				nearestMonster = enemy;
			}
		}
		return nearestMonster != null;
	}

	private bool GetNearestTileToSpawn(CActor movedActor, CAbility ability, out CTile nearestTile)
	{
		Point arrayIndex = movedActor.ArrayIndex;
		CTile cTile = ScenarioManager.Tiles[arrayIndex.X, arrayIndex.Y];
		nearestTile = null;
		int num = 9999999;
		CTile[,] tiles = ScenarioManager.Tiles;
		foreach (CTile cTile2 in tiles)
		{
			if (cTile2 == null)
			{
				continue;
			}
			int tileDistance = ScenarioManager.GetTileDistance(cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y, cTile2.m_ArrayIndex.X, cTile2.m_ArrayIndex.Y);
			if (tileDistance < num && !HasChaoticBeam(cTile2.m_ArrayIndex) && CSpawner.IsEmptyTile(cTile2) && (!(ability is CAbilityMove { AllSelectedPlayerWaypoints: { Count: >0 } allSelectedPlayerWaypoints }) || !(allSelectedPlayerWaypoints[allSelectedPlayerWaypoints.Count - 1].m_ArrayIndex == cTile2.m_ArrayIndex)))
			{
				List<CTile> aIMoveFocusWaypoints = movedActor.AIMoveFocusWaypoints;
				if (aIMoveFocusWaypoints == null || aIMoveFocusWaypoints.Count <= 0 || !(aIMoveFocusWaypoints[aIMoveFocusWaypoints.Count - 1].m_ArrayIndex == cTile2.m_ArrayIndex))
				{
					num = tileDistance;
					nearestTile = cTile2;
				}
			}
		}
		if (nearestTile == null)
		{
			return false;
		}
		return true;
	}

	private void SpawnLivingBones(CActor movedActor, CAbility ability)
	{
		if (!GetNearestTileToSpawn(movedActor, ability, out var nearestTile))
		{
			return;
		}
		CMonsterClass cMonsterClass = MonsterClassManager.Find("LivingBonesID");
		if (cMonsterClass != null)
		{
			int chosenModelIndex = ScenarioScript._css.ScenarioRNG.Next(cMonsterClass.Models.Count);
			int num = cMonsterClass.Health();
			int maxHealth = num;
			int level = 1;
			EnemyState enemyState = new EnemyState("LivingBonesID", chosenModelIndex, null, nearestTile.m_HexMap.MapGuid, new TileIndex(nearestTile.m_ArrayIndex), num, maxHealth, level, new List<PositiveConditionPair>(), new List<NegativeConditionPair>(), playedThisRound: true, CActor.ECauseOfDeath.StillAlive, isSummon: false, null, 1, CActor.EType.Enemy);
			CEnemyActor cEnemyActor = ScenarioManager.Scenario.AddEnemy(enemyState, initial: false, noIDRegen: false, fromSpawnerAtRoundStart: false, resetHealth: false);
			if (cEnemyActor != null)
			{
				enemyActorsSpawnedThisStep.Add(cEnemyActor);
				ScenarioScript._css.Monsters.Add(enemyState);
				GameState.SortIntoInitiativeAndIDOrder();
				CSpawnerSpawningUnit_MessageData message = new CSpawnerSpawningUnit_MessageData
				{
					m_SpawnTile = nearestTile,
					m_SpawnEnemy = cEnemyActor
				};
				ScenarioRuleClient.MessageHandler(message);
				ScenarioScript._css.CheckObjectivesComplete();
				ScenarioScript._css.PerformOnSpawnedModifiers(cEnemyActor, ScenarioScript._css.Players.Count);
			}
		}
	}

	private void ClosestMonsterSuffersTrapDamage(CActor movedActor, CAbility ability)
	{
		if (GetClosestMonster(movedActor, out var nearestMonster))
		{
			if (movedActor == nearestMonster && ability != null && ability is CAbilityMove cAbilityMove)
			{
				cAbilityMove.LocoStop(movedActor.WillActorDie(_trapDamage));
			}
			DealScenarioDamage(nearestMonster, _trapDamage, isTrapDamage: true, isTerrainDamage: false, "FH_Scenario_Campaign_24_DAMAGE_SOURCE", null, null, null, showInflictMessage: false);
		}
	}

	private void SufferTrapDamage(CActor movedActor, CAbility ability)
	{
		if (ability is CAbilityMove cAbilityMove)
		{
			cAbilityMove.LocoStop(movedActor.WillActorDie(_trapDamage));
		}
		DealScenarioDamage(movedActor, _trapDamage, isTrapDamage: true, isTerrainDamage: false, "FH_Scenario_Campaign_24_DAMAGE_SOURCE");
	}

	private bool HasChaoticBeam(Point pt)
	{
		return HasChaoticBeam(pt.X, pt.Y);
	}

	private bool HasChaoticBeam(int x, int y)
	{
		return chaoticBeams.FirstOrDefault((TileIndex b) => b.X == x && b.Y == y) != null;
	}

	private void ChaoticBeamInit()
	{
		chaoticBeams = new List<TileIndex>();
		foreach (CScenarioModifier scenarioModifier in ScenarioScript._css.ScenarioModifiers)
		{
			if (scenarioModifier is CScenarioModifierApplyActiveBonusToActor { TileSpecificAbilityIds: { } tileSpecificAbilityIds } cScenarioModifierApplyActiveBonusToActor && tileSpecificAbilityIds.Keys.Count > 0)
			{
				chaoticBeams = cScenarioModifierApplyActiveBonusToActor.TileSpecificAbilityIds.Keys.ToList();
				break;
			}
		}
		chaoticBeamProps = new List<CObjectProp>();
		foreach (TileIndex item in (IEnumerable<TileIndex>)chaoticBeams)
		{
			CTile cTile = ScenarioManager.Tiles[item.X, item.Y];
			CObjectProp cObjectProp = cTile.FindPropOfTypes(EPropType.TerrainVisualEffect);
			if (cObjectProp != null)
			{
				Trace($"Setting {cObjectProp.PropType} as negative hex.", null, 458);
				cObjectProp.NegativeHex = true;
				cObjectProp.PropHealthDetails.CustomLocKey = "FH_Scenario_Campaign_024_SCENARIO_OBJECT_1";
				cObjectProp.PropCustomDescriptionLocKey = "FH_24_Chaos_Beam_D";
				chaoticBeamProps.Add(cObjectProp);
				ScenarioManager.PathFinder.GetNode(cTile.m_ArrayIndex).IsSuperNegativeHex = true;
			}
		}
		UpdateChaoticBeamPropStates();
	}

	private void UpdateChaoticBeamPropStates()
	{
		foreach (CMap map in ScenarioManager.Scenario.Maps)
		{
			IEnumerable<CObjectProp> enumerable = chaoticBeamProps.Where((CObjectProp x) => x.StartingMapGuid == map.MapGuid);
			bool flag = map.Revealed;
			if (flag)
			{
				foreach (CEnemyActor item in ScenarioManager.Scenario.AllEnemyMonsters.Where((CEnemyActor actor) => actor.Tile?.m_HexMap?.MapGuid == map.MapGuid || actor.Tile?.m_Hex2Map?.MapGuid == map.MapGuid))
				{
					if (!item.IsDead)
					{
						flag = false;
						break;
					}
				}
			}
			foreach (CObjectProp item2 in enumerable)
			{
				CHidePropChild_MessageData cHidePropChild_MessageData = new CHidePropChild_MessageData();
				cHidePropChild_MessageData.m_prop = item2;
				cHidePropChild_MessageData.m_childName = "State1";
				cHidePropChild_MessageData.m_hidden = !flag;
				cHidePropChild_MessageData.m_delay = 0.001f;
				ScenarioRuleClient.MessageHandler(cHidePropChild_MessageData);
			}
		}
	}

	private void TryRefillDeck()
	{
		if (_deck.Count == 0)
		{
			_deckSpent.Clear();
			for (int i = 1; i <= 12; i++)
			{
				_deck.Add(i);
			}
			_deck.Shuffle(ScenarioScript._css.ScenarioRNG);
			Trace("Deck refilled.", null, 499);
		}
	}

	private bool TryApplyChaoticEffect(CAbility ability, CActor actor)
	{
		Point arrayIndex = actor.ArrayIndex;
		if (!HasChaoticBeam(arrayIndex) || BeamSkipEffects_cvar)
		{
			return false;
		}
		Trace("Engaging Chaotic beam...", null, 510);
		TryRefillDeck();
		if (PrintBeamDeckOnMove_cvar)
		{
			string text = "";
			foreach (int item in _deck)
			{
				text = text + item + ", ";
			}
			Trace("deck: " + text, null, 521);
		}
		int num = _deck[0];
		_deck.RemoveAt(0);
		_deckSpent.Add(num);
		int num2 = ((BeamForceEffect_cvar > 0) ? BeamForceEffect_cvar : num);
		Trace($"Chaotic beam effect: {num2} {GetFxName(num2)}", null, 527);
		switch (num2)
		{
		case 1:
			GainRegenerate(actor);
			break;
		case 2:
			GainWard(actor);
			break;
		case 3:
			GainStrengthen(actor);
			break;
		case 4:
			GainBlessTwice(actor);
			break;
		case 5:
			PerformHeal2PlusLSelf(actor);
			break;
		case 6:
			ClosestMonsterSuffersTrapDamage(actor, ability);
			break;
		case 7:
			GainWound(actor);
			break;
		case 8:
			GainBrittle(actor);
			break;
		case 9:
			GainMuddle(actor);
			break;
		case 10:
			GainCurseTwice(actor);
			break;
		case 11:
			SufferTrapDamage(actor, ability);
			break;
		case 12:
			SpawnLivingBones(actor, ability);
			break;
		}
		return true;
	}

	public string GetFxName(int effect)
	{
		return effect switch
		{
			1 => "GainRegenerate", 
			2 => "GainWard", 
			3 => "GainStrengthen", 
			4 => "GainBlessTwice", 
			5 => "PerformHeal2PlusLSelf", 
			6 => "ClosestMonsterSuffersTrapDamage", 
			7 => "GainWound", 
			8 => "GainBrittle", 
			9 => "GainMuddle", 
			10 => "GainCurseTwice", 
			11 => "SufferTrapDamage", 
			12 => "SpawnLivingBones", 
			_ => "", 
		};
	}

	public string GetFxDeckString()
	{
		string text = "";
		foreach (int item in _deckSpent)
		{
			text = text + "[X] " + GetFxName(item) + "\n";
		}
		foreach (int item2 in _deck)
		{
			text = text + GetFxName(item2) + "\n";
		}
		return text;
	}

	private bool TeleportValid(CActor actor, out string link, out CTile nextTile)
	{
		Point arrayIndex = actor.ArrayIndex;
		CTile cTile = ScenarioManager.Tiles[arrayIndex.X, arrayIndex.Y];
		link = null;
		nextTile = null;
		if (cTile.m_HexMap.RoomName == "Room_3")
		{
			link = "Room_2";
		}
		else
		{
			if (!(cTile.m_HexMap.RoomName == "Room_2"))
			{
				link = null;
				return false;
			}
			link = "Room_1";
		}
		string l = link;
		TileIndex tileIndex = chaoticBeams.FirstOrDefault((TileIndex b) => ScenarioManager.Tiles[b.X, b.Y].m_HexMap.RoomName == l && (CSpawner.IsEmptyTile(b.X, b.Y) || !ScenarioManager.Tiles[b.X, b.Y].m_HexMap.Revealed));
		if (tileIndex == null)
		{
			return false;
		}
		nextTile = ScenarioManager.Tiles[tileIndex.X, tileIndex.Y];
		if ((actor.Type == CActor.EType.HeroSummon || !AllMonstersInRoomAreDead(actor)) && !nextTile.m_HexMap.Revealed)
		{
			return false;
		}
		return true;
	}

	private bool TryTeleportToNextRoom(CAbility ability, CActor actor)
	{
		if (!TeleportValid(actor, out var link, out var nextTile))
		{
			return false;
		}
		Trace("Teleporting to the next room " + actor.Class.ID, null, 620);
		CAbilityMove cAbilityMove = ability as CAbilityMove;
		cAbilityMove?.LocoStop();
		if (link == "Room_2")
		{
			if (!_specialRulesRevealed[0])
			{
				_specialRulesRevealed[0] = true;
				_smRoom1.SetHiddenState(shouldSetHidden: false);
			}
		}
		else if (link == "Room_1")
		{
			_smRoom1.SetHiddenState(shouldSetHidden: true);
			if (!_specialRulesRevealed[1])
			{
				_specialRulesRevealed[1] = true;
				_smRoom2.SetHiddenState(shouldSetHidden: false);
			}
		}
		else
		{
			_smRoom2.SetHiddenState(shouldSetHidden: true);
		}
		PlayTeleportChaoticBeamVFX(actor);
		TeleportActor(actor, nextTile, tryReveal: true, cAbilityMove);
		TryApplyChaoticEffect(ability, actor);
		return true;
	}

	private void PlayTeleportChaoticBeamVFX(CActor actor)
	{
		CTile tile = actor.Tile;
		if (chaoticBeamProps.SingleOrDefault((CObjectProp x) => x.PropTile == tile) != null)
		{
			ScenarioRuleClient.MessageHandler(new CTryPlayVfxOnTile_MessageData(null)
			{
				m_VFXPropType = "ChaosBeamTeleport",
				m_ArrayIndex = tile.m_ArrayIndex
			});
		}
	}

	protected override bool AllowTriggerMessageInternal(SEvent ev, CLevelMessage message)
	{
		string text = null;
		string text2 = null;
		if (message.MessageName == "FS_024_Upper_Spire_Special_1_1")
		{
			text = "Room_3";
			text2 = "Room_2";
		}
		else if (message.MessageName == "FS_024_Upper_Spire_Special_2_1")
		{
			text = "Room_2";
			text2 = "Room_1";
		}
		if (text != null)
		{
			bool flag = true;
			foreach (CEnemyActor allMonster in ScenarioManager.Scenario.AllMonsters)
			{
				if (!(allMonster.Tile.m_HexMap.RoomName != text) && !allMonster.IsDead)
				{
					flag = false;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
			foreach (CPlayerActor player in ScenarioManager.Scenario.AllPlayers)
			{
				if (chaoticBeams.FirstOrDefault((TileIndex b) => b.X == player.ArrayIndex.X && b.Y == player.ArrayIndex.Y) != null && player.Tile.m_HexMap.RoomName == text2)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void ResetSpawnedEnemiesCache()
	{
		enemyActorsSpawnedThisStep.Clear();
	}

	private void ChaoticBeamStep(CAbility ability, CActor movedActor, bool afterSpawn = false)
	{
		Trace("Stepping on a Chaotic Beam...", null, 702);
		ResetSpawnedEnemiesCache();
		if (!TryApplyChaoticEffect(ability, movedActor) || !TryTeleportToNextRoom(ability, movedActor))
		{
			return;
		}
		if (afterSpawn && movedActor is CHeroSummonActor cHeroSummonActor)
		{
			cHeroSummonActor.StartArrayIndex = cHeroSummonActor.ArrayIndex;
		}
		foreach (CHeroSummonActor item in ScenarioManager.Scenario.HeroSummons.ToList())
		{
			if (HasChaoticBeam(item.ArrayIndex))
			{
				TryTeleportToNextRoom(null, item);
			}
		}
	}

	private IEnumerable<CEnemyActor> GetEnemyActorsInRoom(string roomName)
	{
		foreach (CEnemyActor allEnemyMonster in ScenarioManager.Scenario.AllEnemyMonsters)
		{
			if (!allEnemyMonster.IsDead && !(allEnemyMonster.Tile.m_HexMap.RoomName != roomName) && !enemyActorsSpawnedThisStep.Contains(allEnemyMonster))
			{
				yield return allEnemyMonster;
			}
		}
	}

	private void UpdateEscapeObjective()
	{
		if (!GetEnemyActorsInRoom("Room_1").Any() && !_escapeObjective.IsActive)
		{
			if (!ScenarioManager.CurrentScenarioState.Maps.Find((CMap map) => map.RoomName == "Room_1").Revealed)
			{
				Trace("UpdateEscapeObjective: last room not revealed yet...", null, 756);
				return;
			}
			_escapeObjectiveDummy.SetActivation(active: false);
			_escapeObjective.SetActivation(active: true);
			Trace("[ffc000]Activating the escape objective...[-]", null, 763);
		}
	}

	public override void OnHeroSummonSpawned(CHeroSummonActor actor)
	{
		base.OnHeroSummonSpawned(actor);
		ChaoticBeamStep(null, actor, afterSpawn: true);
	}
}
