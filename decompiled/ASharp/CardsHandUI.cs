using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DG.Tweening;
using DevLocker.GFrame.Input.UIScope;
using FFSNet;
using FHGUI.Common;
using GLOOM;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CardsHandUI : MonoBehaviour
{
	[Serializable]
	public struct LayoutSettings
	{
		public float Scale;

		public RectOffset Padding;

		public Vector2 CellSize;

		public Vector2 Spacing;

		public GridLayoutGroup.Corner StartCorner;

		public GridLayoutGroup.Axis StartAxis;

		public TextAnchor ChildAlignment;

		public GridLayoutGroup.Constraint Constraint;

		public int ColumnsCount;

		public bool FitInParent;
	}

	public enum CardActionsCommand
	{
		NONE,
		RESET,
		FORCE_RESET
	}

	private class CardState
	{
		public bool isActive;

		public int order;

		public Transform parent;

		public CardState(AbilityCardUI abilityCard)
		{
			isActive = abilityCard.gameObject.activeSelf;
			order = abilityCard.transform.GetSiblingIndex();
			parent = abilityCard.transform.parent;
		}
	}

	private enum EUpdateCardViewMode
	{
		None,
		OnyActive,
		Refresh
	}

	[SerializeField]
	public Transform abilityCardsHolder;

	[SerializeField]
	private CardHandHeader burntHeader;

	[SerializeField]
	private CardHandHeader discardedHeader;

	[SerializeField]
	private CardHandHeader takenHeader;

	[SerializeField]
	private GameObject shortRestPrefab;

	[SerializeField]
	private GameObject longRestCardPrefab;

	[SerializeField]
	private GridLayoutGroup gridLayoutGroup;

	[SerializeField]
	private LayoutElementExtended layoutElement;

	[SerializeField]
	public ItemsUI itemsUI;

	[SerializeField]
	private ModifiersDisplayController modifiersController;

	public ExtendedButton modifiersSelectable;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private SelectionControllerScopeElement _selectionController;

	[Header("Layout Settings")]
	[SerializeField]
	private LayoutSettings m_CardsSelectionLayoutSettings;

	[SerializeField]
	private LayoutSettings m_ActionSelectionLayoutSettings;

	[SerializeField]
	private bool m_ExpandMiniCardInParent;

	[Header("Burn Card Buttons visual Settings")]
	[SerializeField]
	private FHDialogButtonVisuals.VisualParams m_RedrawOptionButtonVisuals;

	[SerializeField]
	private FHDialogButtonVisuals.VisualParams m_BurnOptionButtonVisuals;

	[SerializeField]
	private FHDialogButtonVisuals.VisualParams m_BurnRequiredButtonVisuals;

	[Header("Display settings")]
	[SerializeField]
	private Vector2 separatorOffset;

	[Header("Shuffle animation settings")]
	[SerializeField]
	private Ease easeType = Ease.OutExpo;

	[SerializeField]
	private float cardHighlightTime;

	[SerializeField]
	private float lostCardMoveTime;

	[SerializeField]
	private float cardsReorderingOffsetTime;

	[SerializeField]
	private float discardedCardsTransitionTime;

	[SerializeField]
	private float discardedCardsMoveTime;

	[SerializeField]
	private float postAnimationWaitTime;

	[Header("Blinkblade")]
	[SerializeField]
	private GameObject m_TimeTokensHolder;

	[SerializeField]
	private Text m_TimeTokensText;

	[SerializeField]
	private Toggle m_TimeTokensFastToggle;

	[SerializeField]
	private Toggle m_TimeTokensSlowToggle;

	[Header("Geminate")]
	[SerializeField]
	private GameObject m_GeminateModule;

	[SerializeField]
	private Toggle m_GeminateMeleeToggle;

	[SerializeField]
	private Toggle m_GeminateRangeToggle;

	[Header("Controller Navigation")]
	[SerializeField]
	private UINavigationGroup m_CardsNavigationGroup;

	[SerializeField]
	private UINavigationGroup m_ExtraCardsNavigationGroup;

	[SerializeField]
	private UINavigationGroup m_InventoryNavigationGroup;

	private CPlayerActor playerActor;

	private List<AbilityCardUI> cardsUI = new List<AbilityCardUI>();

	private List<AbilityCardUI> selectedCardsUI = new List<AbilityCardUI>();

	private List<AbilityCardUI> cachedSelectedCardsUI = new List<AbilityCardUI>();

	private bool cardsSpawned;

	private bool useCurrentRoundCards;

	private CardHandMode previousMode;

	private ShortRest shortRest;

	public bool shortRested;

	private Transform highlightTransform;

	private bool invalidateCellSize;

	private LayoutSettings activeLayoutSettings;

	private Dictionary<AbilityCardUI, CardState> cardsStateBeforePreview = new Dictionary<AbilityCardUI, CardState>();

	private CardsActionControlller cardsActionController;

	private bool dialogPopupShown;

	private bool isPreviewingCards;

	private CardHandMode cardHandMode;

	private bool enableLongCardSelection;

	private CardPileType filterType;

	private List<CardPileType> selectableCardTypes;

	private int maxCardsSelected;

	private bool fadeUnselectableCards;

	private bool highlightSelectableCards;

	private CardActionsCommand resetCardActionsPhase;

	private bool forceUseCurrentRoundCards;

	private Action<AbilityCardUI> overrideCardCallback;

	private Func<CAbilityCard, bool> cardSelectableFilter;

	private Func<CAbilityCard, bool> cardVisibleFilter;

	public UnityEvent OnShow;

	public UnityEvent OnHide;

	public const float ALPHA_FORM_CURRENT = 1f;

	public const float ALPHA_FORM_ALT = 0.3f;

	private CPlayerActor.EForm _prevFormRestrict;

	private readonly Dictionary<Transform, Transform> _transformOriginalParents = new Dictionary<Transform, Transform>();

	private bool animatedLosingCard;

	private Action onAnimateCardLostCallback;

	public float animationSpeed;

	private List<Tween> animations = new List<Tween>();

	public CPlayerActor PlayerActor => playerActor;

	public ShortRest ShortRestUI => shortRest;

	public CardHandMode currentMode { get; private set; }

	public bool IsFullCardPreviewShowing { get; private set; }

	[StateDump(Skip = true)]
	public StartRoundCardsToken CachedStartRoundToken { get; set; }

	public List<AbilityCardUI> SelectedCards => selectedCardsUI;

	public List<AbilityCardUI> CachedSelectedCards => cachedSelectedCardsUI;

	public IReadOnlyList<AbilityCardUI> AllCards => cardsUI;

	public int MaxCardsSelected => maxCardsSelected;

	public bool AnimatingLostCards { get; private set; }

	public bool IsPreviewingCards => isPreviewingCards;

	public bool IsShown => gameObject.gameObject.activeSelf;

	private bool IsLongRestForbidden => playerActor.ActiveBonuses.Any((CActiveBonus ab) => ab.Ability.ActiveBonusData.Flags.HasFlag(AbilityData.EActiveBonusFlag.ForbidLongRest));

	public bool IsImprovedLongResting
	{
		get
		{
			if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest && playerActor.CharacterClass.ImprovedShortRest && playerActor.CharacterClass.LongRest)
			{
				if (currentMode != CardHandMode.LoseCard)
				{
					return currentMode == CardHandMode.ActionSelection;
				}
				return true;
			}
			return false;
		}
	}

	public bool IsImprovedLongRestingWithSelection
	{
		get
		{
			if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest && playerActor.CharacterClass.ImprovedShortRest && playerActor.CharacterClass.ImprovedShortRestDrawCount != 0)
			{
				if (currentMode != CardHandMode.LoseCard)
				{
					return currentMode == CardHandMode.CardsSelection;
				}
				return true;
			}
			return false;
		}
	}

	public static bool IsFreeShortRest
	{
		get
		{
			if (ScenarioManager.CurrentScenarioState != null)
			{
				if (!ScenarioManager.CurrentScenarioState.FreeShortRestButMuddle)
				{
					return ScenarioManager.CurrentScenarioState.FreeShortRest;
				}
				return true;
			}
			return false;
		}
	}

	public static bool IsFreeLongRest
	{
		get
		{
			if (ScenarioManager.CurrentScenarioState != null)
			{
				return ScenarioManager.CurrentScenarioState.FreeLongRest;
			}
			return false;
		}
	}

	public bool IsInteractable => canvasGroup.interactable;

	public void OnAfterLoad()
	{
		playerActor = GameState.ResolvePlayerAfterLoad(playerActor);
		foreach (AbilityCardUI item in cardsUI)
		{
			item.OnAfterLoad();
		}
	}

	public void UpdateElements()
	{
		try
		{
			foreach (AbilityCardUI item in selectedCardsUI)
			{
				item.UpdateElements();
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.UpdateElements().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void ToggleAttackModifiers(bool active)
	{
		try
		{
			if (active)
			{
				modifiersController.Display(PlayerActor.CharacterClass, !FFSNetwork.IsOnline || PlayerActor.IsUnderMyControl());
			}
			else
			{
				modifiersController.Hide();
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.ToggleAttackModifiers().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00002", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void ToggleAttackModifiers()
	{
		ToggleAttackModifiers(!modifiersController.IsOpen());
	}

	public void ToggleFullCardsPreview(bool active)
	{
		try
		{
			if (animatedLosingCard)
			{
				active = false;
			}
			if (isPreviewingCards == active)
			{
				return;
			}
			isPreviewingCards = active;
			if (active)
			{
				IsFullCardPreviewShowing = true;
				dialogPopupShown = UIManager.Instance.dialogPopup.IsOpen();
				UIManager.Instance.dialogPopup.Hide(cleanContent: false);
				cardsStateBeforePreview.Clear();
				foreach (AbilityCardUI item in cardsUI)
				{
					cardsStateBeforePreview[item] = new CardState(item);
					item.ToggleFullCardPreview(isHighlighted: false);
				}
				foreach (AbilityCardUI item2 in cardsUI)
				{
					if (!item2.IsLongRest && item2.CardType != CardPileType.Unselected)
					{
						item2.transform.SetParent(Singleton<FullCardHandViewer>.Instance.CardContainer);
						item2.fullAbilityCard.HighlightAvailableConsumes(InfusionBoardUI.Instance?.GetAvailableElements());
						item2.fullAbilityCard.transform.localScale = Singleton<FullCardHandViewer>.Instance.FullCardScaleFactor;
						item2.ToggleFullCard(active: true);
						item2.ToggleFullSelectionCard(active: true);
						item2.Toggle(enabled: true);
					}
				}
				Singleton<FullCardHandViewer>.Instance.Show(() =>
				{
					ToggleFullCardsPreview(active: false);
				});
			}
			else
			{
				Singleton<FullCardHandViewer>.Instance.Hide();
				IsFullCardPreviewShowing = false;
				int num = 0;
				foreach (AbilityCardUI item3 in cardsUI)
				{
					if (!item3.IsLongRest && item3.CardType != CardPileType.Unselected)
					{
						item3.transform.SetParent(cardsStateBeforePreview[item3].parent);
						item3.fullAbilityCard.transform.localScale = Vector3.one;
						item3.Toggle(cardsStateBeforePreview[item3].isActive);
						item3.ToggleFullSelectionCard(active: false);
						if (item3.Mode != CardHandMode.ActionSelection && !item3.IsInPreview)
						{
							item3.ToggleFullCard(active: false);
						}
						num++;
					}
				}
				num = 0;
				foreach (AbilityCardUI item4 in cardsUI)
				{
					item4.transform.SetSiblingIndex(cardsStateBeforePreview[item4].order);
					num++;
				}
				cardsStateBeforePreview.Clear();
				UpdateEffects();
				if (dialogPopupShown)
				{
					UIManager.Instance.dialogPopup.ShowLoadedContent();
				}
			}
			m_CardsNavigationGroup.RefreshNavigationClearCaches();
			m_ExtraCardsNavigationGroup.RefreshNavigationClearCaches();
			m_InventoryNavigationGroup.RefreshNavigationClearCaches();
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.ToggleFullCardsPreview().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00003", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void RecycleCards()
	{
		foreach (AbilityCardUI item in cardsUI)
		{
			ObjectPool.RecycleCard(item.CardID, ObjectPool.ECardType.Ability, item.gameObject);
		}
		cardsUI.Clear();
		selectedCardsUI.Clear();
		cardsStateBeforePreview?.Clear();
	}

	private void OnDestroy()
	{
		if (!GloomUtility.IsAppQuitting)
		{
			RecycleCards();
			OnShow.RemoveAllListeners();
			OnHide.RemoveAllListeners();
			overrideCardCallback = null;
			cardSelectableFilter = null;
			cardVisibleFilter = null;
			itemsUI = null;
			shortRest = null;
			canvasGroup = null;
			_selectionController?.StartSelections.Clear();
			_selectionController?.StartNavigationGroups.Clear();
			_selectionController = null;
			cardsUI.Clear();
			selectedCardsUI.Clear();
			cardsStateBeforePreview = null;
			m_CardsNavigationGroup = null;
			m_InventoryNavigationGroup = null;
			m_ExtraCardsNavigationGroup = null;
			cardsStateBeforePreview?.Clear();
			playerActor = null;
			cardsActionController = null;
			m_TimeTokensFastToggle.onValueChanged.RemoveAllListeners();
			m_TimeTokensSlowToggle.onValueChanged.RemoveAllListeners();
			m_GeminateMeleeToggle.onValueChanged.RemoveAllListeners();
			m_GeminateRangeToggle.onValueChanged.RemoveAllListeners();
		}
	}

	private void OnDisable()
	{
		CancelAnimateCardLost();
	}

	public void UpdateOriginalExtraTurnCards(CPlayerActor playerActor)
	{
		foreach (AbilityCardUI item in cardsUI)
		{
			if (playerActor.CharacterClass.RoundAbilityCardsSelectedInCardSelection.Contains(item.AbilityCard))
			{
				item.fullAbilityCard.TryPlayBurnAnimation(CBaseCard.ActionType.DefaultAttackAction);
			}
		}
	}

	public void Init(CPlayerActor playerActor, Transform highlightTransform, CardsActionControlller cardsActionController)
	{
		try
		{
			this.cardsActionController = cardsActionController;
			this.playerActor = playerActor;
			this.highlightTransform = highlightTransform;
			IsFullCardPreviewShowing = false;
			List<CAbilityCard> abilityCards = playerActor.CharacterClass.SelectedAbilityCards.Concat(playerActor.CharacterClass.ActivatedAbilityCards).Distinct().ToList();
			List<CAbilityCard> unselectedAbilityCards = playerActor.CharacterClass.UnselectedAbilityCards.FindAll((CAbilityCard it) => it.Level <= playerActor.Level);
			Stopwatch stopwatch = Stopwatch.StartNew();
			if (!cardsSpawned)
			{
				SpawnCards(abilityCards, unselectedAbilityCards, longRestCardPrefab);
				Log.Trace($"SpawnCards Init took {stopwatch.ElapsedMilliseconds}ms");
			}
			else
			{
				SetCards(abilityCards, unselectedAbilityCards);
				Log.Trace($"SetCards Init took {stopwatch.ElapsedMilliseconds}ms");
			}
			itemsUI.SetActor(playerActor);
			if (PlayerActor.UsesTimeTokens())
			{
				m_TimeTokensFastToggle.onValueChanged.RemoveListener(OnTimeTokensSelectionValueChanged);
				m_TimeTokensSlowToggle.onValueChanged.RemoveListener(OnTimeTokensSelectionValueChanged);
				m_TimeTokensFastToggle.onValueChanged.AddListener(OnTimeTokensSelectionValueChanged);
				m_TimeTokensSlowToggle.onValueChanged.AddListener(OnTimeTokensSelectionValueChanged);
			}
			else if (PlayerActor.Class.ID == "GeminateID")
			{
				m_GeminateMeleeToggle.onValueChanged.RemoveListener(OnGeminateFormSelectionChanged);
				m_GeminateRangeToggle.onValueChanged.RemoveListener(OnGeminateFormSelectionChanged);
				m_GeminateMeleeToggle.onValueChanged.AddListener(OnGeminateFormSelectionChanged);
				m_GeminateRangeToggle.onValueChanged.AddListener(OnGeminateFormSelectionChanged);
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.Init().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00004", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void Hide()
	{
		try
		{
			modifiersController.Hide(immendiately: true);
			ToggleFullCardsPreview(active: false);
			foreach (AbilityCardUI item in cardsUI)
			{
				item.fullAbilityCard.ToggleSelect(active: false, CBaseCard.ActionType.BottomAction);
				item.fullAbilityCard.ToggleSelect(active: false, CBaseCard.ActionType.TopAction);
				item.transform.SetParent(abilityCardsHolder);
			}
			ShowOrHideInternal(show: false);
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.Hide().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00005", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void Show(Transform holder = null)
	{
		try
		{
			ToggleFullCardsPreview(active: false);
			if (holder != null)
			{
				foreach (AbilityCardUI item in cardsUI)
				{
					item.transform.SetParent(holder);
				}
			}
			ShowOrHideInternal(show: true);
			itemsUI.gameObject.SetActive(value: true);
			itemsUI.RefreshItemsState();
			UpdateLongRest();
			UpdateShortRest();
			UpdateEffects();
			UpdateCharacterSpecificMechanic();
			if (FFSNetwork.IsOnline && !playerActor.IsUnderMyControl() && shortRested && PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
			{
				Log.GUIInfo("Show hand");
				UpdateView(currentMode, CardPileType.Any, new List<CardPileType>
				{
					CardPileType.Hand,
					CardPileType.Round
				}, playerActor.SelectedRoundCardsCount);
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.Show().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00006", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void UpdateCharacterSpecificMechanic()
	{
		UpdateTokens();
		UpdateGeminateModule();
	}

	private void UpdateTokens()
	{
		if (PlayerActor.UsesTimeTokens() && PhaseManager.IsInCardsSelectionPhase && currentMode == CardHandMode.CardsSelection)
		{
			m_TimeTokensHolder.SetActive(IsShown);
			bool flag = !FFSNetwork.IsOnline || playerActor == null || (playerActor.IsUnderMyControl() && !Singleton<UIReadyToggle>.Instance.PlayersReady.Contains(PlayerRegistry.MyPlayer) && !Singleton<UIReadyToggle>.Instance.IsProgressingBar);
			bool flag2 = PlayerActor.GetTimeTokens() > 0;
			m_TimeTokensFastToggle.interactable = flag & flag2;
			m_TimeTokensSlowToggle.interactable = flag;
			m_TimeTokensFastToggle.SetIsOnWithoutNotify((PlayerActor.Agility != CPlayerActor.EAgility.Slow) & flag2);
			m_TimeTokensSlowToggle.SetIsOnWithoutNotify(PlayerActor.Agility == CPlayerActor.EAgility.Slow || !flag2);
			UpdatePlayerActorAgility();
			UpdateTimeTokensText();
		}
		else
		{
			m_TimeTokensHolder.SetActive(value: false);
		}
	}

	private void UpdateTimeTokensText()
	{
		int timeTokens = PlayerActor.GetTimeTokens();
		if (PlayerActor.Agility == CPlayerActor.EAgility.Fast)
		{
			m_TimeTokensText.text = $"Time Tokens: {timeTokens} (-1)";
		}
		else if (PlayerActor.Agility == CPlayerActor.EAgility.Slow)
		{
			string arg = ((timeTokens < 2) ? "(+1)" : "");
			m_TimeTokensText.text = $"Time Tokens: {timeTokens} {arg}";
		}
		else
		{
			m_TimeTokensText.text = $"Time Tokens: {timeTokens}";
		}
	}

	private void UpdatePlayerActorAgility()
	{
		CPlayerActor.EAgility agility = PlayerActor.Agility;
		if (m_TimeTokensFastToggle.isOn)
		{
			PlayerActor.Agility = CPlayerActor.EAgility.Fast;
		}
		else if (m_TimeTokensSlowToggle.isOn)
		{
			PlayerActor.Agility = CPlayerActor.EAgility.Slow;
		}
		else
		{
			PlayerActor.Agility = CPlayerActor.EAgility.None;
		}
		if (agility == PlayerActor.Agility)
		{
			return;
		}
		foreach (AbilityCardUI item in cardsUI.Where((AbilityCardUI cardUI) => !cardUI.IsLongRest))
		{
			item.fullAbilityCard.UpdateClassElements(PlayerActor);
			int num = ((playerActor.Agility == CPlayerActor.EAgility.Slow) ? 30 : 0);
			int num2 = Mathf.Clamp(item.AbilityCard.Initiative + num, 1, 99);
			item.miniAbilityCard.SetInitiativeText(num2.ToString());
		}
		if (FFSNetwork.IsOnline && PlayerActor.IsUnderMyControl())
		{
			int controllableID = ControllableRegistry.GetControllableID(PlayerActor);
			Synchronizer.ReplicateControllableStateChange(GameActionType.ChangeAgility, ActionProcessor.CurrentPhase, controllableID, (int)PlayerActor.Agility);
		}
	}

	public void OnToggleTimeTokenValues()
	{
		if (!PlayerActor.UsesTimeTokens())
		{
			return;
		}
		if (m_TimeTokensSlowToggle.isOn || !m_TimeTokensSlowToggle.IsInteractable())
		{
			if (m_TimeTokensFastToggle.IsInteractable())
			{
				m_TimeTokensFastToggle.isOn = true;
			}
		}
		else if (m_TimeTokensSlowToggle.IsInteractable())
		{
			m_TimeTokensSlowToggle.isOn = true;
		}
	}

	private void OnTimeTokensSelectionValueChanged(bool newValue)
	{
		UpdatePlayerActorAgility();
		UpdateTimeTokensText();
		InitiativeTrack.Instance.CheckRoundAbilityCardsOrLongRestSelected();
		UpdateInitiative();
	}

	public bool TimeTokenSelected()
	{
		if (PlayerActor.UsesTimeTokens())
		{
			return PlayerActor.Agility != CPlayerActor.EAgility.None;
		}
		return true;
	}

	private void UpdateGeminateModule()
	{
		if (PlayerActor.Class.ID != "GeminateID")
		{
			return;
		}
		CPlayerActor.EForm form = PlayerActor.Form;
		CPlayerActor.EForm prevForm = PlayerActor.PrevForm;
		CPlayerActor.EForm eForm = PlayerActor.PendingForm;
		if (eForm == CPlayerActor.EForm.None || eForm == CPlayerActor.EForm.Choose)
		{
			eForm = form;
		}
		if (eForm == CPlayerActor.EForm.None || eForm == CPlayerActor.EForm.Choose)
		{
			eForm = prevForm;
		}
		if (eForm == CPlayerActor.EForm.None || eForm == CPlayerActor.EForm.Choose)
		{
			CPlayerActor.EForm eForm2 = (PlayerActor.PendingForm = CPlayerActor.EForm.Melee);
			eForm = eForm2;
		}
		if ((ScenarioManager.CurrentScenarioState.RoundNumber == 1 || form == CPlayerActor.EForm.Choose) && PhaseManager.IsInCardsSelectionPhase && currentMode == CardHandMode.CardsSelection)
		{
			m_GeminateModule.SetActive(IsShown);
			bool interactable = !FFSNetwork.IsOnline || playerActor == null || (playerActor.IsUnderMyControl() && !Singleton<UIReadyToggle>.Instance.PlayersReady.Contains(PlayerRegistry.MyPlayer) && !Singleton<UIReadyToggle>.Instance.IsProgressingBar);
			if (PhaseManager.CurrentPhase is CPhaseShortRest && GameState.CurrentActor != playerActor)
			{
				interactable = false;
			}
			m_GeminateMeleeToggle.interactable = interactable;
			m_GeminateRangeToggle.interactable = interactable;
			bool flag = eForm == CPlayerActor.EForm.Melee;
			if (m_GeminateMeleeToggle.isOn != flag)
			{
				m_GeminateMeleeToggle.SetIsOnWithoutNotify(flag);
			}
			bool flag2 = eForm == CPlayerActor.EForm.Range;
			if (m_GeminateRangeToggle.isOn != flag2)
			{
				m_GeminateRangeToggle.SetIsOnWithoutNotify(flag2);
			}
			RestrictGeminateCardsSelection(eForm, deselectAllCards: false);
		}
		else
		{
			m_GeminateModule.SetActive(value: false);
			RestrictGeminateCardsSelection(eForm, deselectAllCards: false);
		}
	}

	public void SelectGeminateForm(CPlayerActor.EForm targetForm)
	{
		if (!(PlayerActor.Class.ID != "GeminateID"))
		{
			if (targetForm == CPlayerActor.EForm.Range)
			{
				m_GeminateRangeToggle.isOn = true;
			}
			else
			{
				m_GeminateMeleeToggle.isOn = true;
			}
		}
	}

	public bool TryRestrictToForm(CPlayerActor.EForm form)
	{
		if (_prevFormRestrict == form)
		{
			return false;
		}
		RestrictToForm(form);
		return true;
	}

	private void RestrictToForm(CPlayerActor.EForm form)
	{
		foreach (AbilityCardUI item in cardsUI)
		{
			if (!item.IsLongRest && item.CardType != CardPileType.Active && item.CardType != CardPileType.Discarded && item.CardType != CardPileType.Lost && item.CardType != CardPileType.Permalost && item.CardType != CardPileType.Taken)
			{
				bool flag = item.CardRequiredForm == form;
				item.SetSelectable(flag);
				item.SetFade(flag ? 1f : 0.3f);
			}
		}
		_prevFormRestrict = form;
		SortCards();
	}

	private void RestrictGeminateCardsSelection(CPlayerActor.EForm form, bool deselectAllCards)
	{
		if (!GameState.IsPerformingCardOutOfPhase && currentMode == CardHandMode.CardsSelection)
		{
			if (deselectAllCards)
			{
				DeselectAllCards();
			}
			RestrictToForm(form);
			Singleton<CardsHandManager>.InstanceFast?.TryAutoSelectPlayerCards();
		}
	}

	private void UpdateGeminatePendingForm(bool deselectAllCards)
	{
		CPlayerActor.EForm pendingForm = PlayerActor.PendingForm;
		if (m_GeminateMeleeToggle.isOn)
		{
			PlayerActor.PendingForm = CPlayerActor.EForm.Melee;
		}
		else if (m_GeminateRangeToggle.isOn)
		{
			PlayerActor.PendingForm = CPlayerActor.EForm.Range;
		}
		else
		{
			PlayerActor.PendingForm = CPlayerActor.EForm.None;
		}
		RestrictGeminateCardsSelection(PlayerActor.PendingForm, deselectAllCards);
		Singleton<CardsHandManager>.Instance.HandViewUpdated(this);
		if (pendingForm != PlayerActor.PendingForm && FFSNetwork.IsOnline && PlayerActor.IsUnderMyControl())
		{
			int controllableID = ControllableRegistry.GetControllableID(PlayerActor);
			Synchronizer.ReplicateControllableStateChange(GameActionType.ChangePendingForm, ActionProcessor.CurrentPhase, controllableID, (int)PlayerActor.PendingForm);
		}
	}

	public void OnToggleGeminateFormValues()
	{
		if (!(PlayerActor.Class.ID != "GeminateID"))
		{
			if (m_GeminateMeleeToggle.isOn)
			{
				m_GeminateRangeToggle.isOn = true;
			}
			else
			{
				m_GeminateMeleeToggle.isOn = true;
			}
		}
	}

	private void OnGeminateFormSelectionChanged(bool newValue)
	{
		UpdateGeminatePendingForm(deselectAllCards: true);
		InitiativeTrack.Instance.CheckRoundAbilityCardsOrLongRestSelected();
	}

	public void ResetCharacterMechanicsParent()
	{
		SetCharacterMechanicsParent(null);
	}

	public void SetCharacterMechanicsParent(Transform newParent)
	{
		reparent(m_TimeTokensHolder.transform);
		reparent(m_GeminateModule.transform);
		void reparent(Transform container)
		{
			if (!(container.parent == newParent))
			{
				bool flag = _transformOriginalParents.TryGetValue(container, out var value);
				if (newParent == null)
				{
					if (flag)
					{
						container.SetParent(value, worldPositionStays: false);
					}
				}
				else
				{
					if (!flag)
					{
						_transformOriginalParents[container] = container.parent;
					}
					container.SetParent(newParent, worldPositionStays: false);
				}
			}
		}
	}

	public void DisableCharacterSpecificMechanic()
	{
		m_TimeTokensHolder.SetActive(value: false);
		m_GeminateModule.SetActive(value: false);
	}

	private void ShowOrHideInternal(bool show)
	{
		gameObject.SetActive(show);
		if (show)
		{
			OnShow.Invoke();
		}
		else
		{
			OnHide.Invoke();
		}
		UpdateCharacterSpecificMechanic();
	}

	public void ToggleHighlight(List<CardPileType> highlightCardTypes, bool active, bool keepLongRest = false, bool keepShortRest = false)
	{
		try
		{
			if (active)
			{
				shortRest.Button.SetAsFaded(!keepShortRest || shortRest.Button.IsFaded);
			}
			else
			{
				UpdateShortRest();
			}
			ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
			bool flag = PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest;
			bool num = currentScenarioState?.FreeLongRest ?? false;
			bool flag2 = currentScenarioState != null && (currentScenarioState.FreeShortRest || currentScenarioState.FreeShortRestButMuddle);
			bool flag3 = (num & flag) && playerActor.CharacterClass.LongRest && !playerActor.CharacterClass.HasLongRested;
			bool flag4 = (!flag2 & flag) && playerActor.CharacterClass.ShortRest && !playerActor.CharacterClass.HasShortRested;
			bool flag5 = flag3 | flag4;
			foreach (AbilityCardUI item in cardsUI)
			{
				item.Highlight(active, highlightCardTypes, flag5 && item.CardType == CardPileType.Discarded, keepLongRest || (flag3 && item.CardType == CardPileType.Discarded), keepShortRest);
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.ToggleHighlight().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00007", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void SetRestLoseIndicatorVisibility(bool isVisible, bool checkShortRest, bool checkLongRest)
	{
		if (isVisible)
		{
			if (checkShortRest && IsFreeShortRest)
			{
				isVisible = false;
			}
			if (checkLongRest && IsFreeLongRest)
			{
				isVisible = false;
			}
		}
		foreach (AbilityCardUI item in cardsUI.Where((AbilityCardUI cardUI) => cardUI.CardType == CardPileType.Discarded))
		{
			item.SetLoseIndicatorVisibility(isVisible);
		}
	}

	public void ResetShortRestUIFlag()
	{
		shortRested = false;
	}

	public void UnselectShortRest()
	{
		shortRest.Select(active: false);
	}

	public void ResetViewToCachedSettings()
	{
		UpdateView(cardHandMode, enableLongCardSelection, filterType, selectableCardTypes, maxCardsSelected, fadeUnselectableCards, highlightSelectableCards, resetCardActionsPhase, forceUseCurrentRoundCards, overrideCardCallback, cardSelectableFilter);
	}

	public void UpdateView(CardHandMode mode, CardPileType filterType = CardPileType.Any, List<CardPileType> selectableCardTypes = null, int maxCardsSelected = 0, bool fadeUnselectableCards = false, bool highlightSelectableCards = false, CardActionsCommand resetCardActions = CardActionsCommand.RESET, bool forceUseCurrentRoundCards = false, Action<AbilityCardUI> overrideCardCallback = null, Func<CAbilityCard, bool> cardSelectableFilter = null, Func<CAbilityCard, bool> cardVisibleFilter = null)
	{
		UpdateView(mode, mode != CardHandMode.SelectCardsWithMandatoryActiveBonusesToDeactivate && (!playerActor.CharacterClass.LongRest || PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest), filterType, selectableCardTypes, maxCardsSelected, fadeUnselectableCards, highlightSelectableCards, resetCardActions, forceUseCurrentRoundCards, overrideCardCallback, cardSelectableFilter, cardVisibleFilter);
	}

	public void UpdateView(CardHandMode mode, bool enableLongCardSelection, CardPileType filterType = CardPileType.Any, List<CardPileType> selectableCardTypes = null, int maxCardsSelected = 0, bool fadeUnselectableCards = false, bool highlightSelectableCards = false, CardActionsCommand resetCardActions = CardActionsCommand.RESET, bool forceUseCurrentRoundCards = false, Action<AbilityCardUI> overrideCardCallback = null, Func<CAbilityCard, bool> cardSelectableFilter = null, Func<CAbilityCard, bool> cardVisibleFilter = null)
	{
		try
		{
			selectedCardsUI.Clear();
			cardHandMode = mode;
			this.enableLongCardSelection = enableLongCardSelection;
			this.filterType = filterType;
			this.selectableCardTypes = selectableCardTypes;
			this.maxCardsSelected = maxCardsSelected;
			this.fadeUnselectableCards = fadeUnselectableCards;
			this.highlightSelectableCards = highlightSelectableCards;
			resetCardActionsPhase = resetCardActions;
			this.forceUseCurrentRoundCards = forceUseCurrentRoundCards;
			this.overrideCardCallback = overrideCardCallback;
			this.cardSelectableFilter = cardSelectableFilter;
			this.cardVisibleFilter = cardVisibleFilter;
			useCurrentRoundCards = (mode == CardHandMode.RecoverLostCard || mode == CardHandMode.RecoverDiscardedCard || mode == CardHandMode.DiscardCard || mode == CardHandMode.SelectCardsWithActiveBonusTracker || mode == CardHandMode.SelectCardsWithTideActiveBonusesToKeep || mode == CardHandMode.SelectCardsWithMandatoryActiveBonusesToDeactivate || PhaseManager.IsInCardsSelectionPhase) | forceUseCurrentRoundCards;
			UpdateCards(filterType, EUpdateCardViewMode.None);
			SetMode(mode, filterType, selectableCardTypes, fadeUnselectableCards, highlightSelectableCards, (playerActor.CharacterClass.DiscardedAbilityCards.Count > 1 && PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest) & enableLongCardSelection, resetCardActions, cardSelectableFilter);
			UpdateCards(filterType, EUpdateCardViewMode.Refresh);
			if (mode == CardHandMode.ActionSelection)
			{
				SetLayoutSettings(m_ActionSelectionLayoutSettings);
				burntHeader.Hide();
				discardedHeader.Hide();
				takenHeader.Hide();
				SortCards(reorderPositions: false);
				foreach (AbilityCardUI item in cardsUI)
				{
					item.SetLocalScale(Vector3.one);
					overrideCardCallback?.Invoke(item);
				}
			}
			else
			{
				SetLayoutSettings(m_CardsSelectionLayoutSettings);
				foreach (AbilityCardUI item2 in cardsUI)
				{
					item2.SetLocalScale(Vector3.one);
					overrideCardCallback?.Invoke(item2);
				}
				SortCards();
			}
			UpdateEffects();
			UpdateCharacterSpecificMechanic();
			Singleton<CardsHandManager>.Instance.HandViewUpdated(this);
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.UpdateView().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00008", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void SetLayoutSettings(LayoutSettings settings)
	{
		activeLayoutSettings = settings;
		gridLayoutGroup.transform.localScale = settings.Scale * Vector3.one;
		gridLayoutGroup.padding = settings.Padding;
		gridLayoutGroup.cellSize = settings.CellSize;
		gridLayoutGroup.spacing = settings.Spacing;
		gridLayoutGroup.startCorner = settings.StartCorner;
		gridLayoutGroup.startAxis = settings.StartAxis;
		gridLayoutGroup.childAlignment = settings.ChildAlignment;
		gridLayoutGroup.constraint = settings.Constraint;
		LayoutElementExtended component = abilityCardsHolder.GetComponent<LayoutElementExtended>();
		if (settings.ColumnsCount <= 0)
		{
			float x = settings.CellSize.x;
			int num = Mathf.CeilToInt((float)selectedCardsUI.Count / 2f);
			component.preferredWidth = x * (float)num + (float)settings.Padding.left;
		}
		else
		{
			float x2 = settings.CellSize.x;
			int columnsCount = settings.ColumnsCount;
			component.preferredWidth = x2 * (float)columnsCount + (float)settings.Padding.left;
		}
		invalidateCellSize = settings.FitInParent;
	}

	private void LateUpdate()
	{
		if (invalidateCellSize)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(abilityCardsHolder as RectTransform);
			invalidateCellSize = false;
			StartCoroutine(ExecuteDelayedRebuild());
		}
	}

	private IEnumerator ExecuteDelayedRebuild()
	{
		yield return null;
		gridLayoutGroup?.FitInParent(activeLayoutSettings.CellSize, transform.parent as RectTransform);
	}

	public void UpdateInitiative()
	{
		try
		{
			if (playerActor.SelectingCardsForExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.None && !GameState.IsPerformingCardOutOfPhase && !ScenarioManager.IsFastReloading)
			{
				List<CAbilityCard> roundAbilityCards = playerActor.CharacterClass.RoundAbilityCards;
				CAbilityCard initiativeAbilityCard = ((roundAbilityCards.Count > 0) ? roundAbilityCards[0] : null);
				CAbilityCard subInitiativeAbilityCard = ((roundAbilityCards.Count > 1) ? roundAbilityCards[1] : null);
				playerActor.CharacterClass.SetInitiativeAbilityCard(initiativeAbilityCard);
				playerActor.CharacterClass.SetSubInitiativeAbilityCard(subInitiativeAbilityCard);
			}
			InitiativeTrack.Instance.UpdateActors();
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.UpdateInitiative().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00009", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void UpdateEffects()
	{
		try
		{
			bool flag = !FFSNetwork.IsOnline || playerActor.IsUnderMyControl() || !PhaseManager.IsInCardsSelectionPhase;
			if ((PhaseManager.IsInCardsSelectionPhase && playerActor.CharacterClass.RoundAbilityCards.Count >= 1) & flag)
			{
				foreach (AbilityCardUI item in cardsUI)
				{
					item.PlayEffect((playerActor.CharacterClass.RoundAbilityCards[0] == item.AbilityCard) ? InitiativeTrack.InitiativeEffects.Active : InitiativeTrack.InitiativeEffects.None);
				}
				return;
			}
			if (playerActor.CharacterClass.LongRest & flag)
			{
				foreach (AbilityCardUI item2 in cardsUI)
				{
					item2.PlayEffect(item2.IsLongRest ? InitiativeTrack.InitiativeEffects.Active : InitiativeTrack.InitiativeEffects.None);
				}
				return;
			}
			if ((playerActor.TakingExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater) & flag)
			{
				foreach (AbilityCardUI item3 in cardsUI)
				{
					item3.PlayEffect((playerActor.CharacterClass.ExtraTurnCardsSelectedInCardSelectionStack.Count > 0 && playerActor.CharacterClass.ExtraTurnInitiativeAbilityCard == item3.AbilityCard) ? InitiativeTrack.InitiativeEffects.Active : InitiativeTrack.InitiativeEffects.None);
				}
				return;
			}
			foreach (AbilityCardUI item4 in cardsUI)
			{
				item4.PlayEffect(InitiativeTrack.InitiativeEffects.None);
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.UpdateEffects().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00010", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void UpdateLongRest()
	{
		try
		{
			if (Singleton<CardsHandManager>.Instance.isPreviewing)
			{
				return;
			}
			foreach (AbilityCardUI item in cardsUI)
			{
				if (item.IsLongRest)
				{
					item.Toggle(playerActor.CanUseLongRest && currentMode != CardHandMode.SelectCardsWithMandatoryActiveBonusesToDeactivate);
				}
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.UpdateLongRest().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00011", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void UpdateShortRest()
	{
		try
		{
			if (!Singleton<CardsHandManager>.Instance.isPreviewing && !(Choreographer.s_Choreographer == null))
			{
				if (!IsImprovedLongResting && PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest && currentMode == CardHandMode.CardsSelection && !GameState.s_AutoSelectPlayerCards)
				{
					shortRest.Show(playerActor);
				}
				else
				{
					shortRest.Hide();
				}
				CCharacterClass characterClass = playerActor.CharacterClass;
				bool flag = ((!playerActor.UseTakenPileForShortRest) ? (characterClass.DiscardedAbilityCards.Count > 1 || (characterClass.DiscardedAbilityCards.Count == 1 && characterClass.HandAbilityCards.Count == 0 && characterClass.RoundAbilityCards.Count == 0)) : (characterClass.TakenCards.Count < 2 && characterClass.DiscardedAbilityCards.Count > 1));
				bool playersInValidStartingPositions = Choreographer.s_Choreographer.PlayersInValidStartingPositions;
				shortRest.SetInteractable(!shortRested & flag & playersInValidStartingPositions);
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.UpdateShortRest().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00011", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void StartShortRest(CPlayerActor playerActor)
	{
		if (GameState.HasShortRestWithPreRestAbilities || playerActor.CharacterClass.HasQueuedPreRestBonuses)
		{
			PerformShortRest(playerActor);
			return;
		}
		GameState.QueuePreRestBonuses(playerActor);
		if (GameState.HasPreShortRestItemsValid(playerActor) || GameState.HasPendingRestAbilities)
		{
			Singleton<CardsHandManager>.Instance.Hide();
			Singleton<HelpBox>.Instance.Hide();
			ScenarioRuleClient.PreShortRest(playerActor, FFSNetwork.IsOnline);
		}
		else
		{
			PerformShortRest(playerActor);
		}
	}

	public void PerformShortRest(CPlayerActor actor, bool allowSpecificShortRest = true, IList<int> tideActiveBonusCardsToKeep = null)
	{
		GameState.HasShortRestWithPreRestAbilities = false;
		try
		{
			actor.LogActiveBonuses(atStartOfRest: true);
			AbilityCardUI card = GetCard(-1);
			if (card.IsSelected)
			{
				card.ToggleSelect(active: false);
			}
			if (FFSNetwork.IsOnline)
			{
				ActionProcessor.SetState(actor.IsUnderMyControl() ? ActionProcessorStateType.Halted : ActionProcessorStateType.ProcessFreely, ActionPhaseType.StartOfRound);
			}
			if (allowSpecificShortRest && actor.CharacterClass.ID == "PyroclastID" && actor.CharacterClass.ActivatedAbilityCards.Any((CAbilityCard cAbilityCard) => cAbilityCard.HasMandatoryBonus()))
			{
				PerformPyroclastShortRest(actor);
			}
			else if (allowSpecificShortRest && actor.CharacterClass.ID == "CrashingTideID" && actor.ActiveBonuses.Any((CActiveBonus ab) => ab.Ability.ActiveBonusData.Flags.HasFlag(AbilityData.EActiveBonusFlag.CrashingTideRest) && CustomAbility.GetCancellableTideActiveBonuses(actor).Count > 0))
			{
				PerformCrashingTideShortRest(actor);
			}
			else if (actor.CharacterClass.ImprovedShortRest && (!actor.CharacterClass.ImprovedShortRestCanExcludeCards || !actor.CharacterClass.ExcludedCardsFromShortRest))
			{
				PerformImprovedShortRest(actor, tideActiveBonusCardsToKeep);
			}
			else
			{
				PerformNormalShortRest(actor, card, tideActiveBonusCardsToKeep);
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.PerformShortRest().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00012", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void PerformPyroclastShortRest(CPlayerActor actor)
	{
		Singleton<HelpBox>.Instance.ShowTranslated(LocalizationManager.GetTranslation("FH/GUI_TOOLTIP_CHOOSE_PYROCLAST_ACTIVE_BONUS_TO_DEACTIVATE"));
		if (FFSNetwork.IsOnline)
		{
			Singleton<UIReadyToggle>.Instance?.ToggleVisibility(visible: false);
		}
		Choreographer.s_Choreographer.readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONIMPROVEDSHORTREST, LocalizationManager.GetTranslation("GUI_SHORT_REST_CONFIRMATION"), hideOnClick: true, glowingEffect: false, interactable: true, disregardTurnControlForInteractability: true, haltActionProcessorIfDeactivated: false);
		Choreographer.s_Choreographer.readyButton.QueueAlternativeAction(() =>
		{
			if (!actor.CharacterClass.HasShortRested)
			{
				if (!IsFreeShortRest)
				{
					CardsHandManager.ShowCardsSelection(actor);
				}
				PerformShortRest(actor, allowSpecificShortRest: false);
				if (FFSNetwork.IsOnline && actor.IsUnderMyControl())
				{
					Synchronizer.SendGameAction(GameActionType.ConfirmAction, ActionProcessor.CurrentPhase);
				}
			}
		});
		actor.CharacterClass.ActivatedAbilityCards.Where((CAbilityCard card) => card.HasMandatoryBonus());
		CardHandMode mode = CardHandMode.SelectCardsWithMandatoryActiveBonusesToDeactivate;
		CardPileType cardPileType = CardPileType.Active;
		List<CardPileType> list = new List<CardPileType> { CardPileType.Active };
		int num = 1;
		bool flag = true;
		bool flag2 = true;
		Singleton<CardsHandManager>.Instance.Show(actor, mode, cardPileType, list, num, flag, flag2, allowFullCardPreview: true, CardActionsCommand.RESET, forceUseCurrentRoundCards: false, allowFullDeckPreview: true, null, (CAbilityCard card) => card.HasMandatoryBonus(), (CAbilityCard card) => card.HasMandatoryBonus());
	}

	private void PerformCrashingTideShortRest(CPlayerActor actor)
	{
		Singleton<HelpBox>.Instance.ShowTranslated(LocalizationManager.GetTranslation("FH/GUI_TOOLTIP_CHOOSE_TIDE_ACTIVE_BONUS_TO_KEEP"));
		if (FFSNetwork.IsOnline)
		{
			Singleton<UIReadyToggle>.Instance?.ToggleVisibility(visible: false);
		}
		Choreographer.s_Choreographer.readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONIMPROVEDSHORTREST, LocalizationManager.GetTranslation("GUI_SHORT_REST_CONFIRMATION"), hideOnClick: true, glowingEffect: false, interactable: true, disregardTurnControlForInteractability: true, haltActionProcessorIfDeactivated: false);
		Choreographer.s_Choreographer.readyButton.QueueAlternativeAction(() =>
		{
			if (!actor.CharacterClass.HasShortRested)
			{
				if (!IsFreeShortRest)
				{
					CardsHandManager.ShowCardsSelection(actor);
				}
				PerformShortRest(actor, allowSpecificShortRest: false);
				if (FFSNetwork.IsOnline && actor.IsUnderMyControl())
				{
					Synchronizer.SendGameAction(GameActionType.ConfirmAction, ActionProcessor.CurrentPhase);
				}
			}
		});
		List<CActiveBonus> tideActiveBonuses = CustomAbility.GetCancellableTideActiveBonuses(playerActor);
		CardHandMode mode = CardHandMode.SelectCardsWithTideActiveBonusesToKeep;
		CardPileType cardPileType = CardPileType.Active;
		List<CardPileType> list = new List<CardPileType> { CardPileType.Active };
		int num = 1;
		bool flag = true;
		bool flag2 = true;
		Singleton<CardsHandManager>.Instance.Show(actor, mode, cardPileType, list, num, flag, flag2, allowFullCardPreview: true, CardActionsCommand.RESET, forceUseCurrentRoundCards: false, allowFullDeckPreview: true, null, (CAbilityCard card) => tideActiveBonuses.Any((CActiveBonus ab) => ab.BaseCard == card), (CAbilityCard card) => tideActiveBonuses.Any((CActiveBonus ab) => ab.BaseCard == card));
	}

	private void PerformImprovedShortRest(CPlayerActor actor, IList<int> tideActiveBonusCardsToKeep = null)
	{
		CoroutineHelper.RunCoroutine(PerformImprovedShortRestCoroutine(actor, tideActiveBonusCardsToKeep));
	}

	private IEnumerator PerformImprovedShortRestCoroutine(CPlayerActor actor, IList<int> tideActiveBonusCardsToKeep = null)
	{
		if (!actor.DontCancelTidesOnRest)
		{
			Singleton<CardsHandManager>.Instance.CancelTideActiveBonuses(actor, tideActiveBonusCardsToKeep);
			while ((ScenarioRuleClient.IsProcessingOrMessagesQueued && !ScenarioRuleClient.IsMessageProcessingBlocked) || (Choreographer.s_Choreographer.m_MessageQueue.Count > 0 && !Choreographer.s_Choreographer.BlockClientMessageProcessing))
			{
				yield return new WaitForEndOfFrame();
			}
		}
		uint improvedShortRestDrawCount = actor.CharacterClass.ImprovedShortRestDrawCount;
		bool improvedShortRestCanExcludeCards = actor.CharacterClass.ImprovedShortRestCanExcludeCards;
		Singleton<CardsHandManager>.Instance.Hide();
		if (IsFreeShortRest)
		{
			FinalizeShortRest(actor, null, loseHp: false);
			yield break;
		}
		actor.CharacterClass.LongRest = true;
		string term = (improvedShortRestCanExcludeCards ? "GUI_TOOLTIP_EXCLUDE_CARDS_FROM_SHORT_REST" : "GUI_TOOLTIP_IMPROVED_SHORT_REST");
		Singleton<HelpBox>.Instance.ShowTranslated(LocalizationManager.GetTranslation(term));
		cardSelectableFilter = null;
		if (improvedShortRestCanExcludeCards)
		{
			int num = (int)Math.Floor((float)actor.CharacterClass.DiscardedAbilityCards.Count / 2f);
			Choreographer.s_Choreographer.readyButton.Toggle(active: false, ReadyButton.EButtonState.EREADYBUTTONNA, null, hideOnClick: true, glowingEffect: false, interactable: true, disregardTurnControlForInteractability: false, haltActionProcessorIfDeactivated: false);
			Singleton<CardsHandManager>.Instance.Show(actor, CardHandMode.ExcludeCardsFromShortRest, CardPileType.Any, CardPileType.Discarded, num, fadeUnselectableCards: true, highlightSelectableCards: true);
		}
		else if (improvedShortRestDrawCount != 0)
		{
			List<CAbilityCard> list = new List<CAbilityCard>(actor.CharacterClass.DiscardedAbilityCards);
			list.Shuffle(ScenarioManager.CurrentScenarioState.ScenarioRNG);
			List<CAbilityCard> cards = list.Take((int)Math.Min(list.Count, improvedShortRestDrawCount)).ToList();
			cardSelectableFilter = (CAbilityCard x) => cards.Contains(x);
			Choreographer.s_Choreographer.readyButton.Toggle(active: false, ReadyButton.EButtonState.EREADYBUTTONNA, null, hideOnClick: true, glowingEffect: false, interactable: true, disregardTurnControlForInteractability: false, haltActionProcessorIfDeactivated: false);
			Singleton<CardsHandManager>.Instance.Show(actor, CardHandMode.LoseCard, CardPileType.Any, CardPileType.Discarded, 1, fadeUnselectableCards: true, highlightSelectableCards: true, allowFullCardPreview: true, CardActionsCommand.RESET, forceUseCurrentRoundCards: false, allowFullDeckPreview: true, null, cardSelectableFilter);
		}
		else
		{
			Singleton<CardsHandManager>.Instance.Show(actor, CardHandMode.LoseCard, CardPileType.Any, CardPileType.Discarded, 1, fadeUnselectableCards: true, highlightSelectableCards: true, allowFullCardPreview: true, CardActionsCommand.RESET, forceUseCurrentRoundCards: false, allowFullDeckPreview: true, null, cardSelectableFilter);
			UpdateGeminateModule();
		}
	}

	public void OfferCancelAbilityCards(CPlayerActor actor, int suggestedDiscardAMount)
	{
		Singleton<CardsHandManager>.Instance.EnableCancelActiveAbilities = true;
		string translation = LocalizationManager.GetTranslation("GUI_EXHAUST_PHASE_PROMPT");
		string translation2 = LocalizationManager.GetTranslation(actor.ActorLocKey());
		string text = string.Format(translation, translation2, suggestedDiscardAMount);
		Singleton<HelpBox>.Instance.ShowTranslated(text);
		bool underMyControl = true;
		if (FFSNetwork.IsOnline)
		{
			Singleton<UIReadyToggle>.Instance?.ToggleVisibility(visible: false);
			if (!actor.IsUnderMyControl())
			{
				underMyControl = false;
				ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.CancelAbilityCards);
			}
			else
			{
				ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.CancelAbilityCards);
			}
		}
		Choreographer.s_Choreographer.readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONCONTINUE, LocalizationManager.GetTranslation("GUI_CONTINUE"), hideOnClick: true, glowingEffect: false, underMyControl, disregardTurnControlForInteractability: true, haltActionProcessorIfDeactivated: false);
		Choreographer.s_Choreographer.readyButton.QueueAlternativeAction(() =>
		{
			Singleton<CardsHandManager>.Instance.EnableCancelActiveAbilities = false;
			GameState.SelectedContinueAbility();
			if (FFSNetwork.IsOnline & underMyControl)
			{
				Synchronizer.SendGameAction(GameActionType.ConfirmAction, ActionProcessor.CurrentPhase);
			}
		});
		IEnumerable<CAbilityCard> discardableCards = playerActor.CharacterClass.ActivatedAbilityCards.Where((CAbilityCard x) => x.CanBeRemovedFromActiveBonuses(checkMandatory: false) && x.SelectedAction.CardPile == CBaseCard.ECardPile.Discarded);
		CardHandMode mode = CardHandMode.DeckSelection;
		CardPileType cardPileType = CardPileType.Any;
		List<CardPileType> list = new List<CardPileType> { CardPileType.Active };
		int num = 1;
		bool flag = true;
		bool flag2 = true;
		Singleton<CardsHandManager>.Instance.Show(actor, mode, cardPileType, list, num, flag, flag2, allowFullCardPreview: true, CardActionsCommand.RESET, forceUseCurrentRoundCards: false, allowFullDeckPreview: true, null, (CAbilityCard card) => discardableCards.Contains(card));
	}

	private void PerformNormalShortRest(CPlayerActor actor, AbilityCardUI longRestCard, IList<int> tideActiveBonusCardsToKeep = null)
	{
		CoroutineHelper.RunCoroutine(PerformNormalShortRestCoroutine(actor, longRestCard, tideActiveBonusCardsToKeep));
	}

	private IEnumerator PerformNormalShortRestCoroutine(CPlayerActor actor, AbilityCardUI longRestCard, IList<int> tideActiveBonusCardsToKeep = null)
	{
		if (!actor.DontCancelTidesOnRest)
		{
			Singleton<CardsHandManager>.Instance.CancelTideActiveBonuses(actor, tideActiveBonusCardsToKeep);
			while ((ScenarioRuleClient.IsProcessingOrMessagesQueued && !ScenarioRuleClient.IsMessageProcessingBlocked) || (Choreographer.s_Choreographer.m_MessageQueue.Count > 0 && !Choreographer.s_Choreographer.BlockClientMessageProcessing))
			{
				yield return new WaitForEndOfFrame();
			}
		}
		if (IsFreeShortRest)
		{
			FinalizeShortRest(actor, null, loseHp: false);
			yield break;
		}
		Singleton<CardsHandManager>.Instance.Show(actor, CardHandMode.LoseCard, CardPileType.Any, CardPileType.Discarded, 1, fadeUnselectableCards: true, highlightSelectableCards: true);
		List<CAbilityCard> list = actor.CharacterClass.DiscardedAbilityCards;
		if (actor.CharacterClass.ExcludedCardsFromShortRest && !actor.CharacterClass.ExcludedFromShortRestAbilityCards.IsNullOrEmpty())
		{
			list = actor.CharacterClass.DiscardedAbilityCards.FindAll((CAbilityCard dac) => !actor.CharacterClass.ExcludedFromShortRestAbilityCards.Contains(dac));
		}
		CAbilityCard lostCard = list.FirstOrDefault((CAbilityCard x) => x.ID == GameState.s_ShortRestCardToBurnID);
		if (lostCard == null)
		{
			int index = ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(list.Count);
			lostCard = list[index];
			GameState.s_ShortRestCardToBurnID = lostCard.ID;
		}
		if (LevelEventsController.s_EventsControllerActive)
		{
			Singleton<LevelEventsController>.Instance?.RunActionIfShortRestDataPending((string cardData) =>
			{
				if (actor.CharacterClass.DiscardedAbilityCards.Any((CAbilityCard ac) => ac.Name == cardData))
				{
					lostCard = actor.CharacterClass.DiscardedAbilityCards.Single((CAbilityCard ac) => ac.Name == cardData);
				}
			});
		}
		AbilityCardUI cardUI = GetCardUI(lostCard);
		cardUI.LostAnimation(active: true, 0.5f);
		string text = string.Format(LocalizationManager.GetTranslation("GUI_LOSE_CARD"), cardUI.fullAbilityCard.Title);
		string text2 = LocalizationManager.GetTranslation("GUI_REDRAW_CARD");
		if (actor.Tokens.HasKey(CCondition.ENegativeCondition.Brittle))
		{
			int num = text2.LastIndexOf('1');
			text2 = text2.Substring(0, num) + "2" + text2.Substring(num + 1);
		}
		CacheSelectedCards();
		DeselectAllCards();
		bool wasSelectable = cardUI.IsSelectable;
		cardUI.SetSelectable(isOn: false, isHoverable: false);
		foreach (CardsHandUI item in Singleton<CardsHandManager>.Instance.cardHandsUI)
		{
			foreach (AbilityCardUI item2 in item.cardsUI.Where((AbilityCardUI it) => it != cardUI))
			{
				item2.SetValid(isValid: false);
			}
		}
		shortRest.Hide();
		longRestCard.Toggle(enabled: false);
		Singleton<CardsHandManager>.Instance.ShowTabs(show: false);
		Choreographer.s_Choreographer.readyButton.ToggleVisibility(active: false);
		bool canPreviewAllDecks = Singleton<CardsHandManager>.Instance.IsFullDeckPreviewAllowed;
		Singleton<CardsHandManager>.Instance.IsFullDeckPreviewAllowed = false;
		Action<int> onSelectedOption = (int supplementaryDataIDMin) =>
		{
			cardUI.SetSelectable(wasSelectable);
			foreach (AbilityCardUI item3 in cardsUI.Where((AbilityCardUI it) => it != cardUI))
			{
				item3.SetValid(isValid: true);
			}
			if (FFSNetwork.IsOnline && actor.IsUnderMyControl())
			{
				Synchronizer.SendGameAction(GameActionType.ShortRest, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, actor.ID, supplementaryDataIDMin);
			}
		};
		Action onFinish = () =>
		{
			longRestCard.Toggle(enabled: true);
			Singleton<CardsHandManager>.Instance.ShowTabs(show: true);
			Singleton<CardsHandManager>.Instance.IsFullDeckPreviewAllowed = canPreviewAllDecks;
			Choreographer.s_Choreographer.readyButton.ToggleVisibility(active: true);
		};
		List<CAbilityCard> list2 = actor.CharacterClass.DiscardedAbilityCards.ToList();
		if (actor.CharacterClass.ExcludedCardsFromShortRest)
		{
			foreach (CAbilityCard excludedFromShortRestAbilityCard in actor.CharacterClass.ExcludedFromShortRestAbilityCards)
			{
				list2.Remove(excludedFromShortRestAbilityCard);
			}
		}
		if (list2.Count > 1)
		{
			DialogPopup dialogPopup = UIManager.Instance.dialogPopup;
			GameObject content = GetCardUI(lostCard).fullAbilityCard.gameObject;
			string helpBoxText = text;
			DialogOption[] array = new DialogOption[2];
			InputAction confirm = InputFHManager.Controls.Generic.Confirm;
			UnityAction onClick = () =>
			{
				onSelectedOption(0);
				onFinish();
				FinalizeShortRest(actor, cardUI, loseHp: false);
			};
			UnityAction onEnter = () =>
			{
				cardUI.fullAbilityCard.ToggleCardEffect(active: false, CardEffects.FXTask.BurnCard);
			};
			UnityAction onExit = () =>
			{
				cardUI.fullAbilityCard.RefreshPile();
			};
			CPlayerActor enabledForPlayer = actor;
			FHDialogButtonVisuals.VisualParams? visualParams = m_BurnOptionButtonVisuals;
			array[0] = new DialogOption(text, confirm, onClick, onEnter, onExit, enabled: true, visible: true, null, visualParams, enabledForPlayer);
			string text3 = text2;
			InputAction skipAction = InputFHManager.Controls.Scenario.SkipAction;
			UnityAction action = () =>
			{
				onSelectedOption(1);
				ShortRestChooseRedraw(actor, cardUI, onFinish);
			};
			enabledForPlayer = actor;
			visualParams = m_RedrawOptionButtonVisuals;
			array[1] = new DialogOption(text3, skipAction, action, enabled: true, visible: true, null, visualParams, enabledForPlayer);
			dialogPopup.Show(content, array, -1, null, helpBoxText);
		}
		else
		{
			DialogPopup dialogPopup2 = UIManager.Instance.dialogPopup;
			GameObject content2 = GetCardUI(lostCard).fullAbilityCard.gameObject;
			string helpBoxText = text;
			DialogOption[] array2 = new DialogOption[1];
			InputAction confirm2 = InputFHManager.Controls.Generic.Confirm;
			UnityAction onClick2 = () =>
			{
				onSelectedOption(0);
				onFinish();
				FinalizeShortRest(actor, cardUI, loseHp: false);
			};
			UnityAction onEnter2 = () =>
			{
				cardUI.fullAbilityCard.ToggleCardEffect(active: false, CardEffects.FXTask.BurnCard);
			};
			UnityAction onExit2 = () =>
			{
				cardUI.fullAbilityCard.RefreshPile();
			};
			CPlayerActor enabledForPlayer = actor;
			FHDialogButtonVisuals.VisualParams? visualParams = m_BurnRequiredButtonVisuals;
			array2[0] = new DialogOption(text, confirm2, onClick2, onEnter2, onExit2, enabled: true, visible: true, null, visualParams, enabledForPlayer);
			dialogPopup2.Show(content2, array2, -1, null, helpBoxText);
		}
		UIManager.Instance.dialogPopup.transform.SetParent(Singleton<CardsHandManager>.Instance.transform);
		UIManager.Instance.dialogPopup.transform.SetAsFirstSibling();
	}

	private void ShortRestChooseRedraw(CPlayerActor playerActor, AbilityCardUI firstCard, Action onConfirmed = null)
	{
		try
		{
			firstCard.LostAnimation(active: false, 0.5f);
			List<CAbilityCard> list = new List<CAbilityCard>(playerActor.CharacterClass.DiscardedAbilityCards);
			if (playerActor.CharacterClass.ExcludedCardsFromShortRest && !playerActor.CharacterClass.ExcludedFromShortRestAbilityCards.IsNullOrEmpty())
			{
				list = playerActor.CharacterClass.DiscardedAbilityCards.FindAll((CAbilityCard dac) => !playerActor.CharacterClass.ExcludedFromShortRestAbilityCards.Contains(dac));
			}
			list.Remove(firstCard.AbilityCard);
			int index = ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(list.Count);
			CAbilityCard lostCard = list[index];
			GameState.s_ShortRestCardToBurnID = lostCard.ID;
			if (LevelEventsController.s_EventsControllerActive)
			{
				Singleton<LevelEventsController>.Instance?.RunActionIfShortRestDataPending((string cardData) =>
				{
					if (playerActor.CharacterClass.DiscardedAbilityCards.Any((CAbilityCard ac) => ac.Name == cardData))
					{
						lostCard = playerActor.CharacterClass.DiscardedAbilityCards.Single((CAbilityCard ac) => ac.Name == cardData);
					}
				});
			}
			AbilityCardUI cardUI = GetCardUI(lostCard);
			cardUI.LostAnimation(active: true, 0.5f);
			string text = string.Format(LocalizationManager.GetTranslation("GUI_LOSE_CARD"), cardUI.fullAbilityCard.Title);
			bool wasSelectable = cardUI.IsSelectable;
			cardUI.SetSelectable(isOn: false, isHoverable: false);
			foreach (AbilityCardUI item in cardsUI.Where((AbilityCardUI it) => it != cardUI))
			{
				item.SetValid(isValid: false);
			}
			DialogPopup dialogPopup = UIManager.Instance.dialogPopup;
			GameObject content = cardUI.fullAbilityCard.gameObject;
			string helpBoxText = text;
			DialogOption[] array = new DialogOption[1];
			InputAction confirm = InputFHManager.Controls.Generic.Confirm;
			UnityAction onClick = () =>
			{
				cardUI.SetSelectable(wasSelectable);
				foreach (AbilityCardUI item2 in cardsUI.Where((AbilityCardUI it) => it != cardUI))
				{
					item2.SetValid(isValid: true);
				}
				onConfirmed?.Invoke();
				if (FFSNetwork.IsOnline && playerActor.IsUnderMyControl())
				{
					Synchronizer.SendGameAction(GameActionType.ShortRest, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, playerActor.ID);
				}
				FinalizeShortRest(playerActor, cardUI, loseHp: true);
			};
			UnityAction onEnter = () =>
			{
				cardUI.fullAbilityCard.ToggleCardEffect(active: false, CardEffects.FXTask.BurnCard);
			};
			UnityAction onExit = () =>
			{
				cardUI.fullAbilityCard.RefreshPile();
			};
			CPlayerActor enabledForPlayer = playerActor;
			FHDialogButtonVisuals.VisualParams? visualParams = m_BurnRequiredButtonVisuals;
			array[0] = new DialogOption(text, confirm, onClick, onEnter, onExit, enabled: true, visible: true, null, visualParams, enabledForPlayer);
			dialogPopup.Show(content, array, -1, null, helpBoxText);
			UIManager.Instance.dialogPopup.transform.SetParent(Singleton<CardsHandManager>.Instance.transform);
			UIManager.Instance.dialogPopup.transform.SetAsFirstSibling();
			UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.ShortRestChoseToRedraw));
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.PerformFinalShortRest().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00013", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void FinalizeShortRest(CPlayerActor playerActor, AbilityCardUI cardUI, bool loseHp, bool networkActionIfOnline = true, bool fromStateUpdate = false)
	{
		try
		{
			GameState.s_ShortRestCardToBurnID = 0;
			if (cardUI != null)
			{
				ShowOrHideInternal(show: true);
				cardUI.Toggle(enabled: true);
				if (cardsStateBeforePreview.ContainsKey(cardUI))
				{
					cardsStateBeforePreview[cardUI].isActive = true;
				}
				cardUI.fullAbilityCard.ToggleCardEffect(active: true, CardEffects.FXTask.BurnCard);
			}
			ScenarioRuleClient.ShortRestPlayer(playerActor, cardUI?.AbilityCard, loseHp, !networkActionIfOnline, fromStateUpdate);
			shortRested = true;
			UpdateShortRest();
			if (gameObject.activeSelf && cardUI != null)
			{
				Log.NetworkInfo("Starting Short rest card animation.");
				UIManager.Instance.StartCoroutine(AnimateCardsLost(new List<AbilityCardUI> { cardUI }, () =>
				{
					UpdateView(currentMode, CardPileType.Any, new List<CardPileType>
					{
						CardPileType.Hand,
						CardPileType.Round,
						CardPileType.Active
					}, playerActor.MaxCardsClampedToHand);
				}));
			}
			UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.ShortRestChoseToBurn));
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.FinalizeShortRest().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00014", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private AbilityCardUI GetCardUI(CAbilityCard card)
	{
		try
		{
			foreach (AbilityCardUI item in cardsUI)
			{
				if (item.AbilityCard == card)
				{
					return item;
				}
			}
			return null;
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.GetCardUI().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00015", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
			return null;
		}
	}

	public IEnumerator AnimateCardsLost(List<AbilityCardUI> selectedCards, Action onCompleteCallback = null)
	{
		if (!gameObject.activeInHierarchy)
		{
			InstantCardLost(selectedCards);
			onCompleteCallback?.Invoke();
			yield break;
		}
		AnimatingLostCards = true;
		List<AbilityCardUI> tempSelectedCards = selectedCards.ToList();
		for (int i = 0; i < cardsUI.Count; i++)
		{
			cardsUI[i].MiniAbilityCard.DisableGlowSelected();
			cardsUI[i].MiniAbilityCard.DisplayIndicator(active: false);
		}
		onAnimateCardLostCallback = onCompleteCallback;
		animatedLosingCard = true;
		UIManager.Instance.RequestToggleLockUI(active: true, gameObject);
		AudioController.Play("PlaySound_CardUI_BurnedCard", transform, null, attachToParent: false);
		if (isPreviewingCards)
		{
			ToggleFullCardsPreview(active: false);
			yield return new WaitForEndOfFrame();
		}
		UpdateCards(CardPileType.Any, EUpdateCardViewMode.None);
		foreach (AbilityCardUI item2 in tempSelectedCards)
		{
			CardHandHeader header = GetHeader(item2);
			if (header != null)
			{
				header.Show();
			}
		}
		yield return new WaitForEndOfFrame();
		SortCards(reorderPositions: true, toggleHeader: false);
		bool flag = false;
		foreach (AbilityCardUI item3 in tempSelectedCards)
		{
			flag |= item3.LostAnimation(active: true, cardHighlightTime * animationSpeed);
		}
		if (flag)
		{
			yield return new WaitForSecondsRealtime(cardHighlightTime * animationSpeed);
		}
		RefreshHeaders();
		List<RectTransform> list = (from it in GetOrderedCardScrollElements()
			where it.gameObject.activeSelf
			select it).ToList();
		foreach (AbilityCardUI item4 in tempSelectedCards)
		{
			RectTransform item = item4.transform as RectTransform;
			if (list.IndexOf(item) >= 0)
			{
				item4.UpdateCard();
				item4.LostAnimation(active: false, cardHighlightTime * animationSpeed);
			}
		}
		yield return new WaitForSecondsRealtime(lostCardMoveTime * animationSpeed - cardsReorderingOffsetTime * animationSpeed - discardedCardsTransitionTime * animationSpeed);
		foreach (AbilityCardUI item5 in cardsUI)
		{
			item5.UpdateCard();
		}
		SortCards();
		animatedLosingCard = false;
		UIManager.Instance.RequestToggleLockUI(active: false, gameObject);
		onAnimateCardLostCallback = null;
		onCompleteCallback?.Invoke();
		AnimatingLostCards = false;
	}

	private void InstantCardLost(List<AbilityCardUI> selectedCards)
	{
		if (isPreviewingCards)
		{
			ToggleFullCardsPreview(active: false);
		}
		SortCards();
		foreach (AbilityCardUI item in cardsUI)
		{
			item.MiniAbilityCard.DisableGlowSelected();
			item.MiniAbilityCard.DisplayIndicator(active: false);
			item.UpdateCard();
		}
	}

	public void RefreshConnectionState()
	{
		bool unfocused = FFSNetwork.IsOnline && !playerActor.IsUnderMyControl();
		itemsUI.SetUnfocused(unfocused);
		modifiersController.SetUnfocused(unfocused);
	}

	private IEnumerable<RectTransform> GetOrderedCardScrollElements()
	{
		return from it in cardsUI.Select((AbilityCardUI it) => it.transform as RectTransform).Append(takenHeader.transform as RectTransform).Append(burntHeader.transform as RectTransform)
				.Append(discardedHeader.transform as RectTransform)
				.Append(shortRest.transform as RectTransform)
			orderby it.GetSiblingIndex()
			select it;
	}

	private void CancelAnimateCardLost()
	{
		if (!animatedLosingCard && !ScenarioManager.IsFastReloading)
		{
			return;
		}
		animatedLosingCard = false;
		animations.ForEach((Tween it) =>
		{
			it.Kill();
		});
		animations.Clear();
		StopAllCoroutines();
		SortCards();
		foreach (AbilityCardUI item in cardsUI)
		{
			item.CancelLostAnimation();
			item.UpdateCard();
		}
		UIManager.Instance.RequestToggleLockUI(active: false, gameObject);
		onAnimateCardLostCallback?.Invoke();
		onAnimateCardLostCallback = null;
	}

	public bool IsGivenCard(CAbilityCard card)
	{
		return PlayerActor.CharacterClass.GivenCards.Contains(card);
	}

	private void SortCards(bool reorderPositions = true, bool toggleHeader = true)
	{
		try
		{
			cardsUI.Sort((AbilityCardUI left, AbilityCardUI right) =>
			{
				bool flag = IsGivenCard(left.AbilityCard);
				bool flag2 = IsGivenCard(right.AbilityCard);
				return (flag != flag2) ? ((!flag) ? 1 : (-1)) : left.CompareTo(right);
			});
			if (reorderPositions)
			{
				if (toggleHeader)
				{
					discardedHeader.Hide();
					burntHeader.Hide();
					takenHeader.Hide();
				}
				List<CardHandHeader> list = new List<CardHandHeader>();
				AbilityCardUI abilityCardUI = null;
				int num = 0;
				foreach (AbilityCardUI item in cardsUI)
				{
					if (item.IsLongRest)
					{
						abilityCardUI = item;
						discardedHeader.transform.SetAsLastSibling();
						list.Add(discardedHeader);
						num++;
						continue;
					}
					if (item.CardType == CardPileType.Unselected && !list.Contains(burntHeader))
					{
						list.Add(burntHeader);
						burntHeader.transform.SetAsLastSibling();
						num++;
					}
					CardHandHeader header = GetHeader(item);
					if (header != null)
					{
						if (!list.Contains(header))
						{
							list.Add(header);
							header.transform.SetAsLastSibling();
							num++;
						}
						if (toggleHeader && item.gameObject.activeSelf)
						{
							header.Show();
						}
					}
					if (cardsStateBeforePreview.ContainsKey(item))
					{
						cardsStateBeforePreview[item].order = num;
					}
					item.transform.SetAsLastSibling();
					num++;
				}
				if (abilityCardUI != null)
				{
					abilityCardUI.gameObject.SetActive(value: false);
					abilityCardUI.transform.SetAsLastSibling();
					shortRest.transform.SetAsLastSibling();
				}
				if (!list.Contains(burntHeader))
				{
					burntHeader.transform.SetAsLastSibling();
				}
			}
			UpdateNavigation();
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.SortCards().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00017", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void UpdateNavigation()
	{
		if (currentMode == CardHandMode.ActionSelection)
		{
			_selectionController.StartSelections = (from s in selectedCardsUI.Where((AbilityCardUI c) => c.gameObject.activeSelf).SelectMany((AbilityCardUI c) => new Button[2]
				{
					c.fullAbilityCard.topActionButton.actionButton,
					c.fullAbilityCard.bottomActionButton.actionButton
				})
				where s != null
				select s).Concat(new Selectable[1] { Singleton<CardsHandManager>.Instance.longRestConfirmationButton.Selectable }).ToList();
		}
		else
		{
			_selectionController.StartSelections = (from c in cardsUI
				where c.gameObject.activeSelf
				select c.Selectable).Concat(new Selectable[1] { shortRest.Button }).ToList();
		}
	}

	private CardHandHeader GetHeader(AbilityCardUI card)
	{
		switch (card.CardType)
		{
		case CardPileType.Active:
			return null;
		case CardPileType.Discarded:
			return discardedHeader;
		case CardPileType.Lost:
		case CardPileType.Permalost:
			return burntHeader;
		case CardPileType.Taken:
			return takenHeader;
		default:
			return null;
		}
	}

	public List<AbilityCardUI> GetActiveAbilityCards()
	{
		return cardsUI.FindAll((AbilityCardUI it) => it.CardType == CardPileType.Active);
	}

	private void UpdateCards(CardPileType filtertype, EUpdateCardViewMode updateView)
	{
		try
		{
			List<CAbilityCard> pile = ((playerActor.CharacterClass.RoundAbilityCards.Count == playerActor.SelectedRoundCardsCount || useCurrentRoundCards) ? playerActor.CharacterClass.RoundAbilityCards : playerActor.CharacterClass.RoundAbilityCardsSelectedInCardSelection);
			bool flag = updateView != EUpdateCardViewMode.None;
			if (flag)
			{
				discardedHeader.Hide();
				burntHeader.Hide();
				takenHeader.Hide();
			}
			bool flag2 = ScenarioScript.current != null && ScenarioScript.current.ShowTakenPile();
			foreach (AbilityCardUI item in cardsUI)
			{
				if ((playerActor.CharacterClass.TakenCards.Contains(item.AbilityCard) && !flag2) || playerActor.CharacterClass.OwnedUnusedCards.Contains(item.AbilityCard))
				{
					item.Toggle(enabled: false);
					if (cardsStateBeforePreview.TryGetValue(item, out var value))
					{
						value.isActive = false;
					}
				}
				switch (filtertype)
				{
				case CardPileType.Any:
					if (item.IsLongRest && PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest && !IsImprovedLongResting)
					{
						item.SetType(playerActor.IsLongRestSelected ? CardPileType.Round : CardPileType.Hand);
						if (updateView != EUpdateCardViewMode.None)
						{
							item.UpdateCard();
						}
						bool isActive = !GameState.s_AutoSelectPlayerCards;
						item.Toggle(isActive);
						if (cardsStateBeforePreview.ContainsKey(item))
						{
							cardsStateBeforePreview[item].isActive = isActive;
						}
					}
					else
					{
						if (UpdateCard(item, CardPileType.Round, pile, updateView, cardVisibleFilter) || UpdateCard(item, CardPileType.Active, playerActor.CharacterClass.ActivatedAbilityCards, updateView, cardVisibleFilter) || UpdateCard(item, CardPileType.Hand, playerActor.CharacterClass.HandAbilityCards, updateView, cardVisibleFilter))
						{
							continue;
						}
						if (UpdateCard(item, CardPileType.Discarded, playerActor.CharacterClass.DiscardedAbilityCards, updateView, cardVisibleFilter))
						{
							if (flag)
							{
								discardedHeader.Show();
							}
						}
						else if (UpdateCard(item, CardPileType.Lost, playerActor.CharacterClass.LostAbilityCards, updateView, cardVisibleFilter))
						{
							if (flag)
							{
								burntHeader.Show();
							}
						}
						else if (UpdateCard(item, CardPileType.Permalost, playerActor.CharacterClass.PermanentlyLostAbilityCards, updateView, cardVisibleFilter))
						{
							if (flag)
							{
								burntHeader.Show();
							}
						}
						else if (flag2 && UpdateCard(item, CardPileType.Taken, playerActor.CharacterClass.TakenCards, updateView, cardVisibleFilter))
						{
							if (flag)
							{
								ShowTakenHeader();
							}
						}
						else if (!UpdateCard(item, CardPileType.ExtraTurn, playerActor.CharacterClass.ExtraTurnCardsSelectedInCardSelection, updateView, cardVisibleFilter))
						{
						}
					}
					continue;
				case CardPileType.Round:
					UpdateCard(item, filtertype, pile, updateView, cardVisibleFilter);
					continue;
				case CardPileType.Active:
					UpdateCard(item, filtertype, playerActor.CharacterClass.ActivatedAbilityCards, updateView, cardVisibleFilter);
					continue;
				case CardPileType.Hand:
					UpdateCard(item, filtertype, playerActor.CharacterClass.HandAbilityCards, updateView, cardVisibleFilter);
					continue;
				case CardPileType.Discarded:
					if (UpdateCard(item, filtertype, playerActor.CharacterClass.DiscardedAbilityCards, updateView, cardVisibleFilter) & flag)
					{
						discardedHeader.Show();
					}
					continue;
				case CardPileType.Lost:
					if (UpdateCard(item, filtertype, playerActor.CharacterClass.LostAbilityCards, updateView, cardVisibleFilter) & flag)
					{
						burntHeader.Show();
					}
					continue;
				case CardPileType.Permalost:
					if (UpdateCard(item, filtertype, playerActor.CharacterClass.PermanentlyLostAbilityCards, updateView, cardVisibleFilter) & flag)
					{
						burntHeader.Show();
					}
					continue;
				case CardPileType.Unselected:
					UpdateCard(item, filtertype, playerActor.CharacterClass.UnselectedAbilityCards.FindAll((CAbilityCard c) => c.Level <= playerActor.Level), updateView, cardVisibleFilter);
					continue;
				case CardPileType.ExtraTurn:
					UpdateCard(item, filtertype, playerActor.CharacterClass.ExtraTurnCardsSelectedInCardSelection, updateView, cardVisibleFilter);
					continue;
				case CardPileType.SelectedAbilityCardsOutOfPhase:
				{
					List<CAbilityCard> selectedAbilityCardsOutOfPhase = playerActor.CharacterClass.SelectedAbilityCardsOutOfPhase;
					if (PhaseManager.CurrentPhase is CPhaseActionSelection && GameState.CurrentSelectAndPlayCardAbility != null)
					{
						selectedAbilityCardsOutOfPhase = GameState.CurrentSelectAndPlayCardAbility.selectedAbilityCardsOutOfPhase;
					}
					UpdateCard(item, filtertype, selectedAbilityCardsOutOfPhase, updateView, cardVisibleFilter);
					continue;
				}
				}
				if (flag2 && filtertype == CardPileType.Taken)
				{
					if (UpdateCard(item, filtertype, playerActor.CharacterClass.TakenCards, updateView, cardVisibleFilter))
					{
						ShowTakenHeader();
					}
					continue;
				}
				switch (filtertype)
				{
				case CardPileType.OwnedUnused:
					UpdateCard(item, filtertype, playerActor.CharacterClass.OwnedUnusedCards, updateView, cardVisibleFilter);
					break;
				case CardPileType.None:
					UpdateCard(item, filtertype, null, updateView, cardVisibleFilter);
					break;
				}
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.UpdateCards().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00018", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void RefreshHeaders()
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = ScenarioScript.current != null && ScenarioScript.current.ShowTakenPile();
		foreach (AbilityCardUI item in cardsUI.Where((AbilityCardUI it) => it.gameObject.activeSelf))
		{
			if (flag4 && !flag3 && playerActor.CharacterClass.TakenCards.Contains(item.AbilityCard))
			{
				flag3 = true;
			}
			else if (!flag && playerActor.CharacterClass.DiscardedAbilityCards.Contains(item.AbilityCard))
			{
				flag = true;
			}
			else if (!flag2 && playerActor.CharacterClass.LostAbilityCards.Contains(item.AbilityCard))
			{
				flag2 = true;
			}
			else if (!flag2 && playerActor.CharacterClass.PermanentlyLostAbilityCards.Contains(item.AbilityCard))
			{
				flag2 = true;
			}
		}
		if (!flag)
		{
			discardedHeader.Hide();
		}
		else
		{
			discardedHeader.Show();
		}
		if (!flag2)
		{
			burntHeader.Hide();
		}
		else
		{
			burntHeader.Show();
		}
		if (!flag3)
		{
			takenHeader.Hide();
		}
		else
		{
			ShowTakenHeader();
		}
	}

	private void ShowTakenHeader()
	{
		if (ScenarioScript.current != null)
		{
			takenHeader.Show();
			takenHeader.SetKeys(ScenarioScript.current.TakenPileNameKey(), ScenarioScript.current.TakenPileNameKey(), ScenarioScript.current.TakenPileTooltipKey());
		}
	}

	private bool UpdateCard(AbilityCardUI cardUI, CardPileType displayMode, List<CAbilityCard> pile, EUpdateCardViewMode updateView, Func<CAbilityCard, bool> cardVisibleFilter)
	{
		if (pile != null && pile.Contains(cardUI.AbilityCard))
		{
			if (cardVisibleFilter != null && !cardVisibleFilter(cardUI.AbilityCard))
			{
				cardUI.Toggle(enabled: false);
				if (cardsStateBeforePreview.TryGetValue(cardUI, out var value))
				{
					value.isActive = false;
				}
				return false;
			}
			cardUI.SetType(displayMode);
			if (updateView != EUpdateCardViewMode.None)
			{
				if (updateView == EUpdateCardViewMode.Refresh)
				{
					cardUI.UpdateCard();
				}
				cardUI.Toggle(enabled: true);
				if (cardsStateBeforePreview.TryGetValue(cardUI, out var value2))
				{
					value2.isActive = true;
				}
			}
			return true;
		}
		if (updateView != EUpdateCardViewMode.None)
		{
			cardUI.Toggle(enabled: false);
			if (cardsStateBeforePreview.TryGetValue(cardUI, out var value3))
			{
				value3.isActive = false;
			}
		}
		return false;
	}

	public void PreviewActionCards(Transform holder)
	{
		SetLayoutSettings(m_ActionSelectionLayoutSettings);
		AbilityCardUI abilityCardUI = null;
		foreach (AbilityCardUI item in cardsUI)
		{
			if (item.IsLongRest)
			{
				abilityCardUI = item;
			}
			item.SetLocalScale(Vector3.one);
			bool flag = playerActor.CharacterClass.RoundAbilityCards.Contains(item.AbilityCard) || playerActor.CharacterClass.RoundAbilityCardsSelectedInCardSelection.Contains(item.AbilityCard);
			item.Toggle(flag);
			item.transform.SetParent(holder);
			if (flag)
			{
				item.IsInteractable = true;
				item.ToggleFullCard(active: true);
				item.canvasGroup.alpha = 1f;
				item.fullAbilityCard.BlockRaycasts(block: false);
			}
		}
		UpdateLongRest();
		UpdateShortRest();
		abilityCardUI?.Toggle(enabled: false);
	}

	public void HidePreviewActionCards(CardPileType filter, CardHandMode mode)
	{
		bool flag = mode == CardHandMode.ActionSelection;
		SetLayoutSettings(flag ? m_ActionSelectionLayoutSettings : m_CardsSelectionLayoutSettings);
		foreach (AbilityCardUI item in cardsUI)
		{
			if (item.IsLongRest)
			{
				item.Toggle(item.CardType == CardPileType.Hand && PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest);
			}
			item.transform.SetParent(abilityCardsHolder);
			item.SetLocalScale(Vector3.one);
			if (playerActor.CharacterClass.RoundAbilityCards.Contains(item.AbilityCard) || playerActor.CharacterClass.RoundAbilityCardsSelectedInCardSelection.Contains(item.AbilityCard))
			{
				item.ToggleFullCard(flag);
				item.fullAbilityCard.BlockRaycasts(block: true);
			}
		}
		UpdateCards(filter, EUpdateCardViewMode.OnyActive);
		shortRest.Hide();
	}

	private void SetMode(CardHandMode newMode, CardPileType filterType = CardPileType.Any, List<CardPileType> selectableCardTypes = null, bool fadeUnselectableCards = false, bool highlightSelectableCards = false, bool longRestAvailable = false, CardActionsCommand commandCardActions = CardActionsCommand.RESET, Func<CAbilityCard, bool> cardSelectableFilter = null)
	{
		try
		{
			if (currentMode == CardHandMode.SelectCardsWithActiveBonusTracker && newMode != currentMode)
			{
				Choreographer.s_Choreographer.ClearGenericButton();
			}
			selectedCardsUI.Clear();
			previousMode = currentMode;
			currentMode = newMode;
			if (!cardsSpawned)
			{
				Log.GUIError("Cards are not spawned.");
				return;
			}
			if (currentMode == CardHandMode.ActionSelection)
			{
				bool reset = commandCardActions != CardActionsCommand.NONE;
				foreach (AbilityCardUI item in cardsUI)
				{
					if (GameState.IsSelectingCardForMode(out var modeAbility))
					{
						if (modeAbility.SelectedModeCard == item.AbilityCard)
						{
							selectedCardsUI.Add(item);
						}
					}
					else if (GameState.IsPerformingCardOutOfPhase)
					{
						if (playerActor.CharacterClass.SelectedAbilityCardsOutOfPhase.Contains(item.AbilityCard))
						{
							selectedCardsUI.Add(item);
						}
					}
					else if (playerActor.IsTakingExtraTurn)
					{
						if (playerActor.CharacterClass.ExtraTurnCardsSelectedInCardSelection.Contains(item.AbilityCard))
						{
							selectedCardsUI.Add(item);
						}
					}
					else if (playerActor.CharacterClass.RoundAbilityCards.Contains(item.AbilityCard) || playerActor.CharacterClass.RoundAbilityCardsSelectedInCardSelection.Contains(item.AbilityCard) || (playerActor.CharacterClass.LongRest && item.IsLongRest))
					{
						selectedCardsUI.Add(item);
					}
				}
				if (selectedCardsUI.Count > 0 && !IsLongRestSelected())
				{
					if (commandCardActions != CardActionsCommand.FORCE_RESET && (previousMode == CardHandMode.RecoverDiscardedCard || previousMode == CardHandMode.RecoverLostCard))
					{
						reset = false;
					}
					FullAbilityCard firstCard = ((selectedCardsUI.Count >= 1) ? selectedCardsUI[0].fullAbilityCard : null);
					FullAbilityCard secondCard = ((selectedCardsUI.Count >= 2) ? selectedCardsUI[1].fullAbilityCard : null);
					FullAbilityCard thirdCard = ((selectedCardsUI.Count >= 3) ? selectedCardsUI[2].fullAbilityCard : null);
					CAbility currentAbility = Choreographer.s_Choreographer.m_CurrentAbility;
					if (currentAbility == null || !currentAbility.IsControlAbility || currentAbility is CAbilitySelectAndPlayCard)
					{
						cardsActionController.Init(firstCard, secondCard, thirdCard, reset, playerActor.TakingExtraTurnOfType, playerActor);
					}
				}
			}
			else if (currentMode == CardHandMode.CardsSelection && ((selectedCardsUI.Count != playerActor.CharacterClass.RoundAbilityCards.Count && !GameState.IsPerformingCardOutOfPhase && !GameState.IsSelectingCardForMode()) || PhaseManager.IsInCardsSelectionPhase))
			{
				selectedCardsUI.Clear();
				foreach (CAbilityCard roundAbilityCard in playerActor.CharacterClass.RoundAbilityCards)
				{
					AbilityCardUI cardUI = GetCardUI(roundAbilityCard);
					selectedCardsUI.Insert(0, cardUI);
				}
				if (playerActor.IsLongRestSelected)
				{
					selectedCardsUI.Add(GetCard(-1));
				}
			}
			bool flag = !FFSNetwork.IsOnline || playerActor == null || playerActor.IsUnderMyControl();
			if (((currentMode == CardHandMode.CardsSelection) & flag) && GameState.s_ShortRestingPlayer != null)
			{
				flag = false;
				if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
				{
					flag = playerActor == GameState.s_ShortRestingPlayer;
				}
			}
			bool flag2 = flag;
			if ((currentMode == CardHandMode.CardsSelection) & flag2)
			{
				flag2 = !Singleton<UIReadyToggle>.Instance.PlayersReady.Contains(PlayerRegistry.MyPlayer) && !Singleton<UIReadyToggle>.Instance.IsProgressingBar;
			}
			if (currentMode == CardHandMode.ActionSelection && playerActor.TakingExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater)
			{
				Func<CAbilityCard, bool> filter = cardSelectableFilter;
				cardSelectableFilter = (CAbilityCard card) => playerActor.Initiative() < card.Initiative && (filter == null || filter(card));
			}
			else if (currentMode == CardHandMode.LoseCard)
			{
				Func<CAbilityCard, bool> filter2 = cardSelectableFilter;
				cardSelectableFilter = (CAbilityCard card) => !card.SupplyCard && (filter2 == null || filter2(card));
				if (flag && PhaseManager.PhaseType == CPhase.PhaseType.ShortRest && !playerActor.CharacterClass.ImprovedShortRest)
				{
					flag2 = playerActor == GameState.s_ShortRestingPlayer && playerActor.CharacterClass.HasShortRested;
				}
			}
			foreach (AbilityCardUI item2 in cardsUI)
			{
				item2.SetMode(newMode, selectableCardTypes, item2.IsLongRest | fadeUnselectableCards, highlightSelectableCards, longRestAvailable && !IsImprovedLongResting && !IsLongRestForbidden, cardSelectableFilter);
				if (flag2)
				{
					item2.SetValid(isValid: true);
				}
				else if (flag)
				{
					item2.SetValid(isValid: false, UIMultiplayerNotifications.ShowCancelReadiedCards);
				}
				else
				{
					item2.SetValid(isValid: false, UIMultiplayerNotifications.ShowSelectedOtherPlayerCard, (currentMode == CardHandMode.ActionSelection) ? new Action(UIMultiplayerNotifications.ShowSelectedOtherPlayerCard) : null);
				}
				item2.SetUnfocused(FFSNetwork.IsOnline && !playerActor.IsUnderMyControl());
				item2.SetLoseIndicatorVisibility(currentMode == CardHandMode.LoseCard && item2.IsSelectable);
			}
			UpdateLongRest();
			UpdateShortRest();
			if (newMode == CardHandMode.CardsSelection)
			{
				UpdateInitiative();
				OnSelectedCardsNumberChanged();
			}
			else
			{
				Choreographer.s_Choreographer.PlayerToSelectAbilityCardsOrLongRest = false;
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.SetMode().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00019", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void RefreshValidCards()
	{
		bool flag = !FFSNetwork.IsOnline || playerActor == null || playerActor.IsUnderMyControl();
		if (((currentMode == CardHandMode.CardsSelection) & flag) && PhaseManager.PhaseType == CPhase.PhaseType.ShortRest)
		{
			flag = playerActor == GameState.s_ShortRestingPlayer && playerActor.CharacterClass.HasShortRested;
		}
		bool flag2 = flag;
		if (flag2 && FFSNetwork.IsOnline)
		{
			flag2 = !Singleton<UIReadyToggle>.Instance.PlayersReady.Contains(PlayerRegistry.MyPlayer) && !Singleton<UIReadyToggle>.Instance.IsProgressingBar;
		}
		foreach (AbilityCardUI item in cardsUI)
		{
			if (flag2)
			{
				item.SetValid(isValid: true);
			}
			else if (flag)
			{
				item.SetValid(isValid: false, UIMultiplayerNotifications.ShowCancelReadiedCards);
			}
			else
			{
				item.SetValid(isValid: false, UIMultiplayerNotifications.ShowSelectedOtherPlayerCard, (currentMode == CardHandMode.ActionSelection) ? new Action(UIMultiplayerNotifications.ShowSelectedOtherPlayerCard) : null);
			}
		}
		if (shortRest.gameObject.activeInHierarchy)
		{
			if (flag2)
			{
				shortRest.SetValid(isValid: true);
			}
			else if (playerActor.IsUnderMyControl())
			{
				shortRest.SetValid(isValid: false, UIMultiplayerNotifications.ShowCancelReadiedCards);
			}
			else
			{
				shortRest.SetValid(isValid: false, UIMultiplayerNotifications.ShowSelectedOtherPlayerCard);
			}
		}
		UpdateCharacterSpecificMechanic();
	}

	public void SpawnCardUIFromCheat(CAbilityCard abilityCard)
	{
		AbilityCardUI abilityCardUI = cardsUI.Find((AbilityCardUI x) => x.CardID == abilityCard.ID);
		if (abilityCardUI == null)
		{
			abilityCardUI = ObjectPool.SpawnCard(abilityCard.ID, ObjectPool.ECardType.Ability, abilityCardsHolder, resetLocalScale: true).GetComponent<AbilityCardUI>();
			if (m_ExpandMiniCardInParent)
			{
				UIRectTransformUtils.StretchToParent(abilityCardUI.miniAbilityCard.transform as RectTransform);
			}
			cardsUI.Add(abilityCardUI);
		}
		abilityCardUI.Init(abilityCard, playerActor, CardPileType.Hand, OnCardSelected, OnCardDeselected, null, OnCardHover, OnCardAction, OnActionCompleted, OnCancelActiveAbility);
		abilityCardUI.transform.localPosition = Vector3.zero;
		cardsStateBeforePreview[abilityCardUI] = new CardState(abilityCardUI);
	}

	public void SpawnGivenCardUI(List<CAbilityCard> newCards)
	{
		foreach (CAbilityCard newCard in newCards)
		{
			if (GetCardUI(newCard) != null)
			{
				continue;
			}
			if (newCard.GetAbilityCardYML == null)
			{
				Log.GUIError("Ability card data is null! Check YML file for " + newCard.Name);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00020", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu);
				continue;
			}
			try
			{
				Log.NetworkTrace("Spawning card " + newCard.Name + " (ID: " + newCard.ID + " CardInstanceID: " + newCard.CardInstanceID + ") for the actor " + playerActor.CharacterClass.ID + ", ActorID: " + playerActor.ID);
				AbilityCardUI orSpawnCard = GetOrSpawnCard(newCard);
				orSpawnCard.Init(newCard, playerActor, CardPileType.Hand, OnCardSelected, OnCardDeselected, null, OnCardHover, OnCardAction, OnActionCompleted, OnCancelActiveAbility);
				orSpawnCard.transform.localPosition = Vector3.zero;
				cardsStateBeforePreview[orSpawnCard] = new CardState(orSpawnCard);
			}
			catch (Exception ex)
			{
				Log.GUIException(ex, "Unable to initialise ability card " + newCard.Name);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00021", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
			}
		}
	}

	public void DestroyCardUI(CAbilityCard abilityCard)
	{
		AbilityCardUI abilityCardUI = cardsUI.SingleOrDefault((AbilityCardUI x) => x.AbilityCard == abilityCard);
		if (abilityCardUI != null)
		{
			cardsUI.Remove(abilityCardUI);
			cardsStateBeforePreview.Remove(abilityCardUI);
			ObjectPool.RecycleCard(abilityCardUI.CardID, ObjectPool.ECardType.Ability, abilityCardUI.gameObject);
		}
		else
		{
			Log.GUIError("Unable to find attached card UI to destroy for ability card " + abilityCard.Name);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00048", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu);
		}
	}

	private AbilityCardUI GetOrSpawnCard(CAbilityCard abilityCard)
	{
		AbilityCardUI cardUI = GetCardUI(abilityCard);
		if (cardUI != null)
		{
			return cardUI;
		}
		return SpawnCard(abilityCard);
	}

	private AbilityCardUI SpawnCard(CAbilityCard abilityCard)
	{
		AbilityCardUI component = ObjectPool.SpawnCard(abilityCard.ID, ObjectPool.ECardType.Ability, abilityCardsHolder, resetLocalScale: true).GetComponent<AbilityCardUI>();
		if (m_ExpandMiniCardInParent)
		{
			UIRectTransformUtils.StretchToParent(component.miniAbilityCard.transform as RectTransform);
		}
		cardsUI.Add(component);
		return component;
	}

	public void SetCards(List<CAbilityCard> abilityCards, List<CAbilityCard> unselectedAbilityCards)
	{
		selectedCardsUI.Clear();
		foreach (CAbilityCard abilityCard in abilityCards)
		{
			initCard(abilityCard);
		}
		foreach (CAbilityCard unselectedAbilityCard in unselectedAbilityCards)
		{
			initCard(unselectedAbilityCard);
		}
		AbilityCardUI abilityCardUI = cardsUI.FirstOrDefault((AbilityCardUI x) => x.IsLongRest);
		if (abilityCardUI != null)
		{
			if (playerActor.CanUseLongRest)
			{
				abilityCardUI.Init(null, playerActor, CardPileType.Hand, OnCardSelected, OnCardDeselected, null, OnCardHover, OnCardAction, OnActionCompleted, null, isLongRest: true);
			}
			else
			{
				abilityCardUI.gameObject.SetActive(value: false);
			}
		}
		SortCards();
		void initCard(CAbilityCard abilityCard)
		{
			try
			{
				CardPileType cardPileType = CrossReference.ECardPileToCardPileType(abilityCard.CurrentCardPile);
				AbilityCardUI orSpawnCard = GetOrSpawnCard(abilityCard);
				if (!orSpawnCard.IsLongRest || playerActor.CanUseLongRest)
				{
					orSpawnCard.Init(abilityCard, playerActor, cardPileType, OnCardSelected, OnCardDeselected, null, OnCardHover, OnCardAction, OnActionCompleted, OnCancelActiveAbility);
					if (cardPileType == CardPileType.Hand)
					{
						selectedCardsUI.Add(orSpawnCard);
					}
				}
			}
			catch (Exception ex)
			{
				Log.GUIException(ex, "Unable to initialise ability card " + abilityCard.Name);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00020", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
			}
		}
	}

	private void SpawnCards(List<CAbilityCard> abilityCards, List<CAbilityCard> unselectedAbilityCards, GameObject longRestPrefab)
	{
		cardsSpawned = true;
		cardsUI.Clear();
		foreach (CAbilityCard abilityCard in abilityCards)
		{
			if (abilityCard.GetAbilityCardYML == null)
			{
				Log.GUIError("Ability card data is null! Check YML file for " + abilityCard.Name);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00020", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu);
				continue;
			}
			try
			{
				Log.NetworkTrace("Spawning card " + abilityCard.Name + " (ID: " + abilityCard.ID + " CardInstanceID: " + abilityCard.CardInstanceID + ") for the actor " + playerActor.CharacterClass.ID + ", ActorID: " + playerActor.ID);
				AbilityCardUI abilityCardUI = SpawnCard(abilityCard);
				abilityCardUI.Init(abilityCard, playerActor, CardPileType.Hand, OnCardSelected, OnCardDeselected, null, OnCardHover, OnCardAction, OnActionCompleted, OnCancelActiveAbility);
				abilityCardUI.transform.localPosition = Vector3.zero;
			}
			catch (Exception ex)
			{
				Log.GUIException(ex, "Unable to initialise ability card " + abilityCard.Name);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00021", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
			}
		}
		foreach (CAbilityCard unselectedAbilityCard in unselectedAbilityCards)
		{
			if (unselectedAbilityCard.GetAbilityCardYML == null)
			{
				Log.GUIError("Ability card data is null! Check YML file for " + unselectedAbilityCard.Name);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00020", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu);
				continue;
			}
			try
			{
				AbilityCardUI component = ObjectPool.SpawnCard(unselectedAbilityCard.ID, ObjectPool.ECardType.Ability, abilityCardsHolder, resetLocalScale: true).GetComponent<AbilityCardUI>();
				component.Init(unselectedAbilityCard, playerActor, CardPileType.Unselected, OnCardSelected, OnCardDeselected, null, OnCardHover, OnCardAction, OnActionCompleted, OnCancelActiveAbility);
				component.transform.localPosition = Vector3.zero;
				if (m_ExpandMiniCardInParent)
				{
					UIRectTransformUtils.StretchToParent(component.miniAbilityCard.transform as RectTransform);
				}
				cardsUI.Add(component);
			}
			catch (Exception ex2)
			{
				Log.GUIException(ex2, "Unable to initialise ability card " + unselectedAbilityCard.Name);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00021", "GUI_ERROR_MAIN_MENU_BUTTON", ex2.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex2.Message);
			}
		}
		if (CharacterClassManager.LongRestLayout != null)
		{
			try
			{
				if (playerActor.CanUseLongRest)
				{
					AbilityCardUI component2 = ObjectPool.SpawnCard(-1, ObjectPool.ECardType.Ability, abilityCardsHolder).GetComponent<AbilityCardUI>();
					component2.transform.localScale = Vector3.one;
					component2.Init(null, playerActor, CardPileType.Hand, OnCardSelected, OnCardDeselected, null, OnCardHover, OnCardAction, OnActionCompleted, null, isLongRest: true);
					cardsUI.Add(component2);
				}
			}
			catch (Exception ex3)
			{
				Log.GUIException(ex3, "Unable to initialise Long Rest card\n");
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00022", "GUI_ERROR_MAIN_MENU_BUTTON", ex3.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex3.Message);
			}
		}
		shortRest = UnityEngine.Object.Instantiate(shortRestPrefab, abilityCardsHolder).GetComponent<ShortRest>();
		shortRest.Init(playerActor, StartShortRest);
		SortCards();
	}

	private void OnCancelActiveAbility(AbilityCardUI abilityCardUi)
	{
		try
		{
			if (playerActor.CanCancelActiveBonuses)
			{
				Singleton<CardsHandManager>.Instance.DiscardActiveAbilityCard(playerActor, abilityCardUi.AbilityCard, null);
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.OnCancelActiveAbility().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00023", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void OnActionCompleted()
	{
		try
		{
			int selectedRoundCardsCount = playerActor.SelectedRoundCardsCount;
			if ((selectedRoundCardsCount == 2 && GameState.GetCurrentActionSequenceType(selectedRoundCardsCount) == GameState.EActionSequenceType.SecondAction) || (selectedRoundCardsCount > 2 && GameState.GetCurrentActionSequenceType(selectedRoundCardsCount) == GameState.EActionSequenceType.ThirdAction))
			{
				selectedCardsUI.Clear();
			}
			cardsActionController.OnActionFinished();
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.OnActionCompleted().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00024", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void OnCardAction(FullAbilityCard cardActionUI, CBaseCard.ActionType actionType)
	{
		try
		{
			cardsActionController.OnCardClicked(cardActionUI, actionType);
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.OnCardAction().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00025", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void Undo()
	{
		try
		{
			itemsUI.RefreshItemsState();
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.Undo().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00026", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void OnCardHover(bool isActive, AbilityCardUI cardUI)
	{
		try
		{
			InitiativeTrack.Instance?.OnCardHover(isActive, cardUI, playerActor);
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.OnCardHover().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00027", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void OnCardSelected(AbilityCardUI cardUI, bool networkAction = true)
	{
		try
		{
			Singleton<UIReadyToggle>.Instance?.RequestChangeCards(changeCards: true);
			if (currentMode == CardHandMode.ActionSelection || currentMode == CardHandMode.Preview || currentMode == CardHandMode.PreviewMiniCard)
			{
				return;
			}
			if (DesyncDetector.IsCheckingHostHash())
			{
				cardUI.ToggleSelect(active: false, highlight: false, networkAction: false);
				return;
			}
			if (currentMode == CardHandMode.CardsSelection)
			{
				shortRest.Select(active: false, fromCardSelected: true);
			}
			if (currentMode == CardHandMode.CardsSelection && playerActor.SelectingCardsForExtraTurnOfType != CAbilityExtraTurn.EExtraTurnType.None && selectedCardsUI.Count == maxCardsSelected)
			{
				selectedCardsUI.Insert(0, cardUI);
				cardUI.ToggleSelect(active: false, highlight: false, networkAction: false);
				cardUI.MiniAbilityCard.ShowWarning(show: true);
				Singleton<HelpBox>.Instance.HighlightWarning();
				return;
			}
			if (cardUI.IsLongRest)
			{
				if (playerActor.CanUseLongRest)
				{
					selectedCardsUI.Insert(0, cardUI);
					for (int num = selectedCardsUI.Count - 1; num >= 1; num--)
					{
						selectedCardsUI[num].ToggleSelect(active: false, highlight: false, networkAction: false);
					}
					playerActor.CharacterClass.LongRest = true;
					UpdateInitiative();
					OnSelectedCardsNumberChanged();
					InitiativeTrack.Instance.CheckRoundAbilityCardsOrLongRestSelected();
					if (networkAction)
					{
						NetworkSelectedRoundCards();
					}
					UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.AbilityCardSelected, playerActor.GetPrefabName(), "Long Rest"));
				}
				return;
			}
			if (selectedCardsUI.Count == 1 && selectedCardsUI[0].IsLongRest)
			{
				selectedCardsUI[0].ToggleSelect(active: false, highlight: false, networkAction: false);
			}
			selectedCardsUI.Insert(0, cardUI);
			if (maxCardsSelected > 0)
			{
				for (int i = maxCardsSelected; i < selectedCardsUI.Count; i++)
				{
					selectedCardsUI[i].ToggleSelect(active: false, highlight: false, networkAction: false);
				}
			}
			else
			{
				Log.GUIError($"CardsHandUI.OnCardSelected called with maxCardSelected {maxCardsSelected}. Skipping excess cards deselection.");
			}
			if (currentMode == CardHandMode.CardsSelection)
			{
				if (GameState.IsSelectingCardForMode(out var modeAbility))
				{
					if (!playerActor.IsUnderMyControl())
					{
						UIMultiplayerNotifications.ShowSelectedOtherPlayerCard();
						return;
					}
					if (FFSNetwork.IsOnline)
					{
						Synchronizer.SendGameAction(GameActionType.CardForModeSelected, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, playerActor.ID, 0, 0, cardUI.CardInstanceID);
					}
					Choreographer.s_Choreographer.OnCardForModeSelected(modeAbility, cardUI);
				}
				else if (playerActor.SelectingCardsForExtraTurnOfType != CAbilityExtraTurn.EExtraTurnType.None)
				{
					int num2 = playerActor.OriginalInitiative();
					if (playerActor.SelectingCardsForExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater && selectedCardsUI.Count == 1 && selectedCardsUI[0].AbilityCard.Initiative <= num2)
					{
						InitiativeTrack.Instance.helpBox.ShowTranslated(string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_EXTRA_TURN_BOTH_ACTIONS_LATER"), playerActor.OriginalInitiative()), Singleton<AbilityEffectManager>.Instance.ExtraTurnAbilityEffectTitle(playerActor));
						InitiativeTrack.Instance.helpBox.HighlightWarning();
						DeselectAllCards(networkAction: false);
						return;
					}
					if (playerActor.SelectingCardsForExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater && selectedCardsUI.Count == 1)
					{
						InitiativeTrack.Instance.helpBox.ShowTranslated(LocalizationManager.GetTranslation("GUI_TOOLTIP_EXTRA_TURN_BOTH_ACTIONS_LATER_SECOND_CARD"), Singleton<AbilityEffectManager>.Instance.ExtraTurnAbilityEffectTitle(playerActor));
						List<CAbilityCard> list = selectedCardsUI.Select((AbilityCardUI it) => it.AbilityCard).ToList();
						playerActor.CharacterClass.SetExtraTurnInitiativeAbilityCard(list[0]);
						UpdateInitiative();
						OnSelectedCardsNumberChanged();
					}
					if ((playerActor.SelectingCardsForExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.SingleCardAnyActionAtTheEndOfTurn || playerActor.SelectingCardsForExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.SingleActionBeforeCardSelection || playerActor.SelectingCardsForExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.SingleActionAfterExtraTurnPerform) && selectedCardsUI.Count == 1)
					{
						List<CAbilityCard> list2 = selectedCardsUI.Select((AbilityCardUI it) => it.AbilityCard).ToList();
						playerActor.CharacterClass.SetExtraTurnInitiativeAbilityCard(list2[0]);
						UpdateInitiative();
						OnSelectedCardsNumberChanged();
					}
					if (maxCardsSelected == selectedCardsUI.Count && maxCardsSelected != 0)
					{
						Choreographer.s_Choreographer.readyButton.AlternativeAction(OnSelectExtraTurnCardsClick);
						if (!FFSNetwork.IsOnline || playerActor.IsUnderMyControl())
						{
							Choreographer.s_Choreographer.readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONENDSELECTION, null, hideOnClick: true, glowingEffect: true, interactable: true, disregardTurnControlForInteractability: true);
							Choreographer.s_Choreographer.m_SkipButton.SetInteractable(GameState.CurrentExtraTurnAbility != null);
						}
					}
					if ((FFSNetwork.IsOnline && playerActor.IsUnderMyControl()) & networkAction)
					{
						Synchronizer.SendGameAction(GameActionType.SelectExtraTurnCards, ActionPhaseType.ExtraTurnCardSelection, validateOnServerBeforeExecuting: false, disableAutoReplication: false, playerActor.ID, 0, 0, 0, supplementaryDataBoolean: false, null, new CardsToken(selectedCardsUI));
					}
				}
				else if (GameState.IsPerformingCardOutOfPhase)
				{
					if (!playerActor.CharacterClass.SelectedAbilityCardsOutOfPhase.Contains(cardUI.AbilityCard))
					{
						List<CAbilityCard> cardPileList = playerActor.CharacterClass.GetCardPileList(cardUI.AbilityCard.CurrentCardPile);
						if (!cardPileList.Contains(cardUI.AbilityCard))
						{
							Log.NetworkError("ERROR_MULTIPLAYER_00036", "Attempted to select a card that was not in the correct pile!  Player: " + playerActor.CharacterClass.ID + "  Card: " + cardUI.AbilityCard.Name);
							return;
						}
						if (!GameState.__OutOfPhaseCardFromPiles.ContainsKey(cardUI.AbilityCard))
						{
							GameState.__OutOfPhaseCardFromPiles.Add(cardUI.AbilityCard, cardUI.AbilityCard.CurrentCardPile);
						}
						ScenarioRuleClient.MoveAbilityCard(playerActor.CharacterClass, cardUI.AbilityCard, cardPileList, playerActor.CharacterClass.SelectedAbilityCardsOutOfPhase, cardUI.AbilityCard.CurrentCardPile.ToString() + "AbilityCards", "SelectedAbilityCardsOutOfPhase", networkAction);
					}
					if (!FFSNetwork.IsOnline || playerActor.IsUnderMyControl())
					{
						int val = GameState.CurrentOutOfPhaseCardSelectionAbility.GetMiscVar("MinRequiredCards").GetInt();
						bool interactable = selectedCardsUI.Count >= Math.Min(val, maxCardsSelected) && selectedCardsUI.Count <= maxCardsSelected;
						Choreographer.s_Choreographer.readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONENDSELECTION, null, hideOnClick: true, glowingEffect: true, interactable, disregardTurnControlForInteractability: true);
						Choreographer.s_Choreographer.m_SkipButton.SetInteractable(GameState.CurrentOutOfPhaseCardSelectionAbility?.CanSkip ?? false);
					}
					if ((FFSNetwork.IsOnline && playerActor.IsUnderMyControl()) & networkAction)
					{
						Synchronizer.SendGameAction(GameActionType.SelectOutOfPhaseCards, ActionPhaseType.ExtraTurnCardSelection, validateOnServerBeforeExecuting: false, disableAutoReplication: false, playerActor.ID, 0, 0, 0, supplementaryDataBoolean: false, null, new CardsToken(selectedCardsUI));
					}
				}
				else
				{
					bool flag = playerActor.CharacterClass.RoundAbilityCards.Contains(cardUI.AbilityCard);
					if (!flag)
					{
						if (!playerActor.CharacterClass.HandAbilityCards.Contains(cardUI.AbilityCard))
						{
							Log.NetworkError("ERROR_MULTIPLAYER_00036", "Attempted to select a card that was not in the players hand!  Player: " + playerActor.CharacterClass.ID + "  Card: " + cardUI.AbilityCard.Name);
							return;
						}
						ScenarioRuleClient.MoveAbilityCard(playerActor.CharacterClass, cardUI.AbilityCard, playerActor.CharacterClass.HandAbilityCards, playerActor.CharacterClass.RoundAbilityCards, "HandAbilityCards", "RoundAbilityCards", networkAction);
					}
					InitiativeTrack.Instance.CheckRoundAbilityCardsOrLongRestSelected();
					Log.GUIInfoFormat("[CARDS HAND UI]: OnCardSelected {0}. Is contained in round abilities: {1}. RoundAbilityCards {2}", cardUI.AbilityCard.Name, flag, playerActor.CharacterClass.RoundAbilityCards.Count);
					UpdateInitiative();
					OnSelectedCardsNumberChanged();
				}
			}
			else if (currentMode == CardHandMode.LoseCard || currentMode == CardHandMode.DiscardCard || currentMode == CardHandMode.ExcludeCardsFromShortRest)
			{
				if (maxCardsSelected == selectedCardsUI.Count && maxCardsSelected != 0)
				{
					string text = "";
					if (currentMode == CardHandMode.LoseCard)
					{
						text = ((maxCardsSelected == 1) ? string.Format(LocalizationManager.GetTranslation("GUI_LOSE_CARD"), cardUI.fullAbilityCard.Title) : LocalizationManager.GetTranslation("GUI_LOSE_CARDS"));
					}
					else if (currentMode == CardHandMode.DiscardCard)
					{
						text = ((maxCardsSelected == 1) ? string.Format(LocalizationManager.GetTranslation("GUI_DISCARD_CARD"), cardUI.fullAbilityCard.Title) : LocalizationManager.GetTranslation("GUI_DISCARD_CARDS"));
					}
					else if (currentMode == CardHandMode.ExcludeCardsFromShortRest)
					{
						text = ((maxCardsSelected == 1) ? string.Format(LocalizationManager.GetTranslation("GUI_SELECT_CARD"), cardUI.fullAbilityCard.Title) : LocalizationManager.GetTranslation("GUI_SELECT_CARDS"));
					}
					foreach (AbilityCardUI item in selectedCardsUI)
					{
						item.OnPointerExit();
					}
					List<FullAbilityCard> fullCards = selectedCardsUI.Select((AbilityCardUI it) => it.fullAbilityCard).ToList();
					Dictionary<AbilityCardUI, bool> wasSelectable = new Dictionary<AbilityCardUI, bool>();
					foreach (AbilityCardUI item2 in cardsUI)
					{
						wasSelectable[item2] = item2.IsSelectable;
						item2.SetSelectable(isOn: false, !selectedCardsUI.Contains(item2));
					}
					UIManager.Instance.dialogPopup.Show(fullCards.Select((FullAbilityCard it) => it.gameObject).ToList(), new DialogOption[2]
					{
						new DialogOption(text, InputFHManager.Controls.Generic.Confirm, () =>
						{
							foreach (AbilityCardUI item3 in cardsUI)
							{
								item3.SetSelectable(wasSelectable[item3]);
							}
							OnLoseCardClick();
						}, () =>
						{
							fullCards.ForEach((FullAbilityCard it) =>
							{
								it.ToggleCardEffect(active: false, CardEffects.FXTask.BurnCard);
							});
						}, () =>
						{
							fullCards.ForEach((FullAbilityCard it) =>
							{
								it.RefreshPile();
							});
						}, enabled: true, visible: true, CLevelUIInteractionProfile.EIsolatedControlType.DialogOptionButton, enabledForPlayer: PlayerActor, visualParams: m_BurnOptionButtonVisuals),
						new DialogOption(LocalizationManager.GetTranslation("GUI_CHOOSE_OTHER_CARD"), InputFHManager.Controls.Generic.Back, () =>
						{
							foreach (AbilityCardUI item4 in cardsUI)
							{
								item4.SetSelectable(wasSelectable[item4]);
							}
							DeselectAllCards();
							Singleton<TakeDamagePanel>.Instance.ToggleVisibility(visible: true);
							Singleton<LoseCardsOptionsPanel>.Instance.ToggleVisibility(visible: true);
							CAbility cAbility = PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability;
							if (cAbility != null && (cAbility.AbilityType == CAbility.EAbilityType.DiscardCards || cAbility.AbilityType == CAbility.EAbilityType.LoseCards))
							{
								Choreographer.s_Choreographer.Handle_SelectLoseCards(new CSelectLoseCards_MessageData(null, cAbility.TargetingActor)
								{
									m_ActorLosingCards = PlayerActor,
									m_Ability = cAbility,
									m_SkipAnim = true
								});
							}
						}, enabled: true, visible: true, CLevelUIInteractionProfile.EIsolatedControlType.DialogOptionButton, enabledForPlayer: PlayerActor, visualParams: m_RedrawOptionButtonVisuals)
					}, 1);
					UIManager.Instance.dialogPopup.transform.SetParent(Singleton<CardsHandManager>.Instance.transform);
					UIManager.Instance.dialogPopup.transform.SetSiblingIndex(1);
					Singleton<TakeDamagePanel>.Instance.ToggleVisibility(visible: false);
					Singleton<LoseCardsOptionsPanel>.Instance.ToggleVisibility(visible: false);
					Choreographer.s_Choreographer.m_SkipButton.Toggle(active: false);
				}
			}
			else if (currentMode == CardHandMode.SelectCardsWithActiveBonusTracker)
			{
				Action action = () =>
				{
					string text2 = ((!(PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability is CAbilityMoveActiveBonusTracker { Strength: >0 })) ? ((maxCardsSelected == 1) ? LocalizationManager.GetTranslation("FH/GUI_MOVE_ACTIVE_BONUS_TRACKER") : LocalizationManager.GetTranslation("FH/GUI_MOVE_ACTIVE_BONUS_TRACKERS")) : ((maxCardsSelected == 1) ? LocalizationManager.GetTranslation("FH/GUI_LOSE_ACTIVE_BONUS_TRACKER") : LocalizationManager.GetTranslation("FH/GUI_LOSE_ACTIVE_BONUS_TRACKERS")));
					string text3 = ((maxCardsSelected == 1) ? LocalizationManager.GetTranslation("GUI_CHOOSE_OTHER_CARD") : LocalizationManager.GetTranslation("FH/GUI_CHOOSE_OTHER_CARDS"));
					foreach (AbilityCardUI item5 in selectedCardsUI)
					{
						item5.OnPointerExit();
					}
					List<FullAbilityCard> fullCards2 = selectedCardsUI.Select((AbilityCardUI it) => it.fullAbilityCard).ToList();
					Dictionary<AbilityCardUI, bool> wasSelectable2 = new Dictionary<AbilityCardUI, bool>();
					foreach (AbilityCardUI item6 in cardsUI)
					{
						wasSelectable2[item6] = item6.IsSelectable;
						item6.SetSelectable(isOn: false, !selectedCardsUI.Contains(item6));
					}
					DialogPopup dialogPopup = UIManager.Instance.dialogPopup;
					List<GameObject> content = fullCards2.Select((FullAbilityCard it) => it.gameObject).ToList();
					DialogOption[] array = new DialogOption[2];
					InputAction confirm = InputFHManager.Controls.Generic.Confirm;
					UnityAction onClick = () =>
					{
						foreach (AbilityCardUI item7 in cardsUI)
						{
							item7.SetSelectable(wasSelectable2[item7]);
						}
						OnMoveTrackerOfActiveBonusCardClick();
					};
					UnityAction onEnter = () =>
					{
					};
					UnityAction onExit = () =>
					{
						fullCards2.ForEach((FullAbilityCard it) =>
						{
							it.RefreshPile();
						});
					};
					CPlayerActor enabledForPlayer = PlayerActor;
					array[0] = new DialogOption(text2, confirm, onClick, onEnter, onExit, enabled: true, visible: true, null, null, enabledForPlayer);
					InputAction back = InputFHManager.Controls.Generic.Back;
					UnityAction action3 = () =>
					{
						foreach (AbilityCardUI item8 in cardsUI)
						{
							item8.SetSelectable(wasSelectable2[item8]);
						}
						DeselectAllCards();
						Singleton<TakeDamagePanel>.Instance.ToggleVisibility(visible: true);
						Singleton<LoseCardsOptionsPanel>.Instance.ToggleVisibility(visible: true);
						if (PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability is CAbilityMoveActiveBonusTracker cAbilityMoveActiveBonusTracker2)
						{
							Choreographer.s_Choreographer.m_UndoButton.Toggle(cAbilityMoveActiveBonusTracker2.CanUndo && Choreographer.s_Choreographer.FirstAbility);
							Choreographer.s_Choreographer.m_SkipButton.Toggle(cAbilityMoveActiveBonusTracker2.CanSkip);
						}
					};
					enabledForPlayer = PlayerActor;
					array[1] = new DialogOption(text3, back, action3, enabled: true, visible: true, null, null, enabledForPlayer);
					dialogPopup.Show(content, array, 1);
					UIManager.Instance.dialogPopup.transform.SetParent(Singleton<CardsHandManager>.Instance.transform);
					UIManager.Instance.dialogPopup.transform.SetAsFirstSibling();
					Singleton<TakeDamagePanel>.Instance.ToggleVisibility(visible: false);
					Singleton<LoseCardsOptionsPanel>.Instance.ToggleVisibility(visible: false);
					Choreographer.s_Choreographer.m_UndoButton.Toggle(active: false);
					Choreographer.s_Choreographer.m_SkipButton.Toggle(active: false);
				};
				Choreographer.s_Choreographer.ClearGenericButton();
				if (selectedCardsUI.Count >= 1 && selectedCardsUI.Count < maxCardsSelected)
				{
					Choreographer.s_Choreographer.SetupGenericButton(playerActor.IsUnderMyControl(), LocalizationManager.GetTranslation("GUI_END_SELECTION"), action);
				}
				if (maxCardsSelected == selectedCardsUI.Count && maxCardsSelected != 0)
				{
					action();
				}
			}
			else if (currentMode == CardHandMode.SelectCardsWithTideActiveBonusesToKeep)
			{
				Choreographer.s_Choreographer.readyButton.Toggle(active: false, ReadyButton.EButtonState.EREADYBUTTONNA, null, hideOnClick: true, glowingEffect: false, interactable: true, disregardTurnControlForInteractability: false, haltActionProcessorIfDeactivated: false);
				Action action2 = () =>
				{
					string text2 = ((maxCardsSelected == 1) ? LocalizationManager.GetTranslation("FH/GUI_KEEP_TIDE_ACTIVE_BONUS_CARD") : LocalizationManager.GetTranslation("FH/GUI_KEEP_TIDE_ACTIVE_BONUS_CARDS"));
					string text3 = ((maxCardsSelected == 1) ? LocalizationManager.GetTranslation("GUI_CHOOSE_OTHER_CARD") : LocalizationManager.GetTranslation("FH/GUI_CHOOSE_OTHER_CARDS"));
					foreach (AbilityCardUI item9 in selectedCardsUI)
					{
						item9.OnPointerExit();
					}
					List<FullAbilityCard> fullCards2 = selectedCardsUI.Select((AbilityCardUI it) => it.fullAbilityCard).ToList();
					Dictionary<AbilityCardUI, bool> wasSelectable2 = new Dictionary<AbilityCardUI, bool>();
					foreach (AbilityCardUI item10 in cardsUI)
					{
						wasSelectable2[item10] = item10.IsSelectable;
						item10.SetSelectable(isOn: false, !selectedCardsUI.Contains(item10));
					}
					Choreographer.s_Choreographer.readyButton.ClearAlternativeAction();
					DialogPopup dialogPopup = UIManager.Instance.dialogPopup;
					List<GameObject> content = fullCards2.Select((FullAbilityCard it) => it.gameObject).ToList();
					DialogOption[] array = new DialogOption[2];
					InputAction confirm = InputFHManager.Controls.Generic.Confirm;
					UnityAction onClick = () =>
					{
						foreach (AbilityCardUI item11 in cardsUI)
						{
							item11.SetSelectable(wasSelectable2[item11]);
						}
						Singleton<HelpBox>.Instance.Hide();
						List<int> list3 = selectedCardsUI.Select((AbilityCardUI x) => x.AbilityCard.ID).ToList();
						if (playerActor.CharacterClass.LongRest)
						{
							Choreographer.s_Choreographer.PerformNormalLongRest(playerActor, list3);
							if (FFSNetwork.IsOnline && playerActor.IsUnderMyControl())
							{
								Synchronizer.SendGameAction(GameActionType.ContinueCrashingTideLongRest, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, playerActor.ID, 0, 0, 0, supplementaryDataBoolean: false, null, new CardsToken(list3));
							}
							if (FFSNetwork.IsOnline)
							{
								if (!playerActor.IsUnderMyControl())
								{
									ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.LongRest);
								}
								else
								{
									ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.LongRest);
								}
							}
						}
						else
						{
							if (!IsFreeShortRest)
							{
								CardsHandManager.ShowCardsSelection(playerActor);
							}
							PerformShortRest(playerActor, allowSpecificShortRest: false, list3);
							if (FFSNetwork.IsOnline && playerActor.IsUnderMyControl())
							{
								Synchronizer.SendGameAction(GameActionType.ContinueCrashingTideLongRest, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, playerActor.ID, 0, 0, 0, supplementaryDataBoolean: false, null, new CardsToken(list3));
							}
						}
					};
					UnityAction onEnter = () =>
					{
					};
					UnityAction onExit = () =>
					{
						fullCards2.ForEach((FullAbilityCard it) =>
						{
							it.RefreshPile();
						});
					};
					CPlayerActor enabledForPlayer = PlayerActor;
					array[0] = new DialogOption(text2, confirm, onClick, onEnter, onExit, enabled: true, visible: true, null, null, enabledForPlayer);
					InputAction back = InputFHManager.Controls.Generic.Back;
					UnityAction action3 = () =>
					{
						foreach (AbilityCardUI item12 in cardsUI)
						{
							item12.SetSelectable(wasSelectable2[item12]);
						}
						DeselectAllCards();
						if (playerActor.CharacterClass.LongRest)
						{
							Choreographer.s_Choreographer.ContinueLongRest(playerActor);
						}
						else
						{
							PerformShortRest(playerActor);
						}
					};
					enabledForPlayer = PlayerActor;
					array[1] = new DialogOption(text3, back, action3, enabled: true, visible: true, null, null, enabledForPlayer);
					dialogPopup.Show(content, array, 1);
					UIManager.Instance.dialogPopup.transform.SetParent(Singleton<CardsHandManager>.Instance.transform);
					UIManager.Instance.dialogPopup.transform.SetAsFirstSibling();
					Singleton<TakeDamagePanel>.Instance.ToggleVisibility(visible: false);
					Singleton<LoseCardsOptionsPanel>.Instance.ToggleVisibility(visible: false);
					Choreographer.s_Choreographer.m_SkipButton.Toggle(active: false);
				};
				if (maxCardsSelected == selectedCardsUI.Count && maxCardsSelected != 0)
				{
					action2();
				}
			}
			else if (currentMode == CardHandMode.SelectCardsWithMandatoryActiveBonusesToDeactivate)
			{
				foreach (AbilityCardUI item13 in selectedCardsUI)
				{
					item13.OnPointerExit();
				}
				Singleton<HelpBox>.Instance.Hide();
				DeselectAllCards(networkAction: false);
				Choreographer.s_Choreographer.readyButton.ClearAlternativeAction();
				CAbilityCard card = cardUI.AbilityCard;
				LocalizationManager.TryGetTranslation("GUI_CONFIRMATION_CANCEL_" + card.Name + "_DESCR", out var translation);
				Singleton<UIConfirmationBoxManager>.Instance.ShowCancelActiveAbility(LocalizationManager.GetTranslation(card.Name), card.SelectedAction.CardPile != CBaseCard.ECardPile.Discarded, () =>
				{
					Singleton<CardsHandManager>.Instance.CancelActiveAbility(playerActor, card);
					Singleton<CardsHandManager>.Instance.NetworkCancelingActiveBonuses(playerActor, card);
					if (playerActor.CharacterClass.LongRest)
					{
						if (Choreographer.s_Choreographer.ContinueLongRest(PlayerActor))
						{
							if (FFSNetwork.IsOnline && playerActor.IsUnderMyControl())
							{
								Synchronizer.SendGameAction(GameActionType.ContinueCrashingTideLongRest, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, playerActor.ID, 0, 0, 0, supplementaryDataBoolean: false, null, new CardsToken(selectedCardsUI));
							}
							if (FFSNetwork.IsOnline)
							{
								if (!playerActor.IsUnderMyControl())
								{
									ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.LongRest);
								}
								else
								{
									ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.LongRest);
								}
							}
						}
					}
					else
					{
						if (!IsFreeShortRest)
						{
							CardsHandManager.ShowCardsSelection(playerActor);
						}
						PerformShortRest(playerActor);
						if (FFSNetwork.IsOnline && playerActor.IsUnderMyControl() && !playerActor.CharacterClass.ActivatedAbilityCards.Any((CAbilityCard cAbilityCard) => cAbilityCard.HasMandatoryBonus()))
						{
							Synchronizer.SendGameAction(GameActionType.ConfirmAction, ActionProcessor.CurrentPhase);
						}
					}
				}, translation, () =>
				{
					if (playerActor.CharacterClass.LongRest)
					{
						Choreographer.s_Choreographer.ContinueLongRest(playerActor);
					}
					else
					{
						PerformShortRest(playerActor);
					}
				});
			}
			else if ((currentMode == CardHandMode.RecoverDiscardedCard || currentMode == CardHandMode.RecoverLostCard || currentMode == CardHandMode.IncreaseCardLimit) && ((currentMode == CardHandMode.IncreaseCardLimit && maxCardsSelected == selectedCardsUI.Count) || maxCardsSelected >= selectedCardsUI.Count) && maxCardsSelected != 0)
			{
				if (currentMode == CardHandMode.IncreaseCardLimit)
				{
					Choreographer.s_Choreographer.readyButton.AlternativeAction(OnInceaseLimitCardClick);
				}
				else
				{
					Choreographer.s_Choreographer.readyButton.AlternativeAction(OnRecoverCardClick);
				}
				Choreographer.s_Choreographer.readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONRECOVERCARD, LocalizationManager.GetTranslation("GUI_CONFIRM"), hideOnClick: true, glowingEffect: true, interactable: true, disregardTurnControlForInteractability: true);
				CAbility currentAbility = Choreographer.s_Choreographer.m_CurrentAbility;
				if (currentAbility.Flags.HasFlag(CAbility.EAbilityFlag.FrozenFistMechanic))
				{
					Choreographer.s_Choreographer.m_SkipButton.Toggle(active: true, null, hideOnClick: true, currentAbility.CanSkip);
					ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(Choreographer.s_Choreographer.FindClientActorGameObject(playerActor));
					int num3 = 2 + cardUI.AbilityCard.Level / 2;
					if (playerActor.Tokens.HasKey(CCondition.ENegativeCondition.Brittle))
					{
						num3 *= 2;
					}
					if (playerActor.Tokens.HasKey(CCondition.EPositiveCondition.Ward))
					{
						num3 /= 2;
					}
					actorBehaviour.m_WorldspacePanelUI.PreviewDamage(num3);
				}
				else
				{
					Choreographer.s_Choreographer.m_SkipButton.Toggle(active: false);
				}
			}
			UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.AbilityCardSelected, playerActor.GetPrefabName(), cardUI.AbilityCard.Name));
			OnSelectedCardsNumberChanged();
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.OnCardSelected().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00028", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void SetAllCardsSelectable(bool selectable)
	{
		foreach (AbilityCardUI item in cardsUI)
		{
			item.SetSelectable(selectable);
		}
	}

	private void CacheSelectedCards()
	{
		cachedSelectedCardsUI.Clear();
		cachedSelectedCardsUI.AddRange(selectedCardsUI);
	}

	public void DeselectAllCards(bool networkAction = true)
	{
		for (int num = selectedCardsUI.Count - 1; num >= 0; num--)
		{
			selectedCardsUI[num].ToggleSelect(active: false, highlight: false, networkAction);
		}
		selectedCardsUI.Clear();
	}

	public void OnCardDeselected(AbilityCardUI cardUI, bool networkAction = true)
	{
		try
		{
			Singleton<UIReadyToggle>.Instance?.RequestChangeCards(changeCards: true);
			if (currentMode == CardHandMode.ActionSelection || currentMode == CardHandMode.Preview || currentMode == CardHandMode.PreviewMiniCard)
			{
				return;
			}
			selectedCardsUI.Remove(cardUI);
			if (cardUI.IsLongRest && PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
			{
				playerActor.CharacterClass.LongRest = false;
			}
			if (currentMode == CardHandMode.CardsSelection)
			{
				if (GameState.IsSelectingCardForMode(out var modeAbility))
				{
					if (!playerActor.IsUnderMyControl())
					{
						UIMultiplayerNotifications.ShowSelectedOtherPlayerCard();
						return;
					}
					if (FFSNetwork.IsOnline)
					{
						Synchronizer.SendGameAction(GameActionType.CardForModeSelected, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, playerActor.ID, 0, 0, cardUI.CardInstanceID);
					}
					Choreographer.s_Choreographer.OnCardForModeSelected(modeAbility, cardUI);
				}
				else if (playerActor.SelectingCardsForExtraTurnOfType != CAbilityExtraTurn.EExtraTurnType.None)
				{
					if (playerActor.SelectingCardsForExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.SingleCardAnyActionAtTheEndOfTurn && cardUI.AbilityCard.Initiative == playerActor.Initiative())
					{
						playerActor.CharacterClass.SetExtraTurnInitiativeAbilityCard(null);
						if (!FFSNetwork.IsOnline || playerActor.IsUnderMyControl())
						{
							DeselectAllCards(networkAction);
						}
						UpdateInitiative();
						OnSelectedCardsNumberChanged();
						InitiativeTrack.Instance.CheckRoundAbilityCardsOrLongRestSelected();
					}
					if (playerActor.SelectingCardsForExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater && cardUI.AbilityCard.Initiative == playerActor.Initiative())
					{
						playerActor.CharacterClass.SetExtraTurnInitiativeAbilityCard(null);
						int num = playerActor.OriginalInitiative();
						if (selectedCardsUI.Count > 0 && selectedCardsUI[selectedCardsUI.Count - 1].AbilityCard.Initiative > num)
						{
							playerActor.CharacterClass.SetExtraTurnInitiativeAbilityCard(selectedCardsUI[selectedCardsUI.Count - 1].AbilityCard);
						}
						else
						{
							InitiativeTrack.Instance.helpBox.ShowTranslated(string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_EXTRA_TURN_BOTH_ACTIONS_LATER"), playerActor.Initiative()), Singleton<AbilityEffectManager>.Instance.ExtraTurnAbilityEffectTitle(playerActor));
							if (selectedCardsUI.Count > 0)
							{
								InitiativeTrack.Instance.helpBox.HighlightWarning();
							}
							if (!FFSNetwork.IsOnline || playerActor.IsUnderMyControl())
							{
								DeselectAllCards(networkAction);
							}
						}
						UpdateInitiative();
						OnSelectedCardsNumberChanged();
						InitiativeTrack.Instance.CheckRoundAbilityCardsOrLongRestSelected();
					}
					if (selectedCardsUI.Count != maxCardsSelected)
					{
						Choreographer.s_Choreographer.readyButton.SetInteractable(interactable: false);
					}
					if ((FFSNetwork.IsOnline && playerActor.IsUnderMyControl()) & networkAction)
					{
						Synchronizer.SendGameAction(GameActionType.SelectExtraTurnCards, ActionPhaseType.ExtraTurnCardSelection, validateOnServerBeforeExecuting: false, disableAutoReplication: false, playerActor.ID, 0, 0, 0, supplementaryDataBoolean: false, null, new CardsToken(selectedCardsUI));
					}
				}
				else if (GameState.IsPerformingCardOutOfPhase)
				{
					if (!cardUI.IsLongRest)
					{
						if (!GameState.__OutOfPhaseCardFromPiles.TryGetValue(cardUI.AbilityCard, out var value))
						{
							Log.NetworkError("ERROR_MULTIPLAYER_00036", "Attempted to select a card for an invalid ability!  Player: " + playerActor.CharacterClass.ID + "  Card: " + cardUI.AbilityCard.Name);
							return;
						}
						List<CAbilityCard> cardPileList = playerActor.CharacterClass.GetCardPileList(value);
						Log.NetworkTrace("Moving ability card " + cardUI.CardName + $" from SelectedAbilityCardsOutOfPhase to {value}.");
						GameState.__OutOfPhaseCardFromPiles.Remove(cardUI.AbilityCard);
						ScenarioRuleClient.MoveAbilityCard(playerActor.CharacterClass, cardUI.AbilityCard, playerActor.CharacterClass.SelectedAbilityCardsOutOfPhase, cardPileList, "SelectedAbilityCardsOutOfPhase", value.ToString() + "AbilityCards", networkAction);
						Log.NetworkInfo("[CARDS HAND UI]: OnCardDeselected: Ability card moved.");
					}
					int val = GameState.CurrentOutOfPhaseCardSelectionAbility.GetMiscVar("MinRequiredCards").GetInt();
					if (selectedCardsUI.Count < Math.Min(val, maxCardsSelected) || selectedCardsUI.Count > maxCardsSelected)
					{
						Choreographer.s_Choreographer.readyButton.SetInteractable(interactable: false);
					}
					if ((FFSNetwork.IsOnline && playerActor.IsUnderMyControl()) & networkAction)
					{
						Synchronizer.SendGameAction(GameActionType.SelectOutOfPhaseCards, ActionPhaseType.ExtraTurnCardSelection, validateOnServerBeforeExecuting: false, disableAutoReplication: false, playerActor.ID, 0, 0, 0, supplementaryDataBoolean: false, null, new CardsToken(selectedCardsUI));
					}
				}
				else
				{
					if (!cardUI.IsLongRest)
					{
						Log.NetworkTrace("Moving ability card " + cardUI.CardName + " from RoundCards to Hand.");
						ScenarioRuleClient.MoveAbilityCard(playerActor.CharacterClass, cardUI.AbilityCard, playerActor.CharacterClass.RoundAbilityCards, playerActor.CharacterClass.HandAbilityCards, "RoundAbilityCards", "HandAbilityCards", networkAction);
						Log.NetworkInfo("[CARDS HAND UI]: OnCardDeselected: Ability card moved.");
					}
					else if (networkAction)
					{
						NetworkSelectedRoundCards();
					}
					UpdateInitiative();
					OnSelectedCardsNumberChanged();
					InitiativeTrack.Instance.CheckRoundAbilityCardsOrLongRestSelected();
				}
			}
			else if (currentMode == CardHandMode.LoseCard || currentMode == CardHandMode.DiscardCard || currentMode == CardHandMode.RecoverDiscardedCard || currentMode == CardHandMode.RecoverLostCard || currentMode == CardHandMode.IncreaseCardLimit || currentMode == CardHandMode.SelectCardsWithActiveBonusTracker || currentMode == CardHandMode.SelectCardsWithTideActiveBonusesToKeep || currentMode == CardHandMode.SelectCardsWithMandatoryActiveBonusesToDeactivate)
			{
				CAbility currentAbility = Choreographer.s_Choreographer.m_CurrentAbility;
				bool flag = currentAbility != null && currentAbility.Flags.HasFlag(CAbility.EAbilityFlag.FrozenFistMechanic);
				if (selectedCardsUI.Count == 0 || (currentMode != CardHandMode.RecoverDiscardedCard && currentMode != CardHandMode.RecoverLostCard))
				{
					if (flag)
					{
						Choreographer.s_Choreographer.readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONNA, null, hideOnClick: true, glowingEffect: false, interactable: false, disregardTurnControlForInteractability: false, haltActionProcessorIfDeactivated: false);
					}
					else
					{
						Choreographer.s_Choreographer.readyButton.Toggle(active: false, ReadyButton.EButtonState.EREADYBUTTONNA, null, hideOnClick: true, glowingEffect: false, interactable: true, disregardTurnControlForInteractability: false, haltActionProcessorIfDeactivated: false);
					}
				}
				if (selectedCardsUI.Count == 0 && currentMode == CardHandMode.SelectCardsWithActiveBonusTracker)
				{
					Choreographer.s_Choreographer.ClearGenericButton();
				}
				if (flag)
				{
					Choreographer.s_Choreographer.m_SkipButton.Toggle(active: true, null, hideOnClick: true, currentAbility.CanSkip);
					ActorBehaviour.GetActorBehaviour(Choreographer.s_Choreographer.FindClientActorGameObject(playerActor)).m_WorldspacePanelUI.ResetPreview();
				}
				else
				{
					Choreographer.s_Choreographer.m_SkipButton.Toggle(active: false);
				}
				if ((FFSNetwork.IsOnline && playerActor.SelectingCardsForExtraTurnOfType != CAbilityExtraTurn.EExtraTurnType.None && playerActor.IsUnderMyControl()) & networkAction)
				{
					Synchronizer.SendGameAction(GameActionType.SelectExtraTurnCards, ActionPhaseType.ExtraTurnCardSelection, validateOnServerBeforeExecuting: false, disableAutoReplication: false, playerActor.ID, 0, 0, 0, supplementaryDataBoolean: false, null, new CardsToken(selectedCardsUI));
				}
			}
			OnSelectedCardsNumberChanged();
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.OnCardDeselected().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00029", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void OnMoveTrackerOfActiveBonusCardClick()
	{
		List<CAbilityCard> selectedCards = selectedCardsUI.Select((AbilityCardUI x) => x.AbilityCard).ToList();
		int strength = Choreographer.s_Choreographer.m_CurrentAbility.Strength;
		(PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability as CAbilityMoveActiveBonusTracker).OnTrackerUpdated(selectedCards, strength);
		if (FFSNetwork.IsOnline && playerActor.IsUnderMyControl())
		{
			Synchronizer.SendGameAction(GameActionType.AbilityMoveActiveBonusTracker, ActionPhaseType.MoveActiveBonusTracker, validateOnServerBeforeExecuting: false, disableAutoReplication: false, playerActor.ID, 0, 0, 0, supplementaryDataBoolean: false, null, new CardsToken(selectedCardsUI), new IntToken(strength));
		}
	}

	private void OnLoseCardClick()
	{
		try
		{
			Log.GUIInfo("OnLoseCardClick");
			CPhase.PhaseType phaseType = PhaseManager.PhaseType;
			if (phaseType == CPhase.PhaseType.StartTurn || phaseType == CPhase.PhaseType.Action || phaseType == CPhase.PhaseType.EndTurn || phaseType == CPhase.PhaseType.EndRound || phaseType == CPhase.PhaseType.MonsterClassesSelectAbilityCards)
			{
				CAbility currentAbility = Choreographer.s_Choreographer.m_CurrentAbility;
				if (currentAbility != null && currentAbility.AbilityType == CAbility.EAbilityType.LoseCards)
				{
					List<CAbilityCard> list = selectedCardsUI.Select((AbilityCardUI x) => x.AbilityCard).ToList();
					if (Choreographer.s_Choreographer.m_CurrentAbility is CAbilityLoseCards cAbilityLoseCards)
					{
						cAbilityLoseCards.OnCardsSelected(list, PlayerActor);
					}
					else
					{
						GameState.PlayerSelectedCardsToLose(playerActor, list);
					}
					UIManager.Instance.StartCoroutine(AnimateCardsLost(selectedCardsUI, () =>
					{
						SetMode(currentMode, CardPileType.Any, new List<CardPileType> { CardPileType.None }, fadeUnselectableCards: true, highlightSelectableCards: true);
						GameState.PlayerFinishedMovingCards(playerActor);
					}));
					if (FFSNetwork.IsOnline && playerActor.IsUnderMyControl())
					{
						Synchronizer.SendGameAction(GameActionType.AbilityLoseCard, ActionPhaseType.CardRecovery, validateOnServerBeforeExecuting: false, disableAutoReplication: false, playerActor.ID, 0, 0, 0, supplementaryDataBoolean: false, null, new CardsToken(selectedCardsUI));
					}
				}
				else
				{
					CAbility currentAbility2 = Choreographer.s_Choreographer.m_CurrentAbility;
					if (currentAbility2 != null && currentAbility2.AbilityType == CAbility.EAbilityType.DiscardCards)
					{
						List<CAbilityCard> list2 = selectedCardsUI.Select((AbilityCardUI x) => x.AbilityCard).ToList();
						if (Choreographer.s_Choreographer.m_CurrentAbility is CAbilityDiscardCards cAbilityDiscardCards)
						{
							cAbilityDiscardCards.OnCardsSelected(list2, PlayerActor);
						}
						else
						{
							GameState.PlayerSelectedCardsToDiscard(playerActor, list2);
						}
						UIManager.Instance.StartCoroutine(AnimateCardsLost(selectedCardsUI, () =>
						{
							SetMode(currentMode, CardPileType.Any, new List<CardPileType> { CardPileType.None }, fadeUnselectableCards: true, highlightSelectableCards: true);
							GameState.PlayerFinishedMovingCards(playerActor);
						}));
						if (FFSNetwork.IsOnline && playerActor.IsUnderMyControl())
						{
							Synchronizer.SendGameAction(GameActionType.AbilityDiscardCard, ActionPhaseType.CardRecovery, validateOnServerBeforeExecuting: false, disableAutoReplication: false, playerActor.ID, 0, 0, 0, supplementaryDataBoolean: false, null, new CardsToken(selectedCardsUI));
						}
					}
					else
					{
						HandleAvoidDamage(hideOnComplete: true);
					}
				}
			}
			phaseType = PhaseManager.PhaseType;
			if (phaseType == CPhase.PhaseType.ActionSelection || phaseType == CPhase.PhaseType.EndTurnLoot || phaseType == CPhase.PhaseType.AfterEndTurnLoot || phaseType == CPhase.PhaseType.StartRoundEffects)
			{
				if (playerActor.CharacterClass.LongRest && GameState.InternalCurrentActor == playerActor)
				{
					HandleLongRest(selectedCardsUI[0].AbilityCard);
					if (FFSNetwork.IsOnline && playerActor.IsUnderMyControl())
					{
						Synchronizer.SendGameAction(GameActionType.LongRest, ActionPhaseType.LongRest, validateOnServerBeforeExecuting: false, disableAutoReplication: false, playerActor.ID, 0, 0, selectedCardsUI[0].AbilityCard.ID);
					}
				}
				else
				{
					GameState.EAvoidDamageOption avoidDamageOption = GameState.EAvoidDamageOption.None;
					Singleton<UIActiveBonusBar>.Instance.Hide(toggle: true);
					if (maxCardsSelected == 2)
					{
						GameState.Lose2DiscardCardsToAvoidAttack(playerActor, selectedCardsUI[0].AbilityCard, selectedCardsUI[1].AbilityCard);
						avoidDamageOption = GameState.EAvoidDamageOption.Lose2DiscardCards;
					}
					else if (maxCardsSelected == 1)
					{
						GameState.Lose1HandCardToAvoidAttack(playerActor, selectedCardsUI[0].AbilityCard);
						avoidDamageOption = GameState.EAvoidDamageOption.Lose1HandCard;
					}
					UIManager.Instance.StartCoroutine(AnimateCardsLost(selectedCardsUI, () =>
					{
						GameState.PlayerAvoidingDamage(avoidDamageOption);
						Hide();
					}));
					if (FFSNetwork.IsOnline && playerActor.IsUnderMyControl())
					{
						Synchronizer.SendGameAction((avoidDamageOption == GameState.EAvoidDamageOption.Lose2DiscardCards) ? GameActionType.BurnDiscardedCards : GameActionType.BurnAvailableCard, (Singleton<UIScenarioMultiplayerController>.Instance.damageSummaryToBurnCardForAlly != null) ? ActionPhaseType.ChooseCardsToBurnForAllyMonster : ActionPhaseType.TakeDamageConfirmation, validateOnServerBeforeExecuting: false, disableAutoReplication: false, playerActor.ID, 0, 0, 0, supplementaryDataBoolean: false, null, new CardsToken(selectedCardsUI));
					}
				}
			}
			if (PhaseManager.IsInCardsSelectionPhase)
			{
				TakeDamagePanel instanceFast = Singleton<TakeDamagePanel>.InstanceFast;
				if ((object)instanceFast != null && instanceFast.IsOpen)
				{
					HandleAvoidDamage(hideOnComplete: false);
				}
				else if (playerActor.CharacterClass.ImprovedShortRest && playerActor.CharacterClass.LongRest)
				{
					if (playerActor.CharacterClass.ImprovedShortRestCanExcludeCards && !playerActor.CharacterClass.ExcludedCardsFromShortRest)
					{
						playerActor.CharacterClass.ExcludedCardsFromShortRest = true;
						List<CAbilityCard> collection = selectedCardsUI.ConvertAll((AbilityCardUI sc) => sc.AbilityCard);
						playerActor.CharacterClass.ExcludedFromShortRestAbilityCards.Clear();
						playerActor.CharacterClass.ExcludedFromShortRestAbilityCards.AddRange(collection);
						if (FFSNetwork.IsOnline && playerActor.IsUnderMyControl())
						{
							ScenarioRuleClient.GetAndReplicateStartRoundDeckState(playerActor, 51, FastReload.CurrentHistoryID);
						}
						PerformShortRest(playerActor);
					}
					else
					{
						HandleImprovedShortRest(selectedCardsUI[0].AbilityCard, playerActor);
					}
				}
			}
			UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.ChooseLoseCardConfirmed));
			Singleton<TakeDamagePanel>.Instance.ResetAndHide(stopActiveBonus: true, switchBackToSavedState: true);
			Singleton<LoseCardsOptionsPanel>.Instance.ResetAndHide();
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.OnLoseCardClick().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00030", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void HandleAvoidDamage(bool hideOnComplete)
	{
		GameState.EAvoidDamageOption avoidDamageOption = GameState.EAvoidDamageOption.None;
		Singleton<UIActiveBonusBar>.Instance.Hide(toggle: true);
		if (maxCardsSelected == 2)
		{
			GameState.Lose2DiscardCardsToAvoidAttack(playerActor, selectedCardsUI[0].AbilityCard, selectedCardsUI[1].AbilityCard);
			avoidDamageOption = GameState.EAvoidDamageOption.Lose2DiscardCards;
		}
		else if (maxCardsSelected == 1)
		{
			GameState.Lose1HandCardToAvoidAttack(playerActor, selectedCardsUI[0].AbilityCard);
			avoidDamageOption = GameState.EAvoidDamageOption.Lose1HandCard;
		}
		UIManager.Instance.StartCoroutine(AnimateCardsLost(selectedCardsUI, () =>
		{
			GameState.PlayerAvoidingDamage(avoidDamageOption, playerActor);
			if (hideOnComplete)
			{
				Hide();
			}
		}));
		if (FFSNetwork.IsOnline && playerActor.IsUnderMyControl())
		{
			Synchronizer.SendGameAction((avoidDamageOption == GameState.EAvoidDamageOption.Lose2DiscardCards) ? GameActionType.BurnDiscardedCards : GameActionType.BurnAvailableCard, (Singleton<UIScenarioMultiplayerController>.Instance.damageSummaryToBurnCardForAlly != null) ? ActionPhaseType.ChooseCardsToBurnForAllyMonster : ActionPhaseType.TakeDamageConfirmation, validateOnServerBeforeExecuting: false, disableAutoReplication: false, playerActor.ID, 0, 0, 0, supplementaryDataBoolean: false, null, new CardsToken(selectedCardsUI));
		}
	}

	private void HandleLongRest(CAbilityCard cardToBurn)
	{
		ScenarioRuleClient.HandleLongRest(playerActor, cardToBurn, fromStateUpdate: false, FastReload.CurrentHistoryID);
	}

	private void HandleImprovedShortRest(CAbilityCard cardToBurn, CPlayerActor playerActor, bool fromStateUpdate = false)
	{
		ScenarioRuleClient.HandleImprovedShortRest(playerActor, cardToBurn, fromStateUpdate, FastReload.CurrentHistoryID);
	}

	private void OnRecoverCardClick()
	{
		try
		{
			if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
			{
				List<CAbilityCard> recoveredAbilityCards = selectedCardsUI.Select((AbilityCardUI x) => x.AbilityCard).ToList();
				if (FFSNetwork.IsOnline && playerActor.IsUnderMyControl())
				{
					Synchronizer.SendGameAction(GameActionType.RecoverCards, ActionPhaseType.CardRecovery, validateOnServerBeforeExecuting: false, disableAutoReplication: false, playerActor.ID, 0, 0, 0, supplementaryDataBoolean: false, null, new CardsToken(selectedCardsUI));
				}
				Log.GUIInfo("OnRecoverCardClick");
				GameState.PlayerSelectedCardsToRecover(playerActor, recoveredAbilityCards, currentMode.GetRecoverECardPile());
				UIManager.Instance.StartCoroutine(AnimateCardsLost(selectedCardsUI, () =>
				{
					SetMode(currentMode, CardPileType.Any, new List<CardPileType> { CardPileType.None }, fadeUnselectableCards: true, highlightSelectableCards: true);
					GameState.PlayerFinishedMovingCards(playerActor);
				}));
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.OnRecoverCardClick().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00031", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void OnInceaseLimitCardClick()
	{
		try
		{
			if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
			{
				List<CAbilityCard> increasingLimitAbilityCards = selectedCardsUI.Select((AbilityCardUI x) => x.AbilityCard).ToList();
				if (FFSNetwork.IsOnline && playerActor.IsUnderMyControl())
				{
					Synchronizer.SendGameAction(GameActionType.RecoverCards, ActionPhaseType.CardRecovery, validateOnServerBeforeExecuting: false, disableAutoReplication: false, playerActor.ID, 0, 0, 0, supplementaryDataBoolean: true, null, new CardsToken(selectedCardsUI));
				}
				Log.GUIInfo("OnInceaseLimitCardClick");
				GameState.PlayerSelectedCardsToIncreaseLimit(playerActor, increasingLimitAbilityCards);
				UIManager.Instance.StartCoroutine(AnimateCardsLost(selectedCardsUI, () =>
				{
					SetMode(currentMode, CardPileType.Any, new List<CardPileType> { CardPileType.None }, fadeUnselectableCards: true, highlightSelectableCards: true);
					GameState.PlayerFinishedMovingCards(playerActor);
				}));
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.OnInceaseLimitCardClick().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00031", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void OnSelectExtraTurnCardsClick()
	{
		try
		{
			if (PhaseManager.PhaseType == CPhase.PhaseType.Action || PhaseManager.PhaseType == CPhase.PhaseType.Autosave)
			{
				List<CAbilityCard> extraTurnCards = selectedCardsUI.Select((AbilityCardUI x) => x.AbilityCard).ToList();
				GameState.PlayerSelectedExtraTurnCards(playerActor, extraTurnCards);
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.OnSelectExtraTurnCardsClick().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00031", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void OnSelectedCardsNumberChanged(int number = -1)
	{
		try
		{
			if (number < 0)
			{
				number = selectedCardsUI.Count;
			}
			Singleton<CardsHandManager>.Instance.OnSelectedCardsNumberChanged(playerActor, number);
			if (maxCardsSelected <= 1)
			{
				return;
			}
			for (int i = 0; i < selectedCardsUI.Count; i++)
			{
				if (!selectedCardsUI[i].IsLongRest && playerActor.IsUnderMyControl())
				{
					int num = playerActor.CharacterClass.RoundAbilityCards.IndexOf(selectedCardsUI[i].AbilityCard);
					if (num >= 0)
					{
						selectedCardsUI[i].MiniAbilityCard.ShowSelectedNumber(num + 1);
					}
				}
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.OnSelectedCardsNumberChanged().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00032", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void RefreshCancelActiveAbility(CardPileType type, CAbilityCard disabledAbilityCard, bool animateDisabling)
	{
		try
		{
			AbilityCardUI cardUI = GetCardUI(disabledAbilityCard);
			if (!(cardUI != null))
			{
				return;
			}
			if (animateDisabling && gameObject.activeSelf)
			{
				if (AnimatingLostCards || playerActor.CharacterClass.ActivatedCards.Contains(disabledAbilityCard))
				{
					return;
				}
				UIManager.Instance.StartCoroutine(AnimateCardsLost(new List<AbilityCardUI> { cardUI }, () =>
				{
					cardUI.SetSelectable(isOn: false);
					if (PhaseManager.IsInCardsSelectionPhase)
					{
						UpdateLongRest();
						UpdateShortRest();
						ResetViewToCachedSettings();
					}
				}));
			}
			else
			{
				UpdateCards(type, EUpdateCardViewMode.Refresh);
				SortCards();
				cardUI.SetSelectable(isOn: false);
				if (PhaseManager.IsInCardsSelectionPhase)
				{
					UpdateLongRest();
					UpdateShortRest();
					ResetViewToCachedSettings();
				}
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.RefreshCancelActiveAbility().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00033", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void SetInteractable(bool interactable, bool allowHover)
	{
		canvasGroup.interactable = interactable;
		canvasGroup.blocksRaycasts = allowHover;
	}

	public void SelectCard(CAbilityCard card)
	{
		foreach (AbilityCardUI item in cardsUI)
		{
			if (card == item.AbilityCard)
			{
				if (!item.IsSelected)
				{
					item.OnClick(ignoreHiglight: true);
				}
				break;
			}
		}
	}

	public void UnselectCard(CAbilityCard card)
	{
		foreach (AbilityCardUI item in cardsUI)
		{
			if (card == item.AbilityCard)
			{
				if (item.IsSelected)
				{
					item.OnClick(ignoreHiglight: true);
				}
				break;
			}
		}
	}

	public List<InfuseElement> PickUnselectedInfusionsForItem(CItem item)
	{
		return itemsUI.PickUnselectedInfusionsForItem(item);
	}

	public void ResetItemElements(CItem item)
	{
		itemsUI.ResetItemElements(item);
	}

	public AbilityCardUI GetCard(int cardID, bool throwError = true)
	{
		try
		{
			AbilityCardUI abilityCardUI = cardsUI.SingleOrDefault((AbilityCardUI s) => s != null && s.CardID == cardID);
			if ((abilityCardUI == null) & throwError)
			{
				Log.GUIError("Could not find a card with ID " + cardID + " from actor with ID: " + ((playerActor != null) ? playerActor.ID.ToString() : "null"));
			}
			return abilityCardUI;
		}
		catch (Exception exception)
		{
			Log.GUIException(exception, "Unable to get card with id " + cardID);
			return null;
		}
	}

	public bool IsLongRestSelected()
	{
		if (selectedCardsUI != null && selectedCardsUI.Count == 1)
		{
			return selectedCardsUI[0].CardID == -1;
		}
		return false;
	}

	public bool IsShortRestSelected()
	{
		return shortRest.IsSelected;
	}

	public bool IsCardSelected(int cardID)
	{
		if (selectedCardsUI != null)
		{
			return selectedCardsUI.Exists((AbilityCardUI e) => e.CardID == cardID);
		}
		return false;
	}

	public void NetworkSelectedRoundCards()
	{
		if (FFSNetwork.IsOnline && !playerActor.IsUnderMyControl())
		{
			return;
		}
		CAbilityCard initiativeAbilityCard = playerActor.CharacterClass.InitiativeAbilityCard;
		if (initiativeAbilityCard != null && selectedCardsUI != null && selectedCardsUI.Count == playerActor.MaxRoundCardsCount)
		{
			string text = "Checking initiative\n";
			text += $"Initiative Card ID: {initiativeAbilityCard.ID} Initiative CardInstanceID: {initiativeAbilityCard.CardInstanceID}\n";
			for (int i = 0; i < selectedCardsUI.Count; i++)
			{
				text += string.Format("SelectedCards[{0}] ID: {1} SelectedCards[{2}] CardInstanceID: {3}\n", i, selectedCardsUI[i].IsLongRest ? "LongRest" : selectedCardsUI[i].AbilityCard.ID.ToString(), i, selectedCardsUI[i].IsLongRest ? "LongRest" : selectedCardsUI[i].AbilityCard.CardInstanceID.ToString());
			}
			Log.NetworkInfo(text);
		}
		Log.NetworkTrace(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat("" + "NEW BACKEND CARDS SELECTED FOR " + playerActor.CharacterClass.ID + "\n", "ROUND: ", playerActor.CharacterClass.RoundAbilityCards.Select((CAbilityCard x) => x.StrictName + " CardInstanceID: " + x.CardInstanceID).ToStringPretty(), "\n"), "HAND: ", playerActor.CharacterClass.HandAbilityCards.Select((CAbilityCard x) => x.StrictName + " CardInstanceID: " + x.CardInstanceID).ToStringPretty(), "\n"), "DISCARDED: ", playerActor.CharacterClass.DiscardedAbilityCards.Select((CAbilityCard x) => x.StrictName + " CardInstanceID: " + x.CardInstanceID).ToStringPretty(), "\n"), "BURNED: ", playerActor.CharacterClass.LostAbilityCards.Select((CAbilityCard x) => x.StrictName + " CardInstanceID: " + x.CardInstanceID).ToStringPretty(), "\n"), "ACTIVE: ", playerActor.CharacterClass.ActivatedAbilityCards.Select((CAbilityCard x) => x.StrictName + " CardInstanceID: " + x.CardInstanceID).ToStringPretty(), "\n"));
		if (!AutoTestController.s_AutoTestCurrentlyLoaded)
		{
			ScenarioRuleClient.GetAndReplicateStartRoundDeckState(playerActor, 32, FastReload.CurrentHistoryID);
		}
	}

	public void ProxySelectCard(int cardInstanceID)
	{
		AbilityCardUI abilityCardUI = cardsUI.Find((AbilityCardUI x) => x != null && x.IsMatchingCard(cardInstanceID));
		if (abilityCardUI != null)
		{
			abilityCardUI.ToggleSelect(active: true, highlight: false, networkAction: false);
			return;
		}
		throw new Exception("Error with proxy card selection. Could not find a card with CardInstanceID " + cardInstanceID + " from actor with ID: " + playerActor.ID);
	}

	public void ProxySelectDiscardedCardToPlay(int cardInstanceID)
	{
		AbilityCardUI abilityCardUI = cardsUI.Find((AbilityCardUI x) => x != null && x.IsMatchingCard(cardInstanceID));
		if (abilityCardUI != null)
		{
			abilityCardUI.OnClick();
			return;
		}
		throw new Exception("Error with proxy card selection. Could not find a card with CardInstanceID " + cardInstanceID + " from actor with ID: " + playerActor.ID);
	}

	public void ProxySelectCardAction(int cardInstanceID, CBaseCard.ActionType cardActionType)
	{
		AbilityCardUI abilityCardUI = cardsUI.Find((AbilityCardUI x) => (object)x != null && x.AbilityCard?.CardInstanceID == cardInstanceID);
		if (abilityCardUI != null)
		{
			abilityCardUI.fullAbilityCard.OnAbilityClick(cardActionType, isProxyAction: true, checkValid: false);
		}
	}

	public void ProxyBurnOneAvailableCard(int burnedCardID, bool animateCardLose = false)
	{
		AbilityCardUI abilityCardUI = cardsUI.Find((AbilityCardUI x) => (object)x != null && x.AbilityCard?.ID == burnedCardID);
		if (abilityCardUI != null)
		{
			selectedCardsUI.Insert(0, abilityCardUI);
			GameState.Lose1HandCardToAvoidAttack(playerActor, selectedCardsUI[0].AbilityCard);
			if (animateCardLose)
			{
				UIManager.Instance.StartCoroutine(AnimateCardsLost(selectedCardsUI, () =>
				{
					GameState.PlayerAvoidingDamage(GameState.EAvoidDamageOption.Lose1HandCard, playerActor);
					Hide();
				}));
			}
			else
			{
				CoroutineHelper.RunCoroutine(DelayCardLost(GameState.EAvoidDamageOption.Lose1HandCard));
			}
			return;
		}
		throw new Exception("Error trying to burn an available card for a proxy playerActor. Could not find a card with ID " + burnedCardID + " from actor with ID: " + playerActor.ID);
	}

	private IEnumerator DelayCardLost(GameState.EAvoidDamageOption option)
	{
		yield return null;
		InstantCardLost(selectedCardsUI);
		GameState.PlayerAvoidingDamage(option, playerActor);
		Hide();
	}

	public void ProxyBurnTwoDiscardedCards(int burnedCard1ID, int burnedCard2ID, bool animateCardLose = false)
	{
		AbilityCardUI abilityCardUI = cardsUI.Find((AbilityCardUI x) => x?.IsMatchingCardID(burnedCard1ID) ?? false);
		if (abilityCardUI != null)
		{
			selectedCardsUI.Insert(0, abilityCardUI);
			AbilityCardUI abilityCardUI2 = cardsUI.Find((AbilityCardUI x) => x?.IsMatchingCardID(burnedCard2ID) ?? false);
			if (abilityCardUI2 != null)
			{
				selectedCardsUI.Insert(1, abilityCardUI2);
				GameState.Lose2DiscardCardsToAvoidAttack(playerActor, selectedCardsUI[0].AbilityCard, selectedCardsUI[1].AbilityCard);
				if (animateCardLose)
				{
					UIManager.Instance.StartCoroutine(AnimateCardsLost(selectedCardsUI, () =>
					{
						GameState.PlayerAvoidingDamage(GameState.EAvoidDamageOption.Lose2DiscardCards, playerActor);
						Hide();
					}));
				}
				else
				{
					CoroutineHelper.RunCoroutine(DelayCardLost(GameState.EAvoidDamageOption.Lose2DiscardCards));
				}
				return;
			}
			throw new Exception("Error trying to burn the second discarded card for a proxy playerActor. Could not find a card with ID " + burnedCard2ID + " from actor with ID: " + playerActor.ID);
		}
		throw new Exception("Error trying to burn the first discarded card for a proxy playerActor. Could not find a card with ID " + burnedCard1ID + " from actor with ID: " + playerActor.ID);
	}

	public void ProxyGiveFromTakenInsteadOfBurnFS100(GameState.EAvoidDamageOption avoidChoice)
	{
		GameState.PlayerAvoidingDamage(avoidChoice);
		Hide();
	}

	public void ProxyShortRest(StartRoundCardsToken startRoundCardToken, bool fromStateUpdate = true)
	{
		if (playerActor.CharacterClass.HasShortRested || !startRoundCardToken.HasShortRested)
		{
			if (!startRoundCardToken.HasShortRested && startRoundCardToken.ImprovedShortRest && playerActor.CharacterClass.ImprovedShortRestCanExcludeCards)
			{
				playerActor.CharacterClass.ProxySetStartRoundDeckState(startRoundCardToken.GetStartRoundCardState());
				PerformShortRest(playerActor);
			}
			return;
		}
		AbilityCardUI abilityCardUI = cardsUI.Find((AbilityCardUI x) => x?.IsMatchingCardID(startRoundCardToken.CardBurnedID) ?? false);
		if (abilityCardUI != null)
		{
			if (startRoundCardToken.HasImprovedShortRested)
			{
				HandleImprovedShortRest(abilityCardUI.AbilityCard, playerActor, fromStateUpdate);
			}
			else
			{
				FinalizeShortRest(playerActor, abilityCardUI, startRoundCardToken.ShortRestCardRedrawn, networkActionIfOnline: false, fromStateUpdate);
			}
			Log.NetworkInfo("Short rest used for " + playerActor.CharacterClass.ID);
			return;
		}
		throw new Exception("Error using short rest for a proxy playerActor. Could not find a card with ID " + startRoundCardToken.CardBurnedID + " from actor with ID: " + playerActor.ID);
	}

	public void ProxyLongRest(int burnedCardID)
	{
		AbilityCardUI abilityCardUI = cardsUI.Find((AbilityCardUI x) => x?.IsMatchingCardID(burnedCardID) ?? false);
		if (abilityCardUI != null)
		{
			selectedCardsUI.Insert(0, abilityCardUI);
			HandleLongRest(abilityCardUI.AbilityCard);
			return;
		}
		throw new Exception("Error using long rest for a proxy playerActor. Could not find a card with ID " + burnedCardID + " from actor with ID: " + playerActor.ID);
	}

	public void ProxyRecoverCards(CardsToken cardsToken, bool increaseCardHandSize)
	{
		int i;
		for (i = 0; i < cardsToken.CardIDs.Length; i++)
		{
			AbilityCardUI abilityCardUI = cardsUI.Find((AbilityCardUI x) => x?.IsMatchingCardID(cardsToken.CardIDs[i]) ?? false);
			if (abilityCardUI != null)
			{
				selectedCardsUI.Insert(0, abilityCardUI);
				continue;
			}
			throw new Exception("Error recovering cards for a proxy playerActor. Could not find a card with ID " + cardsToken.CardIDs[i] + " from actor with ID: " + playerActor.ID);
		}
		Choreographer.s_Choreographer.readyButton.AlternativeAction(increaseCardHandSize ? new Action(OnInceaseLimitCardClick) : new Action(OnRecoverCardClick));
		Choreographer.s_Choreographer.readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONRECOVERCARD, LocalizationManager.GetTranslation("GUI_CONFIRM"), hideOnClick: true, glowingEffect: true);
		Choreographer.s_Choreographer.m_SkipButton.Toggle(active: false);
		OnSelectedCardsNumberChanged();
		Choreographer.s_Choreographer.readyButton.OnClickInternal(networkActionIfOnline: false);
	}

	public void ProxyAbilityDiscardCard(CardsToken cardsToken)
	{
		List<CAbilityCard> list = new List<CAbilityCard>();
		for (int i = 0; i < cardsToken.CardIDs.Length; i++)
		{
			int cardID = cardsToken.CardIDs[i];
			AbilityCardUI abilityCardUI = cardsUI.Find((AbilityCardUI x) => x?.IsMatchingCardID(cardID) ?? false);
			if (abilityCardUI == null)
			{
				throw new Exception("Error with proxy card discard. Could not find a card with card ID " + cardID + " from actor with ID: " + playerActor.ID);
			}
			list.Add(abilityCardUI.AbilityCard);
		}
		Log.GUIInfo("ProxyAbilityDiscardCard");
		if (Choreographer.s_Choreographer.m_CurrentAbility is CAbilityDiscardCards cAbilityDiscardCards)
		{
			cAbilityDiscardCards.OnCardsSelected(list, PlayerActor);
		}
		else
		{
			GameState.PlayerSelectedCardsToDiscard(playerActor, list);
		}
		UIManager.Instance.StartCoroutine(AnimateCardsLost(selectedCardsUI, () =>
		{
			SetMode(CardHandMode.DiscardCard, CardPileType.Any, new List<CardPileType> { CardPileType.None }, fadeUnselectableCards: true, highlightSelectableCards: true);
			GameState.PlayerFinishedMovingCards(playerActor);
		}));
		Singleton<LoseCardsOptionsPanel>.Instance.ResetAndHide();
	}

	public void ProxyAbilityLoseCard(CardsToken cardsToken)
	{
		List<CAbilityCard> list = new List<CAbilityCard>();
		for (int i = 0; i < cardsToken.CardIDs.Length; i++)
		{
			int cardID = cardsToken.CardIDs[i];
			AbilityCardUI abilityCardUI = cardsUI.Find((AbilityCardUI x) => x?.IsMatchingCardID(cardID) ?? false);
			if (abilityCardUI == null)
			{
				throw new Exception("Error with proxy card loss. Could not find a card with card ID " + cardID + " from actor with ID: " + playerActor.ID);
			}
			list.Add(abilityCardUI.AbilityCard);
		}
		Log.GUIInfo("ProxyAbilityLoseCard");
		if (Choreographer.s_Choreographer.m_CurrentAbility is CAbilityLoseCards cAbilityLoseCards)
		{
			cAbilityLoseCards.OnCardsSelected(list, PlayerActor);
		}
		else
		{
			GameState.PlayerSelectedCardsToLose(playerActor, list);
		}
		UIManager.Instance.StartCoroutine(AnimateCardsLost(selectedCardsUI, () =>
		{
			SetMode(CardHandMode.LoseCard, CardPileType.Any, new List<CardPileType> { CardPileType.None }, fadeUnselectableCards: true, highlightSelectableCards: true);
			GameState.PlayerFinishedMovingCards(playerActor);
		}));
		Singleton<LoseCardsOptionsPanel>.Instance.ResetAndHide();
	}

	public void ProxyAbilityMoveActiveBonusTracker(CardsToken cardsToken, IntToken intToken)
	{
		List<CAbilityCard> list = new List<CAbilityCard>();
		for (int i = 0; i < cardsToken.CardIDs.Length; i++)
		{
			int cardID = cardsToken.CardIDs[i];
			AbilityCardUI abilityCardUI = cardsUI.Find((AbilityCardUI x) => x?.IsMatchingCardID(cardID) ?? false);
			if (abilityCardUI == null)
			{
				throw new Exception("Error with proxy moving ability tracker. Could not find a card with card ID " + cardID + " from actor with ID: " + playerActor.ID);
			}
			list.Add(abilityCardUI.AbilityCard);
		}
		int data = intToken.Data;
		(PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability as CAbilityMoveActiveBonusTracker).OnTrackerUpdated(list, data);
	}

	public void ProxyChangeAgility(CPlayerActor.EAgility agility)
	{
		switch (agility)
		{
		case CPlayerActor.EAgility.None:
			PlayerActor.Agility = CPlayerActor.EAgility.None;
			return;
		case CPlayerActor.EAgility.Fast:
			m_TimeTokensFastToggle.SetIsOnWithoutNotify(value: true);
			m_TimeTokensSlowToggle.SetIsOnWithoutNotify(value: false);
			break;
		case CPlayerActor.EAgility.Slow:
			m_TimeTokensSlowToggle.SetIsOnWithoutNotify(value: true);
			m_TimeTokensFastToggle.SetIsOnWithoutNotify(value: false);
			break;
		}
		OnTimeTokensSelectionValueChanged(newValue: false);
	}

	public void ProxyChangePendingForm(CPlayerActor.EForm form)
	{
		if (form == CPlayerActor.EForm.Range)
		{
			m_GeminateRangeToggle.SetIsOnWithoutNotify(value: true);
			m_GeminateMeleeToggle.SetIsOnWithoutNotify(value: false);
		}
		else
		{
			m_GeminateMeleeToggle.SetIsOnWithoutNotify(value: true);
			m_GeminateRangeToggle.SetIsOnWithoutNotify(value: false);
		}
		OnGeminateFormSelectionChanged(newValue: false);
	}
}
