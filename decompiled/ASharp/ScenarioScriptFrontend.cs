using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AStar;
using FH.ApparanceBaking;
using FH.LevelComponents.Occlusion;
using FH.LevelComponents.ProcGen;
using FH.SeasonTech;
using Libs.GameConsole;
using Libs.Utils;
using RuntimeInspectorNamespace;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.InputSystem;

public class ScenarioScriptFrontend
{
	public class MapTileGenBeforeLightsAttribute : Attribute
	{
	}

	protected class GameObjectHeightInfo
	{
		public GameObject go;

		public float originalHeight;

		public int platform;

		public bool dirty;
	}

	private static bool SkipScenarioBackground_cvar;

	private static bool ShowOrientation_cvar;

	private static int ShowWorldCursor_cvar;

	private static bool TraceRendererVisiblity_cvar;

	private bool deinitialized;

	protected bool initialized;

	private static Dictionary<ScenarioScript, ScenarioScriptFrontend> _clientScripts = new Dictionary<ScenarioScript, ScenarioScriptFrontend>();

	protected static ScenarioScriptFrontend _tickSSF;

	protected Dictionary<string, RectTransform> customRectsForHighlights = new Dictionary<string, RectTransform>();

	public static Dictionary<Type, Action<ScenarioScript>> scriptsTick = new Dictionary<Type, Action<ScenarioScript>>
	{
		{
			typeof(ScenarioScript_FH009),
			(ScenarioScript ss) =>
			{
				InvokeTick<ScenarioScriptFrontend_FH009>(ss);
			}
		},
		{
			typeof(ScenarioScript_FH015),
			(ScenarioScript ss) =>
			{
				InvokeTick<ScenarioScriptFrontend_FH015>(ss);
			}
		},
		{
			typeof(ScenarioScript_FH016),
			(ScenarioScript ss) =>
			{
				InvokeTick<ScenarioScriptFrontend_FH016>(ss);
			}
		},
		{
			typeof(ScenarioScript_FH017),
			(ScenarioScript ss) =>
			{
				InvokeTick<ScenarioScriptFrontend_FH017>(ss);
			}
		},
		{
			typeof(ScenarioScript_FH024),
			(ScenarioScript ss) =>
			{
				InvokeTick<ScenarioScriptFrontend_FH024>(ss);
			}
		},
		{
			typeof(ScenarioScript_FH034),
			(ScenarioScript ss) =>
			{
				InvokeTick<ScenarioScriptFrontend_FH034>(ss);
			}
		},
		{
			typeof(ScenarioScript_FH046),
			(ScenarioScript ss) =>
			{
				InvokeTick<ScenarioScriptFrontend_FH046>(ss);
			}
		},
		{
			typeof(ScenarioScript_FH047),
			(ScenarioScript ss) =>
			{
				InvokeTick<ScenarioScriptFrontend_FH047>(ss);
			}
		},
		{
			typeof(ScenarioScript_FH049),
			(ScenarioScript ss) =>
			{
				InvokeTick<ScenarioScriptFrontend_FH049>(ss);
			}
		},
		{
			typeof(ScenarioScript_FH050),
			(ScenarioScript ss) =>
			{
				InvokeTick<ScenarioScriptFrontend_FH050>(ss);
			}
		},
		{
			typeof(ScenarioScript_FH052),
			(ScenarioScript ss) =>
			{
				InvokeTick<ScenarioScriptFrontend_FH052>(ss);
			}
		},
		{
			typeof(ScenarioScript_FH059),
			(ScenarioScript ss) =>
			{
				InvokeTick<ScenarioScriptFrontend_FH059>(ss);
			}
		},
		{
			typeof(ScenarioScript_FH064),
			(ScenarioScript ss) =>
			{
				InvokeTick<ScenarioScriptFrontend_FH064>(ss);
			}
		},
		{
			typeof(ScenarioScript_FH073),
			(ScenarioScript ss) =>
			{
				InvokeTick<ScenarioScriptFrontend_FH073>(ss);
			}
		},
		{
			typeof(ScenarioScript_FH078),
			(ScenarioScript ss) =>
			{
				InvokeTick<ScenarioScriptFrontend_FH078>(ss);
			}
		},
		{
			typeof(ScenarioScript_FH080),
			(ScenarioScript ss) =>
			{
				InvokeTick<ScenarioScriptFrontend_FH080>(ss);
			}
		},
		{
			typeof(ScenarioScript_FH089),
			(ScenarioScript ss) =>
			{
				InvokeTick<ScenarioScriptFrontend_FH089>(ss);
			}
		},
		{
			typeof(ScenarioScript_FH096),
			(ScenarioScript ss) =>
			{
				InvokeTick<ScenarioScriptFrontend_FH096>(ss);
			}
		},
		{
			typeof(ScenarioScript_FH098),
			(ScenarioScript ss) =>
			{
				InvokeTick<ScenarioScriptFrontend_FH098>(ss);
			}
		},
		{
			typeof(ScenarioScript_FH109),
			(ScenarioScript ss) =>
			{
				InvokeTick<ScenarioScriptFrontend_FH109>(ss);
			}
		},
		{
			typeof(ScenarioScript_FH099),
			(ScenarioScript ss) =>
			{
				InvokeTick<ScenarioScriptFrontend_FH099>(ss);
			}
		},
		{
			typeof(ScenarioScript_FH102),
			(ScenarioScript ss) =>
			{
				InvokeTick<ScenarioScriptFrontend_FH102>(ss);
			}
		},
		{
			typeof(ScenarioScript_FH123),
			(ScenarioScript ss) =>
			{
				InvokeTick<ScenarioScriptFrontend_FH123>(ss);
			}
		},
		{
			typeof(ScenarioScript_FH127),
			(ScenarioScript ss) =>
			{
				InvokeTick<ScenarioScriptFrontend_FH127>(ss);
			}
		},
		{
			typeof(ScenarioScript_FH130),
			(ScenarioScript ss) =>
			{
				InvokeTick<ScenarioScriptFrontend_FH130>(ss);
			}
		},
		{
			typeof(ScenarioScript_FH131),
			(ScenarioScript ss) =>
			{
				InvokeTick<ScenarioScriptFrontend_FH131>(ss);
			}
		},
		{
			typeof(ScenarioScript_FH132),
			(ScenarioScript ss) =>
			{
				InvokeTick<ScenarioScriptFrontend_FH132>(ss);
			}
		},
		{
			typeof(ScenarioScript_FH136),
			(ScenarioScript ss) =>
			{
				InvokeTick<ScenarioScriptFrontend_FH136>(ss);
			}
		},
		{
			typeof(ScenarioScript_RFH001),
			(ScenarioScript ss) =>
			{
				InvokeTick<ScenarioScriptFrontend_RFH001>(ss);
			}
		},
		{
			typeof(ScenarioScript_RFH002),
			(ScenarioScript ss) =>
			{
				InvokeTick<ScenarioScriptFrontend_RFH002>(ss);
			}
		},
		{
			typeof(ScenarioScript_RFH003),
			(ScenarioScript ss) =>
			{
				InvokeTick<ScenarioScriptFrontend_RFH003>(ss);
			}
		},
		{
			typeof(CardScript_Geminate0181),
			(ScenarioScript ss) =>
			{
				InvokeTick<ScenarioScriptFrontend_Geminate0181>(ss);
			}
		},
		{
			typeof(CardScript_Geminate0182),
			(ScenarioScript ss) =>
			{
				InvokeTick<ScenarioScriptFrontend_Geminate0182>(ss);
			}
		}
	};

