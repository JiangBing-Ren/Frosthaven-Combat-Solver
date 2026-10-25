using System;
using System.Collections.Generic;
using FHGUI.Common;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class UIUseAbility : UIUseConsumeInfuseOptionsSlot<IAbility>, IGeneralInfoProvider
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private UIUsePreview previewEffect;

	[SerializeField]
	private UIAbilityTooltip tooltip;

	[NonSerialized]
	public IAbility ability;

	protected override void Awake()
	{
		base.Awake();
		tooltip.gameObject.SetActive(value: false);
	}

	public void Init(CActor player, IAbility ability, Action<IAbility> onSelect = null, Action<IAbility> onUnselect = null, bool isSelected = false)
	{
		this.ability = ability;
		List<IInfuseElement> list = ability.Infusions;
		List<ElementInfusionBoardManager.EElement> list2 = ability.Consumes;
		List<ElementInfusionBoardManager.EElement> excludedConsumes = ability.ExcludedConsumes;
		Tuple<IOptionHolder, List<IOption>> option = ability.GenerateOption(optionsUI[0]);
		Init(player, ability, onSelect, onUnselect, null, isSelected, isInteractable: true, list, list2, option, excludedConsumes);
		Decorate(ability, player);
		Refresh();
	}

	private void Decorate(IAbility ability, CActor player)
	{
		selectAudioItem = ability.GetSelectAudioItem();
		if (!string.IsNullOrEmpty(ability.OverrideDescription))
		{
			previewEffect.SetDescription(ability.OverrideDescription);
		}
		else
		{
			previewEffect.SetDescription(ability.Abilities);
		}
		icon.sprite = ability.Icon;
		tooltip.Initialize(ability, player as CPlayerActor);
	}

	protected override void ShowTooltip(bool show)
	{
		if (ability is ChooseAttackModifier)
		{
			show = false;
		}
		else if (ability is ChooseAbility chooseAbility && (chooseAbility.SelectedAbility == null || chooseAbility.SelectedAbility.GetMiscVar("HideChooseTooltip").GetBool()))
		{
			show = false;
		}
		if (show)
		{
			if (!tooltip.gameObject.activeSelf)
			{
				UIManager.Instance.HighlightElement(tooltip.gameObject, fadeEverythingElse: false, lockUI: false);
				tooltip.Show();
			}
		}
		else if (tooltip.gameObject.activeSelf)
		{
			UIManager.Instance.UnhighlightElement(tooltip.gameObject, unlockUI: false);
			tooltip.Hide();
		}
	}

	protected override void OnDisable()
	{
		OnPointerExit();
		if (ability?.Abilities != null)
		{
			foreach (CAbility ability in ability.Abilities)
			{
				if (ability is CAbilityConsume cAbilityConsume)
				{
					cAbilityConsume.WaitingForUI = false;
				}
			}
		}
		base.OnDisable();
	}

	public IGeneralInfoData GetInfoData()
	{
		if (ability.DescriptionItem != null)
		{
			return UIGeneralInfoUtils.GetItemInfo(ability.DescriptionItem);
		}
		return null;
	}
}
