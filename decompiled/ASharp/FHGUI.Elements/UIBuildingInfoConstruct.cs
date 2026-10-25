using FHGUI.DTO;
using FHGUI.DTO.Outpost;
using FHRL;
using TMPro;
using UnityEngine;

namespace FHGUI.Elements;

public class UIBuildingInfoConstruct : UIBuildInfoState<UIBuildingInfoConstruct.ConstructBuildingParams>
{
	public struct ConstructBuildingParams : IBuildingInfoStateParams
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

		public string FinalizeProjectTooltip { get; set; }
	}

	[Header("Texts")]
	[SerializeField]
	private TextLocalizedListener _description;

	[SerializeField]
	private TextLocalizedListener _buildText;

	[SerializeField]
	private TMP_Text _prosperityText;

	[Header("Cost")]
	[SerializeField]
	private UIBuildingResourceCostContainer _costContainer;

	[Header("Under Construction State")]
	[SerializeField]
	private GameObject[] _showGameObjects;

	[SerializeField]
	private GameObject[] _hideGameObjects;

	protected override bool CanStartBuildProject
	{
		get
		{
			if (base.CanStartBuildProject)
			{
				return _building.CanBuild;
			}
			return false;
		}
	}

	protected override void SetData()
	{
		int paramValue = 1;
		_description.SetTextKey(BuildInfoUtils.GetConstructEffect(_building.Def));
		_buildText.SetParam("0", paramValue);
		uint num = _building.ConstructionCost?.RequiredProsperityLevel ?? 0;
		bool flag = num != 0 && num > _data.OutpostState.TownLevel && !_building.IsUnderConstruction;
		StartProjectButton.interactable = !flag;
		int prosperityReward = BuildInfoUtils.GetProsperityReward(_building);
		_prosperityText.enabled = prosperityReward > 0;
		_prosperityText.text = $"+{prosperityReward}<space=-0.4em><size=20px><sprite name=Prosperity></size>";
		ToggleUnderConstructionState(_building.IsUnderConstruction);
		FinalizeProjectButton.Tooltip?.SetText(_data.FinalizeProjectTooltip);
		BuildInfoUtils.InitCost(_building.ConstructionCost, _data.DonatedResources, _data.PartyResources, _data.DonatedGold, _data.PartyGold, _data.OutpostState, _costContainer);
	}

	public void ToggleUnderConstructionState(bool underConstruction)
	{
		GameObject[] showGameObjects = _showGameObjects;
		for (int i = 0; i < showGameObjects.Length; i++)
		{
			showGameObjects[i].SetActive(underConstruction);
		}
		showGameObjects = _hideGameObjects;
		for (int i = 0; i < showGameObjects.Length; i++)
		{
			showGameObjects[i].SetActive(!underConstruction);
		}
	}
}
