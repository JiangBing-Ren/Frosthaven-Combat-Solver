using System;
using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using FHGUI.Modules.Inventory;
using UnityEngine;

namespace FHGUI.Modules;

public class PotionDiscoveryModule : FHMapViewModule
{
	public struct Params
	{
		public ItemDataDTO Potion;

		public Action OnConfirm;
	}

	[Header("Components")]
	[SerializeField]
	private TextLocalizedListener m_PotionName;

	[SerializeField]
	private ItemTooltipCommonData m_ItemTooltip;

	[SerializeField]
	private FHButton m_ButtonConfirm;

	private bool m_Initialized;

	public Params Data { get; set; }

	private void InitCache()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			if ((bool)m_ButtonConfirm)
			{
				m_ButtonConfirm.onClick.AddListener(HandleOnConfirm);
			}
		}
	}

	public void Init(Params @params)
	{
		InitCache();
		Data = @params;
		if (m_ItemTooltip != null)
		{
			FHItemTooltip.ItemTooltipParams itemTooltipParams = new FHItemTooltip.ItemTooltipParams(Data.Potion);
			m_ItemTooltip.ShowImmediate(itemTooltipParams);
		}
		if ((bool)m_PotionName)
		{
			m_PotionName.SetTextKey(Data.Potion.NameKey);
		}
	}

	private void HandleOnConfirm()
	{
		Data.OnConfirm?.Invoke();
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
	}
}
