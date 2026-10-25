using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Libs.Utils;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;

public class GlobalSettings : MonoBehaviour
{
	[Serializable]
	public class DefaultHitEffects
	{
		public GameObject DefaultStandardHitEffect;

		public GameObject DefaultCriticalHitEffect;

		public GameObject DefaultShieldHitEffect;

		public int CriticalHitThreshold;
	}

	[Serializable]
	public class GlobalParticleEffects
	{
		public GameObject DefaultHealEffect;

		public GameObject DefaultDeathDissolve;

		public GameObject DefaultPositiveCondition;

		public GameObject DefaultNegativeCondition;

		public GameObject DefaultCharacterReveal;

		public GameObject DefaultCharacterSwap;

		public GameObject SpawnActorVFX;
	}

	[Serializable]
	public class JumpMoveParticleEffects
	{
		public GameObject PhaseStart;

		public GameObject PhaseArrive;

		public GameObject InvisibilityIdle;

		public GameObject InvisibilitySmoke;
	}

	[Serializable]
	public class ActiveBonusBuffTargetEffects
	{
		public GameObject AttackBuffTargetEffect;

		public GameObject ShieldActiveBonusTargetEffect;

		public GameObject ShieldSilentTargetEffect;

		public GameObject RetaliateActiveBonusTargetEffect;

		public GameObject GainShield;

		public GameObject GainRetaliate;

		public GameObject GainDisarm;

		public GameObject GainImmobilize;

		public GameObject GainPoison;

		public GameObject GainStun;

		public GameObject GainWound;

		public GameObject GainBless;

		public GameObject GainCurse;

		public GameObject GainSleep;

		public GameObject GainStrengthen;

		public GameObject GainMuddle;

		public GameObject GainInvisibility;

		public GameObject GainAddTarget;

		public GameObject GainAddHeal;

		public GameObject GainAddRange;

		public GameObject GainAttackersGainDisadvantage;

		public GameObject GainAttackActiveBonus;

		public GameObject GainDefault;

		public GameObject GainBane;

		public GameObject GainWard;

		public GameObject RegenerateGain;

		public GameObject RegenerateTrigger;

		public GameObject BrittleGain;

		public GameObject BrittleRemoved;

		public GameObject GainImpair;

		public GameObject MutateGain;

		public GameObject MutateRemoved;

		public GameObject GainExploitableVulnerability;
	}

	[Serializable]
	public class MagicEffects
	{
		public GameObject RetaliateHit;

		public GameObject RetaliateTarget;

		public GameObject WoundDamage;
	}

	[Serializable]
	public class AbilityCardSettings
	{
		[Header("Font")]
		public TMP_FontAsset CardFont;

		public float PreviewFontSize;

		public float StandardFontSize;

		public float SmallFontSize;

		public float SmallerFontSize;

		public float MinFontSize;

		public float SummonStatFontSize;

		public float ItemCardFontSize;

		[Header("Spacing")]
		public float RowSpacing;

		public float ColumnSpacing;

		[Header("Hex Size")]
		public float AreaHexWidth;

		[Header("Enhancement Icons")]
		public float EnhancementIconSize;

		public float EnhancementIconSpacing;

		public float EnhancementIconInitialSpace;

		[Header("Element Icons")]
		public float ElementIconSize;

		[Header("XP Icons")]
		public float XPIconSize;

		public float XPArrowSize;

		[Header("Misc Icons")]
		public float DurationIconSize;

		[Header("Added effect text")]
		public float AddedEffectPadding;

		public float AddedEffectSingleRowHeight;

		[Header("Mandatory Trigger")]
		public float MandatoryTriggerPadding;

		[Header("Conditional Elements")]
		public float ConditionalElementPadding;
	}

	[Serializable]
	public class ApparanceProps
	{
		public GameObject Trap;

		public GameObject GoldPile;

		public GameObject Chest;

		public GameObject GoalChest;

		public GameObject OneHexObstacles;

		public GameObject TwoHexObstacles;

		public GameObject ThreeHexObstacles;

		public GameObject ThreeHexCurvedObstacles;

		public GameObject ThreeHexStraightObstacles;

		public GameObject Spawner;

		public GameObject PressurePlate;

		public GameObject TerrainHotCoals;

		public GameObject TerrainWater;

		public GameObject TerrainRubble;

		public GameObject TerrainThorns;

		public GameObject DarkPitObstacle;

		public GameObject Portal;

		public GameObject TerrainVisualEffect;

		public GameObject MonsterGrave;

		public GameObject TerrainIce;

		public GameObject WallObstacle;

		public GameObject Snowdrift;

		public GameObject Shadow;
	}

	[Serializable]
	public class SpecificProps
	{
		public GameObject RockSingle;

		public GameObject RockTriple;

		public GameObject PlinthSingle;

		public GameObject GlowingOrb;

		public GameObject PowerTower;

		public GameObject HulkingPillar;

		public GameObject SpecialConsole;

		public GameObject Pedestal;

		public GameObject AlgoxAltar;

		public GameObject MaintenanceShaft;

		public GameObject DebrisPile;

		public GameObject ShadowWallResidue;

		public GameObject Bathysphere;

		public GameObject FrozenFistObstacle;

		public GameObject Sled;

		public GameObject OrbOfBones;

		public GameObject CagedVermlingScout;

		public GameObject RockColumn;

		public GameObject WeatheredAltar;

		public GameObject TrueOakA;

		public GameObject TrueOakB;

		public GameObject LurkerStatue;

		public GameObject HealingPillar;

		public GameObject GlowingCrystal;

		public GameObject Temporal;

		public GameObject MysteriousMachine;

		public GameObject PyroclastObstacle;

		public GameObject TotemOfBewilderment;

		public GameObject TotemOfEarth;

		public GameObject TotemOfRestoration;

		public GameObject PetOoze1HObstacle;

		public GameObject PetOoze2HObstacle;

		public GameObject PetOoze3HObstacle;

		public GameObject Infuser1HObstacle;

		public GameObject Infuser3HObstacle;

		public GameObject SnowRock;

		public GameObject PyroclastHazardTerrain;

		public GameObject SnowdancerHazardTerrain;

		public GameObject VestigeHazardTerrain;

		public GameObject VestigeDifficultTerrain;

		public GameObject LurkerWavethrowerWater;

		public GameObject CrashingTideWater;

		public GameObject BearTrap;

		public GameObject BombTrap;

		public GameObject DamageMine;

		public GameObject PoisonMine;

		public GameObject StunMine;

		public GameObject TrapperTrap;

		public GameObject DetonationCharge;

		public GameObject SewerPipe;

		public GameObject GraveSingle;

		public GameObject GraveDouble;

		public GameObject QuestClaw;

		public GameObject QuestDoll;

		public GameObject TheFavorite;

		public GameObject TheNewFavorite;

		public GameObject DemonCorpsePile;

		public GameObject SmallTree;

		public GameObject Chasm;

		public GameObject TreeBridgeA;

		public GameObject TreeBridgeB;

		public GameObject LavaLootToken;

