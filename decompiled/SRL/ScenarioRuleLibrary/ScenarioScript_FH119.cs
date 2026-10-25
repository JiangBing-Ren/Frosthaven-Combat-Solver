using System.Collections.Generic;
using System.Linq;
using AStar;
using Libs.GameConsole;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH119 : ScenarioScript
{
	private string infuserID = "TheInfuserID";

	private string resourceID = "RadiantDust";

	private string dustPrefabName = "RadiantDust";

	private Point goalPosition = new Point(2, 28);

	private CObjective_Scripted winObjective;

	private CScenarioModifier scenarioAbilityHolderInfuserEndTurn;

	private CScenarioModifier textNumDustsModifier;

	private CScenarioModifier textMoveModifier1;

	private CScenarioModifier textMoveModifier2;

	private CEnemyActor infuser;

	private CActor previouslyConvertedActor;

	private CActor currentlyConvertedActor;

	private int numDusts = 4;

	protected override string SerializeInternal()
	{
		string text = JsonConvert.SerializeObject(numDusts);
		string text2 = JsonConvert.SerializeObject(currentlyConvertedActor?.ActorGuid);
		string text3 = JsonConvert.SerializeObject(new string[2] { text, text2 });
		Trace("[ffa000]Serialize: " + text3 + "[-]", null, 34);
		return text3;
	}

	protected override void DeserializeInternal(string s)
	{
		Trace("[ffa000]Deserialize " + s + "[-]", null, 40);
		if (!string.IsNullOrEmpty(s))
		{
			string[] array = JsonConvert.DeserializeObject<string[]>(s);
			numDusts = JsonConvert.DeserializeObject<int>(array[0]);
			ModifyNumDusts(0);
			string currActorGuid = JsonConvert.DeserializeObject<string>(array[1]);
			currentlyConvertedActor = ScenarioManager.Scenario.AllActors.Find((CActor a) => a.ActorGuid == currActorGuid);
		}
	}

	protected override bool Initialize()
	{
		winObjective = (CObjective_Scripted)GetObjective("winObjective");
		scenarioAbilityHolderInfuserEndTurn = GetScenarioModifier("scenarioAbilityHolderInfuserEndTurn");
		textNumDustsModifier = GetScenarioModifier("textNumDustsModifier");
		textMoveModifier1 = GetScenarioModifier("moveTextModifer1");
		textMoveModifier2 = GetScenarioModifier("moveTextModifer2");
		infuser = ScenarioManager.Scenario.AllActors.Find((CActor a) => a?.MonsterClass?.ID == infuserID) as CEnemyActor;
		ScenarioManager.ToggleTileHighlights(new List<Point> { goalPosition }, enabled: true);
		ProcessSpawnedMonsters();
		ModifyNumDusts(0);
		return true;
	}

	protected override void TickPhaseInternal()
	{
		switch (_scenarioModifier.ScenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.OnActorSpawnedOrRevealed:
			ProcessSpawnedMonster(_tickActor as CEnemyActor);
			break;
		case EScenarioModifierTriggerPhase.EndTurnLoot:
			InfuserAndSummonsLoot();
			ProcessLootRadiantDust();
			break;
		case EScenarioModifierTriggerPhase.AfterAbility:
			SaveConvertedActors();
			HandleAfterLoot();
			break;
		}
	}

	private void InfuserAndSummonsLoot()
	{
		if ((_tickActor == infuser || _tickActor.Type == CActor.EType.HeroSummon) && _tickActor.Tile.m_Props.Any((CObjectProp x) => x is CObjectResource cObjectResource && cObjectResource.ResourceID == resourceID))
		{
			CAbility cAbility = CAbilityLoot.CreateDefaultLoot(0);
			cAbility.SetMiscVar("AutoLootTiles", new TValue(new List<CTile> { _tickActor.Tile }));
			if (_tickActor == infuser)
			{
				cAbility.SetMiscVar("ForcedFinalLooter", new TValue(GetFirstAlivePlayer()));
			}
			cAbility.TargetingActor = _tickActor;
			StartSpecialAbility(cAbility);
		}
	}

	private CActor GetFirstAlivePlayer()
	{
		return ScenarioScript._css.Players.Find((PlayerState p) => !p.IsDead).Actor;
	}

	private void StartSpecialAbility(CAbility ability)
	{
		ability.SetCanUndo(canUndo: false);
		if (PhaseManager.Phase is CPhaseAction cPhaseAction)
		{
			cPhaseAction.InjectAbility(ability, _tickActor, ability.GetAbilityBaseCard());
		}
		else
		{
			PhaseManager.StartAbility(ability, ability.GetAbilityBaseCard(), fullCopy: true);
		}
	}

	private void HandleAfterLoot()
	{
		if (_tickActor.LastAbilityPerformed is CAbilityLoot)
		{
			ProcessLootRadiantDust();
		}
	}

	private void SaveConvertedActors()
	{
		if (!(_tickActor.LastAbilityPerformed is CAbilityChangeAllegiance cAbilityChangeAllegiance) || cAbilityChangeAllegiance.TargetingActor != infuser)
		{
			return;
		}
		if (cAbilityChangeAllegiance.ChangeAllegianceData.ChangeToType == CActor.EType.Ally)
		{
			if (currentlyConvertedActor != null)
			{
				previouslyConvertedActor = currentlyConvertedActor;
			}
			currentlyConvertedActor = cAbilityChangeAllegiance.ActorsToTarget[0];
		}
		else if (cAbilityChangeAllegiance.ChangeAllegianceData.ChangeToType == CActor.EType.Enemy)
		{
			previouslyConvertedActor = null;
		}
	}

	protected override void OnDoorOpenedInternal(CObjectDoor door, CActor actor, CActor creditActor, bool visualOnly)
	{
		if (door.PropTile.m_ArrayIndex == new Point(6, 19))
		{
			textMoveModifier1.SetHiddenState(shouldSetHidden: true);
			textMoveModifier2.SetHiddenState(shouldSetHidden: false);
		}
		base.OnDoorOpenedInternal(door, actor, creditActor, visualOnly);
	}

	protected override void TickAbilityStartedInternal()
	{
		if (_tickAbility.AbilityType == CAbility.EAbilityType.Move && _tickActor == infuser)
		{
			CAbilityMove cAbilityMove = (CAbilityMove)_tickAbility;
			if (cAbilityMove.AIFocusOverride != null)
			{
				cAbilityMove.AIFocusOverride.ForcedUseTileIndex = new TileIndex(goalPosition);
			}
		}
	}

	protected override void TickRepositionInternal()
	{
		if (_tickActor == infuser && infuser.ArrayIndex == goalPosition)
		{
			winObjective.ScriptedIsComplete = true;
			CheckObjectivesComplete();
		}
	}

	protected override void TickDeathInternal()
	{
		if (_tickActor is CEnemyActor { NoGoldDrop: not false } cEnemyActor && !(cEnemyActor.CauseOfDeathAbility is CAbilityCapture))
		{
			OnMonsterDeath(_tickActor);
		}
		if (_tickActor == currentlyConvertedActor)
		{
			currentlyConvertedActor = null;
		}
	}

	private void ProcessSpawnedMonsters()
	{
		foreach (CEnemyActor enemy in ScenarioManager.Scenario.Enemies)
		{
			ProcessSpawnedMonster(enemy);
		}
	}

	private void ProcessSpawnedMonster(CEnemyActor actor)
	{
		if (actor != null)
		{
			actor.NoGoldDrop = true;
		}
	}

	private void OnMonsterDeath(CActor actor)
	{
		if (actor.Type == CActor.EType.Enemy)
		{
			PlaceDustOn(actor.Tile);
		}
	}

	private CObjectResource PlaceDustOn(CTile tile)
	{
		CObjectResource cObjectResource = new CObjectResource(ScenarioRuleClient.SRLYML.CharacterResources.SingleOrDefault((CharacterResourceData x) => x.ID == resourceID), dustPrefabName, ScenarioManager.ObjectImportType.Resource, new TileIndex(tile.m_ArrayIndex), null, null, null, tile.m_HexMap.MapGuid);
		cObjectResource.PropCustomLocKey = "FH/FH_Scenario_Campaign_119_SCENARIO_OBJECT_1";
		tile.SpawnProp(cObjectResource);
		return cObjectResource;
	}

	private bool ProcessLootRadiantDust()
	{
		CActor cActor = (_tickActor as CHeroSummonActor)?.Summoner;
		if (cActor == null)
		{
			cActor = _tickActor as CPlayerActor;
		}
		if (cActor == null)
		{
			cActor = GetFirstAlivePlayer();
		}
		int characterResourceAmount = cActor.GetCharacterResourceAmount(resourceID);
		if (characterResourceAmount > 0)
		{
			cActor.RemoveCharacterResource(resourceID, characterResourceAmount);
			ModifyNumDusts(characterResourceAmount);
			return true;
		}
		return false;
	}

	public override bool CanActorLootResources(CActor actor)
	{
		if (actor.Class.ID == infuserID || actor.Type == CActor.EType.Player || actor.Type == CActor.EType.HeroSummon)
		{
			return true;
		}
		return base.CanActorLootResources(actor);
	}

	public override bool CanActorIgnoreIceSlideEffect(CActor actor)
	{
		if (actor == infuser && Room2Revealed())
		{
			return PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability is CAbilityMove;
		}
		return false;
	}

	public void ModifyNumDusts(int amount)
	{
		numDusts += amount;
		textNumDustsModifier.CustomLocKeyArgs = new List<object> { numDusts };
		ScenarioManager.CurrentScenarioState.UpdateScenarioModifiersDescriptions();
	}

	public int GetNumDusts()
	{
		return numDusts;
	}

	public CActor GetPreviousConveredActor()
	{
		return previouslyConvertedActor;
	}

	protected override List<object> GetConclusionParametersInternal()
	{
		return new List<object> { GetNumDusts() };
	}

	[ConsoleCommand(Command = "ss119_mod_num_dusts")]
	public static void SS040ModUsedAidKits(IConsole console, int amount)
	{
		if (ScenarioManager.CurrentScenarioState.FindScenarioScript("ScenarioScript_FH119", out var script) && script is ScenarioScript_FH119 scenarioScript_FH)
		{
			scenarioScript_FH.ModifyNumDusts(amount);
		}
	}

	public static bool Room2Revealed()
	{
		if (ScenarioScript.current == null)
		{
			return false;
		}
		return ScenarioScript.FindRoom("Room_2")?.Revealed ?? false;
	}

	protected override void TickAbilityEndedInternal()
	{
		if (!_tickAbility.GetMiscVar("DustCost").isValid || !_tickAbility.AbilityHasHappened)
		{
			return;
		}
		int num = _tickAbility.GetMiscVar("DustCost").GetInt();
		ModifyNumDusts(-num);
		if (PhaseManager.CurrentPhaseAction == null)
		{
			return;
		}
		List<CPhaseAction.CPhaseAbility> list = PhaseManager.CurrentPhaseAction.CurrentPhaseAbilities?.Where((CPhaseAction.CPhaseAbility x) => x.m_Ability.Name == "AChooseInfuserDust")?.ToList();
		if (list == null)
		{
			return;
		}
		foreach (CPhaseAction.CPhaseAbility item in list)
		{
			item.m_Ability.AbilityHasHappened = true;
			item.m_Ability.SetCancelAbilityFlag(cancel: true);
		}
	}

	public override bool CanToggleBonusesForNonPlayerAbilityStart(CAbility ability)
	{
		return ability.Name == "AChooseInfuserDust";
	}

	public override IEnumerable<(string, string)> GetSpecialRulesParamsString()
	{
		yield return ("heal_with_dust_value", (2 + ScenarioManager.CurrentScenarioState.Level).ToString());
	}
}
