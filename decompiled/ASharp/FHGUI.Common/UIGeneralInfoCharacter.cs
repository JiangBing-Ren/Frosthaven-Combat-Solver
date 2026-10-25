using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Modules;
using GLOOM;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Common;

public class UIGeneralInfoCharacter : UIGeneralInfoBase, IUIGeneralInfo<UIGeneralInfoCharacter.PlayerCharacterGeneralInfoData>, IUIGeneralInfo
{
	public struct PlayerCharacterGeneralInfoData : IGeneralInfoData
	{
		public ECharacter Character;

		public string Name;

		public string ClassName;

		public uint Level;

		public Sprite Illustration;

		public int? Initiative;

		public bool IsOwner;

		public bool isExhausted;

		public UIMultiplayerUser.Data? MultiplayerUserData;

		public uint Health;

		public uint MaxHealth;

		public uint OriginalMaxHealth;

		public uint Experience;

		public uint MaxExperience;

		public StatusEffectDTO[] PositiveConditions;

		public StatusEffectDTO[] NegativeConditions;

		public StatusEffectDTO[] Attributes;

		public AttackModifierInfo[] AttackModifiers;

		public PerkDTO[] Perks;

		public HeroSummonGeneralDTO[] Summons;

		public ActiveAbilityDTO[] ActiveAbilities;

		public ItemDataDTO[] Equipment;

		public ItemDataDTO[] SmallItems;

		public ItemDataDTO[] Pets;

		public ItemDataDTO[] Loot;

		public int? ChestsLooted;

		public CAbilityCard[] ActivatedCards;

		public CAbilityCard[] CardsInHand;

		public CAbilityCard[] DiscardedCards;

		public CAbilityCard[] LostCards;

		public CAbilityCard[] RoundCards;

		public CAbilityCard[] TakenCards;

		public string TakenCardPileKey;

		public Action<HeroSummonGeneralDTO> ViewSummonSource;

		public Action<ActiveAbilityDTO> ViewActiveAbilitySource;

		public Action<int> DeactivateActiveAbilityCard;

		public CPlayerActor Actor;

		public float HealthPercent
		{
			get
			{
				if (MaxHealth != 0)
				{
					return (float)Health / (float)MaxHealth;
				}
				return 1f;
			}
		}

		public float ExpPercent
		{
			get
			{
				if (MaxExperience != 0)
				{
					return (float)Experience / (float)MaxExperience;
				}
				return 1f;
			}
		}

		public IEnumerable<ItemDataDTO> AllItems => Equipment.Concat(SmallItems);

		public IEnumerable<AttackModifierInfo> BasicAttackModifiers => AttackModifiers?.Where((AttackModifierInfo mod) => mod.IsBasicModifier);

		public IEnumerable<AttackModifierInfo> ConditionalModifiers => AttackModifiers?.Where((AttackModifierInfo mod) => !mod.IsBasicModifier);
	}

	[Header("Top")]
	[SerializeField]
	private TMP_Text _name;

	[SerializeField]
	private TMP_Text _typeLabel;

	[SerializeField]
	private Image _portrait;

	[SerializeField]
	private GameObject _initiativeContainer;

	[SerializeField]
	private TMP_Text _initiative;

	[SerializeField]
	private Image _classIcon;

	[SerializeField]
	private FHImageFillElement _healthbar;

	[SerializeField]
	private TMP_Text _healthbarText;

	[SerializeField]
	private MultiplayerUserState _multiplayerUI;

	[Header("Tabs")]
	[SerializeField]
	private FHTabsGroup _tabsGroup;

	[SerializeField]
	private FHTabButton _statusTab;

	[SerializeField]
	private ScrollRect _statusContainer;

	[SerializeField]
	private FHTabButton _cardsTab;

	[SerializeField]
	private ScrollRect _cardsContainer;

	[Header("Status")]
	[SerializeField]
	private TMP_Text _levelText;

	[SerializeField]
	private TMP_Text _xpText;

	[SerializeField]
	private FHImageFillElement _xpBar;

	[SerializeField]
	private UIGeneralInfoSection _perksContainer;

	[SerializeField]
	private UIGeneralInfoSection _positiveConditionsContainer;

	[SerializeField]
	private UIGeneralInfoSection _negativeConditionsContainer;

