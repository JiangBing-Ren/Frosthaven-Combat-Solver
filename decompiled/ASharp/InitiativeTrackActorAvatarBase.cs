using System;
using System.Collections.Generic;
using FHGUI.Common;
using GLOOM;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InitiativeTrackActorAvatarBase : MonoBehaviour, IGeneralInfoProvider
{
	[Header("Settings")]
	[SerializeField]
	private bool m_UsePrefabForPortraitLookup;

	[SerializeField]
	private float _selectedScale = 1.2f;

	[Header("Events")]
	[SerializeField]
	private UIActorEventsChannel _actorEventsChannel;

	[Header("Base Elements")]
	[SerializeField]
	private LayoutElement m_SlotLayoutElement;

	[SerializeField]
	private Button m_Button;

	[SerializeField]
	private Image m_AvatarImage;

	[SerializeField]
	private Image m_SelectionImage;

	[SerializeField]
	private Image m_SelectionUnderlayImage;

	[SerializeField]
	private Image m_HighlightImage;

	[SerializeField]
	private Image m_HighlightUnderlayImage;

	[SerializeField]
	private Image m_HealthOverlayImage;

	[SerializeField]
	private Image m_HealthOverlayLineImage;

	[SerializeField]
	private Image m_ActingIndicatorImage;

	[SerializeField]
	private Image m_ActingUnderlayImage;

	[SerializeField]
	private Image m_TargetedIndicatorImage;

	[SerializeField]
	private Image m_TargetedUnderlayImage;

	[SerializeField]
	private Image m_TargetedByPositiveIndicatorImage;

	[SerializeField]
	private Image m_TargetedByPositiveUnderlayImage;

	[SerializeField]
	private TextMeshProUGUI m_NameText;

	[SerializeField]
	private Material m_GrayscaleMaterial;

	[SerializeField]
	private Material m_RegularMaterial;

	[SerializeField]
	private PersistentAbilitiesUI m_PersistentAbilities;

	[Header("Overlay Elements")]
	[SerializeField]
	private string m_HealthSpritePrefix = "<sprite name=\"Health\" tint>";

	[SerializeField]
	private TMP_Text m_HpText;

	[SerializeField]
	public TextMeshProUGUI m_InitiativeText;

	[Space]
	[SerializeField]
	private Transform m_PhaseStateHolder;

	[SerializeField]
	private Image m_PhaseIcon;

	[SerializeField]
	private Sprite m_PhaseCardsSelectionSprite;

	[SerializeField]
	private Sprite m_PhaseRestingSprite;

	private CActor _actor;

	private Action<bool> _onAvatarHighlightAction;

	private bool _isHighlighted;

	private readonly Dictionary<RectMask2D, Vector4> _startingMaskPaddings = new Dictionary<RectMask2D, Vector4>();

	private Vector2 _startingPreferredDimensions;

	private float _scale = 1f;

	[field: SerializeField]
	public InitiativeTrackActorAvatarExtension Extension { get; private set; }

	public bool IsSelected { get; private set; }

	public bool IsActing { get; private set; }

	public bool IsTargeted { get; private set; }

	public Button Button => m_Button;

	public virtual bool IsActiveBonusInteractable
	{
		get
		{
			return m_PersistentAbilities.IsInteractable;
		}
		set
		{
			m_PersistentAbilities.IsInteractable = value;
		}
	}

	private static void SetImageEnabledState(bool isEnabled, params Image[] images)
	{
		for (int i = 0; i < images.Length; i++)
		{
			images[i].enabled = isEnabled;
		}
	}

	private void Awake()
	{
		_startingPreferredDimensions = new Vector2(m_SlotLayoutElement.preferredWidth, m_SlotLayoutElement.preferredHeight);
		bool active = _actor?.IsTakingExtraTurn ?? false;
		ToggleSelection(active);
		SetImageEnabledState(_isHighlighted, m_HighlightImage, m_HighlightUnderlayImage);
		RectMask2D[] componentsInChildren = GetComponentsInChildren<RectMask2D>(includeInactive: true);
		foreach (RectMask2D rectMask2D in componentsInChildren)
		{
			_startingMaskPaddings.Add(rectMask2D, rectMask2D.padding);
		}
	}

	public void OnPointerEnter()
	{
		AutoTestController.LogButtonHover(gameObject, this);
		SetHighlighted(highlight: true);
		UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.InitiativeAvatarHovered, null, _actor?.GetPrefabName()));
	}

	public void OnPointerExit()
	{
		SetHighlighted(highlight: false);
	}

	public void Init(Action<bool> onHighlightCallback, CActor actor, bool interactable)
	{
		_actor = actor;
		_onAvatarHighlightAction = onHighlightCallback;
		OnShowDetails(active: false);
		SetHighlighted(highlight: false);
	}

	public void SetHighlighted(bool highlight, bool raiseEvent = true)
	{
		_isHighlighted = highlight;
		_onAvatarHighlightAction(highlight);
		SetImageEnabledState(highlight, m_HighlightImage, m_HighlightUnderlayImage);
		if (raiseEvent)
		{
			_actorEventsChannel.ActorHighlighted(this, _actor, highlight, showCards: true);
		}
	}

	public void SetGrayscale(bool active)
	{
		m_AvatarImage.material = (active ? m_GrayscaleMaterial : m_RegularMaterial);
	}

	public void SetBannerScale(float scale)
	{
		_scale = scale;
		UpdateBannerScale();
	}

	private void UpdateBannerScale()
	{
		float scale = _scale;
		scale *= (IsSelected ? _selectedScale : 1f);
		m_SlotLayoutElement.preferredWidth = scale * _startingPreferredDimensions.x;
		m_SlotLayoutElement.preferredHeight = scale * _startingPreferredDimensions.y;
		foreach (KeyValuePair<RectMask2D, Vector4> startingMaskPadding in _startingMaskPaddings)
		{
			startingMaskPadding.Key.padding = scale * startingMaskPadding.Value;
		}
	}

	private void ToggleSelection(bool active)
	{
		SetImageEnabledState(active && !_actor.IsTakingExtraTurn, m_SelectionImage, m_SelectionUnderlayImage);
		UpdateBannerScale();
	}

	public void OnSelect()
	{
		IsSelected = true;
		ToggleSelection(active: true);
		Extension.OnSelect(_actor);
	}

	public void OnDeselect()
	{
		IsSelected = false;
		ToggleSelection(active: false);
	}

	public void OnClick(InitiativeTrackActorBehaviourBase actorUI)
	{
		if (!Extension.IsSelectableByClick)
		{
			InitiativeTrack.Instance.SetFocusedActor(actorUI, showHighlight: true, focusCamera: true);
			return;
		}
		Log.GUIInfo("Select by click");
		InitiativeTrack.Instance.Select(actorUI);
	}

	public void OnShowDetails(bool active)
	{
		m_NameText.enabled = active;
	}

	public void OnRefreshInitiative(CClass mClass)
	{
		if (_actor != null)
		{
			m_InitiativeText.text = ((mClass is CMonsterClass mClass2) ? Extension.GetInitiativeText(mClass2, _actor as CEnemyActor) : Extension.GetInitiativeText(_actor));
			UpdatePhaseStateDisplay(Extension.GetPhaseState(_actor));
		}
	}

	public void OnSetAttributesDirect(InitiativeTrack.ActorAttributesParams aaParams)
	{
		Extension.PreSetAttributesDirect(aaParams);
		_actor = aaParams.Actor;
		IsActing = aaParams.IsActing;
		IsTargeted = aaParams.IsTargeted;
		bool flag = _actor != null;
		bool isEnabled = IsTargeted && aaParams.IsTargetedByPositiveAbility;
		bool isEnabled2 = IsTargeted && !aaParams.IsTargetedByPositiveAbility;
		string term = (flag ? _actor.ActorLocKey() : aaParams.Class.LocKey);
		m_NameText.text = LocalizationManager.GetTranslation(term);
		CClass cClass = _actor?.Class ?? aaParams.Class;
		string actorModel = (flag ? _actor.Class.Models[_actor.ChosenModelIndex] : cClass.DefaultModel);
		string iD = cClass.ID;
		Sprite portrait = GetPortrait(cClass, iD, actorModel);
		m_AvatarImage.sprite = portrait;
		m_AvatarImage.enabled = portrait != null;
		SetImageEnabledState(IsActing, m_ActingIndicatorImage, m_ActingUnderlayImage);
		SetImageEnabledState(isEnabled2, m_TargetedIndicatorImage, m_TargetedUnderlayImage);
		SetImageEnabledState(isEnabled, m_TargetedByPositiveIndicatorImage, m_TargetedByPositiveUnderlayImage);
		m_Button.interactable = true;
		float num = ((aaParams.MaxHealth > 0) ? ((float)(aaParams.MaxHealth - aaParams.CurrentHealth) / (float)aaParams.MaxHealth) : 0f);
		m_HpText.text = $"{m_HealthSpritePrefix}{aaParams.CurrentHealth}";
		m_HealthOverlayImage.fillAmount = num;
		m_HealthOverlayLineImage.rectTransform.anchorMax = new Vector2(1f, num);
		m_InitiativeText.text = aaParams.TextInitiative;
		UpdatePhaseStateDisplay(aaParams.PhaseState);
		Extension.PostSetAttributesDirect(aaParams);
	}

	private Sprite GetPortrait(CClass cClass, string actorId, string actorModel)
	{
		if (m_UsePrefabForPortraitLookup && _actor != null)
		{
			return UIInfoTools.Instance.GetActorPortrait(_actor.GetPrefabName());
		}
		if (!(cClass is CCharacterClass))
		{
			bool isFromConfig;
			if (!(cClass is CHeroSummonClass))
			{
				if (cClass is CMonsterClass)
				{
					return UIInfoTools.GetMonsterPortrait(actorId, actorModel, out isFromConfig);
				}
				throw new ArgumentOutOfRangeException("cClass");
			}
			return UIInfoTools.GetSummonPortrait(actorId, actorModel, out isFromConfig);
		}
		return UIInfoTools.GetHeroPortrait(actorModel);
	}

	public void OnActiveBonusTriggered(CActiveBonus activeBonus)
	{
	}

	public void OnRefreshAbilities()
	{
	}

	public void RefreshActiveInteractable()
	{
	}

	private void UpdatePhaseStateDisplay(InitiativeTrack.ActorPhaseState phaseState)
	{
		bool flag = _actor?.IsUnderMyControl() ?? false;
		m_PhaseStateHolder.gameObject.SetActive(!flag && phaseState != InitiativeTrack.ActorPhaseState.None);
		Image phaseIcon = m_PhaseIcon;
		phaseIcon.sprite = phaseState switch
		{
			InitiativeTrack.ActorPhaseState.None => null, 
			InitiativeTrack.ActorPhaseState.CardSelection => m_PhaseCardsSelectionSprite, 
			InitiativeTrack.ActorPhaseState.Resting => m_PhaseRestingSprite, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public IGeneralInfoData GetInfoData()
	{
		if (_actor != null)
		{
			return UIGeneralInfoUtils.GetActor(_actor);
		}
		return null;
	}
}
