using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using GLOOM;
using Libs.Utils;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;

namespace FHGUI.Common;

public static class UICharacterUtils
{
	public const string ATTACK_MOD_MISS = "x0";

	public const string ATTACK_MOD_M2 = "-2";

	public const string ATTACK_MOD_M1 = "-1";

	public const string ATTACK_MOD_P0 = "+0";

	public const string ATTACK_MOD_P1 = "+1";

	public const string ATTACK_MOD_P2 = "+2";

	public const string ATTACK_MOD_P3 = "+3";

	public const string ATTACK_MOD_P4 = "+4";

	public const string ATTACK_MOD_T2 = "*2";

	private static readonly string[] _basicAttackModifiers = new string[9] { "x0", "-2", "-1", "+0", "+1", "+2", "+3", "+4", "*2" };

	public static readonly CAbility.EAbilityType[] AllowedImmunities = new CAbility.EAbilityType[22]
	{
		CAbility.EAbilityType.Bless,
		CAbility.EAbilityType.Invisible,
		CAbility.EAbilityType.Regenerate,
		CAbility.EAbilityType.Strengthen,
		CAbility.EAbilityType.Ward,
		CAbility.EAbilityType.Bane,
		CAbility.EAbilityType.Brittle,
		CAbility.EAbilityType.Curse,
		CAbility.EAbilityType.Disarm,
		CAbility.EAbilityType.Immobilize,
		CAbility.EAbilityType.Muddle,
		CAbility.EAbilityType.Poison,
		CAbility.EAbilityType.Stun,
		CAbility.EAbilityType.Wound,
		CAbility.EAbilityType.Push,
		CAbility.EAbilityType.Pull,
		CAbility.EAbilityType.Teleport2,
		CAbility.EAbilityType.Move,
		CAbility.EAbilityType.Damage,
		CAbility.EAbilityType.Heal,
		CAbility.EAbilityType.Attack,
		CAbility.EAbilityType.Retaliate
	};

	public static readonly CAbility.EAbilityType[] MovementImmunities = new CAbility.EAbilityType[6]
	{
		CAbility.EAbilityType.Push,
		CAbility.EAbilityType.Pull,
		CAbility.EAbilityType.Swap,
		CAbility.EAbilityType.Teleport,
		CAbility.EAbilityType.Teleport2,
		CAbility.EAbilityType.Move
	};

	public const string IMMUNITY_ALL_CONDITIONS = "AllConditions";

	public const string IMMUNITY_FORCED_MOVEMENT = "Immovable";

	public static List<CActiveBonus> GetAllActiveBonuses(CActor actor, bool showHidden = false)
	{
		List<CActiveBonus> list = CharacterClassManager.FindAllActiveBonuses().ToList();
		List<CActiveBonus> activeBonuses = actor.ActiveBonuses;
		for (int num = list.Count - 1; num >= 0; num--)
		{
			bool flag = true;
			CActiveBonus cActiveBonus = list[num];
			if (cActiveBonus.Ability.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.Summon)
			{
				flag = false;
			}
			else if (cActiveBonus.IsAura)
			{
				if (cActiveBonus.Ability.ActiveBonusData.TriggerOnCaster || cActiveBonus.Ability.ActiveBonusData.DisplayAuraOnCaster)
				{
					if (cActiveBonus.Caster != actor)
					{
						flag = false;
					}
				}
				else if (!cActiveBonus.ValidActorsInRangeOfAura.Contains(actor))
				{
					flag = false;
				}
			}
			else if (!activeBonuses.Contains(cActiveBonus))
			{
				flag = false;
			}
			if (flag && cActiveBonus.BaseCard is CItem cItem && cItem.GetVar("HideItemActiveForCaster").GetBool())
			{
				flag = false;
			}
			if (flag && !showHidden && (cActiveBonus.Ability.ActiveBonusData.Hidden || (cActiveBonus.Caster != actor && cActiveBonus.Ability.ActiveBonusData.HiddenWhenAura) || cActiveBonus.IsHiddenInUI() || cActiveBonus.Ability.GetMiscVar("DontShowActiveBonusInActorStatPanel").GetBool()))
			{
				flag = false;
			}
			if (!flag)
			{
				list.RemoveAt(num);
			}
		}
		return list;
	}

