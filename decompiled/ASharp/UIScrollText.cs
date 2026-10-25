using System.Collections;
using Chronos;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIScrollText : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI textMeshPro;

	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private float scrollSpeed = 2.5f;

	private bool scrolling;

	private string lastScrollingText;

	public void ShowText(string text, bool instant = false)
	{
		if (text.IsNullOrEmpty())
		{
			Hide();
		}
		else if (instant)
		{
			lastScrollingText = null;
			StopScroll();
			textMeshPro.text = text;
			gameObject.SetActive(value: true);
		}
		else if (lastScrollingText != text)
		{
			if (lastScrollingText.IsNullOrEmpty())
			{
				textMeshPro.text = null;
			}
			lastScrollingText = text;
			textMeshPro.text = (textMeshPro.text.IsNullOrEmpty() ? text : (textMeshPro.text + "\n" + text));
			gameObject.SetActive(value: true);
			if (!scrolling)
			{
				StartCoroutine(Scroll());
			}
		}
	}

	public void Hide()
	{
		lastScrollingText = null;
		gameObject.SetActive(value: false);
		StopScroll();
		textMeshPro.text = null;
	}

	private void StopScroll()
	{
		scrolling = false;
		StopAllCoroutines();
	}

	private IEnumerator Scroll()
	{
		scrolling = true;
		scrollRect.verticalNormalizedPosition = 1f;
		while (scrollRect.verticalNormalizedPosition > 0f)
		{
			yield return null;
			scrollRect.verticalNormalizedPosition = Mathf.Max(0f, scrollRect.verticalNormalizedPosition - Timekeeper.instance.m_GlobalClock.deltaTime * scrollSpeed);
		}
		textMeshPro.text = lastScrollingText;
		scrollRect.verticalNormalizedPosition = 1f;
		StopScroll();
	}
}
