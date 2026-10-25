using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.Common.DTO;
using FHGUI.DTO;
using FHGUI.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Common;

public class UIGeneralInfoSummon : UIGeneralInfoBase, IUIGeneralInfo<UIGeneralInfoSummon.SummonGeneralInfoData>, IUIGeneralInfo
{
	public struct SummonGeneralInfoData : IGeneralInfoData
	{
		public string ActorGuid;

		public string ID;

		public string Name;

		public Sprite Illustration;

		public uint Level;

		public BaseStatsDTO Stats;

		public BaseStatsDTO BaseStats;

		public int? Initiative;

		public CardDTO AbilityCardSource;

		public UIGeneralInfoUtils.CharacterTokenAttributes CharacterTokenAttributes;

		public Action ViewCardSource;

		public StatusEffectDTO[] PositiveConditions;

		public StatusEffectDTO[] NegativeConditions;

		public StatusEffectDTO[] Immunities;

		public StatusEffectDTO[] Innates;

		public ActiveAbilityDTO[] ActiveAbilities;
	}

	[SerializeField]
	private TMP_Text _name;

	[SerializeField]
	private Image _portrait;

	[SerializeField]
	private FHImageFillElement _healthbar;

	[SerializeField]
	private TMP_Text _healthbarText;

	[SerializeField]
	private UICharacterAttributesBanner _banner;

	[Header("Status")]
	[SerializeField]
	private UIGeneralInfoSection _positiveConditionsContainer;

	[SerializeField]
	private UIGeneralInfoSection _negativeConditionsContainer;

	[SerializeField]
	private UIGeneralInfoSection _activeAbilitiesContainer;

	[SerializeField]
	private UIGeneralInfoSection _sourceCardContainer;

	[Header("Stats")]
	[SerializeField]
	private UIBaseStatsView _stats;

	[SerializeField]
	private UIConditionEffectListContainer _immunities;

	private SummonGeneralInfoData _data;

	private List<StatusEffectDTO> _innatesAndImmunities = new List<StatusEffectDTO>();

	public bool Show(SummonGeneralInfoData data, params object[] parameters)
	{
		gameObject.SetActive(value: true);
		_data = data;
		_name.text = data.Name;
		_healthbarText.text = $"{_data.Stats.Health}/{_data.Stats.MaxHealth}";
		_healthbar.SetProgress(_data.Stats.HealthPercent);
		_portrait.sprite = _data.Illustration;
		_portrait.enabled = _data.Illustration != null;
		_banner.Init(_data.CharacterTokenAttributes, isSummon: true);
		_stats.Init(_data.BaseStats);
		_positiveConditionsContainer.Fill(show: true, (UIConditionEffectListContainer c) =>
		{
			c.Init(_data.PositiveConditions, 3);
		});
		_negativeConditionsContainer.Fill(show: true, (UIConditionEffectListContainer c) =>
		{
			c.Init(_data.NegativeConditions, 3);
		});
		_innatesAndImmunities.Clear();
		_innatesAndImmunities.AddRange(_data.Innates.Concat(_data.Immunities));
		_immunities.Init(_innatesAndImmunities);
		UIGeneralInfoSection activeAbilitiesContainer = _activeAbilitiesContainer;
		ActiveAbilityDTO[] activeAbilities = _data.ActiveAbilities;
		activeAbilitiesContainer.Fill(activeAbilities != null && activeAbilities.Length > 0, (UIActiveAbilityListContainer c) =>
		{
			c.Init(null, _data.ActiveAbilities);
		});
		_sourceCardContainer.Fill(_data.AbilityCardSource != null, (RectTransform c) =>
		{
			GetComponentInChildren<UICompactAbilityCard>().Init(_data.AbilityCardSource, new UICardInteractionEvents
			{
				OnClick = (CardDTO _) =>
				{
					_data.ViewCardSource?.Invoke();
				}
			});
		});
		return true;
	}
}
