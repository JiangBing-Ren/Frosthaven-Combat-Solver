using System;
using System.Linq;
using ScenarioRuleLibrary;
using UnityEngine;

namespace FHGUI.DTO;

[CreateAssetMenu(fileName = "HeroGaugeVisualsUIDataSO", menuName = "Def/HeroGaugeVisualsUIDataSO")]
public class HeroGaugeVisualsUIDataSO : ScriptableObject
{
	[Serializable]
	public struct HeroGaugeVisuals
	{
		public ECharacter Character;

		public Sprite Token;

		public Sprite[] GaugeLevels;
	}

	public HeroGaugeVisuals[] Visuals;

	public bool GetGaugeVisuals(ECharacter character, int gaugeLevel, out Sprite gaugeSprite, out Sprite tokenSprite)
	{
		HeroGaugeVisuals heroGaugeVisuals = Visuals.FirstOrDefault((HeroGaugeVisuals v) => v.Character == character);
		if (heroGaugeVisuals.GaugeLevels == null || heroGaugeVisuals.GaugeLevels.Length == 0)
		{
			gaugeSprite = null;
			tokenSprite = null;
			return false;
		}
		int num = Mathf.Clamp(gaugeLevel, 0, heroGaugeVisuals.GaugeLevels.Length - 1);
		gaugeSprite = heroGaugeVisuals.GaugeLevels[num];
		tokenSprite = heroGaugeVisuals.Token;
		return true;
	}
}
