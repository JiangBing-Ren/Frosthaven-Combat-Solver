using System.Linq;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.EventSystems;

public class FanHandCardViewSlot : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerUpHandler
{
	public delegate void SlotHoverEvent(FanHandCardViewSlot slot, bool isHovered);

	public delegate void SlotClickEvent(FanHandCardViewSlot slot);

	public Transform ContainerVisuals;

	public UIHoverMoveEffects HoverMoveVisuals;

	public UIHoverScaleEffects HoverScaleVisuals;

	public UIHoverScaleEffects HoverScaleRaycast;

	public CanvasGroup CanvasGroup;

	private AbilityCardUI _slotCardUi;

	private SlotHoverEvent _onHover;

	private SlotClickEvent _onClick;

	public CAbilityCard AbilityCard
	{
		get
		{
			if (!(_slotCardUi != null))
			{
				return null;
			}
			return _slotCardUi.AbilityCard;
		}
	}

	public int VisibleIndex { get; set; } = -1;

	public void Init(int abilityCardId, SlotHoverEvent onHover, SlotClickEvent onClick)
	{
		SpawnCardUI(abilityCardId);
		_onHover = onHover;
		_onClick = onClick;
	}

	public void Clear()
	{
		RecycleCardUI();
		_onHover = null;
		_onClick = null;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (CanvasGroup.interactable)
		{
			_onHover?.Invoke(this, isHovered: true);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_onHover?.Invoke(this, isHovered: false);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left && CanvasGroup.interactable)
		{
			_onClick?.Invoke(this);
		}
	}

	public void SetCardPileType(CardPileType type)
	{
		_slotCardUi.SetType(type);
		_slotCardUi.UpdateCard();
	}

	public void SetInteractable(bool isInteractable, bool blocksRaycasts = true, float alpha = 1f, bool moveScaleOnClick = true)
	{
		CanvasGroup.interactable = isInteractable;
		CanvasGroup.blocksRaycasts = blocksRaycasts;
		CanvasGroup.alpha = alpha;
		HoverMoveVisuals.MoveOnClick = moveScaleOnClick;
		HoverScaleVisuals.ScaleOnClick = moveScaleOnClick;
		HoverScaleVisuals.ScaleOnClick = moveScaleOnClick;
	}

	private void RecycleCardUI()
	{
		if (_slotCardUi != null)
		{
			ObjectPool.RecycleCard(_slotCardUi.CardID, ObjectPool.ECardType.Ability, _slotCardUi.gameObject);
			_slotCardUi = null;
		}
		name = "FanHandCardViewSlot_EMPTY";
	}

	private void SpawnCardUI(int id)
	{
		_slotCardUi = ObjectPool.SpawnCard(id, ObjectPool.ECardType.Ability, ContainerVisuals).GetComponent<AbilityCardUI>();
		Transform transform = _slotCardUi.transform;
		transform.localScale = Vector3.one;
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		CAbilityCard abilityCard = CharacterClassManager.AllAbilityCards.First((CAbilityCard cCard) => cCard.ID == id);
		_slotCardUi.Init(abilityCard, disableEventDetection: true);
		name = "FanHandCardViewSlot_" + _slotCardUi.CardName;
	}
}
