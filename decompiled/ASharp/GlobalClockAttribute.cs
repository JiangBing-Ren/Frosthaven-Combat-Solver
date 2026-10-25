using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class GlobalClockAttribute : PropertyAttribute
{
}
