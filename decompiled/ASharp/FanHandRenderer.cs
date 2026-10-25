using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using UnityEngine;

public class FanHandRenderer : MonoBehaviour
{
	public enum FanMode
	{
		Empty = 0,
		Hand = 10,
		ActiveCards = 20,
		Graveyard = 30,
		GeminateMelee = 41,
		GeminateRanged = 42
	}

	[Serializable]
	public class FanSettings
	{
		[Header("Fan Settings")]
		[Range(100f, 2000f)]
		public float HorizontalRadius = 1000f;

		[Range(0f, 1000f)]
		public float VerticalRadius = 100f;

		[Range(5f, 90f)]
		public float ArcPerCard = 10f;

		[Range(5f, 90f)]
		public float MaxArc = 75f;

		[Range(0f, 80f)]
		public float HandTogglesMinArc = 30f;

		[Range(0f, 500f)]
		public float HandTogglesFanOffset = 120f;

		[Range(1f, 100f)]
		public float ArcAnimationLerpTiming = 10f;

		public bool AddCardCountToArc;

		[Range(1f, 100f)]
		public float CardRotPercentage = 10f;

		[Range(45f, 135f)]
		public float OffsetAngle = 90f;

		[Range(0f, 0.5f)]
		public float SelectedAngleWeight = 0.2f;

		[Range(1f, 100f)]
		public float SelectedAnimationLerpTiming = 10f;

		[Header("Folded Fan")]
		public float FoldedOffset = -30f;

		[Range(0.5f, 1.5f)]
		public float FoldedScale = 1f;

		[Range(5f, 90f)]
		public float FoldedArc = 30f;

		[Range(1f, 100f)]
		public float FoldedAnimationLerpTiming = 20f;

		[Range(-1000f, 3000f)]
		public float HiddenOffset = -1000f;

		[Header("Selected Cards")]
		[Range(0f, 1f)]
		public float SCNotFocusedAlpha = 0.5f;

		[Range(1f, 100f)]
		public float SCAlphaLerpTiming = 10f;

		[Header("Z Ordering")]
		public ZOrderNoSelectionBehaviour CardZOrderingNoSelection = ZOrderNoSelectionBehaviour.RightOnTop;

		public bool BringHighlightedCardToFront = true;
	}

	public enum ZOrderNoSelectionBehaviour
	{
		LeftOnTop = 1,
		RightOnTop
	}

	public bool DebugDraw;

	[Header("Links")]
	public FanHandCardViewSlot CardSlotPrototype;

	public RectTransform CardsContainer;

	public RectTransform SelectedCardsContainer;

	public RectTransform CardAnchorReferencePoint;

	public RectTransform MouseCaptureArea;

	public RectTransform MouseFanExtendArea;

	public RectTransform[] MouseAreaBlockers;

	[Header("Settings")]
	public FanSettings Settings;

	private readonly List<RectTransform> _currentHandSlotTransforms = new List<RectTransform>();

	private readonly List<RectTransform> _selectedCardsAnchors = new List<RectTransform>();

	private readonly List<FanHandCardViewSlot> _handCardSlots = new List<FanHandCardViewSlot>();

	private readonly SmoothFloat _offsetAngle = new SmoothFloat();

	private readonly SmoothFloat _selectedAngleWeight = new SmoothFloat();

	private readonly SmoothFloat _maxArc = new SmoothFloat();

	private readonly SmoothFloat _scale = new SmoothFloat();

	private readonly SmoothFloat _offsetY = new SmoothFloat();

	private readonly SmoothFloat _selectedAlpha = new SmoothFloat();

	private CardsHandUI _cardsHandUi;

	private int _lastHighlightedIndex;

	private FanMode _fanMode;

	private FanHandCardViewSlot _highlightedSlot;

	public int MaxSelected
	{
		get
		{
			if (!(_cardsHandUi != null))
			{
				return 2;
			}
			return _cardsHandUi.PlayerActor.MaxCardsClampedToHand;
		}
	}

	private bool IsFocused
	{
		get
		{
			if (PhaseManager.IsInCardsSelectionPhase && _cardsHandUi != null)
			{
				return _cardsHandUi.gameObject.activeInHierarchy;
			}
			return false;
		}
	}

