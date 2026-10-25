using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.Events;

public class InfusionBoardUI : MonoBehaviour
{
	[Serializable]
	private struct UIInfusionElementConfig
	{
		public ElementInfusionBoardManager.EElement element;

		public Sprite creationIcon;

		public Sprite strongIcon;

		public Sprite waningIcon;

		public Sprite textHighlightBackground;

		public Color textColor;
	}

	[Serializable]
	public class InfusionEvent : UnityEvent<List<ElementInfusionBoardManager.EElement>>
	{
	}

	private static InfusionBoardUI instance;

	[SerializeField]
	private InfusionElementUI elementPrefab;

	[SerializeField]
	private Transform elementsHolder;

	[SerializeField]
	private UIInfusionElementConfig[] elementConfigs;

	public InfusionEvent OnReservedElement = new InfusionEvent();

	public InfusionEvent OnUnreserveElement = new InfusionEvent();

	private Dictionary<ElementInfusionBoardManager.EElement, InfusionElementUI> elementsUI = new Dictionary<ElementInfusionBoardManager.EElement, InfusionElementUI>();

	private List<ElementInfusionBoardManager.EElement> elementsInCreation = new List<ElementInfusionBoardManager.EElement>();

	private List<ElementInfusionBoardManager.EElement> elementsReserved = new List<ElementInfusionBoardManager.EElement>();

	private List<ElementInfusionBoardManager.EElement> elementsInCreationSaved = new List<ElementInfusionBoardManager.EElement>();

	private List<ElementInfusionBoardManager.EElement> elementsReservedSaved = new List<ElementInfusionBoardManager.EElement>();

	private bool stateSaved;

	private bool _invalidateElements;

	public static InfusionBoardUI Instance
	{
		get
		{
			if (instance == null)
			{
				instance = UnityEngine.Object.FindObjectOfType<InfusionBoardUI>();
			}
			return instance;
		}
	}

	private void Awake()
	{
		stateSaved = false;
		for (int i = 0; i < 6; i++)
		{
			ElementInfusionBoardManager.EElement element = (ElementInfusionBoardManager.EElement)i;
			elementsUI[element] = UnityEngine.Object.Instantiate(elementPrefab, elementsHolder);
			UIInfusionElementConfig uIInfusionElementConfig = elementConfigs.First((UIInfusionElementConfig it) => it.element == element);
			elementsUI[element].Init(element, uIInfusionElementConfig.strongIcon, uIInfusionElementConfig.creationIcon, uIInfusionElementConfig.waningIcon, uIInfusionElementConfig.textHighlightBackground, uIInfusionElementConfig.textColor);
			elementsUI[element].gameObject.SetActive(value: false);
		}
	}

	private void OnDestroy()
	{
		instance = null;
	}

	public void UnreserveElements()
	{
		elementsReserved.Clear();
		stateSaved = false;
		UpdateBoard();
	}

	public void ReserveElement(ElementInfusionBoardManager.EElement element, bool active)
	{
		if (active)
		{
			if (!elementsReserved.Contains(element))
			{
				elementsReserved.Add(element);
				UpdateBoard();
				OnReservedElement.Invoke(new List<ElementInfusionBoardManager.EElement> { element });
			}
		}
		else if (elementsReserved.Contains(element))
		{
			elementsReserved.Remove(element);
			UpdateBoard();
			OnUnreserveElement.Invoke(new List<ElementInfusionBoardManager.EElement> { element });
		}
	}

	public void MarkElementsDirty()
	{
		_invalidateElements = true;
	}

	private void LateUpdate()
	{
		if (_invalidateElements)
		{
			_invalidateElements = false;
			CalcReserveElements();
		}
	}

	public void CalcReserveElements()
	{
		List<ElementInfusionBoardManager.EElement> list = elementsReserved.ToList();
		elementsReserved.Clear();
		foreach (ElementInfusionBoardManager.EElement allConsume in Singleton<UIUseItemsBar>.Instance.GetAllConsumes())
		{
			elementsReserved.Add(allConsume);
		}
		foreach (ElementInfusionBoardManager.EElement allConsume2 in Singleton<UIActiveBonusBar>.Instance.GetAllConsumes())
		{
			elementsReserved.Add(allConsume2);
		}
		foreach (ElementInfusionBoardManager.EElement allConsume3 in Singleton<UIUseAbilitiesBar>.Instance.GetAllConsumes())
		{
			elementsReserved.Add(allConsume3);
		}
		foreach (ElementInfusionBoardManager.EElement allConsume4 in Singleton<UIUseAugmentationsBar>.Instance.GetAllConsumes())
		{
			elementsReserved.Add(allConsume4);
		}
		List<ElementInfusionBoardManager.EElement> list2 = elementsReserved.Except(list).ToList();
		List<ElementInfusionBoardManager.EElement> list3 = list.Except(elementsReserved).ToList();
		if (list2.Count > 0)
		{
			UpdateBoard();
			OnReservedElement?.Invoke(list2);
		}
		if (list3.Count > 0)
		{
			UpdateBoard();
			OnUnreserveElement?.Invoke(list3);
		}
	}

