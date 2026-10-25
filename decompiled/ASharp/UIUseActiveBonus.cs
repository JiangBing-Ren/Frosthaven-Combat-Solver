using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.Common;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class UIUseActiveBonus : UIUseConsumeInfuseOptionsSlot<IActiveBonus>, IGeneralInfoProvider
{
	[Header("Bonus")]
	[SerializeField]
	private Image icon;

	[SerializeField]
	private UIUseActiveBonusTooltip tooltip;

	[SerializeField]
	private GUIAnimator mandatoryHighlight;

	[SerializeField]
	private UIUsePreview previewEffect;

	[SerializeField]
	private UIUseOption initiativeOption;

	[SerializeField]
	private Sprite initiativePickerIcon;

	private Action<IActiveBonus, bool> onHovered;

	private CActiveBonus _bonus;

	protected override void Awake()
	{
		base.Awake();
		OnPointerExit();
	}

	public void SetActiveBonus(CActor actor, CActiveBonus bonus, Action<CActiveBonus, CActor> onSelectBonus = null, Action<CActiveBonus, CActor> onUnselectBonus = null, Func<CActiveBonus, bool> isMandatoryChecker = null, bool isInteractable = true)
	{
		Tuple<IOptionHolder, List<IOption>> tuple = null;
		Action<IActiveBonus, bool> action = null;
		_bonus = bonus;
		for (int i = 0; i < optionsUI.Count; i++)
		{
			optionsUI[i].Hide();
		}
		IActiveBonus bonus2;
		if (bonus is CAdjustInitiativeActiveBonus cAdjustInitiativeActiveBonus && bonus.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.AdjustInitiative))
		{
			InitiativeOptionController item = new InitiativeOptionController(initiativeOption, actor.Initiative());
			tuple = new Tuple<IOptionHolder, List<IOption>>(item, new List<IOption>
			{
				new InitiativeOption(-cAdjustInitiativeActiveBonus.Ability.Strength, actor.Initiative() - cAdjustInitiativeActiveBonus.Ability.Strength),
				new InitiativeOption(cAdjustInitiativeActiveBonus.Ability.Strength, actor.Initiative() + cAdjustInitiativeActiveBonus.Ability.Strength)
			});
			bonus2 = new AdjustInitativeActiveBonus(bonus, actor, item, tuple.Item2);
		}
		else if (bonus is CForgoActionsForCompanionActiveBonus cForgoActionsForCompanionActiveBonus)
		{
			ForgoActiveBonus forgoActiveBonus = new ForgoActiveBonus(cForgoActionsForCompanionActiveBonus, actor, optionsUI.FindAll((UIUseOption it) => it != initiativeOption));
			tuple = new Tuple<IOptionHolder, List<IOption>>(forgoActiveBonus, new List<IOption>
			{
				new AbilityOption(cForgoActionsForCompanionActiveBonus.ForgoActionsForCompanionAbility.ForgoTopActionAbility),
				new AbilityOption(cForgoActionsForCompanionActiveBonus.ForgoActionsForCompanionAbility.ForgoBottomActionAbility)
			});
			action = (IActiveBonus _, bool hovered) =>
			{
				forgoActiveBonus.OnHovered(hovered);
			};
			bonus2 = forgoActiveBonus;
		}
		else if (bonus is CChooseAbilityActiveBonus cChooseAbilityActiveBonus)
		{
			ChooseAbilityOptionController item2 = new ChooseAbilityOptionController(optionsUI.First((UIUseOption it) => it != initiativeOption));
			tuple = new Tuple<IOptionHolder, List<IOption>>(item2, cChooseAbilityActiveBonus.ChooseAbility.ChooseAbilities.ConvertAll((Converter<CAbility, IOption>)((CAbility it) => new AbilityOption(it))));
			bonus2 = new ChooseAbilityActiveBonus(cChooseAbilityActiveBonus, actor, item2, tuple.Item2, this);
		}
		else
		{
			bonus2 = new ActiveBonus(bonus, actor);
		}
		Decorate(bonus);
		SetActiveBonus(actor, bonus2, (IActiveBonus x) =>
		{
			onSelectBonus?.Invoke(bonus, actor);
		}, (IActiveBonus x) =>
		{
			onUnselectBonus?.Invoke(bonus, actor);
		}, action, (IActiveBonus x) => isMandatoryChecker != null && isMandatoryChecker(bonus), tuple, isInteractable);
	}

	protected void SetActiveBonus(CActor actor, IActiveBonus bonus, Action<IActiveBonus> onSelectBonus = null, Action<IActiveBonus> onUnselectBonus = null, Action<IActiveBonus, bool> onHovered = null, Func<IActiveBonus, bool> isMandatoryChecker = null, Tuple<IOptionHolder, List<IOption>> option = null, bool isInteractable = true)
	{
		this.onHovered = onHovered;
		Init(actor, bonus, (IActiveBonus activeBonus) =>
		{
			activeBonus.ToggleActiveBonus((consumes.Count > 0) ? consumes[0].SelectedElement : ((ElementInfusionBoardManager.EElement?)null), fromClick: true);
			onSelectBonus?.Invoke(activeBonus);
		}, (IActiveBonus activeBonus) =>
		{
			onUnselectBonus?.Invoke(activeBonus);
		}, isMandatoryChecker, bonus.IsToggled & isInteractable, isInteractable, null, bonus.GetConsumes(), option);
		Decorate(bonus);
		OnPointerExit();
	}

	protected override void CreateConsumes(List<ElementInfusionBoardManager.EElement> elements, List<ElementInfusionBoardManager.EElement> excludedElements)
	{
		base.CreateConsumes(elements, excludedElements);
		if (element.GetSelectedConsume().HasValue)
		{
			consumes[0].SetSelectedElement(element.GetSelectedConsume());
		}
	}

	public override void OnPointerEnter()
	{
		if (!hovered)
		{
			onHovered?.Invoke(element, arg2: true);
		}
		base.OnPointerEnter();
	}

	public override void OnPointerExit()
	{
		if (hovered)
		{
			onHovered?.Invoke(element, arg2: false);
		}
		base.OnPointerExit();
	}

	public override void ClearSelection(bool fromClick = false, bool forceClear = false)
	{
		if (!element.IsToggleLocked)
		{
			base.ClearSelection(fromClick, forceClear);
			element.UntoggleActiveBonus(fromClick);
		}
	}

	private void Decorate(CActiveBonus bonus)
	{
		tooltip.Init(bonus);
		if (bonus.BaseCard is CItem description)
		{
			previewEffect.SetDescription(description);
		}
		else
		{
			previewEffect.SetDescription(bonus.Ability);
		}
	}

	private void Decorate(IActiveBonus bonus)
	{
		icon.sprite = bonus.GetIcon();
		selectAudioItem = bonus.GetSelectAudioItem();
	}

	protected override void ShowTooltip(bool show)
	{
		if (show)
		{
			tooltip.Show();
		}
		else
		{
			tooltip.Hide();
		}
	}

	public override void Hide()
	{
		base.Hide();
		tooltip.Clear();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		OnPointerExit();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		tooltip.Clear();
	}

	protected override void CancelAnimations()
	{
		base.CancelAnimations();
		mandatoryHighlight.Stop();
	}

	public void Highlight()
	{
		mandatoryHighlight.Play();
	}

	public IGeneralInfoData GetInfoData()
	{
		CActiveBonus bonus = _bonus;
		if (bonus != null && bonus.BaseCard is CItem item)
		{
			return UIGeneralInfoUtils.GetItemInfo(item);
		}
		return null;
	}
}
