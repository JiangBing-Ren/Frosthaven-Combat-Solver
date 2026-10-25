using System;
using System.Collections.Generic;
using System.Linq;
using GLOOM;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIUseAugmentation : UIUseSlot<IAugmentation>
{
	[SerializeField]
	private UIUseAugmentationElement augmentationPrefab;

	[SerializeField]
	private List<UIUseAugmentationElement> augmentations;

	[SerializeField]
	private UIUsePreview previewEffect;

	[SerializeField]
	private UIAugmentTooltip tooltip;

	[SerializeField]
	private UIElementPicker elementPicker;

	[SerializeField]
	private UIOptionPicker optionPicker;

	[SerializeField]
	private UIElementHighlight highlight;

	[SerializeField]
	private Color defaultHighlightColor;

	private PickController pickerController;

	private Action<IAugmentation, bool> onHovered;

	private bool hovered;

	private List<UseAugmentationElementController> augmentationElementControllers = new List<UseAugmentationElementController>();

	private UseAugmentationOptionController augmentationOptionController;

	protected virtual void Awake()
	{
		WireEvents();
	}

	protected virtual void WireEvents()
	{
		button.onClick.RemoveListener(OnPointerDown);
		button.onMouseEnter.RemoveListener(OnPointerEnter);
		button.onMouseExit.RemoveListener(OnPointerExit);
		tooltip.gameObject.SetActive(value: false);
		button.onClick.AddListener(OnPointerDown);
		button.onMouseEnter.AddListener(OnPointerEnter);
		button.onMouseExit.AddListener(OnPointerExit);
	}

	public void Init(CActor actor, IAugmentation augmentation, Action<IAugmentation> onSelect = null, Action<IAugmentation> onUnselect = null, Action<IAugmentation, bool> onHovered = null)
	{
		WireEvents();
		hovered = false;
		this.onHovered = onHovered;
		base.Init(actor, augmentation, onSelect, onUnselect, null, isSelected: false, ShouldBeInteractable(defaultInteractable: true, actor));
		previewEffect.gameObject.SetActive(value: true);
		selectAudioItem = augmentation.GetSelectAudioItem();
		if (augmentation?.GetAugmentation() != null && PhaseManager.CurrentPhaseAction?.ActionAugmentationsToConsume != null && PhaseManager.CurrentPhaseAction.ActionAugmentationsToConsume.Contains(augmentation.GetAugmentation()))
		{
			selected = true;
		}
		else
		{
			if (!(augmentation is ConsumeButtonGroupAugmentation consumeButtonGroupAugmentation) || !(pickerController is SingleOptionPickController singleOptionPickController))
			{
				return;
			}
			foreach (CActionAugmentation aug in consumeButtonGroupAugmentation.GetAvailableAugmentations())
			{
				if (PhaseManager.CurrentPhaseAction?.ActionAugmentationsToConsume != null && PhaseManager.CurrentPhaseAction.ActionAugmentationsToConsume.Contains(aug))
				{
					consumeButtonGroupAugmentation.SelectedAugment = singleOptionPickController.Options.FirstOrDefault((IOption x) => x.ID == aug.Name) as AugmentationHolder;
					selected = true;
					break;
				}
			}
		}
	}

	private bool ShouldBeInteractable(bool defaultInteractable, CActor actor = null)
	{
		bool result = defaultInteractable;
		CActionAugmentation cActionAugmentation = element?.GetAugmentation();
		if (cActionAugmentation != null && !selected && !CanConsumeAccountingForReserved(cActionAugmentation))
		{
			result = false;
		}
		if (!(actor ?? base.actor).IsUnderMyControl())
		{
			result = false;
		}
		return result;
	}

	public void RefreshInteractable(bool defaultInteractable)
	{
		SetInteractable(ShouldBeInteractable(defaultInteractable));
	}

	private bool CanConsumeAccountingForReserved(CActionAugmentation augment)
	{
		if (augment == null)
		{
			return true;
		}
		List<ElementInfusionBoardManager.EElement> availableElements = InfusionBoardUI.Instance.GetAvailableElements();
		if (availableElements.Count < augment.Elements.Count)
		{
			return false;
		}
		foreach (ElementInfusionBoardManager.EElement element in augment.Elements)
		{
			if (element != ElementInfusionBoardManager.EElement.Any && !availableElements.Contains(element))
			{
				return false;
			}
		}
		if (augment.Trohpies > 0 && (!(actor is CPlayerActor cPlayerActor) || !cPlayerActor.UsesTrophyTokens() || cPlayerActor.GetCurrentAndReservedTrophyTokens() < augment.Trohpies))
		{
			return false;
		}
		if (augment.Resonance > 0 && (!(actor is CPlayerActor cPlayerActor2) || !cPlayerActor2.UsesResonanceTokens() || cPlayerActor2.GetCurrentAndReservedResonanceTokens() < augment.Resonance))
		{
			return false;
		}
		return true;
	}

	public void Init(CActor actor, ConsumeButton consumeButton, Action<IAugmentation> onSelect = null, Action<IAugmentation> onUnselect = null)
	{
		Init(actor, new ConsumeButtonAugmentation(consumeButton), onSelect, onUnselect, (IAugmentation _, bool hovered) =>
		{
			consumeButton.SetHovered(hovered);
		});
		tooltip.Initialize(consumeButton);
		Decorate(consumeButton.AbilityConsume.ConsumeData);
	}

	public void Init(CActor actor, CActionAugmentation augmentation, Action<IAugmentation> onSelect = null, Action<IAugmentation> onUnselect = null)
	{
		Init(actor, new AugmentationOption(augmentation), onSelect, onUnselect);
		tooltip.Initialize(augmentation);
		Decorate(augmentation);
	}

	public void Init(CActor actor, string groupId, List<CActionAugmentation> augmentations, Action<CActionAugmentation, IAugmentation> onSelect = null, Action<CActionAugmentation, IAugmentation> onUnselect = null)
	{
		List<IOption> list = new List<IOption>();
		for (int i = 0; i < augmentations.Count; i++)
		{
			list.Add(new AugmentationOption(augmentations[i]));
		}
		Decorate(actor, null, list);
		ConsumeButtonGroupAugmentation augmentGroup = new ConsumeButtonGroupAugmentation(groupId, augmentations, augmentationOptionController, previewEffect);
		CActionAugmentation selectedConsume = null;
		Init(actor, augmentGroup, (IAugmentation group) =>
		{
			selectedConsume = augmentGroup.SelectedAugment.Augmentation;
			onSelect(selectedConsume, group);
		}, (IAugmentation group) =>
		{
			onUnselect(selectedConsume, group);
			selectedConsume = null;
		});
		string text = null;
		string text2 = null;
		foreach (CActionAugmentation augmentation in augmentations)
		{
			if (!string.IsNullOrEmpty(augmentation.PreviewEffectId))
			{
				text = ((!LocalizationManager.TryGetTranslation(augmentation.PreviewEffectId, out var translation)) ? augmentation.PreviewEffectId : translation);
			}
			if (!string.IsNullOrEmpty(augmentation.PreviewEffectText))
			{
				text2 = ((!LocalizationManager.TryGetTranslation(augmentation.PreviewEffectText, out var translation2)) ? augmentation.PreviewEffectText : translation2);
			}
		}
		if (text != null && text2 != null)
		{
			tooltip.Initialize(text, text2, translated: true);
		}
		else
		{
			tooltip.Initialize($"UI_AUGMENT_GROUP_TOOLTIP_TITLE_{groupId}", $"UI_AUGMENT_GROUP_TOOLTIP_{groupId}");
		}
	}

	public List<ElementInfusionBoardManager.EElement> GetSelectedElements()
	{
		return (from it in augmentationElementControllers
			where it.SelectedElement.HasValue
			select it.SelectedElement.Value).ToList();
	}

	private void Decorate(CActionAugmentation augmentation)
	{
		previewEffect.SetDescription(augmentation);
		Decorate(actor, augmentation.Elements, null, augmentation);
	}

	private void Decorate(CActor actor, List<ElementInfusionBoardManager.EElement> consumeElements = null, List<IOption> options = null, CActionAugmentation augmentation = null)
	{
		base.actor = actor;
		augmentationOptionController = null;
		augmentationElementControllers.Clear();
		if (consumeElements != null && consumeElements.Count > 0)
		{
			HelperTools.NormalizePool(ref augmentations, augmentationPrefab.gameObject, augmentations[0].transform.parent, consumeElements.Count, null, (UIUseAugmentationElement slot) =>
			{
				slot.Hide();
			});
			for (int num = 0; num < consumeElements.Count; num++)
			{
				Sprite overrideSprite = ((augmentation != null && augmentation.PreviewEffectId.IsNOTNullOrEmpty()) ? GetConsumeIcon(augmentation) : null);
				augmentationElementControllers.Add(new UseAugmentationElementController(augmentations[num], consumeElements[num], overrideSprite));
			}
		}
		else
		{
			HelperTools.NormalizePool(ref augmentations, augmentationPrefab.gameObject, augmentations[0].transform.parent, 1, null, (UIUseAugmentationElement slot) =>
			{
				slot.Hide();
			});
			augmentationOptionController = new UseAugmentationOptionController(augmentations[augmentationElementControllers.Count], GetConsumeIcon(augmentation), options != null && options.Count > 0);
		}
		if (IsSelected())
		{
			foreach (UIUseAugmentationElement augmentation2 in augmentations)
			{
				augmentation2.OnConsumed();
			}
		}
		Highlight();
		CreatePickers(consumeElements, options);
	}

	public Sprite GetConsumeIcon(CActionAugmentation augment)
	{
		if (!string.IsNullOrEmpty(augment?.PreviewEffectId))
		{
			PreviewEffectInfo previewEffectConfig = UIInfoTools.Instance.GetPreviewEffectConfig(augment.PreviewEffectId);
			if (previewEffectConfig != null)
			{
				return previewEffectConfig.previewEffectIcon;
			}
		}
		return UIInfoTools.Instance.GetCharacterActiveAbilityIcon(actor.Class.ID);
	}

	public void ProxySetElements(List<ElementInfusionBoardManager.EElement> consumeElements)
	{
		if (pickerController is SingleElementPickController singleElementPickController)
		{
			singleElementPickController.OnSelected(consumeElements);
		}
		else if (pickerController is MultiElementPickController multiElementPickController)
		{
			multiElementPickController.OnSelectedAll(consumeElements);
		}
		else
		{
			Select();
		}
	}

	private void CreatePickers(List<ElementInfusionBoardManager.EElement> consumeElements, List<IOption> options)
	{
		pickerController = null;
		if (consumeElements != null && consumeElements.Count == 1)
		{
			pickerController = new SingleElementPickController(elementPicker).SetOnSelectedElement((ElementInfusionBoardManager.EElement _) =>
			{
				Highlight();
				Select();
			}).SetOnClosePicker(OnClosePicker).SetOnOpenPicker(OnOpenPicker);
			((SingleElementPickController)pickerController).Setup(augmentationElementControllers[0]);
		}
		else if (consumeElements != null && consumeElements.Count > 1)
		{
			pickerController = new MultiElementPickController(elementPicker).SetOnSelectedAllElements((List<ElementInfusionBoardManager.EElement> _) =>
			{
				Select();
			}).SetOnSelectedElement(OnSelectedElement).SetOnCancelled(() =>
			{
				Highlight();
			})
				.SetExcludedElements(consumeElements)
				.SetOnClosePicker(OnClosePicker)
				.SetOnOpenPicker(OnOpenPicker);
			((MultiElementPickController)pickerController).Setup(augmentationElementControllers.Cast<IElementHolder>().ToList());
		}
		else if (options != null && options.Count > 0)
		{
			pickerController = new SingleOptionPickController(optionPicker).SetOnSelected((IOption _) =>
			{
				Select();
			}).SetOnClosePicker(OnClosePicker).SetOnOpenPicker(OnOpenPicker);
			((SingleOptionPickController)pickerController).Setup(augmentationOptionController, options);
		}
	}

	private void OnSelectedElement(int index, ElementInfusionBoardManager.EElement element)
	{
		Highlight(index);
	}

	private void Highlight(int index = 0)
	{
		if (!gameObject.activeSelf)
		{
			return;
		}
		if (augmentationElementControllers.Count == 0)
		{
			highlight.Highlight(defaultHighlightColor);
			return;
		}
		highlight.Highlight(augmentationElementControllers.Select((UseAugmentationElementController it) => it.SelectedElement.GetValueOrDefault(it.RequiredElement)).ToList(), index);
	}

	private void OnClosePicker()
	{
		if (!(gameObject == null))
		{
			previewEffect.gameObject.SetActive(value: true);
			CheckShowTooltip();
			for (int i = 0; i < augmentations.Count && augmentations[i].gameObject.activeSelf; i++)
			{
				augmentations[i].Focus(focus: true);
			}
			if (Singleton<InputFHManager>.Instance.UsingGamepad)
			{
				EventSystem.current.SetSelectedGameObject(gameObject);
			}
		}
	}

	private void OnOpenPicker()
	{
		previewEffect.gameObject.SetActive(value: false);
		CheckShowTooltip();
		for (int i = 0; i < augmentations.Count && augmentations[i].gameObject.activeSelf; i++)
		{
			augmentations[i].Focus(focus: false);
		}
	}

	public override void Show()
	{
		base.Show();
		for (int i = 0; i < augmentations.Count && augmentations[i].gameObject.activeSelf; i++)
		{
			augmentations[i].Show();
		}
	}

	public override void Unselect(bool forceUnselect = false)
	{
		if (element.CanBeDisactivated())
		{
			base.Unselect(forceUnselect);
		}
	}

	public override void Select()
	{
		if (pickerController != null && pickerController.IsSelecting())
		{
			pickerController.ClosePicker();
		}
		else
		{
			if (pickerController != null && !pickerController.Pick())
			{
				return;
			}
			for (int i = 0; i < augmentations.Count && augmentations[i].gameObject.activeSelf; i++)
			{
				augmentations[i].OnConsumed();
			}
			selected = true;
			foreach (UseAugmentationElementController item in augmentationElementControllers.Where((UseAugmentationElementController it) => it.RequiredElement != ElementInfusionBoardManager.EElement.Any))
			{
				AudioControllerUtils.PlaySound(UIInfoTools.Instance.GetSelectElementAudioItemIcon(item.RequiredElement));
			}
			element.ActiveAugment(GetSelectedElements());
			if (Singleton<InputFHManager>.Instance.UsingGamepad)
			{
				EventSystem.current.SetSelectedGameObject(gameObject);
			}
			base.Select();
		}
	}

	public void OnPointerEnter()
	{
		hovered = true;
		for (int i = 0; i < augmentations.Count && augmentations[i].gameObject.activeSelf; i++)
		{
			augmentations[i].ShowHovered(hovered: true);
		}
		CheckShowTooltip();
		onHovered?.Invoke(element, arg2: true);
	}

	public void OnPointerExit()
	{
		hovered = false;
		if (tooltip.gameObject.activeSelf)
		{
			UIManager.Instance.UnhighlightElement(tooltip.gameObject, unlockUI: false);
			tooltip.gameObject.SetActive(value: false);
		}
		for (int i = 0; i < augmentations.Count && augmentations[i].gameObject.activeSelf; i++)
		{
			augmentations[i].ShowHovered(hovered: false);
		}
		onHovered?.Invoke(element, arg2: false);
	}

	private void CheckShowTooltip()
	{
		if (!gameObject.activeSelf)
		{
			return;
		}
		if (pickerController != null && pickerController.IsSelecting())
		{
			if (tooltip.gameObject.activeSelf)
			{
				UIManager.Instance.UnhighlightElement(tooltip.gameObject, unlockUI: false);
				tooltip.gameObject.SetActive(value: false);
			}
		}
		else if (hovered)
		{
			UIManager.Instance.HighlightElement(tooltip.gameObject, fadeEverythingElse: false, lockUI: false);
			tooltip.gameObject.SetActive(value: true);
		}
	}

	protected override void OnDisable()
	{
		hovered = false;
		OnPointerExit();
	}

	protected void OnDestroy()
	{
		if (!GloomUtility.IsAppQuitting)
		{
			pickerController?.ClosePicker();
		}
	}

	public override void Hide()
	{
		pickerController?.ClosePicker();
		OnPointerExit();
		base.Hide();
	}

	public override void ClearSelection(bool fromClick = false, bool forceClear = false)
	{
		if (selected || forceClear)
		{
			base.ClearSelection(fromClick, forceClear);
			for (int i = 0; i < augmentationElementControllers.Count; i++)
			{
				augmentationElementControllers[i].ClearSelection();
			}
			element.DisactiveAugment();
			augmentationOptionController?.ClearSelection();
			Highlight();
		}
	}

	public void SetConsumes(List<ElementInfusionBoardManager.EElement> elements)
	{
		for (int i = 0; i < augmentationElementControllers.Count; i++)
		{
			if (augmentationElementControllers[i].RequiredElement != ElementInfusionBoardManager.EElement.Any && elements.Contains(augmentationElementControllers[i].RequiredElement))
			{
				augmentationElementControllers[i].SetSelectedElement(augmentationElementControllers[i].RequiredElement);
				elements.Remove(augmentationElementControllers[i].RequiredElement);
			}
		}
		int num = 0;
		for (int j = 0; j < augmentationElementControllers.Count; j++)
		{
			if (num >= elements.Count)
			{
				break;
			}
			if (augmentationElementControllers[j].RequiredElement == ElementInfusionBoardManager.EElement.Any)
			{
				augmentationElementControllers[j].SetSelectedElement(elements[num]);
				num++;
			}
		}
	}

	public void SetOption(string optionId)
	{
		if (pickerController != null && pickerController is SingleOptionPickController)
		{
			((SingleOptionPickController)pickerController).SelectOption(optionId);
		}
	}
}
