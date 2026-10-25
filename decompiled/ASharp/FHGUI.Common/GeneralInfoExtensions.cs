using System;
using TMPro;
using UnityEngine;

namespace FHGUI.Common;

public static class GeneralInfoExtensions
{
	public static void InitInfoCategory<T>(this IUIGeneralInfo info, bool show, T container, TMP_Text label, Action<T> fill) where T : MonoBehaviour
	{
		label.gameObject.SetActive(show);
		container.gameObject.SetActive(show);
		if (show)
		{
			fill?.Invoke(container);
		}
	}
}