	public FanHandCardViewSlot HighlightedSlot
	{
		get
		{
			return _highlightedSlot;
		}
		set
		{
			if (_highlightedSlot != null)
			{
				_highlightedSlot.transform.SetSiblingIndex(_highlightedSlot.VisibleIndex);
			}
			if (value != null && (_cardsHandUi.SelectedCards.Count >= MaxSelected || IsCardSelected(value.AbilityCard)))
			{
				_highlightedSlot = null;
				return;
			}
			_highlightedSlot = value;
			if (_highlightedSlot != null)
			{
				if (Settings.BringHighlightedCardToFront)
				{
					_highlightedSlot.transform.SetAsLastSibling();
				}
				_highlightedSlot.transform.eulerAngles = new Vector3(0f, 0f, 0f);
			}
		}
	}

	private void Start()
	{
		_offsetAngle.ForceSet(Settings.OffsetAngle);
		_maxArc.ForceSet(GetMaxArc(isFanExtended: false));
		_scale.ForceSet(1f);
		_selectedCardsAnchors.AddRange(from e in SelectedCardsContainer.GetComponentsInChildren<FanHandCardViewSlot>(includeInactive: true)
			select (RectTransform)e.transform);
	}

	private void OnEnable()
	{
		_offsetY.ForceSet(Settings.HiddenOffset);
		Update();
	}

	private void Update()
	{
		Vector3 mousePosition = Input.mousePosition;
		Camera worldCamera = CardsContainer.GetComponentInParent<Canvas>().worldCamera;
		bool flag = RectTransformUtility.RectangleContainsScreenPoint(MouseCaptureArea, mousePosition, worldCamera) && MouseAreaBlockers.All((RectTransform rt) => !rt.gameObject.activeInHierarchy);
		bool flag2 = HighlightedSlot != null || (flag && RectTransformUtility.RectangleContainsScreenPoint(MouseFanExtendArea, mousePosition, worldCamera));
		_scale.Value = (flag2 ? 1f : Settings.FoldedScale);
		SmoothFloat offsetY = _offsetY;
		float value;
		if (IsFanHidden())
		{
			value = Settings.HiddenOffset;
		}
		else
		{
			value = (flag2 ? 0f : Settings.FoldedOffset);
		}
		offsetY.Value = value;
		_offsetAngle.Value = Settings.OffsetAngle;
		_selectedAlpha.Value = ((!IsFocused) ? Settings.SCNotFocusedAlpha : 1f);
		RearrangeCards(flag2);
		UpdateAnimation();
	}

	public void DeselectCard()
	{
		if (!(_cardsHandUi == null) && _cardsHandUi.SelectedCards.Count != 0)
		{
			CAbilityCard lastAbilityCard = _cardsHandUi.SelectedCards.Last().AbilityCard;
			FanHandCardViewSlot fanHandCardViewSlot = _handCardSlots.FirstOrDefault((FanHandCardViewSlot slot) => slot.AbilityCard == lastAbilityCard);
			if (fanHandCardViewSlot != null)
			{
				OnSlotClick(fanHandCardViewSlot);
			}
		}
	}

	private void UpdateAnimation()
	{
		_scale.Update(Time.deltaTime * Settings.FoldedAnimationLerpTiming);
		_offsetY.Update(Time.deltaTime * Settings.FoldedAnimationLerpTiming);
		_offsetAngle.Update(Time.deltaTime * Settings.SelectedAnimationLerpTiming);
		_selectedAngleWeight.Update(Time.deltaTime * Settings.SelectedAnimationLerpTiming);
		_maxArc.Update(Time.deltaTime * Settings.ArcAnimationLerpTiming);
		_selectedAlpha.Update(Time.deltaTime * Settings.SCAlphaLerpTiming);
	}

	private bool IsFanHidden()
	{
		if (!IsFocused || _currentHandSlotTransforms.Count == 0 || _cardsHandUi == null || UIManager.Instance.dialogPopup.IsOpen() || Singleton<ESCMenu>.Instance.IsOpen || MouseAreaBlockers.Any((RectTransform rt) => rt.gameObject.activeInHierarchy))
		{
			return true;
		}
		bool flag = _cardsHandUi.SelectedCards.Count >= MaxSelected || _cardsHandUi.PlayerActor.IsLongRestSelected;
		if (_fanMode == FanMode.Hand)
		{
			return flag;
		}
		FanMode fanMode = _fanMode;
		if (fanMode == FanMode.GeminateMelee || fanMode == FanMode.GeminateRanged)
		{
			CPlayerActor.EForm requiredForm = FanHandManager.Geminate_GetCurrentIntendedForm(_cardsHandUi);
			return !IsGeminateAltFormPreviewMode(requiredForm) & flag;
		}
		return false;
	}

