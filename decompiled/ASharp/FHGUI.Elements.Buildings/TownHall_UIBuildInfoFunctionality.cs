using FHGUI.DTO;
using TMPro;
using UnityEngine;

namespace FHGUI.Elements.Buildings;

public class TownHall_UIBuildInfoFunctionality : BaseBuildInfoFunctionality, IBuildInfoFunctionality<TownHall_UIBuildInfoFunctionality.TownHallStateParams>, IBuildInfoFunctionality
{
	public struct TownHallStateParams : IBuildingInfoParams
	{
		public int CompletedChallengesCount;

		public int AllChallengesCount;
	}

	[Header("Components")]
	[SerializeField]
	private TextLocalizedListener _description;

	[SerializeField]
	private FHImageFillElement _challenge;

	[SerializeField]
	private TMP_Text _challengeText;

	[SerializeField]
	private UIBuildingLockRequirement _lockRequirement;

	private TownHallStateParams? _data;

	public void Init(BuildingStateDTO building, TownHallStateParams data)
	{
		gameObject.SetActive(value: true);
		_lockRequirement.Show(1u, building.Level);
		_data = data;
		_description.SetTextKey(BuildInfoUtils.GetDescription(building));
		_challenge.Set(data.CompletedChallengesCount, data.AllChallengesCount);
		_challengeText.text = $"{data.CompletedChallengesCount}/{data.AllChallengesCount}";
	}

	private void OnDisable()
	{
		_data = null;
	}
}
