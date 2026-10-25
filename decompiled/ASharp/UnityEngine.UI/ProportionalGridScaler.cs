namespace UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(GridLayoutGroup))]
public class ProportionalGridScaler : MonoBehaviour
{
	[SerializeField]
	private Vector2 m_PreferredCellSize;

	private GridLayoutGroup m_GridLayout;

	private RectTransform m_RectTransform;

	private bool m_Invalidate;

	private void Awake()
	{
		m_GridLayout = GetComponent<GridLayoutGroup>();
		m_RectTransform = GetComponent<RectTransform>();
	}

	private void OnTransformChildrenChanged()
	{
		m_Invalidate = true;
	}

	private void OnEnable()
	{
		m_Invalidate = true;
	}

	private void LateUpdate()
	{
		UpdateCellSize();
	}

	private void UpdateCellSize()
	{
		if (m_GridLayout == null || m_RectTransform == null)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < m_RectTransform.childCount; i++)
		{
			if (m_RectTransform.GetChild(i).gameObject.activeInHierarchy)
			{
				num++;
			}
		}
		if (num == 0)
		{
			m_GridLayout.cellSize = m_PreferredCellSize;
			return;
		}
		float height = m_RectTransform.rect.height;
		float num2 = m_GridLayout.padding.top + m_GridLayout.padding.bottom;
		if (m_GridLayout.spacing.y != 0f && num > 1)
		{
			num2 -= m_GridLayout.spacing.y * (float)num - 1f;
		}
		float num3 = height - num2;
		float num4 = (float)num * m_PreferredCellSize.y;
		if (num3 >= num4)
		{
			m_GridLayout.cellSize = m_PreferredCellSize;
			return;
		}
		float num5 = num3 / num4;
		m_GridLayout.cellSize = m_PreferredCellSize * num5;
	}
}
