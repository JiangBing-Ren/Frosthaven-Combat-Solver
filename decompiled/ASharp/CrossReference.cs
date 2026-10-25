using System;
using System.Linq;
using FHGUI.DTO;
using FHRL;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;

public static class CrossReference
{
	public static ECharacter ClassIdToECharacterModel(string classId)
	{
		CCharacterClass cCharacterClass = CharacterClassManager.Classes.LastOrDefault((CCharacterClass s) => s.CharacterID.ToLowerInvariant() == classId?.ToLowerInvariant());
		int num;
		if (cCharacterClass == null)
		{
			num = 0;
		}
		else
		{
			num = (int)cCharacterClass.CharacterModel;
			if (num != 0)
			{
				goto IL_0048;
			}
		}
		Log.Error(classId + " is not a recognized ECharacter!");
		goto IL_0048;
		IL_0048:
		return (ECharacter)num;
	}

	public static string ClassIdToCharacterSprite(string classId)
	{
		ECharacter eCharacter = CharacterClassManager.Classes.LastOrDefault((CCharacterClass s) => s.CharacterID.ToLowerInvariant() == classId?.ToLowerInvariant())?.CharacterModel ?? ECharacter.None;
		if (eCharacter == ECharacter.None)
		{
			Log.Error(classId + " is not a recognized ECharacter!");
		}
		string text = eCharacter.ToString();
		if (eCharacter == ECharacter.Metalmosaic)
		{
			text = "MetalMosaic";
		}
		if (eCharacter == ECharacter.Crashingtide)
		{
			text = "CrashingTide";
		}
		if (eCharacter == ECharacter.Painconduit)
		{
			text = "PainConduit";
		}
		if (eCharacter == ECharacter.Frozenfist)
		{
			text = "FrozenFist";
		}
		return "<sprite name=" + text + ">";
	}

	public static ECharacter ClassDefToECharacterModel(CharacterClassDef classDef)
	{
		return ClassIdToECharacterModel(classDef.ID.ID);
	}

	public static string ECharacterModelToClassId(ECharacter model)
	{
		return model.ToString() + "ID";
	}

	public static CharacterClassDef ECharacterModelToClassDef(string id)
	{
		CCharacterClass cCharacterClass = CharacterClassManager.Classes.FirstOrDefault((CCharacterClass s) => s.ID.ToLowerInvariant() == id?.ToLowerInvariant());
		if (cCharacterClass == null)
		{
			return null;
		}
		DefRepository defRepository = Singleton<FHMapChoreographer>.InstanceFast?.DefRepo;
		if (defRepository == null)
		{
			Log.Error("DefRepo missing!");
			return null;
		}
		CharacterClassID characterClassID = new CharacterClassID(cCharacterClass.CharacterID);
		if (!defRepository.CharacterClasses.TryGetValue(characterClassID, out var value))
		{
			Log.Error(string.Format("{0} with {1} {2}!", "CharacterClassDef", "CharacterClassID", characterClassID));
			return null;
		}
		return value;
	}

	public static string ECharacterModelToClassLocKey(string id)
	{
		CCharacterClass cCharacterClass = CharacterClassManager.Classes.LastOrDefault((CCharacterClass s) => s.ID.ToLowerInvariant() == id?.ToLowerInvariant());
		if (cCharacterClass == null)
		{
			return string.Empty;
		}
		return cCharacterClass.LocKey;
	}

	public static string ECharacterModelToClassLocKey(ECharacter character)
	{
		return ECharacterModelToClassLocKey(ECharacterModelToClassId(character));
	}

	public static void GetExperienceProgress(CharacterState character, out uint currentProgress, out uint maxProgress)
	{
		if (Singleton<FHMapChoreographer>.InstanceFast == null)
		{
			currentProgress = 0u;
			maxProgress = 0u;
		}
		else
		{
			Singleton<FHMapChoreographer>.InstanceFast.GameController.CharacterController.GetExperienceProgress(character, out currentProgress, out maxProgress);
		}
	}

