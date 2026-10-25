using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Common;

public class UIAttackModifierElement : MonoBehaviour
{
	[SerializeField]
	private Image _icon;

	[SerializeField]
	private Image _valueIcon;

	[SerializeField]
	private TMP_Text _value;

	[SerializeField]
	private RectTransform _counters;

	[SerializeField]
	private GameObject _rolling;

	[SerializeField]
	private GameObject _shuffle;

	[SerializeField]
	private GameObject _persistent;

	[SerializeField]
	private UIAttackModifierPip _pipPrototype;

	private List<UIAttackModifierPip> _pips;

	private void Awake()
	{
		InitCache();
	}

	private void InitCache()
	{
		if (_pips == null)
		{
			_pips = _counters.GetComponentsInChildren<UIAttackModifierPip>(includeInactive: true).ToList();
		}
	}

	public void Init(AttackModifierInfo modifier)
	{
		InitCache();
		_icon.sprite = modifier.Icon;
		_icon.enabled = modifier.Icon != null;
		_valueIcon.enabled = modifier.ValueIcon != null;
		_valueIcon.sprite = modifier.ValueIcon;
		_value.enabled = !modifier.IsBasicModifier && modifier.Value != null;
		_value.text = modifier.Value;
		_rolling.SetActive(modifier.Rolling);
		_shuffle.SetActive(modifier.Shuffle);
		_rolling.SetActive(modifier.Persistent);
		HelperTools.NormalizePool(ref _pips, _pipPrototype.gameObject, _counters, modifier.AvailableCounters + modifier.UsedCounters);
		for (int i = 0; i < modifier.AvailableCounters; i++)
		{
			_pips[i].Init(active: true);
		}
		for (int j = modifier.AvailableCounters; j < _pips.Count; j++)
		{
			_pips[j].Init(active: false);
		}
	}
}
