using FHGUI.DTO;
using FHGUI.DTO.Outpost;
using FHRL;
using UnityEngine;

namespace FHGUI.Elements;

public class UIBuildingInfoRebuild : UIBuildInfoState<UIBuildingInfoRebuild.RebuildBuildingParams>
{
	public struct RebuildBuildingParams : IBuildingInfoStateParams
	{
		public OutpostStateDTO OutpostState;

		public OutpostResources? DonatedResources;

		public OutpostResources PartyResources;

		public uint PartyGold;

		public uint? DonatedGold;

		public BuildDonationActionData DonationData { get; set; }

		public bool IsPartyLeader { get; set; }

		public bool CanStartProject { get; set; }

		public string StartProjectTooltip { get; set; }
	}

	[Header("Buttons (Leader)")]
	[SerializeField]
	private TextLocalizedListener _description;

	[SerializeField]
	private TextLocalizedListener _rebuildText;

	[SerializeField]
	private UIBuildingResourceCostContainer _costContainer;

	protected override bool CanStartBuildProject
	{
		get
		{
			if (base.CanStartBuildProject)
			{
				return _building.CanRebuild;
			}
			return false;
		}
	}

	protected override void SetData()
	{
		_rebuildText.SetParam("0", _building.Level);
		_description.SetTextKey(BuildInfoUtils.GetDescription(_building));
		StartProjectButton.gameObject.SetActive(_building.IsWrecked);
		BuildInfoUtils.InitCost(_building.RebuildCost, _data.DonatedResources, _data.PartyResources, _data.DonatedGold, _data.PartyGold, _data.OutpostState, _costContainer);
	}
}
