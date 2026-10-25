using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Gloomhaven;

public class UISliderController : UISliderBar, IPointerUpHandler, IEventSystemHandler, ISelectHandler, IDeselectHandler
{
	[Serializable]
	public class SliderOnPointerUpEvent : UnityEvent<float>
	{
	}

	public SliderOnPointerUpEvent sliderOnPointerUp;

	public UnityEvent OnSelected;

	public UnityEvent OnDeselected;

	private int oldValue;

	private void Start()
	{
		progress.onValueChanged.AddListener((float val) =>
		{
			SetAmountText((int)val);
		});
	}

	public override void SetAmount(int amount)
	{
		oldValue = amount;
		base.SetAmount(amount);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if ((int)progress.value != oldValue)
		{
			sliderOnPointerUp.Invoke(progress.value);
			oldValue = (int)progress.value;
		}
	}

	private void OnDisable()
	{
		if (!GloomUtility.IsAppQuitting && EventSystem.current.currentSelectedGameObject == gameObject)
		{
			OnDeselect(null);
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		if (Singleton<InputFHManager>.Instance.UsingGamepad)
		{
			OnSelected?.Invoke();
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		if ((int)progress.value != oldValue)
		{
			sliderOnPointerUp.Invoke(progress.value);
			oldValue = (int)progress.value;
		}
		OnDeselected?.Invoke();
	}
}
