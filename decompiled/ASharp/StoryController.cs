using System;
using System.Collections;
using System.Collections.Generic;
using FFSNet;
using MapRuleLibrary.MapState;
using MapRuleLibrary.YML.Message;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using UnityEngine;
using UnityEngine.UI;

public class StoryController : Singleton<StoryController>
{
	public class DialogInfo
	{
		public CLevelMessage LevelMsg;

		public CMapMessageState MapMsg;

		public List<DialogLineDTO> DialogPages;

		public Action OnClosedAction;

		public bool HideOtherGUI = true;

		public DialogInfo(List<DialogLineDTO> dialogs, Action onClosed, bool hideOtherUI = true)
		{
			DialogPages = dialogs;
			OnClosedAction = onClosed;
			HideOtherGUI = hideOtherUI;
		}

		public DialogInfo(CLevelMessage levelMessage, Action onClosed)
		{
			LevelMsg = levelMessage;
			DialogPages = new List<DialogLineDTO>();
			foreach (CLevelMessagePage page in levelMessage.Pages)
			{
				DialogPages.Add(new DialogLineDTO(page));
			}
			OnClosedAction = onClosed;
			HideOtherGUI = levelMessage.DisplayTrigger.EventTriggerTypeInt != 30 && levelMessage.DisplayTrigger.EventTriggerTypeInt != 31;
		}

		public DialogInfo(CMapMessageState mapMessageState, Action onClosed)
		{
			MapMsg = mapMessageState;
			DialogPages = new List<DialogLineDTO>();
			foreach (MapDialogueLine dialogueLine in mapMessageState.MapMessage.DialogueLines)
			{
				DialogPages.Add(new DialogLineDTO(dialogueLine));
			}
			OnClosedAction = onClosed;
		}
	}

	[SerializeField]
	private UICharacterStoryBox dialogBox;

	[SerializeField]
	private UIWindow window;

	private Queue<DialogInfo> m_PendingMessages = new Queue<DialogInfo>();

	private DialogInfo m_CurrentMessage;

	[SerializeField]
	private List<GameObject> HideElements;

	private bool isVisibleOtherUI = true;

	public bool IsVisible => window.IsVisible;

	public DialogInfo CurrentMessage => m_CurrentMessage;

	public bool IsBusy
	{
		get
		{
			if (CurrentMessage == null)
			{
				return m_PendingMessages.Count > 0;
			}
			return true;
		}
	}

	private void ShowOtherGUI(bool show)
	{
		if (isVisibleOtherUI == show)
		{
			return;
		}
		isVisibleOtherUI = show;
		for (int i = 0; i < HideElements.Count; i++)
		{
			Canvas component = HideElements[i].GetComponent<Canvas>();
			GraphicRaycaster component2 = HideElements[i].GetComponent<GraphicRaycaster>();
			CanvasGroup component3 = HideElements[i].GetComponent<CanvasGroup>();
			if ((bool)component)
			{
				component.enabled = show;
			}
			if (component2 != null)
			{
				component2.enabled = show;
			}
			if ((bool)component3)
			{
				component3.alpha = (show ? 1f : 0f);
				component3.blocksRaycasts = show;
			}
		}
	}

	private void ShowNext()
	{
		if (m_PendingMessages.Count > 0)
		{
			m_PendingMessages.Dequeue();
		}
		if (m_PendingMessages.Count > 0)
		{
			ShowImmediately(m_PendingMessages.Peek());
			return;
		}
		ShowOtherGUI(show: true);
		window.Hide();
		if (FFSNetwork.IsOnline && SaveData.Instance.Global.CurrentGameState == EGameState.Scenario && PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest && ActionProcessor.CurrentPhase != ActionPhaseType.ScenarioEnded)
		{
			ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.StartOfRound);
			if (FFSNetwork.IsClient && !SceneController.Instance.GameLoadedAndClientReady)
			{
				Synchronizer.SendSideAction(GameActionType.GameLoadedAndClientReady, null, canBeUnreliable: false, sendToHostOnly: true);
				SceneController.Instance.GameLoadedAndClientReady = true;
			}
		}
		if (Singleton<TakeDamagePanel>.Instance != null && Singleton<TakeDamagePanel>.Instance.IsOpen)
		{
			Singleton<TakeDamagePanel>.Instance.RefreshButtons(cardBurn: true, resetToggles: false);
		}
	}

	public void Show(CLevelMessage message, Action onClosed = null)
	{
		Show(new DialogInfo(message, onClosed));
	}

	public void Show(DialogInfo dialogInf)
	{
		UIManager.Instance.StartCoroutine(DisplayMessageInWindowAfterDelay(dialogInf));
	}

	public void Clear()
	{
		Log.GUIInfo("story controller clear");
		m_PendingMessages.Clear();
		m_CurrentMessage = null;
		dialogBox.Hide();
	}

	private IEnumerator DisplayMessageInWindowAfterDelay(DialogInfo msg)
	{
		if (msg.LevelMsg != null)
		{
			if (!AutoTestController.s_Instance.AutotestStarted && msg.LevelMsg.DisplayDelay > 0f)
			{
				yield return new WaitForSecondsRealtime(msg.LevelMsg.DisplayDelay);
			}
			Singleton<LevelEventsController>.Instance.MessageWasDisplayed(msg.LevelMsg, fromStoryController: true);
		}
		m_PendingMessages.Enqueue(msg);
		if (m_PendingMessages.Count == 1)
		{
			ShowImmediately(msg);
		}
	}

	private void ShowImmediately(DialogInfo message)
	{
		m_CurrentMessage = message;
		dialogBox.Show(message.DialogPages, OnFinishShow);
		ShowOtherGUI(!message.HideOtherGUI);
		if (!AutoTestController.s_Instance.AutotestStarted)
		{
			window.Show(instant: true);
		}
		else
		{
			OnFinishShow();
		}
	}

	private void OnFinishShow()
	{
		m_CurrentMessage?.OnClosedAction?.Invoke();
		if (m_CurrentMessage != null && m_CurrentMessage.LevelMsg != null)
		{
			Singleton<LevelEventsController>.Instance.MessageWasDismissed(m_CurrentMessage.LevelMsg);
		}
		m_CurrentMessage = null;
		ShowNext();
	}
}
