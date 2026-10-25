using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FFSNet;
using GLOOM;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.Events;

public class UIUseAbilitiesBar : Singleton<UIUseAbilitiesBar>
{
	public class AlternateAttackModifier : IAbility
	{
		private CAbility ability;

		private CBaseAbilityCard card;

		private string tooltipTitle;

		private Sprite _icon;

		public List<IInfuseElement> Infusions { get; private set; }

		public List<ElementInfusionBoardManager.EElement> Consumes { get; private set; }

		public List<ElementInfusionBoardManager.EElement> ExcludedConsumes { get; private set; }

		public List<ElementInfusionBoardManager.EElement> SelectedElements { get; set; }

		public List<CAbility> Abilities => new List<CAbility> { ability };

		public Sprite Icon
		{
			get
			{
				return _icon;
			}
			set
			{
				_icon = value;
			}
		}

		public string ID { get; private set; }

		public CItem DescriptionItem => null;

		public string DescriptionTitle
		{
			get
			{
				if (tooltipTitle != null || card == null)
				{
					return tooltipTitle;
				}
				return LocalizationManager.GetTranslation(card.Name);
			}
		}

		public string DescriptionText { get; private set; }

		public string OverrideDescription { get; set; }

		public AlternateAttackModifier(AttackModifierCard card, string tooltipTitle = null, string tooltipText = null)
		{
			Infusions = null;
			Consumes = null;
			ability = card?.YMLData.Abilities.First();
			this.card = ability?.GetAbilityBaseCard() as CBaseAbilityCard;
			this.tooltipTitle = tooltipTitle;
			_icon = UIInfoTools.Instance.GetAttackModifierSprite(card.YMLData.AlternateMathModifier);
			DescriptionText = tooltipText;
			OverrideDescription = tooltipTitle;
			ID = $"{card.Name}{((card == null) ? string.Empty : card.ID.ToString())}";
			SelectedElements = new List<ElementInfusionBoardManager.EElement>();
		}

		public Tuple<IOptionHolder, List<IOption>> GenerateOption(UIUseOption optionUI)
		{
			return null;
		}

		public string GetSelectAudioItem()
		{
			return null;
		}
	}

	public class Ability : IAbility
	{
		private CAbility ability;

		private CBaseAbilityCard card;

		private string tooltipTitle;

		private Sprite _icon;

		public List<IInfuseElement> Infusions { get; private set; }

		public List<ElementInfusionBoardManager.EElement> Consumes { get; private set; }

		public List<ElementInfusionBoardManager.EElement> ExcludedConsumes { get; private set; }

		public List<ElementInfusionBoardManager.EElement> SelectedElements { get; set; }

		public List<CAbility> Abilities => new List<CAbility> { ability };

		public Sprite Icon
		{
			get
			{
				return _icon;
			}
			set
			{
				_icon = value;
			}
		}

		public string ID { get; private set; }

		public CItem DescriptionItem => null;

		public string DescriptionTitle
		{
			get
			{
				if (tooltipTitle != null || card == null)
				{
					return tooltipTitle;
				}
				return LocalizationManager.GetTranslation(card.Name);
			}
		}

		public string DescriptionText { get; private set; }

		public string OverrideDescription { get; set; }

		public Ability(CAbility ability, CBaseAbilityCard card, List<IInfuseElement> infusions = null, List<ElementInfusionBoardManager.EElement> consumes = null, string tooltipTitle = null, string tooltipText = null, List<ElementInfusionBoardManager.EElement> excludedConsumes = null)
		{
			Infusions = infusions;
			Consumes = consumes;
			this.ability = ability;
			this.card = card;
			this.tooltipTitle = tooltipTitle;
			_icon = UIInfoTools.Instance.GetElementUseSprite(ElementInfusionBoardManager.EElement.Any);
			DescriptionText = tooltipText;
			ID = $"{ability.Name}{((card == null) ? string.Empty : card.ID.ToString())}";
			SelectedElements = new List<ElementInfusionBoardManager.EElement>();
			ExcludedConsumes = excludedConsumes;
		}

		public Tuple<IOptionHolder, List<IOption>> GenerateOption(UIUseOption optionUI)
		{
			return null;
		}

		public string GetSelectAudioItem()
		{
			return null;
		}
	}

	public class LootCard : IAbility
	{
		private ScenarioRuleLibrary.LootCard.EType type;

		private int amount;

		private Sprite _icon;

		public Sprite Icon
		{
			get
			{
				return _icon;
			}
			set
			{
				_icon = value;
			}
		}

		public string ID { get; private set; }

		public CItem DescriptionItem => null;

		public string DescriptionText
		{
			get
			{
				switch (type)
				{
				case ScenarioRuleLibrary.LootCard.EType.Lumber:
				case ScenarioRuleLibrary.LootCard.EType.Metal:
				case ScenarioRuleLibrary.LootCard.EType.Hide:
				case ScenarioRuleLibrary.LootCard.EType.Arrowvine:
				case ScenarioRuleLibrary.LootCard.EType.Axenut:
				case ScenarioRuleLibrary.LootCard.EType.Corpsecap:
				case ScenarioRuleLibrary.LootCard.EType.Flamefruit:
				case ScenarioRuleLibrary.LootCard.EType.Rockroot:
				case ScenarioRuleLibrary.LootCard.EType.Snowthistle:
					return $"{amount} <sprite name={type}>";
				case ScenarioRuleLibrary.LootCard.EType.Money:
					return string.Format("{0} {1}", amount, LocalizationManager.GetTranslation("Gold"));
				case ScenarioRuleLibrary.LootCard.EType.RandomItem:
					return string.Format("{0} {1}", amount, LocalizationManager.GetTranslation("RANDOM_ITEM_TOOLTIP"));
				default:
					return $"{amount} {LocalizationManager.GetTranslation(type.ToString())}";
				}
			}
		}

