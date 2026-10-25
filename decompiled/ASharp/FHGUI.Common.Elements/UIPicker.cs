using System;
using System.Collections.Generic;
using System.Linq;
using FHRL;
using UnityEngine;

namespace FHGUI.Common.Elements;

public class UIPicker : MonoBehaviour
{
	public record UIPickerData
	{
		public UIPickerSlot.UIPickerSlotDTO[] PickerElements;

		public string TitleLocKey;

		public RectTransform targetPoint;

		public static UIPickerData GetMaterialPicker(OutpostResources resources, OutpostMaterialType selected, string overrideTitle = null)
		{
			UIPickerData uIPickerData = new UIPickerData();
			uIPickerData.TitleLocKey = overrideTitle ?? "GUI_AVAILABLE_MATERIALS";
			uIPickerData.PickerElements = new UIPickerSlot.UIPickerSlotDTO[3]
			{
				new UIPickerSlot.UIPickerSlotDTO
				{
					Icon = UIInfoTools.Instance.GetResourceConfig(OutpostMaterialType.Hide).BigIcon,
					Quantity = resources.Hide,
					Tooltip = "RESOURCE_Hide",
					Available = (resources.Hide > 0),
					SlotData = OutpostMaterialType.Hide
				},
				new UIPickerSlot.UIPickerSlotDTO
				{
					Icon = UIInfoTools.Instance.GetResourceConfig(OutpostMaterialType.Metal).BigIcon,
					Quantity = resources.Metal,
					Tooltip = "RESOURCE_Metal",
					Available = (resources.Metal > 0),
					SlotData = OutpostMaterialType.Metal
				},
				new UIPickerSlot.UIPickerSlotDTO
				{
					Icon = UIInfoTools.Instance.GetResourceConfig(OutpostMaterialType.Lumber).BigIcon,
					Quantity = resources.Lumber,
					Tooltip = "RESOURCE_Lumber",
					Available = (resources.Lumber > 0),
					SlotData = OutpostMaterialType.Lumber
				}
			};
			return uIPickerData;
		}

		public static UIPickerData GetResourcePicker(OutpostResources resources, string overrideTitle = null)
		{
			UIPickerData uIPickerData = new UIPickerData();
			uIPickerData.TitleLocKey = overrideTitle ?? "GUI_SELECT";
			uIPickerData.PickerElements = new UIPickerSlot.UIPickerSlotDTO[6]
			{
				new UIPickerSlot.UIPickerSlotDTO
				{
					Icon = UIInfoTools.Instance.GetResourceConfig(OutpostHerbType.Arrowvine).BigIcon,
					Quantity = resources.Arrowvine,
					Tooltip = "RESOURCE_Arrowvine",
					Available = (resources.Arrowvine > 0),
					SlotData = OutpostHerbType.Arrowvine
				},
				new UIPickerSlot.UIPickerSlotDTO
				{
					Icon = UIInfoTools.Instance.GetResourceConfig(OutpostHerbType.Axenut).BigIcon,
					Quantity = resources.Axenut,
					Tooltip = "RESOURCE_Axenut",
					Available = (resources.Axenut > 0),
					SlotData = OutpostHerbType.Axenut
				},
				new UIPickerSlot.UIPickerSlotDTO
				{
					Icon = UIInfoTools.Instance.GetResourceConfig(OutpostHerbType.Corpsecap).BigIcon,
					Quantity = resources.Corpsecap,
					Tooltip = "RESOURCE_Corpsecap",
					Available = (resources.Corpsecap > 0),
					SlotData = OutpostHerbType.Corpsecap
				},
				new UIPickerSlot.UIPickerSlotDTO
				{
					Icon = UIInfoTools.Instance.GetResourceConfig(OutpostHerbType.Flamefruit).BigIcon,
					Quantity = resources.Flamefruit,
					Tooltip = "RESOURCE_Flamefruit",
					Available = (resources.Flamefruit > 0),
					SlotData = OutpostHerbType.Flamefruit
				},
				new UIPickerSlot.UIPickerSlotDTO
				{
					Icon = UIInfoTools.Instance.GetResourceConfig(OutpostHerbType.Rockroot).BigIcon,
					Quantity = resources.Rockroot,
					Tooltip = "RESOURCE_Rockroot",
					Available = (resources.Rockroot > 0),
					SlotData = OutpostHerbType.Rockroot
				},
				new UIPickerSlot.UIPickerSlotDTO
				{
					Icon = UIInfoTools.Instance.GetResourceConfig(OutpostHerbType.Snowthistle).BigIcon,
					Quantity = resources.Snowthistle,
					Tooltip = "RESOURCE_Snowthistle",
					Available = (resources.Snowthistle > 0),
					SlotData = OutpostHerbType.Snowthistle
				}
			};
			return uIPickerData;
		}