		public GameObject TerrainLampA;

		public GameObject TerrainLampB;

		public GameObject TerrainLampC;

		public GameObject TerrainLampD;

		public GameObject Sarcophagus;

		public GameObject LootableCorpse;

		public GameObject LootableCrate;

		public GameObject LootableCrateLarge;

		public GameObject RadiantDust;

		public GameObject ReceptacleContainer;

		public GameObject GoalChestAmuletProtection118;

		public GameObject GoalChestAmuletReanimation118;

		public GameObject MovingPlatform;

		public GameObject MovingBoat136;

		public GameObject MovingBoat136B;

		public GameObject TemporalHex;

		public GameObject PortalTerrain;

		public GameObject DarkPitTerrain;

		public GameObject NothingTerrain;

		public GameObject FloatingIce;

		public GameObject SunAgate;

		public GameObject LumberPile;

		public GameObject WarBanner;

		public GameObject CoralPiece;

		public GameObject DarkCrystalExposed;

		public GameObject FlotsamOneHex;

		public GameObject FlotsamTwoHex;

		public GameObject ItemWildGrowthSeedsObstacle;

		public GameObject Ladder;

		public GameObject AbaelHallway;

		public GameObject BlackCauldron;

		public GameObject StatueOfTheBuilder;
	}

	[Serializable]
	public class VFXProps
	{
		public GameObject ShowerOfPebbles;

		public GameObject IceFall;

		public GameObject WindCarving;

		public GameObject ElectricityBurst;

		public GameObject ElectricityBurstPillar;

		public GameObject ElectricityBurstRepair;

		public GameObject EMPWave3HEX;

		public GameObject EMPWave1HEX;

		public GameObject EMPWave2HEX;

		public GameObject HarvestSnow;

		public GameObject DowsingRune;

		public GameObject RuneInfluence;

		public GameObject RuneInfluenceStrong;

		public GameObject RuneInfluenceActivated;

		public GameObject ShadowAttack;

		public GameObject ShadowPortal;

		public GameObject DellingDestruction;

		[HideInInspector]
		public GameObject DrifterShockwave;

		[HideInInspector]
		public GameObject DrifterShockwaveHit;

		[Tooltip("Applies for the bottom part of the Exploding Corpse card, the upper effect is handled in the animator")]
		public GameObject BoneshaperExplodingCorpse;

		public GameObject DrifterDangerZoneImpact;

		public GameObject DrifterDangerZone;

		public GameObject RoomRedLight;

		public GameObject RoomBlueLight;

		public GameObject PolarWind;

		public GameObject EnemySpawnEffect;

		public GameObject BoneHordeAssimilateSkeletonVFX;

		public GameObject RendererShield;

		public GameObject WindCarvingShieldVFX;

		public GameObject DustCloudS25Room1;

		public GameObject DustCloudS25Room2;

		public GameObject ChaosBeamTeleport;

		public GameObject SoilSample;

		public GameObject PolarBearTrap;

		public GameObject OozeSlime;

		public GameObject OozeRemains;

		public GameObject HeartOfIceVFX;

		public GameObject TuningDevice;

		public GameObject TreasurePiece;

		public GameObject ElementalCore;

		public GameObject IceDestructionGeneric;

		public GameObject ElectricityRockColumnCoil;

		public GameObject LaserSpireBeam;

		public GameObject DemoCharge;

		public GameObject BannerSpearSetForTheChargeImpact;

		public GameObject BannerSpearSetForTheCharge;

		public GameObject TrapperEnhanceTrap;

		public GameObject IceCorridorAS70;

		public GameObject IceCorridorBS70;

		public GameObject IcyRootsHazardous;

		public GameObject IcyRootsDifficultA;

		public GameObject IcyRootsDifficultB;

		public GameObject IcyRootsObstacle;

		public GameObject VermlingWarchiefShield;

		public GameObject BlizzardBarrier;

		public GameObject BlizzardBarrier2Hex;

		public GameObject LevelUpQuestDebriefing;

		public GameObject ItemShovelEffect;

		public GameObject ItemChaosCannonEffect;

		public GameObject ItemVolatileBombEffect;

		public GameObject ItemBoomBarrelModel;

		public GameObject ItemBoomBarrelEffect;

		public GameObject ItemTrainersNetEffect;

		public GameObject ItemExplosiveVialEffect;

		public GameObject PetPiranhaPigEffect;

		public GameObject PetHoundEffect;

		public GameObject PetSpittingDrakeEffect;

		public GameObject PetRendingDrakeEffect;

		public GameObject PetBlackImpEffect;

		public GameObject PetForestImpEffect;

		public GameObject PetSnowImpEffect;

		public GameObject PetOozeEffect;

		public GameObject PetOozeTargetEffect;

		public GameObject PetRuinedMachineEffect;

		public GameObject PetLightningEelEffect;

		public GameObject PetHEROICSEffect;

		public GameObject PetBrummixEffect;

		public GameObject TormentorCreateTerrain;

		public GameObject TormentorViolentTerrainDamage;

		public GameObject TitanBeam123;

		public GameObject AmuletProtectionVFX;

		public GameObject AmuletReanimationVFX;

		public GameObject ChillingWind;

		public GameObject PyroclastTerrainFormationVFX;

		public GameObject PyroclastObsidianPuffVFX;

		public GameObject PyroclastObstacleExplosionVFX;

		public GameObject PiranhaPigTerrainSpawnVFX;

		public GameObject WindCorridorMild105;

		public GameObject WindCorridorIntense105;

		public GameObject ElementConsumeFireVFX;

		public GameObject ElementConsumeIceVFX;

		public GameObject ElementConsumeAirVFX;

		public GameObject ElementConsumeEarthVFX;

		public GameObject ElementConsumeLightVFX;

		public GameObject ElementConsumeDarkVFX;

		public GameObject ElementInfuseFireVFX;

		public GameObject ElementInfuseIceVFX;

		public GameObject ElementInfuseAirVFX;

		public GameObject ElementInfuseEarthVFX;

		public GameObject ElementInfuseLightVFX;

		public GameObject ElementInfuseDarkVFX;

		public GameObject S52GoalTilesVFX;

		public GameObject OozeToBossAssimilationVFXS84;

		public GameObject BossToOozeAssimilationVFXS84;

		public GameObject S77EnvironmentalVFX;

		public GameObject S109LaserSpireLightingVFX;

		public GameObject S90FrozenCorpseIce;

		public GameObject NumberCounter;

		public GameObject S21OrbExplosion;

		public GameObject S64BossLigtningVFX;

		public GameObject S125FireVFX;

		public GameObject S125ExplosionVFX;

		public GameObject SnowMist;

		public GameObject S135CopyIndicationVFX;

		public GameObject WaterSplash;

		public GameObject S63EnvVFX;

		public GameObject S98FallingDebris;

		public GameObject NumberCounterActor;

		public GameObject NumberCounterActorDescending;

		public GameObject RotatingMonsterProjection;
	}

