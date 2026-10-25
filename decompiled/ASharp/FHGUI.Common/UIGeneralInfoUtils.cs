using System;
using System.Collections.Generic;
using System.Linq;
using FH.Tools;
using FHGUI.Common.DTO;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Scenario;
using FHGUI.Scenario.Services;
using FHGUI.Scenario.Use_Bar;
using FHGUI.Services;
using FHRL;
using GLOOM;
using Libs.Utils.DataStructures;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.Events;

namespace FHGUI.Common;

public static class UIGeneralInfoUtils
{
	public struct CharacterTokenAttributes
	{
		public bool IsFlying;

		public bool Invulnerable;

		public bool Untargetable;

		public bool HealingBlocked;

		public CharacterTokenAttributes(CActor actor)
		{
			IsFlying = actor.Flying;
			Invulnerable = actor.Invulnerable;
			Untargetable = actor.Untargetable;
			HealingBlocked = actor.CachedHealingBlocked;
		}

		public CharacterTokenAttributes(CMonsterClass monster)
		{
			IsFlying = monster.Flying;
			Invulnerable = monster.Invulnerable;
			Untargetable = monster.Untargetable;
			HealingBlocked = false;
		}
	}

	private static ScenarioManager.ObjectImportType[] _propPriority = new ScenarioManager.ObjectImportType[17]
	{
		ScenarioManager.ObjectImportType.MoneyToken,
		ScenarioManager.ObjectImportType.Chest,
		ScenarioManager.ObjectImportType.GoalChest,
		ScenarioManager.ObjectImportType.Obstacle,
		ScenarioManager.ObjectImportType.Door,
		ScenarioManager.ObjectImportType.PressurePlate,
		ScenarioManager.ObjectImportType.TerrainIce,
		ScenarioManager.ObjectImportType.Portal,
		ScenarioManager.ObjectImportType.CarryableQuestItem,
		ScenarioManager.ObjectImportType.Resource,
		ScenarioManager.ObjectImportType.Shadow,
		ScenarioManager.ObjectImportType.VFXProp,
		ScenarioManager.ObjectImportType.TerrainVisualEffect,
		ScenarioManager.ObjectImportType.TerrainLampVisualEffect,
		ScenarioManager.ObjectImportType.TerrainRubble,
		ScenarioManager.ObjectImportType.Trap,
		ScenarioManager.ObjectImportType.Spawner
	};

	public static UIGeneralInfoItem.UIGeneralInfoItemData GetItemInfo(CItem item)
	{
		UIGeneralInfoItem.UIGeneralInfoItemData itemInfo = GetItemInfo(item.YMLData);
		itemInfo.IsUsed = item.HasBeenUsed;
		return itemInfo;
	}

	public static UIGeneralInfoItem.UIGeneralInfoItemData GetItemInfo(ItemCardYMLData item)
	{
		return new UIGeneralInfoItem.UIGeneralInfoItemData
		{
			ItemID = item.ID,
			Name = LocalizationManager.GetTranslation(item.Name),
			Description = (item.Name.IsNullOrEmpty() ? string.Empty : LocalizationManager.GetTranslation(item.Name.Substring(0, item.Name.Length - 1) + "2")),
			Slot = item.Slot,
			Icon = UIInfoTools.Instance.GetItemMiniSprite(item.Art),
			IsUsed = false
		};
	}

	public static UIGeneralInfoItem.UIGeneralInfoItemData GetItemInfo(ItemDataDTO item)
	{
		int num = item.ScenarioItemID;
		if (item.ScenarioItemID < 0)
		{
			int scenarioItemID = (Singleton<FHMapChoreographer>.InstanceFast?.DefRepo)?.Items[item.ID].ScenarioItemID ?? (-1);
			ScenarioRuleClient.SRLYML?.ItemCards?.FirstOrDefault((ItemCardYMLData i) => i.ID == scenarioItemID);
			num = num;
		}
		return new UIGeneralInfoItem.UIGeneralInfoItemData
		{
			ItemID = num,
			Name = LocalizationManager.GetTranslation(item.NameKey),
			Description = LocalizationManager.GetTranslation(item.DescriptionKey),
			Slot = CrossReference.ItemEquipTypeToSlot(item.ItemType),
			Icon = item.Icon,
			IsUsed = false
		};
	}

	public static UIGeneralInfoItem.UIGeneralInfoItemData GetItemInfo(ItemDef item)
	{
		return GetItemInfo(new ItemDataDTO(item));
	}

	private static int[] GetXPTracker(CItem item)
	{
		return item.ActiveBonuses.FirstOrDefault((CActiveBonus bonus) =>
		{
			CAbility ability = bonus.Ability;
			return ability != null && ability.ActiveBonusData?.Tracker?.Count > 0;
		})?.Ability.ActiveBonusData.Tracker.ToArray();
	}

	public static UIGeneralInfoItem.UIGeneralInfoItemData GetPetInfo(PetDTO pet)
	{
		return GetItemInfo(ScenarioRuleClient.SRLYML?.PetCards?.FirstOrDefault((ItemCardYMLData i) => i.ID == pet.Def.ID.ID));
	}

	public static UIGeneralInfoItem.UIGeneralInfoItemData GetPetInfo(PetDef pet)
	{
		return GetPetInfo(new PetDTO(pet));
	}

	private static IEnumerable<CActiveBonus> GetCardActiveBonuses(CAbilityCard abilityCard)
	{
		return abilityCard.ActiveBonuses.Where((CActiveBonus c) => !c.IsHiddenInUI()).Distinct();
	}

	public static UIGeneralInfoCard.UIGeneralInfoCardData GetAbilityCardInfo(AbilityCardUI card)
	{
		if (card.PlayerActor != null)
		{
			return GetAbilityCardInfo(card.AbilityCard, card.PlayerActor);
		}
		if (card.CharacterID != null)
		{
			return GetAbilityCardInfo(card.AbilityCard, card.CharacterID);
		}
		return GetAbilityCardInfo(card.AbilityCard, card.Enhancements);
	}

	public static UIGeneralInfoCard.UIGeneralInfoCardData GetAbilityCardInfo(CAbilityCard card, CPlayerActor owner = null)
	{
		return new UIGeneralInfoCard.UIGeneralInfoCardData
		{
			Card = card.GetAbilityCardYML,
			PileType = card.CurrentCardPile,
			ActiveBonuses = GetCardActiveBonuses(card).ToArray(),
			Enhancements = ((owner == null) ? null : UIPlayerCharacterUtils.GetEnhancements(owner).ToArray())
		};
	}

	public static UIGeneralInfoCard.UIGeneralInfoCardData GetAbilityCardInfo(CAbilityCard card, CharacterID characterID)
	{
		return new UIGeneralInfoCard.UIGeneralInfoCardData
		{
			Card = card.GetAbilityCardYML,
			PileType = card.CurrentCardPile,
			ActiveBonuses = GetCardActiveBonuses(card).ToArray(),
			Enhancements = ((characterID == null) ? null : UIPlayerCharacterUtils.GetEnhancements(characterID).ToArray())
		};
	}

	public static UIGeneralInfoCard.UIGeneralInfoCardData GetAbilityCardInfo(CAbilityCard card, IEnumerable<CEnhancement> enhancements)
	{
		return new UIGeneralInfoCard.UIGeneralInfoCardData
		{
			Card = card.GetAbilityCardYML,
			PileType = card.CurrentCardPile,
			ActiveBonuses = GetCardActiveBonuses(card).ToArray(),
			Enhancements = enhancements.ToArray()
		};
	}

