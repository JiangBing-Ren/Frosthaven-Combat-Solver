using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace MeshSlicing;

public class Triangulator
{
	private struct CHole : IComparable<CHole>
	{
		public int HoleIndex;

		public int HoleFirstIndex;

		public int HoleLength;

		public int BridgePointIndex;

		public float2 BridgePoint;

		public int CompareTo(CHole other)
		{
			if (!(BridgePoint.x > other.BridgePoint.x))
			{
				return 1;
			}
			return -1;
		}
	}

	public SinglePolygonData Polygon;

	public Triangulator(SinglePolygonData polygon)
	{
		Polygon = polygon;
	}

	public (IList<float2> Points, int[] Triangles) Execute(int debugStopAt = -1)
	{
		LinkedList<int> linkedList = StorePolygonContourAsLinkedList();
		CHole[] holesDataSortedByMaxX = GetHolesDataSortedByMaxX();
		for (int i = 0; i < holesDataSortedByMaxX.Length; i++)
		{
			CHole cHole = holesDataSortedByMaxX[i];
			LinkedListNode<int> linkedListNode = linkedList.First;
			LinkedListNode<int> linkedListNode2 = linkedList.First;
			float2 float5 = new float2(float.MaxValue, cHole.BridgePoint.y);
			for (LinkedListNode<int> linkedListNode3 = linkedList.First; linkedListNode3 != null; linkedListNode3 = linkedListNode3.Next)
			{
				LinkedListNode<int> linkedListNode4 = ((linkedListNode3.Next != null) ? linkedListNode3.Next : linkedList.First);
				float2 a = Polygon[linkedListNode3.Value];
				float2 b = Polygon[linkedListNode4.Value];
				if (!(Math2DUtils.LineSide(cHole.BridgePoint, a, b) < 0f) && (!(a.x < cHole.BridgePoint.x) || !(b.x < cHole.BridgePoint.x)) && a.y > cHole.BridgePoint.y != b.y > cHole.BridgePoint.y)
				{
					float num = b.x;
					if (math.abs(a.x - b.x) > float.Epsilon)
					{
						float y = cHole.BridgePoint.y;
						float num2 = (a.y - b.y) / (a.x - b.x);
						float num3 = b.y - num2 * b.x;
						num = (y - num3) / num2;
					}
					if (num < float5.x)
					{
						float5.x = num;
						linkedListNode = linkedListNode3;
						linkedListNode2 = linkedListNode4;
					}
				}
			}
			LinkedListNode<int> linkedListNode5 = linkedList.First;
			if (Math2DUtils.SamePoints(float5, Polygon[linkedListNode.Value]))
			{
				linkedListNode5 = linkedListNode;
			}
			else if (Math2DUtils.SamePoints(float5, Polygon[linkedListNode2.Value]))
			{
				linkedListNode5 = linkedListNode2;
			}
			else
			{
				LinkedListNode<int> linkedListNode6 = ((Polygon[linkedListNode.Value].x > Polygon[linkedListNode2.Value].x) ? linkedListNode : linkedListNode2);
				bool flag = false;
				float num4 = float.MaxValue;
				float num5 = float.MaxValue;
				for (LinkedListNode<int> linkedListNode7 = linkedList.First; linkedListNode7 != null; linkedListNode7 = linkedListNode7.Next)
				{
					if (linkedListNode7.Value != linkedListNode6.Value)
					{
						LinkedListNode<int> linkedListNode8 = ((linkedListNode7.Next != null) ? linkedListNode7.Next : linkedList.First);
						LinkedListNode<int> linkedListNode9 = ((linkedListNode7.Previous != null) ? linkedListNode7.Previous : linkedList.Last);
						if (Math2DUtils.IsVertexReflex(Polygon[linkedListNode9.Value], Polygon[linkedListNode7.Value], Polygon[linkedListNode8.Value], isCCW: true))
						{
							bool flag2 = Math2DUtils.IsInsideTriangle(Polygon[linkedListNode7.Value], cHole.BridgePoint, float5, Polygon[linkedListNode6.Value]);
							flag |= flag2;
							if (flag2)
							{
								float2 x = Polygon[linkedListNode7.Value] - cHole.BridgePoint;
								float num6 = math.atan2(x.y, x.x);
								if (num6 < num4)
								{
									linkedListNode5 = linkedListNode7;
									num4 = num6;
								}
								else if (math.abs(num6 - num4) <= float.Epsilon)
								{
									float num7 = math.length(x);
									if (num7 < num5)
									{
										linkedListNode5 = linkedListNode7;
										num5 = num7;
									}
								}
							}
						}
						if (!flag)
						{
							linkedListNode5 = linkedListNode6;
						}
					}
				}
			}
			linkedList.AddAfter(linkedListNode5, linkedListNode5.Value);
			int num8 = cHole.BridgePointIndex - cHole.HoleFirstIndex;
			for (int j = 0; j < cHole.HoleLength; j++)
			{
				linkedList.AddAfter(linkedListNode5, num8 + cHole.HoleFirstIndex);
				num8 = (num8 + cHole.HoleLength - 1) % cHole.HoleLength;
			}
			linkedList.AddAfter(linkedListNode5, cHole.BridgePointIndex);
		}
		return Triangulate(linkedList, debugStopAt);
	}