	public static bool IsMaxLevel(CharacterState character)
	{
		if (Singleton<FHMapChoreographer>.InstanceFast == null)
		{
			return false;
		}
		return Singleton<FHMapChoreographer>.InstanceFast.GameController.CharacterController.IsMaxLevel(character);
	}

	public static ElementInfusionBoardManager.EElement CardEnhancementTypeToEElement(CardEnhancementEffectType enhancement)
	{
		return enhancement switch
		{
			CardEnhancementEffectType.ElementFire => ElementInfusionBoardManager.EElement.Fire, 
			CardEnhancementEffectType.ElementAir => ElementInfusionBoardManager.EElement.Air, 
			CardEnhancementEffectType.ElementIce => ElementInfusionBoardManager.EElement.Ice, 
			CardEnhancementEffectType.ElementEarth => ElementInfusionBoardManager.EElement.Earth, 
			CardEnhancementEffectType.ElementLight => ElementInfusionBoardManager.EElement.Light, 
			CardEnhancementEffectType.ElementDark => ElementInfusionBoardManager.EElement.Dark, 
			CardEnhancementEffectType.ElementWild => ElementInfusionBoardManager.EElement.Any, 
			_ => ElementInfusionBoardManager.EElement.Any, 
		};
	}

	public static CardEnhancementEffectType EElementToCardEnhancementType(ElementInfusionBoardManager.EElement element)
	{
		return element switch
		{
			ElementInfusionBoardManager.EElement.Fire => CardEnhancementEffectType.ElementFire, 
			ElementInfusionBoardManager.EElement.Ice => CardEnhancementEffectType.ElementIce, 
			ElementInfusionBoardManager.EElement.Air => CardEnhancementEffectType.ElementAir, 
			ElementInfusionBoardManager.EElement.Earth => CardEnhancementEffectType.ElementEarth, 
			ElementInfusionBoardManager.EElement.Light => CardEnhancementEffectType.ElementLight, 
			ElementInfusionBoardManager.EElement.Dark => CardEnhancementEffectType.ElementDark, 
			ElementInfusionBoardManager.EElement.Any => CardEnhancementEffectType.ElementWild, 
			_ => throw new ArgumentOutOfRangeException("element", element, null), 
		};
	}

	public static CItem.EItemSlot EItemSlotToItemEquipType(ItemEquipType element)
	{
		return element switch
		{
			ItemEquipType.Small => CItem.EItemSlot.SmallItem, 
			ItemEquipType.Head => CItem.EItemSlot.Head, 
			ItemEquipType.Body => CItem.EItemSlot.Body, 
			ItemEquipType.Feet => CItem.EItemSlot.Legs, 
			ItemEquipType.OneHandWeapon => CItem.EItemSlot.OneHand, 
			ItemEquipType.TwoHandWeapon => CItem.EItemSlot.TwoHand, 
			_ => throw new ArgumentOutOfRangeException("element", element, null), 
		};
	}

	public static ItemEquipType ItemEquipTypeToEItemSlot(CItem.EItemSlot element)
	{
		return element switch
		{
			CItem.EItemSlot.None => ItemEquipType.Small, 
			CItem.EItemSlot.Head => ItemEquipType.Head, 
			CItem.EItemSlot.Body => ItemEquipType.Body, 
			CItem.EItemSlot.Legs => ItemEquipType.Feet, 
			CItem.EItemSlot.OneHand => ItemEquipType.OneHandWeapon, 
			CItem.EItemSlot.TwoHand => ItemEquipType.TwoHandWeapon, 
			CItem.EItemSlot.QuestItem => ItemEquipType.Small, 
			CItem.EItemSlot.SmallItem => ItemEquipType.Small, 
			_ => throw new ArgumentOutOfRangeException("element", element, null), 
		};
	}

