using System;
using System.Collections.Generic;
using GLOOM;

[Serializable]
public struct LocalizationParam(string paramName, string paramValue, bool localize = false)
{
	public string Name = paramName;

	public string Value = paramValue;

	public bool Localize = localize;

	public LocalizationParam(string paramName, int paramValue, bool localize = false)
		: this(paramName, paramValue.ToString(), localize)
	{
	}

	public LocalizationParam(string paramName, uint paramValue, bool localize = false)
		: this(paramName, paramValue.ToString(), localize)
	{
	}

	public LocalizationParam(KeyValuePair<string, string> pair, bool localize = false)
		: this(pair.Key, pair.Value, localize)
	{
	}

	public bool EqualName(string paramName)
	{
		return Name.Equals(paramName, StringComparison.OrdinalIgnoreCase);
	}

	public string Apply(string text)
	{
		return text.Replace("{" + Name + "}", Localize ? LocalizationManager.GetTranslation(Value) : Value, StringComparison.OrdinalIgnoreCase);
	}
}