	public bool IsElementReserved(ElementInfusionBoardManager.EElement element)
	{
		if (element == ElementInfusionBoardManager.EElement.Any)
		{
			for (int i = 0; i < 6; i++)
			{
				if (ElementInfusionBoardManager.ElementColumn((ElementInfusionBoardManager.EElement)i) != ElementInfusionBoardManager.EColumn.Inert && !IsElementReserved((ElementInfusionBoardManager.EElement)i))
				{
					return false;
				}
			}
			return true;
		}
		if (!elementsReserved.Contains(element))
		{
			return false;
		}
		if (ScenarioScript.current != null && !ScenarioScript.current.ValidateElementReserved(element))
		{
			return false;
		}
		return true;
	}

	public List<ElementInfusionBoardManager.EElement> GetAvailableElements()
	{
		List<ElementInfusionBoardManager.EElement> list = new List<ElementInfusionBoardManager.EElement>();
		for (int i = 0; i < 6; i++)
		{
			ElementInfusionBoardManager.EElement eElement = (ElementInfusionBoardManager.EElement)i;
			if (ElementInfusionBoardManager.ElementColumn(eElement, Choreographer.s_Choreographer.CurrentPlayerActor) != ElementInfusionBoardManager.EColumn.Inert && !IsElementReserved(eElement))
			{
				list.Add(eElement);
			}
		}
		return list;
	}

	public void SaveState()
	{
		elementsInCreationSaved = new List<ElementInfusionBoardManager.EElement>(elementsInCreation);
		elementsReservedSaved = new List<ElementInfusionBoardManager.EElement>(elementsReserved);
		stateSaved = true;
	}

	public void RestoreState()
	{
		if (stateSaved)
		{
			elementsInCreation = new List<ElementInfusionBoardManager.EElement>(elementsInCreationSaved);
			elementsReserved = new List<ElementInfusionBoardManager.EElement>(elementsReservedSaved);
		}
		stateSaved = false;
	}

	public void OnAfterLoad()
	{
		elementsInCreation.Clear();
		elementsReserved.Clear();
		elementsInCreationSaved.Clear();
		elementsReservedSaved.Clear();
		stateSaved = false;
		foreach (KeyValuePair<string, List<ElementInfusionBoardManager.EElement>> item in ElementInfusionBoardManager.GetElementsToInfuse())
		{
			foreach (ElementInfusionBoardManager.EElement item2 in item.Value)
			{
				if (!elementsInCreation.Contains(item2))
				{
					elementsInCreation.Add(item2);
				}
			}
		}
	}

	public void UpdateBoard(ElementInfusionBoardManager.EElement element)
	{
		UpdateBoard(new List<ElementInfusionBoardManager.EElement> { element });
	}

	public void RemoveElementsInCreation(List<ElementInfusionBoardManager.EElement> elements)
	{
		bool flag = false;
		foreach (ElementInfusionBoardManager.EElement element in elements)
		{
			flag |= elementsInCreation.Remove(element);
		}
		if (flag)
		{
			UpdateBoard();
		}
	}

	public void UpdateBoard(List<ElementInfusionBoardManager.EElement> createdElements = null)
	{
		if (createdElements != null && createdElements.Count > 0 && createdElements[0] == ElementInfusionBoardManager.EElement.Any)
		{
			return;
		}
		if (createdElements != null)
		{
			foreach (ElementInfusionBoardManager.EElement createdElement in createdElements)
			{
				if ((ScenarioScript.current == null || ScenarioScript.current.ValidateElementInCreation(createdElement)) && !elementsInCreation.Contains(createdElement))
				{
					elementsInCreation.Add(createdElement);
				}
			}
		}
		List<ElementInfusionBoardManager.EElement> list = new List<ElementInfusionBoardManager.EElement>();
		for (int i = 0; i < 6; i++)
		{
			if (ElementInfusionBoardManager.ElementColumn((ElementInfusionBoardManager.EElement)i) > ElementInfusionBoardManager.EColumn.Inert)
			{
				if (elementsInCreation.Contains((ElementInfusionBoardManager.EElement)i))
				{
					elementsInCreation.Remove((ElementInfusionBoardManager.EElement)i);
				}
				list.Add((ElementInfusionBoardManager.EElement)i);
			}
		}
		list.AddRange(elementsInCreation);
		list.Sort();
		foreach (KeyValuePair<ElementInfusionBoardManager.EElement, InfusionElementUI> item in elementsUI)
		{
			if (!list.Contains(item.Key))
			{
				item.Value.SetState(null);
				item.Value.gameObject.SetActive(value: false);
			}
			else if (IsElementReserved(item.Key))
			{
				item.Value.gameObject.SetActive(value: false);
			}
			else
			{
				item.Value.gameObject.SetActive(value: true);
				item.Value.SetState(ElementInfusionBoardManager.ElementColumn(item.Key), elementsInCreation.Contains(item.Key) && ElementInfusionBoardManager.ElementColumn(item.Key) == ElementInfusionBoardManager.EColumn.Inert, elementsReserved.Contains(item.Key));
			}
		}
	}

	public void SetAvailableElements(List<ElementInfusionBoardManager.EElement> elements)
	{
		foreach (KeyValuePair<ElementInfusionBoardManager.EElement, InfusionElementUI> item in elementsUI)
		{
			item.Value.SetAvailable(elements.Contains(ElementInfusionBoardManager.EElement.Any) || elements.Contains(item.Key));
		}
	}
}