	public static List<CCondition.ENegativeCondition> GetScenarioModifierAddConditionToAttacks(CActor actor)
	{
		List<CCondition.ENegativeCondition> list = new List<CCondition.ENegativeCondition>();
		foreach (CScenarioModifierAddConditionsToAbilities item in from x in ScenarioManager.CurrentScenarioState.ScenarioModifiers.OfType<CScenarioModifierAddConditionsToAbilities>()
			where x.ScenarioModifierType == EScenarioModifierType.AddConditionsToAbilities
			select x)
		{
			if (item.ShouldAddConditions(actor, CAbility.EAbilityType.Attack))
			{
				list.AddRange(item.NegativeConditions);
			}
		}
		return list.Distinct().ToList();
	}

	public static IEnumerable<AttackModifierYMLData> GetAttackModifiersData(CCharacterClass charClass)
	{
		return charClass.AttackModifierCards.Concat(charClass.DiscardedAttackModifierCards);
	}

	public static IEnumerable<AttackModifierYMLData> GetConditionalAttackModifiers(CCharacterClass charClass)
	{
		return from x in GetAttackModifiersData(charClass)
			where !IsBasicModifier(x)
			select x;
	}

	public static IEnumerable<IGrouping<string, AttackModifierYMLData>> GetConditionalAttackModifiersGrouped(CCharacterClass charClass)
	{
		return from it in GetConditionalAttackModifiers(charClass)
			group it by it.Name;
	}

	public static IEnumerable<IGrouping<string, AttackModifierYMLData>> GetAllAttackModifiersGrouped(CCharacterClass charClass)
	{
		return from it in GetAllPossibleAttackModifiers(charClass)
			group it by it.Name;
	}

	public static IEnumerable<IGrouping<string, AttackModifierYMLData>> GetAllBasicAttackModifiersGrouped(CCharacterClass charClass)
	{
		return from it in GetAllPossibleAttackModifiers(charClass).Where(IsBasicModifier)
			group it by it.Name;
	}

	public static int GetAttackModifierUIOrder(string value)
	{
		return _basicAttackModifiers.IndexOf(value);
	}

	public static bool IsBasicModifier(AttackModifierYMLData modifier)
	{
		if (!IsConditionalModifier(modifier))
		{
			return modifier.MathModifier.In(_basicAttackModifiers);
		}
		return false;
	}

	public static bool IsConditionalModifier(AttackModifierYMLData modifier)
	{
		if (!modifier.IsConditionalModifier && !modifier.IsBless)
		{
			return modifier.IsCurse;
		}
		return true;
	}

	public static bool IsBasicModifier(AttackModifierYMLData modifier, string counterId)
	{
		if (modifier.MathModifier.Equals(counterId))
		{
			return IsBasicModifier(modifier);
		}
		return false;
	}

	public static void GetAttackModifierCounters(string counterID, CActor actor, CCharacterClass charClass, out int availableCounters, out int usedCounters)
	{
		availableCounters = 0;
		usedCounters = 0;
		if (charClass == null)
		{
			if (actor == null)
			{
				availableCounters = MonsterClassManager.EnemyMonsterAttackModifierDeck.AttackModifierCards.Count((AttackModifierYMLData x) => IsBasicModifier(x, counterID));
				usedCounters = MonsterClassManager.EnemyMonsterAttackModifierDeck.DiscardedAttackModifierCards.Count((AttackModifierYMLData x) => IsBasicModifier(x, counterID));
			}
			else if (actor is CEnemyActor enemyActor)
			{
				CMonsterAttackModifierDeck attackModifiersDeck = MonsterClassManager.GetAttackModifiersDeck(enemyActor);
				availableCounters = attackModifiersDeck.AttackModifierCards.Count((AttackModifierYMLData x) => IsBasicModifier(x, counterID));
				usedCounters = attackModifiersDeck.DiscardedAttackModifierCards.Count((AttackModifierYMLData x) => IsBasicModifier(x, counterID));
			}
			else
			{
				Log.GUIError("Tried to setup counters for non-enemy actor without a character class");
			}
		}
		else
		{
			usedCounters = charClass.DiscardedAttackModifierCards.Count((AttackModifierYMLData x) => IsBasicModifier(x, counterID));
			availableCounters = charClass.AttackModifierCards.Count((AttackModifierYMLData x) => IsBasicModifier(x, counterID));
		}
	}

	private static IEnumerable<IGrouping<string, AttackModifierYMLData>> GroupAttackModifiers(IEnumerable<AttackModifierYMLData> modifiers)
	{
		return from it in modifiers
			group it by it.Name;
	}

