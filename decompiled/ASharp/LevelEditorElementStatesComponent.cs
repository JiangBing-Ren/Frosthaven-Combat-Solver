using System;
using System.Collections.Generic;
using ScenarioRuleLibrary;
using UnityEngine;

public class LevelEditorElementStatesComponent : MonoBehaviour
{
	public Transform ElementsContent;

	public GameObject LevelEditorElementStatesElementGroupPrefab;

	private Dictionary<ElementInfusionBoardManager.EElement, LevelEditorElementStatesElementGroup> elementToElementGroup;

	private void Awake()
	{
		elementToElementGroup = new Dictionary<ElementInfusionBoardManager.EElement, LevelEditorElementStatesElementGroup>();
		LevelEditorElementStatesElementGroupPrefab.SetActive(value: false);
		ElementInfusionBoardManager.EElement[] elements = ElementInfusionBoardManager.Elements;
		for (int i = 0; i < elements.Length; i++)
		{
			ElementInfusionBoardManager.EElement eElement = elements[i];
			if (eElement != ElementInfusionBoardManager.EElement.Any)
			{
				LevelEditorElementStatesElementGroup component = UnityEngine.Object.Instantiate(LevelEditorElementStatesElementGroupPrefab, ElementsContent).GetComponent<LevelEditorElementStatesElementGroup>();
				component.Setup(eElement.ToString());
				component.gameObject.SetActive(value: true);
				elementToElementGroup[eElement] = component;
			}
		}
	}

	public void SetStates(List<ElementInfusionBoardManager.EElement> strongElements, List<ElementInfusionBoardManager.EElement> waningElements, List<ElementInfusionBoardManager.EElement> inertElements)
	{
		RefreshUi(strongElements, waningElements, inertElements);
	}

	public void GetStates(out List<ElementInfusionBoardManager.EElement> strongElements, out List<ElementInfusionBoardManager.EElement> waningElements, out List<ElementInfusionBoardManager.EElement> inertElements)
	{
		List<ElementInfusionBoardManager.EElement> list = new List<ElementInfusionBoardManager.EElement>();
		List<ElementInfusionBoardManager.EElement> list2 = new List<ElementInfusionBoardManager.EElement>();
		List<ElementInfusionBoardManager.EElement> list3 = new List<ElementInfusionBoardManager.EElement>();
		ElementInfusionBoardManager.EElement[] elements = ElementInfusionBoardManager.Elements;
		foreach (ElementInfusionBoardManager.EElement eElement in elements)
		{
			if (eElement != ElementInfusionBoardManager.EElement.Any)
			{
				ElementInfusionBoardManager.EColumn? column = elementToElementGroup[eElement].GetColumn();
				switch (column)
				{
				case ElementInfusionBoardManager.EColumn.Strong:
					list.Add(eElement);
					break;
				case ElementInfusionBoardManager.EColumn.Waning:
					list2.Add(eElement);
					break;
				case ElementInfusionBoardManager.EColumn.Inert:
					list3.Add(eElement);
					break;
				default:
					throw new NotImplementedException($"Invalid EColumn:{column}");
				case null:
					break;
				}
			}
		}
		strongElements = list;
		waningElements = list2;
		inertElements = list3;
	}

	public void Clear()
	{
		if (elementToElementGroup == null)
		{
			return;
		}
		foreach (LevelEditorElementStatesElementGroup value in elementToElementGroup.Values)
		{
			value.SetColumn(null);
		}
	}

	private void RefreshUi(List<ElementInfusionBoardManager.EElement> strongElements, List<ElementInfusionBoardManager.EElement> waningElements, List<ElementInfusionBoardManager.EElement> inertElements)
	{
		RefreshListWithState(strongElements, ElementInfusionBoardManager.EColumn.Strong);
		RefreshListWithState(waningElements, ElementInfusionBoardManager.EColumn.Waning);
		RefreshListWithState(inertElements, ElementInfusionBoardManager.EColumn.Inert);
	}

	private void RefreshListWithState(List<ElementInfusionBoardManager.EElement> strongElements, ElementInfusionBoardManager.EColumn column)
	{
		foreach (ElementInfusionBoardManager.EElement strongElement in strongElements)
		{
			if (strongElement != ElementInfusionBoardManager.EElement.Any)
			{
				elementToElementGroup[strongElement].SetColumn(column);
			}
		}
	}
}
