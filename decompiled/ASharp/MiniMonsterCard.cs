using FHGUI;
using FHGUI.Modules;
using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MiniMonsterCard : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
{
	[SerializeField]
	private TextMeshProUGUI _cardName;

	[SerializeField]
	private TextMeshProUGUI _initiativeText;

	[SerializeField]
	private TextMeshProUGUI _initiativeTextAlt;

	[SerializeField]
	private GameObject _shuffleIndicator;

	[Header("States")]
	[SerializeField]
	private Image[] _hoveredImages;

	[SerializeField]
	private GameObject[] _selectedObjects;

	[SerializeField]
	private GameObject[] _discardedObjects;

	[SerializeField]
	private Image _background;

	[SerializeField]
	private Image _initiativeBackground;

	[SerializeField]
	private Sprite _normalBackground;

	[SerializeField]
	private Sprite _normalInitiativeBackground;

	[SerializeField]
	private Sprite _bossBackground;

	[SerializeField]
	private Sprite _bossInitiativeBackground;

	[SerializeField]
	private ImageMaterialSwapper _materialSwapper;

	private UIMonsterCardInteractionEvents _events;

	private MonsterCardUIContext _context;

	private MonsterRoundCardUI _cardUI;

	private RectTransform _tooltipPos;

	public bool Hovered { get; private set; }

	public bool Selected { get; private set; }

	public void Init(MonsterCardUIContext context, UIMonsterCardInteractionEvents events = null, RectTransform tooltipPos = null)
	{
		_events = events;
		_context = context;
		_tooltipPos = tooltipPos;
		_cardName.text = LocalizationManager.GetTranslation(context.CardYML.Name);
		string text = context.CardYML.Initiative.ToString();
		_initiativeText.text = text;
		_initiativeTextAlt.text = text;
		_shuffleIndicator.SetActive(context.CardYML.Shuffle);
		bool flag = context.EnemyForBaseStats?.IsBoss ?? context.FallbackMonsterClass?.Boss ?? false;
		_background.sprite = (flag ? _bossBackground : _normalBackground);
		_initiativeBackground.sprite = (flag ? _bossInitiativeBackground : _normalInitiativeBackground);
		if (context.AltMonsterClass == null)
		{
			_ = context.MonsterClass;
		}
		Selected = context.CardPile == CardPileType.Round;
		SetVisualState();
	}

	private void OnDisable()
	{
		Selected = false;
		Hovered = false;
		_events = null;
		_context = null;
		RecycleCardUI();
	}

	private void SetVisualState()
	{
		Image[] hoveredImages = _hoveredImages;
		for (int i = 0; i < hoveredImages.Length; i++)
		{
			hoveredImages[i].enabled = Hovered;
		}
		GameObject[] selectedObjects = _selectedObjects;
		for (int i = 0; i < selectedObjects.Length; i++)
		{
			selectedObjects[i].SetActive(Selected);
		}
		bool active = _context.CardPile == CardPileType.Discarded;
		selectedObjects = _discardedObjects;
		for (int i = 0; i < selectedObjects.Length; i++)
		{
			selectedObjects[i].SetActive(active);
		}
		SetSwapMaterial(_context.CardPile == CardPileType.Discarded);
	}

	private void SetSwapMaterial(bool shouldBeSwapped)
	{
		if (!(_context == null))
		{
			if (shouldBeSwapped)
			{
				_materialSwapper.SwapToTargetMaterial();
			}
			else
			{
				_materialSwapper.ResetToPreSwap();
			}
		}
	}

	private void RecycleCardUI()
	{
		if (!(_cardUI == null))
		{
			SetSwapMaterial(shouldBeSwapped: false);
			ObjectPool.RecycleCard(_cardUI.CardID, ObjectPool.ECardType.Monster, _cardUI.gameObject);
			_cardUI = null;
		}
	}

	private void SpawnCardUI()
	{
		if (_cardUI != null && _cardUI.CardID == _context.CardYML.ID)
		{
			_cardUI.Init(_context);
			return;
		}
		RecycleCardUI();
		int iD = _context.CardYML.ID;
		_cardUI = ObjectPool.SpawnCard(iD, ObjectPool.ECardType.Monster, _tooltipPos).GetComponent<MonsterRoundCardUI>();
		RectTransform rectTransform = _cardUI.transform as RectTransform;
		rectTransform.localPosition = Vector3.zero;
		rectTransform.localScale = Vector3.one;
		rectTransform.anchoredPosition = new Vector2(rectTransform.rect.width * rectTransform.pivot.x, (0f - rectTransform.rect.height) * rectTransform.pivot.y);
		_cardUI.Init(_context);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!(_context == null))
		{
			Hovered = true;
			SetVisualState();
			SpawnCardUI();
			if (_cardUI != null)
			{
				_cardUI.gameObject.SetActive(value: true);
			}
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (!(_context == null))
		{
			Hovered = false;
			SetVisualState();
			if (_cardUI != null)
			{
				_cardUI.gameObject.SetActive(value: false);
			}
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		_events?.OnClick?.Invoke(_context);
	}
}