	[SerializeField]
	private UIGeneralInfoSection _activeAbilitiesContainer;

	[SerializeField]
	private UIGeneralInfoSection _attackModifiersContainer;

	[SerializeField]
	private UISummonListContainer _summonsContainer;

	[Header("Inventory")]
	[SerializeField]
	private UIGeneralInfoSection _inventoryItemsContainer;

	[SerializeField]
	private UIGeneralInfoSection _petsContainer;

	[SerializeField]
	private UIGeneralInfoSection _lootContainer;

	[Header("Cards")]
	[SerializeField]
	private UIGeneralInfoSection _roundCardsContainer;

	[SerializeField]
	private UIGeneralInfoSection _activeCardsContainer;

	[SerializeField]
	private UIGeneralInfoSection _availableCardsContainer;

	[SerializeField]
	private UIGeneralInfoSection _discardedCardsContainer;

	[SerializeField]
	private UIGeneralInfoSection _lostCardsContainer;

	[SerializeField]
	private UIGeneralInfoSection _takenCardsContainer;

	private PlayerCharacterGeneralInfoData _data;

	private const int MIN_CONDITIONS_TO_SHOW = 3;

	private void Awake()
	{
		_statusTab.onClick.AddListener(ShowStatusTab);
	}

	public bool Show(PlayerCharacterGeneralInfoData data, params object[] parameters)
	{
		gameObject.SetActive(value: true);
		_data = data;
		bool flag = _data.MultiplayerUserData.HasValue && FFSNetwork.Players.Count() > 1;
		_multiplayerUI.gameObject.SetActive(flag);
		if (flag)
		{
			_multiplayerUI.Show(_data.MultiplayerUserData.Value);
		}
		_name.text = data.Name;
		_portrait.sprite = data.Illustration;
		_initiativeContainer.SetActive(value: false);
		_classIcon.sprite = UIInfoTools.Instance.GetCharacterSprite(_data.Character, highlight: true);
		_levelText.text = _data.Level.ToString();
		_xpText.text = $"{_data.Experience}/{_data.MaxExperience}";
		_xpText.enabled = _data.Level != 9;
		_xpBar.SetProgress(_data.ExpPercent);
		_healthbarText.text = $"{_data.Health}/{_data.MaxHealth}";
		_healthbar.SetProgress(_data.HealthPercent);
		_typeLabel.text = _data.ClassName ?? "";
		FillCardsTab();
		ShowStatusTab();
		UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.WindowOpened, null, "GeneralInfoCharacter"));
		return true;
	}

	private void ShowStatusTab()
	{
		_tabsGroup.SwitchToTab(_statusTab);
		_statusContainer.ScrollToTop();
		_attackModifiersContainer.Fill(_data.BasicAttackModifiers?.Any() ?? false, (UIAttackModifiersContainer c) =>
		{
			c.Init(null, _data.AttackModifiers);
		});
		ItemDataDTO[] allItems = _data.AllItems.ToArray();
		_inventoryItemsContainer.Fill(show: true, (UIScenarioEquipmentContainer c) =>
		{
			c.Init(allItems);
		});
		UIGeneralInfoSection petsContainer = _petsContainer;
		ItemDataDTO[] pets = _data.Pets;
		petsContainer.Fill(pets != null && pets.Length > 0, (UIItemsSimpleContainer c) =>
		{
			c.Init(_data.Pets);
		});
		UIGeneralInfoSection lootContainer = _lootContainer;
		pets = _data.Loot;
		lootContainer.Fill(pets != null && pets.Length > 0, (UIItemsSimpleContainer c) =>
		{
			c.Init(null, _data.Loot);
		});
		if (_data.ChestsLooted.HasValue)
		{
			_lootContainer.Label.SetFormat("{KEY} ({0})");
			_lootContainer.Label.SetParam("0", _data.ChestsLooted.Value);
		}
		else
		{
			_lootContainer.Label.SetFormat("{KEY}");
		}
		_positiveConditionsContainer.Fill(show: true, (UIConditionEffectListContainer c) =>
		{
			c.Init(_data.PositiveConditions, 3);
		});
		_negativeConditionsContainer.Fill(show: true, (UIConditionEffectListContainer c) =>
		{
			c.Init(_data.NegativeConditions, 3);
		});
		UIGeneralInfoSection perksContainer = _perksContainer;
		PerkDTO[] perks = _data.Perks;
		perksContainer.Fill(perks != null && perks.Length > 0, (UIPerkContainer c) =>
		{
			c.Init(null, _data.Perks);
		});
		UIGeneralInfoSection activeAbilitiesContainer = _activeAbilitiesContainer;
		ActiveAbilityDTO[] activeAbilities = _data.ActiveAbilities;
		activeAbilitiesContainer.Fill(activeAbilities != null && activeAbilities.Length > 0, (UIActiveAbilityListContainer c) =>
		{
			c.Init(null, _data.ActiveAbilities);
		});
		_summonsContainer.Init(new UISimpleInteractionEvents
		{
			OnClick = (object target) =>
			{
				if (target is HeroSummonGeneralDTO heroSummonGeneralDTO && _data.IsOwner && heroSummonGeneralDTO.SourceCard.HasValue)
				{
					_data.DeactivateActiveAbilityCard?.Invoke(heroSummonGeneralDTO.SourceCard.Value);
				}
			}
		}, _data.Summons, FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick) + " " + LocalizationManager.GetTranslation("GUI_ACTION_DEACTIVATE"));
	}

	public void FillCardsTab()
	{
		UIGeneralInfoSection roundCardsContainer = _roundCardsContainer;
		CAbilityCard[] roundCards = _data.RoundCards;
		roundCardsContainer.Fill(roundCards != null && roundCards.Length > 0, (UIAbilityCardContainer c) =>
		{
			c.Init(_data.RoundCards, _data.Actor, _data.Character);
		});
		UIGeneralInfoSection activeCardsContainer = _activeCardsContainer;
		roundCards = _data.ActivatedCards;
		activeCardsContainer.Fill(roundCards != null && roundCards.Length > 0, (UIAbilityCardContainer c) =>
		{
			c.Init(_data.ActivatedCards, _data.Actor, _data.Character, new UIScenarioCardInteractionEvents
			{
				OnClick = (_data.IsOwner ? ((UIInteractionEvents<AbilityCardUI>.OnUIEvent)((AbilityCardUI card) =>
				{
					_data.DeactivateActiveAbilityCard?.Invoke(card.CardID);
				})) : null)
			});
		});
		UIGeneralInfoSection availableCardsContainer = _availableCardsContainer;
		roundCards = _data.CardsInHand;
		availableCardsContainer.Fill(roundCards != null && roundCards.Length > 0, (UIAbilityCardContainer c) =>
		{
			c.Init(_data.CardsInHand, _data.Actor, _data.Character);
		});
		UIGeneralInfoSection discardedCardsContainer = _discardedCardsContainer;
		roundCards = _data.DiscardedCards;
		discardedCardsContainer.Fill(roundCards != null && roundCards.Length > 0, (UIAbilityCardContainer c) =>
		{
			c.Init(_data.DiscardedCards, _data.Actor, _data.Character);
		});
		UIGeneralInfoSection lostCardsContainer = _lostCardsContainer;
		roundCards = _data.LostCards;
		lostCardsContainer.Fill(roundCards != null && roundCards.Length > 0, (UIAbilityCardContainer c) =>
		{
			c.Init(_data.LostCards, _data.Actor, _data.Character);
		});
		UIGeneralInfoSection takenCardsContainer = _takenCardsContainer;
		roundCards = _data.TakenCards;
		takenCardsContainer.Fill(roundCards != null && roundCards.Length > 0, (UIAbilityCardContainer c) =>
		{
			c.Init(_data.TakenCards, _data.Actor, _data.Character);
		});
		_takenCardsContainer.Label.SetTextKey(_data.TakenCardPileKey);
		_cardsContainer.ScrollToTop();
	}

	private void ClearCardsTab()
	{
		UIAbilityCardContainer[] componentsInChildren = _cardsContainer.GetComponentsInChildren<UIAbilityCardContainer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Clear();
		}
	}

	protected override void OnHide_Internal()
	{
		base.OnHide_Internal();
		ClearCardsTab();
		UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.WindowClosed, null, "GeneralInfoCharacter"));
	}
}
