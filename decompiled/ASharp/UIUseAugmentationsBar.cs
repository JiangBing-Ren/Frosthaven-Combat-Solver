using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.Events;

public class UIUseAugmentationsBar : Singleton<UIUseAugmentationsBar>
{
	public struct ConsumeAugmentationParam
	{
		public CActionAugmentation Augmentation;

		public ConsumeButton ConsumeButton;
	}

	public struct ConsumeGroupParam
	{
		public string ConsumeGroupKey;

		public List<CActionAugmentation> Augmentations;
	}

	[SerializeField]
	private RectTransform container;

	[SerializeField]
	private UIUseAugmentation slotPrefab;

	[SerializeField]
	private UnityEvent OnChangedAugmentsAvailable;

	private Dictionary<CActionAugmentation, UIUseAugmentation> augmentSlots = new Dictionary<CActionAugmentation, UIUseAugmentation>();

	private Dictionary<string, UIUseAugmentation> augmentGroupSlots = new Dictionary<string, UIUseAugmentation>();

	private Stack<UIUseAugmentation> pool = new Stack<UIUseAugmentation>();

	private CActor actor;

	private CAbility ability;

	private List<ConsumeAugmentationParam> _normalConsumes = new List<ConsumeAugmentationParam>();

	private Transform lastCreatedSlot;

	public bool IsShown { get; private set; }

	public bool HasShownUsableSlots()
	{
		if (!IsShown)
		{
			return false;
		}
		if (!augmentSlots.Any((KeyValuePair<CActionAugmentation, UIUseAugmentation> s) => s.Value.gameObject.activeSelf))
		{
			return augmentGroupSlots.Any((KeyValuePair<string, UIUseAugmentation> s) => s.Value.gameObject.activeSelf);
		}
		return true;
	}

	public IEnumerable<ElementInfusionBoardManager.EElement> GetAllConsumes()
	{
		if (!IsShown)
		{
			yield break;
		}
		List<UIUseAugmentation> source = ((augmentSlots.Count > 0) ? augmentSlots.Values.ToList() : pool.ToList());
		foreach (UIUseAugmentation item in source.Where((UIUseAugmentation x) => x.IsSelected()))
		{
			foreach (ElementInfusionBoardManager.EElement selectedElement in item.GetSelectedElements())
			{
				yield return selectedElement;
			}
		}
	}

	private void Create(ConsumeAugmentationParam param)
	{
		ConsumeButton button = param.ConsumeButton;
		bool flag = button != null;
		bool flag2 = CanConsume(param.Augmentation);
		bool flag3 = GameState.CurrentActionSelectedAugmentations?.Contains(param.Augmentation) ?? false;
		if (flag)
		{
			button.Enable();
			button.SetAvailable(flag2);
		}
		if (!flag2 && !flag3)
		{
			return;
		}
		CActionAugmentation augmentation = param.Augmentation;
		if (!augmentSlots.ContainsKey(augmentation))
		{
			augmentSlots[augmentation] = GetSlotFromPool();
			if (flag)
			{
				augmentSlots[augmentation].Init(actor, button, (IAugmentation _) =>
				{
					OnActivatedConsumeButton(button);
				}, (IAugmentation _) =>
				{
					OnDeactivatedConsumeButton(button);
				});
			}
			else
			{
				augmentSlots[augmentation].Init(actor, param.Augmentation, (IAugmentation _) =>
				{
					OnActivatedAugmentNoButton(param.Augmentation);
				}, (IAugmentation _) =>
				{
					OnDeactivatedAugmentNoButton(param.Augmentation);
				});
			}
			augmentSlots[augmentation].Show();
		}
		OnChangedAugmentsAvailable.Invoke();
	}

	private void Create(string consumeGroup, List<CActionAugmentation> augmentations)
	{
		if (!augmentGroupSlots.ContainsKey(consumeGroup))
		{
			augmentGroupSlots[consumeGroup] = GetSlotFromPool();
			augmentGroupSlots[consumeGroup].Init(actor, consumeGroup, augmentations, OnActivatedAugmentGroup, OnDeactivatedAugmentGroup);
			augmentGroupSlots[consumeGroup].Show();
		}
		OnChangedAugmentsAvailable.Invoke();
	}