	private bool IsCardSelected(CAbilityCard abilityCard)
	{
		return _cardsHandUi.SelectedCards.Any((AbilityCardUI acui) => acui.AbilityCard == abilityCard);
	}

	private bool IsGeminateAltFormPreviewMode(CPlayerActor.EForm requiredForm)
	{
		if (_fanMode != FanMode.GeminateRanged || requiredForm != CPlayerActor.EForm.Melee)
		{
			if (_fanMode == FanMode.GeminateMelee)
			{
				return requiredForm == CPlayerActor.EForm.Range;
			}
			return false;
		}
		return true;
	}

	public void OnSlotHover(FanHandCardViewSlot cardSlot, bool isHovered)
	{
		if (isHovered)
		{
			HighlightedSlot = cardSlot;
		}
		else if (HighlightedSlot == cardSlot)
		{
			HighlightedSlot = null;
		}
	}

	public void OnSlotClick(FanHandCardViewSlot cardSlot)
	{
		if (Singleton<CardsHandManager>.Instance == null || Singleton<CardsHandManager>.Instance.CurrentHand == null || Singleton<CardsHandManager>.Instance.CurrentHand != _cardsHandUi)
		{
			Log.GUIError("Trying to select a card from the fan without a CardsHandManager or with a different hand!");
			return;
		}
		if (IsCardSelected(cardSlot.AbilityCard))
		{
			CPlayerActor.EForm requiredForm = cardSlot.AbilityCard.GetAbilityCardYML.RequiredForm;
			if (IsUsingNonHandFilter(requiredForm))
			{
				return;
			}
		}
		_cardsHandUi.AllCards.First((AbilityCardUI c) => c.AbilityCard == cardSlot.AbilityCard).OnClick();
	}

	private bool IsUsingNonHandFilter()
	{
		return IsUsingNonHandFilter(FanHandManager.Geminate_GetCurrentIntendedForm(_cardsHandUi));
	}

	private bool IsUsingNonHandFilter(CPlayerActor.EForm requiredForm)
	{
		bool flag = _fanMode == FanMode.Hand;
		FanMode fanMode = _fanMode;
		bool flag2 = fanMode == FanMode.GeminateRanged || fanMode == FanMode.GeminateMelee;
		if (!flag && !flag2)
		{
			return true;
		}
		if (flag2 && IsGeminateAltFormPreviewMode(requiredForm))
		{
			return true;
		}
		return false;
	}

	public void Init(CardsHandUI cardsHandUI)
	{
		_cardsHandUi = cardsHandUI;
		name = "FanHandRenderer_" + _cardsHandUi.name;
		_currentHandSlotTransforms.Clear();
		UIHelperUtil.UpdateUIList(_cardsHandUi.AllCards.Count, _handCardSlots, CardSlotPrototype, CardsContainer);
		_lastHighlightedIndex = -1;
		_offsetY.ForceSet(Settings.HiddenOffset);
		if (!(_cardsHandUi == null) && _cardsHandUi.AllCards.Count != 0)
		{
			SyncHandView(cacheAll: true);
			_maxArc.ForceSet(GetMaxArc(isFanExtended: false));
		}
	}

	public void SetFanMode(FanMode fanMode)
	{
		if (_fanMode != fanMode)
		{
			_fanMode = fanMode;
			_offsetY.ForceSet(Settings.HiddenOffset);
			SyncHandView();
		}
	}

