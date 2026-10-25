using System;
using System.Linq;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;

[CreateAssetMenu(fileName = "SummonUIDataSO", menuName = "Def/SummonUIDataSO")]
public class SummonUIDataSO : ScriptableObject
{
	public SummonConfigUI[] Summons;

	public SummonConfigUI GetConfig(string summonId)
	{
		return Summons.FirstOrDefault((SummonConfigUI m) => m.ID.Equals(summonId, StringComparison.InvariantCultureIgnoreCase));
	}

	public SummonConfigUI GetConfig(HeroSummonYMLData summon)
	{
		return GetConfig(summon.ID);
	}

	public SummonConfigUI GetConfig(CHeroSummonClass summon)
	{
		return GetConfig(summon.ID);
	}

	public SummonConfigUI GetConfig(CHeroSummonActor summon)
	{
		return GetConfig(summon.Class.ID);
	}
}