	public static CardPileType ECardPileToCardPileType(CBaseCard.ECardPile cardPile)
	{
		return cardPile switch
		{
			CBaseCard.ECardPile.Discarded => CardPileType.Discarded, 
			CBaseCard.ECardPile.Round => CardPileType.Round, 
			CBaseCard.ECardPile.Hand => CardPileType.Hand, 
			CBaseCard.ECardPile.Activated => CardPileType.Active, 
			CBaseCard.ECardPile.Lost => CardPileType.Lost, 
			CBaseCard.ECardPile.PermanentlyLost => CardPileType.Permalost, 
			CBaseCard.ECardPile.None => CardPileType.Unselected, 
			CBaseCard.ECardPile.SelectedAbilityCardsOutOfPhase => CardPileType.SelectedAbilityCardsOutOfPhase, 
			CBaseCard.ECardPile.Taken => CardPileType.Taken, 
			CBaseCard.ECardPile.OwnedUnused => CardPileType.OwnedUnused, 
			_ => CardPileType.None, 
		};
	}

	public static CBaseCard.ECardPile CardPileTypeToECardPile(CardPileType cardPileType)
	{
		return cardPileType switch
		{
			CardPileType.Discarded => CBaseCard.ECardPile.Discarded, 
			CardPileType.Round => CBaseCard.ECardPile.Round, 
			CardPileType.Hand => CBaseCard.ECardPile.Hand, 
			CardPileType.Active => CBaseCard.ECardPile.Activated, 
			CardPileType.Lost => CBaseCard.ECardPile.Lost, 
			CardPileType.Permalost => CBaseCard.ECardPile.PermanentlyLost, 
			CardPileType.Unselected => CBaseCard.ECardPile.None, 
			CardPileType.ExtraTurn => CBaseCard.ECardPile.Round, 
			CardPileType.SelectedAbilityCardsOutOfPhase => CBaseCard.ECardPile.SelectedAbilityCardsOutOfPhase, 
			CardPileType.Taken => CBaseCard.ECardPile.Taken, 
			CardPileType.OwnedUnused => CBaseCard.ECardPile.OwnedUnused, 
			_ => CBaseCard.ECardPile.None, 
		};
	}

	public static ItemEquipType? ItemSlotToEquipType(CItem.EItemSlot slot)
	{
		return slot switch
		{
			CItem.EItemSlot.None => (ItemEquipType?)null, 
			CItem.EItemSlot.Head => ItemEquipType.Head, 
			CItem.EItemSlot.Body => ItemEquipType.Body, 
			CItem.EItemSlot.Legs => ItemEquipType.Feet, 
			CItem.EItemSlot.OneHand => ItemEquipType.OneHandWeapon, 
			CItem.EItemSlot.TwoHand => ItemEquipType.TwoHandWeapon, 
			CItem.EItemSlot.SmallItem => ItemEquipType.Small, 
			CItem.EItemSlot.QuestItem => null, 
			CItem.EItemSlot.Pet => null, 
		};
	}

	public static CItem.EItemSlot? ItemEquipTypeToSlot(ItemEquipType slot)
	{
		return slot switch
		{
			ItemEquipType.Small => CItem.EItemSlot.SmallItem, 
			ItemEquipType.Head => CItem.EItemSlot.Head, 
			ItemEquipType.Body => CItem.EItemSlot.Body, 
			ItemEquipType.Feet => CItem.EItemSlot.Legs, 
			ItemEquipType.OneHandWeapon => CItem.EItemSlot.OneHand, 
			ItemEquipType.TwoHandWeapon => CItem.EItemSlot.TwoHand, 
		};
	}

	public static AttackModifierYMLData AttackModifierDefToYML(CharacterAttackModifierDef modifier)
	{
		return ScenarioRuleClient.SRLYML.AttackModifiers.Find((AttackModifierYMLData data) => data.Name == modifier.ID.ID);
	}

	public static AttackModifierYMLData AttackModifierDefToYML(AttackModifierDTO modifier)
	{
		return ScenarioRuleClient.SRLYML.AttackModifiers.Find((AttackModifierYMLData data) => data.Name == modifier.ID);
	}

	public static PerksYMLData PerkDefToYML(PerkDef perkDef)
	{
		return ScenarioRuleClient.SRLYML.Perks.Find((PerksYMLData data) => data.ID == perkDef.ID.ID);
	}

