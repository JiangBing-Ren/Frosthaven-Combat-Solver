using System;

namespace Chronos.Reflection;

[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
public sealed class SelfTargetedAttribute : Attribute
{
}