	public static IEnumerable<AttackModifierInfo> GetAttackModifiers(CActor actor)
	{
		if (actor == null)
		{
			CMonsterAttackModifierDeck enemyMonsterAttackModifierDeck = MonsterClassManager.EnemyMonsterAttackModifierDeck;
			return GetAttackModifierInfo(enemyMonsterAttackModifierDeck.AttackModifierCards, enemyMonsterAttackModifierDeck.DiscardedAttackModifierCards);
		}
		if (actor.CharacterClass != null)
		{
			return GetAttackModifiers(actor.CharacterClass);
		}
		if (actor is CEnemyActor enemy)
		{
			return GetAttackModifiers(enemy);
		}
		Log.GUIError("Tried to setup counters for non-enemy actor without a character class");
		return Enumerable.Empty<AttackModifierInfo>();
	}

	private static IEnumerable<AttackModifierInfo> GetAttackModifierInfo(IEnumerable<AttackModifierYMLData> available, IEnumerable<AttackModifierYMLData> discarded)
	{
		IEnumerable<IGrouping<string, AttackModifierYMLData>> enumerable = GroupAttackModifiers(available.Concat(discarded));
		foreach (IGrouping<string, AttackModifierYMLData> item in enumerable)
		{
			AttackModifierYMLData yml = item.First();
			int num = item.Count();
			int num2 = discarded.Count((AttackModifierYMLData x) => x.Name == yml.Name);
			yield return new AttackModifierInfo(yml, num - num2, num2);
		}
	}

	public static IEnumerable<AttackModifierInfo> GetAttackModifiers(CEnemyActor enemy)
	{
		CMonsterAttackModifierDeck attackModifiersDeck = MonsterClassManager.GetAttackModifiersDeck(enemy);
		return GetAttackModifierInfo(attackModifiersDeck.AttackModifierCards, attackModifiersDeck.DiscardedAttackModifierCards);
	}

	public static IEnumerable<AttackModifierInfo> GetAttackModifiers(CCharacterClass charClass)
	{
		return GetAttackModifierInfo(charClass.AttackModifierCards, charClass.DiscardedAttackModifierCards);
	}

	public static IEnumerable<AttackModifierInfo> GetAttackModifiers(CPlayerActor actor)
	{
		return GetAttackModifiers((CActor)actor);
	}

	public static IEnumerable<AttackModifierYMLData> GetAllPossibleAttackModifiers(CCharacterClass charClass)
	{
		return charClass.AttackModifierCards.Concat(charClass.DiscardedAttackModifierCards);
	}

	public static IEnumerable<StatusEffectDTO> GetAllPositiveConditionsData(CActor actor)
	{
		return from c in actor.Tokens.GetAllPositiveConditions()
			where c != CCondition.EPositiveCondition.Advantage
			select new StatusEffectDTO(c);
	}

	public static IEnumerable<StatusEffectDTO> GetAllNegativeConditionsData(CActor actor)
	{
		return from c in actor.Tokens.GetAllTransferableConditions()
			select new StatusEffectDTO(c);
	}

	public static IEnumerable<CCondition.EPositiveCondition> GetAllPositiveConditions(CActor actor)
	{
		return actor.Tokens.GetAllPositiveConditions();
	}

	public static IEnumerable<CCondition.ENegativeCondition> GetAllNegativeConditions(CActor actor)
	{
		return actor.Tokens.GetAllNegativeConditions();
	}

	public static IEnumerable<StatusEffectDTO> GetAllStatusEffects(CActor actor)
	{
		IEnumerable<StatusEffectDTO> innateAbilities = GetInnateAbilities(actor);
		IEnumerable<StatusEffectDTO> allPositiveConditionsData = GetAllPositiveConditionsData(actor);
		IEnumerable<StatusEffectDTO> allNegativeConditionsData = GetAllNegativeConditionsData(actor);
		return allPositiveConditionsData.Concat(allNegativeConditionsData).Concat(innateAbilities);
	}

