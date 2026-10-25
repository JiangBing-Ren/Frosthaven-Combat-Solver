namespace UnityEngine.UI;

public static class UIUtility
{
	public static void BringToFront(GameObject go, Canvas optionalCanvas = null, bool worldPositionStays = true)
	{
		if (optionalCanvas != null)
		{
			go.transform.SetParent(optionalCanvas.transform, worldPositionStays);
		}
		else
		{
			Canvas canvas = FindInParents<Canvas>(go);
			if (canvas != null)
			{
				go.transform.SetParent(canvas.transform, worldPositionStays);
			}
		}
		go.transform.SetAsLastSibling();
	}

	public static void BringToFront(GameObject go, Transform optionalParent, bool worldPositionStays = true)
	{
		if (optionalParent != null)
		{
			go.transform.SetParent(optionalParent, worldPositionStays);
		}
		else
		{
			Canvas canvas = FindInParents<Canvas>(go);
			if (canvas != null)
			{
				go.transform.SetParent(canvas.transform, worldPositionStays);
			}
		}
		go.transform.SetAsLastSibling();
	}

	public static T FindInParents<T>(GameObject go) where T : Component
	{
		if (go == null)
		{
			return null;
		}
		T component = go.GetComponent<T>();
		if (component != null)
		{
			return component;
		}
		Transform parent = go.transform.parent;
		while (parent != null && component == null)
		{
			component = parent.gameObject.GetComponent<T>();
			parent = parent.parent;
		}
		return component;
	}

	public static void ForceRecalclulateLayouts(RectTransform transform)
	{
		if (transform == null || !transform.gameObject.activeSelf)
		{
			return;
		}
		foreach (RectTransform item in transform)
		{
			ForceRecalclulateLayouts(item);
		}
		LayoutGroup component = transform.GetComponent<LayoutGroup>();
		ContentSizeFitter component2 = transform.GetComponent<ContentSizeFitter>();
		if (component != null)
		{
			component.SetLayoutHorizontal();
			component.SetLayoutVertical();
		}
		if (component2 != null)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
		}
	}
}
