using System;

namespace FH.Platforms;

[AttributeUsage(AttributeTargets.Class)]
public class PlatformIDAttribute : Attribute
{
	public string ID;

	public PlatformIDAttribute(string id)
	{
		ID = id;
	}
}
