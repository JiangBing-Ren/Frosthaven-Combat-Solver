using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FFSNet;
using FHGUI;
using FHGUI.Common.Glossary;
using FHGUI.Common.Tooltips;
using FHGUI.Configs;
using FHGUI.DTO;
using FHGUI.DTO.Outpost;
using FHGUI.DataSO;
using FHGUI.Scenario.Use_Bar;
using FHGUI.Services;
using FHRL;
using Libs.Utils;
using MapRuleLibrary.Adventure;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.U2D;

public class UIInfoTools : MonoBehaviour
{
	public enum BackgroundColour
	{
		None,
		Blue,
		Red,
		AdvantageBlue,
		DisadvantageRed
	}

	public enum AdvantageType
	{
		None,
		PositiveUsed,
		PositiveDiscarded,
		NegativeUsed,
		NegativeDiscarded
	}

	public enum TooltipColours
	{
		NoChange,
		White,
		Orange,
		Red,
		Purple
	}

	[Serializable]
	public class CharacterOutlinableColourSetting
	{
		public CActor.EType ActorType;

		[ColorUsage(true, true)]
		public Color OutlineColor;
	}

	[Serializable]
	public class PropOutlinableColourSetting
	{
		public ScenarioManager.ObjectImportType PropType;

		[ColorUsage(true, true)]
		public Color OutlineColor;
	}

	[Serializable]
	public struct PuzzleBookImage
	{
		public string PuzzleID;

		public Sprite PuzzleImage;

		public Sprite PuzzleCompletedImage;
	}

	private static UIInfoTools instance;

	public Sprite MissingSpriteIcon;

	public Sprite BlankSpriteIcon;

	[Header("Ability Card Layout Sprites")]
	public Sprite CommandIcon;

	public Sprite XPArrowIcon;

	public SpriteAtlas AreaEffectSpriteAtlas;

	[Header("Effects")]
	public EffectsUIDataSO EffectsUIData;

	[Header("Loot rewards")]
	public Sprite LootMoney;

	public Sprite LootLumber;

	public Sprite LootMetal;

	public Sprite LootHide;

	public Sprite LootArrowvine;

	public Sprite LootAxenut;

	public Sprite LootCorpsecap;

	public Sprite LootFlamefruit;

	public Sprite LootRockroot;

	public Sprite LootSnowthistle;

	public Sprite LootRandomItem;

	public Sprite DamageSprite;

	public ElementConfigUI airConfig;

	public ElementConfigUI fireConfig;

	public ElementConfigUI iceConfig;

	public ElementConfigUI earthConfig;

	public ElementConfigUI lightConfig;

	public ElementConfigUI darkConfig;

	public ElementConfigUI anyElementConfig;

	public ElementConfigUI airDarkConfig;

	public ElementConfigUI airEarthConfig;

	public ElementConfigUI airLightConfig;

	public ElementConfigUI earthLightConfig;

	public ElementConfigUI earthDarkConfig;

	public ElementConfigUI fireAirConfig;

	public ElementConfigUI fireDarkConfig;

	public ElementConfigUI fireEarthConfig;

	public ElementConfigUI fireIceConfig;

	public ElementConfigUI firelightConfig;

	public ElementConfigUI iceAirConfig;

	public ElementConfigUI iceDarkConfig;

	public ElementConfigUI iceEarthConfig;

	public ElementConfigUI iceLightConfig;

	public ElementConfigUI lightDarkConfig;

	[SerializeField]
	private Sprite[] TraitIocns;

	[SerializeField]
	private Sprite[] xpSprites;

	[SerializeField]
	private Sprite[] modifiersSprites;

	[SerializeField]
	private Sprite[] immunityIcons;

	[SerializeField]
	private EnhancementEffectsData[] enhancementEffects;

	public Color White;

	public Color Orange;

	public Color Red;

	public Color Purple;

	public Color Green;

	[Header("Attack modifiers")]
	public Sprite NullModifierIcon;

	public Sprite Pull;

	public Sprite Push;

	public Sprite Push2;

	public Sprite Push3;

	public Sprite Heal;

	public Sprite Target;

	public Sprite Pierce;

	public Sprite Generic;

	public Sprite Damage;

	public Sprite HealAlly0;

	public Sprite HealAlly1;

	public Sprite Heal2Ally0;

	public Sprite Shield;

	public Sprite Blessed;

	public Sprite Cursed;

	public Sprite Brittle;

	public Sprite Kill;

	public Sprite IgnoreNegativeItemEffectsIcon;

	public Sprite IgnoreNegativeScenarioEffectsIcon;

	public Sprite TimeToken;

	public Sprite CreateIce;

	public Sprite Strengthen;

	public Sprite Move;

	public Sprite ModeTransfer;

	public AttackModifierConfig[] customModifierConfigs;

	public Color lowModifierColor = Color.magenta;

	public Color regularModifierColor = Color.white;

	public Color highModifierColor = Color.red;

	[Header("Perks")]
	public PerksUIDataSO PerksUIData;

	[Header("Characters")]
	[SerializeField]
	public List<CharacterConfigUI> characterConfigsUI;

	public Sprite[] characterIcons;

	public Sprite[] actorPortraits;

	[Header("UI Text Colors")]
	public Material greyedOutMaterial;

	public Material disabledGrayscaleMaterial;

	[Header("New adventure Text Colors")]
	[ColorUsage(true, true)]
	public Color warningColor;

	[ColorUsage(true, true)]
	public Color goldColor;

	[ColorUsage(true, true)]
	public Color buyEnhancementColor;

	[ColorUsage(true, true)]
	public Color sellEnhancementColor;

	[Header("MP Text Colors")]
	[ColorUsage(true, true)]
	public Color playerOnlineColor = Color.green;

	[ColorUsage(true, true)]
	public Color playerConnectingColor = Color.red;

	public MultiplayerVisualsUIDataSO MultiplayerVisualsUIDataSo;

	[Header("Active abilities")]
	[ColorUsage(true, true)]
	public Color currentRoundColor = Color.white;

	[ColorUsage(true, true)]
	public Color pastRoundColor;

	[ColorUsage(true, true)]
	public Color leftRoundColor;

	[ColorUsage(true, true)]
	public Color currentRoundTextColor = Color.white;

	[ColorUsage(true, true)]
	public Color pastRoundTextColor;

	[ColorUsage(true, true)]
	public Color leftRoundTextColor;

	[Header("Class-Specific")]
	[SerializeField]
	private Sprite _agilityFast;

	[SerializeField]
	private Sprite _agilitySlow;

	[SerializeField]
	private Sprite[] difficultyIconSprites;

	[Header("Items")]
	[SerializeField]
	private ItemRarityConfigUI[] itemRarityConfigs;

	[SerializeField]
	private List<ItemConfigUI> itemConfigs;

	private Dictionary<string, ItemConfigUI> tempItemConfigsDict;

	public Sprite PersistentBonus;

	public Sprite RoundBonus;

	public Sprite SpentIcon;

	public Sprite BurnIcon;

	[SerializeField]
	private ItemSlotConfigUI[] itemSlotConfigs;

	[SerializeField]
	private List<OutpostResourceConfigUI> resourcesConfigs;

	[SerializeField]
	private List<FavorConfigUI> favorsConfigs;

	[Header("Challenges")]
	[SerializeField]
	private Sprite[] ChallengesSprites;

	[Header("Input Action Configs")]
	[SerializeField]
	private List<InputActionConfigUI> inputActionConfigs;

	[Header("Enhancements")]
	[SerializeField]
	private EnhancementsUIDataSO EnhancementsUIData;

	[Header("Abilities")]
	[SerializeField]
	private AbilityTypePreviewConfigUI[] abilityTypePreviewConfigs;

	private Dictionary<CAbility.EAbilityType, AbilityTypePreviewConfigUI> tempAbilityTypePreviewConfigsDict;

	[SerializeField]
	private PreviewEffectConfigUI[] previewEffectConfigs;

	private Dictionary<string, PreviewEffectConfigUI> tempPreviewEffectConfigsDict;

	[Header("Campaigns")]
	[SerializeField]
	private Sprite[] campaignIllustrationSprites;

	[SerializeField]
	private Sprite[] LocationSprites;

	[Header("ContextualHints")]
	[SerializeField]
	private Sprite[] contextualHintSprites;

	[Header("ClassIcons")]
	public Sprite[] characterClassIcons;

	[Header("CharacterSlot Action Icons")]
	public Sprite[] characterSlotActionIcons;

	public Sprite[] genericIcons;

	[Header("Story")]
	public List<StoryCharacterConfigUI> StoryCharacters;

	[Header("Outpost Panel Illustrations")]
	public Sprite[] outpostPanelIllustration;

	[Header("Quest")]
	public Sprite FHGenericQuestChainIcon;

	public Color FHGenericQuestChainColor;

	public QuestChainConfigUI[] FHQuestChains;

	[Header("Narrative")]
	[FormerlySerializedAs("questLoadouts")]
	public NarrativeConfigUI[] narrativeImages;

	public NarrativeDataSO narrativeDB;

	[Header("Battle Stats")]
	[SerializeField]
	private Sprite[] BattleStats;

	[Header("Debriefing Badges")]
	[SerializeField]
	private Sprite[] DebriefingBadges;

	[Header("Outpost Events")]
	public OutpostEventsConfigUI[] outpostEventImages;

	public OutpostEventTypesConfigUI[] outpostEventTypeImages;

	[Header("Rewards")]
	[SerializeField]
	private RewardConfigUI[] campaingQuestRewards;

	[Header("Carryable Item")]
	public Sprite CarryableQuestItemWorldSprite;

	public Sprite CarryableQuestItemIconSprite;

	[Header("UI Components")]
	public float typingRevealDelay = 0.1f;

	[SerializeField]
	private TMP_SpriteAsset AbilityCardSpriteAsset;

	private List<TMP_SpriteAsset> tmpAbilityCardSpriteAssets = new List<TMP_SpriteAsset>();

