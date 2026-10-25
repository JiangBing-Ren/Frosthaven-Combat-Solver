using System;

[AttributeUsage(AttributeTargets.Field)]
public sealed class NameAttribute : Attribute
{
	public string Name { get; set; }

	public string Description { get; set; }

	public float Value { get; set; }

	public NameAttribute(string name)
	{
		Name = name;
	}

	public NameAttribute(string name, string description)
	{
		Name = name;
		Description = description;
	}

	public NameAttribute(string name, float value)
	{
		Name = name;
		Value = value;
	}
}