	public static Dictionary<Type, Type> scriptsOnTileGen = new Dictionary<Type, Type>
	{
		{
			typeof(ScenarioScript_FH015),
			typeof(ScenarioScriptFrontend_FH015)
		},
		{
			typeof(ScenarioScript_FH016),
			typeof(ScenarioScriptFrontend_FH016)
		},
		{
			typeof(ScenarioScript_FH089),
			typeof(ScenarioScriptFrontend_FH089)
		},
		{
			typeof(ScenarioScript_FH099),
			typeof(ScenarioScriptFrontend_FH099)
		}
	};

	protected ScenarioScript _script;

	public static List<CActor> matchingActors = new List<CActor>();

	private static List<Vector2Int> _tll = new List<Vector2Int>();

	private static bool _tllUpdate;

	protected static List<MeshRenderer> _hiddenRends = new List<MeshRenderer>();

	protected static List<ParticleSystem> _hiddenParticles = new List<ParticleSystem>();

	protected static List<Light> _hiddenLights = new List<Light>();

	protected Transform _envRoot;

	protected Transform _boardRoot;

	private static Dictionary<GameObject, GameObjectHeightInfo> environmentHeightInfo = new Dictionary<GameObject, GameObjectHeightInfo>();

	protected static bool RendHeightsInvalidated = false;

	private static Dictionary<GameObject, float> objectHeights = new Dictionary<GameObject, float>();

	protected static CClientTile[,] _cta => ClientScenarioManager.s_ClientScenarioManager?.ClientTileArray;

	protected static ScenarioState _css => ScenarioManager.CurrentScenarioState;

	public virtual bool _ManualActorHeightControl => false;

	public static bool ManualActorHeightControl
	{
		get
		{
			if (_clientScripts == null)
			{
				return false;
			}
			return _clientScripts.Any((KeyValuePair<ScenarioScript, ScenarioScriptFrontend> x) => x.Value._ManualActorHeightControl);
		}
	}

	public static void UnityUpdate()
	{
		DebugMenuWBUI.UISprite = DebugMenu.Instance?.QUISprite;
		if (_css != null && !LevelEditorController.s_Instance.IsEditing && EnvironmentLoader.IsInstanceValid && EnvironmentLoader.Instance.IsLoadingComplete)
		{
			IMMGO.Begin();
			DebugMenuWBUI.Begin();
			foreach (ScenarioScript scenarioScript in _css.ScenarioScripts)
			{
				if (scriptsTick.TryGetValue(scenarioScript.GetType(), out var value))
				{
					value(scenarioScript);
				}
			}
			DebugMenuWBUI.End();
			IMMGO.End();
		}
		else if (_clientScripts != null && _clientScripts.Count > 0)
		{
			DeinitilizeFrontendScripts();
		}
	}

	public static void DeinitilizeFrontendScripts()
	{
		foreach (KeyValuePair<ScenarioScript, ScenarioScriptFrontend> clientScript in _clientScripts)
		{
			clientScript.Value?.Deinitialize();
		}
		_clientScripts.Clear();
		_tickSSF = null;
	}

	public static void OnMapTileGenBeforeLights(ProceduralMapTile mapTile)
	{
		if (!Application.isPlaying)
		{
			return;
		}
		foreach (ScenarioScript scenarioScript in ScenarioManager.CurrentScenarioState.ScenarioScripts)
		{
			Type type = scenarioScript.GetType();
			if (scriptsOnTileGen.TryGetValue(type, out var value))
			{
				InvokeMapTileGenBeforeLights(scenarioScript, value, mapTile);
			}
		}
	}

	public static void OnFastLoad()
	{
		foreach (KeyValuePair<ScenarioScript, ScenarioScriptFrontend> clientScript in _clientScripts)
		{
			clientScript.Value?.OnFastLoadInternal();
		}
	}

	protected virtual void OnFastLoadInternal()
	{
	}

	private static void InvokeMapTileGenBeforeLights(ScenarioScript ss, Type frontEndScript, ProceduralMapTile mapTile)
	{
		if (_css == null || _cta == null || Camera.main == null)
		{
			return;
		}
		MethodInfo[] methods = frontEndScript.GetMethods();
		foreach (MethodInfo methodInfo in methods)
		{
			if (methodInfo != null && methodInfo.HasAttribute<MapTileGenBeforeLightsAttribute>())
			{
				methodInfo.Invoke(null, new object[1] { mapTile });
			}
		}
	}

