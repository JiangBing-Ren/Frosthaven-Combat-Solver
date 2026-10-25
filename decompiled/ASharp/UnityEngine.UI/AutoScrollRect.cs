using System.Collections;
using Chronos;

namespace UnityEngine.UI;

public class AutoScrollRect : MonoBehaviour
{
	public enum AutoscrollType
	{
		VERTICAL,
		HORIZONTAL,
		BOTH
	}

	public enum AutoscrollSpeedType
	{
		PERCENT,
		DISTANCE
	}

	public ScrollRect scroll;

	public bool autoscroll;

	public AutoscrollType autoscrollType = AutoscrollType.BOTH;

	public AutoscrollSpeedType speedType = AutoscrollSpeedType.DISTANCE;

	public float autoscrollSpeed = 0.01f;

	public float autoscrollUpdateTime = 0.1f;

	public float autoscrollDelayExtremes = 0.1f;

	private int directionV = 1;

	private int directionH = 1;

	private void Awake()
	{
		if (scroll == null)
		{
			scroll = GetComponent<ScrollRect>();
		}
		if (scroll == null)
		{
			autoscroll = false;
		}
	}

	public bool NeedsVerticalScroll()
	{
		if (scroll == null || scroll.content == null || scroll.viewport == null)
		{
			return false;
		}
		return scroll.content.sizeDelta.y > scroll.viewport.sizeDelta.y;
	}

	public bool NeedsHorizontalScroll()
	{
		if (scroll == null || scroll.content == null || scroll.viewport == null)
		{
			return false;
		}
		return scroll.content.sizeDelta.x > scroll.viewport.sizeDelta.x;
	}

	protected void OnEnable()
	{
		StartAutoscrollCorutine();
	}

	protected void OnDisable()
	{
		if (autoscroll)
		{
			StopAllCoroutines();
		}
	}

	private void StartAutoscrollCorutine()
	{
		if (autoscroll && gameObject.activeInHierarchy)
		{
			StartCoroutine(Autoscroll());
		}
	}

	[ContextMenu("StartAutoscroll")]
	public void StartAutoscroll()
	{
		if (!autoscroll)
		{
			autoscroll = scroll != null;
			StartAutoscrollCorutine();
		}
	}

	[ContextMenu("ForceStartAutoscroll")]
	public void ForceStartAutoscroll()
	{
		autoscroll = scroll != null;
		StopAllCoroutines();
		StartAutoscrollCorutine();
	}

	private IEnumerator Autoscroll()
	{
		while (autoscroll)
		{
			bool flag = false;
			if (autoscrollType == AutoscrollType.VERTICAL || autoscrollType == AutoscrollType.BOTH)
			{
				flag |= AutoscrollVertical();
			}
			if (autoscrollType == AutoscrollType.HORIZONTAL || autoscrollType == AutoscrollType.BOTH)
			{
				flag |= AutoscrollHorizontal();
			}
			yield return Timekeeper.instance.WaitForSeconds(flag ? (autoscrollUpdateTime + autoscrollDelayExtremes) : autoscrollUpdateTime);
		}
	}

	private bool AutoscrollVertical()
	{
		if (!NeedsVerticalScroll())
		{
			return false;
		}
		scroll.verticalNormalizedPosition += CalculateIncreaseV() * (float)directionV;
		if (scroll.verticalNormalizedPosition >= 1f && directionV > 0)
		{
			directionV = -1;
			return true;
		}
		if (scroll.verticalNormalizedPosition <= 0f && directionV < 0)
		{
			directionV = 1;
			return true;
		}
		return false;
	}

	private bool AutoscrollHorizontal()
	{
		if (!NeedsHorizontalScroll())
		{
			return false;
		}
		scroll.horizontalNormalizedPosition += CalculateIncreaseH() * (float)directionH;
		if (scroll.horizontalNormalizedPosition >= 1f && directionH > 0)
		{
			directionH = -1;
			return true;
		}
		if (scroll.horizontalNormalizedPosition <= 0f && directionH < 0)
		{
			directionH = 1;
			return true;
		}
		return false;
	}

	private float CalculateIncreaseH()
	{
		if (speedType == AutoscrollSpeedType.PERCENT)
		{
			return autoscrollSpeed;
		}
		return autoscrollSpeed / scroll.content.sizeDelta.x;
	}

	private float CalculateIncreaseV()
	{
		if (speedType == AutoscrollSpeedType.PERCENT)
		{
			return autoscrollSpeed;
		}
		return autoscrollSpeed / scroll.content.sizeDelta.y;
	}

	[ContextMenu("StopAutoscroll")]
	public void StopAutoscroll()
	{
		autoscroll = false;
		StopAllCoroutines();
	}
}
