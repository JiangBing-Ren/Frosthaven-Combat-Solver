using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TextMeshProTyping : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI dialogText;

	[SerializeField]
	private float reveralStartDelay = 0.1f;

	[SerializeField]
	private bool useGlobalRevealTime = true;

	[ConditionalField("useGlobalRevealTime", "false", true)]
	[SerializeField]
	private float revealTime = 0.1f;

	[SerializeField]
	private string audioItemSkip = "PlaySound_UIDialogueBoxSkip";

	private Coroutine reveralCoroutine;

	private Action onFinish;

	public void SetText(string text)
	{
		StopReveal();
		dialogText.text = text;
		dialogText.maxVisibleCharacters = 0;
	}

	public void Reveal(string text, Action onFinish = null)
	{
		SetText(text);
		Reveal(onFinish);
	}

	public void Reveal(Action onFinish = null)
	{
		this.onFinish = onFinish;
		StopReveal();
		reveralCoroutine = StartCoroutine(PlayReveal());
	}

	private IEnumerator PlayReveal()
	{
		int totalVisibleCharacters = dialogText.textInfo.characterCount;
		int characters = 0;
		float time = (useGlobalRevealTime ? UIInfoTools.Instance.typingRevealDelay : revealTime);
		while (true)
		{
			int num = characters % (totalVisibleCharacters + 1);
			dialogText.maxVisibleCharacters = num;
			if (num >= totalVisibleCharacters)
			{
				break;
			}
			yield return new WaitForSecondsRealtime((characters == 0) ? reveralStartDelay : time);
			characters++;
		}
		reveralCoroutine = null;
		OnEndReveal();
	}

	private void OnEndReveal()
	{
		onFinish?.Invoke();
	}

	private void StopReveal()
	{
		if (reveralCoroutine != null)
		{
			StopCoroutine(reveralCoroutine);
			reveralCoroutine = null;
		}
	}

	public void Skip()
	{
		AudioControllerUtils.PlaySound(audioItemSkip);
		if (reveralCoroutine != null)
		{
			SkipReveal();
		}
		else
		{
			OnEndReveal();
		}
	}

	private void SkipReveal()
	{
		StopReveal();
		dialogText.maxVisibleCharacters = dialogText.textInfo.characterCount;
		OnEndReveal();
	}
}