		public string DescriptionTitle { get; private set; }

		public string OverrideDescription { get; set; }

		List<IInfuseElement> IAbility.Infusions => null;

		List<ElementInfusionBoardManager.EElement> IAbility.Consumes => null;

		List<ElementInfusionBoardManager.EElement> IAbility.ExcludedConsumes => null;

		List<ElementInfusionBoardManager.EElement> IAbility.SelectedElements
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		List<CAbility> IAbility.Abilities => null;

		public LootCard(ScenarioRuleLibrary.LootCard.EType type, int amount)
		{
			this.type = type;
			this.amount = amount;
			_icon = UIInfoTools.Instance.GetIconLootType(type);
			OverrideDescription = DescriptionText;
			ID = $"{type}{amount}";
		}

		public Tuple<IOptionHolder, List<IOption>> GenerateOption(UIUseOption optionUI)
		{
			return null;
		}

		public string GetSelectAudioItem()
		{
			return null;
		}
	}

	public class DummyAbilityInfusion : IAbility
	{
		public List<IInfuseElement> Infusions { get; private set; }

		public List<ElementInfusionBoardManager.EElement> Consumes => null;

		List<ElementInfusionBoardManager.EElement> IAbility.ExcludedConsumes => null;

		public List<ElementInfusionBoardManager.EElement> SelectedElements { get; set; }

		public List<CAbility> Abilities => new List<CAbility>();

		public Sprite Icon
		{
			get
			{
				return UIInfoTools.Instance.GetElementUseSprite(ElementInfusionBoardManager.EElement.Any);
			}
			set
			{
			}
		}

		public string ID { get; private set; }

		public CItem DescriptionItem => null;

		public string DescriptionTitle { get; private set; }

		public string DescriptionText { get; private set; }

		public string OverrideDescription { get; set; }

		public DummyAbilityInfusion(string id, List<IInfuseElement> infusions, string tooltipTitle = null, string tooltipText = null)
		{
			DescriptionText = tooltipText;
			DescriptionTitle = tooltipTitle;
			Infusions = infusions;
			ID = id;
			SelectedElements = new List<ElementInfusionBoardManager.EElement>();
		}

		public Tuple<IOptionHolder, List<IOption>> GenerateOption(UIUseOption optionUI)
		{
			return null;
		}

		public string GetSelectAudioItem()
		{
			return null;
		}
	}

	public class InfuseAction : IAbility
	{
		private CAction action;

		private FullAbilityCard card;

		public List<IInfuseElement> Infusions { get; private set; }

		public List<ElementInfusionBoardManager.EElement> Consumes => null;

		List<ElementInfusionBoardManager.EElement> IAbility.ExcludedConsumes => null;

		public List<ElementInfusionBoardManager.EElement> SelectedElements { get; set; }

		public List<CAbility> Abilities => action.Abilities;

		public Sprite Icon
		{
			get
			{
				return UIInfoTools.Instance.GetElementUseSprite(ElementInfusionBoardManager.EElement.Any);
			}
			set
			{
			}
		}

		public string ID { get; private set; }

		public CItem DescriptionItem => null;

		public string DescriptionTitle => LocalizationManager.GetTranslation(card.AbilityCard.Name);

		public string DescriptionText => LocalizationManager.GetTranslation("GUI_CHOOSE_ELEMENT_INFUSE");

		public string OverrideDescription { get; set; }

		public InfuseAction(CAction action, List<IInfuseElement> infusions, FullAbilityCard card)
		{
			Infusions = infusions;
			this.action = action;
			this.card = card;
			ID = $"{card.AbilityCard.ID} {((card.AbilityCard.BottomAction != action) ? CBaseCard.ActionType.TopAction : CBaseCard.ActionType.BottomAction)}";
			SelectedElements = new List<ElementInfusionBoardManager.EElement>();
		}

		public Tuple<IOptionHolder, List<IOption>> GenerateOption(UIUseOption optionUI)
		{
			return null;
		}

		public string GetSelectAudioItem()
		{
			return null;
		}
	}

	private class DummyAnyInfusion : IInfuseElement
	{
		public ElementInfusionBoardManager.EElement SelectedElement { get; private set; } = ElementInfusionBoardManager.EElement.Any;

		public bool IsAnyElement => true;

		public void PickElement(ElementInfusionBoardManager.EElement element)
		{
			SelectedElement = element;
		}

		public void ResetElementToInitial()
		{
			SelectedElement = ElementInfusionBoardManager.EElement.Any;
		}
	}

	public class DummyInfusion : IInfuseElement
	{
		public List<ElementInfusionBoardManager.EElement> Elements;

		public ElementInfusionBoardManager.EElement SelectedElement { get; private set; } = ElementInfusionBoardManager.EElement.Any;

		public bool IsAnyElement => false;

		public DummyInfusion(List<ElementInfusionBoardManager.EElement> elements)
		{
			Elements = elements;
		}

		public void PickElement(ElementInfusionBoardManager.EElement element)
		{
			SelectedElement = element;
		}

		public void ResetElementToInitial()
		{
			SelectedElement = ElementInfusionBoardManager.EElement.Any;
		}
	}

