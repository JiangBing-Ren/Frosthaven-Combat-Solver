using System;
using System.Collections;
using System.Collections.Generic;
using ScenarioRuleLibrary.CustomLevels;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class LevelMessageUILayoutGroup : MonoBehaviour
{
	[Serializable]
	public struct UILevelMessagePrefabTuple
	{
		public CLevelMessage.ELevelMessageLayoutType type;

		public LevelMessageUILayout uiMessage;
	}

	[SerializeField]
	private UIWindow window;

	public List<UILevelMessagePrefabTuple> LevelMessageLayouts;

	private Action onCloseButtonPressed;

	private int lastFrameHidden = -1;

	private Coroutine autocloseCoroutine;

	private void Awake()
	{
		window.onHidden.AddListener(StopListenAutoclose);
	}

	public void Show(CLevelMessage message, Action onClosedButtonPressed = null, Func<bool> autocloseCondition = null)
	{
		LevelMessageUILayout uiMsg = null;
		bool wasOpen = false;
		StopListenAutoclose();
		LevelMessageLayouts.ForEach((UILevelMessagePrefabTuple it) =>
		{
			if (it.type == message.LayoutType)
			{
				uiMsg = it.uiMessage;
				wasOpen = it.uiMessage.gameObject.activeSelf;
			}
			it.uiMessage.Hide();
		});
		uiMsg.Init(message, onClosedButtonPressed, !wasOpen || (!window.IsOpen && lastFrameHidden != Time.frameCount));
		onCloseButtonPressed = onClosedButtonPressed;
		window.Show();
		if (autocloseCondition != null && onCloseButtonPressed != null)
		{
			autocloseCoroutine = StartCoroutine(Autoclose(autocloseCondition));
		}
	}

	private IEnumerator Autoclose(Func<bool> checker)
	{
		yield return new WaitUntil(checker);
		autocloseCoroutine = null;
		onCloseButtonPressed?.Invoke();
	}

	public void HideWindow()
	{
		LevelMessageLayouts.ForEach((UILevelMessagePrefabTuple it) =>
		{
			it.uiMessage.Clear();
		});
		window.Hide();
	}

	private void StopListenAutoclose()
	{
		if (autocloseCoroutine != null)
		{
			StopCoroutine(autocloseCoroutine);
			autocloseCoroutine = null;
		}
	}
}
