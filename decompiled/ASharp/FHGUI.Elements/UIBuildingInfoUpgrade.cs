using FHGUI.DTO;
using FHGUI.DTO.Outpost;
using FHRL;
using TMPro;
using UnityEngine;

namespace FHGUI.Elements;

public class UIBuildingInfoUpgrade : UIBuildInfoState<UIBuildingInfoUpgrade.UpgradeBuildingParams>
{
	public struct UpgradeBuildingParams : IBuildingInfoStateParams
	{
		public OutpostStateDTO OutpostState;

		public OutpostResources? DonatedResources;

		public OutpostResources PartyResources;

		public uint PartyGold;

		public uint? DonatedGold;

		public bool CanStartProject { get; set; }

		public string StartProjectTooltip { get; set; }

		public string FinalizeProjectTooltip { get; set; }

		public BuildDonationActionData DonationData { get; set; }

		public bool IsPartyLeader { get; set; }
	}

	[Header("Texts")]
	[SerializeField]
	private TextLocalizedListener _description;

	[SerializeField]
	private TextLocalizedListener _upgradeText;

	[SerializeField]
	private TMP_Text _prosperityText;

	[Header("Cost")]
	[SerializeField]
	private UIBuildingResourceCostContainer _costContainer;

	[SerializeField]
	private UITextTooltipTarget _upgradeButtonTooltip;

	[Header("Warning")]
	[SerializeField]
	private TextLocalizedListener _warningCannotUpgrade;

	[SerializeField]
	private GameObject _warningContainer;

	[Header("Containers")]
	[SerializeField]
	private GameObject _descriptionContainer;

	[SerializeField]
	private GameObject _titleContainer;

	[SerializeField]
	private GameObject _actionContainer;

	[SerializeField]
	private GameObject _underConstruction;

	[SerializeField]
	private GameObject _maxLevel;

	protected override bool CanStartBuildProject
	{
		get
		{
			if (base.CanStartBuildProject)
			{
				return _building.CanUpgrade;
			}
			return false;
		}
	}

	protected override void SetData()
	{
		uint paramValue = (_building.IsUnderConstruction ? _building.Level : (_building.Level + 1));
		if (_building.HasMoreUpgrades)
		{
			_description.Text.enabled = true;
			_description.SetTextKey(BuildInfoUtils.GetUpgradeEffect(_building.Def, _building.Level));
			_upgradeText.SetTextKey("GUI_upgrade_level");
			_upgradeText.SetParam("0", paramValue);
		}
		else
		{
			_upgradeText.SetTextKey("GUI_LEVEL_NUM");
			_upgradeText.SetParam("0", _building.Level);
			_description.Text.enabled = false;
		}
		uint num = _building.UpgradeCost?.RequiredProsperityLevel ?? 0;
		if (num != 0)
		{
			_ = num > _data.OutpostState.TownLevel;
		}
		else
			_ = 0;
		int prosperityReward = BuildInfoUtils.GetProsperityReward(_building);
		_prosperityText.enabled = prosperityReward > 0 && _building.HasMoreUpgrades;
		_prosperityText.text = $"+{prosperityReward}<space=-0.4em><size=20px><sprite name=Prosperity></size>";
		GameObject gameObject = StartProjectButton.gameObject;
		BuildingStateDTO building = _building;
		gameObject.SetActive(building.IsActive && building.CanBuyUpgrades);
		FinalizeProjectButton.Tooltip?.SetText(_data.FinalizeProjectTooltip);
		_warningCannotUpgrade.enabled = !_building.CanBuyUpgrades;
		_warningCannotUpgrade.SetTextKey(UIInfoTools.Instance.GetBuildingConfig(_building.Def).SpecialUpgradeText);
		GameObject warningContainer = _warningContainer;
		building = _building;
		warningContainer.SetActive(!building.CanBuyUpgrades && building.HasMoreUpgrades);
		SetVisualState();
		BuildInfoUtils.InitCost(_building.UpgradeCost, _data.DonatedResources, _data.PartyResources, _data.DonatedGold, _data.PartyGold, _data.OutpostState, _costContainer);
	}

	private void SetVisualState()
	{
		SetVisualState(!_building.HasMoreUpgrades, _building.IsUnderConstruction);
	}

	private void SetVisualState(bool maxLevel, bool underConstruction)
	{
		bool flag = !maxLevel && !underConstruction;
		_maxLevel.SetActive(maxLevel);
		_underConstruction.SetActive(underConstruction);
		_descriptionContainer.SetActive(flag | underConstruction);
		_actionContainer.SetActive(flag && _building.CanBuyUpgrades);
		_titleContainer.SetActive(flag | maxLevel);
	}
}