	[Serializable]
	public class AttackModifierSettings
	{
		[Tooltip("Consider TimeScale and FadeIn time as 0")]
		public bool InstantSlowMo = true;

		[Tooltip("Time we should wait until we begin the attack modifier flow in the case of not receiving the signal to do so")]
		public float AttackModifierFlowBeginTimeout = 5f;

		[Tooltip("Time we should wait until reveal attack modifier in case ProgressModifier is not firing")]
		public float DefaultAttackModifierTimeout = 5f;

		[Tooltip("Time offset for reveal time. Gives ability to microtune mod revealing during attack")]
		public float AttackRevealTimeOffset;

		[Tooltip("During in sec of one Rolling Modifier")]
		public float RollingModDuration = 2f;

		[Tooltip("SlowMo duration")]
		public float SlowMoDuration = 2f;

		[Tooltip("SlowMo fade-in time. How fast we go to minimum TimeScale value")]
		public float SlowMoFadeInTime = 1f;

		[Tooltip("SlowMo minimal TimeScale. To completely stop action set to 0")]
		public float SlowMoMinimalTimeScale = 0.1f;
	}

	[Serializable]
	public class AdventureLocationMeshes
	{
		public List<GameObject> BossMeshes;

		public List<GameObject> VillageMeshes;

		public List<GameObject> ScenarioMeshes;

		public List<GameObject> AllMeshes => BossMeshes.Concat(VillageMeshes.Concat(ScenarioMeshes)).ToList();
	}

	[Serializable]
	public class AdventureLocationMaterialSettings
	{
		public Material LocationMaterial;

		public bool ShouldOverrideLocationScale;

		[ConditionalField("ShouldOverrideLocationScale", null, true)]
		public Vector3 OverrideLocationScale;

		public Vector3 CityLocationCenterPositionOffset;
	}

	[Serializable]
	public class Projectile
	{
		public string Name;

		public FireSpawnedProjectile.ExternalSettings settings;
	}

	[Serializable]
	public struct BossRoomDef
	{
		public string LevelId;

		public int RoomIndex;
	}

	public DefaultHitEffects m_DefaultHitEffects;

	public GlobalParticleEffects m_GlobalParticles;

	public JumpMoveParticleEffects m_JumpMoveParticleEffects;

	public ActiveBonusBuffTargetEffects m_ActiveBonusBuffTargetEffects;

	public MagicEffects m_MagicEffects;

	public GameObject m_WaypointHolder;

	public GameObject m_GenericHexStar;

	public GameObject m_NavLinkLineObject;

	public AbilityCardSettings m_AbilityCardSettings;

	public AbilityCardSettings m_GlossaryCardSettings;

	public ApparanceProps m_ApparanceProps;

	public SpecificProps m_SpecificProps;

	public VFXProps m_VFXProps;

	public List<Projectile> m_GlobalProjectiles;

	public GameObject[] m_CinematicPrefabs;

	public AdventureLocationMeshes AdventureMapLocationMeshes;

	public GameObject AdventureLocationDecalPrefab;

	public List<AdventureLocationMaterialSettings> m_AdventureLocationMaterialSettings;

	public GameObject m_WaypointPrefab;

	public ApparanceEnumFilterSettings ApparanceEnumFilter;

	private static GlobalSettings s_instance;

	private static GlobalSettingsDef s_defInstance;

	public static GlobalSettingsDef DefInstance => GlobalSettingsDef.Instance;

	public static GlobalSettings Instance => s_instance;

	public static IEnumerator InitInstanceCrt()
	{
		if (!(s_instance != null))
		{
			ByRef<GameObject> asset = new ByRef<GameObject>();
			yield return AssetBundleManager.Instance.LoadAssetFromBundleAsync("misc_globalsettings", "GlobalSettings", "globalsettings", asset);
			if ((bool)asset.Value)
			{
				s_instance = asset.Value.GetComponent<GlobalSettings>();
			}
		}
	}

	public static void ForceLoadInstance()
	{
		s_instance = AssetBundleManager.Instance.LoadAssetFromBundle<GameObject>("misc_globalsettings", "GlobalSettings", "globalsettings").GetComponent<GlobalSettings>();
	}

	public static GameObject GetCinematicPrefab(string prefabName)
	{
		return Instance.m_CinematicPrefabs.FirstOrDefault((GameObject pr) => pr.name == prefabName);
	}

	public static GameObject GetPropPrefab(string propType)
	{
		GameObject gameObject = GetApparancePropPrefab(propType);
		if (gameObject == null)
		{
			gameObject = GetSpecificPropPrefab(propType);
		}
		if (gameObject == null)
		{
			gameObject = GetVFXPropPrefab(propType);
		}
		return gameObject;
	}

	public static GameObject GetApparancePropPrefab(string propTypeString)
	{
		if (propTypeString == "GoldPile" || propTypeString == "LootToken")
		{
			return GetApparancePropPrefab(EPropType.LootToken);
		}
		EPropType ePropType = CObjectProp.PropTypes.SingleOrDefault((EPropType x) => x.ToString() == propTypeString);
		if (ePropType == EPropType.None)
		{
			return null;
		}
		return GetApparancePropPrefab(ePropType);
	}

	public static GameObject GetApparancePropPrefab(EPropType propType)
	{
		return propType switch
		{
			EPropType.Chest => Instance.m_ApparanceProps.Chest, 
			EPropType.GoalChest => Instance.m_ApparanceProps.GoalChest, 
			EPropType.LootToken => Instance.m_ApparanceProps.GoldPile, 
			EPropType.OneHexObstacle => Instance.m_ApparanceProps.OneHexObstacles, 
			EPropType.TwoHexObstacle => Instance.m_ApparanceProps.TwoHexObstacles, 
			EPropType.ThreeHexObstacle => Instance.m_ApparanceProps.ThreeHexObstacles, 
			EPropType.Trap => Instance.m_ApparanceProps.Trap, 
			EPropType.Spawner => Instance.m_ApparanceProps.Spawner, 
			EPropType.PressurePlate => Instance.m_ApparanceProps.PressurePlate, 
			EPropType.TerrainHotCoals => Instance.m_ApparanceProps.TerrainHotCoals, 
			EPropType.TerrainWater => Instance.m_ApparanceProps.TerrainWater, 
			EPropType.TerrainRubble => Instance.m_ApparanceProps.TerrainRubble, 
			EPropType.TerrainThorns => Instance.m_ApparanceProps.TerrainThorns, 
			EPropType.DarkPitObstacle => Instance.m_ApparanceProps.DarkPitObstacle, 
			EPropType.Portal => Instance.m_ApparanceProps.Portal, 
			EPropType.TerrainVisualEffect => Instance.m_ApparanceProps.TerrainVisualEffect, 
			EPropType.ThreeHexCurvedObstacle => Instance.m_ApparanceProps.ThreeHexCurvedObstacles, 
			EPropType.ThreeHexStraightObstacle => Instance.m_ApparanceProps.ThreeHexStraightObstacles, 
			EPropType.MonsterGrave => Instance.m_ApparanceProps.MonsterGrave, 
			EPropType.TerrainIce => Instance.m_ApparanceProps.TerrainIce, 
			EPropType.WallObstacle => Instance.m_ApparanceProps.WallObstacle, 
			EPropType.Snowdrift => Instance.m_ApparanceProps.Snowdrift, 
			EPropType.QuestItem => Instance.m_ApparanceProps.GoalChest, 
			EPropType.Shadow => Instance.m_ApparanceProps.Shadow, 
			_ => null, 
		};
	}

