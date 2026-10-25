using System.Collections;
using System.IO;
using FHGUI.Elements;
using Libs.GameConsole;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GLOOM.MainMenu;

public class UIIntroCinematicWindow : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler
{
	[Header("Components")]
	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private UIVideoElement video;

	[SerializeField]
	private Button exitButton;

	[SerializeField]
	private GameObject[] wideScreeOrnaments;

	[Header("Skip")]
	[SerializeField]
	private float HoldDelay = 1.2f;

	[SerializeField]
	private float HoldDeadZone = 0.35f;

	[SerializeField]
	private GameObject SkipGroup;

	[SerializeField]
	private GameObject SkipProgressGroup;

	[SerializeField]
	private Image SkipProgressImage;

	[Header("Sewttings")]
	[SerializeField]
	private float m_WideScreenThresholdRatio = 1.7777778f;

	private bool isFirstTimeShown;

	private float holdDuration;

	private Coroutine skipRoutine;

	[ConsoleCommand(Command = "reset_intro_cinematic", Description = "Set Intro Cinematic as unseen")]
	public static void ClearCodexStats(IConsole console)
	{
		SaveData.Instance.Global.IntroCinematicSeen = false;
		SaveData.Instance.SaveGlobalData();
	}

	private void Awake()
	{
		exitButton.onClick.AddListener(window.Hide);
		SkipProgressGroup.gameObject.SetActive(value: false);
	}

	private void OnEnable()
	{
		PlayIntro(!SaveData.Instance.Global.IntroCinematicSeen);
		if (!SaveData.Instance.Global.IntroCinematicSeen)
		{
			SaveData.Instance.Global.IntroCinematicSeen = true;
			SaveData.Instance.SaveGlobalData();
		}
	}

	private void OnDisable()
	{
		isFirstTimeShown = false;
	}

	public void PlayIntro(bool isFirstTime)
	{
		isFirstTimeShown = isFirstTime;
		bool active = (float)Screen.width / (float)Screen.height > m_WideScreenThresholdRatio;
		GameObject[] array = wideScreeOrnaments;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(active);
		}
		video.Init(new UIVideoElement.Params
		{
			URL = GetIntroCinematicPath(),
			Volume = (float)SaveData.Instance.Global.StoryVolume / 100f * (float)SaveData.Instance.Global.MasterVolume / 100f,
			Loop = false,
			OnComplete = window.Hide
		});
		exitButton.interactable = !isFirstTimeShown;
		SkipGroup.SetActive(isFirstTimeShown);
	}

	private string GetIntroCinematicPath()
	{
		string text = "Frosthaven_Intro_Cinematic.mp4";
		if (Application.platform != RuntimePlatform.OSXPlayer)
		{
			return Path.Combine(Application.dataPath, "StreamingAssets", "Movies", "CP_Intro", text);
		}
		return Path.Combine(Application.dataPath, "Resources", "Data", "StreamingAssets", "Movies", "CP_Intro", text);
	}

	private IEnumerator TrackSkip()
	{
		holdDuration = 0f;
		while (holdDuration < HoldDelay)
		{
			holdDuration += Time.unscaledDeltaTime;
			if (holdDuration < HoldDeadZone)
			{
				yield return null;
			}
			SkipProgressGroup.gameObject.SetActive(holdDuration > HoldDeadZone);
			SkipProgressImage.fillAmount = holdDuration / HoldDelay;
			yield return null;
		}
		SkipProgressGroup.gameObject.SetActive(value: false);
		holdDuration = 0f;
		window.Hide();
	}

	private void CancelSkip()
	{
		SkipProgressGroup.gameObject.SetActive(value: false);
		SkipProgressImage.fillAmount = 0f;
		holdDuration = 0f;
		if (skipRoutine != null)
		{
			StopCoroutine(skipRoutine);
		}
		skipRoutine = null;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (!isFirstTimeShown)
		{
			window.Hide();
		}
		else if (eventData.button == PointerEventData.InputButton.Left)
		{
			if (skipRoutine != null)
			{
				CancelSkip();
			}
			skipRoutine = StartCoroutine(TrackSkip());
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left && skipRoutine != null)
		{
			CancelSkip();
		}
	}
}
