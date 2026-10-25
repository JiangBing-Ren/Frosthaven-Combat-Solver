using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using FFSNet;
using FHGUI.Scenario;
using FHGUI.Scenario.Services;
using Libs.Utils.Maths;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISimpleActionBox : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField]
	private bool _updateEveryFrame;

	[Range(0.001f, 1f)]
	[SerializeField]
	private float _fadeDurationSeconds = 0.5f;

	[SerializeField]
	private float _spacingBetweenButtons = 5f;

	[SerializeField]
	private float _paddingTop = 10f;

	[SerializeField]
	private float _paddingItemsBar = 20f;

	[SerializeField]
	private float _heightMin = 150f;

	[SerializeField]
	private float _heightLerpSpeed = 5f;

	[SerializeField]
	private float _monsterActorIllustrationsOffset = 40f;

	[Header("Elements")]
	[SerializeField]
	private CanvasGroup _canvasGroupMain;

	[SerializeField]
	private CanvasGroup _canvasGroupBackgroundVisuals;

	[SerializeField]
	private RectTransform _containerCurrentActorPortrait;

	[SerializeField]
	private Image _imageCurrentActorPortrait;

	[SerializeField]
	private RectTransform _containerUnderlay;

	[SerializeField]
	private RectTransform _containerCharacterMechanics;

	[SerializeField]
	private RectTransform _containerInitiatives;

	[SerializeField]
	private RectTransform _containerRests;

	[SerializeField]
	private RectTransform _anchorReadyTracker;

	[SerializeField]
	private RectTransform _currentPlayerUI;

	[SerializeField]
	private FHToggle _toggleShortRest;

	[SerializeField]
	private FHToggle _toggleLongRest;

	[SerializeField]
	private UndoButton _undoButton;

	[SerializeField]
	private SkipButton _skipButton;

	[SerializeField]
	private ShortRest _dialogShortRest;

	[SerializeField]
	private WarningBox _warningBox;

	private List<FHToggle> _toggleInitiatives = new List<FHToggle>();

	private readonly Dictionary<RectTransform, Vector2> _transformStartingAnchoredPositions = new Dictionary<RectTransform, Vector2>();

	private float _startingBackgroundHeight;

	private Coroutine _sizeCrt;

	private Tween _tweenFade;

	private readonly SmoothFloat _targetHeight = new SmoothFloat();

	private readonly SmoothFloat _targetAlpha = new SmoothFloat();

	private CanvasGroup _canvasGroupItemsBar;

	private bool _isDirty;

	private const float MAX_ALPHA = 1f;

	private const float MIN_ALPHA = 0f;

	public FHToggle ShortRest => _toggleShortRest;

	public FHToggle LongRest => _toggleLongRest;

	private void Awake()
	{
		_toggleShortRest.onValueChanged.AddListener(OnShortRestClick);
		_toggleShortRest.PointerEntered += OnShortRestMouseEnter;
		_toggleShortRest.PointerExited += OnShortRestMouseExit;
		_toggleLongRest.onValueChanged.AddListener(OnLongRestClick);
		_toggleLongRest.PointerEntered += OnLongRestMouseEnter;
		_toggleLongRest.PointerExited += OnLongRestMouseExit;
		_toggleInitiatives = _containerInitiatives.GetComponentsInChildren<FHToggle>(includeInactive: true).ToList();
		for (int i = 0; i < _toggleInitiatives.Count; i++)
		{
			int targetIndex = i;
			_toggleInitiatives[i].onValueChanged.AddListener((bool isOn) =>
			{
				OnInitiativeClick(targetIndex, isOn);
			});
		}
		_warningBox.Hide();
		CacheStartingAnchoredPosition(_containerRests);
		CacheStartingAnchoredPosition(_containerCharacterMechanics);
		UpdateBackgroundSize(immediate: true);
		StartCoroutine(SetupMultiplayerControlsPositions_Crt());
	}

	private void CacheStartingAnchoredPosition(RectTransform rt)
	{
		_transformStartingAnchoredPositions[rt] = rt.anchoredPosition;
	}

	private IEnumerator SetupMultiplayerControlsPositions_Crt()
	{
		while (!Singleton<UIReadyToggle>.IsInitialized || Choreographer.s_Choreographer == null)
		{
			yield return null;
		}
		RectTransform rectTransform = (RectTransform)Singleton<UIReadyToggle>.InstanceFast.transform;
		RectTransform rectTransform2 = (RectTransform)Choreographer.s_Choreographer.readyButton.transform;
		rectTransform.anchoredPosition = rectTransform2.anchoredPosition;
		rectTransform.sizeDelta = rectTransform2.sizeDelta;
		while (!Singleton<UIReadyTrackerBar>.IsInitialized)
		{
			yield return null;
		}
		UIHelperUtil.TrySetPositionConstraint(Singleton<UIReadyTrackerBar>.InstanceFast.gameObject, _anchorReadyTracker, 1f, createConstraintIfMissing: true);
		UIScrollText uIScrollText = Singleton<UIScenarioMultiplayerController>.InstanceFast?.CurrentPlayerUI;
		if ((bool)uIScrollText)
		{
			UIHelperUtil.TrySetPositionConstraint(uIScrollText.gameObject, _currentPlayerUI, 1f, createConstraintIfMissing: true);
		}
	}

	private CardsHandUI TryGetCurrentHand()
	{
		if (!Singleton<CardsHandManager>.IsInitialized)
		{
			return null;
		}
		return Singleton<CardsHandManager>.InstanceFast.CurrentHand;
	}

	private static IReadOnlyList<AbilityCardUI> GetOrderedSelectedCards(CardsHandUI hand)
	{
		return hand.SelectedCards.OrderBy((AbilityCardUI acui) => acui.AbilityCard?.Initiative).ToList();
	}

	private void OnShortRestClick(bool isOn)
	{
		if (FFSNetwork.IsOnline && PlayerRegistry.OtherPlayersAreJoining())
		{
			UIMultiplayerNotifications.ShowSelectedCardWhileWaitingForPlayersToJoin();
			return;
		}
		CardsHandUI cardsHandUI = TryGetCurrentHand();
		if (cardsHandUI == null)
		{
			_toggleShortRest.SetIsOnWithoutNotify(value: false);
			return;
		}
		_dialogShortRest.Init(cardsHandUI.PlayerActor, cardsHandUI.StartShortRest, () =>
		{
			_toggleShortRest.SetIsOnWithoutNotify(value: false);
		});
		_dialogShortRest.MouseClick();
	}

	private void OnShortRestMouseEnter()
	{
		if (_toggleShortRest.IsInteractable())
		{
			CardsHandUI cardsHandUI = TryGetCurrentHand();
			if (cardsHandUI != null)
			{
				cardsHandUI.SetRestLoseIndicatorVisibility(isVisible: true, checkShortRest: true, checkLongRest: false);
			}
		}
	}

	private void OnShortRestMouseExit()
	{
		CardsHandUI cardsHandUI = TryGetCurrentHand();
		if (cardsHandUI != null)
		{
			cardsHandUI.SetRestLoseIndicatorVisibility(isVisible: false, checkShortRest: false, checkLongRest: false);
		}
	}

	private void OnLongRestClick(bool isOn)
	{
		if (FFSNetwork.IsOnline && PlayerRegistry.OtherPlayersAreJoining())
		{
			UIMultiplayerNotifications.ShowSelectedCardWhileWaitingForPlayersToJoin();
			return;
		}
		AbilityCardUI abilityCardUI = TryGetCurrentHand()?.AllCards.FirstOrDefault((AbilityCardUI cui) => cui.IsLongRest);
		if (abilityCardUI != null)
		{
			abilityCardUI.OnClick();
		}
	}

	private void OnLongRestMouseEnter()
	{
		if (_toggleLongRest.IsInteractable())
		{
			CardsHandUI cardsHandUI = TryGetCurrentHand();
			if (cardsHandUI != null)
			{
				cardsHandUI.SetRestLoseIndicatorVisibility(isVisible: true, checkShortRest: false, checkLongRest: true);
			}
		}
	}

	private void OnLongRestMouseExit()
	{
		CardsHandUI cardsHandUI = TryGetCurrentHand();
		if (cardsHandUI != null)
		{
			cardsHandUI.SetRestLoseIndicatorVisibility(isVisible: false, checkShortRest: false, checkLongRest: false);
		}
	}

	private void OnInitiativeClick(int toggleIndex, bool isOn)
	{
		CardsHandUI cardsHandUI = TryGetCurrentHand();
		if (cardsHandUI == null)
		{
			return;
		}
		if (!isOn)
		{
			SetInitiativeTogglesState(cardsHandUI);
			return;
		}
		IReadOnlyList<AbilityCardUI> orderedSelectedCards = GetOrderedSelectedCards(cardsHandUI);
		if (orderedSelectedCards.Count >= 1)
		{
			while (cardsHandUI.PlayerActor.CharacterClass.InitiativeAbilityCard != orderedSelectedCards[toggleIndex].AbilityCard && orderedSelectedCards[toggleIndex].SwapInitiative())
			{
			}
			SetInitiativeTogglesState(cardsHandUI);
		}
	}

	public void Refresh(bool immediate)
	{
		_isDirty = true;
		if (immediate)
		{
			LateUpdate();
		}
	}

	private void Update()
	{
		if ((Singleton<UIReadyToggle>.IsInitialized && Singleton<UIReadyToggle>.InstanceFast.IsVisible) || Choreographer.IsInNoPartyCampaign())
		{
			Refresh(immediate: false);
		}
		if (!_targetHeight.IsCloseEnough(0.001f))
		{
			_targetHeight.Update(Time.deltaTime * _heightLerpSpeed);
			_containerUnderlay.sizeDelta = new Vector2(_containerUnderlay.sizeDelta.x, _targetHeight.Value);
		}
		bool flag = Singleton<FHScenarioViewController>.IsInitialized && Singleton<FHScenarioViewController>.InstanceFast.IsAnyReactionModuleActive();
		_targetAlpha.Value = (flag ? 0f : 1f);
		if (_targetAlpha.IsCloseEnough(0.001f))
		{
			return;
		}
		_targetAlpha.Update(Time.deltaTime * _heightLerpSpeed);
		float value = _targetAlpha.Value;
		bool clickable = !flag;
		UpdateCanvasGroup(_canvasGroupMain, value, clickable);
		if (Singleton<UIUseItemsBar>.IsInitialized)
		{
			if (_canvasGroupItemsBar == null)
			{
				_canvasGroupItemsBar = Singleton<UIUseItemsBar>.InstanceFast.GetComponent<CanvasGroup>();
			}
			UpdateCanvasGroup(_canvasGroupItemsBar, value, clickable);
		}
	}

	private static void UpdateCanvasGroup(CanvasGroup group, float alpha, bool clickable)
	{
		group.alpha = alpha;
		group.interactable = clickable;
		group.blocksRaycasts = clickable;
	}

	private void LateUpdate()
	{
		if (!UIManager.Instance.dialogPopup.IsOpen() && !_isDirty && !_updateEveryFrame)
		{
			return;
		}
		_isDirty = false;
		bool flag = ScenarioManager.CurrentScenarioState?.FreeShortRestButMuddle ?? false;
		bool flag2 = ScenarioManager.CurrentScenarioState?.FreeLongRest ?? false;
		TrySetTooltipText(_toggleShortRest, flag ? "GUI_TOOLTIP_SHORT_REST_StoryDifficulty" : "GUI_TOOLTIP_SHORT_REST");
		TrySetTooltipText(_toggleLongRest, flag2 ? "GUI_TOOLTIP_LONG_REST_StoryDifficulty" : "GUI_TOOLTIP_LONG_REST");
		UpdateBackgroundVisuals(out var currentActor);
		if (currentActor == null || !currentActor.IsUnderMyControl() || !PhaseManager.IsInCardsSelectionPhase || IsChoiceView() || Choreographer.IsInNoPartyCampaign())
		{
			_containerCharacterMechanics.gameObject.SetActive(value: false);
			_containerInitiatives.gameObject.SetActive(value: false);
			_containerRests.gameObject.SetActive(value: false);
			return;
		}
		CardsHandUI cardsHandUI = TryGetCurrentHand();
		if (!(cardsHandUI == null))
		{
			_containerCharacterMechanics.gameObject.SetActive(value: true);
			SetRestTogglesState(cardsHandUI);
			SetInitiativeTogglesState(cardsHandUI);
			UpdateControlsPositions();
		}
	}

	private void UpdateBackgroundVisuals(out CActor currentActor)
	{
		_tweenFade?.Kill();
		if (TryGetPortrait(out var sprite, out currentActor))
		{
			_imageCurrentActorPortrait.sprite = sprite;
			float num = 1f - Mathf.Min(_canvasGroupBackgroundVisuals.alpha, 1f);
			if (Utl.GreaterThan(num, 1E-05f))
			{
				float duration = _fadeDurationSeconds * num;
				_tweenFade = _canvasGroupBackgroundVisuals.DOFade(1f, duration);
			}
			bool flag = currentActor is CEnemyActor && UIManager.Instance?.dialogPopup?.IsOpen() == true;
			Vector2 anchoredPosition = _containerCurrentActorPortrait.anchoredPosition;
			anchoredPosition.y = (flag ? _monsterActorIllustrationsOffset : 0f);
			_containerCurrentActorPortrait.anchoredPosition = anchoredPosition;
		}
		else
		{
			float num2 = Mathf.Min(_canvasGroupBackgroundVisuals.alpha, 1f);
			if (Utl.GreaterThan(num2, 0f))
			{
				float num3 = num2 / 1f;
				float duration2 = _fadeDurationSeconds * num3;
				_tweenFade = _canvasGroupBackgroundVisuals.DOFade(0f, duration2);
			}
		}
		UpdateBackgroundSize(immediate: false);
		if (_sizeCrt == null)
		{
			_sizeCrt = StartCoroutine(UpdateBackgroundSizeNextFrame_Crt());
		}
	}

	private IEnumerator UpdateBackgroundSizeNextFrame_Crt()
	{
		yield return null;
		UpdateBackgroundSize(immediate: false);
		_sizeCrt = null;
	}

	private void UpdateBackgroundSize(bool immediate)
	{
		if (_startingBackgroundHeight == 0f)
		{
			_startingBackgroundHeight = _containerUnderlay.sizeDelta.y;
		}
		IsTallView(out var paddingHeight);
		_targetHeight.Value = Mathf.Max(_heightMin, _startingBackgroundHeight + paddingHeight);
		if (immediate)
		{
			_targetHeight.SnapToTarget();
		}
	}

	private void UpdateControlsPositions()
	{
		if (Singleton<CardsHandManager>.IsInitialized)
		{
			foreach (CardsHandUI item in Singleton<CardsHandManager>.InstanceFast.cardHandsUI)
			{
				item.SetCharacterMechanicsParent(_containerCharacterMechanics);
			}
		}
		UpdatePosition(_containerCharacterMechanics, (((RectTransform)_undoButton.transform).sizeDelta.x + _spacingBetweenButtons) * Vector2.right, !_undoButton.IsActiveAndEnabled);
		UpdatePosition(_containerRests, (((RectTransform)_skipButton.transform).sizeDelta.x + _spacingBetweenButtons) * Vector2.left, !_skipButton.IsActiveAndEnabled);
	}

	private void UpdatePosition(RectTransform rt, Vector2 offset, bool offsetCondition)
	{
		rt.anchoredPosition = (offsetCondition ? (_transformStartingAnchoredPositions[rt] + offset) : _transformStartingAnchoredPositions[rt]);
	}

	private void SetInitiativeTogglesState(CardsHandUI hand)
	{
		_containerInitiatives.gameObject.SetActive(value: true);
		bool flag = hand.PlayerActor.IsUnderMyControl();
		bool globalIsInteractable = GetGlobalIsInteractable();
		CAbilityCard initiativeAbilityCard = hand.PlayerActor.CharacterClass.InitiativeAbilityCard;
		IReadOnlyList<AbilityCardUI> orderedSelectedCards = GetOrderedSelectedCards(hand);
		int num = 2;
		int maxCardsSelected = hand.MaxCardsSelected;
		int count = orderedSelectedCards.Count;
		bool flag2 = count == 1 && orderedSelectedCards[0].IsLongRest;
		bool flag3 = hand.IsShortRestSelected();
		for (int i = 0; i < _toggleInitiatives.Count; i++)
		{
			FHToggle fHToggle = _toggleInitiatives[i];
			bool flag4 = ((flag2 | flag3) ? (i == 0) : (count >= num && i < maxCardsSelected));
			fHToggle.gameObject.SetActive(flag4);
			if (flag4)
			{
				int num2 = ((hand.PlayerActor.Agility == CPlayerActor.EAgility.Slow) ? 30 : 0);
				int num3 = (flag2 ? 99 : ((i < count) ? (orderedSelectedCards[i].AbilityCard.Initiative + num2) : 0));
				bool flag5 = num3 > 0;
				bool interactable = (globalIsInteractable & flag & flag5) && count >= num && i < count;
				fHToggle.SetIsOnWithoutNotify(flag5 && initiativeAbilityCard == orderedSelectedCards[i].AbilityCard);
				fHToggle.interactable = interactable;
				TextMeshProUGUI text = fHToggle.Text;
				string text2;
				if (flag5)
				{
					text2 = num3.ToString();
				}
				else
				{
					text2 = (flag3 ? "-" : string.Empty);
				}
				text.text = text2;
			}
		}
	}

	private void SetRestTogglesState(CardsHandUI hand)
	{
		bool flag = PhaseManager.PhaseType == CPhase.PhaseType.ShortRest;
		_containerRests.gameObject.SetActive(!flag);
		if (!flag)
		{
			bool globalIsInteractable = GetGlobalIsInteractable();
			AbilityCardUI abilityCardUI = hand.AllCards.FirstOrDefault((AbilityCardUI c) => c.IsLongRest);
			bool interactable = globalIsInteractable && hand.ShortRestUI.Button.interactable;
			bool interactable2 = globalIsInteractable && (abilityCardUI?.IsSelectable ?? false);
			_toggleShortRest.interactable = interactable;
			_toggleShortRest.SetIsOnWithoutNotify(hand.IsShortRestSelected());
			_toggleLongRest.interactable = interactable2;
			_toggleLongRest.SetIsOnWithoutNotify(hand.IsLongRestSelected());
		}
	}

	private bool GetGlobalIsInteractable()
	{
		if (!FFSNetwork.IsOnline)
		{
			return true;
		}
		if (Singleton<UIReadyToggle>.IsInitialized)
		{
			UIReadyToggle instanceFast = Singleton<UIReadyToggle>.InstanceFast;
			if ((instanceFast.ToggledOn || instanceFast.IsProgressingBar) && instanceFast.IsVisible)
			{
				return false;
			}
		}
		return true;
	}

	private bool TryGetPortrait(out Sprite sprite, out CActor currentActor)
	{
		sprite = null;
		currentActor = (Singleton<FHScenarioViewController>.IsInitialized ? Singleton<FHScenarioViewController>.InstanceFast.GetService<PlayerViewService>().GetActorMakingChoice() : null);
		if (currentActor == null)
		{
			return false;
		}
		bool flag = false;
		if (GameState.ThreadIsSleeping && GameState.IsChoosingToBurnCardForAlly)
		{
			currentActor = GameState.CurrentDamageData.ActorDamaged;
			TakeDamagePanel instanceFast = Singleton<TakeDamagePanel>.InstanceFast;
			if ((object)instanceFast != null && instanceFast.IsOpen && Singleton<TakeDamagePanel>.InstanceFast.ActorToShowCardsFor != null && Singleton<TakeDamagePanel>.InstanceFast.state == TakeDamagePanel.State.ShowingBurnCardForAlly)
			{
				currentActor = Singleton<TakeDamagePanel>.InstanceFast.ActorToShowCardsFor;
			}
			flag = currentActor != null;
		}
		CActor cActor = currentActor;
		Sprite sprite2;
		if (cActor is CEnemyActor cEnemyActor)
		{
			sprite2 = UIInfoTools.Instance.GetMonsterConfig(cEnemyActor.MonsterClass)?.Illustration;
		}
		else if (cActor is CHeroSummonActor cHeroSummonActor)
		{
			sprite2 = UIInfoTools.Instance.GetSummonConfig(cHeroSummonActor.HeroSummonClass)?.Illusration;
		}
		else
		{
			sprite2 = ((!(cActor is CPlayerActor cPlayerActor)) ? null : UIInfoTools.Instance.GetCharacterConfigUI(cPlayerActor.CharacterClass.CharacterModel)?.InfoIllustration);
		}
		sprite = sprite2;
		if (sprite == null)
		{
			return false;
		}
		float paddingHeight;
		return (Choreographer.s_Choreographer.readyButton.IsActiveAndEnabled || _undoButton.IsActiveAndEnabled || _skipButton.IsActiveAndEnabled || (Singleton<UIReadyToggle>.IsInitialized && Singleton<UIReadyToggle>.InstanceFast.IsVisible) || IsChoiceView() || IsTallView(out paddingHeight)) | flag;
	}

	private bool IsTallView(out float paddingHeight)
	{
		paddingHeight = _paddingTop;
		HelpBox instanceFast = Singleton<HelpBox>.InstanceFast;
		if ((bool)instanceFast && instanceFast.IsShown)
		{
			paddingHeight += ((RectTransform)instanceFast.transform).sizeDelta.y;
		}
		if (_toggleInitiatives.Any((FHToggle t) => t.gameObject.activeInHierarchy))
		{
			paddingHeight += _containerInitiatives.sizeDelta.y;
			return true;
		}
		if ((Singleton<UIUseItemsBar>.IsInitialized && Singleton<UIUseItemsBar>.InstanceFast.HasShownUsableSlots()) || (Singleton<UIUseAugmentationsBar>.IsInitialized && Singleton<UIUseAugmentationsBar>.InstanceFast.HasShownUsableSlots()) || (Singleton<UIActiveBonusBar>.IsInitialized && Singleton<UIActiveBonusBar>.InstanceFast.HasShownUsableSlots()) || (Singleton<UIActiveBonusBar>.IsInitialized && Singleton<UIUseAbilitiesBar>.InstanceFast.HasShownUsableSlots()))
		{
			paddingHeight += ((RectTransform)Singleton<UIUseItemsBar>.InstanceFast.transform).sizeDelta.y;
			paddingHeight += _paddingItemsBar;
			return true;
		}
		if (UIManager.Instance.dialogPopup.IsOpen())
		{
			paddingHeight += _containerInitiatives.sizeDelta.y;
			return true;
		}
		return false;
	}

	private bool IsChoiceView()
	{
		if (!Singleton<TakeDamagePanel>.IsInitialized || !Singleton<TakeDamagePanel>.InstanceFast.IsOpen)
		{
			return UIManager.Instance.dialogPopup.IsOpen();
		}
		return true;
	}

	private static void TrySetTooltipText(Component component, string locKey)
	{
		UITextTooltipTarget componentInChildren = component.GetComponentInChildren<UITextTooltipTarget>();
		if (componentInChildren != null)
		{
			componentInChildren.tooltipText = locKey;
		}
	}

	private void OnDestroy()
	{
		if (Singleton<UIReadyTrackerBar>.IsInitialized)
		{
			UIHelperUtil.ClearPositionConstraint(Singleton<UIReadyTrackerBar>.InstanceFast.gameObject);
		}
	}

	public void ShowWarning(string text)
	{
		_warningBox.ShowTranslated(text, HelpBox.FormatTarget.ALL);
	}
}
