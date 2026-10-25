using System.Collections.Generic;
using FHGUI.Common.Glossary;
using UnityEngine;

public class GlossaryProvider : MonoBehaviour, IGlossaryProvider
{
	[field: SerializeField]
	public string GlossaryKey { get; protected set; }

	public GameObject GameObject => gameObject;

	public void OverrideGlossaryKey(string key)
	{
		GlossaryKey = key;
	}

	public IEnumerable<UIGlossaryData> GetGlossary()
	{
		return UIInfoTools.GetFullGlossaryDataFromKey(GlossaryKey);
	}
}
