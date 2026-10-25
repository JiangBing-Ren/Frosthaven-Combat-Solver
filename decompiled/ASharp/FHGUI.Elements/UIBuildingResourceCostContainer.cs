using FHGUI.DTO.Outpost;
using FHRL;
using I2.Loc;
using UnityEngine;
using UnityEngine.Serialization;

namespace FHGUI.Elements;

public class UIBuildingResourceCostContainer : MonoBehaviour
{
	[SerializeField]
	private UIBuildingResourceCostElement Lumber;

	[SerializeField]
	private UIBuildingResourceCostElement Metal;

	[SerializeField]
	private UIBuildingResourceCostElement Hide;

	[SerializeField]
	private UIBuildingResourceCostElement Gold;

	[SerializeField]
	private UIBuildingResourceCostElement Morale;

	[SerializeField]
	private UIBuildingResourceCostElement Inspiration;

	[FormerlySerializedAs("Prosperity")]
	[SerializeField]
	private UIBuildingResourceCostElement TownLevel;

	public void Init(BuildingCostDTO cost, uint townLevelRequirement, uint currentTownLevel, OutpostResources availableResources, uint availableGold, bool showTotal = false, bool markUnavailable = true)
	{
		Lumber.gameObject.SetActive(cost.Resources.Lumber != 0 || cost.RawResources.Lumber != 0);
		Lumber.Init(cost.Resources.Lumber, cost.CurrentResources.Lumber, availableResources.Lumber, showTotal);
		Metal.gameObject.SetActive(cost.Resources.Metal != 0 || cost.RawResources.Metal != 0);
		Metal.Init(cost.Resources.Metal, cost.CurrentResources.Metal, availableResources.Metal, showTotal);
		Hide.gameObject.SetActive(cost.Resources.Hide != 0 || cost.RawResources.Hide != 0);
		Hide.Init(cost.Resources.Hide, cost.CurrentResources.Hide, availableResources.Hide, showTotal);
		Gold.gameObject.SetActive(cost.Gold != 0);
		Gold.Init(cost.Gold, cost.CurrentGold, availableGold, showTotal);
		Morale.gameObject.SetActive(cost.Morale != 0);
		Morale.Init((int)cost.Morale, cost.CurrentMorale, 0);
		Inspiration.gameObject.SetActive(cost.Inspiration != 0);
		Inspiration.Init(cost.Inspiration, cost.CurrentInspiration);
		if ((bool)TownLevel)
		{
			TownLevel.gameObject.SetActive(townLevelRequirement != 0);
			TownLevel.Init(townLevelRequirement, currentTownLevel);
			TownLevel.Tooltip.SetText(string.Format(LocalizationManager.GetTranslation("GUI_prosperity_level_requirement"), townLevelRequirement));
		}
	}
}
