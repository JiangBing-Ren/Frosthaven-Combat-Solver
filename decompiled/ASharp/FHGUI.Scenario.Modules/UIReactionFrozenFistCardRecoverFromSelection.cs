using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.Common;
using FHGUI.Modules;
using FHGUI.Scenario.Elements;
using GLOOM;
using Libs.Utils.Maths;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Scenario.Modules;

public class UIReactionFrozenFistCardRecoverFromSelection : MonoBehaviour, IUIViewModule, ICharacterReactionModule
{
	public delegate void ReactionEvent(CPlayerActor playerActor, List<CAbilityCard> selectedCards);

	public delegate bool ReactionCondition(List<CAbilityCard> selectedCards);

	public struct Params
	{
		public CPlayerActor PlayerActor;

		public CAbility Ability;

		public IList<CAbilityCard> ChoiceCards;

		public string TitleKey;

		public string DescriptionKey;

		public string ConfirmKey;

		public string BuffWarningKey;

		public Sprite SourceIllustrations;

		public ILegacyButton UndoButtonReference;

		public ILegacyButton SkipButtonReference;

		public ReactionEvent OnConfirm;

		public ReactionEvent OnSelectedCardsUpdated;

		public ReactionCondition ConfirmCondition;

		public ReactionCondition CancelRoundBonusCondition;

		public Func<List<CAbilityCard>, int> SelfDamageValue;
	}

	[SerializeField]
	private Image _sourceIllustrations;

	[SerializeField]
	private TextMeshProUGUI _titleText;

	[SerializeField]
	private TextMeshProUGUI _hintText;

	[SerializeField]
	private TextMeshProUGUI _selfDamageHintText;

	[SerializeField]
	private TextMeshProUGUI _cancelBuffHintText;

	[SerializeField]
	private UIAbilityCardContainer _availableCardsContainer;

	[SerializeField]
	private FHButton _confirmButton;

	[SerializeField]
	private FHButton _undoButton;

	[SerializeField]
	private FHButton _skipButton;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private UIReactionCardHandInfoElement _cardHandInfoElement;

	[SerializeField]
	private UIReactionAbilityNameInfoElement _sourceAbilityName;

	[SerializeField]
	private GameObject _minimizableContent;

	private Params _params;

	private readonly List<CAbilityCard> _selectedCards = new List<CAbilityCard>();

	private bool _minimized;

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

	public void Init(Params @params)
	{
		_params = @params;
		_selectedCards.Clear();
		gameObject.SetActive(value: true);
		SetVisibility(isVisible: true);
		UIHelperUtil.SetTextKey(_titleText, _params.TitleKey);
		UIHelperUtil.SetTextKey(_hintText, _params.DescriptionKey);
		UIHelperUtil.SetTextKey(_cancelBuffHintText, _params.BuffWarningKey);
		_selfDamageHintText.text = "Suffer 2+(X/2)(rounded down) where X is equal to card's level";
		if ((bool)_sourceIllustrations)
		{
			_sourceIllustrations.enabled = _params.SourceIllustrations != null;
			if (_sourceIllustrations.enabled)
			{
				_sourceIllustrations.sprite = _params.SourceIllustrations;
			}
		}
		_cardHandInfoElement.Init(new UIReactionCardHandInfoElement.Params
		{
			PlayerActor = _params.PlayerActor
		});
		_sourceAbilityName.Init(new UIReactionAbilityNameInfoElement.Params
		{
			PlayerActor = _params.PlayerActor,
			Ability = _params.Ability,
			Card = _params.Ability?.GetAbilityBaseCard()
		});
		UpdateSelectedCards();
		UpdateButtonStates();
		enabled = true;
	}

	public void UpdateState()
	{
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
		_params.CancelRoundBonusCondition?.Invoke(_selectedCards);
		_confirmButton.interactable = flag & flag2;
		if (flag2)
		{
			bool flag3 = false;
			string nonlocalizedText = (flag3 ? ("<color=#" + UIInfoTools.DefInstance.negativeTextColor.ToHex() + ">") : "") + LocalizationManager.GetTranslation("GUI_TAKE_DAMAGE") + (flag3 ? " <sprite name=Enemy> " : " <sprite name=Damage> ") + _params.SelfDamageValue(_selectedCards);
			_confirmButton.SetNonLocalizedText(nonlocalizedText);
		}
		else
		{
			_confirmButton.SetTextKey(_params.ConfirmKey);
		}
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

	public void ToggleMinimized()
	{
		SetMinimized(!_minimized);
	}

	private void SetMinimized(bool minimized)
	{
		_minimized = minimized;
		_canvasGroup.alpha = (_minimized ? 1f : 0f);
		_canvasGroup.blocksRaycasts = _minimized;
		_minimizableContent.SetActive(_minimized);
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
		if (_params.PlayerActor != null && _params.PlayerActor.IsUnderMyControl())
		{
			if (!_selectedCards.Remove(target.AbilityCard))
			{
				_selectedCards.Clear();
				_selectedCards.Add(target.AbilityCard);
			}
			UpdateSelectedCards();
			_params.OnSelectedCardsUpdated?.Invoke(_params.PlayerActor, _selectedCards);
		}
	}

	private void UpdateSelectedCards()
	{
		UIScenarioCardInteractionEvents events = new UIScenarioCardInteractionEvents
		{
			OnHover = null,
			OnClick = OnCardClicked,
			OnDoubleClick = null,
			OnRightClick = null,
			OnDrag = null
		};
		_availableCardsContainer.Init(_params.ChoiceCards, _params.PlayerActor, ECharacter.Drifter, events, _selectedCards);
		_cancelBuffHintText.enabled = _params.CancelRoundBonusCondition?.Invoke(_selectedCards) ?? false;
	}
}
