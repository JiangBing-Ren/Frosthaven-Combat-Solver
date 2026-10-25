using System;
using System.Collections;

namespace ScenarioRuleLibrary;

public struct MultiDimentionalArrayEnumerator : IEnumerator
{
	public Array array;

	public int rank;

	public int[] lengths;

	public int[] indices;

	public object Current => array.GetValue(indices);

	public MultiDimentionalArrayEnumerator(Array array)
	{
		this.array = array;
		rank = array.Rank;
		lengths = new int[rank];
		for (int i = 0; i < rank; i++)
		{
			lengths[i] = array.GetLength(i);
			if (lengths[i] == 0)
			{
				rank = 0;
				break;
			}
		}
		indices = new int[rank];
		Reset();
	}

	public bool MoveNext()
	{
		int num = 0;
		while (true)
		{
			if (num >= rank)
			{
				rank = 0;
				return false;
			}
			indices[num]++;
			if (indices[num] < lengths[num])
			{
				break;
			}
			indices[num] = 0;
			num++;
		}
		return true;
	}

	public string IndexString()
	{
		string text = "";
		for (int i = 0; i < rank; i++)
		{
			if (i > 0)
			{
				text += ", ";
			}
			text += indices[i];
		}
		return text;
	}

	public void Reset()
	{
		if (rank != 0)
		{
			indices[0] = -1;
			for (int i = 1; i < rank; i++)
			{
				indices[i] = 0;
			}
		}
	}
}