		public static UIPickerData GetResourcePickerAvailableOnly(OutpostResources resources, string overrideTitle = null, bool skipQuantity = false)
		{
			UIPickerData uIPickerData = new UIPickerData
			{
				TitleLocKey = (overrideTitle ?? "GUI_SELECT")
			};
			List<UIPickerSlot.UIPickerSlotDTO> list = new List<UIPickerSlot.UIPickerSlotDTO>();
			if (resources.Hide > 0)
			{
				list.Add(new UIPickerSlot.UIPickerSlotDTO
				{
					Icon = UIInfoTools.Instance.GetResourceConfig(OutpostMaterialType.Hide).BigIcon,
					Quantity = (skipQuantity ? ((int?)null) : new int?(resources.Hide)),
					Tooltip = "RESOURCE_Hide",
					Available = true,
					SlotData = OutpostMaterialType.Hide
				});
			}
			if (resources.Metal > 0)
			{
				list.Add(new UIPickerSlot.UIPickerSlotDTO
				{
					Icon = UIInfoTools.Instance.GetResourceConfig(OutpostMaterialType.Metal).BigIcon,
					Quantity = (skipQuantity ? ((int?)null) : new int?(resources.Metal)),
					Tooltip = "RESOURCE_Metal",
					Available = true,
					SlotData = OutpostMaterialType.Metal
				});
			}
			if (resources.Lumber > 0)
			{
				list.Add(new UIPickerSlot.UIPickerSlotDTO
				{
					Icon = UIInfoTools.Instance.GetResourceConfig(OutpostMaterialType.Lumber).BigIcon,
					Quantity = (skipQuantity ? ((int?)null) : new int?(resources.Lumber)),
					Tooltip = "RESOURCE_Lumber",
					Available = true,
					SlotData = OutpostMaterialType.Lumber
				});
			}
			if (resources.Arrowvine > 0)
			{
				list.Add(new UIPickerSlot.UIPickerSlotDTO
				{
					Icon = UIInfoTools.Instance.GetResourceConfig(OutpostHerbType.Arrowvine).BigIcon,
					Quantity = (skipQuantity ? ((int?)null) : new int?(resources.Arrowvine)),
					Tooltip = "RESOURCE_Arrowvine",
					Available = true,
					SlotData = OutpostHerbType.Arrowvine
				});
			}
			if (resources.Axenut > 0)
			{
				list.Add(new UIPickerSlot.UIPickerSlotDTO
				{
					Icon = UIInfoTools.Instance.GetResourceConfig(OutpostHerbType.Axenut).BigIcon,
					Quantity = (skipQuantity ? ((int?)null) : new int?(resources.Axenut)),
					Tooltip = "RESOURCE_Axenut",
					Available = true,
					SlotData = OutpostHerbType.Axenut
				});
			}
			if (resources.Corpsecap > 0)
			{
				list.Add(new UIPickerSlot.UIPickerSlotDTO
				{
					Icon = UIInfoTools.Instance.GetResourceConfig(OutpostHerbType.Corpsecap).BigIcon,
					Quantity = (skipQuantity ? ((int?)null) : new int?(resources.Corpsecap)),
					Tooltip = "RESOURCE_Corpsecap",
					Available = true,
					SlotData = OutpostHerbType.Corpsecap
				});
			}
			if (resources.Flamefruit > 0)
			{
				list.Add(new UIPickerSlot.UIPickerSlotDTO
				{
					Icon = UIInfoTools.Instance.GetResourceConfig(OutpostHerbType.Flamefruit).BigIcon,
					Quantity = (skipQuantity ? ((int?)null) : new int?(resources.Flamefruit)),
					Tooltip = "RESOURCE_Flamefruit",
					Available = true,
					SlotData = OutpostHerbType.Flamefruit
				});
			}
			if (resources.Rockroot > 0)
			{
				list.Add(new UIPickerSlot.UIPickerSlotDTO
				{
					Icon = UIInfoTools.Instance.GetResourceConfig(OutpostHerbType.Rockroot).BigIcon,
					Quantity = (skipQuantity ? ((int?)null) : new int?(resources.Rockroot)),
					Tooltip = "RESOURCE_Rockroot",
					Available = true,
					SlotData = OutpostHerbType.Rockroot
				});
			}
			if (resources.Snowthistle > 0)
			{
				list.Add(new UIPickerSlot.UIPickerSlotDTO
				{
					Icon = UIInfoTools.Instance.GetResourceConfig(OutpostHerbType.Snowthistle).BigIcon,
					Quantity = (skipQuantity ? ((int?)null) : new int?(resources.Snowthistle)),
					Tooltip = "RESOURCE_Snowthistle",
					Available = true,
					SlotData = OutpostHerbType.Snowthistle
				});
			}
			uIPickerData.PickerElements = list.ToArray();
			return uIPickerData;
		}
	}

