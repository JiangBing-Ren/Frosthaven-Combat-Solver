using System;
using System.Collections.Generic;
using System.Linq;
using GLOOM;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;

public class ChooseAbility : ISingleOptionHolder, IAbility
{
	private CActor actor;

	private CAbilityChooseAbility ability;

	public List<IOption> options;

	private IOption selectedOption;

	private UIUseOption optionUI;

	public List<IInfuseElement> Infusions => null;

	public List<ElementInfusionBoardManager.EElement> Consumes => null;

	public List<ElementInfusionBoardManager.EElement> ExcludedConsumes => null;

	public List<ElementInfusionBoardManager.EElement> SelectedElements { get; set; }

	public List<CAbility> Abilities => new List<CAbility> { ability };

	public Sprite Icon
	{
		get
		{
			string text = ability.GetMiscVar("OverrideIcon").GetString();
			if (text.IsNOTNullOrEmpty())
			{
				return UIInfoTools.Instance.GetActiveAbilityIcon(text);
			}
			if (ability.GetAbilityBaseCard() is CItem cItem)
			{
				return UIInfoTools.Instance.GetItemMiniSprite(cItem.YMLData.Art);
			}
			return UIInfoTools.Instance.GetCharacterActiveAbilityIcon(actor.GetPrefabName(), actor.CharacterClass?.CharacterYML.CustomCharacterConfig);
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public string ID => ability.Name;

	public CItem DescriptionItem
	{
		get
		{
			if (ability.GetAbilityBaseCard() is CItem result)
			{
				return result;
			}
			return null;
		}
	}

	public string DescriptionTitle
	{
		get
		{
			if (string.IsNullOrEmpty(ability.PerkID))
			{
				if (!(ability.GetAbilityBaseCard() is CItem) && !(ability.GetAbilityBaseCard() is AttackModifierCard))
				{
					if (!(ability?.GetAbilityBaseCard()?.Name == "DummyCardID"))
					{
						CAbilityChooseAbility cAbilityChooseAbility = ability;
						if (cAbilityChooseAbility == null || cAbilityChooseAbility.GetAbilityBaseCard()?.CardType != CBaseCard.ECardType.ScenarioModifier)
						{
							return LocalizationManager.GetTranslation((ability.GetAbilityBaseCard() != null) ? ability.GetAbilityBaseCard().Name : ability.ParentAbilityBaseCard.Name);
						}
					}
					return PreviewEffectGenerator.GenerateDescription(ability);
				}
				return null;
			}
			return LocalizationManager.GetTranslation(ability.perkData.Description);
		}
	}

	public string DescriptionText
	{
		get
		{
			if (ability.GetAbilityBaseCard() is CItem)
			{
				return null;
			}
			CActiveBonus cActiveBonus = null;
			cActiveBonus = ((ability.GetAbilityBaseCard() == null) ? ability.ParentAbilityBaseCard.ActiveBonuses.FirstOrDefault((CActiveBonus it) => it is CStartTurnAbilityActiveBonus cStartTurnAbilityActiveBonus && cStartTurnAbilityActiveBonus.AddAbility == ability) : ability.GetAbilityBaseCard().ActiveBonuses.FirstOrDefault((CActiveBonus it) => it is CStartTurnAbilityActiveBonus cStartTurnAbilityActiveBonus && cStartTurnAbilityActiveBonus.AddAbility == ability));
			if (cActiveBonus?.Layout?.ListLayouts != null)
			{
				return CreateLayout.LocaliseText(cActiveBonus.Layout.ListLayouts.FirstOrDefault());
			}
			return null;
		}
	}

	public override IOption SelectedOption
	{
		get
		{
			return selectedOption;
		}
		set
		{
			selectedOption = value;
			if (selectedOption == null)
			{
				optionUI.Clear();
				ability.ResetChosenAbility();
				return;
			}
			int num = options.IndexOf(selectedOption);
			if (PreviewEffectGenerator.TryGetAbilityOverrideIcon(ability.ApplicableAbilities[num], out var icon))
			{
				optionUI.SetOption(icon);
			}
			else
			{
				optionUI.SetOption(PreviewEffectGenerator.GenerateDescription(ability.ApplicableAbilities[num], isForPreview: false, isFromItemPreview: false, isForSelectedOption: true));
			}
			ability.AbilityChosen(num);
		}
	}

	public CAbility SelectedAbility
	{
		get
		{
			if (SelectedOption != null)
			{
				return ability.ApplicableAbilities[options.IndexOf(SelectedOption)];
			}
			return null;
		}
		set
		{
			if (value != null)
			{
				int num = ability.ApplicableAbilities.IndexOf(value);
				if (num != -1)
				{
					SelectedOption = options[num];
					return;
				}
			}
			SelectedOption = null;
		}
	}

	public string OverrideDescription { get; set; }

	public ChooseAbility(CActor actor, CAbilityChooseAbility ability)
	{
		this.ability = ability;
		options = new List<IOption>();
		this.actor = actor;
		foreach (CAbility applicableAbility in ability.ApplicableAbilities)
		{
			if (applicableAbility.MeetsStartAbilityRequirements(actor))
			{
				options.Add(new AbilityOption(applicableAbility));
			}
		}
		SelectedElements = new List<ElementInfusionBoardManager.EElement>();
	}

	public CAbility GetSelectedAbility()
	{
		if (selectedOption == null)
		{
			return null;
		}
		int index = options.IndexOf(selectedOption);
		return ability.ApplicableAbilities[index];
	}

	public Tuple<IOptionHolder, List<IOption>> GenerateOption(UIUseOption optionUI)
	{
		this.optionUI = optionUI;
		optionUI.Clear();
		optionUI.Show();
		return new Tuple<IOptionHolder, List<IOption>>(this, options);
	}

	public string GetSelectAudioItem()
	{
		if (ability.GetAbilityBaseCard() is CItem cItem)
		{
			return UIInfoTools.Instance.GetItemConfig(cItem.YMLData.Art).toggleAudioItem;
		}
		return UIInfoTools.Instance.toggleUseCharacterSlotAudioItem;
	}
}
