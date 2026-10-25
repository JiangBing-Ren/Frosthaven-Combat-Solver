using System;
using System.Collections.Generic;

namespace AStar;

public class MinHeap<T>
{
	private const int InitialCapacity = 128;

	private T[] _arr;

	private int _lastItemIndex;

	private readonly IComparer<T> _comparer;

	public int Count => _lastItemIndex + 1;

	public MinHeap()
		: this(128, (IComparer<T>)Comparer<T>.Default)
	{
	}

	public MinHeap(int capacity)
		: this(capacity, (IComparer<T>)Comparer<T>.Default)
	{
	}

	public MinHeap(Comparison<T> comparison)
		: this(128, (IComparer<T>)Comparer<T>.Create(comparison))
	{
	}

	public MinHeap(IComparer<T> comparer)
		: this(128, comparer)
	{
	}

	public MinHeap(int capacity, IComparer<T> comparer)
	{
		_arr = new T[capacity];
		_lastItemIndex = -1;
		_comparer = comparer;
	}

	public void Add(T item)
	{
		if (_lastItemIndex == _arr.Length - 1)
		{
			Resize();
		}
		_lastItemIndex++;
		_arr[_lastItemIndex] = item;
		MinHeapifyUp(_lastItemIndex);
	}

	public T Remove()
	{
		if (_lastItemIndex == -1)
		{
			throw new InvalidOperationException("The heap is empty");
		}
		T result = _arr[0];
		_arr[0] = _arr[_lastItemIndex];
		_lastItemIndex--;
		MinHeapifyDown(0);
		return result;
	}

	public T Peek()
	{
		if (_lastItemIndex == -1)
		{
			throw new InvalidOperationException("The heap is empty");
		}
		return _arr[0];
	}

	public void Clear()
	{
		_lastItemIndex = -1;
	}

	private void MinHeapifyUp(int index)
	{
		if (index != 0)
		{
			int num = (index - 1) / 2;
			if (_comparer.Compare(_arr[index], _arr[num]) < 0)
			{
				T val = _arr[index];
				_arr[index] = _arr[num];
				_arr[num] = val;
				MinHeapifyUp(num);
			}
		}
	}

	private void MinHeapifyDown(int index)
	{
		int num = index * 2 + 1;
		int num2 = index * 2 + 2;
		int num3 = index;
		if (num <= _lastItemIndex && _comparer.Compare(_arr[num], _arr[num3]) < 0)
		{
			num3 = num;
		}
		if (num2 <= _lastItemIndex && _comparer.Compare(_arr[num2], _arr[num3]) < 0)
		{
			num3 = num2;
		}
		if (num3 != index)
		{
			T val = _arr[index];
			_arr[index] = _arr[num3];
			_arr[num3] = val;
			MinHeapifyDown(num3);
		}
	}

	private void Resize()
	{
		T[] array = new T[_arr.Length * 2];
		for (int i = 0; i < _arr.Length; i++)
		{
			array[i] = _arr[i];
		}
		_arr = array;
	}
}
