using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH091 : ScenarioScript
{
	public class UnrevealedRoomInfo
	{
		public bool revealed;

		public List<Point> TriggerPoints = new List<Point>();

		public List<CSpawner> spawners = new List<CSpawner>();

		public Point vfxPosition;
	}

	public List<UnrevealedRoomInfo> unrevealedRoomInfos = new List<UnrevealedRoomInfo>();

	private CScenarioModifier SectionRevealModifier;

	public static string SnowMistVFXKey = EVFXPropType.SnowMist.ToString();

	public override bool ShouldSortModifierTextsInOrderOfModifiers()
	{
		return true;
	}

	protected override bool Initialize()
	{
		SectionRevealModifier = GetScenarioModifier("SectionRevealModifier");
		unrevealedRoomInfos.Clear();
		unrevealedRoomInfos.Add(new UnrevealedRoomInfo
		{
			revealed = false,
			TriggerPoints = new List<Point>
			{
				new Point(17, 4),
				new Point(16, 3),
				new Point(17, 2),
				new Point(16, 1)
			},
			spawners = new List<CSpawner>
			{
				GetSpawner(new Point(16, 3)),
				GetSpawner(new Point(16, 2)),
				GetSpawner(new Point(10, 3)),
				GetSpawner(new Point(11, 1))
			},
			vfxPosition = new Point(13, 3)
		});
		unrevealedRoomInfos.Add(new UnrevealedRoomInfo
		{
			revealed = false,
			TriggerPoints = new List<Point>
			{
				new Point(10, 4),
				new Point(9, 3),
				new Point(10, 2),
				new Point(9, 1)
			},
			spawners = new List<CSpawner>
			{
				GetSpawner(new Point(3, 4)),
				GetSpawner(new Point(4, 2)),
				GetSpawner(new Point(8, 4)),
				GetSpawner(new Point(8, 1))
			},
			vfxPosition = new Point(6, 3)
		});
		return base.Initialize();
	}

	private void RefreshVFX()
	{
		foreach (UnrevealedRoomInfo unrevealedRoomInfo in unrevealedRoomInfos)
		{
			CTile tile = ScenarioManager.GetTile(unrevealedRoomInfo.vfxPosition);
			CObjectProp cObjectProp = tile.m_Props.FirstOrDefault((CObjectProp x) => x.PrefabName == SnowMistVFXKey);
			if (unrevealedRoomInfo.revealed)
			{
				cObjectProp?.DestroyProp();
			}
			else if (cObjectProp == null)
			{
				SpawnVFXProp(tile, SnowMistVFXKey);
			}
		}
	}

	protected override string SerializeInternal()
	{
		List<bool> list = new List<bool>();
		foreach (UnrevealedRoomInfo unrevealedRoomInfo in unrevealedRoomInfos)
		{
			list.Add(unrevealedRoomInfo.revealed);
		}
		return JsonConvert.SerializeObject(list);
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		if (!string.IsNullOrEmpty(storage))
		{
			List<bool> list = JsonConvert.DeserializeObject<List<bool>>(storage);
			for (int i = 0; i < unrevealedRoomInfos.Count; i++)
			{
				unrevealedRoomInfos[i].revealed = list[i];
			}
		}
		ToggleSectionPathfinding(active: false);
		UpdateSectionHighlights();
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartTurn:
			ToggleSectionPathfinding(active: false);
			break;
		case EScenarioModifierTriggerPhase.EndTurn:
			ToggleSectionPathfinding(active: false);
			TryEscapeMap();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 110);
			break;
		case EScenarioModifierTriggerPhase.StartRound:
		case EScenarioModifierTriggerPhase.EndRound:
		case EScenarioModifierTriggerPhase.StartScenario:
		case EScenarioModifierTriggerPhase.AfterAbility:
		case EScenarioModifierTriggerPhase.OnRoomRevealed:
			break;
		}
	}

	protected override void TickAbilityStartedInternal()
	{
		base.TickAbilityStartedInternal();
		CActor targetingActor = _tickAbility.TargetingActor;
		ToggleSectionPathfinding(targetingActor is CPlayerActor && _tickAbility is CAbilityMove);
	}

	protected override void TickAbilityEndedInternal()
	{
		base.TickAbilityEndedInternal();
		ToggleSectionPathfinding(active: false);
	}

	protected override void TickAbilityUndoneInternal()
	{
		base.TickAbilityUndoneInternal();
		ToggleSectionPathfinding(active: false);
	}

	private void ToggleSectionPathfinding(bool active)
	{
		if (unrevealedRoomInfos == null)
		{
			return;
		}
		foreach (UnrevealedRoomInfo unrevealedRoomInfo in unrevealedRoomInfos)
		{
			if (unrevealedRoomInfo.revealed)
			{
				continue;
			}
			foreach (Point triggerPoint in unrevealedRoomInfo.TriggerPoints)
			{
				for (int i = 0; i <= triggerPoint.X; i++)
				{
					CTile tile = ScenarioManager.GetTile(i, triggerPoint.Y);
					if (tile != null)
					{
						CNode node = ScenarioManager.PathFinder.GetNode(i, triggerPoint.Y);
						if (node != null)
						{
							node.Blocked = !active || tile.FindObstacleProp(forDestructionFromPlayer: false, checkMultiHexProps: true) != null;
							node.Walkable = active;
						}
					}
				}
			}
		}
	}

	protected override void TickRepositionInternal()
	{
		CheckSectionsReveal();
	}

	private void RevealSectionMonsters(UnrevealedRoomInfo info)
	{
		info.revealed = true;
		foreach (CSpawner spawner in info.spawners)
		{
			spawner.SpawnUnit(ScenarioScript.PartySize, treatAsSummon: false, initial: false, startRound: false, forceUseAnyEntry: false, addIntoInitiativeOrder: true);
		}
	}

	private void CheckSectionsReveal()
	{
		foreach (UnrevealedRoomInfo unrevealedRoomInfo in unrevealedRoomInfos)
		{
			if (!unrevealedRoomInfo.revealed && IsAnyPlayerOnSectionTile(unrevealedRoomInfo))
			{
				if (PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability is CAbilityMove cAbilityMove)
				{
					cAbilityMove.ForceInterrupt();
				}
				RevealSectionMonsters(unrevealedRoomInfo);
				UpdateSectionHighlights();
				if (!unrevealedRoomInfos.Any((UnrevealedRoomInfo x) => !x.revealed))
				{
					SectionRevealModifier.Hide();
				}
			}
		}
	}

	private bool IsAnyPlayerOnSectionTile(UnrevealedRoomInfo info)
	{
		foreach (Point triggerPoint in info.TriggerPoints)
		{
			foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
			{
				if (playerActor.ArrayIndex.Y == triggerPoint.Y && playerActor.ArrayIndex.X <= triggerPoint.X)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void UpdateSectionHighlights()
	{
		List<Point> list = new List<Point>();
		List<Point> list2 = new List<Point>();
		foreach (UnrevealedRoomInfo unrevealedRoomInfo3 in unrevealedRoomInfos)
		{
			list.AddRange(unrevealedRoomInfo3.TriggerPoints);
		}
		foreach (UnrevealedRoomInfo unrevealedRoomInfo4 in unrevealedRoomInfos)
		{
			if (unrevealedRoomInfo4.revealed)
			{
				continue;
			}
			foreach (Point triggerPoint in unrevealedRoomInfo4.TriggerPoints)
			{
				list.Remove(triggerPoint);
				list2.Add(triggerPoint);
			}
			break;
		}
		ScenarioManager.ToggleTileHighlights(list, enabled: false, new CVector4(1f, 1f, 0f, 1f), standingHighlights: true);
		ScenarioManager.ToggleTileHighlights(list2, enabled: true, new CVector4(1f, 1f, 0f, 1f), standingHighlights: true);
		RefreshVFX();
	}
}
