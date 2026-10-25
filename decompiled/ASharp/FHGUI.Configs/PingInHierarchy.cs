using System;
using UnityEngine;

namespace FHGUI.Configs;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
public class PingInHierarchy : PropertyAttribute
{
	public readonly int Space = 6;

	public PingInHierarchy(int space)
	{
		Space = space;
	}

	public PingInHierarchy()
	{
	}
}
