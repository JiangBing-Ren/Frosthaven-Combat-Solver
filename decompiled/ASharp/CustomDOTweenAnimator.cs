using DG.Tweening;
using UnityEngine;

public abstract class CustomDOTweenAnimator : MonoBehaviour
{
	public abstract void SetFinalValue();

	public abstract void RestorOriginalValue();

	public abstract Tween BuildTweenAction(float duration);
}
