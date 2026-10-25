using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chronos;
using MapRuleLibrary.Adventure;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	private class ElementPreviousState
	{
		private Transform parent;

		private Vector2 anchoredPosition;

		private Vector3 scale;

		public ElementPreviousState(GameObject gameObject)
		{
			RectTransform component = gameObject.GetComponent<RectTransform>();
			if (component != null)
			{
				anchoredPosition = component.anchoredPosition;
			}
			parent = gameObject.transform.parent;
			scale = gameObject.transform.localScale;
		}

		public void RestoreState(GameObject element)
		{
			element.transform.SetParent(parent, worldPositionStays: true);
			RectTransform component = element.GetComponent<RectTransform>();
			if (component != null)
			{
				component.anchoredPosition = anchoredPosition;
			}
			element.transform.localScale = scale;
		}
	}

	private static UIManager instance;

	[HideInInspector]
	[SerializeField]
	private GraphicRaycaster graphicRaycaster;

	[SerializeField]
	private List<GraphicRaycaster> graphicRaycasters;

	[SerializeField]
	private AudioSource uiAudioSource;

	[SerializeField]
	private Text debugText;

	[SerializeField]
	private RectTransform highlightHolder;

	[SerializeField]
	private Image highlightImage;

	[SerializeField]
	private Camera uiCamera;

	[Tooltip("Define preferable area in which camera should show Characters and enemies, everything outside of this area could be obscured by UI")]
	[SerializeField]
	private RectTransform m_ScreenVisibleArea;

	private readonly Dictionary<GameObject, ElementPreviousState> highlightedElements = new Dictionary<GameObject, ElementPreviousState>();

	private readonly HashSet<GameObject> elementsLockUI = new HashSet<GameObject>();

	private readonly Dictionary<GameObject, bool> _objectLateActiveStates = new Dictionary<GameObject, bool>();

	public DialogPopup dialogPopup;

	public static UIManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = UnityEngine.Object.FindObjectOfType<UIManager>();
			}
			return instance;
		}
	}

	public static bool IsLateActiveStatePhase { get; private set; }

	public Camera UICamera => uiCamera;

	public RectTransform ScreenVisibleArea => m_ScreenVisibleArea;

	public static bool IsPointerOverUI
	{
		get
		{
			if (!EventSystem.current.IsPointerOverGameObject() && !IsControllerOverUI)
			{
				return WatchesWindow.IsPointerOverGameObject();
			}
			return true;
		}
	}

	private static bool IsControllerOverUI
	{
		get
		{
			if (!Singleton<InputFHManager>.Instance.UsingGamepad)
			{
				return false;
			}
			PointerEventData eventData = new PointerEventData(EventSystem.current)
			{
				position = InputFHManager.CursorPosition
			};
			List<RaycastResult> list = new List<RaycastResult>();
			EventSystem.current.RaycastAll(eventData, list);
			return list.Any();
		}
	}

	private void OnDestroy()
	{
		instance = null;
	}

	private void LateUpdate()
	{
		DoLateActiveStates();
	}

	private void DoLateActiveStates()
	{
		IsLateActiveStatePhase = true;
		if (_objectLateActiveStates.Any())
		{
			foreach (KeyValuePair<GameObject, bool> item in _objectLateActiveStates.ToList())
			{
				_objectLateActiveStates.Remove(item.Key);
				item.Key.SetActive(item.Value);
			}
		}
		IsLateActiveStatePhase = false;
	}

	public void HighlightElement(GameObject element, bool fadeEverythingElse = false, bool lockUI = true, bool autoUnhighlightElement = false)
	{
		if (highlightedElements.ContainsKey(element))
		{
			Log.GUIWarning("Element already highlighted " + element.name);
			return;
		}
		highlightedElements.Add(element, new ElementPreviousState(element));
		element.transform.SetParent(highlightHolder, worldPositionStays: true);
		if (fadeEverythingElse)
		{
			if (lockUI)
			{
				RequestToggleLockUI(active: true, element);
			}
			highlightImage.enabled = true;
			if (autoUnhighlightElement)
			{
				StartCoroutine(DelayUnfade(element));
			}
		}
	}

	public void UnhighlightElement(GameObject element, bool unlockUI = true)
	{
		if (highlightedElements.ContainsKey(element))
		{
			highlightImage.enabled = false;
			if (unlockUI)
			{
				RequestToggleLockUI(active: false, element);
			}
			highlightedElements[element].RestoreState(element);
			highlightedElements.Remove(element);
		}
	}

	private IEnumerator DelayUnfade(GameObject element)
	{
		yield return Timekeeper.instance.WaitForSeconds(2f);
		UnhighlightElement(element);
	}

	public static GameObject GameObjectUnderPointer()
	{
		if (Pointer.current == null)
		{
			return null;
		}
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.position = Pointer.current.position.ReadValue();
		List<RaycastResult> list = new List<RaycastResult>();
		EventSystem.current.RaycastAll(pointerEventData, list);
		return list.FirstOrDefault().gameObject;
	}

	public static T ComponentUnderPointer<T>() where T : Component
	{
		GameObject gameObject = GameObjectUnderPointer();
		if (gameObject == null)
		{
			return null;
		}
		return gameObject.GetComponent<T>();
	}

	public void PlayUISound(AudioClip audioClip)
	{
		if (audioClip != null)
		{
			uiAudioSource.PlayOneShot(audioClip);
		}
	}

	public void DisplayDebugMessage(string message)
	{
		debugText.text = message;
		debugText.enabled = true;
	}

	public void HideDebugMessage()
	{
		debugText.enabled = false;
	}

	private void ToggleLockUI(bool active)
	{
		graphicRaycaster.enabled = !active;
		for (int i = 0; i < graphicRaycasters.Count; i++)
		{
			graphicRaycasters[i].enabled = !active;
		}
	}

	public void RequestToggleLockUI(bool active, GameObject element)
	{
		if (active)
		{
			elementsLockUI.Add(element);
		}
		else
		{
			elementsLockUI.Remove(element);
		}
		ToggleLockUI(elementsLockUI.Count > 0);
	}

	public static void RestartScenario()
	{
		SaveData.Instance.Global.DisallowSpeedUp();
		SceneController.Instance.RestartScenario();
	}

	public static void RestartScenarioFromInitial()
	{
		if (Choreographer.IsInNoPartyCampaign())
		{
			RegenerateAndRestartScenarioKeepGoldAndXP();
			return;
		}
		SaveData.Instance.Global.DisallowSpeedUp();
		SceneController.Instance.RestartScenarioFromInitial();
	}

	public static void RegenerateAndRestartScenario(Action onFinish = null)
	{
		SaveData.Instance.Global.DisallowSpeedUp();
		SceneController.Instance.RegenerateAndRestartScenario(onFinish);
	}

	public static void RegenerateAndRestartScenarioKeepGoldAndXP()
	{
		SaveData.Instance.Global.DisallowSpeedUp();
		AdventureState.MapState?.CurrentMapScenarioState?.ClearAutoSaveState();
		SceneController.Instance.RegenerateAndRestartScenario();
	}

	public static Coroutine LoadMainMenu(string joinSessionID = null, Action onLoaded = null)
	{
		try
		{
			AudioController.StopAmbienceSound();
			SceneController.Instance.ShowLoadingScreen();
			Main.Unpause3DWorld(forceUnpause: true);
			Singleton<ESCMenu>.Instance?.Hide();
			if (AutoTestController.s_AutoTestCurrentlyLoaded)
			{
				AutoTestController.s_Instance.QuitFromAutotest();
			}
		}
		catch (Exception message)
		{
			Debug.LogError(message);
			return CoroutineHelper.instance.StartCoroutine(Break());
		}
		return CoroutineHelper.instance.StartCoroutine(MainMenuCoroutine(joinSessionID, onLoaded));
		static IEnumerator Break()
		{
			yield break;
		}
	}

	private static IEnumerator MainMenuCoroutine(string joinSessionID = null, Action onFinish = null)
	{
		SaveData.Instance.Global.DisallowSpeedUp();
		bool saveScenario = false;
		if (SaveData.Instance.Global.CurrentGameState == EGameState.Scenario)
		{
			yield return SceneController.Instance.EndScenarioSafely();
			if (Singleton<UIResultsManager>.Instance == null || !Singleton<UIResultsManager>.Instance.IsShown)
			{
				Task task = SaveData.Instance.EndScenario(EResult.InProgress);
				yield return new WaitForTask(task);
			}
		}
		else if (SaveData.Instance.Global.CurrentGameState == EGameState.Map)
		{
			StatsDataTracker.Instance.TrackRunEnd(SaveData.Instance.Global.GameMode, AWSessionEndReason.quit);
		}
		SaveData.Instance.Global.CloseCustomLevel();
		ScenarioRuleClient.ToggleMessageProcessing(process: false);
		if (saveScenario)
		{
			yield return Choreographer.s_Choreographer.EndGameCoroutine(EResult.InProgress);
		}
		Singleton<UIBlackOverlay>.Instance.ParentToDefaultCanvas();
		yield return UnityGameEditorRuntime.UnloadScenario();
		SceneController.Instance.LoadMainMenu(joinSessionID);
		onFinish?.Invoke();
	}

	private static IEnumerator RestartScenarioCoroutine()
	{
		Singleton<UIBlackOverlay>.Instance.ParentToDefaultCanvas();
		yield return UnityGameEditorRuntime.UnloadScenario();
		if (SaveData.Instance.Global.GameMode == EGameMode.Campaign)
		{
			SceneController.Instance.CampaignRestart();
			yield break;
		}
		throw new Exception("Restart is unsupported in this mode");
	}

	public static bool RegisterLateActiveState(GameObject go, bool active)
	{
		if (instance == null)
		{
			go.SetActive(active);
			return false;
		}
		if (IsLateActiveStatePhase)
		{
			go.SetActive(active);
		}
		else
		{
			instance._objectLateActiveStates[go] = active;
		}
		return true;
	}

	public static bool TryGetLateActiveState(GameObject go, out bool active)
	{
		active = go.activeSelf;
		if (instance == null || !instance._objectLateActiveStates.ContainsKey(go))
		{
			return false;
		}
		active = instance._objectLateActiveStates[go];
		return true;
	}
}
