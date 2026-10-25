using System;

namespace Chronos.Reflection;

[Flags]
public enum TypeFamily
{
	None = 0,
	All = -1,
	Value = 1,
	Reference = 2,
	Primitive = 4,
	Array = 8,
	Class = 0x10,
	Enum = 0x20,
	Interface = 0x40,
	Void = 0x80
}
