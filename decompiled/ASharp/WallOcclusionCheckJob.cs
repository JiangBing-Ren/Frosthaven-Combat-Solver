using System;
using System.Collections.Generic;
using FH.LevelComponents.ProcGen;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct WallOcclusionCheckJob : IOcclusionCheckJob<WallOcclusionCheckResult>, IJobFor, IDisposable
{
	[ReadOnly]
	private int StartIndex;

	[ReadOnly]
	private Vector2 CameraPos;

	[ReadOnly]
	private Vector2 FocusPoint;

	[ReadOnly]
	private NativeArray<Vector2> PointsOfInterest;

	[ReadOnly]
	private NativeArray<Vector2> OccluderForward;

	[ReadOnly]
	private NativeArray<Vector2> WallLeftCorner;

	[ReadOnly]
	private NativeArray<Vector2> WallRightCorner;

	public NativeArray<WallOcclusionCheckResult> Results;

	public const float FocusPointInterestSqrRadius = 100f;

	public const float WallThickness = 0.6f;

	public WallOcclusionCheckResult[] GetResults()
	{
		WallOcclusionCheckResult[] array = new WallOcclusionCheckResult[Results.Length];
		for (int i = 0; i < Results.Length; i++)
		{
			array[i] = Results[i];
		}
		return array;
	}

	public int GetStartIndex()
	{
		return StartIndex;
	}

	public WallOcclusionCheckJob(int startIndex, int count, Vector2 cameraPosition, Vector2 focusPoint, List<Vector2> pointsOfInterest, List<IWallOccluder> occluders)
	{
		StartIndex = startIndex;
		CameraPos = cameraPosition;
		FocusPoint = focusPoint;
		PointsOfInterest = new NativeArray<Vector2>(pointsOfInterest.Count, Allocator.TempJob);
		OccluderForward = new NativeArray<Vector2>(count, Allocator.TempJob);
		WallLeftCorner = new NativeArray<Vector2>(count, Allocator.TempJob);
		WallRightCorner = new NativeArray<Vector2>(count, Allocator.TempJob);
		Results = new NativeArray<WallOcclusionCheckResult>(count, Allocator.TempJob);
		for (int i = 0; i < pointsOfInterest.Count; i++)
		{
			PointsOfInterest[i] = pointsOfInterest[i];
		}
		for (int j = 0; j < count; j++)
		{
			int index = (startIndex + j) % occluders.Count;
			OccluderForward[j] = occluders[index].Forward;
			WallLeftCorner[j] = occluders[index].LeftCorner;
			WallRightCorner[j] = occluders[index].RightCorner;
		}
	}

	public void Execute(int index)
	{
		WallOcclusionCheckResult value = WallOcclusionCheckResult.NonIntersecting;
		for (int i = 0; i < PointsOfInterest.Length; i++)
		{
			if (Vector2.SqrMagnitude(PointsOfInterest[i] - FocusPoint) > 100f)
			{
				value = WallOcclusionCheckResult.TooFarAway;
				continue;
			}
			if (IntersectionDetection.CheckIntersection(PointsOfInterest[i], CameraPos, WallLeftCorner[index], WallRightCorner[index]))
			{
				value = WallOcclusionCheckResult.HitCenter;
				break;
			}
			Vector2 wallEdgeA = WallLeftCorner[index] + OccluderForward[index] * 0.6f;
			Vector2 wallEdgeB = WallLeftCorner[index] - OccluderForward[index] * 0.6f;
			if (IntersectionDetection.CheckIntersection(PointsOfInterest[i], CameraPos, wallEdgeA, wallEdgeB))
			{
				value = WallOcclusionCheckResult.HitLeftEdge;
				break;
			}
			Vector2 wallEdgeA2 = WallRightCorner[index] + OccluderForward[index] * 0.6f;
			Vector2 wallEdgeB2 = WallRightCorner[index] - OccluderForward[index] * 0.6f;
			if (IntersectionDetection.CheckIntersection(PointsOfInterest[i], CameraPos, wallEdgeA2, wallEdgeB2))
			{
				value = WallOcclusionCheckResult.HitRightEdge;
				break;
			}
		}
		Results[index] = value;
	}

	public void Dispose()
	{
		PointsOfInterest.Dispose();
		OccluderForward.Dispose();
		WallLeftCorner.Dispose();
		WallRightCorner.Dispose();
		Results.Dispose();
	}
}