	public static UIGeneralInfoCard.UIGeneralInfoCardData GetAbilityCardInfo(CardDTO card)
	{
		UIGeneralInfoCard.UIGeneralInfoCardData abilityCardInfo = GetAbilityCardInfo(ScenarioRuleClient.SRLYML.AbilityCards.SingleOrDefault((AbilityCardYMLData x) => x.ID == card.Id));
		abilityCardInfo.Enhancements = card.Enhancements;
		return abilityCardInfo;
	}

	public static UIGeneralInfoCard.UIGeneralInfoCardData GetAbilityCardInfo(ClassCardDef card)
	{
		return GetAbilityCardInfo(new CardDTO(card, isInHand: false, null, null, isSelectable: false));
	}

	public static UIGeneralInfoCard.UIGeneralInfoCardData GetAbilityCardInfo(AbilityCardYMLData card)
	{
		return new UIGeneralInfoCard.UIGeneralInfoCardData
		{
			Card = card,
			PileType = CBaseCard.ECardPile.None
		};
	}

	public static CardDTO GetAbilityCardInfo(int cardID, string customConfigID = null)
	{
		CAbilityCard card = CharacterClassManager.AllAbilityCards.FirstOrDefault((CAbilityCard c) => c.ID == cardID);
		CharacterClassManager.Classes.FirstOrDefault((CCharacterClass c) => c.CharacterID == card.ClassID);
		return new CardDTO(card);
	}

	public static MonsterCardYMLData GetMonsterCardYML(int id)
	{
		return ScenarioRuleClient.SRLYML.MonsterCards.FirstOrDefault((MonsterCardYMLData c) => c.ID == id);
	}

	public static UIGeneralInfoMonsterCard.MonsterCardGeneralInfoData GetMonsterCard(MonsterCardYMLData card, CEnemyActor actor = null, CMonsterClass monster = null)
	{
		CardPileType pile = GetMonsterCardPile(card.ID, actor?.MonsterClass ?? monster);
		return new UIGeneralInfoMonsterCard.MonsterCardGeneralInfoData
		{
			Card = new MonsterCardDTO(card, pile, actor, monster),
			Monster = ((actor != null) ? new MonsterSimpleDTO?(new MonsterSimpleDTO(actor)) : ((monster != null) ? new MonsterSimpleDTO?(new MonsterSimpleDTO(monster)) : ((MonsterSimpleDTO?)null))),
			GetMonsterCardContext = () => new MonsterCardUIContext
			{
				CardYML = card,
				EnemyForBaseStats = actor,
				FallbackMonsterClass = monster,
				ShowFaction = UICharacterUtils.HasFactions(),
				CardPile = pile,
				OverrideLevel = actor?.Level
			}
		};
	}

	public static IGeneralInfoData GetActor(CActor actor)
	{
		if (!(actor is CObjectActor enemyActor))
		{
			if (!(actor is CEnemyActor cEnemyActor))
			{
				if (!(actor is CHeroSummonActor summon))
				{
					if (actor is CPlayerActor actor2)
					{
						return GetCharacterData(actor2);
					}
					throw new ArgumentOutOfRangeException("actor");
				}
				return GetSummon(summon);
			}
			IGeneralInfoData result;
			if (cEnemyActor.MonsterClass.AbilityCardsPool.Count != 0 || cEnemyActor.GetClonedFromActor() == null)
			{
				IGeneralInfoData generalInfoData = GetMonster(cEnemyActor);
				result = generalInfoData;
			}
			else
			{
				result = GetActor(cEnemyActor.GetClonedFromActor());
			}
			return result;
		}
		if (actor.SkipVisualsAndGUI)
		{
			throw new NotImplementedException();
		}
		return GetMonster(enemyActor);
	}

	public static UIGeneralInfoCharacter.PlayerCharacterGeneralInfoData GetCharacterData(CPlayerActor actor)
	{
		CMapCharacter character = UIPlayerCharacterUtils.GetCharacter(actor);
		(int, int) xp = UIPlayerCharacterUtils.GetXp(character, actor);
		return new UIGeneralInfoCharacter.PlayerCharacterGeneralInfoData
		{
			Character = actor.CharacterClass.CharacterModel,
			Name = ((character == null) ? LocalizationManager.GetTranslation(actor.ActorLocKey()) : character.CharacterName),
			ClassName = LocalizationManager.GetTranslation(actor.ActorLocKey()),
			IsOwner = actor.IsUnderMyControl(),
			Illustration = UIInfoTools.Instance.GetCharacterConfigUI(actor.CharacterClass.CharacterModel).InfoIllustration,
			Level = (uint)actor.Level,
			Health = (uint)((actor.PreDamageHealth > 0) ? actor.PreDamageHealth : Mathf.Clamp(actor.Health, 0, actor.MaxHealth)),
			MaxHealth = (uint)actor.MaxHealth,
			OriginalMaxHealth = (uint)actor.OriginalMaxHealth,
			Experience = (uint)xp.Item1,
			MaxExperience = (uint)xp.Item2,
			Initiative = actor.Initiative(),
			MultiplayerUserData = UIPlayerCharacterUtils.GetPlayerData(actor),
			ChestsLooted = UICharacterUtils.GetNumTimesLooted(actor),
			Perks = (from p in UIPlayerCharacterUtils.GetActivePassivePerks(character)
				select new PerkDTO(p)).ToArray(),
			AttackModifiers = UICharacterUtils.GetAttackModifiers(actor)?.OrderBy((AttackModifierInfo m) => UICharacterUtils.GetAttackModifierUIOrder(m.MathModifier)).ToArray(),
			PositiveConditions = UICharacterUtils.GetAllPositiveConditionsData(actor).ToArray(),
			NegativeConditions = UICharacterUtils.GetAllNegativeConditionsData(actor).ToArray(),
			Summons = UIPlayerCharacterUtils.GetSummons(actor).ToArray(),
			ActiveAbilities = GetActiveBonuses(actor).ToArray(),
			Attributes = (from att in UIPlayerCharacterUtils.GetPassiveAttributes(actor)
				select new StatusEffectDTO
				{
					Name = att.Item1,
					Description = att.Item2,
					Icon = att.Item3
				}).ToArray(),
			Equipment = UICharacterUtils.GetEquipment(actor).ToArray(),
			SmallItems = UICharacterUtils.GetSmallItems(actor).ToArray(),
			Pets = UICharacterUtils.GetPets(actor).ToArray(),
			Loot = UICharacterUtils.GetLoot(actor).ToArray(),
			ActivatedCards = actor.CharacterClass.ActivatedAbilityCards.ToArray(),
			CardsInHand = actor.CharacterClass.HandAbilityCards.ToArray(),
			DiscardedCards = actor.CharacterClass.DiscardedAbilityCards.ToArray(),
			LostCards = actor.CharacterClass.LostAbilityCards.Concat(actor.CharacterClass.PermanentlyLostAbilityCards).ToArray(),
			TakenCards = ((ScenarioScript.current != null && ScenarioScript.current.ShowTakenPile()) ? actor.CharacterClass.TakenCards.ToArray() : new CAbilityCard[0]),
			TakenCardPileKey = (ScenarioScript.current?.TakenPileNameKey() ?? string.Empty),
			RoundCards = actor.CharacterClass.RoundAbilityCards.ToArray(),
			ViewSummonSource = ViewSummonSource,
			ViewActiveAbilitySource = (ActiveAbilityDTO activeAbilityDTO) =>
			{
				ViewActiveAbilitySource(activeAbilityDTO.Bonus);
			},
			DeactivateActiveAbilityCard = (int cardID) =>
			{
				DeactivateActiveAbilityCard(cardID, actor, () =>
				{
					Singleton<FHScenarioViewController>.Instance.GeneralInfo.CloseInfo();
				});
			},
			Actor = actor
		};
	}

