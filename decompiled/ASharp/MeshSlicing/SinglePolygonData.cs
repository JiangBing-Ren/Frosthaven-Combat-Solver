using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace MeshSlicing;

public struct SinglePolygonData : IDisposable
{
	private struct Hole
	{
		public int Length;

		public int StartIndex;
	}

	public struct Vertex
	{
		public int Index;

		public float2 Point;
	}

	private List<Vertex> vertices = new List<Vertex>();

	private List<Hole> holes = new List<Hole>();

	public float2 this[int i] => vertices[i].Point;

	public int VerticesNum => vertices.Count;

	public int ContourPointsNum { get; private set; } = 0;

	public int HolesNum => holes.Count;

	public Vertex GetVertex(int i)
	{
		return vertices[i];
	}

	public List<float2> VectorPoints()
	{
		return vertices.Select((Vertex v) => v.Point).ToList();
	}

	public SinglePolygonData(IList<Vector2> contour, Vector2[][] holes = null)
	{
		int i = 0;
		int count = vertices.Count;
		for (; i < contour.Count; i++)
		{
			vertices.Add(new Vertex
			{
				Index = count++,
				Point = contour[ContourPointsNum++]
			});
		}
		if (holes != null)
		{
			foreach (Vector2[] hole in holes)
			{
				AddHole(hole);
			}
		}
	}

	public void AddHole(NativeArray<float2> hole)
	{
		holes.Add(new Hole
		{
			Length = hole.Length,
			StartIndex = vertices.Count
		});
		int num = 0;
		int num2 = vertices.Count;
		while (num < hole.Length)
		{
			vertices.Add(new Vertex
			{
				Index = num2,
				Point = hole[num]
			});
			num++;
			num2++;
		}
	}

	public void AddHole(Vector2[] hole)
	{
		holes.Add(new Hole
		{
			Length = hole.Length,
			StartIndex = vertices.Count
		});
		int num = 0;
		int num2 = vertices.Count;
		while (num < hole.Length)
		{
			vertices.Add(new Vertex
			{
				Index = num2,
				Point = hole[num]
			});
			num++;
			num2++;
		}
	}

	public List<Vertex> GetPolygonHole(int holeIndex)
	{
		if (holeIndex < 0 || holeIndex >= holes.Count)
		{
			return new List<Vertex>();
		}
		return vertices.GetRange(holes[holeIndex].StartIndex, holes[holeIndex].Length);
	}

	public void ClearPolygons()
	{
		vertices.Clear();
		holes.Clear();
	}

	public void Dispose()
	{
		vertices.Clear();
		holes.Clear();
	}

	public float Area()
	{
		float num = 0f;
		for (int i = 0; i < ContourPointsNum; i++)
		{
			int i2 = (i + 1) % ContourPointsNum;
			num += this[i].x * this[i2].y;
			num -= this[i].y * this[i2].x;
		}
		for (int j = 0; j < holes.Count; j++)
		{
			int startIndex = holes[j].StartIndex;
			int length = holes[j].Length;
			for (int k = 0; k < length; k++)
			{
				int i3 = k + startIndex;
				int i4 = (k + 1) % length + startIndex;
				num -= this[i3].x * this[i4].y;
				num += this[i3].y * this[i4].x;
			}
		}
		return num;
	}
}
