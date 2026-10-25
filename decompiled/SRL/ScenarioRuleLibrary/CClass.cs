using System;
using System.Collections.Generic;
using System.Linq;
using FH.Tools;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

[StateDump(UseToString = true)]
public class CClass
{
	[KronosEnumPolice(3)]
	public enum ENPCModel
	{
		None,
		BanditArcher,
		BanditArcherElite,
		LivingBones,
		LivingBonesElite,
		BanditGuard,
		BanditGuardElite,
		Hound,
		HoundElite,
		RendingDrake,
		RendingDrakeElite,
		SpittingDrake,
		SpittingDrakeElite,
		LivingCorpse,
		LivingCorpseElite,
		LivingSpirit,
		LivingSpiritElite,
		InoxGuard,
		InoxGuardElite,
		InoxArcher,
		InoxArcherElite,
		InoxShaman,
		InoxShamanElite,
		VermlingScout,
		VermlingScoutElite,
		CaveBear,
		CaveBearElite,
		Cultist,
		CultistElite,
		Lurker,
		LurkerElite,
		FrostDemon,
		FrostDemonElite,
		FlameDemon,
		FlameDemonElite,
		EarthDemon,
		EarthDemonElite,
		WindDemon,
		WindDemonElite,
		NightDemon,
		NightDemonElite,
		SunDemon,
		SunDemonElite,
		ForestImp,
		ForestImpElite,
		BlackImp,
		BlackImpElite,
		StoneGolem,
		StoneGolemElite,
		Ooze,
		OozeElite,
		AncientArtillery,
		AncientArtilleryElite,
		GiantViper,
		GiantViperElite,
		CityGuard,
		CityGuardElite,
		CityArcher,
		CityArcherElite,
		VermlingShaman,
		VermlingShamanElite,
		SavvasIcestorm,
		SavvasIcestormElite,
		SavvasLavaflow,
		SavvasLavaflowElite,
		HarrowerInfester,
		HarrowerInfesterElite,
		DeepTerror,
		DeepTerrorElite,
		WingedHorrorEgg,
		BanditCommander,
		InoxBodyguard,
		MercilessOverseer,
		ElderDrake,
		TheBetrayer,
		TheColorless,
		SightlessEye,
		DarkRider,
		WingedHorror,
		Jekserah,
		CaptainOfTheGuard,
		PrimeDemon,
		TheGloom,
		UndeadCommander,
		HighCultist,
		BoneRanger,
		BoneRangerElite,
		Zephyr,
		Hail,
		HailCenser,
		Redthorn,
		Captive01,
		Captive02,
		Captive03,
		Captive04,
		Captive05,
		Captive06,
		HungrySoul,
		Fish,
		GiantOoze,
		Villager01,
		Villager02,
		Villager03,
		Villager04,
		Villager05,
		Villager06,
		Villager07,
		Villager08,
		Villager09,
		Villager10,
		Villager11,
		Villager12,
		ArcaneGolem,
		CultistandVictim,
		SiegeCannon,
		Harvester,
		Infiltrator,
		BloatedRegent,
		RitualCorpse,
		RitualCorpseElite,
		SnakeCultist,
		SnakeCultistElite,
		Battlebot,
		BurningAvatar,
		Decoy,
		GiantRat,
		Killbot,
		ManaSphere,
		MysticAlly,
		PlagueRat,
		RatKing,
		RatSwarm,
		Skeleton,
		WarriorSpirit,
		Doppelganger,
		BeastTyrantBear,
		JadeFalcon,
		GreenAdder,
		TatteredWolf,
		RedFalcon,
		SwampLizard,
		Monolith,
		WindTotem,
		SpiritBanner,
		LivingBomb,
		ShadowWolf,
		IronBeast,
		NailSphere,
		VoidEater,
		BlackUnicorn,
		GiantBat,
		LavaGolem,
		RockColossus,
		ThornShooter,
		SlimeSpirit,
		HealingSprite,
		WarHawk,
		BattleBoar,
		ViciousJackal,
		GiantToad,
		SpittingCobra,
		SteelConstruct,
		SummonsAltar,
		CrystalAltar,
		Elementalist3DemonSummonAltar,
		UndeadSummoningAltar,
		PropDummyObject,
		PrimeDemonAltar,
		LurkerKing,
		Captive01Weapon,
		Captive02Weapon,
		Captive03Weapon,
		Captive04Weapon,
		Captive05Weapon,
		Captive06Weapon,
		BlackSludge,
		BlackSludgeElite,
		FilthySludge,
		BloodImp,
		BloodImpElite,
		BloodMonstrosity,
		BloodMonstrosityElite,
		ChaosDemon,
		ChaosDemonElite,
		RatMonstrosity,
		RatMonstrosityElite,
		VermlingRaider,
		VermlingRaiderElite,
		Zealot,
		ZealotElite,
		EntropyDemon,
		Crowd01,
		Crowd02,
		Crowd03,
		Crowd04,
		Crowd05,
		Crowd06,
		Crowd07,
		Crowd08,
		BloodHorror,
		BloodTumor,
		FirstOfTheOrder,
		SandDevil,
		Ward,
		Rikharn,
		EarthLord,
		InoxNecromancer,
		SpiritBear,
		SpiritOfXorn,
		SongOfTheDeep,
		GhostWolf,
		DeepEarth,
		HighFlame,
		WoundedGuard,
		CityGuardLieutenant,
		VermlingDruid,
		Benchmark,
		BurrowingBlade,
		BurrowingBladeElite,
		RuinedMachine,
		RuinedMachineElite,
		SteelAutomaton,
		SteelAutomatonElite,
		AbaelHerder,
		AbaelHerderElite,
		AbaelScout,
		AbaelScoutElite,
		AlgoxArcher,
		AlgoxArcherElite,
		AlgoxGuard,
		AlgoxGuardElite,
		AlgoxIcespeaker,
		AlgoxIcespeakerElite,
		AlgoxPriest,
		AlgoxPriestElite,
		AlgoxScout,
		AlgoxScoutElite,
		AlgoxSnowspeaker,
		AlgoxSnowspeakerElite,
		FlamingBladespinner,
		FlamingBladespinnerElite,
		FrozenCorpse,
		FrozenCorpseElite,
		IceWraith,
		IceWraithElite,
		LightningEel,
		LightningEelElite,
		LivingDoom,
		LivingDoomElite,
		LurkerClawcrusher,
		LurkerClawcrusherElite,
		LurkerMindsnipper,
		LurkerMindsnipperElite,
		LurkerSoldier,
		LurkerSoldierElite,
		LurkerWavethrower,
		LurkerWavethrowerElite,
		PiranhaPig,
		PiranhaPigElite,
		PolarBear,
		PolarBearElite,
		RoboticBoltshooter,
		RoboticBoltshooterElite,
		ShrikeFiend,
		ShrikeFiendElite,
		SnowImp,
		SnowImpElite,
		VermlingPriest,
		VermlingPriestElite,
		ChaosSpark,
		AnimatedClaymore,
		ArcingGenerator,
		ArmoredTank,
		BannerOfCourage,
		BannerOfDoom,
		BannerOfHope,
		BannerOfStrength,
		BannerOfValor,
		Bombardier,
		BoneHorde,
		DampeningUnit,
		DefenseGrid,
		FleshBomb,
		FleshGolem,
		JackalMech,
		Longbowman,
		MachineBolter,
		Leaper,
		PolarCat,
		Reinforcement,
		RagingCorpse,
		RepairBot,
		ShaggyLure,
		ShadowBeast,
		ShadowHorror,
		ShamblingSkeleton,
		ShieldSpider,
		SkeletonSorcerer,
		SledgeDriver,
		SniperTurret,
		SnowFox,
		SpiritboundFalchion,
		StitchedAtrocity,
		SwordPropeller,
		TorchBearer,
		ToxinDistributor,
		TrappingUnit,
		TrainedFalcon,
		WhiteOwl,
		Wraith,
		AlgoxSnowdancer,
		AlgoxFrozenfist,
		AlgoxStormcaller,
		ElderOoze,
		FishKing,
		FractureOfTheDeep,
		HarbingerOfShadow,
		LordOfChaos,
		PrinceOfFrost,
		ProgramDirector,
		SeekerOfTheAbyss,
		TheRender,
		TheRelic,
		TheOrphan,
		VestigeOfTheImprisonedGod,
		ChaosLieutenant,
		HatefulCorpse,
		CoralCorpse,
		FishKingLeviathan,
		Moonshard,
		Barduu,
		Gurndel,
		CrainTallengyr,
		HulkingPillar,
		DummyHealthDoor,
		DarkCrystalExposed,
		DenpangHelper,
		SpectralHelper,
		Satha,
		Militia,
		LurkerConductor,
		TheCollector,
		FlamingSwordOfJustice,
		LadyInWhite,
		Artificer,
		TheTormentor,
		FleshFiend,
		FracturedLieutenantClawcrusherDark,
		FracturedLieutenantClawcrusherLight,
		FracturedLieutenantMindsnipperEarth,
		FracturedLieutenantMindsnipperIce,
		FracturedLieutenantWavethrowerAir,
		FracturedLieutenantWavethrowerFire,
		FracturedLieutenantClawcrusherDarkElite,
		FracturedLieutenantClawcrusherLightElite,
		FracturedLieutenantMindsnipperEarthElite,
		FracturedLieutenantMindsnipperIceElite,
		FracturedLieutenantWavethrowerAirElite,
		FracturedLieutenantWavethrowerFireElite,
		CrainTallengyrS62,
		TheBlacksmith,
		TheBlueBrother,
		TheRedBrother,
		HobblingHound,
		ValrathMerchant,
		CaravanWagon,
		VermlingScoutS121,
		VermlingScoutElite121,
		VermlingWarchief,
		Geminate,
		Deathwalker,
		Marko,
		Rimeheart,
		RimeheartElite,
		AugmentedMindthief,
		Belara,
		CentralProcessor,
		HungryMaw,
		ReluctantGhost,
		Scabwit,
		TheShimmer,
		ThePowerCore,
		Vanjal,
		Yoglang,
		Faye,
		HelperVillager,
		HEROICS,
		ZU,
		IceBlock,
		LaserSpire,
		LadyInWhiteElite,
		AlgoxGuardSnowspeaker,
		AlgoxGuardSnowspeakerElite,
		AlgoxArcherSnowspeaker,
		AlgoxArcherSnowspeakerElite,
		AlgoxPriestSnowspeaker,
		AlgoxPriestSnowspeakerElite,
		AlgoxScoutSnowspeaker,
		AlgoxScoutSnowspeakerElite,
		CityGuardS01,
		ThePowerCoreElite,
		GiantPiranhaPig,
		AlgoxSnowspeakerOrange,
		AlgoxSnowspeakerOrangeElite,
		ZuSpawnS101,
		TheInfuser,
		MirroredCharacter111Simplified
	}