	public void SetInteractionAvailableSlots(bool interactable, bool onlyChangeUnselected = false, bool checkForSingleConsume = false)
	{
		foreach (KeyValuePair<string, UIUseAugmentation> augmentGroupSlot in augmentGroupSlots)
		{
			if (!onlyChangeUnselected || !augmentGroupSlot.Value.IsSelected())
			{
				augmentGroupSlot.Value.SetInteractable(interactable);
			}
		}
		foreach (KeyValuePair<CActionAugmentation, UIUseAugmentation> augmentSlot in augmentSlots)
		{
			if (!onlyChangeUnselected || !augmentSlot.Value.IsSelected())
			{
				augmentSlot.Value.RefreshInteractable(interactable);
			}
		}
		if (!checkForSingleConsume || !augmentSlots.Any((KeyValuePair<CActionAugmentation, UIUseAugmentation> pair) => pair.Key.ConsumeOnlyOneElement && pair.Value.IsSelected()))
		{
			return;
		}
		foreach (KeyValuePair<CActionAugmentation, UIUseAugmentation> augmentSlot2 in augmentSlots)
		{
			if (!augmentSlot2.Value.IsSelected() && augmentSlot2.Key.ConsumeOnlyOneElement)
			{
				augmentSlot2.Value.SetInteractable(interactable: false);
			}
		}
	}

	private void OnDeactivatedConsumeButton(ConsumeButton consume)
	{
		UIEventManager.AddUIEvent(new UIEvent(consume.IsAbilityCardTopButton ? UIEvent.EUIEventType.ConsumeElementCardTop : UIEvent.EUIEventType.ConsumeElementCardBottom, null, consume.ID, consume.ConsumeButtonIndex));
		consume.ClearSelection();
		TryNetworkConsumeButtonToggle(consume, toggleOn: false);
	}

	private void OnActivatedConsumeButton(ConsumeButton consume)
	{
		UIEventManager.AddUIEvent(new UIEvent(consume.IsAbilityCardTopButton ? UIEvent.EUIEventType.ConsumeElementCardTop : UIEvent.EUIEventType.ConsumeElementCardBottom, null, consume.ID, consume.ConsumeButtonIndex));
		TryNetworkConsumeButtonToggle(consume, toggleOn: true);
	}

	private void OnDeactivatedAugmentGroup(CActionAugmentation augment, IAugmentation augmentationGroup)
	{
		TryNetworkAugmentationGroupToggle(augment, augmentationGroup, toggleOn: false);
	}

	private void OnActivatedAugmentGroup(CActionAugmentation augment, IAugmentation augmentationGroup)
	{
		TryNetworkAugmentationGroupToggle(augment, augmentationGroup, toggleOn: true);
	}

	private void OnActivatedAugmentNoButton(CActionAugmentation augment)
	{
		TryNetworkAbilityAugmentationNoButtonToggle(augment, toggleOn: true);
	}

	private void OnDeactivatedAugmentNoButton(CActionAugmentation augment)
	{
		TryNetworkAbilityAugmentationNoButtonToggle(augment, toggleOn: false);
	}

