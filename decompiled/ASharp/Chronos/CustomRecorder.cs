using System;
using System.Collections.Generic;
using Chronos.Reflection;
using UnityEngine;

namespace Chronos;

public class CustomRecorder : Recorder<CustomRecorder.Snapshot>
{
	public class Snapshot
	{
		public object[] values { get; private set; }

		public Snapshot(int size)
		{
			values = new object[size];
		}
	}

	[SerializeField]
	[SelfTargeted]
	[Filter(new Type[] { }, TypeFamilies = TypeFamily.Value, Inherited = true, ReadOnly = false)]
	private List<UnityVariable> variables;

	public CustomRecorder()
	{
		variables = new List<UnityVariable>();
	}

	protected override void ApplySnapshot(Snapshot snapshot)
	{
		int num = 0;
		foreach (UnityVariable variable in variables)
		{
			variable.Set(snapshot.values[num++]);
		}
	}

	protected override Snapshot CopySnapshot()
	{
		Snapshot snapshot = new Snapshot(variables.Count);
		int num = 0;
		foreach (UnityVariable variable in variables)
		{
			snapshot.values[num++] = variable.Get();
		}
		return snapshot;
	}

	protected override Snapshot LerpSnapshots(Snapshot from, Snapshot to, float t)
	{
		Snapshot snapshot = new Snapshot(from.values.Length);
		for (int i = 0; i < snapshot.values.Length; i++)
		{
			snapshot.values[i] = LerpValue(from.values[i], to.values[i], t);
		}
		return snapshot;
	}

	private static object LerpValue(object from, object to, float t)
	{
		Type type = from.GetType();
		if (type == typeof(float) || type == typeof(double))
		{
			return Mathf.Lerp((float)from, (float)to, t);
		}
		if (type == typeof(Vector3))
		{
			return Vector3.Lerp((Vector3)from, (Vector3)to, t);
		}
		if (type == typeof(Vector2))
		{
			return Vector2.Lerp((Vector2)from, (Vector2)to, t);
		}
		if (type == typeof(Quaternion))
		{
			return Quaternion.Lerp((Quaternion)from, (Quaternion)to, t);
		}
		if (type == typeof(Color))
		{
			return Color.Lerp((Color)from, (Color)to, t);
		}
		return from;
	}
}