	public void SyncHandView(bool cacheAll = false)
	{
		if (_cardsHandUi == null)
		{
			Log.GUIError("No hand set for the fan to render!", this);
			return;
		}
		_currentHandSlotTransforms.Clear();
		CCharacterClass characterClass = _cardsHandUi.PlayerActor.CharacterClass;
		foreach (AbilityCardUI allCard in _cardsHandUi.AllCards)
		{
			CAbilityCard abilityCard = allCard.AbilityCard;
			if (abilityCard == null || allCard.IsLongRest)
			{
				continue;
			}
			FanHandCardViewSlot fanHandCardViewSlot = null;
			if (cacheAll)
			{
				fanHandCardViewSlot = GetOrCreateCardSlot(abilityCard);
			}
			if (_fanMode == FanMode.Empty || !PhaseManager.IsInCardsSelectionPhase || !allCard.gameObject.activeSelf)
			{
				continue;
			}
			bool flag = characterClass.RoundAbilityCards.Contains(abilityCard) || characterClass.RoundAbilityCardsSelectedInCardSelection.Contains(abilityCard);
			bool flag2 = flag;
			if (_fanMode == FanMode.ActiveCards)
			{
				flag2 |= characterClass.ActivatedAbilityCards.Contains(abilityCard);
			}
			else if (_fanMode == FanMode.Graveyard)
			{
				flag2 |= characterClass.DiscardedAbilityCards.Contains(abilityCard) || characterClass.LostAbilityCards.Contains(abilityCard) || characterClass.PermanentlyLostAbilityCards.Contains(abilityCard);
			}
			else
			{
				FanMode fanMode = _fanMode;
				if (fanMode == FanMode.Hand || fanMode == FanMode.GeminateMelee || fanMode == FanMode.GeminateRanged)
				{
					flag2 |= characterClass.HandAbilityCards.Contains(abilityCard) || characterClass.ExtraTurnCardsSelectedInCardSelection.Contains(abilityCard) || characterClass.GivenCards.Contains(abilityCard);
					if (_fanMode == FanMode.GeminateMelee)
					{
						flag2 &= flag || abilityCard.GetAbilityCardYML.RequiredForm == CPlayerActor.EForm.Melee;
					}
					else if (_fanMode == FanMode.GeminateRanged)
					{
						flag2 &= flag || abilityCard.GetAbilityCardYML.RequiredForm == CPlayerActor.EForm.Range;
					}
				}
			}
			if (flag2)
			{
				if (fanHandCardViewSlot == null)
				{
					fanHandCardViewSlot = GetOrCreateCardSlot(abilityCard);
				}
				fanHandCardViewSlot.SetCardPileType(allCard.CardType);
				fanHandCardViewSlot.VisibleIndex = _currentHandSlotTransforms.Count;
				_currentHandSlotTransforms.Add((RectTransform)fanHandCardViewSlot.transform);
			}
		}
		foreach (FanHandCardViewSlot handCardSlot in _handCardSlots)
		{
			bool flag3 = _currentHandSlotTransforms.Contains(handCardSlot.transform);
			handCardSlot.gameObject.SetActive(flag3);
			if (!flag3)
			{
				handCardSlot.VisibleIndex = -1;
			}
		}
	}

	private FanHandCardViewSlot GetOrCreateCardSlot(CAbilityCard abilityCard)
	{
		FanHandCardViewSlot fanHandCardViewSlot = _handCardSlots.FirstOrDefault((FanHandCardViewSlot c) => c.AbilityCard == abilityCard);
		if (fanHandCardViewSlot != null)
		{
			return fanHandCardViewSlot;
		}
		fanHandCardViewSlot = _handCardSlots.FirstOrDefault((FanHandCardViewSlot c) => c.AbilityCard == null);
		bool flag = false;
		if (fanHandCardViewSlot == null)
		{
			fanHandCardViewSlot = UnityEngine.Object.Instantiate(CardSlotPrototype, CardsContainer).GetComponent<FanHandCardViewSlot>();
			flag = true;
		}
		fanHandCardViewSlot.GetComponent<RectTransform>().localPosition = GetCurrentCenter();
		fanHandCardViewSlot.Init(abilityCard.ID, OnSlotHover, OnSlotClick);
		fanHandCardViewSlot.HoverMoveVisuals.ReferencePoint = CardAnchorReferencePoint;
		fanHandCardViewSlot.HoverMoveVisuals.UseReferenceY = true;
		if (flag)
		{
			_handCardSlots.Add(fanHandCardViewSlot);
		}
		return fanHandCardViewSlot;
	}

	public void Clear()
	{
		Init(null);
	}