	public static readonly ENPCModel[] NPCModels = (ENPCModel[])Enum.GetValues(typeof(ENPCModel));

	public const int CURSE_LIMIT = 10;

	public const int BLESS_LIMIT = 10;

	private List<PerksYMLData> m_DummyPerks = new List<PerksYMLData>();

	[StateDump(Skip = true)]
	protected int? m_TempSetInitiative;

	[StateDump(Skip = true)]
	protected int? m_TempInitiativeBonus;

	private List<CAbilityCard> m_TemporaryCards = new List<CAbilityCard>();

	public string ID { get; private set; }

	public List<string> Models { get; private set; }

	public string DefaultModel => Models[0];

	private string OverrideAbilityCardArt { get; set; }

	public string AbilityCardArt
	{
		get
		{
			if (OverrideAbilityCardArt != null)
			{
				return OverrideAbilityCardArt;
			}
			return DefaultModel;
		}
	}

	public string LocKey { get; private set; }

	public List<CAbilityCard> TemporaryCards => m_TemporaryCards;

	[StateDump(Skip = true)]
	public int? TempInitiativeBonus => m_TempInitiativeBonus;

	[StateDump(Skip = true)]
	public int? TempSetInitiative => m_TempSetInitiative;

	public virtual List<PerksYMLData> Perks => m_DummyPerks;

