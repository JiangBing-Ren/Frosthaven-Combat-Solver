using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.Common;
using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using ScenarioRuleLibrary;
using UnityEngine;

public class UIUseItemScenario : UIUseConsumeInfuseSlot<CItem>, IGeneralInfoProvider
{
	[SerializeField]
	private UIItemElement itemUI;

	[SerializeField]
	private RectTransform tooltipAnchor;

	[SerializeField]
	private UIUseOption optionTick;

	[SerializeField]
	private UIUsePreview previewEffect;

	private Action<CItem, bool> onMouseHover;

	private CItem.EItemSlotState lastState;

	private CItem _item;

	public void SetItem(CItem item, Action<CItem> onSelectItem, Action<CItem> onUnselectItem, CActor iactor, Action<CItem, bool> onMouseHover = null, List<InfuseElement> infusions = null)
	{
		this.onMouseHover = onMouseHover;
		_item = item;
		List<ElementInfusionBoardManager.EElement> list = item.YMLData.Consumes.ToList();
		if (item.YMLData.Data.Abilities != null)
		{
			foreach (ElementInfusionBoardManager.EElement item2 in item.YMLData.Data.Abilities.Where((CAbility it) => it is CAbilityConsumeElement).SelectMany((CAbility it) => (it as CAbilityConsumeElement).ElementsToConsume))
			{
				int num = list.LastIndexOf(item2);
				if (num != -1)
				{
					list.RemoveAt(num);
				}
			}
		}
		if (item.SlotState == CItem.EItemSlotState.Useable && !item.ChosenElement.IsNullOrEmpty())
		{
			InfusionBoardUI.Instance?.MarkElementsDirty();
			item.ChosenElement.Clear();
			selected = false;
		}
		Init(iactor, item, onSelectItem, onUnselectItem, null, item.SlotState == CItem.EItemSlotState.Selected, isInteractable: true, infusions?.Cast<IInfuseElement>().ToList(), list);
		Decorate(item);
		Refresh();
	}

	public bool WasSelected()
	{
		if (lastState != CItem.EItemSlotState.Selected)
		{
			return lastState == CItem.EItemSlotState.Locked;
		}
		return true;
	}

	protected override void Infuse(List<ElementInfusionBoardManager.EElement> infusions, bool active)
	{
		if (active)
		{
			ElementInfusionBoardManager.Infuse(infusions, actor);
		}
		else
		{
			ElementInfusionBoardManager.Infuse(infusions, actor);
		}
		InfusionBoardUI.Instance?.UpdateBoard(infusions);
	}

	protected override void Consume(List<ElementInfusionBoardManager.EElement> elements, bool active)
	{
		if (consumes.Exists((IElementHolder it) => it.RequiredElement == ElementInfusionBoardManager.EElement.Any))
		{
			if (active)
			{
				element.ChosenElement = (from it in consumes
					where it.RequiredElement == ElementInfusionBoardManager.EElement.Any
					select it.SelectedElement.Value).ToList();
				foreach (ElementInfusionBoardManager.EElement item in element.ChosenElement)
				{
					ElementInfusionBoardManager.Consume(item, actor);
				}
			}
			else if (!element.ChosenElement.IsNullOrEmpty())
			{
				element.ChosenElement.Clear();
			}
		}
		base.Consume(elements, active);
	}

	protected override void CreateConsumes(List<ElementInfusionBoardManager.EElement> elements, List<ElementInfusionBoardManager.EElement> excludedElements)
	{
		base.CreateConsumes(elements, excludedElements);
		if (!element.ChosenElement.IsNullOrEmpty())
		{
			for (int i = 0; i < element.ChosenElement.Count; i++)
			{
				if (element.ChosenElement[i] != ElementInfusionBoardManager.EElement.Any)
				{
					consumes[i].SetSelectedElement(element.ChosenElement[i]);
				}
			}
		}
		else if (element.SlotState == CItem.EItemSlotState.Selected)
		{
			for (int j = 0; j < consumes.Count; j++)
			{
				consumes[j].SetSelectedElement(elements[j]);
			}
		}
	}

