using System;
using UnityEngine;

public class ElementPicker : MonoBehaviour
{
	[SerializeField]
	private GameObject content;

	private Action onOpenPicker;

	private Action onClosePicker;

	public bool IsOpen => content.gameObject.activeSelf;

	protected void Init(Action onOpenPicker = null, Action onClosePicker = null)
	{
		this.onClosePicker = onClosePicker;
		this.onOpenPicker = onOpenPicker;
	}

	public virtual void Show()
	{
		if (!content.activeSelf)
		{
			content.SetActive(value: true);
			onOpenPicker?.Invoke();
		}
	}

	public virtual void Hide()
	{
		if (content.activeSelf)
		{
			content.SetActive(value: false);
			onClosePicker?.Invoke();
		}
	}
}
