using System;
using System.Collections.Generic;
using FFSNet;
using GLOOM;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPartyCharacterAbilityCardsDisplay : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI infoText;

	[SerializeField]
	private Image infoIcon;

	[SerializeField]
	private TextMeshProUGUI warningText;

	[SerializeField]
	private GUIAnimator errorAnimator;

	[SerializeField]
	private Color regularTitleColor;

	[SerializeField]
	private Color errorTitleColor;

	[SerializeField]
	private string audioItemShow;

	[SerializeField]
	private string audioItemHide;

	[SerializeField]
	private ScrollRect abilityCardsPanel;

	[SerializeField]
	private RectTransform fullCardAreaToFit;

	[SerializeField]
	private float cardHighlightScale = 1.08f;

	[SerializeField]
	private Vector3 cardHighlightDisplacement = new Vector3(14f, 0f, 0f);

	[SerializeField]
	private float fullCardSpacing = 33f;

	[SerializeField]
	private VerticalPointerUI verticalPointer;

	[SerializeField]
	private GUIAnimator showAnimation;

	[SerializeField]
	private GameObject levelUpHeader;

	[SerializeField]
	private ExtendedButton levelUpButton;

	[SerializeField]
	private GameObject levelUpWarning;

	[SerializeField]
	private GameObject levelUpHighlight;

	private bool isLevelUpValid;

	private Action onLevelup;

	private List<AbilityCardUI> abilityCardsUI = new List<AbilityCardUI>();

	private CMapCharacter characterData;

	private Action<bool> onAbilityCardSelected;

	private Action<bool, AbilityCardUI> onAbilityCardHovered;

	private Color? defaultLevelColor;

	private Action onClosed;

	private Coroutine selectElementCoroutine;

	public bool IsVisible => gameObject.activeSelf;

	private void Awake()
	{
		levelUpButton.onClick.AddListener(() =>
		{
			if (!isLevelUpValid)
			{
				AudioControllerUtils.PlaySound(UIInfoTools.Instance.InvalidOptionAudioItem);
				levelUpWarning.SetActive(value: true);
			}
			else
			{
				AudioControllerUtils.PlaySound(UIInfoTools.Instance.generalAudioButtonProfile.mouseDownAudioItem);
				onLevelup?.Invoke();
			}
		});
		if (!defaultLevelColor.HasValue)
		{
			defaultLevelColor = levelUpButton.buttonText.color;
		}
	}

	public void Display(CMapCharacter characterData, Action<bool> onAbilityCardSelected, RectTransform sourceUI, bool isEditAllowed, bool animate = true, Action onLevelup = null, Action onClosed = null)
	{
		verticalPointer.PointAt(sourceUI);
		abilityCardsPanel.verticalNormalizedPosition = 1f;
		AnalyticsWrapper.LogScreenDisplay(AWScreenName.card_selection);
		this.characterData = characterData;
		this.onAbilityCardSelected = onAbilityCardSelected;
		onAbilityCardHovered = null;
		this.onLevelup = onLevelup;
		this.onClosed = onClosed;
		EnableLevelUp(enable: false);
		foreach (AbilityCardUI item in abilityCardsUI)
		{
			ObjectPool.RecycleCard(item.CardID, ObjectPool.ECardType.Ability, item.gameObject);
		}
		abilityCardsUI.Clear();
		foreach (CAbilityCard ownedAbilityCard in characterData.GetOwnedAbilityCards())
		{
			if (ownedAbilityCard.GetAbilityCardYML == null)
			{
				Log.GUIError("Ability card data is null! Check YML file for " + ownedAbilityCard.Name);
				continue;
			}
			try
			{
				AbilityCardUI component = ObjectPool.SpawnCard(ownedAbilityCard.ID, ObjectPool.ECardType.Ability, abilityCardsPanel.content, resetLocalScale: false, resetToMiddle: true).GetComponent<AbilityCardUI>();
				component.transform.localScale = Vector3.one;
				component.Init(ownedAbilityCard, null, characterData, OnAbilityCardSelect, OnAbilityCardDeselect, OnUnableToSelectAbilityCard, OnAbilityCardHover, cardHighlightScale, cardHighlightDisplacement, fullCardSpacing);
				abilityCardsUI.Add(component);
			}
			catch (Exception exception)
			{
				Log.GUIException(exception, "Unable to initialise ability card " + ownedAbilityCard.Name);
			}
		}
		foreach (AbilityCardUI item2 in abilityCardsUI)
		{
			item2.SetSelectable(FFSNetwork.IsOnline ? (characterData.IsMapCharacterUnderMyControl & isEditAllowed) : isEditAllowed);
		}
		Show(!animate);
		UpdateDisplay();
	}

	public void SetSelectable(bool isEditAllowed)
	{
		if (!IsVisible)
		{
			return;
		}
		foreach (AbilityCardUI item in abilityCardsUI)
		{
			item.SetSelectable(FFSNetwork.IsOnline ? (characterData.IsMapCharacterUnderMyControl & isEditAllowed) : isEditAllowed);
		}
	}

	public void EnableLevelUp(bool enable, bool isValid = true)
	{
		isLevelUpValid = isValid;
		levelUpWarning.SetActive(value: false);
		levelUpButton.targetGraphic.material = ((!isValid) ? UIInfoTools.Instance.disabledGrayscaleMaterial : null);
		if (!defaultLevelColor.HasValue)
		{
			defaultLevelColor = levelUpButton.buttonText.color;
		}
		levelUpButton.buttonText.color = (isValid ? defaultLevelColor.Value : UIInfoTools.DefInstance.greyedOutTextColor);
		levelUpHighlight.SetActive(isValid);
		_ = levelUpHeader.gameObject.activeSelf;
		_ = onLevelup;
		levelUpHeader.gameObject.SetActive((onLevelup != null) & enable);
	}

	public void DisplayCardSelection(CMapCharacter characterData, Action<AbilityCardUI, bool> onAbilityCardSelected, Action<bool, AbilityCardUI> onAbilityCardHover, RectTransform sourceUI, Func<AbilityCardUI, bool> filter, bool autoselectFirst)
	{
		onLevelup = null;
		EnableLevelUp(enable: false);
		verticalPointer.PointAt(sourceUI);
		this.characterData = characterData;
		this.onAbilityCardSelected = null;
		onAbilityCardHovered = onAbilityCardHover;
		foreach (AbilityCardUI item in abilityCardsUI)
		{
			ObjectPool.RecycleCard(item.CardID, ObjectPool.ECardType.Ability, item.gameObject);
		}
		abilityCardsUI.Clear();
		AbilityCardUI selectedCard = null;
		foreach (CAbilityCard ownedAbilityCard in characterData.GetOwnedAbilityCards())
		{
			if (ownedAbilityCard.GetAbilityCardYML == null)
			{
				Log.GUIError("Ability card data is null! Check YML file for " + ownedAbilityCard.Name);
				continue;
			}
			try
			{
				AbilityCardUI component = ObjectPool.SpawnCard(ownedAbilityCard.ID, ObjectPool.ECardType.Ability, abilityCardsPanel.content, resetLocalScale: false, resetToMiddle: true).GetComponent<AbilityCardUI>();
				component.transform.localScale = Vector3.one;
				component.Init(ownedAbilityCard, null, characterData, (AbilityCardUI cardUI, bool networkAction) =>
				{
					if (selectedCard != cardUI)
					{
						onAbilityCardSelected(cardUI, networkAction);
						AbilityCardUI abilityCardUI = selectedCard;
						selectedCard = cardUI;
						abilityCardUI?.ToggleSelect(active: false, highlight: false, networkAction);
					}
				}, (AbilityCardUI cardUI, bool networkAction) =>
				{
					if (selectedCard != null && selectedCard == cardUI)
					{
						cardUI.OnClick();
					}
				}, OnUnableToSelectAbilityCard, OnAbilityCardHover, cardHighlightScale, cardHighlightDisplacement, fullCardSpacing, disableFullCard: true);
				abilityCardsUI.Add(component);
			}
			catch (Exception exception)
			{
				Log.GUIException(exception, "Unable to initialise ability card " + ownedAbilityCard.Name);
			}
		}
		abilityCardsPanel.verticalNormalizedPosition = 1f;
		foreach (AbilityCardUI item2 in abilityCardsUI)
		{
			bool flag = (filter != null && !filter(item2)) || !FFSNetwork.IsOnline || characterData.IsMapCharacterUnderMyControl;
			item2.SetSelectable(flag);
			if (!flag)
			{
				item2.SetType(CardPileType.Discarded);
				item2.UpdateCard();
			}
			else if (autoselectFirst)
			{
				autoselectFirst = false;
				item2.OnClick();
			}
		}
		if (autoselectFirst)
		{
			onAbilityCardSelected(null, arg2: true);
		}
		Show();
		UpdateDisplay();
	}

	private void CancelAnimations()
	{
		errorAnimator.Stop();
	}

	public void Show(bool instant = false, Action callback = null)
	{
		CancelAnimations();
		gameObject.SetActive(value: true);
		AudioControllerUtils.PlaySound(audioItemShow);
		if (instant)
		{
			showAnimation.GoToFinishState();
		}
		else
		{
			showAnimation.Play();
		}
	}

	private void UpdateDisplay()
	{
		TextMeshProUGUI textMeshProUGUI = infoText;
		string text = (warningText.text = characterData.HandAbilityCardIDs.Count + " / " + characterData.MaxCards + " " + LocalizationManager.GetTranslation("GUI_EQUIPPED"));
		textMeshProUGUI.text = text;
		TextMeshProUGUI textMeshProUGUI2 = infoText;
		Color color = (infoIcon.color = ((characterData.HandAbilityCardIDs.Count == characterData.MaxCards) ? regularTitleColor : errorTitleColor));
		textMeshProUGUI2.color = color;
		foreach (AbilityCardUI item in abilityCardsUI)
		{
			item.PlayEffect(InitiativeTrack.InitiativeEffects.None);
		}
	}

	private void OnUnableToSelectAbilityCard(AbilityCardUI cardUI)
	{
		TextMeshProUGUI textMeshProUGUI = infoText;
		Color color = (infoIcon.color = regularTitleColor);
		textMeshProUGUI.color = color;
		errorAnimator.Play();
		cardUI.MiniAbilityCard.ShowWarning(show: true);
	}

	private void OnAbilityCardSelect(AbilityCardUI cardUI, bool networkAction = true)
	{
		if (!characterData.HandAbilityCardIDs.Contains(cardUI.AbilityCard.ID))
		{
			characterData.HandAbilityCardIDs.Add(cardUI.AbilityCard.ID);
		}
		UpdateDisplay();
		onAbilityCardSelected(obj: true);
		if ((FFSNetwork.IsOnline && characterData.IsMapCharacterUnderMyControl) & networkAction)
		{
			int controllableID = ControllableRegistry.GetControllableID(characterData);
			Synchronizer.ReplicateControllableStateChange(GameActionType.ModifyCardInventory, ActionProcessor.CurrentPhase, controllableID, 0, 0, 0, supplementaryDataBoolean: false, null, new CardInventoryToken(characterData.HandAbilityCardIDs, characterData.OwnedAbilityCardIDs));
		}
		Log.SimpleInfo("MapCharacter: " + characterData.CharacterID + " selected card: " + cardUI.CardName);
	}

	private void OnAbilityCardDeselect(AbilityCardUI cardUI, bool networkAction = true)
	{
		if (characterData.HandAbilityCardIDs.Contains(cardUI.AbilityCard.ID))
		{
			TextMeshProUGUI textMeshProUGUI = infoText;
			Color color = (infoIcon.color = regularTitleColor);
			textMeshProUGUI.color = color;
			characterData.HandAbilityCardIDs.Remove(cardUI.AbilityCard.ID);
			errorAnimator.Stop();
			cardUI.MiniAbilityCard.ShowWarning(show: false);
		}
		UpdateDisplay();
		onAbilityCardSelected(obj: false);
		if ((FFSNetwork.IsOnline && characterData.IsMapCharacterUnderMyControl) & networkAction)
		{
			int controllableID = ControllableRegistry.GetControllableID(characterData);
			Synchronizer.ReplicateControllableStateChange(GameActionType.ModifyCardInventory, ActionProcessor.CurrentPhase, controllableID, 0, 0, 0, supplementaryDataBoolean: false, null, new CardInventoryToken(characterData.HandAbilityCardIDs, characterData.OwnedAbilityCardIDs));
		}
		Log.SimpleInfo("MapCharacter: " + characterData.CharacterID + " deselected card: " + cardUI.CardName);
	}

	private void OnAbilityCardHover(bool isActive, AbilityCardUI cardUI)
	{
		if (isActive)
		{
			if (Singleton<InputFHManager>.Instance.UsingGamepad)
			{
				abilityCardsPanel.ScrollVerticallyToTop(cardUI.MiniAbilityCard.transform as RectTransform);
			}
			cardUI.fullAbilityCard.transform.position += cardUI.fullAbilityCard.GetComponent<RectTransform>().DeltaWorldPositionToFitRectTransform(UIManager.Instance.UICamera, fullCardAreaToFit);
		}
		onAbilityCardHovered?.Invoke(isActive, cardUI);
	}

	public void Hide(bool playHideAudio = true)
	{
		if (!gameObject.activeSelf)
		{
			return;
		}
		CancelAnimations();
		if (gameObject.activeSelf & playHideAudio)
		{
			AudioControllerUtils.PlaySound(audioItemHide);
		}
		gameObject.SetActive(value: false);
		foreach (AbilityCardUI item in abilityCardsUI)
		{
			ObjectPool.RecycleCard(item.CardID, ObjectPool.ECardType.Ability, item.gameObject);
		}
		abilityCardsUI.Clear();
		onClosed?.Invoke();
	}

	private void OnDisable()
	{
		if (!GloomUtility.IsAppQuitting)
		{
			CancelAnimations();
		}
	}

	private void OnDestroy()
	{
		if (GloomUtility.IsAppQuitting)
		{
			return;
		}
		foreach (AbilityCardUI item in abilityCardsUI)
		{
			ObjectPool.RecycleCard(item.CardID, ObjectPool.ECardType.Ability, item.gameObject);
		}
	}
}