	public static StatusEffectDTO? GetMindControlStatus(CActor actor)
	{
		if (!(actor is CEnemyActor) || actor.MindControlDuration == CAbilityControlActor.EControlDurationType.None)
		{
			return null;
		}
		string description = string.Empty;
		if (actor.MindControlDuration == CAbilityControlActor.EControlDurationType.ControlForOneAction)
		{
			description = "GUI_MIND_CONTROL_ACTION";
		}
		else if (actor.MindControlDuration == CAbilityControlActor.EControlDurationType.ControlForOneTurn)
		{
			description = "GUI_MIND_CONTROL_TURN";
		}
		return new StatusEffectDTO("GUI_MIND_CONTROL", "GUI_MIND_CONTROL", description, 1, UIInfoTools.Instance.GetActiveAbilityIcon("MindControl"), StatusEffectDTO.StatusEffectType.Negative, localize: true);
	}

	public static bool ImmuneToAllConditionsCheck(List<CAbility.EAbilityType> immunities)
	{
		bool result = true;
		foreach (CAbility.EAbilityType immunityConditionAbilityType in CActor.ImmunityConditionAbilityTypes)
		{
			if (immunityConditionAbilityType != CAbility.EAbilityType.Sleep && immunityConditionAbilityType != CAbility.EAbilityType.StopFlying && !immunities.Contains(immunityConditionAbilityType))
			{
				result = false;
				break;
			}
		}
		return result;
	}

	public static IEnumerable<StatusEffectDTO> GetImmunities(CActor actor, bool combine = true)
	{
		List<StatusEffectDTO> list = GetImmunities(actor.Immunities, actor.MonsterClass, combine).ToList();
		List<StatusEffectDTO> list2 = list.ToList();
		if (list2.All((StatusEffectDTO i) => i.ID != "Immovable"))
		{
			if (!CActiveBonus.FindApplicableActiveBonuses(actor).Any((CActiveBonus b) =>
			{
				CAbility ability = b.Ability;
				bool? flag;
				if (ability == null)
				{
					flag = null;
				}
				else
				{
					AbilityData.ActiveBonusData activeBonusData = ability.ActiveBonusData;
					flag = ((activeBonusData != null) ? new bool?(!activeBonusData.GetMiscVar("ImmunityToForcedMovement").isUnknown) : ((bool?)null));
				}
				bool? flag2 = flag;
				return flag2 == true;
			}))
			{
				return list2;
			}
			CAbility.EAbilityType[] movementImmunities = MovementImmunities;
			for (int num = 0; num < movementImmunities.Length; num++)
			{
				CAbility.EAbilityType immunity = movementImmunities[num];
				list.RemoveWhere((StatusEffectDTO e) => e.ID.Equals(immunity.ToString(), StringComparison.InvariantCultureIgnoreCase));
			}
			list.Add(new StatusEffectDTO
			{
				ID = "Immovable",
				Name = LocalizationManager.GetTranslation("Immovable"),
				Icon = UIInfoTools.Instance.GetActiveAbilityIcon("AA_Immovable"),
				Description = LocalizationManager.GetTranslation("Glossary_Immovable")
			});
		}
		return list;
	}

	public static IEnumerable<StatusEffectDTO> GetImmunities(CMonsterClass monster, bool combine = true)
	{
		return GetImmunities(monster.Immunities, monster, combine);
	}

	public static IEnumerable<StatusEffectDTO> GetImmunities(IEnumerable<CAbility.EAbilityType> Immunities, CMonsterClass monsterClass, bool combine = true)
	{
		List<CAbility.EAbilityType> immunities = new List<CAbility.EAbilityType>(Immunities);
		bool flag = false;
		bool containsAllForcedMovement = false;
		if (combine)
		{
			flag = CActor.ImmuneToAllConditionsCheck(immunities);
			if (flag)
			{
				foreach (CAbility.EAbilityType immunityConditionAbilityType in CActor.ImmunityConditionAbilityTypes)
				{
					if (monsterClass == null || monsterClass.Immunities == null || !monsterClass.AlwaysShowInnateImmunities || !monsterClass.Immunities.Contains(immunityConditionAbilityType))
					{
						immunities.Remove(immunityConditionAbilityType);
					}
				}
			}
			containsAllForcedMovement = monsterClass?.CurrentMonsterStat?.ImmuneToForcedMovement ?? CActor.ImmunityForcedMovementAbilityTypes.All((CAbility.EAbilityType i) => immunities.Contains(i));
			if (containsAllForcedMovement)
			{
				CAbility.EAbilityType[] movementImmunities = MovementImmunities;
				foreach (CAbility.EAbilityType item in movementImmunities)
				{
					immunities.Remove(item);
				}
			}
		}
		if (flag)
		{
			string text = "AllConditions";
			yield return new StatusEffectDTO
			{
				ID = text,
				Name = CreateLayout.LocaliseText("$ImmunityTo$ ") + LocalizationManager.GetTranslation(text),
				Icon = UIInfoTools.Instance.GetImmunityIcon(text)
			};
		}
		if (containsAllForcedMovement)
		{
			string text2 = "Immovable";
			yield return new StatusEffectDTO
			{
				ID = text2,
				Name = LocalizationManager.GetTranslation(text2),
				Icon = UIInfoTools.Instance.GetActiveAbilityIcon("AA_Immovable"),
				Description = LocalizationManager.GetTranslation("Glossary_Immovable")
			};
		}
		immunities.RemoveWhere((CAbility.EAbilityType i) => !AllowedImmunities.Contains(i));
		foreach (CAbility.EAbilityType item2 in immunities)
		{
			string text3 = item2.ToString();
			if (item2 == CAbility.EAbilityType.Teleport2)
			{
				text3 = "Teleport";
			}
			yield return new StatusEffectDTO
			{
				ID = text3,
				Name = CreateLayout.LocaliseText("$ImmunityTo$ ") + LocalizationManager.GetTranslation(text3),
				Icon = UIInfoTools.Instance.GetImmunityIcon(text3)
			};
		}
	}