	public static IEnumerable<ActiveAbilityDTO> GetActiveBonuses(CActor actor)
	{
		IEnumerable<CActiveBonus> enumerable = from aa in UICharacterUtils.GetAllActiveBonuses(actor)
			where !aa.IsHiddenInUI()
			select aa;
		CActiveBonus[] array = (enumerable as CActiveBonus[]) ?? enumerable.ToArray();
		CActiveBonus[] array2 = array;
		foreach (CActiveBonus cActiveBonus in array2)
		{
			if (cActiveBonus.Layout == null)
			{
				ActiveBonusUIUtils.CheckForLayoutToGenerate(cActiveBonus, out var _, actor);
			}
		}
		IEnumerable<ActiveAbilityDTO> enumerable2 = from aa in array
			where aa.Layout != null
			select new ActiveAbilityDTO(aa, actor);
		foreach (ActiveAbilityDTO item in enumerable2)
		{
			yield return item;
		}
		ActiveAbilityDataSO.QuestItemActiveBonusData questItem = default;
		foreach (CObjectProp carriedQuestItem in actor.CarriedQuestItems)
		{
			if (UIInfoTools.Instance?.ActiveAbilityData?.GetQuestItemData(carriedQuestItem.SpecificPropType, out questItem) == true)
			{
				yield return new ActiveAbilityDTO(carriedQuestItem.PropGuid, LocalizationManager.GetTranslation(questItem.OverrideBonusName.IsNullOrEmpty() ? FHUILocFormatting.GetScenarioQuestItemName(carriedQuestItem) : questItem.OverrideBonusName), LocalizationManager.GetTranslation(questItem.OverrideBonusDescription.IsNullOrEmpty() ? FHUILocFormatting.GetScenarioQuestItemDesc(carriedQuestItem) : questItem.OverrideBonusDescription), questItem.Icon ?? UIInfoTools.Instance.CarryableQuestItemIconSprite);
			}
		}
		ActiveAbilityDataSO.ResourceActiveBonusData data = default;
		foreach (CCharacterResource characterResource in actor.CharacterResources)
		{
			if (UIInfoTools.Instance?.ActiveAbilityData?.GetResourceData(characterResource.ID, out data) == true)
			{
				string localizedName;
				if (data.NameLocKey.IsNullOrEmpty())
				{
					localizedName = string.Empty;
				}
				else
				{
					localizedName = (data.NameLocKey.Contains('$') ? CreateLayout.LocaliseText(data.NameLocKey, skipWarnings: true, characterResource.Amount.ToString()) : LocalizationManager.GetTranslation(data.NameLocKey, fixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: true));
				}
				string description;
				if (data.DescLocKey.IsNullOrEmpty())
				{
					description = string.Empty;
				}
				else
				{
					description = (data.DescLocKey.Contains('$') ? CreateLayout.LocaliseText(data.DescLocKey, skipWarnings: true, characterResource.Amount.ToString()) : LocalizationManager.GetTranslation(data.DescLocKey, fixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: true));
				}
				yield return new ActiveAbilityDTO(characterResource.ID, localizedName, description, data.Icon);
			}
		}
	}

	private static void ViewSummonSource(HeroSummonGeneralDTO dto)
	{
		if (dto.SourceCard.HasValue)
		{
			Singleton<FHScenarioViewController>.Instance.GeneralInfo.RequestGeneralInfo(GetAbilityCardInfo(CharacterClassManager.AllAbilityCards.FirstOrDefault((CAbilityCard c) => c.ID == dto.SourceCard.Value)));
		}
	}

	private static void ViewActiveAbilitySource(CActiveBonus bonus)
	{
		if (bonus == null)
		{
			return;
		}
		UIGeneralInfoModule generalInfo = Singleton<FHScenarioViewController>.Instance.GeneralInfo;
		CBaseCard baseCard = bonus.BaseCard;
		if (!(baseCard is CAbilityCard card))
		{
			if (!(baseCard is CMonsterAbilityCard) && baseCard is CItem item)
			{
				generalInfo.RequestGeneralInfo(GetItemInfo(item));
			}
		}
		else
		{
			generalInfo.RequestGeneralInfo(GetAbilityCardInfo(card));
		}
	}

	private static void DeactivateActiveAbilityCard(int cardID, CPlayerActor actor, UnityAction onFinished = null)
	{
		if (Choreographer.s_Choreographer.ThisPlayerHasTurnControl && actor.IsUnderMyControl())
		{
			CAbilityCard cAbilityCard = actor.CharacterClass.ActivatedAbilityCards.FirstOrDefault((CAbilityCard c) => c.ID == cardID);
			if (cAbilityCard != null)
			{
				Singleton<CardsHandManager>.Instance.DiscardActiveAbilityCard(actor, cAbilityCard, onFinished);
			}
		}
	}

	public static UIGeneralInfoCharacter.PlayerCharacterGeneralInfoData GetCharacter(CharacterDataDTO character, bool isOwned)
	{
		FHMapChoreographer instanceFast = Singleton<FHMapChoreographer>.InstanceFast;
		CampaignController campaignController = instanceFast.GameController;
		DefRepository defRepo = instanceFast.DefRepo;
		IEnumerable<ItemDataDTO> source = character.Items.Where((ItemDataDTO item) => campaignController.IsItemValid(defRepo.Items[item.ID]));
		return new UIGeneralInfoCharacter.PlayerCharacterGeneralInfoData
		{
			Character = CrossReference.ClassDefToECharacterModel(character.Class),
			Name = character.Name,
			IsOwner = isOwned,
			Illustration = UIInfoTools.Instance.GetCharacterConfigUI(CrossReference.ClassDefToECharacterModel(character.Class)).InfoIllustration,
			Level = character.Level,
			Health = ((character.Health.Value >= 0) ? ((uint)character.Health.Value) : 0u),
			MaxHealth = (uint)character.Health.Max,
			OriginalMaxHealth = (uint)character.Health.Max,
			Experience = (uint)character.Experience.Value,
			MaxExperience = (uint)character.Experience.Max,
			Initiative = -1,
			Equipment = (from i in source
				where i.ItemType != ItemEquipType.Small && i.IsEquipped
				select new ItemDataDTO(i)).ToArray(),
			SmallItems = (from i in source
				where i.ItemType == ItemEquipType.Small && i.IsEquipped
				select new ItemDataDTO(i)).ToArray(),
			Actor = null
		};
	}

	public static IGeneralInfoData GetPartySlotCharacter(PartySlotDTO partySlotDto)
	{
		if (ScenarioManager.Scenario != null)
		{
			return GetActor(Singleton<FHScenarioViewController>.Instance.GetService<PlayerViewService>().GetPlayerActor(partySlotDto.Character));
		}
		if (Singleton<FHMapChoreographer>.InstanceFast == null)
		{
			return null;
		}
		if (Singleton<FHMapChoreographer>.InstanceFast.View.GetService<PartyViewService>().TryGetCharacter(partySlotDto, out var characterState, out var isSlotOwner))
		{
			UIGeneralInfoCharacter.PlayerCharacterGeneralInfoData character = GetCharacter(characterState, isSlotOwner);
			character.MultiplayerUserData = partySlotDto.MultiplayerUserData;
			return character;
		}
		return null;
	}

