using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using Libs.GameConsole;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class FanHandManager : Singleton<FanHandManager>
{
	[Serializable]
	public struct RectWidth
	{
		public RectTransform RectTransform;

		public float CompactWidth;

		public float ExpandedWidth;
	}

	[ConsoleVariable(Alias = "use_fan")]
	public static bool ConsoleVar_UseFan;

	[SerializeField]
	private GameObject _fanHandPrefab;

	[SerializeField]
	private RectTransform _fanHandsContainer;

	[Header("Fan Settings")]
	[SerializeField]
	private FanHandRenderer.FanSettings _settings;

	[SerializeField]
	private RectTransform _cardAnchorReferencePoint;

	[SerializeField]
	private RectTransform _selectedCardsContainer;

	[SerializeField]
	private RectTransform _mouseCaptureArea;

	[SerializeField]
	private RectTransform _mouseFanExtendArea;

	[SerializeField]
	private RectTransform[] _mouseAreaBlockers;

	[Header("Button Toggles")]
	[SerializeField]
	private RectTransform _containerButtons;

	[SerializeField]
	private RectWidth _containerButtonsWidth;

	[SerializeField]
	private List<RectWidth> _lockedRectWidths = new List<RectWidth>();

	[SerializeField]
	private FHToggle _toggleCardDeck;

	[SerializeField]
	private FHToggle _toggleLongRest;

	[SerializeField]
	private FHToggle _toggleShortRest;

	[SerializeField]
	private FHToggle _toggleActiveCards;

	[SerializeField]
	private FHToggle _toggleGraveyard;

	[SerializeField]
	private FHToggle _toggleGeminateForm;

	[Header("Character Specific")]
	[SerializeField]
	private Sprite _spriteGeminateMeleeForm;

	[SerializeField]
	private Sprite _spriteGeminateRangedForm;

	[Header("Dialogs")]
	[SerializeField]
	private ShortRest _dialogShortRest;

	private readonly Dictionary<CardsHandUI, FanHandRenderer> _handRenderers = new Dictionary<CardsHandUI, FanHandRenderer>();

	private readonly List<FHToggle> _toggles = new List<FHToggle>();

	private readonly SmoothFloat _currentContainerWidth = new SmoothFloat();

	private readonly SmoothFloat _currentContainerOffset = new SmoothFloat();

	private void Start()
	{
		_toggleLongRest.onValueChanged.AddListener(OnLongRestValueChanged);
		_toggleShortRest.onValueChanged.AddListener(OnShortRestValueChanged);
		_toggleGeminateForm.onValueChanged.AddListener(OnGeminateFormChanged);
		RegisterToggle(_toggleCardDeck);
		RegisterToggle(_toggleLongRest);
		RegisterToggle(_toggleShortRest);
		RegisterToggle(_toggleActiveCards);
		RegisterToggle(_toggleGraveyard);
		RegisterToggle(_toggleGeminateForm);
	}

	private void OnGeminateFormChanged(bool isOn)
	{
		CardsHandUI currentHand = Singleton<CardsHandManager>.Instance.CurrentHand;
		if (!(currentHand == null) && !(currentHand.PlayerActor.CharacterClass.ID != "GeminateID"))
		{
			CPlayerActor playerActor = currentHand.PlayerActor;
			if ((!FFSNetwork.IsOnline || playerActor == null || (playerActor.IsUnderMyControl() && !Singleton<UIReadyToggle>.Instance.PlayersReady.Contains(PlayerRegistry.MyPlayer) && !Singleton<UIReadyToggle>.Instance.IsProgressingBar)) && (ScenarioManager.CurrentScenarioState.RoundNumber == 1 || currentHand.PlayerActor.Form == CPlayerActor.EForm.Choose) && PhaseManager.IsInCardsSelectionPhase)
			{
				CPlayerActor.EForm targetForm = ((Geminate_GetCurrentIntendedForm(currentHand) == CPlayerActor.EForm.Melee) ? CPlayerActor.EForm.Range : CPlayerActor.EForm.Melee);
				currentHand.SelectGeminateForm(targetForm);
				_toggleGeminateForm.SetIsOnWithoutNotify(value: true);
			}
		}
	}

	private void RegisterToggle(FHToggle toggle)
	{
		_toggles.Add(toggle);
		toggle.onValueChanged.AddListener((bool _) =>
		{
			OnToggleValueChanged(toggle);
		});
	}

	private void OnToggleValueChanged(FHToggle toggle)
	{
		if (toggle.isOn)
		{
			foreach (FHToggle item in _toggles.Where((FHToggle t) => t != toggle))
			{
				item.SetIsOnWithoutNotify(value: false);
			}
		}
		UpdateHandViewMode();
	}

	private void OnLongRestValueChanged(bool isOn)
	{
		CardsHandUI currentHand = Singleton<CardsHandManager>.Instance.CurrentHand;
		if (!(currentHand == null))
		{
			AbilityCardUI abilityCardUI = currentHand.AllCards.FirstOrDefault((AbilityCardUI cui) => cui.IsLongRest);
			if (abilityCardUI != null)
			{
				abilityCardUI.OnClick();
			}
		}
	}

	private void OnShortRestValueChanged(bool isOn)
	{
		if (!(Singleton<CardsHandManager>.Instance.CurrentHand == null))
		{
			_dialogShortRest.MouseClick();
		}
	}

	public void UpdateHandViewMode()
	{
		if (ConsoleVar_UseFan)
		{
			CardsHandUI currentHand = Singleton<CardsHandManager>.Instance.CurrentHand;
			SwitchHand(currentHand);
			SyncHandView(currentHand);
		}
	}

	public void CreateHandFan(CardsHandUI cardsHandUI)
	{
		if (ConsoleVar_UseFan)
		{
			if (_handRenderers.ContainsKey(cardsHandUI))
			{
				Log.GUIWarning("Trying to create a FanHandRenderer for " + cardsHandUI.name + " but it already has one!", this);
				return;
			}
			FanHandRenderer componentInChildren = UnityEngine.Object.Instantiate(_fanHandPrefab, _fanHandsContainer).GetComponentInChildren<FanHandRenderer>();
			componentInChildren.Settings = _settings;
			componentInChildren.CardAnchorReferencePoint = _cardAnchorReferencePoint;
			componentInChildren.SelectedCardsContainer = _selectedCardsContainer;
			componentInChildren.MouseCaptureArea = _mouseCaptureArea;
			componentInChildren.MouseFanExtendArea = _mouseFanExtendArea;
			componentInChildren.MouseAreaBlockers = _mouseAreaBlockers;
			componentInChildren.Init(cardsHandUI);
			_handRenderers.Add(cardsHandUI, componentInChildren);
		}
	}

	public void RemoveHandFan(CardsHandUI cardsHandUI)
	{
		if (_handRenderers.TryGetValue(cardsHandUI, out var value))
		{
			_handRenderers.Remove(cardsHandUI);
			value.RecycleCards();
			UnityEngine.Object.Destroy(value.gameObject);
		}
	}

	public void Reset()
	{
		foreach (FanHandRenderer value in _handRenderers.Values)
		{
			value.RecycleCards();
			UnityEngine.Object.Destroy(value.gameObject);
		}
		_handRenderers.Clear();
	}

	public void SyncHandView(CardsHandUI cardsHandUI)
	{
		if (!(cardsHandUI == null) && ConsoleVar_UseFan)
		{
			if (!_handRenderers.ContainsKey(cardsHandUI))
			{
				CreateHandFan(cardsHandUI);
			}
			ReadTogglesState(cardsHandUI, reset: false);
			bool isImprovedLongRestingWithSelection = cardsHandUI.IsImprovedLongRestingWithSelection;
			FanHandRenderer.FanMode fanMode = FanHandRenderer.FanMode.Hand;
			if (_toggleActiveCards.isOn)
			{
				fanMode = FanHandRenderer.FanMode.ActiveCards;
			}
			else if (_toggleGraveyard.isOn | isImprovedLongRestingWithSelection)
			{
				fanMode = FanHandRenderer.FanMode.Graveyard;
			}
			else if (cardsHandUI.PlayerActor.CharacterClass.ID == "GeminateID")
			{
				bool isOn = _toggleGeminateForm.isOn;
				fanMode = Geminate_GetCurrentIntendedForm(cardsHandUI) switch
				{
					CPlayerActor.EForm.None => FanHandRenderer.FanMode.Empty, 
					CPlayerActor.EForm.Choose => FanHandRenderer.FanMode.Hand, 
					CPlayerActor.EForm.Melee => isOn ? FanHandRenderer.FanMode.GeminateMelee : FanHandRenderer.FanMode.GeminateRanged, 
					CPlayerActor.EForm.Range => isOn ? FanHandRenderer.FanMode.GeminateRanged : FanHandRenderer.FanMode.GeminateMelee, 
					_ => throw new ArgumentOutOfRangeException(), 
				};
			}
			_handRenderers[cardsHandUI].SetFanMode(fanMode);
		}
	}

	public void SwitchHand(CardsHandUI cardsHandUI)
	{
		if (Singleton<CardsHandManager>.Instance.ViewMode != CardHandViewMode.Fan)
		{
			cardsHandUI = null;
			_dialogShortRest.Clear();
		}
		foreach (KeyValuePair<CardsHandUI, FanHandRenderer> item in _handRenderers.ToList())
		{
			if (item.Key == null)
			{
				Log.GUIInfo("Missing fan hand base, was its gameObject destroyed? " + item.Value.name);
				RemoveHandFan(item.Key);
				continue;
			}
			bool flag = item.Key == cardsHandUI;
			bool activeSelf = item.Value.gameObject.activeSelf;
			if (flag != activeSelf)
			{
				item.Value.gameObject.SetActive(flag);
				if (flag)
				{
					ReadTogglesState(cardsHandUI, reset: true);
					_dialogShortRest.Init(cardsHandUI.PlayerActor, cardsHandUI.StartShortRest);
				}
			}
		}
	}

	private void ReadTogglesState(CardsHandUI cardsHandUI, bool reset)
	{
		if (reset)
		{
			_toggles.ForEach((FHToggle toggle) =>
			{
				toggle.SetIsOnWithoutNotify(value: false);
			});
		}
		CCharacterClass characterClass = cardsHandUI.PlayerActor.CharacterClass;
		bool isImprovedLongRestingWithSelection = cardsHandUI.IsImprovedLongRestingWithSelection;
		bool flag = characterClass.DiscardedAbilityCards.Count > 1 && !isImprovedLongRestingWithSelection;
		bool interactable = characterClass.ActivatedAbilityCards.Count > 0 && !isImprovedLongRestingWithSelection;
		bool interactable2 = characterClass.DiscardedAbilityCards.Count > 0 || characterClass.LostAbilityCards.Count > 0;
		bool flag2 = characterClass.ID == "GeminateID";
		_toggleShortRest.interactable = flag && !characterClass.HasShortRested;
		_toggleShortRest.SetIsOnWithoutNotify(cardsHandUI.IsShortRestSelected());
		_toggleLongRest.interactable = flag;
		_toggleLongRest.SetIsOnWithoutNotify(cardsHandUI.IsLongRestSelected());
		_toggleActiveCards.interactable = interactable;
		_toggleGraveyard.interactable = interactable2;
		_toggleGeminateForm.gameObject.SetActive(flag2);
		if (flag2)
		{
			CPlayerActor playerActor = cardsHandUI.PlayerActor;
			bool interactable3 = !FFSNetwork.IsOnline || playerActor == null || (playerActor.IsUnderMyControl() && !Singleton<UIReadyToggle>.Instance.PlayersReady.Contains(PlayerRegistry.MyPlayer) && !Singleton<UIReadyToggle>.Instance.IsProgressingBar);
			_toggleGeminateForm.interactable = interactable3;
			CPlayerActor.EForm eForm = Geminate_GetCurrentIntendedForm(cardsHandUI);
			((Image)_toggleGeminateForm.targetGraphic).sprite = ((eForm == CPlayerActor.EForm.Range) ? _spriteGeminateRangedForm : _spriteGeminateMeleeForm);
			if (reset)
			{
				CPlayerActor.EForm form = cardsHandUI.PlayerActor.Form;
				bool isOnWithoutNotify = form == eForm || form == CPlayerActor.EForm.Choose || form == CPlayerActor.EForm.None;
				_toggleGeminateForm.SetIsOnWithoutNotify(isOnWithoutNotify);
			}
		}
	}

	private void Update()
	{
		UpdateFanButtonsAnimation();
	}

	private void UpdateFanButtonsAnimation()
	{
		CardsHandUI currentHand = Singleton<CardsHandManager>.Instance.CurrentHand;
		bool flag = Singleton<CardsHandManager>.Instance.ViewMode == CardHandViewMode.Fan && PhaseManager.IsInCardsSelectionPhase && !UIManager.Instance.dialogPopup.IsOpen();
		_containerButtons.gameObject.SetActive(flag);
		_fanHandsContainer.gameObject.SetActive(flag);
		if (!flag)
		{
			return;
		}
		float t = Time.deltaTime * _settings.SelectedAnimationLerpTiming;
		float fanAnchoredOffset = _handRenderers[currentHand].GetFanAnchoredOffset();
		bool flag2 = false;
		float value = (flag2 ? _containerButtonsWidth.CompactWidth : Mathf.Max(_containerButtonsWidth.ExpandedWidth, 2f * fanAnchoredOffset));
		_currentContainerWidth.Value = value;
		_currentContainerWidth.Update(t);
		_containerButtons.sizeDelta = new Vector2(_currentContainerWidth.Value, _containerButtons.sizeDelta.y);
		foreach (RectWidth lockedRectWidth in _lockedRectWidths)
		{
			float x = (flag2 ? lockedRectWidth.CompactWidth : lockedRectWidth.ExpandedWidth);
			lockedRectWidth.RectTransform.sizeDelta = new Vector2(x, _containerButtons.sizeDelta.y);
		}
	}

	public static CPlayerActor.EForm Geminate_GetCurrentIntendedForm(CardsHandUI cardsHandUI)
	{
		if ((object)cardsHandUI == null)
		{
			cardsHandUI = Singleton<CardsHandManager>.Instance.CurrentHand;
		}
		if (cardsHandUI == null || cardsHandUI.PlayerActor.CharacterClass.ID != "GeminateID")
		{
			return CPlayerActor.EForm.None;
		}
		CPlayerActor playerActor = cardsHandUI.PlayerActor;
		CPlayerActor.EForm form = playerActor.Form;
		CPlayerActor.EForm prevForm = playerActor.PrevForm;
		CPlayerActor.EForm eForm = playerActor.PendingForm;
		if (eForm == CPlayerActor.EForm.None || eForm == CPlayerActor.EForm.Choose)
		{
			eForm = form;
		}
		if (eForm == CPlayerActor.EForm.None || eForm == CPlayerActor.EForm.Choose)
		{
			eForm = prevForm;
		}
		if (eForm == CPlayerActor.EForm.None || eForm == CPlayerActor.EForm.Choose)
		{
			eForm = CPlayerActor.EForm.Melee;
		}
		return eForm;
	}
}
