using System.Collections.Generic;

namespace UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Graphic))]
[DisallowMultipleComponent]
[AddComponentMenu("UI/Flippable")]
public class UIFlippable : MonoBehaviour, IMeshModifier
{
	[SerializeField]
	private bool m_Horizontal;

	[SerializeField]
	private bool m_Veritical;

	public bool horizontal
	{
		get
		{
			return m_Horizontal;
		}
		set
		{
			m_Horizontal = value;
		}
	}

	public bool vertical
	{
		get
		{
			return m_Veritical;
		}
		set
		{
			m_Veritical = value;
		}
	}

	public void ModifyMesh(VertexHelper vertexHelper)
	{
		if (enabled)
		{
			List<UIVertex> list = new List<UIVertex>();
			vertexHelper.GetUIVertexStream(list);
			ModifyVertices(list);
			vertexHelper.Clear();
			vertexHelper.AddUIVertexTriangleStream(list);
		}
	}

	public void ModifyMesh(Mesh mesh)
	{
		if (!enabled)
		{
			return;
		}
		List<UIVertex> list = new List<UIVertex>();
		using (VertexHelper vertexHelper = new VertexHelper(mesh))
		{
			vertexHelper.GetUIVertexStream(list);
		}
		ModifyVertices(list);
		using VertexHelper vertexHelper2 = new VertexHelper();
		vertexHelper2.AddUIVertexTriangleStream(list);
		vertexHelper2.FillMesh(mesh);
	}

	public void ModifyVertices(List<UIVertex> verts)
	{
		if (enabled)
		{
			RectTransform rectTransform = transform as RectTransform;
			for (int i = 0; i < verts.Count; i++)
			{
				UIVertex value = verts[i];
				value.position = new Vector3(m_Horizontal ? (value.position.x + (rectTransform.rect.center.x - value.position.x) * 2f) : value.position.x, m_Veritical ? (value.position.y + (rectTransform.rect.center.y - value.position.y) * 2f) : value.position.y, value.position.z);
				verts[i] = value;
			}
		}
	}
}
