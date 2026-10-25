using UnityEngine;
using UnityEngine.Events;

public abstract class GUIAnimator : MonoBehaviour
{
	public UnityEvent OnAnimationStarted = new UnityEvent();

	public UnityEvent OnAnimationStopped = new UnityEvent();

	public UnityEvent OnAnimationFinished = new UnityEvent();

	[SerializeField]
	protected bool enableDebug;

	private bool isPlaying;

	public bool IsPlaying => isPlaying;

	protected abstract void Animate();

	protected abstract void ResetFinishState();

	protected abstract void ResetInitialState();

	protected void OnDestroy()
	{
		OnAnimationStarted.RemoveAllListeners();
		OnAnimationStopped.RemoveAllListeners();
		OnAnimationFinished.RemoveAllListeners();
	}

	public virtual void Stop()
	{
		Stop(goToEnd: false);
	}

	public virtual void Stop(bool goToEnd)
	{
		if (isPlaying)
		{
			if (enableDebug)
			{
				Log.GUIInfo("[Animation] Stop anims " + gameObject.name);
			}
			Clear();
			if (goToEnd)
			{
				GoToFinishState();
			}
			OnAnimationStopped.Invoke();
		}
	}

	public virtual void StopAndReset()
	{
		if (isPlaying)
		{
			if (enableDebug)
			{
				Log.GUIInfo("[Animation] StopAndReset anims " + gameObject.name);
			}
			Clear();
			GoInitState();
			OnAnimationStopped.Invoke();
		}
	}

	public virtual void GoInitState()
	{
		Stop();
		ResetInitialState();
	}

	public virtual void Play()
	{
		Play(fromStart: true);
	}

	public virtual void Play(bool fromStart)
	{
		if (!isPlaying)
		{
			if (enableDebug)
			{
				Log.GUIInfo("[Animation] Play anims " + gameObject.name);
			}
			isPlaying = true;
			OnAnimationStarted.Invoke();
			if (fromStart)
			{
				ResetInitialState();
			}
			Animate();
		}
	}

	protected virtual void Clear()
	{
		isPlaying = false;
		if (enableDebug)
		{
			Log.GUIInfo("[Animation] Clear anim " + gameObject.name);
		}
	}

	public virtual void GoToFinishState()
	{
		Stop();
		ResetFinishState();
	}

	protected virtual void FinishAnimation()
	{
		if (enableDebug)
		{
			Log.GUIInfo($"[Animation] Finish anims {gameObject.name} isPlaying {isPlaying}");
		}
		if (isPlaying)
		{
			Clear();
			OnAnimationFinished.Invoke();
		}
	}
}
