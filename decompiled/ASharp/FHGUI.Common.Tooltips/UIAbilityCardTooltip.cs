using System.Linq;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;

namespace FHGUI.Common.Tooltips;

public class UIAbilityCardTooltip : MonoBehaviour, ITooltip<UIAbilityCardTooltip.FHAbilityCardTooltipParams>, IPooleable
{
	public record FHAbilityCardTooltipParams
	{
		public int AbilityCardID;

		public string Hints;

		public FHAbilityCardTooltipParams()
		{
		}

		public FHAbilityCardTooltipParams(int id)
		{
			AbilityCardID = id;
		}
	}

	[SerializeField]
	private RectTransform _cardContainer;

	[SerializeField]
	private TMP_Text m_Hints;

	private AbilityCardUI _cardUI;

	public GameObject GameObject => gameObject;

	public void Show(FHAbilityCardTooltipParams data)
	{
		ClearCard();
		_cardUI = ObjectPool.SpawnCard(data.AbilityCardID, ObjectPool.ECardType.Ability, _cardContainer).GetComponent<AbilityCardUI>();
		_cardUI.transform.localPosition = Vector3.zero;
		_cardUI.transform.localScale = Vector3.one;
		CAbilityCard abilityCard = CharacterClassManager.AllAbilityCards.First((CAbilityCard cCard) => cCard.ID == data.AbilityCardID);
		_cardUI.Init(abilityCard);
		_cardUI.ToggleFullCard(active: true);
		_cardUI.fullAbilityCard.BlockRaycasts(block: true);
		gameObject.SetActive(value: true);
		if (m_Hints != null)
		{
			m_Hints.text = data.Hints;
		}
	}

	public void Hide()
	{
		ClearCard();
		gameObject.SetActive(value: false);
	}

	private void ClearCard()
	{
		if (_cardUI != null)
		{
			ObjectPool.RecycleCard(_cardUI.CardID, ObjectPool.ECardType.Ability, _cardUI.gameObject);
			_cardUI = null;
		}
	}

	public void OnReturnedToPool()
	{
		ClearCard();
	}

	public void OnRemovedFromPool()
	{
	}
}
