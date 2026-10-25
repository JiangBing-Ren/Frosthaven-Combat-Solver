using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.Common.DTO;
using FHGUI.Elements;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Common;

public class UIGeneralInfoMonster : UIGeneralInfoBase, IUIGeneralInfo<UIGeneralInfoMonster.MonsterGeneralInfoData>, IUIGeneralInfo
{
	public struct MonsterGeneralInfoData : IGeneralInfoData
	{
		public string ActorGuid;

		public string ID;

		public string Name;

		public Sprite Illustration;

		public uint Level;

		public BaseStatsDTO Stats;

		public BaseStatsDTO BaseStats;

		public bool ShowFactions;

		public CActor.EType Faction;

		public UIGeneralInfoUtils.CharacterTokenAttributes CharacterTokenAttributes;

		public CMonsterClass MonsterClass;

		public StatusEffectDTO[] PositiveConditions;

		public StatusEffectDTO[] NegativeConditions;

		public StatusEffectDTO[] Immunities;

		public StatusEffectDTO[] Innates;

		public AttackModifierInfo[] AttackModifiers;

		public ActiveAbilityDTO[] ActiveAbilities;

		public MonsterCardDTO? RoundCard;

		public MonsterCardDTO? NextCard;

		public MonsterCardDTO[] Deck;

		public Func<int, MonsterCardUIContext> GetMonsterCardContext;

		public CMonsterClass AltMonsterClass;

		public MonsterCardDTO[] AltDeck;

		public MonsterCardDTO? AltRoundCard;

		public Func<int, MonsterCardUIContext> AltMonsterCardContext;

		public int? Initiative => RoundCard?.Initiative;
	}

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

	[Header("Top")]
	[SerializeField]
	private TMP_Text _name;

	[SerializeField]
	private Image _portrait;

	[SerializeField]
	private GameObject _factionContainer;

	[SerializeField]
	private TMP_Text _faction;

	[SerializeField]
	private FHImageFillElement _healthbar;

	[SerializeField]
	private TMP_Text _healthbarText;

	[SerializeField]
	private UICharacterAttributesBanner _banner;

	[Header("Status")]
	[SerializeField]
	private TMP_Text _levelText;

	[SerializeField]
	private UIGeneralInfoSection _attackModifiersContainer;

	[SerializeField]
	private UIGeneralInfoSection _negativeConditionsContainer;

	[SerializeField]
	private UIGeneralInfoSection _positiveConditionsContainer;

	[SerializeField]
	private UIConditionEffectListContainer _immunities;

	[SerializeField]
	private UIGeneralInfoSection _activeAbilitiesContainer;

	[SerializeField]
	private UIGeneralInfoSection _roundCardContainer;

	[SerializeField]
	private UIGeneralInfoSection _nextCardContainer;

	[Header("Stats")]
	[SerializeField]
	private UIBaseStatsView _stats;

	[Header("Cards")]
	[SerializeField]
	private UIMonsterMiniCardContainer _cardContainer;

	[SerializeField]
	private UIGeneralInfoSection _altCardContainer;

	private MonsterGeneralInfoData _data;

	private List<StatusEffectDTO> _innatesAndImmunities = new List<StatusEffectDTO>();

	private List<MonsterCardUIContext> _monsterCards = new List<MonsterCardUIContext>(10);

	private List<MonsterCardUIContext> _altMonsterCards = new List<MonsterCardUIContext>(10);

	private void Awake()
	{
		_statusTab.onClick.AddListener(ShowStatusTab);
		_cardsTab.onClick.AddListener(ShowCardsTab);
	}