	public static UIGeneralInfoCharacter.PlayerCharacterGeneralInfoData GetCharacter(CharacterState character, bool isOwner)
	{
		FHMapView view = Singleton<FHMapChoreographer>.InstanceFast.View;
		ECharacter character2 = CrossReference.ClassDefToECharacterModel(character.ClassDef);
		uint characterHealth = view.GameController.CharacterController.GetCharacterHealth(character.ClassDef, character.Level);
		uint num = characterHealth;
		CrossReference.GetExperienceProgress(character, out var currentProgress, out var maxProgress);
		IEnumerable<ItemDef> ownedItems = character.OwnedItems;
		if (view.GameController != null)
		{
			ownedItems = view.GameController.CharacterController.GetOwnedItems(character);
		}
		PerksViewService service = view.GetService<PerksViewService>();
		List<PerkDTO> learnedPerks = service.GetLearnedPerks(character);
		return new UIGeneralInfoCharacter.PlayerCharacterGeneralInfoData
		{
			Character = character2,
			Name = character.Name,
			IsOwner = isOwner,
			Illustration = UIInfoTools.Instance.GetCharacterConfigUI(character2).InfoIllustration,
			Level = character.Level,
			Health = characterHealth,
			MaxHealth = num,
			OriginalMaxHealth = num,
			Experience = currentProgress,
			MaxExperience = maxProgress,
			Initiative = -1,
			AttackModifiers = (from mod in service.GetActiveAttackModifiers(character)
				select new AttackModifierInfo(mod) into m
				orderby UICharacterUtils.GetAttackModifierUIOrder(m.MathModifier)
				select m).ToArray(),
			Perks = learnedPerks.Where((PerkDTO p) => p.IsTactical).ToArray(),
			Equipment = (from i in ownedItems
				where i.Type != ItemEquipType.Small && character.EquipedItemIDs.Contains(i.ID)
				select new ItemDataDTO(i)).ToArray(),
			SmallItems = (from i in ownedItems
				where i.Type == ItemEquipType.Small && character.EquipedItemIDs.Contains(i.ID)
				select new ItemDataDTO(i)).ToArray(),
			CardsInHand = character.SelectedCards.Select((ClassCardDef cardDef) => CharacterClassManager.AllAbilityCards.First((CAbilityCard cCard) => cCard.ID == cardDef.ID.ID)).ToArray(),
			Actor = null
		};
	}

	public static UIGeneralInfoCharacter.PlayerCharacterGeneralInfoData GetCharacter(ECharacter character)
	{
		return default;
	}

	public static UIGeneralInfoMonster.MonsterGeneralInfoData GetMonster(CEnemyActor enemyActor)
	{
		CMonsterClass monsterClass = enemyActor.MonsterClass;
		CMonsterAbilityCard roundAbilityCard = monsterClass.RoundAbilityCard;
		CMonsterAbilityCard cMonsterAbilityCard = (monsterClass.NextAbilityCardKnown ? monsterClass.NextAbilityCard : null);
		List<CMonsterAbilityCard> abilityCardsPool = monsterClass.AbilityCardsPool;
		List<CMonsterAbilityCard> list = null;
		CMonsterClass altMonsterClass = null;
		if (ScenarioManager.CurrentScenarioState != null && ScenarioScript.current is BossHasMonsterCardExtraTurnScenarioScript bossHasMonsterCardExtraTurnScenarioScript && bossHasMonsterCardExtraTurnScenarioScript.Boss == enemyActor)
		{
			altMonsterClass = bossHasMonsterCardExtraTurnScenarioScript.BossClass;
			list = altMonsterClass.AbilityCardsPool;
		}
		string iD = enemyActor.MonsterClass.ID;
		string modelName = enemyActor.MonsterClass.Models[enemyActor.ChosenModelIndex];
		return new UIGeneralInfoMonster.MonsterGeneralInfoData
		{
			ActorGuid = enemyActor.ActorGuid,
			ID = iD,
			Name = UICharacterUtils.GetEnemyName(enemyActor),
			Illustration = UIInfoTools.Instance.GetMonsterConfig(iD, modelName)?.Illustration,
			Level = (uint)enemyActor.Level,
			Stats = new BaseStatsDTO(enemyActor),
			BaseStats = new BaseStatsDTO(enemyActor.MonsterClass.GetMonsterStatsData(enemyActor.Level)),
			CharacterTokenAttributes = new CharacterTokenAttributes(enemyActor),
			Faction = enemyActor.Type,
			MonsterClass = monsterClass,
			ShowFactions = UICharacterUtils.HasFactions(),
			ActiveAbilities = GetActiveBonuses(enemyActor).ToArray(),
			NegativeConditions = UICharacterUtils.GetAllNegativeConditionsData(enemyActor).ToArray(),
			PositiveConditions = UICharacterUtils.GetAllPositiveConditionsData(enemyActor).ToArray(),
			Immunities = UICharacterUtils.GetImmunities(enemyActor).ToArray(),
			Innates = UICharacterUtils.GetInnateAbilities(enemyActor).ToArray(),
			AttackModifiers = (from m in UICharacterUtils.GetAttackModifiers(enemyActor)
				orderby UICharacterUtils.GetAttackModifierUIOrder(m.MathModifier)
				select m).ToArray(),
			RoundCard = ((roundAbilityCard != null) ? new MonsterCardDTO?(new MonsterCardDTO(roundAbilityCard, CardPileType.Round)) : ((MonsterCardDTO?)null)),
			NextCard = ((cMonsterAbilityCard != null) ? new MonsterCardDTO?(new MonsterCardDTO(cMonsterAbilityCard, CardPileType.Hand)) : ((MonsterCardDTO?)null)),
			Deck = (from c in abilityCardsPool
				select new MonsterCardDTO(c, GetMonsterCardPile(c.ID, monsterClass)) into c
				orderby c.Initiative
				select c).ToArray(),
			AltDeck = list?.Select((CMonsterAbilityCard c) => new MonsterCardDTO(c, GetMonsterCardPile(c.ID, altMonsterClass))).OrderBy((MonsterCardDTO c) => c.Initiative).ToArray(),
			AltMonsterClass = altMonsterClass,
			AltRoundCard = ((altMonsterClass?.RoundAbilityCard != null) ? new MonsterCardDTO?(new MonsterCardDTO(altMonsterClass.RoundAbilityCard, CardPileType.Round)) : ((MonsterCardDTO?)null)),
			GetMonsterCardContext = (int cardID) => new MonsterCardUIContext
			{
				CardYML = GetMonsterCardYML(cardID),
				EnemyForBaseStats = enemyActor,
				FallbackMonsterClass = enemyActor.MonsterClass,
				ShowFaction = UICharacterUtils.HasFactions(),
				CardPile = GetMonsterCardPile(cardID, monsterClass),
				OverrideLevel = enemyActor.Level
			},
			AltMonsterCardContext = ((altMonsterClass == null) ? null : ((Func<int, MonsterCardUIContext>)((int cardID) => new MonsterCardUIContext
			{
				CardYML = GetMonsterCardYML(cardID),
				EnemyForBaseStats = enemyActor,
				FallbackMonsterClass = altMonsterClass,
				ShowFaction = UICharacterUtils.HasFactions(),
				CardPile = GetMonsterCardPile(cardID, altMonsterClass),
				AltMonsterClass = altMonsterClass
			})))
		};
	}

