using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
	private static readonly Queue<Action> _executionQueue = new Queue<Action>();

	private static UnityMainThreadDispatcher _instance = null;

	public void Update()
	{
		lock (_executionQueue)
		{
			while (_executionQueue.Count > 0)
			{
				_executionQueue.Dequeue()();
			}
		}
	}

	public void Enqueue(IEnumerator action)
	{
		lock (_executionQueue)
		{
			_executionQueue.Enqueue(() =>
			{
				StartCoroutine(action);
			});
		}
	}

	public void Enqueue(Action action)
	{
		Enqueue(ActionWrapper(action));
	}

	public void EnqueueGlobalErrorMessage(string errorMessageKey, string buttonLabelKey, string errorStack, ErrorMessage.ErrorDelegate action, string overrideErrorMessage = null)
	{
		Enqueue(GlobalErrorMessageWrapper(errorMessageKey, buttonLabelKey, errorStack, action, overrideErrorMessage));
	}

	public void EnqueueGlobalErrorMessage(string errorMessageKey, List<ErrorMessage.LabelAction> buttons, string overrideErrorMessage = null)
	{
		Enqueue(GlobalErrorMessageWrapper(errorMessageKey, buttons, overrideErrorMessage));
	}

	public Task EnqueueAsync(Action action)
	{
		TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
		Enqueue(ActionWrapper(WrappedAction));
		return tcs.Task;
		void WrappedAction()
		{
			try
			{
				action();
				tcs.TrySetResult(result: true);
			}
			catch (Exception exception)
			{
				tcs.TrySetException(exception);
			}
		}
	}

	private IEnumerator ActionWrapper(Action a)
	{
		a();
		yield return null;
	}

	private IEnumerator GlobalErrorMessageWrapper(string errorMessageKey, string buttonLabelKey, string errorStack, ErrorMessage.ErrorDelegate action, string overrideErrorMessage = null)
	{
		SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle(errorMessageKey, buttonLabelKey, errorStack, action, overrideErrorMessage);
		yield return null;
	}

	private IEnumerator GlobalErrorMessageWrapper(string errorMessageKey, List<ErrorMessage.LabelAction> buttons, string errorStack, string overrideErrorMessage = null)
	{
		SceneController.Instance.GlobalErrorMessage.ShowMultiChoiceMessage(errorMessageKey, buttons, overrideErrorMessage, errorStack);
		yield return null;
	}

	public static bool Exists()
	{
		return _instance != null;
	}

	public static UnityMainThreadDispatcher Instance()
	{
		if (!Exists())
		{
			throw new Exception("UnityMainThreadDispatcher could not find the UnityMainThreadDispatcher object. Please ensure you have added the MainThreadExecutor Prefab to your scene.");
		}
		return _instance;
	}

	private void Awake()
	{
		if (_instance == null)
		{
			_instance = this;
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
		}
	}

	private void OnDestroy()
	{
		_instance = null;
	}
}
