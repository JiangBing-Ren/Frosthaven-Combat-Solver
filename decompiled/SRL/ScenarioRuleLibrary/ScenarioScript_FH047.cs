using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.CustomLevels;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH047 : ScenarioScript
{
	public static int TriggerRound_cvar = 6;

	public int[] originA = new int[2] { 12, 9 };

	public int[] originB = new int[2] { 1, 9 };

	public bool collapsed;

	[StateDump(Skip = true)]
	public Action OnCollapsedChanged;

	private CScenarioModifier[] _specialRule6 = new CScenarioModifier[2];

	public CMap _Room1;

	public CMap _Room2;

	public readonly int[] midGround = new int[16]
	{
		6, 9, 6, 11, 6, 15, 3, 15, 3, 13,
		3, 11, 3, 9, 6, 13
	};

	public readonly int[] hiGround = new int[72]
	{
		3, 9, 3, 10, 3, 15, 2, 9, 1, 9,
		2, 10, 1, 11, 2, 11, 3, 11, 3, 12,
		2, 12, 1, 13, 2, 13, 3, 13, 3, 14,
		2, 14, 2, 15, 1, 15, 7, 15, 6, 15,
		7, 14, 8, 14, 8, 13, 7, 13, 6, 13,
		7, 12, 8, 12, 8, 11, 7, 11, 6, 11,
		7, 10, 8, 10, 7, 9, 8, 9, 6, 9,
		8, 15
	};

	public override IEnumerable<CTile> TilesCannotStopOnTopOf()
	{
		for (int i = 0; i < midGround.Length; i += 2)
		{
			int x = midGround[i];
			int y = midGround[i + 1];
			yield return ScenarioManager.GetTile(x, y);
		}
	}

	public void SetCollapsed(bool collapsed)
	{
		if (this.collapsed != collapsed)
		{
			this.collapsed = collapsed;
			OnCollapsedChanged?.Invoke();
		}
	}

	protected override bool Initialize()
	{
		if (CPathFinder.s_CanStepCallback != null)
		{
			Error("The pathfander can-step callback is alredy setup.", null, 44);
			return false;
		}
		_Room1 = ScenarioScript.FindRoom("Room_1");
		_Room2 = ScenarioScript.FindRoom("Room_2");
		_specialRule6[0] = GetScenarioModifier("round6A");
		_specialRule6[1] = GetScenarioModifier("round6B");
		Trace("Setting up pather callback.", null, 53);
		CPathFinder.s_CanStepCallback = (CPathFinderContext context, Point ptFrom, Point ptTo) =>
		{
			if (context.actor == null)
			{
				return CPathFinder.ECanStepCallbackResult.Default;
			}
			if ((PhaseManager.CurrentPhase as CPhaseAction)?.CurrentPhaseAbility?.m_Ability is CAbilityMove cAbilityMove && (cAbilityMove.Jump || cAbilityMove.Fly))
			{
				return CPathFinder.ECanStepCallbackResult.Default;
			}
			if (IsOnMidGround(ptFrom.X, ptFrom.Y) && IsOnLowGround(ptTo.X, ptTo.Y))
			{
				return CPathFinder.ECanStepCallbackResult.Default;
			}
			if (IsOnLowGround(ptFrom.X, ptFrom.Y) && IsOnMidGround(ptTo.X, ptTo.Y))
			{
				return CPathFinder.ECanStepCallbackResult.Default;
			}
			if (IsOnHiGround(ptFrom.X, ptFrom.Y) && !IsOnMidGround(ptTo.X, ptTo.Y) && IsOnLowGround(ptTo.X, ptTo.Y))
			{
				return CPathFinder.ECanStepCallbackResult.ForceCannotStep;
			}
			if (IsOnLowGround(ptFrom.X, ptFrom.Y) && IsOnHiGround(ptTo.X, ptTo.Y))
			{
				return CPathFinder.ECanStepCallbackResult.ForceCannotStep;
			}
			return (IsOnMidGround(ptFrom.X, ptFrom.Y) && IsOnHiGround(ptTo.X, ptTo.Y)) ? CPathFinder.ECanStepCallbackResult.ForceCannotStep : CPathFinder.ECanStepCallbackResult.Default;
		};
		return true;
	}

	protected override void Done()
	{
		Trace("Removing pather callback.", null, 93);
		CPathFinder.s_CanStepCallback = null;
	}

	protected override void TickDeathInternal()
	{
	}

	protected override void TickRepositionInternal()
	{
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartScenario:
		{
			CScenarioModifier[] specialRule = _specialRule6;
			foreach (CScenarioModifier cScenarioModifier in specialRule)
			{
				if (cScenarioModifier == null)
				{
					Error("Need scenario modifiers named round6A and round6B", null, 108);
					break;
				}
				cScenarioModifier.SetHiddenState(shouldSetHidden: true);
			}
			break;
		}
		case EScenarioModifierTriggerPhase.StartRound:
			if (!collapsed && (ScenarioScript.RoundNumber >= TriggerRound_cvar || !ScenarioManager.Scenario.Enemies.Any((CEnemyActor x) => x.Tile.m_HexMap == _Room1)))
			{
				Collapse();
				CScenarioModifier[] specialRule = _specialRule6;
				for (int num = 0; num < specialRule.Length; num++)
				{
					specialRule[num].SetHiddenState(shouldSetHidden: false);
				}
			}
			break;
		default:
			Error($"Unhandled phase {scenarioModifierTriggerPhase}, set proper phases in the editor.", null, 124);
			break;
		}
	}

	protected override string SerializeInternal()
	{
		return collapsed.ToString();
	}

	protected override void DeserializeInternal(string s)
	{
		if (!string.IsNullOrEmpty(s) && !bool.TryParse(s, out collapsed))
		{
			collapsed = _Room2.Revealed;
		}
	}

	protected override void TickAbilityStartedInternal()
	{
	}

	protected override void TickAbilityEndedInternal()
	{
	}

	protected override bool AllowTriggerMessageInternal(SEvent ev, CLevelMessage message)
	{
		if (collapsed)
		{
			Trace($"Allow message {message.MessageName} {message.TitleKey} {ev.Type}...", null, 148);
			return true;
		}
		return false;
	}

	public override void OnHeroSummonSpawned(CHeroSummonActor actor)
	{
	}

	public void Collapse()
	{
		if (!_initialized || collapsed)
		{
			return;
		}
		Trace("Triggering collapse...", null, 162);
		SetCollapsed(collapsed: true);
		ScenarioRuleClient.MessageHandler(new CTriggerCameraShake_MessageData(null)
		{
			duration = 4f,
			magnitude = 0.1f,
			speed = 50f,
			distanceForce = 50f
		});
		GameState.WaitForConfirmation("FadeOut");
		RevealRoom(_Room2);
		foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
		{
			Trace(allAliveActor.Class.ID + "prev: " + allAliveActor.ArrayIndex.ToString(), null, 171);
			if (ScenarioManager.Tiles[allAliveActor.ArrayIndex.X, allAliveActor.ArrayIndex.Y]?.m_HexMap?.RoomName != "Room_1")
			{
				continue;
			}
			int num = allAliveActor.ArrayIndex.X - originA[0];
			int num2 = allAliveActor.ArrayIndex.Y - originA[1];
			int originX = originB[0] + num;
			int originY = originB[1] + num2;
			Trace(allAliveActor.Class.ID + "next: " + originX + "," + originY, null, 184);
			if (!GetNearestFreeTile(originX, originY, out CTile nearestTile, (List<CTile>)null, (Func<int, int, bool>)((int x, int y) => !IsOnMidGround(x, y))))
			{
				continue;
			}
			TeleportActor(allAliveActor, nearestTile, tryReveal: true);
			if (!IsDeathwalker(allAliveActor as CPlayerActor))
			{
				continue;
			}
			foreach (CObjectProp item in ScenarioManager.CurrentScenarioState.ShadowProps.ToList())
			{
				int num3 = item.ArrayIndex.X - originA[0];
				int num4 = item.ArrayIndex.Y - originA[1];
				int num5 = originB[0] + num3;
				int num6 = originB[1] + num4;
				CTile cTile = ScenarioManager.Tiles[num5, num6];
				if (cTile != null)
				{
					item.DestroyProp();
					CObjectShadow prop = new CObjectShadow("Shadow", ScenarioManager.ObjectImportType.Shadow, new TileIndex(cTile.m_ArrayIndex), null, item.Rotation, allAliveActor as CPlayerActor, cTile.m_HexMap.MapGuid);
					cTile.SpawnProp(prop);
				}
			}
		}
		DestroyMap(_Room1);
		GameState.WaitForConfirmation("FadeIn");
		ScenarioRuleClient.MessageHandler(new CTriggerCameraShake_MessageData(null)
		{
			duration = 4f,
			magnitude = 0.1f,
			speed = 50f,
			distanceForce = 50f
		});
	}

	public void DebugUndoCollapse()
	{
		if (!collapsed)
		{
			Trace("Not collapsed yet.", null, 216);
			return;
		}
		Trace("Undo collapse...", null, 219);
		SetCollapsed(collapsed: false);
		foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
		{
			int num = allAliveActor.ArrayIndex.X - originB[0];
			int num2 = allAliveActor.ArrayIndex.Y - originB[1];
			int originX = originA[0] + num;
			int originY = originA[1] + num2;
			if (GetNearestFreeTile(originX, originY, out var nearestTile))
			{
				TeleportActor(allAliveActor, nearestTile);
			}
		}
	}

	public bool IsOnMidGround(int x, int y)
	{
		return IsOnTiles(x, y, midGround);
	}

	public bool IsOnMidGround(CActor actor)
	{
		return IsOnTiles(actor, midGround);
	}

	public bool IsOnMidGround(CTile tile)
	{
		return IsOnTiles(tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y, midGround);
	}

	public bool IsOnHiGround(CTile tile)
	{
		return IsOnHiGround(tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y);
	}

	public bool IsOnHiGround(int x, int y)
	{
		return IsOnTiles(x, y, hiGround);
	}

	public bool IsOnLowGround(int x, int y)
	{
		return !IsOnHiGround(x, y);
	}

	public bool IsOnHiGround(CActor actor)
	{
		return IsOnTiles(actor, hiGround);
	}

	public bool IsOnLowGround(CActor actor)
	{
		return !IsOnHiGround(actor);
	}

	private bool IsOnTiles(CActor actor, int[] tiles)
	{
		return IsOnTiles(actor.ArrayIndex.X, actor.ArrayIndex.Y, tiles);
	}

	private bool IsOnTiles(int x, int y, int[] tiles)
	{
		for (int i = 0; i < tiles.Length; i += 2)
		{
			if (x == tiles[i] && y == tiles[i + 1])
			{
				return true;
			}
		}
		return false;
	}
}
