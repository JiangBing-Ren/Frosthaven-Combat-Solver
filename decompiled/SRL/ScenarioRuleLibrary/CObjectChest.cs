using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ScenarioRuleLibrary.YML;
using SharedLibrary;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjectChest : CObjectProp, ISerializable
{
	[Serializable]
	public enum EType
	{
		None = 0,
		Gain4LootCards_01 = 1,
		Suffer3DamageAndGainMuddle_02 = 2,
		GainRandomItemBlueprint_03 = 3,
		GainEventideScepter_04 = 4,
		GainSeveredClaw_05 = 5,
		GainOpulentShoes_06 = 6,
		GainDataDrive_07 = 7,
		Gain3LootCards_08 = 8,
		UnlockRandomSideScenario_09 = 9,
		GainRandomItemBlueprint_11 = 11,
		RemainsOfCamilla_12 = 12,
		GainFishhook_13 = 13,
		UnlockRandomSideScenario_15 = 15,
		GainRandomItemBlueprint_14 = 14,
		GainDecanterOfMists_16 = 16,
		Gain3Lumber_17 = 17,
		Gain3LootCards_18 = 18,
		Gain4LootCards_20 = 20,
		GainDetonatorBP_21 = 21,
		GainScaledArmorBP_22 = 22,
		UnlockRandomSideScenario_23 = 23,
		GainBane_24 = 24,
		Gain2Herbs_25 = 25,
		GainLightningRod_26 = 26,
		Gain2LootCards_27 = 27,
		Gain4LootCards_28 = 28,
		Gain2LumberAnd2Hide_29 = 29,
		Gain4LootCards_30 = 30,
		GainBalancedScalesBP_31 = 31,
		Gain3LootCards_32 = 32,
		Gain3Lumber_33 = 33,
		Gain1PerkChecks_34 = 34,
		GainExtendablePole_35 = 35,
		GainBlackBox_36 = 36,
		Gain3LootCards_37 = 37,
		GainRandomItemBlueprint_38 = 38,
		UnlockRandomSideScenario_39 = 39,
		GainChaosCannonBP_40 = 40,
		GainAlgoxCarving_41 = 41,
		GainGiantSword_42 = 42,
		Gain4LootCards_43 = 43,
		GainRandomItemBlueprint_44 = 44,
		Gain2LumberAnd2Metal_45 = 45,
		Gain3LootCards_46 = 46,
		Gain5XP_47 = 47,
		Gain3LootCards_48 = 48,
		GainBoneBootsBP_49 = 49,
		GainCuredLeatherArmor_50 = 50,
		ReadSection_78_1_51 = 51,
		GainTitanNail_52 = 52,
		GainEyeOfTheStorm_53 = 53,
		GainHarpoon_55 = 55,
		GainRandomItemBlueprint_56 = 56,
		UnlockScenario103_57 = 57,
		GainRandomItemBlueprint_58 = 58,
		GainPainSimulacrumBP_59 = 59,
		Gain3LootCards_60 = 60,
		UnlockRandomSideScenario_61 = 61,
		GainHalfMechanicalCubeBlueprint_62 = 62,
		Gain5LootCards_63 = 63,
		Gain3Lumber_64 = 64,
		Gain15Gold_65 = 65,
		Suffer2DamageAndGainBrittle_66 = 66,
		UnlockRandomSideScenario_67 = 67,
		Gain4Metal_68 = 68,
		GainHalfMechanicalCubeBlueprint_69 = 69,
		GainHornOfCommandBP_70 = 70,
		Gain4LootCards_71 = 71,
		GainLivingStoneBP_72 = 72,
		GainRandomItemBlueprint_73 = 73,
		GainRandomItemBlueprint_74 = 74,
		Gain1PerkChecks_75 = 75,
		UnlockRandomSideScenario_76 = 76,
		Gain3LootCards_77 = 77,
		GainOpulentShoes_78 = 78,
		Gain10XP_79 = 79,
		Gain15XP_80 = 80,
		GainUnfetteredArm_81 = 81,
		GainOakStaffBP_82 = 82,
		GainTriCornerHat_83 = 83,
		GainIceShiv_84 = 84,
		Gain10Gold_85 = 85,
		Gain3LootCards_96 = 96,
		GainCleavingAxe_86 = 86
	}

	public static readonly Array AllTypes = typeof(EType).GetEnumValues();

	public static readonly Dictionary<EType, int> LootToItemId = new Dictionary<EType, int>
	{
		{
			EType.GainEventideScepter_04,
			1210
		},
		{
			EType.GainSeveredClaw_05,
			1214
		},
		{
			EType.GainDataDrive_07,
			1237
		},
		{
			EType.GainOpulentShoes_06,
			1208
		},
		{
			EType.GainFishhook_13,
			1217
		},
		{
			EType.GainDecanterOfMists_16,
			1276
		},
		{
			EType.GainExtendablePole_35,
			1238
		},
		{
			EType.GainBlackBox_36,
			1232
		},
		{
			EType.GainLightningRod_26,
			1233
		},
		{
			EType.GainAlgoxCarving_41,
			1235
		},
		{
			EType.GainGiantSword_42,
			1216
		},
		{
			EType.GainCuredLeatherArmor_50,
			914
		},
		{
			EType.GainTitanNail_52,
			1218
		},
		{
			EType.GainEyeOfTheStorm_53,
			1249
		},
		{
			EType.GainHarpoon_55,
			1215
		},
		{
			EType.GainOpulentShoes_78,
			1208
		},
		{
			EType.GainUnfetteredArm_81,
			1236
		},
		{
			EType.GainIceShiv_84,
			1231
		},
		{
			EType.GainTriCornerHat_83,
			1203
		},
		{
			EType.GainCleavingAxe_86,
			938
		}
	};

	public EType Type { get; set; }

	public List<CItem> StartingItems { get; set; }

	public int Gold { get; set; }

	public int XP { get; set; }

	public int DamageValue { get; set; }

	public List<CCondition.ENegativeCondition> Conditions { get; private set; }

	public List<string> ChestTreasureTablesID { get; set; }

	public bool HasChestPrepopulated
	{
		get
		{
			if ((StartingItems == null || StartingItems.Count <= 0) && Gold == 0 && XP == 0)
			{
				return Type != EType.None;
			}
			return true;
		}
	}

	public bool IsFHLoot => Type != EType.None;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("StartingItems", StartingItems);
		info.AddValue("Gold", Gold);
		info.AddValue("XP", XP);
		info.AddValue("Conditions", Conditions);
		info.AddValue("DamageValue", DamageValue);
		info.AddValue("Type", Type);
		info.AddValue("ChestTreasureTablesID", ChestTreasureTablesID);
	}

	public CObjectChest(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "StartingItems":
					StartingItems = (List<CItem>)info.GetValue("StartingItems", typeof(List<CItem>));
					break;
				case "Gold":
					Gold = info.GetInt32("Gold");
					break;
				case "XP":
					XP = info.GetInt32("XP");
					break;
				case "Conditions":
					Conditions = (List<CCondition.ENegativeCondition>)info.GetValue("Conditions", typeof(List<CCondition.ENegativeCondition>));
					break;
				case "DamageValue":
					DamageValue = info.GetInt32("DamageValue");
					break;
				case "Type":
					Type = (EType)info.GetValue("Type", typeof(EType));
					break;
				case "ChestTreasureTablesID":
					ChestTreasureTablesID = (List<string>)info.GetValue("ChestTreasureTablesID", typeof(List<string>));
					break;
				case "ChestTreasureTables":
				{
					ChestTreasureTablesID = (List<string>)info.GetValue("ChestTreasureTables", typeof(List<string>));
					if (ChestTreasureTablesID == null || ChestTreasureTablesID.Count <= 0)
					{
						break;
					}
					for (int i = 0; i < ChestTreasureTablesID.Count; i++)
					{
						string text = ChestTreasureTablesID[i];
						if (text.StartsWith("£") && text.EndsWith("£") && !text.Contains("ID"))
						{
							text = text.Replace(" ", "");
							text = text.Replace("£", "");
							ChestTreasureTablesID[i] = "£" + text + "ID£";
						}
					}
					break;
				}
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CObjectChest entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	[OnDeserialized]
	internal new void OnDeserialized(StreamingContext context)
	{
		if (Conditions == null)
		{
			Conditions = new List<CCondition.ENegativeCondition>();
		}
	}

	public CObjectChest(string name, ScenarioManager.ObjectImportType type, TileIndex arrayIndex, CVector3 position, CVector3 rotation, CActor owner, string mapGuid)
		: base(name, type, arrayIndex, position, rotation, owner, mapGuid)
	{
		Conditions = new List<CCondition.ENegativeCondition>();
	}

	public CObjectChest(string name, ScenarioManager.ObjectImportType type, string mapGuid)
		: base(name, type, null, null, null, null, mapGuid)
	{
		Conditions = new List<CCondition.ENegativeCondition>();
	}

	public CObjectChest(SharedLibrary.Random scenarioGenerationRNG, string name, ScenarioManager.ObjectImportType type, string mapGuid)
		: base(scenarioGenerationRNG, name, type, null, null, null, null, mapGuid)
	{
		Conditions = new List<CCondition.ENegativeCondition>();
	}

	public CObjectChest(SharedLibrary.Random scenarioGenerationRNG, string name, ScenarioManager.ObjectImportType type, string mapGuid, List<string> treasureTable)
		: base(scenarioGenerationRNG, name, type, null, null, null, null, mapGuid)
	{
		ChestTreasureTablesID = treasureTable;
		Conditions = new List<CCondition.ENegativeCondition>();
	}

	public CObjectChest(SharedLibrary.Random scenarioGenerationRNG, string name, ScenarioManager.ObjectImportType type, string mapGuid, List<string> treasureTable, List<CCondition.ENegativeCondition> conditions, int damageValue, EType lootType)
		: base(scenarioGenerationRNG, name, type, null, null, null, null, mapGuid)
	{
		ChestTreasureTablesID = treasureTable;
		Conditions = conditions;
		DamageValue = damageValue;
		Type = lootType;
	}

	public override bool Activate(CActor actor, CActor creditActor = null, bool actorIsJumping = false, bool actorIsFlying = false)
	{
		bool flag;
		if (!Activated && (actor is CPlayerActor || actor is CHeroSummonActor))
		{
			ScenarioRuleClient.ToggleMessageProcessing(process: false);
			actor.StartLooting();
			flag = TryFHLoot(creditActor);
			List<CAbility> list = new List<CAbility>();
			if (actor != null && DamageValue > 0 && ScenarioManager.Scenario.HasActor(actor))
			{
				CAbility cAbility = CAbilityDamage.CreateDefaultDamage(DamageValue, 1, 1, isMonster: false, "", isMultipass: true, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self));
				cAbility.MiscAbilityData = new AbilityData.MiscAbilityData
				{
					AutotriggerAbility = true
				};
				list.Add(cAbility);
			}
			if (actor.LastLoot?.NegativeConditions?.Any() == true)
			{
				if (Conditions == null)
				{
					Conditions = new List<CCondition.ENegativeCondition>();
				}
				Conditions.AddRange(actor.LastLoot.NegativeConditions);
			}
			base.Activate(actor, creditActor);
			ActorActivated = ((actor == null) ? string.Empty : actor.ActorGuid);
			string characterID = "";
			if (actor == null)
			{
				goto IL_0414;
			}
			if (DamageValue <= 0)
			{
				List<CCondition.ENegativeCondition> conditions = Conditions;
				if (conditions == null || conditions.Count <= 0)
				{
					goto IL_0414;
				}
			}
			if (actor is CPlayerActor cPlayerActor)
			{
				characterID = cPlayerActor.CharacterClass.ID;
			}
			if (actor is CHeroSummonActor { Summoner: var summoner })
			{
				characterID = summoner?.CharacterClass.ID;
			}
			SEventLogMessageHandler.AddEventLogMessage(new SEventObjectPropChest(DamageValue, characterID, "Activate", ESESubTypeObjectProp.Activated, ObjectType, PropGuid, PrefabName, m_OwnerGuid, "", ArrayIndex.ToPoint()));
			if ((actor.Type == CActor.EType.Player || actor.Type == CActor.EType.HeroSummon || actor.Type == CActor.EType.Ally) && !ScenarioRuleClient.IsWorkerThread)
			{
				ScenarioRuleClient.AddSRLQueueMessage(new CSRLChestMessage(this, actor), processImmediately: false);
				return false;
			}
			if (ScenarioManager.Scenario.HasActor(actor))
			{
				if (GameState.ActorHealthCheck(actor, actor) && Conditions != null && Conditions.Count > 0)
				{
					if (Owner != null)
					{
						_ = Owner;
					}
					else
					{
						_ = actor;
					}
					foreach (CCondition.ENegativeCondition condition in Conditions)
					{
						CAbility.EAbilityType eAbilityType = CAbility.AbilityTypes.SingleOrDefault((CAbility.EAbilityType s) => s.ToString() == condition.ToString());
						if (condition != CCondition.ENegativeCondition.NA && eAbilityType != CAbility.EAbilityType.None)
						{
							CAbility cAbility2 = CAbility.CreateAbility(eAbilityType, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), isMonster: false, isTargetedAbility: false);
							cAbility2.MiscAbilityData = new AbilityData.MiscAbilityData
							{
								AutotriggerAbility = true
							};
							list.Add(cAbility2);
						}
						else
						{
							Log.ErrorWithReport("Condition " + condition.ToString() + " could not be found in EAbilityType enum.");
						}
					}
				}
				PhaseManager.StartAbilities(list.Select((CAbility x) => (x: x, actor: actor)).ToList(), stackInsteadOfInject: true, reverseOrder: true);
			}
		}
		goto IL_0456;
		IL_0456:
		return false;
		IL_0414:
		if (flag)
		{
			SEventLogMessageHandler.AddEventLogMessage(new SEventObjectProp(ESESubTypeObjectProp.Activated, ObjectType, PropGuid, PrefabName, null, "Activated", m_OwnerGuid, "", new List<CMap>(), ArrayIndex.ToPoint()));
		}
		goto IL_0456;
	}

	public override bool AutomaticActivate(CActor actor, bool actorIsJumping = false, bool actorIsFlying = false)
	{
		return false;
	}

	public virtual bool DelayedActivate(CActor actor)
	{
		return Activate(actor);
	}

	public List<Reward> GetRewardsFromStartingItems()
	{
		List<Reward> list = new List<Reward>();
		if (StartingItems != null)
		{
			foreach (CItem startingItem in StartingItems)
			{
				list.Add(new Reward(startingItem.ID, 1, ETreasureType.Item, null, EGiveToCharacterType.Give));
			}
		}
		if (Gold != 0)
		{
			list.Add(new Reward(ETreasureType.Gold, Gold, ETreasureDistributionType.Combined, null));
		}
		if (XP != 0)
		{
			list.Add(new Reward(ETreasureType.XP, XP, ETreasureDistributionType.Combined, null));
		}
		return list;
	}

	private void Trace(object o, object ctx = null)
	{
		Log.GameplayTrace($"{this}: {o.ToString()}", ctx);
	}

	private void Error(object o, object ctx = null)
	{
		Log.GameplayError($"{this}: {o.ToString()}", ctx);
	}

	private void ReadCampaignSection(string id)
	{
		GameState.TriggerCampaignSection(id);
	}

	private void GainLumber_loot(int amount, CActor actor)
	{
		Trace($"Looting {amount} lumber...");
		actor.GainResource(LootCard.EType.Lumber, amount);
	}

	private void GainMetal_loot(int amount, CActor actor)
	{
		Trace($"Looting {amount} metal...");
		actor.GainResource(LootCard.EType.Metal, amount);
	}

	private void GainHide_loot(int amount, CActor actor)
	{
		Trace($"Looting {amount} hide...");
		actor.GainResource(LootCard.EType.Hide, amount);
	}

	private void GainCorpsecap_loot(int amount, CActor actor)
	{
		Trace($"Looting {amount} corpsecap...");
		actor.GainResource(LootCard.EType.Corpsecap, amount);
	}

	private void GainFlamefruit_loot(int amount, CActor actor)
	{
		Trace($"Looting {amount} Flamefruit...");
		actor.GainResource(LootCard.EType.Flamefruit, amount);
	}

	private void GainNLootCards_loot(CActor actor, int n)
	{
		Trace($"Looting {n} cards from the loot deck...");
		for (int i = 0; i < n; i++)
		{
			if (!actor.TryLootFromDeck())
			{
				Trace(actor.Class.ID + " failed to loot from deck.");
			}
		}
	}

	private void GainRandomItemBlueprint_loot(CActor actor)
	{
		Trace("Gain random item blueprint...");
		actor.GainRandomItemBlueprint();
		SEventLogMessageHandler.AddEventLogMessage(new SEventActorLootedRandomItemBlueprint(null, new SEventActor.SEventActorBaseData(ESESubTypeActor.ActorLooted, actor)));
	}

	private void UnlockCampaignSection_loot(CActor actor, string section, int delayWeeks = 0)
	{
		Trace("Unlock section...");
		actor.GainCampaignSection(section, delayWeeks);
	}

	private void GainXP(CActor actor, int amount)
	{
		Trace($"Gain {amount} XP...");
		actor.GainXP(amount);
	}

	private void GainPerkChecks_loot(CActor actor, uint amount)
	{
		Trace($"Gain {amount} perks...");
		actor.GainPerkChecks(amount);
	}

	private void GainSticker_loot(CActor actor, string sticker)
	{
		Trace("Gain sticker...");
		actor.GainCampaignSticker(sticker);
	}

	private void UnlockRandomSideScenario_loot(CActor actor)
	{
		Trace("Gain random side scenario...");
		actor.GainRandomSideScenario();
	}

	private void GainHalfMechanicalCubeBlueprint_loot(CActor actor)
	{
		CObjectProp cObjectProp = ScenarioManager.CurrentScenarioState.Props.Find((CObjectProp p) => p != this && p is CObjectChest);
		if (cObjectProp == null || cObjectProp.Activated)
		{
			GainBlueprint_loot(actor, 1029u);
		}
	}

	private void GainCondition_loot(CActor actor, CCondition.ENegativeCondition condition)
	{
		actor.LastLoot?.NegativeConditions?.Add(condition);
	}

	private void GainItem_loot(CActor actor, EType type)
	{
		GainItem_loot(actor, LootToItemId[type]);
	}

	private void GainItem_loot(CActor actor, int id)
	{
		actor.TryLootItem(id);
	}

	private void GainBlueprint_loot(CActor actor, uint id)
	{
		Trace("Gain item blueprint...");
		actor.GainItemBlueprint(id);
		SEventLogMessageHandler.AddEventLogMessage(new SEventActorLootedRandomItemBlueprint(null, new SEventActor.SEventActorBaseData(ESESubTypeActor.ActorLooted, actor)));
	}

	private void Gain30CollectiveGold_loot(CActor actor)
	{
		int count = ScenarioManager.Scenario.PlayerActors.Count;
		int num = 30 / count;
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			playerActor.GainLootMoney(num);
			Trace($"{playerActor.CharacterName} Gained {num} gold.");
		}
	}

	private bool TryFHLoot(CActor actor)
	{
		if (Type == EType.None)
		{
			return false;
		}
		Trace($"Chest loot is '{Type}'");
		CCondition.ENegativeCondition eNegativeCondition = CCondition.ENegativeCondition.NA;
		switch (Type)
		{
		case EType.GainEventideScepter_04:
		case EType.GainSeveredClaw_05:
		case EType.GainOpulentShoes_06:
		case EType.GainDataDrive_07:
		case EType.GainFishhook_13:
		case EType.GainDecanterOfMists_16:
		case EType.GainLightningRod_26:
		case EType.GainExtendablePole_35:
		case EType.GainBlackBox_36:
		case EType.GainAlgoxCarving_41:
		case EType.GainGiantSword_42:
		case EType.GainCuredLeatherArmor_50:
		case EType.GainTitanNail_52:
		case EType.GainEyeOfTheStorm_53:
		case EType.GainHarpoon_55:
		case EType.GainOpulentShoes_78:
		case EType.GainUnfetteredArm_81:
		case EType.GainTriCornerHat_83:
		case EType.GainIceShiv_84:
		case EType.GainCleavingAxe_86:
			GainItem_loot(actor, Type);
			break;
		case EType.UnlockRandomSideScenario_09:
		case EType.UnlockRandomSideScenario_15:
		case EType.UnlockRandomSideScenario_23:
		case EType.UnlockRandomSideScenario_39:
		case EType.UnlockRandomSideScenario_61:
		case EType.UnlockRandomSideScenario_67:
		case EType.UnlockRandomSideScenario_76:
			UnlockRandomSideScenario_loot(actor);
			break;
		case EType.GainHalfMechanicalCubeBlueprint_62:
		case EType.GainHalfMechanicalCubeBlueprint_69:
			GainHalfMechanicalCubeBlueprint_loot(actor);
			break;
		case EType.Gain3Lumber_17:
		case EType.Gain3Lumber_33:
		case EType.Gain3Lumber_64:
			GainLumber_loot(3, actor);
			break;
		case EType.Gain2LootCards_27:
			GainNLootCards_loot(actor, 2);
			break;
		case EType.Gain3LootCards_08:
		case EType.Gain3LootCards_18:
		case EType.Gain3LootCards_32:
		case EType.Gain3LootCards_37:
		case EType.Gain3LootCards_46:
		case EType.Gain3LootCards_48:
		case EType.Gain3LootCards_60:
		case EType.Gain3LootCards_77:
		case EType.Gain3LootCards_96:
			GainNLootCards_loot(actor, 3);
			break;
		case EType.Gain4LootCards_01:
		case EType.Gain4LootCards_20:
		case EType.Gain4LootCards_28:
		case EType.Gain4LootCards_30:
		case EType.Gain4LootCards_43:
		case EType.Gain4LootCards_71:
			GainNLootCards_loot(actor, 4);
			break;
		case EType.Gain5LootCards_63:
			GainNLootCards_loot(actor, 5);
			break;
		case EType.GainRandomItemBlueprint_03:
		case EType.GainRandomItemBlueprint_11:
		case EType.GainRandomItemBlueprint_14:
		case EType.GainRandomItemBlueprint_38:
		case EType.GainRandomItemBlueprint_44:
		case EType.GainRandomItemBlueprint_56:
		case EType.GainRandomItemBlueprint_58:
		case EType.GainRandomItemBlueprint_73:
		case EType.GainRandomItemBlueprint_74:
			GainRandomItemBlueprint_loot(actor);
			break;
		case EType.GainDetonatorBP_21:
			GainBlueprint_loot(actor, 1299u);
			break;
		case EType.GainScaledArmorBP_22:
			GainBlueprint_loot(actor, 1001u);
			break;
		case EType.GainBalancedScalesBP_31:
			GainBlueprint_loot(actor, 1021u);
			break;
		case EType.GainChaosCannonBP_40:
			GainBlueprint_loot(actor, 1019u);
			break;
		case EType.GainBoneBootsBP_49:
			GainBlueprint_loot(actor, 1007u);
			break;
		case EType.GainHornOfCommandBP_70:
			GainBlueprint_loot(actor, 1017u);
			break;
		case EType.GainLivingStoneBP_72:
			GainBlueprint_loot(actor, 1025u);
			break;
		case EType.GainPainSimulacrumBP_59:
			GainBlueprint_loot(actor, 1027u);
			break;
		case EType.GainOakStaffBP_82:
			GainBlueprint_loot(actor, 1009u);
			break;
		case EType.Gain2LumberAnd2Metal_45:
			GainLumber_loot(2, actor);
			GainMetal_loot(2, actor);
			break;
		case EType.Gain4Metal_68:
			GainMetal_loot(4, actor);
			break;
		case EType.Suffer3DamageAndGainMuddle_02:
			eNegativeCondition = CCondition.ENegativeCondition.Muddle;
			break;
		case EType.GainBane_24:
			eNegativeCondition = CCondition.ENegativeCondition.Bane;
			break;
		case EType.Suffer2DamageAndGainBrittle_66:
			eNegativeCondition = CCondition.ENegativeCondition.Brittle;
			break;
		case EType.ReadSection_78_1_51:
			ReadCampaignSection("78.1");
			break;
		case EType.Gain2LumberAnd2Hide_29:
			GainLumber_loot(2, actor);
			GainHide_loot(2, actor);
			break;
		case EType.Gain2Herbs_25:
			GainCorpsecap_loot(1, actor);
			GainFlamefruit_loot(1, actor);
			break;
		case EType.RemainsOfCamilla_12:
			GainSticker_loot(actor, "Brummix");
			UnlockCampaignSection_loot(actor, "99.1", 3);
			break;
		case EType.Gain1PerkChecks_34:
		case EType.Gain1PerkChecks_75:
			GainPerkChecks_loot(actor, 1u);
			break;
		case EType.Gain5XP_47:
			GainXP(actor, 5);
			break;
		case EType.Gain10XP_79:
			GainXP(actor, 10);
			break;
		case EType.Gain15XP_80:
			GainXP(actor, 15);
			break;
		case EType.Gain10Gold_85:
			actor.GainLootMoney(10);
			break;
		case EType.Gain15Gold_65:
			actor.GainLootMoney(15);
			break;
		case EType.UnlockScenario103_57:
			actor.GainScenario(103u);
			break;
		default:
			throw new NotImplementedException(Type.ToString());
		}
		if (eNegativeCondition != CCondition.ENegativeCondition.NA)
		{
			GainCondition_loot(actor, eNegativeCondition);
		}
		return true;
	}

	public static List<Tuple<int, string>> Compare(CObjectChest chest1, CObjectChest chest2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		try
		{
			list.AddRange(CObjectProp.Compare(chest1, chest2, isMPCompare));
			if (chest1.Gold != chest2.Gold)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1701, "CObjectChest Gold does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", chest1.PropGuid, chest2.PropGuid },
					new string[3]
					{
						"ObjectType",
						chest1.ObjectType.ToString(),
						chest2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", chest1.PrefabName, chest2.PrefabName },
					new string[3]
					{
						"Gold",
						chest1.Gold.ToString(),
						chest2.Gold.ToString()
					}
				});
			}
			if (chest1.XP != chest2.XP)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1702, "CObjectChest XP does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", chest1.PropGuid, chest2.PropGuid },
					new string[3]
					{
						"ObjectType",
						chest1.ObjectType.ToString(),
						chest2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", chest1.PrefabName, chest2.PrefabName },
					new string[3]
					{
						"XP",
						chest1.XP.ToString(),
						chest2.XP.ToString()
					}
				});
			}
			switch (StateShared.CheckNullsMatch(chest1.StartingItems, chest2.StartingItems))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 1703, "CObjectChest StartingItems null state does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", chest1.PropGuid, chest2.PropGuid },
					new string[3]
					{
						"ObjectType",
						chest1.ObjectType.ToString(),
						chest2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", chest1.PrefabName, chest2.PrefabName },
					new string[3]
					{
						"StartingItems",
						(chest1.StartingItems == null) ? "is null" : "is not null",
						(chest2.StartingItems == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (chest1.StartingItems.Count != chest2.StartingItems.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 1704, "CObjectChest StartingItems Count does not match.", new List<string[]>
					{
						new string[3] { "Prop GUID", chest1.PropGuid, chest2.PropGuid },
						new string[3]
						{
							"ObjectType",
							chest1.ObjectType.ToString(),
							chest2.ObjectType.ToString()
						},
						new string[3] { "PrefabName", chest1.PrefabName, chest2.PrefabName },
						new string[3]
						{
							"StartingItems Count",
							chest1.StartingItems.Count.ToString(),
							chest2.StartingItems.Count.ToString()
						}
					});
					break;
				}
				bool flag = false;
				foreach (CItem item in chest1.StartingItems)
				{
					if (!chest2.StartingItems.Exists((CItem e) => e.ItemGuid == item.ItemGuid))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1705, "CObjectChest StartingItems in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing item from " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Prop GUID", chest1.PropGuid, chest2.PropGuid },
							new string[3]
							{
								"ObjectType",
								chest1.ObjectType.ToString(),
								chest2.ObjectType.ToString()
							},
							new string[3] { "PrefabName", chest1.PrefabName, chest2.PrefabName },
							new string[3] { "Items GUID", item.ItemGuid, "Missing" },
							new string[3]
							{
								"Items Name",
								item.YMLData.Name,
								"Missing"
							}
						});
						flag = true;
					}
				}
				foreach (CItem item2 in chest2.StartingItems)
				{
					if (!chest1.StartingItems.Exists((CItem e) => e.ItemGuid == item2.ItemGuid))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1705, "CObjectChest StartingItems in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing item from " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Prop GUID", chest1.PropGuid, chest2.PropGuid },
							new string[3]
							{
								"ObjectType",
								chest1.ObjectType.ToString(),
								chest2.ObjectType.ToString()
							},
							new string[3] { "PrefabName", chest1.PrefabName, chest2.PrefabName },
							new string[3] { "Items GUID", "Missing", item2.ItemGuid },
							new string[3]
							{
								"Items Name",
								"Missing",
								item2.YMLData.Name
							}
						});
						flag = true;
					}
				}
				if (flag)
				{
					break;
				}
				foreach (CItem item3 in chest1.StartingItems)
				{
					try
					{
						CItem item4 = chest2.StartingItems.Single((CItem s) => s.ItemGuid == item3.ItemGuid);
						list.AddRange(CItem.Compare(item3, item4, "Chest", chest1.PropGuid, chest1.ObjectType.ToString(), isMPCompare));
					}
					catch (Exception ex)
					{
						list.Add(new Tuple<int, string>(1706, "Exception during Item State compare.\n" + ex.Message + "\n" + ex.StackTrace));
					}
				}
				break;
			}
			}
			switch (StateShared.CheckNullsMatch(chest1.ChestTreasureTablesID, chest2.ChestTreasureTablesID))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 1707, "CObjectChest ChestTreasureTablesID Null state does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", chest1.PropGuid, chest2.PropGuid },
					new string[3]
					{
						"ObjectType",
						chest1.ObjectType.ToString(),
						chest2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", chest1.PrefabName, chest2.PrefabName },
					new string[3]
					{
						"ChestTreasureTablesID",
						(chest1.ChestTreasureTablesID == null) ? "is null" : "is not null",
						(chest2.ChestTreasureTablesID == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
				if (chest1.ChestTreasureTablesID.Count != chest2.ChestTreasureTablesID.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 1708, "CObjectChest ChestTreasureTablesID Count does not match.", new List<string[]>
					{
						new string[3] { "Prop GUID", chest1.PropGuid, chest2.PropGuid },
						new string[3]
						{
							"ObjectType",
							chest1.ObjectType.ToString(),
							chest2.ObjectType.ToString()
						},
						new string[3] { "PrefabName", chest1.PrefabName, chest2.PrefabName },
						new string[3]
						{
							"ChestTreasureTablesID Count",
							chest1.ChestTreasureTablesID.Count.ToString(),
							chest2.ChestTreasureTablesID.Count.ToString()
						}
					});
					break;
				}
				foreach (string item5 in chest1.ChestTreasureTablesID)
				{
					if (!chest2.ChestTreasureTablesID.Contains(item5))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1709, "CObjectChest ChestTreasureTablesID in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Prop GUID", chest1.PropGuid, chest2.PropGuid },
							new string[3]
							{
								"ObjectType",
								chest1.ObjectType.ToString(),
								chest2.ObjectType.ToString()
							},
							new string[3] { "PrefabName", chest1.PrefabName, chest2.PrefabName },
							new string[3] { "ChestTreasureTablesID", item5, "Missing" }
						});
					}
				}
				foreach (string item6 in chest2.ChestTreasureTablesID)
				{
					if (!chest1.ChestTreasureTablesID.Contains(item6))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1709, "CObjectChest ChestTreasureTablesID in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Prop GUID", chest1.PropGuid, chest2.PropGuid },
							new string[3]
							{
								"ObjectType",
								chest1.ObjectType.ToString(),
								chest2.ObjectType.ToString()
							},
							new string[3] { "PrefabName", chest1.PrefabName, chest2.PrefabName },
							new string[3] { "ChestTreasureTablesID", "Missing", item6 }
						});
					}
				}
				break;
			}
		}
		catch (Exception ex2)
		{
			list.Add(new Tuple<int, string>(1799, "Exception during CObjectChest compare.\n" + ex2.Message + "\n" + ex2.StackTrace));
		}
		return list;
	}
}