	private void Decorate(CItem item)
	{
		ItemConfigUI itemConfig = UIInfoTools.Instance.GetItemConfig(item.YMLData.Art);
		selectAudioItem = itemConfig.toggleAudioItem;
		previewEffect.SetDescription(item);
		if (item.YMLData.Data.Abilities != null && item.YMLData.Data.Abilities.Exists((CAbility it) => it.AbilityType == CAbility.EAbilityType.Choose) && (!GameState.ThreadIsSleeping || GameState.ThreadSleepReason != "ItemSelection"))
		{
			optionTick.Clear();
			optionTick.Show();
			CAbilityChooseAbility first = item.YMLData.Data.Abilities.FirstOrDefault((CAbility x) => x is CAbilityChooseAbility) as CAbilityChooseAbility;
			if (PhaseManager.CurrentPhaseAction != null && PhaseManager.CurrentPhaseAction.CurrentAndPreviousPhaseAbilities.FirstOrDefault((CPhaseAction.CPhaseAbility x) => x.m_Ability.ID == first.ID)?.m_Ability is CAbilityChooseAbility { AbilityHasHappened: not false, ChosenAbility: not null } cAbilityChooseAbility)
			{
				if (PreviewEffectGenerator.TryGetAbilityOverrideIcon(cAbilityChooseAbility.ChosenAbility, out var icon))
				{
					optionTick.SetOption(icon);
				}
				else
				{
					optionTick.SetOption(PreviewEffectGenerator.GenerateDescription(cAbilityChooseAbility.ChosenAbility, isForPreview: false, isFromItemPreview: false, isForSelectedOption: true));
				}
			}
		}
		else
		{
			optionTick.Hide();
		}
		ItemDTOUtilities.GetFullItemInfo(_item, out var itemData, out var flipItem);
		if (itemUI != null)
		{
			itemUI.Init(new ItemDataDTO(_item), new FHItemTooltip.ItemTooltipParams(itemData, flipItem), new TooltipPositionParams
			{
				Anchor = TextAnchor.LowerLeft,
				TransformAnchor = tooltipAnchor
			}, manualShowHide: true);
		}
		InteractabilityIsolatedUIControl component = GetComponent<InteractabilityIsolatedUIControl>();
		if (component != null)
		{
			component.ControlIdentifier = item.Slot.ToString();
			component.ControlSecondIdentifier = actor.ActorGuid;
			component.ControlIndex = item.ID;
			component.Initialise();
		}
	}

	public override void Refresh()
	{
		if (element != null)
		{
			lastState = element.SlotState;
			selected = element.SlotState != CItem.EItemSlotState.Useable && element.SlotState != CItem.EItemSlotState.Equipped && element.SlotState != CItem.EItemSlotState.None;
			base.Refresh();
			InfusionBoardUI.Instance?.MarkElementsDirty();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (itemUI != null && itemUI.Tooltip != null)
		{
			itemUI.Tooltip.HideTooltip();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (itemUI != null && itemUI.Tooltip != null)
		{
			itemUI.Tooltip.HideTooltip();
		}
	}

	public override void OnPointerEnter()
	{
		base.OnPointerEnter();
		onMouseHover?.Invoke(element, arg2: true);
	}

	public override void OnPointerExit()
	{
		base.OnPointerExit();
		onMouseHover?.Invoke(element, arg2: false);
	}

	protected override void ShowTooltip(bool show)
	{
		if (!(itemUI == null) && !(itemUI.Tooltip == null))
		{
			if (show)
			{
				itemUI.Tooltip.ShowManually();
			}
			else
			{
				itemUI.Tooltip.HideTooltip();
			}
		}
	}

	public override void OnPointerDown()
	{
		if (element.SlotState != CItem.EItemSlotState.Locked && element.SlotState != CItem.EItemSlotState.Spent && element.SlotState != CItem.EItemSlotState.Consumed)
		{
			base.OnPointerDown();
		}
	}

	public void SetInteractable(bool interactable, bool updateLook = false)
	{
		if (FFSNetwork.IsOnline)
		{
			interactable = interactable && actor.IsUnderMyControl();
		}
		button.interactable = interactable;
		if (updateLook)
		{
			base.SetInteractable(interactable);
		}
	}

	public IGeneralInfoData GetInfoData()
	{
		if (_item == null)
		{
			return null;
		}
		return UIGeneralInfoUtils.GetItemInfo(_item);
	}
}
