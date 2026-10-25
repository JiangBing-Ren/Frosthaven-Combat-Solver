using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Libs.GameConsole;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH066 : ScenarioScript
{
	[ConsoleVariable(Alias = "ss066_debug_elements")]
	public static int DebugElements_cvar = 1;

	private static int maximumOpenedDoors = 4;

	private static string resourceID = "ElementalCore";

	private static string elementPrefabName = "ElementalCore";

	private Dictionary<Point, ElementInfusionBoardManager.EElement> activatedElementalCoreTiles = new Dictionary<Point, ElementInfusionBoardManager.EElement>
	{
		{
			new Point(14, 14),
			ElementInfusionBoardManager.EElement.Any
		},
		{
			new Point(14, 8),
			ElementInfusionBoardManager.EElement.Any
		},
		{
			new Point(11, 14),
			ElementInfusionBoardManager.EElement.Any
		},
		{
			new Point(11, 8),
			ElementInfusionBoardManager.EElement.Any
		}
	};

	public Dictionary<Point, List<ElementInfusionBoardManager.EElement>> elementalCoreTiles = new Dictionary<Point, List<ElementInfusionBoardManager.EElement>>
	{
		{
			new Point(12, 2),
			new List<ElementInfusionBoardManager.EElement> { ElementInfusionBoardManager.EElement.Dark }
		},
		{
			new Point(6, 18),
			new List<ElementInfusionBoardManager.EElement> { ElementInfusionBoardManager.EElement.Earth }
		},
		{
			new Point(22, 16),
			new List<ElementInfusionBoardManager.EElement> { ElementInfusionBoardManager.EElement.Fire }
		},
		{
			new Point(3, 6),
			new List<ElementInfusionBoardManager.EElement> { ElementInfusionBoardManager.EElement.Ice }
		},
		{
			new Point(13, 20),
			new List<ElementInfusionBoardManager.EElement> { ElementInfusionBoardManager.EElement.Light }
		},
		{
			new Point(19, 4),
			new List<ElementInfusionBoardManager.EElement> { ElementInfusionBoardManager.EElement.Air }
		}
	};

	private CScenarioModifier _scenarioAbilityHolderActivateCore;

	private List<CObjectDoor> _doors;

	private CObjective_Scripted _winObjective;

	public Dictionary<CPlayerActor, List<ElementInfusionBoardManager.EElement>> playerElements = new Dictionary<CPlayerActor, List<ElementInfusionBoardManager.EElement>>();

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartScenario:
			SpawnCores();
			break;
		case EScenarioModifierTriggerPhase.EndTurn:
			TickPhaseEndTurn();
			break;
		case EScenarioModifierTriggerPhase.StartRound:
			TickPhaseStartRound();
			break;
		case EScenarioModifierTriggerPhase.OnRoomRevealed:
			TickRoomRevealed();
			break;
		case EScenarioModifierTriggerPhase.AfterAbility:
			LootElementalCoresLootAbility();
			ActivateElementCoreAfterAbility();
			break;
		case EScenarioModifierTriggerPhase.EndTurnLoot:
			LootElementalCoresEndTurnLoot();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 81);
			break;
		case EScenarioModifierTriggerPhase.EndRound:
		case EScenarioModifierTriggerPhase.StartTurn:
			break;
		}
	}

	private void SpawnCores()
	{
		if (OncePerRound())
		{
			if (ScenarioManager.Tiles == null)
			{
				Error("[ERROR] Tiles is null", null, 91);
				return;
			}
			foreach (KeyValuePair<Point, List<ElementInfusionBoardManager.EElement>> elementalCoreTile in elementalCoreTiles)
			{
				CTile cTile = ScenarioManager.Tiles[elementalCoreTile.Key.X, elementalCoreTile.Key.Y];
				if (cTile == null)
				{
					Error($"[ERROR] Tile is null for {elementalCoreTile.Key}", null, 101);
				}
				else
				{
					SpawnElementalCoreProp(cTile, elementalCoreTile.Value[0]);
				}
			}
		}
		ScenarioManager.ToggleTileHighlights(activatedElementalCoreTiles.Keys, enabled: true);
		ScenarioManager.ToggleTileHighlights(elementalCoreTiles.Keys, enabled: true);
	}

	protected override bool Initialize()
	{
		_doors = (from d in ScenarioManager.CurrentScenarioState.Props
			where d.ObjectType == ScenarioManager.ObjectImportType.Door
			select d as CObjectDoor).ToList();
		_scenarioAbilityHolderActivateCore = GetScenarioModifier("ScenarioAbilityHolderActivateCore");
		_winObjective = (CObjective_Scripted)GetObjective("ActivateElements");
		_winObjective.Total = activatedElementalCoreTiles.Count();
		return true;
	}

	private void TickPhaseStartRound()
	{
		if (ScenarioScript.RoundNumber == 1)
		{
			foreach (CObjectDoor doorProp in ScenarioScript._css.DoorProps)
			{
				doorProp.SetHasExtraLock(hasExtraLock: false);
			}
		}
		InfuseElements();
	}

	private void InfuseElements()
	{
		if (ScenarioScript.RoundNumber > 1 && !OncePerRound())
		{
			return;
		}
		IEnumerable<ElementInfusionBoardManager.EElement> enumerable = Enum.GetValues(typeof(ElementInfusionBoardManager.EElement)).Cast<ElementInfusionBoardManager.EElement>();
		foreach (CObjectProp prop in ScenarioManager.CurrentScenarioState.Props)
		{
			if (!(prop.PrefabName == elementPrefabName) || !prop.PropTile.m_HexMap.Revealed)
			{
				continue;
			}
			foreach (ElementInfusionBoardManager.EElement item in enumerable)
			{
				if (!activatedElementalCoreTiles.Values.Contains(item) && prop.PropCustomLocKey.EndsWith(item.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					InfuseElement(item);
					break;
				}
			}
		}
		foreach (KeyValuePair<CPlayerActor, List<ElementInfusionBoardManager.EElement>> playerElement in playerElements)
		{
			foreach (ElementInfusionBoardManager.EElement item2 in playerElement.Value)
			{
				InfuseElement(item2);
			}
		}
	}

	private void InfuseElement(ElementInfusionBoardManager.EElement element)
	{
		ElementInfusionBoardManager.SetElementInstantly(element, ElementInfusionBoardManager.EColumn.Strong, fromScenarioScript: true);
		CElementsInfused_MessageData message = new CElementsInfused_MessageData(null, null)
		{
			m_ChangedElements = new List<ElementInfusionBoardManager.EElement> { element },
			m_InfuseAbility = null,
			m_IsConsume = false
		};
		ScenarioRuleClient.MessageHandler(message);
	}

	protected override string SerializeInternal()
	{
		if (activatedElementalCoreTiles.Count != 4)
		{
			Log.Error($"Serializing scenario with invalid number of core elementals - {activatedElementalCoreTiles.Count}!");
		}
		string text = JsonConvert.SerializeObject(activatedElementalCoreTiles.Values.ToList());
		string text2 = JsonConvert.SerializeObject(elementalCoreTiles.Keys.ToList());
		string text3 = JsonConvert.SerializeObject(elementalCoreTiles.Values.ToList());
		string text4 = JsonConvert.SerializeObject(playerElements.Keys.ToList().ConvertAll((CPlayerActor p) => p.CharacterName));
		string text5 = JsonConvert.SerializeObject(playerElements.Values.ToList());
		string text6 = JsonConvert.SerializeObject(new string[5] { text, text2, text3, text4, text5 });
		Trace("Serialize scenario 66: " + text6, null, 190);
		return text6;
	}

	protected override void DeserializeInternal(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return;
		}
		int idx = 0;
		string[] array = JsonConvert.DeserializeObject<string[]>(s);
		List<Point> source = activatedElementalCoreTiles.Keys.ToList();
		List<ElementInfusionBoardManager.EElement> activatedCoreValues = JsonConvert.DeserializeObject<List<ElementInfusionBoardManager.EElement>>(array[0]);
		activatedElementalCoreTiles = source.ToDictionary((Point k) => k, (Point k) => activatedCoreValues[idx++]);
		if (activatedElementalCoreTiles.Count != 4)
		{
			Log.Error($"Deserializing scenario with invalid number of core elementals - {activatedElementalCoreTiles.Count}!");
		}
		idx = 0;
		List<Point> list = JsonConvert.DeserializeObject<List<Point>>(array[1]);
		List<List<ElementInfusionBoardManager.EElement>> coreValues = JsonConvert.DeserializeObject<List<List<ElementInfusionBoardManager.EElement>>>(array[2]);
		if (list.Count > 0)
		{
			elementalCoreTiles = list.ToDictionary((Point k) => k, (Point k) => coreValues[idx++]);
		}
		else
		{
			elementalCoreTiles.Clear();
		}
		idx = 0;
		List<string> playerKeys = JsonConvert.DeserializeObject<List<string>>(array[3]);
		List<List<ElementInfusionBoardManager.EElement>> playerValues = JsonConvert.DeserializeObject<List<List<ElementInfusionBoardManager.EElement>>>(array[4]);
		if (playerKeys.Count > 0)
		{
			playerElements = playerKeys.ToDictionary((string k) => ScenarioScript._css.Players.Find((PlayerState p) => p.CharacterName == playerKeys[idx]).Actor as CPlayerActor, (string k) => playerValues[idx++]);
		}
		RefreshCoresVisualState();
		ScenarioManager.ToggleTileHighlights((from pair in activatedElementalCoreTiles
			where pair.Value == ElementInfusionBoardManager.EElement.Any
			select pair.Key).ToList(), enabled: true);
		ScenarioManager.ToggleTileHighlights(elementalCoreTiles.Keys, enabled: true);
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			RefreshElementalCoreMoveBonus(playerActor);
		}
	}

	private void RefreshCoresVisualState()
	{
		IEnumerable<ElementInfusionBoardManager.EElement> enumerable = Enum.GetValues(typeof(ElementInfusionBoardManager.EElement)).Cast<ElementInfusionBoardManager.EElement>();
		foreach (CObjectProp prop in ScenarioManager.CurrentScenarioState.Props)
		{
			if (!(prop.PrefabName == elementPrefabName))
			{
				continue;
			}
			foreach (ElementInfusionBoardManager.EElement item in enumerable)
			{
				if (prop.PropCustomLocKey.EndsWith(item.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					RefreshElementalCoreVisualState(prop, item, prop.ObjectType == ScenarioManager.ObjectImportType.VFXProp);
					break;
				}
			}
		}
	}

	public override bool ShouldSortModifierTextsInOrderOfModifiers()
	{
		return true;
	}

	private void TickRoomRevealed()
	{
		List<CObjectDoor> list = _doors.ToList();
		if (list.Count((CObjectDoor d) => d.DoorIsOpen) < maximumOpenedDoors)
		{
			return;
		}
		foreach (CObjectDoor item in list.FindAll((CObjectDoor d) => d.DoorIsClosed))
		{
			item.SetHasExtraLock(hasExtraLock: true);
		}
	}

	private void TickPhaseEndTurn()
	{
		if (_tickActor.Type == CActor.EType.Player && _tickActor is CPlayerActor cPlayerActor && activatedElementalCoreTiles.TryGetValue(cPlayerActor.ArrayIndex, out var value) && ((value == ElementInfusionBoardManager.EElement.Any) & playerElements.TryGetValue(cPlayerActor, out var _)))
		{
			OfferActivateElement(cPlayerActor);
		}
	}

	protected override void TickDeathInternal()
	{
		if (_tickActor.Type == CActor.EType.Player && _tickActor is CPlayerActor cPlayerActor && playerElements.Keys.ToList().Contains(cPlayerActor))
		{
			DropElementsLoots(cPlayerActor);
		}
	}

	private void DropElementsLoots(CPlayerActor playerActor)
	{
		if (!playerElements.TryGetValue(playerActor, out var value))
		{
			return;
		}
		if (!elementalCoreTiles.TryGetValue(playerActor.ArrayIndex, out var value2))
		{
			value2 = new List<ElementInfusionBoardManager.EElement>();
			elementalCoreTiles[playerActor.ArrayIndex] = value2;
		}
		foreach (ElementInfusionBoardManager.EElement item in value)
		{
			SpawnElementalCoreProp(playerActor.Tile, item);
			value2.Add(item);
		}
		playerElements.Remove(playerActor);
		ScenarioManager.ToggleTileHighlights(new Point[1] { playerActor.ArrayIndex }, enabled: true);
		RefreshElementalCoreMoveBonus(playerActor);
	}

	private void OfferActivateElement(CPlayerActor actor)
	{
		CAbility cAbility = ScenarioRuleClient.SRLYML.ScenarioAbilities.FirstOrDefault((ScenarioAbilitiesYMLData a) => a.ScenarioAbilityID == _scenarioAbilityHolderActivateCore.ScenarioAbilityID)?.ScenarioAbilities[0];
		CAbility cAbility2 = CAbility.CopyAbility(cAbility, generateNewID: false);
		cAbility2.IsScenarioModifierAbility = true;
		CPhaseAction currentPhaseAction = PhaseManager.CurrentPhaseAction;
		if (currentPhaseAction != null)
		{
			currentPhaseAction.StackNextAbilities(new List<CAbility> { cAbility2 }, actor, killAfter: false, stackToNextCurrent: true);
		}
		else
		{
			cAbility.IsScenarioModifierAbility = true;
			GameState.PendingScenarioModifierAbilities.Add(new Tuple<CAbility, CActor>(cAbility, actor));
		}
	}

	private void ActivateElementCore(CPlayerActor playerActor, ElementInfusionBoardManager.EElement element)
	{
		_winObjective.Current++;
		_winObjective.ScriptedIsComplete = _winObjective.Current >= _winObjective.Total;
		_winObjective.CheckObjectiveComplete(ScenarioScript.PartySize);
		CheckObjectivesComplete();
		SpawnElementalCoreProp(playerActor.Tile, element, underground: true);
		CPlaySound_MessageData message = new CPlaySound_MessageData("ElementalCore_Place");
		ScenarioRuleClient.MessageHandler(message);
		activatedElementalCoreTiles[playerActor.ArrayIndex] = element;
		List<ElementInfusionBoardManager.EElement> list = playerElements[playerActor];
		list.Remove(element);
		if (list.Count == 0)
		{
			playerElements.Remove(playerActor);
		}
		ScenarioManager.ToggleTileHighlights(new Point[1] { playerActor.ArrayIndex }, enabled: false);
		RefreshElementalCoreMoveBonus(playerActor);
	}

	private void LootElementalCoresLootAbility()
	{
		CPlayerActor cPlayerActor = (_tickActor as CHeroSummonActor)?.Summoner;
		if (cPlayerActor == null)
		{
			cPlayerActor = _tickActor as CPlayerActor;
		}
		CAbilityLoot cAbilityLoot = _tickActor.LastAbilityPerformed as CAbilityLoot;
		if (cPlayerActor == null || cAbilityLoot == null || !RemoveResourceFromActor(cPlayerActor))
		{
			return;
		}
		foreach (CTile item in cAbilityLoot.TilesSelected)
		{
			LootElements(cPlayerActor, item);
		}
	}

	private void RefreshElementalCoreMoveBonus(CActor actor)
	{
		if (ScenarioScript.PartySize >= 4)
		{
			return;
		}
		if (actor is CHeroSummonActor cHeroSummonActor)
		{
			actor = cHeroSummonActor.Summoner;
		}
		if (!(actor is CPlayerActor))
		{
			return;
		}
		bool num = playerElements.TryGetValue(actor as CPlayerActor, out var value) && value.Count > 0;
		CActiveBonus cActiveBonus = FindElementalCoreMoveBonus(actor);
		if (num)
		{
			if (cActiveBonus == null)
			{
				CAbilityMove obj = CAbilityMove.CreateDefaultMove(0, isMonster: true, 0, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, isDefault: false) as CAbilityMove;
				obj.Name = "FH066_MoveBuff";
				obj.StatIsBasedOnXEntries = new List<AbilityData.StatIsBasedOnXData>
				{
					new AbilityData.StatIsBasedOnXData
					{
						BasedOn = CAbility.EStatIsBasedOnXType.Vars,
						Vars = new List<(string, TValue)> { ("Value", new TValue(CExpression.Parse("4 - ScenarioScript.PartySize"))) },
						AbilityStatType = CAbility.EAbilityStatType.Strength
					}
				};
				obj.ActiveBonusData.Duration = CActiveBonus.EActiveBonusDurationType.Persistent;
				obj.ActiveBonusData.Behaviour = CActiveBonus.EActiveBonusBehaviourType.BuffMove;
				obj.ActiveBonusData.ListLayoutOverride = ($"<sprite name=Move> {4 - ScenarioScript.PartySize}", "Positive_Move_DifficultTerrain_Campaign");
				obj.CachedAbilityBaseCard = CBaseCard.CreateDummy("ElementalCore_TOOLTIP");
				obj.Start(actor, actor);
				obj.ApplyToActor(actor);
			}
		}
		else
		{
			cActiveBonus?.Finish();
		}
	}

	private CActiveBonus FindElementalCoreMoveBonus(CActor actor)
	{
		return actor.FindApplicableActiveBonuses(CAbility.EAbilityType.Move, CActiveBonus.EActiveBonusBehaviourType.BuffMove).FirstOrDefault((CActiveBonus x) => x.Ability.Name == "FH066_MoveBuff");
	}

	private void ActivateElementCoreAfterAbility()
	{
		if (_tickActor.LastAbilityPerformed is CAbilityNullTargeting cAbilityNullTargeting && (cAbilityNullTargeting.Flags & CAbility.EAbilityFlag.Special1) != CAbility.EAbilityFlag.None)
		{
			if (!activatedElementalCoreTiles.ContainsKey(_tickActor.ArrayIndex))
			{
				Log.Error($"Trying to activate core at invalid position {_tickActor.ArrayIndex}. Ignore this!");
				return;
			}
			ElementInfusionBoardManager.EElement element = Enum.Parse<ElementInfusionBoardManager.EElement>(cAbilityNullTargeting.GetVar("Element").GetString());
			ActivateElementCore(_tickActor as CPlayerActor, element);
		}
	}

	private void LootElementalCoresEndTurnLoot()
	{
		CPlayerActor cPlayerActor = (_tickActor as CHeroSummonActor)?.Summoner;
		if (cPlayerActor == null)
		{
			cPlayerActor = _tickActor as CPlayerActor;
		}
		if (cPlayerActor != null && RemoveResourceFromActor(cPlayerActor))
		{
			LootElements(cPlayerActor, ScenarioManager.Tiles[_tickActor.ArrayIndex.X, _tickActor.ArrayIndex.Y]);
		}
	}

	private bool RemoveResourceFromActor(CPlayerActor actor)
	{
		int characterResourceAmount = actor.GetCharacterResourceAmount(resourceID);
		if (characterResourceAmount > 0)
		{
			actor.RemoveCharacterResource(resourceID, characterResourceAmount);
			return true;
		}
		return false;
	}

	private void LootElements(CPlayerActor playerActor, CTile tile)
	{
		if (!elementalCoreTiles.TryGetValue(tile.m_ArrayIndex, out var value))
		{
			return;
		}
		if (!playerElements.TryGetValue(playerActor, out var value2))
		{
			value2 = new List<ElementInfusionBoardManager.EElement>();
			playerElements[playerActor] = value2;
		}
		foreach (ElementInfusionBoardManager.EElement item in value)
		{
			value2.Add(item);
		}
		elementalCoreTiles.Remove(tile.m_ArrayIndex);
		CPlaySound_MessageData message = new CPlaySound_MessageData("ElementalCore_Pickup");
		ScenarioRuleClient.MessageHandler(message);
		ScenarioManager.ToggleTileHighlights(new Point[1] { tile.m_ArrayIndex }, enabled: false);
		RefreshElementalCoreMoveBonus(playerActor);
	}

	protected override List<object> GetConclusionParametersInternal()
	{
		if (activatedElementalCoreTiles.Count != 4)
		{
			Log.Error($"Ending scenario with invalid number of core elementals - {activatedElementalCoreTiles.Count}!");
		}
		return ((IEnumerable<ElementInfusionBoardManager.EElement>)activatedElementalCoreTiles.Values).Select((Func<ElementInfusionBoardManager.EElement, object>)((ElementInfusionBoardManager.EElement e) => e)).ToList();
	}

	public static bool ValidateActivateCore(string elementStr)
	{
		ScenarioScript_FH066 scenarioScript_FH = ScenarioScript.current as ScenarioScript_FH066;
		if (!scenarioScript_FH.playerElements.TryGetValue(scenarioScript_FH._tickActor as CPlayerActor, out var value))
		{
			return false;
		}
		ElementInfusionBoardManager.EElement item = Enum.Parse<ElementInfusionBoardManager.EElement>(elementStr);
		return value.Contains(item);
	}

	private static CObjectProp SpawnElementalCoreProp(CTile tile, ElementInfusionBoardManager.EElement element, bool underground = false)
	{
		CObjectResource cObjectResource = new CObjectResource(ScenarioRuleClient.SRLYML.CharacterResources.SingleOrDefault((CharacterResourceData x) => x.ID == resourceID), elementPrefabName, underground ? ScenarioManager.ObjectImportType.VFXProp : ScenarioManager.ObjectImportType.Resource, new TileIndex(tile.m_ArrayIndex), null, null, null, tile.m_HexMap.MapGuid);
		tile.SpawnProp(cObjectResource);
		cObjectResource.PropCustomLocKey = "GUI_ELEMENT_" + element.ToString().ToUpper();
		cObjectResource.PropCustomDescriptionLocKey = "FH_66_Elemental_Core_D";
		RefreshElementalCoreVisualState(cObjectResource, element, underground);
		return cObjectResource;
	}

	private static void RefreshElementalCoreVisualState(CObjectProp prop, ElementInfusionBoardManager.EElement element, bool underground)
	{
		CChangePropVisualState_MessageData cChangePropVisualState_MessageData = new CChangePropVisualState_MessageData();
		cChangePropVisualState_MessageData.m_prop = prop;
		cChangePropVisualState_MessageData.m_stateName = "State" + (underground ? "Underground" : "") + element;
		cChangePropVisualState_MessageData.m_delay = 0.001f;
		ScenarioRuleClient.MessageHandler(cChangePropVisualState_MessageData);
	}

	[ConsoleCommand(Command = "ss066_spawn_cores")]
	public static void SS066SpawnCores(IConsole console)
	{
		if (ScenarioManager.CurrentScenarioState.FindScenarioScript("ScenarioScript_FH066", out var script) && script is ScenarioScript_FH066)
		{
			SpawnElementalCoreProp(ScenarioManager.Tiles[13, 14], ElementInfusionBoardManager.EElement.Fire);
			SpawnElementalCoreProp(ScenarioManager.Tiles[14, 14], ElementInfusionBoardManager.EElement.Fire, underground: true);
		}
	}

	public override bool LoopUnopenedDoorParticlesOnLoad()
	{
		return true;
	}

	public override IEnumerable<(string, string)> GetSpecialRulesParamsString()
	{
		yield return ("bonus_movement", (4 - ScenarioScript.PartySize).ToString());
	}
}
