using System;

namespace Assets.Script.Misc;

public interface ICallbackPromise
{
	void Done(Action callback, Action onCancelled = null);

	void Cancelled(Action callback);

	ICallbackPromise Then(Action onDone, Action onCancel = null);

	ICallbackPromise Then(Func<ICallbackPromise> action);

	ICallbackPromise<T> Then<T>(Func<ICallbackPromise<T>> action);
}
public interface ICallbackPromise<T>
{
	void Done(Action<T> callback, Action onCancelled = null);

	void Cancelled(Action callback);

	ICallbackPromise<T> Then(Action<T> onDone, Action onCancel = null);

	ICallbackPromise<T> Then(Func<T, ICallbackPromise<T>> action);

	ICallbackPromise Then(Func<T, ICallbackPromise> action);
}