	[Header("Audio")]
	public AudioButtonProfile generalAudioButtonProfile;

	public AudioMapViewModuleProfile defaultAudioMapViewModuleProfile;

	public string toggleUseCharacterSlotAudioItem = "PlaySound_UIMercAbility";

	public string tileClickAudioItem = "PlaySound_ScenarioUI_TileConfirm";

	public string tileCancelClickAudioItem = "PlaySound_UIUndoHex";

	[AudioEventName]
	public string merchantBuyAudioItem = "PlaySound_UIMerchantBuy";

	[AudioEventName]
	public string merchantSellAudioItem = "PlaySound_UIMerchantSell";

	[AudioEventName]
	public string itemEquipAudioItem = "PlaySound_UIItemEquip";

	[AudioEventName]
	public string itemUnequipAudioItem = "PlaySound_UIItemUnequip";

	[AudioEventName]
	public string cardToDeckEquipAudioItem = "PlaySound_UICardToDeckEquip";

	[AudioEventName]
	public string cardRemoveFromDeckAudioItem = "PlaySound_UICardRemoveFromDeckUnequip";

	[AudioEventName]
	public string heroAddAudioItem = "PlaySound_UIHeroAdd";

	[AudioEventName]
	public string heroRemoveAudioItem = "PlaySound_UIHeroRemove";

	[AudioEventName]
	public string brewPotionAudioItem = "PlaySound_UIBrewPotion";

	[AudioEventName]
	public string distillPotionAudioItem = "PlaySound_UIDistillPotion";

	[AudioEventName]
	public string craftItemAudioItem = "PlaySound_UICraftItem";

	[AudioEventName]
	public string startBuildingAudioItem = "PlaySound_UIStartBuilding";

	[AudioEventName]
	public string radioButtonPressAudioItem = "PlaySound_UIRadioButtonPress";

	[AudioEventName]
	public string questStartAudioItem = "PlaySound_UIQuestStart";

	[AudioEventName]
	public string questMovementStartAudioItem = "PlaySound_UIQuestMovementStart";

	[AudioEventName]
	public string encounterAudioItem = "PlaySound_UIEncounter";

	[AudioEventName]
	public string outpostAttackAudioItem = "PlaySound_UIOutpostAttack";

	[AudioEventName]
	public string outpostAttackSuccessAudioItem = "PlaySound_UIOutpostAttackSuccess";

	[AudioEventName]
	public string outpostAttackDefeatAudioItem = "PlaySound_UIOutpostAttackDefeat";

	[AudioEventName]
	public string outpostAttackBuildingDamageAudioItem = "PlaySound_UIOutpostAttackBuildingDamage";

	[AudioEventName]
	public string genericSystemMessageAudioItem = "PlaySound_UIGenericSystemMessage";

	[AudioEventName]
	public string playerJoinedAudioItem = "PlaySound_UIPlayerJoined";

	[AudioEventName]
	public string playerDisconnectedAudioItem = "PlaySound_UIPlayerDisconnected";

	[AudioEventName]
	public string genericNotificationAudioItem = "PlaySound_UIGenericNotification";

	[Header("MP Notifications")]
	public float disconnectedPlayerNotificationDuration = 10f;

	public float selectedOtherPlayerCardNotificationDuration = 10f;

	public float defaultNotificationDuration = 5f;

	[Header("Outpost Notifications")]
	public NotificationUIDataSO NotificationData;

	private static readonly Color[] defaultPlayerColors = new Color[4]
	{
		Color.red,
		Color.blue,
		Color.yellow,
		Color.green
	};

	public AbilityCardUISkinConfig DefaultCardSkin;

	[Header("DLC")]
	[SerializeField]
	private List<DLCConfig> dlcConfigs;

	[Header("Buildings")]
	public OutpostBuildingDataSO Buildings;

	[Header("Monsters")]
	public MonsterUIDataSO Monsters;

	[Header("Summons")]
	public SummonUIDataSO Summons;

	[Header("General Info")]
	public Sprite TrapDefaultIcon;

	public Sprite ShadowPropDefaultIcon;

	public Sprite DoorPropDefaultIcon;

	public Sprite LootPropDefaultIcon;

	public Sprite ObstaclePropDefaultIcon;

	public Sprite ChestPropDefaultIcon;

	public Sprite SpawnerPropDefaultIcon;

	public Sprite PressurePlatePropDefaultIcon;

	public Sprite HazardousTerrainDefaultIcon;

	public Sprite DifficultTerrainDefaultIcon;

	public Sprite IceTerrainDefaultIcon;

	public Sprite PortalDefaultIcon;

	public Sprite QuestItemDefaultIcon;

	[SerializeField]
	private GlossaryDataSO _glossaryData;

	[Header("Puzzle Book")]
	public PuzzleBookImage[] PuzzleImages;

	[Header("Calendar")]
	[SerializeField]
	public Sprite UnknownCalendarEventSprite;

	[SerializeField]
	public Sprite CalendarEventSprite;

	[SerializeField]
	public Sprite TravellingMerchantCalendarEventSprite;

	public static UIInfoToolsDef DefInstance => UIInfoToolsDef.Instance;

	public static UIInfoTools Instance => instance;

	[field: SerializeField]
	public ActiveAbilityDataSO ActiveAbilityData { get; private set; }

	public string InvalidOptionAudioItem => generalAudioButtonProfile.nonInteractableMouseDownAudioItem;

	[field: SerializeField]
	public FHItemTooltip ItemTooltip { get; private set; }

	public static GlossaryDataSO GlossaryData => Instance._glossaryData;

	[field: SerializeField]
	public UIAbilityCardTooltip CardTooltip { get; private set; }

	public static IEnumerator InitInstanceCrt()
	{
		if (!(instance != null))
		{
			ByRef<GameObject> asset = new ByRef<GameObject>();
			yield return AssetBundleManager.Instance.LoadAssetFromBundleAsync("misc_globalsettings", "UIInfoTools", "globalsettings", asset);
			if ((bool)asset.Value)
			{
				instance = asset.Value.GetComponent<UIInfoTools>();
			}
		}
	}

	public static void ForceLoadInstance()
	{
		if (!(instance != null))
		{
			instance = AssetBundleManager.Instance.LoadAssetFromBundle<GameObject>("misc_globalsettings", "UIInfoTools", "globalsettings").GetComponent<UIInfoTools>();
		}
	}

	public Sprite GetIconPositiveCondition(CCondition.EPositiveCondition condition)
	{
		return EffectsUIData.GetIcon(condition);
	}

	public Sprite GetIconNegativeCondition(CCondition.ENegativeCondition condition)
	{
		return EffectsUIData.GetIcon(condition);
	}

	public Sprite GetIconCondition(string conditionId)
	{
		return EffectsUIData.GetIcon(conditionId);
	}

	public Sprite GetIconLootType(LootCard.EType type)
	{
		return type switch
		{
			LootCard.EType.Money => LootMoney, 
			LootCard.EType.Lumber => LootLumber, 
			LootCard.EType.Metal => LootMetal, 
			LootCard.EType.Hide => LootHide, 
			LootCard.EType.Arrowvine => LootArrowvine, 
			LootCard.EType.Axenut => LootAxenut, 
			LootCard.EType.Corpsecap => LootCorpsecap, 
			LootCard.EType.Flamefruit => LootFlamefruit, 
			LootCard.EType.Rockroot => LootRockroot, 
			LootCard.EType.Snowthistle => LootSnowthistle, 
			LootCard.EType.RandomItem => LootRandomItem, 
			_ => null, 
		};
	}

	private ElementConfigUI GetElementConfig(CardEnhancementEffectType element, CardEnhancementEffectType? secondElement = null)
	{
		return GetElementConfig(CrossReference.CardEnhancementTypeToEElement(element), secondElement.HasValue ? new ElementInfusionBoardManager.EElement?(CrossReference.CardEnhancementTypeToEElement(secondElement.Value)) : ((ElementInfusionBoardManager.EElement?)null));
	}

	public ElementConfigUI GetElementConfig(ElementInfusionBoardManager.EElement element, ElementInfusionBoardManager.EElement? secondElement = null)
	{
		return element switch
		{
			ElementInfusionBoardManager.EElement.Air => secondElement switch
			{
				ElementInfusionBoardManager.EElement.Earth => airEarthConfig, 
				ElementInfusionBoardManager.EElement.Dark => airDarkConfig, 
				ElementInfusionBoardManager.EElement.Ice => iceAirConfig, 
				ElementInfusionBoardManager.EElement.Fire => fireAirConfig, 
				ElementInfusionBoardManager.EElement.Light => airLightConfig, 
				_ => airConfig, 
			}, 
			ElementInfusionBoardManager.EElement.Earth => secondElement switch
			{
				ElementInfusionBoardManager.EElement.Air => airEarthConfig, 
				ElementInfusionBoardManager.EElement.Dark => earthDarkConfig, 
				ElementInfusionBoardManager.EElement.Ice => iceEarthConfig, 
				ElementInfusionBoardManager.EElement.Fire => fireEarthConfig, 
				ElementInfusionBoardManager.EElement.Light => earthLightConfig, 
				_ => earthConfig, 
			}, 
			ElementInfusionBoardManager.EElement.Fire => secondElement switch
			{
				ElementInfusionBoardManager.EElement.Light => firelightConfig, 
				ElementInfusionBoardManager.EElement.Air => fireAirConfig, 
				ElementInfusionBoardManager.EElement.Earth => fireEarthConfig, 
				ElementInfusionBoardManager.EElement.Dark => fireDarkConfig, 
				ElementInfusionBoardManager.EElement.Ice => fireIceConfig, 
				_ => fireConfig, 
			}, 
			ElementInfusionBoardManager.EElement.Ice => secondElement switch
			{
				ElementInfusionBoardManager.EElement.Earth => iceEarthConfig, 
				ElementInfusionBoardManager.EElement.Air => iceAirConfig, 
				ElementInfusionBoardManager.EElement.Dark => iceDarkConfig, 
				ElementInfusionBoardManager.EElement.Light => iceLightConfig, 
				_ => iceConfig, 
			}, 
			ElementInfusionBoardManager.EElement.Light => secondElement switch
			{
				ElementInfusionBoardManager.EElement.Air => airLightConfig, 
				ElementInfusionBoardManager.EElement.Dark => lightDarkConfig, 
				_ => lightConfig, 
			}, 
			ElementInfusionBoardManager.EElement.Dark => secondElement switch
			{
				ElementInfusionBoardManager.EElement.Air => airDarkConfig, 
				ElementInfusionBoardManager.EElement.Earth => earthDarkConfig, 
				_ => darkConfig, 
			}, 
			_ => anyElementConfig, 
		};
	}