	public static ScenarioManager.ObjectImportType GetObjectImportType(string propTypeString)
	{
		EPropType ePropType = CObjectProp.PropTypes.SingleOrDefault((EPropType x) => x.ToString() == propTypeString);
		if (ePropType != EPropType.None)
		{
			return GetApparancePropType(ePropType);
		}
		ESpecificPropType eSpecificPropType = CObjectProp.SpecificPropTypes.SingleOrDefault((ESpecificPropType x) => x.ToString() == propTypeString);
		if (eSpecificPropType != ESpecificPropType.None)
		{
			return GetSpecificPropType(eSpecificPropType);
		}
		if (CObjectProp.VFXPropTypes.SingleOrDefault((EVFXPropType x) => x.ToString() == propTypeString) != EVFXPropType.None)
		{
			return ScenarioManager.ObjectImportType.VFXProp;
		}
		return ScenarioManager.ObjectImportType.None;
	}

	public static ScenarioManager.ObjectImportType GetApparancePropType(EPropType apparancePropType)
	{
		switch (apparancePropType)
		{
		case EPropType.Chest:
			return ScenarioManager.ObjectImportType.Chest;
		case EPropType.GoalChest:
			return ScenarioManager.ObjectImportType.GoalChest;
		case EPropType.LootToken:
			return ScenarioManager.ObjectImportType.MoneyToken;
		case EPropType.QuestItem:
			return ScenarioManager.ObjectImportType.CarryableQuestItem;
		case EPropType.OneHexObstacle:
		case EPropType.TwoHexObstacle:
		case EPropType.ThreeHexObstacle:
		case EPropType.DarkPitObstacle:
		case EPropType.ThreeHexCurvedObstacle:
		case EPropType.ThreeHexStraightObstacle:
		case EPropType.WallObstacle:
			return ScenarioManager.ObjectImportType.Obstacle;
		case EPropType.Trap:
			return ScenarioManager.ObjectImportType.Trap;
		case EPropType.PressurePlate:
			return ScenarioManager.ObjectImportType.PressurePlate;
		case EPropType.TerrainHotCoals:
			return ScenarioManager.ObjectImportType.TerrainHotCoals;
		case EPropType.TerrainWater:
			return ScenarioManager.ObjectImportType.TerrainWater;
		case EPropType.TerrainRubble:
			return ScenarioManager.ObjectImportType.TerrainRubble;
		case EPropType.TerrainThorns:
			return ScenarioManager.ObjectImportType.TerrainThorns;
		case EPropType.Spawner:
			return ScenarioManager.ObjectImportType.Spawner;
		case EPropType.Portal:
			return ScenarioManager.ObjectImportType.Portal;
		case EPropType.TerrainVisualEffect:
			return ScenarioManager.ObjectImportType.TerrainVisualEffect;
		case EPropType.MonsterGrave:
			return ScenarioManager.ObjectImportType.MonsterGrave;
		case EPropType.TerrainIce:
			return ScenarioManager.ObjectImportType.TerrainIce;
		case EPropType.Snowdrift:
			return ScenarioManager.ObjectImportType.Snowdrift;
		case EPropType.Shadow:
			return ScenarioManager.ObjectImportType.Shadow;
		case EPropType.TerrainLampVisualEffect:
			return ScenarioManager.ObjectImportType.TerrainLampVisualEffect;
		default:
			return ScenarioManager.ObjectImportType.None;
		}
	}

	public static ScenarioManager.ObjectImportType GetSpecificPropType(ESpecificPropType specificPropType)
	{
		switch (specificPropType)
		{
		case ESpecificPropType.RockSingle:
		case ESpecificPropType.RockTriple:
		case ESpecificPropType.PlinthSingle:
		case ESpecificPropType.GlowingOrb:
		case ESpecificPropType.PowerTower:
		case ESpecificPropType.HulkingPillar:
		case ESpecificPropType.SpecialConsole:
		case ESpecificPropType.Pedestal:
		case ESpecificPropType.AlgoxAltar:
		case ESpecificPropType.MaintenanceShaft:
		case ESpecificPropType.DebrisPile:
		case ESpecificPropType.ShadowWallResidue:
		case ESpecificPropType.Bathysphere:
		case ESpecificPropType.FrozenFistObstacle:
		case ESpecificPropType.SmallTree:
		case ESpecificPropType.Chasm:
		case ESpecificPropType.Sled:
		case ESpecificPropType.OrbOfBones:
		case ESpecificPropType.CagedVermlingScout:
		case ESpecificPropType.RockColumn:
		case ESpecificPropType.WeatheredAltar:
		case ESpecificPropType.TrueOakA:
		case ESpecificPropType.TrueOakB:
		case ESpecificPropType.LurkerStatue:
		case ESpecificPropType.HealingPillar:
		case ESpecificPropType.GlowingCrystal:
		case ESpecificPropType.Temporal:
		case ESpecificPropType.MysteriousMachine:
		case ESpecificPropType.TotemOfEarth:
		case ESpecificPropType.TotemOfRestoration:
		case ESpecificPropType.WarBanner:
		case ESpecificPropType.TotemOfBewilderment:
			return ScenarioManager.ObjectImportType.Obstacle;
		case ESpecificPropType.LurkerWavethrowerWater:
			return ScenarioManager.ObjectImportType.TerrainWater;
		case ESpecificPropType.BearTrap:
		case ESpecificPropType.BombTrap:
		case ESpecificPropType.DamageMine:
		case ESpecificPropType.PoisonMine:
		case ESpecificPropType.StunMine:
		case ESpecificPropType.TrapperTrap:
		case ESpecificPropType.DetonationCharge:
			return ScenarioManager.ObjectImportType.Trap;
		case ESpecificPropType.SewerPipeSpawner:
		case ESpecificPropType.GraveSingleSpawner:
		case ESpecificPropType.GraveDoubleSpawner:
			return ScenarioManager.ObjectImportType.Spawner;
		case ESpecificPropType.QuestDoll:
		case ESpecificPropType.QuestClaw:
		case ESpecificPropType.CoralPiece:
		case ESpecificPropType.LavaLootToken:
			return ScenarioManager.ObjectImportType.CarryableQuestItem;
		case ESpecificPropType.TheFavorite:
		case ESpecificPropType.TheNewFavorite:
		case ESpecificPropType.DemonCorpsePile:
		case ESpecificPropType.RadiantDust:
			return ScenarioManager.ObjectImportType.Resource;
		case ESpecificPropType.SnowRock:
			return ScenarioManager.ObjectImportType.SnowRock;
		case ESpecificPropType.TreeBridgeA:
		case ESpecificPropType.TreeBridgeB:
			return ScenarioManager.ObjectImportType.TerrainRubble;
		case ESpecificPropType.TerrainLampA:
		case ESpecificPropType.TerrainLampB:
		case ESpecificPropType.TerrainLampC:
		case ESpecificPropType.TerrainLampD:
			return ScenarioManager.ObjectImportType.TerrainLampVisualEffect;
		case ESpecificPropType.Sarcophagus:
			return ScenarioManager.ObjectImportType.TerrainRubble;
		case ESpecificPropType.LootableCorpse:
		case ESpecificPropType.ReceptacleContainer:
		case ESpecificPropType.GoalChestAmuletReanimation118:
		case ESpecificPropType.GoalChestAmuletProtection118:
		case ESpecificPropType.MammothBark:
			return ScenarioManager.ObjectImportType.GoalChest;
		case ESpecificPropType.LootableCrate:
		case ESpecificPropType.LootableCrate127:
		case ESpecificPropType.FH_Scenario_Campaign_126_SCENARIO_OBJECT_1:
		case ESpecificPropType.SunAgate:
		case ESpecificPropType.LumberPile:
		case ESpecificPropType.LootableCrateLarge:
			return ScenarioManager.ObjectImportType.Resource;
		case ESpecificPropType.MovingPlatform:
		case ESpecificPropType.TemporalHex:
		case ESpecificPropType.PortalTerrain:
		case ESpecificPropType.DarkPitTerrain:
		case ESpecificPropType.NothingTerrain:
		case ESpecificPropType.MovingBoat136:
		case ESpecificPropType.MovingBoat136B:
		case ESpecificPropType.FlotsamOneHex:
		case ESpecificPropType.FlotsamTwoHex:
			return ScenarioManager.ObjectImportType.TerrainVisualEffect;
		case ESpecificPropType.FloatingIce:
		case ESpecificPropType.Ladder:
		case ESpecificPropType.AbaelHallway:
			return ScenarioManager.ObjectImportType.GenericProp;
		case ESpecificPropType.SnowDriftPile:
			return ScenarioManager.ObjectImportType.Obstacle;
		case ESpecificPropType.DarkCrystalExposed:
			return ScenarioManager.ObjectImportType.Obstacle;
		case ESpecificPropType.BlackCauldron:
			return ScenarioManager.ObjectImportType.Obstacle;
		case ESpecificPropType.StatueOfTheBuilder:
			return ScenarioManager.ObjectImportType.Obstacle;
		default:
			return ScenarioManager.ObjectImportType.None;
		}
	}

