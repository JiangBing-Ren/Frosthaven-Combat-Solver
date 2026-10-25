using System;
using System.Collections.Generic;
using System.Linq;
using FH.ApparanceBaking;
using FH.LevelComponents.ProcGen;
using FH.LevelComponents.TopDownProjection;
using MeshSlicing;
using SETUtil.Extend;
using ScenarioRuleLibrary;
using UnityEngine;

internal class ScenarioScriptFrontend_FH047 : ScenarioScriptFrontend
{
	private const float fadeOutDuration = 1f;

	private const float fadeInDuration = 1f;

	private static bool ShowBounds_cvar;

	private static float _fadeStartTime;

	private static Action _fade;

	private static Bounds _bounds0 = default;

	private static Bounds _bounds1 = default;

	private static ProceduralMistSurrounding _proceduralMist;

	private static SurroundingOverrideFH047 _mistOverride;

	private static (bool hidden0, bool hidden1) _lastHiddenState;

	private static bool _forceMistRegeneration = true;

	private const string MAP_0 = "FH_04_06:";

	private const string MAP_1 = "N:";

	private static HashSet<GameObject> _raised = new HashSet<GameObject>();

	private static Plane lowGroundPlane;

	private static Plane highGroundPlane;

	private const int lowGroundPlaneID = 0;

	private const int highGroundPlaneID = 1;

	private const int startingRoomPlatformID = 2;

	private const int secondRoomPlatformID = 3;

	private bool _CollapsedDirty;

	private const int numRaise = 6;

	private const float dRaise = 0.3f;

	private ScenarioScript_FH047 _script47 => (ScenarioScript_FH047)_script;

	protected override bool GetPlatformID(CTile tile, object logicObject, out int plane)
	{
		if (_script47.IsOnHiGround(tile))
		{
			if (logicObject is CObjectTerrainVisual)
			{
				if (tile.m_HexMap == _script47._Room1)
				{
					plane = 2;
				}
				else
				{
					plane = 3;
				}
			}
			else
			{
				plane = 1;
			}
		}
		else
		{
			plane = 0;
		}
		return true;
	}

	protected override bool GetPlane(int platformID, out Plane plane)
	{
		switch (platformID)
		{
		case 0:
		case 2:
		case 3:
			plane = lowGroundPlane;
			return true;
		case 1:
			plane = highGroundPlane;
			return true;
		default:
			plane = default;
			return false;
		}
	}

	protected override void DeinitializeInternal()
	{
		base.DeinitializeInternal();
		_raised.Clear();
		_forceMistRegeneration = false;
		_mistOverride = null;
		_proceduralMist = null;
		_bounds0 = default;
		_bounds1 = default;
		_fade = null;
		_lastHiddenState = (hidden0: false, hidden1: false);
	}

	protected override void InitializeInternal()
	{
		base.InitializeInternal();
		_fade = null;
		_script47.OnCollapsedChanged = OnCollapsedChanged;
		_forceMistRegeneration = true;
		_bounds0 = new Bounds(new Vector3(21f, -1f, 0f), new Vector3(18.5f, 20f, 35f));
		_bounds1 = new Bounds(new Vector3(-2f, -1f, 0f), new Vector3(18.5f, 20f, 35f));
		lowGroundPlane = new Plane(Vector3.up, Vector3.zero);
		highGroundPlane = new Plane(Vector3.up, new Vector3(0f, 1.8000001f, 0f));
		CacheRendHeights();
		if (_script47.collapsed)
		{
			Hide0();
		}
		else
		{
			Hide1();
		}
	}

	private void PlayAudioIfSceneReady(string audioId)
	{
		if ((!(Choreographer.s_Choreographer != null) || Choreographer.s_Choreographer.LoadingComplete) && (!(TransitionManager.s_Instance != null) || TransitionManager.s_Instance.TransitionDone))
		{
			AudioController.Play(audioId);
		}
	}

	private void OnCollapsedChanged()
	{
		_CollapsedDirty = true;
	}

	protected override void TickInternal()
	{
		base.TickInternal();
		if (_CollapsedDirty)
		{
			_CollapsedDirty = false;
			if (_script47.collapsed)
			{
				StartFadeOut();
			}
		}
		_fade?.Invoke();
		if (_script47.collapsed)
		{
			TickMidgroundVFX();
		}
		if (ShowBounds_cvar)
		{
			DrawBox(_bounds0);
			QGL.LatePrintWorld("bounds 0", _bounds0.center);
			DrawBox(_bounds1);
			QGL.LatePrintWorld("bounds 1", _bounds1.center);
		}
	}

