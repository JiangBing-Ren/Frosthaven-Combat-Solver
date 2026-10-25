using System;
using System.Collections.Generic;

namespace Chronos.Reflection;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public sealed class FilterAttribute : Attribute
{
	private readonly List<Type> types;

	public bool Inherited { get; set; }

	public bool Instance { get; set; }

	public bool Static { get; set; }

	public bool Public { get; set; }

	public bool NonPublic { get; set; }

	public bool ReadOnly { get; set; }

	public bool WriteOnly { get; set; }

	public bool Extension { get; set; }

	public bool Parameters { get; set; }

	public TypeFamily TypeFamilies { get; set; }

	public List<Type> Types => types;

	public FilterAttribute(params Type[] types)
	{
		this.types = new List<Type>(types);
		Inherited = false;
		Instance = true;
		Static = false;
		Public = true;
		NonPublic = false;
		ReadOnly = true;
		WriteOnly = true;
		Extension = true;
		Parameters = true;
		TypeFamilies = TypeFamily.All;
	}
}
