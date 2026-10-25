using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH060 : ScenarioScript
{
	private const string MonsterShardPrefix = "FH60_MonsterShard_";

	private const string PlayerShardPrefix = "FH60_PlayerShard_";

	private CActor boss;

	private bool NarrativeEventAlreadyShown;

	[StateDump(Skip = true)]
	private bool ShowNarrativeEvent;

	protected override bool Initialize()
	{
		boss = FindActorByGUID("01d462d5-214c-52ec-99ca-ba912a327526");
		UpdateObjectives();
		PutSnorkelsOn();
		return true;
	}

	public override bool ShouldSortModifierTextsInOrderOfModifiers()
	{
		return true;
	}

	protected override string SerializeInternal()
	{
		string text = JsonConvert.SerializeObject(NarrativeEventAlreadyShown);
		string text2 = JsonConvert.SerializeObject(new string[1] { text });
		Trace("[ffa000]Serialize: " + text2 + "[-]", null, 34);
		return text2;
	}

	protected override void DeserializeInternal(string s)
	{
		Trace("[ffa000]Deserialize " + s + "[-]", null, 40);
		if (!string.IsNullOrEmpty(s))
		{
			string[] array = JsonConvert.DeserializeObject<string[]>(s);
			NarrativeEventAlreadyShown = JsonConvert.DeserializeObject<bool>(array[0]);
		}
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartScenario:
			InitMonsterShards();
			UnderwaterStart();
			break;
		case EScenarioModifierTriggerPhase.StartTurn:
			TickStartTurn();
			break;
		case EScenarioModifierTriggerPhase.EndTurn:
			TickEndTurn();
			break;
		case EScenarioModifierTriggerPhase.StartRound:
			TickStartRound();
			break;
		case EScenarioModifierTriggerPhase.EndRound:
			TickEndRound();
			break;
		case EScenarioModifierTriggerPhase.OnActorSpawnedOrRevealed:
			PutSnorkelsOn(_tickActor);
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 73);
			break;
		}
	}

	private void TickStartTurn()
	{
		InfusePlayerElements();
		CheckOfferTransferShards(_tickActor);
	}

	private void TickEndTurn()
	{
	}

	private void TickStartRound()
	{
	}

	private void TickEndRound()
	{
	}

	protected override void TickDeathInternal()
	{
		CActor tickActor = _tickActor;
		CheckIfMonsterWithShardWasKilled(tickActor, _tickAbility);
		CheckIfPlayerWithShardWasKilled(tickActor);
	}

	private CActor FindNearestPlayer(CActor aroundActor, int maxDist = int.MaxValue)
	{
		CActor result = null;
		int num = int.MaxValue;
		int num2 = int.MaxValue;
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			if (playerActor != aroundActor)
			{
				int num3 = Point.HexDist(playerActor.ArrayIndex, aroundActor.ArrayIndex);
				int num4 = playerActor.Initiative();
				if (num3 <= maxDist && num3 <= num && (num3 != num || num4 < num2))
				{
					result = playerActor;
					num = num3;
					num2 = num4;
				}
			}
		}
		return result;
	}

	private void InitMonsterShards()
	{
		if (OncePerRound())
		{
			InitMonsterShard("de5d2d3a-fd70-6e43-5425-e61801b1d4c3", ElementInfusionBoardManager.EElement.Earth);
			InitMonsterShard("78c14c34-2ce2-034b-b57e-8f2cdb5dc2bd", ElementInfusionBoardManager.EElement.Dark);
			InitMonsterShard("adf13ca8-e400-9a4c-b793-0fc3d1a843d3", ElementInfusionBoardManager.EElement.Fire);
			InitMonsterShard("91ea1c51-54a7-0e0e-89e0-52f30a8622c3", ElementInfusionBoardManager.EElement.Air);
			InitMonsterShard("6402fa98-b7e8-44c1-8425-e0ba7afdd653", ElementInfusionBoardManager.EElement.Light);
			InitMonsterShard("8156e9d0-2b44-7ca4-8b7d-f3bd4b9088b8", ElementInfusionBoardManager.EElement.Ice);
		}
	}

	private void InitMonsterShard(string guid, ElementInfusionBoardManager.EElement element)
	{
		CActor cActor = FindActorByGUID(guid);
		if (cActor == null || cActor.IsDead)
		{
			return;
		}
		if (FindMonsterShardBonus(cActor, out var element2) != null)
		{
			if (element2 != element)
			{
				Error($"{cActor} has {element2} shard while attempting to add {element} shard", null, 156);
			}
			return;
		}
		string scenarioAbilityID = string.Format("{0}{1}", "FH60_MonsterShard_", element);
		ScenarioAbilitiesYMLData scenarioAbilitiesYMLData = ScenarioRuleClient.SRLYML.ScenarioAbilities.FirstOrDefault((ScenarioAbilitiesYMLData x) => x.ScenarioAbilityID == scenarioAbilityID);
		if (scenarioAbilitiesYMLData == null)
		{
			Error("Could not find scenario ability " + scenarioAbilityID, null, 163);
			return;
		}
		CAbility cAbility = CAbility.CopyAbility(scenarioAbilitiesYMLData.ScenarioAbilities[0], generateNewID: true);
		cAbility.Start(cActor, cActor);
		cAbility.ApplyToActor(cActor);
	}

	private CActiveBonus FindMonsterShardBonus(CActor monster, out ElementInfusionBoardManager.EElement element)
	{
		element = ElementInfusionBoardManager.EElement.Any;
		foreach (CActiveBonus item in CActiveBonus.FindApplicableActiveBonuses(monster))
		{
			string text = item.Ability?.Name;
			if (!string.IsNullOrEmpty(text) && text.StartsWith("FH60_MonsterShard_", StringComparison.Ordinal) && Enum.TryParse<ElementInfusionBoardManager.EElement>(text.Substring("FH60_MonsterShard_".Length), out element))
			{
				return item;
			}
		}
		return null;
	}

	private CActor FindMonsterWithShard(ElementInfusionBoardManager.EElement element)
	{
		foreach (CEnemyActor allAliveMonster in ScenarioManager.Scenario.AllAliveMonsters)
		{
			if (FindMonsterShardBonus(allAliveMonster, out var element2) != null && element2 == element)
			{
				return allAliveMonster;
			}
		}
		return null;
	}

	private void CheckIfMonsterWithShardWasKilled(CActor actor, CAbility ability)
	{
		if (FindMonsterShardBonus(actor, out var element) != null)
		{
			CActor killer = ability?.TargetingActor;
			killer = ResolveKiller(actor, killer);
			if (killer == null)
			{
				Error($"Could not determine who killed {actor}", null, 213);
				return;
			}
			AddPlayerShard(killer, element);
			UpdateElementsBoard();
			CElementsInfused_MessageData message = new CElementsInfused_MessageData(null, killer)
			{
				m_ChangedElements = new List<ElementInfusionBoardManager.EElement> { element },
				m_InfuseAbility = null,
				m_IsConsume = false
			};
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	private CActor ResolveKiller(CActor monster, CActor killer)
	{
		if (killer is CPlayerActor)
		{
			return killer;
		}
		if (killer is CHeroSummonActor cHeroSummonActor)
		{
			return cHeroSummonActor.Summoner;
		}
		return FindNearestPlayer(monster);
	}

	public static bool ResolveBonusToPlayerShard(CActiveBonus bonus, out ElementInfusionBoardManager.EElement element)
	{
		element = ElementInfusionBoardManager.EElement.Any;
		string text = bonus.Ability?.Name;
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		if (!text.StartsWith("FH60_PlayerShard_", StringComparison.Ordinal))
		{
			return false;
		}
		if (!Enum.TryParse<ElementInfusionBoardManager.EElement>(text.Substring("FH60_PlayerShard_".Length), out element))
		{
			return false;
		}
		return true;
	}

	public static bool HasPlayerShard(CActor player, ElementInfusionBoardManager.EElement element = ElementInfusionBoardManager.EElement.Any)
	{
		return HasPlayerShard(CActiveBonus.FindApplicableActiveBonuses(player), element);
	}

	public static bool HasPlayerShard(List<CActiveBonus> bonuses, ElementInfusionBoardManager.EElement element = ElementInfusionBoardManager.EElement.Any)
	{
		foreach (CActiveBonus bonuse in bonuses)
		{
			if (ResolveBonusToPlayerShard(bonuse, out var element2) && (element2 == element || element == ElementInfusionBoardManager.EElement.Any))
			{
				return true;
			}
		}
		return false;
	}

	public static CActor FindPlayerWithShard(ElementInfusionBoardManager.EElement element)
	{
		foreach (CPlayerActor allPlayer in ScenarioManager.Scenario.AllPlayers)
		{
			if (HasPlayerShard(allPlayer, element))
			{
				return allPlayer;
			}
		}
		return null;
	}

	public static int NumPlayerShards(CActor player)
	{
		if (player == null)
		{
			return 0;
		}
		int num = 0;
		foreach (CActiveBonus item in CActiveBonus.FindApplicableActiveBonuses(player))
		{
			if (ResolveBonusToPlayerShard(item, out var _))
			{
				num++;
			}
		}
		return num;
	}

	private void AddPlayerShard(CActor player, ElementInfusionBoardManager.EElement element)
	{
		List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(player);
		if (!HasPlayerShard(list, element))
		{
			if (!list.Any((CActiveBonus bonus) => bonus.Ability?.Name == "FH60_PlayerShardAttackBuff"))
			{
				AddScenarioAbility(player, "FH60_PlayerShardAttackBuff", "FH_Scenario_Campaign_060_ShardsAttackBonusName");
			}
			AddScenarioAbility(player, string.Format("{0}{1}", "FH60_PlayerShard_", element));
			ShowNarrativeOnAllElements(player);
		}
	}

	private bool AddScenarioAbility(CActor player, string scenarioAbilityID, string cardName = null)
	{
		ScenarioAbilitiesYMLData scenarioAbilitiesYMLData = ScenarioRuleClient.SRLYML.ScenarioAbilities.FirstOrDefault((ScenarioAbilitiesYMLData x) => x.ScenarioAbilityID == scenarioAbilityID);
		if (scenarioAbilitiesYMLData == null)
		{
			Error("Could not find scenario ability " + scenarioAbilityID, null, 315);
			return false;
		}
		CAbility cAbility = CAbility.CopyAbility(scenarioAbilitiesYMLData.ScenarioAbilities[0], generateNewID: true);
		if (cardName != null)
		{
			cAbility.CachedAbilityBaseCard = CBaseCard.CreateDummy(cardName);
		}
		cAbility.Start(player, player);
		cAbility.ApplyToActor(player);
		return true;
	}

	private void ShowNarrativeOnAllElements(CActor player)
	{
		if (!NarrativeEventAlreadyShown)
		{
			List<CActiveBonus> bonuses = CActiveBonus.FindApplicableActiveBonuses(player);
			if (HasPlayerShard(bonuses, ElementInfusionBoardManager.EElement.Earth) && HasPlayerShard(bonuses, ElementInfusionBoardManager.EElement.Air) && HasPlayerShard(bonuses, ElementInfusionBoardManager.EElement.Fire) && HasPlayerShard(bonuses, ElementInfusionBoardManager.EElement.Ice) && HasPlayerShard(bonuses, ElementInfusionBoardManager.EElement.Light) && HasPlayerShard(bonuses, ElementInfusionBoardManager.EElement.Dark))
			{
				ShowNarrativeEvent = true;
			}
		}
	}

	private void DelPlayerShard(CActor actor, CActiveBonus bonus)
	{
		bonus.Finish();
	}

	private void TransferPlayerShards(CActor fromPlayer, CActor toPlayer)
	{
		foreach (CActiveBonus item in CActiveBonus.FindApplicableActiveBonuses(fromPlayer))
		{
			ElementInfusionBoardManager.EElement element;
			if (item.Ability?.Name == "FH60_PlayerShardAttackBuff")
			{
				item.Finish();
			}
			else if (ResolveBonusToPlayerShard(item, out element))
			{
				DelPlayerShard(fromPlayer, item);
				AddPlayerShard(toPlayer, element);
			}
		}
	}

	private void CheckOfferTransferShards(CActor actor)
	{
		if (!HasPlayerShard(actor))
		{
			return;
		}
		List<CTile> list = null;
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			if (playerActor.IsAdjacentTo(actor))
			{
				if (list == null)
				{
					list = new List<CTile>();
				}
				list.Add(ScenarioManager.GetTile(playerActor.ArrayIndex));
			}
		}
		if (list != null)
		{
			ScenarioScript.PickHexes("$FH/FH_Scenario_Campaign_060_Select_Hex_1$", actor, list, TransferShardsCallback, 1, animateActor: false, null, canSkip: true, canUndo: true, recalculateValidTilesOnSelect: false, preSelectValidTiles: true, "UseItem", null, CAbilityFilter.EFilterTile.None, ignoreLos: false, ignoreDoorLocks: false, stackAsSeparateAbilities: false, autoTrigger: false, null, interruptCurrentAbility: false, tryInject: false, null, ignoreStun: true);
		}
	}

	private void TransferShardsCallback(CAbilityNullHex ability, List<CTile> selectedTiles)
	{
		if (selectedTiles != null && selectedTiles.Count != 0)
		{
			CTile cTile = selectedTiles[0];
			CActor cActor = ScenarioManager.Scenario.FindActorAt(cTile.m_ArrayIndex);
			if (cActor is CPlayerActor)
			{
				TransferPlayerShards(ability.TargetingActor, cActor);
			}
		}
	}

	private void CheckIfPlayerWithShardWasKilled(CActor actor)
	{
		if (HasPlayerShard(actor))
		{
			CActor cActor = FindNearestPlayer(actor);
			if (cActor != null)
			{
				TransferPlayerShards(actor, cActor);
			}
		}
	}

	public override bool ShouldPierceInvulnerability(CActor actor)
	{
		return NumPlayerShards(actor) == 6;
	}

	public override bool ShouldIgnoreImmunity(CActor actor, CAbility ability)
	{
		if (actor != boss)
		{
			return false;
		}
		if (ability?.TargetingActor == null)
		{
			return false;
		}
		if (NumPlayerShards(ability.TargetingActor) != 6)
		{
			return false;
		}
		CAbility.EAbilityType abilityType = ability.AbilityType;
		if (boss.MonsterClass.Immunities.Contains(abilityType))
		{
			return false;
		}
		return true;
	}

	public override bool ValidateElementInCreation(ElementInfusionBoardManager.EElement element)
	{
		return false;
	}

	public override bool UpdateElementsBoard()
	{
		bool result = false;
		ElementInfusionBoardManager.EElement[] elements = ElementInfusionBoardManager.Elements;
		foreach (ElementInfusionBoardManager.EElement element in elements)
		{
			if (FindMonsterWithShard(element) != null)
			{
				ElementInfusionBoardManager.SetElementInstantly(element, ElementInfusionBoardManager.EColumn.Inert, fromScenarioScript: true);
				result = true;
			}
		}
		return result;
	}

	public void InfusePlayerElements()
	{
		List<ElementInfusionBoardManager.EElement> list = new List<ElementInfusionBoardManager.EElement>();
		ElementInfusionBoardManager.EElement[] elements = ElementInfusionBoardManager.Elements;
		foreach (ElementInfusionBoardManager.EElement eElement in elements)
		{
			if (ElementInfusionBoardManager.ElementColumn(eElement) != ElementInfusionBoardManager.EColumn.Strong && FindPlayerWithShard(eElement) != null)
			{
				list.Add(eElement);
				ElementInfusionBoardManager.SetElementInstantly(eElement, ElementInfusionBoardManager.EColumn.Strong, fromScenarioScript: true);
			}
		}
		if (list.Count > 0)
		{
			CElementsInfused_MessageData message = new CElementsInfused_MessageData(null, null)
			{
				m_ChangedElements = list,
				m_InfuseAbility = null,
				m_IsConsume = false
			};
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	public static bool ValidateBossConsume(string elementName, int additionalAnyElements, List<string> preferConsumes = null)
	{
		if (!Enum.TryParse<ElementInfusionBoardManager.EElement>(elementName, out var result))
		{
			return false;
		}
		List<ElementInfusionBoardManager.EElement> availableElements = ElementInfusionBoardManager.GetAvailableElements();
		if (availableElements == null || availableElements.Count < 1 + additionalAnyElements)
		{
			return false;
		}
		if (!availableElements.Contains(result))
		{
			return false;
		}
		List<ElementInfusionBoardManager.EElement> list = new List<ElementInfusionBoardManager.EElement>();
		if (Consume(availableElements, result))
		{
			list.Add(result);
		}
		if (additionalAnyElements > 0 && preferConsumes != null)
		{
			foreach (string preferConsume in preferConsumes)
			{
				if (Enum.TryParse<ElementInfusionBoardManager.EElement>(preferConsume, out var result2) && Consume(availableElements, result2))
				{
					list.Add(result2);
					additionalAnyElements--;
					if (additionalAnyElements <= 0)
					{
						break;
					}
				}
			}
		}
		while (additionalAnyElements > 0)
		{
			additionalAnyElements--;
			int index = ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(availableElements.Count);
			ElementInfusionBoardManager.EElement eElement = availableElements[index];
			if (Consume(availableElements, eElement))
			{
				list.Add(eElement);
			}
		}
		if (list.Any() && ScenarioScript.current is ScenarioScript_FH060)
		{
			CElementsInfused_MessageData message = new CElementsInfused_MessageData(null, (ScenarioScript.current as ScenarioScript_FH060)?.boss)
			{
				m_ChangedElements = list,
				m_IsConsume = true
			};
			ScenarioRuleClient.MessageHandler(message);
		}
		return true;
	}

	private static bool Consume(List<ElementInfusionBoardManager.EElement> elements, ElementInfusionBoardManager.EElement element)
	{
		if (!elements.Remove(element))
		{
			return false;
		}
		ElementInfusionBoardManager.Consume(element, GameState.CurrentActor);
		return true;
	}

	private void UpdateObjectives()
	{
	}

	public override bool IsUnderwater(CTile tile = null)
	{
		return true;
	}

	public override bool IgnoreAIFocus(CActor targetingActor, CActor target)
	{
		if (target == boss && targetingActor is CHeroSummonActor)
		{
			return true;
		}
		return base.IgnoreAIFocus(targetingActor, target);
	}

	protected override bool AllowTriggerMessageInternal(SEvent ev, CLevelMessage message)
	{
		if (ShowNarrativeEvent && message.MessageName == "FS_060_AllShardsMessage")
		{
			ShowNarrativeEvent = false;
			NarrativeEventAlreadyShown = true;
			return true;
		}
		return base.AllowTriggerMessageInternal(ev, message);
	}
}
