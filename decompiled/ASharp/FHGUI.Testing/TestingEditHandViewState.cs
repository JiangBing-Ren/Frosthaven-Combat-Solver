using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Modals;
using FHGUI.Modules;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Testing;

public class TestingEditHandViewState : MonoBehaviour
{
	private enum ViewMode
	{
		HandManagement,
		UnlockNewCards
	}

	public string CharacterID = "BannerSpearID";

	public DeckCardListModule DeckListModule;

	public HandCardListModule HandListModule;

	public NewCardsListModule NewCardsListModule;

	public GenericModalViewModule ConfirmationModule;

	public CardInfoModal CardInfoModal;

	public Button ButtonToUnlockNewCards;

	public Button ButtonToHandManagement;

	public Button ButtonRemoveChoices;

	public Button ButtonAddChoices;

	public Button ButtonRemoveLevel;

	public Button ButtonAddLevel;

	public TMP_Text TextChoices;

	public TMP_Text TextLevel;

	public TMP_Text TextCurrentMode;

	private List<CardDTO> _deckCardDtos = new List<CardDTO>();

	private List<CardDTO> _newCardDtos = new List<CardDTO>();

	private int _handCardCount = 12;

	private int _newCardChoicesRemaining = 1;

	private int _newCardChoicesMax = 1;

	private int _level = 1;

	private Color _characterColor;

	private readonly UICardInteractionEvents _handCardInteractions = new UICardInteractionEvents();

	private readonly UICardInteractionEvents _deckCardInteractions = new UICardInteractionEvents();

	private readonly UICardInteractionEvents _newCardInteractions = new UICardInteractionEvents();

	private Vector2Int _levelBounds = new Vector2Int(1, 9);

	private Vector2Int _choicesBounds = new Vector2Int(0, 8);

	private void Awake()
	{
		ButtonRemoveChoices.onClick.AddListener(() =>
		{
			addToInt(ref _newCardChoicesRemaining, -1, _choicesBounds);
		});
		ButtonAddChoices.onClick.AddListener(() =>
		{
			addToInt(ref _newCardChoicesRemaining, 1, _choicesBounds);
		});
		ButtonRemoveLevel.onClick.AddListener(() =>
		{
			addToInt(ref _level, -1, _levelBounds);
		});
		ButtonAddLevel.onClick.AddListener(() =>
		{
			addToInt(ref _level, 1, _levelBounds);
		});
		ButtonToUnlockNewCards.onClick.AddListener(() =>
		{
			SetViewMode(ViewMode.UnlockNewCards);
		});
		ButtonToHandManagement.onClick.AddListener(() =>
		{
			SetViewMode(ViewMode.HandManagement);
		});
		_handCardInteractions.OnClick = OnHandCardClick;
		_handCardInteractions.OnRightClick = OnCardInfo;
		_deckCardInteractions.OnClick = OnDeckCardClick;
		_deckCardInteractions.OnRightClick = OnCardInfo;
		_newCardInteractions.OnClick = OnNewCardClick;
		_newCardInteractions.OnRightClick = OnCardInfo;
		ConfirmationModule.Hide();
		CardInfoModal.Hide();
		void addToInt(ref int i, int d, Vector2Int bounds)
		{
			i = Mathf.Clamp(i + d, bounds.x, bounds.y);
			InitModules();
		}
	}

	public void Start()
	{
		CCharacterClass cCharacterClass = CharacterClassManager.Classes.First((CCharacterClass c) => c.CharacterID == CharacterID);
		_handCardCount = cCharacterClass.CharacterYML.NumberAbilityCardsInBattle;
		_characterColor = UIInfoTools.Instance.GetCharacterColor(cCharacterClass.CharacterModel);
		List<AbilityCardYMLData> list = (from ac in ScenarioRuleClient.SRLYML.AbilityCards
			where ac.CharacterID == CharacterID
			orderby ac.ID
			select ac).ToList();
		int handCardCount = _handCardCount;
		foreach (AbilityCardYMLData item in list)
		{
			CardDTO cardDTO = GenerateCardDto(item.ID, isInHand: false, item);
			if (GetLevel(item) < 2)
			{
				cardDTO.IsInHand = handCardCount-- > 0;
				_deckCardDtos.Add(cardDTO);
			}
			else
			{
				_newCardDtos.Add(cardDTO);
			}
		}
		SetViewMode(ViewMode.HandManagement);
		InitModules();
	}

