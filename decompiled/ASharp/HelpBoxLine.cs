using System.Collections;
using System.Text;
using FHGUI;
using GLOOM;
using TMPro;
using UnityEngine;

public class HelpBoxLine : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI tipText;

	[SerializeField]
	private GUIAnimator warningAnimation;

	[SerializeField]
	private TextMeshProUGUI warningTextAnimation;

	public Color ColorWarning;

	public Color ColorTitle;

	public Color ColorTip;

	public void Show(string tipKey = null, string titleKey = null, string translatedPrefix = null, HelpBox.FormatTarget useWarningFormat = HelpBox.FormatTarget.NONE)
	{
		ShowTranslated((tipKey != null) ? LocalizationManager.GetTranslation(tipKey) : null, (titleKey != null) ? LocalizationManager.GetTranslation(titleKey) : null, translatedPrefix, useWarningFormat);
	}

	public void ShowTranslated(string tip = null, string title = null, string translatedPrefix = null, HelpBox.FormatTarget useWarningFormat = HelpBox.FormatTarget.NONE)
	{
		StringBuilder str = new StringBuilder();
		Color targetColor;
		if (!string.IsNullOrEmpty(title))
		{
			targetColor = ((useWarningFormat == HelpBox.FormatTarget.ALL || useWarningFormat == HelpBox.FormatTarget.TITLE) ? ColorWarning : ColorTitle);
			appendWithColor(title);
		}
		if (!string.IsNullOrEmpty(tip))
		{
			targetColor = ((useWarningFormat == HelpBox.FormatTarget.ALL || useWarningFormat == HelpBox.FormatTarget.TEXT) ? ColorWarning : ColorTip);
			if (str.Length > 0)
			{
				appendWithColor(": ");
			}
			if (!string.IsNullOrEmpty(translatedPrefix))
			{
				appendWithColor(translatedPrefix);
			}
			appendWithColor(tip);
		}
		tipText.text = str.ToString();
		if (gameObject.activeInHierarchy)
		{
			OnEnable();
		}
		gameObject.SetActive(value: true);
		warningAnimation.Stop();
		void appendWithColor(string text)
		{
			str.Append(FHUILocFormatting.Color(text, targetColor));
		}
	}

	public void Hide()
	{
		warningAnimation.Stop();
		gameObject.SetActive(value: false);
	}

	public void HighlightWarning()
	{
		warningTextAnimation.text = tipText.text;
		warningAnimation.Play();
	}

	private void OnDisable()
	{
		warningAnimation.Stop();
	}

	private void OnEnable()
	{
		if (tipText != null)
		{
			tipText.raycastTarget = true;
			if (tipText.GetComponent<UIHelpBoxTooltipTarget>() == null)
			{
				tipText.gameObject.AddComponent<UIHelpBoxTooltipTarget>();
			}
		}
		StartCoroutine(WaitRegenerateSubmeshes());
	}

	private IEnumerator WaitRegenerateSubmeshes()
	{
		yield return null;
		RefreshSubmeshes();
	}

	private void RefreshSubmeshes()
	{
		TMP_SubMeshUI[] componentsInChildren = tipText.GetComponentsInChildren<TMP_SubMeshUI>();
		foreach (TMP_SubMeshUI obj in componentsInChildren)
		{
			obj.transform.SetParent(tipText.transform.parent);
			obj.transform.SetSiblingIndex(tipText.transform.GetSiblingIndex());
		}
	}
}
