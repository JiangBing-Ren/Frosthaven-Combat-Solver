using System.Collections.Generic;
using System.Linq;
using Libs.Utils;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH107 : ScenarioScript
{
	private CObjective[] _objectives;

	private CObjective_ActorReachPosition _pressurePlateObjective;

	protected override bool Initialize()
	{
		_objectives = new CObjective[5]
		{
			GetObjective("Episode1"),
			GetObjective("Episode2"),
			GetObjective("Episode3"),
			GetObjective("Episode4"),
			GetObjective("Episode5")
		};
		_pressurePlateObjective = (CObjective_ActorReachPosition)GetObjective("Press Pressure Plate");
		foreach (CObjectPressurePlate item in ScenarioManager.CurrentScenarioState.Props.OfType<CObjectPressurePlate>())
		{
			item.ActivatedByJump = true;
			item.ActivatedByFly = true;
		}
		return true;
	}

	private int CalcEpisodesPassed()
	{
		return _objectives.Count((CObjective x) => x?.IsComplete ?? false);
	}

	protected override List<object> GetConclusionParametersInternal()
	{
		int num = CalcEpisodesPassed();
		return new List<object> { num };
	}

	protected override int GetConclusionIndexInternal()
	{
		if (CalcEpisodesPassed() >= 5)
		{
			return 1;
		}
		return 0;
	}

	protected override void TickPhaseInternal()
	{
		if (_scenarioModifier.ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.EndRound)
		{
			TickEndRound();
		}
	}

	private void TickEndRound()
	{
		if (!OncePerRound())
		{
			return;
		}
		CObjective cObjective = _objectives.FirstOrDefault((CObjective o) => !o.IsComplete);
		if (cObjective == null || cObjective.IsActive || _pressurePlateObjective.IsActive)
		{
			return;
		}
		if (!GameState.ShowGenericConfirmButton(GameState.HostCharacter, "FH_Scenario_Campaign_107_SCENARIO_MODIFIER_3", "GUI_CONTINUE", null, "GUI_COMPLETE", showHeader: false))
		{
			ScenarioManager.CurrentScenarioState.WinObjectives.RemoveAll((CObjective o) => !o.IsComplete);
			return;
		}
		_pressurePlateObjective.SetActivation(active: true);
		CObjective[] objectives = _objectives;
		foreach (CObjective cObjective2 in objectives)
		{
			if (cObjective2.IsComplete)
			{
				cObjective2.SetActivation(active: false);
			}
		}
		int num2 = _objectives.IndexOf(cObjective);
		switch (num2)
		{
		case 1:
			((CObjectDoor)GetProp(8, 7)).Activate(GameState.HostCharacter);
			_pressurePlateObjective.ActorTargetPositions[0] = new TileIndex(8, 12);
			break;
		case 2:
			((CObjectDoor)GetProp(8, 17)).Activate(GameState.HostCharacter);
			_pressurePlateObjective.ActorTargetPositions[0] = new TileIndex(10, 21);
			break;
		case 3:
		{
			SEventLogMessageHandler.AddEventLogMessage(new SEvent(ESEType.CustomSEvent, "End 3"));
			CObjectPressurePlate reusedPressurePlate = ScenarioManager.CurrentScenarioState.Props.OfType<CObjectPressurePlate>().First((CObjectPressurePlate p) => p.ArrayIndex.X == 10 && p.ArrayIndex.Y == 21);
			List<CSpawner> list = (from s in ScenarioManager.CurrentScenarioState.Spawners
				where !s.IsActive
				where s.CurrentMapGuid == reusedPressurePlate.StartingMapGuid
				select s).ToList();
			foreach (string spawnerGUID in reusedPressurePlate.LinkedSpawnerGuids.ToList())
			{
				reusedPressurePlate.UnlinkSpawner(ScenarioManager.CurrentScenarioState.Spawners.First((CSpawner y) => y.SpawnerGuid == spawnerGUID));
			}
			foreach (CSpawner item in list)
			{
				reusedPressurePlate.LinkSpawner(item);
			}
			reusedPressurePlate.ResetLock();
			break;
		}
		case 4:
			((CObjectDoor)GetProp(14, 21)).Activate(GameState.HostCharacter);
			_pressurePlateObjective.ActorTargetPositions[0] = new TileIndex(16, 21);
			break;
		}
	}

	public override void TickPropActivated(CObjectProp prop)
	{
		base.TickPropActivated(prop);
		if (prop is CObjectPressurePlate && PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability is CAbilityMove { HasEnded: false, RemainingMoves: >0 } cAbilityMove)
		{
			cAbilityMove.ForceInterrupt();
		}
	}
}
