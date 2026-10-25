using System.Collections;
using UnityEngine;

namespace FH.Misc;

public class SmartCoroutine
{
	private MonoBehaviour _owner;

	private Coroutine _coroutine;

	public bool IsCompleted => _coroutine == null;

	private SmartCoroutine(MonoBehaviour owner, IEnumerator code)
	{
		_owner = owner;
		_coroutine = owner.StartCoroutine(code);
	}

	public static SmartCoroutine Start(MonoBehaviour owner, IEnumerator code)
	{
		return new SmartCoroutine(owner, code);
	}

	public void Stop()
	{
		if (_coroutine != null)
		{
			_owner.StopCoroutine(_coroutine);
		}
	}

	public IEnumerator WaitForCompletion()
	{
		yield return _coroutine;
	}
}