	public struct UIPickerResult
	{
		public UIPickerSlot.UIPickerSlotDTO[] Selected;
	}

	[SerializeField]
	private TextLocalizedListener _title;

	[SerializeField]
	private RectTransform _container;

	[SerializeField]
	private UIPickerSlot _pickerPrefab;

	[SerializeField]
	private FHButton _close;

	private Action<UIPickerSlot> _onClick;

	private Predicate<UIPickerResult> _onResult;

	private UIPickerResult? _result;

	private List<UIPickerSlot> _slots;

	public Action OnHide;

	private bool _initialized;

	public UIPickerData Data { get; private set; }

	private void Awake()
	{
		if ((bool)_close)
		{
			_close.onClick.AddListener(Hide);
		}
		InitCache();
	}

	private void InitCache()
	{
		if (!_initialized)
		{
			_initialized = true;
			_slots = _container.GetComponentsInChildren<UIPickerSlot>(includeInactive: true).ToList();
		}
	}

	private void OnDisable()
	{
		_onClick = null;
		_onResult = null;
		_result = null;
		Data = null;
	}

	public void Init(UIPickerData data, Predicate<UIPickerResult> onResult, Predicate<UIPickerSlot> validateSelect = null)
	{
		gameObject.SetActive(value: true);
		InitCache();
		_title.SetTextKey(data.TitleLocKey);
		HelperTools.NormalizePool(ref _slots, _pickerPrefab.gameObject, _container, data.PickerElements.Length);
		for (int i = 0; i < data.PickerElements.Length; i++)
		{
			_slots[i].Init(data.PickerElements[i], OnClickAction, validateSelect);
		}
		_onResult = onResult;
	}

	private void OnClickAction(UIPickerSlot slot, bool selected)
	{
		_result = new UIPickerResult
		{
			Selected = (from s in _slots
				where s.Selected
				select s.Data).ToArray()
		};
		Predicate<UIPickerResult> onResult = _onResult;
		if (onResult == null || onResult(_result.Value))
		{
			Hide();
		}
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
		OnHide?.Invoke();
	}

	public bool IsVisible()
	{
		return gameObject.activeInHierarchy;
	}
}
