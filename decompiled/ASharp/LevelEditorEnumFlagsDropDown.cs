using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class LevelEditorEnumFlagsDropDown : MonoBehaviour
{
	public TMP_Dropdown DropDown;

	public Sprite SelectedSprite;

	private Type m_TypeUsed;

	private List<Enum> m_EnumOptions;

	private string m_CurrentEnumToStringValue;

	public void SetEnumFlagType<T>()
	{
		m_TypeUsed = null;
		if (!typeof(T).IsEnum)
		{
			Log.ErrorFormat("Can't create flags dropdown for type provided: {0}", typeof(T).ToString());
			return;
		}
		m_TypeUsed = typeof(T);
		m_EnumOptions = new List<Enum>();
		List<T> list = ((T[])Enum.GetValues(m_TypeUsed)).ToList();
		for (int i = 0; i < list.Count; i++)
		{
			Enum item = Enum.Parse(typeof(T), list[i].ToString()) as Enum;
			m_EnumOptions.Add(item);
		}
		DropDown.ClearOptions();
		DropDown.AddOptions(m_EnumOptions.Select((Enum o) => o.ToString()).ToList());
	}

	public void SetCurrentValue<T>(string enumToStringValue)
	{
		if (m_TypeUsed != typeof(T))
		{
			Log.ErrorFormat("Incorrect Type passed to Flags dropDown: {0}", typeof(T).ToString());
		}
		else
		{
			RefreshWithCurrentValue(enumToStringValue);
		}
	}

	private void RefreshWithCurrentValue(string enumToStringValue)
	{
		m_CurrentEnumToStringValue = enumToStringValue;
		Enum obj = null;
		if (!string.IsNullOrEmpty(enumToStringValue))
		{
			obj = Enum.Parse(m_TypeUsed, enumToStringValue) as Enum;
		}
		List<TMP_Dropdown.OptionData> list = new List<TMP_Dropdown.OptionData>();
		int num = 0;
		for (int i = 1; i < m_EnumOptions.Count; i++)
		{
			if (obj != null && obj.HasFlag(m_EnumOptions[i]))
			{
				list.Add(new TMP_Dropdown.OptionData(m_EnumOptions[i].ToString(), SelectedSprite));
				num++;
			}
			else
			{
				list.Add(new TMP_Dropdown.OptionData(m_EnumOptions[i].ToString()));
			}
		}
		if (num > 0)
		{
			list.Insert(0, new TMP_Dropdown.OptionData(m_EnumOptions[0].ToString()));
		}
		else
		{
			list.Insert(0, new TMP_Dropdown.OptionData(m_EnumOptions[0].ToString(), SelectedSprite));
		}
		list.Insert(0, new TMP_Dropdown.OptionData($"<i><b>{num}</b> Selected</i>"));
		DropDown.ClearOptions();
		DropDown.AddOptions(list);
		DropDown.SetValueWithoutNotify(-1);
	}

	public T GetCurrentFlagEnum<T>()
	{
		if (string.IsNullOrEmpty(m_CurrentEnumToStringValue))
		{
			return (T)(object)(Enum.Parse(typeof(T), "None") as Enum);
		}
		return (T)(object)(Enum.Parse(typeof(T), m_CurrentEnumToStringValue) as Enum);
	}

	public void OnDropdownValueSelected(int value)
	{
		value--;
		if (value < 0)
		{
			DropDown.SetValueWithoutNotify(0);
			return;
		}
		if (value == 0)
		{
			RefreshWithCurrentValue(string.Empty);
			return;
		}
		List<string> list = m_CurrentEnumToStringValue.Split(',').ToList();
		string item = m_EnumOptions[value].ToString();
		if (list.Contains(item))
		{
			list.Remove(item);
		}
		else
		{
			list.Add(item);
		}
		list.RemoveAll((string s) => string.IsNullOrWhiteSpace(s));
		string text = string.Empty;
		for (int num = 0; num < list.Count; num++)
		{
			text += string.Format("{0}{1}", (num > 0) ? "," : string.Empty, list[num]);
		}
		RefreshWithCurrentValue(text);
	}

	public void OnClearButtonPressed()
	{
		OnDropdownValueSelected(0);
	}
}
