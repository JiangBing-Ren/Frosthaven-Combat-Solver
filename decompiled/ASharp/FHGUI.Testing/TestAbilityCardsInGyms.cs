using System.Linq;
using ScenarioRuleLibrary;
using UnityEngine;

namespace FHGUI.Testing;

[DefaultExecutionOrder(-99)]
public class TestAbilityCardsInGyms : MonoBehaviour
{
	[SerializeField]
	private RectTransform _container;

	[SerializeField]
	private CardHandMode _mode = CardHandMode.Preview;

	private void Awake()
	{
		AbilityCardUI[] componentsInChildren = (_container ?? transform).GetComponentsInChildren<AbilityCardUI>();
		UIInfoTools.ForceLoadInstance();
		AbilityCardUI[] array = componentsInChildren;
		foreach (AbilityCardUI card in array)
		{
			CAbilityCard abilityCard = CharacterClassManager.AllAbilityCards.FirstOrDefault((CAbilityCard c) => c.ID == card.CardID);
			card.Init(abilityCard);
			card.SetMode(_mode, null);
		}
	}
}
