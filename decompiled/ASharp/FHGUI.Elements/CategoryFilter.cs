using System;

namespace FHGUI.Elements;

[Flags]
public enum CategoryFilter
{
	Small = 1,
	Head = 2,
	Body = 4,
	Feet = 8,
	OneHandWeapon = 0x10,
	TwoHandWeapon = 0x20,
	Purchasable = 0x40,
	None = 0,
	All = Small | Head | Body | Feet | OneHandWeapon | TwoHandWeapon
}