	public static void InvokeTick<T>(ScenarioScript ss) where T : ScenarioScriptFrontend, new()
	{
		if (_css == null || _cta == null || Camera.main == null)
		{
			DebugMenuWBUI.Done();
			DeinitilizeFrontendScripts();
			return;
		}
		if (!_clientScripts.TryGetValue(ss, out var value))
		{
			value = new T();
			value._script = ss;
			value.Initialize();
			_clientScripts[ss] = value;
			DebugMenuWBUI.Error = (string s) =>
			{
				ss.Error(s, null, 170);
			};
		}
		_tickSSF = value;
		value.Tick();
		if (_tllUpdate)
		{
			foreach (Vector2Int item in _tll)
			{
				Vector3 tileClientPos = _tickSSF.GetTileClientPos(item.x, item.y);
				QGL.LatePrintWorld(item, tileClientPos);
			}
		}
		if (ShowOrientation_cvar)
		{
			float num = (0f - Camera.main.transform.position.y) / Camera.main.transform.forward.y;
			Vector3 vector = Camera.main.transform.forward * num + Camera.main.transform.position;
			Vector3 vector2 = vector + new Vector3(1f, 0f, 0f);
			Vector3 vector3 = vector + new Vector3(0f, 1f, 0f);
			Vector3 vector4 = vector + new Vector3(0f, 0f, 1f);
			QGL.WorldToScreenPos(vector);
			QGL.WorldToScreenPos(vector2);
			QGL.WorldToScreenPos(vector3);
			QGL.WorldToScreenPos(vector4);
			QGL.LateDrawLineWorld(vector, vector2, Color.red);
			QGL.LateDrawLineWorld(vector, vector3, Color.green);
			QGL.LateDrawLineWorld(vector, vector4, Color.blue);
		}
		if (ShowWorldCursor_cvar == 0)
		{
			return;
		}
		Vector2Int vector2Int = Vector2Int.zero;
		if (GetTileUnderCursor(out var tile))
		{
			vector2Int = new Vector2Int(tile.m_Tile.m_ArrayIndex.X, tile.m_Tile.m_ArrayIndex.Y);
		}
		Vector2 vector5 = Mouse.current.position.ReadValue();
		Ray ray = Camera.main.ScreenPointToRay(vector5);
		Plane plane = new Plane(new Vector3(0f, 1f, 0f), Vector3.zero);
		Vector3 zero = Vector3.zero;
		if (plane.Raycast(ray, out var enter))
		{
			zero = ray.GetPoint(enter);
			if (ShowWorldCursor_cvar == 1)
			{
				Vector3 vector6 = zero;
				string text = vector6.ToString();
				Vector2Int vector2Int2 = vector2Int;
				QGL.LatePrintWorld(text + "\n" + vector2Int2.ToString(), zero);
			}
			else
			{
				Vector3 vector6 = zero;
				string text2 = vector6.ToString();
				Vector2Int vector2Int2 = vector2Int;
				QGL.LatePrint(text2 + "\n" + vector2Int2.ToString(), Screen.width - 100, 20f);
			}
		}
	}

	private void Deinitialize()
	{
		DeinitializeInternal();
		deinitialized = true;
		initialized = false;
		_hiddenRends.Clear();
		_hiddenLights.Clear();
		_hiddenParticles.Clear();
	}

	protected virtual void DeinitializeInternal()
	{
		UIEventManager.OnEventAdded -= OnEventAdded;
		environmentHeightInfo.Clear();
		objectHeights.Clear();
		RendHeightsInvalidated = false;
	}

	protected virtual void InitializeInternal()
	{
		CacheRendHeights();
		UIEventManager.OnEventAdded += OnEventAdded;
	}

	private void Initialize()
	{
		deinitialized = false;
		_script.Trace($"Initialized frontend script {GetType()}", null, 252);
		if (SkipScenarioBackground_cvar)
		{
			ProceduralBackground proceduralBackground = UnityEngine.Object.FindObjectOfType<ProceduralBackground>();
			if ((bool)proceduralBackground)
			{
				proceduralBackground.gameObject.SetActive(value: false);
				_script.Trace("Skipping background.", null, 258);
			}
		}
		if (!_envRoot)
		{
			Component component = UnityEngine.Object.FindObjectOfType<ProceduralScenario>();
			if (!component)
			{
				component = UnityEngine.Object.FindObjectOfType<SeasonTechRuntime>();
				if ((bool)component)
				{
					component = component.transform.parent;
				}
			}
			if (!component)
			{
				_script.Error("No environment root.", null, 272);
				return;
			}
			_envRoot = component.transform;
		}
		if (!_boardRoot)
		{
			_boardRoot = GameObject.Find("Board").transform;
		}
	}

	private void Tick()
	{
		if (deinitialized)
		{
			return;
		}
		if (!initialized)
		{
			if (EnvironmentLoader.Instance == null || (from a in EnvironmentLoader.Instance.GetMaps()
				select a.GetComponent<ProceduralMapTile>()).ToArray().Any((ProceduralMapTile x) => !x.HasSetInitialVisibility))
			{
				return;
			}
			initialized = true;
			InitializeInternal();
		}
		TickInternal();
	}

	public static Vector3 GetCTPos(TileIndex ti)
	{
		return GetCTPos(ti.X, ti.Y);
	}

	public static Vector3 GetCTPos(int x, int y, out CClientTile ct)
	{
		if (_cta == null)
		{
			ct = null;
			return Vector3.zero;
		}
		ct = _cta[x, y];
		return GetCTPos(ct);
	}

	public static bool GetCT(TileIndex ti, out CClientTile ct)
	{
		return GetCT(ti.X, ti.Y, out ct);
	}

	public static bool GetCT(Point arrayIndex, out CClientTile ct)
	{
		return GetCT(arrayIndex.X, arrayIndex.Y, out ct);
	}

	public static bool GetCT(int x, int y, out CClientTile ct)
	{
		if (_cta == null)
		{
			ct = null;
			return false;
		}
		ct = _cta[x, y];
		return true;
	}

	public static Vector3 GetCTPos(int x, int y)
	{
		CClientTile ct;
		return GetCTPos(x, y, out ct);
	}

	public static Vector3 GetCTPos(CClientTile ct)
	{
		if (ct == null || ct.m_GameObject == null)
		{
			return Vector3.zero;
		}
		return ct.m_GameObject.transform.position;
	}

	public static bool GetTileUnderCursor(out CClientTile tile)
	{
		tile = null;
		if (Singleton<WorldspacePicker>.InstanceFast == null)
		{
			return false;
		}
		tile = MF.FindInteractableAtMousePosition(ignoreinteractableviaguiflag: false, Singleton<WorldspacePicker>.Instance.m_HexSelectionRaycastLayer)?.GetComponent<TileBehaviour>()?.m_ClientTile;
		return tile != null;
	}

