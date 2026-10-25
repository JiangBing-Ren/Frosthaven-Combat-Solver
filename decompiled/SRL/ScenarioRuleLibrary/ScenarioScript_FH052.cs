using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH052 : ScenarioScript
{
	public enum CorpseType
	{
		FrostDemon,
		SnowImp
	}

	private string goalTileID = "S52GoalTilesVFX";

	private string resourceID = "MovableCorpse";

	private string corpsesPrefabName = "DemonCorpsePile";

	private string corpsesStateDemon1 = "StateDemon1";

	private string corpsesStateDemon2 = "StateDemon2";

	private string corpsesStateDemon3 = "StateDemon3";

	private string corpsesStateImp1 = "StateImp1";

	private string corpsesStateImp2 = "StateImp2";

	private string corpsesStateImp3 = "StateImp3";

	private string corpsesStateMixed2 = "StateMixed2";

	private string corpsesStateMixed3MoreDemons = "StateMixed3MoreDemons";

	private string corpsesStateMixed3MoreImps = "StateMixed3MoreImps";

	private string corpsesStateDemonText = "$FH/FH_Scenario_Campaign_052_Prop_Demon_Corpse$";

	private string corpsesStateImpText = "$FH/FH_Scenario_Campaign_052_Prop_Imp_Corpse$";

	private string corpsesStateMixedText = "$FH/FH_Scenario_Campaign_052_Prop_Mixed_Corpse$";

	private string corpsesStateSharedDesc = "$FH_52_Essene_D$";

	private CObjective_Scripted _WinObjectiveDemons;

	private CObjective_Scripted _WinObjectiveImps;

	public Dictionary<CTile, List<CorpseType>> corpses = new Dictionary<CTile, List<CorpseType>>();

	private List<CTile> victoryTilesDemons;

	private List<CTile> victoryTilesImps;

	protected override string SerializeInternal()
	{
		string text = JsonConvert.SerializeObject(corpses.Keys.Select((CTile k) => (X: k.m_ArrayIndex.X, Y: k.m_ArrayIndex.Y)));
		string text2 = JsonConvert.SerializeObject(corpses.Values.ToList());
		string text3 = JsonConvert.SerializeObject(new string[2] { text, text2 });
		Trace("[ffa000]Serialize: " + text3 + "[-]", null, 48);
		return text3;
	}

	protected override void DeserializeInternal(string s)
	{
		ProcessSpawnedMonsters();
		Trace("[ffa000]Deserialize " + s + "[-]", null, 54);
		if (!string.IsNullOrEmpty(s))
		{
			string[] array = JsonConvert.DeserializeObject<string[]>(s);
			List<(int, int)> source = JsonConvert.DeserializeObject<List<(int, int)>>(array[0]);
			List<List<CorpseType>> values = JsonConvert.DeserializeObject<List<List<CorpseType>>>(array[1]);
			int idx = 0;
			corpses = source.ToDictionary(((int, int) p) => ScenarioManager.Tiles[p.Item1, p.Item2], ((int, int) p) => values[idx++]);
		}
		RefreshAllCorpseProps();
		CheckObjectivesComplete();
	}

	private void ProcessSpawnedMonster(CEnemyActor actor)
	{
		if (actor != null && !actor.IsDead)
		{
			actor.NoGoldDrop = true;
			actor.m_OnDeathListeners = (CActor.DeathListener)Delegate.Combine(actor.m_OnDeathListeners, new CActor.DeathListener(OnMonsterDeath));
		}
	}

	private void ProcessSpawnedMonsters()
	{
		foreach (CEnemyActor allEnemy in ScenarioManager.Scenario.AllEnemies)
		{
			ProcessSpawnedMonster(allEnemy);
		}
	}

	private bool CheckVictoryDemonCorpses(out int count)
	{
		count = 0;
		foreach (CTile victoryTilesDemon in victoryTilesDemons)
		{
			if (IsThereACorpseOnTile(victoryTilesDemon, CorpseType.FrostDemon))
			{
				count++;
			}
		}
		return count >= victoryTilesDemons.Count;
	}

	private bool CheckVictoryImpCorpses(out int count)
	{
		count = 0;
		foreach (CTile victoryTilesImp in victoryTilesImps)
		{
			if (IsThereACorpseOnTile(victoryTilesImp, CorpseType.SnowImp))
			{
				count++;
			}
		}
		return count >= victoryTilesImps.Count;
	}

	private bool IsThereACorpseOnTile(CTile tile, CorpseType type)
	{
		if (corpses.TryGetValue(tile, out var value))
		{
			return value.Contains(type);
		}
		return false;
	}

	private void CheckVictory()
	{
		_WinObjectiveDemons.ScriptedIsComplete = CheckVictoryDemonCorpses(out var count);
		_WinObjectiveDemons.Current = count;
		_WinObjectiveImps.ScriptedIsComplete = CheckVictoryImpCorpses(out var count2);
		_WinObjectiveImps.Current = count2;
		CheckObjectivesComplete();
		RefreshGoalTiles();
	}

	private bool RemoveResourceFromActor()
	{
		CPlayerActor cPlayerActor = (_tickActor as CHeroSummonActor)?.Summoner;
		if (cPlayerActor == null)
		{
			cPlayerActor = _tickActor as CPlayerActor;
		}
		if (cPlayerActor == null)
		{
			return false;
		}
		int characterResourceAmount = cPlayerActor.GetCharacterResourceAmount(resourceID);
		if (characterResourceAmount > 0)
		{
			cPlayerActor.RemoveCharacterResource(resourceID, characterResourceAmount);
			return true;
		}
		return false;
	}

	private void ReplaceCorpses(CTile lootedTile)
	{
		if (!corpses.TryGetValue(lootedTile, out var value))
		{
			return;
		}
		foreach (CorpseType item in value)
		{
			CAbility cAbility = CAbility.CreateAbility(CAbility.EAbilityType.NullHex, new CAbilityFilterContainer(), isMonster: true, isTargetedAbility: true);
			cAbility.MiscAbilityData = new AbilityData.MiscAbilityData();
			cAbility.MiscAbilityData.CanUndo = false;
			cAbility.MiscAbilityData.CanSkip = false;
			cAbility.RangeIsBase = true;
			cAbility.TileFilter = CAbilityFilter.EFilterTile.NotBlocked;
			cAbility.AnimOverload = "UseItem";
			cAbility.SetCanUndo(canUndo: false);
			cAbility.SetCanSkip(canSkip: false);
			cAbility.MiscVars = new List<(string, TValue)> { ("includeCasterTile", true) };
			if (item == CorpseType.FrostDemon)
			{
				cAbility.Flags = CAbility.EAbilityFlag.Special1;
				cAbility.HelpBoxTooltipLocKey = "FH/FH_Scenario_Campaign_052_Action_Move_Demon";
			}
			else
			{
				cAbility.Flags = CAbility.EAbilityFlag.Special2;
				cAbility.HelpBoxTooltipLocKey = "FH/FH_Scenario_Campaign_052_Action_Move_Imp";
			}
			if (_tickActor is CHeroSummonActor cHeroSummonActor)
			{
				CAbilityControlActor cAbilityControlActor = CAbility.CreateAbility(CAbility.EAbilityType.ControlActor, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.OwnSummon), isMonster: false, isTargetedAbility: true, 0, 99) as CAbilityControlActor;
				cAbilityControlActor.ControlActorData = new CAbilityControlActor.ControlActorAbilityData();
				cAbilityControlActor.ControlActorData.UseControllingActorPlayerControl = true;
				cAbilityControlActor.ControlActorData.ControlAbilities = new List<CAbility> { cAbility };
				cAbilityControlActor.MiscAbilityData = new AbilityData.MiscAbilityData();
				cAbilityControlActor.MiscAbilityData.AutotriggerAbility = true;
				cAbilityControlActor.SkipAnim = true;
				cAbilityControlActor.TargetThisActorAutomatically = cHeroSummonActor;
				GameState.OverrideCurrentActorForOneAction(_tickActor, CActor.EType.Player, killActorAfterAction: false, null, cHeroSummonActor.Summoner, useControllingActorModifierDeck: false, useControllingActorPlayerControl: true, immediatelySetCurrentActor: true);
				if (PhaseManager.Phase is CPhaseAction cPhaseAction)
				{
					cPhaseAction.InjectAbility(cAbilityControlActor, cHeroSummonActor.Summoner, cAbility.GetAbilityBaseCard());
				}
				else
				{
					PhaseManager.StartAbility(cAbilityControlActor, cAbility.GetAbilityBaseCard(), fullCopy: true);
				}
			}
			else if (PhaseManager.Phase is CPhaseAction cPhaseAction2)
			{
				cPhaseAction2.InjectAbility(cAbility, _tickActor, cAbility.GetAbilityBaseCard());
			}
			else
			{
				PhaseManager.StartAbility(cAbility, cAbility.GetAbilityBaseCard(), fullCopy: true);
			}
		}
		value.Clear();
		CheckVictory();
	}

	private void ReplaceCorpsesAfterLoot()
	{
		if (!(_tickActor.LastAbilityPerformed is CAbilityLoot cAbilityLoot) || !RemoveResourceFromActor())
		{
			return;
		}
		foreach (CTile item in cAbilityLoot.TilesSelected)
		{
			ReplaceCorpses(item);
		}
	}

	private void ReplaceCorpsesAfterEndTurnLoot()
	{
		if (RemoveResourceFromActor())
		{
			ReplaceCorpses(ScenarioManager.Tiles[_tickActor.ArrayIndex.X, _tickActor.ArrayIndex.Y]);
		}
	}

	private string GetCorpsePileState(CTile tile, out int demons, out int imps)
	{
		demons = 0;
		imps = 0;
		if (!corpses.TryGetValue(tile, out var value))
		{
			return null;
		}
		foreach (CorpseType item in value)
		{
			if (item == CorpseType.FrostDemon)
			{
				demons++;
			}
			else
			{
				imps++;
			}
		}
		if (demons == 0)
		{
			if (imps == 1)
			{
				return corpsesStateImp1;
			}
			if (imps == 2)
			{
				return corpsesStateImp2;
			}
			if (imps >= 3)
			{
				return corpsesStateImp3;
			}
		}
		else
		{
			if (imps != 0)
			{
				if (imps == 1 && demons == 1)
				{
					return corpsesStateMixed2;
				}
				if (imps > demons)
				{
					return corpsesStateMixed3MoreImps;
				}
				return corpsesStateMixed3MoreDemons;
			}
			if (demons == 1)
			{
				return corpsesStateDemon1;
			}
			if (demons == 2)
			{
				return corpsesStateDemon2;
			}
			if (demons >= 3)
			{
				return corpsesStateDemon3;
			}
		}
		return corpsesStateDemon1;
	}

	private bool IsCorpsePile(CObjectProp prop)
	{
		if (prop.ObjectType == ScenarioManager.ObjectImportType.Resource)
		{
			return prop.PrefabName == corpsesPrefabName;
		}
		return false;
	}

	private CObjectProp GetExistingCorpsesProp(CTile tile)
	{
		CObjectProp result = null;
		foreach (CObjectProp item in tile.m_Props.ToList())
		{
			if (IsCorpsePile(item))
			{
				result = item;
				break;
			}
		}
		return result;
	}

	private CObjectProp SpawnCorpsesProp(CTile tile)
	{
		CObjectResource cObjectResource = new CObjectResource(ScenarioRuleClient.SRLYML.CharacterResources.SingleOrDefault((CharacterResourceData x) => x.ID == resourceID), corpsesPrefabName, ScenarioManager.ObjectImportType.Resource, new TileIndex(tile.m_ArrayIndex), null, null, null, tile.m_HexMap.MapGuid);
		tile.SpawnProp(cObjectResource);
		return cObjectResource;
	}

	private void RefreshCorsePileVisualState(CObjectProp prop)
	{
		string corpsePileState = GetCorpsePileState(prop.PropTile, out var demons, out var imps);
		CChangePropVisualState_MessageData cChangePropVisualState_MessageData = new CChangePropVisualState_MessageData();
		cChangePropVisualState_MessageData.m_prop = prop;
		cChangePropVisualState_MessageData.m_stateName = corpsePileState;
		cChangePropVisualState_MessageData.m_delay = 0.001f;
		ScenarioRuleClient.MessageHandler(cChangePropVisualState_MessageData);
		if (corpsePileState == corpsesStateDemon1 || corpsePileState == corpsesStateDemon2 || corpsePileState == corpsesStateDemon3)
		{
			prop.PropCustomLocKey = corpsesStateDemonText;
		}
		else if (corpsePileState == corpsesStateImp1 || corpsePileState == corpsesStateImp2 || corpsePileState == corpsesStateImp3)
		{
			prop.PropCustomLocKey = corpsesStateImpText;
		}
		else if (corpsePileState == corpsesStateMixed2 || corpsePileState == corpsesStateMixed3MoreDemons || corpsePileState == corpsesStateMixed3MoreImps)
		{
			prop.PropCustomLocKey = corpsesStateMixedText;
		}
		prop.PropCustomDescriptionLocKey = corpsesStateSharedDesc;
		prop.CustomLocKeyArgs = new List<object> { demons, imps };
	}

	private void PlaceCorpseOn(CTile tile, CorpseType corpseType)
	{
		if (!corpses.TryGetValue(tile, out var value))
		{
			value = new List<CorpseType> { corpseType };
			corpses[tile] = value;
		}
		else
		{
			value.Add(corpseType);
		}
		CObjectProp cObjectProp = GetExistingCorpsesProp(tile);
		if (cObjectProp == null)
		{
			cObjectProp = SpawnCorpsesProp(tile);
		}
		RefreshCorsePileVisualState(cObjectProp);
		CheckVictory();
	}

	private void HandleAfterReplaceCorpses()
	{
		CAbilityNullHex cAbilityNullHex = _tickActor.LastAbilityPerformed as CAbilityNullHex;
		if (cAbilityNullHex?.TilesSelected != null && cAbilityNullHex.TilesSelected.Count != 0)
		{
			if ((cAbilityNullHex.Flags & CAbility.EAbilityFlag.Special1) != 0)
			{
				PlaceCorpseOn(cAbilityNullHex.TilesSelected[0], CorpseType.FrostDemon);
			}
			else if ((cAbilityNullHex.Flags & CAbility.EAbilityFlag.Special2) != 0)
			{
				PlaceCorpseOn(cAbilityNullHex.TilesSelected[0], CorpseType.SnowImp);
			}
		}
	}

	protected void OnMonsterDeath(CActor actor, CAbility causeOfDeathAbility)
	{
		actor.m_OnDeathListeners = (CActor.DeathListener)Delegate.Remove(actor.m_OnDeathListeners, new CActor.DeathListener(OnMonsterDeath));
		if (actor.Type != CActor.EType.Enemy)
		{
			return;
		}
		CTile cTile = actor.Tile;
		if (cTile.Obstacle != null)
		{
			List<(CTile, int, int)> list = new List<(CTile, int, int)>();
			List<CTile> list2 = new List<CTile>();
			list2 = ((!(actor.MonsterClass.ID == "FrostDemonID") && !(actor.MonsterClass.ID == "FrostDemonEliteID")) ? victoryTilesImps : victoryTilesDemons);
			CTile[,] tiles = ScenarioManager.Tiles;
			foreach (CTile cTile2 in tiles)
			{
				if (cTile2 == null || cTile2.Obstacle != null || !cTile2.IsWalkable || !cTile2.m_HexMap.Revealed)
				{
					continue;
				}
				int item = Point.HexDist(cTile2.m_ArrayIndex, cTile.m_ArrayIndex);
				int num = int.MaxValue;
				foreach (CTile item2 in list2)
				{
					int num2 = Point.HexDist(cTile2.m_ArrayIndex, item2.m_ArrayIndex);
					if (num2 < num)
					{
						num = num2;
					}
				}
				list.Add((cTile2, item, num));
			}
			(CTile, int, int) tuple = list.OrderBy(((CTile, int distToDyingActor, int distToClosestGoalTile) x) => x.distToDyingActor).ThenBy(((CTile, int distToDyingActor, int distToClosestGoalTile) x) => x.distToClosestGoalTile).FirstOrDefault();
			if (tuple.Item1 != null)
			{
				(cTile, _, _) = tuple;
			}
		}
		if (actor.MonsterClass.ID == "FrostDemonID" || actor.MonsterClass.ID == "FrostDemonEliteID")
		{
			PlaceCorpseOn(cTile, CorpseType.FrostDemon);
		}
		else
		{
			PlaceCorpseOn(cTile, CorpseType.SnowImp);
		}
	}

	private void RefreshAllCorpseProps()
	{
		foreach (CObjectProp prop in ScenarioManager.CurrentScenarioState.Props)
		{
			if (IsCorpsePile(prop))
			{
				RefreshCorsePileVisualState(prop);
			}
		}
	}

	protected override bool Initialize()
	{
		victoryTilesDemons = new List<CTile>
		{
			ScenarioManager.Tiles[7, 7],
			ScenarioManager.Tiles[8, 7],
			ScenarioManager.Tiles[10, 6],
			ScenarioManager.Tiles[10, 7],
			ScenarioManager.Tiles[8, 5],
			ScenarioManager.Tiles[9, 4]
		};
		victoryTilesImps = new List<CTile>
		{
			ScenarioManager.Tiles[7, 6],
			ScenarioManager.Tiles[8, 8],
			ScenarioManager.Tiles[10, 8],
			ScenarioManager.Tiles[11, 6],
			ScenarioManager.Tiles[8, 4],
			ScenarioManager.Tiles[10, 4]
		};
		_WinObjectiveDemons = (CObjective_Scripted)GetObjective("WinObjectiveDemons");
		_WinObjectiveImps = (CObjective_Scripted)GetObjective("WinObjectiveImps");
		_WinObjectiveDemons.Total = victoryTilesDemons.Count;
		_WinObjectiveImps.Total = victoryTilesImps.Count;
		RefreshGoalTiles();
		return true;
	}

	private void RefreshGoalTiles()
	{
		foreach (CTile victoryTilesDemon in victoryTilesDemons)
		{
			RefreshGoalTile(victoryTilesDemon, CorpseType.FrostDemon);
		}
		foreach (CTile victoryTilesImp in victoryTilesImps)
		{
			RefreshGoalTile(victoryTilesImp, CorpseType.SnowImp);
		}
	}

	private void RefreshGoalTile(CTile tile, CorpseType corpseType)
	{
		CObjectProp cObjectProp = tile.m_Props.Find((CObjectProp p) => p.PrefabName == goalTileID);
		if (cObjectProp == null)
		{
			cObjectProp = SpawnVFXProp(tile, goalTileID);
		}
		string stateName = (IsThereACorpseOnTile(tile, corpseType) ? "StateWithCorpse" : "StateNoCorpse");
		CChangePropVisualState_MessageData cChangePropVisualState_MessageData = new CChangePropVisualState_MessageData();
		cChangePropVisualState_MessageData.m_prop = cObjectProp;
		cChangePropVisualState_MessageData.m_stateName = stateName;
		cChangePropVisualState_MessageData.m_delay = 0.001f;
		ScenarioRuleClient.MessageHandler(cChangePropVisualState_MessageData);
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.OnActorSpawnedOrRevealed:
			ProcessSpawnedMonster(_tickActor as CEnemyActor);
			break;
		case EScenarioModifierTriggerPhase.AfterAbility:
			ReplaceCorpsesAfterLoot();
			HandleAfterReplaceCorpses();
			break;
		case EScenarioModifierTriggerPhase.EndTurnLoot:
			ReplaceCorpsesAfterEndTurnLoot();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 473);
			break;
		}
	}

	public override bool CanActorLootResources(CActor actor)
	{
		if (actor is CHeroSummonActor)
		{
			return true;
		}
		return base.CanActorLootResources(actor);
	}
}
