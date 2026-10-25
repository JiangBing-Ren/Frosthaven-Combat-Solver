using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIGivenCardsGroup : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private Color highlightedTitleColor;

	[SerializeField]
	private Color defaultTitleColor;

	[SerializeField]
	private TextMeshProUGUI amountText;

	[SerializeField]
	private ExtendedButton headerButton;

	[SerializeField]
	private CanvasGroup cardsContainer;

	[SerializeField]
	private VerticalLayoutGroup verticalLayoutGroup;

	private List<AbilityCardUI> cards = new List<AbilityCardUI>();

	private bool isExpanded;

	public Transform Container => cardsContainer.transform;

	private void Awake()
	{
		headerButton.onMouseEnter.AddListener(() =>
		{
			OnHovered(hovered: true);
		});
		headerButton.onMouseExit.AddListener(() =>
		{
			OnHovered(hovered: true);
		});
		headerButton.onClick.AddListener(Toggle);
		Expand();
	}

	public AbilityCardUI GetOrAddCard(CAbilityCard abilityCard)
	{
		return GetCard(abilityCard) ?? AddCard(abilityCard);
	}

	public AbilityCardUI GetCard(CAbilityCard abilityCard)
	{
		return cards.FirstOrDefault((AbilityCardUI c) => c.AbilityCard == abilityCard || c.AbilityCard.ID == abilityCard.ID);
	}

	public AbilityCardUI AddCard(CAbilityCard abilityCard)
	{
		AbilityCardUI component = ObjectPool.SpawnCard(abilityCard.ID, ObjectPool.ECardType.Ability, cardsContainer.transform, resetLocalScale: true).GetComponent<AbilityCardUI>();
		cards.Add(component);
		RefreshGroupInfo();
		return component;
	}

	public void RemoveCard(AbilityCardUI card)
	{
		if (cards.Remove(card))
		{
			RefreshGroupInfo();
			if (cards.Count == 0)
			{
				Hide();
			}
		}
	}

	public void Show(Func<AbilityCardUI, bool> filter = null)
	{
		if (filter != null)
		{
			foreach (AbilityCardUI item in cards.Where((AbilityCardUI it) => !filter(it)).ToList())
			{
				RemoveCard(item);
			}
		}
		gameObject.SetActive(cards.Count > 0);
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
	}

	public bool IsGivenCard(AbilityCardUI card)
	{
		if (cards != null)
		{
			return cards.Contains(card);
		}
		return false;
	}

	private void RefreshGroupInfo()
	{
		amountText.text = cards?.Count.ToString() ?? string.Empty;
	}

	public void Expand()
	{
		isExpanded = true;
		amountText.enabled = false;
		cardsContainer.alpha = 1f;
		cardsContainer.interactable = true;
		cardsContainer.blocksRaycasts = true;
	}

	public void Srink()
	{
		isExpanded = false;
		amountText.enabled = true;
		cardsContainer.alpha = 0f;
		cardsContainer.interactable = false;
		cardsContainer.blocksRaycasts = false;
	}

	public void Toggle()
	{
		if (isExpanded)
		{
			Srink();
		}
		else
		{
			Expand();
		}
	}

	private void OnHovered(bool hovered)
	{
		title.color = (hovered ? highlightedTitleColor : defaultTitleColor);
	}

	public void PrepareLoseCard()
	{
		verticalLayoutGroup.enabled = false;
	}

	public List<Tuple<RectTransform, float>> GetOrderedCardScrollElementsPosition()
	{
		List<Tuple<RectTransform, float>> list = new List<Tuple<RectTransform, float>>();
		float num = -verticalLayoutGroup.padding.top;
		foreach (RectTransform item in from it in GetOrderedCardScrollElements()
			where it.gameObject.activeSelf
			select it)
		{
			list.Add(new Tuple<RectTransform, float>(item, num - item.sizeDelta.y * item.pivot.y));
			num -= item.sizeDelta.y + verticalLayoutGroup.spacing;
		}
		return list;
	}

	private IEnumerable<RectTransform> GetOrderedCardScrollElements()
	{
		return from it in cards
			select it.transform as RectTransform into it
			orderby it.GetSiblingIndex()
			select it;
	}

	public void ResetLoseCard()
	{
		verticalLayoutGroup.enabled = true;
	}
}
