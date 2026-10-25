using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH096 : ScenarioScript
{
	private CObjectTerrainVisual MovingPlatformProp;

	private List<CTile> MovingPlatformFullPath;

	[StateDump(Skip = true)]
	private List<CPlayerActor> actorsWhoHaveMovedPlatformThisRound = new List<CPlayerActor>();

	private List<CScenarioModifier> DamageScenarioModifiers = new List<CScenarioModifier>();

	protected override bool Initialize()
	{
		CalcMovingPlatformFullPath();
		MovingPlatformProp = ScenarioManager.CurrentScenarioState.Props.FirstOrDefault((CObjectProp x) => x.SpecificPropType == ESpecificPropType.MovingPlatform) as CObjectTerrainVisual;
		if (MovingPlatformProp?.PropTile != null)
		{
			MovingPlatformProp.PropTile.AddProp(MovingPlatformProp, recursiveTiles: true);
		}
		DamageScenarioModifiers.Clear();
		CScenarioModifier cScenarioModifier = ScenarioScript._css.ScenarioModifiers.SingleOrDefault((CScenarioModifier x) => x.EventIdentifier == "DamageActorModifier");
		if (cScenarioModifier != null)
		{
			DamageScenarioModifiers.Add(cScenarioModifier);
		}
		CScenarioModifier cScenarioModifier2 = ScenarioScript._css.ScenarioModifiers.SingleOrDefault((CScenarioModifier x) => x.EventIdentifier == "DamagePropModifier");
		if (cScenarioModifier2 != null)
		{
			DamageScenarioModifiers.Add(cScenarioModifier2);
		}
		foreach (CScenarioModifier damageScenarioModifier in DamageScenarioModifiers)
		{
			damageScenarioModifier.CustomLocKeyArgs = new List<object> { 6 + ScenarioManager.CurrentScenarioState.Level };
		}
		return base.Initialize();
	}

	private void CalcMovingPlatformFullPath()
	{
		CTile tile = ScenarioManager.GetTile(4, 26);
		int num = 0;
		CTile[] array = new CTile[3]
		{
			ScenarioManager.GetTile(14, 26),
			ScenarioManager.GetTile(21, 11),
			ScenarioManager.GetTile(17, 3)
		};
		CTile cTile = tile;
		MovingPlatformFullPath = new List<CTile>();
		while (true)
		{
			CTile goal = array[num];
			MovingPlatformFullPath.Add(cTile);
			if (cTile == goal)
			{
				num++;
				if (num == array.Length)
				{
					break;
				}
			}
			cTile = (from x in ScenarioManager.GetAllAdjacentTiles(cTile)
				orderby Point.HexDist(x.m_ArrayIndex, goal.m_ArrayIndex)
				select x).FirstOrDefault((CTile x) => x.m_Props.Any((CObjectProp prop) => prop is CObjectDoor || prop is CObjectTerrainVisual));
		}
	}

	protected override void TickPhaseInternal()
	{
		switch (_scenarioModifier.ScenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.EndRound:
			TickEndRound();
			break;
		case EScenarioModifierTriggerPhase.EndTurn:
			TickPhaseEndTurn();
			break;
		}
	}

	private void TickPhaseEndTurn()
	{
		TryEscapeMap();
	}

	private void TickEndRound()
	{
		if (!OncePerRound() || CalcNextTileOnPlatformPath(MovingPlatformProp.PropTile) == MovingPlatformProp.PropTile)
		{
			return;
		}
		CPlayerActor hostCharacter = GameState.HostCharacter;
		if (hostCharacter != null)
		{
			PhaseManager.StartAbilities(CAbility.CopyAbility((ScenarioRuleClient.SRLYML.ScenarioAbilities.SingleOrDefault((ScenarioAbilitiesYMLData a) => a.ScenarioAbilityID == "ScenarioAbility_FHScenario096_MoveMovingPlatform")?.ScenarioAbilities.ToList())[0], generateNewID: false), hostCharacter, stackInsteadOfInject: true, hostCharacter);
		}
	}

	public static void MoveMovingPlatformOnPath(CAbility ability)
	{
		if (!(ability is CAbilityNullHex { AbilityHasHappened: not false }))
		{
			return;
		}
		ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
		ScenarioScript_FH096 scenarioScript_FH = ScenarioScript.current as ScenarioScript_FH096;
		CActor targetingActor = ability.TargetingActor;
		bool flag = ability.TilesSelected.LastOrDefault()?.FindObstacleProp(forDestructionFromPlayer: false, checkMultiHexProps: true)?.RuntimeAttachedActor != null;
		foreach (CTile item in ability.TilesSelected)
		{
			CTile propTile = scenarioScript_FH.MovingPlatformProp.PropTile;
			scenarioScript_FH.MovePlatformCallback(scenarioScript_FH.MovingPlatformProp, propTile, item, targetingActor);
		}
		if (CalcNextTileOnPlatformPath(scenarioScript_FH.MovingPlatformProp.PropTile) != scenarioScript_FH.MovingPlatformProp.PropTile && ability.NumberTargetsRemaining > 0 && !flag)
		{
			CAbility cAbility = CAbility.CopyAbility(ability, generateNewID: false);
			cAbility.StatIsBasedOnXEntries.Clear();
			cAbility.NumberTargets = ability.NumberTargetsRemaining;
			cAbility.SetCanSkip(canSkip: true);
			if (cAbility.MiscAbilityData == null)
			{
				cAbility.MiscAbilityData = new AbilityData.MiscAbilityData();
			}
			cAbility.MiscAbilityData.CanSkip = true;
			PhaseManager.StartAbilities(cAbility, ability.TargetingActor, stackInsteadOfInject: true);
		}
	}

	public static CTile CalcNextTileOnPlatformPath(CTile startTile)
	{
		ScenarioScript_FH096 scenarioScript_FH = ScenarioScript.current as ScenarioScript_FH096;
		for (int i = 0; i < scenarioScript_FH.MovingPlatformFullPath.Count; i++)
		{
			if (scenarioScript_FH.MovingPlatformFullPath[i] == startTile)
			{
				return scenarioScript_FH.MovingPlatformFullPath[Math.Min(i + 1, scenarioScript_FH.MovingPlatformFullPath.Count - 1)];
			}
		}
		return scenarioScript_FH.MovingPlatformFullPath.LastOrDefault();
	}

	public static List<CTile> GetTilesForPreviewFromTile(CAbility ability, CTile mouseOverTile)
	{
		CTile startTile = ability.TargetingActor.Tile;
		if (ability.TilesSelected.Count > 0)
		{
			startTile = ability.TilesSelected.Last();
		}
		return GetTilesForPreviewFromTile(ability, startTile, mouseOverTile);
	}

	public static List<CTile> GetTilesForPreviewFromTile(CAbility ability, CTile startTile, CTile lastSelectedTile)
	{
		ScenarioScript_FH096 scenarioScript_FH = ScenarioScript.current as ScenarioScript_FH096;
		List<CTile> list = new List<CTile>();
		int numberTargetsRemaining = ability.NumberTargetsRemaining;
		CTile cTile = startTile;
		if (cTile == lastSelectedTile)
		{
			return list;
		}
		int num = scenarioScript_FH.MovingPlatformFullPath.IndexOf(startTile);
		for (int i = num; i <= Math.Min(scenarioScript_FH.MovingPlatformFullPath.Count, num + numberTargetsRemaining); i++)
		{
			cTile = scenarioScript_FH.MovingPlatformFullPath[i];
			list.Add(cTile);
			if (cTile == lastSelectedTile)
			{
				break;
			}
		}
		return list;
	}

	public static void RecalcValidTilesToFormPath(CAbility ability, bool asPush = false)
	{
		CTile cTile = (ScenarioScript.current as ScenarioScript_FH096).MovingPlatformProp.PropTile;
		List<CTile> tilesSelected = ability.TilesSelected;
		int numberTargets = ability.NumberTargets;
		CTile cTile2 = cTile;
		if (tilesSelected.Count < numberTargets && tilesSelected.Count > 0)
		{
			CTile cTile3 = tilesSelected[tilesSelected.Count - 1];
			if (tilesSelected.Count > 1)
			{
				cTile2 = tilesSelected[tilesSelected.Count - 2];
			}
			tilesSelected.Remove(cTile3);
			List<CTile> tilesForPreviewFromTile = GetTilesForPreviewFromTile(ability, cTile2, cTile3);
			tilesForPreviewFromTile.Remove(cTile2);
			tilesSelected.AddRange(tilesForPreviewFromTile);
		}
		ability.TilesInRange.Clear();
		for (int i = 0; i < numberTargets; i++)
		{
			CTile cTile4 = CalcNextTileOnPlatformPath(cTile);
			ability.TilesInRange.Add(cTile4);
			CObjectObstacle cObjectObstacle = cTile4.FindObstacleProp(forDestructionFromPlayer: false, checkMultiHexProps: true);
			if (cObjectObstacle != null && cObjectObstacle.RuntimeAttachedActor != null)
			{
				break;
			}
			cTile = cTile4;
		}
		if (tilesSelected.Count == 0)
		{
			tilesSelected.Add(ability.TilesInRange[0]);
		}
		ability.NumberTargetsRemaining = numberTargets - tilesSelected.Count;
		if (ability.TilesInRange.Count == 1 && !ability.CanSkip && !ability.AutoTrigger)
		{
			ability.SetMiscVar("AutoTrigger", true);
		}
	}

	public static int NumberOfPlayersOnMovingPlatform()
	{
		return (ScenarioScript.current as ScenarioScript_FH096).MovingPlatformProp.GetTiles().Count((Point x) => ScenarioManager.GetTile(x).LivingActor != null);
	}

	protected override void MoveProp(CObjectProp prop, CTile from, CTile to, float speed = 2f)
	{
		if (!(prop is CObjectTerrainVisual) || prop == MovingPlatformProp)
		{
			base.MoveProp(prop, from, to, speed);
		}
	}

	protected override void MovePlatformCallback(CObjectProp boat, CTile tileToMoveFrom, CTile tileToMoveTo, CActor movingActor = null)
	{
		List<CObjectProp> list = tileToMoveTo.m_Props.ToList();
		foreach (CObjectProp prop in ScenarioManager.CurrentScenarioState.Props)
		{
			if (prop is CObjectObstacle cObjectObstacle && !list.Contains(prop) && cObjectObstacle.GetTiles().Contains(tileToMoveTo.m_ArrayIndex))
			{
				list.Add(prop);
			}
		}
		foreach (CObjectProp item in list)
		{
			if (item != MovingPlatformProp && (item is CObjectObstacle || item.PropType == EPropType.TerrainIce || item.PropType == EPropType.TerrainHotCoals || item.PropType == EPropType.TerrainRubble || item.PropType == EPropType.TerrainThorns || item.PropType == EPropType.TerrainWater || item.PropType == EPropType.Trap) && item.PropType != EPropType.TerrainVisualEffect && (item.RuntimeAttachedActor == null || item.RuntimeAttachedActor.IsDead))
			{
				item.DestroyProp();
			}
		}
		CActor cActor = tileToMoveTo.LivingActor;
		if (cActor == null)
		{
			CObjectObstacle cObjectObstacle2 = tileToMoveTo.FindObstacleProp(forDestructionFromPlayer: false, checkMultiHexProps: true);
			if (cObjectObstacle2?.RuntimeAttachedActor != null)
			{
				cActor = cObjectObstacle2.RuntimeAttachedActor;
			}
		}
		if (cActor != null)
		{
			if (cActor is CObjectActor)
			{
				CustomAbility.Damage(cActor, ScenarioManager.CurrentScenarioState.Level + 6, cActor);
				return;
			}
			CTile closestEmptyTile = ScenarioScript.GetClosestEmptyTile(tileToMoveTo.m_ArrayIndex);
			CustomAbility.PushActorTo(cActor, closestEmptyTile, tileToMoveTo);
			CustomAbility.Damage(cActor, ScenarioManager.CurrentScenarioState.Level + 6, cActor);
		}
		base.MovePlatformCallback(boat, tileToMoveFrom, tileToMoveTo, movingActor);
		foreach (CObjectDoor doorProp in ScenarioManager.CurrentScenarioState.DoorProps)
		{
			if (doorProp.DoorIsLocked && Point.HexDist(doorProp.PropTile.m_ArrayIndex, tileToMoveTo.m_ArrayIndex) <= 1)
			{
				doorProp.UnlockLockedDoorWithoutOpening();
			}
		}
		List<CObjectDoor> list2 = (from arrayIndex in MovingPlatformProp.GetTiles()
			select ScenarioManager.GetTile(arrayIndex)?.FindDoor() into door
			where door != null
			select door).ToList();
		if (!list2.Any((CObjectDoor x) => x.DoorIsClosed))
		{
			return;
		}
		foreach (CObjectDoor item2 in list2)
		{
			item2.ForceActivate(null);
		}
	}

	public override IEnumerable<(string, string)> GetSpecialRulesParamsString()
	{
		yield return ("collustion_damage", (ScenarioManager.CurrentScenarioState.Level + 6).ToString());
	}
}
