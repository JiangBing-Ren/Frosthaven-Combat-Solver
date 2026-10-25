using FHGUI.Common;
using FHGUI.DTO;
using SETUtil.Extend;
using UnityEngine;

namespace FHGUI.Elements;

public abstract class UIBuildInfoState<T> : MonoBehaviour where T : IBuildingInfoStateParams
{
	protected readonly BuildDonationActionData _donationData = new BuildDonationActionData();

	protected BuildingStateDTO _building;

	protected T _data;

	[field: SerializeField]
	protected FHButton StartProjectButton { get; set; }

	[field: SerializeField]
	protected FHButton CancelProjectButton { get; set; }

	[field: SerializeField]
	protected FHButton DonateButton { get; set; }

	[field: SerializeField]
	protected FHButton DonateLeaderButton { get; set; }

	[field: SerializeField]
	protected FHButton FinalizeProjectButton { get; set; }

	protected virtual bool CanStartBuildProject
	{
		get
		{
			if (_data.IsPartyLeader)
			{
				return _building.IsActive;
			}
			return false;
		}
	}

	protected virtual bool CanCancelBuildProject
	{
		get
		{
			if (_data.IsPartyLeader)
			{
				return _building.HasStartedProject;
			}
			return false;
		}
	}

	protected virtual bool CanDonate
	{
		get
		{
			if (_building.HasStartedProject)
			{
				return !_building.IsDonationComplete;
			}
			return false;
		}
	}

	protected virtual bool CanFinalizeBuildProject
	{
		get
		{
			if (_data.IsPartyLeader && _building.HasStartedProject)
			{
				return _building.IsDonationComplete;
			}
			return false;
		}
	}

	public void Init(BuildingStateDTO building, T data)
	{
		gameObject.SetActive(value: true);
		_building = building;
		_data = data;
		_donationData.BindActions(_data.DonationData);
		StartProjectButton.gameObject.SetActive(_building.IsActive);
		StartProjectButton.interactable = CanStartBuildProject;
		StartProjectButton.Tooltip.SetText(_data.StartProjectTooltip);
		CancelProjectButton.gameObject.SetActive(CanCancelBuildProject);
		DonateLeaderButton.gameObject.SetActive(_data.IsPartyLeader && CanDonate);
		DonateButton.gameObject.SetActive(!_data.IsPartyLeader && CanDonate);
		FinalizeProjectButton.gameObject.SetActive(CanFinalizeBuildProject);
		FinalizeProjectButton.interactable = _building.CanFinalizeDonation;
		FinalizeProjectButton.GetOrAdd<UITextTooltipTarget>().tooltipText = _building.FinalizeDonationTT;
		SetData();
		UIContextualTutorialModule.OnEvent("BuildingConstructionOptionsShown", gameObject, CanDonate);
	}

	protected abstract void SetData();

	public void Hide()
	{
		gameObject.SetActive(value: false);
	}

	protected void Awake()
	{
		StartProjectButton.onClick.AddListener(() =>
		{
			_donationData.Start?.Invoke(_building.Def.ID);
		});
		CancelProjectButton.onClick.AddListener(() =>
		{
			_donationData.Cancel?.Invoke(_building.Def.ID);
		});
		DonateButton.onClick.AddListener(() =>
		{
			_donationData.Donate?.Invoke(_building.Def.ID);
		});
		DonateLeaderButton.onClick.AddListener(() =>
		{
			_donationData.Donate?.Invoke(_building.Def.ID);
		});
		FinalizeProjectButton.onClick.AddListener(() =>
		{
			_donationData.FinalizeDonations?.Invoke(_building.Def.ID);
		});
		OnAwake();
	}

	protected virtual void OnAwake()
	{
	}

	protected void OnDisable()
	{
		Cleanup();
	}

	protected virtual void Cleanup()
	{
		_donationData?.Clear();
	}
}
