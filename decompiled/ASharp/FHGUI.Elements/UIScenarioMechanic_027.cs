using System.Collections.Generic;
using System.Linq;
using FHGUI.Common;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;

namespace FHGUI.Elements;

public class UIScenarioMechanic_027 : UIScenarioMechanicsEntry
{
	[Header("Components")]
	[SerializeField]
	private TMP_Text m_DebugText;

	[SerializeField]
	private RectTransform m_AbilityCardsContainer;

	[SerializeField]
	private UIAbilityCardContainer m_AbilityCardContainerPrototype;

	private RectTransform m_FullCardReparentContainer;

	private List<UIAbilityCardContainer> m_AbilityCards = new List<UIAbilityCardContainer>();

	private bool _initialized;

	private void InitCache()
	{
		if (!_initialized)
		{
			_initialized = true;
			m_FullCardReparentContainer = GetComponentInParent<ScenarioMechanicsInfoModule>()?.AbilityCardsToolipLocations;
			m_AbilityCardContainerPrototype.gameObject.SetActive(value: false);
		}
	}

	public override void Init(bool newEntry, object data, LocalizationParam[] localizationParams, string simpleTextLocKey)
	{
		base.Init(newEntry, data, localizationParams, simpleTextLocKey);
		InitCache();
		BuildTackedDeck();
	}

	private void BuildTackedDeck()
	{
		if (ScenarioManager.Scenario == null)
		{
			return;
		}
		List<CPlayerActor> list = ScenarioManager.Scenario.PlayerActors.Where((CPlayerActor actor) => actor.CharacterClass.TakenCards.Count > 0).ToList();
		int count = list.Count;
		HelperTools.NormalizePool(ref m_AbilityCards, m_AbilityCardContainerPrototype.gameObject, m_AbilityCardsContainer, count);
		for (int num = 0; num < count; num++)
		{
			CPlayerActor cPlayerActor = list[num];
			UIAbilityCardContainer uIAbilityCardContainer = m_AbilityCards[num];
			uIAbilityCardContainer.SetTooltipContainer(m_FullCardReparentContainer);
			uIAbilityCardContainer.Init(cPlayerActor.CharacterClass.TakenCards.ToList(), cPlayerActor);
			TMP_Text tMP_Text = uIAbilityCardContainer.transform.Find("HeroName")?.GetComponent<TMP_Text>();
			if ((bool)tMP_Text)
			{
				tMP_Text.text = cPlayerActor.CharacterName;
			}
		}
	}

	private void OnDisable()
	{
		foreach (UIAbilityCardContainer abilityCard in m_AbilityCards)
		{
			abilityCard.Clear();
		}
	}
}
