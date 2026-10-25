using System;

namespace Chronos;

public class ChronosException : Exception
{
	public ChronosException()
	{
	}

	public ChronosException(string message)
		: base(message)
	{
	}

	public ChronosException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
