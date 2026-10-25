using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevLocker.GFrame.Input;
using FFSNet;
using FHGUI.Scenario;
using GLOOM;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class TakeDamagePanel : Singleton<TakeDamagePanel>
{
	public enum State
	{
		None,
		ShowingShields,
		ShowingBurnCardForAlly,
		ShowingPrevents
	}

	private enum PreviewOptions
	{
		None,
		PreviewFullHand,
		PreviewAvailableCards,
		PreviewDiscardedCards,
		PreviewDamage
	}

	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private TMP_Text takeDamageText;

	[SerializeField]
	private TMP_Text damageAmount;

	[SerializeField]
	private Image damageObject;

	[SerializeField]
	private GameObject fatalDamageObject;

	[SerializeField]
	private FHToggle burnAvailableCardsToggle;

	[SerializeField]
	private FHToggle burnDiscardedCardsToggle;

	[SerializeField]
	private FHButton takeDamageButton;

	[SerializeField]
	private FHButton changeSelectionButton;

	[SerializeField]
	private GameObject mandatoryTakeDamageHighlight;

	public InteractabilityIsolatedUIControl.InteractabilityReference takeDamageButtonInteractability;

	public InteractabilityIsolatedUIControl.InteractabilityReference burnAvailableCardsToggleInteractability;

	public InteractabilityIsolatedUIControl.InteractabilityReference burnDiscardedCardsInteractability;

	[SerializeField]
	private Color shieldAppliedColor;

	[SerializeField]
	private GameObject buttonsContainer;

	private Color defaultColor;

	private UITextTooltipTarget takeDamageTooltip;

	private UITextTooltipTarget changeSelectionTooltip;

	private CActor actorBeingAttacked;

	private CPlayerActor actorToShowCardsFor;

	private CActor actorAttacking;

	private CAbility damageAbility;

	private WorldspacePanelUIController actorHUDController;

	private CAttackSummary.TargetSummary targetSummary;

	private int pierce;

	private int addedShield;

	private int initialHealth;

	private PreviewOptions currentlyPreviewing;

	private Toggle currentlyToggled;

	private bool hasEnoughAvailableCards;

	private bool hasEnoughDiscardedCards;

	private bool isDirectDamage;

	private bool allowBurnCards;

	private List<CItem> toggledShieldItems;

	private bool preventAllDamage;

	private int m_DamageToPrevent;

	private List<CActiveBonus> toggledActiveBonuses;

	private bool m_TakeDamageInvoked;

	private List<GameAction> pendingGameActions = new List<GameAction>();

	public CPlayerActor ActorToShowCardsFor => actorToShowCardsFor;

	public State state { get; private set; }

	private int damageToTake => targetSummary.FinalAttackStrength;

	private int FinalDamage => CalculateCurrentDamage();

	public FHButton TakeDamageButton => takeDamageButton;

	public FHToggle BurnAvailableCardsToggle => burnAvailableCardsToggle;

	public FHToggle BurnDiscardedCardsToggle => burnDiscardedCardsToggle;

	public bool ThisPlayerHasTakeDamageControl
	{
		get
		{
			if (!FFSNetwork.IsOnline)
			{
				return true;
			}
			CActor cActor = ((actorToShowCardsFor != null) ? actorToShowCardsFor : actorBeingAttacked);
			bool result = false;
			if (!(cActor is CPlayerActor actor))
			{
				if (!(cActor is CHeroSummonActor cHeroSummonActor))
				{
					if (cActor is CEnemyActor)
					{
						result = FFSNetwork.IsHost;
					}
				}
				else
				{
					result = cHeroSummonActor.Summoner.IsUnderMyControl();
				}
			}
			else
			{
				result = actor.IsUnderMyControl();
			}
			return result;
		}
	}

	public CPlayerActor PlayerActorMakingChoice
	{
		get
		{
			CActor cActor = actorToShowCardsFor ?? actorBeingAttacked;
			if (!(cActor is CPlayerActor result))
			{
				if (!(cActor is CHeroSummonActor { Summoner: var summoner }))
				{
					if (cActor is CEnemyActor)
					{
						return Singleton<CardsHandManager>.Instance.CurrentHand?.PlayerActor;
					}
					return null;
				}
				return summoner;
			}
			return result;
		}
	}

	public CActor ActorBeingAttacked => actorBeingAttacked;

	public bool IsOpen => window.IsOpen;

	private bool IsLethalDamage
	{
		get
		{
			if (initialHealth - FinalDamage <= 0)
			{
				return !preventAllDamage;
			}
			return false;
		}
	}

	protected override void AwakeInit()
	{
		defaultColor = damageAmount.color;
		window.onHidden.AddListener(ClearButtonsSelected);
		toggledShieldItems = new List<CItem>();
		toggledActiveBonuses = new List<CActiveBonus>();
		burnAvailableCardsToggle.onValueChanged.AddListener(BurnAvailableCard);
		burnAvailableCardsToggle.PointerEntered += OnMouseEnterBurnOne;
		burnAvailableCardsToggle.PointerExited += OnMouseExitBurnOne;
		takeDamageButton.onClick.AddListener(OnClickTakeDamageButton);
		takeDamageButton.PointerEntered += OnMouseEnterTakeDamage;
		takeDamageButton.PointerExited += OnMouseExitTakeDamage;
		changeSelectionButton.onClick.AddListener(OnClickChangeSelectionButton);
		changeSelectionButton.PointerEntered += OnMouseEnterTakeDamage;
		changeSelectionButton.PointerExited += OnMouseExitTakeDamage;
		burnDiscardedCardsToggle.onValueChanged.AddListener(BurnDiscardedCards);
		burnDiscardedCardsToggle.PointerEntered += OnMouseEnterBurnTwo;
		burnDiscardedCardsToggle.PointerExited += OnMouseExitBurnTwo;
		takeDamageTooltip = TakeDamageButton.GetComponent<UITextTooltipTarget>();
		changeSelectionTooltip = changeSelectionButton.GetComponent<UITextTooltipTarget>();
	}

	private void OnClickChangeSelectionButton()
	{
		if (state == State.ShowingBurnCardForAlly && ThisPlayerHasTakeDamageControl)
		{
			if (FFSNetwork.IsOnline)
			{
				Synchronizer.SendGameAction(GameActionType.ChangeSelectionPlayerToBurnCardsForAlly, ActionPhaseType.ChooseCardsToBurnForAllyMonster, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, 0, 0, supplementaryDataBoolean: false, actorBeingAttacked.ActorGuid);
			}
			ChooseOtherPlayerToBurnCard();
		}
	}

	private void OnClickTakeDamageButton()
	{
		if (state == State.ShowingShields)
		{
			ConfirmShields();
		}
		else if (state == State.ShowingPrevents)
		{
			TakeDamage();
		}
	}

	private void ClearButtonsSelected()
	{
	}

	public bool IsTakingDamage(CActor actor)
	{
		if (actorBeingAttacked == actor)
		{
			return IsOpen;
		}
		return false;
	}

	public void RefreshButtons(bool cardBurn, bool resetToggles)
	{
		Choreographer.s_Choreographer.m_CurrentActor = actorToShowCardsFor ?? actorBeingAttacked;
		UpdateTakeDamageOptionVisuals();
		if (cardBurn)
		{
			UpdateCardRemovalOptionVisuals(resetToggles);
		}
	}

	public void Show(CActor actorBeingAttacked, CPlayerActor actorToShowCardsFor, int damageToTake, int pierce, bool isDirectDamage, CAbility damageAbility, bool allowBurnCards, CAbility.EAbilityType damageType, CAttackSummary.TargetSummary targetSummary)
	{
		try
		{
			ResetToggles();
			state = State.ShowingPrevents;
			this.targetSummary = targetSummary;
			m_DamageToPrevent = 0;
			preventAllDamage = false;
			this.actorBeingAttacked = actorBeingAttacked;
			this.actorToShowCardsFor = actorToShowCardsFor;
			actorAttacking = damageAbility?.TargetingActor;
			this.damageAbility = damageAbility;
			this.pierce = pierce;
			this.isDirectDamage = isDirectDamage;
			this.allowBurnCards = allowBurnCards;
			addedShield = 0;
			initialHealth = CalculateCurrentHealth();
			damageAmount.color = defaultColor;
			damageObject.color = defaultColor;
			mandatoryTakeDamageHighlight.SetActive(value: false);
			buttonsContainer.SetActive(value: true);
			_ = this.allowBurnCards;
			if (GameState.s_AutoTakeDamage && (actorToShowCardsFor == null || actorToShowCardsFor == actorBeingAttacked))
			{
				allowBurnCards = false;
			}
			burnAvailableCardsToggle.gameObject.SetActive(allowBurnCards);
			burnDiscardedCardsToggle.gameObject.SetActive(allowBurnCards);
			takeDamageButton.gameObject.SetActive(value: true);
			changeSelectionButton.gameObject.SetActive(value: false);
			WorldspacePanelUIController[] array = WorldspaceUITools.Instance?.GetComponentsInChildren<WorldspacePanelUIController>(includeInactive: false);
			for (int i = 0; i < array?.Length; i++)
			{
				if (array[i].InfoUI.m_ActorBehavior.Actor.Equals(actorBeingAttacked))
				{
					actorHUDController = array[i];
					break;
				}
			}
			damageAmount.text = damageToTake.ToString();
			RefreshButtons(cardBurn: true, resetToggles: true);
			ClearPreviouslyPreviewed();
			ClearTakeDamageInvoked();
			UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.DamageChoiceDialogAppeared));
			window.Show();
			ClearSelectedToggle();
			if (!isDirectDamage)
			{
				Singleton<UIActiveBonusBar>.Instance.ShowReduceDamageActiveBonuses(actorBeingAttacked, damageAbility, IsLethalDamage, FinalDamage, showPrevent: true, showShield: false, ToggleActiveBonus);
			}
			else
			{
				Singleton<UIActiveBonusBar>.Instance.ShowReduceDamageActiveBonuses(actorBeingAttacked, (damageType != CAbility.EAbilityType.None) ? damageType : CAbility.EAbilityType.Damage, IsLethalDamage, FinalDamage, showPrevent: true, showShield: false, ToggleActiveBonus);
			}
			ToggleAutoToggledBonuses();
			CalculateAddedShield();
			RefreshDamageInformation();
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the TakeDamagePanel.Show().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00034", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void ShowShields(CActor actorBeingAttacked, CPlayerActor actorToShowCardsFor, int damageToTake, int pierce, CAbility damageAbility, CAbility.EAbilityType damageType, CAttackSummary.TargetSummary targetSummary)
	{
		try
		{
			ResetToggles();
			state = State.ShowingShields;
			this.targetSummary = targetSummary;
			m_DamageToPrevent = 0;
			preventAllDamage = false;
			this.actorBeingAttacked = actorBeingAttacked;
			this.actorToShowCardsFor = actorToShowCardsFor;
			actorAttacking = ((damageAbility != null) ? damageAbility.TargetingActor : null);
			this.damageAbility = damageAbility;
			this.pierce = pierce;
			addedShield = 0;
			initialHealth = CalculateCurrentHealth();
			damageAmount.color = defaultColor;
			damageObject.color = defaultColor;
			mandatoryTakeDamageHighlight.SetActive(value: false);
			buttonsContainer.SetActive(value: true);
			burnAvailableCardsToggle.gameObject.SetActive(value: false);
			burnDiscardedCardsToggle.gameObject.SetActive(value: false);
			takeDamageButton.gameObject.SetActive(value: true);
			changeSelectionButton.gameObject.SetActive(value: false);
			WorldspacePanelUIController[] array = WorldspaceUITools.Instance?.GetComponentsInChildren<WorldspacePanelUIController>(includeInactive: false);
			for (int i = 0; i < array?.Length; i++)
			{
				if (array[i].InfoUI.m_ActorBehavior.Actor.Equals(actorBeingAttacked))
				{
					actorHUDController = array[i];
					break;
				}
			}
			damageAmount.text = damageToTake.ToString();
			RefreshButtons(cardBurn: false, resetToggles: true);
			ClearPreviouslyPreviewed();
			ClearTakeDamageInvoked();
			UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.DamageChoiceDialogAppeared));
			window.Show();
			ClearSelectedToggle();
			if (isDirectDamage)
			{
				return;
			}
			Singleton<UIUseItemsBar>.Instance.ShowItems(actorBeingAttacked, (CItem item) =>
			{
				if ((item.YMLData.Trigger.Equals(CItem.EItemTrigger.OnAttacked) || item.YMLData.Trigger.Equals(CItem.EItemTrigger.OnAttackedIgnoringDamageTaken)) && actorBeingAttacked.Inventory.IsItemUsable(item))
				{
					if (item.YMLData.Data.CompareAbility == null)
					{
						return true;
					}
					if (damageAbility == null)
					{
						return false;
					}
					return item.YMLData.Data.CompareAbility.CompareAbility(damageAbility, actorBeingAttacked);
				}
				return false;
			}, ToggleShieldItem, clear: true);
			Singleton<UIActiveBonusBar>.Instance.ShowReduceDamageActiveBonuses(actorBeingAttacked, damageAbility, IsLethalDamage, FinalDamage, showPrevent: false, showShield: true, ToggleActiveBonus);
			ToggleAutoToggledBonuses();
			CalculateAddedShield();
			RefreshDamageInformation();
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the TakeDamagePanel.ShowShields().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00034", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void ShowShieldsOtherPlayer(CActor actorBeingAttacked, CPlayerActor actorToShowCardsFor, int damageToTake, int pierce, CAbility damageAbility, CAbility.EAbilityType damageType, CAttackSummary.TargetSummary targetSummary)
	{
		try
		{
			ResetToggles();
			state = State.ShowingShields;
			this.targetSummary = targetSummary;
			m_DamageToPrevent = 0;
			preventAllDamage = false;
			this.actorBeingAttacked = actorBeingAttacked;
			this.actorToShowCardsFor = actorToShowCardsFor;
			actorAttacking = ((damageAbility != null) ? damageAbility.TargetingActor : null);
			this.damageAbility = damageAbility;
			this.pierce = pierce;
			addedShield = 0;
			initialHealth = CalculateCurrentHealth();
			damageAmount.color = defaultColor;
			damageObject.color = defaultColor;
			mandatoryTakeDamageHighlight.SetActive(value: false);
			buttonsContainer.SetActive(value: true);
			burnAvailableCardsToggle.gameObject.SetActive(value: false);
			burnDiscardedCardsToggle.gameObject.SetActive(value: false);
			takeDamageButton.gameObject.SetActive(value: true);
			changeSelectionButton.gameObject.SetActive(value: false);
			WorldspacePanelUIController[] array = WorldspaceUITools.Instance?.GetComponentsInChildren<WorldspacePanelUIController>(includeInactive: false);
			for (int i = 0; i < array?.Length; i++)
			{
				if (array[i].InfoUI.m_ActorBehavior.Actor.Equals(actorBeingAttacked))
				{
					actorHUDController = array[i];
					break;
				}
			}
			damageAmount.text = damageToTake.ToString();
			RefreshButtons(cardBurn: false, resetToggles: false);
			ClearPreviouslyPreviewed();
			ClearTakeDamageInvoked();
			UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.DamageChoiceDialogAppeared));
			window.Show();
			ClearSelectedToggle();
			if (isDirectDamage)
			{
				return;
			}
			Singleton<UIUseItemsBar>.Instance.ShowItems(actorBeingAttacked, (CItem item) =>
			{
				if ((item.YMLData.Trigger.Equals(CItem.EItemTrigger.OnAttacked) || item.YMLData.Trigger.Equals(CItem.EItemTrigger.OnAttackedIgnoringDamageTaken)) && actorBeingAttacked.Inventory.IsItemUsable(item))
				{
					if (item.YMLData.Data.CompareAbility == null)
					{
						return true;
					}
					if (damageAbility == null)
					{
						return false;
					}
					return item.YMLData.Data.CompareAbility.CompareAbility(damageAbility, actorBeingAttacked);
				}
				return false;
			}, ToggleShieldItem, clear: true);
			Singleton<UIActiveBonusBar>.Instance.ShowReduceDamageActiveBonuses(actorBeingAttacked, damageAbility, IsLethalDamage, FinalDamage, showPrevent: false, showShield: true, ToggleActiveBonus);
			ToggleAutoToggledBonuses();
			CalculateAddedShield();
			RefreshDamageInformation();
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the TakeDamagePanel.ShowShields().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00034", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void ToggleAutoToggledBonuses()
	{
		foreach (CActiveBonus item in Singleton<UIActiveBonusBar>.Instance.ShowingActiveBonuses.Where((CActiveBonus x) => x.ToggledBonus && x.Ability.ActiveBonusData.IsToggleBonus))
		{
			toggledActiveBonuses.Add(item);
		}
	}

	public void ShowBurnCardForAlly(CActor actorBeingAttacked, CPlayerActor actorToShowCardsFor, int damageToTake, int pierce, bool isDirectDamage, CAbility damageAbility, bool allowBurnCards, CAbility.EAbilityType damageType, CAttackSummary.TargetSummary targetSummary)
	{
		try
		{
			ResetToggles();
			state = State.ShowingBurnCardForAlly;
			this.targetSummary = targetSummary;
			m_DamageToPrevent = 0;
			preventAllDamage = false;
			this.actorBeingAttacked = actorBeingAttacked;
			this.actorToShowCardsFor = actorToShowCardsFor;
			actorAttacking = damageAbility?.TargetingActor;
			this.damageAbility = damageAbility;
			this.pierce = pierce;
			this.isDirectDamage = isDirectDamage;
			this.allowBurnCards = allowBurnCards;
			addedShield = 0;
			initialHealth = CalculateCurrentHealth();
			damageAmount.color = defaultColor;
			damageObject.color = defaultColor;
			mandatoryTakeDamageHighlight.SetActive(value: false);
			buttonsContainer.SetActive(value: true);
			_ = this.allowBurnCards;
			if (GameState.s_AutoTakeDamage && (actorToShowCardsFor == null || actorToShowCardsFor == actorBeingAttacked))
			{
				allowBurnCards = false;
			}
			burnAvailableCardsToggle.gameObject.SetActive(allowBurnCards);
			burnDiscardedCardsToggle.gameObject.SetActive(allowBurnCards);
			takeDamageButton.gameObject.SetActive(value: false);
			changeSelectionButton.gameObject.SetActive(value: true);
			WorldspacePanelUIController[] array = WorldspaceUITools.Instance?.GetComponentsInChildren<WorldspacePanelUIController>(includeInactive: false);
			for (int i = 0; i < array?.Length; i++)
			{
				if (array[i].InfoUI.m_ActorBehavior.Actor.Equals(actorBeingAttacked))
				{
					actorHUDController = array[i];
					break;
				}
			}
			damageAmount.text = damageToTake.ToString();
			RefreshButtons(cardBurn: true, resetToggles: true);
			ClearPreviouslyPreviewed();
			ClearSelectedToggle();
			ClearTakeDamageInvoked();
			UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.DamageChoiceDialogAppeared));
			window.Show();
			RefreshDamageInformation();
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the TakeDamagePanel.ShowBurnCardForAlly().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00034", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void ShowBurnCardForAllyOtherPlayer(CActor actorBeingAttacked, CPlayerActor actorToShowCardsFor, int damageToTake, int pierce, bool isDirectDamage, CAbility damageAbility, CAttackSummary.TargetSummary targetSummary)
	{
		ResetToggles();
		state = State.ShowingBurnCardForAlly;
		this.targetSummary = targetSummary;
		this.actorBeingAttacked = actorBeingAttacked;
		this.actorToShowCardsFor = actorToShowCardsFor;
		actorAttacking = damageAbility?.TargetingActor;
		this.damageAbility = damageAbility;
		this.pierce = pierce;
		this.isDirectDamage = isDirectDamage;
		addedShield = 0;
		preventAllDamage = false;
		initialHealth = CalculateCurrentHealth();
		WorldspacePanelUIController[] array = WorldspaceUITools.Instance?.GetComponentsInChildren<WorldspacePanelUIController>(includeInactive: false);
		for (int i = 0; i < array?.Length; i++)
		{
			if (array[i].InfoUI.m_ActorBehavior.Actor.Equals(actorBeingAttacked))
			{
				actorHUDController = array[i];
				break;
			}
		}
		if (actorToShowCardsFor == null)
		{
			actorToShowCardsFor = ((!(actorBeingAttacked is CHeroSummonActor cHeroSummonActor)) ? ScenarioManager.Scenario.PlayerActors[0] : cHeroSummonActor.Summoner);
		}
		burnAvailableCardsToggle.gameObject.SetActive(value: false);
		burnDiscardedCardsToggle.gameObject.SetActive(value: false);
		takeDamageButton.gameObject.SetActive(value: false);
		changeSelectionButton.gameObject.SetActive(value: false);
		int controllableID = ControllableRegistry.GetControllableID(actorToShowCardsFor);
		InitiativeTrack.Instance.helpBox.ShowTranslated(string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_DEAL_DAMAGE_MP"), ControllableRegistry.GetController(controllableID).Username), LocalizationManager.GetTranslation("GUI_TOOLTIP_TITLE_DEAL_DAMAGE"), HelpBox.FormatTarget.TITLE);
		GameObject gameObject = Choreographer.s_Choreographer.FindClientActorGameObject(actorBeingAttacked);
		if (gameObject != null)
		{
			ActorBehaviour.GetActorBehaviour(gameObject).m_WorldspacePanelUI?.PreviewSimpleDamage(FinalDamage, initialHealth);
		}
		window.Show();
		RefreshDamageInformation();
	}

	private void ClearSelectedToggle()
	{
		currentlyToggled = null;
		RefreshPreviewing();
		ShowDamageTooltip();
		Singleton<UIActiveBonusBar>.Instance.SetInteractionAvailableSlots(interactable: true);
		Singleton<UIUseItemsBar>.Instance.SetItemsInteractable(enable: true);
	}

	private void ShowDamageTooltip()
	{
		if (state == State.ShowingShields)
		{
			StringBuilder stringBuilder = new StringBuilder();
			CAbilityAttack cAbilityAttack = this.targetSummary.OriginalAbility as CAbilityAttack;
			CAttackSummary.TargetSummary targetSummary = new CAttackSummary.TargetSummary(this.targetSummary);
			GameState.PlayerFinishedTogglingShields(targetSummary, apply: false);
			GameState.HandleFinishedTogglingShield(targetSummary, apply: false);
			string damageCalcFormulaString = Choreographer.GetDamageCalcFormulaString(cAbilityAttack.GetLogEvent(ESESubTypeAbility.AbilityPerform), targetSummary.AttackModifierEvent, cAbilityAttack.TargetingActor, targetSummary.ActorToAttack, cAbilityAttack, "COMBAT_LOG_DAMAGE_WHILE_SELECTING_SHIELDS");
			stringBuilder.AppendLine(damageCalcFormulaString);
			stringBuilder.Append(LocalizationManager.GetTranslation("GUI_TOOLTIP_CONFIRM_SHIELDS"));
			Singleton<HelpBox>.Instance.ShowTranslated(stringBuilder.ToString());
			return;
		}
		bool flag = ((FinalDamage > 0 && IsLethalDamage) ? (Singleton<UIActiveBonusBar>.Instance.GetNonSelectedActiveBonus().Count((CActiveBonus it) => !it.Ability.ActiveBonusData.ToggleIsOptional) > 0) : (Singleton<UIActiveBonusBar>.Instance.GetNonSelectedActiveBonus().Count((CActiveBonus it) => !it.Ability.ActiveBonusData.ToggleIsOptional && (!(it is CPreventDamageActiveBonus cPreventDamageActiveBonus) || !cPreventDamageActiveBonus.PreventOnlyIfLethal)) > 0));
		mandatoryTakeDamageHighlight.SetActive(flag);
		if ((currentlyToggled == null) & flag)
		{
			StringBuilder stringBuilder2 = new StringBuilder();
			foreach (CActiveBonus nonSelectedActiveBonu in Singleton<UIActiveBonusBar>.Instance.GetNonSelectedActiveBonus())
			{
				string text = LocalizationNameConverter.MultiLookupLocalization(nonSelectedActiveBonu.Ability.MiscAbilityData?.NameKeyOverride ?? nonSelectedActiveBonu.BaseCard.Name, out var _, suppressErrors: true);
				if (!string.IsNullOrEmpty(text))
				{
					if (stringBuilder2.Length > 0)
					{
						stringBuilder2.AppendFormat("{0} ", LocalizationManager.GetTranslation("AND"));
					}
					stringBuilder2.AppendFormat("<color=\"red\"><font=\"MarcellusSC-Regular SDF\">{0}</font></color> ", text);
				}
			}
			stringBuilder2.Append(LocalizationManager.GetTranslation("GUI_TOOLTIP_DEAL_DAMAGE_MANDATORY_USE"));
			Singleton<HelpBox>.Instance.ShowTranslated(stringBuilder2.ToString());
			return;
		}
		if (damageAbility == null || damageAbility.AbilityType != CAbility.EAbilityType.Wound)
		{
			if (damageAbility == null)
			{
				GameState.DamageData currentDamageData = GameState.CurrentDamageData;
				if (currentDamageData != null && currentDamageData.DamageSourceAbilityType == CAbility.EAbilityType.Wound)
				{
					goto IL_0247;
				}
			}
			if (actorBeingAttacked is CHeroSummonActor cHeroSummonActor)
			{
				if (cHeroSummonActor.IsCompanionSummon)
				{
					InitiativeTrack.Instance.helpBox.ShowTranslated(string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_DEAL_DAMAGE_COMPANION"), LocalizationManager.GetTranslation(cHeroSummonActor.ActorLocKey()), LocalizationManager.GetTranslation(cHeroSummonActor.Summoner.ActorLocKey())), LocalizationManager.GetTranslation("GUI_TOOLTIP_TITLE_DEAL_DAMAGE"), HelpBox.FormatTarget.TITLE);
				}
				else
				{
					InitiativeTrack.Instance.helpBox.Show("GUI_TOOLTIP_DEAL_DAMAGE_SUMMON", "GUI_TOOLTIP_DEAL_DAMAGE_SUMMON_TITLE", HelpBox.FormatTarget.TITLE);
				}
			}
			else
			{
				InitiativeTrack.Instance.helpBox.Show("GUI_TOOLTIP_DEAL_DAMAGE", "GUI_TOOLTIP_TITLE_DEAL_DAMAGE", HelpBox.FormatTarget.TITLE);
			}
			return;
		}
		goto IL_0247;
		IL_0247:
		Singleton<HelpBox>.Instance.ShowTranslated(string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_PLAYER_WOUNDED"), LocalizationManager.GetTranslation(actorBeingAttacked.ActorLocKey())));
	}

	public void RefreshDamageTooltip()
	{
		if (IsOpen && state != State.None && targetSummary != null)
		{
			ShowDamageTooltip();
		}
	}

	public bool IsValid()
	{
		if (IsOpen && state != State.None)
		{
			return targetSummary != null;
		}
		return false;
	}

	public void ClearPreviouslyPreviewed()
	{
		currentlyPreviewing = PreviewOptions.None;
	}

	public void ClearTakeDamageInvoked()
	{
		m_TakeDamageInvoked = false;
	}

	private void UpdateTakeDamageOptionVisuals()
	{
		try
		{
			if (gameObject == null || !gameObject.scene.isLoaded || actorBeingAttacked == null)
			{
				return;
			}
			if (!IsLethalDamage)
			{
				fatalDamageObject.SetActive(value: false);
				damageObject.gameObject.SetActive(value: true);
				TMP_Text tMP_Text = damageAmount;
				Color color = (takeDamageText.color = UIInfoTools.DefInstance.basicTextColor);
				tMP_Text.color = color;
			}
			else
			{
				damageObject.gameObject.SetActive(value: false);
				fatalDamageObject.SetActive(value: true);
				TMP_Text tMP_Text2 = damageAmount;
				Color color = (takeDamageText.color = UIInfoTools.DefInstance.negativeTextColor);
				tMP_Text2.color = color;
			}
			takeDamageButton.interactable = ThisPlayerHasTakeDamageControl;
			changeSelectionButton.interactable = ThisPlayerHasTakeDamageControl;
			if (state == State.ShowingBurnCardForAlly)
			{
				if (changeSelectionTooltip != null)
				{
					changeSelectionTooltip.tooltipText = null;
				}
			}
			else if (takeDamageTooltip != null)
			{
				if (state == State.ShowingShields)
				{
					takeDamageTooltip.tooltipText = "GUI_CONFIRM_SHIELDS_TOOLTIP";
				}
				else if (state == State.ShowingPrevents)
				{
					takeDamageTooltip.tooltipText = "GUI_TAKE_DAMAGE_TOOLTIP";
				}
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the TakeDamagePanel.UpdateTakeDamageOptionVisuals().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00035", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void SetupTakeDamageInteractability(InteractabilityIsolatedUIControl control)
	{
		takeDamageButtonInteractability = new InteractabilityIsolatedUIControl.InteractabilityReference
		{
			cachedInteractable = false,
			control = control,
			onInteractabilityChanged = () =>
			{
				if (window.IsOpen)
				{
					RefreshButtons(cardBurn: true, resetToggles: false);
				}
			}
		};
	}

	public void SetupBurnAvailableCardsToggleInteractability(InteractabilityIsolatedUIControl control)
	{
		burnAvailableCardsToggleInteractability = new InteractabilityIsolatedUIControl.InteractabilityReference
		{
			cachedInteractable = false,
			control = control,
			onInteractabilityChanged = () =>
			{
				if (window.IsOpen)
				{
					RefreshButtons(cardBurn: true, resetToggles: false);
				}
			}
		};
	}

	public void SetupBurnDiscardedCardsInteractability(InteractabilityIsolatedUIControl control)
	{
		burnDiscardedCardsInteractability = new InteractabilityIsolatedUIControl.InteractabilityReference
		{
			cachedInteractable = false,
			control = control,
			onInteractabilityChanged = () =>
			{
				if (window.IsOpen)
				{
					RefreshButtons(cardBurn: true, resetToggles: false);
				}
			}
		};
	}

	private void UpdateCardRemovalOptionVisuals(bool resetToggles = true)
	{
		try
		{
			if (!(gameObject == null) && gameObject.scene.isLoaded)
			{
				if (resetToggles)
				{
					ResetToggles();
				}
				bool thisPlayerHasTakeDamageControl = ThisPlayerHasTakeDamageControl;
				hasEnoughAvailableCards = actorToShowCardsFor != null && actorToShowCardsFor.CharacterClass.HandAbilityCards.Count > 0;
				burnAvailableCardsToggle.interactable = (hasEnoughAvailableCards & thisPlayerHasTakeDamageControl) && InteractabilityManager.ShouldAllowClickForFHToggle(burnAvailableCardsToggle);
				hasEnoughDiscardedCards = actorToShowCardsFor != null && actorToShowCardsFor.CharacterClass.DiscardedAbilityCards.Count > 1;
				burnDiscardedCardsToggle.interactable = (hasEnoughDiscardedCards & thisPlayerHasTakeDamageControl) && InteractabilityManager.ShouldAllowClickForFHToggle(burnDiscardedCardsToggle);
				if (ScenarioScript_FH100.IsActive)
				{
					burnAvailableCardsToggle.interactable = true;
					burnDiscardedCardsToggle.interactable = true;
				}
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the TakeDamagePanel.UpdateCardRemovalOptionVisuals().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00036", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void ResetToggles()
	{
		burnAvailableCardsToggle.SetValue(value: false);
		burnDiscardedCardsToggle.SetValue(value: false);
		currentlyToggled = null;
		burnDiscardedCardsToggle.group.SetAllTogglesOff(sendCallback: false);
		Singleton<UIUseItemsBar>.Instance?.Hide();
		Singleton<UIActiveBonusBar>.Instance?.Hide();
		toggledShieldItems.Clear();
		toggledActiveBonuses.Clear();
		addedShield = 0;
		preventAllDamage = false;
	}

	public void OnMouseEnterTakeDamage()
	{
		if (!InteractabilityManager.ShouldAllowClickForFHButton(takeDamageButton) && window.IsVisible)
		{
			Log.GUIInfo("Mouse Enter for button " + takeDamageButton.name + " intercepted and prevented by InteractabilityManager");
		}
		else if (IsOpen && state != State.None && targetSummary != null)
		{
			PreviewDamage();
		}
	}

	public void OnMouseExitTakeDamage()
	{
		if (!InteractabilityManager.ShouldAllowClickForFHButton(takeDamageButton) && window.IsVisible)
		{
			Log.GUIInfo("Mouse Exit for button " + takeDamageButton.name + " intercepted and prevented by InteractabilityManager");
		}
		else
		{
			ResetPreviewing();
		}
	}

	public void OnMouseEnterBurnOne()
	{
		if (!InteractabilityManager.ShouldAllowClickForFHToggle(burnAvailableCardsToggle) && window.IsVisible)
		{
			Log.GUIInfo("Mouse Enter for button " + burnAvailableCardsToggle.name + " intercepted and prevented by InteractabilityManager");
		}
		else
		{
			PreviewAvailableCards();
		}
	}

	public void OnMouseExitBurnOne()
	{
		if (!InteractabilityManager.ShouldAllowClickForFHToggle(burnAvailableCardsToggle) && window.IsVisible)
		{
			Log.GUIInfo("Mouse Exit for button " + burnAvailableCardsToggle.name + " intercepted and prevented by InteractabilityManager");
		}
		else
		{
			ResetPreviewing();
		}
	}

	public void OnMouseEnterBurnTwo()
	{
		if (!InteractabilityManager.ShouldAllowClickForFHToggle(burnDiscardedCardsToggle) && window.IsVisible)
		{
			Log.GUIInfo("Mouse Enter for button " + burnDiscardedCardsToggle.name + " intercepted and prevented by InteractabilityManager");
		}
		else
		{
			PreviewDiscardedCards();
		}
	}

	public void OnMouseExitBurnTwo()
	{
		if (!InteractabilityManager.ShouldAllowClickForFHToggle(burnDiscardedCardsToggle) && window.IsVisible)
		{
			Log.GUIInfo("Mouse Exit for button " + burnDiscardedCardsToggle.name + " intercepted and prevented by InteractabilityManager");
		}
		else
		{
			ResetPreviewing();
		}
	}

	public void PreviewFullHand()
	{
		try
		{
			currentlyPreviewing = PreviewOptions.PreviewFullHand;
			Singleton<CardsHandManager>.Instance.EnableCancelActiveAbilities = false;
			Singleton<CardsHandManager>.Instance.Show(actorToShowCardsFor, CardHandMode.LoseCard, CardPileType.Any, CardPileType.Any, 0, fadeUnselectableCards: false, highlightSelectableCards: false, allowFullCardPreview: true, CardsHandUI.CardActionsCommand.NONE, forceUseCurrentRoundCards: true);
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the TakeDamagePanel.PreviewFullHand().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00037", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void PreviewAvailableCards()
	{
		try
		{
			if (!ScenarioScript_FH100.IsActive && hasEnoughAvailableCards)
			{
				currentlyPreviewing = PreviewOptions.PreviewAvailableCards;
				Singleton<CardsHandManager>.Instance.EnableCancelActiveAbilities = false;
				Singleton<CardsHandManager>.Instance.Show(actorToShowCardsFor, CardHandMode.LoseCard, CardPileType.Any, CardPileType.Hand, 1, fadeUnselectableCards: true, highlightSelectableCards: true, allowFullCardPreview: true, CardsHandUI.CardActionsCommand.NONE, forceUseCurrentRoundCards: true);
				Singleton<HelpBox>.Instance.OverrideControllerOrKeyboardTip(() => string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_GAMEPAD_BURN_1_ABILITY_CARD"), Singleton<InputFHManager>.Instance.InputContext.GetDisplayTextFor(InputFHManager.Controls.Generic.Confirm)), LocalizationManager.GetTranslation("GUI_TAKE_DAMAGE_BURN"));
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the TakeDamagePanel.PreviewAvailableCards().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00038", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void PreviewDiscardedCards()
	{
		try
		{
			if (!ScenarioScript_FH100.IsActive && hasEnoughDiscardedCards)
			{
				currentlyPreviewing = PreviewOptions.PreviewDiscardedCards;
				Singleton<CardsHandManager>.Instance.EnableCancelActiveAbilities = false;
				Singleton<CardsHandManager>.Instance.Show(actorToShowCardsFor, CardHandMode.LoseCard, CardPileType.Any, CardPileType.Discarded, 2, fadeUnselectableCards: true, highlightSelectableCards: true, allowFullCardPreview: true, CardsHandUI.CardActionsCommand.NONE, forceUseCurrentRoundCards: true);
				Singleton<HelpBox>.Instance.OverrideControllerOrKeyboardTip(() => string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_GAMEPAD_BURN_2_DISCARDED_ABILITY_CARDS"), Singleton<InputFHManager>.Instance.InputContext.GetDisplayTextFor(InputFHManager.Controls.Generic.Confirm)), LocalizationManager.GetTranslation("GUI_TAKE_DAMAGE_DISCARD"));
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the TakeDamagePanel.PreviewDiscardedCards().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00039", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void PreviewDamage()
	{
		if (!IsOpen || state == State.None || targetSummary == null)
		{
			return;
		}
		try
		{
			currentlyPreviewing = PreviewOptions.PreviewDamage;
			Singleton<HelpBox>.Instance.ClearOverrideController();
			HideCardPreview();
			RefreshDamageInformation();
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the TakeDamagePanel.PreviewDamage().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00040", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private int CalculateCurrentHealth()
	{
		return Mathf.Min(actorBeingAttacked.Health + FinalDamage, actorBeingAttacked.MaxHealth);
	}

	public void RefreshDamageInformation()
	{
		int finalDamage = FinalDamage;
		if (state == State.ShowingBurnCardForAlly)
		{
			takeDamageButton.gameObject.SetActive(value: false);
			changeSelectionButton.gameObject.SetActive(value: true);
		}
		else
		{
			takeDamageButton.gameObject.SetActive(value: true);
			changeSelectionButton.gameObject.SetActive(value: false);
			damageAmount.text = finalDamage.ToString();
			damageAmount.color = ((preventAllDamage || addedShield > 0) ? shieldAppliedColor : defaultColor);
			damageObject.color = ((preventAllDamage || addedShield > 0) ? shieldAppliedColor : defaultColor);
			takeDamageButton.interactable = ThisPlayerHasTakeDamageControl;
		}
		RefreshDamageTooltip();
		RefreshButtons(cardBurn: false, resetToggles: false);
		if (currentlyToggled == null || currentlyPreviewing == PreviewOptions.PreviewDamage)
		{
			actorHUDController?.PreviewSimpleDamage(finalDamage, initialHealth);
			if (finalDamage == 0)
			{
				Singleton<UIActiveBonusBar>.Instance.RefreshLethalReduceDamageActiveBonuses(actorBeingAttacked as CPlayerActor, IsLethalDamage);
				Singleton<UIActiveBonusBar>.Instance.SetInteractionAvailableSlots(interactable: false, onlyChangeUnselected: true);
				Singleton<UIUseItemsBar>.Instance.SetItemsInteractable(enable: false, toggledShieldItems, (CItem item) => item.GetVar("NotAShieldEffect").GetBool());
			}
			else
			{
				Singleton<UIActiveBonusBar>.Instance.SetInteractionAvailableSlots(interactable: true);
				Singleton<UIActiveBonusBar>.Instance.RefreshLethalReduceDamageActiveBonuses(actorBeingAttacked as CPlayerActor, IsLethalDamage);
				Singleton<UIUseItemsBar>.Instance.SetItemsInteractable(enable: true);
			}
		}
		Singleton<FHScenarioViewController>.Instance?.Modules.SimpleActionBox.Refresh(immediate: false);
	}

	private int CalculateCurrentDamage()
	{
		preventAllDamage = false;
		m_DamageToPrevent = 0;
		foreach (CActiveBonus toggledActiveBonuse in toggledActiveBonuses)
		{
			if (!(toggledActiveBonuse is CPreventDamageActiveBonus))
			{
				continue;
			}
			if (toggledActiveBonuse.Ability.ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.PreventBySettingRetaliateToValue))
			{
				int num = damageToTake;
				if (num > 1)
				{
					m_DamageToPrevent = num - toggledActiveBonuse.Ability.Strength;
				}
				else
				{
					m_DamageToPrevent = num;
				}
				preventAllDamage = false;
			}
			else if (!toggledActiveBonuse.Ability.ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.PreventPartialDamage))
			{
				preventAllDamage = true;
				m_DamageToPrevent = 0;
			}
			else
			{
				preventAllDamage = false;
				m_DamageToPrevent = toggledActiveBonuse.Ability.Strength;
			}
		}
		Log.GUIInfo("TakeDamagePanel Calculate Current Damage: bonuses[" + toggledActiveBonuses.Count + "], preventedAll[" + preventAllDamage + "], DamageToPrevent[" + m_DamageToPrevent + "]");
		if (preventAllDamage)
		{
			return 0;
		}
		CalculateAddedShield();
		int num2 = targetSummary.CalcExpressionString(Mathf.Max(0, addedShield - pierce), calcPostShieldScalars: true, calcShield: true);
		return Mathf.Max(0, num2 - m_DamageToPrevent);
	}

	private void ResetPreviewing()
	{
		try
		{
			currentlyPreviewing = PreviewOptions.None;
			if (burnAvailableCardsToggle.Equals(currentlyToggled))
			{
				BurnAvailableCard();
				return;
			}
			if (burnDiscardedCardsToggle.Equals(currentlyToggled))
			{
				BurnDiscardedCards();
				return;
			}
			if (window.IsOpen)
			{
				actorHUDController?.PreviewSimpleDamage(FinalDamage, initialHealth);
			}
			HideCardPreview();
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the TakeDamagePanel.ResetPreviewing().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00041", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void RefreshDamagePreview()
	{
		if (IsOpen && state != State.None && targetSummary != null)
		{
			if (currentlyToggled == null || currentlyPreviewing == PreviewOptions.PreviewDamage)
			{
				actorHUDController?.PreviewSimpleDamage(FinalDamage, initialHealth);
			}
			else
			{
				actorHUDController?.ResetDamagePreview(FinalDamage);
			}
		}
	}

	private void RefreshPreviewing()
	{
		if (IsOpen && state != State.None && targetSummary != null)
		{
			if (currentlyToggled == null)
			{
				actorHUDController?.PreviewSimpleDamage(FinalDamage, initialHealth);
			}
			else
			{
				actorHUDController?.ResetDamagePreview(FinalDamage);
			}
			switch (currentlyPreviewing)
			{
			case PreviewOptions.None:
				HideCardPreview();
				break;
			case PreviewOptions.PreviewFullHand:
				PreviewFullHand();
				break;
			case PreviewOptions.PreviewAvailableCards:
				PreviewAvailableCards();
				break;
			case PreviewOptions.PreviewDiscardedCards:
				PreviewDiscardedCards();
				break;
			case PreviewOptions.PreviewDamage:
				PreviewDamage();
				break;
			default:
				Log.GUIError("Unknown preview option encountered for the Take Damage Panel.");
				break;
			}
		}
	}

	public void BurnAvailableCard(bool toggledOn = true)
	{
		try
		{
			if (toggledOn)
			{
				if (ScenarioScript_FH100.IsActive)
				{
					burnAvailableCardsToggle.SetIsOnWithoutNotify(value: false);
					string translation = LocalizationManager.GetTranslation("FH_Scenario_Campaign_100_SCENARIO_MODIFIER_4");
					translation = translation.Replace("{STRENGTH}", "1");
					Singleton<UIConfirmationBoxManager>.Instance.ShowGenericConfirmation(LocalizationManager.GetTranslation("GUI_CONFIRMATION"), translation, () =>
					{
						Singleton<UIActiveBonusBar>.Instance.Hide(toggle: true);
						GameState.PlayerAvoidingDamage(GameState.EAvoidDamageOption.Lose1HandCard, actorToShowCardsFor);
						if (FFSNetwork.IsOnline && actorToShowCardsFor.IsUnderMyControl())
						{
							Synchronizer.SendGameAction(GameActionType.BurnAvailableCard, (state == State.ShowingBurnCardForAlly) ? ActionPhaseType.ChooseCardsToBurnForAllyMonster : ActionPhaseType.TakeDamageConfirmation, validateOnServerBeforeExecuting: false, disableAutoReplication: false, actorToShowCardsFor.ID, 0, 0, 0, supplementaryDataBoolean: false, null, new CardsToken(new List<AbilityCardUI>()));
						}
						ResetAndHide();
					});
				}
				else
				{
					UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.PlayerChoseBurnAvailable));
					OnSelectedToggle(burnAvailableCardsToggle);
					PreviewAvailableCards();
				}
			}
			else if (currentlyToggled == burnAvailableCardsToggle)
			{
				ClearSelectedToggle();
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the TakeDamagePanel.BurnAvailableCard().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00042", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void HideCardPreview()
	{
		Singleton<CardsHandManager>.Instance.Hide();
	}

	public void ConfirmShields()
	{
		try
		{
			burnDiscardedCardsToggle.group.SetAllTogglesOff();
			currentlyToggled = null;
			ClearPreviouslyPreviewed();
			if (!CanTakeDamage() || m_TakeDamageInvoked)
			{
				return;
			}
			m_TakeDamageInvoked = true;
			if (FFSNetwork.IsOnline && ThisPlayerHasTakeDamageControl)
			{
				Synchronizer.SendGameAction(GameActionType.ConfirmShields, ActionPhaseType.ShieldConfirmation, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, 0, 0, supplementaryDataBoolean: false, actorBeingAttacked.ActorGuid, new ItemsToken(toggledShieldItems), new ActiveBonusesToken(toggledActiveBonuses));
			}
			actorBeingAttacked.Inventory.LockInSelectedItemsAndResetUnselected();
			GameState.CalcShieldPierceResult(targetSummary.ActorToAttack.FindApplicableActiveBonuses(CAbility.EAbilityType.Shield), toggledShieldItems, targetSummary);
			foreach (CItem toggledShieldItem in toggledShieldItems)
			{
				if (toggledShieldItem.YMLData.Data.Abilities != null && toggledShieldItem.YMLData.Data.Abilities.Count > 0)
				{
					foreach (CAbility ability in toggledShieldItem.YMLData.Data.Abilities)
					{
						if (!(ability is CAbilityTargeting cAbilityTargeting) || !(PhaseManager.CurrentPhase is CPhaseAction cPhaseAction))
						{
							continue;
						}
						cPhaseAction.StackNextAbilities(new List<CAbility> { cAbilityTargeting }, actorBeingAttacked);
						CBaseCard baseCard = actorBeingAttacked.FindCardWithAbility(cAbilityTargeting);
						if (baseCard == null || !((actorToShowCardsFor?.CharacterClass.ActivatedCards.FindAll((CBaseCard x) => x is CItem)).SingleOrDefault((CBaseCard s) => s.ID.Equals(baseCard.ID)) is CItem { ActiveBonuses: { Count: >0 } } cItem))
						{
							continue;
						}
						foreach (CActiveBonus activeBonuse in cItem.ActiveBonuses)
						{
							if (activeBonuse.Ability == null || !activeBonuse.Ability.Equals(cAbilityTargeting))
							{
								continue;
							}
							foreach (CBespokeBehaviour bespokeBehaviour in activeBonuse.BespokeBehaviours)
							{
								bespokeBehaviour?.OnBehaviourTriggered();
							}
							activeBonuse.UpdateXPTracker();
						}
					}
				}
				if (toggledShieldItem.YMLData.Data.RetaliateValue > 0 && actorAttacking != null)
				{
					CAbilityRetaliate.SingleShotRetaliate(toggledShieldItem.YMLData.Data.RetaliateValue, toggledShieldItem.YMLData.Data.RetaliateRange, actorAttacking, actorBeingAttacked, damageAbility);
				}
			}
			GameState.PlayerFinishedTogglingShields(targetSummary);
			if (actorToShowCardsFor != null && ThisPlayerHasTakeDamageControl)
			{
				Singleton<CardsHandManager>.Instance.Hide(actorToShowCardsFor);
			}
			ResetAndHide(stopActiveBonus: false, switchBackToSavedState: true, Lockin: false);
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the TakeDamagePanel.TakeDamage().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00043", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void ChooseOtherPlayerToBurnCard()
	{
		try
		{
			burnDiscardedCardsToggle.group.SetAllTogglesOff();
			currentlyToggled = null;
			ClearPreviouslyPreviewed();
			if (!m_TakeDamageInvoked)
			{
				m_TakeDamageInvoked = true;
				ResetAndHide(stopActiveBonus: false, switchBackToSavedState: false, Lockin: false);
				Singleton<UIScenarioDistributePointsManager>.Instance.ProxyChoosePlayerToBurnCardsForAlly(null, targetSummary);
				if (actorToShowCardsFor != null && ThisPlayerHasTakeDamageControl)
				{
					Singleton<CardsHandManager>.Instance.Hide(actorToShowCardsFor);
				}
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the TakeDamagePanel.ChooseOtherPlayerToBurnCard().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00043", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void TakeDamage()
	{
		try
		{
			burnDiscardedCardsToggle.group.SetAllTogglesOff();
			currentlyToggled = null;
			ClearPreviouslyPreviewed();
			if (CanTakeDamage() && !m_TakeDamageInvoked)
			{
				m_TakeDamageInvoked = true;
				UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.PlayerChoseDamage));
				if (FFSNetwork.IsOnline && ThisPlayerHasTakeDamageControl)
				{
					Synchronizer.SendGameAction(GameActionType.TakeDamage, ActionPhaseType.TakeDamageConfirmation, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, 0, 0, supplementaryDataBoolean: false, actorBeingAttacked.ActorGuid, new ItemsToken(toggledShieldItems), new ActiveBonusesToken(toggledActiveBonuses));
				}
				actorBeingAttacked.Inventory.LockInSelectedItemsAndResetUnselected();
				if (actorBeingAttacked != actorToShowCardsFor && actorToShowCardsFor?.Inventory != null)
				{
					actorToShowCardsFor.Inventory.LockInSelectedItemsAndResetUnselected();
				}
				if (!preventAllDamage)
				{
					_ = m_DamageToPrevent;
					_ = addedShield;
					_ = pierce;
				}
				else
				{
					_ = damageToTake;
				}
				if (!preventAllDamage)
				{
					Mathf.Max(0, Mathf.Min(damageToTake, addedShield - pierce + m_DamageToPrevent));
				}
				else
				{
					_ = damageToTake;
				}
				if (preventAllDamage && (FinalDamage > 0 || state == State.ShowingPrevents))
				{
					GameState.UsePreventDamageBonusToAvoidAttack(toggledActiveBonuses);
					GameState.PlayerAvoidingDamage(GameState.EAvoidDamageOption.UsePreventDamageActiveBonus);
				}
				else
				{
					GameState.PlayerNotAvoidingDamage(toggledActiveBonuses);
				}
				if (actorToShowCardsFor != null && ThisPlayerHasTakeDamageControl)
				{
					Singleton<CardsHandManager>.Instance.Hide(actorToShowCardsFor);
				}
				ResetAndHide(stopActiveBonus: false, switchBackToSavedState: true, Lockin: false);
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the TakeDamagePanel.TakeDamage().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00043", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private bool CanTakeDamage()
	{
		if (FinalDamage == 0)
		{
			return true;
		}
		List<CActiveBonus> list = Singleton<UIActiveBonusBar>.Instance.ShowingActiveBonuses.Where((CActiveBonus a) => !a.Ability.ActiveBonusData.ToggleIsOptional).ToList();
		if (list.Count == 0)
		{
			return true;
		}
		if (actorBeingAttacked.Health + addedShield > 0)
		{
			for (int num = list.Count - 1; num >= 0; num--)
			{
				CActiveBonus cActiveBonus = list[num];
				if (cActiveBonus is CPreventDamageActiveBonus { PreventOnlyIfLethal: not false })
				{
					list.Remove(cActiveBonus);
				}
			}
		}
		if (preventAllDamage || (toggledActiveBonuses.Count == list.Count && !list.Except(toggledActiveBonuses).Any()))
		{
			return true;
		}
		Singleton<HelpBox>.Instance.HighlightWarning();
		Singleton<UIActiveBonusBar>.Instance.HighlightMandatoryActiveBonus();
		return false;
	}

	public void ToggleShieldItem(CItem item, bool networkActionIfOnline = true)
	{
		try
		{
			if (!toggledShieldItems.Contains(item))
			{
				if (item.YMLData.Data.ShieldValue > 0 || item.YMLData.Data.RetaliateValue > 0 || item.YMLData.Data.Abilities.Count > 0)
				{
					toggledShieldItems.Add(item);
					actorBeingAttacked.Inventory.SelectItem(item);
				}
			}
			else
			{
				DeselectShieldItem(item);
			}
			GameState.CurrentAttackAbility.AttackSummary.RefreshingTargetDataAfterToggleItemOrActiveBonus(GameState.CurrentAttackAbility, actorBeingAttacked, targetSummary);
			actorBeingAttacked.CheckForCachedValuesAfterActiveBonusesUpdate();
			CalculateAddedShield();
			RefreshDamageInformation();
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the TakeDamagePanel.ToggleShieldItem().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00044", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void DeselectAllShieldItems(bool stopActiveBonus = false)
	{
		if (stopActiveBonus)
		{
			if (Singleton<UIActiveBonusBar>.Instance.IsShowing)
			{
				Singleton<UIActiveBonusBar>.Instance.UndoSelection();
			}
			toggledActiveBonuses.Clear();
			preventAllDamage = false;
		}
		if (toggledShieldItems.Count > 0)
		{
			for (int num = toggledShieldItems.Count - 1; num >= 0; num--)
			{
				CItem item = toggledShieldItems[num];
				DeselectShieldItem(item);
			}
		}
		CalculateAddedShield();
	}

	private void DeselectShieldItem(CItem item)
	{
		if (item != null && (item.YMLData.Data.ShieldValue > 0 || item.YMLData.Data.RetaliateValue > 0 || item.YMLData.Data.Abilities.Count > 0))
		{
			toggledShieldItems.Remove(item);
			actorBeingAttacked.Inventory.DeselectItem(item);
		}
	}

	public void ToggleActiveBonus(CActiveBonus activeBonus, CActor actor)
	{
		try
		{
			if (toggledActiveBonuses.Contains(activeBonus))
			{
				toggledActiveBonuses.Remove(activeBonus);
			}
			else
			{
				toggledActiveBonuses.Add(activeBonus);
			}
			CalculateAddedShield();
			RefreshDamageInformation();
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the TakeDamagePanel.TogglePreventDamageActiveBonus().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00045", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void CalculateAddedShield()
	{
		if (!IsValid())
		{
			return;
		}
		pierce = targetSummary.Pierce;
		if (state == State.ShowingPrevents || state == State.ShowingBurnCardForAlly)
		{
			addedShield = targetSummary.Shield;
		}
		else if (state == State.ShowingShields)
		{
			int num = addedShield;
			addedShield = actorBeingAttacked.CalculateShield(targetSummary.AttackAbilityWithOverrides, ignoreNeutralizeShield: false, includeItemShieldValues: true, cache: true, forceActiveItems: true);
			if (addedShield != num)
			{
				actorBeingAttacked.CheckForCachedValuesAfterActiveBonusesUpdate();
			}
		}
	}

	private void OnSelectedToggle(FHToggle toggle)
	{
		currentlyToggled = toggle;
		DeselectAllShieldItems(stopActiveBonus: true);
		RefreshDamageInformation();
		Choreographer.s_Choreographer.readyButton.Toggle(active: false, ReadyButton.EButtonState.EREADYBUTTONNA, null, hideOnClick: true, glowingEffect: false, interactable: true, disregardTurnControlForInteractability: false, haltActionProcessorIfDeactivated: false);
		actorHUDController?.ResetDamagePreview(FinalDamage);
		Singleton<UIActiveBonusBar>.Instance.SetInteractionAvailableSlots(interactable: false, onlyChangeUnselected: true);
		Singleton<UIUseItemsBar>.Instance.SetItemsInteractable(enable: false);
	}

	public void BurnDiscardedCards(bool toggledOn = true)
	{
		try
		{
			if (toggledOn)
			{
				if (ScenarioScript_FH100.IsActive)
				{
					burnDiscardedCardsToggle.SetIsOnWithoutNotify(value: false);
					string translation = LocalizationManager.GetTranslation("FH_Scenario_Campaign_100_SCENARIO_MODIFIER_4");
					translation = translation.Replace("{STRENGTH}", "2");
					Singleton<UIConfirmationBoxManager>.Instance.ShowGenericConfirmation(LocalizationManager.GetTranslation("GUI_CONFIRMATION"), translation, () =>
					{
						Singleton<UIActiveBonusBar>.Instance.Hide(toggle: true);
						GameState.PlayerAvoidingDamage(GameState.EAvoidDamageOption.Lose2DiscardCards, actorToShowCardsFor);
						if (FFSNetwork.IsOnline && actorToShowCardsFor.IsUnderMyControl())
						{
							Synchronizer.SendGameAction(GameActionType.BurnDiscardedCards, ActionPhaseType.TakeDamageConfirmation, validateOnServerBeforeExecuting: false, disableAutoReplication: false, actorToShowCardsFor.ID, 0, 0, 0, supplementaryDataBoolean: false, null, new CardsToken(new List<AbilityCardUI>()));
						}
						ResetAndHide();
					});
				}
				else
				{
					UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.PlayerChoseBurnDiscarded));
					OnSelectedToggle(burnDiscardedCardsToggle);
					PreviewDiscardedCards();
				}
			}
			else if (currentlyToggled == burnDiscardedCardsToggle)
			{
				ClearSelectedToggle();
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the TakeDamagePanel.BurnDiscardedCards().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00046", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void ResetAndHide(bool stopActiveBonus = false, bool switchBackToSavedState = false, bool Lockin = true)
	{
		if (state != State.None)
		{
			Singleton<HelpBox>.Instance.Hide();
			DeselectAllShieldItems(stopActiveBonus);
			ResetToggles();
			window.Hide();
			if (Lockin)
			{
				actorBeingAttacked?.Inventory.LockInSelectedItemsAndResetUnselected();
			}
			if ((FFSNetwork.IsOnline && ActionProcessor.HasSavedState) & switchBackToSavedState)
			{
				ActionProcessor.SetState(ActionProcessorStateType.SwitchBackToSavedState);
			}
			Singleton<FHScenarioViewController>.Instance?.Modules.SimpleActionBox.Refresh(immediate: true);
			while (pendingGameActions.Count > 0)
			{
				GameAction gameAction = pendingGameActions[0];
				pendingGameActions.RemoveAt(0);
				gameAction.Execute();
			}
			m_DamageToPrevent = 0;
			preventAllDamage = false;
			actorBeingAttacked = null;
			actorToShowCardsFor = null;
			actorAttacking = null;
			damageAbility = null;
			pierce = 0;
			isDirectDamage = false;
			allowBurnCards = false;
			addedShield = 0;
			initialHealth = 0;
		}
	}

	public void ToggleVisibility(bool visible)
	{
		if (window.IsOpen)
		{
			if (visible)
			{
				buttonsContainer.SetActive(value: true);
				ShowDamageTooltip();
			}
			else
			{
				buttonsContainer.SetActive(value: false);
				Singleton<HelpBox>.Instance.Hide();
			}
		}
	}

	public void ShowOtherPlayer(CActor actorBeingAttacked, CPlayerActor actorToShowCardsFor, int damageToTake, int pierce, bool isDirectDamage, CAbility damageAbility, CAttackSummary.TargetSummary targetSummary)
	{
		ResetToggles();
		state = State.ShowingPrevents;
		this.targetSummary = targetSummary;
		this.actorBeingAttacked = actorBeingAttacked;
		this.actorToShowCardsFor = actorToShowCardsFor;
		actorAttacking = damageAbility?.TargetingActor;
		this.damageAbility = damageAbility;
		this.pierce = pierce;
		this.isDirectDamage = isDirectDamage;
		addedShield = 0;
		preventAllDamage = false;
		initialHealth = CalculateCurrentHealth();
		WorldspacePanelUIController[] array = WorldspaceUITools.Instance?.GetComponentsInChildren<WorldspacePanelUIController>(includeInactive: false);
		for (int i = 0; i < array?.Length; i++)
		{
			if (array[i].InfoUI.m_ActorBehavior.Actor.Equals(actorBeingAttacked))
			{
				actorHUDController = array[i];
				break;
			}
		}
		if (actorToShowCardsFor == null)
		{
			actorToShowCardsFor = ((!(actorBeingAttacked is CHeroSummonActor cHeroSummonActor)) ? ScenarioManager.Scenario.PlayerActors[0] : cHeroSummonActor.Summoner);
		}
		burnAvailableCardsToggle.gameObject.SetActive(value: false);
		burnDiscardedCardsToggle.gameObject.SetActive(value: false);
		takeDamageButton.gameObject.SetActive(value: false);
		changeSelectionButton.gameObject.SetActive(value: false);
		int controllableID = ControllableRegistry.GetControllableID(actorToShowCardsFor);
		InitiativeTrack.Instance.helpBox.ShowTranslated(string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_DEAL_DAMAGE_MP"), ControllableRegistry.GetController(controllableID).Username), LocalizationManager.GetTranslation("GUI_TOOLTIP_TITLE_DEAL_DAMAGE"), HelpBox.FormatTarget.TITLE);
		GameObject gameObject = Choreographer.s_Choreographer.FindClientActorGameObject(actorBeingAttacked);
		if (gameObject != null)
		{
			ActorBehaviour.GetActorBehaviour(gameObject).m_WorldspacePanelUI?.PreviewSimpleDamage(FinalDamage, initialHealth);
		}
		window.Show();
		RefreshDamageInformation();
	}

	public void ProxyConfirmShields(GameAction gameAction)
	{
		CActor cActor = ScenarioManager.FindActor(gameAction.SupplementaryDataString);
		if (cActor != actorBeingAttacked && IsOpen)
		{
			pendingGameActions.Add(gameAction);
			return;
		}
		if (actorBeingAttacked != cActor && actorBeingAttacked != null)
		{
			Log.Error($"ProxyConfirmShields() invoked with different actor: {cActor} instead of {actorBeingAttacked}");
		}
		actorBeingAttacked = cActor;
		if (actorToShowCardsFor == null)
		{
			actorToShowCardsFor = actorBeingAttacked.PlayerActor;
		}
		List<CItem> source = actorBeingAttacked.Inventory.AllItemsAndPets.ToList();
		uint[] activatedItemNetworkIDs = ((ItemsToken)gameAction.SupplementaryDataToken).ItemNetworkIDs;
		int i;
		for (i = 0; i < activatedItemNetworkIDs.Length; i++)
		{
			CItem cItem = source.FirstOrDefault((CItem x) => x.NetworkID == activatedItemNetworkIDs[i]);
			if (cItem != null)
			{
				ToggleShieldItem(cItem);
				continue;
			}
			throw new Exception("Error applying shield item to mitigate damage. Item returns null (NetworkID: " + activatedItemNetworkIDs[i] + ").");
		}
		CAbility.EAbilityType abilityType = ((damageAbility != null) ? damageAbility.AbilityType : CAbility.EAbilityType.None);
		List<CActiveBonus> preventDamageActiveBonuses = Singleton<UIActiveBonusBar>.Instance.GetPreventDamageActiveBonuses(actorBeingAttacked, abilityType, IsLethalDamage, FinalDamage);
		preventDamageActiveBonuses.AddRange(Singleton<UIActiveBonusBar>.Instance.GetShieldIncomingAttackActiveBonuses(actorBeingAttacked, abilityType));
		ActiveBonusesToken.ActiveBonusData[] appliedActiveBonusesData = ((ActiveBonusesToken)gameAction.SupplementaryDataToken2).ActiveBonuses;
		int i2;
		for (i2 = 0; i2 < appliedActiveBonusesData.Length; i2++)
		{
			CActiveBonus cActiveBonus = preventDamageActiveBonuses.FirstOrDefault((CActiveBonus x) => x.Ability.Name == appliedActiveBonusesData[i2].AbilityName && x.BaseCard.ID == appliedActiveBonusesData[i2].BaseCardID);
			if (cActiveBonus != null)
			{
				ToggleActiveBonus(cActiveBonus, actorBeingAttacked);
				ScenarioRuleClient.ToggleActiveBonus(cActiveBonus, actorBeingAttacked, null, 0, null, processImmediately: true);
				CalculateAddedShield();
				RefreshDamageInformation();
				continue;
			}
			throw new Exception("Error applying active bonus to mitigate damage. Active bonus returns null (AbilityName: " + appliedActiveBonusesData[i2].AbilityName + ", BaseCardID: " + appliedActiveBonusesData[i2].BaseCardID + ").");
		}
		Singleton<TakeDamagePanel>.Instance.ClearTakeDamageInvoked();
		ConfirmShields();
	}

	public void ProxyTakeDamage(GameAction gameAction)
	{
		CActor cActor = ScenarioManager.FindActor(gameAction.SupplementaryDataString);
		if (cActor != actorBeingAttacked && IsOpen)
		{
			pendingGameActions.Add(gameAction);
			return;
		}
		if (actorBeingAttacked != cActor && actorBeingAttacked != null)
		{
			Log.Error($"ProxyTakeDamage() invoked with different actor: {cActor} instead of {actorBeingAttacked}");
		}
		actorBeingAttacked = cActor;
		if (actorToShowCardsFor == null)
		{
			actorToShowCardsFor = actorBeingAttacked.PlayerActor;
		}
		List<CItem> source = actorBeingAttacked.Inventory.AllItemsAndPets.ToList();
		uint[] activatedItemNetworkIDs = ((ItemsToken)gameAction.SupplementaryDataToken).ItemNetworkIDs;
		int i;
		for (i = 0; i < activatedItemNetworkIDs.Length; i++)
		{
			CItem cItem = source.FirstOrDefault((CItem x) => x.NetworkID == activatedItemNetworkIDs[i]);
			if (cItem != null)
			{
				ToggleShieldItem(cItem);
				continue;
			}
			throw new Exception("Error applying shield item to mitigate damage. Item returns null (NetworkID: " + activatedItemNetworkIDs[i] + ").");
		}
		CAbility.EAbilityType abilityType = ((damageAbility != null) ? damageAbility.AbilityType : CAbility.EAbilityType.None);
		List<CActiveBonus> preventDamageActiveBonuses = Singleton<UIActiveBonusBar>.Instance.GetPreventDamageActiveBonuses(actorBeingAttacked, abilityType, IsLethalDamage, FinalDamage);
		preventDamageActiveBonuses.AddRange(Singleton<UIActiveBonusBar>.Instance.GetShieldIncomingAttackActiveBonuses(actorBeingAttacked, abilityType));
		ActiveBonusesToken.ActiveBonusData[] appliedActiveBonusesData = ((ActiveBonusesToken)gameAction.SupplementaryDataToken2).ActiveBonuses;
		int i2;
		for (i2 = 0; i2 < appliedActiveBonusesData.Length; i2++)
		{
			CActiveBonus cActiveBonus = preventDamageActiveBonuses.FirstOrDefault((CActiveBonus x) => x.Ability.Name == appliedActiveBonusesData[i2].AbilityName && x.BaseCard.ID == appliedActiveBonusesData[i2].BaseCardID);
			if (cActiveBonus != null)
			{
				ToggleActiveBonus(cActiveBonus, actorBeingAttacked);
				ScenarioRuleClient.ToggleActiveBonus(cActiveBonus, actorBeingAttacked, null, 0, null, processImmediately: true);
				CalculateAddedShield();
				RefreshDamageInformation();
				continue;
			}
			throw new Exception("Error applying active bonus to mitigate damage. Active bonus returns null (AbilityName: " + appliedActiveBonusesData[i2].AbilityName + ", BaseCardID: " + appliedActiveBonusesData[i2].BaseCardID + ").");
		}
		Singleton<TakeDamagePanel>.Instance.ClearTakeDamageInvoked();
		TakeDamage();
	}

	public void ProxyChangeSelectionPlayerToBurnCardsForAlly(GameAction gameAction)
	{
		CActor cActor = ScenarioManager.FindActor(gameAction.SupplementaryDataString);
		if (cActor != actorBeingAttacked && IsOpen)
		{
			pendingGameActions.Add(gameAction);
			return;
		}
		if (actorBeingAttacked != cActor && actorBeingAttacked != null)
		{
			Log.Error($"ProxyChangeSelectionPlayerToBurnCardsForAlly() invoked with different actor: {cActor} instead of {actorBeingAttacked}");
		}
		actorBeingAttacked = cActor;
		if (actorToShowCardsFor == null)
		{
			actorToShowCardsFor = actorBeingAttacked.PlayerActor;
		}
		Singleton<TakeDamagePanel>.Instance.ClearTakeDamageInvoked();
		ChooseOtherPlayerToBurnCard();
	}

	public void ProxyBurnAvailableCard(GameAction action)
	{
		CardsHandUI hand = Singleton<CardsHandManager>.Instance.GetHand(action.ActorID);
		if (hand != null)
		{
			if (hand.PlayerActor != actorToShowCardsFor && IsOpen)
			{
				pendingGameActions.Add(action);
				return;
			}
			if (ScenarioScript_FH100.IsActive)
			{
				hand.ProxyGiveFromTakenInsteadOfBurnFS100(GameState.EAvoidDamageOption.Lose1HandCard);
			}
			else
			{
				hand.ProxyBurnOneAvailableCard(((CardsToken)action.SupplementaryDataToken).CardIDs[0]);
			}
			ResetAndHide(stopActiveBonus: false, switchBackToSavedState: true);
			return;
		}
		throw new Exception("Error burning an available card to avoid damage. Card Hand returns null for the actor (ID: " + action.ActorID + ").");
	}

	public void ProxyBurnDiscardedCards(GameAction action)
	{
		CardsHandUI hand = Singleton<CardsHandManager>.Instance.GetHand(action.ActorID);
		if (hand != null)
		{
			if (hand.PlayerActor != actorToShowCardsFor && IsOpen)
			{
				pendingGameActions.Add(action);
				return;
			}
			if (ScenarioScript_FH100.IsActive)
			{
				hand.ProxyGiveFromTakenInsteadOfBurnFS100(GameState.EAvoidDamageOption.Lose2DiscardCards);
			}
			else
			{
				CardsToken cardsToken = (CardsToken)action.SupplementaryDataToken;
				hand.ProxyBurnTwoDiscardedCards(cardsToken.CardIDs[0], cardsToken.CardIDs[1]);
			}
			ResetAndHide(stopActiveBonus: false, switchBackToSavedState: true);
			return;
		}
		throw new Exception("Error burning an available card to avoid damage. Card Hand returns null for the actor (ID: " + action.ActorID + ").");
	}

	private void OnDisable()
	{
		state = State.None;
		UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.DamageChoiceDialogDisappeared));
	}
}