	public static UIGeneralInfoMonster.MonsterGeneralInfoData GetMonster(CMonsterClass monster)
	{
		CMonsterAbilityCard roundAbilityCard = monster.RoundAbilityCard;
		CMonsterAbilityCard cMonsterAbilityCard = (monster.NextAbilityCardKnown ? monster.NextAbilityCard : null);
		return new UIGeneralInfoMonster.MonsterGeneralInfoData
		{
			ID = monster.ID,
			Name = LocalizationManager.GetTranslation(monster.LocKey),
			Illustration = UIInfoTools.Instance.GetMonsterConfig(monster)?.Illustration,
			Deck = (from c in monster.AbilityCardsPool
				select new MonsterCardDTO(c, GetMonsterCardPile(c.ID, monster)) into c
				orderby c.Initiative
				select c).ToArray(),
			Level = (uint)monster.StatLevel,
			Stats = new BaseStatsDTO(monster),
			BaseStats = new BaseStatsDTO(monster.GetMonsterStatsData(monster.StatLevel)),
			RoundCard = ((roundAbilityCard != null) ? new MonsterCardDTO?(new MonsterCardDTO(roundAbilityCard)) : ((MonsterCardDTO?)null)),
			NextCard = ((cMonsterAbilityCard != null) ? new MonsterCardDTO?(new MonsterCardDTO(cMonsterAbilityCard)) : ((MonsterCardDTO?)null)),
			MonsterClass = monster,
			Immunities = UICharacterUtils.GetImmunities(monster).ToArray(),
			Innates = UICharacterUtils.GetInnateAbilities(monster).ToArray()
		};
	}

	public static CardPileType GetMonsterCardPile(int cardId, CMonsterClass monsterClass)
	{
		_ = cardId;
		if (monsterClass == null)
		{
			return CardPileType.None;
		}
		CMonsterAbilityCard roundAbilityCard = monsterClass.RoundAbilityCard;
		if (roundAbilityCard != null && roundAbilityCard.ID == cardId && PhaseManager.PhaseType >= CPhase.PhaseType.MonsterClassesSelectAbilityCards)
		{
			return CardPileType.Round;
		}
		if (monsterClass.AbilityCards.Any((CMonsterAbilityCard c) => c.ID == cardId))
		{
			return CardPileType.Hand;
		}
		if (monsterClass.DiscardedAbilityCards.Any((CMonsterAbilityCard c) => c.ID == cardId))
		{
			return CardPileType.Discarded;
		}
		return CardPileType.None;
	}

	public static UIGeneralInfoSummon.SummonGeneralInfoData GetSummon(CHeroSummonActor summon)
	{
		_ = summon.SummonData;
		CAbilityAttack.GetAttackBonuses(summon, out var _, out var _, out var _, out var _, out var _);
		_ = summon?.Summoner?.CharacterClass?.AbilityCardArt;
		SummonConfigUI summonConfig = UIInfoTools.Instance.GetSummonConfig(summon.HeroSummonClass);
		CAbilityCard summonCard = summon.FindSummonCard();
		return new UIGeneralInfoSummon.SummonGeneralInfoData
		{
			ActorGuid = summon.ActorGuid,
			ID = summon.HeroSummonClass.ID,
			Name = LocalizationManager.GetTranslation(summon.ActorLocKey()),
			Illustration = (summonConfig?.Illusration ?? summonConfig?.Portrait),
			Level = (uint)summon.Level,
			Stats = new BaseStatsDTO(summon),
			BaseStats = new BaseStatsDTO(summon.HeroSummonClass.SummonYML, summon.Level),
			CharacterTokenAttributes = new CharacterTokenAttributes(summon),
			ActiveAbilities = GetActiveBonuses(summon).ToArray(),
			PositiveConditions = UICharacterUtils.GetAllPositiveConditionsData(summon).ToArray(),
			NegativeConditions = UICharacterUtils.GetAllNegativeConditionsData(summon).ToArray(),
			Immunities = UICharacterUtils.GetImmunities(summon).ToArray(),
			Innates = UICharacterUtils.GetInnateAbilities(summon).ToArray(),
			Initiative = summon.Initiative(),
			AbilityCardSource = new CardDTO(summonCard),
			ViewCardSource = () =>
			{
				Singleton<FHScenarioViewController>.Instance.GeneralInfo.RequestGeneralInfo(GetAbilityCardInfo(summonCard));
			}
		};
	}

	public static UIGeneralInfoSpawner.SpawnerGeneralInfoData GetSpawnerData(CTile tile)
	{
		return new UIGeneralInfoSpawner.SpawnerGeneralInfoData
		{
			Image = null,
			Spawners = tile.m_Spawners.Select(GetSpawnerDTO).ToArray()
		};
	}

	public static UIGeneralInfoSpawner.SpawnerGeneralInfoData GetSpawnerData(CSpawner spawner)
	{
		return new UIGeneralInfoSpawner.SpawnerGeneralInfoData
		{
			Image = null,
			Spawners = new SpawnerDTO[1] { GetSpawnerDTO(spawner) }
		};
	}

	public static SpawnerDTO GetSpawnerDTO(CSpawner spawner)
	{
		CMonsterClass cMonsterClass = null;
		cMonsterClass = spawner.GetNextMonsterClassToSpawn(ScenarioManager.CurrentScenarioState.Players.Count);
		string text = spawner.SpawnerData.SpawnerHoverNameLoc;
		string text2 = string.Empty;
		if (text.Contains('|'))
		{
			string[] array = text.Split('|');
			if (array.Length == 2)
			{
				text = array[0];
				text2 = array[1];
			}
		}
		string name = (text.IsNullOrEmpty() ? LocalizationManager.GetTranslation("GUI_SPAWNER") : LocalizationManager.GetTranslation(text));
		return new SpawnerDTO
		{
			Name = name,
			MonsterData = ((cMonsterClass == null) ? ((MonsterSimpleDTO?)null) : new MonsterSimpleDTO?(new MonsterSimpleDTO(cMonsterClass))),
			Description = (text2.IsNOTNullOrEmpty() ? LocalizationManager.GetTranslation(text2) : string.Empty)
		};
	}

	public static CObjectProp GetPropOnTile(CClientTile clientTile)
	{
		if (!clientTile.m_Tile.m_Props.Any())
		{
			return null;
		}
		return clientTile.m_Tile.m_Props.OrderByDescending((CObjectProp p) => _propPriority.IndexOf(p.ObjectType)).First();
	}

	public static IGeneralInfoData GetPropData(UnityGameEditorObject propGameObject)
	{
		if (propGameObject == null)
		{
			return null;
		}
		CObjectProp cObjectProp = ScenarioManager.CurrentScenarioState.Props.SingleOrDefault((CObjectProp s) => s.InstanceName == propGameObject.name);
		if (cObjectProp != null)
		{
			return GetPropData(cObjectProp, propGameObject);
		}
		return null;
	}

