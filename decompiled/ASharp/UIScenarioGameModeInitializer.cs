using UnityEngine;

public class UIScenarioGameModeInitializer : MonoBehaviour
{
	[SerializeField]
	private ScenarioRewardManager campaignRewardManager;

	public void Awake()
	{
		Object.Instantiate(campaignRewardManager, transform);
	}
}