	public virtual int Health()
	{
		return 0;
	}

	public virtual string Avatar()
	{
		return null;
	}

	public CClass(string id, string model, string locKey, string overrideAbilityCardArt = null)
	{
		ID = id;
		Models = new List<string> { model };
		LocKey = locKey;
		OverrideAbilityCardArt = overrideAbilityCardArt;
	}

	public CClass(string id, List<string> models, string locKey, string overrideAbilityCardArt = null)
	{
		ID = id;
		Models = models;
		LocKey = locKey;
		OverrideAbilityCardArt = overrideAbilityCardArt;
	}

	public override string ToString()
	{
		return GetType().Name + "(" + ID + ")";
	}

	public virtual void SortLists()
	{
	}

	public static void MarkRoundActiveBonusesFinished(CActor actor)
	{
		List<CActiveBonus> list;
		if (actor.IsOriginalMonsterType)
		{
			list = MonsterClassManager.FindAllActiveBonuses(CActiveBonus.EActiveBonusDurationType.Round, actor);
			list.AddRange(CharacterClassManager.FindAllActiveBonuses(CActiveBonus.EActiveBonusDurationType.Round, actor));
		}
		else
		{
			if (actor.IsOriginalMonsterType)
			{
				return;
			}
			list = CharacterClassManager.FindAllActiveBonuses(CActiveBonus.EActiveBonusDurationType.Round, actor);
		}
		foreach (CActiveBonus item in list)
		{
			item.Finish();
		}
		foreach (CActiveBonus item2 in actor.FindApplicableActiveBonuses(CAbility.EAbilityType.AddActiveBonus, CActiveBonus.EActiveBonusBehaviourType.PreCalcInitiativeAbility))
		{
			if (item2.ToggledBonus && item2.Ability.ActiveBonusData.IsToggleBonus)
			{
				item2.ToggleActiveBonusFromVars(actor, markCostAbilityAsExecuted: false);
			}
		}
	}

