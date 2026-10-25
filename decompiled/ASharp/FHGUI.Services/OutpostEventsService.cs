using System.Collections.Generic;
using System.Linq;
using System.Text;
using FHGUI.DTO;
using FHGUI.States;
using FHRL;
using FHRL.Buildings;
using GLOOM;
using I2.Loc;
using ScenarioRuleLibrary;
using UnityEngine;

namespace FHGUI.Services;

public class OutpostEventsService : FHMapViewService
{
	private const string UI_TAG_EVENT_OPTIONS = "EVENT_OPTIONS";

	private readonly UIElementTagsViewService _tagsViewService;

	private FHRandom _random;

	private List<Sprite> _summerEventImages = new List<Sprite>();

	private List<Sprite> _winterEventImages = new List<Sprite>();

	private Dictionary<string, Sprite> _eventImageCache = new Dictionary<string, Sprite>();

	private const string CAMPAIGN_EFFECT_PREFIX = "CampaignEffect_";

	public OutpostEventsService(FHMapView view, UIElementTagsViewService tagsViewService)
		: base(view)
	{
		_tagsViewService = tagsViewService;
		_random = new FHRandom(view.GameState.Random.Seed);
	}

	public OutpostEventsService(ScenarioRuleLibrary.ScenarioState scenarioState)
		: base(null)
	{
		_tagsViewService = null;
		_random = new FHRandom();
	}

