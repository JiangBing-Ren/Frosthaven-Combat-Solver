using System;
using ScenarioRuleLibrary.YML;
using UnityEngine;

namespace FH.LevelComponents.ProcGen;

public class FakeApparance : MonoBehaviour
{
	[Serializable]
	public class Listing
	{
		public ScenarioPossibleRoom.ESubBiome SubBiome;

		public GameObject Visual;
	}

	public GameObject Fallback;

	public Listing[] List;

	public void Start()
	{
		if (!Fallback)
		{
			foreach (Transform item in base.transform)
			{
				if (item.name == "Fallback")
				{
					Fallback = item.gameObject;
					break;
				}
			}
		}
		Fallback.SetActive(value: false);
		Listing[] list = List;
		for (int i = 0; i < list.Length; i++)
		{
			list[i].Visual.SetActive(value: false);
		}
		ProceduralMapTile componentInParent = transform.GetComponentInParent<ProceduralMapTile>();
		if (componentInParent == null)
		{
			Debug.LogError("FakeApparance must be a child of a ProceduralMapTile");
			return;
		}
		ScenarioPossibleRoom.ESubBiome subBiome = componentInParent.GetComponent<ProceduralStyle>().GetSubBiome();
		if (subBiome == ScenarioPossibleRoom.ESubBiome.Inherit)
		{
			subBiome = componentInParent.GetComponentInParent<ProceduralScenario>().GetComponent<ProceduralStyle>().GetSubBiome();
		}
		bool flag = false;
		list = List;
		foreach (Listing listing in list)
		{
			if (listing.SubBiome == subBiome)
			{
				listing.Visual.SetActive(value: true);
				flag = true;
				break;
			}
		}
		if (!flag && (bool)Fallback)
		{
			Fallback.SetActive(value: true);
		}
	}
}