	public static void MarkTurnActiveBonusesFinished(CActor actor)
	{
		List<CActiveBonus> list;
		if (actor.IsOriginalMonsterType)
		{
			list = MonsterClassManager.FindAllActiveBonuses(CActiveBonus.EActiveBonusDurationType.Turn, actor);
			list.AddRange(CharacterClassManager.FindAllActiveBonuses(CActiveBonus.EActiveBonusDurationType.Turn, actor));
		}
		else
		{
			if (actor.IsOriginalMonsterType)
			{
				return;
			}
			list = CharacterClassManager.FindAllActiveBonuses(CActiveBonus.EActiveBonusDurationType.Turn, actor);
		}
		foreach (CActiveBonus item in list)
		{
			item.Finish();
		}
	}

	public static void MarkAbilityActiveBonusesFinished(CActor actor)
	{
		List<CBaseCard> list = null;
		List<CActiveBonus> list2 = ((!actor.IsOriginalMonsterType) ? CharacterClassManager.FindAllActiveBonuses(CActiveBonus.EActiveBonusDurationType.Ability, actor) : MonsterClassManager.FindAllActiveBonuses(CActiveBonus.EActiveBonusDurationType.Ability, actor));
		if (actor.Class is CMonsterClass cMonsterClass)
		{
			list = cMonsterClass.ActivatedCards;
		}
		else if (actor.Class is CCharacterClass cCharacterClass)
		{
			list = cCharacterClass.ActivatedCards;
		}
		if (list != null)
		{
			foreach (CBaseCard item in list)
			{
				foreach (CActiveBonus activeBonuse in item.ActiveBonuses)
				{
					if (activeBonuse.Duration == CActiveBonus.EActiveBonusDurationType.Ability && !list2.Contains(activeBonuse))
					{
						list2.Add(activeBonuse);
					}
				}
			}
		}
		foreach (CActiveBonus item2 in list2)
		{
			CItem cItem = null;
			if (item2.BaseCard.IsItemOrPet)
			{
				cItem = (CItem)item2.BaseCard;
				if (item2.HasTracker && item2.TrackerIndex == 0)
				{
					if (item2.Ability.ActiveBonusData.Consuming.Count > 0)
					{
						foreach (ElementInfusionBoardManager.EElement item3 in item2.Ability.ActiveBonusData.Consuming)
						{
							ElementInfusionBoardManager.EElement element;
							if (item3 == ElementInfusionBoardManager.EElement.Any)
							{
								element = (item2.ToggledElement.HasValue ? item2.ToggledElement.Value : item3);
							}
							else
							{
								element = item3;
							}
							ElementInfusionBoardManager.Consume(element, actor);
						}
					}
				}
				else
				{
					cItem = null;
				}
			}
			item2.Finish();
			if (cItem != null)
			{
				actor.Inventory.ReactivateItem(cItem, actor);
			}
		}
		foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
		{
			allAliveActor.CheckForCachedValuesAfterActiveBonusesUpdate();
		}
	}

	public virtual IEnumerable<CActiveBonus> FindActiveBonuses(CActor actor)
	{
		throw new NotImplementedException("'CClass.FindActiveBonuses(CActor actor)' is not implemented by " + GetType().Name);
	}

