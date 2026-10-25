using UnityEngine;

namespace FHGUI.Common.DTO;

public struct UISimpleElementWithQuantityDTO
{
	public string Name;

	public Sprite Icon;

	public string Tooltip;

	public uint? Quantity;

	public uint? MaxQuantity;

	public uint? CurrentQuantity;
}
