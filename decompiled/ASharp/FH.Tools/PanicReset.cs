using System.Collections;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FH.Tools;

public class PanicReset : MonoBehaviour
{
	public static void EnsureInstance()
	{
		if (Object.FindObjectOfType<PanicReset>() == null)
		{
			new GameObject("PanicReset").AddComponent<PanicReset>();
		}
	}

	private void Awake()
	{
		Object.DontDestroyOnLoad(gameObject);
	}

	private void Update()
	{
		if ((bool)Choreographer.s_Choreographer && Keyboard.current.ctrlKey.isPressed && Keyboard.current.altKey.isPressed && Keyboard.current.shiftKey.isPressed)
		{
			if (Keyboard.current.f9Key.wasPressedThisFrame)
			{
				Debug.Log("Panic Reset - restart round");
				RestartRound();
			}
			else if (Keyboard.current.f8Key.wasPressedThisFrame)
			{
				Debug.Log("Panic Reset - lose game");
				LoseGame();
			}
		}
	}

	private void RestartRound()
	{
		if ((bool)Choreographer.s_Choreographer && !SaveData.Instance.IsSavingThreadActive)
		{
			if (LevelEventsController.s_EventsControllerActive)
			{
				Singleton<LevelEventsController>.Instance?.CompleteLevel(completionSuccess: false, userQuit: true);
			}
			if (Singleton<UIResultsManager>.Instance.IsShown)
			{
				Singleton<UIResultsManager>.Instance.Hide();
				Choreographer.s_Choreographer.RestartRound();
			}
			else
			{
				Choreographer.s_Choreographer.SetBlockClientMessageProcessing(active: false);
				Choreographer.s_Choreographer.PostMessageFromUI(new CRestartRound_MessageData());
			}
		}
	}

	private void LoseGame()
	{
		if ((bool)Choreographer.s_Choreographer)
		{
			Choreographer.s_Choreographer.LoseScenario();
			StartCoroutine(EndGameDelayed());
		}
	}

	private IEnumerator EndGameDelayed()
	{
		yield return new WaitForSeconds(3f);
		Choreographer.s_Choreographer.EndGame(EResult.Lose);
	}
}
