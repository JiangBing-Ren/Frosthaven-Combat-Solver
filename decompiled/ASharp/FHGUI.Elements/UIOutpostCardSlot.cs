using System.Linq;
using FHGUI.Common;
using FHGUI.DTO;
using FHGUI.Modules;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIOutpostCardSlot : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IGeneralInfoProvider
{
	public Image ImageBackground;

	public Image ImageSelected;

	public Image ImageUnavailable;

	public Image ImageCardFace;

	public TMP_Text TextName;

	public Transform TransformContainer;

	public Button ButtonSlot;

	public Sprite DefaultCardImage;

	[Header("Initiative")]
	public Image ImageInitiativeBackground;

	public Image ImageInitiativeIllustration;

	public TMP_Text TextInitiative;

	public GameObject InitiativeAlt;

	public TMP_Text TextInitiativeAlt;

	public GameObject SelectedContainer;

	public TMP_Text TextInitiativeSelected;

	public bool ShowTooltips;

	private CardDTO _cardDto;

	private bool _isMiniCard;

	private bool _isHovering;

	private bool _isSelected;

	private UICardInteractionEvents _interactionEvents;

	private bool _invalidateFitInParent;

	private AbilityCardUI _cardUI;

	public CardDTO CardDto => _cardDto;

	public AbilityCardUI CardUI => _cardUI;

	public bool HasImage => ImageCardFace.sprite != DefaultCardImage;

	private void Awake()
	{
		ButtonSlot.onClick.AddListener(OnClick);
	}

	private void OnEnable()
	{
		bool swapMaterial = _cardDto?.IsDisabled ?? false;
		SetSwapMaterial(swapMaterial);
	}

	private void OnDisable()
	{
		SetSwapMaterial(shouldBeSwapped: false);
	}

	private void OnDestroy()
	{
		if (_cardUI != null)
		{
			RecycleCardUI();
		}
	}

	public void Init(CardDTO cardDto, bool isMiniCard, UICardInteractionEvents interactionEvents, bool fitInParent = false)
	{
		gameObject.SetActive(value: true);
		_cardDto = cardDto;
		_isMiniCard = isMiniCard;
		_interactionEvents = interactionEvents;
		if (cardDto == null)
		{
			name = "CardSlot_EMPTY";
			TextName.text = string.Empty;
			TextInitiative.text = string.Empty;
			ImageSelected.gameObject.SetActive(value: false);
			ImageUnavailable.gameObject.SetActive(value: false);
			ButtonSlot.gameObject.SetActive(value: false);
			if (InitiativeAlt != null)
			{
				InitiativeAlt.SetActive(value: false);
			}
		}
		else
		{
			name = $"CardSlot_ID[{cardDto.Id}]_{cardDto.Initiative}";
			UIHelperUtil.SetTextKey(TextName, cardDto.NameLocKey);
			TextInitiative.text = cardDto.Initiative.ToString();
			ImageSelected.gameObject.SetActive(cardDto.IsInHand);
			ImageUnavailable.gameObject.SetActive(!cardDto.IsSelectable);
			ButtonSlot.gameObject.SetActive(value: true);
			if (InitiativeAlt != null)
			{
				InitiativeAlt.SetActive(cardDto.HasAltInitiative);
				TextInitiativeAlt.text = cardDto.AltInitiative.ToString();
			}
			if ((bool)TextInitiativeSelected)
			{
				TextInitiativeSelected.text = cardDto.Initiative.ToString();
			}
			if (_isMiniCard)
			{
				Sprite characterCardHeader = UIInfoTools.Instance.GetCharacterCardHeader(cardDto.CharacterID, cardDto.Id);
				characterCardHeader = ((characterCardHeader == null) ? DefaultCardImage : characterCardHeader);
				ImageCardFace.sprite = characterCardHeader;
				ImageSelected.sprite = characterCardHeader;
			}
		}
		if (ImageBackground != null)
		{
			if (_cardDto == null)
			{
				ImageBackground.overrideSprite = null;
			}
			else
			{
				ImageBackground.overrideSprite = (_cardDto.IsAltColor ? UIInfoTools.Instance.GetCharacterConfigUI(_cardDto.CharacterID).altFormCardSkin.regularPreviewBackground : UIInfoTools.Instance.GetCharacterConfigUI(_cardDto.CharacterID).cardSkin.regularPreviewBackground);
			}
		}
		if (ImageInitiativeIllustration != null)
		{
			Image imageInitiativeIllustration = ImageInitiativeIllustration;
			Sprite overrideSprite;
			if (_cardDto == null)
			{
				overrideSprite = null;
			}
			else
			{
				overrideSprite = (_cardDto.IsAltColor ? UIInfoTools.Instance.GetCharacterConfigUI(_cardDto.CharacterID).altFormCardSkin.initiativeBackground : UIInfoTools.Instance.GetCharacterConfigUI(_cardDto.CharacterID).cardSkin.initiativeBackground);
			}
			imageInitiativeIllustration.overrideSprite = overrideSprite;
		}
		SetAbilityCardUI(fitInParent);
		UpdateCardEffects();
		SetSelected(selected: false);
		UpdateHighlights();
	}

	public void Clear()
	{
		Init(null, _isMiniCard, null);
	}

	private void UpdateCardEffects()
	{
		if (_cardUI == null)
		{
			return;
		}
		bool flag = _cardDto.IsDisabled || !_cardDto.IsSelectable;
		if (_cardUI.fullAbilityCard.cardEffects != null)
		{
			if (flag)
			{
				_cardUI.fullAbilityCard.cardEffects.ToggleEffect(active: false, CardEffects.FXTask.DiscardMode);
			}
			else
			{
				_cardUI.fullAbilityCard.cardEffects.RestoreCard();
			}
		}
		SetSwapMaterial(flag);
	}

	private void SetSwapMaterial(bool shouldBeSwapped)
	{
		if (!(_cardUI == null) && _cardUI.fullAbilityCard.TryGetComponent<ImageMaterialSwapper>(out var component))
		{
			if (shouldBeSwapped)
			{
				component.SwapToTargetMaterial();
			}
			else
			{
				component.ResetToPreSwap();
			}
		}
	}

	public void SetFaceColor(Color color)
	{
		if (ImageCardFace != null)
		{
			ImageCardFace.color = color;
		}
		if (ImageInitiativeBackground != null)
		{
			ImageInitiativeBackground.color = color;
		}
	}

	public void SetHoverReparenting(RectTransform reparentTargetTransform)
	{
		UIHelperUtil.SetHoverReparenting(ButtonSlot.gameObject, reparentTargetTransform, (RectTransform)transform, (RectTransform)ButtonSlot.transform);
	}

	private void SetAbilityCardUI(bool fitInParent = false)
	{
		bool flag = _cardDto == null;
		bool flag2 = _cardUI == null;
		bool active = !flag && (!_isMiniCard || _isHovering);
		TransformContainer.gameObject.SetActive(active);
		if (flag & flag2)
		{
			return;
		}
		if (!flag2 & flag)
		{
			RecycleCardUI();
		}
		else if (flag2 && !flag)
		{
			SpawnCardUI(fitInParent);
		}
		else if (_cardDto.Id == _cardUI.CardID)
		{
			int id = _cardDto.Id;
			CAbilityCard abilityCard = CharacterClassManager.AllAbilityCards.First((CAbilityCard cCard) => cCard.ID == id);
			_cardUI.Init(abilityCard, disableEventDetection: true, _cardDto.Enhancements);
		}
		else
		{
			RecycleCardUI();
			SpawnCardUI(fitInParent);
		}
	}

	public void SetSelected(bool selected)
	{
		_isSelected = selected;
		if ((bool)SelectedContainer)
		{
			SelectedContainer.gameObject.SetActive(selected);
		}
		UpdateHighlights();
	}

	private void UpdateHighlights()
	{
		CanvasGroup componentInChildren = GetComponentInChildren<CanvasGroup>();
		if (componentInChildren != null)
		{
			componentInChildren.alpha = ((_cardDto.IsInHand || _isSelected) ? 1f : 0.4f);
		}
	}

	private void RecycleCardUI()
	{
		SetSwapMaterial(shouldBeSwapped: false);
		SetSelected(selected: false);
		ObjectPool.RecycleCard(_cardUI.CardID, ObjectPool.ECardType.Ability, _cardUI.gameObject);
		_cardUI = null;
	}

	private void SpawnCardUI(bool fitInParent)
	{
		int id = _cardDto.Id;
		_cardUI = ObjectPool.SpawnCard(id, ObjectPool.ECardType.Ability, TransformContainer).GetComponent<AbilityCardUI>();
		RectTransform obj = _cardUI.transform as RectTransform;
		obj.anchorMin = new Vector2(0.5f, 0.5f);
		obj.anchorMax = new Vector2(0.5f, 0.5f);
		_cardUI.transform.localPosition = Vector3.zero;
		CAbilityCard abilityCard = CharacterClassManager.AllAbilityCards.First((CAbilityCard cCard) => cCard.ID == id);
		_cardUI.Init(abilityCard, disableEventDetection: true, _cardDto.Enhancements);
		_cardUI.SetHoverDetectionActive(active: false);
		_cardUI.ToggleFullCard(active: true);
		_cardUI.fullAbilityCard.BlockRaycasts(ShowTooltips);
		_cardUI.fullAbilityCard.SetInteractable(active: true);
		if (fitInParent)
		{
			_invalidateFitInParent = true;
		}
		else
		{
			_cardUI.transform.localScale = Vector3.one;
		}
	}

	private void FitInParent()
	{
		if (!(_cardUI == null))
		{
			RectTransform rectTransform = (RectTransform)_cardUI.transform;
			RectTransform rectTransform2 = (RectTransform)TransformContainer;
			Rect rect = rectTransform.rect;
			Rect rect2 = rectTransform2.rect;
			float num = rect2.width / rect.width;
			float num2 = rect2.height / rect.height;
			rectTransform.localScale = new Vector3(num, num2, 1f);
			bool flag = num > Mathf.Epsilon && num2 > Mathf.Epsilon;
			_invalidateFitInParent = !flag;
		}
	}

	private void LateUpdate()
	{
		if (_invalidateFitInParent)
		{
			FitInParent();
		}
	}

	private void OnClick()
	{
		_interactionEvents?.OnClick?.Invoke(_cardDto);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_isHovering = true;
		if (_isMiniCard)
		{
			TransformContainer.gameObject.SetActive(_isHovering);
		}
		_interactionEvents?.OnHover?.Invoke(_cardDto, _isHovering);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_isHovering = false;
		if (_isMiniCard)
		{
			TransformContainer.gameObject.SetActive(_isHovering);
		}
		_interactionEvents?.OnHover?.Invoke(_cardDto, _isHovering);
	}

	public IGeneralInfoData GetInfoData()
	{
		return UIGeneralInfoUtils.GetAbilityCardInfo(_cardDto);
	}
}