	private void Update()
	{
		TextChoices.text = _newCardChoicesRemaining.ToString();
		TextLevel.text = _level.ToString();
	}

	private int GetLevel(AbilityCardYMLData cardData)
	{
		if (!int.TryParse(cardData.Level, out var result))
		{
			return 1;
		}
		return result;
	}

	private CardDTO GenerateCardDto(int id, bool isInHand, AbilityCardYMLData cardData)
	{
		return new CardDTO
		{
			Id = id,
			Level = GetLevel(cardData),
			NameLocKey = cardData.Name,
			Initiative = cardData.Initiative,
			IsInHand = isInHand
		};
	}

	private void OnDeckCardClick(CardDTO cardDto)
	{
		cardDto.IsInHand = !cardDto.IsInHand;
		InitModules();
	}

	private void OnHandCardClick(CardDTO cardDto)
	{
		if (cardDto != null)
		{
			cardDto.IsInHand = false;
			InitModules();
		}
	}

	private void OnNewCardClick(CardDTO cardDto)
	{
		if (!cardDto.IsInHand && !cardDto.IsDisabled && _newCardChoicesRemaining >= 1)
		{
			ButtonsBarParam value = new ButtonsBarParam
			{
				LabelKey = "GUI_CONFIRM",
				Callback = unlockCard,
				Interactable = true
			};
			ButtonsBarParam value2 = new ButtonsBarParam
			{
				LabelKey = "GUI_CANCEL",
				Callback = CardInfoModal.Hide,
				Interactable = true
			};
			CardInfoModal.Show("Add to deck?", "Add this card to your available abilities?", cardDto, value, value2);
		}
		void unlockCard()
		{
			CardInfoModal.Hide();
			_deckCardDtos.Add(cardDto.Clone());
			cardDto.IsInHand = true;
			ConfirmationModule.gameObject.SetActive(value: false);
			_newCardChoicesRemaining--;
			InitModules();
		}
	}

	private void OnCloseNewCardsClicked()
	{
		SetViewMode(ViewMode.HandManagement);
	}

	private void OnCardInfo(CardDTO cardDto)
	{
		CardInfoModal.Show("Card Info", "Add this card to your available abilities?", cardDto);
	}

	private void InitModules()
	{
		bool flag = _newCardChoicesRemaining > 0;
		foreach (CardDTO item in _newCardDtos.Where((CardDTO cardDto) => !cardDto.IsInHand))
		{
			item.IsDisabled = !flag || item.Level > _level;
		}
		DeckListModule.Init(new CardListModuleBase.Params
		{
			Cards = _deckCardDtos,
			InteractionEvents = _deckCardInteractions
		});
		HandListModule.Init(new CardListModuleBase.Params
		{
			Cards = _deckCardDtos.Where((CardDTO dto) => dto.IsInHand).ToList(),
			InteractionEvents = _handCardInteractions,
			MinCount = _handCardCount
		}, _characterColor, _characterColor, isDualDeck: false);
		NewCardsListModule.Init(_newCardDtos, string.Empty, _newCardChoicesRemaining, _newCardChoicesMax, canRespec: false, canRespecThisCharacter: false, canAffordRespec: false, 0u, null, _newCardInteractions);
	}

	private void SetViewMode(ViewMode mode)
	{
		_newCardChoicesMax = _newCardChoicesRemaining;
		NewCardsListModule.gameObject.SetActive(mode == ViewMode.UnlockNewCards);
		DeckListModule.gameObject.SetActive(mode == ViewMode.HandManagement);
		HandListModule.gameObject.SetActive(mode == ViewMode.HandManagement);
		TextCurrentMode.text = mode.ToString().SplitCamelCase();
		InitModules();
	}
}
