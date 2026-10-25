using System;
using FHGUI.Scenario.DTO;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISelectActorSlot : MonoBehaviour
{
	[SerializeField]
	private FHButton slotButton;

	[SerializeField]
	private Image characterIcon;

	[SerializeField]
	private TMP_Text cardCountActive;

	[SerializeField]
	private TMP_Text cardCountDiscarded;

	[SerializeField]
	private TMP_Text cardCountLost;

	[SerializeField]
	private GameObject highlightObject;

	[SerializeField]
	private GameObject selectedObject;

	[SerializeField]
	private Material materialSelected;

	[SerializeField]
	private Material materialNormal;

	private Action<bool> onHoveredSlot;

	private Action<IDistributePointsActor> onSelect;

	private bool selected;

	public IDistributePointsActor Actor { get; private set; }

	private void Awake()
	{
		slotButton.onClick.AddListener(ToggleSelect);
		if (slotButton == null)
		{
			slotButton = GetComponent<FHButton>();
		}
		slotButton.PointerEntered += () =>
		{
			OnHoveredSlot(hovered: true);
		};
		slotButton.PointerExited += () =>
		{
			OnHoveredSlot(hovered: false);
		};
	}

	public virtual void Init(IDistributePointsActor actor, int slotIndex, bool interactable, Action<IDistributePointsActor> onSelect, Action<bool> onHoveredSlot, bool selected)
	{
		this.onHoveredSlot = onHoveredSlot;
		this.onSelect = onSelect;
		this.selected = selected;
		Actor = actor;
		slotButton.interactable = interactable;
		ScenarioPartySlotDetailsDTO scenarioPartySlotDetailsDTO = new ScenarioPartySlotDetailsDTO(actor.Actor as CPlayerActor, slotIndex, isInCardsSelectionPhase: false, isSelected: false);
		characterIcon.sprite = scenarioPartySlotDetailsDTO.PartySlotDto.Portrait;
		characterIcon.material = (this.selected ? materialSelected : materialNormal);
		if ((bool)cardCountActive)
		{
			cardCountActive.text = Actor.Actor.CharacterClass.HandAbilityCards.Count.ToString();
		}
		if ((bool)cardCountDiscarded)
		{
			cardCountDiscarded.text = Actor.Actor.CharacterClass.DiscardedAbilityCards.Count.ToString();
		}
		if ((bool)cardCountLost)
		{
			cardCountLost.text = Actor.Actor.CharacterClass.PermanentlyLostAbilityCards.Count.ToString();
		}
		if ((bool)highlightObject)
		{
			highlightObject.SetActive(value: false);
		}
		if ((bool)selectedObject)
		{
			selectedObject.SetActive(selected);
		}
	}

	private void ToggleSelect()
	{
		if (selected)
		{
			onSelect?.Invoke(null);
		}
		else
		{
			onSelect?.Invoke(Actor);
		}
	}

	private void OnHoveredSlot(bool hovered)
	{
		onHoveredSlot?.Invoke(hovered);
		if ((bool)highlightObject)
		{
			highlightObject.SetActive(hovered);
		}
	}
}
