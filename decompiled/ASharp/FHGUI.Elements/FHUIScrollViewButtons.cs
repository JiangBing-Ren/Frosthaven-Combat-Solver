using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class FHUIScrollViewButtons : MonoBehaviour
{
	[Serializable]
	public enum MovementType
	{
		Absolute,
		PerItem
	}

	public struct VisibilityData
	{
		public int VisibleCount;

		public int InvisibleLeft;

		public int InvisibleRight;

		public int InvisibleOver;

		public int InvisibleUnder;
	}

	[Header("Components")]
	[FormerlySerializedAs("m_ScrollView")]
	[SerializeField]
	private ScrollRect m_ScrollRect;

	[SerializeField]
	private FHButton m_Up;

	[SerializeField]
	private FHButton m_Down;

	[SerializeField]
	private FHButton m_Left;

	[SerializeField]
	private FHButton m_Right;

	[SerializeField]
	private TMP_Text m_CountUp;

	[SerializeField]
	private TMP_Text m_CountDown;

	[SerializeField]
	private TMP_Text m_CountLeft;

	[SerializeField]
	private TMP_Text m_CountRight;

	[SerializeField]
	private GameObject m_GroupCountUp;

	[SerializeField]
	private GameObject m_GroupCountDown;

	[SerializeField]
	private GameObject m_GroupCountLeft;

	[SerializeField]
	private GameObject m_GroupCountRight;

	[Header("Settings")]
	[SerializeField]
	private float m_Speed = 90f;

	[SerializeField]
	private float m_HideButtonsIfNotOverflowing;

	[SerializeField]
	private float m_TweenFocusDuration = 0.3f;

	[SerializeField]
	private Ease m_TweenFocusEase = Ease.OutCubic;

	[SerializeField]
	private MovementType movementType;

	private bool m_InvalidateVisuals;

	private void Awake()
	{
		if (m_ScrollRect == null)
		{
			GetComponent<ScrollRect>();
		}
		if (m_ScrollRect != null)
		{
			m_ScrollRect.onValueChanged.AddListener(HandleOnScroll);
		}
		if (m_Up != null)
		{
			m_Up.onClick.AddListener(HandleOnUp);
		}
		if (m_Down != null)
		{
			m_Down.onClick.AddListener(HandleOnDown);
		}
		if (m_Left != null)
		{
			m_Left.onClick.AddListener(HandleOnLeft);
		}
		if (m_Right != null)
		{
			m_Right.onClick.AddListener(HandleOnRight);
		}
	}

	private void OnEnable()
	{
		m_InvalidateVisuals = true;
	}

	private void LateUpdate()
	{
		if (m_InvalidateVisuals)
		{
			UpdateVisuals();
			m_InvalidateVisuals = false;
		}
	}

	private void UpdateVisuals()
	{
		Rect rect = m_ScrollRect.content.rect;
		Rect rect2 = m_ScrollRect.viewport.rect;
		if (rect2.width >= rect.width && rect2.height >= rect.height)
		{
			UpdateButton(m_Up, show: false, interactable: false);
			UpdateButton(m_Down, show: false, interactable: false);
			UpdateButton(m_Left, show: false, interactable: false);
			UpdateButton(m_Right, show: false, interactable: false);
		}
		else
		{
			UpdateButton(m_Up, show: true, !IsAtTopBorder());
			UpdateButton(m_Down, show: true, !IsAtBottomBorder());
			UpdateButton(m_Left, show: true, !IsAtLeftBorder());
			UpdateButton(m_Right, show: true, !IsAtRightBorder());
		}
		VisibilityData visibilityMetrics = GetVisibilityMetrics();
		if ((bool)m_CountUp)
		{
			m_CountUp.gameObject.SetActive(visibilityMetrics.InvisibleOver > 0);
			if (visibilityMetrics.InvisibleOver > 0)
			{
				m_CountUp.text = visibilityMetrics.InvisibleOver.ToString();
			}
		}
		if ((bool)m_CountDown)
		{
			m_CountDown.gameObject.SetActive(visibilityMetrics.InvisibleUnder > 0);
			if (visibilityMetrics.InvisibleUnder > 0)
			{
				m_CountDown.text = visibilityMetrics.InvisibleUnder.ToString();
			}
		}
		if ((bool)m_CountLeft)
		{
			m_CountLeft.gameObject.SetActive(visibilityMetrics.InvisibleLeft > 0);
			if (visibilityMetrics.InvisibleLeft > 0)
			{
				m_CountLeft.text = visibilityMetrics.InvisibleLeft.ToString();
			}
		}
		if ((bool)m_CountRight)
		{
			m_CountRight.gameObject.SetActive(visibilityMetrics.InvisibleRight > 0);
			if (visibilityMetrics.InvisibleRight > 0)
			{
				m_CountRight.text = visibilityMetrics.InvisibleRight.ToString();
			}
		}
		if ((bool)m_GroupCountUp)
		{
			m_GroupCountUp.gameObject.SetActive(visibilityMetrics.InvisibleOver > 0);
		}
		if ((bool)m_GroupCountDown)
		{
			m_GroupCountDown.gameObject.SetActive(visibilityMetrics.InvisibleUnder > 0);
		}
		if ((bool)m_GroupCountLeft)
		{
			m_GroupCountLeft.gameObject.SetActive(visibilityMetrics.InvisibleLeft > 0);
		}
		if ((bool)m_GroupCountRight)
		{
			m_GroupCountRight.gameObject.SetActive(visibilityMetrics.InvisibleRight > 0);
		}
	}

	private void UpdateButton(FHButton button, bool show, bool interactable)
	{
		if (!(button == null))
		{
			button.gameObject.SetActive(show);
			button.interactable = interactable;
		}
	}

	private bool IsAtTopBorder()
	{
		Vector2 normalizedPosition = m_ScrollRect.normalizedPosition;
		if (m_ScrollRect.vertical && normalizedPosition.y < 1f)
		{
			return Mathf.Approximately(normalizedPosition.y, 1f);
		}
		return true;
	}

	private bool IsAtBottomBorder()
	{
		Vector2 normalizedPosition = m_ScrollRect.normalizedPosition;
		if (m_ScrollRect.vertical && m_ScrollRect.normalizedPosition.y > 0f)
		{
			return Mathf.Approximately(normalizedPosition.y, 0f);
		}
		return true;
	}

	private bool IsAtRightBorder()
	{
		Vector2 normalizedPosition = m_ScrollRect.normalizedPosition;
		if (m_ScrollRect.horizontal && normalizedPosition.x < 1f)
		{
			return Mathf.Approximately(normalizedPosition.x, 1f);
		}
		return true;
	}

	private bool IsAtLeftBorder()
	{
		Vector2 normalizedPosition = m_ScrollRect.normalizedPosition;
		if (m_ScrollRect.horizontal && normalizedPosition.x > 0f)
		{
			return Mathf.Approximately(normalizedPosition.x, 0f);
		}
		return true;
	}

	private void HandleOnUp()
	{
		if (movementType == MovementType.Absolute)
		{
			m_ScrollRect.velocity -= new Vector2(0f, m_Speed);
		}
		if (movementType == MovementType.PerItem)
		{
			FitInScrollRectPosition(m_ScrollRect, GetPrevInvisibleElement(), Vector2.zero);
		}
	}

	private void HandleOnDown()
	{
		if (movementType == MovementType.Absolute)
		{
			m_ScrollRect.velocity += new Vector2(0f, m_Speed);
		}
		if (movementType == MovementType.PerItem)
		{
			FitInScrollRectPosition(m_ScrollRect, GetNextInvisibleElement(), Vector2.zero);
		}
	}

	private void HandleOnLeft()
	{
		if (movementType == MovementType.Absolute)
		{
			m_ScrollRect.velocity += new Vector2(m_Speed, 0f);
		}
		if (movementType == MovementType.PerItem)
		{
			FitInScrollRectPosition(m_ScrollRect, GetPrevInvisibleElement(), Vector2.zero);
		}
	}

	private void HandleOnRight()
	{
		if (movementType == MovementType.Absolute)
		{
			m_ScrollRect.velocity -= new Vector2(m_Speed, 0f);
		}
		if (movementType == MovementType.PerItem)
		{
			FitInScrollRectPosition(m_ScrollRect, GetNextInvisibleElement(), Vector2.zero);
		}
	}

	private void HandleOnScroll(Vector2 pos)
	{
		m_InvalidateVisuals = true;
	}

	private RectTransform GetPrevInvisibleElement()
	{
		RectTransform viewport = m_ScrollRect.viewport;
		RectTransform content = m_ScrollRect.content;
		Vector3[] array = new Vector3[4];
		viewport.GetWorldCorners(array);
		Rect screenRect = GetScreenRect(array);
		RectTransform result = null;
		foreach (RectTransform item in content)
		{
			if (item.gameObject.activeSelf && !item.name.Contains("Separator"))
			{
				Vector3[] array2 = new Vector3[4];
				item.GetWorldCorners(array2);
				Rect screenRect2 = GetScreenRect(array2);
				if (screenRect.Overlaps(screenRect2, allowInverse: false))
				{
					break;
				}
				result = item;
			}
		}
		return result;
	}

	private RectTransform GetNextInvisibleElement()
	{
		RectTransform viewport = m_ScrollRect.viewport;
		RectTransform content = m_ScrollRect.content;
		Vector3[] array = new Vector3[4];
		viewport.GetWorldCorners(array);
		Rect screenRect = GetScreenRect(array);
		RectTransform rectTransform = null;
		RectTransform result = null;
		foreach (RectTransform item in content)
		{
			if (item.gameObject.activeSelf && !item.name.Contains("Separator"))
			{
				Vector3[] array2 = new Vector3[4];
				item.GetWorldCorners(array2);
				Rect screenRect2 = GetScreenRect(array2);
				if (screenRect.Overlaps(screenRect2, allowInverse: false))
				{
					rectTransform = item;
				}
				else if (rectTransform != null)
				{
					result = item;
					break;
				}
			}
		}
		return result;
	}

	public void Refresh()
	{
		m_InvalidateVisuals = true;
	}

	public void Focus(RectTransform childToFocus)
	{
		if (m_ScrollRect?.content == null)
		{
			return;
		}
		foreach (RectTransform item in m_ScrollRect.content)
		{
			if (item == childToFocus && item.gameObject.activeSelf)
			{
				FitInScrollRectPosition(m_ScrollRect, item, Vector2.zero);
			}
		}
	}

	private Rect GetScreenRect(Vector3[] corners)
	{
		float num = Mathf.Min(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
		float num2 = Mathf.Max(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
		float num3 = Mathf.Min(corners[0].y, corners[1].y, corners[2].y, corners[3].y);
		float num4 = Mathf.Max(corners[0].y, corners[1].y, corners[2].y, corners[3].y);
		return new Rect(num, num3, num2 - num, num4 - num3);
	}

	public VisibilityData GetVisibilityMetrics(bool requireCompletelyVisible = false)
	{
		VisibilityData result = default;
		RectTransform viewport = m_ScrollRect.viewport;
		RectTransform content = m_ScrollRect.content;
		if (viewport == null || content == null)
		{
			return result;
		}
		Vector3[] array = new Vector3[4];
		viewport.GetWorldCorners(array);
		Rect screenRect = GetScreenRect(array);
		foreach (RectTransform item in content)
		{
			if (item.gameObject.activeSelf)
			{
				Vector3[] array2 = new Vector3[4];
				item.GetWorldCorners(array2);
				Rect screenRect2 = GetScreenRect(array2);
				bool flag = (requireCompletelyVisible ? (screenRect2.max.x < screenRect.max.x) : (screenRect2.max.x <= screenRect.min.x));
				bool flag2 = (requireCompletelyVisible ? (screenRect2.min.x > screenRect.min.x) : (screenRect2.min.x >= screenRect.max.x));
				bool flag3 = (requireCompletelyVisible ? (screenRect2.min.y > screenRect.min.y) : (screenRect2.min.y >= screenRect.max.y));
				bool flag4 = (requireCompletelyVisible ? (screenRect2.max.y < screenRect.max.y) : (screenRect2.max.y <= screenRect.min.y));
				if (flag)
				{
					result.InvisibleLeft++;
				}
				else if (flag2)
				{
					result.InvisibleRight++;
				}
				else if (flag3)
				{
					result.InvisibleOver++;
				}
				else if (flag4)
				{
					result.InvisibleUnder++;
				}
				else
				{
					result.VisibleCount++;
				}
			}
		}
		return result;
	}

	private void FitInScrollRectPosition(ScrollRect scrollRect, RectTransform child, Vector2 margin)
	{
		if (!(child == null))
		{
			Vector2 min = scrollRect.viewport.rect.min;
			Vector2 max = scrollRect.viewport.rect.max;
			Vector2 vector = scrollRect.viewport.InverseTransformPoint(child.TransformPoint(child.rect.min));
			Vector2 vector2 = scrollRect.viewport.InverseTransformPoint(child.TransformPoint(child.rect.max));
			vector -= margin;
			vector2 += margin;
			Vector2 zero = Vector2.zero;
			if (vector2.y > max.y)
			{
				zero.y = vector2.y - max.y;
			}
			if (vector.x < min.x)
			{
				zero.x = vector.x - min.x;
			}
			if (vector2.x > max.x)
			{
				zero.x = vector2.x - max.x;
			}
			if (vector.y < min.y)
			{
				zero.y = vector.y - min.y;
			}
			Vector3 direction = scrollRect.viewport.TransformDirection(zero);
			Vector3 endValue = scrollRect.content.localPosition - scrollRect.content.InverseTransformDirection(direction);
			scrollRect.content.DOLocalMove(endValue, m_TweenFocusDuration).SetEase(m_TweenFocusEase);
		}
	}
}
