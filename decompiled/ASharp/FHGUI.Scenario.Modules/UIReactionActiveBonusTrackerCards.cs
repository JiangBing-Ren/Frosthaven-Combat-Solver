using System.Collections.Generic;
using System.Linq;
using FHGUI.Common;
using FHGUI.Modules;
using FHGUI.Scenario.Elements;
using Libs.Utils.Maths;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Scenario.Modules;

public class UIReactionActiveBonusTrackerCards : MonoBehaviour, IUIViewModule, ICharacterReactionModule
{
	public delegate void ReactionEvent(CPlayerActor playerActor, List<CAbilityCard> selectedCards);

	public delegate bool ReactionCondition(List<CAbilityCard> selectedCards);

	public struct Params
	{
		public CPlayerActor PlayerActor;

		public CAbility Ability;

		public IList<CAbilityCard> ChoiceCards;

		public int MaxCards;

		public string TitleKey;

		public string Description;

		public string ConfirmKey;

		public Sprite SourceIllustrations;

		public ILegacyButton UndoButtonReference;

		public ILegacyButton SkipButtonReference;

		public ReactionEvent OnConfirm;

		public ReactionEvent OnSelectedCardsUpdated;

		public ReactionCondition ConfirmCondition;
	}

	[SerializeField]
	private Image _sourceIllustrations;

	[SerializeField]
	private TextMeshProUGUI _titleText;

	[SerializeField]
	private TextMeshProUGUI _hintText;

	[SerializeField]
	private UIAbilityCardContainer _availableCardsContainer;

	[SerializeField]
	private UIAbilityCardContainer _selectedCardsContainer;

	[SerializeField]
	private FHButton _confirmButton;

	[SerializeField]
	private FHButton _undoButton;

	[SerializeField]
	private FHButton _skipButton;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private UIReactionAbilityNameInfoElement _sourceAbilityName;

	private Params _params;

	private readonly List<CAbilityCard> _selectedCards = new List<CAbilityCard>();

	private const float ALPHA_VISIBLE = 1f;

	private const float ALPHA_INVISIBLE = 0f;

	public GameObject GameObject => gameObject;

	private void Awake()
	{
		_undoButton.onClick.AddListener(OnUndoClicked);
		_skipButton.onClick.AddListener(OnSkipClicked);
		_confirmButton.onClick.AddListener(OnConfirmClicked);
	}

	private void OnUndoClicked()
	{
		_params.UndoButtonReference?.OnClick();
		Close();
	}

	private void OnSkipClicked()
	{
		_params.SkipButtonReference?.OnClick();
		Close();
	}

	private void OnConfirmClicked()
	{
		_params.OnConfirm?.Invoke(_params.PlayerActor, _selectedCards.ToList());
		Close();
	}

	private void Update()
	{
		UpdateButtonStates();
	}

	public void Init(Params @params)
	{
		_params = @params;
		_selectedCards.Clear();
		gameObject.SetActive(value: true);
		SetVisibility(isVisible: true);
		UIScenarioCardInteractionEvents events = new UIScenarioCardInteractionEvents
		{
			OnHover = null,
			OnClick = OnCardClicked,
			OnDoubleClick = null,
			OnRightClick = null,
			OnDrag = null
		};
		_availableCardsContainer.Init(_params.ChoiceCards, _params.PlayerActor, ECharacter.None, events);
		UIHelperUtil.SetTextKey(_titleText, _params.TitleKey);
		UIHelperUtil.SetNonLocalizedText(_hintText, _params.Description);
		_confirmButton.SetTextKey(_params.ConfirmKey);
		if ((bool)_sourceIllustrations)
		{
			_sourceIllustrations.enabled = _params.SourceIllustrations != null;
			if (_sourceIllustrations.enabled)
			{
				_sourceIllustrations.sprite = _params.SourceIllustrations;
			}
		}
		_sourceAbilityName.Init(new UIReactionAbilityNameInfoElement.Params
		{
			PlayerActor = _params.PlayerActor,
			Ability = _params.Ability,
			Card = _params.Ability?.GetAbilityBaseCard()
		});
		UpdateSelectedCards();
		UpdateButtonStates();
	}

	public void SetSelectedCards(List<CAbilityCard> selectedCards)
	{
		if (selectedCards.Any((CAbilityCard c) => !_params.ChoiceCards.Contains(c)))
		{
			Log.GUIError("Cards desync!", this);
			return;
		}
		_selectedCards.Clear();
		_selectedCards.AddRange(selectedCards);
		UpdateSelectedCards();
	}

	private void UpdateButtonStates()
	{
		bool flag = _params.PlayerActor?.IsUnderMyControl() ?? false;
		bool flag2 = _params.ConfirmCondition?.Invoke(_selectedCards) ?? true;
		_confirmButton.interactable = flag & flag2;
		UpdateButtonState(_undoButton, _params.UndoButtonReference, flag);
		UpdateButtonState(_skipButton, _params.SkipButtonReference, flag);
	}

	private static void UpdateButtonState(FHButton button, ILegacyButton legacyButton, bool isUnderMyControl)
	{
		if (legacyButton == null)
		{
			button.gameObject.SetActive(value: false);
			return;
		}
		button.gameObject.SetActive(legacyButton.IsActiveAndEnabled);
		button.interactable = isUnderMyControl && legacyButton.IsInteractable;
	}

	public void ToggleVisibility()
	{
		SetVisibility(Utl.LesserThan(_canvasGroup.alpha, 1f));
	}

	private void SetVisibility(bool isVisible)
	{
		_canvasGroup.alpha = (isVisible ? 1f : 0f);
		_canvasGroup.blocksRaycasts = isVisible;
	}

	public void Close()
	{
		_params = default;
		_selectedCards.Clear();
		SetVisibility(isVisible: false);
		gameObject.SetActive(value: false);
	}

	private void OnCardClicked(AbilityCardUI target)
	{
		if (_params.PlayerActor == null || !_params.PlayerActor.IsUnderMyControl())
		{
			return;
		}
		int count = _selectedCards.Count;
		if (!_selectedCards.Remove(target.AbilityCard))
		{
			_selectedCards.Add(target.AbilityCard);
			_selectedCards.Sort((CAbilityCard c1, CAbilityCard c2) => (_params.ChoiceCards.IndexOf(c1) >= _params.ChoiceCards.IndexOf(c2)) ? 1 : (-1));
		}
		UpdateSelectedCards();
		if (count != _selectedCards.Count)
		{
			_params.OnSelectedCardsUpdated?.Invoke(_params.PlayerActor, _selectedCards);
		}
	}

	private void UpdateSelectedCards()
	{
		_selectedCardsContainer.Init(_selectedCards, _params.PlayerActor, ECharacter.Drifter);
		foreach (UIAbilityCardSlot element in _availableCardsContainer.Elements)
		{
			if (element.TryGetCardUi(out var cardUI))
			{
				CardPileType type = (_selectedCards.Contains(cardUI.AbilityCard) ? CardPileType.Round : CardPileType.Active);
				cardUI.SetType(type);
				cardUI.UpdateCard();
			}
		}
	}

	public void UpdateState()
	{
	}
}
