using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConsumeButton : MonoBehaviour
{
	[SerializeField]
	private GameObject Trophy;

	[SerializeField]
	private GameObject Resonance;

	[SerializeField]
	private ConsumeElement Element1;

	[SerializeField]
	private ConsumeElement Element2;

	[SerializeField]
	private GameObject Colon;

	[SerializeField]
	public GameObject Description;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private UIElementHighlight highlight;

	[SerializeField]
	private Image selectedImage;

	[SerializeField]
	private float selectedOpacity = 0.8f;

	[SerializeField]
	private float unselectedOpacity = 0.5f;

	[SerializeField]
	public int abilityCardID;

	[SerializeField]
	[ReadOnlyField]
	private string consumeName;

	[SerializeField]
	private bool m_ManuallyGenerateLayout;

	[SerializeField]
	private bool m_UseDescription = true;

	[NonSerialized]
	public static float colonWidth;

	[NonSerialized]
	public static float colonToDescSpacer;

	private const string IconPath = "GUI/Icons";

	public CreateLayout layout;

	private bool isSelected;

	private bool isEnabled;

	private bool isHovered;

	private bool isAvailable;

	private bool m_IsAbilityCardConsume;

	private bool m_IsAbilityCardTopButton;

	private int m_ConsumeButtonIndex;

	private bool m_IsLayoutGenerated;

	private AbilityConsume _abilityConsume;

	private GameObject _gameObject;

	public string ConsumeName => consumeName;

	public int ConsumeButtonIndex => m_ConsumeButtonIndex;

	public bool IsAbilityCardTopButton => m_IsAbilityCardTopButton;

	public bool IsSelected => isSelected;

	public bool IsUsingDescription => m_UseDescription;

	public bool IsUsingBothElements => Element1.IsInitialised;

	public ElementInfusionBoardManager.EElement SelectedElement1 => Element1.SelectedElement;

	public ElementInfusionBoardManager.EElement SelectedElement2 => Element2.SelectedElement;

	public AbilityConsume AbilityConsume
	{
		get
		{
			if (Application.isPlaying)
			{
				LoadAbilityConsume();
				if (_abilityConsume == null && !string.IsNullOrEmpty(consumeName))
				{
					Log.GUIError("Unable to initialise consume button for ability card ID " + abilityCardID + ", consume name: " + consumeName);
				}
			}
			return _abilityConsume;
		}
	}

	public string ID => $"{abilityCardID}_{(m_IsAbilityCardTopButton ? 1 : 4)}_{m_ConsumeButtonIndex}";

	public bool TryFindAbilityConsumeInYMLData()
	{
		IAbilityCardUI componentInParent = GetComponentInParent<IAbilityCardUI>(includeInactive: true);
		if ((componentInParent != null && componentInParent.CardType == ObjectPool.ECardType.Monster) || (bool)GetComponentInParent<MonsterCardActionUI>(includeInactive: true))
		{
			CActionAugmentation cActionAugmentation = ScenarioRuleClient.SRLYML.MonsterCards.SingleOrDefault((MonsterCardYMLData x) => x.ID == abilityCardID)?.CardAction.GetAllConsumeData().SingleOrDefault((CActionAugmentation x) => x.Name == consumeName);
			if (cActionAugmentation == null)
			{
				return false;
			}
			_abilityConsume = new AbilityConsume(cActionAugmentation);
			return true;
		}
		AbilityCardYMLData abilityCardYMLData = ScenarioRuleClient.SRLYML.AbilityCards?.SingleOrDefault((AbilityCardYMLData x) => x.ID == abilityCardID);
		if (abilityCardYMLData == null)
		{
			return false;
		}
		_abilityConsume = abilityCardYMLData.FindConsumeByName(consumeName);
		return _abilityConsume != null;
	}

	private void LoadAbilityConsume()
	{
		if (_abilityConsume == null)
		{
			if (TryFindAbilityConsumeInYMLData())
			{
				RefreshElements();
			}
			else
			{
				Log.GUIError($"Could not load AbilityConsume data for id=[{abilityCardID}] consumeName=[{consumeName}]", this);
			}
		}
	}

	private void Start()
	{
		LoadAbilityConsume();
		if (m_ManuallyGenerateLayout && !m_IsLayoutGenerated)
		{
			Init(_abilityConsume, abilityCardID, (transform as RectTransform).sizeDelta.x);
			GenerateLayout();
		}
	}

	public void Init(AbilityConsume consume, int abilityID, float width)
	{
		colonWidth = GlobalSettings.Instance.m_AbilityCardSettings.ElementIconSize / 6f;
		colonToDescSpacer = GlobalSettings.Instance.m_AbilityCardSettings.ElementIconSize / 5f;
		abilityCardID = abilityID;
		consumeName = consume.Name;
		isSelected = false;
		_abilityConsume = consume;
		RectTransform obj = transform as RectTransform;
		obj.sizeDelta = new Vector2(width, GlobalSettings.Instance.m_AbilityCardSettings.ElementIconSize);
		RectTransform obj2 = Element1.transform as RectTransform;
		obj2.anchorMin = new Vector2(0f, 0.5f);
		obj2.anchorMax = new Vector2(0f, 0.5f);
		obj2.sizeDelta = new Vector2(GlobalSettings.Instance.m_AbilityCardSettings.ElementIconSize, GlobalSettings.Instance.m_AbilityCardSettings.ElementIconSize);
		RectTransform obj3 = Element2.transform as RectTransform;
		obj3.anchorMin = new Vector2(0f, 0.5f);
		obj3.anchorMax = new Vector2(0f, 0.5f);
		obj3.sizeDelta = new Vector2(GlobalSettings.Instance.m_AbilityCardSettings.ElementIconSize, GlobalSettings.Instance.m_AbilityCardSettings.ElementIconSize);
		obj3.anchoredPosition = new Vector2(GlobalSettings.Instance.m_AbilityCardSettings.ElementIconSize, 0f);
		Trophy.SetActive(consume.ConsumeData.Trohpies > 0);
		Resonance.SetActive(consume.ConsumeData.Resonance > 0);
		if (consume.ConsumeData.Resonance > 0)
		{
			GetComponentInChildren<TextMeshProUGUI>().text = (-1 * consume.ConsumeData.Resonance).ToString();
		}
		if (consume.ConsumeData.Elements.Count > 0)
		{
			Element2.Init(consume.ConsumeData.Elements[0]);
			if (consume.ConsumeData.Elements.Count == 2)
			{
				Element1.Init(consume.ConsumeData.Elements[1], 1);
			}
			else
			{
				Element1.gameObject.SetActive(value: false);
			}
			colonWidth = GlobalSettings.Instance.m_AbilityCardSettings.ElementIconSize / 6f;
			colonToDescSpacer = GlobalSettings.Instance.m_AbilityCardSettings.ElementIconSize / 5f;
			RectTransform obj4 = Colon.transform as RectTransform;
			obj4.sizeDelta = new Vector2(colonWidth, GlobalSettings.Instance.m_AbilityCardSettings.ElementIconSize);
			obj4.anchoredPosition = new Vector2(GlobalSettings.Instance.m_AbilityCardSettings.ElementIconSize * 2f, 0f);
			obj4.anchorMin = new Vector2(0f, 0.5f);
			obj4.anchorMax = new Vector2(0f, 0.5f);
			obj4.gameObject.SetActive(value: true);
		}
		else
		{
			Element1.gameObject.SetActive(value: false);
			Element2.gameObject.SetActive(value: false);
			colonWidth = 0f;
			colonToDescSpacer = 0f;
			RectTransform obj5 = Colon.transform as RectTransform;
			obj5.sizeDelta = new Vector2(0f, 0f);
			obj5.gameObject.SetActive(value: false);
		}
		RectTransform obj6 = Description.transform as RectTransform;
		obj6.anchorMin = new Vector2(0f, 0.5f);
		obj6.anchorMax = new Vector2(0f, 0.5f);
		float width2 = obj.rect.width - (GlobalSettings.Instance.m_AbilityCardSettings.ElementIconSize * (float)consume.ConsumeData.Elements.Count + colonWidth + colonToDescSpacer);
		if (consume.Text != null)
		{
			layout = new CreateLayout(consume.Text.ParentGroup, new Rect(0f, 0f, width2, Screen.height), abilityID, isLongRest: false, inConsume: true);
		}
	}

	public void GenerateLayout(bool monsterCard = false)
	{
		RectTransform rectTransform = layout.FullLayout.transform as RectTransform;
		float fullLayoutHeight = layout.GetFullLayoutHeight();
		rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, fullLayoutHeight);
		layout.GenerateFullLayout(scaleHeight: false, scaleWidth: true, shrinkWidthToFit: true);
		layout.FullLayout.transform.SetParent(Description.transform);
		rectTransform.anchoredPosition = Vector2.zero;
		RectTransform rectTransform2 = Description.transform as RectTransform;
		rectTransform2.sizeDelta = new Vector2(rectTransform.rect.width, fullLayoutHeight);
		rectTransform2.anchoredPosition = new Vector2(GlobalSettings.Instance.m_AbilityCardSettings.ElementIconSize * 2f + colonWidth + colonToDescSpacer, 0f);
		RectTransform obj = transform as RectTransform;
		obj.sizeDelta = new Vector2(0f, (fullLayoutHeight > GlobalSettings.Instance.m_AbilityCardSettings.ElementIconSize) ? fullLayoutHeight : GlobalSettings.Instance.m_AbilityCardSettings.ElementIconSize);
		float num = (Trophy.gameObject.activeInHierarchy ? (Trophy.transform as RectTransform).sizeDelta.x : 0f);
		float num2 = (Resonance.gameObject.activeInHierarchy ? (Resonance.transform as RectTransform).sizeDelta.x : 0f);
		float num3 = num + num2 + GlobalSettings.Instance.m_AbilityCardSettings.ElementIconSize * (float)_abilityConsume.ConsumeData.Elements.Count + colonWidth + colonToDescSpacer + rectTransform2.rect.width;
		float width = (obj.parent.transform as RectTransform).rect.width;
		float num4 = (monsterCard ? 0f : (num + num2 + (width - num3) / 2f - GlobalSettings.Instance.m_AbilityCardSettings.ElementIconSize * (float)((_abilityConsume.ConsumeData.Elements.Count > 0) ? 1 : 0) / 16f));
		RectTransform rectTransform3 = Element1.transform as RectTransform;
		RectTransform rectTransform4 = Element2.transform as RectTransform;
		RectTransform rectTransform5 = Colon.transform as RectTransform;
		if (Trophy.gameObject.activeInHierarchy)
		{
			(Trophy.transform as RectTransform).anchoredPosition = new Vector2(num4 - num, 0f);
		}
		if (Resonance.gameObject.activeInHierarchy)
		{
			(Resonance.transform as RectTransform).anchoredPosition = new Vector2(num4 - num2, 0f);
		}
		if (Element1.gameObject.activeInHierarchy)
		{
			rectTransform3.anchoredPosition = new Vector2(num4, 0f);
			rectTransform4.anchoredPosition = new Vector2(num4 + rectTransform3.rect.width, 0f);
			rectTransform5.anchoredPosition = new Vector2(num4 + rectTransform3.rect.width + rectTransform4.rect.width, 0f);
			rectTransform2.anchoredPosition = new Vector2(num4 + rectTransform3.rect.width + rectTransform4.rect.width + rectTransform5.rect.width + colonToDescSpacer, 0f);
		}
		else if (Element2.gameObject.activeInHierarchy)
		{
			rectTransform4.anchoredPosition = new Vector2(num4, 0f);
			rectTransform5.anchoredPosition = new Vector2(num4 + rectTransform4.rect.width, 0f);
			rectTransform2.anchoredPosition = new Vector2(num4 + rectTransform4.rect.width + rectTransform5.rect.width + colonToDescSpacer, 0f);
		}
		else
		{
			rectTransform2.anchoredPosition = new Vector2(num4, 0f);
		}
		GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0.5f);
		GetComponent<RectTransform>().anchorMax = new Vector2(1f, 0.5f);
		ConsumeElement[] array = new ConsumeElement[2] { Element1, Element2 };
		for (int i = 0; i < array.Length; i++)
		{
			RectTransform rectTransform6 = array[i].transform as RectTransform;
			rectTransform6.pivot = new Vector2(0.5f, 0.5f);
			rectTransform6.position = new Vector3(rectTransform6.position.x + rectTransform6.rect.width / 2f, rectTransform6.position.y, rectTransform6.position.z);
		}
		UpdateView(fullAlpha: true);
		m_IsLayoutGenerated = true;
	}

	public bool CanConsumeRequiredElements(List<ElementInfusionBoardManager.EElement> availableElements)
	{
		if (availableElements == null)
		{
			return true;
		}
		if (AbilityConsume == null)
		{
			return false;
		}
		List<ElementInfusionBoardManager.EElement> elements = AbilityConsume.ConsumeData.Elements;
		if (elements.Count > availableElements.Count)
		{
			return false;
		}
		foreach (ElementInfusionBoardManager.EElement item in elements)
		{
			if (item != ElementInfusionBoardManager.EElement.Any && !availableElements.Contains(item))
			{
				return false;
			}
		}
		return true;
	}

	public void SetupForAbilityCard(bool isTopAction, int p_AbilityCardID, int buttonIndex)
	{
		m_IsAbilityCardConsume = true;
		m_IsAbilityCardTopButton = isTopAction;
		abilityCardID = p_AbilityCardID;
		m_ConsumeButtonIndex = buttonIndex;
	}

	public void ShowSelected(List<ElementInfusionBoardManager.EElement> element)
	{
		isSelected = true;
		if (Element1.IsInitialised)
		{
			Element1.SetSelected(element[1]);
		}
		if (Element2.IsInitialised)
		{
			Element2.SetSelected(element[0]);
		}
		RefreshHighlight();
	}

	public void ClearSelection()
	{
		isSelected = false;
		if (Element1.IsInitialised)
		{
			Element1.SetUnselected();
		}
		if (Element2.IsInitialised)
		{
			Element2.SetUnselected();
		}
		RefreshHighlight();
	}

	public void Enable()
	{
		bool flag = SaveData.Instance == null || SaveData.Instance.Global.CurrentGameState != EGameState.Scenario;
		if (flag || PhaseManager.IsInCardsSelectionPhase)
		{
			ResetState(clearSelection: true, flag);
			return;
		}
		isEnabled = true;
		if (AbilityConsume.ConsumeData.Elements.Count > 0)
		{
			if (!Element2.IsInitialised)
			{
				Element2.Init(AbilityConsume.ConsumeData.Elements[0]);
			}
			if (AbilityConsume.ConsumeData.Elements.Count == 2 && !Element1.IsInitialised)
			{
				Element1.Init(AbilityConsume.ConsumeData.Elements[1]);
			}
		}
		UpdateView();
	}

	public void ResetState(bool clearSelection = true, bool overrideAvailable = false, bool skipElementCheck = false)
	{
		isHovered = false;
		bool available = overrideAvailable || (!skipElementCheck && CanConsumeRequiredElements(InfusionBoardUI.Instance?.GetAvailableElements()));
		SetAvailable(available);
		if (clearSelection)
		{
			ClearSelection();
		}
		Disable();
	}

	public void Disable()
	{
		isEnabled = false;
		UpdateView();
	}

	public void SetAvailable(bool isAvailable)
	{
		this.isAvailable = isAvailable;
		UpdateView();
	}

	public void SetHovered(bool hovered)
	{
		isHovered = hovered;
		RefreshHighlight();
	}

	private void UpdateView(bool fullAlpha = false)
	{
		canvasGroup.alpha = ((fullAlpha || isAvailable) ? 1f : 0.5f);
		if (Element1.IsInitialised)
		{
			Element1.SetEnabled(isEnabled);
			Element1.SetAvailable(isAvailable);
		}
		if (Element2.IsInitialised)
		{
			Element2.SetEnabled(isEnabled);
			Element2.SetAvailable(isAvailable);
		}
		RefreshHighlight();
	}

	private void RefreshHighlight()
	{
		if (highlight == null)
		{
			return;
		}
		highlight.gameObject.SetActive(isEnabled && (isHovered || isSelected) && Element2.IsInitialised);
		if (highlight.gameObject.activeSelf)
		{
			List<ElementInfusionBoardManager.EElement> list = new List<ElementInfusionBoardManager.EElement>();
			if (Element2.IsInitialised)
			{
				list.Add(Element2.SelectedElement);
			}
			if (Element1.IsInitialised)
			{
				list.Add(Element1.SelectedElement);
			}
			highlight.Highlight(list);
		}
	}

	private void RefreshElements()
	{
		if (_abilityConsume == null)
		{
			return;
		}
		if (_abilityConsume.ConsumeData.Elements.Count > 0)
		{
			Element2.gameObject.SetActive(value: true);
			Element2.Init(_abilityConsume.ConsumeData.Elements[0]);
			if (_abilityConsume.ConsumeData.Elements.Count == 2)
			{
				Element1.gameObject.SetActive(value: true);
				Element1.Init(_abilityConsume.ConsumeData.Elements[1], 1);
			}
			else
			{
				Element1.gameObject.SetActive(value: false);
			}
		}
		else
		{
			Element1.gameObject.SetActive(value: false);
			Element2.gameObject.SetActive(value: false);
		}
	}
}
