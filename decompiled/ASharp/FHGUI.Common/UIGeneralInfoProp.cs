using Libs.Utils.DataStructures;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;

namespace FHGUI.Common;

public class UIGeneralInfoProp : UIGeneralInfoBase, IUIGeneralInfo<UIGeneralInfoProp.PropGeneralInfoData>, IUIGeneralInfo
{
	public struct PropGeneralInfoData : IGeneralInfoData
	{
		public EPropType PropType;

		public string Name;

		public string Description;

		public PropImage Image;

		public RangeDataInt? Health;

		public StatusEffectDTO[] Statuses;

		public int CurrentHealth => Health?.Min ?? 0;

		public int MaxHealth => Health?.Max ?? 0;

		public float HealthPercent
		{
			get
			{
				if (MaxHealth != 0)
				{
					return (float)CurrentHealth / (float)MaxHealth;
				}
				return 0f;
			}
		}

		public PropGeneralInfoData(CObjectProp prop)
		{
			PropType = prop.PropType;
			Name = string.Empty;
			Description = string.Empty;
			Image = null;
			Health = null;
			Statuses = null;
		}
	}

	[SerializeField]
	private TMP_Text _name;

	[SerializeField]
	private UIAsyncImage _image;

	[SerializeField]
	private TMP_Text _description;

	[SerializeField]
	private UIGeneralInfoSection _conditionsContainer;

	[SerializeField]
	private FHImageFillElement _healthbar;

	[SerializeField]
	private TMP_Text _healthbarText;

	private PropGeneralInfoData _data;

	public bool Show(PropGeneralInfoData data, params object[] parameters)
	{
		gameObject.SetActive(value: true);
		_data = data;
		_name.text = _data.Name;
		_description.enabled = _data.Description.IsNOTNullOrEmpty();
		_description.text = _data.Description;
		_image.Init(_data.Image?.Sprite, _data.Image?.FallbackSprite);
		bool hasValue = _data.Health.HasValue;
		_healthbar.gameObject.SetActive(hasValue);
		if (hasValue)
		{
			_healthbar.SetProgress(_data.HealthPercent);
			_healthbarText.text = $"{_data.CurrentHealth} / {data.MaxHealth}";
		}
		UIGeneralInfoSection conditionsContainer = _conditionsContainer;
		StatusEffectDTO[] statuses = _data.Statuses;
		conditionsContainer.Fill(statuses != null && statuses.Length > 0, (UIConditionEffectListContainer container) =>
		{
			container.Init(_data.Statuses);
		});
		return true;
	}
}
