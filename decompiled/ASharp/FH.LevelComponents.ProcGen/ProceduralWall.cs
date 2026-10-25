using System;
using System.Collections.Generic;
using System.Linq;
using Apparance.Net;
using UnityEngine;

namespace FH.LevelComponents.ProcGen;

[RequireComponent(typeof(ApparanceEntity))]
[RequireComponent(typeof(ProceduralStyle))]
public class ProceduralWall : ProceduralTileObserver
{
	public struct CornerInfo
	{
		public bool IsOwner;

		public UnityEngine.Vector3 Position;

		public float InsideAngle;

		public float AdjacentDepth;

		public int Flags;
	}

	[NonSerialized]
	public bool IsOwner = true;

	private static HashSet<ProceduralWall> m_WallCache = new HashSet<ProceduralWall>();

	private Bounds _cachedBounds;

	public CornerInfo LeftCorner { get; set; }

	public CornerInfo RightCorner { get; set; }

	public float Length { get; set; }

	public float perimeterOffset { get; set; }

	public float perimiterLength { get; set; }

	internal UnityEngine.Vector3 cachedSize => _cachedBounds.size;

	public override void WriteExtraParameters(ParameterCollection parameters, int next_id)
	{
		parameters.WriteBool(IsOwner, next_id++);
		ParameterCollection parameterCollection = parameters.WriteListBegin(next_id++);
		parameterCollection.WriteBool(LeftCorner.IsOwner);
		parameterCollection.WriteVector3(Conversion.AVfromUV(LeftCorner.Position));
		parameterCollection.WriteFloat(LeftCorner.InsideAngle);
		parameterCollection.WriteFloat(LeftCorner.AdjacentDepth);
		parameterCollection.WriteInteger(LeftCorner.Flags);
		parameters.WriteListEnd();
		ParameterCollection parameterCollection2 = parameters.WriteListBegin(next_id++);
		parameterCollection2.WriteBool(RightCorner.IsOwner);
		parameterCollection2.WriteVector3(Conversion.AVfromUV(RightCorner.Position));
		parameterCollection2.WriteFloat(RightCorner.InsideAngle);
		parameterCollection2.WriteFloat(RightCorner.AdjacentDepth);
		parameterCollection2.WriteInteger(RightCorner.Flags);
		parameters.WriteListEnd();
		ProceduralStyle proceduralStyle = GloomUtility.EnsureComponent<ProceduralStyle>(gameObject);
		ParameterCollection style_struct = parameters.WriteListBegin(next_id++);
		proceduralStyle.WriteStyleParameters(style_struct, GetScenarioSeed(), GetRoomSeed(), proceduralStyle.Seed);
		parameters.WriteListEnd();
		ParameterCollection parameterCollection3 = parameters.WriteListBegin(next_id++);
		parameterCollection3.WriteFloat(Length);
		parameterCollection3.WriteFloat(perimeterOffset);
		parameterCollection3.WriteFloat(perimiterLength);
		parameters.WriteListEnd();
	}

	private ProceduralWall()
	{
		EnsureCacheValidity(checkForNulls: false, useFindObjects: false);
		m_WallCache.Add(this);
	}

	public static void ResetWallCache()
	{
		m_WallCache = null;
	}

	protected override void OnDestroy()
	{
		m_WallCache?.Remove(this);
		base.OnDestroy();
	}

	private static void EnsureCacheValidity(bool checkForNulls, bool useFindObjects)
	{
		if (m_WallCache == null || (checkForNulls && m_WallCache.Any((ProceduralWall w) => w == null)))
		{
			m_WallCache = (useFindObjects ? UnityEngine.Object.FindObjectsOfType<ProceduralWall>().ToHashSet() : new HashSet<ProceduralWall>());
		}
	}

	public static IEnumerable<ProceduralWall> FindWallsNear(UnityEngine.Vector3 position, float tolerance = 1f)
	{
		EnsureCacheValidity(checkForNulls: true, useFindObjects: true);
		IEnumerable<ProceduralWall> wallCache = m_WallCache;
		foreach (ProceduralWall item in wallCache)
		{
			Bounds outBounds = default;
			item.CollectBounds(ref outBounds);
			outBounds.Expand(tolerance);
			position.y = outBounds.center.y;
			if (outBounds.Contains(position))
			{
				yield return item;
			}
		}
	}

	public void CollectBounds(ref Bounds outBounds)
	{
		if (_cachedBounds == default(Bounds))
		{
			_cachedBounds = default;
			RecursiveBoundsSearch(transform, ref _cachedBounds);
		}
		outBounds = _cachedBounds;
		static void RecursiveBoundsSearch(Transform obj, ref Bounds b)
		{
			if (obj != null)
			{
				BoxCollider component = obj.GetComponent<BoxCollider>();
				if (component != null)
				{
					if (b.size.x < 0.001f && b.size.y < 0.001f && b.size.z < 0.001f)
					{
						b = component.bounds;
					}
					else
					{
						b.Encapsulate(component.bounds);
					}
				}
				else
				{
					for (int i = 0; i < obj.childCount; i++)
					{
						RecursiveBoundsSearch(obj.GetChild(i), ref b);
					}
				}
			}
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(LeftCorner.Position, 0.1f);
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(RightCorner.Position, 0.1f);
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(LeftCorner.Position, RightCorner.Position);
	}
}
