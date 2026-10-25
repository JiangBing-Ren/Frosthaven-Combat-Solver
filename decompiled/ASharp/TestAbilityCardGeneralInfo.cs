using System;
using System.Linq;
using FH.YML;
using FHGUI.Common;
using FHGUI.DTO;
using FHRL;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;

public class TestAbilityCardGeneralInfo : MonoBehaviour, IGeneralInfoProvider
{
	public enum AbilityCardGeneralInfoType
	{
		CardYML,
		CardUI,
		CardDef,
		CardDTO
	}

	[SerializeField]
	[HideInInspector]
	public int _cardID;

	[SerializeField]
	public RectTransform _cardPivot;

	public AbilityCardGeneralInfoType DataType;

	private AbilityCardUI _card;

	private AbilityCardYMLData _cardYML;

	private void Awake()
	{
		SetCardID(_cardID);
	}

	public IGeneralInfoData GetInfoData()
	{
		return DataType switch
		{
			AbilityCardGeneralInfoType.CardYML => (IGeneralInfoData)UIGeneralInfoUtils.GetAbilityCardInfo(_cardYML), 
			AbilityCardGeneralInfoType.CardUI => UIGeneralInfoUtils.GetAbilityCardInfo(_card), 
			AbilityCardGeneralInfoType.CardDef => UIGeneralInfoUtils.GetAbilityCardInfo(YMLDefRepository.Repo.DefRepository.ClassCards[new ClassCardID((uint)_cardID)]), 
			AbilityCardGeneralInfoType.CardDTO => UIGeneralInfoUtils.GetAbilityCardInfo(new CardDTO(YMLDefRepository.Repo.DefRepository.ClassCards[new ClassCardID((uint)_cardID)], isInHand: false, null, null, isSelectable: false)), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public void SetCardID(int cardId)
	{
		_cardID = cardId;
		_cardYML = ScenarioRuleClient.SRLYML.AbilityCards.Single((AbilityCardYMLData s) => s.ID == _cardID);
		if (_card != null)
		{
			ObjectPool.RecycleCard(_cardID, ObjectPool.ECardType.Ability, _card.gameObject);
		}
		_card = ObjectPool.SpawnCard(_cardID, ObjectPool.ECardType.Ability, _cardPivot, resetLocalScale: true).GetComponent<AbilityCardUI>();
		_card.transform.localPosition = Vector3.zero;
		_card.transform.localScale = Vector3.one * 0.8f;
		_card.SetMode(CardHandMode.Preview, null);
	}
}
