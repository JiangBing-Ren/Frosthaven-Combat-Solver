using System;
using UnityEngine;

public class SmoothFloat
{
	private float _value;

	private float _target;

	public float Value
	{
		get
		{
			return _value;
		}
		set
		{
			_target = value;
		}
	}

	public float Target => _target;

	public void Update(float t)
	{
		_value = Mathf.Lerp(_value, _target, t);
	}

	public void ForceSet(float value)
	{
		_value = value;
		_target = value;
	}

	public void SnapToTarget()
	{
		_value = _target;
	}

	public bool IsCloseEnough(float approx)
	{
		return Mathf.Abs(Math.Abs(_target) - Mathf.Abs(_value)) < approx + 1E-05f;
	}
}
