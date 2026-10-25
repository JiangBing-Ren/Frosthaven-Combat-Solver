using GLOOM;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPartyItemInventoryTooltip : MonoBehaviour
{
	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private RectTransform cardHolder;

	[SerializeField]
	private TextMeshProUGUI itemBoundText;

	[SerializeField]
	private GameObject boundInformation;

	[SerializeField]
	private GameObject informationPanel;

	[SerializeField]
	private TextMeshProUGUI informationText;

	[SerializeField]
	private Image informationSlotIcon;

	[SerializeField]
	private bool showNodifiersLeft;

	[SerializeField]
	private bool trackScreenBound;

	[ConditionalField("trackScreenBound", null, true)]
	[SerializeField]
	private float offsetScreen = 20f;

	private ItemCardUI m_ItemCardUI;

	public bool IsShown
	{
		get
		{
			if (gameObject.activeSelf)
			{
				if (!(window == null))
				{
					return window.IsOpen;
				}
				return true;
			}
			return false;
		}
	}

	private void Build(CItem item, RectTransform target, Vector2 offset, CMapCharacter boundTo = null, string information = null, CItem.EItemSlotState? state = null)
	{
		if (m_ItemCardUI == null || m_ItemCardUI.CardName != item.Name)
		{
			if (m_ItemCardUI != null)
			{
				ObjectPool.RecycleCard(m_ItemCardUI.CardID, ObjectPool.ECardType.Item, m_ItemCardUI.gameObject);
			}
			m_ItemCardUI = ObjectPool.SpawnCard(item.ID, ObjectPool.ECardType.Item, cardHolder, resetLocalScale: true, resetToMiddle: true).GetComponent<ItemCardUI>();
			m_ItemCardUI.item = item;
			m_ItemCardUI.EnableShowModifiers(enable: true, showNodifiersLeft);
		}
		if (boundTo == null)
		{
			boundInformation.SetActive(value: false);
		}
		else
		{
			informationSlotIcon.sprite = UIInfoTools.Instance.GetItemSlotIcon(item.YMLData.Slot.ToString());
			itemBoundText.text = string.Format(LocalizationManager.GetTranslation("GUI_ITEM_BOUND_TO"), UIInfoTools.Instance.GetCharacterColor(boundTo.CharacterYMLData.Model, boundTo.CharacterYMLData.CustomCharacterConfig).ToHex(), boundTo.CharacterName.IsNOTNullOrEmpty() ? boundTo.CharacterName : LocalizationManager.GetTranslation(boundTo.CharacterYMLData.LocKey), $"AA_{boundTo.CharacterYMLData.Model}");
			boundInformation.SetActive(value: true);
		}
		if (information == null)
		{
			informationPanel.SetActive(value: false);
		}
		else
		{
			informationText.text = information;
			informationPanel.SetActive(value: true);
		}
		transform.SetParent(target, worldPositionStays: false);
		RefreshPosition(offset);
		if (window != null)
		{
			window.Show();
		}
		else
		{
			gameObject.SetActive(value: true);
		}
		enabled = trackScreenBound;
		if (state.HasValue)
		{
			m_ItemCardUI.UpdateState(state.Value, force: true);
		}
	}

	public void Show(CItem item, RectTransform target, CMapCharacter boundTo = null, string information = null, CItem.EItemSlotState? state = null)
	{
		Build(item, target, Vector2.zero, boundTo, information, state);
	}

	public void Show(CItem item, RectTransform target, Vector2 offset)
	{
		Build(item, target, offset);
	}

	public void Hide()
	{
		if (window != null)
		{
			window.Hide();
			enabled = false;
		}
		else
		{
			gameObject.SetActive(value: false);
		}
	}

	private void LateUpdate()
	{
		transform.position += (transform as RectTransform).DeltaWorldPositionToFitTheScreen(UIManager.Instance.UICamera, offsetScreen);
	}

	private void OnDisable()
	{
		if (m_ItemCardUI != null)
		{
			ObjectPool.RecycleCard(m_ItemCardUI.CardID, ObjectPool.ECardType.Item, m_ItemCardUI.gameObject);
			m_ItemCardUI = null;
		}
	}

	public void RefreshPosition(Vector2 offset)
	{
		(transform as RectTransform).anchoredPosition = offset;
		transform.position += (transform as RectTransform).DeltaWorldPositionToFitTheScreen(UIManager.Instance.UICamera, offsetScreen);
	}
}
