using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace OxOD;

public class DoubleclickUI : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	private int tap;

	public float interval = 0.5f;

	private bool readyForDoubleTap;

	public UnityEvent onSingleClick;

	public UnityEvent onDoubleClick;

	public void OnPointerClick(PointerEventData eventData)
	{
		tap++;
		if (tap == 1)
		{
			onSingleClick.Invoke();
			StartCoroutine(DoubleTapInterval());
		}
		else if (tap > 1 && readyForDoubleTap)
		{
			onDoubleClick.Invoke();
			StopCoroutine(DoubleTapInterval());
			tap = 0;
			readyForDoubleTap = false;
		}
	}

	private IEnumerator DoubleTapInterval()
	{
		readyForDoubleTap = true;
		yield return new WaitForSeconds(interval);
		readyForDoubleTap = false;
		tap = 0;
	}
}
