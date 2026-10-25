using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI;
using FHGUI.Common;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Modules;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIPartySlot : UIBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IGeneralInfoProvider
{
	public delegate void SlotEventHandler(UIPartySlot slot);

	public delegate IEnumerable<UIPartySlotHints.HintedActionParams> SlotToHintedActions(PartySlotDTO partySlot);

	[Header("Events")]
	[FormerlySerializedAs("_characterDisplayEventsChannel")]
	[SerializeField]
	protected UI3DCharacterDisplayEventsChannel m_CharacterDisplayEventsChannel;

	[Header("Empty State")]
	[SerializeField]
	protected Image m_EmptySlotImage;

	[Space(8f)]
	[SerializeField]
	protected Image m_EmptyStateBackgroundImage;

	[SerializeField]
	protected Image m_EmptyStateFrameImage;

	[SerializeField]
	protected Image[] m_EmptyStateSelectImage;

	[SerializeField]
	protected Image[] m_EmptyStateHighlightImage;

	[Header("Filled State")]
	[SerializeField]
	protected Image m_BackgroundImage;

	[SerializeField]
	protected Image m_FrameImage;

	[SerializeField]
	protected Image[] m_SelectionImage;

	[SerializeField]
	protected Image[] m_HighlightImage;

	[SerializeField]
	protected TMP_Text m_PartyStatus;

	[Header("Base Elements")]
	[SerializeField]
	protected Image m_NotificationBadgeImage;

	[SerializeField]
	protected Image m_LockedSlotImage;

	[Header("Character info")]
	[SerializeField]
	protected TextMeshProUGUI m_NameText;

	[SerializeField]
	protected Image m_PortraitImage;

	[SerializeField]
	protected Image m_IllustrationImage;

	[SerializeField]
	protected Image m_InfoIllustrationImage;

	[SerializeField]
	protected TMP_Text m_GoldText;

	[SerializeField]
	protected TMP_Text m_LevelText;

	[FormerlySerializedAs("m_XP")]
	[FormerlySerializedAs("_xp")]
	[SerializeField]
	protected TMP_Text m_XPText;

	[SerializeField]
	protected Image _xpBarFill;

	[SerializeField]
	protected Image m_ClassIcon;

	[SerializeField]
	protected Image m_ClassColor;

	[SerializeField]
	protected GameObject m_InPartyIndicator;

	[SerializeField]
	protected Image m_SlotFrame;

	[SerializeField]
	protected Image m_SlotInnerFrame;

	[Header("Multiplayer")]
	[SerializeField]
	protected MultiplayerUserState m_MultiplayerUserState;

	[SerializeField]
	protected MultiplayerVisualsUIDataSO m_MultiplayerVisualsSO;

	[SerializeField]
	protected GameObject m_VoteReady;

	[SerializeField]
	protected GameObject m_VoteWaiting;

	[Header("Interaction Hints")]
	[SerializeField]
	protected UIPartySlotHints m_InteractionHints;

	[SerializeField]
	protected FHButton m_Button;

	[SerializeField]
	protected FHButton m_DeleteHero;

	[SerializeField]
	protected FHButton m_LevelUpHero;

	[Header("Highlighted States")]
	[SerializeField]
	protected GameObject m_CanRetireIndication;

	[SerializeField]
	protected GameObject m_RetiredIndication;

	[Header("Settings")]
	[SerializeField]
	protected Color m_InteractableColor = Color.white;

	[SerializeField]
	protected Color m_NotInteractableColor = Color.white;

	[SerializeField]
	protected Material m_NotAvailableMaterial;

	[Header("Tooltip")]
	[SerializeField]
	protected UITextTooltipTarget m_TooltipTarget;

	protected UIPartySlotInteractionEvents _interactionEvents = new UIPartySlotInteractionEvents();

	protected SlotEventHandler _onClick;

	private Action _onDelete;

	private Action _onLevelUp;

	private bool _drag;

	public PartySlotDTO CurrentData { get; protected set; } = new PartySlotDTO();

	public int SlotId => CurrentData.SlotId;

	public bool Interactable
	{
		get
		{
			if (!(m_Button != null) || !m_Button.interactable)
			{
				if (m_InteractionHints != null)
				{
					return m_InteractionHints.HasInteractions;
				}
				return false;
			}
			return true;
		}
	}

	public bool IsHighlighted
	{
		get
		{
			if (m_Button != null)
			{
				return m_Button.IsHighlighted();
			}
			return false;
		}
	}

	public bool IsSelected
	{
		get
		{
			if (m_Button != null)
			{
				return m_Button.IsSelected();
			}
			return false;
		}
	}

	public bool HasCharacter
	{
		get
		{
			if (CurrentData != null)
			{
				return CurrentData.Character != ECharacter.None;
			}
			return false;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (m_Button != null)
		{
			m_Button.PointerEntered += UpdateHighlight;
			m_Button.PointerExited += UpdateHighlight;
			m_Button.Selected += UpdateHighlight;
			m_Button.Deselected += UpdateHighlight;
			m_Button.onClick.AddListener(OnClick);
		}
		if ((bool)m_DeleteHero)
		{
			m_DeleteHero.onClick.AddListener(HandleOnDelete);
		}
		if ((bool)m_LevelUpHero)
		{
			m_LevelUpHero.onClick.AddListener(HandleOnLevelUp);
		}
	}

	public void Init(PartySlotDTO data, SlotEventHandler onSlotClicked = null, IEnumerable<UIPartySlotHints.HintedActionParams> actions = null, UIPartySlotInteractionEvents interactionEvents = null, UIPartySlotHints.HintedActionParams deleteAction = null, UIPartySlotHints.HintedActionParams levelUPAction = null)
	{
		int num;
		if (isActiveAndEnabled && data.Character != ECharacter.None && object.Equals(CurrentData?.CharacterID, data.CharacterID))
		{
			PartySlotDTO currentData = CurrentData;
			if (currentData == null || !currentData.HasNotificationBadge)
			{
				num = (data.HasNotificationBadge ? 1 : 0);
				goto IL_004c;
			}
		}
		num = 0;
		goto IL_004c;
		IL_004c:
		bool flag = (byte)num != 0;
		CurrentData = data;
		_onClick = onSlotClicked;
		_onDelete = deleteAction?.ActionCallback;
		_onLevelUp = levelUPAction?.ActionCallback;
		bool flag2 = data.Character != ECharacter.None;
		if ((bool)m_Button)
		{
			m_Button.interactable = onSlotClicked != null || (actions?.Any() ?? false);
		}
		if ((bool)m_DeleteHero)
		{
			m_DeleteHero.gameObject.SetActive(deleteAction != null);
		}
		if ((bool)m_LevelUpHero)
		{
			m_LevelUpHero.gameObject.SetActive(levelUPAction != null);
			m_LevelUpHero.GetComponent<UITextTooltipTarget>()?.SetText(FHUILocFormatting.PopulateParams("GUI_CHARACTER_LEVEL_CU_INFO", CurrentData.MaxLevelToBeGainedFromProsperity));
		}
		UpdateInteractionHints(actions);
		UpdatePartyInfo(data, flag2);
		UpdateCharacterInfo(data, flag2);
		UpdateMultiplayerInfo(data);
		UpdateHighlight();
		UpdateHighlightedStatus(data);
		if (m_NotificationBadgeImage != null)
		{
			m_NotificationBadgeImage.gameObject.SetActive(data.HasNotificationBadge);
			if (flag)
			{
				AudioControllerUtils.PlaySound(UIInfoTools.Instance?.genericNotificationAudioItem);
			}
		}
		if (m_TooltipTarget != null)
		{
			m_TooltipTarget.tooltipText = CrossReference.ECharacterModelToClassLocKey(CurrentData.ClassID);
		}
		if (m_CharacterDisplayEventsChannel != null)
		{
			if (flag2)
			{
				m_CharacterDisplayEventsChannel.Display(this, data.Character);
			}
			else
			{
				m_CharacterDisplayEventsChannel.Hide(this);
			}
		}
		bool interactable = Interactable;
		if (m_EmptyStateBackgroundImage != null)
		{
			m_EmptyStateBackgroundImage.enabled = !flag2;
			m_EmptyStateBackgroundImage.color = (interactable ? m_InteractableColor : m_NotInteractableColor);
		}
		if (m_BackgroundImage != null)
		{
			m_BackgroundImage.enabled = flag2;
			m_BackgroundImage.color = (interactable ? m_InteractableColor : m_NotInteractableColor);
		}
		if (m_EmptyStateFrameImage != null)
		{
			m_EmptyStateFrameImage.enabled = !flag2;
			m_EmptyStateFrameImage.color = (interactable ? m_InteractableColor : m_NotInteractableColor);
		}
		if (m_FrameImage != null)
		{
			m_FrameImage.enabled = flag2;
			m_FrameImage.color = (interactable ? m_InteractableColor : m_NotInteractableColor);
		}
		_interactionEvents = interactionEvents;
	}

	protected void UpdatePartyInfo(PartySlotDTO data, bool hasCharacter)
	{
		if (m_InPartyIndicator != null)
		{
			m_InPartyIndicator.gameObject.SetActive(data.InParty);
		}
		if ((bool)m_EmptySlotImage)
		{
			m_EmptySlotImage.enabled = !hasCharacter && Interactable;
		}
		if ((bool)m_LockedSlotImage)
		{
			m_LockedSlotImage.enabled = !Interactable && !hasCharacter;
		}
		if ((bool)m_SlotFrame)
		{
			m_SlotFrame.enabled = data.Owned;
		}
		if (m_PartyStatus != null)
		{
			m_PartyStatus.text = (data.InCreation ? "Pending" : string.Empty);
			m_PartyStatus.gameObject.SetActive(!string.IsNullOrEmpty(m_PartyStatus.text));
		}
	}

	protected void UpdateCharacterInfo(PartySlotDTO data, bool hasCharacter)
	{
		if (m_PortraitImage != null)
		{
			m_PortraitImage.sprite = data.Portrait;
			m_PortraitImage.enabled = hasCharacter;
			m_PortraitImage.material = (data.IsAvailable ? null : m_NotAvailableMaterial);
		}
		if (m_IllustrationImage != null)
		{
			if (hasCharacter)
			{
				m_IllustrationImage.sprite = data.Illustration;
			}
			m_IllustrationImage.enabled = hasCharacter;
			m_IllustrationImage.material = (data.IsAvailable ? null : m_NotAvailableMaterial);
		}
		if (m_InfoIllustrationImage != null)
		{
			if (hasCharacter)
			{
				m_InfoIllustrationImage.sprite = data.InfoIllustration;
			}
			m_InfoIllustrationImage.enabled = hasCharacter;
			m_InfoIllustrationImage.material = (data.IsAvailable ? null : m_NotAvailableMaterial);
		}
		if ((bool)m_NameText)
		{
			m_NameText.text = data.Name;
		}
		if ((bool)m_ClassIcon)
		{
			if (data.Character != ECharacter.None)
			{
				m_ClassIcon.sprite = UIInfoTools.Instance.GetCharacterConfigUI(data.Character.ToString()).iconClassGold;
			}
			m_ClassIcon.gameObject.SetActive(!string.IsNullOrWhiteSpace(data.Name));
		}
		if ((bool)m_ClassColor)
		{
			if (data.Character != ECharacter.None)
			{
				m_ClassColor.color = UIInfoTools.Instance.GetCharacterColor(data.Character);
			}
			m_ClassColor.gameObject.SetActive(!string.IsNullOrWhiteSpace(data.Name));
		}
		if (m_GoldText != null)
		{
			SetTextValue(m_GoldText, data.Gold, data.ShowValues);
		}
		if (m_LevelText != null)
		{
			SetTextValue(m_LevelText, data.Level, data.ShowValues);
		}
		if (m_XPText != null)
		{
			m_XPText.text = data.Experience.ToDisplayString();
			m_XPText.enabled = data.Level != 0 && data.ShowValues;
			if (_xpBarFill != null)
			{
				_xpBarFill.fillAmount = data.Experience.Progress;
				_xpBarFill.transform.parent.gameObject.SetActive(data.Level != 0 && data.ShowValues);
			}
		}
	}

	protected void UpdateMultiplayerInfo(PartySlotDTO data)
	{
		if (m_MultiplayerUserState != null)
		{
			if (data.MultiplayerUserData.HasValue)
			{
				m_MultiplayerUserState.Show(data.MultiplayerUserData.Value);
			}
			else
			{
				m_MultiplayerUserState.Hide();
			}
		}
		if ((bool)m_VoteReady && (bool)m_VoteWaiting)
		{
			m_VoteReady.SetActive(data.IsReady.HasValue && data.IsReady.Value);
			m_VoteWaiting.SetActive(data.IsReady.HasValue && !data.IsReady.Value);
		}
		if (!(m_SlotFrame != null))
		{
			return;
		}
		m_SlotFrame.enabled = data.Owned;
		if (m_MultiplayerVisualsSO != null)
		{
			int id = data.MultiplayerUserData?.Id ?? 0;
			Sprite partySlotFrame = m_MultiplayerVisualsSO.GetPlayerVisuals(id).PartySlotFrame;
			if (partySlotFrame != null)
			{
				m_SlotFrame.sprite = partySlotFrame;
			}
		}
	}

	protected void UpdateHighlightedStatus(PartySlotDTO data)
	{
		if (m_RetiredIndication != null)
		{
			m_RetiredIndication.gameObject.SetActive(data.Retired);
		}
		if (m_CanRetireIndication != null)
		{
			m_CanRetireIndication.gameObject.SetActive(data.CanRetire);
		}
	}

	protected void UpdateInteractionHints(IEnumerable<UIPartySlotHints.HintedActionParams> actions)
	{
		if ((object)m_InteractionHints != null)
		{
			if (actions != null)
			{
				m_InteractionHints.Init(actions);
			}
			else
			{
				m_InteractionHints.Clear();
			}
		}
	}

	protected void SetColor(Color color, bool hasCharacter)
	{
		setImageColor(m_BackgroundImage);
		void setImageColor(Image image)
		{
			if (!(image == null))
			{
				image.enabled = hasCharacter;
				image.color = color;
			}
		}
	}

	protected void SetTextValue(TMP_Text tmpText, uint value, bool showValues)
	{
		if ((object)tmpText == null)
		{
			return;
		}
		if (!showValues)
		{
			tmpText.transform.parent.gameObject.SetActive(value: false);
			return;
		}
		tmpText.transform.parent.gameObject.SetActive(value: true);
		TextLocalizedListener component = tmpText.GetComponent<TextLocalizedListener>();
		if ((bool)component)
		{
			component.SetParam("CURRENT", value);
		}
		else
		{
			tmpText.text = value.ToString();
		}
	}

	protected void UpdateInteractionHintsVisibility(bool visible)
	{
		if (!(m_InteractionHints == null))
		{
			m_InteractionHints.SetHintsVisible(visible);
		}
	}

	protected void UpdateHighlight()
	{
		bool visible = IsHighlighted || IsSelected;
		UpdateInteractionHintsVisibility(visible);
		UpdateImageHighlightState(m_EmptyStateHighlightImage, IsHighlighted && !HasCharacter);
		UpdateImageHighlightState(m_EmptyStateSelectImage, IsSelected && !HasCharacter);
		UpdateImageHighlightState(m_HighlightImage, IsHighlighted && HasCharacter);
		UpdateImageHighlightState(m_SelectionImage, CurrentData.IsSelected && HasCharacter);
		static void UpdateImageHighlightState(Image[] imageArray, bool isActive)
		{
			if (imageArray != null && imageArray.Length != 0)
			{
				foreach (Image image in imageArray)
				{
					if (image != null)
					{
						image.enabled = isActive;
					}
				}
			}
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		UpdateHighlight();
	}

	public void ToggleName(bool active)
	{
		m_NameText.enabled = active;
	}

	public virtual void OnClick()
	{
		if (!_drag)
		{
			_onClick?.Invoke(this);
		}
	}

	public void HandleOnDelete()
	{
		_onDelete?.Invoke();
	}

	public void HandleOnLevelUp()
	{
		_onLevelUp?.Invoke();
	}

	protected override void OnDestroy()
	{
		if (m_Button != null)
		{
			m_Button.PointerEntered -= UpdateHighlight;
			m_Button.PointerExited -= UpdateHighlight;
			m_Button.Selected -= UpdateHighlight;
			m_Button.Deselected -= UpdateHighlight;
			m_Button.onClick.RemoveListener(OnClick);
		}
		base.OnDestroy();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_interactionEvents?.OnHover?.Invoke(CurrentData, isFlagRaised: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_interactionEvents?.OnHover?.Invoke(CurrentData, isFlagRaised: false);
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		_drag = true;
	}

	public void OnDrag(PointerEventData eventData)
	{
		_interactionEvents?.OnDrag?.Invoke(CurrentData, eventData.delta);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		_drag = false;
	}

	public IGeneralInfoData GetInfoData()
	{
		if (CurrentData == null || CurrentData.Character == ECharacter.None)
		{
			return null;
		}
		return UIGeneralInfoUtils.GetPartySlotCharacter(CurrentData);
	}

	public static List<PartySlotDTO> GetList(CMapCharacter[] mapPartySelectedCharactersArray)
	{
		List<PartySlotDTO> list = new List<PartySlotDTO>(mapPartySelectedCharactersArray.Length);
		for (int i = 0; i < mapPartySelectedCharactersArray.Length; i++)
		{
			CMapCharacter cMapCharacter = mapPartySelectedCharactersArray[i];
			if (cMapCharacter == null)
			{
				_ = mapPartySelectedCharactersArray[0];
				PartySlotDTO partySlotDTO = PartySlotDTO.Empty();
				partySlotDTO.MultiplayerUserData = UIPlayerCharacterUtils.GetPlayerData(i);
				partySlotDTO.SlotId = i;
				list.Add(partySlotDTO);
				continue;
			}
			ECharacter character = CrossReference.ClassIdToECharacterModel(cMapCharacter.CharacterID);
			PartySlotDTO item = new PartySlotDTO
			{
				SlotId = i,
				Character = character,
				ClassID = cMapCharacter.CharacterID,
				Portrait = UIInfoTools.Instance.GetCharacterConfigUI(character).illustration,
				Illustration = UIInfoTools.Instance.GetCharacterConfigUI(character).illustration,
				InfoIllustration = UIInfoTools.Instance.GetCharacterConfigUI(character).InfoIllustration,
				Color = UIInfoTools.Instance.GetCharacterColor(character),
				Name = cMapCharacter.CharacterName,
				Level = (uint)cMapCharacter.Level,
				ShowValues = true,
				MultiplayerUserData = UIPlayerCharacterUtils.GetPlayerData(cMapCharacter.CharacterID)
			};
			list.Add(item);
		}
		return list;
	}
}
