using System;
using I2.Loc;
using UnityEngine;

[Serializable]
public struct UIGlossaryData : IEquatable<UIGlossaryData>
{
	public string Key;

	public string Name;

	public string TMPKey;

	public string Description;

	public string DescriptionLocKey;

	public string[] Alias;

	public Sprite Icon;

	public GameObject Prefab;

	public string GetDescription()
	{
		if (Description.IsNOTNullOrEmpty())
		{
			return Description;
		}
		if (DescriptionLocKey.IsNullOrEmpty())
		{
			return string.Empty;
		}
		if (LocalizationManager.TryGetTranslation(DescriptionLocKey, out var Translation))
		{
			return Translation;
		}
		return CreateLayout.LocaliseText(DescriptionLocKey, skipWarnings: true);
	}

	public bool Equals(UIGlossaryData other)
	{
		return Key == other.Key;
	}

	public override bool Equals(object obj)
	{
		if (obj is UIGlossaryData other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (Key == null)
		{
			return 0;
		}
		return Key.GetHashCode();
	}

	public static bool operator ==(UIGlossaryData left, UIGlossaryData right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(UIGlossaryData left, UIGlossaryData right)
	{
		return !left.Equals(right);
	}
}
