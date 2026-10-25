using System;

namespace BestHTTP.SignalRCore;

internal struct CallbackDescriptor(Type[] paramTypes, Action<object[]> callback)
{
	public readonly Type[] ParamTypes = paramTypes;

	public readonly Action<object[]> Callback = callback;
}