	public static IEnumerable<StatusEffectDTO> GetInnateAbilities(HeroSummonYMLData summon)
	{
		return GetInnatesFromData(PersistentAbilitiesUI.BuildSummonBaseStatsDict(summon));
	}

	public static IEnumerable<StatusEffectDTO> GetInnateAbilities(CMonsterClass monster)
	{
		return GetInnatesFromData(PersistentAbilitiesUI.BuildMonsterBaseStatsDict(monster));
	}

	public static IEnumerable<StatusEffectDTO> GetInnateAbilities(CMonsterClass monster, BaseStats overrideStats = null)
	{
		return GetInnatesFromData(PersistentAbilitiesUI.BuildMonsterBaseStatsDict(overrideStats ?? monster.CurrentMonsterStat, monster.OriginalBaseStats));
	}

	public static IEnumerable<StatusEffectDTO> GetInnateAbilities(CActor actor)
	{
		List<CCondition.ENegativeCondition> scenarioModifierAddConditionToAttacks = GetScenarioModifierAddConditionToAttacks(actor);
		if (actor is CEnemyActor actor2)
		{
			return GetInnatesFromData(PersistentAbilitiesUI.GetMonsterOrSummonBaseStats(actor2, scenarioModifierAddConditionToAttacks));
		}
		if (actor is CHeroSummonActor actor3)
		{
			return GetInnatesFromData(PersistentAbilitiesUI.GetMonsterOrSummonBaseStats(actor3, scenarioModifierAddConditionToAttacks));
		}
		return Enumerable.Empty<StatusEffectDTO>();
	}

	private static IEnumerable<StatusEffectDTO> GetInnatesFromData(Dictionary<string, string> monsterOrSummonStats)
	{
		foreach (KeyValuePair<string, string> monsterOrSummonStat in monsterOrSummonStats)
		{
			yield return new StatusEffectDTO("Innate", LocalizationManager.GetTranslation("Innate"), CreateLayout.LocaliseText(monsterOrSummonStat.Key), 1, (UIInfoTools.Instance != null) ? UIInfoTools.Instance.GetActiveAbilityIcon(monsterOrSummonStat.Value) : null, StatusEffectDTO.StatusEffectType.Permanent);
		}
	}

	public static IEnumerable<ItemDataDTO> GetEquipment(CActor actor)
	{
		return actor.Inventory.EquipmentItems.Select((CItem item) => new ItemDataDTO(item));
	}

	public static IEnumerable<ItemDataDTO> GetSmallItems(CActor actor)
	{
		return actor.Inventory.SmallItems.Select((CItem item) => new ItemDataDTO(item));
	}

	public static IEnumerable<ItemDataDTO> GetQuestItems(CActor actor)
	{
		return actor.Inventory.QuestItems.Select((CItem item) => new ItemDataDTO(item));
	}

	public static IEnumerable<ItemDataDTO> GetPets(CActor actor)
	{
		return from item in actor.Inventory.AllItemsAndPets
			where item.Slot == CItem.EItemSlot.Pet
			select new ItemDataDTO(item);
	}

