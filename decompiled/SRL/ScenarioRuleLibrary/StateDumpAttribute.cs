using System;

namespace ScenarioRuleLibrary;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
public class StateDumpAttribute : Attribute
{
	public bool Skip;

	public bool UseToString;

	public bool NoMembers;

	public bool AlwaysDumpMembers;
}
