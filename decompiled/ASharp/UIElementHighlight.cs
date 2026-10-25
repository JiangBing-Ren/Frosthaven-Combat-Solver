using System.Collections;
using System.Collections.Generic;
using Chronos;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class UIElementHighlight : MonoBehaviour
{
	[SerializeField]
	private List<Image> images;

	[SerializeField]
	private float highlightElementDuration = 3f;

	private Coroutine highlightCoroutine;

	public void Highlight(Color color)
	{
		StopHighlight();
		for (int i = 0; i < images.Count; i++)
		{
			images[i].color = color;
		}
	}

	public void Highlight(List<ElementInfusionBoardManager.EElement> elements, int index = 0)
	{
		StopHighlight();
		if (elements.Count == 0)
		{
			for (int i = 0; i < images.Count; i++)
			{
				images[i].color = UIInfoTools.Instance.GetElementHighlightColor(elements[0], images[i].color.a);
			}
		}
		else if (elements.Count == 1)
		{
			for (int j = 0; j < images.Count; j++)
			{
				images[j].color = UIInfoTools.Instance.GetElementHighlightColor(elements[0], images[j].color.a);
			}
		}
		else if (gameObject.activeInHierarchy)
		{
			highlightCoroutine = StartCoroutine(IterateElementHighlight(elements));
		}
	}

	private IEnumerator IterateElementHighlight(List<ElementInfusionBoardManager.EElement> elements, int index = 0)
	{
		while (true)
		{
			for (int i = 0; i < images.Count; i++)
			{
				images[i].color = UIInfoTools.Instance.GetElementHighlightColor(elements[index], images[i].color.a);
			}
			yield return Timekeeper.instance.WaitForSeconds(highlightElementDuration);
			index = (index + 1) % elements.Count;
		}
	}

	public void StopHighlight()
	{
		if (highlightCoroutine != null)
		{
			StopCoroutine(highlightCoroutine);
		}
		highlightCoroutine = null;
	}
}
