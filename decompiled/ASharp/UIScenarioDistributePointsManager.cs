using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using FHGUI;
using FHRL;
using GLOOM;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class UIScenarioDistributePointsManager : Singleton<UIScenarioDistributePointsManager>
{
	[SerializeField]
	private UIDistributePointsPopup distributePointsSelectPopup;

	[SerializeField]
	private UIDistributePointsPopup distributePointsAssignPopup;

	[SerializeField]
	private UISelectActorPopup selectActorPopup;

	public void Refresh()
	{
		distributePointsAssignPopup.Refresh();
		distributePointsSelectPopup.Refresh();
		selectActorPopup.Refresh();
	}

	public void Hide()
	{
		distributePointsAssignPopup.Hide();
		distributePointsSelectPopup.Hide();
		selectActorPopup.Hide();
	}

	public void ShowAssign(IDistributePointsService service, Action<IDistributePointsActor, int> onUpdatedPoints, Action<IDistributePointsActor, bool> onHovered)
	{
		distributePointsAssignPopup.Show(service, onUpdatedPoints, onHovered);
	}

	public void ShowSelect(IDistributePointsService service, Action<IDistributePointsActor, int> onUpdatedPoints, Action<IDistributePointsActor, bool> onHovered = null)
	{
		distributePointsSelectPopup.Show(service, onUpdatedPoints, onHovered, LocalizationManager.GetTranslation("GUI_SELECT"));
	}

	public void ShowSelectNegateOrReceiveDamageForAllyMonster(CAttackSummary.TargetSummary summary)
	{
		if (summary == null)
		{
			throw new FHGameException("Trying to show negate damage for ally monster popup with no active target summary");
		}
		UIManager.Instance.dialogPopup.Hide();
		Singleton<UIScenarioDistributePointsManager>.Instance.Hide();
		Choreographer.s_Choreographer.m_SkipButton.Toggle(active: false);
		Choreographer.s_Choreographer.readyButton.Toggle(active: false, ReadyButton.EButtonState.EREADYBUTTONNA, null, hideOnClick: true, glowingEffect: false, interactable: true, disregardTurnControlForInteractability: false, haltActionProcessorIfDeactivated: false);
		Choreographer.s_Choreographer.m_UndoButton.Toggle(active: false);
		if (FFSNetwork.IsOnline)
		{
			Singleton<UIScenarioMultiplayerController>.Instance.OnChooseToBurnCardForAlly(null, summary);
			if (!FFSNetwork.IsHost)
			{
				Singleton<HelpBox>.Instance.Show("GUI_MULTIPLAYER_WAITING_FOR_PARTYLEADER_ACTION", "GUI_CHOOSE_ACTOR_PREVENT_DAMAGE_TIP_TITLE");
				ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.HealthRedistribution);
			}
			else
			{
				Singleton<HelpBox>.Instance.ShowTranslated(FHUILocFormatting.PopulateParams("GUI_CHOOSE_ALLY_PREVENT_DAMAGE_CHOICE", LocalizationManager.GetTranslation(summary.Actor.ActorLocKey())), LocalizationManager.GetTranslation("GUI_CHOOSE_ACTOR_PREVENT_DAMAGE_TIP_TITLE"));
				ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.HealthRedistribution);
			}
		}
		else
		{
			Singleton<HelpBox>.Instance.ShowTranslated(FHUILocFormatting.PopulateParams("GUI_CHOOSE_ALLY_PREVENT_DAMAGE_CHOICE", LocalizationManager.GetTranslation(summary.Actor.ActorLocKey())), LocalizationManager.GetTranslation("GUI_CHOOSE_ACTOR_PREVENT_DAMAGE_TIP_TITLE"));
		}
		string translation = LocalizationManager.GetTranslation("GUI_NEGATE_DAMAGE");
		bool flag = summary.Actor.Health <= 0;
		string text = (flag ? ("<color=#" + UIInfoTools.DefInstance.negativeTextColor.ToHex() + ">") : "") + LocalizationManager.GetTranslation("GUI_TAKE_DAMAGE") + (flag ? " <sprite name=Enemy> " : " <sprite name=Damage> ") + summary.FinalAttackStrength;
		DialogPopup dialogPopup = UIManager.Instance.dialogPopup;
		List<GameObject> content = new List<GameObject> { null };
		DialogOption[] array = new DialogOption[2];
		InputAction burnOneCard = InputFHManager.Controls.Scenario.BurnOneCard;
		UnityAction action = () =>
		{
			if (FFSNetwork.IsOnline)
			{
				Synchronizer.SendGameAction(GameActionType.ChoosePlayerToBurnCardsForAllyMonster);
			}
			ProxyChoosePlayerToBurnCardsForAlly(null, summary);
		};
		CLevelUIInteractionProfile.EIsolatedControlType? controlType = CLevelUIInteractionProfile.EIsolatedControlType.DialogOptionButton;
		CPlayerActor hostCharacterIncludingDead = GameState.HostCharacterIncludingDead;
		array[0] = new DialogOption(translation, burnOneCard, action, enabled: true, visible: true, controlType, null, hostCharacterIncludingDead);
		InputAction receiveDamage = InputFHManager.Controls.Scenario.ReceiveDamage;
		UnityAction action2 = () =>
		{
			if (FFSNetwork.IsOnline)
			{
				Synchronizer.SendGameAction(GameActionType.ReceiveDamageInsteadOfNegatingForAllyMonster);
			}
			ProxyReceiveDamageInsteadOfNegating(null);
		};
		CLevelUIInteractionProfile.EIsolatedControlType? controlType2 = CLevelUIInteractionProfile.EIsolatedControlType.DialogOptionButton;
		hostCharacterIncludingDead = GameState.HostCharacterIncludingDead;
		array[1] = new DialogOption(text, receiveDamage, action2, enabled: true, visible: true, controlType2, null, hostCharacterIncludingDead);
		dialogPopup.Show(content, array);
	}

	public void ProxyChooseCardsToBurnForActor(GameAction a)
	{
		CAttackSummary.TargetSummary damageSummaryToBurnCardForAlly = Singleton<UIScenarioMultiplayerController>.Instance.damageSummaryToBurnCardForAlly;
		if (damageSummaryToBurnCardForAlly == null)
		{
			throw new FHGameException("Trying to show cards to burn for an ally monster while no such target summary is present on other players");
		}
		CPlayerActor playerActor = Choreographer.s_Choreographer.FindPlayerActor(a.ActorID) as CPlayerActor;
		Singleton<UIScenarioMultiplayerController>.Instance.OnChooseToBurnCardForAlly(playerActor, damageSummaryToBurnCardForAlly);
	}

	public void ProxyChoosePlayerToBurnCardsForAlly(GameAction a, CAttackSummary.TargetSummary summary)
	{
		if (FFSNetwork.IsOnline)
		{
			if (!FFSNetwork.IsHost)
			{
				Singleton<HelpBox>.Instance.Show("GUI_MULTIPLAYER_WAITING_FOR_PARTYLEADER_ACTION", "GUI_CHOOSE_ACTOR_PREVENT_DAMAGE_TIP_TITLE");
				ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.HealthRedistribution);
			}
			else
			{
				Singleton<HelpBox>.Instance.ShowTranslated(FHUILocFormatting.PopulateParams("GUI_CHOOSE_ALLY_PREVENT_DAMAGE_CHOICE", LocalizationManager.GetTranslation(summary.Actor.ActorLocKey())), LocalizationManager.GetTranslation("GUI_CHOOSE_ACTOR_PREVENT_DAMAGE_TIP_TITLE"));
				ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.HealthRedistribution);
			}
			Singleton<UIScenarioMultiplayerController>.Instance.OnShowChoosePlayerToBurnCard();
		}
		else
		{
			Singleton<HelpBox>.Instance.ShowTranslated(FHUILocFormatting.PopulateParams("GUI_CHOOSE_ALLY_PREVENT_DAMAGE_CHOICE", LocalizationManager.GetTranslation(summary.Actor.ActorLocKey())), LocalizationManager.GetTranslation("GUI_CHOOSE_ACTOR_PREVENT_DAMAGE_TIP_TITLE"));
		}
		UIManager.Instance.dialogPopup.Hide();
		DistributeSelectPlayerActorService service = new DistributeSelectPlayerActorService(ScenarioManager.Scenario.PlayerActors.Where((CPlayerActor it) => it.CharacterClass.HandAbilityCards.Count > 0 || it.CharacterClass.DiscardedAbilityCards.Count > 1).ToList(), "GUI_CHOOSE_ACTOR_PREVENT_DAMAGE");
		Singleton<UIScenarioDistributePointsManager>.Instance.ShowSelectActor(service, GameState.CurrentDamageData.ActorDamaged, summary.FinalAttackStrength, (IDistributePointsActor actor) =>
		{
		}, (IDistributePointsActor actor) =>
		{
			Singleton<UIScenarioDistributePointsManager>.Instance.Hide();
			if (actor?.Actor is CPlayerActor cPlayerActor)
			{
				if (FFSNetwork.IsOnline)
				{
					Synchronizer.SendGameAction(GameActionType.ChooseCardsToBurnForAllyMonster, ActionPhaseType.NONE, validateOnServerBeforeExecuting: false, disableAutoReplication: false, cPlayerActor.ID);
					Singleton<UIScenarioMultiplayerController>.Instance.OnChooseToBurnCardForAlly(cPlayerActor, summary);
				}
				else
				{
					Singleton<TakeDamagePanel>.Instance.ShowBurnCardForAlly(summary.ActorToAttack, cPlayerActor, summary.FinalAttackStrength, summary.Pierce, summary.AttackAbility == null, summary.OriginalAbility, allowBurnCards: true, summary.OriginalAbility?.AbilityType ?? CAbility.EAbilityType.None, summary);
				}
			}
			else
			{
				if (FFSNetwork.IsOnline)
				{
					Synchronizer.SendGameAction(GameActionType.ChooseIfNegateOrReceiveDamageForAllyMonster);
				}
				ShowSelectNegateOrReceiveDamageForAllyMonster(summary);
			}
		});
	}

	public void ProxyReceiveDamageInsteadOfNegating(GameAction a)
	{
		UIManager.Instance.dialogPopup.Hide();
		GameState.SelectedPlayerToAvoidDamage(null);
	}

	public void ProxyShowSelectNegateDamage(GameAction a)
	{
		ShowSelectNegateOrReceiveDamageForAllyMonster(Singleton<UIScenarioMultiplayerController>.Instance.damageSummaryToBurnCardForAlly);
	}

	public void ShowSelectActor(IDistributePointsService service, CActor targetActor, int damageToPrevent, Action<IDistributePointsActor> onSelectActor, Action<IDistributePointsActor> onConfirm, Action<IDistributePointsActor, bool> onHovered = null)
	{
		string translation = LocalizationManager.GetTranslation(targetActor.ActorLocKey());
		selectActorPopup.Show(new UISelectActorPopup.Params
		{
			Service = service,
			OnSelectActor = onSelectActor,
			OnHovered = onHovered,
			OnConfirm = onConfirm,
			TitleText = LocalizationManager.GetTranslation("GUI_CHOOSE_ALLY_PREVENT_DAMAGE_TITLE"),
			DescriptionText = FHUILocFormatting.PopulateParams("GUI_CHOOSE_ALLY_PREVENT_DAMAGE_DESC", translation, damageToPrevent),
			HintText = LocalizationManager.GetTranslation("GUI_CHOOSE_ALLY_PREVENT_DAMAGE_HINT"),
			ButtonTextConfirm = LocalizationManager.GetTranslation("GUI_CONFIRM"),
			ButtonTextCancel = LocalizationManager.GetTranslation("GUI_CANCEL"),
			Interactable = (GameState.HostCharacterIncludingDead?.IsUnderMyControl() ?? true)
		});
	}

	public void ProxyDistributeAssignPopup(GameAction action)
	{
		distributePointsAssignPopup.ProxyRedistributeHealth(action);
	}

	public void ProxyDistributeSelectPopup(GameAction action)
	{
		selectActorPopup.ProxySelectActor(action);
	}
}