	private class DummyAnyConsume : IInfuseElement
	{
		public ElementInfusionBoardManager.EElement SelectedElement { get; private set; } = ElementInfusionBoardManager.EElement.Any;

		public bool IsAnyElement => true;

		public void PickElement(ElementInfusionBoardManager.EElement element)
		{
			SelectedElement = element;
		}

		public void ResetElementToInitial()
		{
			SelectedElement = ElementInfusionBoardManager.EElement.Any;
		}
	}

	[SerializeField]
	private RectTransform container;

	[SerializeField]
	private UIUseAbility slotPrefab;

	[SerializeField]
	private UnityEvent OnChangedAbilitiesAvailable;

	private Dictionary<string, UIUseAbility> slots = new Dictionary<string, UIUseAbility>();

	private Stack<UIUseAbility> pool = new Stack<UIUseAbility>();

	private CActor actor;

	private Transform lastCreatedSlot;

	public bool IsShown => slots.Count > 0;

	public bool HasShownUsableSlots()
	{
		if (IsShown)
		{
			return slots.Any((KeyValuePair<string, UIUseAbility> s) => s.Value.gameObject.activeSelf);
		}
		return false;
	}

	public IEnumerable<ElementInfusionBoardManager.EElement> GetAllConsumes()
	{
		foreach (KeyValuePair<string, UIUseAbility> item in slots.Where((KeyValuePair<string, UIUseAbility> x) => x.Value.IsSelected()))
		{
			foreach (ElementInfusionBoardManager.EElement selectedConsume in item.Value.GetSelectedConsumes())
			{
				yield return selectedConsume;
			}
		}
	}

	private void Create(IAbility ability, Action<IAbility> onSelect, Action<IAbility> onDeselect = null, bool preSelect = false, bool isSelected = false, bool hideConsumableElementUI = false)
	{
		if (!slots.ContainsKey(ability.ID))
		{
			UIUseAbility slotFromPool = GetSlotFromPool();
			slots[ability.ID] = slotFromPool;
			slotFromPool.Show(preSelect);
		}
		else
		{
			slots[ability.ID].Show(instant: true);
		}
		slots[ability.ID].Init(actor, ability, onSelect, onDeselect, isSelected);
		if (preSelect)
		{
			slots[ability.ID].Select();
		}
		if (hideConsumableElementUI)
		{
			slots[ability.ID].HideConsumeElements();
		}
	}

	public void Remove(CAbility ability)
	{
		string key = $"{ability.Name}{((ability.GetAbilityBaseCard() != null) ? ability.GetAbilityBaseCard().ID.ToString() : string.Empty)}";
		if (slots.ContainsKey(key))
		{
			UIUseAbility uIUseAbility = slots[key];
			uIUseAbility.Hide();
			pool.Push(uIUseAbility);
			slots.Remove(key);
			OnChangedAbilitiesAvailable.Invoke();
		}
	}

	private UIUseAbility GetSlotFromPool()
	{
		UIUseAbility uIUseAbility;
		if (pool.Count > 0)
		{
			uIUseAbility = pool.Pop();
			uIUseAbility.gameObject.SetActive(value: true);
			return uIUseAbility;
		}
		uIUseAbility = UnityEngine.Object.Instantiate(slotPrefab, container);
		if (lastCreatedSlot != null)
		{
			uIUseAbility.transform.SetSiblingIndex(lastCreatedSlot.GetSiblingIndex() + 1);
		}
		lastCreatedSlot = uIUseAbility.transform;
		return uIUseAbility;
	}

	public void Hide(bool clearSelection = false)
	{
		foreach (KeyValuePair<string, UIUseAbility> slot in slots)
		{
			if (clearSelection)
			{
				slot.Value.ClearSelection();
			}
			slot.Value.Hide();
			pool.Push(slot.Value);
		}
		if (!slots.IsNullOrEmpty())
		{
			OnChangedAbilitiesAvailable.Invoke();
		}
		slots.Clear();
		lastCreatedSlot = null;
	}

	public void UndoSelection()
	{
		foreach (KeyValuePair<string, UIUseAbility> slot in slots)
		{
			slot.Value.ClearSelection();
		}
	}

	private void NetworkConsumeAbility(IAbility ability, bool consume)
	{
		if (FFSNetwork.IsOnline && actor != null && actor.IsUnderMyControl())
		{
			Synchronizer.SendGameAction(GameActionType.PickElement, ActionPhaseType.ElementPicking, validateOnServerBeforeExecuting: false, disableAutoReplication: false, actor.ID, 0, 0, 0, supplementaryDataBoolean: false, null, new InfuseToken(ability.ID, isConsume: true, !consume, ability.SelectedElements));
		}
	}

	private void NetworkInfuseAbility(IAbility ability, bool infuse)
	{
		if (FFSNetwork.IsOnline && actor != null && actor.IsUnderMyControl())
		{
			Synchronizer.SendGameAction(GameActionType.PickElement, ActionPhaseType.ElementPicking, validateOnServerBeforeExecuting: false, disableAutoReplication: false, actor.ID, 0, 0, 0, supplementaryDataBoolean: false, null, new InfuseToken(ability.ID, isConsume: false, !infuse, ability.SelectedElements));
		}
	}

	private void NetworkSelectAbility(IAbility ability, bool selected, IOption selectedOption = null)
	{
		if (FFSNetwork.IsOnline && actor != null && actor.IsUnderMyControl())
		{
			Synchronizer.SendGameAction(GameActionType.ChooseAbility, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, actor.ID, 0, 0, 0, supplementaryDataBoolean: false, null, new ChooseAbilityToken(ability, selected, selectedOption));
		}
	}