	public static EEnhancement EnhancementDTOToEEnhancement(CardEnhancementDTO def)
	{
		if (def == null)
		{
			return EEnhancement.NoEnhancement;
		}
		return def.Effect switch
		{
			CardEnhancementEffectType.AddOne => def.ForEffect switch
			{
				CardEffectType.Damage => EEnhancement.PlusDamage, 
				CardEffectType.Attack => EEnhancement.PlusAttack, 
				CardEffectType.Heal => EEnhancement.PlusHeal, 
				CardEffectType.Move => EEnhancement.PlusMove, 
				CardEffectType.Pierce => EEnhancement.PlusPierce, 
				CardEffectType.Pull => EEnhancement.PlusPull, 
				CardEffectType.Push => EEnhancement.PlusPush, 
				CardEffectType.Range => EEnhancement.PlusRange, 
				CardEffectType.Retailate => EEnhancement.PlusRetaliate, 
				CardEffectType.Shield => EEnhancement.PlusShield, 
				CardEffectType.Target => EEnhancement.PlusTarget, 
				CardEffectType.SummonAttack => EEnhancement.SummonAttack, 
				CardEffectType.SummonHP => EEnhancement.SummonHP, 
				CardEffectType.SummonMove => EEnhancement.SummonMove, 
				CardEffectType.SummonRange => EEnhancement.SummonRange, 
				CardEffectType.Teleport => EEnhancement.PlusTeleport, 
				_ => EEnhancement.NoEnhancement, 
			}, 
			CardEnhancementEffectType.Ward => EEnhancement.Ward, 
			CardEnhancementEffectType.Regenerate => EEnhancement.Regenerate, 
			CardEnhancementEffectType.Strengthen => EEnhancement.Strengthen, 
			CardEnhancementEffectType.Bless => EEnhancement.Bless, 
			CardEnhancementEffectType.Wound => EEnhancement.Wound, 
			CardEnhancementEffectType.Poison => EEnhancement.Poison, 
			CardEnhancementEffectType.Immobilize => EEnhancement.Immobilize, 
			CardEnhancementEffectType.Muddle => EEnhancement.Muddle, 
			CardEnhancementEffectType.Curse => EEnhancement.Curse, 
			CardEnhancementEffectType.ElementAir => EEnhancement.Air, 
			CardEnhancementEffectType.ElementDark => EEnhancement.Dark, 
			CardEnhancementEffectType.ElementEarth => EEnhancement.Earth, 
			CardEnhancementEffectType.ElementFire => EEnhancement.Fire, 
			CardEnhancementEffectType.ElementIce => EEnhancement.Ice, 
			CardEnhancementEffectType.ElementLight => EEnhancement.Light, 
			CardEnhancementEffectType.ElementWild => EEnhancement.AnyElement, 
			CardEnhancementEffectType.Jump => EEnhancement.Jump, 
			CardEnhancementEffectType.AoEHex => EEnhancement.Area, 
			_ => EEnhancement.NoEnhancement, 
		};
	}

	public static EEnhancement CardEnhancementEffectTypeToEEnhancement(CardEnhancementEffectType ceet)
	{
		return ceet switch
		{
			CardEnhancementEffectType.Ward => EEnhancement.Ward, 
			CardEnhancementEffectType.Regenerate => EEnhancement.Regenerate, 
			CardEnhancementEffectType.Strengthen => EEnhancement.Strengthen, 
			CardEnhancementEffectType.Bless => EEnhancement.Bless, 
			CardEnhancementEffectType.Wound => EEnhancement.Wound, 
			CardEnhancementEffectType.Poison => EEnhancement.Poison, 
			CardEnhancementEffectType.Immobilize => EEnhancement.Immobilize, 
			CardEnhancementEffectType.Muddle => EEnhancement.Muddle, 
			CardEnhancementEffectType.Curse => EEnhancement.Curse, 
			CardEnhancementEffectType.ElementAir => EEnhancement.Air, 
			CardEnhancementEffectType.ElementDark => EEnhancement.Dark, 
			CardEnhancementEffectType.ElementEarth => EEnhancement.Earth, 
			CardEnhancementEffectType.ElementFire => EEnhancement.Fire, 
			CardEnhancementEffectType.ElementIce => EEnhancement.Ice, 
			CardEnhancementEffectType.ElementLight => EEnhancement.Light, 
			CardEnhancementEffectType.ElementWild => EEnhancement.AnyElement, 
			CardEnhancementEffectType.Jump => EEnhancement.Jump, 
			CardEnhancementEffectType.AoEHex => EEnhancement.Area, 
			_ => EEnhancement.NoEnhancement, 
		};
	}

