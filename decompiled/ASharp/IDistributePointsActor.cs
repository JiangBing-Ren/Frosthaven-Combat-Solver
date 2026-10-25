using ScenarioRuleLibrary;
using UnityEngine;

public interface IDistributePointsActor
{
	string Name { get; }

	Sprite Portrait { get; }

	CActor Actor { get; }
}
