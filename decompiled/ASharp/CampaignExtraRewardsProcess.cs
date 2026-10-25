using System.Collections.Generic;
using Assets.Script.Misc;
using ScenarioRuleLibrary.YML;
using UnityEngine;

public abstract class CampaignExtraRewardsProcess : MonoBehaviour
{
	public abstract ICallbackPromise Process(List<Reward> rewards);
}