	public static IGeneralInfoData GetPropData(CObjectProp prop, UnityGameEditorObject propEditorObject)
	{
		if (!(prop is CObjectChest) || (object)propEditorObject == null || propEditorObject.m_ObjectType != ScenarioManager.ObjectImportType.GoalChest)
		{
			if (!(prop is CObjectChest chest))
			{
				if (!(prop is CObjectDoor cObjectDoor))
				{
					if (!(prop is CObjectGoldPile goldPile))
					{
						if (!(prop is CObjectDifficultTerrain difficultTerrain))
						{
							if (!(prop is CObjectHazardousTerrain hazardousTerrain))
							{
								if (!(prop is CObjectIceTerrain iceTerrain))
								{
									if (!(prop is CObjectMonsterGrave))
									{
										if (!(prop is CObjectObstacle obstacle))
										{
											if (!(prop is CObjectPortal portal))
											{
												if (!(prop is CObjectPressurePlate pressurePlate))
												{
													if (!(prop is CObjectQuestItem questItem))
													{
														if (!(prop is CObjectResource resource))
														{
															if (!(prop is CObjectShadow shadow))
															{
																if (!(prop is CObjectTerrainVisual terrainVisual))
																{
																	if (prop is CObjectTrap trap)
																	{
																		return GetTrapData(trap, propEditorObject);
																	}
																	return GetDefaultData(prop, propEditorObject);
																}
																return GetTerrainData(terrainVisual, propEditorObject);
															}
															return GetShadowPropData(shadow, propEditorObject);
														}
														return GetResourcePropData(resource, propEditorObject);
													}
													return GetQuestItemData(questItem, propEditorObject);
												}
												return GetPressurePlateData(pressurePlate, propEditorObject);
											}
											return GetPortalData(portal, propEditorObject);
										}
										return GetObstacleData(obstacle, propEditorObject);
									}
									return null;
								}
								return GetTerrainData(iceTerrain, propEditorObject);
							}
							return GetTerrainData(hazardousTerrain, propEditorObject);
						}
						return GetTerrainData(difficultTerrain, propEditorObject);
					}
					return GetGoldPileData(goldPile, propEditorObject);
				}
				object result;
				if (cObjectDoor.DoorType != CObjectDoor.EDoorType.EmptyDoor && cObjectDoor.DoorType != CObjectDoor.EDoorType.EmptyDoorThin)
				{
					IGeneralInfoData generalInfoData = GetDoorData(cObjectDoor, propEditorObject);
					result = generalInfoData;
				}
				else
				{
					result = null;
				}
				return (IGeneralInfoData)result;
			}
			return GetTreasureChestData(chest, propEditorObject);
		}
		return GetGoalChestData(prop, propEditorObject);
	}

	private static string GetPropName(CObjectProp prop, string defaultKey = "")
	{
		string text = (prop.PropCustomLocKey.IsNOTNullOrEmpty() ? prop.PropCustomLocKey : prop.PropHealthDetails?.CustomLocKey);
		string text2 = string.Empty;
		bool flag = prop is CObjectObstacle { BlockLOS: not false } cObjectObstacle && cObjectObstacle.IgnoresFlyAndJump;
		if (prop.CanLootLocKey.IsNOTNullOrEmpty())
		{
			text2 = FHUILocFormatting.GetCanBeLootedByProp(prop);
		}
		else if (text.IsNOTNullOrEmpty() && prop.CustomLocKeyArgs != null && prop.CustomLocKeyArgs.Any())
		{
			text2 = LocalizationManager.GetTranslation(text);
			text2 = string.Format(text2, prop.CustomLocKeyArgs.ToArray());
		}
		else if (prop.CustomLocKeyArgs != null && prop.CustomLocKeyArgs.Any())
		{
			text2 = string.Format(text2, prop.CustomLocKeyArgs.ToArray());
		}
		else if (flag)
		{
			text2 = LocalizationManager.GetTranslation((!text.IsNullOrEmpty()) ? text : "WALL_TOOLTIP");
		}
		else if (text.IsNOTNullOrEmpty() || defaultKey.IsNOTNullOrEmpty())
		{
			text2 = LocalizationManager.GetTranslation((!text.IsNullOrEmpty()) ? text : defaultKey);
		}
		return text2;
	}

	private static string GetPropDescription(CObjectProp prop, string defaultKey = "", bool ignoreCustomDescription = false)
	{
		string text = ((!ignoreCustomDescription && prop.PropCustomDescriptionLocKey.IsNOTNullOrEmpty()) ? prop.PropCustomDescriptionLocKey : defaultKey);
		string text2 = string.Empty;
		if (!text.IsNullOrEmpty() && LocalizationManager.TryGetTranslation(text, out var translation))
		{
			text2 = translation;
		}
		if (prop.OverrideDisallowDestroyAndMove)
		{
			text2 = text2 + "\n" + LocalizationManager.GetTranslation("IMMOVABLE_OBSTACLE_DESCR_TOOLTIP");
		}
		return text2;
	}

	public static PropImage GetPropImage(UnityGameEditorObject propEditorObject, Sprite fallbackImage = null)
	{
		if (propEditorObject == null)
		{
			return new PropImage
			{
				Sprite = null,
				FallbackSprite = fallbackImage
			};
		}
		PreviewIconReferenceHolder[] componentsInChildren = propEditorObject.GetComponentsInChildren<PreviewIconReferenceHolder>();
		int num = -1;
		PreviewIconReferenceHolder previewIconReferenceHolder = componentsInChildren.FirstOrDefault();
		PreviewIconReferenceHolder[] array = componentsInChildren;
		foreach (PreviewIconReferenceHolder previewIconReferenceHolder2 in array)
		{
			int num2 = GetComponentDepth(propEditorObject.transform, previewIconReferenceHolder2.transform);
			if (num2 > num)
			{
				num = num2;
				previewIconReferenceHolder = previewIconReferenceHolder2;
			}
		}
		if (previewIconReferenceHolder == null)
		{
			return new PropImage
			{
				Sprite = null,
				FallbackSprite = fallbackImage
			};
		}
		return new PropImage
		{
			Sprite = previewIconReferenceHolder.PreviewIcon,
			FallbackSprite = fallbackImage
		};
		static int GetComponentDepth(Transform root, Transform component, int currentDepth = 0)
		{
			if (component.parent == null || component.parent == root.parent)
			{
				return currentDepth;
			}
			return GetComponentDepth(root, component.parent, ++currentDepth);
		}
	}

	public static UIGeneralInfoTerrain.TerrainGeneralInfoData GetTerrainData(CObjectDifficultTerrain difficultTerrain, UnityGameEditorObject propEditorObject)
	{
		return new UIGeneralInfoTerrain.TerrainGeneralInfoData
		{
			Name = GetPropName(difficultTerrain, "DIFFICULT_TERRAIN_TOOLTIP"),
			Description = GetPropDescription(difficultTerrain, "DIFFICULT_TERRAIN_DESC"),
			Image = GetPropImage(propEditorObject, UIInfoTools.Instance.GetPropDefaultImage(propEditorObject.m_ObjectType)),
			Statuses = new StatusEffectDTO[1]
			{
				new StatusEffectDTO("MoveCost", string.Format("{0} : {1}", LocalizationManager.GetTranslation("MoveCost"), 2), string.Empty, 2, UIInfoTools.Instance.Move, StatusEffectDTO.StatusEffectType.Negative)
			}
		};
	}

	public static UIGeneralInfoTerrain.TerrainGeneralInfoData GetTerrainData(CObjectHazardousTerrain hazardousTerrain, UnityGameEditorObject propEditorObject)
	{
		if (ScenarioManager.CurrentScenarioState == null)
		{
			return default;
		}
		IEnumerable<StatusEffectDTO> source = new StatusEffectDTO[1]
		{
			new StatusEffectDTO("Damage", "Damage", string.Empty, ScenarioManager.HazardousTerrainDamage, UIInfoTools.Instance.Damage, StatusEffectDTO.StatusEffectType.Negative, localize: true)
		};
		StatusEffectDTO ownerEffect = UIPropUtils.GetOwnerEffect(hazardousTerrain);
		if (ownerEffect != null)
		{
			source = source.Append(ownerEffect);
		}
		return new UIGeneralInfoTerrain.TerrainGeneralInfoData
		{
			Name = GetPropName(hazardousTerrain, "HAZARDOUS_TERRAIN_TOOLTIP"),
			Description = GetPropDescription(hazardousTerrain, "HAZARDOUS_TERRAIN_DESC"),
			Image = GetPropImage(propEditorObject, UIInfoTools.Instance.GetPropDefaultImage(propEditorObject.m_ObjectType)),
			Statuses = source.ToArray()
		};
	}