	public void ShowConsumeAbilities(CPlayerActor actor, List<CAbilityConsume> abilities, Action<CAbility, List<ElementInfusionBoardManager.EElement>> onSelect, Action<CAbility, List<ElementInfusionBoardManager.EElement>> onDeselect)
	{
		this.actor = actor;
		foreach (string item in slots.Keys.ToList())
		{
			slots.Remove(item);
		}
		if (abilities.Any((CAbilityConsume cAbilityConsume) => !cAbilityConsume.CanConsume()))
		{
			return;
		}
		foreach (CAbilityConsume it in abilities)
		{
			int num = it.Consumes.Sum((AbilityConsume consume) => consume.ConsumeData.Trohpies);
			int num2 = it.Consumes.Sum((AbilityConsume consume) => consume.ConsumeData.Resonance);
			List<ElementInfusionBoardManager.EElement> elementConsumes = it.Consumes.SelectMany((AbilityConsume consume) => consume.ConsumeData.Elements).ToList();
			List<ElementInfusionBoardManager.EElement> excludedConsumes = null;
			if (it.ShowPicker && elementConsumes.Count > 1)
			{
				excludedConsumes = (from ElementInfusionBoardManager.EElement e in Enum.GetValues(typeof(ElementInfusionBoardManager.EElement))
					where e != ElementInfusionBoardManager.EElement.Any && !elementConsumes.Contains(e)
					select e).ToList();
				elementConsumes.Clear();
				elementConsumes.Add(ElementInfusionBoardManager.EElement.Any);
			}
			string tooltipTitle = "";
			switch (elementConsumes.Count)
			{
			case 0:
				if (num > 0)
				{
					tooltipTitle = string.Format(LocalizationManager.GetTranslation("FH/GUI_CONSUME_TROPHY"), num);
				}
				else if (num2 > 0)
				{
					tooltipTitle = string.Format(LocalizationManager.GetTranslation("FH/GUI_CONSUME_RESONANCE"), num2);
				}
				break;
			case 1:
				if (num <= 0)
				{
					tooltipTitle = ((num2 <= 0) ? string.Format(LocalizationManager.GetTranslation("GUI_CONSUME_ELEMENT"), elementConsumes[0]) : string.Format(LocalizationManager.GetTranslation("FH/GUI_CONSUME_ELEMENT_AND_RESONANCE"), elementConsumes[0], num2));
				}
				else
				{
					tooltipTitle = string.Format(LocalizationManager.GetTranslation("FH/GUI_CONSUME_ELEMENT_AND_TROPHIES"), elementConsumes[0], num);
				}
				break;
			default:
				if (num <= 0)
				{
					tooltipTitle = ((num2 <= 0) ? string.Format(LocalizationManager.GetTranslation("GUI_CHOOSE_ELEMENTS_CONSUME"), elementConsumes.Count) : string.Format(LocalizationManager.GetTranslation("FH/GUI_CHOOSE_ELEMENTS_AND_RESONANCE_CONSUME"), elementConsumes.Count, num2));
				}
				else
				{
					tooltipTitle = string.Format(LocalizationManager.GetTranslation("FH/GUI_CHOOSE_ELEMENTS_AND_TROPHIES_CONSUME"), elementConsumes.Count, num);
				}
				break;
			}
			string text = null;
			Sprite sprite = null;
			bool hideConsumableElementUI = false;
			StringBuilder stringBuilder = new StringBuilder();
			foreach (AbilityConsume consume in it.Consumes)
			{
				if (!string.IsNullOrEmpty(consume.OverrideIcon))
				{
					text = consume.OverrideIcon;
				}
				if (!string.IsNullOrEmpty(consume.ConsumeData?.PreviewEffectId))
				{
					PreviewEffectInfo previewEffectConfig = UIInfoTools.Instance.GetPreviewEffectConfig(consume.ConsumeData?.PreviewEffectId);
					if (previewEffectConfig != null)
					{
						sprite = previewEffectConfig.previewEffectIcon;
					}
				}
				if (!string.IsNullOrEmpty(consume.Description))
				{
					string value = CreateLayout.LocaliseText(consume.Description);
					stringBuilder.Append(value);
				}
			}
			Ability ability = new Ability(it, it.GetAbilityBaseCard() as CAbilityCard, null, elementConsumes, tooltipTitle, stringBuilder.ToString(), excludedConsumes);
			if ((bool)sprite)
			{
				ability.Icon = sprite;
				hideConsumableElementUI = true;
			}
			else if (!string.IsNullOrEmpty(text))
			{
				ability.Icon = UIInfoTools.Instance.GetActiveAbilityIcon(text);
			}
			else
			{
				if (elementConsumes.Count == 1 && elementConsumes[0] != ElementInfusionBoardManager.EElement.Any)
				{
					ability.Icon = UIInfoTools.Instance.GetElementUseSprite(elementConsumes[0]);
					hideConsumableElementUI = true;
				}
				if (elementConsumes.Count == 0)
				{
					if (num > 0)
					{
						ability.Icon = UIInfoTools.Instance.GetCharacterActiveAbilityIcon(actor.Class.ID, actor?.CharacterClass?.CharacterYML.CustomCharacterConfig);
					}
					else if (num2 > 0)
					{
						ability.Icon = UIInfoTools.Instance.GetCharacterActiveAbilityIcon(actor.Class.ID, actor?.CharacterClass?.CharacterYML.CustomCharacterConfig);
					}
				}
			}
			Create(ability, (IAbility ability2) =>
			{
				NetworkConsumeAbility(ability2, consume: true);
				onSelect?.Invoke(it, ability2.SelectedElements);
			}, (IAbility ability2) =>
			{
				NetworkConsumeAbility(ability2, consume: false);
				onDeselect?.Invoke(it, ability2.SelectedElements);
			}, it.GetMiscVar("PreSelect").GetBool(), isSelected: false, hideConsumableElementUI);
		}
		SaveElementsState();
		OnChangedAbilitiesAvailable.Invoke();
	}

