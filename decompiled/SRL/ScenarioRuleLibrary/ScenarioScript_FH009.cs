using System.Linq;
using AStar;
using Libs.GameConsole;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH009 : ScenarioScript
{
	public struct Lever
	{
		public CObjectObstacle prop;

		public CSpawner spawner;

		public CObjectActor actor => prop.RuntimeAttachedActor;
	}

	[ConsoleVariable(Alias = "ss009_debug_lever_state")]
	public static int DebugLeverState_cvar = 0;

	private static readonly string OozeClassId = "OozeID";

	private static readonly string OozeEliteClassId = "OozeEliteID";

	public Lever[] levers = new Lever[2];

	private CMap _lastRoom;

	private CScenarioModifier _specialRule;

	private CObjective _objective;

	protected override bool Initialize()
	{
		Point[] doorPoints = new Point[3]
		{
			new Point(16, 13),
			new Point(17, 14),
			new Point(17, 15)
		};
		CObjectDoor[] array = new CObjectDoor[3];
		int i;
		for (i = 0; i < 3; i++)
		{
			array[i] = (CObjectDoor)ScenarioManager.CurrentScenarioState.Props.FirstOrDefault((CObjectProp p) => p.ArrayIndex.ToPoint() == doorPoints[i]);
			if (array[i] == null)
			{
				Error("Needs three twin doors.", null, 49);
				return false;
			}
			array[i].TwinDoors = new CObjectDoor[2];
		}
		for (int num = 0; num < 3; num++)
		{
			int num2 = (num + 1) % 3;
			int num3 = (num + 2) % 3;
			array[num].TwinDoors[0] = array[num2];
			array[num].TwinDoors[1] = array[num3];
			Trace($"Twin doors on door {num}: {num2} {num3}", null, 61);
		}
		_lastRoom = ScenarioManager.Tiles[20, 10]?.m_HexMap;
		if (_lastRoom == null)
		{
			Error("Broken last room.", null, 67);
			return false;
		}
		_specialRule = GetScenarioModifier("lastRoom");
		if (_specialRule == null)
		{
			Error("Need scenario modifier named lastRoom with special rule in it.", null, 73);
			return false;
		}
		_specialRule.SetHiddenState(shouldSetHidden: true);
		_objective = GetObjective("lastRoom");
		if (_objective == null)
		{
			Error("Need objective named lastRoom.", null, 80);
			return false;
		}
		return true;
	}

	protected override void TickPhaseInternal()
	{
		if (_scenarioModifier.ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.StartRound)
		{
			for (int i = 0; i < levers.Length; i++)
			{
				Lever lever = levers[i];
				if (lever.actor == _tickActor && _lastRoom.Revealed && lever.actor != null && !lever.actor.IsDead)
				{
					lever.spawner.SpawnUnit(ScenarioScript.PartySize, treatAsSummon: false, initial: false, startRound: true);
					lever.spawner.UpdateWillSpawnNextRoundIndicator(ScenarioScript.PartySize);
					break;
				}
			}
		}
		UpdateSpecialRule();
		if (OncePerRound())
		{
			UpdateLeverDeath();
		}
	}

	protected override void TickRepositionInternal()
	{
		UpdateSpecialRule();
	}

	protected override void TickDeathInternal()
	{
		UpdateLeverDeath();
	}

	protected override void TickAbilityEndedInternal()
	{
		UpdateLeverDeath();
	}

	protected override void DeserializeInternal(string s)
	{
		levers[0].prop = GetProp(20, 10, "OneHexObstacle") as CObjectObstacle;
		levers[1].prop = GetProp(21, 13, "OneHexObstacle") as CObjectObstacle;
		UpdateSpawner(0, 20, 10);
		UpdateSpawner(1, 21, 13);
		UpdateSpecialRule();
		void UpdateSpawner(int leverIdx, int locationX, int locationY)
		{
			if (levers[leverIdx].actor == null || !levers[leverIdx].actor.IsDead)
			{
				CSpawner cSpawner = ScenarioScript._css.Spawners[leverIdx];
				levers[leverIdx].spawner = cSpawner;
				cSpawner.SetLocation(new TileIndex(locationX, locationY));
			}
		}
	}

	private void UpdateSpecialRule()
	{
		if (!_lastRoom.Revealed || !_specialRule.IsHidden)
		{
			return;
		}
		_specialRule.SetHiddenState(shouldSetHidden: false);
		_objective.SetActivation(active: true);
		Trace("Reveal the special rule.", null, 153);
		Lever[] array = levers;
		for (int i = 0; i < array.Length; i++)
		{
			Lever lever = array[i];
			if (lever.actor == null || !lever.actor.IsDead)
			{
				lever.spawner?.UpdateWillSpawnNextRoundIndicator(ScenarioScript.PartySize);
			}
		}
	}

	private void UpdateLeverDeath()
	{
		for (int i = 0; i < levers.Length; i++)
		{
			Lever lever = levers[i];
			if (lever.actor != null && lever.actor.IsDead)
			{
				CNode node = ScenarioManager.PathFinder.GetNode(lever.prop.ArrayIndex);
				node.Walkable = false;
				node.Blocked = true;
			}
			if (lever.spawner != null && lever.spawner.Prop != null && (lever.actor == null || lever.actor.IsDead))
			{
				lever.spawner.HideSpawnerProp();
				lever.spawner = null;
			}
		}
	}
}
