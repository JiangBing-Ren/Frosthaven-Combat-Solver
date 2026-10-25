using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ProceduralMistTools
{
	private struct Edge(int a, int b)
	{
		public int A = Mathf.Min(a, b);

		public int B = Mathf.Max(a, b);

		public override bool Equals(object obj)
		{
			if (!(obj is Edge edge))
			{
				return false;
			}
			if (A == edge.A)
			{
				return B == edge.B;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return A * 31 + B;
		}
	}

	public static void FindPerimeter(List<Vector3> vertices, List<int> triangles, out List<int> outerLoop)
	{
		Dictionary<Edge, int> dictionary = new Dictionary<Edge, int>();
		for (int i = 0; i < triangles.Count; i += 3)
		{
			int num = triangles[i];
			int num2 = triangles[i + 1];
			int num3 = triangles[i + 2];
			CountEdge(dictionary, new Edge(num, num2));
			CountEdge(dictionary, new Edge(num2, num3));
			CountEdge(dictionary, new Edge(num3, num));
		}
		List<Edge> list = new List<Edge>();
		foreach (KeyValuePair<Edge, int> item in dictionary)
		{
			if (item.Value == 1)
			{
				list.Add(item.Key);
			}
		}
		List<List<int>> list2 = BuildLoops(list);
		outerLoop = null;
		float num4 = float.MinValue;
		foreach (List<int> item2 in list2)
		{
			float f = ComputePolygonArea(item2, vertices);
			if (Mathf.Abs(f) > num4)
			{
				num4 = Mathf.Abs(f);
				outerLoop = item2;
			}
		}
	}

	private static void CountEdge(Dictionary<Edge, int> edgeCount, Edge edge)
	{
		if (edgeCount.ContainsKey(edge))
		{
			edgeCount[edge]++;
		}
		else
		{
			edgeCount[edge] = 1;
		}
	}

	private static List<List<int>> BuildLoops(List<Edge> edges)
	{
		Dictionary<int, List<int>> dictionary = new Dictionary<int, List<int>>();
		foreach (Edge edge in edges)
		{
			if (!dictionary.ContainsKey(edge.A))
			{
				dictionary[edge.A] = new List<int>();
			}
			if (!dictionary.ContainsKey(edge.B))
			{
				dictionary[edge.B] = new List<int>();
			}
			dictionary[edge.A].Add(edge.B);
			dictionary[edge.B].Add(edge.A);
		}
		List<List<int>> list = new List<List<int>>();
		HashSet<int> hashSet = new HashSet<int>();
		foreach (int key in dictionary.Keys)
		{
			if (hashSet.Contains(key))
			{
				continue;
			}
			List<int> list2 = new List<int>();
			int num = key;
			int previous = -1;
			do
			{
				list2.Add(num);
				hashSet.Add(num);
				int num2 = dictionary[num].First((int n) => n != previous);
				previous = num;
				num = num2;
			}
			while (num != key && !hashSet.Contains(num));
			list.Add(list2);
		}
		return list;
	}

	private static float ComputePolygonArea(List<int> loop, List<Vector3> vertices)
	{
		float num = 0f;
		for (int i = 0; i < loop.Count; i++)
		{
			Vector3 vector = vertices[loop[i]];
			Vector3 vector2 = vertices[loop[(i + 1) % loop.Count]];
			num += vector.x * vector2.z - vector2.x * vector.z;
		}
		return 0.5f * num;
	}

	public static void Extrude(List<Vector3> vertices, List<int> triangles, List<int> outerLoop, float padding, out List<Vector3> extrudedVertices, out List<int> extrudedTriangles, out List<int> newVertices)
	{
		extrudedVertices = new List<Vector3>(vertices);
		extrudedTriangles = new List<int>(triangles);
		newVertices = new List<int>();
		int count = outerLoop.Count;
		List<int> list = new List<int>();
		for (int i = 0; i < count; i++)
		{
			int index = outerLoop[i];
			Vector3 vector = vertices[index];
			Vector3 normalized = new Vector3(Mathf.Sign(vector.x), 0f, Mathf.Sign(vector.z)).normalized;
			Vector3 item = vector + normalized * padding;
			int count2 = extrudedVertices.Count;
			newVertices.Add(count2);
			list.Add(count2);
			extrudedVertices.Add(item);
		}
		for (int j = 0; j < count; j++)
		{
			int item2 = outerLoop[j];
			int item3 = outerLoop[(j + 1) % count];
			int item4 = list[j];
			int item5 = list[(j + 1) % count];
			extrudedTriangles.Add(item2);
			extrudedTriangles.Add(item5);
			extrudedTriangles.Add(item3);
			extrudedTriangles.Add(item2);
			extrudedTriangles.Add(item4);
			extrudedTriangles.Add(item5);
		}
	}
}
