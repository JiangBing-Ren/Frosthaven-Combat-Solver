using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using ScenarioRuleLibrary;

public class AugmentationHolder : IAugmentation
{
	private CActionAugmentation augmentation;

	public CActionAugmentation Augmentation => augmentation;

	public string ID { get; private set; }

	public CActionAugmentation GetAugmentation()
	{
		return augmentation;
	}

	public IEnumerable<CActionAugmentation> GetAvailableAugmentations()
	{
		yield return augmentation;
	}

	public AugmentationHolder(string id, CActionAugmentation augmentation)
	{
		this.augmentation = augmentation;
		ID = id;
	}

	public virtual void ActiveAugment(List<ElementInfusionBoardManager.EElement> elements)
	{
		for (int i = 0; i < elements.Count; i++)
		{
			augmentation = GameState.SelectActionAugmentation(augmentation, elements[i], remove: false, i);
		}
		InfusionBoardUI.Instance?.MarkElementsDirty();
		if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
		{
			Choreographer.s_Choreographer.readyButton.SetInteractable(interactable: false);
			Choreographer.s_Choreographer.m_SkipButton.SetInteractable(active: false);
			Choreographer.s_Choreographer.m_UndoButton.SetInteractable(active: false);
			Singleton<UIUseAugmentationsBar>.Instance.SetInteractionAvailableSlots(interactable: false);
			Singleton<UIActiveBonusBar>.Instance.SetInteractionAvailableSlots(interactable: false);
			Singleton<UIUseItemsBar>.Instance.SetItemsInteractable(enable: false);
			if (FFSNetwork.IsOnline && !Choreographer.s_Choreographer.ThisPlayerHasTurnControl)
			{
				ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionProcessor.CurrentPhase, savePreviousState: true);
			}
			ScenarioRuleClient.ToggleActionAugmentation(augmentation);
		}
	}

	public virtual void DisactiveAugment()
	{
		try
		{
			List<ElementInfusionBoardManager.EElement> list = augmentation.Elements.ToList();
			augmentation = GameState.SelectActionAugmentation(augmentation, ElementInfusionBoardManager.EElement.Any, remove: true, 0, list.Count == 0);
			augmentation.ResetElements();
			InfusionBoardUI.Instance?.MarkElementsDirty();
			if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
			{
				Choreographer.s_Choreographer.readyButton.SetInteractable(interactable: false);
				Choreographer.s_Choreographer.m_SkipButton.SetInteractable(active: false);
				Choreographer.s_Choreographer.m_UndoButton.SetInteractable(active: false);
				Singleton<UIUseAugmentationsBar>.Instance.SetInteractionAvailableSlots(interactable: false);
				Singleton<UIActiveBonusBar>.Instance.SetInteractionAvailableSlots(interactable: false);
				Singleton<UIUseItemsBar>.Instance.SetItemsInteractable(enable: false);
				if (FFSNetwork.IsOnline && !Choreographer.s_Choreographer.ThisPlayerHasTurnControl)
				{
					ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionProcessor.CurrentPhase, savePreviousState: true);
				}
				ScenarioRuleClient.ToggleActionAugmentation(augmentation);
			}
		}
		catch (Exception exception)
		{
			Log.GUIException(exception, "An exception occurred during DisactiveAugment.");
		}
	}

	public bool CanBeDisactivated()
	{
		if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction)
		{
			return !cPhaseAction.HasConsumedActionAugmentation(augmentation);
		}
		return false;
	}

	public string GetSelectAudioItem()
	{
		if (augmentation.Elements.Count != 0)
		{
			return null;
		}
		return UIInfoTools.Instance.toggleUseCharacterSlotAudioItem;
	}
}
