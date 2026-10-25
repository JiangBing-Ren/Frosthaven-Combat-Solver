using System;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class ShowCreditsPopupModule : FHMapViewModule
{
	public struct Params
	{
		public Action OnClose;
	}

	[Header("Components")]
	[SerializeField]
	private UIWindow m_CredistWindiw;

	[SerializeField]
	private GameObject m_CampaignCompletePanel;

	[SerializeField]
	private FHButton m_Close;

	private void Awake()
	{
		m_CredistWindiw.onHidden.AddListener(ShowCampaignCompleteModalMessage);
	}

	public void Init(Params @params)
	{
		m_CampaignCompletePanel.gameObject.SetActive(value: false);
		m_CredistWindiw.gameObject.SetActive(value: true);
		m_CredistWindiw.Show();
		m_Close.RemoveAllEventListeners();
		m_Close.onClick.AddListener(() =>
		{
			@params.OnClose?.Invoke();
		});
	}

	private void ShowCampaignCompleteModalMessage()
	{
		m_CampaignCompletePanel.gameObject.SetActive(value: true);
	}
}