	public string GetSelectElementAudioItemIcon(ElementInfusionBoardManager.EElement element)
	{
		return GetElementConfig(element)?.selectAudioItem;
	}

	public Sprite GetRewardElementIcon(ElementInfusionBoardManager.EElement element)
	{
		ElementConfigUI elementConfig = GetElementConfig(element);
		if (!(elementConfig.pickerIcon == null))
		{
			return elementConfig.rewardIcon;
		}
		return elementConfig.icon;
	}

	public Sprite GetElementIcon(ElementInfusionBoardManager.EElement element, ElementInfusionBoardManager.EElement? secondElement = null)
	{
		return GetElementConfig(element, secondElement).icon;
	}

	public Sprite GetElementIcon(CardEnhancementEffectType element, CardEnhancementEffectType? secondElement = null)
	{
		return GetElementConfig(element, secondElement).icon;
	}

	private Sprite GetElementIcon(CardEnhancementEffectType element, CardEnhancementEffectType secondElement)
	{
		return GetElementConfig(element, secondElement).icon;
	}

	public Color GetElementHighlightColor(ElementInfusionBoardManager.EElement element, float? alpha = null)
	{
		Color result = GetElementConfig(element).highlightColor;
		if (alpha.HasValue)
		{
			result = new Color(result.r, result.g, result.b, alpha.Value);
		}
		return result;
	}

	public Sprite GetElementPickerSprite(ElementInfusionBoardManager.EElement element)
	{
		ElementConfigUI elementConfig = GetElementConfig(element);
		if (!(elementConfig.pickerIcon == null))
		{
			return elementConfig.pickerIcon;
		}
		return elementConfig.icon;
	}

	public Sprite GetElementPickerSelectedSprite(ElementInfusionBoardManager.EElement element)
	{
		ElementConfigUI elementConfig = GetElementConfig(element);
		if (!(elementConfig.pickerIcon == null))
		{
			return elementConfig.selectedIcon;
		}
		return elementConfig.icon;
	}

	public Sprite GetElementUseSprite(ElementInfusionBoardManager.EElement element)
	{
		ElementConfigUI elementConfig = GetElementConfig(element);
		if (!(elementConfig.useIcon == null))
		{
			return elementConfig.useIcon;
		}
		return elementConfig.icon;
	}

	public Sprite GetCharacterTraitIcon(CharacterTraitType trait)
	{
		return TraitIocns.GetSprite(trait.ToString());
	}

	public Sprite GetXPSprite(int xp)
	{
		if (xp < 0 || xp >= xpSprites.Count())
		{
			Log.GUIWarning("Can't find 'XP" + xp + "' sprite, using 0 instead");
			return xpSprites[0];
		}
		return xpSprites[xp];
	}

	public CharacterTabSkin GetCharacterTabSkin(string characterName, string custom = null)
	{
		return GetCharacterConfigUI(characterName, useDefault: true, custom).tabIconConfig;
	}

	public Sprite GetCharacterAssemblySprite(ECharacter character, string custom = null)
	{
		return GetCharacterConfigUI(character, useDefault: true, custom).assemblyPortrait;
	}

	public Sprite GetCharacterAssemblyIcon(ECharacter character, string custom = null)
	{
		return GetCharacterConfigUI(character, useDefault: true, custom).iconClassGold;
	}

	public Sprite GetCharacterAssemblyIcon(string character, string custom = null)
	{
		return GetCharacterConfigUI(character, useDefault: true, custom).iconClassGold;
	}

	public List<string> GetCharacterAppearanceSkin(string characterName, string custom = null)
	{
		return GetCharacterConfigUI(characterName, useDefault: false, custom)?.alternativeSkins;
	}

	public Sprite GetCharacterActiveAbilityIcon(string characterName, string custom = null)
	{
		return GetCharacterConfigUI(characterName, useDefault: true, custom)?.activeAbility;
	}

	public Sprite GetActiveAbilityIcon(string iconName, string fallbackIconName = null, bool displayError = true)
	{
		return ActiveAbilityData.GetActiveAbilityIcon(iconName, fallbackIconName, displayError);
	}

	public Sprite GetActiveAbilityIcon(CActiveBonus activeBonus)
	{
		return ActiveAbilityData.GetActiveAbilityIcon(activeBonus);
	}

	public EffectDataBase GetElementEffect(ElementInfusionBoardManager.EElement element)
	{
		return GetElementConfig(element).effectData;
	}

	public EnhancementEffectsData GetEnhancementEffect(EEnhancement enhancement)
	{
		EnhancementEffectsData[] array = enhancementEffects;
		foreach (EnhancementEffectsData enhancementEffectsData in array)
		{
			if (enhancementEffectsData.enhancement == enhancement)
			{
				return enhancementEffectsData;
			}
		}
		return null;
	}

	public string GetActiveAbilityIconName(CActiveBonus activeBonus)
	{
		return ActiveAbilityData.GetActiveAbilityIconName(activeBonus);
	}

	public Sprite GetActiveAbilityIconBackground(CActor actor)
	{
		if (actor is CPlayerActor)
		{
			return GetCharacterConfigUI(actor.GetPrefabName(), useDefault: true, (actor as CPlayerActor)?.CharacterClass?.CharacterYML.CustomCharacterConfig).initativeExtensionBackground;
		}
		return GetCharacterConfigUI(actor.GetPrefabName()).initativeExtensionBackground;
	}

	public Sprite GetAttackModifierSprite(string mathModifier)
	{
		return modifiersSprites.GetSprite(mathModifier.Replace('*', 'x'));
	}

	public Sprite GetImmunityIcon(string immunity)
	{
		return immunityIcons.GetSprite(immunity);
	}

	public Sprite GetCharacterHeroPortrait(string character, string custom = null)
	{
		return GetCharacterConfigUI(character, useDefault: true, custom).scenarioPortrait;
	}

	public Sprite GetNewAdventureCharacterPortrait(ECharacter character, bool highlighted = false, string custom = null)
	{
		return GetCharacterConfigUI(character, useDefault: true, custom).GetAdventurePortrait(highlighted);
	}

	public static Sprite GetHeroPortrait(string actorModel)
	{
		Sprite sprite = Instance.GetCharacterConfigUI(actorModel)?.illustration;
		if (!(sprite != null))
		{
			return GetCharacterBannerPortrait(actorModel);
		}
		return sprite;
	}

	public static Sprite GetMonsterPortrait(string monsterId, string model, out bool isFromConfig)
	{
		Sprite sprite = Instance.GetMonsterConfig(monsterId, model)?.Portrait;
		isFromConfig = sprite != null;
		if (!isFromConfig)
		{
			return GetCharacterBannerPortrait(model);
		}
		return sprite;
	}

	public static Sprite GetSummonPortrait(string summonId, string model, out bool isFromConfig)
	{
		Sprite sprite = Instance.GetSummonConfig(summonId)?.Portrait;
		isFromConfig = sprite != null;
		if (isFromConfig)
		{
			return sprite;
		}
		sprite = GetCharacterBannerPortrait(model, usePlaceholderOnFail: false);
		if (sprite != null)
		{
			return sprite;
		}
		return Instance.GetActorPortrait(model);
	}

	public static Sprite GetCharacterBannerPortrait(string character, bool usePlaceholderOnFail = true)
	{
		Sprite sprite = AssetBundleManager.Instance.LoadAssetFromBundle<Sprite>("misc_characterportraits", character.ToLowerInvariant(), "characterportraits", "png", null, suppressError: true);
		if ((sprite == null) & usePlaceholderOnFail)
		{
			Log.GUIWarningFormat("[ASSET BUNDLE MANAGER] - Unable to load asset: {0} in asset bundle: misc_characterportraits  folder: characterportraits", character);
			return GetCharacterBannerPortrait("_Placeholder_Portrait", usePlaceholderOnFail: false);
		}
		return sprite;
	}