	public static void CancelAllActiveBonusesOnDeath(CActor actor)
	{
		List<CActiveBonus> list = (from x in CharacterClassManager.FindAllActiveBonuses()
			where x.Actor == actor || (x is CSummonActiveBonus cSummonActiveBonus && cSummonActiveBonus.Caster == actor)
			select x).ToList();
		if (actor is CPlayerActor { SummonsContinueAfterDeath: not false })
		{
			return;
		}
		foreach (CActiveBonus item in list)
		{
			if (item.HasBehaviour<CMoveActiveBonus_MoveScript>() || item.Ability.ActiveBonusData.GetMiscVar("DontCancelOnDeath").GetBool() || (item.IsAura && item.Actor != actor))
			{
				continue;
			}
			if (item.IsDoom)
			{
				CDoom cDoom = null;
				foreach (CDoom doom in item.Caster.Dooms)
				{
					if (doom.DoomAbilities.Contains(item.Ability))
					{
						cDoom = doom;
					}
				}
				if (cDoom != null)
				{
					if (!item.Caster.CachedRemovedOnDeathDooms.Contains(cDoom))
					{
						item.Caster.CachedRemovedOnDeathDooms.Add(cDoom);
					}
					continue;
				}
			}
			item.Finish();
			if (item.Duration == CActiveBonus.EActiveBonusDurationType.Summon && !actor.IsDead)
			{
				GameState.KillActor(actor, item.Actor, CActor.ECauseOfDeath.SummonActiveBonusCancelled, out var _);
			}
			item.Actor.RefreshCharacterSpecialMechanicSlots();
		}
		foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
		{
			allAliveActor.CheckForCachedValuesAfterActiveBonusesUpdate();
		}
		actor.CheckForCachedValuesAfterActiveBonusesUpdate();
		if (actor.Dooms != null && actor.Dooms.Count > 0)
		{
			actor.RemoveAllDooms();
		}
	}

	public static void CancelActiveBonus(CActiveBonus activeBonus, bool allowFromMainThread = false)
	{
		if (activeBonus == null)
		{
			return;
		}
		if (activeBonus.Ability.Augment != null)
		{
			activeBonus.Actor.RemoveAugment(activeBonus.Ability.Augment);
			return;
		}
		if (activeBonus.Ability.Song != null)
		{
			activeBonus.Actor.RemoveSong(activeBonus.Ability.Song);
			return;
		}
		if (activeBonus.IsDoom)
		{
			activeBonus.Actor = activeBonus.Caster;
			{
				foreach (CDoom doom in activeBonus.Caster.Dooms)
				{
					if (doom.DoomAbilities.Contains(activeBonus.Ability))
					{
						activeBonus.Caster.RemoveDoom(doom);
						break;
					}
				}
				return;
			}
		}
		activeBonus.Finish();
		if (activeBonus.Duration == CActiveBonus.EActiveBonusDurationType.Summon)
		{
			GameState.KillActor(activeBonus.Caster, activeBonus.Actor, CActor.ECauseOfDeath.SummonActiveBonusCancelled, out var _);
		}
		activeBonus.Actor.RefreshCharacterSpecialMechanicSlots();
		foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
		{
			allAliveActor.CheckForCachedValuesAfterActiveBonusesUpdate();
		}
	}

	public void AddAugmentBaseCardToTemporaryCards(CAbilityCard augmentBaseCard)
	{
		m_TemporaryCards.Add(augmentBaseCard);
	}

	public void ClearTemporaryCards()
	{
		m_TemporaryCards.Clear();
	}

	public virtual CBaseCard FindCardWithAbility(CAbility ability, CActor actor)
	{
		if (ability is CAbilityAttack { IsDefaultAttack: not false } || ability is CAbilityMove { IsDefaultMove: not false })
		{
			return null;
		}
		return CBaseCard.CreateDummy(null, 0, ability, actor);
	}

	public virtual CBaseCard FindCard(int id, string name)
	{
		return null;
	}

	public virtual void AddCurseCard(CActor actor, bool canGoOverLimit = false)
	{
	}

	public static void AddCurseCardToDeck(List<AttackModifierYMLData> deck, bool allowShuffle = true)
	{
		deck.Add(AttackModifiersYML.CreateCurse());
		if (allowShuffle)
		{
			deck.Shuffle(ScenarioManager.CurrentScenarioState.ScenarioRNG);
		}
	}

