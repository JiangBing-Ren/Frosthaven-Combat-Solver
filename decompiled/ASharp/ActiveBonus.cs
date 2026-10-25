using System.Collections.Generic;
using FFSNet;
using ScenarioRuleLibrary;
using UnityEngine;

public class ActiveBonus : BaseActiveBonus<CActiveBonus>
{
	public ActiveBonus(CActiveBonus bonus, CActor actor)
		: base(bonus, actor)
	{
	}

	public override Sprite GetIcon()
	{
		string text = bonus.Ability.ActiveBonusData.GetMiscVar("OverrideIcon").GetString();
		if (!string.IsNullOrEmpty(text))
		{
			return UIInfoTools.Instance.GetActiveAbilityIcon(text);
		}
		if (bonus.BaseCard is CItem cItem)
		{
			return UIInfoTools.Instance.GetItemMiniSprite(cItem.YMLData.Art);
		}
		if (bonus.Ability.PerkID.IsNOTNullOrEmpty())
		{
			return UIInfoTools.Instance.GetPerkIcon(bonus.Ability.PerkID);
		}
		if (bonus.IsAura && bonus.Actor is CPlayerActor cPlayerActor)
		{
			return UIInfoTools.Instance.GetCharacterActiveAbilityIcon(cPlayerActor.CharacterClass.ID, cPlayerActor.CharacterClass.CharacterYML.CustomCharacterConfig);
		}
		if (bonus.Caster is CPlayerActor cPlayerActor2)
		{
			return UIInfoTools.Instance.GetCharacterActiveAbilityIcon(cPlayerActor2.Class.ID, cPlayerActor2.CharacterClass?.CharacterYML.CustomCharacterConfig);
		}
		if (actor is CPlayerActor cPlayerActor3)
		{
			return UIInfoTools.Instance.GetCharacterActiveAbilityIcon(cPlayerActor3.Class.ID, cPlayerActor3?.CharacterClass?.CharacterYML.CustomCharacterConfig);
		}
		if (actor is CHeroSummonActor cHeroSummonActor)
		{
			return UIInfoTools.Instance.GetCharacterActiveAbilityIcon(cHeroSummonActor.Summoner.Class.ID, cHeroSummonActor.Summoner?.CharacterClass?.CharacterYML.CustomCharacterConfig);
		}
		return UIInfoTools.Instance.GetCharacterActiveAbilityIcon(actor.Class.ID);
	}

	public override string GetSelectAudioItem()
	{
		if (bonus.BaseCard is CItem cItem)
		{
			return UIInfoTools.Instance.GetItemConfig(cItem.YMLData.Art).toggleAudioItem;
		}
		return base.GetSelectAudioItem();
	}

	public override void UntoggleActiveBonus(bool fromClick = false)
	{
		Log.GUIInfo("Attempting to untoggle active bonus slot");
		if (!IsToggled || IsToggleLocked)
		{
			return;
		}
		Singleton<UIActiveBonusBar>.Instance.SetInteractionAvailableSlots(interactable: false);
		if (fromClick)
		{
			bool flag = Choreographer.s_Choreographer.ActorOrHisSummonerIsUnderMyControl(actor);
			if ((FFSNetwork.IsOnline & flag) && ActionProcessor.CurrentPhase != ActionPhaseType.TakeDamageConfirmation && ActionProcessor.CurrentPhase != ActionPhaseType.ShieldConfirmation)
			{
				Log.GUIInfo("Sending untoggle active bonus slot click action");
				Synchronizer.SendGameAction(GameActionType.ClickActiveBonusSlot, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, actor.ID, 0, 0, 0, supplementaryDataBoolean: false, null, new ActiveBonusToken(bonus, selected: false));
			}
			if (FFSNetwork.IsOnline && !actor.IsUnderMyControl())
			{
				ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionProcessor.CurrentPhase, savePreviousState: true);
			}
		}
		Choreographer.s_Choreographer.readyButton.SetInteractable(interactable: false);
		Choreographer.s_Choreographer.m_SkipButton.SetInteractable(active: false);
		Choreographer.s_Choreographer.m_UndoButton.SetInteractable(active: false);
		Singleton<UIUseAbilitiesBar>.Instance.Hide(clearSelection: true);
		ScenarioRuleClient.ToggleActiveBonus(bonus, actor, null, (int)ActionProcessor.CurrentPhase, null, GameState.WaitingForPlayerToSelectDamageResponse || GameState.WaitingForPlayerActorToAvoidDamageResponse || GameState.ThreadIsSleeping);
	}

	public void ToggleActiveBonus(ElementInfusionBoardManager.EElement? eElement, object extraOption, bool fromClick = false)
	{
		Log.GUIInfo("Attempting to toggle active bonus slot");
		if (IsToggled || IsToggleLocked)
		{
			return;
		}
		Singleton<UIActiveBonusBar>.Instance.SetInteractionAvailableSlots(interactable: false);
		if (PhaseManager.CurrentPhase is CPhaseAction { CurrentPhaseAbility: not null } cPhaseAction && cPhaseAction.CurrentPhaseAbility.m_Ability.IsWaitingForSingleTargetActiveBonus())
		{
			Choreographer.s_Choreographer.readyButton.SetInteractable(interactable: false);
		}
		if (fromClick)
		{
			bool flag = Choreographer.s_Choreographer.ActorOrHisSummonerIsUnderMyControl(actor);
			if ((FFSNetwork.IsOnline & flag) && ActionProcessor.CurrentPhase != ActionPhaseType.TakeDamageConfirmation && ActionProcessor.CurrentPhase != ActionPhaseType.ShieldConfirmation)
			{
				Log.GUIInfo("Sending toggle active bonus slot click action");
				UIUseActiveBonus slotForActiveBonus = Singleton<UIActiveBonusBar>.Instance.GetSlotForActiveBonus(bonus);
				List<ElementInfusionBoardManager.EElement> selectedConsumes = slotForActiveBonus.GetSelectedConsumes();
				Synchronizer.SendGameAction(GameActionType.ClickActiveBonusSlot, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, actor.ID, 0, 0, 0, supplementaryDataBoolean: false, null, new ActiveBonusToken(bonus, selected: true, selectedConsumes.IsNullOrEmpty() ? ((ElementInfusionBoardManager.EElement?)null) : new ElementInfusionBoardManager.EElement?(selectedConsumes[0]), slotForActiveBonus.SelectedOptions));
			}
			if (FFSNetwork.IsOnline && !actor.IsUnderMyControl())
			{
				ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionProcessor.CurrentPhase, savePreviousState: true);
			}
		}
		Choreographer.s_Choreographer.readyButton.SetInteractable(interactable: false);
		Choreographer.s_Choreographer.m_SkipButton.SetInteractable(active: false);
		Choreographer.s_Choreographer.m_UndoButton.SetInteractable(active: false);
		ScenarioRuleClient.ToggleActiveBonus(bonus, actor, eElement, (int)ActionProcessor.CurrentPhase, extraOption, GameState.WaitingForPlayerToSelectDamageResponse || GameState.WaitingForPlayerActorToAvoidDamageResponse || GameState.ThreadIsSleeping);
	}

	public override void ToggleActiveBonus(ElementInfusionBoardManager.EElement? eElement, bool fromClick = false)
	{
		ToggleActiveBonus(eElement, null, fromClick);
	}
}
