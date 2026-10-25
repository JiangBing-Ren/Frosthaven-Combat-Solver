using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace FH.Misc;

public class BudgetCoroutineRunner
{
	public float BudgetSeconds = 0.015f;

	private Stopwatch watch = new Stopwatch();

	private IEnumerator _initialCrt;

	private Stack<IEnumerator> _crtStack = new Stack<IEnumerator>();

	private object _crtLastResult;

	private Stopwatch _crtTime = new Stopwatch();

	public readonly string Name;

	private int _iterations;

	public TimeSpan ElapsedTime => _crtTime.Elapsed;

	public BudgetCoroutineRunner(string name, IEnumerator crt)
	{
		_initialCrt = crt;
		_crtStack.Push(_initialCrt);
		Name = name;
	}

	public IEnumerator RunCoroutine()
	{
		_iterations = 0;
		_crtTime.Restart();
		_iterations++;
		while (Iterate())
		{
			yield return _crtLastResult;
			_iterations++;
		}
		_crtTime.Stop();
		int num = (int)(_crtTime.Elapsed.TotalMilliseconds / (double)_iterations);
		Log.Info($"Coroutine '{Name}' completed after {_crtTime.Elapsed.TotalSeconds:F2} seconds with {_iterations} iterations ({num} ms average).");
	}

	private bool Iterate()
	{
		_crtLastResult = null;
		watch.Restart();
		do
		{
			if (!_crtStack.TryPeek(out var result))
			{
				return false;
			}
			if (!result.MoveNext())
			{
				_crtStack.Pop();
				continue;
			}
			object current = result.Current;
			if (current is IEnumerator item)
			{
				_crtStack.Push(item);
			}
			else if (!(current is WaitForEndOfFrame) && !(current is WaitForFixedUpdate) && current != null)
			{
				_crtLastResult = current;
				break;
			}
		}
		while (watch.Elapsed.TotalSeconds < (double)BudgetSeconds);
		return true;
	}
}