	public static GameObject GetSpecificPropPrefab(string propTypeString)
	{
		switch (CObjectProp.SpecificPropTypes.SingleOrDefault((ESpecificPropType x) => x.ToString() == propTypeString))
		{
		case ESpecificPropType.RockSingle:
			return Instance.m_SpecificProps.RockSingle;
		case ESpecificPropType.RockTriple:
			return Instance.m_SpecificProps.RockTriple;
		case ESpecificPropType.PlinthSingle:
			return Instance.m_SpecificProps.PlinthSingle;
		case ESpecificPropType.SnowRock:
			return Instance.m_SpecificProps.SnowRock;
		case ESpecificPropType.DebrisPile:
			return Instance.m_SpecificProps.DebrisPile;
		case ESpecificPropType.Bathysphere:
			return Instance.m_SpecificProps.Bathysphere;
		case ESpecificPropType.FrozenFistObstacle:
			return Instance.m_SpecificProps.FrozenFistObstacle;
		case ESpecificPropType.Sled:
			return Instance.m_SpecificProps.Sled;
		case ESpecificPropType.PyroclastObstacle:
			return Instance.m_SpecificProps.PyroclastObstacle;
		case ESpecificPropType.PetOoze1HObstacle:
			return Instance.m_SpecificProps.PetOoze1HObstacle;
		case ESpecificPropType.PetOoze2HObstacle:
			return Instance.m_SpecificProps.PetOoze2HObstacle;
		case ESpecificPropType.PetOoze3HObstacle:
			return Instance.m_SpecificProps.PetOoze3HObstacle;
		case ESpecificPropType.Infuser1HObstacle:
			return Instance.m_SpecificProps.Infuser1HObstacle;
		case ESpecificPropType.Infuser3HObstacle:
			return Instance.m_SpecificProps.Infuser3HObstacle;
		case ESpecificPropType.ItemWildGrowthSeedsObstacle:
			return Instance.m_SpecificProps.ItemWildGrowthSeedsObstacle;
		case ESpecificPropType.PyroclastHazardTerrain:
			return Instance.m_SpecificProps.PyroclastHazardTerrain;
		case ESpecificPropType.SnowdancerHazardTerrain:
			return Instance.m_SpecificProps.SnowdancerHazardTerrain;
		case ESpecificPropType.VestigeHazardTerrain:
			return Instance.m_SpecificProps.VestigeHazardTerrain;
		case ESpecificPropType.VestigeDifficultTerrain:
			return Instance.m_SpecificProps.VestigeDifficultTerrain;
		case ESpecificPropType.LurkerWavethrowerWater:
			return Instance.m_SpecificProps.LurkerWavethrowerWater;
		case ESpecificPropType.CrashingTideWater:
			return Instance.m_SpecificProps.CrashingTideWater;
		case ESpecificPropType.BearTrap:
			return Instance.m_SpecificProps.BearTrap;
		case ESpecificPropType.BombTrap:
			return Instance.m_SpecificProps.BombTrap;
		case ESpecificPropType.DamageMine:
			return Instance.m_SpecificProps.DamageMine;
		case ESpecificPropType.PoisonMine:
			return Instance.m_SpecificProps.PoisonMine;
		case ESpecificPropType.StunMine:
			return Instance.m_SpecificProps.StunMine;
		case ESpecificPropType.TrapperTrap:
			return Instance.m_SpecificProps.TrapperTrap;
		case ESpecificPropType.DetonationCharge:
			return Instance.m_SpecificProps.DetonationCharge;
		case ESpecificPropType.SewerPipeSpawner:
			return Instance.m_SpecificProps.SewerPipe;
		case ESpecificPropType.GraveSingleSpawner:
			return Instance.m_SpecificProps.GraveSingle;
		case ESpecificPropType.GraveDoubleSpawner:
			return Instance.m_SpecificProps.GraveDouble;
		case ESpecificPropType.QuestClaw:
			return Instance.m_SpecificProps.QuestClaw;
		case ESpecificPropType.QuestDoll:
			return Instance.m_SpecificProps.QuestDoll;
		case ESpecificPropType.TheFavorite:
			return Instance.m_SpecificProps.TheFavorite;
		case ESpecificPropType.TheNewFavorite:
			return Instance.m_SpecificProps.TheNewFavorite;
		case ESpecificPropType.LavaLootToken:
			return Instance.m_SpecificProps.LavaLootToken;
		case ESpecificPropType.CoralPiece:
			return Instance.m_SpecificProps.CoralPiece;
		case ESpecificPropType.GlowingOrb:
			return Instance.m_SpecificProps.GlowingOrb;
		case ESpecificPropType.PowerTower:
			return Instance.m_SpecificProps.PowerTower;
		case ESpecificPropType.HulkingPillar:
			return Instance.m_SpecificProps.HulkingPillar;
		case ESpecificPropType.SpecialConsole:
			return Instance.m_SpecificProps.SpecialConsole;
		case ESpecificPropType.Pedestal:
			return Instance.m_SpecificProps.Pedestal;
		case ESpecificPropType.AlgoxAltar:
			return Instance.m_SpecificProps.AlgoxAltar;
		case ESpecificPropType.MaintenanceShaft:
			return Instance.m_SpecificProps.MaintenanceShaft;
		case ESpecificPropType.ShadowWallResidue:
			return Instance.m_SpecificProps.ShadowWallResidue;
		case ESpecificPropType.DemonCorpsePile:
			return Instance.m_SpecificProps.DemonCorpsePile;
		case ESpecificPropType.SmallTree:
			return Instance.m_SpecificProps.SmallTree;
		case ESpecificPropType.Chasm:
			return Instance.m_SpecificProps.Chasm;
		case ESpecificPropType.TreeBridgeA:
			return Instance.m_SpecificProps.TreeBridgeA;
		case ESpecificPropType.TreeBridgeB:
			return Instance.m_SpecificProps.TreeBridgeB;
		case ESpecificPropType.OrbOfBones:
			return Instance.m_SpecificProps.OrbOfBones;
		case ESpecificPropType.CagedVermlingScout:
			return Instance.m_SpecificProps.CagedVermlingScout;
		case ESpecificPropType.TerrainLampA:
			return Instance.m_SpecificProps.TerrainLampA;
		case ESpecificPropType.TerrainLampB:
			return Instance.m_SpecificProps.TerrainLampB;
		case ESpecificPropType.TerrainLampC:
			return Instance.m_SpecificProps.TerrainLampC;
		case ESpecificPropType.TerrainLampD:
			return Instance.m_SpecificProps.TerrainLampD;
		case ESpecificPropType.Sarcophagus:
			return Instance.m_SpecificProps.Sarcophagus;
		case ESpecificPropType.LootableCorpse:
			return Instance.m_SpecificProps.LootableCorpse;
		case ESpecificPropType.ReceptacleContainer:
			return Instance.m_SpecificProps.ReceptacleContainer;
		case ESpecificPropType.LootableCrate:
		case ESpecificPropType.LootableCrate127:
			return Instance.m_SpecificProps.LootableCrate;
		case ESpecificPropType.LootableCrateLarge:
			return Instance.m_SpecificProps.LootableCrateLarge;
		case ESpecificPropType.FH_Scenario_Campaign_126_SCENARIO_OBJECT_1:
			return Instance.m_ApparanceProps.GoalChest;
		case ESpecificPropType.SunAgate:
			return Instance.m_SpecificProps.SunAgate;
		case ESpecificPropType.LumberPile:
			return Instance.m_SpecificProps.LumberPile;
		case ESpecificPropType.RadiantDust:
			return Instance.m_SpecificProps.RadiantDust;
		case ESpecificPropType.RockColumn:
			return Instance.m_SpecificProps.RockColumn;
		case ESpecificPropType.WeatheredAltar:
			return Instance.m_SpecificProps.WeatheredAltar;
		case ESpecificPropType.MovingPlatform:
			return Instance.m_SpecificProps.MovingPlatform;
		case ESpecificPropType.FloatingIce:
			return Instance.m_SpecificProps.FloatingIce;
		case ESpecificPropType.MovingBoat136:
			return Instance.m_SpecificProps.MovingBoat136;
		case ESpecificPropType.FlotsamOneHex:
			return Instance.m_SpecificProps.FlotsamOneHex;
		case ESpecificPropType.FlotsamTwoHex:
			return Instance.m_SpecificProps.FlotsamTwoHex;
		case ESpecificPropType.MovingBoat136B:
			return Instance.m_SpecificProps.MovingBoat136B;
		case ESpecificPropType.TrueOakA:
			return Instance.m_SpecificProps.TrueOakA;
		case ESpecificPropType.TrueOakB:
			return Instance.m_SpecificProps.TrueOakB;
		case ESpecificPropType.LurkerStatue:
			return Instance.m_SpecificProps.LurkerStatue;
		case ESpecificPropType.HealingPillar:
			return Instance.m_SpecificProps.HealingPillar;
		case ESpecificPropType.GlowingCrystal:
			return Instance.m_SpecificProps.GlowingCrystal;
		case ESpecificPropType.TemporalHex:
			return Instance.m_SpecificProps.TemporalHex;
		case ESpecificPropType.Temporal:
			return Instance.m_SpecificProps.Temporal;
		case ESpecificPropType.PortalTerrain:
			return Instance.m_SpecificProps.PortalTerrain;
		case ESpecificPropType.DarkPitTerrain:
			return Instance.m_SpecificProps.DarkPitTerrain;
		case ESpecificPropType.NothingTerrain:
			return Instance.m_SpecificProps.NothingTerrain;
		case ESpecificPropType.MysteriousMachine:
			return Instance.m_SpecificProps.MysteriousMachine;
		case ESpecificPropType.TotemOfBewilderment:
			return Instance.m_SpecificProps.TotemOfBewilderment;
		case ESpecificPropType.TotemOfEarth:
			return Instance.m_SpecificProps.TotemOfEarth;
		case ESpecificPropType.TotemOfRestoration:
			return Instance.m_SpecificProps.TotemOfRestoration;
		case ESpecificPropType.WarBanner:
			return Instance.m_SpecificProps.WarBanner;
		case ESpecificPropType.GoalChestAmuletProtection118:
			return Instance.m_SpecificProps.GoalChestAmuletProtection118;
		case ESpecificPropType.GoalChestAmuletReanimation118:
			return Instance.m_SpecificProps.GoalChestAmuletReanimation118;
		case ESpecificPropType.SnowDriftPile:
			return Instance.m_ApparanceProps.Snowdrift;
		case ESpecificPropType.DarkCrystalExposed:
			return Instance.m_SpecificProps.DarkCrystalExposed;
		case ESpecificPropType.Ladder:
			return Instance.m_SpecificProps.Ladder;
		case ESpecificPropType.AbaelHallway:
			return Instance.m_SpecificProps.AbaelHallway;
		case ESpecificPropType.MammothBark:
			return Instance.m_ApparanceProps.GoalChest;
		case ESpecificPropType.BlackCauldron:
			return Instance.m_SpecificProps.BlackCauldron;
		case ESpecificPropType.StatueOfTheBuilder:
			return Instance.m_SpecificProps.StatueOfTheBuilder;
		default:
			return null;
		}
	}

