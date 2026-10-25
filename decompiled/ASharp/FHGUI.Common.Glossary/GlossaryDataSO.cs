using System.Collections.Generic;
using UnityEngine;

namespace FHGUI.Common.Glossary;

[CreateAssetMenu(fileName = "GlossaryData", menuName = "UI Config/GlossaryData")]
public class GlossaryDataSO : ScriptableObject
{
	[SerializeField]
	private List<UIGlossaryData> _data;

	private Dictionary<string, UIGlossaryData> _glossary;

	private IReadOnlyDictionary<string, UIGlossaryData> Glossary
	{
		get
		{
			if (_glossary == null)
			{
				BuildGlossary();
			}
			return _glossary;
		}
	}

	private void BuildGlossary()
	{
		_glossary = new Dictionary<string, UIGlossaryData>();
		foreach (UIGlossaryData datum in _data)
		{
			string key = datum.Key.ToLowerInvariant();
			_glossary.TryAdd(key, datum);
			if (datum.Alias != null)
			{
				string[] alias = datum.Alias;
				for (int i = 0; i < alias.Length; i++)
				{
					string key2 = alias[i].ToLowerInvariant();
					_glossary.TryAdd(key2, datum);
				}
			}
		}
	}

	public UIGlossaryData? GetData(string key)
	{
		if (Glossary.TryGetValue(key.ToLowerInvariant(), out var value))
		{
			return value;
		}
		return null;
	}

	public IEnumerable<UIGlossaryData> GetFullDataFromKey(string key)
	{
		UIGlossaryData? data = GetData(key);
		if (data.HasValue)
		{
			yield return data.Value;
		}
	}

	public bool GetDataFromImage(Sprite sprite, out UIGlossaryData? data)
	{
		int num = _data.FindIndex((UIGlossaryData g) => g.Icon == sprite);
		data = ((num >= 0) ? new UIGlossaryData?(_data[num]) : ((UIGlossaryData?)null));
		return num >= 0;
	}
}