	public static void AddBlessCardToDeck(List<AttackModifierYMLData> deck, bool allowShuffle = true)
	{
		deck.Add(AttackModifiersYML.CreateBless());
		if (allowShuffle)
		{
			deck.Shuffle(ScenarioManager.CurrentScenarioState.ScenarioRNG);
		}
	}

	public virtual int RemoveCurses(CActor actor)
	{
		return 0;
	}

	public virtual void AddBlessCard(CActor actor, bool canGoOverLimit = false)
	{
	}

	public virtual void RemoveBlesses(CActor actor)
	{
	}

	public virtual List<AttackModifierYMLData> GetCurseCards()
	{
		return new List<AttackModifierYMLData>();
	}

	public virtual List<AttackModifierYMLData> GetBlessCard()
	{
		return new List<AttackModifierYMLData>();
	}

	public static void AddModifierCards(List<string> cardNames, ref List<AttackModifierYMLData> list, bool allowShuffle = true)
	{
		foreach (string attackModName in cardNames)
		{
			if (attackModName == CCondition.EPositiveCondition.Bless.ToString())
			{
				AddBlessCardToDeck(list, allowShuffle);
				continue;
			}
			if (attackModName == CCondition.ENegativeCondition.Curse.ToString())
			{
				AddCurseCardToDeck(list, allowShuffle);
				continue;
			}
			list.Add(ScenarioRuleClient.SRLYML.AttackModifiers.Single((AttackModifierYMLData c) => c.Name == attackModName));
		}
	}

	public static void RemoveModifierCards(List<string> cardNames, ref List<AttackModifierYMLData> list)
	{
		foreach (string attackModName in cardNames)
		{
			AttackModifierYMLData attackModifierYMLData = list.FirstOrDefault((AttackModifierYMLData c) => c.Name == attackModName);
			if (attackModifierYMLData != null)
			{
				list.Remove(attackModifierYMLData);
			}
		}
	}

	public static void RemoveAllModifierCards(List<string> cardNames, ref List<AttackModifierYMLData> list)
	{
		foreach (string attackModName in cardNames)
		{
			list.RemoveAll((AttackModifierYMLData c) => c.Name.Equals(attackModName, StringComparison.InvariantCultureIgnoreCase));
		}
	}

	public void UpdateInitiativeBonusPreview(int initiativeBonus)
	{
		if (!m_TempInitiativeBonus.HasValue)
		{
			m_TempInitiativeBonus = 0;
		}
		m_TempInitiativeBonus += initiativeBonus;
		if (this is CCharacterClass cCharacterClass && cCharacterClass.GetActor() is CPlayerActor cPlayerActor)
		{
			Log.SimpleInfo(cPlayerActor.Class.ID + " Initiative adjusted " + initiativeBonus + ".  New Initiative " + cPlayerActor.Initiative());
		}
		if (m_TempInitiativeBonus.HasValue && m_TempInitiativeBonus.Value == 0)
		{
			m_TempInitiativeBonus = null;
		}
	}

	public void ResetInitiativeBonus(CPlayerActor actor)
	{
		m_TempInitiativeBonus = null;
		if (actor == null)
		{
			return;
		}
		foreach (CActiveBonus item in from x in actor.FindApplicableActiveBonuses(CAbility.EAbilityType.AdjustInitiative, CActiveBonus.EActiveBonusBehaviourType.AdjustInitiative)
			where !x.Ability.ActiveBonusData.IsToggleBonus
			select x)
		{
			item.GetBehaviour<CAdjustInitiativeActiveBonus_AdjustInitiative>().ApplyInitiative(positive: true);
		}
	}

	public void UpdateSetInitiativePreview(int val)
	{
		m_TempSetInitiative = val;
		if (this is CCharacterClass cCharacterClass && cCharacterClass.GetActor() is CPlayerActor cPlayerActor)
		{
			Log.SimpleInfo(cPlayerActor.Class.ID + " Initiative adjusted " + val + ".  New Initiative " + cPlayerActor.Initiative());
		}
	}

	public void ResetSetInitiative()
	{
		m_TempSetInitiative = null;
	}

	public virtual void Reset()
	{
		ResetSetInitiative();
		ResetInitiativeBonus(null);
	}
}
