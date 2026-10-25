using System;
using System.Collections;
using FHGUI.DTO;
using FHGUI.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FHEventMarker : MonoBehaviour
{
	[SerializeField]
	private Image _icon;

	[SerializeField]
	private TextLocalizedListener _label;

	[SerializeField]
	private UITweenEffects _iconShowEffects;

	[SerializeField]
	private UITweenEffects _labelShowEffects;

	public void Init(OutpostEventDTO eventDef)
	{
		if ((bool)_icon)
		{
			_icon.sprite = eventDef.Icon;
		}
		if (_label != null)
		{
			_label.SetTextKey("GUI_ENCOUNTER");
		}
	}

	public void Show(Vector3 position, Action OnComplete)
	{
		transform.position = position;
		gameObject.SetActive(value: true);
		StartCoroutine(WaitForVisualEffect(OnComplete));
	}

	private IEnumerator WaitForVisualEffect(Action OnComplete)
	{
		_labelShowEffects.Play();
		_iconShowEffects.Play();
		while (_labelShowEffects.IsActive() || _iconShowEffects.IsActive())
		{
			yield return null;
		}
		OnComplete?.Invoke();
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
	}
}
