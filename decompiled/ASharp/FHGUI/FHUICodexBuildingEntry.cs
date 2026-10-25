using System.Linq;
using FH.YML;
using FHGUI.Common;
using FHGUI.DTO.Outpost;
using FHRL;
using UnityEngine;

namespace FHGUI;

public class FHUICodexBuildingEntry : FHUICodexEntry
{
	[Header("Building Entry Components")]
	[SerializeField]
	private FHUICodexTable m_CostsStatsTable;

	[SerializeField]
	private FHUICodexTable m_EffectsStatsTable;

	[Header("Settings")]
	[SerializeField]
	private float m_StatRowHeight = 42f;

	[SerializeField]
	private float m_StatColumnWidth = 64f;

	[Header("Colors")]
	[SerializeField]
	private Color m_ColorStatText;

	[SerializeField]
	private Color m_ColorLevelText;

	private uint BuildingId
	{
		get
		{
			if (Data == null || !uint.TryParse(Data.ID, out var result))
			{
				return 0u;
			}
			return result;
		}
	}

	public override void Setup(CodexConfig.CodexEntry entry)
	{
		base.Setup(entry);
		Rebuild();
	}

	private void Rebuild()
	{
		if (BuildingId != 0)
		{
			BuildingConfigUI buildingConfig = UIInfoTools.Instance.GetBuildingConfig(new BuildingTypeID(BuildingId));
			BuildingTypeDef buildingTypeDef = YMLDefRepository.Repo?.DefRepository?.Buildings.Values.FirstOrDefault((BuildingTypeDef itemDef) => itemDef.ID.ID == BuildingId);
			m_Caption.SetTextKey(buildingConfig.GetCodexName());
			m_Description.SetTextKey(buildingConfig.FlavorTextKey);
			m_Illustration.sprite = buildingConfig.Banner;
			if (buildingTypeDef != null)
			{
				TableData data = FHUICodexUtils.PopulateBuildingCostData(BuildingId, m_StatRowHeight, m_StatColumnWidth, m_ColorStatText, m_ColorLevelText);
				m_CostsStatsTable.Init(data);
				TableData data2 = FHUICodexUtils.PopulateBuildingEffectsData(BuildingId, m_StatRowHeight, m_StatColumnWidth, m_ColorStatText, m_ColorLevelText);
				m_EffectsStatsTable.Init(data2);
			}
		}
	}
}
