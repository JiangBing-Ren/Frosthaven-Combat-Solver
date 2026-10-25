using System.Collections.Generic;
using FFSNet;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIAdventurePartyAssemblyRosterSlot : MonoBehaviour
{
	public class PartyRosterEvent : UnityEvent<CMapCharacter>
	{
	}

	[SerializeField]
	private ExtendedButton button;

	[SerializeField]
	private Image portrait;

	[SerializeField]
	private Image selected;

	[SerializeField]
	private Image classIcon;

	[SerializeField]
	private GameObject removePanel;

	[SerializeField]
	private Image lockedIcon;

	[SerializeField]
	private string audioItemSelect;

	[SerializeField]
	private string audioItemUnselect;

	[SerializeField]
	private List<UICharacterInformation> extraInformations;

	private CMapCharacter character;

	[Header("Hover")]
	[SerializeField]
	private RectTransform animationRect;

	[SerializeField]
	private Vector2 hoverFactor = Vector2.one;

	private Vector2 portraitPivot;

	private Vector2 anchorsPortraitMax;

	private Vector2 anchorsPortraitMin;

	private Vector2 rectSize;

	public PartyRosterEvent OnCharacterSelected = new PartyRosterEvent();

	public PartyRosterEvent OnCharacterDeselected = new PartyRosterEvent();

	public PartyRosterEvent OnCharacterHover = new PartyRosterEvent();

	public PartyRosterEvent OnCharacterUnhover = new PartyRosterEvent();

	private Canvas canvas;

	private int sortingOrder;

	private bool isSelected;

	private bool isHovered;

	private UITextTooltipTarget tooltipTarget;

	private HashSet<Component> disableInteractionRequests = new HashSet<Component>();

	public CMapCharacter Character => character;

	public bool IsLocked { get; private set; }

	public bool IsInteractable => button.IsInteractable();

	private void Awake()
	{
		canvas = animationRect.GetComponent<Canvas>();
		sortingOrder = canvas.sortingOrder;
		anchorsPortraitMax = portrait.rectTransform.anchorMax;
		anchorsPortraitMin = portrait.rectTransform.anchorMin;
		rectSize = animationRect.sizeDelta;
		portraitPivot = portrait.rectTransform.pivot;
		button.onMouseEnter.AddListener(OnPointerEnter);
		button.onMouseExit.AddListener(OnPointerExit);
		button.onClick.AddListener(Click);
		tooltipTarget = GetComponent<UITextTooltipTarget>();
	}

	public void Init(CMapCharacter characterOption, bool selected = false, bool locked = false)
	{
		character = characterOption;
		classIcon.sprite = UIInfoTools.Instance.GetCharacterMarker(character.CharacterYMLData.Model, character.CharacterYMLData.CustomCharacterConfig);
		portrait.sprite = UIInfoTools.Instance.GetCharacterAssemblySprite(character.CharacterYMLData.Model, characterOption.CharacterYMLData.CustomCharacterConfig);
		portrait.rectTransform.anchoredPosition = UIInfoTools.Instance.GetCharacterConfigUI(Character.CharacterYMLData.Model).rosterPortraitOffset;
		isHovered = false;
		foreach (UICharacterInformation extraInformation in extraInformations)
		{
			extraInformation.Display(characterOption);
		}
		ClearEvents();
		SetLocked(locked);
		SetSelected(selected);
		if (removePanel != null)
		{
			removePanel.SetActive(value: false);
		}
		button.interactable = true;
		disableInteractionRequests.Clear();
	}

	public void SetSelected(bool isSelected)
	{
		this.isSelected = isSelected;
		selected.enabled = isSelected;
		RefreshRemove();
	}

	private void RefreshRemove()
	{
		if (removePanel != null)
		{
			removePanel.SetActive(isSelected && isHovered && !IsLocked);
		}
	}

	public void DisableOption(bool disable, Component request = null)
	{
		if (disable)
		{
			disableInteractionRequests.Add((request == null) ? this : request);
		}
		else
		{
			disableInteractionRequests.Remove((request == null) ? this : request);
		}
		button.interactable = disableInteractionRequests.Count == 0;
		Image image = portrait;
		Color color = (classIcon.color = (button.interactable ? Color.white : Color.gray));
		image.color = color;
	}

	public void DetermineInteractability(CMapCharacter currentlySlottedCharacter)
	{
		NetworkPlayer controller = ControllableRegistry.GetController((int)Character.CharacterYMLData.Model);
		bool flag = !FFSNetwork.IsOnline || currentlySlottedCharacter == Character || ((controller.In(null, PlayerRegistry.MyPlayer) || AdventureState.MapState.MapParty.UnselectedCharacters.Contains(character)) && (currentlySlottedCharacter?.IsMapCharacterUnderMyControl ?? true));
		if (FFSNetwork.IsOnline && UIAdventurePartyAssemblyWindow.IsChangingCharacter)
		{
			flag = false;
		}
		bool flag2 = !FFSNetwork.IsOnline || (FFSNetwork.IsHost && (ActionProcessor.CurrentPhase != ActionPhaseType.MapHQ || PlayerRegistry.JoiningPlayers.Count == 0)) || (FFSNetwork.IsClient && (ActionProcessor.CurrentPhase != ActionPhaseType.MapHQ || !PlayerRegistry.OtherClientsAreJoining));
		if (flag2)
		{
			tooltipTarget.TooltipEnabled = false;
			tooltipTarget.HideTooltip(0f);
		}
		else
		{
			tooltipTarget.TooltipEnabled = true;
		}
		flag &= flag2;
		DisableOption(!flag);
	}

	public void SetLocked(bool isLocked, bool showLockImage = false)
	{
		IsLocked = isLocked;
		lockedIcon.enabled = isLocked & showLockImage;
		RefreshRemove();
	}

	public void Click()
	{
		if (IsLocked)
		{
			OnCharacterSelected?.Invoke(character);
		}
		else if (!isSelected)
		{
			AudioControllerUtils.PlaySound(audioItemSelect);
			OnCharacterSelected?.Invoke(character);
		}
		else
		{
			AudioControllerUtils.PlaySound(audioItemUnselect);
			OnCharacterDeselected?.Invoke(character);
		}
	}

	public void OnPointerEnter()
	{
		if (button.interactable)
		{
			isHovered = true;
			Vector2 size = portrait.rectTransform.rect.size;
			animationRect.sizeDelta = rectSize + hoverFactor;
			portrait.rectTransform.pivot = new Vector2(portraitPivot.x, (portrait.rectTransform.anchorMax.y + portrait.rectTransform.anchorMin.y) / 2f);
			RectTransform rectTransform = portrait.rectTransform;
			Vector2 anchorMax = (portrait.rectTransform.anchorMin = new Vector2(portrait.rectTransform.anchorMax.x, 0.5f));
			rectTransform.anchorMax = anchorMax;
			portrait.rectTransform.sizeDelta = size;
			canvas.sortingOrder = sortingOrder + 1;
			RefreshRemove();
			OnCharacterHover?.Invoke(character);
		}
	}

	public void OnPointerExit()
	{
		isHovered = false;
		Unhighlight();
		OnCharacterUnhover?.Invoke(character);
	}

	private void Unhighlight()
	{
		animationRect.sizeDelta = rectSize;
		portrait.rectTransform.pivot = portraitPivot;
		portrait.rectTransform.anchorMax = anchorsPortraitMax;
		portrait.rectTransform.anchorMin = anchorsPortraitMin;
		portrait.rectTransform.sizeDelta = Vector2.zero;
		canvas.sortingOrder = sortingOrder;
		RefreshRemove();
	}

	public void ClearEvents()
	{
		OnCharacterSelected.RemoveAllListeners();
		OnCharacterUnhover.RemoveAllListeners();
		OnCharacterHover.RemoveAllListeners();
		OnCharacterDeselected.RemoveAllListeners();
	}
}
