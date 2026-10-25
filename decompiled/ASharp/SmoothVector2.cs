using UnityEngine;

internal class SmoothVector2
{
	private Vector2 _value;

	private Vector2 _target;

	public Vector2 Value
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

	public void Update(float t)
	{
		_value = Vector2.Lerp(_value, _target, t);
	}

	public void ForceSet(Vector2 value)
	{
		_value = value;
		_target = value;
	}
}
