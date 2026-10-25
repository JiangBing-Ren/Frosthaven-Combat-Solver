using System.Linq;
using FH.YML;
using FHGUI.Common;
using FHGUI.Common.Tooltips;
using FHGUI.DTO.Outpost;
using FHRL;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI;

public class FHUICodexItemEntry : FHUICodexEntry
{
	[Header("Item Entry Components")]
	[SerializeField]
	private GameObject m_GroupItemSingle;

	[SerializeField]
	private FHItemTooltip m_ItemTooltip;

	[SerializeField]
	private FHUICodexTable m_StatsTable;

	[SerializeField]
	private GameObject m_GroupItemDouble;

	[SerializeField]
	private FHItemTooltip m_ItemTooltipDouble;

	[SerializeField]
	private FHUICodexTable m_StatsTableDouble;

	[Header("Settings")]
	[SerializeField]
	private float m_StatRowHeight = 42f;

	[SerializeField]
	private float m_StatColumnWidth = 64f;

	[Header("Colors")]
	[SerializeField]
	private Color m_ColorText;

	private void OnEnable()
	{
		if (Data != null)
		{
			Rebuild();
		}
	}

	public override void Setup(CodexConfig.CodexEntry entry)
	{
		base.Setup(entry);
		Rebuild();
	}

	private void Rebuild()
	{
		ItemConfigUI itemConfig = UIInfoTools.Instance.GetItemConfig(Data.ID);
		ItemDef itemDef = YMLDefRepository.Repo?.DefRepository?.Items.Values.FirstOrDefault((ItemDef itemDef2) => itemDef2.Icon == Data.ID);
		if (itemDef == null)
		{
			return;
		}
		m_Caption.SetTextKey(itemDef.NameLocKey);
		m_Illustration.sprite = itemConfig.miniIcon;
		FHItemTooltip.ItemTooltipParams itemTooltipParams = FHUICodexUtils.GetItemTooltipParams(itemDef.ScenarioItemID);
		bool flag = itemTooltipParams.FlipItem != null;
		m_GroupItemSingle.SetActive(!flag);
		m_GroupItemDouble.SetActive(flag);
		if (flag)
		{
			m_StatsTableDouble.Init(FHUICodexUtils.PopulateItemData(itemDef, m_StatRowHeight, m_StatColumnWidth, m_ColorText));
			m_ItemTooltipDouble.Show(itemTooltipParams);
			LayoutElement component = m_ItemTooltipDouble.GetComponent<LayoutElement>();
			if (component != null)
			{
				component.ignoreLayout = false;
				component.flexibleWidth = 1f;
				component.flexibleHeight = 1f;
			}
		}
		else
		{
			m_StatsTable.Init(FHUICodexUtils.PopulateItemData(itemDef, m_StatRowHeight, m_StatColumnWidth, m_ColorText));
			m_ItemTooltip.Show(itemTooltipParams);
		}
	}
}
