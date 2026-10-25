using System;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

namespace FH.LevelComponents.ProcGen;

public abstract class OcclusionDetection<TOccluder, TJob, TResult> : MonoBehaviour where TOccluder : IOccluder<TResult> where TJob : struct, IOcclusionCheckJob<TResult> where TResult : struct
{
	protected List<TOccluder> _occluders;

	protected TResult[] _lastResults;

	private int _nextCheckIndex;

	private readonly List<(JobHandle handle, TJob job)> _runningJobs = new List<(JobHandle, TJob)>();

	protected virtual int MaxChecksPerFrame => 10;

	private void OnEnable()
	{
		if (_occluders == null || _occluders.Count == 0 || !Application.isPlaying)
		{
			enabled = false;
		}
	}

	protected void Tick()
	{
		if (!enabled || _lastResults == null || _lastResults.Length == 0)
		{
			enabled = false;
			return;
		}
		int startIndex = _nextCheckIndex % _occluders.Count;
		_nextCheckIndex = ScheduleOcclusionCheck(startIndex, MaxChecksPerFrame);
		(JobHandle, TJob) job;
		while (FindCompletedJob(out job))
		{
			job.Item1.Complete();
			CollectResults(job.Item2, out var results);
			DisposeJob(job.Item2);
			for (int i = 0; i < results.Length; i++)
			{
				_lastResults[(job.Item2.GetStartIndex() + i) % _occluders.Count] = results[i];
			}
		}
		bool FindCompletedJob(out (JobHandle handle, TJob job) reference)
		{
			for (int j = 0; j < _runningJobs.Count; j++)
			{
				(JobHandle, TJob) tuple = _runningJobs[j];
				if (tuple.Item1.IsCompleted)
				{
					reference = tuple;
					_runningJobs.RemoveAt(j);
					return true;
				}
			}
			reference = (handle: default, job: default);
			return false;
		}
	}

	public void Setup()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		_occluders = FetchOccluders();
		foreach (TOccluder occluder2 in _occluders)
		{
			TOccluder occluder = occluder2;
			ref TOccluder reference = ref occluder;
			Action onDestroyCallback = () =>
			{
				RemoveOccluder(occluder);
			};
			reference.SetOnDestroyCallback(onDestroyCallback);
		}
		InitializeReferences();
		if (_lastResults == null)
		{
			_lastResults = new TResult[_occluders.Count];
		}
		enabled = true;
	}

	protected abstract List<TOccluder> FetchOccluders();

	protected abstract void InitializeReferences();

	private void RemoveOccluder(TOccluder occluder)
	{
		_occluders.Remove(occluder);
	}

	protected abstract TJob CreateJob(int startIndex, int count);

	protected abstract void DisposeJob(TJob job);

	private int ScheduleOcclusionCheck(int startIndex, int count)
	{
		TJob jobData = CreateJob(startIndex, count);
		JobHandle item = IJobForExtensions.ScheduleParallelByRef(ref jobData, count, 1, default);
		_runningJobs.Add((item, jobData));
		return (startIndex + (count - 1)) % _occluders.Count;
	}

	private void CollectResults(TJob job, out TResult[] results)
	{
		results = job.GetResults();
		int startIndex = job.GetStartIndex();
		for (int i = 0; i < results.Length; i++)
		{
			int index = (startIndex + i) % _occluders.Count;
			_occluders[index].OnUpdatedOcclusionCheck(results[i]);
		}
	}

	private void OnDestroy()
	{
		for (int i = 0; i < _runningJobs.Count; i++)
		{
			_runningJobs[i].handle.Complete();
			DisposeJob(_runningJobs[i].job);
		}
		_runningJobs.Clear();
		_occluders.Clear();
	}

	protected static Vector2 ToVector2(Vector3 vector)
	{
		return new Vector2(vector.x, vector.z);
	}

	protected static Vector3 ToVector3(Vector2 vector)
	{
		return new Vector3(vector.x, 0f, vector.y);
	}
}
