using UnityEngine;
using UnityEngine.UI;

public abstract class ZLEGACY_UILocalTooltip : MonoBehaviour
{
	[Tooltip("If window is null, it will enable/disable the object when this toolip is shown/hidden. If a window is assigned, it will show/hide that window when this tooltip is shown/hidden")]
	[SerializeField]
	private UIWindow window;

	[Header("Target Position")]
	[SerializeField]
	private RectTransform target;

	[SerializeField]
	private Vector2 targetOffset;

	[Header("Screen bound")]
	[SerializeField]
	private bool trackScreenBound;

	[ConditionalField("trackScreenBound", null, true)]
	[SerializeField]
	private float offsetScreen = 20f;

	public bool IsShown
	{
		get
		{
			if (gameObject.activeSelf)
			{
				if (!(window == null))
				{
					return window.IsOpen;
				}
				return true;
			}
			return false;
		}
	}

	public virtual void SetTarget(RectTransform newTarget)
	{
		target = newTarget;
		RefreshPosition();
	}

	public void SetTarget(RectTransform newTarget, Vector2 newTargetOffset)
	{
		targetOffset = newTargetOffset;
		SetTarget(newTarget);
	}

	public void Show()
	{
		if (window != null)
		{
			window.Show();
		}
		else
		{
			gameObject.SetActive(value: true);
		}
		RefreshPosition();
		enabled = trackScreenBound;
	}

	public virtual void Hide()
	{
		if (window != null)
		{
			window.Hide();
			enabled = false;
		}
		else
		{
			gameObject.SetActive(value: false);
		}
	}

	private void LateUpdate()
	{
		RefreshPosition();
	}

	public void RefreshPosition()
	{
		if (!(target == null) && IsShown)
		{
			SetPosition(target);
			(transform as RectTransform).anchoredPosition += targetOffset;
			transform.position += (transform as RectTransform).DeltaWorldPositionToFitTheScreen(UIManager.Instance.UICamera, offsetScreen);
		}
	}

	protected virtual void SetPosition(RectTransform targetPosition)
	{
		transform.position = target.position;
	}
}
