using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.CustomLevels;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH020 : ScenarioScript
{
	private CSpawner _bladespinnerSpawner;

	private List<CSpawner> _steelAutomatonsSpawners;

	private CObjectProp _glowingOrb;

	private Dictionary<CObjectProp, List<CObjectProp>> _cabinetsConduitsDict;

	private Dictionary<CObjectProp, CSpawner> _cabinetsSpawnerDict;

	private CObjective destroyOrbObjective;

	private CObjective destroyPillarsObjective;

	private CScenarioModifier _specialRule2;

	private CScenarioModifier _specialRule3;

	private bool IsCharacter => _tickActor.Type == CActor.EType.Player;

	private bool IsSummon => _tickActor.Type == CActor.EType.HeroSummon;

	private bool IsRuinedMachine
	{
		get
		{
			if (!_tickActor.Class.ID.Equals("RuinedMachineID"))
			{
				return _tickActor.Class.ID.Equals("RuinedMachineEliteID");
			}
			return true;
		}
	}

	private bool IsProperActor
	{
		get
		{
			if (IsCharacter || IsSummon || IsRuinedMachine)
			{
				return !_tickActor.IsDead;
			}
			return false;
		}
	}

	private bool IsOddRound => ScenarioScript.RoundNumber % 2 != 0;

	protected override bool Initialize()
	{
		List<CSpawner> spawners = ScenarioManager.CurrentScenarioState.Spawners;
		_bladespinnerSpawner = spawners[4];
		_steelAutomatonsSpawners = new List<CSpawner>
		{
			spawners[5],
			spawners[6]
		};
		_glowingOrb = GetProp(8, 7, "GlowingOrb");
		_specialRule2 = GetScenarioModifier("SpecialRule2");
		_specialRule3 = GetScenarioModifier("SpecialRule3");
		_specialRule2.CustomLocKeyArgs = new List<object> { ScenarioManager.TrapDamage };
		_specialRule3.CustomLocKeyArgs = new List<object> { ScenarioManager.TrapDamage };
		destroyOrbObjective = GetObjective("DestroyOrb");
		destroyPillarsObjective = GetObjective("DestroyPillars");
		return true;
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		List<CSpawner> spawners = ScenarioManager.CurrentScenarioState.Spawners;
		_cabinetsConduitsDict = new Dictionary<CObjectProp, List<CObjectProp>>();
		_cabinetsSpawnerDict = new Dictionary<CObjectProp, CSpawner>();
		List<CObjectProp> list = new List<CObjectProp>
		{
			GetProp(3, 2, "OneHexObstacle"),
			GetProp(3, 12, "OneHexObstacle"),
			GetProp(14, 12, "OneHexObstacle"),
			GetProp(14, 2, "OneHexObstacle"),
			GetProp(10, 9, "TerrainRubble"),
			GetProp(11, 10, "TerrainRubble"),
			GetProp(6, 9, "TerrainRubble"),
			GetProp(6, 10, "TerrainRubble"),
			GetProp(6, 5, "TerrainRubble"),
			GetProp(6, 4, "TerrainRubble"),
			GetProp(10, 5, "TerrainRubble"),
			GetProp(11, 4, "TerrainRubble"),
			GetProp(8, 7, "GlowingOrb")
		};
		InitializeCabinet(list[0], list[8], list[9], spawners[3]);
		InitializeCabinet(list[1], list[6], list[7], spawners[0]);
		InitializeCabinet(list[2], list[4], list[5], spawners[2]);
		InitializeCabinet(list[3], list[10], list[11], spawners[1]);
		ToggleGlowingOrbImmuneToDamage(_cabinetsSpawnerDict.Count != 0);
		UpdateObjectivesVisibility();
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartRound:
			TickPhaseStartRound();
			break;
		case EScenarioModifierTriggerPhase.EndRound:
			TickPhaseEndRound();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 94);
			break;
		case EScenarioModifierTriggerPhase.StartTurn:
		case EScenarioModifierTriggerPhase.EndTurn:
		case EScenarioModifierTriggerPhase.StartScenario:
			break;
		}
	}

	private void TickPhaseStartRound()
	{
		if (ScenarioScript.RoundNumber <= 1 || !IsOddRound || !IsProperActor)
		{
			return;
		}
		foreach (KeyValuePair<CObjectProp, List<CObjectProp>> item in _cabinetsConduitsDict)
		{
			CObjectProp key = item.Key;
			if (DealDamageIfAdjacentOrOccupy(key))
			{
				continue;
			}
			foreach (CObjectProp item2 in item.Value)
			{
				if (DealDamageIfAdjacentOrOccupy(item2, isConduit: true))
				{
					break;
				}
			}
		}
	}

	private void TickPhaseEndRound()
	{
		HandleDestroyedCabinets();
	}

	protected override void TickAbilityEndedInternal()
	{
		HandleDestroyedCabinets();
		UpdateObjectivesVisibility();
		UpdateSpecialRulesVisibility();
	}

	protected override void TickRepositionInternal()
	{
		UpdateObjectivesVisibility();
		UpdateSpecialRulesVisibility();
	}

	private void InitializeCabinet(CObjectProp cabinet, CObjectProp rubble1, CObjectProp rubble2, CSpawner spawner)
	{
		if (cabinet != null)
		{
			_cabinetsConduitsDict.Add(cabinet, new List<CObjectProp> { rubble1, rubble2 });
			_cabinetsSpawnerDict.Add(cabinet, spawner);
		}
		else
		{
			TurnOffRubbleDamageVisuals(rubble1);
			TurnOffRubbleDamageVisuals(rubble2);
		}
	}

	private void UpdateObjectivesVisibility()
	{
		destroyOrbObjective.SetActivation(_cabinetsSpawnerDict.Count == 0);
		destroyPillarsObjective.SetActivation(_cabinetsSpawnerDict.Count != 0);
	}

	private void UpdateSpecialRulesVisibility()
	{
		if (_cabinetsSpawnerDict.Count != 0)
		{
			return;
		}
		foreach (CScenarioModifier scenarioModifier in ScenarioManager.CurrentScenarioState.ScenarioModifiers)
		{
			scenarioModifier.SetHiddenState(shouldSetHidden: true);
		}
	}

	private void HandleDestroyedCabinets()
	{
		foreach (CObjectProp item in _cabinetsConduitsDict.Keys.ToList())
		{
			if (item.PropHealthDetails.CurrentHealth <= 0)
			{
				HandleDestroyedCabinet(item);
			}
		}
	}

	private void HandleDestroyedCabinet(CObjectProp cabinet)
	{
		SpawnEnemiesAtOrbAfterDestroyedCabinet();
		if (_cabinetsConduitsDict.TryGetValue(cabinet, out var value))
		{
			foreach (CObjectProp item in value)
			{
				TurnOffRubbleDamageVisuals(item);
			}
		}
		_cabinetsConduitsDict.Remove(cabinet);
		_cabinetsSpawnerDict[cabinet].SetActive(active: false);
		_cabinetsSpawnerDict.Remove(cabinet);
		if (_cabinetsSpawnerDict.Count == 0)
		{
			ToggleGlowingOrbImmuneToDamage(isImmune: false);
		}
	}

	private void TurnOffRubbleDamageVisuals(CObjectProp rubble)
	{
		CChangePropVisualState_MessageData cChangePropVisualState_MessageData = new CChangePropVisualState_MessageData();
		cChangePropVisualState_MessageData.m_prop = rubble;
		cChangePropVisualState_MessageData.m_stateName = "StateOff";
		cChangePropVisualState_MessageData.m_delay = 0.001f;
		ScenarioRuleClient.MessageHandler(cChangePropVisualState_MessageData);
	}

	private void SpawnEnemiesAtOrbAfterDestroyedCabinet()
	{
		if (_cabinetsSpawnerDict.Count == 1)
		{
			foreach (CSpawner steelAutomatonsSpawner in _steelAutomatonsSpawners)
			{
				steelAutomatonsSpawner.SpawnUnit(ScenarioManager.CurrentScenarioState.Players.Count, treatAsSummon: false, initial: false, startRound: false, forceUseAnyEntry: false, addIntoInitiativeOrder: true);
			}
			return;
		}
		_bladespinnerSpawner.SetLocation(_glowingOrb.ArrayIndex);
		_bladespinnerSpawner.SpawnUnit(ScenarioManager.CurrentScenarioState.Players.Count, treatAsSummon: false, initial: false, startRound: false, forceUseAnyEntry: false, addIntoInitiativeOrder: true);
	}

	private bool DealDamageIfAdjacentOrOccupy(CObjectProp prop, bool isConduit = false)
	{
		if (prop.IsDestroyed)
		{
			return false;
		}
		if (ScenarioManager.IsTileAdjacent(_tickActorTile.m_ArrayIndex.X, _tickActorTile.m_ArrayIndex.Y, prop.ArrayIndex.X, prop.ArrayIndex.Y) || (_tickActorTile.m_ArrayIndex.X == prop.ArrayIndex.X && _tickActorTile.m_ArrayIndex.Y == prop.ArrayIndex.Y))
		{
			ScenarioRuleClient.MessageHandler(new CTryPlayVfxOnTile_MessageData(null)
			{
				m_VFXPropType = (isConduit ? "ElectricityBurst" : "ElectricityBurstPillar"),
				m_ArrayIndex = prop.ArrayIndex.ToPoint()
			});
			string damageSourceLocKey = (prop.PropHealthDetails.HasHealth ? prop.PropHealthDetails.CustomLocKey : prop.PropCustomLocKey);
			DealScenarioDamage(_tickActor, ScenarioManager.TrapDamage, isTrapDamage: false, isTerrainDamage: false, damageSourceLocKey);
			return true;
		}
		return false;
	}

	private void ToggleGlowingOrbImmuneToDamage(bool isImmune)
	{
		_glowingOrb.IsImmuneToDamage = isImmune;
		CSetActiveActorHealthBar_MessageData message = new CSetActiveActorHealthBar_MessageData(_glowingOrb.RuntimeAttachedActor, !isImmune);
		ScenarioRuleClient.MessageHandler(message);
		ScenarioRuleClient.MessageHandler(new HideShowChildObject_MessageData(_glowingOrb.RuntimeAttachedActor)
		{
			m_GOName = "Shield",
			m_Show = isImmune
		});
	}

	protected override bool AllowTriggerMessageInternal(SEvent ev, CLevelMessage message)
	{
		if (!(ev is SEventActor sEventActor))
		{
			return false;
		}
		if (sEventActor.ActorSubType != ESESubTypeActor.ActorOnDeath)
		{
			return false;
		}
		Trace("Checking message trigger for " + message.MessageName + " " + message.TitleKey + "...", null, 286);
		int num = 0;
		foreach (ObjectState @object in ScenarioManager.CurrentScenarioState.Objects)
		{
			if (@object.MaxHealth > 0)
			{
				Trace($"{@object.ClassID} hlt: {@object.MaxHealth} is dead: " + IsDead(@object), null, 290);
				num += (IsDead(@object) ? 1 : 0);
			}
		}
		if (num >= 4)
		{
			Trace("...success", null, 296);
			return true;
		}
		Trace("...failed", null, 300);
		return false;
	}
}
