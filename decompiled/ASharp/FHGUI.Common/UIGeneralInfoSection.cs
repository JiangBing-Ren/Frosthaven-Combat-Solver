using System;
using UnityEngine;

namespace FHGUI.Common;

public class UIGeneralInfoSection : MonoBehaviour
{
	[SerializeField]
	private GameObject _labelContainer;

	private Component _component;

	[field: SerializeField]
	public TextLocalizedListener Label { get; private set; }

	[field: SerializeField]
	public RectTransform Content { get; private set; }

	public void Fill<T>(bool showLabel, bool showContent, Action<T> initContainer = null) where T : Component
	{
		bool flag = showLabel | showContent;
		gameObject.SetActive(flag);
		_labelContainer.SetActive(showLabel);
		Content.gameObject.SetActive(showContent);
		if (flag)
		{
			T val = Get<T>();
			if ((bool)val)
			{
				initContainer?.Invoke(val);
			}
		}
	}

	public void Fill<T>(bool show, Action<T> initContainer = null) where T : Component
	{
		Fill(show, show, initContainer);
	}

	private T Get<T>() where T : Component
	{
		if (Content == null)
		{
			return null;
		}
		if (_component == null)
		{
			_component = Content.GetComponent<T>();
		}
		if (_component is T result)
		{
			return result;
		}
		return null;
	}
}
