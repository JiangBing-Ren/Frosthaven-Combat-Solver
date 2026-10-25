using System;

[AttributeUsage(AttributeTargets.Method)]
public class NetworkConsoleCommandAttribute : Attribute
{
	public bool SingleplayerOnly;
}