	public static EEnhancementMarkType CardEnhancementMarkTypeToEEnhancementMarkType(CardEnhancementMarkType cemt)
	{
		return cemt switch
		{
			CardEnhancementMarkType.Square => EEnhancementMarkType.Square, 
			CardEnhancementMarkType.Circle => EEnhancementMarkType.Circle, 
			CardEnhancementMarkType.Diamond => EEnhancementMarkType.Diamond, 
			CardEnhancementMarkType.DiamondPlus => EEnhancementMarkType.DiamondPlus, 
			CardEnhancementMarkType.Hex => EEnhancementMarkType.Hex, 
			_ => throw new ArgumentOutOfRangeException("cemt", cemt, "Unknown mark type!!"), 
		};
	}

	public static LootCard.EType MaterialToLootCard(OutpostMaterialType materialType)
	{
		return materialType switch
		{
			OutpostMaterialType.Lumber => LootCard.EType.Lumber, 
			OutpostMaterialType.Metal => LootCard.EType.Metal, 
			OutpostMaterialType.Hide => LootCard.EType.Hide, 
			_ => LootCard.EType.None, 
		};
	}

	public static LootCard.EType HerbToLootCard(OutpostHerbType herbType)
	{
		return herbType switch
		{
			OutpostHerbType.Arrowvine => LootCard.EType.Arrowvine, 
			OutpostHerbType.Axenut => LootCard.EType.Axenut, 
			OutpostHerbType.Corpsecap => LootCard.EType.Corpsecap, 
			OutpostHerbType.Flamefruit => LootCard.EType.Flamefruit, 
			OutpostHerbType.Rockroot => LootCard.EType.Rockroot, 
			OutpostHerbType.Snowthistle => LootCard.EType.Snowthistle, 
			_ => LootCard.EType.None, 
		};
	}

	public static OutpostMaterialType LootCardToMaterial(LootCard.EType loot)
	{
		return loot switch
		{
			LootCard.EType.Lumber => OutpostMaterialType.Lumber, 
			LootCard.EType.Metal => OutpostMaterialType.Metal, 
			LootCard.EType.Hide => OutpostMaterialType.Hide, 
			_ => OutpostMaterialType.None, 
		};
	}

	public static OutpostHerbType LootCardToHerb(LootCard.EType loot)
	{
		return loot switch
		{
			LootCard.EType.Arrowvine => OutpostHerbType.Arrowvine, 
			LootCard.EType.Axenut => OutpostHerbType.Axenut, 
			LootCard.EType.Corpsecap => OutpostHerbType.Corpsecap, 
			LootCard.EType.Flamefruit => OutpostHerbType.Flamefruit, 
			LootCard.EType.Rockroot => OutpostHerbType.Rockroot, 
			LootCard.EType.Snowthistle => OutpostHerbType.Snowthistle, 
			_ => OutpostHerbType.None, 
		};
	}

	public static string GetLootTypeKey(LootCard.EType lootType)
	{
		return lootType switch
		{
			LootCard.EType.None => string.Empty, 
			LootCard.EType.Money => "Gold", 
			LootCard.EType.Lumber => "Lumber", 
			LootCard.EType.Metal => "Metal", 
			LootCard.EType.Hide => "Hide", 
			LootCard.EType.Arrowvine => "Arrowvine", 
			LootCard.EType.Axenut => "Axenut", 
			LootCard.EType.Corpsecap => "Corpsecap", 
			LootCard.EType.Flamefruit => "Flamefruit", 
			LootCard.EType.Rockroot => "Rockroot", 
			LootCard.EType.Snowthistle => "Snowthistle", 
			LootCard.EType.RandomItem => string.Empty, 
			_ => throw new ArgumentOutOfRangeException("lootType", lootType, null), 
		};
	}
}