	public bool Show(MonsterGeneralInfoData data, params object[] parameters)
	{
		gameObject.SetActive(value: true);
		_data = data;
		_name.text = data.Name;
		_healthbarText.text = $"{_data.Stats.Health}/{_data.Stats.MaxHealth}";
		_healthbar.SetProgress(_data.Stats.HealthPercent);
		_levelText.text = _data.Level.ToString();
		_portrait.sprite = _data.Illustration;
		_portrait.enabled = _data.Illustration != null;
		_banner.Init(_data.CharacterTokenAttributes);
		_stats.Init(_data.BaseStats);
		string factionText = UICharacterUtils.GetFactionText(_data.Faction);
		_factionContainer.SetActive(_data.ShowFactions && !string.IsNullOrEmpty(factionText));
		_faction.text = factionText;
		_positiveConditionsContainer.Fill(show: true, (UIConditionEffectListContainer c) =>
		{
			c.Init(_data.PositiveConditions, 3);
		});
		_negativeConditionsContainer.Fill(show: true, (UIConditionEffectListContainer c) =>
		{
			c.Init(_data.NegativeConditions, 3);
		});
		_innatesAndImmunities.Clear();
		_innatesAndImmunities.AddRange(_data.Innates.Concat(_data.Immunities));
		_immunities.Init(_innatesAndImmunities);
		_attackModifiersContainer.Fill(_data.AttackModifiers?.Any() ?? false, (UIAttackModifiersContainer c) =>
		{
			c.Init(null, _data.AttackModifiers);
		});
		UIGeneralInfoSection activeAbilitiesContainer = _activeAbilitiesContainer;
		ActiveAbilityDTO[] activeAbilities = _data.ActiveAbilities;
		activeAbilitiesContainer.Fill(activeAbilities != null && activeAbilities.Length > 0, (UIActiveAbilityListContainer c) =>
		{
			c.Init(null, _data.ActiveAbilities);
		});
		_roundCardContainer.Fill(_data.RoundCard.HasValue, (RectTransform rectTransform) =>
		{
			MonsterCardDTO value = _data.RoundCard.Value;
			RecycleCards(_roundCardContainer);
			SpawnCard(value.ID, rectTransform, _data.GetMonsterCardContext);
			if (_data.AltRoundCard.HasValue)
			{
				SpawnCard(_data.AltRoundCard.Value.ID, rectTransform, _data.AltMonsterCardContext);
			}
		});
		_nextCardContainer.Fill(_data.NextCard.HasValue, (RectTransform rectTransform) =>
		{
			MonsterCardDTO value = _data.NextCard.Value;
			RecycleCards(_nextCardContainer);
			SpawnCard(value.ID, rectTransform, _data.GetMonsterCardContext);
		});
		UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.WindowOpened, null, "GeneralInfoMonster"));
		return true;
	}

	private void ShowStatusTab()
	{
		_tabsGroup.SwitchToTab(_statusTab);
		_statusContainer.ScrollToTop();
	}

	private void ShowCardsTab()
	{
		_tabsGroup.SwitchToTab(_cardsTab);
		_cardsContainer.ScrollToTop();
		_monsterCards.Clear();
		_monsterCards.AddRange(_data.Deck.Select((MonsterCardDTO card) => _data.GetMonsterCardContext(card.ID)));
		_cardContainer.Init(null, _monsterCards);
		_altMonsterCards.Clear();
		if (_data.AltDeck != null)
		{
			_altMonsterCards.AddRange(_data.AltDeck.Select((MonsterCardDTO card) => _data.AltMonsterCardContext(card.ID)));
		}
		_altCardContainer.Fill(_altMonsterCards.Count > 0, (UIMonsterMiniCardContainer container) =>
		{
			container.Init(null, _altMonsterCards);
		});
	}

	private void RecycleCards(UIGeneralInfoSection section)
	{
		MonsterRoundCardUI[] componentsInChildren = section.Content.GetComponentsInChildren<MonsterRoundCardUI>();
		for (int num = componentsInChildren.Length - 1; num >= 0; num--)
		{
			ObjectPool.RecycleCard(componentsInChildren[num].CardID, ObjectPool.ECardType.Monster, componentsInChildren[num].gameObject);
		}
	}

	private void SpawnCard(int id, RectTransform container, Func<int, MonsterCardUIContext> context)
	{
		ObjectPool.SpawnCard(id, ObjectPool.ECardType.Monster, container, resetLocalScale: true).GetComponent<MonsterRoundCardUI>().Init(context(id));
	}

	protected override void OnHide_Internal()
	{
		base.OnHide_Internal();
		UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.WindowClosed, null, "GeneralInfoMonster"));
	}
}