	public static IEnumerable<ItemDataDTO> GetLoot(CActor actor)
	{
		foreach (KeyValuePair<LootCard.EType, int> item in actor.TotalLoot.ResourceTypeToCount.Where((KeyValuePair<LootCard.EType, int> loot) => loot.Key != LootCard.EType.RandomItem))
		{
			if (item.Value > 0)
			{
				string translation = LocalizationManager.GetTranslation(FHUILocKeys.GetLootTypeKey(item.Key));
				string lootTypeDescription = FHUILocFormatting.GetLootTypeDescription(item.Key);
				yield return new ItemDataDTO(translation, lootTypeDescription, UIInfoTools.Instance.GetResourceConfig(CrossReference.GetLootTypeKey(item.Key))?.BigIcon, (uint)item.Value);
			}
		}
		foreach (int itemID in actor.TotalLoot.ItemIds)
		{
			ItemCardYMLData itemYml = ScenarioRuleClient.SRLYML.ItemCards.FirstOrDefault((ItemCardYMLData i) => i.ID == itemID);
			yield return new ItemDataDTO(itemYml);
		}
	}

	public static int? GetNumTimesLooted(CPlayerActor playerActor)
	{
		List<CPlayerStatsScenario> list = AdventureState.MapState?.MapParty?.CurrentScenarioStats;
		if (ScenarioManager.Scenario == null || list == null)
		{
			return null;
		}
		return list.FirstOrDefault((CPlayerStatsScenario st) => st.CharacterID == playerActor.Class.ID)?.Loot.Count((CPlayerStatsLoot l) => l.LootType == "MoneyToken");
	}

	public static string GetName(CActor actor)
	{
		if (!(actor is CObjectActor enemyActor))
		{
			if (!(actor is CEnemyActor enemyActor2))
			{
				if (!(actor is CHeroSummonActor cHeroSummonActor))
				{
					if (actor is CPlayerActor actor2)
					{
						CMapCharacter character = UIPlayerCharacterUtils.GetCharacter(actor2);
						if (character != null)
						{
							return character.CharacterName;
						}
						return LocalizationManager.GetTranslation(actor.ActorLocKey());
					}
					return string.Empty;
				}
				return LocalizationManager.GetTranslation(cHeroSummonActor.SummonData.LocKey);
			}
			return GetEnemyName(enemyActor2);
		}
		return GetEnemyName(enemyActor);
	}

	public static string GetEnemyName(CEnemyActor enemyActor)
	{
		string text = "";
		if (enemyActor is CObjectActor { IsAttachedToProp: not false } cObjectActor)
		{
			text = (string.IsNullOrEmpty(cObjectActor.AttachedProp.PropHealthDetails.CustomLocKey) ? cObjectActor.AttachedProp.PrefabName : cObjectActor.AttachedProp.PropHealthDetails.CustomLocKey);
		}
		if (!string.IsNullOrEmpty(enemyActor.CustomLocKey))
		{
			text = enemyActor.CustomLocKey;
		}
		if (enemyActor.MonsterClass != null)
		{
			text = (string.IsNullOrEmpty(text) ? enemyActor.MonsterClass.LocKey : text);
		}
		return LocalizationManager.GetTranslation(text);
	}

	public static bool HasFactions()
	{
		if (ScenarioManager.Scenario != null && ScenarioManager.Scenario.Enemy2Monsters.Count > 0)
		{
			return ScenarioManager.Scenario.Enemies.Count > 0;
		}
		return false;
	}

	public static float GetSummonHealthModifier()
	{
		float result = 1f;
		if (ScenarioManager.CurrentScenarioState != null)
		{
			result = ScenarioManager.CurrentScenarioState.SummonHealthModifier;
		}
		else if (SaveData.Instance?.Global?.CurrentAdventureData?.AdventureMapState?.Difficulty != null && SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.Difficulty.HasSummonHealthModifier)
		{
			result = SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.Difficulty.SummonHealthModifier;
		}
		return result;
	}

	public static string GetFactionText(CActor.EType actorType)
	{
		return actorType switch
		{
			CActor.EType.Player => string.Empty, 
			CActor.EType.Enemy => "I", 
			CActor.EType.HeroSummon => string.Empty, 
			CActor.EType.Ally => string.Empty, 
			CActor.EType.Enemy2 => "II", 
			CActor.EType.Neutral => string.Empty, 
			CActor.EType.Unknown => "", 
			_ => throw new ArgumentOutOfRangeException("actorType", actorType, null), 
		};
	}
}