	public static bool GetHoveredActor(out CActor actor)
	{
		actor = null;
		if (GetTileUnderCursor(out var tile))
		{
			actor = ScenarioManager.Scenario.FindActorAt(tile.m_Tile.m_ArrayIndex);
		}
		if (actor == null || actor.IsDead)
		{
			actor = null;
			return false;
		}
		return true;
	}

	public static bool GetMatchingOrHoveredActor(string[] argv, out CActor actor, int max = 2, int guid = 1)
	{
		actor = null;
		if (argv == null || argv.Length < max)
		{
			return GetHoveredActor(out actor);
		}
		if (guid > 0)
		{
			return GetFirstMatchingActor(argv[guid], out actor);
		}
		return false;
	}

	public static bool GetFirstMatchingActor(string partialNameOrGUID, out CActor result)
	{
		if (GetMatchingActors(partialNameOrGUID))
		{
			result = matchingActors[0];
			return true;
		}
		result = null;
		return false;
	}

	public static bool GetMatchingActors(string partialNameOrGUID)
	{
		matchingActors.Clear();
		string value = partialNameOrGUID.ToLowerInvariant();
		foreach (CActor allActor in ScenarioManager.Scenario.AllActors)
		{
			if ((allActor.Class.ID + " " + allActor.ActorGuid).ToLowerInvariant().Contains(value))
			{
				matchingActors.Add(allActor);
			}
			if (allActor is CObjectActor { IsAttachedToProp: not false, AttachedProp: var attachedProp } && (attachedProp.PrefabName + " " + attachedProp.PropGuid).ToLowerInvariant().Contains(value))
			{
				matchingActors.Add(allActor);
			}
		}
		return matchingActors.Count > 0;
	}

	public static void KillActor(CActor actor)
	{
		actor.Health = 0;
		GameState.KillActor(actor, actor, CActor.ECauseOfDeath.Damage, out var _);
		if (actor.Type == CActor.EType.Player)
		{
			(actor.Class as CCharacterClass).CheckForFinishedActiveBonuses(actor);
		}
		else if (actor.Type == CActor.EType.HeroSummon)
		{
			(actor as CHeroSummonActor).Summoner.CharacterClass.CheckForFinishedActiveBonuses((actor as CHeroSummonActor).Summoner);
		}
	}

	[ConsoleCommand(Command = "backend_kill")]
	public static void BackendKill_kmd(IConsole console, string guid)
	{
		if (GetFirstMatchingActor(guid, out var result))
		{
			KillActor(result);
		}
	}

	public static void BackendKill_kmd(string[] argv)
	{
		if (argv.Length >= 2 && GetFirstMatchingActor(argv[1], out var result))
		{
			KillActor(result);
		}
	}

	public static void TileListSelect_kmd(string[] argv)
	{
		if (!_tllUpdate)
		{
			_tllUpdate = true;
			_tll.Clear();
		}
		if (GetTileUnderCursor(out var tile))
		{
			Vector2Int item = new Vector2Int(tile.m_Tile.m_ArrayIndex.X, tile.m_Tile.m_ArrayIndex.Y);
			if (_tll.Contains(item))
			{
				_tll.Remove(item);
			}
			else
			{
				_tll.Add(item);
			}
			string text = "\n";
			foreach (Vector2Int item2 in _tll)
			{
				text = text + item2.x + ", " + item2.y + ",\n";
			}
			Trace(text);
			GUIUtility.systemCopyBuffer = text;
		}
		else
		{
			Trace("Pick a valid tile.");
		}
	}

	public static void TileListDump_kmd(string[] argv)
	{
		string text = "\n";
		foreach (Vector2Int item in _tll)
		{
			text = text + item.x + ", " + item.y + ",\n";
		}
		Trace(text);
		GUIUtility.systemCopyBuffer = text;
		_tllUpdate = false;
	}

	protected virtual void TickInternal()
	{
		if (RendHeightsInvalidated)
		{
			CacheRendHeights();
		}
		UpdateHeights();
	}

	protected virtual bool UpdatePlanes(bool instant = false)
	{
		return false;
	}

