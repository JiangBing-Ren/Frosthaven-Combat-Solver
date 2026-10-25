using System.Collections.Generic;
using UnityEngine;

public class TacticalOnlyElements : MonoBehaviour
{
	public List<GameObject> elements;

	public void EnableElements(bool enable)
	{
		foreach (GameObject element in elements)
		{
			if (!(element == null))
			{
				element.SetActive(enable);
			}
		}
	}
}
