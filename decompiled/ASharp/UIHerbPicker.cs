using System;
using FHRL;
using UnityEngine;

public class UIHerbPicker : MonoBehaviour
{
	public class HerbPickerData
	{
		public OutpostResources Resources;

		public OutpostHerbType Selected;

		public string TitleLocKey;

		public bool Locked;

		public bool IsEmptySelectable;
	}

	[SerializeField]
	private TextLocalizedListener _title;

	[Header("Herbs")]
	[SerializeField]
	private UIHerbPickerSlot _arrowVine;

	[SerializeField]
	private UIHerbPickerSlot _axenut;

	[SerializeField]
	private UIHerbPickerSlot _corpsecap;

	[SerializeField]
	private UIHerbPickerSlot _flamefruit;

	[SerializeField]
	private UIHerbPickerSlot _rockroot;

	[SerializeField]
	private UIHerbPickerSlot _snowthistle;

	[Header("Components")]
	[SerializeField]
	private FHButton _close;

	private Action<OutpostHerbType> _onSelect;

	private void Awake()
	{
		_arrowVine.Button.onClick.AddListener(() =>
		{
			ChooseHerb(OutpostHerbType.Arrowvine);
		});
		_axenut.Button.onClick.AddListener(() =>
		{
			ChooseHerb(OutpostHerbType.Axenut);
		});
		_corpsecap.Button.onClick.AddListener(() =>
		{
			ChooseHerb(OutpostHerbType.Corpsecap);
		});
		_flamefruit.Button.onClick.AddListener(() =>
		{
			ChooseHerb(OutpostHerbType.Flamefruit);
		});
		_rockroot.Button.onClick.AddListener(() =>
		{
			ChooseHerb(OutpostHerbType.Rockroot);
		});
		_snowthistle.Button.onClick.AddListener(() =>
		{
			ChooseHerb(OutpostHerbType.Snowthistle);
		});
		if ((bool)_close)
		{
			_close.onClick.AddListener(Hide);
		}
	}

	private void OnDisable()
	{
		_onSelect = null;
	}

	public void Show(HerbPickerData data, Action<OutpostHerbType> onSelect)
	{
		_onSelect = onSelect;
		_title.SetTextKey(data.TitleLocKey);
		gameObject.SetActive(value: true);
		_arrowVine.Init(new UIHerbPickerSlot.UIHerbData
		{
			Herb = OutpostHerbType.Arrowvine,
			Quantity = data.Resources.Arrowvine,
			Locked = data.Locked,
			IsEmptySelectable = data.IsEmptySelectable
		}, data.Selected == OutpostHerbType.Arrowvine);
		_axenut.Init(new UIHerbPickerSlot.UIHerbData
		{
			Herb = OutpostHerbType.Axenut,
			Quantity = data.Resources.Axenut,
			Locked = data.Locked,
			IsEmptySelectable = data.IsEmptySelectable
		}, data.Selected == OutpostHerbType.Axenut);
		_corpsecap.Init(new UIHerbPickerSlot.UIHerbData
		{
			Herb = OutpostHerbType.Corpsecap,
			Quantity = data.Resources.Corpsecap,
			Locked = data.Locked,
			IsEmptySelectable = data.IsEmptySelectable
		}, data.Selected == OutpostHerbType.Corpsecap);
		_flamefruit.Init(new UIHerbPickerSlot.UIHerbData
		{
			Herb = OutpostHerbType.Flamefruit,
			Quantity = data.Resources.Flamefruit,
			Locked = data.Locked,
			IsEmptySelectable = data.IsEmptySelectable
		}, data.Selected == OutpostHerbType.Flamefruit);
		_rockroot.Init(new UIHerbPickerSlot.UIHerbData
		{
			Herb = OutpostHerbType.Rockroot,
			Quantity = data.Resources.Rockroot,
			Locked = data.Locked,
			IsEmptySelectable = data.IsEmptySelectable
		}, data.Selected == OutpostHerbType.Rockroot);
		_snowthistle.Init(new UIHerbPickerSlot.UIHerbData
		{
			Herb = OutpostHerbType.Snowthistle,
			Quantity = data.Resources.Snowthistle,
			Locked = data.Locked,
			IsEmptySelectable = data.IsEmptySelectable
		}, data.Selected == OutpostHerbType.Snowthistle);
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
	}

	private void ChooseHerb(OutpostHerbType herb)
	{
		_onSelect?.Invoke(herb);
		Hide();
	}

	public bool IsShown()
	{
		return gameObject.activeInHierarchy;
	}
}