	public void ShowAttackModifierOption(CPlayerActor actor, AttackModifierCard card, Action<AttackModifierCard> onSelect, Action<AttackModifierCard> onDeselect)
	{
		this.actor = actor;
		Hide(clearSelection: true);
		AlternateAttackModifier abilityUI = new AlternateAttackModifier(card, card.YMLData.MathModifier + " -> " + card.YMLData.AlternateMathModifier, LocalizationManager.GetTranslation(card.YMLData.DescriptionForToggleAlternate));
		abilityUI.Icon = UIInfoTools.Instance.GetCharacterActiveAbilityIcon(actor.GetPrefabName(), actor.CharacterClass.CharacterYML.CustomCharacterConfig);
		Create(abilityUI, (IAbility ab) =>
		{
			NetworkSelectAbility(abilityUI, selected: true);
			GameState.AttackModifierCardsSelectedToUseAfterUnfreezingThread.Add(card.YMLData);
			onSelect?.Invoke(card);
		}, (IAbility ab) =>
		{
			NetworkSelectAbility(abilityUI, selected: false);
			GameState.AttackModifierCardsSelectedToUseAfterUnfreezingThread.Remove(card.YMLData);
			onDeselect?.Invoke(card);
		});
		SaveElementsState();
		OnChangedAbilitiesAvailable.Invoke();
	}

	public void ShowAbilityToggle(CActor actor, CAbility ability, Action<CAbility> onSelect, Action<CAbility> onDeselect, string tooltip = null, string tooltipText = null)
	{
		this.actor = ((actor.OriginalType == CActor.EType.Player) ? ((CPlayerActor)actor) : ((CHeroSummonActor)actor).Summoner);
		string prefabName = this.actor.GetPrefabName();
		Hide(clearSelection: true);
		Ability abilityUI = new Ability(ability, ability.GetAbilityBaseCard() as CAbilityCard, null, null, tooltip, tooltipText);
		abilityUI.Icon = UIInfoTools.Instance.GetCharacterActiveAbilityIcon(prefabName);
		abilityUI.OverrideDescription = " ";
		Create(abilityUI, (IAbility ab) =>
		{
			NetworkSelectAbility(abilityUI, selected: true);
			onSelect?.Invoke(ability);
		}, (IAbility ab) =>
		{
			NetworkSelectAbility(abilityUI, selected: false);
			onDeselect?.Invoke(ability);
		});
		SaveElementsState();
		OnChangedAbilitiesAvailable.Invoke();
	}

	public void ShowPerkAbilities(CPlayerActor actor, EPerkFlag trigger, Action<CAbility> onSelect, Action<CAbility> onDeselect)
	{
		this.actor = actor;
		Hide(clearSelection: true);
		foreach (PerksYMLData perk in actor.CharacterClass.Perks)
		{
			if ((perk.Flags & trigger) == 0 || perk.Abilities == null || perk.Abilities.Count == 0)
			{
				continue;
			}
			CAbility cAbility = perk.Abilities[0];
			List<CActiveBonus> list = cAbility.GetAbilityBaseCard()?.ActiveBonuses;
			if (list != null && list.Count > 0 && list[0].IsRestricted(actor))
			{
				continue;
			}
			Ability abilityUI = new Ability(cAbility, cAbility.GetAbilityBaseCard() as CAbilityCard, null, null, LocalizationManager.GetTranslation(perk.Description));
			abilityUI.Icon = UIInfoTools.Instance.GetCharacterActiveAbilityIcon(actor.GetPrefabName(), actor.CharacterClass.CharacterYML.CustomCharacterConfig);
			Create(abilityUI, (IAbility ab) =>
			{
				NetworkSelectAbility(abilityUI, selected: true);
				GameState.PerksSelectedToUseAfterUnfreezingThread.AddRange(perk.Abilities);
				onSelect?.Invoke(perk.Abilities.FirstOrDefault());
			}, (IAbility ab) =>
			{
				NetworkSelectAbility(abilityUI, selected: false);
				foreach (CAbility ability in perk.Abilities)
				{
					GameState.PerksSelectedToUseAfterUnfreezingThread.Remove(ability);
				}
				onDeselect?.Invoke(perk.Abilities.FirstOrDefault());
			});
		}
		SaveElementsState();
		OnChangedAbilitiesAvailable.Invoke();
	}

