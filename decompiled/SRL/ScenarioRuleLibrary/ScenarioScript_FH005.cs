namespace ScenarioRuleLibrary;

public class ScenarioScript_FH005 : ScenarioScript
{
	private CScenarioModifier _specialRule1;

	private CScenarioModifier _specialRule2;

	private CObjectDoor _door;

	public bool IsCorpse(CActor actor)
	{
		if (actor.Class.ID.ToLowerInvariant().Contains("corpse"))
		{
			return actor is CEnemyActor;
		}
		return false;
	}

	public bool IsAltar(CActor actor)
	{
		return actor.Class.ID.ToLowerInvariant().Contains("propdummy");
	}

	protected override bool Initialize()
	{
		_specialRule1 = GetScenarioModifier("rule1");
		_specialRule2 = GetScenarioModifier("rule2");
		if (_specialRule1 == null || _specialRule2 == null)
		{
			Error("Missing scenario modifiers for script", null, 25);
			return false;
		}
		_door = GetPropPartName(4, 9, "door") as CObjectDoor;
		if (_door == null)
		{
			Error("Missing door prop for scenario script", null, 31);
			return false;
		}
		foreach (CObjectActor @object in ScenarioManager.Scenario.Objects)
		{
			IsAltarImmuneAll(@object);
		}
		return true;
	}

	protected override void TickPhaseInternal()
	{
		switch (_scenarioModifier.ScenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.OnActorSpawnedOrRevealed:
			Trace("Spawned " + _tickActor.Class.ID, null, 44);
			if (IsAltarImmuneAll(_tickActor) || !IsCorpse(_tickActor) || (_tickActor as CEnemyActor).IsImmuneToDamage)
			{
				break;
			}
			{
				foreach (CObjectActor @object in ScenarioManager.Scenario.Objects)
				{
					if (IsAltar(@object))
					{
						Trace("[ffc000]Found alive altar " + @object.Class.ID + "[-]", null, 50);
						Immune(_tickActor as CEnemyActor);
						break;
					}
				}
				break;
			}
		case EScenarioModifierTriggerPhase.StartScenario:
			Trace("Hide scenario special rules", null, 60);
			_specialRule1.SetHiddenState(shouldSetHidden: true);
			_specialRule2.SetHiddenState(shouldSetHidden: true);
			break;
		}
	}

	private void Immune(CEnemyActor m)
	{
		m.IsImmuneToDamage = true;
		Trace("[ffc000]" + m.Class.ID + " immuned to damage[-]", null, 68);
		RefreshVFX(m);
	}

	private void RefreshVFX(CEnemyActor m)
	{
		if (m != null)
		{
			string vFXName = "AmuletReanimationVFX";
			ScenarioRuleClient.MessageHandler(new CRemoveCasterVFX_MessageData(m)
			{
				m_VFXName = vFXName
			});
			if (m.IsImmuneToDamage)
			{
				ScenarioRuleClient.MessageHandler(new CAddCasterVFX_MessageData(m)
				{
					m_VFXName = vFXName
				});
			}
		}
	}

	private bool IsAltarImmuneAll(CActor obj)
	{
		if (IsAltar(obj))
		{
			Trace("[ffc000]Found alive altar " + obj.Class.ID + ", make corpses undying[-]", null, 93);
			foreach (CEnemyActor allAliveMonster in ScenarioManager.Scenario.AllAliveMonsters)
			{
				if (IsCorpse(allAliveMonster) && !allAliveMonster.IsImmuneToDamage)
				{
					Immune(allAliveMonster);
				}
			}
			return true;
		}
		return false;
	}

	protected override void TickDeathInternal()
	{
		if (!IsAltar(_tickActor))
		{
			return;
		}
		foreach (CEnemyActor allAliveMonster in ScenarioManager.Scenario.AllAliveMonsters)
		{
			if (IsCorpse(allAliveMonster))
			{
				allAliveMonster.IsImmuneToDamage = false;
				Trace("[ffc000]" + allAliveMonster.Class.ID + ": removed immune to damage[-]", null, 109);
				RefreshVFX(allAliveMonster);
			}
		}
		_specialRule1.SetDeactivated(deactivate: true);
	}

	protected override void TickRepositionInternal()
	{
		UpdateSpecialRuleVisibility();
	}

	protected override void TickAbilityEndedInternal()
	{
		UpdateSpecialRuleVisibility();
	}

	private void UpdateSpecialRuleVisibility()
	{
		if (_door.DoorIsOpen)
		{
			if (_specialRule1.IsHidden)
			{
				Trace("Unhiding special rules 1", null, 128);
				_specialRule1.SetHiddenState(shouldSetHidden: false);
			}
			if (_specialRule2.IsHidden)
			{
				Trace("Unhiding special rules 2", null, 132);
				_specialRule2.SetHiddenState(shouldSetHidden: false);
			}
		}
	}

	protected override void DeserializeInternal(string storage)
	{
		foreach (EnemyState monster in ScenarioScript._css.Monsters)
		{
			RefreshVFX((CEnemyActor)monster.Actor);
		}
	}

	public override void OnSpawnerHasSpawned(CSpawner spawner, CActor actor)
	{
		_specialRule2.SetDeactivated(deactivate: true);
	}
}