	public Sprite GetAttackModifierIcon(AttackModifierYMLData modifier, bool useOriginal = false)
	{
		AttackModifierConfig attackModifierConfig = customModifierConfigs.FirstOrDefault((AttackModifierConfig it) => it.name == modifier.Name);
		if (attackModifierConfig != null)
		{
			return attackModifierConfig.icon;
		}
		if (modifier.UseGenericIcon)
		{
			return Generic;
		}
		if (modifier.Card.Infuse)
		{
			if (modifier.Card.InfuseElements.Count == 6)
			{
				return GetElementIcon(CardEnhancementEffectType.ElementWild);
			}
			if (modifier.Card.InfuseElements.Count > 1)
			{
				return GetElementIcon(modifier.Card.InfuseElements[0], modifier.Card.InfuseElements[1]);
			}
			return GetElementIcon(modifier.Card.InfuseElements[0]);
		}
		if (modifier.Card.Consume)
		{
			if (modifier.Card.ConsumeElements.Count > 1)
			{
				return GetElementIcon(modifier.Card.ConsumeElements[0], modifier.Card.ConsumeElements[1]);
			}
			return GetElementIcon(modifier.Card.ConsumeElements[0]);
		}
		if (modifier.Card.NegativeConditions.Count > 0)
		{
			return GetIconNegativeCondition(modifier.Card.NegativeConditions[0]);
		}
		if (modifier.AddTarget)
		{
			return Target;
		}
		if (modifier.IsBless)
		{
			return Blessed;
		}
		if (modifier.IsCurse)
		{
			return Cursed;
		}
		if (modifier.Card.Pierce)
		{
			return Pierce;
		}
		if (modifier.Card.Damage)
		{
			return Damage;
		}
		if (modifier.Card.Strengthen)
		{
			return Strengthen;
		}
		if (modifier.Card.Heal)
		{
			return Heal;
		}
		if (modifier.Card.HealAlly)
		{
			if (modifier.MathModifier == "+0")
			{
				if (modifier.Card.MainAbilityStrength == 2)
				{
					return Heal2Ally0;
				}
				return HealAlly0;
			}
			return HealAlly1;
		}
		if (modifier.Card.Pull)
		{
			return Pull;
		}
		if (modifier.Card.Push)
		{
			if (modifier.Card.Push3)
			{
				return Push3;
			}
			if (modifier.Card.Push2)
			{
				return Push2;
			}
			return Push;
		}
		if (modifier.Card.Shield)
		{
			return Shield;
		}
		if (modifier.Card.Kill)
		{
			return Kill;
		}
		if (modifier.Card.Brittle)
		{
			return Brittle;
		}
		if (modifier.Card.PositiveConditions.Count > 0)
		{
			return GetIconPositiveCondition(modifier.Card.PositiveConditions[0]);
		}
		if (modifier.Card.TimeToken)
		{
			return TimeToken;
		}
		if (modifier.Card.CreateIce)
		{
			return CreateIce;
		}
		if (modifier.Card.Move)
		{
			return Move;
		}
		if (modifier.Card.ModeTransfer)
		{
			return ModeTransfer;
		}
		string text = ((useOriginal && modifier.OriginalModifier.IsNOTNullOrEmpty()) ? modifier.OriginalModifier : modifier.MathModifier);
		if (text == "*0" || text == "x0")
		{
			return NullModifierIcon;
		}
		return null;
	}

	public Sprite GetPremadeAttackModifierIcon(CharacterAttackModifierDef def)
	{
		return PerksUIData.GetPremadeAttackModifierIcon(def.ID.ID);
	}

	public Sprite GetPremadeAttackModifierIcon(AttackModifierYMLData modifier)
	{
		return PerksUIData.GetPremadeAttackModifierIcon(modifier.Name);
	}

	public Sprite GetAttackModifierBaseIcon(CharacterAttackModifierDef modifier)
	{
		return PerksUIData.GetAttackModifierBaseIcon(modifier.MathModifier);
	}

	public Sprite GetAttackModifierBaseIcon(AttackModifierYMLData modifier)
	{
		return PerksUIData.GetAttackModifierBaseIcon(modifier.MathModifier);
	}

	public Sprite GetAttackModifierIconNew(AttackModifierYMLData modifier)
	{
		Sprite valueIcon = PerksUIData.GetValueIcon(modifier.Name);
		if (valueIcon != null)
		{
			return valueIcon;
		}
		AttackModifierConfig attackModifierConfig = customModifierConfigs.FirstOrDefault((AttackModifierConfig it) => it.name == modifier.Name);
		if (attackModifierConfig != null)
		{
			return attackModifierConfig.icon;
		}
		if (modifier.Card.Infuse)
		{
			if (modifier.Card.InfuseElements.Count > 1)
			{
				return GetElementIcon(modifier.Card.InfuseElements[0], modifier.Card.InfuseElements[1]);
			}
			return GetElementIcon(modifier.Card.InfuseElements[0]);
		}
		if (modifier.Card.Consume)
		{
			if (modifier.Card.ConsumeElements.Count > 1)
			{
				return GetElementIcon(modifier.Card.ConsumeElements[0], modifier.Card.ConsumeElements[1]);
			}
			return GetElementIcon(modifier.Card.ConsumeElements[0]);
		}
		if (modifier.Card.NegativeConditions.Count > 0)
		{
			string text = modifier.Card.NegativeConditions[0].ToString();
			return PerksUIData.GetValueIcon(text) ?? GetIconCondition(text);
		}
		if (modifier.AddTarget)
		{
			return PerksUIData.GetValueIcon("AddTarget", Target);
		}
		if (modifier.IsBless)
		{
			return PerksUIData.GetValueIcon("Bless", Blessed);
		}
		if (modifier.IsCurse)
		{
			return PerksUIData.GetValueIcon("Curse", Cursed);
		}
		if (modifier.Card.Pierce)
		{
			return PerksUIData.GetValueIcon("Pierce", Pierce);
		}
		if (modifier.Card.Damage)
		{
			return PerksUIData.GetValueIcon("Damage", Damage);
		}
		if (modifier.Card.Heal)
		{
			return PerksUIData.GetValueIcon("Heal", Heal);
		}
		if (modifier.Card.HealAlly)
		{
			Sprite sprite = PerksUIData.GetValueIcon("HealAlly");
			if ((object)sprite == null)
			{
				if (!(modifier.MathModifier == "+0"))
				{
					return HealAlly1;
				}
				if (modifier.Card.MainAbilityStrength != 2)
				{
					return HealAlly0;
				}
				sprite = Heal2Ally0;
			}
			return sprite;
		}
		if (modifier.Card.Pull)
		{
			return PerksUIData.GetValueIcon("Pull", Pull);
		}
		if (modifier.Card.Push)
		{
			if (!modifier.Card.Push2)
			{
				return PerksUIData.GetValueIcon("Push", Push);
			}
			return PerksUIData.GetValueIcon("Push2", Push2);
		}
		if (modifier.Card.Shield)
		{
			return PerksUIData.GetValueIcon("Shield", Shield);
		}
		if (modifier.Card.Kill)
		{
			return PerksUIData.GetValueIcon("Kill", Kill);
		}
		if (modifier.Card.Brittle)
		{
			return PerksUIData.GetValueIcon("Brittle", Brittle);
		}
		if (modifier.Card.PositiveConditions.Count > 0)
		{
			string text2 = modifier.Card.PositiveConditions[0].ToString();
			return PerksUIData.GetValueIcon(text2, GetIconCondition(text2));
		}
		if (modifier.Card.TimeToken)
		{
			return PerksUIData.GetValueIcon("TimeToken", TimeToken);
		}
		if (modifier.Card.CreateIce)
		{
			return PerksUIData.GetValueIcon("CreateIce", CreateIce);
		}
		if (modifier.Card.Strengthen)
		{
			return PerksUIData.GetValueIcon("Strengthen", Strengthen);
		}
		if (modifier.Card.Move)
		{
			return PerksUIData.GetValueIcon("Move", Move);
		}
		if (modifier.Card.ModeTransfer)
		{
			return PerksUIData.GetValueIcon("ModeTransfer", ModeTransfer);
		}
		return null;
	}

	public Sprite GetAttackModifierIcon(CharacterAttackModifierDef modifier, bool useOriginal = false)
	{
		if (useOriginal)
		{
			return GetAttackModifierIcon(CrossReference.AttackModifierDefToYML(modifier));
		}
		Sprite valueIcon = PerksUIData.GetValueIcon(modifier.ID.ID);
		if (valueIcon != null)
		{
			return valueIcon;
		}
		AttackModifierConfig attackModifierConfig = customModifierConfigs.FirstOrDefault((AttackModifierConfig it) => it.name == modifier.ID.ID);
		if (attackModifierConfig != null)
		{
			return attackModifierConfig.icon;
		}
		if (modifier.Infuse)
		{
			if (modifier.InfuseElements.Count > 1)
			{
				return GetElementIcon(modifier.InfuseElements[0], modifier.InfuseElements[1]);
			}
			return GetElementIcon(modifier.InfuseElements[0]);
		}
		if (modifier.Consume)
		{
			if (modifier.ConsumeElements.Count > 1)
			{
				return GetElementIcon(modifier.ConsumeElements[0], modifier.ConsumeElements[1]);
			}
			return GetElementIcon(modifier.ConsumeElements[0]);
		}
		if (modifier.NegativeConditions.Count > 0)
		{
			string text = modifier.NegativeConditions[0];
			return PerksUIData.GetValueIcon(text) ?? GetIconCondition(text);
		}
		if (modifier.AddTarget)
		{
			return PerksUIData.GetValueIcon("AddTarget", Target);
		}
		if (modifier.IsBless)
		{
			return PerksUIData.GetValueIcon("Bless", Blessed);
		}
		if (modifier.IsCurse)
		{
			return PerksUIData.GetValueIcon("Curse", Cursed);
		}
		if (modifier.Pierce)
		{
			return PerksUIData.GetValueIcon("Pierce", Pierce);
		}
		if (modifier.Damage)
		{
			return PerksUIData.GetValueIcon("Damage", Damage);
		}
		if (modifier.Heal)
		{
			return PerksUIData.GetValueIcon("Heal", Heal);
		}
		if (modifier.HealAlly)
		{
			Sprite sprite = PerksUIData.GetValueIcon("HealAlly");
			if ((object)sprite == null)
			{
				if (!(modifier.MathModifier == "+0"))
				{
					return HealAlly1;
				}
				if (modifier.MainAbilityStrength != 2)
				{
					return HealAlly0;
				}
				sprite = Heal2Ally0;
			}
			return sprite;
		}
		if (modifier.Pull)
		{
			return PerksUIData.GetValueIcon("Pull", Pull);
		}
		if (modifier.Push)
		{
			if (modifier.Push3)
			{
				return PerksUIData.GetValueIcon("Push3", Push3);
			}
			if (modifier.Push2)
			{
				return PerksUIData.GetValueIcon("Push2", Push2);
			}
			return PerksUIData.GetValueIcon("Push", Push);
		}
		if (modifier.Shield)
		{
			return PerksUIData.GetValueIcon("Shield", Shield);
		}
		if (modifier.Kill)
		{
			return PerksUIData.GetValueIcon("Kill", Kill);
		}
		if (modifier.Brittle)
		{
			return PerksUIData.GetValueIcon("Brittle", Brittle);
		}
		if (modifier.PositiveConditions.Count > 0)
		{
			string text2 = modifier.PositiveConditions[0];
			return PerksUIData.GetValueIcon(text2, GetIconCondition(text2));
		}
		if (modifier.TimeToken)
		{
			return PerksUIData.GetValueIcon("TimeToken", TimeToken);
		}
		if (modifier.CreateIce)
		{
			return PerksUIData.GetValueIcon("CreateIce", CreateIce);
		}
		if (modifier.Strengthen)
		{
			return PerksUIData.GetValueIcon("Strengthen", Strengthen);
		}
		if (modifier.Move)
		{
			return PerksUIData.GetValueIcon("Move", Move);
		}
		if (modifier.ModeTransfer)
		{
			return PerksUIData.GetValueIcon("ModeTransfer", ModeTransfer);
		}
		return null;
	}

