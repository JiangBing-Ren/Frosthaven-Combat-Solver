using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.CustomLevels;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH002 : ScenarioScript
{
	private CObjective[] _objectives;

	private CActor _algoxPriest;

	[StateDump(Skip = true)]
	private bool _priestInLastRoom;

	private CScenarioModifier _priestInfo;

	private CActor AlgoxPriest
	{
		get
		{
			if (_algoxPriest != null)
			{
				return _algoxPriest;
			}
			_algoxPriest = ScenarioManager.Scenario.AllAllyMonsters.Where((CEnemyActor x) => x.Class.ID.Contains("AlgoxPriest", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
			return _algoxPriest;
		}
		set
		{
			_algoxPriest = value;
		}
	}

	protected override bool Initialize()
	{
		_objectives = new CObjective[3]
		{
			GetObjective("KillAll"),
			GetObjective("AllyDies"),
			GetObjective("IceSheet")
		};
		CObjective[] objectives = _objectives;
		for (int i = 0; i < objectives.Length; i++)
		{
			if (objectives[i] == null)
			{
				Error("Create 'KillAll', 'IceSheet' and 'AllyDies' objectives.", null, 43);
				return false;
			}
		}
		_priestInfo = GetScenarioModifier("PriestInfo");
		if (_priestInfo == null)
		{
			Error("Create 'PriestInfo' scenario modifier", null, 51);
			return false;
		}
		if (ScenarioManager.CurrentScenarioState.AllyMonsters.Count == 0)
		{
			Error("Couldn't find any AllyMonsters on the map.", null, 56);
			return false;
		}
		EnemyState enemyState = ScenarioManager.CurrentScenarioState.AllyMonsters[0];
		if (enemyState.AIFocusOverride == null)
		{
			Error("Algox Priest EnemyState has no AIFocusOverride.", null, 63);
			return false;
		}
		if (enemyState.AIFocusOverride.OverrideTargetType == CAIFocusOverrideDetails.EOverrideTargetType.None)
		{
			Error("Algox Priest AIFocusOverride is not set in the Editor.", null, 69);
			return false;
		}
		Trace("Algox Priest is setup correctly.", null, 73);
		_priestInfo.SetDeactivated(deactivate: true);
		UpdateSpecialRulesAndObjectives();
		return true;
	}

	protected override string SerializeInternal()
	{
		return AlgoxPriest?.ActorGuid;
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		if (!string.IsNullOrEmpty(storage))
		{
			AlgoxPriest = FindActorByGUID(storage);
		}
		else
		{
			AlgoxPriest = null;
		}
	}

	protected override bool AllowTriggerMessageInternal(SEvent ev, CLevelMessage message)
	{
		if (AlgoxPriest == null)
		{
			return false;
		}
		if (_priestInfo.Deactivated)
		{
			return false;
		}
		if (!_priestInLastRoom)
		{
			return false;
		}
		if (message.MessageName.EndsWith("_1"))
		{
			Trace($"Allow message {message.MessageName} {message.TitleKey} {ev.Type}...", null, 112);
			return true;
		}
		return false;
	}

	protected override void TickPhaseInternal()
	{
		Trace("TickPhase " + _tickActor.Class.ID, null, 120);
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartTurn:
			if (_tickActor == AlgoxPriest)
			{
				TickAlgoxPriest(_tickActor);
			}
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 134);
			break;
		case EScenarioModifierTriggerPhase.StartRound:
		case EScenarioModifierTriggerPhase.EndRound:
		case EScenarioModifierTriggerPhase.EndTurn:
		case EScenarioModifierTriggerPhase.StartScenario:
			break;
		}
	}

	protected override void TickRepositionInternal()
	{
		if (_tickActor.Type == CActor.EType.Player || _tickActor.Type == CActor.EType.Ally)
		{
			UpdateSpecialRulesAndObjectives();
			if (_tickActorTile?.m_HexMap?.RoomName == "Room_3" || _tickActorTile?.m_Hex2Map?.RoomName == "Room_3")
			{
				_priestInLastRoom = true;
			}
		}
	}

	private void UpdateSpecialRulesAndObjectives()
	{
		Trace("Updating special rules and objectives UI...", null, 151);
		List<CMap> maps = ScenarioManager.CurrentScenarioState.Maps;
		if (maps.Count < 3)
		{
			Error("The scenario needs at least three rooms.", null, 155);
			return;
		}
		for (int i = 1; i < 3; i++)
		{
			if (!_objectives[i].IsActive && maps[i].Revealed)
			{
				_objectives[i].SetActivation(active: true);
				Trace("Reveal Objective '" + _objectives[i].EventIdentifier + "'", null, 162);
				_objectives[i].CheckObjectiveComplete(ScenarioManager.CurrentScenarioState.Players.Count);
			}
		}
		if (_priestInfo.Deactivated && maps[1].Revealed)
		{
			_priestInfo.SetDeactivated(deactivate: false);
		}
	}

	private void TickAlgoxPriest(CActor algoxPriest)
	{
		ActorState actorState = null;
		foreach (EnemyState allyMonster in ScenarioManager.CurrentScenarioState.AllyMonsters)
		{
			if (allyMonster.ActorGuid == algoxPriest.ActorGuid)
			{
				actorState = allyMonster;
				break;
			}
		}
		if (actorState.IsDead)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (EnemyState allEnemyState in ScenarioManager.CurrentScenarioState.AllEnemyStates)
		{
			CActor actor = allEnemyState.Actor;
			if (actor == null)
			{
				Trace("Zero actor on " + allEnemyState.ClassID + " " + allEnemyState.ActorGuid + ".", null, 204);
			}
			else if (allEnemyState.Actor.IsEnemyMonsterType)
			{
				if (allEnemyState.IsRevealed)
				{
					num2++;
				}
				bool flag = !allEnemyState.IsDead && !actor.IsDead;
				Trace($"Is alive {actor.Class.ID} {actor.ActorGuid}: {flag}", null, 220);
				if (flag)
				{
					num++;
				}
				if (allEnemyState.IsRevealed & flag)
				{
					num3++;
				}
			}
		}
		actorState.AIFocusOverride.IsDisabled = num3 > 0;
		Trace($"Algox Priest has {num2} revealed enemies", null, 233);
		Trace($"Algox Priest has {num} alive enemies", null, 234);
		Trace($"Algox Priest has {num3} revealed alive enemies", null, 235);
		Trace($"Algox Priest focus on door: {!actorState.AIFocusOverride.IsDisabled}", null, 236);
	}

	public override string GetOverrideCharacterModelPrefabName(CActor actor)
	{
		string prefabName = actor.GetPrefabName();
		if (prefabName == "AlgoxPriest")
		{
			return "AlgoxPriestSnowspeaker";
		}
		if (prefabName == "AlgoxPriest Elite")
		{
			return "AlgoxPriestSnowspeaker Elite";
		}
		return null;
	}
}
