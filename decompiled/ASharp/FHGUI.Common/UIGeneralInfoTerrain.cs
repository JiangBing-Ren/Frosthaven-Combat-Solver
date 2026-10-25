using TMPro;
using UnityEngine;

namespace FHGUI.Common;

public class UIGeneralInfoTerrain : UIGeneralInfoBase, IUIGeneralInfo<UIGeneralInfoTerrain.TerrainGeneralInfoData>, IUIGeneralInfo
{
	public struct TerrainGeneralInfoData : IGeneralInfoData
	{
		public string Name;

		public string Description;

		public PropImage Image;

		public StatusEffectDTO[] Statuses;
	}

	[SerializeField]
	private TMP_Text _name;

	[SerializeField]
	private TMP_Text _description;

	[SerializeField]
	private UIAsyncImage _image;

	[SerializeField]
	private UIGeneralInfoSection _statusContainer;

	private TerrainGeneralInfoData _data;

	public bool Show(TerrainGeneralInfoData data, params object[] parameters)
	{
		gameObject.SetActive(value: true);
		_data = data;
		_name.text = _data.Name;
		_image.Init(_data.Image?.Sprite, _data.Image?.FallbackSprite);
		_description.enabled = _data.Description.IsNOTNullOrEmpty();
		_description.text = _data.Description;
		UIGeneralInfoSection statusContainer = _statusContainer;
		StatusEffectDTO[] statuses = _data.Statuses;
		statusContainer.Fill(statuses != null && statuses.Length > 0, (UIConditionEffectListContainer c) =>
		{
			c.Init(null, _data.Statuses);
		});
		return true;
	}
}
