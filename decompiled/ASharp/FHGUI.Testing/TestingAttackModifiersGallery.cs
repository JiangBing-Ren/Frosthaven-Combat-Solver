using System.Collections.Generic;
using FH.YML;
using FHGUI.DTO;
using FHGUI.Elements;
using FHRL;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;

namespace FHGUI.Testing;

public class TestingAttackModifiersGallery : MonoBehaviour
{
	private class DataPair
	{
		public AttackModifierYMLData Scenario;

		public CharacterAttackModifierDef Outpost;
	}

	[Header("Components")]
	[SerializeField]
	private RectTransform Contianer;

	[SerializeField]
	private GameObject RowPrototype;

	private void Start()
	{
		Build();
	}

	private void Build()
	{
		List<DataPair> list = new List<DataPair>();
		foreach (AttackModifierYMLData attackModifier in ScenarioRuleClient.SRLYML.AttackModifiers)
		{
			DataPair dataPair = new DataPair();
			dataPair.Scenario = attackModifier;
			if (YMLDefRepository.Repo.DefRepository.AttackModifiers.TryGetValue(new CharacterAttackModifierID(attackModifier.Name), out var value))
			{
				dataPair.Outpost = value;
			}
			list.Add(dataPair);
		}
		int num = 0;
		foreach (DataPair item in list)
		{
			GameObject gameObject = Object.Instantiate(RowPrototype, Contianer);
			AttackModElementUI componentInChildren = gameObject.GetComponentInChildren<AttackModElementUI>();
			if ((bool)componentInChildren)
			{
				componentInChildren.m_Modifier = item.Scenario;
				componentInChildren.SetTextStyleForModifiers(componentInChildren.m_Modifier, Color.white, showOriginalModifier: false);
				componentInChildren.m_ValueGroup.alpha = 1f;
			}
			FHUIAttackModifier componentInChildren2 = gameObject.GetComponentInChildren<FHUIAttackModifier>();
			if ((bool)componentInChildren2)
			{
				componentInChildren2.Init(new AttackModifierDTO(item.Outpost, 1));
			}
			TMP_Text tMP_Text = gameObject.FindInChildren("_ModId", includeInactive: true)?.GetComponent<TMP_Text>();
			if ((bool)tMP_Text)
			{
				tMP_Text.text = $"{num + 1} {item.Scenario.Name}";
			}
			num++;
		}
		RowPrototype.gameObject.SetActive(value: false);
	}
}
