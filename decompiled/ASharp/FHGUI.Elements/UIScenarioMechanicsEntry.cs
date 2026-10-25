using System.Collections.Generic;
using UnityEngine;

namespace FHGUI.Elements;

public class UIScenarioMechanicsEntry : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private GameObject m_NewEntryHighlight;

	private readonly List<TextLocalizedListener> m_Texts = new List<TextLocalizedListener>();

	private bool m_Initialized;

	private void InitCache()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			gameObject.GetComponentsInChildren(includeInactive: true, m_Texts);
		}
	}

	public virtual void Init(bool newEntry, object data, LocalizationParam[] localizationParams, string simpleTextLocKey)
	{
		InitCache();
		if ((bool)m_NewEntryHighlight)
		{
			m_NewEntryHighlight.SetActive(newEntry);
		}
		foreach (TextLocalizedListener text in m_Texts)
		{
			if (simpleTextLocKey.IsNOTNullOrEmpty())
			{
				text.SetTextKey(simpleTextLocKey);
			}
			text.SetParameters(localizationParams);
		}
	}
}