	private void StartFadeOut()
	{
		_fadeStartTime = Time.unscaledTime;
		_fade = FadeOut;
		PlayAudioIfSceneReady("TunnelSupport_DestructionAndDebris");
	}

	private void FadeOut()
	{
		Color black = Color.black;
		float num = (black.a = Math.Min(1f, (Time.unscaledTime - _fadeStartTime) / 1f));
		QGL.LateBlit(0f, 0f, Screen.width, Screen.height, float.MaxValue, black);
		black = Color.magenta;
		black.a = num;
		if (num >= 1f)
		{
			if (GameState.ThreadIsSleeping && GameState.ThreadSleepReason == "FadeOut")
			{
				GameState.SelectedContinueAbility();
			}
			else if (GameState.ThreadIsSleeping && GameState.ThreadSleepReason == "FadeIn" && !Choreographer.s_Choreographer.m_MessageQueue.Any((CMessageData x) => x.m_Type == CMessageData.MessageType.DestroyProp))
			{
				Hide0();
				_fadeStartTime = Time.unscaledTime;
				_fade = FadeIn;
			}
		}
	}

	private void FadeIn()
	{
		Color black = Color.black;
		float num = Math.Min(1f, (Time.unscaledTime - _fadeStartTime) / 1f);
		black.a = 1f - num;
		QGL.LateBlit(0f, 0f, Screen.width, Screen.height, float.MaxValue, black);
		black = Color.magenta;
		black.a = num;
		if (num >= 1f)
		{
			_fade = null;
			if (GameState.ThreadIsSleeping && GameState.ThreadSleepReason == "FadeIn")
			{
				GameState.SelectedContinueAbility();
			}
		}
	}

	private void Hide0()
	{
		UnhideVisualObjects(_bounds1);
		HideVisualObjects(_bounds0, () => true, includeInactive: true);
		RegenerateMist(mapHidden0: false, mapHidden1: true);
	}

	private void Hide1()
	{
		UnhideVisualObjects(_bounds0);
		HideVisualObjects(_bounds1, () => true, includeInactive: true);
		RegenerateMist(mapHidden0: true, mapHidden1: false);
	}

	private void RegenerateMist(bool mapHidden0, bool mapHidden1)
	{
		if (!_forceMistRegeneration && _lastHiddenState.hidden0 == mapHidden0 && _lastHiddenState.hidden1 == mapHidden1)
		{
			return;
		}
		if ((object)_proceduralMist == null)
		{
			_proceduralMist = EnvironmentLoader.Instance.EnvironmentRoot.GetComponentInChildren<ProceduralMistSurrounding>();
		}
		if ((object)_mistOverride == null)
		{
			_mistOverride = _proceduralMist.GetOrAdd<SurroundingOverrideFH047>();
		}
		if (_proceduralMist.Done)
		{
			_mistOverride.MapHidden0 = mapHidden0;
			List<ProceduralMapTile> maps = (from map in UnityEngine.Object.FindObjectsOfType<ProceduralMapTile>(includeInactive: true)
				where (!mapHidden0 && map.name.Contains("FH_04_06:")) || (!mapHidden1 && map.name.Contains("N:"))
				select map).ToList();
			VerticalMeshSlicer.OnMeshSlicingCompletion onMeshSlicingComplete = () =>
			{
			};
			_proceduralMist.Generate(MultiplaneSystem.GetInstance(), maps, ProceduralScenario.GetVisualsBounds(maps), ref onMeshSlicingComplete);
			_lastHiddenState = (hidden0: mapHidden0, hidden1: mapHidden1);
			_forceMistRegeneration = false;
		}
	}

	private void TickMidgroundVFX()
	{
		if (_fade != null)
		{
			return;
		}
		float y = 1.8000001f;
		Color c = Color.magenta;
		c.a = 0.5f;
		int num = (int)(Time.time * 1000f);
		int num2 = (int)((float)num - Time.deltaTime * 1000f);
		if ((num & 0x100) != (num2 & 0x100))
		{
			SingleShot.AddConditional((float dt) =>
			{
				c.a -= 0.5f * dt;
				y -= dt * 1.25f;
				return c.a > 0f;
			});
		}
	}
}
