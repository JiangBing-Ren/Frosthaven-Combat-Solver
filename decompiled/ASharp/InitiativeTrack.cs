using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chronos;
using DG.Tweening;
using FFSNet;
using FHGUI.Scenario.Services;
using GLOOM;
using Libs.Utils;
using Libs.Utils.Profiler;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class InitiativeTrack : MonoBehaviour
{
	public enum ActorPhaseState
	{
		None,
		CardSelection,
		Resting
	}

	public enum InitiativeEffects
	{
		None,
		Hover,
		Select,
		Active
	}

	public struct ActorAttributesParams
	{
		public CActor Actor;

		public CClass Class;

		public string TextInitiative;

		public int CurrentHealth;

		public int MaxHealth;

		public bool IsActiveButton;

		public bool IsFactionDisplayed;

		public bool IsActing;

		public bool IsTargeted;

		public bool IsTargetedByPositiveAbility;

		public int SummonsCount;

		public ActorPhaseState PhaseState;

		public bool ShowNextCard;
	}

	public struct TargetingParams
	{
		public CActor ActingActor;

		public List<CActor> TargetedActors;

		public bool IsPositiveAbility;

		private static readonly CAbility.EAbilityType[] _positiveAbilities = new CAbility.EAbilityType[66]
		{
			CAbility.EAbilityType.Heal,
			CAbility.EAbilityType.PreventDamage,
			CAbility.EAbilityType.Shield,
			CAbility.EAbilityType.RecoverLostCards,
			CAbility.EAbilityType.Invisible,
			CAbility.EAbilityType.Strengthen,
			CAbility.EAbilityType.Bless,
			CAbility.EAbilityType.Infuse,
			CAbility.EAbilityType.RefreshItemCards,
			CAbility.EAbilityType.RecoverDiscardedCards,
			CAbility.EAbilityType.Summon,
			CAbility.EAbilityType.Advantage,
			CAbility.EAbilityType.Teleport,
			CAbility.EAbilityType.AddHeal,
			CAbility.EAbilityType.AddRange,
			CAbility.EAbilityType.ControlActor,
			CAbility.EAbilityType.AddAugment,
			CAbility.EAbilityType.AddSong,
			CAbility.EAbilityType.PlaySong,
			CAbility.EAbilityType.AddActiveBonus,
			CAbility.EAbilityType.ConsumeElement,
			CAbility.EAbilityType.ChooseAbility,
			CAbility.EAbilityType.IncreaseCardLimit,
			CAbility.EAbilityType.Augment,
			CAbility.EAbilityType.ExtraTurn,
			CAbility.EAbilityType.ImprovedShortRest,
			CAbility.EAbilityType.Overheal,
			CAbility.EAbilityType.GainXP,
			CAbility.EAbilityType.AdjustInitiative,
			CAbility.EAbilityType.Swap,
			CAbility.EAbilityType.RedistributeDamage,
			CAbility.EAbilityType.ForgoActionsForCompanion,
			CAbility.EAbilityType.ChangeModifier,
			CAbility.EAbilityType.Immovable,
			CAbility.EAbilityType.RemoveConditions,
			CAbility.EAbilityType.GiveSupplyCard,
			CAbility.EAbilityType.LongRest,
			CAbility.EAbilityType.ShortRest,
			CAbility.EAbilityType.Invulnerability,
			CAbility.EAbilityType.Untargetable,
			CAbility.EAbilityType.RecoverResources,
			CAbility.EAbilityType.ChangeCharacterModel,
			CAbility.EAbilityType.Revive,
			CAbility.EAbilityType.DisableCardAction,
			CAbility.EAbilityType.Regenerate,
			CAbility.EAbilityType.PlayDiscardedCard,
			CAbility.EAbilityType.Teleport2,
			CAbility.EAbilityType.ShiftForm,
			CAbility.EAbilityType.MirrorDamage,
			CAbility.EAbilityType.RepeatAbility,
			CAbility.EAbilityType.PreventLoseCard,
			CAbility.EAbilityType.AddAttackModifier,
			CAbility.EAbilityType.ExcludeCardsFromShortRest,
			CAbility.EAbilityType.CreateShadow,
			CAbility.EAbilityType.RemoveShadow,
			CAbility.EAbilityType.MoveShadow,
			CAbility.EAbilityType.MoveActiveBonusTracker,
			CAbility.EAbilityType.ReviveDowned,
			CAbility.EAbilityType.PerformFromShadow,
			CAbility.EAbilityType.Capture,
			CAbility.EAbilityType.ModeTransfer,
			CAbility.EAbilityType.SelectAndPlayCard,
			CAbility.EAbilityType.ControlSummonTurn,
			CAbility.EAbilityType.SelectAndMoveCard,
			CAbility.EAbilityType.CreateLimitedHazardousTerrain,
			CAbility.EAbilityType.SelectCard
		};

		public static TargetingParams GetFromChoreographerState()
		{
			CAbility currentAbility = Choreographer.s_Choreographer.m_CurrentAbility;
			bool isPositiveAbility = false;
			if (currentAbility != null)
			{
				isPositiveAbility = currentAbility.PositiveConditions.Any() || _positiveAbilities.Contains(currentAbility.AbilityType);
			}
			return new TargetingParams
			{
				ActingActor = Choreographer.s_Choreographer.CurrentActor,
				TargetedActors = currentAbility?.ActorsToTarget,
				IsPositiveAbility = isPositiveAbility
			};
		}
	}

	[Serializable]
	private struct AvatarScalingRule
	{
		[MinMaxSlider(0f, 99f)]
		public Vector2Int Range;

		[Range(0.1f, 1f)]
		public float ScaleFactor;
	}

	private static InitiativeTrack instance;

	[SerializeField]
	private UIActorEventsChannel _actorEventsChannel;

	[Header("Animation")]
	[SerializeField]
	private float trackReorderDuration = 0.5f;

	[SerializeField]
	private float delayAnimationDraw = 0.5f;

	[Header("Slots")]
	[SerializeField]
	private List<AvatarScalingRule> m_AvatarScalingRules = new List<AvatarScalingRule>();

	[SerializeField]
	public GameObject playerBehaviourPrefab;

	[SerializeField]
	private GameObject enemyBehaviourPrefab;

	[SerializeField]
	private GameObject summonBehaviourPrefab;

	[SerializeField]
	private Transform initiativeTrackHolder;

	[SerializeField]
	private GameObject initiativeTrackActorsSeparator;

	[Header("Container Elements")]
	[SerializeField]
	private HorizontalLayoutGroup layoutGroup;

	[SerializeField]
	private ContentSizeFitter contentSizeFitter;

	[SerializeField]
	public HelpBox helpBox;

	[SerializeField]
	private Transform enemyCardsHolder;

	[SerializeField]
	private Transform hotkeysHolder;

	[SerializeField]
	private string revealEnemiesAudioItem = "PlaySound_EnemyCardDraw";

	private readonly AvatarScalingRule m_DefaultScaling = new AvatarScalingRule
	{
		Range = Vector2Int.zero,
		ScaleFactor = 1f
	};

	private List<InitiativeTrackActorBehaviourBase> actorsUI = new List<InitiativeTrackActorBehaviourBase>();

	private List<InitiativeTrackPlayerBehaviourExtension> playersUI = new List<InitiativeTrackPlayerBehaviourExtension>();

	private List<InitiativeTrackEnemyBehaviourExtension> enemiesUI = new List<InitiativeTrackEnemyBehaviourExtension>();

	private List<InitiativeTrackSummonBehaviourExtension> summonsUI = new List<InitiativeTrackSummonBehaviourExtension>();

	private InitiativeTrackActorBehaviourBase selectedActor;

	private InitiativeTrackActorBehaviourBase _focusedActor;

	private bool _prevGamepadInUse;

	private bool isAnimating;

	private bool animationDelayed;

	private List<float> locations = new List<float>();

	private List<Tween> moveXAnimations = new List<Tween>();

	private Comparer<InitiativeTrackActorBehaviourBase> PlayerTypeComparer = Comparer<InitiativeTrackActorBehaviourBase>.Create((InitiativeTrackActorBehaviourBase x, InitiativeTrackActorBehaviourBase y) =>
	{
		if (y != null && x != null && x.ActorType == y.ActorType)
		{
			return x.CompareTo(y);
		}
		if (x != null)
		{
			CActor.EType actorType = x.ActorType;
			if (actorType == CActor.EType.Player || actorType == CActor.EType.HeroSummon)
			{
				return -1;
			}
		}
		return (x != null && x.ActorType == CActor.EType.Enemy) ? 1 : 0;
	});

	public static InitiativeTrack Instance
	{
		get
		{
			if (instance == null)
			{
				instance = UnityEngine.Object.FindObjectOfType<InitiativeTrack>();
			}
			return instance;
		}
	}

	public RectTransform InitiativeTrackRect => initiativeTrackHolder as RectTransform;

	public List<InitiativeTrackPlayerBehaviourExtension> PlayersUI => playersUI;

	public bool IsSelectable
	{
		get
		{
			if (PhaseManager.PhaseType == CPhase.PhaseType.MonsterClassesSelectAbilityCards)
			{
				return false;
			}
			if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
			{
				return false;
			}
			if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest && selectedActor?.Actor is CPlayerActor cPlayer)
			{
				CardsHandUI hand = Singleton<CardsHandManager>.Instance.GetHand(cPlayer);
				if (hand != null && hand.IsImprovedLongResting)
				{
					return false;
				}
			}
			return true;
		}
	}

	private void OnDestroy()
	{
		instance = null;
		actorsUI.Clear();
		playersUI.Clear();
		enemiesUI.Clear();
		summonsUI.Clear();
		selectedActor = null;
		_focusedActor = null;
	}

	private void OnEnable()
	{
		_actorEventsChannel.OnActorHighlighted.AddListener(OnActorHighlightedOrFocused);
		_actorEventsChannel.OnActorFocused.AddListener(OnActorHighlightedOrFocused);
	}

	private void OnDisable()
	{
		_actorEventsChannel.OnActorHighlighted.RemoveListener(OnActorHighlightedOrFocused);
		_actorEventsChannel.OnActorFocused.RemoveListener(OnActorHighlightedOrFocused);
	}

	private void Update()
	{
		if (animationDelayed && !isAnimating)
		{
			animationDelayed = false;
			UpdateActors();
		}
		bool usingGamepad = Singleton<InputFHManager>.Instance.UsingGamepad;
		if (_prevGamepadInUse != usingGamepad)
		{
			ClearActorHighlight();
		}
		_prevGamepadInUse = usingGamepad;
		hotkeysHolder.gameObject.SetActive(IsSelectable && _focusedActor != null);
	}

	public void OnCardHover(bool isActive, AbilityCardUI cardUI, CPlayerActor playerActor)
	{
		if (PhaseManager.IsInCardsSelectionPhase && playerActor?.CharacterClass?.RoundAbilityCards != null)
		{
			_ = playerActor.CharacterClass.RoundAbilityCards.Count;
		}
	}

	public void UpdateActors()
	{
		if (!isAnimating)
		{
			StopAllCoroutines();
			foreach (InitiativeTrackActorBehaviourBase item in actorsUI)
			{
				item.RefreshInitiative();
			}
			if (Singleton<CardsHandManager>.Instance.CurrentHand != null)
			{
				Singleton<CardsHandManager>.Instance.CurrentHand.UpdateEffects();
			}
			StartCoroutine(AnimateInitiativeReorder());
		}
		else
		{
			animationDelayed = true;
		}
	}

	private IEnumerator AnimateInitiativeReorder()
	{
		isAnimating = true;
		contentSizeFitter.enabled = false;
		layoutGroup.enabled = false;
		if (trackReorderDuration > 1E-05f)
		{
			yield return new WaitForEndOfFrame();
		}
		locations.Clear();
		moveXAnimations.Clear();
		InitiativeTrackActorBehaviourBase[] componentsInChildren = initiativeTrackHolder.GetComponentsInChildren<InitiativeTrackActorBehaviourBase>(includeInactive: false);
		foreach (InitiativeTrackActorBehaviourBase initiativeTrackActorBehaviourBase in componentsInChildren)
		{
			locations.Add(initiativeTrackActorBehaviourBase.transform.position.x);
		}
		UpdateSortingOrder();
		int num = 0;
		foreach (InitiativeTrackActorBehaviourBase item in actorsUI)
		{
			if (item.gameObject.activeSelf)
			{
				Tween anim = item.transform.DOMoveX(locations[num], trackReorderDuration).SetEase(Ease.OutQuad);
				anim.OnComplete(() =>
				{
					moveXAnimations.Remove(anim);
				});
				moveXAnimations.Add(anim);
				num++;
			}
		}
		yield return Timekeeper.instance.WaitForSeconds(trackReorderDuration);
		moveXAnimations.Clear();
		locations.Clear();
		layoutGroup.enabled = true;
		contentSizeFitter.enabled = true;
		if (initiativeTrackActorsSeparator.activeSelf)
		{
			ShowSeparatorActorsByType();
		}
		if (trackReorderDuration > 1E-05f)
		{
			yield return new WaitForEndOfFrame();
		}
		isAnimating = false;
	}

	private void StopUpdateActors()
	{
		if (isAnimating)
		{
			int num = 0;
			if (locations.Count > 0)
			{
				foreach (InitiativeTrackActorBehaviourBase item in actorsUI)
				{
					if (item.gameObject.activeSelf)
					{
						item.transform.position = new Vector3(locations[locations.Count - 1 - num], item.transform.position.y, item.transform.position.z);
						num++;
					}
				}
			}
			isAnimating = false;
			layoutGroup.enabled = true;
			contentSizeFitter.enabled = true;
		}
		locations.Clear();
		moveXAnimations.ForEach((Tween it) =>
		{
			it.Kill();
		});
		moveXAnimations.Clear();
	}

	public void Select(InitiativeTrackActorBehaviourBase actorUI, bool forced = false)
	{
		if (!forced && !IsSelectable)
		{
			Log.GUIInfo("Skip select " + actorUI?.Actor?.GetPrefabName() + " in phase " + PhaseManager.PhaseType);
			return;
		}
		if (selectedActor != null && !actorUI.Actor.IsTakingExtraTurn)
		{
			selectedActor.Deselect();
		}
		selectedActor = actorUI;
		selectedActor.Select();
		SetFocusedActor(selectedActor, showHighlight: false, focusCamera: true);
		UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.ActorSelected, actorUI.Actor.GetPrefabName(), actorUI.Actor.ActorGuid));
	}

	public bool Select(CPlayerActor playerActor, bool forced = false)
	{
		if (!forced && !IsSelectable)
		{
			return false;
		}
		foreach (InitiativeTrackPlayerBehaviourExtension item in playersUI)
		{
			if (item.Base.Actor == playerActor)
			{
				if (!forced && !InteractabilityManager.ShouldAllowClickForFHButton(item.Base.Avatar.Button as FHButton))
				{
					Log.GUIInfo("Button press for button " + gameObject.name + " intercepted and prevented by InteractabilityManager");
					return false;
				}
				Select(item.Base, forced);
				return true;
			}
		}
		return false;
	}

	public void SelectNext()
	{
		CycleActorSelection(next: true);
	}

	public void SelectPrev()
	{
		CycleActorSelection(next: false);
	}

	private void CycleActorSelection(bool next)
	{
		if (!IsSelectable || (PhaseManager.PhaseType != CPhase.PhaseType.SelectAbilityCardsOrLongRest && !PhaseManager.IsInActionSelectionPhase))
		{
			return;
		}
		bool isCardSelection = PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest;
		List<InitiativeTrackActorBehaviourBase> list = actorsUI.Where((InitiativeTrackActorBehaviourBase it) => it.gameObject.activeSelf && (!isCardSelection || it.ActorType == CActor.EType.Player)).ToList();
		if (list.Count != 0)
		{
			InitiativeTrackActorBehaviourBase item = (isCardSelection ? selectedActor : _focusedActor);
			int num = (next ? ((list.IndexOf(item) + 1) % list.Count) : (list.IndexOf(item) - 1));
			if (num < 0)
			{
				num += list.Count;
			}
			InitiativeTrackActorBehaviourBase initiativeTrackActorBehaviourBase = list[num];
			if (isCardSelection)
			{
				Select(initiativeTrackActorBehaviourBase);
			}
			else
			{
				SetFocusedActor(initiativeTrackActorBehaviourBase, showHighlight: true, focusCamera: true);
			}
		}
	}

	public void ClearActorHighlight()
	{
		if (IsSelectable && isActiveAndEnabled)
		{
			SetFocusedActor(selectedActor, showHighlight: false, focusCamera: false);
		}
	}

	private void OnActorHighlightedOrFocused(object sender, CActor actor, bool highlighted, bool showCards)
	{
		if (actor != null && !(sender is InitiativeTrackActorAvatarBase) && WorldspaceStarHexDisplay.Instance.CurrentAbilityDisplayType == WorldspaceStarHexDisplay.EAbilityDisplayType.None && TryGetActorBehaviourBase(actor, out var behaviourBase))
		{
			behaviourBase.Avatar.SetHighlighted(highlighted, raiseEvent: false);
		}
	}

	public void SetFocusedActor(InitiativeTrackActorBehaviourBase targetActorBehaviour, bool showHighlight, bool focusCamera)
	{
		if (_focusedActor != null)
		{
			_actorEventsChannel.ActorFocused(this, _focusedActor.Actor, focused: false, showCards: false);
		}
		_focusedActor = targetActorBehaviour;
		if (_focusedActor == null)
		{
			hotkeysHolder.gameObject.SetActive(value: false);
			return;
		}
		_actorEventsChannel.ActorFocused(this, _focusedActor.Actor, showHighlight, showCards: true);
		hotkeysHolder.gameObject.SetActive(value: true);
		hotkeysHolder.position = new Vector3(_focusedActor.transform.position.x, hotkeysHolder.position.y);
		Choreographer.s_Choreographer.ClearHilightedActors();
		GameObject gameObject = Choreographer.s_Choreographer.FindClientActorGameObject(_focusedActor.Actor);
		ActorBehaviour.SetHilighted(gameObject, hilight: true);
		if (focusCamera)
		{
			CameraController.s_CameraController.SmartFocus(gameObject, pauseDuringTransition: true);
			CameraController.s_CameraController.SetFocalPointGameObject(gameObject);
		}
	}

	[ContextMenu("Play draw enemyActor card animation")]
	private void EnemyAbilityCardsAnimation(List<CEnemyActor> showSpecificActors = null)
	{
		UpdateSortingOrderByPlayerType();
		int num = 0;
		List<CClass> list = new List<CClass>();
		foreach (InitiativeTrackEnemyBehaviourExtension item in enemiesUI)
		{
			CClass cClass = item.Base.Class ?? item.Base.Actor?.Class;
			bool flag = cClass == null || !item.gameObject.activeSelf || (list.Contains(cClass) && cClass.Models.Count == 1);
			if (showSpecificActors != null && item.Base.Actor != null && !showSpecificActors.Contains(item.Base.Actor))
			{
				flag = true;
			}
			if (!flag)
			{
				list.Add(cClass);
			}
		}
		int count = list.Count;
		AudioControllerUtils.PlaySound(revealEnemiesAudioItem);
		foreach (InitiativeTrackEnemyBehaviourExtension item2 in enemiesUI)
		{
			if (item2.gameObject.activeSelf)
			{
				CClass cClass2 = item2.Base.Class ?? item2.Base.Actor?.Class;
				if (cClass2 != null && list.Contains(cClass2))
				{
					Action onCompleteCallback = ((num == 0) ? new Action(EndedEnemyAbilityCardsAnimation) : null);
					item2.AnimateCardHighlight(num, count, onCompleteCallback, delayAnimationDraw * (float)(count - 1 - num));
					list.Remove(cClass2);
					num++;
				}
			}
		}
		Choreographer.s_Choreographer.readyButton.AlternativeAction(FinishAbilityCardsAnimation, ReadyButton.EButtonState.EREADYBUTTONCONTINUE, LocalizationManager.GetTranslation("GUI_CONTINUE"));
	}

	private void EndedEnemyAbilityCardsAnimation()
	{
		foreach (InitiativeTrackActorBehaviourBase item in actorsUI)
		{
			if (item.gameObject.activeSelf && item.Actor.IsEnemyByDefault())
			{
				((InitiativeTrackEnemyBehaviourExtension)item.Extension).OnCardHighlightAnimationComplete();
			}
		}
		StartCoroutine(InitiativeEnemyAbilityCardsAnimation());
	}

	private void ShowSeparatorActorsByType()
	{
		int num = -1;
		foreach (InitiativeTrackActorBehaviourBase item in actorsUI)
		{
			if (item.Actor.IsPlayerByDefault() && item.gameObject.activeSelf)
			{
				num = item.transform.GetSiblingIndex();
			}
		}
		if (num >= 0)
		{
			initiativeTrackActorsSeparator.SetActive(value: true);
			initiativeTrackActorsSeparator.transform.SetSiblingIndex(num);
		}
	}

	private IEnumerator InitiativeEnemyAbilityCardsAnimation()
	{
		initiativeTrackActorsSeparator.SetActive(value: false);
		yield return AnimateInitiativeReorder();
		if (!GameState.ThreadIsSleeping)
		{
			Choreographer.s_Choreographer.readyButton.AlternativeAction(PostEnemyCardAnimationProceed, ReadyButton.EButtonState.EREADYBUTTONCONTINUE, LocalizationManager.GetTranslation("GUI_CONTINUE"));
		}
	}

	private void FinishAbilityCardsAnimation()
	{
		initiativeTrackActorsSeparator.SetActive(value: false);
		int num = 0;
		int num2 = enemiesUI.Count((InitiativeTrackEnemyBehaviourExtension enemyUI) => enemyUI.gameObject.activeSelf);
		foreach (InitiativeTrackEnemyBehaviourExtension item in enemiesUI.Where((InitiativeTrackEnemyBehaviourExtension enemyUI) => enemyUI.gameObject.activeSelf))
		{
			StopAllCoroutines();
			item.FinishCardHighlightAnimation(num, num2, () =>
			{
				Choreographer.s_Choreographer.readyButton.AlternativeAction(PostEnemyCardAnimationProceed, ReadyButton.EButtonState.EREADYBUTTONCONTINUE, LocalizationManager.GetTranslation("GUI_CONTINUE"));
			});
			num++;
		}
		StopUpdateActors();
		PostEnemyCardAnimationProceed();
	}

	private void PostEnemyCardAnimationProceed()
	{
		foreach (InitiativeTrackEnemyBehaviourExtension item in enemiesUI)
		{
			item.Base.Deselect();
		}
		Choreographer.s_Choreographer.Pass();
	}

	public InitiativeTrackActorBehaviourBase SelectedActor()
	{
		return selectedActor;
	}

	public InitiativeTrackActorBehaviourBase FindInitiativeTrackActor(CActor actor)
	{
		foreach (InitiativeTrackActorBehaviourBase item in actorsUI)
		{
			if (item.Actor == actor)
			{
				return item;
			}
		}
		return null;
	}

	public InitiativeTrackActorBehaviourBase FindInitiativeTrackActor(InitiativeTrackActorAvatarBase avatar)
	{
		foreach (InitiativeTrackActorBehaviourBase item in actorsUI)
		{
			if (item.Avatar == avatar)
			{
				return item;
			}
		}
		return null;
	}

	private void NormalizeActorsPool(List<CActor> actors)
	{
		actorsUI.Clear();
		int playerIndex = 0;
		int enemyIndex = 0;
		int summonIndex = 0;
		foreach (CActor actor in actors)
		{
			if (actor.IsPlayerByDefault())
			{
				normalizeBehaviourPool<InitiativeTrackPlayerBehaviourExtension>(ref playerIndex, ref playersUI, playerBehaviourPrefab);
			}
			else if (actor.IsEnemyByDefault() && !actor.IsPropActorByDefault(checkAttachedToProp: false))
			{
				normalizeBehaviourPool<InitiativeTrackEnemyBehaviourExtension>(ref enemyIndex, ref enemiesUI, enemyBehaviourPrefab, (InitiativeTrackEnemyBehaviourExtension ui) =>
				{
					ui.SetCardHolder(enemyCardsHolder);
				});
			}
			else if (actor.IsHeroSummonByDefault())
			{
				normalizeBehaviourPool<InitiativeTrackSummonBehaviourExtension>(ref summonIndex, ref summonsUI, summonBehaviourPrefab);
			}
		}
		hideDeadPortraits<InitiativeTrackPlayerBehaviourExtension>(playerIndex, playersUI);
		hideDeadPortraits<InitiativeTrackEnemyBehaviourExtension>(enemyIndex, enemiesUI);
		hideDeadPortraits<InitiativeTrackSummonBehaviourExtension>(summonIndex, summonsUI);
		foreach (InitiativeTrackActorBehaviourBase item in actorsUI)
		{
			item.gameObject.SetActive(value: false);
		}
		static void hideDeadPortraits<T>(int index, List<T> targetList) where T : InitiativeTrackActorBehaviourExtension
		{
			while (index < targetList.Count)
			{
				targetList[index].gameObject.SetActive(value: false);
				index++;
			}
		}
		void normalizeBehaviourPool<T>(ref int index, ref List<T> targetList, GameObject behaviourPrefab, Action<T> onCreateAction = null) where T : InitiativeTrackActorBehaviourExtension
		{
			if (index >= targetList.Count)
			{
				HelperTools.NormalizePool(ref targetList, behaviourPrefab, initiativeTrackHolder, targetList.Count + 1, onCreateAction);
			}
			actorsUI.Add(targetList[index].Base);
			targetList[index].transform.SetSiblingIndex(playerIndex + summonIndex + enemyIndex);
			index++;
		}
	}

	public static bool HasAvatar(CActor actor)
	{
		if (!actor.IsDummyEnemyByDefault() && !actor.IsPropActorByDefault(checkAttachedToProp: false) && (!actor.IsHeroSummonByDefault() || actor.Health > 0))
		{
			return ScenarioManager.Scenario.HasActor(actor);
		}
		return false;
	}

	public void UpdateSelectable(bool playersSelectable, bool enemiesSelectable, TargetingParams targetingParams)
	{
		foreach (InitiativeTrackActorBehaviourBase item in actorsUI)
		{
			CActor actor = item.Actor;
			bool isActiveButton = (playersSelectable && item.Actor.IsPlayerByDefault()) || (enemiesSelectable && item.Actor.IsEnemyByDefault());
			item.SetAttributes(actor, isActiveButton, targetingParams, item.Class);
		}
	}

	private void UpdateSortingOrder()
	{
		if (PhaseManager.IsInCardsSelectionPhase)
		{
			List<InitiativeTrackActorBehaviourBase> list = (from pui in playersUI
				select pui.Base into ui
				where !ui.Actor.IsDead
				select ui).ToList();
			for (int num = list.Count - 1; num >= 0; num--)
			{
				InitiativeTrackActorBehaviourBase playerUi = list[num];
				List<InitiativeTrackSummonBehaviourExtension> list2 = summonsUI.Where((InitiativeTrackSummonBehaviourExtension ui) => ui.SummonActor.Health > 0 && ui.SummonActor.Summoner == playerUi.Actor).ToList();
				if (list2.Count > 1)
				{
					list2.Sort((InitiativeTrackSummonBehaviourExtension x, InitiativeTrackSummonBehaviourExtension y) => GameState.s_ActorInitiativeComparer.Compare(x.SummonActor, y.SummonActor));
					list2.Reverse();
				}
				moveToFront(playerUi);
				foreach (InitiativeTrackSummonBehaviourExtension item in list2)
				{
					moveToFront(item.Base);
				}
			}
		}
		else
		{
			actorsUI.Sort((InitiativeTrackActorBehaviourBase x, InitiativeTrackActorBehaviourBase y) =>
			{
				CClass xClass = x.Class ?? x.Actor?.Class;
				CClass yClass = y.Class ?? y.Actor?.Class;
				return GameState.s_ActorInitiativeComparer.Compare(x.Actor, y.Actor, xClass, yClass);
			});
		}
		foreach (InitiativeTrackActorBehaviourBase item2 in actorsUI)
		{
			item2.transform.SetAsLastSibling();
		}
		void moveToFront(InitiativeTrackActorBehaviourBase actorUi)
		{
			if (actorsUI.Remove(actorUi))
			{
				actorsUI.Insert(0, actorUi);
			}
		}
	}

	private void UpdateSortingOrderByPlayerType()
	{
		actorsUI.Sort(PlayerTypeComparer);
	}

	private bool TryGetActorBehaviourBase(CActor actor, out InitiativeTrackActorBehaviourBase behaviourBase)
	{
		behaviourBase = actorsUI.FirstOrDefault((InitiativeTrackActorBehaviourBase a) => a.Actor == actor || (a.Actor == null && a.Class == actor.Class));
		return behaviourBase != null;
	}

	private void UpdateInitiativeTrack(List<CActor> sortedActors, TargetingParams targetingParams, bool playersSelectable, bool enemiesSelectable, bool selectActor)
	{
		List<(CActor, CClass)> list = new List<(CActor, CClass)>();
		foreach (CActor sortedActor in sortedActors)
		{
			if (HasAvatar(sortedActor))
			{
				list.Add((sortedActor, sortedActor.Class));
			}
		}
		foreach (KeyValuePair<string, List<CEnemyActor>> classActors in GetExtraMonsterClasses())
		{
			CMonsterClass item = MonsterClassManager.MonsterAndObjectClassesWithInitiative.FirstOrDefault((CMonsterClass c) => c.ID == classActors.Key);
			foreach (CEnemyActor item2 in classActors.Value)
			{
				list.Add((item2, item));
			}
		}
		List<CActor> list2 = list.Select(((CActor, CClass) pair) => pair.Item1).ToList();
		NormalizeActorsPool(list2);
		int num = 0;
		int count = list.Count;
		float scaleFactor = m_DefaultScaling.ScaleFactor;
		foreach (AvatarScalingRule avatarScalingRule in m_AvatarScalingRules)
		{
			Vector2Int range = avatarScalingRule.Range;
			if (count >= range.x)
			{
				range = avatarScalingRule.Range;
				if (count <= range.y)
				{
					scaleFactor = avatarScalingRule.ScaleFactor;
					break;
				}
			}
		}
		Log.GUIInfo("UpdateInitiativeTrack selectActor " + selectActor + " isOverridingCurrentActor " + GameState.OverridingCurrentActor + " currentActor " + Choreographer.s_Choreographer.m_CurrentActor?.GetPrefabName() + " " + Choreographer.s_Choreographer.m_CurrentActor?.ID + " " + list2.Contains(Choreographer.s_Choreographer.m_CurrentActor));
		foreach (var actorClassPair in list)
		{
			bool isActiveButton = (playersSelectable && actorClassPair.Item1.IsPlayerByDefault()) || (enemiesSelectable && actorClassPair.Item1.IsHeroSummonByDefault()) || (enemiesSelectable && actorClassPair.Item1.IsEnemyByDefault());
			actorsUI[num].gameObject.SetActive(value: true);
			actorsUI[num].SetAttributes(actorClassPair.Item1, isActiveButton, targetingParams, actorClassPair.Item2);
			if (PhaseManager.PhaseType != CPhase.PhaseType.EndRound && selectActor && actorClassPair.Item1 == Choreographer.s_Choreographer.m_CurrentActor && !GameState.OverridingCurrentActor)
			{
				bool flag = true;
				if (GameState.TurnActor == actorClassPair.Item1 && !GameState.TurnOrderSoFar.Contains(actorClassPair))
				{
					flag = false;
				}
				if (flag && ((PhaseManager.PhaseType != CPhase.PhaseType.StartTurn && !PhaseManager.IsInActionSelectionPhase) || !(actorClassPair.Item1 is CHeroSummonActor cHeroSummonActor) || cHeroSummonActor.SummonData.Move != 0 || cHeroSummonActor.SummonData.Attack != 0 || (cHeroSummonActor.SummonData.SpecialAbilities != null && cHeroSummonActor.SummonData.SpecialAbilities.Count != 0)))
				{
					Log.GUIInfo("Select avatar " + actorClassPair.Item1.ID);
					Select(actorsUI[num]);
				}
			}
			actorsUI[num].Avatar.SetBannerScale(scaleFactor);
			bool grayscale = actorClassPair.Item1.PlayedThisRound && GameState.TurnOrderSoFar.Any(((CActor, CClass) x) => x.Item1 == actorClassPair.Item1 && x.Item2 == actorClassPair.Item2) && PhaseManager.PhaseType != CPhase.PhaseType.ShortRest && PhaseManager.PhaseType != CPhase.PhaseType.SelectAbilityCardsOrLongRest && PhaseManager.PhaseType != CPhase.PhaseType.StartRoundEffects && PhaseManager.PhaseType != CPhase.PhaseType.StartScenarioEffects && PhaseManager.PhaseType != CPhase.PhaseType.Autosave && GameState.s_ShortRestingPlayer == null;
			actorsUI[num].Avatar.SetGrayscale(grayscale);
			num++;
		}
	}

	private void UpdateInitiativeTrack(List<GameObject> players, List<GameObject> enemies, List<GameObject> summons, TargetingParams targetingParams, bool playersSelectable, bool enemiesSelectable, bool forceSelectFirstActor = false)
	{
		List<CActor> actors = new List<CActor>();
		foreach (GameObject player in players)
		{
			CPlayerActor cPlayerActor = (CPlayerActor)ActorBehaviour.GetActor(player);
			actors.AddRange(from summon in cPlayerActor.GetAllHeroSummons()
				orderby summon.SummonedOrderIndex
				select summon);
			actors.Add(cPlayerActor);
		}
		IEnumerable<CHeroSummonActor> collection = from summonGO in summons
			select (CHeroSummonActor)ActorBehaviour.GetActor(summonGO) into cHeroSummonActor
			where !cHeroSummonActor.IsDead && cHeroSummonActor.Summoner.IsDead && !actors.Contains(cHeroSummonActor)
			select cHeroSummonActor;
		actors.AddRange(collection);
		actors.AddRange(enemies.Select(ActorBehaviour.GetActor));
		UpdateInitiativeTrack(actors, targetingParams, playersSelectable, enemiesSelectable, selectActor: true);
		UpdateSortingOrder();
		if (forceSelectFirstActor && playersUI.Count > 0)
		{
			InitiativeTrackPlayerBehaviourExtension initiativeTrackPlayerBehaviourExtension = playersUI[0];
			if (FFSNetwork.IsOnline)
			{
				foreach (InitiativeTrackPlayerBehaviourExtension item in playersUI)
				{
					if (item.Base.Actor.IsUnderMyControl())
					{
						initiativeTrackPlayerBehaviourExtension = item;
						break;
					}
				}
			}
			Select(initiativeTrackPlayerBehaviourExtension.Base);
		}
		ShowSeparatorActorsByType();
	}

	private void UpdateActorsList()
	{
		foreach (InitiativeTrackActorBehaviourBase item in playersUI.Select((InitiativeTrackPlayerBehaviourExtension p) => p.Base).Concat(summonsUI.Select((InitiativeTrackSummonBehaviourExtension e) => e.Base)).Concat(enemiesUI.Select((InitiativeTrackEnemyBehaviourExtension e) => e.Base)))
		{
			if (!actorsUI.Contains(item))
			{
				actorsUI.Add(item);
			}
		}
		actorsUI.RemoveAll((InitiativeTrackActorBehaviourBase a) => playersUI.All((InitiativeTrackPlayerBehaviourExtension p) => p.Base != a) && summonsUI.All((InitiativeTrackSummonBehaviourExtension e) => e.Base != a) && enemiesUI.All((InitiativeTrackEnemyBehaviourExtension e) => e.Base != a));
	}

	public Dictionary<string, List<CEnemyActor>> GetExtraMonsterClasses()
	{
		Dictionary<string, List<CEnemyActor>> dictionary = new Dictionary<string, List<CEnemyActor>>();
		if (ScenarioManager.CurrentScenarioState?.ScenarioScripts != null)
		{
			foreach (ScenarioScript scenarioScript in ScenarioManager.CurrentScenarioState.ScenarioScripts)
			{
				Dictionary<string, List<CEnemyActor>> extraInitiativeActors = scenarioScript.GetExtraInitiativeActors();
				if (extraInitiativeActors != null)
				{
					dictionary.AddRange(extraInitiativeActors);
				}
			}
		}
		return dictionary;
	}

	public void HideMonsterClassesForSelectingRoundAbilityCards()
	{
		MonsterBaseUI[] componentsInChildren = enemyCardsHolder.GetComponentsInChildren<MonsterBaseUI>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].TogglePreview(active: false);
		}
	}

	public void ShowMonsterClassesForSelectingRoundAbilityCards(List<CEnemyActor> allClientEnemies, bool showNextCard = false, List<CEnemyActor> showSpecificActors = null)
	{
		Dictionary<CMonsterClass, int> dictionary = new Dictionary<CMonsterClass, int>();
		Dictionary<CEnemyActor, int> dictionary2 = new Dictionary<CEnemyActor, int>();
		List<CEnemyActor> list = new List<CEnemyActor>();
		foreach (CEnemyActor allClientEnemy in allClientEnemies)
		{
			if (HasAvatar(allClientEnemy) && (!showNextCard || allClientEnemy.MonsterClass.NextAbilityCard != null))
			{
				list.Add(allClientEnemy);
				if (!dictionary.TryAdd(allClientEnemy.MonsterClass, 1))
				{
					dictionary[allClientEnemy.MonsterClass]++;
				}
				if (allClientEnemy.Summoner != null && !dictionary2.TryAdd(allClientEnemy.Summoner, 1))
				{
					dictionary2[allClientEnemy.Summoner]++;
				}
			}
		}
		Dictionary<string, List<CEnemyActor>> extraMonsterClasses = GetExtraMonsterClasses();
		int newLength = list.Count + extraMonsterClasses.Count;
		HelperTools.NormalizePool(ref enemiesUI, enemyBehaviourPrefab, initiativeTrackHolder, newLength, (InitiativeTrackEnemyBehaviourExtension ui) =>
		{
			ui.SetCardHolder(enemyCardsHolder);
		});
		UpdateActorsList();
		foreach (InitiativeTrackPlayerBehaviourExtension item in playersUI)
		{
			item.Base.Deselect();
			item.Base.SetAttributes(item.Base.Actor, isActiveButton: false, isActing: false, isTargeted: false, isPositiveAbility: false, item.Base.Class);
			if (FFSNetwork.IsOnline)
			{
				item.Base.RefreshInitiative();
			}
		}
		bool isFactionDisplayed = ScenarioManager.Scenario.Enemies.Count > 0 && ScenarioManager.Scenario.Enemy2Monsters.Count > 0;
		int num = 0;
		list.Sort(GameState.s_ActorInitiativeComparer);
		foreach (CEnemyActor item2 in list)
		{
			if (item2 != null)
			{
				int value = 0;
				if (item2 != null)
				{
					dictionary2.TryGetValue(item2, out value);
				}
				ActorAttributesParams attributesDirect = new ActorAttributesParams
				{
					Actor = item2,
					Class = item2.MonsterClass,
					TextInitiative = string.Empty,
					CurrentHealth = (item2?.Health ?? 0),
					MaxHealth = (item2?.MaxHealth ?? 0),
					IsActiveButton = true,
					IsFactionDisplayed = isFactionDisplayed,
					SummonsCount = value,
					PhaseState = ActorPhaseState.None,
					ShowNextCard = showNextCard
				};
				enemiesUI[num].Base.SetAttributesDirect(attributesDirect);
				num++;
			}
		}
		foreach (string extraClass in extraMonsterClasses.Keys)
		{
			CMonsterClass cMonsterClass = MonsterClassManager.MonsterAndObjectClassesWithInitiative.FirstOrDefault((CMonsterClass c) => c.ID == extraClass);
			foreach (CEnemyActor item3 in extraMonsterClasses[extraClass])
			{
				int value2 = 0;
				if (item3 != null)
				{
					dictionary2.TryGetValue(item3, out value2);
				}
				ActorAttributesParams attributesDirect2 = new ActorAttributesParams
				{
					Actor = item3,
					Class = cMonsterClass,
					TextInitiative = string.Empty,
					CurrentHealth = (item3?.Health ?? 0),
					MaxHealth = (item3?.MaxHealth ?? 0),
					IsActiveButton = true,
					IsFactionDisplayed = isFactionDisplayed,
					SummonsCount = value2,
					PhaseState = ActorPhaseState.None,
					ShowNextCard = showNextCard
				};
				enemiesUI[num].Base.SetAttributesDirect(attributesDirect2);
				num++;
			}
			if (extraMonsterClasses[extraClass].Count == 0)
			{
				ActorAttributesParams attributesDirect3 = new ActorAttributesParams
				{
					Actor = null,
					Class = cMonsterClass,
					TextInitiative = string.Empty,
					CurrentHealth = 0,
					MaxHealth = 0,
					IsActiveButton = true,
					IsFactionDisplayed = isFactionDisplayed,
					SummonsCount = 0,
					PhaseState = ActorPhaseState.None,
					ShowNextCard = showNextCard
				};
				enemiesUI[num].Base.SetAttributesDirect(attributesDirect3);
				num++;
			}
		}
		EnemyAbilityCardsAnimation(showSpecificActors);
	}

	public static void EnableEndSelectionButton(bool enable)
	{
		if (FFSNetwork.IsOnline && PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
		{
			if (enable)
			{
				Singleton<UIReadyToggle>.Instance?.SetInteractable(interactable: true);
				return;
			}
			Singleton<UIReadyToggle>.Instance?.SetInteractable(interactable: false);
			Singleton<UIReadyToggle>.Instance?.CancelProgress();
		}
		else
		{
			Choreographer.s_Choreographer.readyButton.SetInteractable(enable);
			Singleton<UIReadyToggle>.Instance?.UpdateAutoEndSelection();
		}
	}

	public void CheckRoundAbilityCardsOrLongRestSelected()
	{
		if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
		{
			if (!Choreographer.s_Choreographer.PlayersInValidStartingPositions || Choreographer.s_Choreographer.AnyPlayersInInvalidStartingPositionsForCompanionSummons)
			{
				Singleton<UIReadyToggle>.Instance?.SetInteractable(interactable: false);
				Singleton<UIReadyToggle>.Instance?.CancelProgress();
				return;
			}
			if (Choreographer.s_Choreographer.HaltMultiplayerProgression)
			{
				if (Singleton<UIResultsManager>.Instance == null || !Singleton<UIResultsManager>.Instance.IsShown)
				{
					EnableEndSelectionButton(enable: false);
				}
				return;
			}
		}
		if (FFSNetwork.IsOnline && Singleton<UIConfirmationBoxManager>.Instance.IsOpen && PhaseManager.IsInCardsSelectionPhase)
		{
			EnableEndSelectionButton(enable: false);
		}
		else if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
		{
			bool flag = ((!FFSNetwork.IsOnline) ? (Singleton<CardsHandManager>.Instance != null && Singleton<CardsHandManager>.Instance.CurrentHand != null) : (PlayerRegistry.MyPlayer != null && PlayerRegistry.MyPlayer.IsParticipant && PlayerRegistry.MyPlayer.MyControllables.Any((NetworkControllable x) => x.IsAlive)));
			if (flag)
			{
				flag = PlayerViewService.AreAllOwnedPlayerCardsHandSelectionReady();
			}
			EnableEndSelectionButton(flag);
		}
		else
		{
			if (PhaseManager.PhaseType != CPhase.PhaseType.ShortRest)
			{
				return;
			}
			bool flag2 = GameState.s_ShortRestingPlayer.IsUnderMyControl();
			CardHandMode? cardHandMode = Singleton<CardsHandManager>.InstanceFast.CurrentHand?.currentMode;
			if (!GameState.ThreadIsSleeping && cardHandMode != CardHandMode.SelectCardsWithTideActiveBonusesToKeep && cardHandMode != CardHandMode.SelectCardsWithMandatoryActiveBonusesToDeactivate)
			{
				if (flag2)
				{
					flag2 = GameState.s_ShortRestingPlayer.CharacterClass.HasShortRested;
				}
				if (flag2)
				{
					flag2 = PlayerViewService.IsPlayerCardsHandSelectionReady(GameState.s_ShortRestingPlayer);
				}
			}
			EnableEndSelectionButton(flag2);
		}
	}

	public void OnActiveBonusTriggered(CActor actor, CActiveBonus activeBonus)
	{
		InitiativeTrackActorBehaviourBase initiativeTrackActorBehaviourBase = actorsUI.FirstOrDefault((InitiativeTrackActorBehaviourBase s) => s.Actor == actor && ScenarioManager.Scenario.HasActor(s.Actor));
		if (!(initiativeTrackActorBehaviourBase == null))
		{
			initiativeTrackActorBehaviourBase.OnActiveBonusTriggered(activeBonus);
		}
	}

	public void SetActiveBonusInteractable(bool interactable)
	{
		foreach (InitiativeTrackActorBehaviourBase item in actorsUI)
		{
			item.Avatar.IsActiveBonusInteractable = interactable;
		}
	}

	public void RefreshCharactersController()
	{
		actorsUI.ForEach((InitiativeTrackActorBehaviourBase s) =>
		{
			if (s.Extension is InitiativeTrackPlayerBehaviourExtension initiativeTrackPlayerBehaviourExtension && ScenarioManager.Scenario.HasActor(s.Actor))
			{
				initiativeTrackPlayerBehaviourExtension.RefreshPlayerController();
			}
		});
	}

	public void RefreshCharacterController(string characterID)
	{
		InitiativeTrackActorBehaviourBase initiativeTrackActorBehaviourBase = actorsUI.FirstOrDefault((InitiativeTrackActorBehaviourBase s) => s.Extension is InitiativeTrackPlayerBehaviourExtension && ScenarioManager.Scenario.HasActor(s.Actor) && ((CPlayerActor)s.Actor).CharacterClass.CharacterID == characterID);
		if (initiativeTrackActorBehaviourBase != null)
		{
			((InitiativeTrackPlayerBehaviourExtension)initiativeTrackActorBehaviourBase.Extension).RefreshPlayerController();
		}
	}

	public void ShowActorDetails(bool show)
	{
		foreach (InitiativeTrackActorBehaviourBase item in actorsUI)
		{
			item.ShowDetails(show);
		}
	}

	public static void Refresh(bool forceSelectFirstActor)
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			if (Instance == null)
			{
				return;
			}
			using (new MiniProfilerScope("InitiativeTrack.Refresh"))
			{
				if (PhaseManager.IsInCardsSelectionPhase)
				{
					instance.UpdateInitiativeTrack(Choreographer.s_Choreographer.m_ClientPlayers, summons: Choreographer.s_Choreographer.m_ClientHeroSummons, enemies: Choreographer.s_Choreographer.ClientMonsterObjects, targetingParams: TargetingParams.GetFromChoreographerState(), playersSelectable: true, enemiesSelectable: false, forceSelectFirstActor: forceSelectFirstActor);
					return;
				}
				instance.UpdateInitiativeTrack(GameState.InitiativeSortedActors, TargetingParams.GetFromChoreographerState(), playersSelectable: false, enemiesSelectable: false, forceSelectFirstActor);
				instance.UpdateSortingOrder();
			}
		}
	}

	public static void Refresh(CActor actor, bool isActiveButton)
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			if (!(Instance == null))
			{
				InitiativeTrackActorBehaviourBase initiativeTrackActorBehaviourBase = instance.actorsUI.FirstOrDefault((InitiativeTrackActorBehaviourBase s) => s.Actor == actor && ScenarioManager.Scenario.HasActor(s.Actor));
				if (!(initiativeTrackActorBehaviourBase == null))
				{
					initiativeTrackActorBehaviourBase.SetAttributes(actor, isActiveButton, TargetingParams.GetFromChoreographerState());
					initiativeTrackActorBehaviourBase.RefreshAbilities();
				}
			}
		}
	}
}