	public Color GetTownGuardAttackModifierColor(TownGuardAttackModifierDef modifier)
	{
		if (modifier.Type == TownGuardModType.Failure)
		{
			return lowModifierColor;
		}
		if (modifier.Type == TownGuardModType.Success)
		{
			return highModifierColor;
		}
		if (modifier.Type == TownGuardModType.Modifier)
		{
			if (modifier.Value >= 30)
			{
				return highModifierColor;
			}
			if (modifier.Value < 0)
			{
				return lowModifierColor;
			}
		}
		return regularModifierColor;
	}

	public Sprite GetTownGuardAttackModifierValueIcon(TownGuardAttackModifierDef modifier)
	{
		return GetTownGuardAttackModifierValueIcon(modifier.ID.ID);
	}

	public Sprite GetTownGuardAttackModifierValueIcon(string modifierId)
	{
		return PerksUIData.GetTownGuardModifierBaseIcon(modifierId);
	}

	public string GetTownGuardAttackModifierText(TownGuardAttackModifierDef modifier)
	{
		if (modifier.Type == TownGuardModType.Failure)
		{
			return "Failure";
		}
		if (modifier.Type == TownGuardModType.Success)
		{
			return "Success";
		}
		return modifier.MathModifier;
	}

	public string GetAttackModifierText(AttackModifierYMLData modifier, bool useOriginal = false)
	{
		AttackModifierConfig attackModifierConfig = customModifierConfigs.FirstOrDefault((AttackModifierConfig it) => it.name == modifier.Name);
		string text = ((useOriginal && modifier.OriginalModifier.IsNOTNullOrEmpty()) ? modifier.OriginalModifier : modifier.MathModifier);
		if (attackModifierConfig != null)
		{
			if (!attackModifierConfig.showMathModifier)
			{
				return string.Empty;
			}
			return text;
		}
		if (modifier.Card.Damage)
		{
			return string.Empty;
		}
		if (modifier.Card.HealAlly)
		{
			return string.Empty;
		}
		if (modifier.Card.Heal)
		{
			return $"+{modifier.Card.HealAmount}";
		}
		if (modifier.AddTarget && text == "+0")
		{
			return "+";
		}
		if (text == "*0")
		{
			return "x0";
		}
		if (text == "*2")
		{
			return "x2";
		}
		return text;
	}

	public string GetAttackModifierText(CharacterAttackModifierDef modifier)
	{
		AttackModifierConfig attackModifierConfig = customModifierConfigs.FirstOrDefault((AttackModifierConfig it) => it.name == modifier.ID.ID);
		string mathModifier = modifier.MathModifier;
		if (attackModifierConfig != null)
		{
			if (!attackModifierConfig.showMathModifier)
			{
				return string.Empty;
			}
			return mathModifier;
		}
		if (modifier.Damage)
		{
			return string.Empty;
		}
		if (modifier.HealAlly)
		{
			return string.Empty;
		}
		if (modifier.Heal)
		{
			return $"+{modifier.HealAmount}";
		}
		if (modifier.AddTarget && mathModifier == "+0")
		{
			return "+";
		}
		if (modifier.Rolling && mathModifier == "+0")
		{
			return string.Empty;
		}
		if (mathModifier == "*0")
		{
			return "x0";
		}
		if (mathModifier == "*2")
		{
			return "x2";
		}
		return mathModifier;
	}

	public Color GetAttackModifierColor(string mathModifier)
	{
		switch (mathModifier)
		{
		case "x2":
		case "*2":
			return highModifierColor;
		case "x0":
		case "*0":
		case "-2":
		case "-1":
			return lowModifierColor;
		case "+1":
		case "+2":
		case "+3":
		case "+4":
		case "+0":
		case "-0":
			return regularModifierColor;
		default:
			return regularModifierColor;
		}
	}

	public Sprite GetPerkIcon(PerkDef perkDef, bool useDefaultWhenMissing = false)
	{
		return GetPerkIcon(perkDef.ID.ID, useDefaultWhenMissing);
	}

	public Sprite GetPerkIcon(TownGuardPerkDef townGuardPerkDef, bool useDefaultWhenMissing = false)
	{
		return GetPerkIcon(string.Format("{0}{1}", "TownGuard", townGuardPerkDef.ID.ID), useDefaultWhenMissing);
	}

	public Sprite GetPerkIcon(string perkId, bool useDefaultWhenMissing = false)
	{
		Sprite sprite = PerksUIData.PerkConfigs.FirstOrDefault((PerkConfigUI pc) => pc.Id.Equals(perkId, StringComparison.InvariantCulture))?.Icon;
		if (useDefaultWhenMissing && sprite == null)
		{
			sprite = MissingSpriteIcon;
		}
		return sprite;
	}

	public CharacterConfigUI GetCharacterConfigUIFromString(string character, bool useDefault = true)
	{
		ECharacter eCharacter = CCharacterClass.CharacterNames.SingleOrDefault((ECharacter s) => s.ToString() == character);
		if (eCharacter != ECharacter.None)
		{
			return GetCharacterConfigUI(eCharacter, useDefault);
		}
		return null;
	}

	public CharacterConfigUI GetCharacterConfigUI(ECharacter character, bool useDefault = true, string customConfigID = null)
	{
		return GetCharacterConfigUI(character.ToString(), useDefault, customConfigID);
	}

	public Color GetCharacterColor(ECharacter character, string custom = null)
	{
		return GetCharacterConfigUI(character.ToString(), useDefault: true, custom).questConfig.color;
	}

	public Color GetCharacterColor(CPlayerActor character)
	{
		return GetCharacterColor(character.CharacterClass.CharacterModel, character.CharacterClass.CharacterYML.CustomCharacterConfig);
	}

	public Color GetCharacterAltColor(ECharacter character, string custom = null)
	{
		return GetCharacterConfigUI(character.ToString(), useDefault: true, custom).questConfig.altColor;
	}

	public Color GetCharacterAltColor(CPlayerActor character)
	{
		return GetCharacterAltColor(character.CharacterClass.CharacterModel, character.CharacterClass.CharacterYML.CustomCharacterConfig);
	}

	public string GetCharacterHexColor(CPlayerActor character)
	{
		return GetCharacterColor(character).ToHex();
	}

	public string GetCharacterHexColor(ECharacter character)
	{
		return GetCharacterColor(character).ToHex();
	}

	public Sprite GetCharacterMarker(ECharacter character, string custom = null)
	{
		return GetCharacterMarker(character.ToString(), custom);
	}

	public Sprite GetCharacterMarker(string character, string custom = null)
	{
		return GetCharacterConfigUI(character, useDefault: true, custom).questConfig.marker;
	}

	public Sprite GetCharacterCardHeader(ECharacter character, int cardID, string customConfigID = null)
	{
		return GetCharacterConfigUI(character.ToString(), useDefault: true, customConfigID)?.GetCardHeader(cardID);
	}

	public CharacterConfigUI GetCharacterConfigUI(string character, bool useDefault = true, string customConfigID = null)
	{
		if (customConfigID == null)
		{
			string charName = Regex.Replace(character, "ID$", "");
			CharacterConfigUI characterConfigUI = characterConfigsUI.SingleOrDefault((CharacterConfigUI x) => charName.Equals(x.character.ToString(), StringComparison.InvariantCultureIgnoreCase));
			if (!characterConfigUI)
			{
				if (!useDefault || characterConfigsUI.Count <= 0)
				{
					return null;
				}
				return characterConfigsUI[0];
			}
			return characterConfigUI;
		}
		if (!useDefault || characterConfigsUI.Count <= 0)
		{
			return null;
		}
		return characterConfigsUI[0];
	}

	public Sprite GetCharacterSprite(ECharacter character, bool highlight = false, string custom = null)
	{
		CharacterConfigUI characterConfigUI = GetCharacterConfigUI(character, useDefault: false, custom);
		if (characterConfigUI != null)
		{
			if (!highlight)
			{
				return characterConfigUI.iconClass;
			}
			return characterConfigUI.iconClassHighlight;
		}
		return characterIcons.GetSprite(character.ToString());
	}

	[Obsolete]
	public Sprite GetActorPortrait(string actorModel, string customPortrait = null)
	{
		return actorPortraits.FirstOrDefault();
	}

	public Sprite GetCharacterDistributionPortrait(ECharacter character, string custom = null)
	{
		return GetCharacterConfigUI(character, useDefault: true, custom)?.distributionPortrait;
	}

	public Sprite GetActiveAbilityDurationIcon(CActiveBonus.EActiveBonusDurationType duration)
	{
		return ActiveAbilityData.GetActiveAbilityDurationIcon(duration);
	}

	public Sprite GetDurationIcon(CActiveBonus.EActiveBonusDurationType duration)
	{
		return ActiveAbilityData.GetDurationIcon(duration);
	}

	public static Color GetCharacterOutlineColor(CActor.EType actorType)
	{
		(UIInfoToolsDef.Instance?.m_CharacterOutlineColourDictionary).TryGetValue(actorType, out var value);
		return value;
	}