	public static IGeneralInfoData GetTerrainData(CObjectTerrainVisual terrainVisual, UnityGameEditorObject propEditorObject)
	{
		if (ScenarioManager.CurrentScenarioState == null)
		{
			return null;
		}
		string text = (terrainVisual.PropCustomLocKey.IsNOTNullOrEmpty() ? terrainVisual.PropCustomLocKey : terrainVisual.PropHealthDetails?.CustomLocKey);
		string propCustomDescriptionLocKey = terrainVisual.PropCustomDescriptionLocKey;
		if (text.IsNullOrEmpty() && propCustomDescriptionLocKey.IsNullOrEmpty())
		{
			return null;
		}
		return new UIGeneralInfoTerrain.TerrainGeneralInfoData
		{
			Name = LocalizationManager.GetTranslation(text),
			Description = GetPropDescription(terrainVisual, string.Empty),
			Image = GetPropImage(propEditorObject, UIInfoTools.Instance.GetPropDefaultImage(propEditorObject.m_ObjectType)),
			Statuses = null
		};
	}

	public static UIGeneralInfoTerrain.TerrainGeneralInfoData GetTerrainData(CObjectIceTerrain iceTerrain, UnityGameEditorObject propEditorObject)
	{
		return new UIGeneralInfoTerrain.TerrainGeneralInfoData
		{
			Name = LocalizationManager.GetTranslation("ICE_TERRAIN_TOOLTIP"),
			Description = GetPropDescription(iceTerrain, "ICY_TERRAIN_DESC"),
			Image = GetPropImage(propEditorObject, UIInfoTools.Instance.GetPropDefaultImage(propEditorObject.m_ObjectType))
		};
	}

	public static UIGeneralInfoProp.PropGeneralInfoData GetDoorData(CObjectDoor door, UnityGameEditorObject propEditorObject)
	{
		UIGeneralInfoProp.PropGeneralInfoData result = new UIGeneralInfoProp.PropGeneralInfoData(door);
		string name;
		if (door.DoorIsOpen)
		{
			name = LocalizationManager.GetTranslation("DOORWAY_TOOLTIP");
		}
		else
		{
			name = (door.DoorIsLocked ? LocalizationManager.GetTranslation("LOCKED_DOOR_TOOLTIP") : LocalizationManager.GetTranslation("CLOSED_DOOR_TOOLTIP"));
		}
		result.Name = name;
		result.Description = GetPropDescription(door, "DOOR_DESC");
		result.Image = GetPropImage(propEditorObject, UIInfoTools.Instance.GetPropDefaultImage(propEditorObject.m_ObjectType));
		return result;
	}

	public static UIGeneralInfoProp.PropGeneralInfoData GetPortalData(CObjectPortal portal, UnityGameEditorObject propEditorObject)
	{
		UIGeneralInfoProp.PropGeneralInfoData result = new UIGeneralInfoProp.PropGeneralInfoData(portal);
		result.Name = LocalizationManager.GetTranslation("PORTAL_TOOLTIP");
		result.Description = GetPropDescription(portal, "PORTAL_DESCR_TOOLTIP");
		result.Image = GetPropImage(propEditorObject, UIInfoTools.Instance.GetPropDefaultImage(propEditorObject.m_ObjectType));
		return result;
	}

	public static UIGeneralInfoProp.PropGeneralInfoData GetPressurePlateData(CObjectPressurePlate pressurePlate, UnityGameEditorObject propEditorObject)
	{
		UIGeneralInfoProp.PropGeneralInfoData result = new UIGeneralInfoProp.PropGeneralInfoData(pressurePlate);
		result.Name = GetPropName(pressurePlate, "PRESSURE_PLATE_TOOLTIP");
		result.Description = GetPropDescription(pressurePlate, "PRESSURE_PLATE_DESC");
		result.Image = GetPropImage(propEditorObject, UIInfoTools.Instance.GetPropDefaultImage(propEditorObject.m_ObjectType));
		return result;
	}

	public static UIGeneralInfoProp.PropGeneralInfoData GetQuestItemData(CObjectQuestItem questItem, UnityGameEditorObject prop)
	{
		string translation = LocalizationManager.GetTranslation(FHUILocFormatting.GetScenarioQuestItemName(questItem));
		string translation2 = LocalizationManager.GetTranslation(FHUILocFormatting.GetScenarioQuestItemDesc(questItem));
		UIGeneralInfoProp.PropGeneralInfoData result = new UIGeneralInfoProp.PropGeneralInfoData(questItem);
		result.Name = translation;
		result.Description = translation2;
		result.Statuses = new StatusEffectDTO[1]
		{
			new StatusEffectDTO(translation, translation, translation2, 1, UIInfoTools.Instance.CarryableQuestItemIconSprite, StatusEffectDTO.StatusEffectType.Permanent)
		};
		result.Image = GetPropImage(prop, UIInfoTools.Instance.GetPropDefaultImage(prop.m_ObjectType));
		return result;
	}

	public static UIGeneralInfoProp.PropGeneralInfoData GetResourcePropData(CObjectResource resource, UnityGameEditorObject propEditorObject)
	{
		UIGeneralInfoProp.PropGeneralInfoData result = new UIGeneralInfoProp.PropGeneralInfoData(resource);
		result.Name = (resource.PropCustomLocKey.IsNOTNullOrEmpty() ? PopulatePropArgs(LocalizationManager.GetTranslation(resource.PropCustomLocKey), resource.CustomLocKeyArgs) : LocalizationManager.GetTranslation(FHUILocFormatting.GetResourcePropName(resource)));
		result.Description = GetResourceDescription(resource);
		result.Image = GetPropImage(propEditorObject, UIInfoTools.Instance.GetPropDefaultImage(propEditorObject.m_ObjectType));
		return result;
	}

	private static string PopulatePropArgs(string localizedText, List<object> customLocKeyArgs)
	{
		if (localizedText == null || customLocKeyArgs == null || customLocKeyArgs.Count == 0)
		{
			return localizedText;
		}
		return string.Format(localizedText, customLocKeyArgs.ToArray());
	}

	public static string GetResourceDescription(CObjectResource resource, bool ignoreCustomDescription = false)
	{
		string text = GetPropDescription(resource, FHUILocFormatting.GetResourcePropDesc(resource), ignoreCustomDescription);
		if (!string.IsNullOrEmpty(text) && resource.HasLimitedLoot)
		{
			string term = "GUI_REMAINING_RESOURCE_LOOT";
			text = text + "\n" + string.Format(LocalizationManager.GetTranslation(term), resource.RemainingLoot.ToString());
		}
		return text;
	}

	public static UIGeneralInfoProp.PropGeneralInfoData GetShadowPropData(CObjectShadow shadow, UnityGameEditorObject propEditorObject)
	{
		UIGeneralInfoProp.PropGeneralInfoData result = new UIGeneralInfoProp.PropGeneralInfoData(shadow);
		result.Name = LocalizationManager.GetTranslation("FH/SHADOW_TOOLTIP");
		result.Description = GetPropDescription(shadow, "FH/SHADOW_DESC");
		result.Image = GetPropImage(propEditorObject, UIInfoTools.Instance.GetPropDefaultImage(propEditorObject.m_ObjectType));
		return result;
	}

	public static UIGeneralInfoProp.PropGeneralInfoData GetTreasureChestData(CObjectChest chest, UnityGameEditorObject propEditorObject)
	{
		return new UIGeneralInfoProp.PropGeneralInfoData
		{
			Name = GetPropName(chest, "TREASURE_CHEST_TOOLTIP"),
			Description = GetPropDescription(chest, "TREASURE_CHEST_DESC"),
			Image = GetPropImage(propEditorObject, UIInfoTools.Instance.GetPropDefaultImage(propEditorObject.m_ObjectType))
		};
	}

