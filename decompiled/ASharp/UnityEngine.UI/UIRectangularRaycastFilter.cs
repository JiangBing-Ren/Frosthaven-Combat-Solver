namespace UnityEngine.UI;

[AddComponentMenu("UI/Raycast Filters/Rectangular Raycast Filter")]
[RequireComponent(typeof(RectTransform))]
public class UIRectangularRaycastFilter : MonoBehaviour, ICanvasRaycastFilter
{
	[SerializeField]
	private Vector2 m_Offset = Vector2.zero;

	[SerializeField]
	private RectOffset m_Borders = new RectOffset();

	[Range(0f, 1f)]
	[SerializeField]
	private float m_ScaleX = 1f;

	[Range(0f, 1f)]
	[SerializeField]
	private float m_ScaleY = 1f;

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

	public RectOffset borders
	{
		get
		{
			return m_Borders;
		}
		set
		{
			m_Borders = value;
		}
	}

	public float scaleX
	{
		get
		{
			return m_ScaleX;
		}
		set
		{
			m_ScaleX = value;
		}
	}

	public float scaleY
	{
		get
		{
			return m_ScaleY;
		}
		set
		{
			m_ScaleY = value;
		}
	}

	public Rect scaledRect
	{
		get
		{
			RectTransform rectTransform = (RectTransform)transform;
			return new Rect(offset.x + (float)borders.left + (rectTransform.rect.x + (rectTransform.rect.width - rectTransform.rect.width * m_ScaleX) / 2f), offset.y + (float)borders.bottom + (rectTransform.rect.y + (rectTransform.rect.height - rectTransform.rect.height * m_ScaleY) / 2f), rectTransform.rect.width * m_ScaleX - (float)borders.left - (float)borders.right, rectTransform.rect.height * m_ScaleY - (float)borders.top - (float)borders.bottom);
		}
	}

	public Vector3[] scaledWorldCorners
	{
		get
		{
			RectTransform rectTransform = (RectTransform)transform;
			return new Vector3[4]
			{
				new Vector3(rectTransform.position.x + scaledRect.x, rectTransform.position.y + scaledRect.y, rectTransform.position.z),
				new Vector3(rectTransform.position.x + scaledRect.x + scaledRect.width, rectTransform.position.y + scaledRect.y, rectTransform.position.z),
				new Vector3(rectTransform.position.x + scaledRect.x + scaledRect.width, rectTransform.position.y + scaledRect.y + scaledRect.height, rectTransform.position.z),
				new Vector3(rectTransform.position.x + scaledRect.x, rectTransform.position.y + scaledRect.y + scaledRect.height, rectTransform.position.z)
			};
		}
	}

	public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
	{
		if (!enabled)
		{
			return true;
		}
		RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, screenPoint, eventCamera, out var localPoint);
		return scaledRect.Contains(localPoint);
	}
}
