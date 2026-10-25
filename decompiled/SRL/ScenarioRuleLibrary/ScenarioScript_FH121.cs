using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH121 : ScenarioScript
{
	private CMap room4;

	private Point _hole1Index = new Point(10, 19);

	private Point _hole2Index = new Point(11, 19);

	private Point _room4Hole1Index = new Point(9, 7);

	private Point _room4Hole2Index = new Point(10, 7);

	private CObjective_Scripted _investigateObjective;

	private CObjective_Scripted _objBossHalfHealth;

	private CObjective_Scripted _objBossDead;

	private CScenarioModifier HoleConnectionModifier;

	private CObjective KillBossObjective;

	private bool? _SuitChosen;

	private bool? _SharedTechnology;

	private int roundRevealedRoom4;

	private CEnemyActor AugmentedMindthief => ScenarioManager.Scenario.AllEnemies.FirstOrDefault((CEnemyActor x) => x.Class.ID == "AugmentedMindthiefID");

	protected override bool Initialize()
	{
		room4 = ScenarioScript.FindRoom("Room_4");
		_investigateObjective = GetObjective<CObjective_Scripted>("InvestigateObjective");
		KillBossObjective = GetObjective("KillBossObjective");
		HoleConnectionModifier = GetScenarioModifier("HoleConnectionModifier");
		_objBossHalfHealth = (CObjective_Scripted)GetObjective("objBossHalfHealth");
		_objBossDead = (CObjective_Scripted)GetObjective("objBossDead");
		return base.Initialize();
	}

	protected override string SerializeInternal()
	{
		string text = JsonConvert.SerializeObject(_SuitChosen);
		string text2 = JsonConvert.SerializeObject(_SharedTechnology);
		string text3 = JsonConvert.SerializeObject(roundRevealedRoom4);
		string text4 = JsonConvert.SerializeObject(new string[3] { text, text2, text3 });
		Trace("[ffa000]Serialize: " + text4 + "[-]", null, 44);
		return text4;
	}

	public override bool ShouldSortModifierTextsInOrderOfModifiers()
	{
		return true;
	}

	protected override void DeserializeInternal(string s)
	{
		Trace("[ffa000]Deserialize " + s + "[-]", null, 51);
		if (!string.IsNullOrEmpty(s))
		{
			string[] array = JsonConvert.DeserializeObject<string[]>(s);
			_SuitChosen = JsonConvert.DeserializeObject<bool?>(array[0]);
			_SharedTechnology = JsonConvert.DeserializeObject<bool?>(array[1]);
			roundRevealedRoom4 = JsonConvert.DeserializeObject<int>(array[2]);
		}
	}

	protected override int GetConclusionIndexInternal()
	{
		if (_SuitChosen.HasValue && _SuitChosen.Value)
		{
			if (_SharedTechnology.HasValue && _SharedTechnology.Value)
			{
				return 1;
			}
			return 2;
		}
		return 0;
	}

	protected override void TickRepositionInternal()
	{
		_ = _tickAbility;
		if (_tickActor is CPlayerActor cPlayerActor && (cPlayerActor.Tile.m_ArrayIndex == _hole1Index || cPlayerActor.Tile.m_ArrayIndex == _hole2Index))
		{
			bool revealed = room4.Revealed;
			if (!revealed)
			{
				room4.Reveal(initial: false);
				GameState.SortIntoInitiativeAndIDOrder();
				CRevealRooms_MessageData message = new CRevealRooms_MessageData();
				ScenarioRuleClient.MessageHandler(message);
				HoleConnectionModifier.SetDeactivated(deactivate: false);
			}
			if (cPlayerActor.Tile.m_ArrayIndex == _hole1Index)
			{
				GetNearestFreeTile(_room4Hole1Index.X, _room4Hole1Index.Y, out var nearestTile);
				GameState.TeleportActor(cPlayerActor, nearestTile, tryReveal: true);
				MoveSummons(cPlayerActor, teleport: true);
			}
			else if (cPlayerActor.Tile.m_ArrayIndex == _hole2Index)
			{
				GetNearestFreeTile(_room4Hole2Index.X, _room4Hole2Index.Y, out var nearestTile2);
				GameState.TeleportActor(cPlayerActor, nearestTile2, tryReveal: true);
				MoveSummons(cPlayerActor, teleport: true);
			}
			if (!revealed)
			{
				CFocusCamera_MessageData message2 = new CFocusCamera_MessageData(_tickActor)
				{
					m_Point = AugmentedMindthief.ArrayIndex,
					m_Zoom = 0.5f
				};
				ScenarioRuleClient.MessageHandler(message2);
				GameState.TriggerCampaignSection("FS_121_Black_Memories_Special_2");
			}
		}
	}

	private void MoveSummons(CActor actor, bool teleport = false)
	{
		List<CHeroSummonActor> list = ScenarioManager.Scenario.AllHeroSummons.Where((CHeroSummonActor summon) => summon.Summoner.ActorGuid == actor.ActorGuid && (summon.PhasedOut | teleport)).ToList();
		if (list.Count() <= 0)
		{
			return;
		}
		foreach (CHeroSummonActor item in list)
		{
			if (!GetNearestFreeTile(actor.ArrayIndex.X, actor.ArrayIndex.Y, out var nearestTile))
			{
				break;
			}
			if (teleport)
			{
				GameState.TeleportActor(item, nearestTile);
				continue;
			}
			item.ArrayIndex = nearestTile.m_ArrayIndex;
			GameState.PhaseInActor(item);
		}
	}

	protected override void TickTakenDamageInternal(int damageTaken, int damageReducedByShields, int actualDamage)
	{
		if (_tickActor is CEnemyActor cEnemyActor && cEnemyActor.Class.ID == "AugmentedMindthiefID" && cEnemyActor.Health <= (int)Math.Floor((float)cEnemyActor.MaxHealth / 2f) && !_SuitChosen.HasValue)
		{
			_objBossHalfHealth.ScriptedIsComplete = true;
			CheckObjectivesComplete();
			GameState.TriggerCampaignSection("FS_121_Black_Memories_Special_3");
			_SuitChosen = GameState.ShowGenericConfirmButton(GameState.HostCharacter, "FH_Scenario_Campaign_121_CHOICE_1_DESC", "FH/FH_Scenario_Campaign_121_CHOICE_1_OPTION_1", null, "FH/FH_Scenario_Campaign_121_CHOICE_1_OPTION_2", showHeader: false);
		}
	}

	protected override void TickDeathInternal()
	{
		if (_tickActor is CEnemyActor cEnemyActor && cEnemyActor.Class.ID == "AugmentedMindthiefID" && _SuitChosen.HasValue && _SuitChosen.Value)
		{
			_objBossDead.ScriptedIsComplete = true;
			CheckObjectivesComplete();
			GameState.TriggerCampaignSection("FS_121_Black_Memories_Special_4");
			_SharedTechnology = GameState.ShowGenericConfirmButton(GameState.HostCharacter, "FH_Scenario_Campaign_121_CHOICE_2_DESC", "FH/FH_Scenario_Campaign_121_CHOICE_2_OPTION_1", null, "FH/FH_Scenario_Campaign_121_CHOICE_2_OPTION_2", showHeader: false);
		}
	}

	protected override void OnFinishedRevealingRoom()
	{
		base.OnFinishedRevealingRoom();
		if (_tickRoomRevealed == room4)
		{
			roundRevealedRoom4 = ScenarioScript.RoundNumber;
			_investigateObjective.Current = 1;
			_investigateObjective.Total = 1;
			_investigateObjective.ScriptedIsComplete = true;
			KillBossObjective.SetActivation(active: true);
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		}
	}

	protected override void TickPhaseInternal()
	{
		base.TickPhaseInternal();
		if (_scenarioModifier.ScenarioModifierTriggerPhase != EScenarioModifierTriggerPhase.StartRound || !OncePerRound() || !room4.Revealed)
		{
			return;
		}
		int num = (ScenarioScript.RoundNumber - roundRevealedRoom4 - 1) % 3;
		CEnemyActor augmentedMindthief = AugmentedMindthief;
		if (augmentedMindthief != null)
		{
			CAbility cAbility = null;
			switch (num)
			{
			case 0:
				cAbility = CAbility.CreateAbility(CAbility.EAbilityType.Attack, 0, useSpecialBaseStat: false, 1, 1, 0, CAbility.EAbilityFlag.None, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), null, null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, null, null, null, null, null, null, null, null, new List<CCondition.ENegativeCondition> { CCondition.ENegativeCondition.Muddle }, multiPassAttack: false, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, null, textOnly: false, noLayoutSpace: false, showRange: false, showTarget: false, showArea: false, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, null, null, CAbility.EAbilityTargeting.All, null, null, null, null, null, isMonster: true, null, null, isSubAbility: false, isInlineSubAbility: false, 0, 0, isTargetedAbility: true, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, null, null, 0, null, null, skipAnim: false, 0, EConditionDecTrigger.None, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, null, 0, null, null, null, null, CAbility.EAttackType.None, null, null, CAbility.ECardAction.None, null, null, CAbilityExtraTurn.EExtraTurnType.None, null, null, null, null, null, null, null, null, immuneToSelfDamage: false, null, null, null, null, null, null, ECharacter.None, CAbilityFilter.EFilterTile.None, null, isDefault: false, isItemAbility: false, showElementPicker: false, isModifierAbility: false, null, null, null, null, exploitVulnerability: false, null, null, int.MaxValue);
				cAbility.ActiveBonusData.Behaviour = CActiveBonus.EActiveBonusBehaviourType.BuffAttack;
				cAbility.ActiveBonusData.ListLayoutOverride = ("<sprite name=Muddle> $To_AllAttacks$", "Positive_Muddle_Attack_Campaign");
				break;
			case 1:
				cAbility = CAbility.CreateAbility(CAbility.EAbilityType.Attack, 0, useSpecialBaseStat: false, 1, 1, 0, CAbility.EAbilityFlag.None, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), null, null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, null, null, null, null, null, null, null, null, new List<CCondition.ENegativeCondition> { CCondition.ENegativeCondition.Poison }, multiPassAttack: false, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, null, textOnly: false, noLayoutSpace: false, showRange: false, showTarget: false, showArea: false, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, null, null, CAbility.EAbilityTargeting.All, null, null, null, null, null, isMonster: true, null, null, isSubAbility: false, isInlineSubAbility: false, 0, 0, isTargetedAbility: true, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, null, null, 0, null, null, skipAnim: false, 0, EConditionDecTrigger.None, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, null, 0, null, null, null, null, CAbility.EAttackType.None, null, null, CAbility.ECardAction.None, null, null, CAbilityExtraTurn.EExtraTurnType.None, null, null, null, null, null, null, null, null, immuneToSelfDamage: false, null, null, null, null, null, null, ECharacter.None, CAbilityFilter.EFilterTile.None, null, isDefault: false, isItemAbility: false, showElementPicker: false, isModifierAbility: false, null, null, null, null, exploitVulnerability: false, null, null, int.MaxValue);
				cAbility.ActiveBonusData.Behaviour = CActiveBonus.EActiveBonusBehaviourType.BuffAttack;
				cAbility.ActiveBonusData.ListLayoutOverride = ("<sprite name=Poison> $To_AllAttacks$", "Positive_Poison_Attack_Campaign");
				break;
			case 2:
				cAbility = CAbilityShield.CreateDefaultShield(ScenarioScript.PartySize - 1, 1, 1, isMonster: true, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self));
				break;
			}
			cAbility.Name = "Scenario121MindThiefAugment";
			cAbility.ActiveBonusData.Duration = CActiveBonus.EActiveBonusDurationType.Round;
			cAbility.Start(augmentedMindthief, augmentedMindthief);
			cAbility.ApplyToActor(augmentedMindthief);
		}
	}
}
