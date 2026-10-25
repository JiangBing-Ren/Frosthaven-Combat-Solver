using System.Collections.Generic;
using System.Linq;
using AStar;
using Libs.GameConsole;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH043 : ScenarioScript
{
	[ConsoleVariable(Alias = "fh043_debug_party_size")]
	private static int DebugPartySize_cvar;

	private CSpawner _spawnerA;

	private CScenarioModifier _specialRule1;

	private CScenarioModifier _specialRule2;

	private CMap _lastRoom;

	private int _revealEscapeRoomRound;

	private bool vfxPlayedThisTurn;

	private int SplashAbilityStrength => ScenarioManager.CurrentScenarioState.Level + 3;

	private int RangedAbilityStrength => ScenarioManager.CurrentScenarioState.Level + 5;

	private int _partySize
	{
		get
		{
			if (DebugPartySize_cvar <= 1)
			{
				return ScenarioManager.CurrentScenarioState.Players.Count;
			}
			return DebugPartySize_cvar;
		}
	}

	protected override bool Initialize()
	{
		_lastRoom = ScenarioScript.FindRoom("Room_11");
		_spawnerA = ScenarioManager.CurrentScenarioState.Spawners[0];
		_specialRule1 = GetScenarioModifier("specialRule1");
		_specialRule1.CustomLocKeyArgs = new List<object> { SplashAbilityStrength, RangedAbilityStrength };
		_specialRule2 = GetScenarioModifier("specialRule2");
		return true;
	}

	protected override string SerializeInternal()
	{
		string text = JsonConvert.SerializeObject(_revealEscapeRoomRound);
		Trace("[ffa000]Serialize: " + text + "[-]", null, 46);
		return text;
	}

	protected override void DeserializeInternal(string s)
	{
		Trace("[ffa000]Deserialize " + s + "[-]", null, 51);
		if (!string.IsNullOrEmpty(s))
		{
			_revealEscapeRoomRound = JsonConvert.DeserializeObject<int>(s);
			CheckObjectivesComplete();
		}
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.EndTurn:
			TickEndTurn();
			break;
		case EScenarioModifierTriggerPhase.StartScenario:
			if (_tickActor is CPlayerActor playerActor)
			{
				ApplyAbility(playerActor, "SplashAbility", SplashAbilityStrength);
				ApplyAbility(playerActor, "RangedAbility", RangedAbilityStrength);
			}
			break;
		case EScenarioModifierTriggerPhase.StartRound:
			if (OncePerRound() && _lastRoom.Revealed)
			{
				if (_revealEscapeRoomRound == 0)
				{
					_revealEscapeRoomRound = ScenarioScript._css.RoundNumber;
				}
				UpdateEnemiesSpawner();
			}
			break;
		default:
			Error($"Unhandled phase {scenarioModifierTriggerPhase}", null, 90);
			break;
		case EScenarioModifierTriggerPhase.OnActorSpawnedOrRevealed:
			break;
		}
	}

	private void ApplyAbility(CPlayerActor playerActor, string eventIdentifier, int strength)
	{
		GetScenarioModifier(eventIdentifier).PerformScenarioModifier(ScenarioScript.RoundNumber, playerActor);
	}

	protected override void OnLongRestCompleteInternal()
	{
		refill("43_Splash");
		refill("43_Ranged");
		void refill(string scenarioAbilityPartialName)
		{
			if (GetOptionalAttacks(_tickActor).Count < 2)
			{
				PerformScenarioModifier(scenarioAbilityPartialName, _tickActor);
			}
		}
	}

	protected override void OnFinishedRevealingRoom()
	{
		base.OnFinishedRevealingRoom();
		if (_tickRoomRevealed != _lastRoom)
		{
			return;
		}
		CScenarioModifier specialRule = _specialRule2;
		if (specialRule != null)
		{
			specialRule.SetHiddenState(shouldSetHidden: false);
			if (!_spawnerA.HasIndicator)
			{
				_spawnerA.UpdateWillSpawnNextRoundIndicator(_partySize);
			}
			Trace("Start spawning enemies on last room reveal.", null, 125);
		}
	}

	private List<CActiveBonus> GetOptionalAttacks(CActor actor)
	{
		List<CActiveBonus> list = new List<CActiveBonus>();
		foreach (CScenarioModifier scenarioModifier in ScenarioScript._css.ScenarioModifiers)
		{
			list.AddRange(scenarioModifier.ActiveBonuses.Where((CActiveBonus a) => a.Actor == actor && a.Ability.ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.DuringTurnAbility)));
		}
		return list;
	}

	private void TickEndTurn()
	{
		vfxPlayedThisTurn = false;
		if (_tickActor.Type != CActor.EType.Player || TryEscapeMap())
		{
			return;
		}
		List<CActiveBonus> optionalAttacks = GetOptionalAttacks(_tickActor);
		if (optionalAttacks.Count >= 2)
		{
			return;
		}
		foreach (CActiveBonus item in optionalAttacks)
		{
			CClass.CancelActiveBonus(item);
		}
	}

	public override void OnPerformingAbility(CAbility ability)
	{
		if (ability != null && !vfxPlayedThisTurn)
		{
			vfxPlayedThisTurn = true;
			string text = null;
			Point arrayIndex = default;
			if ((ability.Flags & CAbility.EAbilityFlag.Special1) != 0)
			{
				text = "EMPWave1HEX";
				arrayIndex = ability.TilesSelected[0].m_ArrayIndex;
			}
			else if ((ability.Flags & CAbility.EAbilityFlag.Special2) != 0)
			{
				text = "EMPWave2HEX";
				arrayIndex = ability.TargetingActor.ArrayIndex;
			}
			if (!string.IsNullOrEmpty(text))
			{
				ScenarioRuleClient.MessageHandler(new CTryPlayVfxOnTile_MessageData(null)
				{
					m_VFXPropType = text,
					m_ArrayIndex = arrayIndex
				});
			}
		}
	}

	private void UpdateEnemiesSpawner()
	{
		Trace("Trying to update enemies spawner...", null, 198);
		CObjective_ActorsEscaped cObjective_ActorsEscaped = null;
		foreach (CObjective winObjective in ScenarioScript._css.WinObjectives)
		{
			if (winObjective is CObjective_ActorsEscaped cObjective_ActorsEscaped2)
			{
				cObjective_ActorsEscaped = cObjective_ActorsEscaped2;
				break;
			}
		}
		if (cObjective_ActorsEscaped == null)
		{
			Error($"{this} needs an ActorsEscaped objective.", null, 209);
			return;
		}
		int index = ScenarioScript._css.ScenarioRNG.Next(cObjective_ActorsEscaped.EscapePositions.Count);
		TileIndex location = cObjective_ActorsEscaped.EscapePositions[index];
		_spawnerA.HideSpawnerProp();
		_spawnerA.SpawnUnit(_partySize, treatAsSummon: false, initial: false, startRound: true);
		_spawnerA.SetLocation(location);
		_spawnerA.UpdateWillSpawnNextRoundIndicator(_partySize);
	}

	public override IEnumerable<(string, string)> GetSpecialRulesParamsString()
	{
		yield return ("damage_melee", SplashAbilityStrength.ToString());
		yield return ("damage_ranged", RangedAbilityStrength.ToString());
	}
}
