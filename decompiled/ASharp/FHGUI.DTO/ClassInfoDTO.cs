using System;
using System.Collections.Generic;
using System.Linq;
using FHRL;
using GLOOM;
using ScenarioRuleLibrary;
using UnityEngine;

namespace FHGUI.DTO;

[Serializable]
public class ClassInfoDTO
{
	[Header("General Data")]
	public string ClassName;

	public string ClassNameFull;

	public string Description;

	public int Health;

	public int HandSize;

	public int Complexity;

	public int MaxComplexity;

	public Sprite Icon;

	public List<SpriteWithTooltipDTO> Traits = new List<SpriteWithTooltipDTO>();

	public List<SpriteWithTooltipDTO> Affinities = new List<SpriteWithTooltipDTO>();

	public List<GoalDTO> Masteries = new List<GoalDTO>();

	[Header("Stats")]
	public int Melee;

	public int Ranged;

	public int Mobility;

	public int Support;

	public int Defense;

	public int Control;

	[Header("Misc")]
	public string StrengthsLocKey;

	public string WeaknessesLocKey;

	public string MechanicsLocKey;

	public string CodexID;

	public string CodexInfoID;

	private const uint CHRACTER_LEVEL_MIN = 1u;

	private const uint CHRACTER_LEVEL_MAX = 9u;

	public ClassInfoDTO()
	{
	}

	public ClassInfoDTO(uint level, CharacterClassDef classDef, List<GoalDTO> masteryDTOs)
	{
		ClassName = classDef.ClassName.RemoveChar('$');
		Description = classDef.Description.RemoveChar('$');
		level = Math.Clamp(level, 1u, 9u);
		Health = (int)classDef.HealthTable[level];
		if (SaveData.Instance?.Global?.CurrentAdventureData?.AdventureMapState?.Difficulty != null && SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.Difficulty.HasHeroHealthModifier)
		{
			Health = (int)Math.Floor((float)Health * SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.Difficulty.HeroHealthModifier);
		}
		HandSize = (int)classDef.MaxCards;
		Complexity = (int)classDef.Complexity;
		MaxComplexity = 5;
		Icon = UIInfoTools.Instance.GetCharacterConfigUI(CrossReference.ClassDefToECharacterModel(classDef)).iconClassGold;
		Traits = classDef.Traits.Select((CharacterTraitType traitType) => new SpriteWithTooltipDTO
		{
			Title = LocalizationManager.GetTranslation("TRAIT_" + traitType),
			Sprite = UIInfoTools.Instance.GetCharacterTraitIcon(traitType),
			TextValue = LocalizationManager.GetTranslation("TRAIT_" + traitType)
		}).ToList();
		Affinities = classDef.Affinities.Select((CardEnhancementEffectType ceet) =>
		{
			ElementInfusionBoardManager.EElement element = CrossReference.CardEnhancementTypeToEElement(ceet);
			return new SpriteWithTooltipDTO
			{
				Title = LocalizationManager.GetTranslation("ENHANCEMENT_" + element),
				Sprite = UIInfoTools.Instance.GetElementIcon(element)
			};
		}).ToList();
		Melee = (int)classDef.Melee;
		Ranged = (int)classDef.Ranged;
		Mobility = (int)classDef.Mobility;
		Support = (int)classDef.Support;
		Defense = (int)classDef.Defense;
		Control = (int)classDef.Control;
		StrengthsLocKey = classDef.Strengths.RemoveChar('$');
		WeaknessesLocKey = classDef.Weaknesses.RemoveChar('$');
		Masteries = masteryDTOs;
		string text = classDef.ID.ID.Replace("ID", "");
		MechanicsLocKey = "CODEX_HeroInfo_" + text;
		ClassNameFull = text + "_FullName";
		CodexID = text;
		CodexInfoID = text + "Info";
	}
}
