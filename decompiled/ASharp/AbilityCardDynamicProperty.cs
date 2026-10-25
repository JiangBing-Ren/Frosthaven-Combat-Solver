using System;
using UnityEngine;

[Serializable]
public class AbilityCardDynamicProperty
{
	public enum PropertyTypes
	{
		Value,
		Icon
	}

	public PropertyTypes PropertyType;

	public string Name;

	public int Value;

	public Texture2D Icon;

	public AbilityCardDynamicProperty()
	{
	}

	public AbilityCardDynamicProperty(AbilityCardDynamicProperty copy)
	{
		PropertyType = copy.PropertyType;
		Name = copy.Name;
		Value = copy.Value;
		Icon = copy.Icon;
	}
}