	private bool CanConsume(CActionAugmentation augment)
	{
		List<ElementInfusionBoardManager.EElement> list = InfusionBoardUI.Instance?.GetAvailableElements();
		if (list.Count < augment.Elements.Count)
		{
			return false;
		}
		foreach (ElementInfusionBoardManager.EElement element in augment.Elements)
		{
			if (element != ElementInfusionBoardManager.EElement.Any && !list.Contains(element))
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
		if (augment.CostAbility != null && ability.AbilityType == CAbility.EAbilityType.Attack && ability.StoppedByDisarm())
		{
			return false;
		}
		return true;
	}

	private void RefreshAvailableElements()
	{
		List<ElementInfusionBoardManager.EElement> availableElements = augmentSlots.Values.SelectMany((UIUseAugmentation it) => ElementInfusionBoardManager.Elements).Distinct().ToList();
		InfusionBoardUI.Instance?.SetAvailableElements(availableElements);
		foreach (KeyValuePair<CActionAugmentation, UIUseAugmentation> augmentSlot in augmentSlots)
		{
			augmentSlot.Value.RefreshInteractable(defaultInteractable: true);
		}
	}

	private void OnUnreservedElement(List<ElementInfusionBoardManager.EElement> elements)
	{
		for (int i = 0; i < _normalConsumes.Count; i++)
		{
			if (!augmentSlots.ContainsKey(_normalConsumes[i].Augmentation) && CanConsume(_normalConsumes[i].Augmentation))
			{
				Create(_normalConsumes[i]);
			}
		}
		RefreshAvailableElements();
	}

	private void OnReservedElement(List<ElementInfusionBoardManager.EElement> elements)
	{
		foreach (KeyValuePair<CActionAugmentation, UIUseAugmentation> item in augmentSlots.ToList())
		{
			if (!item.Value.IsSelected() && !CanConsume(item.Key))
			{
				Remove(item.Key, undoSelection: true);
			}
		}
		RefreshAvailableElements();
	}

	private void Remove(CActionAugmentation augmentation, bool undoSelection)
	{
		ConsumeButton consumeButton = _normalConsumes.FirstOrDefault((ConsumeAugmentationParam c) => c.Augmentation == augmentation).ConsumeButton;
		if (consumeButton != null)
		{
			consumeButton.Disable();
			consumeButton.ResetState(undoSelection);
		}
		if (augmentSlots.ContainsKey(augmentation))
		{
			if (undoSelection)
			{
				augmentSlots[augmentation].ClearSelection();
			}
			augmentSlots[augmentation].Hide();
			pool.Push(augmentSlots[augmentation]);
			augmentSlots.Remove(augmentation);
			OnChangedAugmentsAvailable.Invoke();
		}
	}

	private void Remove(string groupId, bool undoSelection = false)
	{
		if (augmentGroupSlots.ContainsKey(groupId))
		{
			UIUseAugmentation uIUseAugmentation = augmentGroupSlots[groupId];
			if (undoSelection)
			{
				uIUseAugmentation.ClearSelection();
			}
			uIUseAugmentation.Hide();
			pool.Push(uIUseAugmentation);
			augmentGroupSlots.Remove(groupId);
			OnChangedAugmentsAvailable.Invoke();
		}
	}

	private UIUseAugmentation GetSlotFromPool()
	{
		UIUseAugmentation uIUseAugmentation;
		if (pool.Count > 0)
		{
			uIUseAugmentation = pool.Pop();
			uIUseAugmentation.gameObject.SetActive(value: true);
			return uIUseAugmentation;
		}
		uIUseAugmentation = UnityEngine.Object.Instantiate(slotPrefab, container);
		if (lastCreatedSlot != null)
		{
			uIUseAugmentation.transform.SetSiblingIndex(lastCreatedSlot.GetSiblingIndex() + 1);
		}
		lastCreatedSlot = uIUseAugmentation.transform;
		return uIUseAugmentation;
	}

	public void Hide(bool clearSelection = false)
	{
		IsShown = false;
		InfusionBoardUI.Instance?.OnReservedElement.RemoveListener(OnReservedElement);
		InfusionBoardUI.Instance?.OnUnreserveElement.RemoveListener(OnUnreservedElement);
		foreach (KeyValuePair<CActionAugmentation, UIUseAugmentation> augmentSlot in augmentSlots)
		{
			if (clearSelection)
			{
				augmentSlot.Value.ClearSelection();
			}
			augmentSlot.Value.Hide();
			pool.Push(augmentSlot.Value);
		}
		foreach (KeyValuePair<string, UIUseAugmentation> augmentGroupSlot in augmentGroupSlots)
		{
			if (clearSelection)
			{
				augmentGroupSlot.Value.ClearSelection();
			}
			augmentGroupSlot.Value.Hide();
			pool.Push(augmentGroupSlot.Value);
		}
		if (!augmentSlots.IsNullOrEmpty() || !augmentGroupSlots.IsNullOrEmpty())
		{
			OnChangedAugmentsAvailable.Invoke();
		}
		augmentSlots.Clear();
		augmentGroupSlots.Clear();
		RefreshAvailableElements();
	}

	public void UndoSelection()
	{
		foreach (KeyValuePair<CActionAugmentation, UIUseAugmentation> augmentSlot in augmentSlots)
		{
			augmentSlot.Value.ClearSelection();
		}
		foreach (KeyValuePair<string, UIUseAugmentation> augmentGroupSlot in augmentGroupSlots)
		{
			augmentGroupSlot.Value.ClearSelection();
		}
	}

	public void Show(CActor actor, CAbility ability, List<ConsumeAugmentationParam> consumes, List<ConsumeGroupParam> groupConsumes)
	{
		IsShown = true;
		this.actor = actor;
		this.ability = ability;
		InfusionBoardUI.Instance?.OnReservedElement.RemoveListener(OnReservedElement);
		InfusionBoardUI.Instance?.OnUnreserveElement.RemoveListener(OnUnreservedElement);
		InfusionBoardUI.Instance?.OnUnreserveElement.AddListener(OnUnreservedElement);
		InfusionBoardUI.Instance?.OnReservedElement.AddListener(OnReservedElement);
		_normalConsumes = consumes;
		foreach (CActionAugmentation item in augmentSlots.Keys.Where((CActionAugmentation x) => !_normalConsumes.Select((ConsumeAugmentationParam c) => c.Augmentation).Contains(x)).ToList())
		{
			Remove(item, undoSelection: false);
		}
		foreach (ConsumeAugmentationParam normalConsume in _normalConsumes)
		{
			Create(normalConsume);
		}
		foreach (string item2 in (from x in augmentGroupSlots
			where !groupConsumes.Select((ConsumeGroupParam c) => c.ConsumeGroupKey).Contains(x.Key)
			select x.Key).ToList())
		{
			Remove(item2);
		}
		foreach (ConsumeGroupParam groupConsume in groupConsumes)
		{
			groupConsume.Augmentations.RemoveAll((CActionAugmentation x) => !CanConsume(x) && !PhaseManager.CurrentPhaseAction.ActionAugmentationsToConsume.Any((CActionAugmentation y) => y.Name == x.Name));
			if (groupConsume.Augmentations.Count != 0)
			{
				Create(groupConsume.ConsumeGroupKey, groupConsume.Augmentations);
			}
		}
		foreach (KeyValuePair<CActionAugmentation, UIUseAugmentation> item3 in augmentSlots.OrderBy((KeyValuePair<CActionAugmentation, UIUseAugmentation> x) => x.Key?.Name))
		{
			item3.Value.transform.SetAsLastSibling();
		}
		foreach (KeyValuePair<string, UIUseAugmentation> item4 in augmentGroupSlots.OrderBy((KeyValuePair<string, UIUseAugmentation> x) => x.Key))
		{
			item4.Value.transform.SetAsLastSibling();
		}
		SaveElementsState();
		RefreshAvailableElements();
	}

	private void SaveElementsState()
	{
		if (augmentSlots.Count != 0 || augmentGroupSlots.Count != 0)
		{
			ElementInfusionBoardManager.SaveState();
			InfusionBoardUI.Instance?.SaveState();
		}
	}

	public void TryNetworkConsumeButtonToggle(ConsumeButton button, bool toggleOn)
	{
		if (FFSNetwork.IsOnline && Choreographer.s_Choreographer.ThisPlayerHasTurnControl)
		{
			Synchronizer.SendGameAction(toggleOn ? GameActionType.AugmentCardAbility : GameActionType.CancelAbilityAugment, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, 0, 0, supplementaryDataBoolean: false, null, new AbilityAugmentToken(button.abilityCardID, button.IsAbilityCardTopButton ? 1 : 4, button.ConsumeButtonIndex, button.IsUsingBothElements, (int)button.SelectedElement1, (int)button.SelectedElement2));
		}
	}

	public void TryNetworkAugmentationGroupToggle(CActionAugmentation augment, IAugmentation augmentationGroup, bool toggleOn)
	{
		if (FFSNetwork.IsOnline && Choreographer.s_Choreographer.ThisPlayerHasTurnControl)
		{
			Synchronizer.SendGameAction(toggleOn ? GameActionType.GroupAugmentCardAbility : GameActionType.CancelAbilityAugmentGroup, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, 0, 0, supplementaryDataBoolean: false, null, new AbilityAugmentGroupToken(augment.Name, augmentationGroup.ID));
		}
	}

	public void TryNetworkAbilityAugmentationNoButtonToggle(CActionAugmentation augment, bool toggleOn)
	{
		if (FFSNetwork.IsOnline && Choreographer.s_Choreographer.ThisPlayerHasTurnControl)
		{
			CActionAugmentation cActionAugmentation = GameState.CurrentActionSelectedAugmentations.FirstOrDefault((CActionAugmentation x) => x.Name == augment.Name);
			if (cActionAugmentation != null)
			{
				augment = cActionAugmentation;
			}
			Synchronizer.SendGameAction(toggleOn ? GameActionType.AugmentCardAbilityNoButton : GameActionType.CancelAbilityAugmentNoButton, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, 0, 0, supplementaryDataBoolean: false, null, new AbilityAugmentSimpleToken(augment.Name, augment.Elements));
		}
	}

	public void ProxyToggleAugmentationNoButton(GameAction action)
	{
		AbilityAugmentSimpleToken token = action.SupplementaryDataToken as AbilityAugmentSimpleToken;
		CActionAugmentation cActionAugmentation = augmentSlots.Keys.FirstOrDefault((CActionAugmentation a) => a.Name == token.AugmentName);
		if (cActionAugmentation != null)
		{
			UIUseAugmentation uIUseAugmentation = augmentSlots[cActionAugmentation];
			if (action.ActionTypeID == 138)
			{
				if (token.Elements != null && token.Elements.Length != 0)
				{
					List<ElementInfusionBoardManager.EElement> list = new List<ElementInfusionBoardManager.EElement>(token.Elements.Length);
					for (int num = 0; num < token.Elements.Length; num++)
					{
						list.Add((ElementInfusionBoardManager.EElement)token.Elements[num]);
					}
					uIUseAugmentation.ProxySetElements(list);
				}
				else
				{
					uIUseAugmentation.Select();
				}
			}
			else
			{
				uIUseAugmentation.Unselect();
			}
			return;
		}
		throw new Exception("Error toggling the augment. No such augmentation exists on the bar (AugmentName: " + token.AugmentName + ").");
	}

	public void ProxyToggleAugmentationGroup(GameAction action)
	{
		AbilityAugmentGroupToken abilityAugmentGroupToken = action.SupplementaryDataToken as AbilityAugmentGroupToken;
		if (augmentGroupSlots.ContainsKey(abilityAugmentGroupToken.AugmentGroupID))
		{
			if (action.ActionTypeID == 36)
			{
				augmentGroupSlots[abilityAugmentGroupToken.AugmentGroupID].SetOption(abilityAugmentGroupToken.AugmentName);
				augmentGroupSlots[abilityAugmentGroupToken.AugmentGroupID].Select();
			}
			else
			{
				augmentGroupSlots[abilityAugmentGroupToken.AugmentGroupID].Unselect();
			}
			return;
		}
		throw new Exception("Error toggling the augment. No such augmentation group exists on the bar (GroupID: " + abilityAugmentGroupToken.AugmentGroupID + ").");
	}

	public void ProxyToggleAugment(ConsumeButton consumeButton, bool toggleOn, AbilityAugmentToken dataToken)
	{
		CActionAugmentation consumeData = consumeButton.AbilityConsume.ConsumeData;
		if (!augmentSlots.ContainsKey(consumeData))
		{
			throw new Exception("Error toggling the augment. No such augment exists on the bar (Ability: " + consumeButton.ID + ", Augment: " + consumeButton.AbilityConsume.Name + ").");
		}
		if (toggleOn)
		{
			List<ElementInfusionBoardManager.EElement> list = new List<ElementInfusionBoardManager.EElement> { (ElementInfusionBoardManager.EElement)dataToken.SecondElementTypeID };
			if (dataToken.UsesBothElements)
			{
				list.Add((ElementInfusionBoardManager.EElement)dataToken.FirstElementTypeID);
			}
			augmentSlots[consumeData].SetConsumes(list);
			augmentSlots[consumeData].Select();
		}
		else
		{
			augmentSlots[consumeData].Unselect(forceUnselect: true);
		}
	}
}
