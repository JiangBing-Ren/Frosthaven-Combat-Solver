using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FHGUI.Common.Elements;

public class UIHoldToSkipElement : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler
{
	[Header("Components")]
	[SerializeField]
	private GameObject SkipGroup;

	[SerializeField]
	private GameObject SkipProgressGroup;

	[SerializeField]
	private Image SkipProgressImage;

	[Header("Settings")]
	[SerializeField]
	private float HoldDelay = 1.2f;

	[SerializeField]
	private float HoldDeadZone = 0.35f;

	private Action OnSkip;

	private float holdDuration;

	private Coroutine skipRoutine;

	public void Init(Action skipCallback)
	{
		OnSkip = skipCallback;
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
		OnSkip?.Invoke();
	}

	private void OnDisable()
	{
		OnSkip = null;
		holdDuration = 0f;
		skipRoutine = null;
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
		if (eventData.button == PointerEventData.InputButton.Left)
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