	public OutpostEventDTO GetEventDTO(OutpostEventState _event)
	{
		bool isSoloGame = _view.IsSoloGame;
		CampaignController gameController = GameController;
		OutpostEventDTO outpostEventDTO = new OutpostEventDTO();
		outpostEventDTO.LastInChain = _event.Completed && !GameController.State.HasUnresolvedActions();
		outpostEventDTO.ID = _event.Def.ID.ID;
		outpostEventDTO.EventType = GLOOM.LocalizationManager.GetTranslation(GetEventTypeKey(_event.Def, gameController.State.Outpost.Calendar.Season));
		string validTermName = I2Utils.GetValidTermName(NarrationSetting.FromOutpostEventFront(_event).Text);
		List<string> list = null;
		string text = null;
		if (_event.OutcomeIteration > 0 && _event.CustomOutcomeLocKeys != null && _event.CustomOutcomeLocKeys.Count > 0)
		{
			text = NarrationSetting.FromCustomEventOutcome(_event);
			list = _event.CustomOutcomeLocKeys.Select((OutpostEventState.CustomOutcomeKey x) => x.locKey).ToList();
		}
		else
		{
			int num = 1;
			while (true)
			{
				string text2 = validTermName + "_" + num;
				if (!I2.Loc.LocalizationManager.TryGetTranslation(text2, out var Translation))
				{
					break;
				}
				if (string.IsNullOrEmpty(text))
				{
					list = new List<string>();
					text = string.Empty;
				}
				else
				{
					text += "\n";
				}
				list.Add(text2);
				text += Translation;
				num++;
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			outpostEventDTO.Text = I2.Loc.LocalizationManager.GetTermTranslation("FH/" + validTermName);
			outpostEventDTO.Voiceover = validTermName;
		}
		else
		{
			outpostEventDTO.Text = text;
			outpostEventDTO.Voiceovers = list;
		}
		outpostEventDTO.Icon = GetEventIcon(_event.Def, gameController.State.Outpost.Calendar.Season);
		outpostEventDTO.Illustration = GetEventIllustration(_event.Def, gameController.State.Outpost.Calendar.Season);
		outpostEventDTO.FadeInIllustration = GetEventFadeInIllustration(_event.Def);
		outpostEventDTO.IsProceedInteractable = isSoloGame || !_view.Player.Ready;
		List<OutpostEventDef.Option> list2 = _event.Def.GetOptions(gameController.State, _event).ToList();
		outpostEventDTO.Options.Clear();
		if (!gameController.OutpostEventController.ShouldContinueResolveEvent)
		{
			for (int num2 = 0; num2 < list2.Count(); num2++)
			{
				uint iD = list2[num2].ID;
				string tag = _tagsViewService.FormatTag("EVENT_OPTIONS", iD.ToString());
				OutpostEventDTO.Option option = new OutpostEventDTO.Option();
				option.ID = iD;
				option.Interactable = gameController.OutpostEventController.IsValidOption(list2[num2]);
				NarrationSetting.Setting setting = NarrationSetting.FromOutpostEventOption(_event, list2[num2]);
				if (!string.IsNullOrEmpty(setting.TextLocalized))
				{
					option.Text = setting.TextLocalized;
				}
				else
				{
					string validTermName2 = I2Utils.GetValidTermName(NarrationSetting.FromOutpostEventOption(_event, list2[num2]).Text);
					option.Text = I2.Loc.LocalizationManager.GetTermTranslation("FH/" + validTermName2);
				}
				option.SuggestingUsers = (isSoloGame ? null : _tagsViewService.GetMultiplayerUsersWithTag(tag).ToList());
				outpostEventDTO.Options.Add(option);
			}
		}
		else
		{
			outpostEventDTO.NoButtonsAtAll = true;
		}
		string optionPrompt = _event.Def.GetOptionPrompt(_event);
		if (optionPrompt != null && I2.Loc.LocalizationManager.TryGetTranslation(optionPrompt, out var Translation2))
		{
			outpostEventDTO.OptionPromp = Translation2;
		}
		return outpostEventDTO;
	}

	public OutpostEventDTO GetEventDTO(EventViewState.Data _data)
	{
		CampaignController gameController = GameController;
		OutpostEventDTO outpostEventDTO = new OutpostEventDTO();
		outpostEventDTO.LastInChain = !GameController.State.HasUnresolvedActions();
		outpostEventDTO.ID = _data.Event.ID.ID;
		outpostEventDTO.EventType = _data.Event.Def.Type.ToString();
		outpostEventDTO.EventType = GLOOM.LocalizationManager.GetTranslation(GetEventTypeKey(_data.Event.Def, gameController.State.Outpost.Calendar.Season));
		outpostEventDTO.Text = _data.Text;
		outpostEventDTO.Icon = GetEventIcon(_data.Event.Def, gameController.State.Outpost.Calendar.Season);
		outpostEventDTO.Illustration = GetEventIllustration(_data.Event.Def, gameController.State.Outpost.Calendar.Season);
		outpostEventDTO.FadeInIllustration = GetEventFadeInIllustration(_data.Event.Def);
		outpostEventDTO.Voiceovers = ((_data.VoiceOvers != null && _data.VoiceOvers.Count > 0) ? _data.VoiceOvers : new List<string> { _data.TextKey });
		outpostEventDTO.IsProceedInteractable = _view.IsSoloGame || !_view.Player.Ready;
		outpostEventDTO.ReadyUsers = _view.TryGetReadyUsersList();
		outpostEventDTO.Effects.Clear();
		IEnumerable<CampaignEffect> effects = _data.Effects.Where((CampaignEffect e) => e.Type != CampaignEffectType.TriggerAttackOutpost);
		foreach (CampaignEffect item in CombineCampaignEffects(effects))
		{
			OutpostEventDTO.Effect formattedEffect = GetFormattedEffect(item);
			outpostEventDTO.Effects.Add(formattedEffect);
		}
		return outpostEventDTO;
	}

	public List<CampaignEffect> CombineCampaignEffects(IEnumerable<CampaignEffect> effects)
	{
		List<CampaignEffect> list = new List<CampaignEffect>();
		foreach (CampaignEffect e in effects)
		{
			bool flag = true;
			switch (e.Type)
			{
			case CampaignEffectType.AddItemsToShop:
			case CampaignEffectType.EnableEvent:
			case CampaignEffectType.AddCalendarCampaignSection:
			case CampaignEffectType.EnableEventsFromTo:
			{
				string locKey = GetEffectTypeKey(e);
				if (list.Any((CampaignEffect x) => GetEffectTypeKey(x) == locKey))
				{
					flag = false;
				}
				break;
			}
			case CampaignEffectType.AddProsperity:
			{
				CampaignEffect item = list.SingleOrDefault((CampaignEffect x) => x.Type == e.Type);
				if (item.Type != CampaignEffectType.None)
				{
					list.Remove(item);
					CampaignEffect item2 = new CampaignEffect(item.Type, item.GetInt(0) + e.GetInt(0));
					list.Add(item2);
					flag = false;
				}
				break;
			}
			}
			if (flag)
			{
				list.Add(e);
			}
		}
		return list;
	}

	private string GetEventTypeKey(OutpostEventDef def, CalendarSeason season)
	{
		switch (def.Type)
		{
		case OutpostEventType.Road:
			switch (season)
			{
			case CalendarSeason.Summer:
				return "GUI_EVENT_ROAD_SUMMER";
			case CalendarSeason.Winter:
				return "GUI_EVENT_ROAD_WINTER";
			}
			break;
		case OutpostEventType.Boat:
			return "GUI_EVENT_BOAT";
		case OutpostEventType.Outpost:
			return "GUI_EVENT_OUTPOST";
		}
		return string.Empty;
	}

	private Sprite GetEventIcon(OutpostEventDef def, CalendarSeason season)
	{
		switch (def.Type)
		{
		case OutpostEventType.Road:
			switch (season)
			{
			case CalendarSeason.Summer:
				return UIInfoTools.Instance.GetOutpostEventTypeImage("Road_Summer", throwError: false);
			case CalendarSeason.Winter:
				return UIInfoTools.Instance.GetOutpostEventTypeImage("Road_Winter", throwError: false);
			}
			break;
		case OutpostEventType.Boat:
			return UIInfoTools.Instance.GetOutpostEventTypeImage("Boat", throwError: false);
		case OutpostEventType.Outpost:
			if (def.MonsterType == OutpostEventMonsterType.Algox)
			{
				return UIInfoTools.Instance.GetOutpostEventTypeImage("Outpost_Algox", throwError: false);
			}
			if (def.MonsterType == OutpostEventMonsterType.Lurker)
			{
				return UIInfoTools.Instance.GetOutpostEventTypeImage("Outpost_Lurker", throwError: false);
			}
			if (def.MonsterType == OutpostEventMonsterType.Unfettered)
			{
				return UIInfoTools.Instance.GetOutpostEventTypeImage("Outpost_Unfettered", throwError: false);
			}
			if (def.MonsterType == OutpostEventMonsterType.None)
			{
				return UIInfoTools.Instance.GetOutpostEventTypeImage("Outpost_Generic", throwError: false);
			}
			break;
		}
		return null;
	}

	private Sprite GetEventFadeInIllustration(OutpostEventDef def)
	{
		if (string.IsNullOrEmpty(def.Illustration))
		{
			return null;
		}
		return UIInfoTools.Instance.GetOutpostEventImageConfig(def.Illustration)?.Image;
	}

	private Sprite GetEventIllustration(OutpostEventDef def, CalendarSeason season)
	{
		if (!string.IsNullOrEmpty(def.BackgroundIllustration))
		{
			return UIInfoTools.Instance.GetOutpostEventImageConfig(def.BackgroundIllustration).Image;
		}
		return GetEventIllustration(def.ID.ID, def.Type, season);
	}

	private Sprite GetEventIllustration(string id, OutpostEventType type, CalendarSeason season)
	{
		switch (type)
		{
		case OutpostEventType.Boat:
			return UIInfoTools.Instance.GetOutpostEventImageConfig("BoatEvent_01").Image;
		case OutpostEventType.Road:
		{
			_eventImageCache.TryGetValue(id, out var value);
			if ((bool)value)
			{
				return value;
			}
			switch (season)
			{
			case CalendarSeason.Summer:
				if (!_summerEventImages.Any())
				{
					_summerEventImages.AddRange(UIInfoTools.Instance.GetOutpostEventImages("RoadEvent_Summer"));
					_random.Shuffle(_summerEventImages);
				}
				value = _summerEventImages.LastOrDefault();
				_summerEventImages.Remove(value);
				_eventImageCache.Add(id, value);
				return value;
			case CalendarSeason.Winter:
				if (!_winterEventImages.Any())
				{
					_winterEventImages.AddRange(UIInfoTools.Instance.GetOutpostEventImages("RoadEvent_Winter"));
					_random.Shuffle(_winterEventImages);
				}
				value = _winterEventImages.LastOrDefault();
				_winterEventImages.Remove(value);
				_eventImageCache.Add(id, value);
				return value;
			}
			break;
		}
		}
		if (type == OutpostEventType.Outpost)
		{
			return UIInfoTools.Instance.GetOutpostEventImageConfig("OutpostEvent_01").Image;
		}
		return null;
	}

	public OutpostEventDTO.Effect GetFormattedEffect(CampaignEffect effect)
	{
		return new OutpostEventDTO.Effect
		{
			Text = EffectToStringLiteral(effect),
			Hidden = IsHiddenEffect(effect)
		};
	}

	private string GetItemNameLocalized(uint itemId)
	{
		ItemDef value = _view.GameController.DefRepository.Items.FirstOrDefault((KeyValuePair<ItemID, ItemDef> pair) => pair.Key.ID == itemId).Value;
		if (value == null)
		{
			return FHUILocFormatting.Color($"Unknown ItemId: {itemId}", FHUIColors.MISSING_RESOURCE);
		}
		if (value.NameLocKey == null)
		{
			return FHUILocFormatting.Color($"Missing Name [ItemId: {itemId}]", FHUIColors.MISSING_RESOURCE);
		}
		return FHUILocFormatting.Style(GLOOM.LocalizationManager.GetTranslation(value.NameLocKey), "H3");
	}

	private string GetPetNameLocalized(uint petID)
	{
		PetDef value = _view.GameController.DefRepository.Pets.FirstOrDefault((KeyValuePair<PetID, PetDef> pair) => pair.Key.ID == petID).Value;
		if (value == null)
		{
			return FHUILocFormatting.Color($"Unknown petID: {petID}", FHUIColors.MISSING_RESOURCE);
		}
		if (value.Name == null)
		{
			return FHUILocFormatting.Color($"Missing Name [petID: {petID}]", FHUIColors.MISSING_RESOURCE);
		}
		return FHUILocFormatting.Style(GLOOM.LocalizationManager.GetTranslation(value.Name), "H3");
	}

	private string GetElementSprite(string elementKey)
	{
		return "<sprite name=Create" + elementKey + ">";
	}

	public string GetBuildingNameLocalized(uint buildingId, bool noStyle = false)
	{
		BuildingTypeDef value = _view.GameController.DefRepository.Buildings.FirstOrDefault((KeyValuePair<BuildingTypeID, BuildingTypeDef> pair) => pair.Key.ID == buildingId).Value;
		if (value == null)
		{
			return FHUILocFormatting.Color($"Unknown Building: {buildingId}", FHUIColors.MISSING_RESOURCE);
		}
		BuildingConfigUI buildingConfig = UIInfoTools.Instance.GetBuildingConfig(value);
		if (buildingConfig == null)
		{
			return FHUILocFormatting.Color($"Missing Building Config: {buildingId}]", FHUIColors.MISSING_RESOURCE);
		}
		string translation = GLOOM.LocalizationManager.GetTranslation(buildingConfig.GetName());
		if (noStyle)
		{
			return translation;
		}
		return FHUILocFormatting.Style(translation, "H3");
	}

	private string GetTownGuardModLocalized(string modId)
	{
		TownGuardAttackModifierDef value = DefRepo.TownGuardAttackModifierDefs.FirstOrDefault((KeyValuePair<TownGuardAttackModifierID, TownGuardAttackModifierDef> pair) => pair.Key.ID == modId).Value;
		if (value == null)
		{
			return FHUILocFormatting.Color("Unknown Modifer: " + modId, FHUIColors.MISSING_RESOURCE);
		}
		string text = value.MathModifier;
		if (value.Type == TownGuardModType.Failure)
		{
			text = "Failure";
		}
		if (value.Type == TownGuardModType.Success)
		{
			text = "Success";
		}
		return "<size=1.2em><sprite name=\"" + text + "\"></size>";
	}

	public string GetHeroNameLocalized(string classId)
	{
		CharacterState characterState = _view.GameController.State.PartyCharacters.SingleOrDefault((CharacterState s) => s.ClassDef.ID.ID == classId);
		if (characterState == null)
		{
			return FHUILocFormatting.Color("Unknown Character Class: " + classId, FHUIColors.MISSING_RESOURCE);
		}
		return characterState.Name;
	}

	public string GetHeroNameLocalized(CharacterID characterID)
	{
		CharacterState characterState = _view.GameController.State.AllCharacters.SingleOrDefault((CharacterState s) => s.ID == characterID);
		if (characterState == null)
		{
			return FHUILocFormatting.Color($"Unknown Character ID: {characterID}", FHUIColors.MISSING_RESOURCE);
		}
		return characterState.Name;
	}

	private string GetHeroClassLocalized(string classId)
	{
		CharacterClassDef value = _view.GameController.DefRepository.CharacterClasses.FirstOrDefault((KeyValuePair<CharacterClassID, CharacterClassDef> pair) => pair.Key.ID == classId).Value;
		if (value == null)
		{
			return FHUILocFormatting.Color("Unknown Character Class " + classId, FHUIColors.MISSING_RESOURCE);
		}
		return FHUILocFormatting.Style(GLOOM.LocalizationManager.GetTranslation(value.ClassName), "H3");
	}

	private string GetAbilityCardNameLocalized(uint cardId)
	{
		ClassCardDef value = _view.GameController.DefRepository.ClassCards.FirstOrDefault((KeyValuePair<ClassCardID, ClassCardDef> pair) => pair.Key.ID == cardId).Value;
		if (value == null)
		{
			return FHUILocFormatting.Color($"Unknown Ability Card id: {cardId}", FHUIColors.MISSING_RESOURCE);
		}
		if (value.NameLocKey == null)
		{
			return FHUILocFormatting.Color($"Missing Name [ItemId: {cardId}]", FHUIColors.MISSING_RESOURCE);
		}
		return GLOOM.LocalizationManager.GetTranslation(value.NameLocKey);
	}

	public string GetScenarioNameLocalized(uint scenarioId, bool noStyle = false)
	{
		ScenarioDef value = _view.GameController.DefRepository.Scenarios.FirstOrDefault((KeyValuePair<ScenarioID, ScenarioDef> pair) => pair.Key.ID == scenarioId).Value;
		if (value == null)
		{
			return FHUILocFormatting.Color($"Unknown Scenario id: {scenarioId}", FHUIColors.MISSING_RESOURCE);
		}
		string translation = GLOOM.LocalizationManager.GetTranslation($"FS_{value.ID}_NAME");
		if (noStyle)
		{
			return translation;
		}
		return FHUILocFormatting.Style(translation, "H3");
	}

	public string GetCampaignStickerNameLocalized(string stickerId, bool noStyle = false)
	{
		string translation = GLOOM.LocalizationManager.GetTranslation("CampaignSticker_" + stickerId);
		if (noStyle)
		{
			return translation;
		}
		return FHUILocFormatting.Style(translation, "H3");
	}

	private string[] GetEffectLocalizedParams(CampaignEffect effect)
	{
		return effect.Type switch
		{
			CampaignEffectType.GainMoraleInsteadOfDamagingBuildings => new string[1] { effect.Param1 }, 
			CampaignEffectType.GainMoraleForSuccessfulBuildingDefense => new string[1] { effect.Param1 }, 
			CampaignEffectType.CharacterRecoverAllDiscardedCardsEndOfSecondRound => new string[1] { GetHeroNameLocalized(effect.GetString(0)) }, 
			CampaignEffectType.LoseAxenut => new string[1] { effect.Param1 }, 
			CampaignEffectType.LoseArrowvine => new string[1] { effect.Param1 }, 
			CampaignEffectType.Event_142_1_TwoCharacters => new string[3]
			{
				GetHeroNameLocalized(effect.GetString(0)),
				effect.GetString(1),
				GetHeroNameLocalized(effect.GetString(2))
			}, 
			CampaignEffectType.Event_142_1_SingleCharacter => new string[1] { GetHeroNameLocalized(effect.GetString(0)) }, 
			CampaignEffectType.AddSoldiers => new string[1] { effect.Param1 }, 
			CampaignEffectType.LoseSoldiers => new string[1] { effect.Param1 }, 
			CampaignEffectType.AddThreat => new string[1] { effect.Param1 }, 
			CampaignEffectType.ReduceThreat => new string[1] { effect.Param1 }, 
			CampaignEffectType.AddGoldOnScenarioSuccess => new string[1] { effect.Param1 }, 
			CampaignEffectType.GainInspiration => new string[1] { effect.Param1 }, 
			CampaignEffectType.LoseInspiration => new string[1] { effect.Param1 }, 
			CampaignEffectType.AddLootCardBonus => new string[1] { effect.Param1 }, 
			CampaignEffectType.GainMorale => new string[1] { Mathf.Abs(effect.GetInt(0)).ToString() }, 
			CampaignEffectType.LoseMorale => new string[1] { Mathf.Abs(effect.GetInt(0)).ToString() }, 
			CampaignEffectType.AddMoraleOnScenarioSucces => new string[1] { effect.Param1 }, 
			CampaignEffectType.AddOutpostDefenseNextBattle => new string[1] { effect.Param1 }, 
			CampaignEffectType.AddProsperity => new string[1] { effect.Param1 }, 
			CampaignEffectType.LoseProsperity => new string[1] { effect.Param1 }, 
			CampaignEffectType.AddTargets => new string[1] { effect.Param1 }, 
			CampaignEffectType.ReduceTargets => new string[1] { effect.Param1 }, 
			CampaignEffectType.AdvantageOnAllTargets => new string[1] { effect.Param1 }, 
			CampaignEffectType.DisadvantageOnAllTargets => null, 
			CampaignEffectType.DisadvantageOnNextTargets => null, 
			CampaignEffectType.AllCharactersAddPerkCheck => new string[1] { effect.Param1 }, 
			CampaignEffectType.AllCharactersGainExperience => new string[1] { effect.Param1 }, 
			CampaignEffectType.AllCharactersLoseExperience => new string[1] { effect.Param1 }, 
			CampaignEffectType.AllCharactersGainGold => new string[1] { effect.Param1 }, 
			CampaignEffectType.AllCharactersGainHide => new string[1] { effect.Param1 }, 
			CampaignEffectType.AllCharactersGainMetal => new string[1] { effect.Param1 }, 
			CampaignEffectType.AllCharactersGainLumber => new string[1] { effect.Param1 }, 
			CampaignEffectType.CannotUseSoldiers => null, 
			CampaignEffectType.CharacterBless => new string[2]
			{
				GetHeroNameLocalized(effect.GetString(0)),
				effect.Param2
			}, 
			CampaignEffectType.CharacterCurse => new string[1] { GetHeroNameLocalized(effect.GetString(0)) }, 
			CampaignEffectType.CharacterMuddle => new string[1] { GetHeroNameLocalized(effect.GetString(0)) }, 
			CampaignEffectType.CharacterDisarm => new string[1] { GetHeroNameLocalized(effect.GetString(0)) }, 
			CampaignEffectType.CharacterWound => new string[1] { GetHeroNameLocalized(effect.GetString(0)) }, 
			CampaignEffectType.CharacterPoison => new string[1] { GetHeroNameLocalized(effect.GetString(0)) }, 
			CampaignEffectType.CharacterInvisible => new string[1] { GetHeroNameLocalized(effect.GetString(0)) }, 
			CampaignEffectType.CharacterImpair => new string[1] { GetHeroNameLocalized(effect.GetString(0)) }, 
			CampaignEffectType.CharacterImmobilize => new string[1] { GetHeroNameLocalized(effect.GetString(0)) }, 
			CampaignEffectType.CharacterDamage => new string[2]
			{
				GetHeroNameLocalized(effect.GetString(0)),
				effect.Param2
			}, 
			CampaignEffectType.CharacterDisableCard => new string[2]
			{
				GetHeroNameLocalized(effect.GetString(0)),
				GetAbilityCardNameLocalized(effect.GetUInt(1))
			}, 
			CampaignEffectType.CharacterDiscardCards => new string[2]
			{
				GetHeroNameLocalized(effect.GetString(0)),
				effect.Param2
			}, 
			CampaignEffectType.CharacterGainExperience => new string[2]
			{
				GetHeroNameLocalized(effect.GetString(0)),
				effect.Param2
			}, 
			CampaignEffectType.CharacterGainGold => new string[2]
			{
				GetHeroNameLocalized(effect.GetString(0)),
				effect.Param2
			}, 
			CampaignEffectType.CharacterGainItem => new string[2]
			{
				GetHeroNameLocalized(effect.GetString(0)),
				GetItemNameLocalized(effect.GetUInt(1))
			}, 
			CampaignEffectType.CharacterGainMetal => new string[2]
			{
				GetHeroNameLocalized(effect.GetString(0)),
				effect.Param2
			}, 
			CampaignEffectType.AddGoldOnScenarioIfKilledAlgox => new string[1] { effect.Param1 }, 
			CampaignEffectType.GainArrowvine => new string[1] { effect.Param1 }, 
			CampaignEffectType.GainCorpsecap => new string[1] { effect.Param1 }, 
			CampaignEffectType.GainSnowthistle => new string[1] { effect.Param1 }, 
			CampaignEffectType.GainRockroot => new string[1] { effect.Param1 }, 
			CampaignEffectType.GainGold => new string[1] { effect.Param1 }, 
			CampaignEffectType.GainHide => new string[1] { effect.Param1 }, 
			CampaignEffectType.GainLumber => new string[1] { effect.Param1 }, 
			CampaignEffectType.GainMaterials => new string[1] { effect.Param1 }, 
			CampaignEffectType.GainMetal => new string[1] { effect.Param1 }, 
			CampaignEffectType.GainHerbs => new string[1] { effect.Param1 }, 
			CampaignEffectType.GainItemBlueprint => new string[1] { GetItemNameLocalized(effect.GetUInt(0)) }, 
			CampaignEffectType.GainItem => new string[1] { GetItemNameLocalized(effect.GetUInt(0)) }, 
			CampaignEffectType.AddPetIgnoringCapacity => new string[1] { GetPetNameLocalized(effect.GetUInt(0)) }, 
			CampaignEffectType.PartyElementWaningToInertDisabled => new string[1] { GetElementSprite(effect.GetString(0)) }, 
			CampaignEffectType.LoseGold => new string[1] { effect.Param1 }, 
			CampaignEffectType.LoseItem => new string[1] { effect.Param1 }, 
			CampaignEffectType.LoseLumber => new string[1] { effect.Param1 }, 
			CampaignEffectType.LoseMaterials => new string[1] { effect.Param1 }, 
			CampaignEffectType.LoseMetal => new string[1] { effect.Param1 }, 
			CampaignEffectType.GainAxenut => new string[1] { effect.Param1 }, 
			CampaignEffectType.GainFlamefruit => new string[1] { effect.Param1 }, 
			CampaignEffectType.LoseAllSnowthistle => new string[1] { effect.Param1 }, 
			CampaignEffectType.PartyBless => new string[1] { effect.Param1 }, 
			CampaignEffectType.PartyBrittle => null, 
			CampaignEffectType.PartyCurse => new string[1] { effect.Param1 }, 
			CampaignEffectType.PartyPoison => null, 
			CampaignEffectType.PartyImpair => null, 
			CampaignEffectType.PartyDamage => new string[1] { effect.Param1 }, 
			CampaignEffectType.PartyMuddle => null, 
			CampaignEffectType.PartyWard => null, 
			CampaignEffectType.PartyWound => null, 
			CampaignEffectType.PartyStrengthen => null, 
			CampaignEffectType.PartyRegenerate => null, 
			CampaignEffectType.PartyDiscardCards => new string[1] { effect.Param1 }, 
			CampaignEffectType.PartyDiscardFireLightOrDamage => null, 
			CampaignEffectType.PartyDiscardSmallItem => null, 
			CampaignEffectType.PartyInfuseIce => null, 
			CampaignEffectType.PartyLootCard => null, 
			CampaignEffectType.PartyLootCardAdvantage => null, 
			CampaignEffectType.PartyNextAttackPoison => null, 
			CampaignEffectType.ProtectBuildings => new string[1] { effect.Param1 }, 
			CampaignEffectType.ReduceBattleGoal => null, 
			CampaignEffectType.UnlockScenario => new string[1] { GetScenarioNameLocalized(effect.GetUInt(0)) }, 
			CampaignEffectType.UnlockLinkedScenario => new string[1] { GetScenarioNameLocalized(effect.GetUInt(0)) }, 
			CampaignEffectType.UnlockMandatoryScenario => new string[1] { GetScenarioNameLocalized(effect.GetUInt(0)) }, 
			CampaignEffectType.RestrictNextScenario => new string[1] { GetScenarioNameLocalized(effect.GetUInt(0)) }, 
			CampaignEffectType.UpgradeBuilding => new string[1] { GetBuildingNameLocalized(effect.GetUInt(0)) }, 
			CampaignEffectType.WreckBuilding => new string[1] { GetBuildingNameLocalized(effect.GetUInt(0)) }, 
			CampaignEffectType.UnlockBuilding => new string[1] { GetBuildingNameLocalized(effect.GetUInt(0)) }, 
			CampaignEffectType.WreckNextDamagedBuildings => null, 
			CampaignEffectType.WR24OptionAEffect => null, 
			CampaignEffectType.Event_SO18_OptionB_SetupScenarioSuccess => null, 
			CampaignEffectType.UnlockClass => new string[1] { GetHeroClassLocalized(effect.GetString(0)) }, 
			CampaignEffectType.GainCampaignSticker => new string[1] { GetCampaignStickerNameLocalized(effect.GetString(0)) }, 
			CampaignEffectType.CharacterLootExtraIfNobodyElseLooted => new string[1] { GetHeroNameLocalized(effect.GetString(0)) }, 
			CampaignEffectType.PartyLootExtraIfEveryoneLootedSameAmount => null, 
			CampaignEffectType.ExtraGoldFromSellingHideThisWeek => new string[1] { effect.Param1 }, 
			CampaignEffectType.ConstructOrUpgradeBuilding => new string[1] { GetBuildingNameLocalized(effect.GetUInt(0)) }, 
			CampaignEffectType.LoseTownGuardMod => new string[1] { GetTownGuardModLocalized(effect.GetString(0)) }, 
			CampaignEffectType.StopAttackOnSuccessfulDefense => new string[1] { effect.Param1 }, 
			CampaignEffectType.LoseMoraleInsteadOfDamagingBuildings => new string[1] { effect.Param1 }, 
			CampaignEffectType.RebuildBuilding => new string[1] { GetBuildingNameLocalized(effect.GetUInt(0)) }, 
			CampaignEffectType.AddBuildingWeeklyProduction => new string[2]
			{
				GetBuildingNameLocalized(effect.GetUInt(0)),
				effect.Param2
			}, 
			CampaignEffectType.LoseFlamefruit => new string[1] { effect.Param1 }, 
			CampaignEffectType.UnlockClassChoise => new string[2]
			{
				GetHeroClassLocalized(effect.GetString(0)),
				GetHeroClassLocalized(effect.GetString(1))
			}, 
			CampaignEffectType.CharacterWard => new string[1] { GetHeroNameLocalized(effect.GetString(0)) }, 
			CampaignEffectType.LoseSoldiersInsteadOfDamagingBuildings => new string[1] { effect.Param1 }, 
			CampaignEffectType.ShowMonsterCardsAtScenarioStart => null, 
			_ => null, 
		};
	}

	private string GetEffectTypeKey(CampaignEffect effect)
	{
		if (effect.Type == CampaignEffectType.EnableEvent)
		{
			string text = effect.GetString(0);
			if (text.IsNullOrEmpty())
			{
				return "CampaignEffect_" + effect.Type;
			}
			if (text.Contains("WO-") || text.Contains("SO-"))
			{
				return "CampaignEffect_EnableOutpostEvent";
			}
			if (text.Contains("WR-") || text.Contains("SR-"))
			{
				return "CampaignEffect_EnableRoadEvent";
			}
			if (text.Contains("B-"))
			{
				return "CampaignEffect_EnableBoatEvent";
			}
			return "CampaignEffect_" + effect.Type;
		}
		return "CampaignEffect_" + effect.Type;
	}

	private string EffectToStringLiteral(CampaignEffect effect)
	{
		string effectTypeKey = GetEffectTypeKey(effect);
		object[] effectLocalizedParams = GetEffectLocalizedParams(effect);
		if (FHUILocFormatting.TryPopulateParams(out var result, effectTypeKey, effectLocalizedParams))
		{
			return result;
		}
		string text = effect.Type.ToString();
		foreach (string stringParam in effect.GetStringParams())
		{
			text = text + ", " + stringParam;
		}
		return FHUILocFormatting.Color(text, FHUIColors.MISSING_RESOURCE);
	}

	public static bool IsHiddenEffect(CampaignEffect effect)
	{
		CampaignEffectType type = effect.Type;
		if (type <= CampaignEffectType.OpenEnvelope)
		{
			switch (type)
			{
			case CampaignEffectType.AddGardenPlots:
			case CampaignEffectType.PlantThenCollectHerbs:
			case CampaignEffectType.CollectAndPlantHerbs:
			case CampaignEffectType.CanCraftItems:
			case CampaignEffectType.CanEnhance:
			case CampaignEffectType.CanBrew:
			case CampaignEffectType.CanBrewAndDistill:
			case CampaignEffectType.AddMoraleOnScenarioSucces:
			case CampaignEffectType.EnhancerDisarm:
			case CampaignEffectType.ReduceEnhacementCost:
			case CampaignEffectType.CanUseFavors:
			case CampaignEffectType.CannotUsePotions:
			case CampaignEffectType.CannotUsePets:
			case CampaignEffectType.CanUsePets:
			case CampaignEffectType.PetCapacity:
			case CampaignEffectType.CanUseChallenges:
			case CampaignEffectType.CannotUseChallenges:
			case CampaignEffectType.IfScenarioActivateCampaignSection:
			case CampaignEffectType.ActivateCampaignSection:
			case CampaignEffectType.ActivateEvent:
			case CampaignEffectType.RemoveEvent:
			case CampaignEffectType.CannotUseSoldiers:
			case CampaignEffectType.UnlockBoatScenarios:
			case CampaignEffectType.UnlockClimbingGearScenarios:
			case CampaignEffectType.UnlockSledScenarios:
			case CampaignEffectType.GainCampaignSticker:
			case CampaignEffectType.GainRandomItemBlueprint:
			case CampaignEffectType.RemoveUnfetteredEvents:
			case CampaignEffectType.RemoveLurkerEvents:
			case CampaignEffectType.RemoveAlgoxEvents:
			case CampaignEffectType.ChooseUnlockMandatoryScenario:
			case CampaignEffectType.LockScenario:
			case CampaignEffectType.FS1ConclusionEffect:
			case CampaignEffectType.FS22ConclusionEffect:
			case CampaignEffectType.FS33ConclusionEffect:
			case CampaignEffectType.FS55ConclusionEffect:
			case CampaignEffectType.FS39ConclusionEffect:
			case CampaignEffectType.FS85ConclusionEffect:
			case CampaignEffectType.FS87ConclusionEffect:
			case CampaignEffectType.FS90ConclusionEffect:
			case CampaignEffectType.FS92ConclusionEffect:
			case CampaignEffectType.FS107ConclusionEffect:
			case CampaignEffectType.FS115ConclusionEffect:
			case CampaignEffectType.FS116ConclusionEffect:
			case CampaignEffectType.FS118ConclusionEffect:
			case CampaignEffectType.FS119ConclusionEffect:
			case CampaignEffectType.FS127ConclusionEffect:
			case CampaignEffectType.FS128ConclusionEffect:
			case CampaignEffectType.FS130ConclusionEffect:
			case CampaignEffectType.WreckAnyOneBuilding:
			case CampaignEffectType.WreckAnyOneEvenBuildingBetween:
			case CampaignEffectType.WreckAnyBuildingBetween:
			case CampaignEffectType.WreckAnyOddBuildingBetween:
			case CampaignEffectType.TriggerAttackOutpost:
			case CampaignEffectType.AllAestherCharactersGainExperience:
			case CampaignEffectType.UnlockScenarioIfScenarioComplete:
			case CampaignEffectType.Event_SO18_OptionB_AfterScenarioSuccess:
			case CampaignEffectType.ProtectBuildings:
			case CampaignEffectType.IncreaseMaxSmallItem:
			case CampaignEffectType.AddCalendarCampaignSectionNextSummer:
			case CampaignEffectType.IfRaceActivateCampaignSections:
			case CampaignEffectType.IfTraitActivateCampaignSections:
			case CampaignEffectType.UnlockPuzzle:
			case CampaignEffectType.ChooseNextCampaignSection:
			case CampaignEffectType.CanBuyStartingItems:
			case CampaignEffectType.RepeatIfDoesntHaveRequireBuilding:
			case CampaignEffectType.AddAdditionalLootCard:
			case CampaignEffectType.RemoveAdditionalLootCard:
			case CampaignEffectType.ReadSectionIfPartyHasTrait:
			case CampaignEffectType.SkipNextOutpostEvent:
			case CampaignEffectType.ReadSectionIfHasRequiredBuilding:
			case CampaignEffectType.ResetChallengeDeck:
			case CampaignEffectType.ChangeActiveScenario:
			case CampaignEffectType.ResolveAnotherSummerRoadEvent:
			case CampaignEffectType.ResolveAnotherWinterRoadEvent:
			case CampaignEffectType.ResolveAnotherBoatEvent:
			case CampaignEffectType.ShowSectionTextOnNextScenarioStart:
			case CampaignEffectType.ShowSectionText:
			case CampaignEffectType.LosePet:
			case CampaignEffectType.GainRandomPet:
			case CampaignEffectType.Event_181_1_SelectCharacterOrPartyToGetRecoverEffect:
			case CampaignEffectType.UnlockRandomSideScenario:
			case CampaignEffectType.GainRandomItem:
			case CampaignEffectType.OpenEnvelope:
				break;
			case CampaignEffectType.UnlockScenario:
			case CampaignEffectType.UnlockLinkedScenario:
			case CampaignEffectType.UnlockMandatoryScenario:
				goto IL_034a;
			case CampaignEffectType.UnlockClass:
				goto IL_039a;
			case CampaignEffectType.GainItemBlueprint:
				goto IL_03ec;
			case CampaignEffectType.CharacterGainItem:
				goto IL_0442;
			case CampaignEffectType.EnableEvent:
				goto IL_0498;
			default:
				goto IL_04d8;
			}
		}
		else
		{
			switch (type)
			{
			case CampaignEffectType.IfHerbsOverCActivateSection:
			case CampaignEffectType.ReadSectionIfPartyHasRace:
			case CampaignEffectType.CantBuyTradingPostItems:
			case CampaignEffectType.CantBuyJewellerItems:
			case CampaignEffectType.ReplaceMoraleSection:
			case CampaignEffectType.RebuildAllBuildings:
			case CampaignEffectType.SetGameMode:
			case CampaignEffectType.LoseCampaignSticker:
			case CampaignEffectType.ActivateSectionOnNextScenarioStart:
			case CampaignEffectType.IfScenarioCompletedAddMoraleElseAddEvent:
			case CampaignEffectType.RFS005ConclusionEffect:
			case CampaignEffectType.EnableBattleGoals:
			case CampaignEffectType.EnableLevelUp:
			case CampaignEffectType.EnableRetire:
			case CampaignEffectType.LockBuilding:
			case CampaignEffectType.GainInspirationPerPartyMembers:
			case CampaignEffectType.ShowCredits:
				break;
			default:
				goto IL_04d8;
			}
		}
		return true;
		IL_0442:
		FHMapView fHMapView = Singleton<FHMapChoreographer>.Instance?.View;
		if (fHMapView == null)
		{
			return false;
		}
		ItemDef def = fHMapView.GameController.DefRepository.Items[new ItemID(effect.GetUInt(1))];
		return !fHMapView.GameController.IsItemValid(def);
		IL_04d8:
		return false;
		IL_0498:
		FHMapView fHMapView2 = Singleton<FHMapChoreographer>.Instance?.View;
		if (fHMapView2 == null)
		{
			return false;
		}
		OutpostEventID id = new OutpostEventID(effect.GetString(0));
		return !fHMapView2.GameController.IsEventValid(id);
		IL_039a:
		FHMapView fHMapView3 = Singleton<FHMapChoreographer>.Instance?.View;
		if (fHMapView3 == null)
		{
			return false;
		}
		CharacterClassDef def2 = fHMapView3.GameController.DefRepository.CharacterClasses[new CharacterClassID(effect.GetString(0))];
		return !fHMapView3.GameController.IsClassValid(def2);
		IL_034a:
		FHMapView fHMapView4 = Singleton<FHMapChoreographer>.Instance?.View;
		if (fHMapView4 == null)
		{
			return false;
		}
		ScenarioDef def3 = fHMapView4.GameController.DefRepository.Scenarios[new ScenarioID(effect.GetUInt(0))];
		return !fHMapView4.GameController.IsScenarioUnlockedAndValid(def3);
		IL_03ec:
		FHMapView fHMapView5 = Singleton<FHMapChoreographer>.Instance?.View;
		if (fHMapView5 == null)
		{
			return false;
		}
		ItemDef def4 = fHMapView5.GameController.DefRepository.Items[new ItemID(effect.GetUInt(0))];
		return !fHMapView5.GameController.IsItemValid(def4);
	}

	public bool IsOptionSuggested(int optionId)
	{
		string tag = _tagsViewService.FormatTag("EVENT_OPTIONS", optionId.ToString());
		return _view.MultiplayerGameController.PlayerHasUITag(_view.PlayerID, tag);
	}

	public void SuggestOption(int optionId)
	{
		_tagsViewService.AddPlayerTag("EVENT_OPTIONS", optionId.ToString(), 1);
	}

	public void DeSuggestOption(int optionId)
	{
		_tagsViewService.RemovePlayerTag("EVENT_OPTIONS", optionId.ToString());
	}

	public void ClearUITags()
	{
		_tagsViewService.ClearPlayerTagGroup("EVENT_OPTIONS");
	}

	public uint GetCurrentUsedSoldiersCount()
	{
		return _view.GameState.Outpost.ActiveEvent.Attack.CurrentSoldiers;
	}

	public bool CanRepairDamagedBuilding()
	{
		if (_view.GameController.AttackController.CurrentTarget != null)
		{
			return _view.GameController.BuildingsController.CanRepair(_view.GameController.AttackController.CurrentTarget, _view.IsPartyLeader);
		}
		return false;
	}

	public List<AttackModifierDTO> GetTownGuardDeck()
	{
		PerksViewService perksService = _view.GetService<PerksViewService>();
		IEnumerable<TownGuardAttackModifierDef> townGuardDeck = _view.GameState.Outpost.TownGuardDeck;
		IEnumerable<TownGuardAttackModifierDef> townGuardUsedDeck = _view.GameState.Outpost.TownGuardUsedDeck;
		Dictionary<TownGuardAttackModifierDef, (int available, int used)> items = new Dictionary<TownGuardAttackModifierDef, (int, int)>();
		CountModifiers(townGuardDeck, isAvailable: true);
		CountModifiers(townGuardUsedDeck, isAvailable: false);
		return (from pair in items
			select perksService.AttackModifierDTOFromDef(pair.Key, pair.Value.available, pair.Value.used) into dto
			orderby dto.IsFailure descending, dto.IsSuccess, dto.Value
			select dto).ToList();
		void CountModifiers(IEnumerable<TownGuardAttackModifierDef> deck, bool isAvailable)
		{
			foreach (TownGuardAttackModifierDef item in deck)
			{
				if (!items.TryGetValue(item, out (int, int) value))
				{
					value = (0, 0);
				}
				value = (isAvailable ? (value.Item1 + 1, value.Item2) : (value.Item1, value.Item2 + 1));
				items[item] = value;
			}
		}
	}

	public List<OutpostAttackEventSummeryEntryDTO> GetAttackEventHistory()
	{
		BuildingViewService service = _view.GetService<BuildingViewService>();
		int totalAttackCount = _view.GameController.AttackController.GetTotalAttackCount();
		List<OutpostAttackEventSummeryEntryDTO> list = new List<OutpostAttackEventSummeryEntryDTO>(totalAttackCount);
		int currentAttack = _view.GameController.AttackController.GetCurrentAttack();
		for (int i = 0; i < totalAttackCount; i++)
		{
			if (i < _view.GameState.Outpost.ActiveEvent.Attack.Summary.Count)
			{
				OutpostAttackState.HistoryEntry historyEntry = _view.GameState.Outpost.ActiveEvent.Attack.Summary[i];
				BuildingState state = _view.GameState.Outpost.Buildings.FirstOrDefault((BuildingState buildingState) => buildingState.ID == historyEntry.TargetBuilding);
				list.Add(new OutpostAttackEventSummeryEntryDTO
				{
					Building = service.GetBuilding(state),
					DrawnCards = historyEntry.DrawnCardSummaries.Select(AttackModifierDTOFromCardUsageSummary).ToList(),
					Defended = historyEntry.Defended,
					Wrecked = historyEntry.Wrecked,
					Defense = historyEntry.Defense,
					FinalDefense = historyEntry.FinalDefense,
					Threat = historyEntry.Threat,
					FinalThreat = historyEntry.FinalThreat,
					Advantage = historyEntry.Advantage,
					Disadvantage = historyEntry.Disadvantage,
					MoraleSpent = historyEntry.MoraleSpent,
					InspirationSpent = historyEntry.InspirationSpent,
					ResourcesSpent = historyEntry.ResourcesSpent,
					Index = i,
					InProgress = (currentAttack == i),
					SoldersSpent = historyEntry.SoldersSpent,
					SoldersAvailable = historyEntry.SoldersAvailable
				});
			}
			else if (i == currentAttack)
			{
				list.Add(new OutpostAttackEventSummeryEntryDTO
				{
					Building = service.GetBuilding(_view.GameController.AttackController.CurrentTarget),
					InProgress = true,
					Index = i
				});
			}
			else
			{
				list.Add(new OutpostAttackEventSummeryEntryDTO
				{
					Index = i
				});
			}
		}
		return list;
	}

	private AttackModifierDTO AttackModifierDTOFromCardUsageSummary(OutpostAttackState.DrawnCardUsageSummary summery)
	{
		TownGuardAttackModifierDef value = DefRepo.TownGuardAttackModifierDefs.FirstOrDefault((KeyValuePair<TownGuardAttackModifierID, TownGuardAttackModifierDef> pair) => pair.Key == summery.CardID).Value;
		if (value == null)
		{
			Log.GUIError($"Trying to create DTO for Unknown TownGuardAttackModifierDef with id {summery.CardID}");
			return null;
		}
		return new AttackModifierDTO(value, 1)
		{
			Usage = summery.State
		};
	}

	public OutpostAttackEventDTO GetAttackEvent(uint soldiersToSpend)
	{
		OutpostAttackState attack = _view.GameState.Outpost.ActiveEvent.Attack;
		int totalAttackCount = _view.GameController.AttackController.GetTotalAttackCount();
		int totalDefense = GetTotalDefense(soldiersToSpend);
		bool isSuccessfulDefend = _view.GameController.AttackController.IsTownGardCardDrawn && _view.GameController.AttackController.IsSuccessfulDefend(soldiersToSpend, out var _, out var _);
		List<AttackModifierDTO> drawnCards = attack?.DrawnCardSummaries.Select(AttackModifierDTOFromCardUsageSummary).ToList();
		List<AttackModifierDTO> toChooseCards = null;
		if (_view.GameState.HasUnresolvedAction(CampaignInteractionType.ChooseAttackModifier))
		{
			toChooseCards = _view.GameController.AttackController?.GetTownGuardAttackModifiersToChoose()?.Select((TownGuardAttackModifierDef def) => new AttackModifierDTO(def, 1)).ToList();
		}
		BuildingViewService service = _view.GetService<BuildingViewService>();
		return new OutpostAttackEventDTO
		{
			CurrentState = attack.State,
			LastUsedSoldiers = attack.CurrentSoldiers,
			IsSuccessfulDefend = isSuccessfulDefend,
			CurrentTargetIndex = _view.GameController.AttackController.GetCurrentAttack(),
			CanUseSoldiers = _view.GameController.AttackController.CanUseSoldiers,
			TotalTargetCount = totalAttackCount,
			CurrentTargetBuilding = service.GetBuilding(_view.GameController.AttackController.CurrentTarget),
			BaseDefenceValue = _view.GameState.Outpost.BaseDefense,
			MoralDefenceValue = _view.GameController.OutpostController.MoraleDefenseModifier,
			BonusDefenceValue = _view.GameState.Outpost.BonusDefense,
			ActionType = _view.GameController.AttackController.GetTownGuardDrawCardAction(soldiersToSpend),
			PerSolderDefenseValueModifier = _view.GameController.AttackController.SoldierDefenseModifier,
			TotalDefenceValue = totalDefense,
			DefenceFromSolder = _view.GameController.AttackController.GetSoldiersDefenseModifier(soldiersToSpend),
			ThreatValue = attack.CurTargetThreat,
			TotalAttackValue = (int)attack.CurTargetThreat,
			DrawnCards = drawnCards,
			ToChooseCards = toChooseCards,
			AvailableMorale = _view.GameState.Outpost.Morale,
			LoseMoraleInsteadOfDamagingBuildings = attack.LoseMoraleInsteadOfDamagingBuildingsInt,
			LoseSoldiersInsteadOfDamagingBuildings = attack.LoseSoldiersInsteadOfDamagingBuildings,
			InitialSolderCount = attack.InitialSoldierCount,
			MaxSolderCount = _view.GameController.OutpostController.SoldierCapacity,
			MoraleSpentOnRepair = attack.MoraleSpent,
			ResourcesSpentOnRepair = attack.ResourcesSpent,
			InspirationSpentOnRepair = attack.InspirationSpent
		};
	}

	private int GetTotalDefense(uint soldiersToSpend)
	{
		if (_view.GameController.AttackController.IsTownGardCardDrawn)
		{
			return _view.GameController.AttackController.GetTotalDefense(soldiersToSpend);
		}
		int soldiersDefenseModifier = _view.GameController.AttackController.GetSoldiersDefenseModifier(soldiersToSpend);
		return _view.GameController.AttackController.GetOutpostDefense() + soldiersDefenseModifier;
	}

	public IEnumerable<FHInputEventElement.IInputEventData> GetSolderInteractions()
	{
		if (!_view.GameController.AttackController.HasActiveAttack() || !_view.IsPartyLeader || _view.GameState.Outpost.ActiveEvent.Attack.State != OutpostAttackState.AttackState.AttackNotStarted || !_view.GameController.AttackController.CanUseSoldiers || _view.GameController.AttackController.AvailableSoliders == 0 || !HasOperationalBarracks())
		{
			yield break;
		}
		uint soldiersToSpend = GetCurrentUsedSoldiersCount();
		yield return new FHInputEventContainer.Params
		{
			HintKey = "GUI_ACTION_ADD",
			HitLocalizedOverride = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick, "GUI_ACTION_ADD"),
			InteractionType = FHInputEventElement.UserInteractionType.LeftClick,
			ActionCallback = () =>
			{
				if (soldiersToSpend + 1 <= _view.GameController.AttackController.AvailableSoliders)
				{
					_view.Choreographer.ExecuteCommand(FHRPC.Campaign.Attack.ModifySoldiers, soldiersToSpend + 1);
				}
			}
		};
		yield return new FHInputEventContainer.Params
		{
			HintKey = "GUI_ACTION_REMOVE",
			HitLocalizedOverride = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.RightClick, "GUI_ACTION_REMOVE"),
			InteractionType = FHInputEventElement.UserInteractionType.RightClick,
			ActionCallback = () =>
			{
				if (soldiersToSpend != 0)
				{
					_view.Choreographer.ExecuteCommand(FHRPC.Campaign.Attack.ModifySoldiers, soldiersToSpend - 1);
				}
			}
		};
	}

	public void RepairBuildingWithMaterials()
	{
		BuildingState building = _view.GameController.AttackController.CurrentTarget;
		_view.PushState(new BuildingMaterialDistributionViewState(building)
		{
			OnConfirm = (BuildingMaterialDistribution distr) =>
			{
				_view.Choreographer.ExecuteCommand(FHRPC.Campaign.Buildings.RepairBuilding, building.Def, distr);
			}
		}, FHMapViewLayer.LayerType.Modal);
	}

	public void RepairBuildingWithMorale()
	{
		if (!_view.GameController.AttackController.HasActiveAttack())
		{
			return;
		}
		foreach (BuildingState item in _view.GameState.Outpost.Buildings.Where((BuildingState b) => b.DamagedState == BuildingDamagedState.Damaged))
		{
			_view.Choreographer.ExecuteCommand(FHRPC.Campaign.Buildings.DontRepairBuilding, item.Def);
		}
	}

	public void NextAttack()
	{
		if (_view.GameController.AttackController.CanEndCurrentAttack())
		{
			_view.Choreographer.ExecuteCommand(FHRPC.Campaign.Attack.EndCurrentAttack);
		}
	}

	public void PerformAttack(uint soldersToSpend)
	{
		if (_view.GameController.AttackController.HasActiveAttack())
		{
			_view.Choreographer.ExecuteCommand(FHRPC.Campaign.Attack.ResolveCurrentTarget, soldersToSpend);
		}
	}

	public void SelectModifier(int modiferIndex)
	{
		List<TownGuardAttackModifierDef> list = _view.GameController.AttackController.GetTownGuardAttackModifiersToChoose()?.ToList();
		if (list != null && list.Count == 2)
		{
			TownGuardAttackModifierDef townGuardAttackModifierDef = list[modiferIndex];
			if (townGuardAttackModifierDef != null)
			{
				_view.Choreographer.ExecuteCommand(FHRPC.Campaign.Attack.ResolveChooseAttackModifier, townGuardAttackModifierDef);
			}
		}
	}

	public bool HasOperationalBarracks()
	{
		return !_view.GetService<BuildingViewService>().GetBuildingByType<Barrack_BuildingTypeDef>().Wrecked;
	}

	public void FormatEffects(IEnumerable<CampaignEffect> effects, StringBuilder changes)
	{
		foreach (CampaignEffect item in CombineCampaignEffects(effects))
		{
			if (item.Type != CampaignEffectType.TriggerAttackOutpost && !IsHiddenEffect(item))
			{
				changes.AppendLine(EffectToStringLiteral(item));
			}
		}
		if (effects.Count() == 0)
		{
			changes.AppendLine("No effect");
		}
	}

	public LinkingSectionDTO GetLinkingSectionDTO(CampaignSectionDef sectionDef, IList<CampaignEffect> appliedEffects, string overrideTextKey = null)
	{
		bool isSoloGame = _view.IsSoloGame;
		LinkingSectionDTO linkingSectionDTO = new LinkingSectionDTO();
		linkingSectionDTO.ID = sectionDef.ID.ID;
		linkingSectionDTO.Caption = GLOOM.LocalizationManager.GetTranslation($"Event_{sectionDef.ID}_Name");
		string text = overrideTextKey ?? $"Event_{sectionDef.ID}_Text";
		linkingSectionDTO.Text = LocalizeMultiKeySectionDescriptions(text);
		linkingSectionDTO.Voiceover = text;
		linkingSectionDTO.Illustration = GetEventIllustration(sectionDef.ID.ID, OutpostEventType.Road, GameController.State.Outpost.Calendar.Season);
		linkingSectionDTO.IsProceedInteractable = isSoloGame || !_view.Player.Ready;
		linkingSectionDTO.ReadyUsers = _view.TryGetReadyUsersList();
		linkingSectionDTO.Effects.Clear();
		if (appliedEffects != null)
		{
			IEnumerable<CampaignEffect> enumerable = appliedEffects.Where((CampaignEffect e) => e.Type != CampaignEffectType.TriggerAttackOutpost);
			if (enumerable.Any())
			{
				foreach (CampaignEffect item in enumerable)
				{
					OutpostEventDTO.Effect formattedEffect = GetFormattedEffect(item);
					linkingSectionDTO.Effects.Add(formattedEffect);
				}
			}
		}
		return linkingSectionDTO;
	}

	public string LocalizeMultiKeySectionDescriptions(string sectionTextKey)
	{
		string text = string.Empty;
		if (GLOOM.LocalizationManager.TryGetTranslation(sectionTextKey, out var translation))
		{
			text = text + translation + "\n";
		}
		for (int i = 1; i <= 100; i++)
		{
			string text2 = sectionTextKey + "_" + i;
			I2.Loc.LocalizationManager.GetTermData("FHS/" + text2);
			if (GLOOM.LocalizationManager.TryGetTranslation(text2, out var translation2))
			{
				text = text + translation2 + "\n";
			}
		}
		return text;
	}

	public LinkingSectionDTO FromCampaignUnresolvedAction(CampaignUnresolvedAction campaignUnresolvedAction)
	{
		LinkingSectionDTO linkingSectionDTO = new LinkingSectionDTO();
		if (campaignUnresolvedAction.Type == CampaignInteractionType.ChooseNextCampaignSection)
		{
			string text2;
			string text = (text2 = campaignUnresolvedAction.GetString(0));
			string term = text + "_A";
			string term2 = text + "_B";
			if (!GLOOM.LocalizationManager.TryGetTranslation(term, out var translation))
			{
				term = "YES";
			}
			if (!GLOOM.LocalizationManager.TryGetTranslation(term2, out translation))
			{
				term2 = "NO";
			}
			linkingSectionDTO.Caption = string.Empty;
			linkingSectionDTO.Text = GLOOM.LocalizationManager.GetTranslation(text2);
			linkingSectionDTO.Voiceover = text2;
			linkingSectionDTO.Options = new List<OutpostEventDTO.Option>(2);
			linkingSectionDTO.Options.Add(new OutpostEventDTO.Option
			{
				Text = GLOOM.LocalizationManager.GetTranslation(term),
				Interactable = true,
				Action = () =>
				{
					Choreographer.ExecuteCommand(FHRPC.Campaign.OutpostEvent.ResolveChooseNextCampaignSection, campaignUnresolvedAction.GetString(1));
				},
				SuggestingUsers = (_view.IsSoloGame ? null : _tagsViewService.GetMultiplayerUsersWithTag(_tagsViewService.FormatTag("EVENT_OPTIONS", "1")).ToList())
			});
			linkingSectionDTO.Options.Add(new OutpostEventDTO.Option
			{
				Text = GLOOM.LocalizationManager.GetTranslation(term2),
				Interactable = true,
				Action = () =>
				{
					Choreographer.ExecuteCommand(FHRPC.Campaign.OutpostEvent.ResolveChooseNextCampaignSection, campaignUnresolvedAction.GetString(2));
				},
				SuggestingUsers = (_view.IsSoloGame ? null : _tagsViewService.GetMultiplayerUsersWithTag(_tagsViewService.FormatTag("EVENT_OPTIONS", "2")).ToList())
			});
			return linkingSectionDTO;
		}
		return linkingSectionDTO;
	}
}
