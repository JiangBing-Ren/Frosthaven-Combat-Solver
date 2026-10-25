using System;
using System.Collections.Generic;
using System.Linq;
using AStar;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH106 : ScenarioScript
{
	private Point pushAltarLoc = new Point(16, 9);

	private Point muddleAltarLoc = new Point(13, 1);

	private Point damageAltarLoc = new Point(2, 9);

	private Point healAltarLoc = new Point(4, 1);

	private List<Point> allAltarPos;

	private CActor boss;

	private CObjectDoor lastDoor;

	private CScenarioModifier scenarioTextHolderPush;

	private CScenarioModifier scenarioTextHolderMuddle;

	private CScenarioModifier scenarioTextHolderDamage;

	private CScenarioModifier scenarioTextHolderHeal;

	private List<CScenarioModifier> allAltarTexts;

	private CObjective killBossObjective;

	protected override bool Initialize()
	{
		scenarioTextHolderPush = GetScenarioModifier("TextHolderPushAltar");
		scenarioTextHolderMuddle = GetScenarioModifier("TextHolderMuddleAltar");
		scenarioTextHolderDamage = GetScenarioModifier("TextHolderDamageAltar");
		scenarioTextHolderHeal = GetScenarioModifier("TextHolderHealAltar");
		killBossObjective = GetObjective("KillBoss");
		allAltarPos = new List<Point> { pushAltarLoc, muddleAltarLoc, damageAltarLoc, healAltarLoc };
		allAltarTexts = new List<CScenarioModifier> { scenarioTextHolderPush, scenarioTextHolderMuddle, scenarioTextHolderDamage, scenarioTextHolderHeal };
		lastDoor = GetProp(9, 9, "ThickDoor") as CObjectDoor;
		return base.Initialize();
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		RefreshAltarTexts();
	}

	protected override void TickPhaseInternal()
	{
		Log.SimpleInfo("TickPhaseInternal");
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.EndRound:
			TickPhaseEndRound();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 72);
			break;
		case EScenarioModifierTriggerPhase.StartRound:
		case EScenarioModifierTriggerPhase.StartTurn:
		case EScenarioModifierTriggerPhase.EndTurn:
		case EScenarioModifierTriggerPhase.StartScenario:
			break;
		}
	}

	private void TickPhaseEndRound()
	{
		if (OncePerRound())
		{
			HandlePushAltar();
			HandleMuddleAltar();
			HandleDamageAltar();
			HandleHealAltar();
		}
	}

	private void HandlePushAltar()
	{
		CObjectActor altar = GetAltar(pushAltarLoc);
		if (altar != null && !altar.IsDead)
		{
			CAbilityPush cAbilityPush = (CAbilityPush)CAbilityPush.CreateDefaultPush(1, 5, -1, isMonster: true, "", new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Enemy, CAbilityFilter.EFilterEnemy.None, CAbilityFilter.EFilterActorType.Player));
			cAbilityPush.TargetingActor = altar;
			cAbilityPush.MustMove = true;
			cAbilityPush.MiscVars = new List<(string, TValue)> { ("SkipAnimAndRotation", true) };
			GameState.PendingScenarioModifierAbilities.Add(Tuple.Create((CAbility)cAbilityPush, cAbilityPush.TargetingActor));
		}
	}

	private void HandleMuddleAltar()
	{
		CObjectActor altar = GetAltar(muddleAltarLoc);
		if (altar == null || altar.IsDead)
		{
			return;
		}
		foreach (PlayerState player in ScenarioScript._css.Players)
		{
			player.Actor.ApplyCondition(altar, CCondition.ENegativeCondition.Muddle, 0, EConditionDecTrigger.Turns);
		}
	}

	private void HandleDamageAltar()
	{
		CObjectActor altar = GetAltar(damageAltarLoc);
		if (altar == null || altar.IsDead)
		{
			return;
		}
		foreach (PlayerState player in ScenarioScript._css.Players)
		{
			DealScenarioDamage(player.Actor, 1);
		}
	}

	private void HandleHealAltar()
	{
		CObjectActor altar = GetAltar(healAltarLoc);
		if (altar == null || altar.IsDead)
		{
			return;
		}
		foreach (EnemyState monster in ScenarioScript._css.Monsters)
		{
			CActor actor = monster.Actor;
			if (actor != null)
			{
				CAbilityHeal cAbilityHeal = (CAbilityHeal)CAbilityHeal.CreateDefaultHeal(2, 1, 1, isMonster: true, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self));
				cAbilityHeal.TargetingActor = actor;
				GameState.PendingScenarioModifierAbilities.Add(Tuple.Create((CAbility)cAbilityHeal, actor));
			}
		}
	}

	private void Heal(CActor actor, int strength)
	{
		int health = Math.Min(actor.Health + strength, actor.MaxHealth);
		ScenarioScript.HealTo(actor, health, removeAllTokens: false, ignoreTokens: false);
	}

	private CObjectActor GetAltar(Point loc)
	{
		return ScenarioScript.FindActor(loc.X, loc.Y) as CObjectActor;
	}

	protected override void TickDeathInternal()
	{
		TryUnlockLastDoor();
		RefreshAltarTexts();
	}

	protected override void OnDoorOpenedInternal(CObjectDoor door, CActor actor, CActor creditActor, bool visualOnly)
	{
		TryShowBossObjective(door);
	}

	protected override void OnFinishedRevealingRoom()
	{
		base.OnFinishedRevealingRoom();
		RefreshAltarTexts();
	}

	private void TryUnlockLastDoor()
	{
		if (!lastDoor.DoorIsLocked)
		{
			return;
		}
		foreach (Point allAltarPo in allAltarPos)
		{
			if (!ScenarioManager.Tiles[allAltarPo.X, allAltarPo.Y].m_HexMap.Revealed)
			{
				return;
			}
			CObjectActor altar = GetAltar(allAltarPo);
			if (altar != null && !altar.IsDead)
			{
				return;
			}
		}
		lastDoor.UnlockLockedDoorWithoutOpening();
	}

	private void RefreshAltarTexts()
	{
		IEnumerable<CActiveBonus> source = CharacterClassManager.FindAllActiveBonuses();
		for (int i = 0; i < allAltarPos.Count; i++)
		{
			Point loc = allAltarPos[i];
			CScenarioModifier cScenarioModifier = allAltarTexts[i];
			CTile cTile = ScenarioManager.Tiles[loc.X, loc.Y];
			CObjectActor altar = GetAltar(loc);
			bool flag = true;
			if (!cTile.m_HexMap.Revealed)
			{
				flag = false;
			}
			else if (altar == null || altar.IsDead)
			{
				flag = false;
			}
			cScenarioModifier.SetHiddenState(!flag);
			if (altar == null || altar.IsDead)
			{
				continue;
			}
			CActiveBonus cActiveBonus = source.FirstOrDefault((CActiveBonus x) => x.Actor == altar && x.Ability.Name == "AltarTextBuff");
			CAbility cAbility = cActiveBonus?.Ability;
			if (cActiveBonus == null)
			{
				cAbility = CAbilityNullTargeting.CreateDefaultNullTargeting(1, 1, isMonster: true);
				cAbility.Name = "AltarTextBuff";
				cAbility.ActiveBonusData.HiddenInLog = true;
				cAbility.ActiveBonusData.Duration = CActiveBonus.EActiveBonusDurationType.Persistent;
				switch (i)
				{
				case 0:
					cAbility.ActiveBonusData.ListLayoutOverride = ("$FH_Scenario_Campaign_106_SCENARIO_MODIFIER_4$", "Negative_Push_Campaign");
					cAbility.ActiveBonusData.IsAura = true;
					cAbility.ActiveBonusData.AuraTargeting = CAbility.EAbilityTargeting.All;
					cAbility.ActiveBonusData.AuraFilter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.None, CAbilityFilter.EFilterEnemy.None, CAbilityFilter.EFilterActorType.Player);
					break;
				case 1:
					cAbility.ActiveBonusData.ListLayoutOverride = ("$FH_Scenario_Campaign_106_SCENARIO_MODIFIER_3$", "Negative_Muddle_Campaign");
					cAbility.ActiveBonusData.IsAura = true;
					cAbility.ActiveBonusData.AuraTargeting = CAbility.EAbilityTargeting.All;
					cAbility.ActiveBonusData.AuraFilter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.None, CAbilityFilter.EFilterEnemy.None, CAbilityFilter.EFilterActorType.Player);
					break;
				case 2:
					cAbility.ActiveBonusData.ListLayoutOverride = ("$FH_Scenario_Campaign_106_SCENARIO_MODIFIER_1$", "Negative_DamageSelf_Campaign");
					cAbility.ActiveBonusData.IsAura = true;
					cAbility.ActiveBonusData.AuraTargeting = CAbility.EAbilityTargeting.All;
					cAbility.ActiveBonusData.AuraFilter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.None, CAbilityFilter.EFilterEnemy.None, CAbilityFilter.EFilterActorType.Player);
					break;
				case 3:
					cAbility.ActiveBonusData.ListLayoutOverride = ("$FH_Scenario_Campaign_106_SCENARIO_MODIFIER_2$", "Positive_HealSelf_Campaign");
					cAbility.ActiveBonusData.IsAura = true;
					cAbility.ActiveBonusData.AuraTargeting = CAbility.EAbilityTargeting.All;
					cAbility.ActiveBonusData.AuraFilter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.None, CAbilityFilter.EFilterEnemy.None, CAbilityFilter.EFilterActorType.Enemy);
					break;
				}
				cAbility.Start(altar, altar);
				cAbility.ApplyToActor(altar);
			}
		}
	}

	private void TryShowBossObjective(CObjectDoor door)
	{
		if (lastDoor == door)
		{
			killBossObjective.SetActivation(active: true);
		}
	}
}
