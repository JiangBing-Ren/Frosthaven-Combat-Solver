using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AStar;
using Libs.GameConsole;
using Newtonsoft.Json;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class ScenarioScript
{
	[Serializable]
	public class SavedState
	{
		public int OncePerRound;

		public string Internal;
	}

	[ConsoleVariable(Alias = "scenario_script_skip_trace_ability")]
	private static bool SkipTraceAbility_cvar = false;

	[ConsoleVariable(Alias = "scenario_script_show_linenum")]
	private static bool ShowLineNum_cvar = false;

	[ConsoleVariable(Alias = "scenario_script_skip_trace_phase")]
	private static bool SkipTracePhase_cvar = false;

	[StateDump(Skip = true)]
	protected CScenarioModifier _scenarioModifier;

	[StateDump(Skip = true)]
	protected EScenarioModifierTriggerPhase _phase;

	[StateDump(Skip = true)]
	protected string _tickActorGUID;

	[StateDump(Skip = true)]
	protected CActor _tickAttacker;

	[StateDump(Skip = true)]
	protected CActor _tickDefender;

	[StateDump(Skip = true)]
	protected CTile _tickActorTile;

	[StateDump(Skip = true)]
	protected CTile _tickActorPrevTile;

	[StateDump(Skip = true)]
	protected CAbility _tickAbility;

	[StateDump(Skip = true)]
	protected CMap _tickRoomRevealed;

	protected bool _initialized;

	protected string _actorClassID;

	protected int _scriptCardID = -1;

	public List<string> NoTeleportMapGuids = new List<string>();

	protected const string CopyActorClassID = "MirroredCharacterID";

	public static Dictionary<string, Func<ScenarioScript>> scriptsMap = new Dictionary<string, Func<ScenarioScript>>
	{
		{
			"ScenarioScript_FH001",
			() => new ScenarioScript_FH001()
		},
		{
			"ScenarioScript_FH002",
			() => new ScenarioScript_FH002()
		},
		{
			"ScenarioScript_FH003",
			() => new ScenarioScript_FH003()
		},
		{
			"ScenarioScript_FH004",
			() => new ScenarioScript_FH004()
		},
		{
			"ScenarioScript_FH005",
			() => new ScenarioScript_FH005()
		},
		{
			"ScenarioScript_FH006",
			() => new ScenarioScript_FH006()
		},
		{
			"ScenarioScript_FH008",
			() => new ScenarioScript_FH008()
		},
		{
			"ScenarioScript_FH009",
			() => new ScenarioScript_FH009()
		},
		{
			"ScenarioScript_FH010",
			() => new ScenarioScript_FH010()
		},
		{
			"ScenarioScript_FH011",
			() => new ScenarioScript_FH011()
		},
		{
			"ScenarioScript_FH013",
			() => new ScenarioScript_FH013()
		},
		{
			"ScenarioScript_FH014",
			() => new ScenarioScript_FH014()
		},
		{
			"ScenarioScript_FH015",
			() => new ScenarioScript_FH015()
		},
		{
			"ScenarioScript_FH016",
			() => new ScenarioScript_FH016()
		},
		{
			"ScenarioScript_FH017",
			() => new ScenarioScript_FH017()
		},
		{
			"ScenarioScript_FH018",
			() => new ScenarioScript_FH018()
		},
		{
			"ScenarioScript_FH019",
			() => new ScenarioScript_FH019()
		},
		{
			"ScenarioScript_FH020",
			() => new ScenarioScript_FH020()
		},
		{
			"ScenarioScript_FH021",
			() => new ScenarioScript_FH021()
		},
		{
			"ScenarioScript_FH022",
			() => new ScenarioScript_FH022()
		},
		{
			"ScenarioScript_FH023",
			() => new ScenarioScript_FH023()
		},
		{
			"ScenarioScript_FH024",
			() => new ScenarioScript_FH024()
		},
		{
			"ScenarioScript_FH025",
			() => new ScenarioScript_FH025()
		},
		{
			"ScenarioScript_FH026",
			() => new ScenarioScript_FH026()
		},
		{
			"ScenarioScript_FH027",
			() => new ScenarioScript_FH027()
		},
		{
			"ScenarioScript_FH028",
			() => new ScenarioScript_FH028()
		},
		{
			"ScenarioScript_FH029",
			() => new ScenarioScript_FH029()
		},
		{
			"ScenarioScript_FH030",
			() => new ScenarioScript_FH030()
		},
		{
			"ScenarioScript_FH031",
			() => new ScenarioScript_FH031()
		},
		{
			"ScenarioScript_FH032",
			() => new ScenarioScript_FH032()
		},
		{
			"ScenarioScript_FH033",
			() => new ScenarioScript_FH033()
		},
		{
			"ScenarioScript_FH034",
			() => new ScenarioScript_FH034()
		},
		{
			"ScenarioScript_FH035",
			() => new ScenarioScript_FH035()
		},
		{
			"ScenarioScript_FH036",
			() => new ScenarioScript_FH036()
		},
		{
			"ScenarioScript_FH037",
			() => new ScenarioScript_FH037()
		},
		{
			"ScenarioScript_FH038",
			() => new ScenarioScript_FH038()
		},
		{
			"ScenarioScript_FH039",
			() => new ScenarioScript_FH039()
		},
		{
			"ScenarioScript_FH040",
			() => new ScenarioScript_FH040()
		},
		{
			"ScenarioScript_FH041",
			() => new ScenarioScript_FH041()
		},
		{
			"ScenarioScript_FH042",
			() => new ScenarioScript_FH042()
		},
		{
			"ScenarioScript_FH043",
			() => new ScenarioScript_FH043()
		},
		{
			"ScenarioScript_FH044",
			() => new ScenarioScript_FH044()
		},
		{
			"ScenarioScript_FH045",
			() => new ScenarioScript_FH045()
		},
		{
			"ScenarioScript_FH046",
			() => new ScenarioScript_FH046()
		},
		{
			"ScenarioScript_FH047",
			() => new ScenarioScript_FH047()
		},
		{
			"ScenarioScript_FH048",
			() => new ScenarioScript_FH048()
		},
		{
			"ScenarioScript_FH049",
			() => new ScenarioScript_FH049()
		},
		{
			"ScenarioScript_FH050",
			() => new ScenarioScript_FH050()
		},
		{
			"ScenarioScript_FH051",
			() => new ScenarioScript_FH051()
		},
		{
			"ScenarioScript_FH052",
			() => new ScenarioScript_FH052()
		},
		{
			"ScenarioScript_FH053",
			() => new ScenarioScript_FH053()
		},
		{
			"ScenarioScript_FH054",
			() => new ScenarioScript_FH054()
		},
		{
			"ScenarioScript_FH055",
			() => new ScenarioScript_FH055()
		},
		{
			"ScenarioScript_FH056",
			() => new ScenarioScript_FH056()
		},
		{
			"ScenarioScript_FH057",
			() => new ScenarioScript_FH057()
		},
		{
			"ScenarioScript_FH058",
			() => new ScenarioScript_FH058()
		},
		{
			"ScenarioScript_FH059",
			() => new ScenarioScript_FH059()
		},
		{
			"ScenarioScript_FH060",
			() => new ScenarioScript_FH060()
		},
		{
			"ScenarioScript_FH061",
			() => new ScenarioScript_FH061()
		},
		{
			"ScenarioScript_FH062",
			() => new ScenarioScript_FH062()
		},
		{
			"ScenarioScript_FH063",
			() => new ScenarioScript_FH063()
		},
		{
			"ScenarioScript_FH064",
			() => new ScenarioScript_FH064()
		},
		{
			"ScenarioScript_FH065",
			() => new ScenarioScript_FH065()
		},
		{
			"ScenarioScript_FH066",
			() => new ScenarioScript_FH066()
		},
		{
			"ScenarioScript_FH067",
			() => new ScenarioScript_FH067()
		},
		{
			"ScenarioScript_FH068",
			() => new ScenarioScript_FH068()
		},
		{
			"ScenarioScript_FH069",
			() => new ScenarioScript_FH069()
		},
		{
			"ScenarioScript_FH070",
			() => new ScenarioScript_FH070()
		},
		{
			"ScenarioScript_FH071",
			() => new ScenarioScript_FH071()
		},
		{
			"ScenarioScript_FH072",
			() => new ScenarioScript_FH072()
		},
		{
			"ScenarioScript_FH073",
			() => new ScenarioScript_FH073()
		},
		{
			"ScenarioScript_FH074",
			() => new ScenarioScript_FH074()
		},
		{
			"ScenarioScript_FH075",
			() => new ScenarioScript_FH075()
		},
		{
			"ScenarioScript_FH076",
			() => new ScenarioScript_FH076()
		},
		{
			"ScenarioScript_FH077",
			() => new ScenarioScript_FH077()
		},
		{
			"ScenarioScript_FH078",
			() => new ScenarioScript_FH078()
		},
		{
			"ScenarioScript_FH079",
			() => new ScenarioScript_FH079()
		},
		{
			"ScenarioScript_FH080",
			() => new ScenarioScript_FH080()
		},
		{
			"ScenarioScript_FH082",
			() => new ScenarioScript_FH082()
		},
		{
			"ScenarioScript_FH083",
			() => new ScenarioScript_FH083()
		},
		{
			"ScenarioScript_FH084",
			() => new ScenarioScript_FH084()
		},
		{
			"ScenarioScript_FH085",
			() => new ScenarioScript_FH085()
		},
		{
			"ScenarioScript_FH086",
			() => new ScenarioScript_FH086()
		},
		{
			"ScenarioScript_FH087",
			() => new ScenarioScript_FH087()
		},
		{
			"ScenarioScript_FH088",
			() => new ScenarioScript_FH088()
		},
		{
			"ScenarioScript_FH089",
			() => new ScenarioScript_FH089()
		},
		{
			"ScenarioScript_FH090",
			() => new ScenarioScript_FH090()
		},
		{
			"ScenarioScript_FH091",
			() => new ScenarioScript_FH091()
		},
		{
			"ScenarioScript_FH092",
			() => new ScenarioScript_FH092()
		},
		{
			"ScenarioScript_FH093",
			() => new ScenarioScript_FH093()
		},
		{
			"ScenarioScript_FH094",
			() => new ScenarioScript_FH094()
		},
		{
			"ScenarioScript_FH095",
			() => new ScenarioScript_FH095()
		},
		{
			"ScenarioScript_FH096",
			() => new ScenarioScript_FH096()
		},
		{
			"ScenarioScript_FH097",
			() => new ScenarioScript_FH097()
		},
		{
			"ScenarioScript_FH098",
			() => new ScenarioScript_FH098()
		},
		{
			"ScenarioScript_FH099",
			() => new ScenarioScript_FH099()
		},
		{
			"ScenarioScript_FH100",
			() => new ScenarioScript_FH100()
		},
		{
			"ScenarioScript_FH101",
			() => new ScenarioScript_FH101()
		},
		{
			"ScenarioScript_FH102",
			() => new ScenarioScript_FH102()
		},
		{
			"ScenarioScript_FH103",
			() => new ScenarioScript_FH103()
		},
		{
			"ScenarioScript_FH104",
			() => new ScenarioScript_FH104()
		},
		{
			"ScenarioScript_FH105",
			() => new ScenarioScript_FH105()
		},
		{
			"ScenarioScript_FH106",
			() => new ScenarioScript_FH106()
		},
		{
			"ScenarioScript_FH107",
			() => new ScenarioScript_FH107()
		},
		{
			"ScenarioScript_FH108",
			() => new ScenarioScript_FH108()
		},
		{
			"ScenarioScript_FH109",
			() => new ScenarioScript_FH109()
		},
		{
			"ScenarioScript_FH110",
			() => new ScenarioScript_FH110()
		},
		{
			"ScenarioScript_FH111",
			() => new ScenarioScript_FH111()
		},
		{
			"ScenarioScript_FH113",
			() => new ScenarioScript_FH113()
		},
		{
			"ScenarioScript_FH114",
			() => new ScenarioScript_FH114()
		},
		{
			"ScenarioScript_FH115",
			() => new ScenarioScript_FH115()
		},
		{
			"ScenarioScript_FH116",
			() => new ScenarioScript_FH116()
		},
		{
			"ScenarioScript_FH117",
			() => new ScenarioScript_FH117()
		},
		{
			"ScenarioScript_FH118",
			() => new ScenarioScript_FH118()
		},
		{
			"ScenarioScript_FH119",
			() => new ScenarioScript_FH119()
		},
		{
			"ScenarioScript_FH120",
			() => new ScenarioScript_FH120()
		},
		{
			"ScenarioScript_FH121",
			() => new ScenarioScript_FH121()
		},
		{
			"ScenarioScript_FH122",
			() => new ScenarioScript_FH122()
		},
		{
			"ScenarioScript_FH123",
			() => new ScenarioScript_FH123()
		},
		{
			"ScenarioScript_FH124",
			() => new ScenarioScript_FH124()
		},
		{
			"ScenarioScript_FH125",
			() => new ScenarioScript_FH125()
		},
		{
			"ScenarioScript_FH126",
			() => new ScenarioScript_FH126()
		},
		{
			"ScenarioScript_FH127",
			() => new ScenarioScript_FH127()
		},
		{
			"ScenarioScript_FH128",
			() => new ScenarioScript_FH128()
		},
		{
			"ScenarioScript_FH129",
			() => new ScenarioScript_FH129()
		},
		{
			"ScenarioScript_FH130",
			() => new ScenarioScript_FH130()
		},
		{
			"ScenarioScript_FH131",
			() => new ScenarioScript_FH131()
		},
		{
			"ScenarioScript_FH132",
			() => new ScenarioScript_FH132()
		},
		{
			"ScenarioScript_FH133",
			() => new ScenarioScript_FH133()
		},
		{
			"ScenarioScript_FH134",
			() => new ScenarioScript_FH134()
		},
		{
			"ScenarioScript_FH135",
			() => new ScenarioScript_FH135()
		},
		{
			"ScenarioScript_FH136",
			() => new ScenarioScript_FH136()
		},
		{
			"ScenarioScript_RFH001",
			() => new ScenarioScript_RFH001()
		},
		{
			"ScenarioScript_RFH002",
			() => new ScenarioScript_RFH002()
		},
		{
			"ScenarioScript_RFH003",
			() => new ScenarioScript_RFH003()
		},
		{
			"ScenarioScript_RFS007",
			() => new ScenarioScript_RFS007()
		},
		{
			"ScenarioScript_RFH004",
			() => new ScenarioScript_RFH004()
		},
		{
			"ScenarioScript_RFH005",
			() => new ScenarioScript_RFH005()
		},
		{
			"ScenarioScript_RFH006",
			() => new ScenarioScript_RFH006()
		},
		{
			"CardScript_Geminate0181",
			() => new CardScript_Geminate0181()
		},
		{
			"CardScript_Geminate0182",
			() => new CardScript_Geminate0182()
		}
	};

	private int _oncePerRound = -1;

	protected CActor _tickActor
	{
		get
		{
			if (!string.IsNullOrEmpty(_tickActorGUID))
			{
				return ScenarioManager.Scenario.AllActors.Find((CActor a) => a.ActorGuid == _tickActorGUID);
			}
			return null;
		}
		set
		{
			_tickActorGUID = value?.ActorGuid;
		}
	}

	protected static ScenarioState _css => ScenarioManager.CurrentScenarioState;

	public bool IsLoading => _css.ScenarioScriptsJSON.Count > 0;

	protected bool _tickAbilityBelongsToCard => _tickAbility?.GetAbilityBaseCard()?.ID == _scriptCardID;

	public bool IsCardScript => _scriptCardID != -1;

	public static ScenarioScript current => GetCurrent();

	public static int PartySize => _css.Players.Count;

	public static int RoundNumber => _css.RoundNumber;

	public virtual bool DisableCameraRotation => false;

	public virtual string StartTurnHelpTextOverride => null;

	public virtual CBaseCard.ECardPile OverrideRecoverCardPile(CActor actor)
	{
		return CBaseCard.ECardPile.None;
	}

	public virtual CBaseCard.ECardPile OverrideMoveAbilityCardToPile(CActor actor, CBaseCard.ECardPile selectedPile, CAbilityCard abilityCard, string pileToMoveFromName)
	{
		return selectedPile;
	}

	private static ScenarioScript GetCurrent()
	{
		if (_css?.ScenarioScripts == null)
		{
			return null;
		}
		foreach (ScenarioScript scenarioScript in ScenarioManager.CurrentScenarioState.ScenarioScripts)
		{
			if (!scenarioScript.IsCardScript)
			{
				return scenarioScript;
			}
		}
		return null;
	}

	public void Error(object o, object ctx = null, [CallerLineNumber] int lineNumber = 0)
	{
		if (ShowLineNum_cvar)
		{
			_ = $" {lineNumber}";
		}
		Log.GameplayError($"{this}: {o.ToString()}", ctx);
	}

	public void Trace(object o, object ctx = null, [CallerLineNumber] int lineNumber = 0)
	{
		string text = ((!ShowLineNum_cvar) ? "" : $" {lineNumber}");
		Log.GameplayInfo(GetType().Name + ":" + text + ": " + o.ToString(), ctx);
	}

	public static bool GetName(AbilityData.ActiveBonusData abData, out string scriptName)
	{
		scriptName = null;
		if (abData == null)
		{
			return false;
		}
		if (!abData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.ScenarioScript) && !abData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.MoveScript))
		{
			Log.GameplayTrace("Not a ScenarioScript active bonus ability");
			return false;
		}
		if (abData.ArbitraryParams == null || abData.ArbitraryParams.Count == 0)
		{
			Log.GameplayError("No scenario script name passed in CActiveBonus.ActiveBonusData.ArbitraryParams");
			return false;
		}
		scriptName = abData.ArbitraryParams[0];
		return !string.IsNullOrEmpty(scriptName);
	}

	public static ScenarioScript Create(AbilityData.ActiveBonusData abData)
	{
		if (!GetName(abData, out var scriptName))
		{
			return null;
		}
		return Create(scriptName);
	}

	public static ScenarioScript Create(string scriptName)
	{
		if (!TryCreate(scriptName, out var result))
		{
			Log.GameplayError("Couldn't create scenario script " + scriptName);
		}
		return result;
	}

	public static bool TryCreate(string scriptName, out ScenarioScript result)
	{
		result = null;
		if (scriptsMap.TryGetValue(scriptName, out var value))
		{
			result = value();
			if (result != null)
			{
				Log.GameplayInfo($"Created ScenarioScript {result}");
				return true;
			}
		}
		return result != null;
	}

	public bool TryInitialize(CScenarioModifier sm, int cardID = -1)
	{
		if (!_initialized)
		{
			Trace("Initializing...", null, 298);
			_scenarioModifier = sm;
			_initialized = Initialize();
			TryOverrideCharacterModels();
			_scriptCardID = cardID;
			Trace($"initialized: {_initialized}", null, 303);
		}
		return _initialized;
	}

	public void TryOverrideCharacterModels()
	{
		if (ScenarioManager.Scenario?.AllAliveActors == null)
		{
			return;
		}
		foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
		{
			TryOverrideCharacterModel(allAliveActor, sendChangeMessage: true);
		}
	}

	public void TryOverrideCharacterModel(CActor actor, bool sendChangeMessage = false)
	{
		if (actor == null || !string.IsNullOrEmpty(actor.OverrideCharacterModel))
		{
			return;
		}
		string overrideCharacterModelPrefabName = GetOverrideCharacterModelPrefabName(actor);
		if (!string.IsNullOrEmpty(overrideCharacterModelPrefabName))
		{
			actor.OverrideCharacterModel = overrideCharacterModelPrefabName;
			if (sendChangeMessage)
			{
				CForcedChangeCharacterModel_MessageData message = new CForcedChangeCharacterModel_MessageData(actor);
				ScenarioRuleClient.MessageHandler(message);
			}
		}
	}

	public virtual string GetOverrideCharacterModelPrefabName(CActor actor)
	{
		return null;
	}

	public void TryDone()
	{
		if (_initialized)
		{
			Done();
			_initialized = false;
			Trace("Done scenario script.", null, 340);
		}
	}

	public void TickPhase(CActor tickActor, CScenarioModifier sm)
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = sm.ScenarioModifierTriggerPhase;
		if (!_initialized)
		{
			Error($"Trying to Tick in phase {scenarioModifierTriggerPhase}, but not initialized yet.", null, 347);
			return;
		}
		_scenarioModifier = sm;
		_phase = _scenarioModifier.ScenarioModifierTriggerPhase;
		_tickAbility = null;
		_tickActor = tickActor;
		_tickActorTile = ScenarioManager.Tiles[tickActor.ArrayIndex.X, tickActor.ArrayIndex.Y];
		_tickActorPrevTile = _tickActorTile;
		if (!SkipTracePhase_cvar)
		{
			Trace($"TickPhase {_tickActor.Class.ID}; phase: {scenarioModifierTriggerPhase}", null, 358);
		}
		TickPhaseInternal();
	}

	public void TickPhase(CActor tickActor, EScenarioModifierTriggerPhase phase)
	{
		if (!_initialized)
		{
			Error($"Trying to Tick in phase {phase}, but not initialized yet.", null, 365);
			return;
		}
		_scenarioModifier = null;
		_phase = phase;
		_tickAbility = null;
		_tickActor = tickActor;
		_tickActorTile = ScenarioManager.Tiles[tickActor.ArrayIndex.X, tickActor.ArrayIndex.Y];
		_tickActorPrevTile = _tickActorTile;
		if (!SkipTracePhase_cvar)
		{
			Trace($"TickPhase {_tickActor.Class.ID}; phase: {phase}", null, 376);
		}
		TickPhaseInternal();
	}

	private bool TickActionBegin(CAbility tickAbility, CActor tickActor, CScenarioModifier sm)
	{
		if (!_initialized)
		{
			return false;
		}
		if (sm != null)
		{
			if (!sm.ApplyToEachActorOnce)
			{
				Error("Move/Damage/etc. ScenarioModifiers should be applied 'Once Per Actor', set the checkbox in the Level Editor.", null, 388);
				return false;
			}
			if (sm.ScenarioModifierTriggerPhase != EScenarioModifierTriggerPhase.OnActorSpawnedOrRevealed)
			{
				Error("Move/Damage/etc. ScenarioModifiers should be triggered OnActorSpawnedOrRevealed.", null, 392);
				return false;
			}
		}
		_scenarioModifier = sm;
		_tickAbility = tickAbility;
		_tickActor = tickActor;
		_tickActorTile = ScenarioManager.Tiles[tickActor.ArrayIndex.X, tickActor.ArrayIndex.Y];
		return true;
	}

	public void TickMoving(CAbility tickAbility, CActor tickActor, CScenarioModifier sm)
	{
		if (TickActionBegin(tickAbility, tickActor, sm))
		{
			Trace("TickReposition " + _tickActor.Class.ID, null, 410);
			TickMovingInternal();
			_tickActorPrevTile = _tickActorTile;
		}
	}

	public void TickReposition(CAbility tickAbility, CActor tickActor, CScenarioModifier sm)
	{
		if (TickActionBegin(tickAbility, tickActor, sm))
		{
			Trace("TickReposition " + _tickActor.Class.ID, null, 420);
			TickRepositionInternal();
			_tickActorPrevTile = _tickActorTile;
		}
	}

	public void TickRepositionInCharacterPlacement(CAbility tickAbility, CActor tickActor, CScenarioModifier sm)
	{
		if (TickActionBegin(tickAbility, tickActor, sm))
		{
			Trace("TickRepositionInCharacterPlacement " + _tickActor.Class.ID, null, 428);
			TickRepositionInCharacterPlacementInternal();
			_tickActorPrevTile = _tickActorTile;
		}
	}

	public virtual void OnActorMoving(CActor movingActor, Point currentPosition, Point nextPosition)
	{
	}

	public void TickAbilityPerformStarted(CAbility tickAbility, CScenarioModifier sm)
	{
		CActor targetingActor = tickAbility.TargetingActor;
		if (TickActionBegin(tickAbility, targetingActor, sm))
		{
			if (!SkipTraceAbility_cvar)
			{
				Trace($"TickAbilityPerformStarted {_tickActor.Class.ID} {tickAbility.AbilityType} {tickAbility.Name}", null, 446);
			}
			TickAbilityPerformStartedInternal();
			_tickActorPrevTile = _tickActorTile;
		}
	}

	public void TickAbilityStarted(CAbility tickAbility, CScenarioModifier sm)
	{
		CActor targetingActor = tickAbility.TargetingActor;
		if (TickActionBegin(tickAbility, targetingActor, sm))
		{
			if (!SkipTraceAbility_cvar)
			{
				Trace($"TickAbilityStarted {_tickActor.Class.ID} {tickAbility.AbilityType} {tickAbility.Name}", null, 459);
			}
			TickAbilityStartedInternal();
			_tickActorPrevTile = _tickActorTile;
		}
	}

	public void TickAbilityEnded(CAbility tickAbility, CScenarioModifier sm)
	{
		CActor targetingActor = tickAbility.TargetingActor;
		if (TickActionBegin(tickAbility, targetingActor, sm))
		{
			if (!SkipTraceAbility_cvar)
			{
				Trace($"TickAbilityEnded {targetingActor.Class.ID} {tickAbility.AbilityType} {tickAbility.Name}", null, 471);
			}
			TickAbilityEndedInternal();
			_tickActorPrevTile = _tickActorTile;
		}
	}

	public void TickAbilityUndone(CAbility tickAbility, CScenarioModifier sm)
	{
		CActor targetingActor = tickAbility.TargetingActor;
		if (TickActionBegin(tickAbility, targetingActor, sm))
		{
			if (!SkipTraceAbility_cvar)
			{
				Trace($"TickAbilityUndone {targetingActor.Class.ID} {tickAbility.AbilityType} {tickAbility.Name}", null, 483);
			}
			TickAbilityUndoneInternal();
			_tickActorPrevTile = _tickActorTile;
		}
	}

	public void TickAbilityOverridesChanged(CAbility tickAbility)
	{
		_tickAbility = tickAbility;
		TickAbilityOverridesChangedInternal();
	}

	public void TickAttacking(CAbility tickAbility, CActor target, CScenarioModifier sm)
	{
		CActor targetingActor = tickAbility.TargetingActor;
		if (TickActionBegin(tickAbility, targetingActor, sm))
		{
			_tickAttacker = targetingActor;
			_tickDefender = target;
			Trace("TickAttacking", null, 501);
			Trace("  attacker: " + _tickAttacker?.Class.ID, null, 502);
			Trace("  defender: " + _tickDefender?.Class.ID, null, 503);
			Trace($"  ability: {_tickAbility}", null, 504);
			TickAttackingInternal();
			_tickActorPrevTile = _tickActorTile;
		}
	}

	public void SetRevealedRoom(CMap map)
	{
		_tickRoomRevealed = map;
		Trace("TickRoomRevealed", null, 511);
		Trace("  room: " + _tickRoomRevealed?.RoomName, null, 512);
		OnStartRevealingRoom(map.Revealed);
	}

	protected virtual void OnStartRevealingRoom(bool wasRevealed)
	{
	}

	protected virtual void OnFinishedRevealingRoom()
	{
	}

	protected void AddMonsterOnRoomReveal(string classID, int x, int y, Dictionary<int, ScenarioManager.EPerPartySizeConfig> setup)
	{
		CTile tile = ScenarioManager.GetTile(x, y);
		TileIndex location = new TileIndex(x, y);
		CMap hexMap = tile.m_HexMap;
		int num = MonsterClassManager.Find(classID).Health();
		EnemyState item = new EnemyState(classID, 0, null, hexMap.MapGuid, location, num, num, ScenarioManager.CurrentScenarioState.Level, null, null, playedThisRound: false, CActor.ECauseOfDeath.StillAlive, isSummon: false, null, 0, CActor.EType.Enemy);
		ScenarioManager.CurrentScenarioState.Monsters.Add(item);
		ModifyMonsterSetup(x, y, setup);
	}

	protected void ModifyMonsterSetup(int x, int y, Dictionary<int, ScenarioManager.EPerPartySizeConfig> setup)
	{
		EnemyState enemyState = ScenarioManager.CurrentScenarioState.Monsters.Concat(ScenarioManager.CurrentScenarioState.Enemy2Monsters).FirstOrDefault((EnemyState state) => state.Location.X == x && state.Location.Y == y);
		if (enemyState != null)
		{
			enemyState.ConfigPerPartySize = setup;
		}
	}

	public void ResetRevealedRoom(bool wasRevealed)
	{
		if (!wasRevealed)
		{
			OnFinishedRevealingRoom();
		}
		_tickRoomRevealed = null;
	}

	public void TickBeingAttacked(CAbilityAttack ability, CActor target, int modifiedStrength, CScenarioModifier sm)
	{
		if (TickActionBegin(ability, target, sm))
		{
			_tickAttacker = _tickAbility?.TargetingActor;
			Trace("TickBeingAttacked", null, 554);
			Trace("  attacker: " + _tickAttacker?.Class.ID, null, 555);
			Trace("  defender: " + target.Class.ID, null, 556);
			Trace($"  ability: {_tickAbility}", null, 557);
			Trace($"  modified strength: {modifiedStrength}", null, 558);
			TickBeingAttackedInternal(ability, modifiedStrength);
			_tickActorPrevTile = _tickActorTile;
		}
	}

	public void TickBeingDamaged(CAbilityDamage ability, CActor target, int modifiedStrength, CScenarioModifier sm)
	{
		if (TickActionBegin(ability, target, sm))
		{
			_tickAttacker = _tickAbility?.TargetingActor;
			Trace("TickBeingDamaged", null, 567);
			Trace("  attacker: " + _tickAttacker?.Class.ID, null, 568);
			Trace("  defender: " + target.Class.ID, null, 569);
			Trace($"  ability: {_tickAbility}", null, 570);
			Trace($"  modified strength: {modifiedStrength}", null, 571);
			TickBeingDamagedInternal(ability, modifiedStrength);
			_tickActorPrevTile = _tickActorTile;
		}
	}

	public void TickAttackSummaryCreated(CAttackSummary.TargetSummary summary, CScenarioModifier sm)
	{
		if (TickActionBegin(summary.OriginalAbility, summary.ActorToAttack, sm))
		{
			_tickAttacker = _tickAbility?.TargetingActor;
			Trace("TickAttackSummaryCreated", null, 580);
			Trace("  attacker: " + _tickAttacker?.Class.ID, null, 581);
			Trace("  defender: " + summary.ActorToAttack.Class.ID, null, 582);
			Trace($"  ability: {_tickAbility}", null, 583);
			TickAttackSummaryCreatedInternal(summary);
			_tickActorPrevTile = _tickActorTile;
		}
	}

	public void TickTakenDamage(int damageTaken, int damageReducedByShields, int actualDamage, CAbility tickAbility, CActor tickActor, CScenarioModifier sm, CActor damageSourceActor)
	{
		if (TickActionBegin(tickAbility, tickActor, sm))
		{
			_tickAttacker = _tickAbility?.TargetingActor;
			if (_tickAttacker == null)
			{
				_tickAttacker = damageSourceActor;
			}
			_tickAbility = tickAbility;
			Trace("TickTakenDamage", null, 598);
			Trace("  attacker: " + _tickAttacker?.Class.ID, null, 599);
			Trace("  defender: " + _tickActor?.Class.ID, null, 600);
			Trace($"  ability: {_tickAbility}", null, 601);
			Trace($"  damage taken: {damageTaken}", null, 602);
			Trace($"  damage reduced by shields: {damageReducedByShields}", null, 603);
			Trace($"  actual damage {actualDamage}", null, 604);
			TickTakenDamageInternal(damageTaken, damageReducedByShields, actualDamage);
			_tickActorPrevTile = _tickActorTile;
		}
	}

	public void TickAvoidedDamage(CActor tickActor, GameState.EAvoidDamageOption avoidOption, List<CAbilityCard> cardsBurnedToAvoidDamage, List<CActiveBonus> usedPreventDamageBonusesToAvoidDamage)
	{
		if (TickActionBegin(null, tickActor, null))
		{
			TickAvoidedDamageInternal(avoidOption, cardsBurnedToAvoidDamage, usedPreventDamageBonusesToAvoidDamage);
		}
	}

	public void TickDeath(CAbility tickAbility, CActor tickActor, CScenarioModifier sm)
	{
		if (TickActionBegin(tickAbility, tickActor, sm))
		{
			Trace("TickDeath " + _tickActor.Class.ID, null, 621);
			TickDeathInternal();
			_tickActorPrevTile = _tickActorTile;
		}
	}

	public void OnShortRestComplete(CPlayerActor actor)
	{
		if (TickActionBegin(null, actor, null) && _initialized)
		{
			OnShortRestCompleteInternal();
			Trace("On Short Rest Complete " + actor.Class.ID, null, 632);
		}
	}

	public void OnLongRestComplete(CPlayerActor actor)
	{
		if (TickActionBegin(null, actor, null) && _initialized)
		{
			OnLongRestCompleteInternal();
			Trace("On Long Rest Complete " + actor.Class.ID, null, 644);
		}
	}

	public void OnDoorOpened(CObjectDoor door, CActor actor, CActor creditActor, bool visualOnly)
	{
		_tickRoomRevealed = door.RoomsRevealedInLastOpening?.FirstOrDefault();
		Trace($"Door {door.PropGuid.Substring(0, 5)} opened by '{actor?.Class.ID}'; credited '{creditActor?.Class.ID}'; visualOnly: {visualOnly}", null, 651);
		OnDoorOpenedInternal(door, actor, creditActor, visualOnly);
		_tickRoomRevealed = null;
	}

	public string Serialize(ScenarioState ss)
	{
		if (!_initialized)
		{
			return string.Empty;
		}
		Trace("Serialize...", null, 666);
		return JsonConvert.SerializeObject(new SavedState
		{
			OncePerRound = _oncePerRound,
			Internal = SerializeInternal()
		});
	}

	public void Deserialize(ScenarioState ss, string storage)
	{
		if (!_initialized)
		{
			return;
		}
		Trace("Deserialize...", null, 677);
		if (string.IsNullOrEmpty(storage))
		{
			DeserializeInternal(storage);
			return;
		}
		SavedState savedState = null;
		try
		{
			savedState = JsonConvert.DeserializeObject<SavedState>(storage);
		}
		catch
		{
		}
		if (savedState == null)
		{
			DeserializeInternal(storage);
			return;
		}
		_oncePerRound = savedState.OncePerRound;
		DeserializeInternal(savedState.Internal);
	}

	public int GetConclusionIndex()
	{
		return GetConclusionIndexInternal();
	}

	public List<object> GetConclusionParameters()
	{
		return GetConclusionParametersInternal();
	}

	public bool AllowTriggerMessage(SEvent ev, CLevelMessage message)
	{
		return AllowTriggerMessageInternal(ev, message);
	}

	public virtual void OnMessagetrigger(CLevelMessage message)
	{
	}

	public bool AllowEscapeZone(CActor actor)
	{
		return AllowEscapeZoneInternal(actor);
	}

	public bool GetPropData(CObjectProp prop, out string data)
	{
		return GetPropDataInternal(prop, out data);
	}

	protected virtual bool Initialize()
	{
		return true;
	}

	protected virtual void Done()
	{
	}

	protected virtual void TickPhaseInternal()
	{
	}

	protected virtual void TickMovingInternal()
	{
	}

	protected virtual void TickRepositionInternal()
	{
	}

	protected virtual void TickRepositionInCharacterPlacementInternal()
	{
	}

	protected virtual void TickAbilityPerformStartedInternal()
	{
	}

	protected virtual void TickAbilityStartedInternal()
	{
	}

	protected virtual void TickAbilityEndedInternal()
	{
	}

	protected virtual void TickAbilityUndoneInternal()
	{
	}

	protected virtual void TickAbilityOverridesChangedInternal()
	{
	}

	public virtual bool ShouldSortModifierTextsInOrderOfModifiers()
	{
		return false;
	}

	public virtual bool IsExhaustedOverride(CPlayerActor actor, out bool isExhausted)
	{
		isExhausted = false;
		return false;
	}

	public virtual void ValidateAbilityTargeting(CAbility ability)
	{
	}

	public virtual bool ValidateElementInCreation(ElementInfusionBoardManager.EElement element)
	{
		return true;
	}

	public virtual bool ValidateElementReserved(ElementInfusionBoardManager.EElement element)
	{
		return true;
	}

	public virtual bool UpdateElementsBoard()
	{
		return false;
	}

	public virtual bool OnConsumedElement(ElementInfusionBoardManager.EElement element, CActor consumingActor)
	{
		return UpdateElementsBoard();
	}

	public virtual bool ShouldPierceInvulnerability(CActor actor)
	{
		return false;
	}

	public virtual bool ShouldIgnoreImmunity(CActor actor, CAbility ability)
	{
		return false;
	}

	public virtual bool ShouldIgnoreImmunityWhileTargeting(CActor actor, CAbility ability)
	{
		return false;
	}

	public virtual bool CanActorIgnoreCollisions(CActor actor)
	{
		return false;
	}

	public virtual Point GetActorIteractionTile(CActor actor)
	{
		return default;
	}

	public virtual bool IsActorInteracting(CActor actor)
	{
		return false;
	}

	public virtual bool TreatTileAsFeatureless(CTile tile)
	{
		return false;
	}

	public virtual bool TreatTileAsNonFeatureless(CTile tile)
	{
		return false;
	}

	public virtual bool AllowPathfindingThroughTileForEnemy(CTile tile, CEnemyActor actor)
	{
		return false;
	}

	public virtual bool TreatTilesAsSameRoom(CTile tileA, CTile tileB)
	{
		return false;
	}

	public virtual bool IsActorConsideringElementAsStrong(CActor actor, ElementInfusionBoardManager.EElement element)
	{
		return false;
	}

	public virtual bool CanActorLoot(CObjectProp prop, CActor actor)
	{
		return true;
	}

	public virtual bool IsScenarioActor(CActor actor)
	{
		return false;
	}

	public virtual bool IsTileEleveated(CTile tile)
	{
		return false;
	}

	public virtual bool EnemiesAttackWithMinimalMovement()
	{
		return false;
	}

	public virtual bool ShouldSpawnProjectile(CActor attacker, CActor target)
	{
		return true;
	}

	public virtual bool UseCustomAreActorsAllied()
	{
		return false;
	}

	public virtual bool UseCustomAreActorsEnemies()
	{
		return false;
	}

	public virtual bool CustomAreActorsAllied(CActor.EType actor1Type, CActor.EType actor2Type, CActor actor1 = null, CActor actor2 = null)
	{
		return CActor.AreActorsAllied(actor1Type, actor2Type, actor1, actor2, skipCheckingScenario: true);
	}

	public virtual bool CustomAreActorsEnemies(CActor.EType actor1Type, CActor.EType actor2Type, CActor actor1 = null, CActor actor2 = null)
	{
		return CActor.AreActorsEnemies(actor1Type, actor2Type, actor1, actor2, skipCheckingScenario: true);
	}

	protected virtual void TickAttackingInternal()
	{
	}

	protected virtual void TickBeingAttackedInternal(CAbilityAttack ability, int modifiedStrength)
	{
	}

	protected virtual void TickBeingDamagedInternal(CAbilityDamage ability, int modifiedStrength)
	{
	}

	protected virtual void TickAttackSummaryCreatedInternal(CAttackSummary.TargetSummary summary)
	{
	}

	protected virtual void TickTakenDamageInternal(int damageTaken, int damageReducedByShields, int actualDamage)
	{
	}

	protected virtual void TickAvoidedDamageInternal(GameState.EAvoidDamageOption avoidOption, List<CAbilityCard> cardsBurnedToAvoidDamage, List<CActiveBonus> usedPreventDamageBonusesToAvoidDamage)
	{
	}

	protected virtual void TickDeathInternal()
	{
	}

	protected virtual void OnShortRestCompleteInternal()
	{
	}

	protected virtual void OnLongRestCompleteInternal()
	{
	}

	public virtual List<Point> CustomFindEnemyPathTransientSuperBlockedToAdd()
	{
		return new List<Point>();
	}

	protected virtual string SerializeInternal()
	{
		return string.Empty;
	}

	protected virtual void DeserializeInternal(string storage)
	{
	}

	protected virtual int GetConclusionIndexInternal()
	{
		return -1;
	}

	protected virtual List<object> GetConclusionParametersInternal()
	{
		return new List<object>();
	}

	protected virtual bool AllowTriggerMessageInternal(SEvent ev, CLevelMessage message)
	{
		return false;
	}

	protected virtual bool GetPropDataInternal(CObjectProp prop, out string data)
	{
		data = null;
		return false;
	}

	protected virtual bool AllowEscapeZoneInternal(CActor actor)
	{
		return true;
	}

	protected virtual void OnDoorOpenedInternal(CObjectDoor door, CActor actor, CActor creditActor, bool visualOnly)
	{
	}

	public virtual Dictionary<string, List<CEnemyActor>> GetExtraInitiativeActors()
	{
		return null;
	}

	public virtual void ApplyReward(bool won)
	{
	}

	public virtual void OnHeroSummonSpawned(CHeroSummonActor actor)
	{
	}

	public virtual void OnHeroSummonFinished(CHeroSummonActor actor)
	{
	}

	public virtual void OnSpawnerHasSpawned(CSpawner spawner, CActor actor)
	{
	}

	protected void CheckObjectivesComplete()
	{
		EObjectiveResult eObjectiveResult = ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		Trace($"Checked objectives complete: {eObjectiveResult}", null, 851);
	}

	protected void UpdateObjectiveProgress()
	{
		CUpdateObjectiveProgress_MessageData message = new CUpdateObjectiveProgress_MessageData();
		ScenarioRuleClient.MessageHandler(message);
	}

	protected bool GetObjective(string eventIdentifier, out CObjective objective)
	{
		objective = GetObjective(eventIdentifier);
		if (objective != null)
		{
			return true;
		}
		Error("Can't find objective event identifier " + eventIdentifier, null, 864);
		return false;
	}

	protected CObjective GetObjective(string eventIdentifier)
	{
		foreach (CObjective allObjective in ScenarioManager.CurrentScenarioState.AllObjectives)
		{
			if (allObjective.EventIdentifier == eventIdentifier)
			{
				return allObjective;
			}
		}
		return null;
	}

	protected TCObjective GetObjective<TCObjective>(string eventIdentifier) where TCObjective : CObjective
	{
		foreach (CObjective allObjective in ScenarioManager.CurrentScenarioState.AllObjectives)
		{
			if (allObjective.EventIdentifier == eventIdentifier && allObjective is TCObjective result)
			{
				return result;
			}
		}
		return null;
	}

	protected CScenarioModifier GetScenarioModifier(string eventIdentifier)
	{
		foreach (CScenarioModifier scenarioModifier in ScenarioManager.CurrentScenarioState.ScenarioModifiers)
		{
			if (scenarioModifier.EventIdentifier == eventIdentifier)
			{
				return scenarioModifier;
			}
		}
		return null;
	}

	protected void AddScenarioAbilityToActor(CScenarioModifier holder, CActor actor)
	{
		CAbility cAbility = CAbility.CopyAbility(ScenarioRuleClient.SRLYML.ScenarioAbilities.FirstOrDefault((ScenarioAbilitiesYMLData a) => a.ScenarioAbilityID == holder.ScenarioAbilityID)?.ScenarioAbilities[0], generateNewID: false);
		cAbility.IsScenarioModifierAbility = true;
		cAbility.Start(actor, actor);
		cAbility.ApplyToActor(actor);
	}

	protected void RemoveScenarioAbilityFromActor(CScenarioModifier holder, CActor actor)
	{
		for (int num = holder.ActiveBonuses.Count - 1; num >= 0; num--)
		{
			CActiveBonus cActiveBonus = holder.ActiveBonuses[num];
			if (cActiveBonus.Actor == actor)
			{
				cActiveBonus.Finish();
			}
		}
	}

	protected void EscapeMap()
	{
		EscapeMap(_tickActor);
	}

	protected void EscapeMap(CActor actor)
	{
		GameState.KillActor(actor, actor, CActor.ECauseOfDeath.ActorRemovedFromMap, out var _);
		Trace("Escaped map", null, 922);
	}

	protected bool TryEscapeMap()
	{
		return TryEscapeMap(_tickActor);
	}

	protected bool TryEscapeMap(CActor actor)
	{
		if (actor.AllowEscape && ScenarioManager.CurrentScenarioState.IsOnEscapeZone(actor))
		{
			if (!AllowEscapeZoneInternal(actor))
			{
				Error("Trying to evacuate while escape zone is forbidden", null, 931);
				return false;
			}
			Trace("Stepped on Escape zone hex..", null, 934);
			EscapeMap(actor);
			return true;
		}
		Trace("Not an escape zone...", null, 938);
		return false;
	}

	protected bool IsAlive(EnemyState es)
	{
		if (!es.IsDead)
		{
			if (es.Actor != null)
			{
				return !es.Actor.IsDead;
			}
			return false;
		}
		return false;
	}

	protected bool IsDead(EnemyState es)
	{
		return !IsAlive(es);
	}

	protected bool TrySpawnActorAtNearestTile(CTile ct, string cls, CActor.EType type, out CActor actor, bool addIntoInitiativeOrder = false, bool startRound = false)
	{
		return TrySpawnActorAtNearestTile(ct.m_ArrayIndex.X, ct.m_ArrayIndex.Y, cls, type, out actor, addIntoInitiativeOrder, startRound);
	}

	protected bool TrySpawnActorAtNearestTile(CTile ct, string cls, out CActor actor, bool addIntoInitiativeOrder = false, bool startRound = false)
	{
		return TrySpawnActorAtNearestTile(ct.m_ArrayIndex.X, ct.m_ArrayIndex.Y, cls, out actor, addIntoInitiativeOrder, startRound);
	}

	protected bool TrySpawnActorAtNearestTile(int x, int y, string cls, CActor.EType type, out CActor actor, bool addIntoInitiativeOrder = false, bool startRound = false)
	{
		actor = null;
		if (!IsFreeTile(x, y))
		{
			if (!GetNearestFreeTile(x, y, out var nearestTile))
			{
				Trace($"Failed to place {cls} at {x}:{y} (no free tiles)", null, 966);
				return false;
			}
			x = nearestTile.m_ArrayIndex.X;
			y = nearestTile.m_ArrayIndex.Y;
		}
		return SpawnActorAtTile(x, y, cls, type, out actor, addIntoInitiativeOrder, treatAsSummon: false, startRound);
	}

	protected bool TrySpawnActorAtNearestTile(int x, int y, string cls, out CActor actor, bool addIntoInitiativeOrder = false, bool startRound = false)
	{
		return TrySpawnActorAtNearestTile(x, y, cls, CActor.EType.Enemy, out actor, addIntoInitiativeOrder, startRound);
	}

	protected bool TrySpawnActorAtNearestTile(Point tile, string cls, out CActor actor, bool addIntoInitiativeOrder = false, bool startRound = false)
	{
		return TrySpawnActorAtNearestTile(tile.X, tile.Y, cls, CActor.EType.Enemy, out actor, addIntoInitiativeOrder, startRound);
	}

	protected bool TrySpawnActorAtTile(int x, int y, string cls, out CActor actor, bool addIntoInitiativeOrder = false, bool startRound = false)
	{
		actor = null;
		if (!IsFreeTile(x, y))
		{
			Trace($"Failed to place {cls} at {x}:{y} (tile occupied)", null, 990);
			return false;
		}
		return SpawnActorAtTile(x, y, cls, out actor, addIntoInitiativeOrder, startRound);
	}

	protected bool SpawnActorAtTile(Point tile, string cls, out CActor actor, bool addIntoInitiativeOrder = false, bool startRound = false)
	{
		return SpawnActorAtTile(tile.X, tile.Y, cls, out actor, addIntoInitiativeOrder, startRound);
	}

	protected bool SpawnActorAtTile(int x, int y, string cls, out CActor actor, bool addIntoInitiativeOrder = false, bool startRound = false)
	{
		return SpawnActorAtTile(x, y, cls, CActor.EType.Enemy, out actor, addIntoInitiativeOrder, treatAsSummon: false, startRound);
	}

	protected bool SpawnActorAtTile(int x, int y, string cls, CActor.EType type, out CActor actor, bool addIntoInitiativeOrder = false, bool treatAsSummon = false, bool startRound = false)
	{
		int count = ScenarioManager.CurrentScenarioState.Players.Count;
		bool treatAsSummon2 = treatAsSummon;
		bool addIntoInitiativeOrder2 = addIntoInitiativeOrder;
		if (CSpawner.SpawnUnitAtTile(count, type, cls, x, y, out actor, treatAsSummon2, initial: false, startRound, null, addIntoInitiativeOrder2))
		{
			Trace($"Placed ({type}) {cls} at {x}:{y}", null, 1013);
			return true;
		}
		Trace($"Failed to place ({type}) {cls} at {x}:{y}", null, 1016);
		return false;
	}

	protected bool SpawnDummyActorOnProp(int x, int y, CObjectProp prop, int health = 0, bool active = false, bool performSpawnedModifiers = true)
	{
		if (prop.PropHealthDetails == null || !prop.PropHealthDetails.HasHealth)
		{
			prop.PropHealthDetails = new PropHealthDetails
			{
				MaxHealthPerPartySize = new List<int> { health, health, health, health }
			};
			prop.PropHealthDetails.HasHealth = true;
		}
		if (CMap.TryCreatePropWithHealthDummy(prop, health, ScenarioManager.Tiles[x, y], initial: false, noIDRegen: false, performSpawnedModifiers, out var dummy))
		{
			Trace($"Created new dummy actor {dummy.Class.ID} at {x},{y}", null, 1035);
			dummy.Deactivated = !active;
			return true;
		}
		Error($"Failed to create dummy actor at {x},{y}", null, 1041);
		return false;
	}

	protected bool SpawnInvisibleDummyProp(int x, int y, int health = 0, bool active = false)
	{
		CObjectObstacle prop;
		return SpawnInvisibleDummyProp(x, y, out prop, health, active);
	}

	protected bool FindOrSpawnInvisibleDummyProp(int x, int y, out CObjectObstacle prop, int health = 0, bool active = false)
	{
		if (FindInvisibleBlocker(x, y, out prop))
		{
			Trace($"Blockers prop at {x},{y} already created.", null, 1056);
			return true;
		}
		return SpawnInvisibleDummyProp(x, y, out prop, health, active);
	}

	protected bool SpawnInvisibleDummyProp(int x, int y, out CObjectObstacle prop, int health = 0, bool active = false)
	{
		if (!SpawnInvisibleBlocker(x, y, out prop))
		{
			return false;
		}
		return SpawnDummyActorOnProp(x, y, prop, health, active);
	}

	protected bool IsFreeTile(CTile tile, bool checkEmpty = true)
	{
		return IsFreeTile(tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y, checkEmpty);
	}

	protected bool IsFreeTile(int x, int y, bool checkEmpty = true)
	{
		if (!ScenarioManager.PathFinder.Nodes[x, y].Walkable || ScenarioManager.PathFinder.Nodes[x, y].Blocked)
		{
			return false;
		}
		if (checkEmpty && !CSpawner.IsEmptyTile(x, y))
		{
			return false;
		}
		return true;
	}

	protected bool GetNearestFreeTile(int originX, int originY, out CTile nearestTile, IList<TileIndex> tiles, Func<int, int, bool> extraCheck = null)
	{
		extraCheck = ((extraCheck != null) ? extraCheck : ((Func<int, int, bool>)((int x, int y) => true)));
		originX = Math.Clamp(originX, 0, ScenarioManager.Tiles.GetLength(0) - 1);
		originY = Math.Clamp(originY, 0, ScenarioManager.Tiles.GetLength(1) - 1);
		_ = ScenarioManager.Tiles[originX, originY];
		nearestTile = null;
		int num = 9999999;
		foreach (TileIndex tile in tiles)
		{
			int tileDistance = ScenarioManager.GetTileDistance(originX, originY, tile.X, tile.Y);
			if (tileDistance < num && IsFreeTile(tile.X, tile.Y) && extraCheck(tile.X, tile.Y))
			{
				num = tileDistance;
				nearestTile = ScenarioManager.Tiles[tile.X, tile.Y];
			}
		}
		if (nearestTile == null)
		{
			return false;
		}
		return true;
	}

	protected bool GetNearestFreeTile(int originX, int originY, out CTile nearestTile, List<CTile> customTilesList = null, Func<int, int, bool> extraCheck = null)
	{
		extraCheck = ((extraCheck != null) ? extraCheck : ((Func<int, int, bool>)((int x, int y) => true)));
		originX = Math.Clamp(originX, 0, ScenarioManager.Tiles.GetLength(0) - 1);
		originY = Math.Clamp(originY, 0, ScenarioManager.Tiles.GetLength(1) - 1);
		if (customTilesList == null)
		{
			customTilesList = new List<CTile>();
			CTile[,] tiles = ScenarioManager.Tiles;
			foreach (CTile item in tiles)
			{
				customTilesList.Add(item);
			}
		}
		_ = ScenarioManager.Tiles[originX, originY];
		nearestTile = null;
		int num3 = 9999999;
		foreach (CTile customTiles in customTilesList)
		{
			if (customTiles != null)
			{
				Point arrayIndex = customTiles.m_ArrayIndex;
				int tileDistance = ScenarioManager.GetTileDistance(originX, originY, arrayIndex.X, arrayIndex.Y);
				if (tileDistance < num3 && IsFreeTile(arrayIndex.X, arrayIndex.Y) && extraCheck(arrayIndex.X, arrayIndex.Y))
				{
					num3 = tileDistance;
					nearestTile = customTiles;
				}
			}
		}
		if (nearestTile == null)
		{
			return false;
		}
		return true;
	}

	protected bool InSameRoom(CActor a1, CActor a2)
	{
		if (a1 == null || a2 == null)
		{
			return false;
		}
		CTile cTile = ScenarioManager.Tiles[a1.ArrayIndex.X, a1.ArrayIndex.Y];
		CTile cTile2 = ScenarioManager.Tiles[a2.ArrayIndex.X, a2.ArrayIndex.Y];
		if (cTile.m_HexMap != cTile2.m_HexMap && cTile.m_Hex2Map != cTile2.m_HexMap)
		{
			return cTile.m_HexMap == cTile2.m_Hex2Map;
		}
		return true;
	}

	protected void TeleportActor(CActor actor, CTile t, bool tryReveal = false, CAbilityMove moveAbility = null, bool invokeOnMovedListeners = false, bool useAnim = true)
	{
		if (actor.IsDead)
		{
			Trace($"Trying to teleport dead actor: {actor.Class.ID} to {t.m_ArrayIndex}", null, 1193);
			return;
		}
		GameState.TeleportActor(actor, t, tryReveal, moveAbility, invokeOnMovedListeners, useAnim);
		Trace($"Teleported {actor.Class.ID} with health {actor.Health} to {t.m_ArrayIndex}", null, 1197);
	}

	protected void RevealRoom(CMap room)
	{
		if (!room.Revealed)
		{
			room.Reveal(initial: false);
			CRevealRooms_MessageData message = new CRevealRooms_MessageData();
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	protected CAbility GetScenarioAbilityByID(string abilityID)
	{
		return ScenarioRuleClient.SRLYML.ScenarioAbilities.FirstOrDefault((ScenarioAbilitiesYMLData s) => s.ScenarioAbilityID == abilityID)?.ScenarioAbilities[0];
	}

	protected void DealScenarioDamage(CActor actorToDamage, int damageAmount, bool isTrapDamage = false, bool isTerrainDamage = false, string damageSourceLocKey = null, string customInflictLogMessageLocKey = null, string customDamageLogMessageLocKey = null, string customAnim = null, bool showInflictMessage = true, bool checkIfPlayerCanAvoidDamage = true)
	{
		if (showInflictMessage)
		{
			CScenarioDamage_MessageData message = new CScenarioDamage_MessageData(actorToDamage, damageAmount, damageSourceLocKey, customInflictLogMessageLocKey);
			ScenarioRuleClient.MessageHandler(message);
		}
		CAbilityMove cAbilityMove = _tickAbility as CAbilityMove;
		if (cAbilityMove != null && cAbilityMove.TargetingActor == actorToDamage && cAbilityMove.GetStateName() == "ActorHasMoved")
		{
			List<CActiveBonus> list = actorToDamage.FindPreventDamageBonuses();
			bool flag = list != null && list.Count > 0;
			if (actorToDamage.OriginalType == CActor.EType.Player || (actorToDamage is CHeroSummonActor cHeroSummonActor && (flag || cHeroSummonActor.IsCompanionSummon)))
			{
				cAbilityMove.LocoPause(actorToDamage, pause: true);
				if (actorToDamage.WillActorDie(damageAmount) && !flag)
				{
					cAbilityMove.LocoStop(cancelAbilityOnStop: true);
				}
			}
		}
		bool skipHitAnimation = cAbilityMove?.Jump ?? false;
		int health = actorToDamage.Health;
		bool actorWasAsleep = actorToDamage.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
		bool isTrapDamage2 = isTrapDamage;
		bool isTerrainDamage2 = isTerrainDamage;
		GameState.ActorBeenDamaged(actorToDamage, damageAmount, checkIfPlayerCanAvoidDamage, null, null, CAbility.EAbilityType.None, 0, isTrapDamage2, isTerrainDamage2, cannotPreventDamageWithActiveBonus: false, pierceInvulnerable: false, 0, triggerOnDamageListeners: true, 0, skipHitAnimation);
		if ((!(actorToDamage.Class is CCharacterClass) || GameState.PlayerSelectedToAvoidDamage == GameState.EAvoidDamageOption.None) && GameState.ActorHealthCheck(actorToDamage, actorToDamage, isTrap: true, isTerrain: false, actorWasAsleep))
		{
			CActorBeenDamaged_MessageData message2 = new CActorBeenDamaged_MessageData(actorToDamage)
			{
				m_ActorBeingDamaged = actorToDamage,
				m_DamageAbility = null,
				m_ActorOriginalHealth = health,
				m_ActualDamage = health - actorToDamage.Health,
				m_ActorWasAsleep = actorWasAsleep,
				m_DamageSourceLocKey = damageSourceLocKey,
				m_OverrideLogMessageLocKey = customDamageLogMessageLocKey,
				m_OverrideAnimState = customAnim,
				m_skipHitAnimation = skipHitAnimation
			};
			ScenarioRuleClient.MessageHandler(message2);
		}
	}

	protected void Gain<T>(CActor actor, CAbility.EAbilityType type, CActor target = null) where T : CAbility
	{
		T val = CAbility.CreateAbility(type, CAbilityFilterContainer.CreateDefaultFilter(), actor.Type == CActor.EType.Enemy, isTargetedAbility: true) as T;
		val.TargetingActor = target;
		val.ApplyToActor(actor);
	}

	protected void GainNeg<T>(CActor actor, CAbility.EAbilityType type, CCondition.ENegativeCondition neg, CActor target = null) where T : CAbility
	{
		Gain<T>(actor, type, target);
		CConditionCombatLog_MessageData message = new CConditionCombatLog_MessageData(actor, CCondition.EPositiveCondition.NA, neg, (target == null) ? actor : target);
		ScenarioRuleClient.MessageHandler(message);
	}

	protected void GainCurse(CActor actor)
	{
		GainNeg<CAbilityCurse>(actor, CAbility.EAbilityType.Curse, CCondition.ENegativeCondition.Curse);
	}

	protected void GainCurseTwice(CActor actor)
	{
		GainCurse(actor);
		GainCurse(actor);
	}

	protected void GainPos<T>(CActor actor, CAbility.EAbilityType type, CCondition.EPositiveCondition pos, CActor target = null) where T : CAbility
	{
		Gain<T>(actor, type, target);
		CConditionCombatLog_MessageData message = new CConditionCombatLog_MessageData(actor, pos, CCondition.ENegativeCondition.NA, (target == null) ? actor : target);
		ScenarioRuleClient.MessageHandler(message);
	}

	protected bool OncePerRound()
	{
		if (_oncePerRound == ScenarioManager.CurrentScenarioState.RoundNumber)
		{
			return false;
		}
		_oncePerRound = ScenarioManager.CurrentScenarioState.RoundNumber;
		return true;
	}

	public CActor FindActorByGUID(string guid, bool errorIfNotFound = true)
	{
		CActor cActor = ScenarioManager.Scenario.AllActors.FirstOrDefault((CActor x) => x.ActorGuid == guid);
		if ((cActor == null) & errorIfNotFound)
		{
			Error("Could not find actor with GUID " + guid, null, 1340);
		}
		return cActor;
	}

	public CActor FindActorByClass(string ID, bool errorIfNotFound = true)
	{
		CActor cActor = ScenarioManager.Scenario.AllActors.FirstOrDefault((CActor x) => x.Class?.ID == ID);
		if ((cActor == null) & errorIfNotFound)
		{
			Error("Could not find actor with class " + ID, null, 1347);
		}
		return cActor;
	}

	protected CObjectProp GetPropPartName(int x, int y, string prefabName)
	{
		return ScenarioManager.CurrentScenarioState.Props.Find((CObjectProp p) => p.ArrayIndex.X == x && p.ArrayIndex.Y == y && p.PrefabName.Contains(prefabName, StringComparison.OrdinalIgnoreCase));
	}

	protected CObjectProp GetProp(int x, int y, string prefabName, bool checkDestroyed = false)
	{
		CObjectProp cObjectProp = ScenarioManager.CurrentScenarioState.Props.Find((CObjectProp p) => p.PrefabName == prefabName && p.ArrayIndex.X == x && p.ArrayIndex.Y == y);
		if ((cObjectProp == null) & checkDestroyed)
		{
			cObjectProp = ScenarioManager.CurrentScenarioState.DestroyedProps.Find((CObjectProp p) => p.PrefabName == prefabName && p.ArrayIndex.X == x && p.ArrayIndex.Y == y);
		}
		return cObjectProp;
	}

	protected CObjectProp GetProp(int x, int y)
	{
		return ScenarioManager.CurrentScenarioState.Props.Find((CObjectProp p) => p.ArrayIndex.X == x && p.ArrayIndex.Y == y);
	}

	protected CSpawner GetSpawner(Point point)
	{
		return ScenarioManager.CurrentScenarioState.Spawners.FirstOrDefault((CSpawner s) => s.ArrayIndex.ToPoint() == point);
	}

	private bool FindInvisibleBlocker(int x, int y, out CObjectObstacle obs)
	{
		CTile cTile = ScenarioManager.Tiles[x, y];
		if (cTile == null)
		{
			obs = null;
			Error("Invalid tile at {x},{y}", null, 1376);
			return false;
		}
		obs = cTile.m_Props.FirstOrDefault((CObjectProp p) => p.SkipVisuals) as CObjectObstacle;
		return obs != null;
	}

	protected bool FindOrSpawnInvisibleBlocker(int x, int y, out CObjectObstacle obs)
	{
		if (!FindInvisibleBlocker(x, y, out obs))
		{
			return SpawnInvisibleBlocker(x, y, out obs);
		}
		Trace($"Blockers prop at {x},{y} already created", null, 1387);
		return true;
	}

	protected bool SpawnInvisibleBlocker(int x, int y, out CObjectObstacle obs)
	{
		CTile cTile = ScenarioManager.Tiles[x, y];
		if (cTile == null)
		{
			obs = null;
			Error("Invalid tile at {x},{y}", null, 1395);
			return false;
		}
		obs = new CObjectObstacle("SkipVisuals", ScenarioManager.ObjectImportType.Tile, new TileIndex(x, y), null, null, new List<TileIndex>(), null, cTile.m_HexMap.MapGuid, ignoresFlyAndJump: true);
		cTile.SpawnProp(obs, notifyClient: false);
		Trace($"Created blockers prop at {x},{y}", null, 1405);
		return true;
	}

	public static void PickHexes(string promptLocKey, CActor actor, List<CTile> validTiles, CAbilityNullHex.CallbackFunc callback, int numTilesToPick = 1, bool animateActor = true, CAbilityNullHex.CalcValidTilesInRangeFunc calcValidTilesInRange = null, bool canSkip = true, bool canUndo = true, bool recalculateValidTilesOnSelect = false, bool preSelectValidTiles = true, string animOverload = "UseItem", CAreaEffect areaEffect = null, CAbilityFilter.EFilterTile tileFilter = CAbilityFilter.EFilterTile.None, bool ignoreLos = false, bool ignoreDoorLocks = false, bool stackAsSeparateAbilities = false, bool autoTrigger = false, List<(string, TValue)> miscVars = null, bool interruptCurrentAbility = false, bool tryInject = false, List<(string, object)> extraTilePreviews = null, bool ignoreStun = false)
	{
		if ((validTiles == null || validTiles.Count == 0) && calcValidTilesInRange == null)
		{
			return;
		}
		CAbilityNullHex.ExternalParams val = new CAbilityNullHex.ExternalParams
		{
			Actor = actor,
			CalcValidTilesInRange = calcValidTilesInRange,
			TilesInRange = validTiles,
			Callback = callback,
			numTilesToPick = numTilesToPick,
			AnimateActor = animateActor,
			RecalculateValidTilesOnSelect = recalculateValidTilesOnSelect
		};
		CVars cVars = new CVars();
		cVars.Set("ExternalParams", new TValue(val));
		cVars.Set("PreSelectValidTiles", new TValue(preSelectValidTiles));
		CVarsContext context = new CVarsContext(cVars);
		CAbilityNullHex cAbilityNullHex = CAbility.CreateAbility(CAbility.EAbilityType.NullHex, 1, useSpecialBaseStat: false, 99, 1, 0, CAbility.EAbilityFlag.None, CAbilityFilterContainer.CreateDefaultFilter(), null, null, null, areaEffect, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, null, null, null, null, null, null, null, null, null, multiPassAttack: false, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, null, textOnly: false, noLayoutSpace: false, showRange: false, showTarget: false, showArea: false, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, null, null, CAbility.EAbilityTargeting.All, null, null, null, null, null, isMonster: false, null, null, isSubAbility: false, isInlineSubAbility: false, 0, 0, isTargetedAbility: true, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, null, null, 0, null, new AbilityData.MiscAbilityData
		{
			AutotriggerAbility = autoTrigger
		}, skipAnim: false, 0, EConditionDecTrigger.None, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, null, 0, null, null, null, null, CAbility.EAttackType.None, null, null, CAbility.ECardAction.None, null, null, CAbilityExtraTurn.EExtraTurnType.None, null, null, null, null, null, null, null, null, immuneToSelfDamage: false, null, null, null, null, null, null, ECharacter.None, CAbilityFilter.EFilterTile.None, null, isDefault: false, isItemAbility: false, showElementPicker: false, isModifierAbility: false, null, null, null, null, exploitVulnerability: false, null, null, int.MaxValue) as CAbilityNullHex;
		cAbilityNullHex.SetCanSkip(canSkip);
		cAbilityNullHex.SetCanUndo(canUndo);
		cAbilityNullHex.AnimOverload = animOverload;
		cAbilityNullHex.HelpBoxTooltipLocKey = promptLocKey;
		cAbilityNullHex.AddParentContext(context);
		cAbilityNullHex.TileFilter = tileFilter;
		cAbilityNullHex.ShouldBePlayerControlled = true;
		if (extraTilePreviews != null)
		{
			if (cAbilityNullHex.Vars == null)
			{
				cAbilityNullHex.Vars = new List<(string, TValue)>();
			}
			foreach (var extraTilePreview in extraTilePreviews)
			{
				cAbilityNullHex.Vars.Add((extraTilePreview.Item1, new TValue(extraTilePreview.Item2)));
			}
			cAbilityNullHex.PerformVars = new List<(string, List<(string, TValue)>)> { ("SelectHexPositions", new List<(string, TValue)> { ("RecalcForcePreview", new TValue(CExpression.Parse("obj.RecalcForcePreviewVars()"))) }) };
		}
		if (ignoreStun)
		{
			if (cAbilityNullHex.MiscAbilityData == null)
			{
				cAbilityNullHex.MiscAbilityData = new AbilityData.MiscAbilityData();
			}
			cAbilityNullHex.MiscAbilityData.IgnoreStun = true;
		}
		if (ignoreLos)
		{
			if (cAbilityNullHex.MiscVars == null)
			{
				cAbilityNullHex.MiscVars = new List<(string, TValue)>();
			}
			cAbilityNullHex.MiscVars.Add(("IgnoreAreaEffectLos", true));
		}
		if (ignoreDoorLocks)
		{
			if (cAbilityNullHex.MiscVars == null)
			{
				cAbilityNullHex.MiscVars = new List<(string, TValue)>();
			}
			cAbilityNullHex.MiscVars.Add(("IgnoreDoorLocks", true));
		}
		if (miscVars != null)
		{
			if (cAbilityNullHex.MiscVars == null)
			{
				cAbilityNullHex.MiscVars = new List<(string, TValue)>();
			}
			foreach (var miscVar in miscVars)
			{
				cAbilityNullHex.MiscVars.Add((miscVar.Item1, miscVar.Item2));
			}
		}
		if (actor == null)
		{
			actor = GameState.HostCharacter;
		}
		CPhaseAction currentPhaseAction = PhaseManager.CurrentPhaseAction;
		if (currentPhaseAction != null)
		{
			if (tryInject && currentPhaseAction.CurrentPhaseAbilities.Count > 0)
			{
				currentPhaseAction.InjectAbility(cAbilityNullHex, actor, cAbilityNullHex.GetAbilityBaseCard());
			}
			else if (stackAsSeparateAbilities)
			{
				currentPhaseAction.StackNextAbilities(new List<CAbility> { cAbilityNullHex }, actor, killAfter: false, stackToNextCurrent: true, copyCurrentActionID: false, interruptCurrentAbility);
			}
			else
			{
				currentPhaseAction.StackInlineSubAbilities(new List<CAbility> { cAbilityNullHex }, actor, performNow: true, stopPlayerSkipping: false, true, stopPlayerUndo: false, null, ignorePerformNow: false, actor);
			}
		}
		else
		{
			cAbilityNullHex.IsScenarioModifierAbility = true;
			GameState.PendingScenarioModifierAbilities.Add(new Tuple<CAbility, CActor>(cAbilityNullHex, actor));
		}
	}

	public virtual void OnPerformingAbility(CAbility ability)
	{
	}

	public virtual bool IsUnderwater(CTile tile = null)
	{
		return false;
	}

	protected void PutSnorkelsOn(CActor actor)
	{
		if (actor.Type == CActor.EType.Player || actor.Type == CActor.EType.HeroSummon)
		{
			Trace("Trying to put the snorkel on " + actor.Class.ID, null, 1535);
			HideShowChildObject_MessageData message = new HideShowChildObject_MessageData(actor)
			{
				m_GOName = "EXT_Snorkel",
				m_Show = true,
				m_SilentFail = true,
				m_waitForActorCreation = true
			};
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	protected void PutSnorkelsOn()
	{
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			PutSnorkelsOn(playerActor);
		}
		foreach (CHeroSummonActor allHeroSummon in ScenarioManager.Scenario.AllHeroSummons)
		{
			PutSnorkelsOn(allHeroSummon);
		}
	}

	protected void UnderwaterStart()
	{
		if (_tickActor.Class.ID != "DeepwraithID" && _tickActor.Class.ID != "CrashingTideID" && _tickActor is CPlayerActor)
		{
			GainCurseTwice(_tickActor);
		}
	}

	protected bool FindScenarioModifier(string scenarioAbilityPartialName, out CScenarioModifier sm)
	{
		sm = _css.ScenarioModifiers.FirstOrDefault((CScenarioModifier s) => s.ScenarioAbilityID != null && s.ScenarioAbilityID.Contains(scenarioAbilityPartialName));
		if (sm == null)
		{
			Error("Couldn't find scenario modifier with ability " + scenarioAbilityPartialName, null, 1570);
			foreach (CScenarioModifier scenarioModifier in _css.ScenarioModifiers)
			{
				Trace("    " + scenarioModifier.ScenarioAbilityID, null, 1572);
			}
			return false;
		}
		return true;
	}

	protected bool PerformScenarioModifier(string scenarioAbilityPartialName, CActor actor, out CScenarioModifier sm)
	{
		if (FindScenarioModifier(scenarioAbilityPartialName, out sm))
		{
			if (!sm.ApplyOnceTotal || !sm.HasBeenAppliedOnce)
			{
				ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.SingleOrDefault((ActorState x) => x.ActorGuid == actor.ActorGuid);
				if (actorState != null && sm.ScenarioModifierFilter.IsValidTarget(actorState))
				{
					Trace("Try to perform " + sm.ScenarioAbilityID + " on " + actor.Class.ID, null, 1586);
					sm.PerformScenarioModifier(_css.RoundNumber, actor, 2, forceActivate: true);
				}
			}
			return true;
		}
		return false;
	}

	protected bool PerformScenarioModifier(string scenarioAbilityPartialName, CActor actor)
	{
		CScenarioModifier sm;
		return PerformScenarioModifier(scenarioAbilityPartialName, actor, out sm);
	}

	protected List<CTile> GetRoomTiles(string roomName)
	{
		List<CTile> list = new List<CTile>();
		CTile[,] tiles = ScenarioManager.Tiles;
		foreach (CTile cTile in tiles)
		{
			if (cTile != null && cTile.m_HexMap.RoomName == roomName)
			{
				list.Add(cTile);
			}
		}
		return list;
	}

	protected Point FindNearestFreePointInPointRoom(Point targetPoint)
	{
		if (IsFreeTile(targetPoint.X, targetPoint.Y))
		{
			return targetPoint;
		}
		Point result = targetPoint;
		int num = int.MaxValue;
		foreach (CMapTile mapTile in ScenarioManager.GetTile(targetPoint).m_HexMap.MapTiles)
		{
			if (IsFreeTile(mapTile.ArrayIndex.X, mapTile.ArrayIndex.Y))
			{
				int tileDistance = ScenarioManager.GetTileDistance(targetPoint.X, targetPoint.Y, mapTile.ArrayIndex.X, mapTile.ArrayIndex.Y);
				if (tileDistance < num)
				{
					result = new Point(mapTile.ArrayIndex);
					num = tileDistance;
				}
			}
		}
		return result;
	}

	protected static void HealTo(CActor actor, int health, bool removeAllTokens = false, bool ignoreTokens = true)
	{
		int health2 = actor.Health;
		int num = health - health2;
		if (num <= 0)
		{
			return;
		}
		if (removeAllTokens)
		{
			List<NegativeConditionPair> list = new List<NegativeConditionPair>(actor.Tokens.CheckNegativeTokens);
			List<PositiveConditionPair> list2 = new List<PositiveConditionPair>(actor.Tokens.CheckPositiveTokens);
			foreach (NegativeConditionPair item in list)
			{
				if (item.NegativeCondition != CCondition.ENegativeCondition.Curse)
				{
					actor.RemoveNegativeConditionToken(item.NegativeCondition, EConditionDecTrigger.None, actor);
				}
			}
			foreach (PositiveConditionPair item2 in list2)
			{
				if (item2.PositiveCondition != CCondition.EPositiveCondition.Bless)
				{
					actor.RemovePositiveConditionToken(item2.PositiveCondition);
				}
			}
		}
		actor.Healed(health - actor.Health, ignoreTokens);
		CActorBeenHealed_MessageData message = new CActorBeenHealed_MessageData(actor, actor, num, health2, poisonTokenRemoved: false, woundTokenRemoved: false, baneTokenRemoved: false, brittleTokenRemoved: false);
		ScenarioRuleClient.MessageHandler(message);
	}

	protected static void Heal(CActor actor, int healAmount, bool removeAllTokens = false)
	{
		if (healAmount > 0)
		{
			int health = actor.Health;
			HealTo(actor, health + healAmount, removeAllTokens);
		}
	}

	public virtual List<CTile> NullAbilityTargetedTiles(CAbilityNull ability)
	{
		return null;
	}

	public virtual List<CTile> NullAbilityAreaTiles(CAbilityNull ability)
	{
		return null;
	}

	protected void SpawnResourceProp(string resourceId, string resourcePrefabName, CTile tile)
	{
		CObjectResource prop = new CObjectResource(ScenarioRuleClient.SRLYML.CharacterResources.SingleOrDefault((CharacterResourceData x) => x.ID == resourceId), resourcePrefabName, ScenarioManager.ObjectImportType.Resource, new TileIndex(tile.m_ArrayIndex), null, null, null, tile.m_HexMap.MapGuid);
		tile.SpawnProp(prop);
	}

	protected CObjectProp SpawnVFXProp(CTile tile, string prefabName, string nameLocKey = null, string decLocKey = null, CVector3 rotation = null, float delay = 0f)
	{
		TileIndex arrayIndex = new TileIndex(tile.m_ArrayIndex);
		CObjectProp cObjectProp = new CObjectProp(prefabName, ScenarioManager.ObjectImportType.VFXProp, arrayIndex, null, rotation, null, tile.m_HexMap.MapGuid);
		cObjectProp.PropCustomLocKey = nameLocKey;
		cObjectProp.PropCustomDescriptionLocKey = decLocKey;
		tile.SpawnProp(cObjectProp, notifyClient: true, delay);
		return cObjectProp;
	}

	protected CObjectObstacle SpawnObstacleProp(CTile tile, List<TileIndex> pathingBlockers, string prefabName, string nameLocKey = null, CVector3 rotation = null, bool ignoresFlyAndJump = false, bool blockLOS = false)
	{
		TileIndex arrayIndex = new TileIndex(tile.m_ArrayIndex);
		CObjectObstacle cObjectObstacle = new CObjectObstacle(prefabName, ScenarioManager.ObjectImportType.Obstacle, arrayIndex, null, rotation, pathingBlockers, null, tile.m_HexMap.MapGuid, ignoresFlyAndJump, setPositionToCenterOfAllPathBlockers: false, blockLOS);
		cObjectObstacle.PropCustomLocKey = nameLocKey;
		tile.SpawnProp(cObjectObstacle);
		return cObjectObstacle;
	}

	public virtual bool CanActorLootResources(CActor actor)
	{
		return false;
	}

	public virtual bool CanActorLootQuestItems(CActor actor)
	{
		return false;
	}

	public virtual bool CanActorIgnoreIceSlideEffect(CActor actor)
	{
		return false;
	}

	protected void GiveCard(params int[] ids)
	{
		List<CAbilityCard> list = new List<CAbilityCard>();
		foreach (CAbilityCard allAbilityCard in CharacterClassManager.AllAbilityCards)
		{
			if (ids.Contains(allAbilityCard.ID))
			{
				CPlayerActor cPlayerActor = FindPlayer(allAbilityCard);
				if (cPlayerActor != null && !ContainsCard(cPlayerActor.CharacterClass.HandAbilityCards, allAbilityCard) && !ContainsCard(cPlayerActor.CharacterClass.ActivatedAbilityCards, allAbilityCard))
				{
					list.Add(allAbilityCard);
				}
			}
		}
		if (list.Count == 0)
		{
			return;
		}
		string text = null;
		foreach (CAbilityCard item in list)
		{
			CPlayerActor cPlayerActor2 = FindPlayer(item);
			if (ContainsCard(cPlayerActor2.CharacterClass.HandAbilityCards, item) || ContainsCard(cPlayerActor2.CharacterClass.ActivatedAbilityCards, item))
			{
				return;
			}
			CAbilityCard cardInstance = null;
			List<CAbilityCard> outDeck = null;
			if (FindCardInstance(cPlayerActor2.CharacterClass.DiscardedAbilityCards, item, out outDeck, out cardInstance))
			{
				text = "DiscardedAbilityCards";
			}
			else if (FindCardInstance(cPlayerActor2.CharacterClass.LostAbilityCards, item, out outDeck, out cardInstance))
			{
				text = "LostAbilityCards";
			}
			else
			{
				cardInstance = item;
				if (!CharacterClassManager.AllAbilityCardInstances.Contains(cardInstance))
				{
					CharacterClassManager.AllAbilityCardInstances.Add(cardInstance);
				}
				cPlayerActor2.CharacterClass.HandAbilityCards.Add(cardInstance);
				if (!ContainsCard(cPlayerActor2.CharacterClass.AbilityCardsPool, item))
				{
					cPlayerActor2.CharacterClass.AbilityCardsPool.Add(cardInstance);
				}
				if (!ContainsCard(cPlayerActor2.CharacterClass.SelectedAbilityCards, item))
				{
					cPlayerActor2.CharacterClass.SelectedAbilityCards.Add(cardInstance);
				}
				cPlayerActor2.CharacterClass.MoveAbilityCard(cardInstance, cPlayerActor2.CharacterClass.HandAbilityCards, cPlayerActor2.CharacterClass.DiscardedAbilityCards, "HandAbilityCards", "DiscardedAbilityCards");
				outDeck = cPlayerActor2.CharacterClass.DiscardedAbilityCards;
				text = "DiscardedAbilityCards";
			}
			cPlayerActor2.CharacterClass.MoveAbilityCard(cardInstance, outDeck, cPlayerActor2.CharacterClass.HandAbilityCards, text, "HandAbilityCards");
			ScenarioManager.CurrentScenarioState.TryCreateCardScripts(cPlayerActor2);
		}
		CAddNewCards_MessageData cAddNewCards_MessageData = new CAddNewCards_MessageData(_tickActor);
		cAddNewCards_MessageData.ids = ids;
		ScenarioRuleClient.MessageHandler(cAddNewCards_MessageData);
	}

	private static CPlayerActor FindPlayer(CAbilityCard card)
	{
		return ScenarioManager.Scenario.PlayerActors.FirstOrDefault((CPlayerActor x) => x.CharacterClass.ID == card.ClassID);
	}

	private static bool ContainsCard(List<CAbilityCard> deck, CAbilityCard card)
	{
		return deck.Any((CAbilityCard x) => x.ID == card.ID);
	}

	private static bool FindCardInstance(List<CAbilityCard> deck, CAbilityCard card, out List<CAbilityCard> outDeck, out CAbilityCard cardInstance)
	{
		outDeck = deck;
		cardInstance = deck.FirstOrDefault((CAbilityCard x) => x.ID == card.ID);
		return cardInstance != null;
	}

	protected static void SetHealth(CActor actor, int health)
	{
		if (actor.Health > health)
		{
			ReduceHp(actor, health);
		}
		else if (actor.Health < health)
		{
			HealTo(actor, health);
		}
	}

	protected static void ReduceHp(CActor actor, int health)
	{
		actor.Damaged(actor.Health - health, fromAttackAbility: false, null, null);
	}

	protected void SpawnStartScenarioAlliesOnStartingTiles(CSpawner spawner, int count)
	{
		List<CTile> list = ScenarioManager.CurrentScenarioState.Players.ConvertAll((PlayerState p) => p.Actor.Tile);
		foreach (CTile startingTile in ScenarioManager.StartingTiles)
		{
			if (count <= 0)
			{
				break;
			}
			if (!list.Contains(startingTile))
			{
				spawner.SetLocation(startingTile.m_Hex.ArrayIndex);
				spawner.SpawnUnit(PartySize, treatAsSummon: false, initial: false, startRound: false, forceUseAnyEntry: false, addIntoInitiativeOrder: false, spawnFarthestOnAdjacent: false, useFirstRoundStartingTile: true);
				count--;
			}
		}
		GameState.SortIntoInitiativeAndIDOrder();
	}

	public static CMap FindRoom(string name)
	{
		return ScenarioManager.Scenario.Maps.SingleOrDefault((CMap x) => x.RoomName == name);
	}

	public static CObjectDoor FindDoor(int x, int y)
	{
		return ScenarioManager.GetTile(x, y)?.FindDoor();
	}

	public static CSpawner FindSpawner(int x, int y)
	{
		return ScenarioManager.GetTile(x, y)?.m_Spawners.FirstOrDefault();
	}

	public static CActor FindActor(int x, int y)
	{
		return ScenarioManager.GetTile(x, y)?.LivingActor;
	}

	public static CObjectProp FindPropWithHealth(int x, int y)
	{
		return ScenarioManager.CurrentScenarioState.Props.Concat(ScenarioManager.CurrentScenarioState.DestroyedProps).FirstOrDefault((CObjectProp prop) => prop.ArrayIndex.X == x && prop.ArrayIndex.Y == y && prop.PropHealthDetails.HasHealth);
	}

	public static CObjectChest FindChest(int x, int y)
	{
		return (from cObjectProp in ScenarioManager.CurrentScenarioState.Props.Concat(ScenarioManager.CurrentScenarioState.DestroyedProps)
			select cObjectProp as CObjectChest).FirstOrDefault((CObjectChest chest) => chest != null && chest.ArrayIndex.X == x && chest.ArrayIndex.Y == y);
	}

	public virtual bool WillMovingOnTileEndMovement(CTile tile, CActor actor, CAbility ability)
	{
		return false;
	}

	public virtual string PropKeyOverride(CObjectProp prop, out string[] formatParameters)
	{
		formatParameters = null;
		return null;
	}

	public virtual IEnumerable<CTile> TilesCannotStopOnTopOf()
	{
		yield break;
	}

	public virtual IEnumerable<CTile> TilesFullyBlockedForMovement()
	{
		yield break;
	}

	public virtual bool OverrideHasLOS(CTile sourceTile, CTile targetTile)
	{
		return false;
	}

	public virtual void OnActorDownedChanged(CActor actor)
	{
	}

	public virtual IEnumerable<(string, string)> GetSpecialRulesParamsString()
	{
		yield break;
	}

	public virtual void OnObjectiveComplete(CObjective objective)
	{
	}

	protected void DestroyMap(CMap mapToDestroy, CScenarioModifier modifier = null)
	{
		modifier?.SetDeactivated(deactivate: true);
		CScenarioModifierDestroyRoom.DestroyMapDefault(mapToDestroy, InstaKillActors: true);
		foreach (CSpawner item in ScenarioManager.CurrentScenarioState.Spawners.Where((CSpawner x) => x.CurrentMap == mapToDestroy).ToList())
		{
			item.SetActive(active: false);
			ScenarioManager.CurrentScenarioState.Spawners.Remove(item);
		}
		List<(Point, Point)> list = new List<(Point, Point)>();
		foreach (CMap map in ScenarioManager.CurrentScenarioState.Maps)
		{
			foreach (CMapTile mapTile in map.MapTiles)
			{
				CNode node = ScenarioManager.PathFinder.GetNode(mapTile.ArrayIndex);
				List<CNavLink> list2 = null;
				list2 = ((map != mapToDestroy) ? node.NavLinks.Where((CNavLink x) => mapToDestroy.MapTiles.Any((CMapTile mapTile) => mapTile.ArrayIndex.ToPoint() == x.DestinationNode.Location)).ToList() : node.NavLinks.ToList());
				foreach (CNavLink item2 in list2)
				{
					node.NavLinks.Remove(item2);
					list.Add((mapTile.ArrayIndex.ToPoint(), item2.DestinationNode.Location));
				}
			}
		}
		ScenarioManager.ToggleTileHighlights(mapToDestroy.MapTiles.Select((CMapTile x) => x.ArrayIndex.ToPoint()), enabled: false);
		ScenarioManager.ToggleTileLinks(list, enabled: false);
	}

	protected bool IsDeathwalker(CPlayerActor playerActor)
	{
		if (playerActor != null)
		{
			return playerActor.Class.ID == "DeathwalkerID";
		}
		return false;
	}

	protected bool RemoveAndCacheShadows(CPlayerActor playerActor, ref int _removedShadows)
	{
		if (IsDeathwalker(playerActor))
		{
			foreach (CObjectProp item in ScenarioManager.CurrentScenarioState.ShadowProps.ToList())
			{
				if (item != null)
				{
					List<CObjectProp> list = new List<CObjectProp>();
					item.DestroyProp(0f, sendMessageToClient: false, forceDestroy: true);
					list.Add(item);
					CDestroyObstacle_MessageData message = new CDestroyObstacle_MessageData(null, playerActor)
					{
						m_ActorDestroyingObstacle = playerActor,
						m_Tiles = list.Select((CObjectProp prop) => prop.PropTile).ToList(),
						m_DestroyedProps = list,
						m_DestroyDelay = 0f,
						m_DestroyObstacleAbility = null
					};
					ScenarioRuleClient.MessageHandler(message);
					_removedShadows++;
				}
			}
			return _removedShadows > 0;
		}
		return false;
	}

	protected void OnPhaseIn(CPlayerActor playerActor, ref int _removedShadows)
	{
		if (playerActor == null)
		{
			return;
		}
		foreach (CHeroSummonActor item in ScenarioManager.Scenario.AllHeroSummons.FindAll((CHeroSummonActor hs) => hs.Summoner == playerActor && hs.PhasedOut))
		{
			item.ArrayIndex = FindNearestFreePointInPointRoom(playerActor.ArrayIndex);
			GameState.PhaseInActor(item);
		}
		if (!IsDeathwalker(playerActor) || _removedShadows <= 0)
		{
			return;
		}
		List<CTile> roomTiles = GetRoomTiles(playerActor.Tile.m_HexMap.RoomName);
		for (int num = 0; num < _removedShadows; num++)
		{
			if (GetNearestFreeTile(playerActor.Tile.m_ArrayIndex.X, playerActor.Tile.m_ArrayIndex.Y, out var nearestTile, roomTiles))
			{
				CVector3 rotation = new CVector3(0f, 0.5f * (float)ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(180) + 0.75f * (float)ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(360), 0f);
				CObjectShadow prop = new CObjectShadow("Shadow", ScenarioManager.ObjectImportType.Shadow, new TileIndex(nearestTile.m_ArrayIndex), null, rotation, playerActor, nearestTile.m_HexMap.MapGuid);
				nearestTile.SpawnProp(prop);
				roomTiles.Remove(nearestTile);
			}
		}
		_removedShadows = 0;
	}

	public virtual bool IsTeleportValid(CTile a, CTile b)
	{
		if (TilesCannotStopOnTopOf().Contains(b))
		{
			return false;
		}
		return true;
	}

	public virtual bool IgnoreAIFocus(CActor targetingActor, CActor target)
	{
		return false;
	}

	public virtual bool LoopUnopenedDoorParticlesOnLoad()
	{
		return false;
	}

	public virtual void OnLevelMessageWasDismissed(CLevelMessage message)
	{
	}

	protected CActor SpawnDamageRedirectionBase(CActor actor, CTile tile, bool copyBonuses = true, string summonId = "MirroredCharacterID", bool isSummon = true)
	{
		Log.Trace("Spawn damage redirection token for " + actor.ID + " " + actor.ActorGuid);
		CActorSelectingObjectPosition_MessageData message = new CActorSelectingObjectPosition_MessageData(actor)
		{
			m_TilesSelected = new List<CTile> { tile }
		};
		ScenarioRuleClient.MessageHandler(message);
		CAbilitySummon cAbilitySummon = new CAbilitySummon(new List<string> { summonId, summonId, summonId, summonId });
		cAbilitySummon.TargetingActor = actor;
		cAbilitySummon.SkipAnim = true;
		if (MonsterClassManager.Find(summonId) == null)
		{
			return null;
		}
		int chosenModelIndex = actor.ChosenModelIndex;
		int health = actor.Health;
		int maxHealth = actor.MaxHealth;
		int level = actor.Level;
		EnemyState enemyState = new EnemyState(summonId, chosenModelIndex, null, tile.m_HexMap.MapGuid, new TileIndex(tile.m_ArrayIndex), health, maxHealth, level, actor.Tokens.CheckPositiveTokens, actor.Tokens.CheckNegativeTokens, playedThisRound: true, CActor.ECauseOfDeath.StillAlive, isSummon, actor.ActorGuid, 1, CActor.EType.Ally);
		CEnemyActor cEnemyActor = ScenarioManager.Scenario.AddAllyMonster(enemyState, initial: false, noIDRegen: false, fromSpawnerAtRoundStart: false, resetHealth: false);
		cEnemyActor.CustomLocKey = actor.CustomLocKey;
		if (actor is CPlayerActor cPlayerActor)
		{
			cEnemyActor.CustomLocKey = cPlayerActor.ActorLocKey();
		}
		if (cEnemyActor != null)
		{
			foreach (PositiveConditionPair checkPositiveToken in actor.Tokens.CheckPositiveTokens)
			{
				cEnemyActor.Tokens.AddPositiveToken(checkPositiveToken.PositiveCondition, checkPositiveToken.RoundDuration, checkPositiveToken.ConditionDecTrigger, cEnemyActor, cEnemyActor);
			}
			foreach (NegativeConditionPair checkNegativeToken in actor.Tokens.CheckNegativeTokens)
			{
				cEnemyActor.Tokens.AddNegativeToken(checkNegativeToken.NegativeCondition, checkNegativeToken.RoundDuration, checkNegativeToken.ConditionDecTrigger, cEnemyActor, checkNegativeToken.Strength);
			}
			ScenarioManager.CurrentScenarioState.AllyMonsters.Add(enemyState);
			cEnemyActor.PlayedThisRound = true;
			CSummon_MessageData message2 = new CSummon_MessageData("Summon", actor)
			{
				m_ActorSummoning = actor,
				m_SummonAbility = cAbilitySummon,
				m_SummonedActors = new List<CActor> { cEnemyActor },
				m_SummonTiles = new List<CTile> { tile },
				m_SkipChoreographerWait = true
			};
			ScenarioRuleClient.MessageHandler(message2);
		}
		CPlayerActor playerActor = actor as CPlayerActor;
		if (playerActor != null)
		{
			CAbilityChangeCharacterModel obj = CAbility.CreateAbility(CAbility.EAbilityType.ChangeCharacterModel, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), isMonster: true, isTargetedAbility: true) as CAbilityChangeCharacterModel;
			obj.TargetingActor = cEnemyActor;
			obj.TargetThisActorAutomatically = cEnemyActor;
			obj.MiscAbilityData = new AbilityData.MiscAbilityData();
			obj.CharacterModel = (ECharacter)Enum.Parse(typeof(ECharacter), actor.GetPrefabName());
			obj.OverrideType = CActor.EType.Player;
			obj.IsScenarioModifierAbility = true;
			obj.MiscAbilityData.AutotriggerAbility = true;
			obj.Start(cEnemyActor, cEnemyActor);
			obj.ApplyToActor(cEnemyActor);
			if (copyBonuses)
			{
				foreach (CActiveBonus item in playerActor.ActiveBonuses.Where((CActiveBonus x) => x.Actor == playerActor))
				{
					if (item.Ability.AbilityType != CAbility.EAbilityType.RepeatAbility && !item.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.EndTurnAbility) && !item.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.MoveScript))
					{
						CAbility cAbility = CAbility.CopyAbility(item.Ability, generateNewID: true);
						cAbility.CachedAbilityBaseCard = CBaseCard.CreateDummy();
						cAbility.Start(cEnemyActor, cEnemyActor);
						cAbility.ApplyToActor(cEnemyActor);
					}
				}
			}
		}
		return cEnemyActor;
	}

	protected void ApplyScenarioActiveBonusToTickActor(CScenarioModifier holder, CActor actor, CActor targetActor = null)
	{
		CAbility ability = ScenarioRuleClient.SRLYML.ScenarioAbilities.FirstOrDefault((ScenarioAbilitiesYMLData a) => a.ScenarioAbilityID == holder.ScenarioAbilityID)?.ScenarioAbilities[0];
		CActor cActor = targetActor ?? actor;
		if (!(from x in cActor.FindApplicableActiveBonuses()
			where x.Ability.HasID(ability.ID)
			select x).Any())
		{
			CAbility cAbility = CAbility.CopyAbility(ability, generateNewID: false);
			cAbility.IsScenarioModifierAbility = true;
			cAbility.TargetThisActorAutomatically = cActor;
			cAbility.Start(actor, actor);
			cAbility.ApplyToActor(cActor);
		}
	}

	public static CTile GetClosestEmptyTile(CActor actor)
	{
		List<Point> list = new List<Point>();
		ScenarioManager.PathFinder.FloodFill_ToRange(actor, actor.ArrayIndex, 99, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: true, ignoreHazardousTerrain: true, 0, null, null, list, null, ignoreDoorLocks: false);
		return (from x in list
			where CAbilityFilter.IsTileEmpty(ScenarioManager.GetTile(x))
			orderby actor.GetDistanceTo(x)
			select ScenarioManager.GetTile(x)).FirstOrDefault();
	}

	public static CTile GetClosestEmptyTile(Point pos, CActor actor = null)
	{
		List<Point> list = new List<Point>();
		ScenarioManager.PathFinder.FloodFill_ToRange(null, pos, 99, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: true, ignoreHazardousTerrain: true, 0, null, null, list, null, ignoreDoorLocks: false);
		return (from x in list
			where (actor != null && ScenarioManager.GetTile(x)?.LivingActor == actor) || CAbilityFilter.IsTileEmpty(ScenarioManager.GetTile(x))
			orderby Point.HexDist(pos, x)
			select ScenarioManager.GetTile(x)).FirstOrDefault();
	}

	public virtual IEnumerable<EnemyState> GetMonstersThatMightSpawn(ScenarioState scenarioState)
	{
		foreach (CSpawner spawner in scenarioState.Spawners)
		{
			foreach (SpawnRoundEntry spawnRoundEntry in spawner.SpawnRoundEntries)
			{
				List<CMonsterClass> list = spawnRoundEntry.SpawnClass.Select((string x) => MonsterClassManager.Classes.FirstOrDefault((CMonsterClass s) => s.ID == x)).ToList();
				if (list.All((CMonsterClass x) => x == null))
				{
					continue;
				}
				List<EnemyState> list2 = new List<EnemyState>();
				for (int num = 1; num <= 4; num++)
				{
					CMonsterClass monsterClass = list[num - 1];
					if (monsterClass != null)
					{
						EnemyState enemyState = list2.FirstOrDefault((EnemyState x) => x.ClassID == monsterClass.ID || x.ClassID == monsterClass.NonEliteVariant?.ID);
						if (enemyState == null)
						{
							enemyState = new EnemyState(monsterClass.NonEliteVariant?.ID ?? monsterClass.ID, 0, null, spawner.ActorTypeToSpawn);
							enemyState.ConfigPerPartySize = new Dictionary<int, ScenarioManager.EPerPartySizeConfig>();
							enemyState.ConfigPerPartySize[1] = ScenarioManager.EPerPartySizeConfig.Hidden;
							enemyState.ConfigPerPartySize[2] = ScenarioManager.EPerPartySizeConfig.Hidden;
							enemyState.ConfigPerPartySize[3] = ScenarioManager.EPerPartySizeConfig.Hidden;
							enemyState.ConfigPerPartySize[4] = ScenarioManager.EPerPartySizeConfig.Hidden;
							list2.Add(enemyState);
						}
						enemyState.ConfigPerPartySize[num] = ((monsterClass.NonEliteVariant != null) ? ScenarioManager.EPerPartySizeConfig.ToElite : ScenarioManager.EPerPartySizeConfig.Normal);
					}
				}
				foreach (EnemyState item in list2)
				{
					yield return item;
				}
			}
		}
	}

	public virtual void TickPropActivated(CObjectProp prop)
	{
	}

	public bool ShowTakenPile()
	{
		return !string.IsNullOrEmpty(TakenPileTooltipKey());
	}

	public virtual string TakenPileTooltipKey()
	{
		return string.Empty;
	}

	public virtual string TakenPileNameKey()
	{
		return string.Empty;
	}

	public virtual bool CanToggleBonusesForNonPlayerAbilityStart(CAbility ability)
	{
		return false;
	}

	protected CVector3 GetRotation(Point fromPoint, Point toPoint)
	{
		int num = Point.AxialDirectionToRotation(Point.OddRToAxial(toPoint) - Point.OddRToAxial(fromPoint));
		return new CVector3(0f, num, 0f);
	}

	protected virtual void MoveProp(CObjectProp prop, CTile from, CTile to, float speed = 2f)
	{
		if (prop is CObjectShadow shadow)
		{
			if (to.FindPropOfType<CObjectShadow>() != null)
			{
				prop.DestroyProp();
			}
			else
			{
				CAbilityMoveShadow.MoveShadow(shadow, to);
			}
			return;
		}
		CTile cTile = null;
		CVector3 cVector = null;
		CVector3 rotation = prop.Rotation;
		CTile propTile = prop.PropTile;
		List<Point> tiles = prop.GetTiles();
		if (tiles.Count == 1)
		{
			prop.PropTile.DelProp(prop, recursiveTiles: true);
			cTile = ScenarioManager.GetTile(to.m_ArrayIndex);
			cVector = prop.Rotation;
			prop.SetLocation(cTile.m_ArrayIndex.ToTileIndex(), null, cVector);
			prop.PropTile.AddProp(prop, recursiveTiles: true);
		}
		else if (tiles.Count == 2)
		{
			Point pt = Point.OddRToAxial(prop.ArrayIndex.ToPoint());
			Point pt2 = Point.OddRToAxial(from.m_ArrayIndex);
			if (Point.HexDist(pt, pt2) > 0)
			{
				prop.PropTile.DelProp(prop, recursiveTiles: true);
				cTile = ScenarioManager.GetTile(from.m_ArrayIndex);
				cVector = GetRotation(from.m_ArrayIndex, to.m_ArrayIndex);
				prop.SetLocation(cTile.m_ArrayIndex.ToTileIndex(), null, cVector);
				prop.ClearTiles();
				prop.AddTile(from.m_ArrayIndex);
				prop.AddTile(to.m_ArrayIndex);
				prop.StartingMapGuid = cTile.m_HexMap.MapGuid;
				prop.PropTile.AddProp(prop, recursiveTiles: true);
			}
			else
			{
				prop.PropTile.DelProp(prop, recursiveTiles: true);
				cTile = ScenarioManager.GetTile(to.m_ArrayIndex);
				cVector = GetRotation(to.m_ArrayIndex, from.m_ArrayIndex);
				prop.SetLocation(cTile.m_ArrayIndex.ToTileIndex(), null, cVector);
				prop.ClearTiles();
				prop.AddTile(to.m_ArrayIndex);
				prop.AddTile(from.m_ArrayIndex);
				prop.StartingMapGuid = cTile.m_HexMap.MapGuid;
				prop.PropTile.AddProp(prop, recursiveTiles: true);
			}
		}
		CMoveProp_MessageData message = new CMoveProp_MessageData
		{
			m_MoveProp = prop,
			m_MoveToTile = cTile,
			m_MoveSpeed = speed,
			m_MoveFromTile = propTile,
			m_MoveFromRotation = rotation,
			m_MoveToRotation = cVector,
			m_MoveLogicPos = false
		};
		ScenarioRuleClient.MessageHandler(message);
	}

	protected virtual void MovePlatformCallback(CObjectProp boat, CActor movingActor, CTile tileToMoveTo)
	{
		MovePlatformCallback(boat, movingActor.Tile, tileToMoveTo, movingActor);
	}

	protected virtual void MovePlatformCallback(CObjectProp boat, CTile tileToMoveFrom, CTile tileToMoveTo, CActor movingActor = null)
	{
		boat.GetTiles();
		foreach (CObjectProp item in tileToMoveTo.m_Props.ToList())
		{
			if (item.PropType == EPropType.Trap && tileToMoveFrom.LivingActor == null)
			{
				item.DestroyProp();
			}
		}
		List<Point> curTiles = boat.GetTiles();
		List<CActor> list = ScenarioManager.Scenario.AllAliveActors.Where((CActor actor) => curTiles.Contains(actor.ArrayIndex)).ToList();
		List<CObjectProp> list2 = ScenarioManager.CurrentScenarioState.Props.Where((CObjectProp prop) => curTiles.Contains(prop.PropTile.m_ArrayIndex)).ToList();
		MoveProp(boat, tileToMoveFrom, tileToMoveTo);
		List<Point> tiles = boat.GetTiles();
		CTile cTile = movingActor?.Tile;
		foreach (CActor item2 in list)
		{
			if (movingActor == null || cTile != item2.Tile || item2 == movingActor)
			{
				int index = curTiles.IndexOf(item2.ArrayIndex);
				Point targetPoint = tiles[index];
				GameState.MoveActorStraightLine(item2, targetPoint, shouldWait: false, slideWithoutAnim: true, 2f);
			}
		}
		foreach (CObjectProp item3 in list2)
		{
			if (item3 != boat && !(item3 is CObjectDoor) && !(item3 is CObjectHazardousTerrain) && !(item3 is CObjectIceTerrain) && !(item3 is CObjectDifficultTerrain))
			{
				int index2 = curTiles.IndexOf(item3.PropTile.m_ArrayIndex);
				Point point = tiles[index2];
				MoveProp(item3, item3.PropTile, ScenarioManager.GetTile(point));
			}
		}
		foreach (CObjectDoor doorProp in ScenarioManager.CurrentScenarioState.DoorProps)
		{
			if (doorProp.DoorIsLocked && Point.HexDist(doorProp.PropTile.m_ArrayIndex, tileToMoveTo.m_ArrayIndex) <= 1)
			{
				doorProp.UnlockLockedDoorWithoutOpening();
			}
		}
		List<CObjectDoor> list3 = (from arrayIndex in boat.GetTiles()
			select ScenarioManager.GetTile(arrayIndex)?.FindDoor() into door
			where door != null
			select door).ToList();
		if (list3.Any((CObjectDoor x) => x.DoorIsClosed))
		{
			foreach (CObjectDoor item4 in list3)
			{
				item4.ForceActivate(null);
			}
		}
		foreach (Point tile2 in boat.GetTiles())
		{
			CTile tile = ScenarioManager.GetTile(tile2);
			tile.FindTrapProp()?.Activate(tile.LivingActor);
			tile.FindHazardousProp()?.Activate(tile.LivingActor);
		}
		ScenarioRuleClient.MessageHandler(new CWaitForActorToBeAtTile_MessageData(boat, boat.PropTile, GameState.SelectedContinueAbility));
		GameState.WaitForConfirmation("FinishedMovingPlatform", waitForMultiplayer: true);
	}

	public virtual bool AllowPushWhenNoPushTiles(CActor actor)
	{
		return false;
	}

	public virtual bool AllowPullWhenNoPushTiles(CActor actor)
	{
		return false;
	}

	public virtual void RemoveInvalidTilesForAbility(CAbility ability, List<CTile> tiles)
	{
	}
}
