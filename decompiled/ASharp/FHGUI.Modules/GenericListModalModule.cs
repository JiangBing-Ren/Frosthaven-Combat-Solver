using System;
using System.Collections.Generic;
using FHGUI.Elements;
using FHGUI.States;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class GenericListModalModule : FHMapViewModule
{
	public struct Params
	{
		public string TitleLocalised;

		public string DescriptionLocalised;

		public List<UIMultiplayerUser.Data> SuggestingUsers;

		public Action<bool> OnResult;

		public string ConfirmButton;

		public string CancelButton;

		public List<GenericListModalViewState.ListItem> Items;

		public Action<GenericListModalViewState.ListItem> OnItemSelected;

		public GenericListModalViewState.ListItem SelectedItem;

		public int Columns;
	}

	[Header("Components")]
	public TMP_Text Title;

	public GameObject TitleContainer;

	public TMP_Text Text;

	public FHButton OkButton;

	public FHButton CancelButton;

	public GridLayoutGroup ItemsContainer;

	public GameObject ItemPrototype;

	public GameObject ItemTogglePrototype;

	public ToggleGroup ToggleGroup;

	public LayoutElement ContentLayoutElement;

	public RectTransform ContentContainer;

	[Header("Multiplayer")]
	[SerializeField]
	private UIUsersListContainer SyncUserList;

	[Header("Settings")]
	[SerializeField]
	private float ContentMinHeight;

	[SerializeField]
	private float ContentMaxHeight;

	private List<GameObject> m_Items = new List<GameObject>();

	private List<GameObject> m_ToggleItems = new List<GameObject>();

	private float m_DefaultItemCellWidth;

	private bool m_Initialized;

	private bool m_InvalidateHeight;

	private void InitCache()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			m_DefaultItemCellWidth = ItemsContainer.cellSize.x;
		}
	}

	public void Init(Params @params)
	{
		InitCache();
		Text.text = @params.DescriptionLocalised;
		bool flag = !string.IsNullOrEmpty(@params.ConfirmButton);
		bool flag2 = !string.IsNullOrEmpty(@params.CancelButton);
		bool flag3 = @params.TitleLocalised.IsNOTNullOrEmpty();
		if ((bool)OkButton)
		{
			OkButton.gameObject.SetActive(flag);
			OkButton.interactable = true;
			OkButton.onClick.RemoveAllListeners();
			OkButton.onClick.AddListener(() =>
			{
				DisableIntractable();
				@params.OnResult?.Invoke(obj: true);
			});
			if (flag)
			{
				OkButton.SetNonLocalizedText(@params.ConfirmButton);
			}
		}
		if ((bool)CancelButton)
		{
			CancelButton.gameObject.SetActive(flag2);
			CancelButton.interactable = true;
			CancelButton.onClick.RemoveAllListeners();
			CancelButton.onClick.AddListener(() =>
			{
				DisableIntractable();
				@params.OnResult?.Invoke(obj: false);
			});
			if (flag2)
			{
				CancelButton.SetNonLocalizedText(@params.CancelButton);
			}
		}
		if ((bool)Title & flag3)
		{
			Title.text = @params.TitleLocalised;
		}
		if ((bool)TitleContainer)
		{
			TitleContainer.SetActive(flag3);
		}
		SyncUserList.Init(@params.SuggestingUsers);
		Clear();
		if (flag)
		{
			SetupToggleItemList(@params);
		}
		else
		{
			SetupDefaultItemList(@params);
		}
		ItemsContainer.cellSize = new Vector2(Mathf.Floor(m_DefaultItemCellWidth / (float)Mathf.Max(1, @params.Columns)), ItemsContainer.cellSize.y);
		m_InvalidateHeight = true;
	}

	private void LateUpdate()
	{
		if (m_InvalidateHeight)
		{
			m_InvalidateHeight = false;
			ContentLayoutElement.preferredHeight = Mathf.Clamp(ContentContainer.rect.height, ContentMinHeight, ContentMaxHeight);
		}
	}

	private void DisableIntractable()
	{
		if ((bool)OkButton)
		{
			OkButton.interactable = false;
		}
		if ((bool)CancelButton)
		{
			CancelButton.interactable = false;
		}
		for (int i = 0; i < m_ToggleItems.Count; i++)
		{
			FHToggle component = m_ToggleItems[i].GetComponent<FHToggle>();
			if ((bool)component)
			{
				component.interactable = false;
			}
		}
	}

	private void SetupToggleItemList(Params @params)
	{
		HelperTools.NormalizePool(ref m_ToggleItems, ItemTogglePrototype.gameObject, ItemsContainer.transform, @params.Items.Count);
		for (int i = 0; i < @params.Items.Count; i++)
		{
			GenericListModalViewState.ListItem item = @params.Items[i];
			GameObject gameObject = m_ToggleItems[i];
			bool flag = item == @params.SelectedItem;
			gameObject.GetComponentInChildren<TMP_Text>().text = item.Text ?? item.Item.ToString();
			FHToggle componentInChildren = gameObject.GetComponentInChildren<FHToggle>();
			componentInChildren.isOn = flag;
			if (flag)
			{
				componentInChildren.Select();
			}
			else
			{
				componentInChildren.OnDeselect(null);
			}
			componentInChildren.interactable = item.Enabled;
			componentInChildren.group = ToggleGroup;
			componentInChildren.onValueChanged.RemoveAllListeners();
			componentInChildren.onValueChanged.AddListener((bool r) =>
			{
				if (r)
				{
					@params.OnItemSelected?.Invoke(item);
				}
			});
			UIUsersListContainer componentInChildren2 = componentInChildren.GetComponentInChildren<UIUsersListContainer>();
			if (componentInChildren2 != null)
			{
				componentInChildren2.Init(item.SuggestingUsers);
			}
		}
	}

	private void SetupDefaultItemList(Params @params)
	{
		HelperTools.NormalizePool(ref m_Items, ItemPrototype.gameObject, ItemsContainer.transform, @params.Items.Count);
		for (int i = 0; i < @params.Items.Count; i++)
		{
			GenericListModalViewState.ListItem item = @params.Items[i];
			GameObject gameObject = m_Items[i];
			gameObject.GetComponentInChildren<TMP_Text>().text = item.Text ?? item.Item.ToString();
			Button componentInChildren = gameObject.GetComponentInChildren<Button>();
			componentInChildren.interactable = item.Enabled;
			componentInChildren.onClick.RemoveAllListeners();
			componentInChildren.onClick.AddListener(() =>
			{
				@params.OnItemSelected?.Invoke(item);
			});
			UIUsersListContainer componentInChildren2 = componentInChildren.GetComponentInChildren<UIUsersListContainer>();
			if (componentInChildren2 != null)
			{
				componentInChildren2.Init(item.SuggestingUsers);
			}
		}
	}

	private void Clear()
	{
		for (int i = 0; i < m_ToggleItems.Count; i++)
		{
			m_ToggleItems[i].gameObject.SetActive(value: false);
		}
		for (int j = 0; j < m_Items.Count; j++)
		{
			m_Items[j].gameObject.SetActive(value: false);
		}
	}
}
