using System;
using TMPro;

public class SelectorOptData<T> : TMP_Dropdown.OptionData
{
	public T key;

	private Func<string> textGenerator;

	public SelectorOptData(T key, Func<string> textGenerator)
	{
		this.textGenerator = textGenerator;
		this.key = key;
		RefreshText();
	}

	public void RefreshText()
	{
		text = textGenerator();
	}
}
