using System;
using System.Collections.Generic;
using FH.LevelComponents.ProcGen;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct TallCharacterOcclusionCheckJob : IOcclusionCheckJob<bool>, IJobFor, IDisposable
{
	[ReadOnly]
	public int StartIndex;

	[ReadOnly]
	public Vector2 CameraPos;

	[ReadOnly]
	public NativeArray<Vector2> ShortCharacters;

	[ReadOnly]
	public NativeArray<Vector2> TallCharactersFront;

	[ReadOnly]
	public NativeArray<Vector2> TallCharactersBack;

	[ReadOnly]
	public NativeArray<Vector2> TallCharactersLeft;

	[ReadOnly]
	public NativeArray<Vector2> TallCharactersRight;

	public NativeArray<bool> Results;

	public bool[] GetResults()
	{
		bool[] array = new bool[Results.Length];
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

	public TallCharacterOcclusionCheckJob(int startIndex, int count, Vector2 cameraPosition, List<Vector2> shortCharacters, List<ICharacterOccluder> occluders)
	{
		StartIndex = startIndex;
		ShortCharacters = new NativeArray<Vector2>(shortCharacters.Count, Allocator.TempJob);
		TallCharactersFront = new NativeArray<Vector2>(count, Allocator.TempJob);
		TallCharactersBack = new NativeArray<Vector2>(count, Allocator.TempJob);
		TallCharactersLeft = new NativeArray<Vector2>(count, Allocator.TempJob);
		TallCharactersRight = new NativeArray<Vector2>(count, Allocator.TempJob);
		Results = new NativeArray<bool>(count, Allocator.TempJob);
		CameraPos = cameraPosition;
		for (int i = 0; i < shortCharacters.Count; i++)
		{
			ShortCharacters[i] = shortCharacters[i];
		}
		for (int j = 0; j < count; j++)
		{
			int index = (startIndex + j) % occluders.Count;
			TallCharactersFront[j] = occluders[index].Front;
			TallCharactersBack[j] = occluders[index].Back;
			TallCharactersLeft[j] = occluders[index].Left;
			TallCharactersRight[j] = occluders[index].Right;
		}
	}

	public void Execute(int index)
	{
		bool value = false;
		for (int i = 0; i < ShortCharacters.Length; i++)
		{
			if (IntersectionDetection.CheckIntersection(ShortCharacters[i], CameraPos, TallCharactersFront[index], TallCharactersBack[index]))
			{
				value = true;
				break;
			}
			if (IntersectionDetection.CheckIntersection(ShortCharacters[i], CameraPos, TallCharactersLeft[index], TallCharactersRight[index]))
			{
				value = true;
				break;
			}
		}
		Results[index] = value;
	}

	public void Dispose()
	{
		ShortCharacters.Dispose();
		TallCharactersFront.Dispose();
		TallCharactersBack.Dispose();
		TallCharactersLeft.Dispose();
		TallCharactersRight.Dispose();
		Results.Dispose();
	}
}
