using System.Collections.Generic;
using AStar;
using Libs.GameConsole;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH048 : ScenarioScript
{
	public static int debugNextDirectionIndex = -1;

	private string demonClassID = "WindDemonID";

	private string eliteDemonClassID = "WindDemonEliteID";

	private CScenarioModifierMoveActorsInDirections pushScenarioModifier;

	private readonly List<Point> windLocations = new List<Point>
	{
		new Point(15, 10),
		new Point(9, 6),
		new Point(3, 4)
	};

	[StateDump(Skip = true)]
	private bool cardDrawnThisRound;

	protected override bool Initialize()
	{
		pushScenarioModifier = GetScenarioModifier("PushCharactersModifier") as CScenarioModifierMoveActorsInDirections;
		return base.Initialize();
	}

	private CEnemyActor GetAliveDemon()
	{
		return ScenarioManager.Scenario.Enemies.Find((CEnemyActor c) => c.MonsterClass?.ID == demonClassID || c.MonsterClass?.ID == eliteDemonClassID);
	}

	private CMonsterClass GetRawDemonClass()
	{
		return MonsterClassManager.Classes.Find((CMonsterClass c) => c.ID == demonClassID);
	}

	private int GetDirectionIndex()
	{
		if (debugNextDirectionIndex != -1)
		{
			int result = debugNextDirectionIndex;
			debugNextDirectionIndex = -1;
			return result;
		}
		CMonsterClass cMonsterClass = GetAliveDemon()?.MonsterClass;
		if (cMonsterClass == null)
		{
			cMonsterClass = GetRawDemonClass();
		}
		return cMonsterClass.RoundAbilityCard.Initiative switch
		{
			2 => 5, 
			9 => 4, 
			21 => 0, 
			29 => 1, 
			37 => 2, 
			43 => 3, 
			_ => 0, 
		};
	}

	private void PlayWindVFX(ScenarioManager.EAdjacentPosition direction)
	{
		int num = 0;
		int num2 = 0;
		switch (direction)
		{
		case ScenarioManager.EAdjacentPosition.ELeft:
			num--;
			break;
		case ScenarioManager.EAdjacentPosition.ERight:
			num++;
			break;
		case ScenarioManager.EAdjacentPosition.ETopLeft:
			num2++;
			break;
		case ScenarioManager.EAdjacentPosition.ETopRight:
			num++;
			num2++;
			break;
		case ScenarioManager.EAdjacentPosition.EBottomLeft:
			num2--;
			break;
		case ScenarioManager.EAdjacentPosition.EBottomRight:
			num++;
			num2--;
			break;
		}
		if ((num2 & 1) == 0)
		{
			_ = ScenarioManager.Width;
		}
		else
		{
			_ = ScenarioManager.Width;
		}
		if (num2 != 0)
		{
			num = (((num2 & 1) != 0) ? (num - 1) : num);
		}
		foreach (Point windLocation in windLocations)
		{
			ScenarioRuleClient.MessageHandler(new CTryPlayVfxOnTile_MessageData(null)
			{
				m_VFXPropType = "PolarWind",
				m_ArrayIndex = windLocation,
				m_faceTowards = new Point(windLocation.X + num, windLocation.Y + num2)
			});
		}
	}

	private void MoveActors()
	{
		if (_tickActor.Type == CActor.EType.Player)
		{
			int directionIndex = GetDirectionIndex();
			if (OncePerRound())
			{
				PlayWindVFX(pushScenarioModifier.MoveDirections[directionIndex]);
			}
			pushScenarioModifier.NextMoveDirectionIndex = directionIndex;
			pushScenarioModifier.PerformScenarioModifier(ScenarioScript.RoundNumber, _tickActor, ScenarioScript.PartySize, forceActivate: true);
		}
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		if (scenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.AfterSelectMonsterClassAbilityCards)
		{
			MoveActors();
			cardDrawnThisRound = false;
		}
		if (scenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.StartRound)
		{
			TickStartRound();
		}
	}

	private bool HasAliveDemons()
	{
		return GetAliveDemon() != null;
	}

	private void TickStartRound()
	{
		if (!HasAliveDemons() && !cardDrawnThisRound)
		{
			GetRawDemonClass().SelectRoundAbilityCard();
			cardDrawnThisRound = true;
		}
	}

	public override Dictionary<string, List<CEnemyActor>> GetExtraInitiativeActors()
	{
		if (HasAliveDemons())
		{
			return null;
		}
		return new Dictionary<string, List<CEnemyActor>> { 
		{
			demonClassID,
			new List<CEnemyActor>()
		} };
	}

	[ConsoleCommand(Command = "ss048_debug_next_wind_direction_index")]
	public static void SS048SetNextWindDirectionIndex(IConsole console, int index)
	{
		if (ScenarioManager.CurrentScenarioState.FindScenarioScript("ScenarioScript_FH048", out var script) && script is ScenarioScript_FH048)
		{
			debugNextDirectionIndex = index;
		}
	}
}