	public static UIGeneralInfoProp.PropGeneralInfoData GetGoalChestData(CObjectProp goalChest, UnityGameEditorObject propEditorObject)
	{
		return new UIGeneralInfoProp.PropGeneralInfoData
		{
			Name = GetPropName(goalChest, "GOAL_CHEST_TOOLTIP"),
			Description = GetPropDescription(goalChest, "GOAL_CHEST_DESC"),
			Image = GetPropImage(propEditorObject, UIInfoTools.Instance.GetPropDefaultImage(propEditorObject.m_ObjectType))
		};
	}

	public static UIGeneralInfoProp.PropGeneralInfoData GetGoldPileData(CObjectGoldPile goldPile, UnityGameEditorObject propEditorObject)
	{
		return new UIGeneralInfoProp.PropGeneralInfoData
		{
			Name = LocalizationManager.GetTranslation("LOOT_TOKEN_TOOLTIP"),
			Description = GetPropDescription(goldPile, "LOOT_TOKEN_DESC"),
			Image = GetPropImage(propEditorObject, UIInfoTools.Instance.GetPropDefaultImage(propEditorObject.m_ObjectType))
		};
	}

	public static string GetObstacleName(CObjectObstacle obstacle)
	{
		return "OBSTACLE_TOOLTIP";
	}

	private static UIGeneralInfoProp.PropGeneralInfoData GetObstacleData(CObjectObstacle obstacle, UnityGameEditorObject propEditorObject)
	{
		return new UIGeneralInfoProp.PropGeneralInfoData
		{
			PropType = obstacle.PropType,
			Name = GetPropName(obstacle, GetObstacleName(obstacle)),
			Description = GetPropDescription(obstacle, "OBSTACLE_DESC"),
			Image = GetPropImage(propEditorObject),
			Health = GetPropHealth(obstacle)
		};
	}

	public static UIGeneralInfoProp.PropGeneralInfoData GetTrapData(CObjectTrap trap, UnityGameEditorObject propEditorObject)
	{
		HexViewService.PropEffectsParams propEffectsParams = new HexViewService.PropEffectsParams(trap);
		IEnumerable<StatusEffectDTO> enumerable = propEffectsParams.Conditions.Select((CCondition.ENegativeCondition c) => new StatusEffectDTO(c));
		if (propEffectsParams.PositiveConditions != null)
		{
			enumerable = enumerable.Concat(propEffectsParams.PositiveConditions.Select((CCondition.EPositiveCondition c) => new StatusEffectDTO(c)));
		}
		if (propEffectsParams.DamageValue > 0)
		{
			string translation = LocalizationManager.GetTranslation("Damage");
			enumerable = enumerable.Append(new StatusEffectDTO("Damage", translation, $"{translation}: {propEffectsParams.DamageValue}", propEffectsParams.DamageValue, UIInfoTools.Instance.DamageSprite, StatusEffectDTO.StatusEffectType.Negative));
		}
		if (propEffectsParams.HealValue > 0)
		{
			string translation2 = LocalizationManager.GetTranslation("Heal");
			enumerable = enumerable.Append(new StatusEffectDTO("Heal", translation2, $"{translation2} : {propEffectsParams.HealValue}", propEffectsParams.HealValue, UIInfoTools.Instance.Heal, StatusEffectDTO.StatusEffectType.Positive));
		}
		return new UIGeneralInfoProp.PropGeneralInfoData
		{
			PropType = trap.PropType,
			Name = LocalizationManager.GetTranslation(trap.PrefabName),
			Description = GetPropDescription(trap, "TRAP_DESC"),
			Image = GetPropImage(propEditorObject, UIInfoTools.Instance.GetPropDefaultImage(propEditorObject.m_ObjectType)),
			Health = GetPropHealth(trap),
			Statuses = enumerable.ToArray()
		};
	}

	public static UIGeneralInfoProp.PropGeneralInfoData? GetDefaultData(CObjectProp prop, UnityGameEditorObject propEditorObject)
	{
		string propName = GetPropName(prop);
		string text = GetPropDescription(prop);
		if (propName.IsNullOrEmpty() && text.IsNullOrEmpty())
		{
			return null;
		}
		if (text.IsNullOrEmpty() && prop.ObjectType == ScenarioManager.ObjectImportType.VFXProp)
		{
			text = "";
			if (prop.PropTile.IsBlocked)
			{
				text = text + ParagraphSpacing(text) + LocalizationManager.GetTranslation("OBSTACLE_DESC");
			}
			if (prop.PropTile.TreatAsHazardous)
			{
				text = text + ParagraphSpacing(text) + LocalizationManager.GetTranslation("HAZARDOUS_TERRAIN_DESC");
			}
			if (prop.PropTile.TreatAsDifficult)
			{
				text = text + ParagraphSpacing(text) + LocalizationManager.GetTranslation("DIFFICULT_TERRAIN_DESC");
			}
		}
		return new UIGeneralInfoProp.PropGeneralInfoData
		{
			PropType = prop.PropType,
			Name = propName,
			Description = text,
			Image = GetPropImage(propEditorObject, UIInfoTools.Instance.GetPropDefaultImage(propEditorObject.m_ObjectType)),
			Statuses = GetCustomStatues().ToArray()
		};
		IEnumerable<StatusEffectDTO> GetCustomStatues()
		{
			if (prop.ObjectType == ScenarioManager.ObjectImportType.VFXProp && prop.PropTile.TreatAsHazardous)
			{
				yield return new StatusEffectDTO("Damage", "Damage", string.Empty, ScenarioManager.HazardousTerrainDamage, UIInfoTools.Instance.Damage, StatusEffectDTO.StatusEffectType.Negative, localize: true);
			}
			if (prop.ObjectType == ScenarioManager.ObjectImportType.VFXProp && prop.PropTile.TreatAsDifficult)
			{
				yield return new StatusEffectDTO("MoveCost", string.Format("{0} : {1}", LocalizationManager.GetTranslation("MoveCost"), 2), string.Empty, 2, UIInfoTools.Instance.Move, StatusEffectDTO.StatusEffectType.Negative);
			}
		}
		static string ParagraphSpacing(string currentText, string spacing = "\n\n")
		{
			if (!currentText.IsNullOrEmpty())
			{
				return spacing;
			}
			return "";
		}
	}

	private static RangeDataInt? GetPropHealth(CObjectProp prop)
	{
		if (ScenarioManager.CurrentScenarioState == null || prop.PropHealthDetails == null || !prop.PropHealthDetails.HasHealth)
		{
			return null;
		}
		int currentHealth = prop.PropHealthDetails.CurrentHealth;
		int propStartingHealth = prop.PropHealthDetails.GetPropStartingHealth();
		return new RangeDataInt(currentHealth, propStartingHealth);
	}

	public static SpriteWithTooltipDTO ToSimpleIconDTO(this HeroSummonGeneralDTO summon)
	{
		return new SpriteWithTooltipDTO
		{
			Title = summon.Name,
			Sprite = summon.Icon
		};
	}

	public static UIPrefabTooltipTarget.TooltipInitParams GetTooltipFromSpriteItemDto(GameObject tooltipPrototype, SpriteWithTooltipDTO dto, string hints = null)
	{
		return new UIPrefabTooltipTarget.TooltipInitParams
		{
			Prefab = tooltipPrototype,
			OnObjectCreatedCallback = (GameObject go) =>
			{
				UIGenericTooltipRenderer.Params obj = new UIGenericTooltipRenderer.Params
				{
					Icon = dto.Sprite,
					CaptionLiteral = dto.Title,
					DescriptionLiteral = dto.Description,
					Hints = hints
				};
				go.GetComponent<UIGenericTooltipRenderer>()?.Init(obj);
			}
		};
	}
}
