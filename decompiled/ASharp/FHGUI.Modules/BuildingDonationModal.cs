using System;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Elements;
using FHRL;
using UnityEngine;

namespace FHGUI.Modules;

public class BuildingDonationModal : MonoBehaviour
{
	[SerializeField]
	private UIDonationRow _gold;

	[SerializeField]
	private UIDonationRow _lumber;

	[SerializeField]
	private UIDonationRow _hide;

	[SerializeField]
	private UIDonationRow _metal;

	[SerializeField]
	private FHButton _closeButton;

	private Action<OutpostMaterialType, bool> _resourceClicked;

	private Action<OutpostMaterialType> _resetClicked;

	private void Awake()
	{
		_gold.OnResourceClick = (bool useInspiration) =>
		{
			_resourceClicked?.Invoke(OutpostMaterialType.None, useInspiration);
		};
		_lumber.OnResourceClick = (bool useInspiration) =>
		{
			_resourceClicked?.Invoke(OutpostMaterialType.Lumber, useInspiration);
		};
		_hide.OnResourceClick = (bool useInspiration) =>
		{
			_resourceClicked?.Invoke(OutpostMaterialType.Hide, useInspiration);
		};
		_metal.OnResourceClick = (bool useInspiration) =>
		{
			_resourceClicked?.Invoke(OutpostMaterialType.Metal, useInspiration);
		};
		_gold.OnResetClicked = () =>
		{
			_resetClicked?.Invoke(OutpostMaterialType.None);
		};
		_lumber.OnResetClicked = () =>
		{
			_resetClicked?.Invoke(OutpostMaterialType.Lumber);
		};
		_hide.OnResetClicked = () =>
		{
			_resetClicked?.Invoke(OutpostMaterialType.Hide);
		};
		_metal.OnResetClicked = () =>
		{
			_resetClicked?.Invoke(OutpostMaterialType.Metal);
		};
		_closeButton.onClick.AddListener(Hide);
	}

	private void OnDisable()
	{
		_resetClicked = null;
		_resourceClicked = null;
	}

	public void Init(BuildingDonationDTO donation, Action<OutpostMaterialType, bool> resourceClicked, Action<OutpostMaterialType> resetClicked)
	{
		gameObject.SetActive(value: true);
		_resourceClicked = resourceClicked;
		_resetClicked = resetClicked;
		bool flag = donation.Distribution.GoldCost > 0;
		bool flag2 = donation.Distribution.MaterialCost.Lumber > 0;
		bool flag3 = donation.Distribution.MaterialCost.Hide > 0;
		bool flag4 = donation.Distribution.MaterialCost.Metal > 0;
		_gold.gameObject.SetActive(flag);
		if (flag)
		{
			_gold.Init(new UIDonationRow.ResourceDonationData
			{
				ResourceQuantity = (int)donation.Gold,
				CurrentDonated = donation.Distribution.TotalGoldDonated,
				TotalNeeded = donation.Distribution.GoldCost,
				InspirationQuantity = donation.Inspiration,
				IsPartyLeader = donation.IsPartyLeader,
				DonatedByPlayer = (donation.Distribution.goldDistributions.FirstOrDefault((GoldDistribution d) => d.CharacterID == donation.Character)?.Gold ?? 0)
			});
		}
		_lumber.gameObject.SetActive(flag2);
		if (flag2)
		{
			_lumber.Init(new UIDonationRow.ResourceDonationData
			{
				ResourceQuantity = donation.CurrentResources.Lumber,
				CurrentDonated = donation.Distribution.TotalMaterialsDonated.Lumber,
				TotalNeeded = donation.Distribution.MaterialCost.Lumber,
				InspirationQuantity = donation.Inspiration,
				IsPartyLeader = donation.IsPartyLeader,
				DonatedByPlayer = (donation.Distribution.materialDistributions.FirstOrDefault((ResourceDistribution d) => d.CharacterID == donation.Character)?.Resources.Lumber ?? 0)
			});
		}
		_hide.gameObject.SetActive(flag3);
		if (flag3)
		{
			_hide.Init(new UIDonationRow.ResourceDonationData
			{
				ResourceQuantity = donation.CurrentResources.Hide,
				CurrentDonated = donation.Distribution.TotalMaterialsDonated.Hide,
				TotalNeeded = donation.Distribution.MaterialCost.Hide,
				InspirationQuantity = donation.Inspiration,
				IsPartyLeader = donation.IsPartyLeader,
				DonatedByPlayer = (donation.Distribution.materialDistributions.FirstOrDefault((ResourceDistribution d) => d.CharacterID == donation.Character)?.Resources.Hide ?? 0)
			});
		}
		_metal.gameObject.SetActive(flag4);
		if (flag4)
		{
			_metal.Init(new UIDonationRow.ResourceDonationData
			{
				ResourceQuantity = donation.CurrentResources.Metal,
				CurrentDonated = donation.Distribution.TotalMaterialsDonated.Metal,
				TotalNeeded = donation.Distribution.MaterialCost.Metal,
				InspirationQuantity = donation.Inspiration,
				IsPartyLeader = donation.IsPartyLeader,
				DonatedByPlayer = (donation.Distribution.materialDistributions.FirstOrDefault((ResourceDistribution d) => d.CharacterID == donation.Character)?.Resources.Metal ?? 0)
			});
		}
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
	}

	public bool IsActive()
	{
		return gameObject.activeInHierarchy;
	}
}