	protected virtual bool UpdateHeights()
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			bool result = UpdatePlanes();
			try
			{
				UpdateMovingObjects();
			}
			catch (Exception)
			{
			}
			return result;
		}
	}

	public static void ShowAbilityHelpBoxTooltip(CAbility ability)
	{
		ScenarioScript current = ScenarioScript.current;
		if (current != null && _clientScripts != null && _clientScripts.TryGetValue(current, out var value))
		{
			value.ShowAbilityHelpBoxTooltipInternal(ability);
		}
	}

	protected virtual void ShowAbilityHelpBoxTooltipInternal(CAbility ability)
	{
	}

	protected Vector3 GetTileClientPos(Point p)
	{
		return GetTileClientPos(p.X, p.Y);
	}

	protected Vector3 GetTileClientPos(TileIndex ti)
	{
		return GetTileClientPos(ti.X, ti.Y);
	}

	protected Vector3 GetTileClientPos(int x, int y)
	{
		if (_cta == null)
		{
			return Vector3.zero;
		}
		x = Mathf.Clamp(x, 0, _cta.GetLength(0) - 1);
		y = Mathf.Clamp(y, 0, _cta.GetLength(1) - 1);
		return GetCTPos(_cta[x, y]);
	}

	protected void BackendCmd(string cmd)
	{
		_script.Trace("Executing backend cmd " + cmd, null, 538);
		ScenarioRuleClient.ExecuteSRLCommand(cmd, _script, null, "CHEAT_" + cmd);
	}

	protected void HideVisualObjects(Bounds bounds, Func<bool> filter = null, bool includeInactive = false)
	{
		List<Bounds> bounds2 = new List<Bounds> { bounds };
		if (GatherRenderers(bounds2, _hiddenRends, filter, includeInactive))
		{
			foreach (MeshRenderer hiddenRend in _hiddenRends)
			{
				if (!(hiddenRend == null) && !(hiddenRend.gameObject == null))
				{
					hiddenRend.gameObject.SetActive(value: false);
					if (TraceRendererVisiblity_cvar)
					{
						_script.Trace("Hidden renderer on " + hiddenRend.gameObject.name, hiddenRend.gameObject, 559);
					}
				}
			}
		}
		if (GatherParticles(bounds2, _hiddenParticles, filter, includeInactive))
		{
			foreach (ParticleSystem hiddenParticle in _hiddenParticles)
			{
				if (!(hiddenParticle == null) && !(hiddenParticle.gameObject == null))
				{
					hiddenParticle.gameObject.SetActive(value: false);
					if (TraceRendererVisiblity_cvar)
					{
						_script.Trace("Hidden particle on " + hiddenParticle.gameObject.name, hiddenParticle.gameObject, 572);
					}
				}
			}
		}
		if (!GatherLights(bounds2, _hiddenLights, filter, includeInactive))
		{
			return;
		}
		foreach (Light hiddenLight in _hiddenLights)
		{
			if (!(hiddenLight == null) && !(hiddenLight.gameObject == null))
			{
				hiddenLight.gameObject.SetActive(value: false);
				if (TraceRendererVisiblity_cvar)
				{
					_script.Trace("Hidden light on " + hiddenLight.gameObject.name, hiddenLight.gameObject, 585);
				}
			}
		}
	}

	protected List<T> GetComponentsInRoots<T>(bool includeInactive)
	{
		if (!_envRoot)
		{
			return null;
		}
		List<T> list = new List<T>(_envRoot.GetComponentsInChildren<T>(includeInactive: false));
		if ((bool)_boardRoot)
		{
			list.AddRange(_boardRoot.GetComponentsInChildren<T>(includeInactive: false));
		}
		return list;
	}

	protected bool GatherRenderers(List<Bounds> bounds, List<MeshRenderer> ioResult, Func<bool> filter = null, bool includeInactive = false)
	{
		if (!_envRoot)
		{
			return false;
		}
		foreach (MeshRenderer componentsInRoot in GetComponentsInRoots<MeshRenderer>(includeInactive))
		{
			if (componentsInRoot == null || componentsInRoot.gameObject == null)
			{
				continue;
			}
			if (bounds != null)
			{
				bool flag = false;
				foreach (Bounds bound in bounds)
				{
					if (bound != default(Bounds) && componentsInRoot.bounds.Intersects(bound))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					continue;
				}
			}
			if ((bool)componentsInRoot.GetComponentInParent<TilesOcclusionVolume>())
			{
				continue;
			}
			if (filter == null)
			{
				if ((bool)componentsInRoot.GetComponentInParent<ProceduralProp>())
				{
					continue;
				}
			}
			else if (!filter())
			{
				continue;
			}
			if (!componentsInRoot.name.ToLowerInvariant().Contains("vfx") && ((bool)componentsInRoot.GetComponentInParent<ProceduralWall>() || (bool)componentsInRoot.GetComponentInParent<ProceduralMapTile>() || (bool)componentsInRoot.GetComponentInParent<HexSelect_Control>()))
			{
				ioResult.Add(componentsInRoot);
			}
		}
		return true;
	}

	protected bool GatherParticles(List<Bounds> bounds, List<ParticleSystem> ioResult, Func<bool> filter = null, bool includeInactive = false)
	{
		if (!_envRoot)
		{
			return false;
		}
		foreach (ParticleSystem componentsInRoot in GetComponentsInRoots<ParticleSystem>(includeInactive))
		{
			if (bounds != null)
			{
				bool flag = false;
				foreach (Bounds bound in bounds)
				{
					if (bound != default(Bounds) && bound.Contains(componentsInRoot.transform.position))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					continue;
				}
			}
			if ((bool)componentsInRoot.GetComponentInParent<TilesOcclusionVolume>())
			{
				continue;
			}
			if (filter == null)
			{
				if ((bool)componentsInRoot.GetComponentInParent<ProceduralProp>())
				{
					continue;
				}
			}
			else if (!filter())
			{
				continue;
			}
			if ((bool)componentsInRoot.GetComponentInParent<ProceduralWall>() || (bool)componentsInRoot.GetComponentInParent<ProceduralMapTile>())
			{
				ioResult.Add(componentsInRoot);
			}
		}
		return true;
	}

	protected bool GatherLights(List<Bounds> bounds, List<Light> ioResult, Func<bool> filter = null, bool includeInactive = false)
	{
		if (!_envRoot)
		{
			return false;
		}
		foreach (Light componentsInRoot in GetComponentsInRoots<Light>(includeInactive))
		{
			if (bounds != null)
			{
				bool flag = false;
				foreach (Bounds bound in bounds)
				{
					if (bound != default(Bounds) && bound.Contains(componentsInRoot.transform.position))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					continue;
				}
			}
			if ((bool)componentsInRoot.GetComponentInParent<TilesOcclusionVolume>())
			{
				continue;
			}
			if (filter == null)
			{
				if ((bool)componentsInRoot.GetComponentInParent<ProceduralProp>())
				{
					continue;
				}
			}
			else if (!filter())
			{
				continue;
			}
			if ((bool)componentsInRoot.GetComponentInParent<ProceduralWall>() || (bool)componentsInRoot.GetComponentInParent<ProceduralMapTile>())
			{
				ioResult.Add(componentsInRoot);
			}
		}
		return true;
	}

	protected void UnhideVisualObjects(Bounds bounds)
	{
		foreach (MeshRenderer hiddenRend in _hiddenRends)
		{
			if ((bool)hiddenRend && hiddenRend.bounds.Intersects(bounds))
			{
				hiddenRend.gameObject.SetActive(value: true);
				if (TraceRendererVisiblity_cvar)
				{
					_script.Trace("Unhidden renderer on " + hiddenRend.gameObject.name, hiddenRend.gameObject, 742);
				}
			}
		}
		_hiddenRends.RemoveWhere((MeshRenderer hr) => !hr || hr.bounds.Intersects(bounds));
		foreach (ParticleSystem hiddenParticle in _hiddenParticles)
		{
			if ((bool)hiddenParticle && bounds.Contains(hiddenParticle.transform.position))
			{
				hiddenParticle.gameObject.SetActive(value: true);
				if (TraceRendererVisiblity_cvar)
				{
					_script.Trace("Unhidden particle on " + hiddenParticle.gameObject.name, hiddenParticle.gameObject, 752);
				}
			}
		}
		_hiddenParticles.RemoveWhere((ParticleSystem hp) => !hp || bounds.Contains(hp.transform.position));
		foreach (Light hiddenLight in _hiddenLights)
		{
			if ((bool)hiddenLight && bounds.Contains(hiddenLight.transform.position))
			{
				hiddenLight.gameObject.SetActive(value: true);
				if (TraceRendererVisiblity_cvar)
				{
					_script.Trace("Unhidden light on " + hiddenLight.gameObject.name, hiddenLight.gameObject, 762);
				}
			}
		}
		_hiddenLights.RemoveWhere((Light hr) => !hr || bounds.Contains(hr.transform.position));
	}

	protected void DrawBox(Bounds bounds)
	{
		DrawBox(bounds.min, bounds.max);
	}

	protected void DrawBox(Vector3 v0, Vector3 v1)
	{
		Vector3[] array = new Vector3[4]
		{
			new Vector3(v0.x, v0.y, v0.z),
			new Vector3(v1.x, v0.y, v0.z),
			new Vector3(v1.x, v0.y, v1.z),
			new Vector3(v0.x, v0.y, v1.z)
		};
		Vector3[] array2 = new Vector3[4]
		{
			new Vector3(v0.x, v1.y, v0.z),
			new Vector3(v1.x, v1.y, v0.z),
			new Vector3(v1.x, v1.y, v1.z),
			new Vector3(v0.x, v1.y, v1.z)
		};
		QGL.LateDrawLineLoopWorld(array);
		QGL.LateDrawLineLoopWorld(array2);
		QGL.LateDrawLineWorld(array[0], array2[0]);
		QGL.LateDrawLineWorld(array[1], array2[1]);
		QGL.LateDrawLineWorld(array[2], array2[2]);
		QGL.LateDrawLineWorld(array[3], array2[3]);
	}

	protected static void Trace(object o)
	{
		if (_tickSSF != null)
		{
			_tickSSF?._script.Trace(o, null, 798);
		}
		else
		{
			Log.SimpleInfo((o == null) ? "null" : o.ToString());
		}
	}

	protected static void Error(object o)
	{
		if (_tickSSF != null)
		{
			_tickSSF?._script.Error(o, null, 810);
		}
		else
		{
			Log.GameplayError((o == null) ? "null" : o.ToString());
		}
	}

	[ConsoleCommand(Command = "fh079_move")]
	public static void Move(IConsole console)
	{
		TileBehaviour tileBehaviourUnderCursor = DebugMenu.GetTileBehaviourUnderCursor();
		if (!(tileBehaviourUnderCursor == null))
		{
			ScenarioScript_FH079._script.MoveSled(tileBehaviourUnderCursor.m_ClientTile.m_Tile.m_ArrayIndex);
		}
	}

	protected void CalcMinMaxForCustomRect(IEnumerable<RectTransform> rects, out Vector3 finalMin, out Vector3 finalMax)
	{
		finalMin = new Vector3(100000f, 100000f, 10000f);
		finalMax = new Vector3(-100000f, -100000f, 10000f);
		foreach (RectTransform rect in rects)
		{
			if (rect.gameObject.activeInHierarchy)
			{
				Vector3[] array = new Vector3[4];
				rect.GetWorldCorners(array);
				Vector3 vector = array[0];
				Vector3 vector2 = array[2];
				if (vector.x < finalMin.x)
				{
					finalMin.x = vector.x;
				}
				if (vector.y < finalMin.y)
				{
					finalMin.y = vector.y;
				}
				if (vector2.x > finalMax.x)
				{
					finalMax.x = vector2.x;
				}
				if (vector2.y > finalMax.y)
				{
					finalMax.y = vector2.y;
				}
			}
		}
	}

	protected void ShowCustomHighlight(Vector3 min, Vector3 max, Transform parent, string key, string highlightPrefab = null, Vector4 padding = default(Vector4))
	{
		if (!customRectsForHighlights.TryGetValue(key, out var value))
		{
			value = new GameObject("CustomHighlightRectToFollow").AddComponent<RectTransform>();
			InteractabilityHighlightCanvas.s_Instance?.TryAddHighlightForRectTransform(value, padding, highlightPrefab);
			customRectsForHighlights[key] = value;
		}
		value.SetParent(parent);
		value.position = (min + max) / 2f;
		value.sizeDelta = max - min;
	}

	protected void ClearCustomRects()
	{
		if (customRectsForHighlights == null)
		{
			return;
		}
		foreach (RectTransform value in customRectsForHighlights.Values)
		{
			InteractabilityHighlightCanvas.s_Instance?.TryRemoveHighlightForRectTransform(value);
			UnityEngine.Object.Destroy(value.gameObject);
		}
		customRectsForHighlights.Clear();
	}

	protected bool GetBoundsFromClientTiles(int x0, int y0, int x1, int y1, out Bounds bounds)
	{
		bounds = default;
		CClientTile cClientTile = _cta[x0, y0];
		CClientTile cClientTile2 = _cta[x1, y1];
		if (cClientTile?.m_GameObject == null)
		{
			_script.Trace($"Hide Platform: Not a valid tile {x0} {y0}", null, 905);
			return false;
		}
		if (cClientTile2?.m_GameObject == null)
		{
			_script.Trace($"Hide Platform: Not a valid tile {x1} {y1}", null, 910);
			return false;
		}
		Vector3 position = cClientTile.m_GameObject.transform.position;
		Vector3 vector = cClientTile2.m_GameObject.transform.position + new Vector3(0f, 5f, 0f);
		bounds.min = new Vector3(Mathf.Min(position.x, vector.x), Mathf.Min(position.y, vector.y), Mathf.Min(position.z, vector.z));
		bounds.max = new Vector3(Mathf.Max(position.x, vector.x), Mathf.Max(position.y, vector.y), Mathf.Max(position.z, vector.z));
		return true;
	}

	protected bool UpdateHeightOfPlane(ref Vector3 pos, ref Plane plane, float t, float maxBoundHeight, float minBoundHeight, bool instant = false)
	{
		float num = Mathf.LerpUnclamped(maxBoundHeight, minBoundHeight, t);
		float y = pos.y;
		pos.y = (instant ? num : Mathf.MoveTowards(pos.y, num, Time.deltaTime));
		plane = new Plane(Vector3.up, pos);
		return Math.Abs(num - y) >= 0.0001f;
	}

	protected bool UpdateHeightOfPlane(ref Plane plane, float t, float maxBoundHeight, float minBoundHeight)
	{
		Vector3 pos = Vector3.zero;
		return UpdateHeightOfPlane(ref pos, ref plane, t, maxBoundHeight, minBoundHeight, instant: true);
	}

	protected virtual bool GetPlatformID(CTile tile, object logicObject, out int plane)
	{
		plane = 0;
		return false;
	}

	protected virtual bool GetPlatformID(GameObjectHeightInfo info, out int plane)
	{
		plane = info.platform;
		return false;
	}

	protected virtual bool GetPlane(int platformID, out Plane plane)
	{
		plane = default;
		return false;
	}

	protected virtual bool GetPlaneHeights(int platformID, out float minHeight, out float maxHeight)
	{
		minHeight = (maxHeight = 0f);
		return false;
	}

	protected virtual bool UpdateActorHeightsInstantly(ActorBehaviour beh)
	{
		return true;
	}

	private void UpdateMovingObjects()
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			foreach (KeyValuePair<GameObject, GameObjectHeightInfo> item in environmentHeightInfo.ToList())
			{
				if (item.Key == null)
				{
					environmentHeightInfo.Remove(item.Key);
				}
				else if (item.Value.dirty)
				{
					item.Value.dirty = false;
					UpdateHeight(item.Key, cache: false, item.Value.platform, instant: true, addFromCachedHeights: true);
				}
			}
			CClientTile[,] clientTileArray = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray;
			foreach (CClientTile cClientTile in clientTileArray)
			{
				if (cClientTile?.m_Tile?.m_HexMap != null && cClientTile.m_Tile.m_HexMap.Revealed)
				{
					_ = cClientTile.m_Tile.m_ArrayIndex;
					GetPlatformID(cClientTile.m_Tile, null, out var plane);
					UpdateHeight(cClientTile.m_GameObject, cache: false, plane, instant: true);
				}
			}
			foreach (CActor item2 in ScenarioManager.Scenario.AllActorsThreadSafe)
			{
				_ = item2.ArrayIndex;
				GameObject gameObject = Choreographer.s_Choreographer.FindClientActorGameObject(item2);
				if (gameObject == null)
				{
					continue;
				}
				ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(gameObject);
				if (actorBehaviour.IsTeleporting)
				{
					if (!actorBehaviour.IsTeleportFinishing)
					{
						continue;
					}
				}
				else if (actorBehaviour.IsJumping)
				{
					continue;
				}
				GetPlatformID(item2.Tile, item2, out var plane2);
				UpdateHeight(gameObject, cache: true, plane2, UpdateActorHeightsInstantly(actorBehaviour));
			}
			foreach (CObjectProp item3 in ScenarioManager.CurrentScenarioState.GetPropsThreadSafe)
			{
				item3.ArrayIndex.ToPoint();
				GameObject gameObject2 = Choreographer.FindPropGameObject(item3);
				if (!(gameObject2 == null))
				{
					GetPlatformID(item3.PropTile, item3, out var plane3);
					UpdateHeight(gameObject2, cache: false, plane3, instant: true);
				}
			}
			foreach (CSpawner spawner in ScenarioManager.CurrentScenarioState.Spawners)
			{
				CObjectProp prop = spawner.Prop;
				spawner.ArrayIndex.ToPoint();
				GameObject gameObject3 = Choreographer.FindPropGameObject(prop);
				if (!(gameObject3 == null))
				{
					GetPlatformID(ScenarioManager.GetTile(spawner.ArrayIndex), spawner, out var plane4);
					UpdateHeight(gameObject3, cache: false, plane4, instant: true);
				}
			}
			foreach (var allActiveStarsAndTile in WorldspaceStarHexDisplay.Instance.GetAllActiveStarsAndTiles())
			{
				if (allActiveStarsAndTile.Item2 != null)
				{
					_ = allActiveStarsAndTile.Item1.m_Tile.m_ArrayIndex;
					GetPlatformID(allActiveStarsAndTile.Item1.m_Tile, null, out var plane5);
					UpdateHeight(allActiveStarsAndTile.Item2.GameObject, cache: false, plane5, instant: true);
				}
			}
		}
	}

	private void UpdateHeight(GameObject go, bool cache, int platformID, bool instant = false, bool addFromCachedHeights = false)
	{
		Vector3 position = go.transform.position;
		float a = position.y;
		if (cache && objectHeights.TryGetValue(go, out var value))
		{
			a = value;
		}
		float num = position.y;
		GetPlane(platformID, out var plane);
		Vector3 origin = new Vector3(position.x, plane.distance + 100f, position.z);
		Ray ray = new Ray(origin, Vector3.down);
		if (plane.Raycast(ray, out var enter))
		{
			num = ray.GetPoint(enter).y;
		}
		if (GetPlaneHeights(platformID, out var minHeight, out var maxHeight))
		{
			num = Mathf.Clamp(num, minHeight, maxHeight);
		}
		if (addFromCachedHeights)
		{
			GameObjectHeightInfo gameObjectHeightInfo = environmentHeightInfo[go];
			num += gameObjectHeightInfo.originalHeight;
		}
		float value2 = (position.y = (instant ? num : Mathf.Lerp(a, num, Time.deltaTime)));
		go.transform.position = position;
		if (cache)
		{
			objectHeights[go] = value2;
		}
	}

	protected virtual void OnEventAdded(UIEvent uiEvent)
	{
		if (uiEvent.EventType == UIEvent.EUIEventType.RoomRevealed)
		{
			CacheRendHeights();
		}
	}

	protected void ToggleVisibiliy(int platform, bool visible)
	{
		foreach (KeyValuePair<GameObject, GameObjectHeightInfo> item in environmentHeightInfo.Where((KeyValuePair<GameObject, GameObjectHeightInfo> x) => x.Value.platform == platform).ToList())
		{
			if (item.Key == null)
			{
				environmentHeightInfo.Remove(item.Key);
			}
			else
			{
				item.Key.SetActive(visible);
			}
		}
	}

	protected virtual void CacheRendHeights()
	{
		RendHeightsInvalidated = false;
	}

	protected void SetPlatformHeightsDirty(params int[] platforms)
	{
		foreach (KeyValuePair<GameObject, GameObjectHeightInfo> item in environmentHeightInfo)
		{
			if (platforms.Contains(item.Value.platform))
			{
				item.Value.dirty = true;
			}
		}
	}

	protected void CacheTilesForPlatform(int platform, List<CTile> tiles, bool visible)
	{
		List<GameObject> list = new List<GameObject>();
		foreach (CTile tile2 in tiles)
		{
			string tileGuid = tile2.m_Hex.TileGuid;
			GameObject tile = EnvironmentLoader.Instance.GetTile(tileGuid);
			if (!(tile == null))
			{
				list.Add(tile);
			}
		}
		CacheRendHeightsForPlatform(platform, list, visible);
	}

	protected void CacheWallsForPlatform(int platform, string roomGuid, bool visible, List<string> objectNames = null)
	{
		GameObject map = EnvironmentLoader.Instance.GetMap(roomGuid);
		if (map == null)
		{
			return;
		}
		List<GameObject> list = new List<GameObject>();
		foreach (Transform item in map.transform.Find("Walls"))
		{
			if (objectNames != null)
			{
				bool flag = false;
				foreach (string objectName in objectNames)
				{
					if (item.name.Contains(objectName, StringComparison.InvariantCultureIgnoreCase))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					continue;
				}
			}
			if (item.name.Contains("Wall", StringComparison.InvariantCultureIgnoreCase))
			{
				list.Add(item.gameObject);
			}
		}
		CacheRendHeightsForPlatform(platform, list, visible);
	}

	protected void CacheObjectsInRoomForPlatform(int platform, string roomGuid, List<string> objectNames, bool visible, List<Bounds> bounds = null)
	{
		GameObject map = EnvironmentLoader.Instance.GetMap(roomGuid);
		if (map == null)
		{
			return;
		}
		Transform transform = map.transform.Find("Generated Content/Full");
		if (transform == null)
		{
			return;
		}
		List<GameObject> list = new List<GameObject>();
		foreach (Transform item in transform)
		{
			if (bounds != null)
			{
				bool flag = false;
				foreach (Bounds bound in bounds)
				{
					if (bound.Contains(item.position))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					continue;
				}
			}
			foreach (string objectName in objectNames)
			{
				if (item.name.Contains(objectName, StringComparison.InvariantCultureIgnoreCase))
				{
					list.Add(item.gameObject);
					break;
				}
			}
		}
		CacheRendHeightsForPlatform(platform, list, visible);
	}

	protected void CacheReflectionProbesInRoomForPlatform(int platform, string roomGuid, bool visible, List<Bounds> bounds = null)
	{
		GameObject map = EnvironmentLoader.Instance.GetMap(roomGuid);
		if (map == null)
		{
			return;
		}
		ReflectionProbe[] componentsInChildren = map.GetComponentsInChildren<ReflectionProbe>();
		if (componentsInChildren == null)
		{
			return;
		}
		List<GameObject> list = new List<GameObject>();
		ReflectionProbe[] array = componentsInChildren;
		foreach (ReflectionProbe reflectionProbe in array)
		{
			if (bounds != null)
			{
				bool flag = false;
				foreach (Bounds bound in bounds)
				{
					if (bound.Contains(reflectionProbe.transform.position))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					continue;
				}
			}
			list.Add(reflectionProbe.gameObject);
		}
		CacheRendHeightsForPlatform(platform, list, visible);
	}

	protected void CacheRendHeightsForPlatform(int platform, List<Bounds> bounds, string roomGuid, bool visible)
	{
		List<MeshRenderer> list = new List<MeshRenderer>();
		List<ParticleSystem> list2 = new List<ParticleSystem>();
		List<Light> list3 = new List<Light>();
		if (!GatherRenderers(bounds, list) || !GatherParticles(bounds, list2) || !GatherLights(bounds, list3))
		{
			return;
		}
		list.RemoveAll((MeshRenderer rend) =>
		{
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			Transform transform = rend.transform;
			do
			{
				if (transform.name == "Board")
				{
					flag = true;
					break;
				}
				if (transform.name == "Generated Content" || transform.name == "Walls")
				{
					flag2 = true;
				}
				if (string.IsNullOrEmpty(roomGuid) || transform.name == roomGuid)
				{
					flag3 = true;
				}
				transform = transform.parent;
			}
			while (transform != null);
			return flag || !flag3 || !flag2;
		});
		list3.RemoveAll((Light light) =>
		{
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			Transform transform = light.transform;
			do
			{
				if (transform.name == "Board")
				{
					flag = true;
					break;
				}
				if (transform.name == "Generated Content" || transform.name == "Walls")
				{
					flag2 = true;
				}
				if (string.IsNullOrEmpty(roomGuid) || transform.name == roomGuid)
				{
					flag3 = true;
				}
				transform = transform.parent;
			}
			while (transform != null);
			return flag || !flag3 || !flag2;
		});
		list2.RemoveAll((ParticleSystem p) =>
		{
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			Transform transform = p.transform;
			do
			{
				if (transform.name == "Board")
				{
					flag = true;
					break;
				}
				if (transform.name == "Generated Content" || transform.name == "Walls")
				{
					flag2 = true;
				}
				if (string.IsNullOrEmpty(roomGuid) || transform.name == roomGuid)
				{
					flag3 = true;
				}
				transform = transform.parent;
			}
			while (transform != null);
			return flag || !flag3 || !flag2;
		});
		CacheRendHeightsForPlatform(platform, list2.Select((ParticleSystem x) => x.gameObject).Concat(list.Select((MeshRenderer x) => x.gameObject)).Concat(list3.Select((Light x) => x.gameObject))
			.ToList(), visible);
	}

	protected void CacheRendHeightsForPlatform(int platform, List<GameObject> gameObjects, bool visible)
	{
		foreach (GameObject gameObject in gameObjects)
		{
			if (!(gameObject == null) && !environmentHeightInfo.TryGetValue(gameObject.gameObject, out var value))
			{
				value = new GameObjectHeightInfo
				{
					go = gameObject.gameObject,
					originalHeight = gameObject.transform.position.y
				};
				environmentHeightInfo[gameObject.gameObject] = value;
				value.platform = platform;
				value.dirty = true;
				value.go.SetActive(visible);
			}
		}
	}
}
