using System;

namespace FHGUI;

[Flags]
public enum UIItemScreenFunctionality
{
	None = 0,
	Buy = 1,
	Sell = 2,
	Equip = 4,
	Unequip = 8,
	BuyAndEquip = 0x10,
	Refund = 0x20,
	Inventory = Equip | Unequip,
	Equipment = Unequip,
	Shop = Buy | BuyAndEquip
}
