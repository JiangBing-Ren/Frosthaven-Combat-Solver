namespace UnityEngine.UI;

[AddComponentMenu("UI/Raycast Filters/Circular Raycast Filter")]
[RequireComponent(typeof(RectTransform))]
public class UICircularRaycastFilter : MonoBehaviour, ICanvasRaycastFilter
{
	[SerializeField]
	private bool m_RadiusInPercents = true;

	[Range(0f, 100f)]
	[SerializeField]
	private float m_Radius = 70f;

	[SerializeField]
	private Vector2 m_Offset = Vector2.zero;

	public bool radiusInPercents
	{
		get
		{
			return m_RadiusInPercents;
		}
		set
		{
			m_RadiusInPercents = value;
		}
	}

	public float radius
	{
		get
		{
			return m_Radius;
		}
		set
		{
			m_Radius = value;
		}
	}

	public Vector2 offset
	{
		get
		{
			return m_Offset;
		}
		set
		{
			m_Offset = value;
		}
	}

	public Vector2 center
	{
		get
		{
			RectTransform rectTransform = (RectTransform)transform;
			return new Vector2(rectTransform.rect.width / 2f, rectTransform.rect.height / 2f);
		}
	}

	private float rectMaxRadius => Mathf.Max(center.x, center.y);

	public float operationalRadius
	{
		get
		{
			if (!m_RadiusInPercents)
			{
				return m_Radius;
			}
			return rectMaxRadius * (m_Radius / 100f);
		}
	}

	public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
	{
		if (!enabled)
		{
			return true;
		}
		if (m_Radius == 0f)
		{
			return false;
		}
		RectTransform rectTransform = (RectTransform)transform;
		RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, screenPoint, eventCamera, out var localPoint);
		Vector2 a = new Vector2(localPoint.x + rectTransform.pivot.x * rectTransform.rect.width, localPoint.y + rectTransform.pivot.y * rectTransform.rect.height);
		Vector2 b = offset + center;
		return Vector2.Distance(a, b) <= operationalRadius;
	}
}
