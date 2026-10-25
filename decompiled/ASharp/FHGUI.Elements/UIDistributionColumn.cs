using System;
using System.Collections.Generic;
using FHGUI.DTO;
using FHGUI.Modules;
using FHRL;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FHGUI.Elements;

public class UIDistributionColumn : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public struct Params
	{
		public PartySlotDTO Character;

		public OutpostResources CharacterResources;

		public OutpostResources TownResources;

		public List<UIDistributionCell.Params> Cells;

		public bool IsHighlighted;
	}

	[Header("Components")]
	[SerializeField]
	private UIPartySlot m_CharacterIcon;

	[SerializeField]
	private GameObject m_TownIcon;

	[SerializeField]
	private CanvasGroup m_CharacterResourcesContainer;

	[SerializeField]
	private ResourceElement m_GoldIcon;

	[SerializeField]
	private ResourceElement m_MetalIcon;

	[SerializeField]
	private ResourceElement m_LumberIcon;

	[SerializeField]
	private ResourceElement m_HideIcon;

	[SerializeField]
	private CanvasGroup m_TownResourcesContainer;

	[SerializeField]
	private ResourceElement m_ArrowvineIcon;

	[SerializeField]
	private ResourceElement m_AxenutIcon;

	[SerializeField]
	private ResourceElement m_CorpsecapIcon;

	[SerializeField]
	private ResourceElement m_FlamefruitIcon;

	[SerializeField]
	private ResourceElement m_RockrootIcon;

	[SerializeField]
	private ResourceElement m_SnowthistleIcon;

	[Header("Settings")]
	[Range(0f, 1f)]
	[SerializeField]
	private float m_ResourcesGroupAlphaNormal = 0.5f;

	[Range(0f, 1f)]
	[SerializeField]
	private float m_ResourcesGroupAlphaFocused = 1f;

	[SerializeField]
	private GameObject m_Highlight;

	private RectTransform m_CellContainer;

	public Action<UIDistributionColumn> OnFocusGain;

	public Action<UIDistributionColumn> OnFocusLoss;

	private bool m_IsHighlighted;

	private List<UIDistributionCell> m_Cells = new List<UIDistributionCell>();

	private bool m_Initialized;

	public Params Data { get; private set; }

	public bool IsHighlighted => m_IsHighlighted;

	private void InitCache()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			m_Cells.AddRange(GetComponentsInChildren<UIDistributionCell>(includeInactive: true));
			m_GoldIcon.SetSprite(UIInfoTools.Instance.GetResourceConfig("Gold").IconStylized);
			m_MetalIcon.SetSprite(UIInfoTools.Instance.GetResourceConfig(OutpostMaterialType.Metal).IconStylized);
			m_LumberIcon.SetSprite(UIInfoTools.Instance.GetResourceConfig(OutpostMaterialType.Lumber).IconStylized);
			m_HideIcon.SetSprite(UIInfoTools.Instance.GetResourceConfig(OutpostMaterialType.Hide).IconStylized);
			m_ArrowvineIcon.SetSprite(UIInfoTools.Instance.GetResourceConfig(OutpostHerbType.Arrowvine).IconStylized);
			m_AxenutIcon.SetSprite(UIInfoTools.Instance.GetResourceConfig(OutpostHerbType.Axenut).IconStylized);
			m_CorpsecapIcon.SetSprite(UIInfoTools.Instance.GetResourceConfig(OutpostHerbType.Corpsecap).IconStylized);
			m_FlamefruitIcon.SetSprite(UIInfoTools.Instance.GetResourceConfig(OutpostHerbType.Flamefruit).IconStylized);
			m_RockrootIcon.SetSprite(UIInfoTools.Instance.GetResourceConfig(OutpostHerbType.Rockroot).IconStylized);
			m_SnowthistleIcon.SetSprite(UIInfoTools.Instance.GetResourceConfig(OutpostHerbType.Snowthistle).IconStylized);
			m_CellContainer = transform as RectTransform;
		}
	}

	public void Init(Params @params)
	{
		InitCache();
		Data = @params;
		PopulateHeader();
		PopulateCells();
		UpdateHighlight();
	}

	private void PopulateHeader()
	{
		bool flag = Data.Character != null;
		m_CharacterIcon.gameObject.SetActive(flag);
		if (flag)
		{
			m_CharacterIcon.Init(Data.Character);
		}
		m_TownIcon.gameObject.SetActive(!flag);
		m_CharacterResourcesContainer.gameObject.SetActive(flag);
		m_TownResourcesContainer.gameObject.SetActive(!flag);
		if (flag)
		{
			m_GoldIcon.Init((int)Data.Character.Gold);
			m_MetalIcon.Init(Data.CharacterResources.Metal);
			m_LumberIcon.Init(Data.CharacterResources.Lumber);
			m_HideIcon.Init(Data.CharacterResources.Hide);
		}
		else
		{
			m_ArrowvineIcon.Init(Data.TownResources.Arrowvine);
			m_AxenutIcon.Init(Data.TownResources.Axenut);
			m_CorpsecapIcon.Init(Data.TownResources.Corpsecap);
			m_FlamefruitIcon.Init(Data.TownResources.Flamefruit);
			m_RockrootIcon.Init(Data.TownResources.Rockroot);
			m_SnowthistleIcon.Init(Data.TownResources.Snowthistle);
		}
	}

	private void PopulateCells()
	{
		int num = Data.Cells?.Count ?? 0;
		HelperTools.NormalizePool(ref m_Cells, m_Cells[0].gameObject, m_CellContainer, num);
		for (int i = 0; i < num; i++)
		{
			m_Cells[i].Init(Data.Cells[i]);
		}
	}

	private void UpdateHighlight()
	{
		if ((bool)m_Highlight)
		{
			m_Highlight.gameObject.SetActive(m_IsHighlighted);
		}
		int num = Data.Cells?.Count ?? 0;
		for (int i = 0; i < num; i++)
		{
			m_Cells[i].SetFocused(m_IsHighlighted);
		}
		if ((bool)m_CharacterResourcesContainer)
		{
			m_CharacterResourcesContainer.alpha = (m_IsHighlighted ? m_ResourcesGroupAlphaFocused : m_ResourcesGroupAlphaNormal);
		}
		if ((bool)m_TownResourcesContainer)
		{
			m_TownResourcesContainer.alpha = (m_IsHighlighted ? m_ResourcesGroupAlphaFocused : m_ResourcesGroupAlphaNormal);
		}
	}

	public void UpdateHitArea(int currentFocusedIndex)
	{
		foreach (UIDistributionCell cell in m_Cells)
		{
			cell.ShrinkHitArena(currentFocusedIndex == -1 || currentFocusedIndex == 1);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		m_IsHighlighted = true;
		UpdateHighlight();
		OnFocusGain?.Invoke(this);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		m_IsHighlighted = false;
		UpdateHighlight();
		OnFocusLoss?.Invoke(this);
	}
}
