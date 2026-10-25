using System.Collections.Generic;
using FHGUI.DTO;
using UnityEngine;

namespace FHGUI.Elements.Buildings;

public class HallOfRevelry_UIBuildInfoFunctionality : BaseBuildInfoFunctionality, IBuildInfoFunctionality<HallOfRevelry_UIBuildInfoFunctionality.HallOfRevelryStateParams>, IBuildInfoFunctionality
{
	public struct HallOfRevelryStateParams : IBuildingInfoParams
	{
		public UIHallOfRevelryRow.HallOfRevelryData[] Rows;
	}

	[Header("Components")]
	[SerializeField]
	private TextLocalizedListener _description;

	[SerializeField]
	private RectTransform _favorsContainer;

	[SerializeField]
	private GameObject _rowPrefab;

	[SerializeField]
	private UIBuildingLockRequirement _lock;

	private HallOfRevelryStateParams? _data;

	private List<UIHallOfRevelryRow> _rows;

	public List<UIHallOfRevelryRow> Rows
	{
		get
		{
			if (_rows == null)
			{
				_rows = new List<UIHallOfRevelryRow>(_favorsContainer.GetComponentsInChildren<UIHallOfRevelryRow>(includeInactive: true));
			}
			return _rows;
		}
	}

	private void OnDisable()
	{
		_data = null;
	}

	public void Init(BuildingStateDTO building, HallOfRevelryStateParams data)
	{
		gameObject.SetActive(value: true);
		_data = data;
		_description.SetTextKey(BuildInfoUtils.GetDescription(building));
		_lock.Show(1u, building.Level);
		List<UIHallOfRevelryRow> pool = Rows;
		HelperTools.NormalizePool(ref pool, _rowPrefab, _favorsContainer, _data.Value.Rows.Length);
		for (int i = 0; i < data.Rows.Length; i++)
		{
			UIHallOfRevelryRow.HallOfRevelryData data2 = data.Rows[i];
			Rows[i].Init(data2, building.Level);
		}
	}
}
