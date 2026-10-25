using MapRuleLibrary.Client;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FH.Controller;

[DefaultExecutionOrder(-1000)]
public class ScenarioRunner : MonoBehaviour
{
	public void Awake()
	{
		if (Object.FindObjectOfType<ApparanceEngine>() == null)
		{
			SceneManager.LoadScene("Bootstrap");
			Transform[] array = Object.FindObjectsOfType<Transform>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(value: false);
			}
		}
		else
		{
			MapRuleLibraryClient.Instance.SetMessageHandler(MessageHandler);
		}
	}

	public void MessageHandler(object message, bool processImmediately)
	{
	}

	private void Update()
	{
		MapRuleLibraryClient.Instance.ProcessMessageQueue();
	}
}