	public static Color GetPropOutlineColor(ScenarioManager.ObjectImportType objectImportType)
	{
		Dictionary<ScenarioManager.ObjectImportType, Color> dictionary = UIInfoToolsDef.Instance?.m_PropOutlineColourDictionary;
		if (dictionary.TryGetValue(objectImportType, out var value))
		{
			return value;
		}
		dictionary.TryGetValue(ScenarioManager.ObjectImportType.None, out value);
		return value;
	}

	public Color ReturnHealthColour(int maxHealth, int currentHealth)
	{
		if ((float)currentHealth <= (float)maxHealth * 0.25f)
		{
			return Red;
		}
		if (currentHealth < maxHealth)
		{
			return Orange;
		}
		return Green;
	}

	public int CalculateModifierTotal(int baseValue, ModifierDisplayStruct mod, bool noNegativeResults = false)
	{
		int num = baseValue + mod.Value;
		int num2 = baseValue * mod.Value;
		if (noNegativeResults)
		{
			if (num < 0)
			{
				num = 0;
			}
			if (num2 < 0)
			{
				num2 = 0;
			}
		}
		return mod.MathType switch
		{
			ModifierDisplayStruct.MathOperation.Add => num, 
			ModifierDisplayStruct.MathOperation.Subtract => num, 
			ModifierDisplayStruct.MathOperation.Multiply => num2, 
			_ => num, 
		};
	}

	public string ConstructModifierValueForDisplay(ModifierDisplayStruct mod)
	{
		return mod.MathType switch
		{
			ModifierDisplayStruct.MathOperation.Add => "+" + mod.Value, 
			ModifierDisplayStruct.MathOperation.Subtract => mod.Value.ToString(), 
			ModifierDisplayStruct.MathOperation.Multiply => "X" + mod.Value, 
			_ => "+" + mod.Value, 
		};
	}

	public BackgroundColour ReturnModifierColour(ModifierDisplayStruct mod, bool advantage = false)
	{
		if (mod.ModifierType == ModifierDisplayStruct.ModType.Critical)
		{
			return BackgroundColour.Blue;
		}
		switch (mod.BenfitType)
		{
		case ModifierDisplayStruct.Benefit.Positive:
			if (!advantage)
			{
				return BackgroundColour.Blue;
			}
			return BackgroundColour.AdvantageBlue;
		case ModifierDisplayStruct.Benefit.Negative:
			if (!advantage)
			{
				return BackgroundColour.Red;
			}
			return BackgroundColour.DisadvantageRed;
		default:
			return BackgroundColour.None;
		}
	}

	public Color ReturnTooltipColour(TooltipColours col)
	{
		return col switch
		{
			TooltipColours.Orange => Orange, 
			TooltipColours.White => White, 
			TooltipColours.Red => Red, 
			TooltipColours.Purple => Purple, 
			_ => White, 
		};
	}

	public AbilityCardUISkin GetCardSkin(string characterName, bool isAltForm = false, string custom = null)
	{
		CharacterConfigUI characterConfigUI = GetCharacterConfigUI(characterName, useDefault: true, custom);
		return (isAltForm ? characterConfigUI.altFormCardSkin : characterConfigUI.cardSkin).Skin ?? Instance.DefaultCardSkin.Skin;
	}

	public Sprite GetAgilityIcon(CPlayerActor.EAgility agility)
	{
		return agility switch
		{
			CPlayerActor.EAgility.Fast => _agilityFast, 
			CPlayerActor.EAgility.Slow => _agilitySlow, 
			_ => null, 
		};
	}

	public Color GetDifficultyColor(EAdventureDifficulty eDifficulty)
	{
		switch (eDifficulty)
		{
		case EAdventureDifficulty.Story:
		case EAdventureDifficulty.Normal:
			return DefInstance.greenDifficultyColor;
		case EAdventureDifficulty.Veteran:
		case EAdventureDifficulty.Hard:
			return DefInstance.yellowDifficultyColor;
		case EAdventureDifficulty.Brutal:
		case EAdventureDifficulty.Unfair:
			return DefInstance.redDifficultyColor;
		default:
			return DefInstance.greenDifficultyColor;
		}
	}

	public Sprite GetDifficultyIcon(EAdventureDifficulty eDifficulty)
	{
		return difficultyIconSprites.GetSprite(eDifficulty.ToString());
	}

	public ItemConfigUI GetItemConfig(string name)
	{
		if (tempItemConfigsDict == null)
		{
			tempItemConfigsDict = itemConfigs.Where((ItemConfigUI it) => it != null).ToDictionary((ItemConfigUI it) => it.itemName.ToLower(), (ItemConfigUI it) => it);
		}
		if (string.IsNullOrEmpty(name))
		{
			return tempItemConfigsDict.First().Value;
		}
		if (tempItemConfigsDict.ContainsKey(name.ToLower()))
		{
			return tempItemConfigsDict[name.ToLower()];
		}
		ItemConfigUI itemConfigUI = itemConfigs.FirstOrDefault((ItemConfigUI it) => it != null && string.Equals(it.itemName, name, StringComparison.InvariantCultureIgnoreCase));
		if ((bool)itemConfigUI)
		{
			tempItemConfigsDict[itemConfigUI.itemName.ToLower()] = itemConfigUI;
			return itemConfigUI;
		}
		Log.GUIErrorFormat("Not found item configuration for {0}", name);
		return tempItemConfigsDict.First().Value;
	}

	public void AddItemConfig(ItemConfigUI config)
	{
		itemConfigs.Add(config);
		if (tempItemConfigsDict != null)
		{
			tempItemConfigsDict[config.itemName.ToLower()] = config;
		}
	}

	public Sprite GetItemBackgroundSprite(string name)
	{
		return GetItemConfig(name).backgroundImage;
	}

	public ItemSlotConfigUI GetItemSlotConfig(string slotName)
	{
		ItemSlotConfigUI[] array = itemSlotConfigs;
		foreach (ItemSlotConfigUI itemSlotConfigUI in array)
		{
			if (itemSlotConfigUI.Id == slotName)
			{
				return itemSlotConfigUI;
			}
		}
		return null;
	}

	public Sprite GetItemSlotIcon(string slotName)
	{
		return GetItemSlotConfig(slotName)?.Icon ?? MissingSpriteIcon;
	}

	public OutpostResourceConfigUI GetResourceConfig(OutpostMaterialType material)
	{
		return GetResourceConfig(material.ToString());
	}

	public OutpostResourceConfigUI GetResourceConfig(OutpostHerbType herb)
	{
		return GetResourceConfig(herb.ToString());
	}

	public OutpostResourceConfigUI GetResourceConfig(string id)
	{
		foreach (OutpostResourceConfigUI resourcesConfig in resourcesConfigs)
		{
			if (resourcesConfig.Id == id)
			{
				return resourcesConfig;
			}
		}
		Log.GUIErrorFormat("Not found resource configuration for " + id);
		return null;
	}

	public OutpostResourceConfigUI GetHerbConfig(OutpostHerbType herb)
	{
		string text = Enum.GetName(typeof(OutpostHerbType), herb);
		foreach (OutpostResourceConfigUI resourcesConfig in resourcesConfigs)
		{
			if (resourcesConfig.Id == text)
			{
				return resourcesConfig;
			}
		}
		Log.GUIErrorFormat("Not found resource configuration for " + text);
		return null;
	}

	public FavorConfigUI GetFavorConfig(ScenarioFavorType type)
	{
		foreach (FavorConfigUI favorsConfig in favorsConfigs)
		{
			if (favorsConfig.Id == type)
			{
				return favorsConfig;
			}
		}
		Log.GUIErrorFormat($"Not found favor configuration for {type}");
		return null;
	}

	public Sprite GetChallengesSprite(string name)
	{
		return ChallengesSprites.GetSprite(name);
	}

	public InputActionConfigUI GetInputActionConfig(FHInputEventElement.UserInteractionType action)
	{
		foreach (InputActionConfigUI inputActionConfig in inputActionConfigs)
		{
			if (inputActionConfig.Action == action)
			{
				return inputActionConfig;
			}
		}
		Log.GUIErrorFormat($"Not found input action configuration for {action}");
		return null;
	}

	public Sprite GetEnhancementIcon(EEnhancement enhancement)
	{
		return EnhancementsUIData.GetEnhancementIcon(enhancement);
	}

	public string GetAbilityPreviewEffectIconName(CAbility.EAbilityType abilityType)
	{
		return GetAbilityPreviewConfig(abilityType)?.iconName;
	}

	public AbilityTypePreviewConfigUI GetAbilityPreviewConfig(CAbility.EAbilityType abilityType)
	{
		if (tempAbilityTypePreviewConfigsDict == null)
		{
			tempAbilityTypePreviewConfigsDict = abilityTypePreviewConfigs.ToDictionary((AbilityTypePreviewConfigUI it) => it.type, (AbilityTypePreviewConfigUI it) => it);
		}
		if (tempAbilityTypePreviewConfigsDict.ContainsKey(abilityType))
		{
			return tempAbilityTypePreviewConfigsDict[abilityType];
		}
		AbilityTypePreviewConfigUI abilityTypePreviewConfigUI = abilityTypePreviewConfigs.FirstOrDefault((AbilityTypePreviewConfigUI it) => it.type == abilityType);
		if (abilityTypePreviewConfigUI != null)
		{
			tempAbilityTypePreviewConfigsDict[abilityTypePreviewConfigUI.type] = abilityTypePreviewConfigUI;
			return abilityTypePreviewConfigUI;
		}
		return null;
	}

