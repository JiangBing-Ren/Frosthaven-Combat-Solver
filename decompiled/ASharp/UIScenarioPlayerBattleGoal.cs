using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class UIScenarioPlayerBattleGoal : MonoBehaviour
{
	[SerializeField]
	private Image characterMarker;

	[SerializeField]
	private UIScenarioBattleGoalProgress progress;

	public void Init(ECharacter character, CBattleGoalState battleGoal, string custom = null)
	{
		if ((bool)characterMarker)
		{
			characterMarker.sprite = UIInfoTools.Instance.GetCharacterMarker(character, custom);
		}
		progress.SetBattleGoal(battleGoal);
	}

	public void Show()
	{
		gameObject.SetActive(value: true);
		progress.UpdateProgress();
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
	}

	public void UpdateProgress()
	{
		if (progress.gameObject.activeSelf)
		{
			progress.UpdateProgress();
		}
	}

	public bool IsComplete()
	{
		return progress.state == UIScenarioBattleGoalProgress.EState.Completed;
	}

	public bool IsFailed()
	{
		return progress.state == UIScenarioBattleGoalProgress.EState.Failed;
	}
}
