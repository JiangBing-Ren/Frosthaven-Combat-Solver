using System;
using System.Collections.Generic;
using UnityEngine;

namespace FHGUI.DTO;

[CreateAssetMenu(fileName = "ScenarioMechanicsSO", menuName = "Defs/ScenarioMechanics")]
public class ScenarioMechanicsConfig : ScriptableObject
{
	[Serializable]
	public class Entry
	{
		public enum Category
		{
			Rule,
			Effect
		}

		public string ID;

		public string Trigger;

		public int Priority;

		public Category StyleCategory;

		public GameObject Content;

		public string SimpleContentKey;

		public bool Hidden;
	}

	public Entry[] Entries;

	public GameObject EntryTemplate;

	public IEnumerable<Entry> GetRelevant(string levelID)
	{
		string striped = levelID.RemoveChar('\'');
		Entry[] entries = Entries;
		foreach (Entry entry in entries)
		{
			if (entry.ID.Contains(striped, StringComparison.InvariantCulture))
			{
				yield return entry;
			}
		}
	}
}