	public PreviewEffectInfo GetPreviewEffectConfig(string id)
	{
		if (id.IsNullOrEmpty())
		{
			return null;
		}
		if (tempPreviewEffectConfigsDict == null)
		{
			tempPreviewEffectConfigsDict = previewEffectConfigs.ToDictionary((PreviewEffectConfigUI it) => it.id, (PreviewEffectConfigUI it) => it);
		}
		if (tempPreviewEffectConfigsDict.ContainsKey(id))
		{
			return tempPreviewEffectConfigsDict[id].previewData;
		}
		PreviewEffectConfigUI previewEffectConfigUI = previewEffectConfigs.FirstOrDefault((PreviewEffectConfigUI it) => it.id == id);
		if (previewEffectConfigUI != null)
		{
			tempPreviewEffectConfigsDict[previewEffectConfigUI.id] = previewEffectConfigUI;
			return previewEffectConfigUI.previewData;
		}
		return null;
	}

	public Sprite GetCampaignIllustration(string campaignId, bool active)
	{
		string text = (active ? ("Campaign_" + campaignId + "_Selected") : ("Campaign_" + campaignId));
		return campaignIllustrationSprites.GetSprite(text);
	}

	public Sprite GetLocationIcon(string regionId)
	{
		return LocationSprites.GetSprite(regionId, toLower: true, useDefaultWhenMissing: false);
	}

	public Sprite GetContextualHintIllustration(string spriteId)
	{
		return contextualHintSprites.GetSprite(spriteId, toLower: true, useDefaultWhenMissing: false);
	}

	public Sprite GetCharacterClassIcon(string id)
	{
		return characterClassIcons.GetSprite(id);
	}

	public Sprite GetCharacterSlotActionIcon(string id)
	{
		return characterSlotActionIcons.GetSprite(id);
	}

	public Sprite GetGenericIcon(string id)
	{
		return genericIcons.GetSprite(id);
	}

	public StoryCharacterConfigUI GetStoryCharacter(string characterGUID)
	{
		StoryCharacterConfigUI storyCharacterConfigUI = StoryCharacters.FirstOrDefault((StoryCharacterConfigUI c) => c.CharacterGUID == characterGUID);
		if (storyCharacterConfigUI == null)
		{
			Log.GUIWarning("Story Character config Warning - Unable to find Character with GUID:" + characterGUID);
		}
		return storyCharacterConfigUI;
	}

	public Sprite GetOutpostPanelIllustration(string spriteName)
	{
		return outpostPanelIllustration.GetSprite(spriteName);
	}

	public string GetStoryCharacterDisplayNameKey(string characterGUID)
	{
		return GetStoryCharacter(characterGUID)?.CharacterDisplayNameKey;
	}

	public Sprite GetStoryCharacterExpression(string characterGUID, EExpression expression)
	{
		return GetStoryCharacter(characterGUID)?.GetExpression(expression);
	}

	public Sprite GetStoryCharacterShield(string characterGUID)
	{
		return GetStoryCharacter(characterGUID)?.shieldIcon;
	}

	public Sprite GetQuestChainIcon(ScenarioChainType chain)
	{
		QuestChainConfigUI questChainConfigUI = FHQuestChains.FirstOrDefault((QuestChainConfigUI c) => c.IsApplicableTo(chain));
		if (questChainConfigUI == null)
		{
			return FHGenericQuestChainIcon;
		}
		return questChainConfigUI.Icon;
	}

	public Color GetQuestChainColor(ScenarioChainType chain)
	{
		QuestChainConfigUI[] fHQuestChains = FHQuestChains;
		foreach (QuestChainConfigUI questChainConfigUI in fHQuestChains)
		{
			if (questChainConfigUI.IsApplicableTo(chain))
			{
				return questChainConfigUI.Color;
			}
		}
		return FHGenericQuestChainColor;
	}

	public NarrativeConfigUI GetNarrativeImageConfig(string id, bool throwError = true)
	{
		NarrativeConfigUI narrativeConfigUI = narrativeImages.FirstOrDefault((NarrativeConfigUI it) => it.id.Equals(id, StringComparison.OrdinalIgnoreCase));
		if (narrativeConfigUI == null)
		{
			if (throwError)
			{
				Log.GUIError("No image found for Scenario Narrative id:" + id);
			}
			return narrativeImages[0];
		}
		return narrativeConfigUI;
	}

	public Sprite GetNarrativeIllustration(string id, bool throwError = true)
	{
		if (string.IsNullOrEmpty(id))
		{
			return null;
		}
		Sprite illustration = narrativeDB.GetIllustration(id);
		if (throwError && illustration == null)
		{
			Log.GUIError("No image found for Scenario Narrative id:" + id);
		}
		return illustration;
	}

	public StoryCharacterConfigUI GetNarrativeStoryCharacter(string id, bool throwError = false)
	{
		if (string.IsNullOrEmpty(id))
		{
			return null;
		}
		StoryCharacterConfigUI storyCharacter = narrativeDB.GetStoryCharacter(id);
		if (throwError && storyCharacter == null)
		{
			Log.GUIError("No image found for Scenario Narrative id:" + id);
		}
		return storyCharacter;
	}

	public Sprite GetNarrativeImage(string id, bool throwError = true)
	{
		NarrativeConfigUI narrativeConfigUI = narrativeImages.FirstOrDefault((NarrativeConfigUI it) => it.id.Equals(id, StringComparison.OrdinalIgnoreCase));
		if (narrativeConfigUI == null)
		{
			if (throwError)
			{
				Log.GUIError("No image found for id:" + id);
			}
			return null;
		}
		return narrativeConfigUI.picture;
	}

	public Sprite GetBattleStatIcon(string statName)
	{
		return BattleStats.GetSprite(statName);
	}

	public Sprite GetDebriefingBadgeIcon(string statName)
	{
		return DebriefingBadges.GetSprite(statName);
	}

