using System;
using System.Collections.Generic;
using System.Linq;
using DevLocker.GFrame.Input.UIScope;
using FHGUI.DTO;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI;

public class UICampaignSelector : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private UICampaignCard m_CampaignCardPrototype;

	[SerializeField]
	private ToggleGroup m_toggleGroup;

	[SerializeField]
	private SelectionController m_SelectionController;

	[SerializeField]
	private UINavigationGroup m_NavigationGroup;

	private List<UICampaignCard> m_CampaignCardsPool = new List<UICampaignCard>();

	private bool m_Initialized;

	private bool m_InvalidateSelection;

	private bool m_InvalidateNavigation;

	public Action OnSelectedCampaignChanged;

	public CampaignDTO SelectedCampaign { get; private set; }

	private void InitCache()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			transform.GetComponentsInChildren(m_CampaignCardsPool);
			for (int i = 0; i < m_CampaignCardsPool.Count; i++)
			{
				PrepCampaignCard(m_CampaignCardsPool[i]);
			}
		}
	}

	private void Awake()
	{
		InitCache();
	}

	public void Init(List<CampaignDTO> campaigns)
	{
		InitCache();
		Populate(campaigns);
		DetermineToggleInteractability();
		List<CampaignDTO> list = campaigns.Where((CampaignDTO x) => x.IsAvailable).ToList();
		if (list.Count > 0)
		{
			CampaignDTO campaignDTO = list.SingleOrDefault((CampaignDTO dto) => dto.Id == "Frosthaven");
			if (campaignDTO == null)
			{
				campaignDTO = list[0];
			}
			SetCampaign(campaignDTO);
		}
		else
		{
			SetCampaign(null);
		}
		RefreshSelectionController();
	}

	private void LateUpdate()
	{
		if (m_InvalidateSelection)
		{
			m_InvalidateSelection = false;
			RefreshSelectionController();
			if (OnSelectedCampaignChanged != null)
			{
				OnSelectedCampaignChanged();
			}
		}
		if (m_InvalidateNavigation)
		{
			m_InvalidateNavigation = true;
			m_NavigationGroup.RefreshNavigationClearCaches();
			m_NavigationGroup.RefreshNavigationSelectables();
		}
	}

	private void Populate(List<CampaignDTO> campaigns)
	{
		HelperTools.NormalizePool(ref m_CampaignCardsPool, m_CampaignCardPrototype.gameObject, m_CampaignCardPrototype.transform.parent, campaigns.Count, PrepCampaignCard);
		for (int i = 0; i < m_CampaignCardsPool.Count; i++)
		{
			UICampaignCard uICampaignCard = m_CampaignCardsPool[i];
			CampaignDTO campaignDTO = ((campaigns.Count > i) ? campaigns[i] : null);
			uICampaignCard.name = "Campaign_Card_" + ((campaignDTO != null) ? campaignDTO.Id : "Empty");
			uICampaignCard.Init(campaignDTO);
			uICampaignCard.gameObject.SetActive(campaignDTO != null);
		}
		m_InvalidateNavigation = true;
	}

	private void PrepCampaignCard(UICampaignCard inst)
	{
		inst.GetComponent<FHToggle>().group = m_toggleGroup;
		inst.OnSelect = HandleOnCardSelected;
	}

	private void SetCampaign(CampaignDTO campaign)
	{
		if (campaign != SelectedCampaign && CanChangeCampaign())
		{
			SelectedCampaign = campaign;
			m_InvalidateSelection = true;
		}
	}

	private void RefreshSelectionController()
	{
		for (int i = 0; i < m_CampaignCardsPool.Count; i++)
		{
			UICampaignCard uICampaignCard = m_CampaignCardsPool[i];
			if (uICampaignCard.Data == SelectedCampaign)
			{
				if (m_SelectionController == null)
				{
					m_SelectionController.SetPersistantSelection(uICampaignCard.Selectable);
				}
				uICampaignCard.Select();
				break;
			}
		}
	}

	private bool CanChangeCampaign()
	{
		return true;
	}

	private void HandleOnCardSelected(UICampaignCard card)
	{
		SetCampaign(card.Data);
	}

	private void DetermineToggleInteractability()
	{
		for (int i = 0; i < m_CampaignCardsPool.Count; i++)
		{
			UICampaignCard uICampaignCard = m_CampaignCardsPool[i];
			if (uICampaignCard.Data != null)
			{
				uICampaignCard.GetComponent<FHToggle>().interactable = uICampaignCard.Data.IsAvailable;
			}
		}
	}

	private void Clear()
	{
		SelectedCampaign = null;
		foreach (UICampaignCard item in m_CampaignCardsPool)
		{
			item.Deselect();
		}
	}

	private void OnDisable()
	{
		Clear();
	}
}
