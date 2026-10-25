using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;
using ScenarioRuleLibrary.CustomLevels;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH062 : ScenarioScript
{
	private readonly List<Point> TargetPoints = new List<Point>
	{
		new Point(5, 13),
		new Point(9, 10),
		new Point(8, 4),
		new Point(4, 2),
		new Point(2, 8),
		new Point(5, 13)
	};

	private CObjective_Scripted[] _crainObjectives;

	public readonly List<List<string>> SpawnerGUIDs = new List<List<string>>
	{
		new List<string> { "f0f029dc-64b9-495e-b0e9-33cb1abf535a", "b2bad81e-9a14-40e0-9736-756986b12a16", "ce940790-ef96-4fb4-8ebb-804ad3920a22", "98daf688-4129-490a-8afb-480a31275058" },
		new List<string> { "006aa70a-5518-49d0-8984-86b38f9a9c63", "0d7f68ef-bc34-475a-92de-71a167a0ec69", "d41edb80-3629-4934-b9fa-d7f9d838458b" },
		new List<string> { "c7fc4e08-1cff-4cbc-a39e-c9f24eac86c1", "377042d3-6147-40bf-b699-2b2ae8b3df60" },
		new List<string> { "95438a2e-9c16-46da-9f4f-1e8e20869bc5", "216d6eb7-c69f-4616-993e-3ab0d6c5135d", "d8809d38-ce85-4bd2-9b37-ddf3c3ffd87b" },
		new List<string> { "cd07675f-40c3-49e1-b8c6-0e89601d9f5c", "db22fe4c-5b3e-42ed-b285-a612b78b1a3c" },
		new List<string> { "4e74af9e-dff5-4d19-9eae-2296d8fb2397", "ed6cd12a-dcdf-4271-8606-2ef23204a3f9" }
	};

	private readonly string tuningDeviceID = "TuningDevice";

	private CActor Crain;

	private CObjective_Scripted ResearchCompleteObjective;

	private CObjective_Scripted CrainDiedObjective;

	public int CurTargetIdx;

	public int CurTargetTokens;

	public List<int> UsedNumberTokens = new List<int>();

	public List<int> RemovedNumberTokens = new List<int>();

	public int NextNumberToken;

	public bool spawnDelay;

	private Point PrevTargetPoint
	{
		get
		{
			if (CurTargetIdx - 1 >= 0 && CurTargetIdx - 1 < TargetPoints.Count)
			{
				return TargetPoints[CurTargetIdx - 1];
			}
			return default;
		}
	}

	private Point CurTargetPoint
	{
		get
		{
			if (CurTargetIdx >= 0 && CurTargetIdx < TargetPoints.Count)
			{
				return TargetPoints[CurTargetIdx];
			}
			return default;
		}
	}

	protected override bool Initialize()
	{
		Crain = FindActorByGUID("744cd1a5-354d-db19-581b-64682ff87187");
		CheckSpawnerGUIDs();
		ResearchCompleteObjective = GetObjective("ResearchComplete") as CObjective_Scripted;
		CrainDiedObjective = GetObjective("CrainDied") as CObjective_Scripted;
		_crainObjectives = new CObjective_Scripted[6]
		{
			GetObjective("crainGoalA1") as CObjective_Scripted,
			GetObjective("crainGoalB") as CObjective_Scripted,
			GetObjective("crainGoalC") as CObjective_Scripted,
			GetObjective("crainGoalD") as CObjective_Scripted,
			GetObjective("crainGoalE") as CObjective_Scripted,
			GetObjective("crainGoalA2") as CObjective_Scripted
		};
		return true;
	}

	protected override string SerializeInternal()
	{
		string text = JsonConvert.SerializeObject(CurTargetIdx);
		string text2 = JsonConvert.SerializeObject(CurTargetTokens);
		string text3 = JsonConvert.SerializeObject(UsedNumberTokens);
		string text4 = JsonConvert.SerializeObject(RemovedNumberTokens);
		string text5 = JsonConvert.SerializeObject(NextNumberToken);
		return JsonConvert.SerializeObject(new string[5] { text, text2, text3, text4, text5 });
	}

	protected override void DeserializeInternal(string s)
	{
		if (!string.IsNullOrEmpty(s))
		{
			string[] array = JsonConvert.DeserializeObject<string[]>(s);
			int curTargetIdx = JsonConvert.DeserializeObject<int>(array[0]);
			int curTargetTokens = JsonConvert.DeserializeObject<int>(array[1]);
			SetTarget(curTargetIdx, curTargetTokens);
			UsedNumberTokens = JsonConvert.DeserializeObject<List<int>>(array[2]);
			RemovedNumberTokens = JsonConvert.DeserializeObject<List<int>>(array[3]);
			NextNumberToken = JsonConvert.DeserializeObject<int>(array[4]);
		}
		else
		{
			SetTarget(0, 2);
		}
		SetNextNumberToken(NextNumberToken);
		ScenarioManager.ToggleTileHighlights(ScenarioManager.CurrentScenarioState.Spawners.Select((CSpawner x) => x.ArrayIndex.ToPoint()), enabled: true, new CVector4(1f, 0.5f, 0f, 1f));
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartTurn:
			TickStartTurn();
			break;
		case EScenarioModifierTriggerPhase.EndTurn:
			TickEndTurn();
			break;
		case EScenarioModifierTriggerPhase.StartRound:
			TickStartRound();
			break;
		case EScenarioModifierTriggerPhase.EndRound:
			TickEndRound();
			break;
		case EScenarioModifierTriggerPhase.StartScenario:
			TickStartScenario();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 161);
			break;
		}
	}

	private void TickStartTurn()
	{
		if (_tickActor == Crain)
		{
			SetCrainFocusOverride();
			if (Crain.ArrayIndex != CurTargetPoint)
			{
				RemoveTuningDevice();
			}
		}
	}

	private void TickEndTurn()
	{
	}

	private void TickStartRound()
	{
		if (OncePerRound())
		{
			SpawnAtStartRound();
		}
	}

	private void TickEndRound()
	{
		if (_tickActor == Crain)
		{
			CheckMoveToNextTarget();
		}
	}

	private void TickStartScenario()
	{
		if (OncePerRound())
		{
			for (int i = 0; i < _crainObjectives.Length; i++)
			{
				CObjective_Scripted obj = _crainObjectives[i];
				obj.Total = 2;
				obj.Current = 0;
				obj.ScriptedIsComplete = false;
			}
			TryPlaceTuningDevice();
			SetTarget(0);
		}
	}

	protected override void TickDeathInternal()
	{
		if (_tickActor == Crain)
		{
			CrainDiedObjective.ScriptedIsComplete = true;
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		}
	}

	public void SetTarget(int curTargetIdx, int curTargetTokens = 0)
	{
		ScenarioManager.ToggleTileHighlights(new List<Point> { CurTargetPoint }, enabled: false);
		CurTargetIdx = curTargetIdx;
		CurTargetTokens = curTargetTokens;
		UpdateObjectiveProgress(_crainObjectives[CurTargetIdx], CurTargetTokens);
		ScenarioManager.ToggleTileHighlights(new List<Point> { CurTargetPoint }, enabled: true);
	}

	private void SetCrainFocusOverride()
	{
		if (CurTargetIdx >= 0 && CurTargetIdx < TargetPoints.Count)
		{
			CAIFocusOverrideDetails.SetAIFocusOverrideOnTileOrNearestEmpty(Crain, ScenarioManager.GetTile(CurTargetPoint));
		}
	}

	private void RemoveTuningDevice()
	{
		foreach (Point targetPoint in TargetPoints)
		{
			ScenarioManager.Tiles[targetPoint.X, targetPoint.Y].m_Props.Find((CObjectProp p) => p.PrefabName == tuningDeviceID)?.DestroyProp();
		}
	}

	private void TryPlaceTuningDevice()
	{
		if (!(Crain.ArrayIndex != CurTargetPoint) && !Crain.Tile.m_Props.Any((CObjectProp p) => p.PrefabName == tuningDeviceID))
		{
			CObjectProp prop = new CObjectProp(tuningDeviceID, ScenarioManager.ObjectImportType.VFXProp, new TileIndex(Crain.ArrayIndex), null, null, null, Crain.Tile.m_HexMap.MapGuid);
			Crain.Tile.SpawnProp(prop);
		}
	}

	private void CheckMoveToNextTarget()
	{
		if (Crain.ArrayIndex != CurTargetPoint)
		{
			return;
		}
		if (CurTargetTokens == 0)
		{
			CurTargetTokens = 1;
			UpdateObjectiveProgress(_crainObjectives[CurTargetIdx], CurTargetTokens);
			return;
		}
		int num = CurTargetIdx + 1;
		UpdateObjectiveProgress(_crainObjectives[CurTargetIdx], 2);
		if (num >= TargetPoints.Count)
		{
			PlayTuningDeviceExplosionVFX();
			RemoveTuningDevice();
			if (CurTargetIdx != 1)
			{
				Crain.Tile.m_Props.Find((CObjectProp p) => p.PropType == EPropType.TerrainVisualEffect)?.DestroyProp();
			}
			ResearchCompleteObjective.ScriptedIsComplete = true;
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
			return;
		}
		CFocusCamera_MessageData message = new CFocusCamera_MessageData(_tickActor)
		{
			m_Point = Crain.ArrayIndex,
			m_Zoom = 0.2f
		};
		ScenarioRuleClient.MessageHandler(message);
		GameState.TriggerCampaignSection($"FS_062_The_Unfettered_Seal_Special_{num}");
		SetTarget(num);
		PlayTuningDeviceExplosionVFX();
		RemoveTuningDevice();
		if (CurTargetIdx != 1)
		{
			Crain.Tile.m_Props.Find((CObjectProp p) => p.PropType == EPropType.TerrainVisualEffect)?.DestroyProp();
		}
		ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		PerformTargetSpecificEffect(CurTargetIdx);
	}

	private void PlayTuningDeviceExplosionVFX()
	{
		if (Crain.Tile.m_Props.Find((CObjectProp p) => p.PrefabName == tuningDeviceID) != null)
		{
			ScenarioRuleClient.MessageHandler(new CTryPlayVfxOnTile_MessageData(null)
			{
				m_VFXPropType = "HeartOfIceVFX",
				m_ArrayIndex = Crain.ArrayIndex
			});
			CWaitingForProlongedSpawns_MessageData message = new CWaitingForProlongedSpawns_MessageData();
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	private void UpdateObjectiveProgress(CObjective_Scripted objective, int current)
	{
		if (!objective.IsActive)
		{
			objective.SetActivation(active: true);
		}
		objective.Current = current;
		objective.ScriptedIsComplete = objective.Current >= objective.Total;
		ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
	}

	public void PerformTargetSpecificEffect(int idx)
	{
		switch (idx)
		{
		case 1:
		{
			foreach (CEnemyActor item in GetMonstersAroundCrain(3))
			{
				item.ApplyCondition(Crain, CCondition.ENegativeCondition.Muddle);
			}
			break;
		}
		case 2:
			foreach (CEnemyActor item2 in GetMonstersAroundCrain(2))
			{
				DealScenarioDamage(item2, ScenarioManager.TrapDamage, isTrapDamage: true);
			}
			RemoveRandomNumberToken();
			break;
		case 3:
			foreach (CEnemyActor item3 in GetMonstersAroundCrain(4))
			{
				item3.ApplyCondition(Crain, CCondition.EPositiveCondition.Strengthen);
			}
			RemoveRandomNumberToken();
			break;
		case 4:
			foreach (CEnemyActor item4 in GetMonstersAroundCrain(3))
			{
				GameState.KillActor(item4, item4, CActor.ECauseOfDeath.Suicide, out var _);
			}
			RemoveRandomNumberToken();
			break;
		case 5:
			foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
			{
				DealScenarioDamage(playerActor, ScenarioManager.TrapDamage, isTrapDamage: true);
			}
			RemoveRandomNumberToken();
			break;
		}
	}

	public void SetNextNumberToken(int idx)
	{
		NextNumberToken = idx;
		if (NextNumberToken <= 0)
		{
			NextNumberToken = ChooseRandomNumberToken(includeUsed: false);
		}
		RemovedNumberTokens.Remove(NextNumberToken);
		UsedNumberTokens.Remove(NextNumberToken);
	}

	public List<int> GetAvailableNumberTokensForSpawners(bool includeUsed = false, bool includeNext = false)
	{
		List<int> list = new List<int>();
		for (int i = 1; i <= 6; i++)
		{
			if ((i != NextNumberToken || includeNext) && !RemovedNumberTokens.Contains(i) && (includeUsed || !UsedNumberTokens.Contains(i)))
			{
				list.Add(i);
			}
		}
		return list;
	}

	private int ChooseRandomNumberToken(bool includeUsed)
	{
		List<int> availableNumberTokensForSpawners = GetAvailableNumberTokensForSpawners(includeUsed);
		if (availableNumberTokensForSpawners.Count == 0)
		{
			if (includeUsed)
			{
				return -1;
			}
			if (UsedNumberTokens.Count == 0)
			{
				return -2;
			}
			availableNumberTokensForSpawners.AddRange(UsedNumberTokens);
			availableNumberTokensForSpawners.Remove(NextNumberToken);
			UsedNumberTokens.Clear();
		}
		int index = ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(availableNumberTokensForSpawners.Count);
		return availableNumberTokensForSpawners[index];
	}

	private void SpawnAtStartRound()
	{
		int num = NextNumberToken;
		NextNumberToken = 0;
		if (num <= 0)
		{
			num = ChooseRandomNumberToken(includeUsed: false);
		}
		if (num <= 0)
		{
			return;
		}
		UsedNumberTokens.Add(num);
		foreach (string guid in SpawnerGUIDs[num - 1])
		{
			CSpawner cSpawner = ScenarioManager.CurrentScenarioState.Spawners.FirstOrDefault((CSpawner x) => x.SpawnerGuid == guid);
			if (cSpawner == null)
			{
				Error("Could not find spawner with GUID " + guid, null, 454);
			}
			else
			{
				cSpawner.SpawnUnit(ScenarioScript.PartySize, treatAsSummon: false, initial: false, startRound: true, forceUseAnyEntry: true, addIntoInitiativeOrder: true, spawnFarthestOnAdjacent: false, useFirstRoundStartingTile: false, spawnDelay ? 3 : 0);
			}
		}
		spawnDelay = false;
		NextNumberToken = ChooseRandomNumberToken(includeUsed: false);
		SetNextNumberToken(NextNumberToken);
	}

	private void CheckSpawnerGUIDs()
	{
		List<string> list = ScenarioManager.CurrentScenarioState.Spawners.Select((CSpawner x) => x.SpawnerGuid).ToList();
		foreach (List<string> spawnerGUID in SpawnerGUIDs)
		{
			foreach (string item in spawnerGUID)
			{
				if (!list.Contains(item))
				{
					Error("Could not find spawner with GUID " + item, null, 474);
				}
			}
		}
	}

	private void RemoveRandomNumberToken()
	{
		int num = ChooseRandomNumberToken(includeUsed: true);
		if (num > 0)
		{
			UsedNumberTokens.Clear();
			RemovedNumberTokens.Add(num);
			NextNumberToken = 0;
		}
	}

	public override bool IsActorInteracting(CActor actor)
	{
		if (actor != Crain)
		{
			return false;
		}
		if (actor.ArrayIndex == CurTargetPoint)
		{
			return true;
		}
		if (actor.ArrayIndex == PrevTargetPoint && actor.Tile.m_Props.Any((CObjectProp p) => p.PrefabName == tuningDeviceID))
		{
			return true;
		}
		return false;
	}

	private List<CEnemyActor> GetMonstersAroundCrain(int range)
	{
		List<CEnemyActor> list = new List<CEnemyActor>();
		foreach (CEnemyActor enemy in ScenarioManager.Scenario.Enemies)
		{
			if (Point.HexDist(enemy.ArrayIndex, Crain.ArrayIndex) <= range)
			{
				list.Add(enemy);
			}
		}
		return list;
	}

	protected override void TickRepositionInternal()
	{
		_ = _tickActor;
		_ = Crain;
	}

	protected override void TickAbilityStartedInternal()
	{
		if (_tickAbility.TargetingActor == Crain)
		{
			RemoveTuningDevice();
		}
	}

	protected override void TickAbilityEndedInternal()
	{
		if (_tickAbility.TargetingActor == Crain)
		{
			TryPlaceTuningDevice();
		}
	}

	public override bool TreatTileAsNonFeatureless(CTile tile)
	{
		if (TargetPoints.Contains(tile.m_ArrayIndex))
		{
			return true;
		}
		return base.TreatTileAsNonFeatureless(tile);
	}

	public IEnumerable<CSpawner> GetTokenSpawners(int token)
	{
		List<string> list = SpawnerGUIDs[token];
		foreach (string guid in list)
		{
			CSpawner cSpawner = ScenarioManager.CurrentScenarioState.Spawners.FirstOrDefault((CSpawner x) => x.SpawnerGuid == guid);
			if (cSpawner != null)
			{
				yield return cSpawner;
			}
		}
	}
}