	public void ShowActiveBonus(CPlayerActor actor, List<CActiveBonus> activeBonuses, Action<CAbility> onSelect, Action<CAbility> onDeselect)
	{
		this.actor = actor;
		Hide(clearSelection: true);
		_ = actor.CharacterClass;
		foreach (CActiveBonus activeBonus in activeBonuses)
		{
			CBaseCard baseCard = activeBonus.BaseCard;
			CAbility ability = activeBonus.Ability;
			if (ability is CAbilityAddActiveBonus cAbilityAddActiveBonus)
			{
				ability = cAbilityAddActiveBonus.AddAbility;
			}
			if (ability != null)
			{
				string translation = LocalizationManager.GetTranslation(baseCard.Name);
				CItem cItem = baseCard as CItem;
				Sprite sprite = null;
				string tooltipText;
				if (cItem != null)
				{
					ItemConfigUI itemConfig = UIInfoTools.Instance.GetItemConfig(cItem.YMLData.Art);
					tooltipText = ((activeBonus.Ability?.ActiveBonusData?.ListLayoutOverride.Item1?.IsNOTNullOrEmpty() != true) ? PreviewEffectGenerator.GeneratePreview(cItem.YMLData.Data) : LocalizationManager.GetTranslation(activeBonus.Ability.ActiveBonusData.ListLayoutOverride.Item1));
					sprite = itemConfig.miniIcon;
				}
				else
				{
					tooltipText = PreviewEffectGenerator.GenerateDescription(ability);
					sprite = UIInfoTools.Instance.GetCharacterActiveAbilityIcon(actor.GetPrefabName(), actor.CharacterClass.CharacterYML.CustomCharacterConfig);
				}
				Ability abilityUI = new Ability(ability, baseCard as CAbilityCard, null, null, translation, tooltipText);
				if (ability.GetMiscVar("DontPreviewDescription").GetBool())
				{
					abilityUI.OverrideDescription = " ";
				}
				else if (ability.GetMiscVar("UseTitleAsOverrideDescription").GetBool())
				{
					abilityUI.OverrideDescription = translation;
				}
				abilityUI.Icon = sprite;
				Create(abilityUI, (IAbility ab) =>
				{
					NetworkSelectAbility(abilityUI, selected: true);
					activeBonus.ToggleActiveBonus(null, actor);
					onSelect?.Invoke(ability);
				}, (IAbility ab) =>
				{
					NetworkSelectAbility(abilityUI, selected: false);
					activeBonus.ToggleActiveBonus(null, actor);
					onDeselect?.Invoke(ability);
				});
			}
		}
		SaveElementsState();
		OnChangedAbilitiesAvailable.Invoke();
	}

	public void ShowLootCards(CPlayerActor actor, List<ScenarioRuleLibrary.LootCard> cards, ScenarioState css, Action<LootCard> onClick)
	{
		this.actor = actor;
		Hide(clearSelection: true);
		foreach (ScenarioRuleLibrary.LootCard card in cards)
		{
			LootCard lootCard = new LootCard(card.Type, CLootDeck.CalcLootAmount(card, css));
			Create(lootCard, (IAbility ab) =>
			{
				NetworkSelectAbility(lootCard, selected: true);
				GameState.LootCardsSelectedAfterUnfreezingThread.Add(card);
				foreach (KeyValuePair<string, UIUseAbility> slot in slots)
				{
					if (slot.Value.element != lootCard)
					{
						slot.Value.Unselect();
					}
				}
				onClick?.Invoke(ab as LootCard);
			}, (IAbility ab) =>
			{
				GameState.LootCardsSelectedAfterUnfreezingThread.Remove(card);
				if (GameState.LootCardsSelectedAfterUnfreezingThread.Count == 0)
				{
					NetworkSelectAbility(lootCard, selected: false);
					onClick?.Invoke(ab as LootCard);
				}
			});
		}
		SaveElementsState();
		OnChangedAbilitiesAvailable.Invoke();
	}

	public void ShowInfuseAbilities(CPlayerActor actor, List<CAbilityInfuse> abilities, Action<CAbility, List<ElementInfusionBoardManager.EElement>> onSelect, Action<CAbility, List<ElementInfusionBoardManager.EElement>> onDeselect)
	{
		this.actor = actor;
		foreach (string item in slots.Keys.ToList())
		{
			slots.Remove(item);
		}
		foreach (CAbilityInfuse it in abilities)
		{
			List<IInfuseElement> infusions = GetInfusions(actor, it);
			Create(new Ability(it, it.GetAbilityBaseCard() as CAbilityCard, infusions, null, (infusions.Count == 1) ? LocalizationManager.GetTranslation("GUI_CHOOSE_ELEMENT_INFUSE") : string.Format(LocalizationManager.GetTranslation("GUI_CHOOSE_ELEMENTS_INFUSE"), infusions.Count)), (IAbility ability) =>
			{
				NetworkInfuseAbility(ability, infuse: true);
				onSelect?.Invoke(it, ability.SelectedElements);
			}, (IAbility ability) =>
			{
				NetworkInfuseAbility(ability, infuse: false);
				onDeselect?.Invoke(it, ability.SelectedElements);
			});
		}
		SaveElementsState();
		OnChangedAbilitiesAvailable.Invoke();
	}

	public void ShowInfusionsAction(CPlayerActor actor, FullAbilityCard card, CAction action, List<InfuseElement> infusions, Action onSelect)
	{
		this.actor = actor;
		foreach (string item in slots.Keys.ToList())
		{
			slots.Remove(item);
		}
		Create(new InfuseAction(action, infusions.Cast<IInfuseElement>().ToList(), card), (IAbility ability) =>
		{
			NetworkInfuseAbility(ability, infuse: true);
			onSelect?.Invoke();
		});
		SaveElementsState();
		OnChangedAbilitiesAvailable.Invoke();
	}

