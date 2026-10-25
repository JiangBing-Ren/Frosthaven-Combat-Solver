using System;
using System.Collections.Generic;
using UnityEngine;

namespace FHGUI.DTO;

[Serializable]
public class SuggestibleSlotDTO
{
	public string Id;

	public string NameLiteral;

	public string InfoLiteral;

	public string FooterLiteral;

	public string TooltipLiteral;

	public int Points;

	public int Charges;

	public int MaxCharges;

	public Sprite Image;

	public bool IsEmpty;

	public bool IsSelected;

	public bool IsSuggested;

	public bool Interactable;

	public bool SuppressInteraction;

	public List<UIMultiplayerUser.Data> SuggestingUsers;

	public RectTransform SlotPickerPoint;
}