	public OutpostEventsConfigUI GetOutpostEventImageConfig(string id)
	{
		OutpostEventsConfigUI outpostEventsConfigUI = outpostEventImages.FirstOrDefault((OutpostEventsConfigUI it) => it.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
		if (outpostEventsConfigUI == null)
		{
			Log.GUIError("No image found for id:" + id);
			return outpostEventImages[0];
		}
		return outpostEventsConfigUI;
	}

	public Sprite GetOutpostEventImage(string id, bool throwError = true)
	{
		OutpostEventsConfigUI outpostEventsConfigUI = outpostEventImages.FirstOrDefault((OutpostEventsConfigUI it) => it.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
		if (outpostEventsConfigUI == null)
		{
			if (throwError)
			{
				Log.GUIError("No image found for id:" + id);
			}
			return null;
		}
		return outpostEventsConfigUI.Image;
	}

	public IEnumerable<Sprite> GetOutpostEventImages(string startsWithId, bool throwError = true)
	{
		IEnumerable<OutpostEventsConfigUI> source = outpostEventImages.Where((OutpostEventsConfigUI i) => i.Id.StartsWith(startsWithId));
		if (!source.Any())
		{
			if (throwError)
			{
				Log.GUIError("No images found for startsWithId:" + startsWithId);
			}
			return null;
		}
		return source.Select((OutpostEventsConfigUI s) => s.Image);
	}

	public Sprite GetOutpostEventTypeImage(string id, bool throwError = true)
	{
		OutpostEventTypesConfigUI outpostEventTypesConfigUI = outpostEventTypeImages.FirstOrDefault((OutpostEventTypesConfigUI it) => it.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
		if (outpostEventTypesConfigUI == null)
		{
			if (throwError)
			{
				Log.GUIError("No image found for id:" + id);
			}
			return null;
		}
		return outpostEventTypesConfigUI.Image;
	}

	public Sprite GetRewardIcon(Reward reward)
	{
		if (reward.Type == ETreasureType.UnlockCharacter)
		{
			string defaultModel = CharacterClassManager.Classes.Single((CCharacterClass s) => s.ID == reward.CharacterID).DefaultModel;
			string customCharacterConfig = CharacterClassManager.Classes.Single((CCharacterClass s) => s.ID == reward.CharacterID).CharacterYML.CustomCharacterConfig;
			CharacterConfigUI characterConfigUI = GetCharacterConfigUI(defaultModel, useDefault: true, customCharacterConfig);
			if (characterConfigUI == null)
			{
				return null;
			}
			return characterConfigUI.iconClass;
		}
		if (reward.Type == ETreasureType.Resource)
		{
			return GetResourceConfig(reward.ResourceType.ToString()).Icon;
		}
		if (reward.Type == ETreasureType.Item || reward.Type == ETreasureType.SoldItem || reward.Type == ETreasureType.ItemStock || reward.Type == ETreasureType.ItemBlueprint || reward.Type == ETreasureType.LoseItem || reward.Type == ETreasureType.UnlockProsperityItem || reward.Type == ETreasureType.UnlockProsperityItemStock || reward.Type == ETreasureType.LoseItem)
		{
			return GetItemConfig(reward.Item.YMLData.Art)?.miniIcon;
		}
		if (reward.Type == ETreasureType.Enhancement)
		{
			return GetEnhancementIcon(reward.Enhancement);
		}
		if (reward.Type == ETreasureType.Infuse)
		{
			return GetRewardElementIcon(reward.Infusions.SingleOrDefault());
		}
		if (reward.Type == ETreasureType.ConsumeItem)
		{
			return campaingQuestRewards.FirstOrDefault((RewardConfigUI it) => it.type == reward.Type && it.extraId == reward.ConsumeSlot && (it.distribution == ETreasureDistributionType.None || it.distribution == reward.TreasureDistributionType))?.icon;
		}
		if (reward.Type == ETreasureType.Condition)
		{
			RewardCondition condition = reward.Conditions.SingleOrDefault();
			if (condition.PositiveCondition == CCondition.EPositiveCondition.NA)
			{
				return campaingQuestRewards.FirstOrDefault((RewardConfigUI it) => it.type == reward.Type && it.extraId == condition.NegativeCondition.ToString() && (it.distribution == ETreasureDistributionType.None || it.distribution == reward.TreasureDistributionType))?.icon ?? GetIconNegativeCondition(condition.NegativeCondition);
			}
			return campaingQuestRewards.FirstOrDefault((RewardConfigUI it) => it.type == reward.Type && it.extraId == condition.PositiveCondition.ToString() && (it.distribution == ETreasureDistributionType.None || it.distribution == reward.TreasureDistributionType))?.icon ?? GetIconPositiveCondition(condition.PositiveCondition);
		}
		return campaingQuestRewards.FirstOrDefault((RewardConfigUI it) => it.type == reward.Type && (it.distribution == ETreasureDistributionType.None || it.distribution == reward.TreasureDistributionType))?.icon;
	}

	public Color GetRewardColor(Reward reward)
	{
		bool flag = AdventureState.MapState?.IsCampaign ?? false;
		if (reward.Type == ETreasureType.UnlockCharacter)
		{
			if (flag)
			{
				return Color.white;
			}
			string defaultModel = CharacterClassManager.Classes.Single((CCharacterClass s) => s.ID == reward.CharacterID).DefaultModel;
			string customCharacterConfig = CharacterClassManager.Classes.Single((CCharacterClass s) => s.ID == reward.CharacterID).CharacterYML.CustomCharacterConfig;
			CharacterConfigUI characterConfigUI = GetCharacterConfigUI(defaultModel, useDefault: true, customCharacterConfig);
			if (!(characterConfigUI == null))
			{
				return characterConfigUI.questConfig.color;
			}
			return Color.white;
		}
		_ = reward.Type;
		_ = 16;
		return campaingQuestRewards.FirstOrDefault((RewardConfigUI it) => it.type == reward.Type && (it.distribution == ETreasureDistributionType.None || it.distribution == reward.TreasureDistributionType))?.color ?? Color.white;
	}

	public Sprite GetItemMiniSprite(string itemName)
	{
		return GetItemConfig(itemName).miniIcon;
	}

	public Color GetItemColor(CItem.EItemRarity itemRarity)
	{
		return itemRarityConfigs.First((ItemRarityConfigUI it) => it.rarity == itemRarity).itemColor;
	}

	public void AddFallbackAbilityCardSpriteAsset(TMP_SpriteAsset spriteAsset)
	{
		tmpAbilityCardSpriteAssets.Add(spriteAsset);
		AbilityCardSpriteAsset.fallbackSpriteAssets.Add(spriteAsset);
	}

	private void OnDestroy()
	{
		for (int i = 0; i < tmpAbilityCardSpriteAssets.Count; i++)
		{
			AbilityCardSpriteAsset.fallbackSpriteAssets.Remove(tmpAbilityCardSpriteAssets[i]);
		}
		instance = null;
	}

	public static Color GetPlaceholderPlayerColor(NetworkPlayer player)
	{
		if (player.Avatar != null)
		{
			return Color.white;
		}
		return GetPlaceholderPlayerColor(player.PlayerID.ID);
	}

	public static Color GetPlaceholderPlayerColor(int ID)
	{
		if (instance != null && instance.MultiplayerVisualsUIDataSo != null)
		{
			return instance.MultiplayerVisualsUIDataSo.GetPlayerVisuals(ID).Color;
		}
		if (ID > 0 && ID <= defaultPlayerColors.Length)
		{
			return defaultPlayerColors[ID - 1];
		}
		return Color.white;
	}

	public CharacterConfigUI.ECharacterGender GetCharacterGender(ECharacter character)
	{
		return GetCharacterConfigUI(character).gender;
	}

	public DLCConfig GetDLCConfig(DLCRegistry.EDLCKey dlc)
	{
		return dlcConfigs.FirstOrDefault((DLCConfig it) => it.DLCKey == dlc);
	}

	public Sprite GetMissingCharacterDLC(DLCRegistry.EDLCKey dlc)
	{
		return GetDLCConfig(dlc)?.MissingCharacterIcon;
	}

	public Sprite[] GetDLCPromotionSprites(DLCRegistry.EDLCKey dlc)
	{
		return GetDLCConfig(dlc)?.PromotionalImages;
	}

	public DLCConfig.EDLCState GetDLCState(DLCRegistry.EDLCKey dlc)
	{
		return GetDLCConfig(dlc)?.State ?? DLCConfig.EDLCState.Unavailable;
	}

	public Sprite GetDLCShieldIcon(DLCRegistry.EDLCKey dlc)
	{
		return GetDLCConfig(dlc)?.ShieldIcon;
	}

	public string GetCharacterCreateAnimation(ECharacter character)
	{
		string createAnimation = GetCharacterConfigUI(character).createAnimation;
		if (!createAnimation.IsNullOrEmpty())
		{
			return createAnimation;
		}
		return "Attack";
	}

	public BuildingConfigUI GetBuildingConfig(BuildingTypeDef def)
	{
		return Buildings[def.ID];
	}

	public BuildingConfigUI GetBuildingConfig(BuildingTypeID id)
	{
		return Buildings[id];
	}

	public BuildingConfigUI GetBuildingConfig(BuildingId id)
	{
		return Buildings[id];
	}

	public MonsterConfigUI GetMonsterConfig(string monsterID, string modelName)
	{
		return Monsters.GetConfig(monsterID, modelName);
	}

	public MonsterConfigUI GetMonsterConfig(string monsterID)
	{
		return Monsters.GetConfig(monsterID);
	}

	public MonsterConfigUI GetMonsterConfig(MonsterYMLData monster)
	{
		return Monsters.GetConfig(monster);
	}

	public MonsterConfigUI GetMonsterConfig(CMonsterClass monster)
	{
		return Monsters.GetConfig(monster);
	}

	public SummonConfigUI GetSummonConfig(string summonID)
	{
		return Summons.GetConfig(summonID);
	}

	public SummonConfigUI GetSummonConfig(HeroSummonYMLData summonYmlData)
	{
		return Summons.GetConfig(summonYmlData);
	}

	public SummonConfigUI GetSummonConfig(CHeroSummonClass summonClass)
	{
		return Summons.GetConfig(summonClass);
	}

	public Sprite GetPropDefaultImage(ScenarioManager.ObjectImportType propType)
	{
		return propType switch
		{
			ScenarioManager.ObjectImportType.Door => DoorPropDefaultIcon, 
			ScenarioManager.ObjectImportType.Chest => ChestPropDefaultIcon, 
			ScenarioManager.ObjectImportType.GoalChest => ChestPropDefaultIcon, 
			ScenarioManager.ObjectImportType.MoneyToken => LootPropDefaultIcon, 
			ScenarioManager.ObjectImportType.Trap => TrapDefaultIcon, 
			ScenarioManager.ObjectImportType.Obstacle => ObstaclePropDefaultIcon, 
			ScenarioManager.ObjectImportType.Spawner => SpawnerPropDefaultIcon, 
			ScenarioManager.ObjectImportType.PressurePlate => PressurePlatePropDefaultIcon, 
			ScenarioManager.ObjectImportType.TerrainHotCoals => HazardousTerrainDefaultIcon, 
			ScenarioManager.ObjectImportType.TerrainWater => HazardousTerrainDefaultIcon, 
			ScenarioManager.ObjectImportType.TerrainRubble => DifficultTerrainDefaultIcon, 
			ScenarioManager.ObjectImportType.TerrainThorns => DifficultTerrainDefaultIcon, 
			ScenarioManager.ObjectImportType.Portal => PortalDefaultIcon, 
			ScenarioManager.ObjectImportType.CarryableQuestItem => QuestItemDefaultIcon, 
			ScenarioManager.ObjectImportType.TerrainVisualEffect => null, 
			ScenarioManager.ObjectImportType.Resource => null, 
			ScenarioManager.ObjectImportType.MonsterGrave => null, 
			ScenarioManager.ObjectImportType.SnowRock => null, 
			ScenarioManager.ObjectImportType.TerrainIce => IceTerrainDefaultIcon, 
			ScenarioManager.ObjectImportType.Snowdrift => null, 
			ScenarioManager.ObjectImportType.Shadow => ShadowPropDefaultIcon, 
			ScenarioManager.ObjectImportType.TerrainLampVisualEffect => null, 
			_ => null, 
		};
	}

	public IEnumerable<GameObject> GetAllTooltips()
	{
		yield return ItemTooltip.gameObject;
		yield return CardTooltip.gameObject;
	}

	public Sprite GetPuzzleImage(PuzzleID puzzleID)
	{
		return PuzzleImages.FirstOrDefault((PuzzleBookImage p) => p.PuzzleID == puzzleID.ID).PuzzleImage ?? BlankSpriteIcon;
	}

	public Sprite GetPuzzleCompletedImage(PuzzleID puzzleID)
	{
		bool num = Singleton<FHMapChoreographer>.InstanceFast?.View.GetService<UIElementTagsViewService>().GetPlayerTagsByKey("UI_TAGS_SHOW_SPOILERS").Any() ?? false;
		PuzzleBookImage puzzleBookImage = PuzzleImages.FirstOrDefault((PuzzleBookImage p) => p.PuzzleID == puzzleID.ID);
		return (num ? puzzleBookImage.PuzzleCompletedImage : puzzleBookImage.PuzzleImage) ?? puzzleBookImage.PuzzleImage;
	}

	public Sprite GetNotificationGroupImage(OutpostNotificationType notificationType)
	{
		return NotificationData.GetIcon(notificationType);
	}

	public Sprite GetNotificationGroupSelectedImage(OutpostNotificationType notificationType)
	{
		return NotificationData.GetSelectedIcon(notificationType);
	}

	public Sprite GetNotificationGroupBackground(OutpostNotificationType notificationType)
	{
		if (!OutpostNotificationUtils.IsOutpostNotification(notificationType))
		{
			return NotificationData.PlayerNotificationBackground;
		}
		return NotificationData.OutpostNotificationBackground;
	}

	public static UIGlossaryData? GetGlossaryByKey(string key)
	{
		return GlossaryData.GetData(key);
	}

	public static IEnumerable<UIGlossaryData> GetFullGlossaryDataFromKey(string key)
	{
		return GlossaryData.GetFullDataFromKey(key);
	}
}
