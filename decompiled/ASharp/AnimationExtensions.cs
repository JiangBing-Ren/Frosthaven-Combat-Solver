using UnityEngine;

public static class AnimationExtensions
{
	public static void ResetToStart(this Animation animation, string name)
	{
		animation.Rewind(name);
		animation.Play(name);
		animation.Sample();
		animation.Stop(name);
	}

	public static void ResetToEnd(this Animation animation, string name)
	{
		animation[name].normalizedTime = 1f;
		animation.Play(name);
		animation.Sample();
		animation.Stop(name);
	}
}
