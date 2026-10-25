using System;
using System.Collections;
using Chronos;
using GLOOM;
using SharedLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
	public enum ELoadingScreenMode
	{
		None,
		Loading,
		LoadingWithProgress,
		ModLoadingWithProgress,
		ModUploadingWithProgress,
		MPEndOfTurnStateCompare,
		WaitingForPlayers
	}

	public const float c_SliderMaxValue = 100f;

	private const string c_HintTextIndexName = "LoadingScreenHintTextIndex";

	private const string LoadingTextLookup = "GUI_LOADING_DOTS";

	private const string ModLoadingTextLookup = "GUI_MOD_LOADING_DOTS";

	private const string ModUploadingTextLookup = "GUI_MODDING_UPLOADING_WITH_DOTS";

	private const string MPEndOfTurnStateCompareLookup = "GUI_LOADING_STATE_COMPARE";

	private const string MPWaitingForPlayers = "GUI_LOADING_WAITING_FOR_PLAYERS";

	private const string LoadingHintPrefix = "LOADING_TIP_";

	[Header("Prefab References")]
	[SerializeField]
	private GameObject m_AutoSaveTip;

	[SerializeField]
	private TextMeshProUGUI m_HintText;

	[SerializeField]
	private GameObject m_LoadingIconBase;

	[SerializeField]
	private GameObject m_LoadingIconOverlay;

	[SerializeField]
	private TextMeshProUGUI m_LoadingText;

	[Header("Progress Bar")]
	[SerializeField]
	private Slider m_Slider;

	[Header("Animation Settings")]
	[SerializeField]
	private float m_IconSpinSpeed;

	[SerializeField]
	private float m_IconGlowSpeed;

	[SerializeField]
	private float m_IconUpdateSpeed;

	[SerializeField]
	private float m_IconOverlayMinAlpha;

	[SerializeField]
	private float m_TextUpdateSpeed;

	private SharedLibrary.Random m_RNG;

	private RectTransform m_IconRect;

	private RectTransform m_IconOverlayRect;

	private Image m_IconOverlayImage;

	private bool m_IncreaseAlpha;

	private Coroutine m_AnimateIconCoroutine;

	private Coroutine m_ShowHintTextCoroutine;

	private void Awake()
	{
		m_RNG = new SharedLibrary.Random();
		m_IconRect = m_LoadingIconBase.GetComponent<RectTransform>();
		m_IconOverlayRect = m_LoadingIconOverlay.GetComponent<RectTransform>();
		m_IconOverlayImage = m_LoadingIconOverlay.GetComponent<Image>();
		m_IncreaseAlpha = true;
		m_Slider.maxValue = 100f;
		SetMode(ELoadingScreenMode.Loading);
	}

	private void OnEnable()
	{
		m_IncreaseAlpha = true;
		m_IconOverlayImage.color = new Color(m_IconOverlayImage.color.r, m_IconOverlayImage.color.g, m_IconOverlayImage.color.b, m_IconOverlayMinAlpha);
		m_AnimateIconCoroutine = StartCoroutine(AnimateIcon());
		m_ShowHintTextCoroutine = StartCoroutine(ShowHintText());
		m_Slider.value = 0f;
	}

	private void OnDisable()
	{
		m_AutoSaveTip.SetActive(value: false);
		StopCoroutine(m_AnimateIconCoroutine);
		StopCoroutine(m_ShowHintTextCoroutine);
	}

	public void UpdateProgressBar(float amount)
	{
		amount = Math.Min(amount, 100f);
		m_Slider.value = amount;
	}

	public void IncrementProgressBar(float amount)
	{
		amount = Math.Min(amount, 100f);
		m_Slider.value += amount;
	}

	public void SetMode(ELoadingScreenMode mode)
	{
		switch (mode)
		{
		case ELoadingScreenMode.Loading:
			m_LoadingText.text = LocalizationManager.GetTranslation("GUI_LOADING_DOTS");
			m_Slider.gameObject.SetActive(value: false);
			break;
		case ELoadingScreenMode.LoadingWithProgress:
			m_LoadingText.text = LocalizationManager.GetTranslation("GUI_LOADING_DOTS");
			m_Slider.value = 0f;
			m_Slider.gameObject.SetActive(value: true);
			break;
		case ELoadingScreenMode.ModLoadingWithProgress:
			m_LoadingText.text = LocalizationManager.GetTranslation("GUI_MOD_LOADING_DOTS");
			m_Slider.value = 0f;
			m_Slider.gameObject.SetActive(value: true);
			break;
		case ELoadingScreenMode.ModUploadingWithProgress:
			m_LoadingText.text = LocalizationManager.GetTranslation("GUI_MODDING_UPLOADING_WITH_DOTS");
			m_Slider.value = 0f;
			m_Slider.gameObject.SetActive(value: true);
			break;
		case ELoadingScreenMode.MPEndOfTurnStateCompare:
			m_LoadingText.text = LocalizationManager.GetTranslation("GUI_LOADING_STATE_COMPARE");
			m_Slider.value = 0f;
			m_Slider.gameObject.SetActive(value: true);
			break;
		case ELoadingScreenMode.WaitingForPlayers:
			m_LoadingText.text = LocalizationManager.GetTranslation("GUI_LOADING_WAITING_FOR_PLAYERS");
			m_Slider.gameObject.SetActive(value: false);
			break;
		default:
			m_LoadingText.text = LocalizationManager.GetTranslation("GUI_LOADING_DOTS");
			break;
		}
	}

	private IEnumerator AnimateIcon()
	{
		while (true)
		{
			m_IconRect.Rotate(new Vector3(0f, 0f, m_IconSpinSpeed));
			m_IconOverlayRect.Rotate(new Vector3(0f, 0f, m_IconSpinSpeed));
			if (m_IconOverlayImage.color.a >= 1f)
			{
				m_IncreaseAlpha = false;
			}
			else if (m_IconOverlayImage.color.a <= m_IconOverlayMinAlpha)
			{
				m_IncreaseAlpha = true;
			}
			if (m_IncreaseAlpha)
			{
				m_IconOverlayImage.color = new Color(m_IconOverlayImage.color.r, m_IconOverlayImage.color.g, m_IconOverlayImage.color.b, Math.Min(1f, m_IconOverlayImage.color.a + m_IconGlowSpeed));
			}
			else
			{
				m_IconOverlayImage.color = new Color(m_IconOverlayImage.color.r, m_IconOverlayImage.color.g, m_IconOverlayImage.color.b, Math.Max(m_IconOverlayMinAlpha, m_IconOverlayImage.color.a - m_IconGlowSpeed));
			}
			yield return Timekeeper.instance.WaitForSeconds(m_IconUpdateSpeed);
		}
	}

	private IEnumerator ShowHintText()
	{
		while (true)
		{
			string text = (GloomUtility.GetExclusiveIndex(m_RNG, "LoadingScreenHintTextIndex", GlobalSettings.DefInstance.LoadingTipsCount) + 1).ToString("000");
			string term = "LOADING_TIP_" + text;
			m_HintText.text = LocalizationManager.GetTranslation(term);
			yield return Timekeeper.instance.WaitForSeconds(m_TextUpdateSpeed);
		}
	}
}
