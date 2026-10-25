using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FH.YML;
using FHGUI.Common;
using MapRuleLibrary.Client;
using ScenarioRuleLibrary;
using SharedLibrary.Client;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Testing;

[DefaultExecutionOrder(-100)]
public class TestCardGlossary : MonoBehaviour
{
	[SerializeField]
	private RectTransform _cardContainer;

	[SerializeField]
	private TMP_InputField _cardIdInput;

	[SerializeField]
	private Button _previousCardButton;

	[SerializeField]
	private Button _nextCardButton;

	[SerializeField]
	private int _cardID = 1;

	[SerializeField]
	private UIGeneralInfoModule _generalInfo;

	private List<CAbilityCard> _cards = new List<CAbilityCard>();

	private AbilityCardUI _card;

	public void Awake()
	{
		_cardIdInput.onSubmit.AddListener(SubmitCard);
		_previousCardButton.onClick.AddListener(OnPreviousClick);
		_nextCardButton.onClick.AddListener(OnNextClick);
		_cardIdInput.SetTextWithoutNotify(_cardID.ToString());
		StartCoroutine(Setup());
	}

	private IEnumerator Setup()
	{
		SharedClient.Initialise(string.Empty, YMLLoading.RulebaseFolder);
		ScenarioRuleClient.Initialise(reinitialise: true);
		MapRuleLibraryClient.Instance.Initialise(reinitialise: true);
		MapRuleLibraryClient.MRLYML.MapMode = ScenarioManager.EDLLMode.Campaign;
		if (YMLDefRepository.Repo.DefRepository == null)
		{
			YMLDefRepository.Repo.DebugGenerateDefRepository();
		}
		yield return UIInfoTools.InitInstanceCrt();
		yield return GlobalSettings.InitInstanceCrt();
		MonsterClassManager.Load();
		CharacterClassManager.Load();
		foreach (CAbilityCard allAbilityCard in CharacterClassManager.AllAbilityCards)
		{
			PersistentData.CreateAbilityCard(allAbilityCard.GetAbilityCardYML, out var _, out var _);
		}
		_cards.AddRange(CharacterClassManager.AllAbilityCards.OrderBy((CAbilityCard c) => c.ID));
		ShowCard(_cardID);
	}

	private void OnPreviousClick()
	{
		if (_cardID >= 1)
		{
			_cardID--;
			_cardIdInput.SetTextWithoutNotify(_cardID.ToString());
			ShowCard(_cardID);
		}
	}

	private void OnNextClick()
	{
		if (_cardID != _cards.Last().ID)
		{
			_cardID++;
			_cardIdInput.SetTextWithoutNotify(_cardID.ToString());
			ShowCard(_cardID);
		}
	}

	private void SubmitCard(string cardId)
	{
		_cardID = int.Parse(cardId);
		ShowCard(int.Parse(cardId));
	}

	private void ShowCard(int cardId)
	{
		SpawnCardUI(cardId);
		_generalInfo.RequestGeneralInfo(UIGeneralInfoUtils.GetAbilityCardInfo(_card));
	}

	private void SpawnCardUI(int cardId)
	{
		if (_card != null)
		{
			ObjectPool.RecycleCard(_card.CardID, ObjectPool.ECardType.Ability, _card.gameObject);
		}
		_card = ObjectPool.SpawnCard(cardId, ObjectPool.ECardType.Ability, _cardContainer, resetLocalScale: true).GetComponent<AbilityCardUI>();
		_card.transform.localScale = Vector3.one;
		_card.Init(_cards.First((CAbilityCard c) => c.ID == cardId));
		_card.SetMode(CardHandMode.Preview, null);
	}
}
