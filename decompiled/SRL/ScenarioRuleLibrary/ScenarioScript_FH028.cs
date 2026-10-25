using System.Collections.Generic;
using System.Linq;
using AStar;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH028 : ScenarioScript
{
	private readonly string _snowspeakerClassId = "AlgoxSnowspeakerID";

	private readonly string _icespeakerClassId = "AlgoxIcespeakerID";

	private readonly List<Point> _bdTiles = new List<Point>
	{
		new Point(12, 1),
		new Point(13, 2),
		new Point(13, 3)
	};

	private readonly List<Point> _ceTiles = new List<Point>
	{
		new Point(18, 14),
		new Point(17, 15),
		new Point(18, 16)
	};

	private CObjective_ActorsEscaped _escapeObjective => ScenarioScript._css.WinObjectives.FirstOrDefault((CObjective o) => o.ObjectiveType == EObjectiveType.ActorsEscaped) as CObjective_ActorsEscaped;

	protected override bool Initialize()
	{
		_ = ScenarioManager.CurrentScenarioState.Spawners;
		foreach (CEnemyActor item in ScenarioManager.Scenario.AllAllyMonsters.Where((CEnemyActor monster) => monster.MonsterClass.ID == "Emissary028ID").ToList())
		{
			if (item.ChosenModelIndex == 0)
			{
				item.CustomLocKey = "FH/Barduu";
			}
			else
			{
				item.CustomLocKey = "FH/Grundel";
			}
		}
		RefreshExitTiles();
		return true;
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.EndTurn:
			TickPhaseEndTurn();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 72);
			break;
		case EScenarioModifierTriggerPhase.StartRound:
		case EScenarioModifierTriggerPhase.EndRound:
		case EScenarioModifierTriggerPhase.StartTurn:
		case EScenarioModifierTriggerPhase.StartScenario:
			break;
		}
	}

	private void TickPhaseEndTurn()
	{
		if ((_tickActor.Type == CActor.EType.Ally || _tickActor.Type == CActor.EType.Player) && (_tickActor.Type != CActor.EType.Player || _tickActor.AllowEscape) && GetExitTiles().Contains(_tickActor.ArrayIndex))
		{
			EscapeMap();
		}
	}

	private bool CheckSpeakersDead(string speakerClassId, CActor lastDeadSpeaker = null)
	{
		List<CEnemyActor> list = ScenarioManager.Scenario.AllAliveMonsters.FindAll((CEnemyActor m) => (m.Type == CActor.EType.Enemy || m.Type == CActor.EType.Enemy2) && (m.MonsterClass.ID == speakerClassId || m.MonsterClass.NonEliteVariant?.ID == speakerClassId));
		if (list.Count == 0)
		{
			return true;
		}
		if (list.Count > 1)
		{
			return false;
		}
		if (lastDeadSpeaker != null && list.Contains(lastDeadSpeaker))
		{
			return true;
		}
		return false;
	}

	private List<Point> GetExitTiles(CActor lastDeadSpeaker = null)
	{
		List<Point> list = new List<Point>();
		if (CheckSpeakersDead(_icespeakerClassId, lastDeadSpeaker))
		{
			list.AddRange(_bdTiles);
		}
		if (CheckSpeakersDead(_snowspeakerClassId, lastDeadSpeaker))
		{
			list.AddRange(_ceTiles);
		}
		return list;
	}

	private void RefreshExitTiles(CActor lastDeadSpeaker = null)
	{
		List<Point> exitTiles = GetExitTiles(lastDeadSpeaker);
		_escapeObjective.EscapePositions.Clear();
		_escapeObjective.EscapePositions.AddRange(exitTiles.Select((Point t) => new TileIndex(t)));
		ScenarioRuleClient.MessageHandler(new CRefreshExitTiles_MessageData());
	}

	protected override void TickDeathInternal()
	{
		if (!(_tickActor?.MonsterClass?.ID != _snowspeakerClassId) || (_tickActor?.MonsterClass?.NonEliteVariant != null && !(_tickActor.MonsterClass.NonEliteVariant.ID != _snowspeakerClassId)) || !(_tickActor?.MonsterClass?.ID != _icespeakerClassId) || (_tickActor?.MonsterClass?.NonEliteVariant != null && !(_tickActor.MonsterClass.NonEliteVariant.ID != _icespeakerClassId)))
		{
			RefreshExitTiles(_tickActor);
		}
	}

	public override string GetOverrideCharacterModelPrefabName(CActor actor)
	{
		return actor.GetPrefabName() switch
		{
			"AlgoxGuard" => "AlgoxGuardSnowspeaker", 
			"AlgoxGuard Elite" => "AlgoxGuardSnowspeaker Elite", 
			"AlgoxSnowspeaker" => "AlgoxSnowspeakerOrange", 
			"AlgoxSnowspeaker Elite" => "AlgoxSnowspeakerOrange Elite", 
			_ => null, 
		};
	}
}
