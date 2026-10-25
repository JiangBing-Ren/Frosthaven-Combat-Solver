using System;
using FHGUI.Elements;
using FHRL;
using UnityEngine;
using UnityEngine.UI;

public class UIGardenSlot : MonoBehaviour
{
	public struct UIGardenSlotData
	{
		public bool Available;

		public bool Locked;

		public bool Interactable;

		public uint LockLevel;

		public OutpostHerbType Herb;
	}

	[SerializeField]
	private FHButton _button;

	[SerializeField]
	private Image _herb;

	[SerializeField]
	private Image _empty;

	[SerializeField]
	private UIBuildingLockRequirement _levelRequirement;

	private UIGardenSlotData? _data;

	public Action<UIGardenSlot> OnSlotClick;

	private void Awake()
	{
		_button.onClick.AddListener(ClickSlot);
	}

	private void OnDisable()
	{
		_data = null;
	}

	private void ClickSlot()
	{
		OnSlotClick?.Invoke(this);
	}

	public void Init(UIGardenSlotData data, uint currentLevel)
	{
		_data = data;
		bool flag = data.Available && data.Herb != OutpostHerbType.None;
		_herb.enabled = flag;
		_empty.enabled = !flag;
		if (data.Herb != OutpostHerbType.None)
		{
			_herb.sprite = UIInfoTools.Instance.GetHerbConfig(data.Herb).Icon;
		}
		if (data.Locked)
		{
			_levelRequirement.Show(data.LockLevel, currentLevel);
		}
		else
		{
			_levelRequirement.Hide();
		}
		_button.interactable = data.Interactable;
	}
}
