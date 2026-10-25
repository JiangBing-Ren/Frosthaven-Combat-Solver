using ScenarioRuleLibrary;
using UnityEngine;

public class UIItemTooltip : MonoBehaviour
{
	[SerializeField]
	private Transform cardHolder;

	[SerializeField]
	private Vector2 cardPivot;

	private ItemCardUI m_ItemCardUI;

	public void Init(CItem item)
	{
		if (m_ItemCardUI != null)
		{
			ObjectPool.RecycleCard(m_ItemCardUI.CardID, ObjectPool.ECardType.Item, m_ItemCardUI.gameObject);
		}
		m_ItemCardUI = ObjectPool.SpawnCard(item.ID, ObjectPool.ECardType.Item, cardHolder, resetLocalScale: true).GetComponent<ItemCardUI>();
		m_ItemCardUI.lockUI = false;
		CanvasGroup canvasGroup = m_ItemCardUI.GetComponent<CanvasGroup>();
		if (canvasGroup == null)
		{
			canvasGroup = m_ItemCardUI.gameObject.AddComponent<CanvasGroup>();
		}
		canvasGroup.enabled = true;
		canvasGroup.blocksRaycasts = false;
		m_ItemCardUI.item = item;
		RectTransform obj = m_ItemCardUI.transform as RectTransform;
		obj.pivot = cardPivot;
		obj.anchoredPosition = Vector2.zero;
		m_ItemCardUI.Hide();
	}

	public void Show()
	{
		m_ItemCardUI.Show(highlightElement: false);
	}

	public void Hide()
	{
		if (m_ItemCardUI != null && m_ItemCardUI.gameObject.activeSelf)
		{
			m_ItemCardUI.Hide();
		}
	}

	public void Clear()
	{
		if (m_ItemCardUI != null)
		{
			ObjectPool.RecycleCard(m_ItemCardUI.CardID, ObjectPool.ECardType.Item, m_ItemCardUI.gameObject);
		}
		m_ItemCardUI = null;
	}
}
