using System;

namespace Chronos.Reflection;

public class UnityReflectionException : Exception
{
	public UnityReflectionException()
	{
	}

	public UnityReflectionException(string message)
		: base(message)
	{
	}

	public UnityReflectionException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
