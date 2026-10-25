using System;
using System.Collections.Generic;
using System.Linq;
using FH.LevelComponents.ProcGen;
using SETUtil;
using UnityEngine;

namespace FH.ApparanceBaking;

public class BakedWallBounds : MonoBehaviour
{
	public class AnalysisAssist : IDisposable
	{
		private ICollection<(BakedWallBounds bake, ProceduralWall wall)> group;

		public AnalysisAssist(ICollection<ProceduralWall> walls)
		{
			group = walls.Select((ProceduralWall a) => (a.GetComponent<BakedWallBounds>(), a: a)).ToList();
			foreach (var item in group)
			{
				if (item.bake != null)
				{
					item.bake.LoadAnalStart();
				}
				else if (!Application.isPlaying)
				{
					item.wall.gameObject.AddComponent<BakedWallBounds>().Save();
				}
			}
		}

		public void Dispose()
		{
			foreach (var item in group)
			{
				if (item.bake != null)
				{
					item.bake.LoadAnalEnd();
				}
			}
		}
	}

	[SerializeField]
	private Bounds bounds;

	[SerializeField]
	private Vector3 position;

	[NonSerialized]
	private Vector3 anStartPosition;

	public void Save()
	{
		BoxCollider component = GetComponent<BoxCollider>();
		bounds = new Bounds(component.center, component.size);
		position = transform.position;
		DrawDebug(Color.cyan);
	}

	private void LoadAnalStart()
	{
		BoxCollider component = GetComponent<BoxCollider>();
		anStartPosition = transform.position;
		transform.position = position;
		component.center = bounds.center;
		component.size = bounds.size;
		DrawDebug(Color.green);
	}

	private void LoadAnalEnd()
	{
		transform.position = anStartPosition;
	}

	[ContextMenu("Debug Show Saved Bounds")]
	private void DebugShowSavedBounds()
	{
		DrawDebug(Color.cyan);
	}

	private void DrawDebug(Color c)
	{
		Vector3 vector = transform.position;
		transform.position = position;
		Matrix4x4 localToWorldMatrix = transform.localToWorldMatrix;
		transform.position = vector;
		EditorUtil.DrawDebugBounds(bounds.center, bounds.size, c, localToWorldMatrix, 10f);
	}
}
