using System.Collections.Generic;
using System.Linq;
using FHGUI.Common;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;

namespace FHGUI.Scenario.Elements;

public class UIReactionCardHandInfoElement : MonoBehaviour
{
	public struct Params
	{
		public CPlayerActor PlayerActor;
	}

	[SerializeField]
	private TMP_Text cardCountActive;

	[SerializeField]
	private TMP_Text cardCountDiscarded;

	[SerializeField]
	private TMP_Text cardCountLost;

	[SerializeField]
	private UIAbilityCardContainer abilityCardContainer;

	[SerializeField]
	private FHButton slotButton;

	private Params _params;

	private void Awake()
	{
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

	public void Init(Params @params)
	{
		_params = @params;
		if ((bool)cardCountActive)
		{
			cardCountActive.text = (_params.PlayerActor.CharacterClass.ActivatedAbilityCards.Count + _params.PlayerActor.CharacterClass.HandAbilityCards.Count).ToString();
		}
		if ((bool)cardCountDiscarded)
		{
			cardCountDiscarded.text = _params.PlayerActor.CharacterClass.DiscardedAbilityCards.Count.ToString();
		}
		if ((bool)cardCountLost)
		{
			cardCountLost.text = _params.PlayerActor.CharacterClass.PermanentlyLostAbilityCards.Count.ToString();
		}
	}

	private void OnHoveredSlot(bool hovered)
	{
		abilityCardContainer.gameObject.SetActive(hovered);
		if (hovered)
		{
			List<CAbilityCard> cards = _params.PlayerActor.CharacterClass.ActivatedAbilityCards.OrderBy((CAbilityCard card) => card.Initiative).Concat(_params.PlayerActor.CharacterClass.HandAbilityCards.OrderBy((CAbilityCard card) => card.Initiative)).Concat(_params.PlayerActor.CharacterClass.DiscardedAbilityCards.OrderBy((CAbilityCard card) => card.Initiative))
				.Concat(_params.PlayerActor.CharacterClass.PermanentlyLostAbilityCards.OrderBy((CAbilityCard card) => card.Initiative))
				.ToList();
			abilityCardContainer.Init(cards, _params.PlayerActor);
		}
	}
}
