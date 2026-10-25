using UnityEngine;

public class LevelMessageCinematicTrigger : MonoBehaviour
{
	public string MessageId;

	private void OnEnable()
	{
		Singleton<LevelEventsController>.Instance.ShowLevelMessage(MessageId);
	}

	private void OnDisable()
	{
		Singleton<LevelEventsController>.Instance.HideLevelMessage(MessageId);
	}
}