	private void RearrangeCards(bool isFanExtended)
	{
		Vector2 currentCenter = GetCurrentCenter();
		Vector2 pivot = CardsContainer.pivot;
		int num = ((_cardsHandUi != null) ? _cardsHandUi.SelectedCards.Count : 0);
		int fanCount = _currentHandSlotTransforms.Count - num;
		if (HighlightedSlot != null)
		{
			_selectedAngleWeight.Value = Settings.SelectedAngleWeight;
		}
		else
		{
			_lastHighlightedIndex = -1;
			_selectedAngleWeight.Value = 0f;
		}
		_maxArc.Value = GetMaxArc(isFanExtended);
		float num2 = Mathf.Min(_maxArc.Value, Settings.MaxArc) / 2f;
		bool isFocused;
		bool isInteractable = (isFocused = IsFocused) & isFanExtended;
		bool flag = _cardsHandUi != null && _cardsHandUi.SelectedCards.Count < MaxSelected;
		bool moveScaleOnClick = _fanMode == FanMode.ActiveCards || (!flag && !IsUsingNonHandFilter());
		int num3 = 0;
		foreach (RectTransform currentHandSlotTransform in _currentHandSlotTransforms)
		{
			FanHandCardViewSlot component = currentHandSlotTransform.GetComponent<FanHandCardViewSlot>();
			if (IsCardSelected(component.AbilityCard))
			{
				component.SetInteractable(isFocused, isFocused, _selectedAlpha.Value);
				currentHandSlotTransform.eulerAngles = Vector3.zero;
				currentHandSlotTransform.localScale = Vector3.one;
				int num4 = _cardsHandUi.PlayerActor.CharacterClass.RoundAbilityCards.IndexOf(component.AbilityCard);
				if (num4 >= 0 && num4 < _selectedCardsAnchors.Count)
				{
					Vector2 vector = CardsContainer.InverseTransformPoint(_selectedCardsAnchors[num4].position);
					if (Vector2.Distance(currentHandSlotTransform.anchoredPosition, vector) <= 0.1f)
					{
						currentHandSlotTransform.anchoredPosition = vector;
					}
					else
					{
						anchorLerp(currentHandSlotTransform, vector);
					}
					component.HoverMoveVisuals.SetLock(UIHoverMoveEffects.LockType.InitialPosition);
				}
				continue;
			}
			component.SetInteractable(isInteractable, blocksRaycasts: true, 1f, moveScaleOnClick);
			currentHandSlotTransform.anchorMin = pivot;
			currentHandSlotTransform.anchorMax = pivot;
			float num5 = Mathf.LerpAngle(num2, 0f - num2, getAngleLerpValue(num3));
			float x = Settings.HorizontalRadius * Mathf.Cos((num5 + _offsetAngle.Value) * (MathF.PI / 180f));
			float y = Settings.VerticalRadius * Mathf.Sin((num5 + _offsetAngle.Value) * (MathF.PI / 180f));
			Vector2 vector2 = new Vector2(x, y);
			Vector2 vector3 = currentCenter + vector2;
			Vector2 zero = Vector2.zero;
			anchorLerp(currentHandSlotTransform, vector3 + zero);
			currentHandSlotTransform.localScale = _scale.Value * Vector3.one;
			if (component == HighlightedSlot)
			{
				currentHandSlotTransform.eulerAngles = Vector3.zero;
				_lastHighlightedIndex = num3;
			}
			else
			{
				float num6 = Mathf.Atan2(vector2.y, vector2.x) * 57.29578f - 90f;
				num6 *= Settings.CardRotPercentage / 100f;
				currentHandSlotTransform.eulerAngles = new Vector3(0f, 0f, num6);
			}
			component.HoverMoveVisuals.SetLock(UIHoverMoveEffects.LockType.None);
			num3++;
		}
		UpdateCardSortOrder();
		if (!DebugDraw)
		{
			return;
		}
		Vector2 vector4 = Vector2.negativeInfinity;
		CardsContainer.DrawDebugLine(currentCenter - new Vector2(10f, 10f), currentCenter + new Vector2(10f, 10f));
		CardsContainer.DrawDebugLine(currentCenter - new Vector2(10f, -10f), currentCenter + new Vector2(10f, -10f));
		for (int i = 0; i <= 360; i += 5)
		{
			float x2 = Settings.HorizontalRadius * Mathf.Cos((float)i * (MathF.PI / 180f));
			float y2 = Settings.VerticalRadius * Mathf.Sin((float)i * (MathF.PI / 180f));
			Vector2 vector5 = currentCenter + new Vector2(x2, y2);
			if (vector4 == Vector2.negativeInfinity)
			{
				vector4 = vector5;
				continue;
			}
			CardsContainer.DrawDebugLine(vector4, vector5);
			vector4 = vector5;
		}
		foreach (RectTransform currentHandSlotTransform2 in _currentHandSlotTransforms)
		{
			CardsContainer.DrawDebugLine(currentHandSlotTransform2.anchoredPosition, currentCenter);
		}
		void anchorLerp(RectTransform rt, Vector2 targetPos)
		{
			rt.anchoredPosition = Vector2.Lerp(rt.anchoredPosition, targetPos, Time.deltaTime * Settings.SelectedAnimationLerpTiming);
		}
		float getAngleLerpValue(int num8)
		{
			float num7 = fanCount - 1;
			if (num7 <= 0f)
			{
				return 0.5f;
			}
			if (_lastHighlightedIndex < 0 || num8 == _lastHighlightedIndex)
			{
				return (float)num8 / num7;
			}
			if (num8 < _lastHighlightedIndex)
			{
				float num9 = (float)num8 * _selectedAngleWeight.Value;
				return ((float)num8 - num9) / num7;
			}
			float num10 = (num7 - (float)num8) * _selectedAngleWeight.Value;
			return ((float)num8 + num10) / num7;
		}
	}

