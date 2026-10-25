using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FFSNet;
using FH.ApparanceBaking;
using FH.Misc;
using FH.YML;
using FHRL;
using JetBrains.Annotations;
using Libs.Utils.Profiler;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class UnityGameEditorRuntime
{
	public static Vector3 s_TileSize;

	public static bool s_DisplayBasicTilesAtRuntime = false;

	public static string s_ScenarioOrProcGenKey = "1";

	public static bool s_Initialised = false;

	public static ScenarioRuleLibrary.ScenarioState s_OverrideCurrentScenarioState = null;

	public static bool s_SkipNextAutoSave = false;

	public static bool s_SkipNextSelectCardSyncCheck = false;

	private static Scene currentlyLoadedScene;

	public static bool InvalidatedScenarioSave = false;

	public static void Initialise()
	{
		if (s_Initialised)
		{
			return;
		}
		try
		{
			Vector3 vector = Vector3.zero;
			GameObject gameObject = Resources.Load<GameObject>("Hex");
			if (!(gameObject != null))
			{
				return;
			}
			BoxCollider component = gameObject.GetComponent<BoxCollider>();
			if (component != null)
			{
				vector = component.size;
			}
			if (vector == Vector3.zero)
			{
				Renderer component2 = gameObject.GetComponent<Renderer>();
				if (component2 != null)
				{
					vector = component2.bounds.size;
				}
			}
			if (vector == Vector3.zero)
			{
				throw new Exception("Missing hex reference asset");
			}
			s_TileSize.x = vector.x;
			s_TileSize.z = vector.z * 0.75f;
			CActor.SetLOSTileScalar(s_TileSize.x, s_TileSize.z);
			s_Initialised = true;
		}
		catch (Exception ex)
		{
			Log.Exception(ex, "An exception occurred while initialising UnityGameEditorRuntime\n");
			if (Application.isPlaying)
			{
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_RUNTIME_00001", "GUI_ERROR_EXIT_GAME_BUTTON", ex.StackTrace, SceneController.Instance.ExitApplication, ex.Message);
			}
		}
	}

	public static async Task LoadScenario(ScenarioRuleLibrary.ScenarioState state, CMapParty party = null, KronosProfilingScaffolding profiling = null)
	{
		if (profiling == null)
		{
			profiling = SceneController.Instance?.LoadingProfiler;
		}
		if (s_OverrideCurrentScenarioState != null)
		{
			state = s_OverrideCurrentScenarioState;
			s_OverrideCurrentScenarioState = null;
		}
		await LoadScenario(state, (ScenarioRuleLibrary.ScenarioState stateCloneToPrepare) =>
		{
			if (Application.isPlaying)
			{
				string text = "Procgen Generating Procgen Level:\n Procgen Key: " + stateCloneToPrepare.Seed;
				if (state.ScenarioType == EScenarioType.YML)
				{
					text = text + "\nFile: " + state.ScenarioFileName;
				}
				text += GenerateRoomLog(stateCloneToPrepare);
				Log.Info(text);
				if (SaveData.Instance.Global.GameMode != EGameMode.LevelEditor && SaveData.Instance.Global.GameMode != EGameMode.Autotest && stateCloneToPrepare.Players.Count == 0)
				{
					if (party == null)
					{
						throw new Exception("No players were added to the scenario being loaded!");
					}
					AddPlayersToState(party, stateCloneToPrepare);
				}
			}
		}, profiling);
	}

	public static async Task LoadScenario(ScenarioRuleLibrary.ScenarioState scenarioState, UnityAction<ScenarioRuleLibrary.ScenarioState> preSceneLoadOperation, KronosProfilingScaffolding profiling)
	{
		using (new MiniProfilerScope("UnityGameEditorRuntime.LoadScenario"))
		{
			_ = 1;
			try
			{
				Log.NetworkInfo("Seed: " + scenarioState.Seed);
				Log.SimpleInfo("RNG STATES (Load Scenario): \nScenarioRNG:" + (scenarioState.ScenarioRNGNotNull ? scenarioState.PeekScenarioRNG.ToString() : "NULL") + "\nEnemyIDRNG:" + ((scenarioState.EnemyIDRNG != null) ? scenarioState.PeekEnemyIDRNG.ToString() : "NULL") + "\nEnemyAbilityCardRNG:" + ((scenarioState.EnemyAbilityCardRNG != null) ? scenarioState.PeekEnemyAbilityCardRNG.ToString() : "NULL") + "\nGuidRNG:" + ((scenarioState.GuidRNG != null) ? scenarioState.PeekGuidRNG.ToString() : "NULL"));
				if (SaveData.Instance.Global.GameMode != EGameMode.None)
				{
					ScenarioRuleLibrary.ScenarioState scenarioState2 = await scenarioState.DeepCopySerializableObject<ScenarioRuleLibrary.ScenarioState>();
					preSceneLoadOperation?.Invoke(scenarioState2);
					if (Application.isPlaying)
					{
						await Choreographer.s_Choreographer.LoadProcGenScene(scenarioState2, !scenarioState2.IsInitialised, profiling);
					}
				}
				else
				{
					Log.Error("Game mode has not been set.  Unable to load into level");
					SceneController.Instance.ReloadMainMenu = true;
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "An exception occurred while loading a Scenario");
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_RUNTIME_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
			}
		}
	}

	private static void AddPlayersToState(CMapParty party, ScenarioRuleLibrary.ScenarioState state)
	{
		foreach (CMapCharacter cData in party.SelectedCharacters)
		{
			CCharacterClass cCharacterClass = CharacterClassManager.Classes.Single((CCharacterClass s) => s.ID == cData.CharacterID);
			PlayerState item = new PlayerState(cData.CharacterID, 0, null, state.Maps[0].MapGuid, null, cData.MaxHealth, cData.MaxHealth, cData.Level, new List<PositiveConditionPair>(), new List<NegativeConditionPair>(), playedThisRound: false, CActor.ECauseOfDeath.StillAlive, 1, cData.CharacterName, 0, 0, null, null, isLongResting: false, isShortResting: false, new AbilityDeckState((from c in cData.HandAbilityCards.Select((CAbilityCard c) => new { c.ID, c.CardInstanceID }).AsEnumerable()
				select new Tuple<int, int>(c.ID, c.CardInstanceID)).ToList(), cData.OwnedAbilityCardIDs), new AttackModifierDeckState(cCharacterClass), cData.CheckEquippedItems, cCharacterClass.TimeTokens, CPlayerActor.EAgility.None, CPlayerActor.EForm.None, CPlayerActor.EForm.None, CPlayerActor.EForm.None, CPlayerActor.EPressure.None, 1, cData.SmallItemMax, cCharacterClass.TrophyTokens, cData.CompletedMasteries, cCharacterClass.ResonanceTokens)
			{
				IsRevealed = true
			};
			state.Players.Add(item);
		}
	}

	public static string GenerateRoomLog(ScenarioRuleLibrary.ScenarioState state)
	{
		string text = string.Empty;
		try
		{
			foreach (CMap mapInput in state.Maps)
			{
				text = text + "Room: " + mapInput.ScenarioRoomName + " (" + mapInput.RoomName + ")\n  Map Type: " + mapInput.MapType.ToString() + "\n  Parent: " + ((mapInput.ParentName == string.Empty) ? "None" : mapInput.ParentName) + "\n";
				foreach (EnemyState monster in from w in state.Monsters.Distinct()
					where w.StartingMapGuid == mapInput.MapGuid
					select w)
				{
					text = text + "    Monster: " + state.Monsters.Where((EnemyState s) => s.StartingMapGuid == mapInput.MapGuid && s.ClassID == monster.ClassID).Count() + " X " + monster.ClassID + "\n";
				}
			}
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "Exception while generating debug log\n");
		}
		return text;
	}

	public static IEnumerator UnloadScenario()
	{
		if (currentlyLoadedScene.IsValid() && currentlyLoadedScene.isLoaded)
		{
			FastReload.Reset();
			ObjectPool.RecyclePendingObjects();
			CameraController.s_CameraController.SetCamera(null);
			AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(currentlyLoadedScene);
			WaitForEndOfFrame waitForEndOfFrameDelay = new WaitForEndOfFrame();
			while (!asyncUnload.isDone)
			{
				yield return waitForEndOfFrameDelay;
			}
			ScenarioManager.Unload();
		}
	}

	public static async Task OnSceneLoaded(Scene scene, [CanBeNull] KronosProfilingScaffolding profiling)
	{
		using (new MiniProfilerScope("UnityGameEditorRuntime.OnSceneLoaded"))
		{
			WorldspacePanelUIController.ResetHighlightCaches();
			FastReload.TriggerAutoSaveGame = false;
			try
			{
				ScenarioManager.CurrentScenarioState.LoadingScenarioState = true;
				Log.SimpleInfo("RNG STATES (start of scenario loaded callback): \nScenarioRNG:" + ScenarioManager.CurrentScenarioState.PeekScenarioRNG + "\nEnemyIDRNG:" + ScenarioManager.CurrentScenarioState.PeekEnemyIDRNG + "\nEnemyAbilityCardRNG:" + ScenarioManager.CurrentScenarioState.PeekEnemyAbilityCardRNG + "\nGuidRNG:" + ScenarioManager.CurrentScenarioState.PeekGuidRNG);
				currentlyLoadedScene = scene;
				bool generatedScenario = scene.name == "ProcGen";
				if (Application.isPlaying)
				{
					SceneManager.SetActiveScene(scene);
					CameraController.s_CameraController.m_Camera.gameObject.GetComponent<CreateLightProbes>()?.Generate();
				}
				GameObject gameObject = GameObject.Find("TEMP");
				if (gameObject != null)
				{
					UnityEngine.Object.Destroy(gameObject);
				}
				if (SaveData.Instance.Global.CurrentlyPlayingCustomLevel || AutoTestController.s_AutoTestCurrentlyLoaded)
				{
					if (SaveData.Instance.Global.CurrentlyPlayingCustomLevel && SaveData.Instance.Global.CurrentCustomLevelData.PartySpawnType == ELevelPartyChoiceType.LoadAdventureParty && SaveData.Instance.Global.CurrentCustomLevelData.EnemyLevelsScaleToPartyLevel)
					{
						int num = 1;
						if (SaveData.Instance.Global.GameMode == EGameMode.Campaign)
						{
							num = SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.MapParty.InProgressScenarioLevel;
						}
						else if (SaveData.Instance.Global.GameMode == EGameMode.SingleScenario || SaveData.Instance.Global.GameMode == EGameMode.FrontEndTutorial)
						{
							num = SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.MapParty.ScenarioLevel;
						}
						else if (SaveData.Instance.Global.GameMode == EGameMode.Autotest)
						{
							int num2 = SaveData.Instance.Global.CurrentCustomLevelData.ScenarioState.Players.Sum((PlayerState x) => x.Level);
							int count = SaveData.Instance.Global.CurrentCustomLevelData.ScenarioState.Players.Count;
							num = (int)Math.Ceiling((float)num2 / (2f * (float)count));
						}
						ScenarioManager.CurrentScenarioState?.SetScenarioLevel(num);
						MonsterClassManager.SetAllMonsterStatLevels(num, SaveData.Instance.Global.CurrentCustomLevelData.StatBasedOnXOverrideList);
						ScenarioManager.Scenario.EnemyMaxHealthBasedOnPartyLevel = SaveData.Instance.Global.CurrentCustomLevelData.EnemyMaxHealthBasedOnPartyLevel;
						ScenarioManager.Scenario.SetEnemyHealthToMaxOnPlay = SaveData.Instance.Global.CurrentCustomLevelData.SetEnemyHealthToMaxOnPlay;
					}
					else
					{
						ScenarioManager.CurrentScenarioState.UpdateMonsterClassesFromState(SaveData.Instance.Global.CurrentCustomLevelData.ScenarioState.Players.Count, SaveData.Instance.Global.CurrentCustomLevelData.StatBasedOnXOverrideList);
					}
				}
				else
				{
					switch (SaveData.Instance.Global.GameMode)
					{
					case EGameMode.Campaign:
						MonsterClassManager.SetAllMonsterStatLevels(SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.MapParty.InProgressScenarioLevel);
						break;
					case EGameMode.LevelEditor:
						ScenarioManager.CurrentScenarioState.UpdateMonsterClassesFromState();
						break;
					}
				}
				if (AdventureState.MapState == null)
				{
					AdventureState.UpdateMapState(new PartyAdventureData(ScenarioManager.CurrentScenarioState.Seed, "Custom Party", EAdventureDifficulty.Normal, new SaveOwner(), EGameMode.SingleScenario, ERulesFlag.None).AdventureMapState);
					int num3 = 0;
					foreach (PlayerState player in ScenarioManager.CurrentScenarioState.Players)
					{
						CMapCharacter cMapCharacter = new CMapCharacter(player.ClassID, player.CharacterName, 0);
						AdventureState.MapState.MapParty.AddCharacterToCharactersList(cMapCharacter);
						AdventureState.MapState.MapParty.AddSelectedCharacterToSlot(cMapCharacter, num3);
						num3++;
					}
				}
				if (ScenarioManager.CurrentScenarioState?.ScenarioSetup != null)
				{
					ScenarioManager.CurrentScenarioState.ScenarioSetup.IsInTutorial |= Choreographer.IsInTutorial();
				}
				bool applyMapData = AdventureState.MapState != null && SaveData.Instance.Global.GameMode != EGameMode.LevelEditor && (SaveData.Instance.Global.CurrentCustomLevelData == null || (SaveData.Instance.Global.CurrentCustomLevelData.PartySpawnType != ELevelPartyChoiceType.PresetSpawnSpecificLocations && SaveData.Instance.Global.CurrentCustomLevelData.PartySpawnType != ELevelPartyChoiceType.PresetSpawnAtEntrance));
				if (applyMapData && AdventureState.MapState.MapParty.NextScenarioEffects != null)
				{
					if (AdventureState.MapState.MapParty.NextScenarioEffects.AttackModifiers != null && AdventureState.MapState.MapParty.NextScenarioEffects.AttackModifiers.Count() > 0)
					{
						foreach (Tuple<string, Dictionary<string, int>> modifierTuple in AdventureState.MapState.MapParty.NextScenarioEffects.AttackModifiers)
						{
							if (modifierTuple.Item1 == "party" || modifierTuple.Item1 == "NoneID")
							{
								foreach (CMapCharacter cData in AdventureState.MapState.MapParty.SelectedCharacters)
								{
									CharacterClassManager.Classes.SingleOrDefault((CCharacterClass s) => s.ID == cData.CharacterID)?.ApplyEventModifiersToList(modifierTuple.Item2);
								}
								continue;
							}
							CMapCharacter cData2 = AdventureState.MapState.MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter x) => x.CharacterID == modifierTuple.Item1);
							if (cData2 != null)
							{
								CharacterClassManager.Classes.SingleOrDefault((CCharacterClass s) => s.ID == cData2.CharacterID)?.ApplyEventModifiersToList(modifierTuple.Item2);
							}
						}
						AdventureState.MapState.MapParty.NextScenarioEffects.AttackModifiers.Clear();
					}
					if (AdventureState.MapState.MapParty.NextScenarioEffects.ExtraCardFirstRound && ScenarioManager.CurrentScenarioState.IsFirstLoad)
					{
						ScenarioManager.CurrentScenarioState.ScenarioModifiers.Add(new CScenarioModifierApplyActiveBonusToActor("ScenarioAbility_PlayAdditionalCard_OneRound_Self_Mod", ScenarioManager.CurrentScenarioState.ScenarioModifiers.Count + 1, 0, EScenarioModifierTriggerPhase.StartScenario, applyToEachActorOnce: true, new CObjectiveFilter(CAbilityFilter.EFilterActorType.Player), isPositive: true, "ScenarioAbility_PlayAdditionalCard_OneRound_Self", null, EScenarioModifierActivationType.None, null, null, null, null, null, null, isHidden: true));
						AdventureState.MapState.MapParty.NextScenarioEffects.Discard = 0;
					}
					if (AdventureState.MapState.MapParty.NextScenarioEffects.ShowMonsterCardsAtScenarioStart && ScenarioManager.CurrentScenarioState.IsFirstLoad)
					{
						ScenarioManager.CurrentScenarioState.ScenarioModifiers.Add(new CScenarioModifierTriggerAbility("ScenarioAbility_ShowMonsterCardsAtScenarioStart_Mod", ScenarioManager.CurrentScenarioState.ScenarioModifiers.Count + 1, 0, EScenarioModifierTriggerPhase.StartScenario, applyToEachActorOnce: true, new CObjectiveFilter(CAbilityFilter.EFilterActorType.Player), isPositive: true, "ScenarioAbility_ShowMonsterCardsAtScenarioStart", CScenarioModifierTriggerAbility.ETriggerAbilityStackingType.StackToPhaseAbilities, null, new List<TileIndex>(), EScenarioModifierActivationType.None, null, null, null, null, null, null, null, isHidden: true, isDeactivated: false, applyOnceTotal: true));
						AdventureState.MapState.MapParty.NextScenarioEffects.ShowMonsterCardsAtScenarioStart = false;
					}
					if (AdventureState.MapState.MapParty.NextScenarioEffects.RecoverTwoDiscardedCardsEndOfFirstRound && ScenarioManager.CurrentScenarioState.IsFirstLoad)
					{
						ScenarioManager.CurrentScenarioState.ScenarioModifiers.Add(new CScenarioModifierTriggerAbility("Recover_Two_Discarded_Cards_Scenario_Mod", ScenarioManager.CurrentScenarioState.ScenarioModifiers.Count + 1, 0, EScenarioModifierTriggerPhase.StartScenario, applyToEachActorOnce: true, new CObjectiveFilter(CAbilityFilter.EFilterActorType.Player), isPositive: true, "ScenarioAbility_RecoverTwoDiscardedCard_EndOfFirstRound_Self", CScenarioModifierTriggerAbility.ETriggerAbilityStackingType.StackToPhaseAbilities, null, new List<TileIndex>(), EScenarioModifierActivationType.None, null, null, null, null, null, null, null, isHidden: true));
						AdventureState.MapState.MapParty.NextScenarioEffects.Discard = 0;
					}
					if (AdventureState.MapState.MapParty.NextScenarioEffects.RecoverAllDiscardedCardsEndOfFirstRound && ScenarioManager.CurrentScenarioState.IsFirstLoad)
					{
						ScenarioManager.CurrentScenarioState.ScenarioModifiers.Add(new CScenarioModifierTriggerAbility("Recover_All_Discarded_Cards_Scenario_Mod", ScenarioManager.CurrentScenarioState.ScenarioModifiers.Count + 1, 0, EScenarioModifierTriggerPhase.StartScenario, applyToEachActorOnce: true, new CObjectiveFilter(CAbilityFilter.EFilterActorType.Player), isPositive: true, "ScenarioAbility_RecoverAllDiscardedCard_EndOfFirstRound_Self", CScenarioModifierTriggerAbility.ETriggerAbilityStackingType.StackToPhaseAbilities, null, new List<TileIndex>(), EScenarioModifierActivationType.None, null, null, null, null, null, null, null, isHidden: true));
						AdventureState.MapState.MapParty.NextScenarioEffects.Discard = 0;
					}
					if (ScenarioManager.CurrentScenarioState.IsFirstLoad)
					{
						bool ormi = AdventureState.MapState.MapParty.NextScenarioEffects.Ormi1;
						bool ormi2 = AdventureState.MapState.MapParty.NextScenarioEffects.Ormi2;
						if (ormi | ormi2)
						{
							ScenarioManager.CurrentScenarioState.ScenarioModifiers.Add(new CScenarioModifierTriggerAbility("OrmiPreventDamage_Modifier", ScenarioManager.CurrentScenarioState.ScenarioModifiers.Count + 1, 0, EScenarioModifierTriggerPhase.OnActorSpawnedOrRevealed, applyToEachActorOnce: true, new CObjectiveFilter(CAbilityFilter.EFilterActorType.Ally, CAbilityFilter.EFilterEnemy.None, CAbilityFilter.ELootType.None, CAbility.EAbilityType.None, CAbilityFilter.EFilterRevealedType.None, null, new List<string> { "Ormi1ID", "Ormi2ID" }), isPositive: true, "ScenarioAbility_PreventDamage_Ormi", CScenarioModifierTriggerAbility.ETriggerAbilityStackingType.StackToPhaseAbilities, null, new List<TileIndex>(), EScenarioModifierActivationType.None, null, null, "ScenarioAbility_PreventDamage_Ormi"));
						}
						if (ormi)
						{
							ScenarioManager.CurrentScenarioState.ScenarioModifiers.Add(new CScenarioModifierTriggerAbility("Ormi1_Modifier", ScenarioManager.CurrentScenarioState.ScenarioModifiers.Count + 1, 0, EScenarioModifierTriggerPhase.StartScenario, applyToEachActorOnce: true, new CObjectiveFilter(CAbilityFilter.EFilterActorType.Player), isPositive: true, "ScenarioAbility_Ormi1", CScenarioModifierTriggerAbility.ETriggerAbilityStackingType.StackToPhaseAbilities, null, new List<TileIndex>(), EScenarioModifierActivationType.None, null, null, "ScenarioAbility_Ormi1"));
							AdventureState.MapState.MapParty.NextScenarioEffects.Ormi1 = false;
						}
						if (ormi2)
						{
							ScenarioManager.CurrentScenarioState.ScenarioModifiers.Add(new CScenarioModifierTriggerAbility("Ormi2_Modifier", ScenarioManager.CurrentScenarioState.ScenarioModifiers.Count + 1, 0, EScenarioModifierTriggerPhase.StartScenario, applyToEachActorOnce: true, new CObjectiveFilter(CAbilityFilter.EFilterActorType.Player), isPositive: true, "ScenarioAbility_Ormi2", CScenarioModifierTriggerAbility.ETriggerAbilityStackingType.StackToPhaseAbilities, null, new List<TileIndex>(), EScenarioModifierActivationType.None, null, null, "ScenarioAbility_Ormi2"));
							AdventureState.MapState.MapParty.NextScenarioEffects.Ormi1 = false;
						}
					}
					if (AdventureState.MapState.MapParty.NextScenarioEffects.Discard > 0 && ScenarioManager.CurrentScenarioState.IsFirstLoad)
					{
						ScenarioManager.CurrentScenarioState.ScenarioModifiers.Add(new CScenarioModifierTriggerAbility("Discard_Cards_Scenario_Mod", ScenarioManager.CurrentScenarioState.ScenarioModifiers.Count + 1, 0, EScenarioModifierTriggerPhase.StartScenario, applyToEachActorOnce: true, new CObjectiveFilter(CAbilityFilter.EFilterActorType.Player), isPositive: true, "ScenarioAbility_DiscardCard_Self", CScenarioModifierTriggerAbility.ETriggerAbilityStackingType.StackToPhaseAbilities, null, new List<TileIndex>(), EScenarioModifierActivationType.None, null, AdventureState.MapState.MapParty.NextScenarioEffects.Discard, null, null, null, null, null, isHidden: true));
						AdventureState.MapState.MapParty.NextScenarioEffects.Discard = 0;
					}
					if (AdventureState.MapState.MapParty.NextScenarioEffects.DiscardCardFireLightOrDamage > 0 && ScenarioManager.CurrentScenarioState.IsFirstLoad)
					{
						ScenarioManager.CurrentScenarioState.ScenarioModifiers.Add(new CScenarioModifierTriggerAbility("Discard_FireLightOrDamage_Scenario_Mod", ScenarioManager.CurrentScenarioState.ScenarioModifiers.Count + 1, 0, EScenarioModifierTriggerPhase.StartScenario, applyToEachActorOnce: true, new CObjectiveFilter(CAbilityFilter.EFilterActorType.Player), isPositive: true, "ScenarioAbility_DiscardCardFireIceOrDamage_Self", CScenarioModifierTriggerAbility.ETriggerAbilityStackingType.StackToPhaseAbilities, null, new List<TileIndex>(), EScenarioModifierActivationType.None, null, null, null, null, null, null, null, isHidden: true));
						AdventureState.MapState.MapParty.NextScenarioEffects.DiscardCardFireLightOrDamage = 0;
					}
					if (AdventureState.MapState.MapParty.NextScenarioEffects.ConsumeItems != null && AdventureState.MapState.MapParty.NextScenarioEffects.ConsumeItems.Count() > 0)
					{
						for (int num4 = 0; num4 < AdventureState.MapState.MapParty.NextScenarioEffects.ConsumeItems.Count; num4++)
						{
							string item = AdventureState.MapState.MapParty.NextScenarioEffects.ConsumeItems[num4].Item1;
							CObjectiveFilter scenarioModFilter = ((!(item == "party") && !(item == "NoneID")) ? new CObjectiveFilter(CAbilityFilter.EFilterActorType.Player, CAbilityFilter.EFilterEnemy.None, CAbilityFilter.ELootType.None, CAbility.EAbilityType.None, CAbilityFilter.EFilterRevealedType.None, new List<string> { item }) : new CObjectiveFilter(CAbilityFilter.EFilterActorType.Player));
							string consumeSlotString = AdventureState.MapState.MapParty.NextScenarioEffects.ConsumeItems[num4].Item2;
							if (CItem.ItemSlots.SingleOrDefault((CItem.EItemSlot x) => x.ToString() == consumeSlotString) == CItem.EItemSlot.SmallItem)
							{
								ScenarioManager.CurrentScenarioState.ScenarioModifiers.Add(new CScenarioModifierTriggerAbility("Consume_Item_Cards_Scenario_Mod_" + num4, ScenarioManager.CurrentScenarioState.ScenarioModifiers.Count + 1, 0, EScenarioModifierTriggerPhase.StartScenario, applyToEachActorOnce: true, scenarioModFilter, isPositive: true, "ScenarioAbility_ConsumeSmallItem_Self", CScenarioModifierTriggerAbility.ETriggerAbilityStackingType.StackToPhaseAbilities, null, new List<TileIndex>(), EScenarioModifierActivationType.None, null, null, null, null, null, null, null, isHidden: true));
							}
							else
							{
								Log.Error("Invalid slot type for consuming items due to start scenario effect after a Road/City Event");
							}
						}
						AdventureState.MapState.MapParty.NextScenarioEffects.ConsumeItems.Clear();
					}
				}
				if (SaveData.Instance.Global.CurrentAdventureData?.FrosthavenCampaign?.LootCards != null && SaveData.Instance.Global.CurrentAdventureData.FrosthavenCampaign.LootCards.Count > 0)
				{
					List<LootCardState> lootCards = SaveData.Instance.Global.CurrentAdventureData.FrosthavenCampaign.LootCards;
					List<LootCardState> additionalLootCards = SaveData.Instance.Global.CurrentAdventureData.FrosthavenCampaign.AdditionalLootCards;
					List<LootCard> list = new List<LootCard>
					{
						new LootCard
						{
							Type = LootCard.EType.None
						}
					};
					list.AddRange(lootCards.Select((LootCardState x) => new LootCard
					{
						Type = Enum.Parse<LootCard.EType>(x.Type.ToString()),
						Amount = Enum.Parse<LootCard.EAmount>(x.Amount.ToString()),
						Bonus = x.Bonus,
						AvailableCampaignSectionIDs = ((x.AvailableCampaignSections == null) ? null : x.AvailableCampaignSections.ToArray()),
						ID = x.ID
					}));
					if (additionalLootCards != null && additionalLootCards.Count > 0)
					{
						CLootDeck.AdditionalLootCards = additionalLootCards.Select((LootCardState x) => new LootCard
						{
							Type = Enum.Parse<LootCard.EType>(x.Type.ToString()),
							Amount = Enum.Parse<LootCard.EAmount>(x.Amount.ToString()),
							Bonus = x.Bonus,
							AvailableCampaignSectionIDs = ((x.AvailableCampaignSections == null) ? null : x.AvailableCampaignSections.ToArray()),
							ID = x.ID
						}).ToArray();
						list.AddRange(CLootDeck.AdditionalLootCards);
					}
					else
					{
						CLootDeck.AdditionalLootCards = null;
					}
					CLootDeck.AllCards = list.ToArray();
				}
				else
				{
					CLootDeck.AllCards = null;
					CLootDeck.AdditionalLootCards = null;
				}
				if (InvalidatedScenarioSave)
				{
					InvalidatedScenarioSave = false;
					List<ErrorMessage.LabelAction> list2 = new List<ErrorMessage.LabelAction>();
					list2.Add(new ErrorMessage.LabelAction("GUI_ERROR_RESET_SCENARIO_BUTTON", ErrorHandlingUnloadSceneResetScenarioStateAndRetryLoad, InputFHManager.Controls.Generic.AlternativeChoice));
					list2.Add(new ErrorMessage.LabelAction("GUI_ERROR_MAIN_MENU_BUTTON", () =>
					{
						SceneController.Instance?.LoadMainMenu();
					}, InputFHManager.Controls.Generic.Confirm));
					SceneController.Instance.GlobalErrorMessage.ShowMultiChoiceMessage("GUI_ERROR_INVALIDATED_SCENARIO_SAVE", list2, "GUI_ERROR_TITLE", string.Empty, null, showErrorReportButton: false, trackDebug: true, allowMessageBox: false);
					return;
				}
				await Choreographer.s_Choreographer.ScenarioCreateClientBoardAndCharacters(profiling);
				if (!Application.isPlaying)
				{
					return;
				}
				Choreographer.s_Choreographer.ScenarioImportProcessedCallback(generatedScenario);
				CacheDataFromCampaign();
				if (applyMapData)
				{
					ApplyMapData();
				}
				InitObjectivesUI();
				TransitionManager.s_Instance?.FadeInLevel();
				Choreographer.s_Choreographer.m_GameScenarioScreen.SetActive(SaveData.Instance.Global.GameMode != EGameMode.LevelEditor);
				Log.SimpleInfo("RNG STATES (end of scenario loaded callback): \nScenarioRNG:" + ScenarioManager.CurrentScenarioState.PeekScenarioRNG + "\nEnemyIDRNG:" + ScenarioManager.CurrentScenarioState.PeekEnemyIDRNG + "\nEnemyAbilityCardRNG:" + ScenarioManager.CurrentScenarioState.PeekEnemyAbilityCardRNG + "\nGuidRNG:" + ScenarioManager.CurrentScenarioState.PeekGuidRNG);
				ScenarioManager.CurrentScenarioState.LoadingScenarioState = false;
				if (!LevelEditorController.s_Instance.IsEditing)
				{
					Singleton<LevelEventsController>.Instance.RemoveAlreadyPlayedMessages();
					InteractabilityManager.s_Instance?.LoadDefaultMessagelessProfile();
					Singleton<LevelEventsController>.Instance.TickEvents(ScenarioManager.CurrentScenarioState.IsFirstLoad);
					if (PlayerRegistry.MyPlayer != null)
					{
						if (!PlayerRegistry.MyPlayer.SentPlayerReadyForAssignment)
						{
							PlayerRegistry.MyPlayer.PlayerReadyForAssignment = true;
							PlayerRegistry.MyPlayer.SentPlayerReadyForAssignment = true;
							Synchronizer.SendSideAction(GameActionType.ReadyForAssignment);
						}
					}
					else
					{
						Log.Error("PlayerRegistry.MyPlayer is null in OnSceneLoaded!");
					}
					if (!FastReload.HasPendingFastAutoSaveToLoad())
					{
						if (ScenarioManager.CurrentScenarioState.IsFirstLoad)
						{
							PhaseManager.SetNextPhase(CPhase.PhaseType.StartScenarioEffects);
						}
						else
						{
							s_SkipNextAutoSave = true;
							PhaseManager.SetNextPhase(CPhase.PhaseType.Autosave);
						}
					}
				}
				Main.Unpause3DWorld(forceUnpause: true);
				SceneController.Instance.ScenarioIsLoading = true;
				CoroutineHelper.RunCoroutine(WaitForLoad());
				UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.ScenarioLoaded));
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "An exception occurred within the OnSceneLoaded callback.  The scene loading did not complete successfully.\n");
				if (SaveData.Instance.Global.GameMode == EGameMode.Campaign)
				{
					if (SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.LoadInitialState)
					{
						List<ErrorMessage.LabelAction> list3 = new List<ErrorMessage.LabelAction>();
						list3.Add(new ErrorMessage.LabelAction("GUI_ERROR_RESET_QUEST_BUTTON", ErrorHandlingUnloadSceneResetQuestStateAndReturnToMap, InputFHManager.Controls.Generic.AlternativeChoice));
						list3.Add(new ErrorMessage.LabelAction("GUI_ERROR_MAIN_MENU_BUTTON", SceneController.Instance.LoadMainMenu, InputFHManager.Controls.Generic.Confirm));
						SceneController.Instance.GlobalErrorMessage.ShowMultiChoiceMessageDefaultTitle("ERROR_RUNTIME_00002", ex.StackTrace, list3, ex.Message);
					}
					else
					{
						List<ErrorMessage.LabelAction> list4 = new List<ErrorMessage.LabelAction>();
						list4.Add(new ErrorMessage.LabelAction("GUI_ERROR_RESET_SCENARIO_BUTTON", ErrorHandlingUnloadSceneResetScenarioStateAndRetryLoad, InputFHManager.Controls.Generic.AlternativeChoice));
						list4.Add(new ErrorMessage.LabelAction("GUI_ERROR_MAIN_MENU_BUTTON", SceneController.Instance.LoadMainMenu, InputFHManager.Controls.Generic.Confirm));
						SceneController.Instance.GlobalErrorMessage.ShowMultiChoiceMessageDefaultTitle("ERROR_RUNTIME_00002", ex.StackTrace, list4, ex.Message);
					}
				}
				else
				{
					List<ErrorMessage.LabelAction> list5 = new List<ErrorMessage.LabelAction>();
					list5.Add(new ErrorMessage.LabelAction("GUI_ERROR_MAIN_MENU_BUTTON", SceneController.Instance.LoadMainMenu, InputFHManager.Controls.Generic.AlternativeChoice));
					SceneController.Instance.GlobalErrorMessage.ShowMultiChoiceMessageDefaultTitle("ERROR_RUNTIME_00002", ex.StackTrace, list5, ex.Message);
				}
			}
		}
	}

	public static void CacheDataFromCampaign()
	{
		if (ScenarioManager.CurrentScenarioState.ScenarioSetup == null)
		{
			ScenarioManager.CurrentScenarioState.ScenarioSetup = new FHScenarioSetup();
		}
		ScenarioManager.CurrentScenarioState.ScenarioSetup.CachedScenarioData = new Dictionary<uint, string>();
		DefRepository defRepository = YMLDefRepository.Repo.DefRepository;
		foreach (KeyValuePair<ScenarioID, ScenarioDef> scenario in defRepository.Scenarios)
		{
			string value = scenario.Value.Name.Replace(' ', '_');
			ScenarioManager.CurrentScenarioState.ScenarioSetup.CachedScenarioData[scenario.Key.ID] = value;
		}
		if (SaveData.Instance?.Global?.CurrentAdventureData?.FrosthavenCampaign != null)
		{
			CampaignController campaignController = new CampaignController(SaveData.Instance.Global.CurrentAdventureData.FrosthavenCampaign, defRepository);
			{
				foreach (ItemDef item in defRepository.Items.Values)
				{
					ItemCardYMLData itemCardYMLData = ScenarioRuleClient.SRLYML.ItemCards.SingleOrDefault((ItemCardYMLData x) => x.ID == item.ScenarioItemID);
					if (itemCardYMLData == null)
					{
						continue;
					}
					int count = campaignController.CountItemInstancesInCharacters(item);
					itemCardYMLData.ModifyItemCount(count);
					if (itemCardYMLData.Usage == CItem.EUsageType.Flip)
					{
						int flipItemID = int.Parse(itemCardYMLData.FlipItemID);
						ScenarioRuleClient.SRLYML.ItemCards.SingleOrDefault((ItemCardYMLData x) => x.ID == flipItemID)?.ModifyItemCount(count);
					}
				}
				return;
			}
		}
		foreach (ItemCardYMLData itemCard in ScenarioRuleClient.SRLYML.ItemCards)
		{
			itemCard.ModifyItemCount(0);
		}
	}

	public static void ApplyMapData()
	{
		bool isFirstLoad = ScenarioManager.CurrentScenarioState.IsFirstLoad;
		ScenarioManager.CurrentScenarioState.SummonHealthModifier = 1f;
		List<CObjectProp> list = ScenarioManager.CurrentScenarioState.DestroyedProps.ToList();
		foreach (ObjectState deadObject in ScenarioManager.CurrentScenarioState.Objects.Where((ObjectState x) => x.IsDead))
		{
			if (!string.IsNullOrEmpty(deadObject.PropGuidAttachedTo))
			{
				CObjectProp cObjectProp = ScenarioManager.CurrentScenarioState.Props.SingleOrDefault((CObjectProp x) => x.PropGuid == deadObject.PropGuidAttachedTo);
				if (cObjectProp != null && cObjectProp.PropHealthDetails != null && cObjectProp.PropHealthDetails.DeathAction != PropHealthDetails.EDeathAction.Destruct)
				{
					list.Add(cObjectProp);
				}
			}
		}
		foreach (CObjectProp item2 in list.Distinct())
		{
			Choreographer.s_Choreographer.DestroyPropImmediate(item2, null, ScenarioManager.CurrentScenarioState.DestroyedProps.Contains(item2));
		}
		foreach (CObjectProp item3 in ScenarioManager.CurrentScenarioState.Players.SelectMany((PlayerState x) => x.CarriedQuestProps))
		{
			Choreographer.s_Choreographer.DestroyPropImmediate(item3);
		}
		for (int num = ScenarioManager.CurrentScenarioState.ChestProps.Count - 1; num >= 0; num--)
		{
			CObjectChest cObjectChest = (CObjectChest)ScenarioManager.CurrentScenarioState.ChestProps[num];
			if (cObjectChest.ObjectType != ScenarioManager.ObjectImportType.GoalChest)
			{
				bool flag = true;
				if (cObjectChest.ChestTreasureTablesID != null && cObjectChest.ChestTreasureTablesID.Count > 0)
				{
					foreach (string item4 in cObjectChest.ChestTreasureTablesID)
					{
						if (AdventureState.MapState.AlreadyRewardedChestTreasureTableIDs.Contains(item4))
						{
							flag = false;
							break;
						}
					}
				}
				if (cObjectChest.Type != CObjectChest.EType.None && (AdventureState.MapState.AlreadyRewardedChestTreasureTableIDs.Contains(cObjectChest.Type.ToString()) || (ScenarioManager.CurrentScenarioState.ScenarioSetup != null && ScenarioManager.CurrentScenarioState.ScenarioSetup.AlreadyLootedChests.Contains(cObjectChest.Type.ToString()))))
				{
					flag = false;
				}
				if (!flag)
				{
					cObjectChest.DestroyProp();
				}
			}
		}
		if (AdventureState.MapState?.MapParty?.NextScenarioEffects == null)
		{
			return;
		}
		if (AdventureState.MapState.Difficulty.HasSummonHealthModifier)
		{
			ScenarioManager.CurrentScenarioState.SummonHealthModifier = AdventureState.MapState.Difficulty.SummonHealthModifier;
		}
		if (AdventureState.MapState.Difficulty.HasFreeLongRest)
		{
			ScenarioManager.CurrentScenarioState.FreeLongRest = AdventureState.MapState.Difficulty.FreeLongRest;
		}
		if (AdventureState.MapState.Difficulty.HasFreeShortRestButMuddle)
		{
			ScenarioManager.CurrentScenarioState.FreeShortRestButMuddle = AdventureState.MapState.Difficulty.FreeShortRestButMuddle;
		}
		foreach (ElementInfusionBoardManager.EElement infusion in AdventureState.MapState.MapParty.NextScenarioEffects.Infusions)
		{
			ElementInfusionBoardManager.Infuse(infusion, null);
			ElementInfusionBoardManager.ProcessEndTurnInfusions();
		}
		AdventureState.MapState.MapParty.NextScenarioEffects.Infusions.Clear();
		if (AdventureState.MapState.MapParty.NextScenarioEffects.MonsterAttackModsToSet != null && AdventureState.MapState.MapParty.NextScenarioEffects.MonsterAttackModsToSet.Count > 0)
		{
			IEnumerable<AttackModifierYMLData> cardPool = AdventureState.MapState.MapParty.NextScenarioEffects.MonsterAttackModsToSet.Select((string x) => ScenarioRuleClient.SRLYML.AttackModifiers.SingleOrDefault((AttackModifierYMLData y) => y.Name == x));
			MonsterClassManager.EnemyMonsterAttackModifierDeck.SetPoolAndLoadCards(ScenarioManager.CurrentScenarioState.EnemyClassManager.EnemyAttackModifierDeck, cardPool, isFirstLoad, isFirstLoad);
			MonsterClassManager.Enemy2MonsterAttackModifierDeck.SetPoolAndLoadCards(ScenarioManager.CurrentScenarioState.EnemyClassManager.Enemy2MonsterAttackModifierDeck, cardPool, isFirstLoad, isFirstLoad);
		}
		if (ScenarioManager.CurrentScenarioState.IsFirstLoad)
		{
			foreach (CMapCharacter selectedCharacter in AdventureState.MapState.MapParty.SelectedCharacters)
			{
				int num2 = AdventureState.MapState.MapParty.NextScenarioEffects.Damage + (int)selectedCharacter.DamageOnScenarioStart;
				if (num2 > 0)
				{
					selectedCharacter.ApplyMapDamage(num2, cappedToOneHealth: true);
					selectedCharacter.DamageOnScenarioStart = 0u;
				}
			}
			AdventureState.MapState.MapParty.NextScenarioEffects.Damage = 0;
		}
		if (ScenarioManager.CurrentScenarioState.IsFirstLoad)
		{
			foreach (CMapCharacter selectedCharacter2 in AdventureState.MapState.MapParty.SelectedCharacters)
			{
				int increasedHealth = (int)selectedCharacter2.IncreasedHealth;
				if (increasedHealth > 0)
				{
					selectedCharacter2.ApplyMapHealth(increasedHealth);
					selectedCharacter2.IncreasedHealth = 0u;
				}
			}
		}
		foreach (CMapCharacter mapCharacter in AdventureState.MapState.MapParty.SelectedCharacters)
		{
			if (ScenarioManager.CurrentScenarioState.IsFirstLoad)
			{
				CharacterClassManager.SetPerks(mapCharacter.Perks.Where((CharacterPerk w) => w.IsActive).ToList());
				CharacterClassManager.SetPerksComplete(mapCharacter.CharacterID);
			}
			else
			{
				CharacterClassManager.AddPerks(mapCharacter.Perks.Where((CharacterPerk w) => w.IsActive).ToList());
			}
			if (SaveData.Instance.Global.GameMode != EGameMode.SingleScenario && SaveData.Instance.Global.GameMode != EGameMode.FrontEndTutorial && ScenarioManager.CurrentScenarioState.IsFirstLoad)
			{
				mapCharacter.ApplyMapConditions(Choreographer.s_Choreographer.PostMessageFromUI);
			}
			mapCharacter.SetActorPersonalQuest();
			if (ScenarioManager.CurrentScenarioState.IsFirstLoad)
			{
				CharacterClassManager.Classes.Single((CCharacterClass s) => s.ID == mapCharacter.CharacterID).ResetCardAbilitiesAndEnhancementsOnly();
				mapCharacter.ApplyEnhancements();
			}
			if (ScenarioManager.CurrentScenarioState.IsFirstLoad)
			{
				PlayerState playerState = ScenarioManager.CurrentScenarioState.Players.SingleOrDefault((PlayerState p) => p.ClassID == mapCharacter.CharacterID);
				mapCharacter.ApplyPassiveItems(playerState);
			}
		}
		foreach (PlayerState player in ScenarioManager.CurrentScenarioState.Players)
		{
			if (player.CheckPerkBonuses == null)
			{
				continue;
			}
			CActor actor = player.Actor;
			if (actor != null)
			{
				List<CActiveBonus> source = actor.FindApplicableActiveBonuses();
				foreach (string perkKey in player.CheckPerkBonuses)
				{
					PerksYMLData perksYMLData = actor.CharacterClass.Perks.FirstOrDefault((PerksYMLData x) => x.ID == perkKey);
					if (perksYMLData != null)
					{
						List<CActiveBonus> list2 = source.Where((CActiveBonus x) => x.Ability.PerkID == perkKey).ToList();
						if (list2.Count > 0)
						{
							foreach (CActiveBonus item5 in list2)
							{
								item5.Finish();
							}
						}
						foreach (CAbility ability in perksYMLData.Abilities)
						{
							CAbility cAbility = CAbility.CopyAbility(ability, generateNewID: false);
							cAbility.Start(actor, actor);
							cAbility.ApplyToActor(actor);
						}
						continue;
					}
					CScenarioModifier modifier = ScenarioManager.CurrentScenarioState.ScenarioModifiers.FirstOrDefault((CScenarioModifier x) => x.ScenarioAbilityID == perkKey);
					if (modifier != null)
					{
						List<CActiveBonus> list3 = source.Where((CActiveBonus x) => x.Ability.GetAbilityBaseCard() == modifier).ToList();
						if (list3.Count > 0)
						{
							foreach (CActiveBonus item6 in list3)
							{
								item6.Finish();
							}
						}
						foreach (CAbility item7 in modifier.TriggerAbilities(actor, ScenarioManager.CurrentScenarioState.Players.Count))
						{
							CAbility cAbility2 = CAbility.CopyAbility(item7, generateNewID: false);
							cAbility2.Start(actor, actor);
							cAbility2.ApplyToActor(actor);
						}
						continue;
					}
					IEnumerable<CItem> enumerable = actor.Inventory.AllItemsAndPets.Where((CItem x) => x.FindAbilityOnItemCard(perkKey) != null);
					if (enumerable == null || !enumerable.Any())
					{
						continue;
					}
					foreach (CItem item in enumerable)
					{
						List<CActiveBonus> list4 = source.Where((CActiveBonus x) => x.Ability.GetAbilityBaseCard() == item).ToList();
						if (list4.Count > 0)
						{
							foreach (CActiveBonus item8 in list4)
							{
								item8.Finish();
							}
						}
						actor.ActivatePassiveItems(firstLoad: true, player, item);
					}
				}
			}
			player.CheckPerkBonuses = null;
		}
		if (isFirstLoad)
		{
			while (AdventureState.MapState.MapParty.SelectedCharacters.Any((CMapCharacter x) =>
			{
				List<PositiveConditionPair> tempBlessConditions = x.TempBlessConditions;
				return tempBlessConditions != null && tempBlessConditions.Count > 0;
			}))
			{
				foreach (CMapCharacter selectedCharacter3 in AdventureState.MapState.MapParty.SelectedCharacters)
				{
					selectedCharacter3.ApplyMapBlessCondition();
				}
			}
		}
		ApplyNextScenarioEnemyConditions(AdventureState.MapState.MapParty.NextScenarioEffects.EnemyNegativeConditions, AdventureState.MapState.MapParty.NextScenarioEffects.EnemyPositiveConditions);
		ScenarioManager.CurrentScenarioState.FinishLoadingChallenges();
		if (isFirstLoad)
		{
			CChallenge.CheckTrigger(CChallenge.Trigger.OnStartScenario);
		}
	}

	private static void ApplyNextScenarioEnemyConditions(List<NegativeConditionPair> enemyNegativeConditions, List<PositiveConditionPair> enemyPositiveConditions)
	{
		if (SaveData.Instance.Global.CurrentGameState != EGameState.Scenario || (enemyNegativeConditions.IsNullOrEmpty() && enemyNegativeConditions.IsNullOrEmpty()))
		{
			return;
		}
		bool flag = true;
		foreach (CEnemyActor enemy in ScenarioManager.Scenario.Enemies)
		{
			if (!enemyNegativeConditions.IsNullOrEmpty())
			{
				foreach (NegativeConditionPair enemyNegativeCondition in enemyNegativeConditions)
				{
					if (flag || enemyNegativeCondition.NegativeCondition != CCondition.ENegativeCondition.Curse)
					{
						enemy.ApplyCondition(enemy, enemyNegativeCondition.NegativeCondition, enemyNegativeCondition.RoundDuration, EConditionDecTrigger.Rounds, "", isMapCondition: true);
					}
				}
			}
			if (!enemyPositiveConditions.IsNullOrEmpty())
			{
				foreach (PositiveConditionPair enemyPositiveCondition in enemyPositiveConditions)
				{
					if (flag || enemyPositiveCondition.PositiveCondition != CCondition.EPositiveCondition.Bless)
					{
						enemy.ApplyCondition(enemy, enemyPositiveCondition.PositiveCondition, enemyPositiveCondition.RoundDuration, EConditionDecTrigger.Rounds, "", isMapCondition: true);
					}
				}
			}
			flag = false;
		}
		enemyNegativeConditions?.Clear();
		enemyPositiveConditions?.Clear();
	}

	public static void InitObjectivesUI()
	{
		ScenarioRuleLibrary.ScenarioState currentState = Choreographer.s_Choreographer.m_CurrentState;
		List<CBattleGoalState> battleGoalStates = null;
		List<CBattleGoalState> masteriesStates = null;
		if (AdventureState.MapState?.CurrentMapScenarioState != null)
		{
			battleGoalStates = AdventureState.MapState.CurrentMapScenarioState.ChosenBattleGoalStates.Select((Tuple<string, CBattleGoalState> s) => s.Item2).ToList();
			masteriesStates = AdventureState.MapState.CurrentMapScenarioState.MasteryBattleGoalStates.Select((Tuple<string, CBattleGoalState> s) => s.Item2).ToList();
		}
		Singleton<ScenarioInfoModule>.Instance.InitScenario(currentState.ID, currentState.WinObjectives, currentState.LoseObjectives, currentState.ScenarioModifiers, battleGoalStates, masteriesStates);
	}

	public static IEnumerator WaitForLoad()
	{
		using (new MiniProfilerScope("UnityGameEditorRuntime.WaitForLoad"))
		{
			if (FFSNetwork.IsOnline && PlayerRegistry.WaitForOtherPlayers && PlayerRegistry.MyPlayer != null && PlayerRegistry.MyPlayer.IsParticipant)
			{
				Synchronizer.SendSideAction(GameActionType.NotifyLoadingFinished);
				PlayerRegistry.NotifyLoadingFinished(PlayerRegistry.MyPlayer);
				if (PlayerRegistry.WaitForOtherPlayers)
				{
					yield return SceneController.Instance.WaitForPlayers();
				}
			}
			using (new MiniProfilerScope("UnityGameEditorRuntime.WaitForLoad.wait IsLoadingComplete"))
			{
				yield return null;
				yield return new WaitUntil(() => EnvironmentLoader.IsInstanceValid && EnvironmentLoader.Instance.IsLoadingComplete);
			}
			yield return FastReload.LoadAfterNormalLoad(SaveData.Instance.Global.CurrentAdventureData?.FastReloadState);
			SceneController.Instance.ScenarioIsLoading = false;
		}
	}

	public static List<GameObject> FindUnityGameObjects(GameObject rootGameObject, ScenarioManager.ObjectImportType objectType, bool findEmptyTiles = false)
	{
		List<GameObject> list = new List<GameObject>();
		foreach (Transform item in rootGameObject.transform)
		{
			UnityGameEditorObject component = item.gameObject.GetComponent<UnityGameEditorObject>();
			if (component != null && component.m_ObjectType == objectType)
			{
				if (((component.m_ObjectType == ScenarioManager.ObjectImportType.Tile) & findEmptyTiles) && component.m_ProcGenGameObjectsOnTile.Count > 0)
				{
					continue;
				}
				list.Add(item.gameObject);
			}
			list.AddRange(FindUnityGameObjects(item.gameObject, objectType));
		}
		return list;
	}

	public static void GetAllChildren(Transform current, List<GameObject> arrayToFill)
	{
		arrayToFill.Add(current.gameObject);
		for (int i = 0; i < current.childCount; i++)
		{
			GetAllChildren(current.GetChild(i), arrayToFill);
		}
	}

	public static List<GameObject> GetAllSceneGameObjects(Scene scene)
	{
		List<GameObject> list = new List<GameObject>();
		GameObject[] rootGameObjects = scene.GetRootGameObjects();
		for (int i = 0; i < rootGameObjects.Length; i++)
		{
			GetAllChildren(rootGameObjects[i].transform, list);
		}
		return list;
	}

	public static void ErrorHandlingUnloadSceneAndLoadMainMenu()
	{
		Log.GUIInfo("Error ErrorHandlingUnloadSceneAndLoadMainMenu");
		SceneController.Instance.GlobalErrorMessage.SuppressShowingErrorMessages = true;
		UIManager.LoadMainMenu(null, () =>
		{
			SceneController.Instance.GlobalErrorMessage.SuppressShowingErrorMessages = false;
		});
	}

	public static void ErrorHandlingUnloadSceneResetScenarioStateAndRetryLoad()
	{
		SceneController.Instance.GlobalErrorMessage.SuppressShowingErrorMessages = true;
		UIManager.RegenerateAndRestartScenario(() =>
		{
			SceneController.Instance.GlobalErrorMessage.SuppressShowingErrorMessages = false;
		});
	}

	public static void ErrorHandlingUnloadSceneResetQuestStateAndReturnToMap()
	{
		SceneController.Instance.GlobalErrorMessage.SuppressShowingErrorMessages = true;
		Choreographer.s_Choreographer.EndGame(EResult.Resign, () =>
		{
			SceneController.Instance.GlobalErrorMessage.SuppressShowingErrorMessages = false;
		});
	}

	public static void GetObjectsInScenario(Scene scene, ScenarioRuleLibrary.ScenarioState scenarioState)
	{
		new List<CObjectProp>();
		foreach (GameObject allSceneGameObject in GetAllSceneGameObjects(scene))
		{
			if (allSceneGameObject.activeInHierarchy)
			{
				UnityGameEditorObject component = allSceneGameObject.GetComponent<UnityGameEditorObject>();
				if (component != null)
				{
					_ = component.m_ExcludeFromExport;
				}
			}
		}
	}

	public static IEnumerator InitialiseScenarioMap(ScenarioRuleLibrary.ScenarioState scenarioState, GameObject root)
	{
		using (RespectFrameTime respect = new RespectFrameTime("UnityGameEditorRuntime.InitialiseScenarioMap"))
		{
			if (s_TileSize.magnitude == 0f)
			{
				Initialise();
			}
			if (scenarioState.IsInitialised)
			{
				foreach (Transform mapTransform in root.transform)
				{
					if (respect.ShouldWait())
					{
						yield return respect.WaitUntilNextFrameIfNeeded();
					}
					ApparanceMap mapInfo = mapTransform.GetComponent<ApparanceMap>();
					if (mapInfo == null)
					{
						continue;
					}
					CMap map = scenarioState.Maps.Single((CMap s) => s.MapInstanceName == mapTransform.name);
					foreach (Transform tileTransform in mapTransform.transform)
					{
						if (respect.ShouldWait())
						{
							yield return respect.WaitUntilNextFrameIfNeeded();
						}
						GameObject tileGO = tileTransform.gameObject;
						UnityGameEditorObject component = tileGO.GetComponent<UnityGameEditorObject>();
						if (!(component != null) || (component.m_ObjectType != ScenarioManager.ObjectImportType.EdgeTile && component.m_ObjectType != ScenarioManager.ObjectImportType.Tile))
						{
							continue;
						}
						Vector3Int intPositiveSpaceSnapSpace = MF.GetTileIntegerSnapSpace(tileGO.transform.position) + new Vector3Int(scenarioState.PositiveSpaceOffset.X, scenarioState.PositiveSpaceOffset.Y, scenarioState.PositiveSpaceOffset.Z);
						try
						{
							if (map.MapTiles == null || map.MapTiles.Count == 0)
							{
								throw new Exception("No maptiles");
							}
							CMapTile cMapTile = map.MapTiles.Single((CMapTile s) => s.ArrayIndex.X == intPositiveSpaceSnapSpace.x && s.ArrayIndex.Y == intPositiveSpaceSnapSpace.z && Vector3.Distance(GloomUtility.CVToV(s.Position), tileGO.transform.position) < 0.1f);
							tileGO.name = cMapTile.TileGuid;
						}
						catch (Exception ex)
						{
							Log.Error($"Error overlapping tile in map {mapInfo.RoomName} in {scenarioState.BakingID} at: {intPositiveSpaceSnapSpace.x} {intPositiveSpaceSnapSpace.z}; Let Level Design team know. ({ex.GetType().Name}) {ex.Message}. The maptile prefab needs to be fixed and the level needs to be re-saved.");
							if (PlayerPrefs.GetInt("KronosFixOverlapHexesTool") != 1 || SaveData.Instance.Global.GameMode != EGameMode.LevelEditor)
							{
								continue;
							}
							List<CMapTile> list = map.MapTiles.FindAll((CMapTile s) => s.ArrayIndex.X == intPositiveSpaceSnapSpace.x && s.ArrayIndex.Y == intPositiveSpaceSnapSpace.z && Vector3.Distance(GloomUtility.CVToV(s.Position), tileGO.transform.position) < 0.1f);
							if (list.Count > 0)
							{
								for (int num = 1; num < list.Count; num++)
								{
									map.MapTiles.Remove(list[num]);
								}
								CMapTile cMapTile2 = map.MapTiles.Single((CMapTile s) => s.ArrayIndex.X == intPositiveSpaceSnapSpace.x && s.ArrayIndex.Y == intPositiveSpaceSnapSpace.z && Vector3.Distance(GloomUtility.CVToV(s.Position), tileGO.transform.position) < 0.1f);
								tileGO.name = cMapTile2.TileGuid;
							}
							else
							{
								CMapTile cMapTile3 = new CMapTile(component.m_ObjectType == ScenarioManager.ObjectImportType.EdgeTile, new TileIndex(intPositiveSpaceSnapSpace.x, intPositiveSpaceSnapSpace.z), GloomUtility.VToCV(tileTransform.position), SharedClient.GlobalRNG);
								tileTransform.name = cMapTile3.TileGuid;
								map.MapTiles.Add(cMapTile3);
							}
						}
					}
				}
				yield break;
			}
			Vector3Int minBounds = new Vector3Int(int.MaxValue, 0, int.MaxValue);
			Vector3Int maxBounds = new Vector3Int(int.MinValue, 0, int.MinValue);
			_ = Vector3Int.zero;
			_ = Vector3Int.zero;
			bool minZTileStaggered = false;
			foreach (Transform item in root.transform)
			{
				foreach (Transform tileTransform in item.transform)
				{
					if (respect.ShouldWait())
					{
						yield return respect.WaitUntilNextFrameIfNeeded();
					}
					try
					{
						UnityGameEditorObject component2 = tileTransform.gameObject.GetComponent<UnityGameEditorObject>();
						if (component2 != null && (component2.m_ObjectType == ScenarioManager.ObjectImportType.EdgeTile || component2.m_ObjectType == ScenarioManager.ObjectImportType.Tile))
						{
							Vector3Int tileIntegerSnapSpace = MF.GetTileIntegerSnapSpace(tileTransform.position);
							if (minBounds.z > tileIntegerSnapSpace.z)
							{
								minZTileStaggered = (Mathf.Abs(tileIntegerSnapSpace.z) & 1) == 1;
							}
							minBounds.x = ((minBounds.x > tileIntegerSnapSpace.x) ? tileIntegerSnapSpace.x : minBounds.x);
							minBounds.z = ((minBounds.z > tileIntegerSnapSpace.z) ? tileIntegerSnapSpace.z : minBounds.z);
							maxBounds.x = ((maxBounds.x < tileIntegerSnapSpace.x) ? tileIntegerSnapSpace.x : maxBounds.x);
							maxBounds.z = ((maxBounds.z < tileIntegerSnapSpace.z) ? tileIntegerSnapSpace.z : maxBounds.z);
						}
					}
					catch (Exception ex2)
					{
						if (!ErrorMessage(ex2))
						{
							throw;
						}
					}
				}
			}
			Vector3Int positiveSpaceOffset = new Vector3Int(-minBounds.x + 1, 0, -minBounds.z);
			if (minZTileStaggered)
			{
				positiveSpaceOffset.z++;
			}
			Vector3Int boardDimensions = new Vector3Int(maxBounds.x - minBounds.x + 1, 0, maxBounds.z - minBounds.z + 1);
			int index = 0;
			foreach (Transform item2 in root.transform)
			{
				ApparanceMap mapInfo2 = item2.gameObject.GetComponent<ApparanceMap>();
				if (mapInfo2 == null)
				{
					continue;
				}
				CMap map = scenarioState.Maps.Single((CMap s) => s.MapGuid == mapInfo2.MapGuid);
				item2.name = map.MapInstanceName;
				foreach (Transform tileTransform in item2.transform)
				{
					if (respect.ShouldWait())
					{
						yield return respect.WaitUntilNextFrameIfNeeded();
					}
					try
					{
						UnityGameEditorObject component3 = tileTransform.gameObject.GetComponent<UnityGameEditorObject>();
						if (component3 != null && (component3.m_ObjectType == ScenarioManager.ObjectImportType.EdgeTile || component3.m_ObjectType == ScenarioManager.ObjectImportType.Tile))
						{
							Vector3Int vector3Int = MF.GetTileIntegerSnapSpace(tileTransform.position) + positiveSpaceOffset;
							CMapTile cMapTile4 = new CMapTile(rng: (ScenarioManager.CurrentScenarioState == null || ScenarioManager.CurrentScenarioState.GuidRNG == null) ? SharedClient.GlobalRNG : ScenarioManager.CurrentScenarioState.GuidRNG, edgeTile: component3.m_ObjectType == ScenarioManager.ObjectImportType.EdgeTile, arrayIndex: new TileIndex(vector3Int.x, vector3Int.z), position: GloomUtility.VToCV(tileTransform.position));
							tileTransform.name = cMapTile4.TileGuid;
							map.MapTiles.Add(cMapTile4);
						}
					}
					catch (Exception ex3)
					{
						if (!ErrorMessage(ex3))
						{
							throw;
						}
					}
				}
				index++;
			}
			try
			{
				CVectorInt3 positiveSpaceOffset2 = new CVectorInt3(positiveSpaceOffset.x, positiveSpaceOffset.y, positiveSpaceOffset.z);
				scenarioState.InitScenarioState(boardDimensions.x, boardDimensions.z, positiveSpaceOffset2);
			}
			catch (Exception ex4)
			{
				if (!ErrorMessage(ex4))
				{
					throw;
				}
			}
		}
		static bool ErrorMessage(Exception ex5)
		{
			if (!Application.isPlaying)
			{
				return false;
			}
			Log.Exception(ex5, "An exception occurred while Initialising Scenario\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_RUNTIME_00004", "GUI_ERROR_MAIN_MENU_BUTTON", ex5.StackTrace, ErrorHandlingUnloadSceneAndLoadMainMenu, ex5.Message);
			return true;
		}
	}

	public static void GenerateStateFromScene(ScenarioRuleLibrary.ScenarioState scenarioState)
	{
	}

	public static void MakeDoor(GameObject doorGO, ScenarioRuleLibrary.ScenarioState state, bool firstLoad, bool levelEditorLevel = false, ApparanceMap mapInfo = null)
	{
		UnityGameEditorDoorProp component = doorGO.GetComponent<UnityGameEditorDoorProp>();
		if (mapInfo == null)
		{
			mapInfo = doorGO.GetComponentInParent<ApparanceMap>();
		}
		CMap cMap = state.Maps.Single((CMap s) => s.MapGuid == mapInfo.MapGuid);
		if (firstLoad)
		{
			GameObject mapSceneRoot = Choreographer.s_Choreographer.m_MapSceneRoot;
			UnityGameEditorObject component2 = doorGO.GetComponent<UnityGameEditorObject>();
			Vector3Int vector3Int = MF.GetTileIntegerSnapSpace(doorGO.transform.position) + new Vector3Int(state.PositiveSpaceOffset.X, state.PositiveSpaceOffset.Y, state.PositiveSpaceOffset.Z);
			FindUnityGameObjects(mapSceneRoot.FindInChildren(cMap.MapInstanceName), ScenarioManager.ObjectImportType.EdgeTile).Find((GameObject x) => x.transform.position == doorGO.transform.position);
			CObjectDoor cObjectDoor = new CObjectDoor(component2.m_PrefabName, component2.m_ObjectType, new TileIndex(vector3Int.x, vector3Int.z), new CVector3(doorGO.transform.position.x, doorGO.transform.position.y, doorGO.transform.position.z), new CVector3(doorGO.transform.eulerAngles.x, doorGO.transform.eulerAngles.y, doorGO.transform.eulerAngles.z), cMap.MapGuid, component.m_DoorType, component.m_IsDungeonEntrance, component.m_IsDungeonExit);
			doorGO.name = cObjectDoor.InstanceName;
			if (component.m_IsDungeonEntrance)
			{
				cMap.DungeonEntranceDoor = cObjectDoor.InstanceName;
			}
			else if (component.m_IsDungeonExit)
			{
				cMap.DungeonExitDoor = cObjectDoor.InstanceName;
			}
			else if (!levelEditorLevel || cMap.EntranceDoor == null)
			{
				cMap.EntranceDoor = cObjectDoor.InstanceName;
			}
			else
			{
				cMap.ExitDoors.Add(cObjectDoor.InstanceName);
			}
			state.Props.Add(cObjectDoor);
		}
		else
		{
			cMap.Props.Single((CObjectProp s) => s.InstanceName == doorGO.name);
		}
		if (0 == 0 && !component.m_IsDungeonEntrance)
		{
			Renderer[] componentsInChildren = doorGO.GetComponentsInChildren<Renderer>();
			for (int num = 0; num < componentsInChildren.Length; num++)
			{
				componentsInChildren[num].enabled = false;
			}
		}
	}

	public static CObjectDoor MakeDoorLevelEditor(GameObject doorGO, ScenarioRuleLibrary.ScenarioState state)
	{
		UnityGameEditorDoorProp component = doorGO.GetComponent<UnityGameEditorDoorProp>();
		ApparanceMap mapInfo = doorGO.GetComponentInParent<ApparanceMap>();
		CMap cMap = state.Maps.Single((CMap s) => s.MapGuid == mapInfo.MapGuid);
		GameObject mapSceneRoot = Choreographer.s_Choreographer.m_MapSceneRoot;
		UnityGameEditorObject component2 = doorGO.GetComponent<UnityGameEditorObject>();
		Vector3Int tileIntegerSnapSpace = MF.GetTileIntegerSnapSpace(doorGO.transform.position);
		FindUnityGameObjects(mapSceneRoot.FindInChildren(cMap.MapInstanceName), ScenarioManager.ObjectImportType.EdgeTile).Find((GameObject x) => x.transform.position == doorGO.transform.position);
		CObjectDoor cObjectDoor = new CObjectDoor(component2.m_PrefabName, component2.m_ObjectType, new TileIndex(tileIntegerSnapSpace.x, tileIntegerSnapSpace.z), new CVector3(doorGO.transform.position.x, doorGO.transform.position.y, doorGO.transform.position.z), new CVector3(doorGO.transform.localEulerAngles.x, doorGO.transform.localEulerAngles.y, doorGO.transform.localEulerAngles.z), cMap.MapGuid, component.m_DoorType, component.m_IsDungeonEntrance, component.m_IsDungeonExit);
		doorGO.name = cObjectDoor.InstanceName;
		state.Props.Add(cObjectDoor);
		return cObjectDoor;
	}
}