	public void ShowGenericInfusion(CPlayerActor actor, Action<ElementInfusionBoardManager.EElement> onSelect, CAbility ability = null, CBaseAbilityCard card = null, string tooltipTitle = null, string tooltipText = null)
	{
		this.actor = actor;
		foreach (string item in slots.Keys.ToList())
		{
			slots.Remove(item);
		}
		List<IInfuseElement> infusions = new List<IInfuseElement>
		{
			new DummyAnyInfusion()
		};
		IAbility ability2 = ((ability != null) ? ((IAbility)new Ability(ability, card, infusions, null, tooltipTitle, tooltipText)) : ((IAbility)new DummyAbilityInfusion("DummyInfusion" + slots.Count, infusions, tooltipTitle, tooltipText)));
		Create(ability2, (IAbility selectedAbility) =>
		{
			NetworkInfuseAbility(selectedAbility, infuse: true);
			onSelect?.Invoke(infusions[0].SelectedElement);
		});
		SaveElementsState();
		OnChangedAbilitiesAvailable.Invoke();
	}

	public void ShowChooseAbility(CActor actor, CAbilityChooseAbility ability, Action<CAbility> onSelect, Action onDeselect)
	{
		this.actor = actor;
		ChooseAbility abilityUI = new ChooseAbility(actor, ability);
		Create(abilityUI, (IAbility ab) =>
		{
			NetworkSelectAbility(abilityUI, selected: true, abilityUI.SelectedOption);
			onSelect?.Invoke(abilityUI.SelectedAbility);
		}, (IAbility ab) =>
		{
			NetworkSelectAbility(abilityUI, selected: false);
			onDeselect?.Invoke();
		}, preSelect: false, ability.ChosenAbility != null);
		foreach (KeyValuePair<string, UIUseAbility> item in slots.ToList())
		{
			if (item.Value.ability == abilityUI)
			{
				abilityUI.SelectedAbility = ability.ChosenAbility;
				continue;
			}
			item.Value.Hide();
			slots.Remove(item.Key);
		}
		SaveElementsState();
		OnChangedAbilitiesAvailable.Invoke();
		if (ability.GetAbilityBaseCard() is CItem && CPhaseAction.IsFirstAbility(ability))
		{
			slots[abilityUI.ID].Select();
		}
	}

	public void ShowChooseNextModifiers(CActor actor, CAbilitySetNextAttackModifiers ability)
	{
		this.actor = actor;
		int count = ability.NextAttackModifiers.Count;
		for (int i = 0; i < count; i++)
		{
			ChooseAttackModifier chooseAttackModifier = new ChooseAttackModifier(actor, ability, i);
			Create(chooseAttackModifier, null, null, preSelect: false, chooseAttackModifier.SelectedAttackModifier != null);
		}
		foreach (KeyValuePair<string, UIUseAbility> item in slots.ToList())
		{
			if (!(item.Value.ability is ChooseAttackModifier chooseAttackModifier2))
			{
				item.Value.Hide();
				slots.Remove(item.Key);
			}
			else
			{
				item.Value.transform.SetSiblingIndex(chooseAttackModifier2.IndexIn);
			}
			item.Value.ClosePicker();
		}
		OnChangedAbilitiesAvailable.Invoke();
	}

	public void ShowChooseNextModifiers(CActor actor, List<AttackModifierYMLData> modifiers, Action<IOption> OnSelect, AttackModifierYMLData autoSelect, bool forceAutoSelect, bool hidden = false, bool asMultipleOptions = true)
	{
		this.actor = actor;
		if (asMultipleOptions)
		{
			int num = 0;
			foreach (AttackModifierYMLData modifier in modifiers)
			{
				ChooseAttackModifier abilityUI = new ChooseAttackModifier(actor, modifier, num, null);
				Create(abilityUI, (IAbility ab) =>
				{
					NetworkSelectAbility(abilityUI, selected: true, abilityUI.SelectedOption);
					OnSelect?.Invoke(abilityUI.SelectedOption);
				}, (IAbility ab) =>
				{
					NetworkSelectAbility(abilityUI, selected: false);
					OnSelect?.Invoke(null);
				}, preSelect: false, abilityUI.SelectedAttackModifier == modifier);
				num++;
			}
			foreach (KeyValuePair<string, UIUseAbility> item in slots.ToList())
			{
				if (!(item.Value.ability is ChooseAttackModifier chooseAttackModifier))
				{
					item.Value.Hide();
					slots.Remove(item.Key);
				}
				else
				{
					item.Value.transform.SetSiblingIndex(chooseAttackModifier.IndexIn);
					if (((autoSelect != null) | forceAutoSelect) && chooseAttackModifier.SelectedAttackModifier != autoSelect && chooseAttackModifier.ShowSpecificModifier == autoSelect)
					{
						chooseAttackModifier.SelectedAttackModifier = autoSelect;
						OnSelect(chooseAttackModifier.SelectedOption);
						return;
					}
				}
				item.Value.ClosePicker();
				item.Value.gameObject.SetActive(!hidden);
			}
		}
		else
		{
			ChooseAttackModifier abilityUI2 = new ChooseAttackModifier(actor, modifiers, null);
			Create(abilityUI2, (IAbility ab) =>
			{
				NetworkSelectAbility(abilityUI2, selected: true, abilityUI2.SelectedOption);
				OnSelect?.Invoke(abilityUI2.SelectedOption);
			}, (IAbility ab) =>
			{
				NetworkSelectAbility(abilityUI2, selected: false);
				OnSelect?.Invoke(null);
			}, preSelect: false, abilityUI2.SelectedAttackModifier != null);
			foreach (KeyValuePair<string, UIUseAbility> item2 in slots.ToList())
			{
				if (!(item2.Value.ability is ChooseAttackModifier chooseAttackModifier2))
				{
					item2.Value.Hide();
					slots.Remove(item2.Key);
				}
				else
				{
					item2.Value.transform.SetSiblingIndex(chooseAttackModifier2.IndexIn);
					if (((autoSelect != null) | forceAutoSelect) && chooseAttackModifier2.SelectedAttackModifier != autoSelect)
					{
						chooseAttackModifier2.SelectedAttackModifier = autoSelect;
						OnSelect(chooseAttackModifier2.SelectedOption);
						return;
					}
				}
				item2.Value.ClosePicker();
				item2.Value.gameObject.SetActive(!hidden);
			}
		}
		OnChangedAbilitiesAvailable.Invoke();
	}

