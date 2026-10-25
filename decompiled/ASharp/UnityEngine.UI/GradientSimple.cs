using System.Collections.Generic;

namespace UnityEngine.UI;

[AddComponentMenu("UI/Effects/Gradient")]
public class GradientSimple : BaseMeshEffect
{
	[SerializeField]
	private Color topColor = Color.white;

	[SerializeField]
	private Color bottomColor = Color.black;

	public override void ModifyMesh(VertexHelper vertexHelper)
	{
		if (IsActive())
		{
			List<UIVertex> list = new List<UIVertex>();
			vertexHelper.GetUIVertexStream(list);
			ModifyVertices(list);
			vertexHelper.Clear();
			vertexHelper.AddUIVertexTriangleStream(list);
		}
	}

	public void ModifyVertices(List<UIVertex> vertexList)
	{
		if (!IsActive())
		{
			return;
		}
		int count = vertexList.Count;
		float num = vertexList[0].position.y;
		float num2 = vertexList[0].position.y;
		for (int i = 1; i < count; i++)
		{
			float y = vertexList[i].position.y;
			if (y > num2)
			{
				num2 = y;
			}
			else if (y < num)
			{
				num = y;
			}
		}
		float num3 = num2 - num;
		for (int j = 0; j < count; j++)
		{
			UIVertex value = vertexList[j];
			value.color *= Color.Lerp(bottomColor, topColor, (value.position.y - num) / num3);
			vertexList[j] = value;
		}
	}
}
