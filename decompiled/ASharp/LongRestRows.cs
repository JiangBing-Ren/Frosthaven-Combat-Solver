using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;

public class LongRestRows : MonoBehaviour
{
	public TMP_Text BurnText;

	public TMP_Text HealText;

	public TMP_Text RefreshText;

	public TMP_Text FooterText;

	private void OnEnable()
	{
		bool flag = ScenarioManager.CurrentScenarioState?.FreeLongRest ?? false;
		BurnText.gameObject.SetActive(!flag);
	}
}
