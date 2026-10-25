using UnityEngine;

namespace Chronos;

public class AnimationTimeline : ComponentTimeline<Animation>
{
	private float _speed;

	public float speed
	{
		get
		{
			return _speed;
		}
		set
		{
			_speed = value;
			AdjustProperties();
		}
	}

	public AnimationTimeline(Timeline timeline, Animation component)
		: base(timeline, component)
	{
	}

	public override void CopyProperties(Animation source)
	{
		float num = 1f;
		bool flag = false;
		foreach (AnimationState item in source)
		{
			if (flag && num != item.speed)
			{
				Debug.LogWarning("Different animation speeds per state are not supported.");
			}
			num = item.speed;
			flag = true;
		}
		_speed = num;
	}

	public override void AdjustProperties(float timeScale)
	{
		foreach (AnimationState item in component)
		{
			item.speed = speed * timeScale;
		}
	}
}