	public static GameObject GetVFXPropPrefab(string propTypeString)
	{
		return CObjectProp.VFXPropTypes.SingleOrDefault((EVFXPropType x) => x.ToString() == propTypeString) switch
		{
			EVFXPropType.ShowerOfPebbles => Instance.m_VFXProps.ShowerOfPebbles, 
			EVFXPropType.IceFall => Instance.m_VFXProps.IceFall, 
			EVFXPropType.WindCarving => Instance.m_VFXProps.WindCarving, 
			EVFXPropType.ElectricityBurst => Instance.m_VFXProps.ElectricityBurst, 
			EVFXPropType.ElectricityBurstPillar => Instance.m_VFXProps.ElectricityBurstPillar, 
			EVFXPropType.ElectricityBurstRepair => Instance.m_VFXProps.ElectricityBurstRepair, 
			EVFXPropType.EMPWave3HEX => Instance.m_VFXProps.EMPWave3HEX, 
			EVFXPropType.EMPWave1HEX => Instance.m_VFXProps.EMPWave1HEX, 
			EVFXPropType.EMPWave2HEX => Instance.m_VFXProps.EMPWave2HEX, 
			EVFXPropType.HarvestSnow => Instance.m_VFXProps.HarvestSnow, 
			EVFXPropType.DowsingRune => Instance.m_VFXProps.DowsingRune, 
			EVFXPropType.RuneInfluence => Instance.m_VFXProps.RuneInfluence, 
			EVFXPropType.RuneInfluenceStrong => Instance.m_VFXProps.RuneInfluenceStrong, 
			EVFXPropType.RuneInfluenceActivated => Instance.m_VFXProps.RuneInfluenceActivated, 
			EVFXPropType.ShadowAttack => Instance.m_VFXProps.ShadowAttack, 
			EVFXPropType.ShadowPortal => Instance.m_VFXProps.ShadowPortal, 
			EVFXPropType.DellingDestruction => Instance.m_VFXProps.DellingDestruction, 
			EVFXPropType.DrifterShockwave => Instance.m_VFXProps.DrifterShockwave, 
			EVFXPropType.DrifterShockwaveHit => Instance.m_VFXProps.DrifterShockwaveHit, 
			EVFXPropType.BoneshaperExplodingCorpse => Instance.m_VFXProps.BoneshaperExplodingCorpse, 
			EVFXPropType.DrifterDangerZoneImpact => Instance.m_VFXProps.DrifterDangerZoneImpact, 
			EVFXPropType.DrifterDangerZone => Instance.m_VFXProps.DrifterDangerZone, 
			EVFXPropType.RoomRedLight => Instance.m_VFXProps.RoomRedLight, 
			EVFXPropType.RoomBlueLight => Instance.m_VFXProps.RoomBlueLight, 
			EVFXPropType.PolarWind => Instance.m_VFXProps.PolarWind, 
			EVFXPropType.EnemySpawnEffect => Instance.m_VFXProps.EnemySpawnEffect, 
			EVFXPropType.BoneHordeAssimilateSkeletonVFX => Instance.m_VFXProps.BoneHordeAssimilateSkeletonVFX, 
			EVFXPropType.RendererShield => Instance.m_VFXProps.RendererShield, 
			EVFXPropType.WindCarvingShieldVFX => Instance.m_VFXProps.WindCarvingShieldVFX, 
			EVFXPropType.DustCloudS25Room1 => Instance.m_VFXProps.DustCloudS25Room1, 
			EVFXPropType.DustCloudS25Room2 => Instance.m_VFXProps.DustCloudS25Room2, 
			EVFXPropType.ChaosBeamTeleport => Instance.m_VFXProps.ChaosBeamTeleport, 
			EVFXPropType.SoilSample => Instance.m_VFXProps.SoilSample, 
			EVFXPropType.PolarBearTrap => Instance.m_VFXProps.PolarBearTrap, 
			EVFXPropType.OozeSlime => Instance.m_VFXProps.OozeSlime, 
			EVFXPropType.OozeRemains => Instance.m_VFXProps.OozeRemains, 
			EVFXPropType.HeartOfIceVFX => Instance.m_VFXProps.HeartOfIceVFX, 
			EVFXPropType.TuningDevice => Instance.m_VFXProps.TuningDevice, 
			EVFXPropType.TreasurePiece => Instance.m_VFXProps.TreasurePiece, 
			EVFXPropType.ElementalCore => Instance.m_VFXProps.ElementalCore, 
			EVFXPropType.IceDestructionGeneric => Instance.m_VFXProps.IceDestructionGeneric, 
			EVFXPropType.ElectricityRockColumnCoil => Instance.m_VFXProps.ElectricityRockColumnCoil, 
			EVFXPropType.LaserSpireBeam => Instance.m_VFXProps.LaserSpireBeam, 
			EVFXPropType.DemoCharge => Instance.m_VFXProps.DemoCharge, 
			EVFXPropType.BannerSpearSetForTheCharge => Instance.m_VFXProps.BannerSpearSetForTheCharge, 
			EVFXPropType.BannerSpearSetForTheChargeImpact => Instance.m_VFXProps.BannerSpearSetForTheChargeImpact, 
			EVFXPropType.TrapperEnhanceTrap => Instance.m_VFXProps.TrapperEnhanceTrap, 
			EVFXPropType.IceCorridorAS70 => Instance.m_VFXProps.IceCorridorAS70, 
			EVFXPropType.IceCorridorBS70 => Instance.m_VFXProps.IceCorridorBS70, 
			EVFXPropType.IcyRootsHazardous => Instance.m_VFXProps.IcyRootsHazardous, 
			EVFXPropType.IcyRootsDifficultA => Instance.m_VFXProps.IcyRootsDifficultA, 
			EVFXPropType.IcyRootsDifficultB => Instance.m_VFXProps.IcyRootsDifficultB, 
			EVFXPropType.IcyRootsObstacle => Instance.m_VFXProps.IcyRootsObstacle, 
			EVFXPropType.VermlingWarchiefShield => Instance.m_VFXProps.VermlingWarchiefShield, 
			EVFXPropType.BlizzardBarrier => Instance.m_VFXProps.BlizzardBarrier, 
			EVFXPropType.BlizzardBarrier2Hex => Instance.m_VFXProps.BlizzardBarrier2Hex, 
			EVFXPropType.LevelUpQuestDebriefing => Instance.m_VFXProps.LevelUpQuestDebriefing, 
			EVFXPropType.ItemShovelEffect => Instance.m_VFXProps.ItemShovelEffect, 
			EVFXPropType.ItemChaosCannonEffect => Instance.m_VFXProps.ItemChaosCannonEffect, 
			EVFXPropType.ItemVolatileBombEffect => Instance.m_VFXProps.ItemVolatileBombEffect, 
			EVFXPropType.ItemBoomBarrelModel => Instance.m_VFXProps.ItemBoomBarrelModel, 
			EVFXPropType.ItemBoomBarrelEffect => Instance.m_VFXProps.ItemBoomBarrelEffect, 
			EVFXPropType.ItemTrainersNetEffect => Instance.m_VFXProps.ItemTrainersNetEffect, 
			EVFXPropType.ItemExplosiveVialEffect => Instance.m_VFXProps.ItemExplosiveVialEffect, 
			EVFXPropType.PetPiranhaPigEffect => Instance.m_VFXProps.PetPiranhaPigEffect, 
			EVFXPropType.PetHoundEffect => Instance.m_VFXProps.PetHoundEffect, 
			EVFXPropType.PetSpittingDrakeEffect => Instance.m_VFXProps.PetSpittingDrakeEffect, 
			EVFXPropType.PetRendingDrakeEffect => Instance.m_VFXProps.PetRendingDrakeEffect, 
			EVFXPropType.PetBlackImpEffect => Instance.m_VFXProps.PetBlackImpEffect, 
			EVFXPropType.PetForestImpEffect => Instance.m_VFXProps.PetForestImpEffect, 
			EVFXPropType.PetSnowImpEffect => Instance.m_VFXProps.PetSnowImpEffect, 
			EVFXPropType.PetOozeEffect => Instance.m_VFXProps.PetOozeEffect, 
			EVFXPropType.PetOozeTargetEffect => Instance.m_VFXProps.PetOozeTargetEffect, 
			EVFXPropType.PetRuinedMachineEffect => Instance.m_VFXProps.PetRuinedMachineEffect, 
			EVFXPropType.PetLightningEelEffect => Instance.m_VFXProps.PetLightningEelEffect, 
			EVFXPropType.PetHEROICSEffect => Instance.m_VFXProps.PetHEROICSEffect, 
			EVFXPropType.PetBrummixEffect => Instance.m_VFXProps.PetBrummixEffect, 
			EVFXPropType.TormentorCreateTerrain => Instance.m_VFXProps.TormentorCreateTerrain, 
			EVFXPropType.TormentorViolentTerrainDamage => Instance.m_VFXProps.TormentorViolentTerrainDamage, 
			EVFXPropType.TitanBeam123 => Instance.m_VFXProps.TitanBeam123, 
			EVFXPropType.AmuletProtectionVFX => Instance.m_VFXProps.AmuletProtectionVFX, 
			EVFXPropType.AmuletReanimationVFX => Instance.m_VFXProps.AmuletReanimationVFX, 
			EVFXPropType.ChillingWind => Instance.m_VFXProps.ChillingWind, 
			EVFXPropType.PyroclastTerrainFormationVFX => Instance.m_VFXProps.PyroclastTerrainFormationVFX, 
			EVFXPropType.PyroclastObsidianPuffVFX => Instance.m_VFXProps.PyroclastObsidianPuffVFX, 
			EVFXPropType.PyroclastObstacleExplosionVFX => Instance.m_VFXProps.PyroclastObstacleExplosionVFX, 
			EVFXPropType.PiranhaPigTerrainSpawnVFX => Instance.m_VFXProps.PiranhaPigTerrainSpawnVFX, 
			EVFXPropType.WindCorridorMild105 => Instance.m_VFXProps.WindCorridorMild105, 
			EVFXPropType.WindCorridorIntense105 => Instance.m_VFXProps.WindCorridorIntense105, 
			EVFXPropType.ElementConsumeFireVFX => Instance.m_VFXProps.ElementConsumeFireVFX, 
			EVFXPropType.ElementConsumeIceVFX => Instance.m_VFXProps.ElementConsumeIceVFX, 
			EVFXPropType.ElementConsumeAirVFX => Instance.m_VFXProps.ElementConsumeAirVFX, 
			EVFXPropType.ElementConsumeEarthVFX => Instance.m_VFXProps.ElementConsumeEarthVFX, 
			EVFXPropType.ElementConsumeLightVFX => Instance.m_VFXProps.ElementConsumeLightVFX, 
			EVFXPropType.ElementConsumeDarkVFX => Instance.m_VFXProps.ElementConsumeDarkVFX, 
			EVFXPropType.ElementInfuseFireVFX => Instance.m_VFXProps.ElementInfuseFireVFX, 
			EVFXPropType.ElementInfuseIceVFX => Instance.m_VFXProps.ElementInfuseIceVFX, 
			EVFXPropType.ElementInfuseAirVFX => Instance.m_VFXProps.ElementInfuseAirVFX, 
			EVFXPropType.ElementInfuseEarthVFX => Instance.m_VFXProps.ElementInfuseEarthVFX, 
			EVFXPropType.ElementInfuseLightVFX => Instance.m_VFXProps.ElementInfuseLightVFX, 
			EVFXPropType.ElementInfuseDarkVFX => Instance.m_VFXProps.ElementInfuseDarkVFX, 
			EVFXPropType.S52GoalTilesVFX => Instance.m_VFXProps.S52GoalTilesVFX, 
			EVFXPropType.OozeToBossAssimilationVFXS84 => Instance.m_VFXProps.OozeToBossAssimilationVFXS84, 
			EVFXPropType.BossToOozeAssimilationVFXS84 => Instance.m_VFXProps.BossToOozeAssimilationVFXS84, 
			EVFXPropType.S77EnvironmentalVFX => Instance.m_VFXProps.S77EnvironmentalVFX, 
			EVFXPropType.S109LaserSpireLightingVFX => Instance.m_VFXProps.S109LaserSpireLightingVFX, 
			EVFXPropType.S90FrozenCorpseIce => Instance.m_VFXProps.S90FrozenCorpseIce, 
			EVFXPropType.NumberCounter => Instance.m_VFXProps.NumberCounter, 
			EVFXPropType.S21OrbExplosion => Instance.m_VFXProps.S21OrbExplosion, 
			EVFXPropType.S64BossLigtningVFX => Instance.m_VFXProps.S64BossLigtningVFX, 
			EVFXPropType.S125FireVFX => Instance.m_VFXProps.S125FireVFX, 
			EVFXPropType.SnowMist => Instance.m_VFXProps.SnowMist, 
			EVFXPropType.S125ExplosionVFX => Instance.m_VFXProps.S125ExplosionVFX, 
			EVFXPropType.S135CopyIndicationVFX => Instance.m_VFXProps.S135CopyIndicationVFX, 
			EVFXPropType.WaterSplash => Instance.m_VFXProps.WaterSplash, 
			EVFXPropType.S63EnvVFX => Instance.m_VFXProps.S63EnvVFX, 
			EVFXPropType.S98FallingDebris => Instance.m_VFXProps.S98FallingDebris, 
			EVFXPropType.NumberCounterActor => Instance.m_VFXProps.NumberCounterActor, 
			EVFXPropType.NumberCounterActorDescending => Instance.m_VFXProps.NumberCounterActorDescending, 
			EVFXPropType.RotatingMonsterProjection => Instance.m_VFXProps.RotatingMonsterProjection, 
			_ => null, 
		};
	}

	public static FireSpawnedProjectile.ExternalSettings GetProjectileSettings(string name)
	{
		if (name == null)
		{
			return null;
		}
		return Instance.m_GlobalProjectiles.FirstOrDefault((Projectile p) => p.Name == name)?.settings;
	}
}
