using System;

namespace ClockStone;

public interface IRegisteredComponent
{
	Type GetRegisteredComponentBaseClassType();
}
