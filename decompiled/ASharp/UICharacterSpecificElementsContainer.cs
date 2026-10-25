using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using UnityEngine;

public class UICharacterSpecificElementsContainer : MonoBehaviour
{
	private List<ICharacterSpecificUIElement> _elements;

	private bool _hasBeenInit;

	private void Awake()
	{
		if (!_hasBeenInit)
		{
			gameObject.SetActive(value: false);
		}
	}

	public void Init(CPlayerActor playerActor)
	{
		if (_elements == null)
		{
			_elements = GetComponentsInChildren<ICharacterSpecificUIElement>(includeInactive: true).ToList();
		}
		_hasBeenInit = true;
		if (playerActor == null || !_elements.Any((ICharacterSpecificUIElement c) => c.IsCharacterElement(playerActor)))
		{
			gameObject.SetActive(value: false);
			return;
		}
		gameObject.SetActive(value: true);
		foreach (ICharacterSpecificUIElement element in _elements)
		{
			element.Init(playerActor);
		}
	}
}