	private LinkedList<int> StorePolygonContourAsLinkedList()
	{
		LinkedList<int> linkedList = new LinkedList<int>();
		for (int i = 0; i < Polygon.ContourPointsNum; i++)
		{
			linkedList.AddLast(i);
		}
		return linkedList;
	}

	private CHole[] GetHolesDataSortedByMaxX()
	{
		CHole[] array = new CHole[Polygon.HolesNum];
		for (int i = 0; i < Polygon.HolesNum; i++)
		{
			List<SinglePolygonData.Vertex> polygonHole = Polygon.GetPolygonHole(i);
			int num = -1;
			float num2 = float.MinValue;
			for (int j = 0; j < polygonHole.Count; j++)
			{
				if (num2 < polygonHole[j].Point.x)
				{
					num2 = polygonHole[j].Point.x;
					num = polygonHole[j].Index;
				}
			}
			array[i] = new CHole
			{
				HoleIndex = i,
				HoleFirstIndex = polygonHole[0].Index,
				HoleLength = polygonHole.Count,
				BridgePointIndex = num,
				BridgePoint = Polygon[num]
			};
		}
		Array.Sort(array);
		return array;
	}

	private (IList<float2>, int[]) Triangulate(LinkedList<int> list, int debugStopAt = -1)
	{
		int num = Polygon.HolesNum * 2 + Polygon.VerticesNum;
		if (num < 3)
		{
			return (new List<float2>(), new int[0]);
		}
		if (num > 12000)
		{
			Debug.LogError("Too many vertices in polygon");
			return (new List<float2>(), new int[0]);
		}
		int[] array = new int[(num - 2) * 3];
		int num2 = 0;
		while (list.Count > 2)
		{
			bool flag = false;
			int num3 = 0;
			LinkedListNode<int> linkedListNode = list.First;
			for (int i = 0; i < list.Count; i++)
			{
				LinkedListNode<int> linkedListNode2 = linkedListNode.Previous ?? list.Last;
				LinkedListNode<int> linkedListNode3 = linkedListNode.Next ?? list.First;
				if (Math2DUtils.IsVertexConvex(Polygon[linkedListNode2.Value], Polygon[linkedListNode.Value], Polygon[linkedListNode3.Value], isCCW: true) && !TriangleContainsVertexInList(linkedListNode2.Value, linkedListNode.Value, linkedListNode3.Value, list))
				{
					array[num2] = linkedListNode2.Value;
					array[num2 + 1] = linkedListNode.Value;
					array[num2 + 2] = linkedListNode3.Value;
					num2 += 3;
					list.Remove(linkedListNode);
					flag = true;
					if (debugStopAt <= -1 || num2 <= debugStopAt * 3)
					{
						break;
					}
					return (Polygon.VectorPoints(), array);
				}
				num3 = (num3 + 1) % list.Count;
				linkedListNode = linkedListNode.Next ?? list.First;
			}
			if (!flag)
			{
				return (Polygon.VectorPoints(), array);
			}
		}
		return (Polygon.VectorPoints(), array);
	}

	private bool TriangleContainsVertexInList(int indexPrev, int indexCurr, int indexNext, LinkedList<int> list)
	{
		for (LinkedListNode<int> linkedListNode = list.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			LinkedListNode<int> linkedListNode2 = ((linkedListNode.Previous != null) ? linkedListNode.Previous : list.Last);
			LinkedListNode<int> linkedListNode3 = ((linkedListNode.Next != null) ? linkedListNode.Next : list.First);
			if (!Math2DUtils.IsVertexConvex(Polygon[linkedListNode2.Value], Polygon[linkedListNode.Value], Polygon[linkedListNode3.Value], isCCW: true))
			{
				int value = linkedListNode.Value;
				if (value != indexPrev && value != indexCurr && value != indexNext && Math2DUtils.IsInsideTriangle(Polygon[value], Polygon[indexPrev], Polygon[indexCurr], Polygon[indexNext]))
				{
					return true;
				}
			}
		}
		return false;
	}
}