	private void SaveElementsState()
	{
		if (!slots.Values.Any((UIUseAbility v) => v.ability.Abilities?.FirstOrDefault((CAbility a) => a != null)?.GetMiscVar("SkipInitAbilitySaveInfusionsState").GetBool() == true))
		{
			ElementInfusionBoardManager.SaveState();
			InfusionBoardUI.Instance?.SaveState();
		}
	}

	private List<IInfuseElement> GetInfusions(CPlayerActor actor, CAbilityInfuse ability)
	{
		List<IInfuseElement> list = new List<IInfuseElement>();
		if (ability.ParentBaseCard == null)
		{
			for (int i = 0; i < ability.ElementsToInfuse.Count((ElementInfusionBoardManager.EElement w) => w == ElementInfusionBoardManager.EElement.Any); i++)
			{
				list.Add(new DummyAnyInfusion());
			}
			if (list.Count() == 0 && ability.ElementsToInfuse.Count > 0)
			{
				list.Add(new DummyInfusion(ability.ElementsToInfuse));
			}
		}
		else
		{
			AbilityCardUI card = Singleton<CardsHandManager>.Instance.GetHand(actor).GetCard(ability.ParentBaseCard.ID);
			CBaseCard.ActionType? action = CardsActionControlller.s_Instance.GetAction(card.AbilityCard);
			List<EnhancementButton> source;
			if (action.HasValue)
			{
				source = ((action == CBaseCard.ActionType.TopAction) ? card.fullAbilityCard.topActionButton.EnhancementButtons : card.fullAbilityCard.bottomActionButton.EnhancementButtons);
			}
			else
			{
				source = card.EnhancementButtons;
			}
			foreach (EnhancementButton item in source.Where((EnhancementButton w) => w.EnhancementType == EEnhancement.AnyElement && w.AbilityName == ability.ParentAbility?.Name))
			{
				list.Add(item.InfuseElement);
			}
		}
		return list;
	}

	public void SetAbilitiesInteractable(bool enable)
	{
		foreach (KeyValuePair<string, UIUseAbility> slot in slots)
		{
			slot.Value.SetInteractable(enable);
		}
	}

	public void ProxyInfuseAbility(GameAction gameAction)
	{
		InfuseToken infuseToken = gameAction.SupplementaryDataToken as InfuseToken;
		bool isConsume = infuseToken.IsConsume;
		bool isDeselect = infuseToken.IsDeselect;
		List<ElementInfusionBoardManager.EElement> list = new List<ElementInfusionBoardManager.EElement>(infuseToken.Elements.Length);
		for (int i = 0; i < infuseToken.Elements.Length; i++)
		{
			list.Add((ElementInfusionBoardManager.EElement)infuseToken.Elements[i]);
		}
		ProxyInfuseAbility(infuseToken.AbilityID, isConsume, isDeselect, list);
	}

	public void ProxyInfuseAbility(string abilityId, bool isConsume, bool isDeselect, List<ElementInfusionBoardManager.EElement> selectedElements)
	{
		Log.GUIInfo("ProxyInfuseAbility " + selectedElements.Count + " " + abilityId);
		if (!slots.TryGetValue(abilityId, out var value))
		{
			throw new Exception("Ability id not found: " + abilityId);
		}
		if (value.ability != null)
		{
			value.ability.SelectedElements = selectedElements;
		}
		if (!isDeselect)
		{
			if (!isConsume)
			{
				value.SetInfusions(selectedElements);
			}
			else
			{
				value.SetConsumes(selectedElements);
			}
			value.Select();
		}
		else
		{
			value.Unselect();
		}
	}

	public void ProxyChooseAbility(GameAction gameAction)
	{
		ChooseAbilityToken chooseAbilityToken = gameAction.SupplementaryDataToken as ChooseAbilityToken;
		ChooseAbility(chooseAbilityToken.AbilityID, chooseAbilityToken.Selected, chooseAbilityToken.SelectedOptionID);
	}

	public static bool ReadyToProxyChooseAbility(GameAction gameAction)
	{
		ChooseAbilityToken chooseAbilityToken = gameAction.SupplementaryDataToken as ChooseAbilityToken;
		Dictionary<string, UIUseAbility> dictionary = Singleton<UIUseAbilitiesBar>.Instance?.slots;
		if (dictionary == null)
		{
			return false;
		}
		if (!dictionary.ContainsKey(chooseAbilityToken.AbilityID))
		{
			return false;
		}
		return true;
	}

	public void ChooseAbility(string abilityId, bool select, string optionId = null)
	{
		Log.GUIInfo("ChooseAbility " + select + " " + abilityId);
		if (!slots.ContainsKey(abilityId))
		{
			throw new Exception("Ability id not found: " + abilityId);
		}
		if (select)
		{
			if (optionId != null)
			{
				slots[abilityId].SetSelectedOptions(new List<string> { optionId });
			}
			slots[abilityId].Select();
		}
		else
		{
			slots[abilityId].Unselect();
		}
	}
}