	public float GetFanAnchoredOffset()
	{
		bool flag = IsFanHidden();
		float num = (flag ? 0f : _currentHandSlotTransforms[0].sizeDelta.x);
		Vector2 currentCenter = GetCurrentCenter();
		float num2 = (flag ? Settings.HandTogglesMinArc : Mathf.Max(Settings.HandTogglesMinArc, Mathf.Min(_maxArc.Value, Settings.MaxArc)));
		num2 *= -0.5f;
		float x = Settings.HorizontalRadius * Mathf.Cos((num2 + _offsetAngle.Value) * (MathF.PI / 180f));
		float y = Settings.VerticalRadius * Mathf.Sin((num2 + _offsetAngle.Value) * (MathF.PI / 180f));
		Vector2 vector = new Vector2(x, y);
		return (currentCenter + vector).x + num + Settings.HandTogglesFanOffset;
	}

	private Vector2 GetCurrentCenter()
	{
		return new Vector2(0f, _offsetY.Value - Settings.VerticalRadius);
	}

	private void UpdateCardSortOrder()
	{
		if (_currentHandSlotTransforms.Count < 2)
		{
			return;
		}
		int num = ((!(HighlightedSlot != null) || IsCardSelected(HighlightedSlot.AbilityCard)) ? (Settings.CardZOrderingNoSelection switch
		{
			ZOrderNoSelectionBehaviour.LeftOnTop => 0, 
			ZOrderNoSelectionBehaviour.RightOnTop => _currentHandSlotTransforms.Count - 1, 
			_ => throw new ArgumentOutOfRangeException(), 
		}) : HighlightedSlot.VisibleIndex);
		if (num >= 0)
		{
			for (int i = 0; i < num; i++)
			{
				_currentHandSlotTransforms[i].SetAsLastSibling();
			}
			for (int j = num + 1; j < _currentHandSlotTransforms.Count; j++)
			{
				_currentHandSlotTransforms[j].SetAsLastSibling();
			}
			if (Settings.BringHighlightedCardToFront)
			{
				_currentHandSlotTransforms[num].SetAsLastSibling();
			}
		}
	}

	private float GetMaxArc(bool isFanExtended)
	{
		int count = _currentHandSlotTransforms.Count;
		float num = Settings.ArcPerCard * (float)count;
		if (Settings.AddCardCountToArc)
		{
			num += (float)count;
		}
		if (HighlightedSlot != null)
		{
			num *= 1f + Settings.SelectedAngleWeight;
		}
		if (!isFanExtended)
		{
			return Mathf.Min(num * (Settings.FoldedArc / Settings.MaxArc), Settings.FoldedArc);
		}
		return num;
	}

	public void RecycleCards()
	{
		foreach (FanHandCardViewSlot handCardSlot in _handCardSlots)
		{
			handCardSlot.Clear();
		}
	}

	private void OnDestroy()
	{
		RecycleCards();
	}
}
