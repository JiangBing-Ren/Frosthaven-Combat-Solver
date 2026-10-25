using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;

public class ChooseAttackModifier : ISingleOptionHolder, IAbility
{
	private CActor actor;

	private CAbilitySetNextAttackModifiers Ability;

	private List<IOption> options;

	private IOption selectedOption;

	private UIUseOption optionUI;

	private List<AttackModifierYMLData> ChoosableModifiers;

	private Action<IAbility, IOption> OnSelected;

	public int IndexIn { get; private set; }

	public AttackModifierYMLData ShowSpecificModifier { get; private set; }

	public List<IInfuseElement> Infusions => null;

	public List<ElementInfusionBoardManager.EElement> Consumes => null;

	public List<ElementInfusionBoardManager.EElement> ExcludedConsumes => null;

	public List<ElementInfusionBoardManager.EElement> SelectedElements { get; set; }

	public List<CAbility> Abilities => new List<CAbility>();

	public Sprite Icon
	{
		get
		{
			AttackModifierYMLData showSpecificModifier = ShowSpecificModifier;
			if (showSpecificModifier != null)
			{
				return UIInfoTools.Instance.GetPremadeAttackModifierIcon(showSpecificModifier);
			}
			return UIInfoTools.Instance.GetCharacterActiveAbilityIcon(actor.GetPrefabName(), actor.CharacterClass?.CharacterYML.CustomCharacterConfig);
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public string ID
	{
		get
		{
			if (Ability == null)
			{
				if (ShowSpecificModifier != null)
				{
					return $"AdvantageChoice_{IndexIn}";
				}
				return "AdvantageChoice";
			}
			return $"{Ability.Name}_{IndexIn}";
		}
	}

	public CItem DescriptionItem => null;

	public string DescriptionTitle => null;

	public string DescriptionText => null;

	public override IOption SelectedOption
	{
		get
		{
			return selectedOption;
		}
		set
		{
			if (selectedOption == value)
			{
				return;
			}
			selectedOption = value;
			if (selectedOption == null)
			{
				optionUI.Clear();
				if (Ability == null)
				{
					GameState.ChooseAttackModifier(null);
				}
				else
				{
					Ability.SetNextModifier(null, IndexIn);
				}
				OnSelected?.Invoke(this, selectedOption);
			}
			else
			{
				int num = options.IndexOf(selectedOption);
				AttackModifierYMLData attackModifierYMLData = null;
				if (num >= 0 && num < ChoosableModifiers.Count)
				{
					attackModifierYMLData = ChoosableModifiers[num];
				}
				optionUI.SetOption(selectedOption.GetPickerText());
				if (Ability == null)
				{
					GameState.ChooseAttackModifier(attackModifierYMLData);
				}
				else
				{
					Ability.SetNextModifier(attackModifierYMLData, IndexIn);
				}
				OnSelected?.Invoke(this, selectedOption);
			}
			if (FFSNetwork.IsOnline && actor != null && actor.IsUnderMyControl())
			{
				Synchronizer.SendGameAction(GameActionType.ChooseAbility, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, actor.ID, 0, 0, 0, supplementaryDataBoolean: false, null, new ChooseAbilityToken(this, selectedOption != null, selectedOption));
			}
		}
	}

	public AttackModifierYMLData SelectedAttackModifier
	{
		get
		{
			if (SelectedOption != null)
			{
				return ChoosableModifiers[options.IndexOf(selectedOption)];
			}
			return null;
		}
		set
		{
			IOption option = options.FirstOrDefault((IOption x) => x is AttackModifierOption attackModifierOption && attackModifierOption.ability == value);
			if (option != null)
			{
				SelectedOption = option;
			}
			else
			{
				SelectedOption = null;
			}
		}
	}

	public string OverrideDescription { get; set; }

	public ChooseAttackModifier(CActor actor, CAbilitySetNextAttackModifiers ability, int indexIn)
	{
		Ability = ability;
		options = new List<IOption>();
		this.actor = actor;
		IndexIn = indexIn;
		OnSelected = null;
		selectedOption = null;
		ChoosableModifiers = Ability.GetRemainingModifiersToChoose().ToList();
		AttackModifierYMLData attackModifierYMLData = ability.NewNextAttackModifiers[IndexIn];
		if (attackModifierYMLData != null)
		{
			ChoosableModifiers.Add(attackModifierYMLData);
		}
		foreach (AttackModifierYMLData choosableModifier in ChoosableModifiers)
		{
			options.Add(new AttackModifierOption(choosableModifier));
		}
		if (attackModifierYMLData != null)
		{
			selectedOption = options[options.Count - 1];
		}
		SelectedElements = new List<ElementInfusionBoardManager.EElement>();
	}

	public ChooseAttackModifier(CActor actor, List<AttackModifierYMLData> modifiers, Action<IAbility, IOption> onSelected)
	{
		Ability = null;
		options = new List<IOption>();
		this.actor = actor;
		IndexIn = 0;
		OnSelected = onSelected;
		selectedOption = null;
		ChoosableModifiers = modifiers.ToList();
		AttackModifierYMLData selMod = GameState.AttackModifierCardsSelectedToUseAfterUnfreezingThread?.FirstOrDefault();
		foreach (AttackModifierYMLData choosableModifier in ChoosableModifiers)
		{
			options.Add(new AttackModifierOption(choosableModifier));
		}
		if (selMod != null)
		{
			selectedOption = options.FirstOrDefault((IOption x) => x is AttackModifierOption attackModifierOption && attackModifierOption.ability == selMod);
		}
		SelectedElements = new List<ElementInfusionBoardManager.EElement>();
	}

	public ChooseAttackModifier(CActor actor, AttackModifierYMLData specificModifier, int indexIn, Action<IAbility, IOption> onSelected)
	{
		Ability = null;
		options = new List<IOption>();
		this.actor = actor;
		OnSelected = onSelected;
		selectedOption = null;
		ChoosableModifiers = new List<AttackModifierYMLData> { specificModifier };
		ShowSpecificModifier = specificModifier;
		IndexIn = indexIn;
		AttackModifierYMLData selMod = GameState.AttackModifierCardsSelectedToUseAfterUnfreezingThread?.FirstOrDefault();
		foreach (AttackModifierYMLData choosableModifier in ChoosableModifiers)
		{
			options.Add(new AttackModifierOption(choosableModifier));
		}
		if (selMod != null)
		{
			selectedOption = options.FirstOrDefault((IOption x) => x is AttackModifierOption attackModifierOption && attackModifierOption.ability == selMod);
		}
		SelectedElements = new List<ElementInfusionBoardManager.EElement>();
	}

	public Tuple<IOptionHolder, List<IOption>> GenerateOption(UIUseOption optionUI)
	{
		this.optionUI = optionUI;
		optionUI.Clear();
		if (ShowSpecificModifier == null)
		{
			optionUI.Show();
		}
		else
		{
			optionUI.Hide();
		}
		if (selectedOption != null)
		{
			optionUI.SetOption(selectedOption.GetPickerIcon());
		}
		else
		{
			optionUI.Clear();
		}
		return new Tuple<IOptionHolder, List<IOption>>(this, options);
	}

	public string GetSelectAudioItem()
	{
		if (Ability?.GetAbilityBaseCard() is CItem cItem)
		{
			return UIInfoTools.Instance.GetItemConfig(cItem.YMLData.Art).toggleAudioItem;
		}
		return UIInfoTools.Instance.toggleUseCharacterSlotAudioItem;
	}
}
