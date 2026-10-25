using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.Events;

public class SelectorWrapper<T>
{
	protected TMP_Dropdown dropdown;

	public UnityEvent<T> OnValuedChanged;

	public SelectorWrapper(TMP_Dropdown dropdown, List<SelectorOptData<T>> values)
	{
		this.dropdown = dropdown;
		OnValuedChanged = new UnityEvent<T>();
		SetOptions(values);
		dropdown.onValueChanged.AddListener(OnValueChanged);
	}

	private void OnValueChanged(int index)
	{
		T key = ((SelectorOptData<T>)dropdown.options[index]).key;
		OnValuedChanged.Invoke(key);
	}

	public void SetValueWithoutNotify(T key)
	{
		dropdown.SetValueWithoutNotify(GetIndexOf(key));
	}

	public void ResetSelection()
	{
		dropdown.SetValueWithoutNotify(-1);
	}

	public void AddOption(SelectorOptData<T> option)
	{
		if (GetIndexOf(option.key) <= 0)
		{
			dropdown.options.Add(option);
			if (dropdown.IsExpanded)
			{
				dropdown.Hide();
				dropdown.Show();
			}
		}
	}

	public void SetOptions(List<SelectorOptData<T>> values)
	{
		dropdown.ClearOptions();
		if (values != null)
		{
			dropdown.AddOptions(values.Cast<TMP_Dropdown.OptionData>().ToList());
		}
		if (dropdown.IsExpanded)
		{
			dropdown.Hide();
			dropdown.Show();
		}
	}

	private int GetIndexOf(T key)
	{
		return dropdown.options.FindIndex((TMP_Dropdown.OptionData it) => object.Equals(((SelectorOptData<T>)it).key, key));
	}

	public void RemoveOption(T key)
	{
		int indexOf = GetIndexOf(key);
		if (indexOf >= 0)
		{
			dropdown.options.RemoveAt(indexOf);
			dropdown.RefreshShownValue();
			if (dropdown.IsExpanded)
			{
				dropdown.Hide();
				dropdown.Show();
			}
		}
	}

	public void RefreshTexts()
	{
		foreach (SelectorOptData<T> option in dropdown.options)
		{
			option.RefreshText();
		}
		dropdown.RefreshShownValue();
	}
}
