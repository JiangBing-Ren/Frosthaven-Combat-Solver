using System.Collections;

public abstract class CorrutineGUIAnimator : GUIAnimator
{
	protected override void Animate()
	{
		StartCoroutine(AnimateCorrutine());
	}

	private IEnumerator AnimateCorrutine()
	{
		yield return Animation();
		OnAnimationFinished.Invoke();
	}

	protected abstract IEnumerator Animation();

	protected override void Clear()
	{
		base.Clear();
		StopAllCoroutines();
	}
}
