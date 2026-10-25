using System.Collections.Generic;
using System.Linq;
using AStar;
using Libs.GameConsole;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH040 : ScenarioScript
{
	private static readonly int _MaxAidKitsCount = 8;

	public static readonly List<Point> SpawnData = new List<Point>
	{
		new Point(2, 6),
		new Point(7, 11),
		new Point(9, 15)
	};

	private int _usedAidKitsCount;

	private CObjective_Scripted _noDownedAlgoxWinObjective;

	private CObjective_Scripted _downedAlgoxLoseObjective;

	private CObjective_Scripted _usedAidKitsLoseObjective;

	private bool HasDownedAlgox => ScenarioManager.CurrentScenarioState.AllyMonsters.Find((EnemyState m) => m.Actor != null && m.Actor.IsDowned) != null;

	private bool UsedAllAidKits => _usedAidKitsCount >= _MaxAidKitsCount;

	private void UpdateAlgoxObjectives()
	{
		_noDownedAlgoxWinObjective.ScriptedIsComplete = !HasDownedAlgox;
		_noDownedAlgoxWinObjective.CheckObjectiveComplete(ScenarioScript.PartySize);
		_downedAlgoxLoseObjective.ScriptedIsComplete = CheckLoseScenario();
		_downedAlgoxLoseObjective.CheckObjectiveComplete(ScenarioScript.PartySize);
		CheckObjectivesComplete();
	}

	protected override bool Initialize()
	{
		_noDownedAlgoxWinObjective = (CObjective_Scripted)GetObjective("NoDownedAlgox");
		_downedAlgoxLoseObjective = (CObjective_Scripted)GetObjective("AnyDownedAlgox");
		_usedAidKitsLoseObjective = (CObjective_Scripted)GetObjective("UsedAidKits");
		_usedAidKitsLoseObjective.Total = _MaxAidKitsCount;
		CheckObjectivesComplete();
		UpdateAlgoxObjectives();
		return true;
	}

	protected override string SerializeInternal()
	{
		string text = JsonConvert.SerializeObject(_usedAidKitsCount);
		Trace("[ffa000]Serialize: " + text + "[-]", null, 53);
		return text;
	}

	protected override void DeserializeInternal(string s)
	{
		Trace("[ffa000]Deserialize " + s + "[-]", null, 59);
		if (!string.IsNullOrEmpty(s))
		{
			_usedAidKitsCount = JsonConvert.DeserializeObject<int>(s);
			_usedAidKitsLoseObjective.Current = _usedAidKitsCount;
			_usedAidKitsLoseObjective.ScriptedIsComplete = UsedAllAidKits;
			_usedAidKitsLoseObjective.CheckObjectiveComplete(ScenarioScript.PartySize);
			CheckObjectivesComplete();
			UpdateAlgoxObjectives();
		}
	}

	private bool CheckLoseScenario()
	{
		if (UsedAllAidKits)
		{
			return HasDownedAlgox;
		}
		return false;
	}

	private void StartSpecialAbility(CAbility ability)
	{
		ability.SetCanUndo(canUndo: false);
		ability.Flags = CAbility.EAbilityFlag.Special1;
		ability.AbilityFilter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Ally, CAbilityFilter.EFilterEnemy.None, CAbilityFilter.EFilterActorType.Ally);
		foreach (CAbilityFilter abilityFilter in ability.AbilityFilter.AbilityFilters)
		{
			abilityFilter.FilterVars = new List<(string, TValue)> { ("ValidateIsNotSummon", new TValue(CExpression.Parse("target.IsSummonedByPlayer == False"))) };
		}
		ability.HelpBoxTooltipLocKey = ((ability is CAbilityHeal) ? "$FH/FH_Scenario_Campaign_040_Select_FirstAid_Heal$" : "$FH/FH_Scenario_Campaign_040_Select_FirstAid_Revive$");
		if (PhaseManager.CurrentPhaseAction != null)
		{
			PhaseManager.StartAbilities(ability, _tickActor, stackInsteadOfInject: true);
		}
		else
		{
			PhaseManager.StartAbility(ability, ability.GetAbilityBaseCard(), fullCopy: true);
		}
	}

	private void StartSpecialReviveAbility()
	{
		CAbility cAbility = CAbilityReviveDowned.CreateDefaulReviveDowned(1, 1, isMonster: false);
		cAbility.PerformVars = new List<(string, List<(string, TValue)>)> { ("OnAbilityEnded", new List<(string, TValue)> { ("OfferHealAbility", new TValue(CExpression.Parse("ScenarioScript_FH040.HandleAfterReviveAbility(obj)"))) }) };
		StartSpecialAbility(cAbility);
	}

	private void StartSpecialHealAbility()
	{
		CAbility cAbility = CAbilityHeal.CreateDefaultHeal(3 + ScenarioManager.CurrentScenarioState.Level, 1, 1, isMonster: false);
		if (cAbility.MiscAbilityData == null)
		{
			cAbility.MiscAbilityData = new AbilityData.MiscAbilityData();
		}
		cAbility.MiscAbilityData.CanUndo = false;
		StartSpecialAbility(cAbility);
	}

	public static void HandleAfterReviveAbility(CAbility revive)
	{
		(ScenarioScript.current as ScenarioScript_FH040).CheckOfferHealAbility(revive.TargetingActor);
	}

	private void HandleConsumeAidKit()
	{
		CAbility lastAbilityPerformed = _tickActor.LastAbilityPerformed;
		if (lastAbilityPerformed != null && lastAbilityPerformed.AbilityHasHappened)
		{
			bool num = lastAbilityPerformed is CAbilityReviveDowned && lastAbilityPerformed.Flags == CAbility.EAbilityFlag.Special1;
			bool flag = lastAbilityPerformed is CAbilityHeal && lastAbilityPerformed.Flags == CAbility.EAbilityFlag.Special1;
			if (num | flag)
			{
				ModifyUsedAidKits(1);
			}
		}
	}

	private void TickEndTurn()
	{
		UpdateAlgoxObjectives();
		if (!CheckLoseScenario() && _tickActor is CPlayerActor && !CheckOfferReviveAbility())
		{
			CheckOfferHealAbility(_tickActor);
		}
	}

	private bool CheckOfferReviveAbility()
	{
		if (UsedAllAidKits)
		{
			return false;
		}
		if (ScenarioManager.CurrentScenarioState.AllyMonsters.Any((EnemyState m) => m.Actor != null && m.Actor.IsDowned && _tickActor.IsAdjacentTo(m.Actor) && !m.Actor.IsSummonedByPlayer))
		{
			StartSpecialReviveAbility();
			return true;
		}
		return false;
	}

	private bool CheckOfferHealAbility(CActor actor)
	{
		if (UsedAllAidKits)
		{
			return false;
		}
		if (ScenarioManager.CurrentScenarioState.AllyMonsters.Any((EnemyState m) => actor.IsAdjacentTo(m.Actor) && !m.Actor.IsDead && !m.Actor.IsDowned && !m.Actor.IsSummonedByPlayer))
		{
			StartSpecialHealAbility();
			return true;
		}
		return false;
	}

	private void ModifyUsedAidKits(int amount)
	{
		_usedAidKitsCount += amount;
		_usedAidKitsLoseObjective.Current = _usedAidKitsCount;
		if (!HasDownedAlgox && ScenarioManager.CurrentScenarioState.Maps.All((CMap x) => x.Revealed) && _usedAidKitsLoseObjective.Current >= _usedAidKitsLoseObjective.Total)
		{
			_usedAidKitsLoseObjective.SetActivation(active: false);
			CheckObjectivesComplete();
		}
		else
		{
			_usedAidKitsLoseObjective.ScriptedIsComplete = UsedAllAidKits;
			_usedAidKitsLoseObjective.CheckObjectiveComplete(ScenarioScript.PartySize);
			CheckObjectivesComplete();
		}
	}

	private void TickAlgoxStart()
	{
		if (_tickActor is CEnemyActor { Type: CActor.EType.Ally, PlayerSummoner: null })
		{
			_tickActor.CanBeDowned = true;
			if (SpawnData.Contains(_tickActor.ArrayIndex))
			{
				_tickActor.SetDowned(downed: true, initial: true);
				UpdateAlgoxObjectives();
			}
		}
	}

	public override void OnActorDownedChanged(CActor actor)
	{
		base.OnActorDownedChanged(actor);
		if (!_usedAidKitsLoseObjective.IsActive && HasDownedAlgox && _usedAidKitsLoseObjective.Current >= _usedAidKitsLoseObjective.Total)
		{
			_usedAidKitsLoseObjective.SetActivation(active: true);
			ModifyUsedAidKits(0);
		}
		else if (!actor.IsDowned)
		{
			actor.MarkWontPlayThisRound(wontPlay: true);
		}
	}

	protected override void TickPhaseInternal()
	{
		switch (_scenarioModifier.ScenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.AfterAbility:
			HandleConsumeAidKit();
			UpdateAlgoxObjectives();
			break;
		case EScenarioModifierTriggerPhase.EndTurn:
			TickEndTurn();
			break;
		case EScenarioModifierTriggerPhase.StartScenario:
			TickAlgoxStart();
			UpdateAlgoxObjectives();
			break;
		case EScenarioModifierTriggerPhase.OnActorSpawnedOrRevealed:
			TickAlgoxStart();
			break;
		}
	}

	public override void ApplyReward(bool won)
	{
		base.ApplyReward(won);
		if (!won)
		{
			return;
		}
		foreach (CPlayerActor allPlayer in ScenarioManager.Scenario.AllPlayers)
		{
			for (int i = 0; i < 2; i++)
			{
				allPlayer.GainLoot(endOfScenario: true, incLootTokenCount: false, i == 0);
			}
			if (!allPlayer.IsDead)
			{
				allPlayer.ShowLastLootReward("", "MoneyToken", null, focusCamera: true);
			}
		}
	}

	public override string GetOverrideCharacterModelPrefabName(CActor actor)
	{
		return actor.GetPrefabName() switch
		{
			"AlgoxGuard" => "AlgoxGuardSnowspeaker", 
			"AlgoxGuard Elite" => "AlgoxGuardSnowspeaker Elite", 
			"AlgoxArcher" => "AlgoxArcherSnowspeaker", 
			"AlgoxArcher Elite" => "AlgoxArcherSnowspeaker Elite", 
			_ => null, 
		};
	}

	public override IEnumerable<(string, string)> GetSpecialRulesParamsString()
	{
		yield return ("heal_ammount", (3 + ScenarioManager.CurrentScenarioState.Level).ToString());
	}

	[ConsoleCommand(Command = "ss040_mod_used_aid_kits")]
	public static void SS040ModUsedAidKits(IConsole console, int count)
	{
		if (ScenarioManager.CurrentScenarioState.FindScenarioScript("ScenarioScript_FH040", out var script) && script is ScenarioScript_FH040 scenarioScript_FH)
		{
			scenarioScript_FH.ModifyUsedAidKits(count);
		}
	}
}
