namespace UnityEngine.UI;

[AddComponentMenu("UI/Raycast Filters/Mask Raycast Filter")]
[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public class UIMaskRaycastFilter : MonoBehaviour, ICanvasRaycastFilter
{
	private Image m_Image;

	private Sprite m_Sprite;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_AlphaTreshold = 0.01f;

	protected void Awake()
	{
		m_Image = gameObject.GetComponent<Image>();
		if (m_Image != null)
		{
			m_Sprite = m_Image.sprite;
		}
	}

	public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
	{
		if (m_Image == null || m_Sprite == null)
		{
			return false;
		}
		RectTransform rectTransform = (RectTransform)transform;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, sp, eventCamera, out var localPoint);
		Vector2 vector = new Vector2(localPoint.x + rectTransform.pivot.x * rectTransform.rect.width, localPoint.y + rectTransform.pivot.y * rectTransform.rect.height);
		Rect textureRect = m_Sprite.textureRect;
		Rect rect = rectTransform.rect;
		int num = 0;
		int num2 = 0;
		Image.Type type = m_Image.type;
		if (type != Image.Type.Simple && type == Image.Type.Sliced)
		{
			Vector4 border = m_Sprite.border;
			if (vector.x < border.x)
			{
				num = Mathf.FloorToInt(textureRect.x + vector.x);
			}
			else
			{
				num = ((!(vector.x > rect.width - border.z)) ? Mathf.FloorToInt(textureRect.x + border.x + (vector.x - border.x) / (rect.width - border.x - border.z) * (textureRect.width - border.x - border.z)) : Mathf.FloorToInt(textureRect.x + textureRect.width - (rect.width - vector.x)));
			}
			if (vector.y < border.y)
			{
				num2 = Mathf.FloorToInt(textureRect.y + vector.y);
			}
			else
			{
				num2 = ((!(vector.y > rect.height - border.w)) ? Mathf.FloorToInt(textureRect.y + border.y + (vector.y - border.y) / (rect.height - border.y - border.w) * (textureRect.height - border.y - border.w)) : Mathf.FloorToInt(textureRect.y + textureRect.height - (rect.height - vector.y)));
			}
		}
		else
		{
			num = Mathf.FloorToInt(textureRect.x + textureRect.width * vector.x / rect.width);
			num2 = Mathf.FloorToInt(textureRect.y + textureRect.height * vector.y / rect.height);
		}
		try
		{
			return m_Sprite.texture.GetPixel(num, num2).a > m_AlphaTreshold;
		}
		catch (UnityException exception)
		{
			Debug.LogException(exception);
			Object.Destroy(this);
			return false;
		}
	}
}
