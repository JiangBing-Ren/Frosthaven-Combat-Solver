using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using UnityEngine;

public class ForgoActiveBonus : BaseActiveBonus<CForgoActionsForCompanionActiveBonus>, IOptionHolder
{
	private bool hovered;

	private bool hoveredOptionTop;

	private bool hoveredOptionBottom;

	private List<IOption> selectedOptions;

	private List<UIUseOption> optionsUI;

	public override bool IsToggled => false;

	public List<IOption> SelectedOptions
	{
		get
		{
			return selectedOptions;
		}
		set
		{
			selectedOptions = value;
			IOption option = selectedOptions.FirstOrDefault((IOption it) => it.ID == bonus.ForgoActionsForCompanionAbility.ForgoBottomActionAbility.Name);
			if (option != null != bonus.BottomActionToggled)
			{
				bonus.BottomActionToggled = option != null;
				if (option == null)
				{
					optionsUI[0].Clear();
				}
				else
				{
					optionsUI[0].SetOption(option.GetSelectedText());
				}
			}
			IOption option2 = selectedOptions.FirstOrDefault((IOption it) => it.ID == bonus.ForgoActionsForCompanionAbility.ForgoTopActionAbility.Name);
			if (option2 != null != bonus.TopActionToggled)
			{
				bonus.TopActionToggled = option2 != null;
				if (option2 == null)
				{
					optionsUI[1].Clear();
				}
				else
				{
					optionsUI[1].SetOption(option2.GetSelectedText());
				}
			}
			RefreshHovered();
		}
	}

	public ForgoActiveBonus(CForgoActionsForCompanionActiveBonus bonus, CActor actor, List<UIUseOption> optionsUI)
		: base(bonus, actor)
	{
		this.optionsUI = optionsUI;
		hovered = false;
		hoveredOptionBottom = (hoveredOptionTop = false);
		for (int i = 0; i < 2; i++)
		{
			optionsUI[i].Show();
			optionsUI[i].Clear();
		}
		for (int j = 2; j < optionsUI.Count; j++)
		{
			optionsUI[j].Hide();
		}
		SelectedOptions = new List<IOption>();
	}

	public override Sprite GetIcon()
	{
		if (actor is CPlayerActor)
		{
			CPlayerActor cPlayerActor = actor as CPlayerActor;
			return UIInfoTools.Instance.GetCharacterActiveAbilityIcon(actor.GetPrefabName(), cPlayerActor?.CharacterClass?.CharacterYML.CustomCharacterConfig);
		}
		return UIInfoTools.Instance.GetCharacterActiveAbilityIcon(actor.GetPrefabName());
	}

	public override void UntoggleActiveBonus(bool fromClick = false)
	{
	}

	public override void ToggleActiveBonus(ElementInfusionBoardManager.EElement? eElement, bool fromClick = false)
	{
	}

	public void OnHovered(bool hovered)
	{
		this.hovered = hovered;
		RefreshHovered();
	}

	public void OnHoveredOption(IOption option, bool optioHovered)
	{
		if (option.ID == bonus.ForgoActionsForCompanionAbility.ForgoTopActionAbility.Name)
		{
			hoveredOptionTop = optioHovered;
		}
		else if (option.ID == bonus.ForgoActionsForCompanionAbility.ForgoBottomActionAbility.Name)
		{
			hoveredOptionBottom = optioHovered;
		}
		RefreshHovered();
	}

	private void RefreshHovered()
	{
		if (actor is CPlayerActor playerActor && (hovered || bonus.BottomActionToggled || bonus.TopActionToggled || hoveredOptionBottom || hoveredOptionTop))
		{
			Singleton<CardsHandManager>.Instance.Show(playerActor, CardHandMode.ActionSelection, CardPileType.Round, CardPileType.None, 0, fadeUnselectableCards: false, highlightSelectableCards: false, allowFullCardPreview: true, CardsHandUI.CardActionsCommand.NONE, forceUseCurrentRoundCards: false, allowFullDeckPreview: true, (AbilityCardUI cardUI) =>
			{
				if (cardUI.gameObject.activeSelf)
				{
					cardUI.fullAbilityCard.ToggleSelect(bonus.TopActionToggled || hoveredOptionTop, CBaseCard.ActionType.TopAction);
					cardUI.fullAbilityCard.ToggleSelect(bonus.BottomActionToggled || hoveredOptionBottom, CBaseCard.ActionType.BottomAction);
				}
			});
		}
		else
		{
			Singleton<CardsHandManager>.Instance.Hide();
		}
	}

	public void ClearSelection()
	{
		selectedOptions.Clear();
	}
}
