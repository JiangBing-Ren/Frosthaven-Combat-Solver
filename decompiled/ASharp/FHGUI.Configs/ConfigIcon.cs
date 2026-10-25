using System;
using UnityEngine;

namespace FHGUI.Configs;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class ConfigIcon : PropertyAttribute
{
	public int Size;

	public string Label;

	public bool ShowAssetName;

	public bool Requiered;

	public ConfigIcon(int size)
	{
		Size = size;
	}

	public ConfigIcon(int size, string label)
	{
		Size = size;
		Label = label;
	}

	public ConfigIcon(int size, bool required)
	{
		Size = size;
		Requiered = required;
	}

	public ConfigIcon(int size, string label, bool required)
	{
		Size = size;
		Label = label;
		Requiered = required;
	}

	public ConfigIcon(int size, string label, bool required, bool showAssetName)
	{
		Size = size;
		Label = label;
		Requiered = required;
		ShowAssetName = showAssetName;
	}
}
