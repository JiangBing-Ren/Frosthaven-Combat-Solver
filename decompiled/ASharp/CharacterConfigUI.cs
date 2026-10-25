using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using UnityEngine;

[CreateAssetMenu(menuName = "UI Config/Character", fileName = "Character Config")]
public class CharacterConfigUI : ScriptableObject
{
	[Serializable]
	public class Object3DPlacement
	{
		public Vector3 position;

		public Vector3 rotation;
	}

	[Serializable]
	public class Summon3DPlacement
	{
		public CClass.ENPCModel summon;

		[SerializeField]
		public Object3DPlacement guildmasterPlacement;

		[SerializeField]
		public Object3DPlacement campaignPlacement;

		public Object3DPlacement GetObject3DPlacement(bool isGuildmaster)
		{
			if (isGuildmaster)
			{
				return guildmasterPlacement;
			}
			return campaignPlacement;
		}
	}

	public enum ECharacterGender
	{
		Male,
		Female
	}

	[Serializable]
	public struct AbilityCardConfigUI
	{
		public int CardID;

		public Sprite Header;

		public AbilityCardUI Prefab;
	}

	public ECharacter character;

	[Header("General")]
	public Sprite iconClass;

	public Sprite iconClassHighlight;

	public Sprite iconClassGold;

	public Sprite illustration;

	public Sprite InfoIllustration;

	public ECharacterGender gender;

	public AbilityCardUI customCardPrefab;

	public AbilityCardUISkinConfig cardSkin;

	public AbilityCardUISkinConfig altFormCardSkin;

	public List<string> alternativeSkins;

	[Header("Adventure")]
	[SerializeField]
	public Sprite newAdventurePortrait;

	[SerializeField]
	public Sprite newHighlightAdventurePortrait;

	public QuestTypeConfigUI questConfig;

	[Header("New Adventure Assembly")]
	public bool customAssemblyCharacter3DPosition = true;

	[Tooltip("Position to place the 3D character in the adventure selection panel")]
	[ConditionalField("customAssemblyCharacter3DPosition", "true", true)]
	public Object3DPlacement guildmasterAssemblyCharacterPlacement;

	[ConditionalField("customAssemblyCharacter3DPosition", "true", true)]
	public Object3DPlacement campaignAssemblyCharacterPlacement;

	public List<Summon3DPlacement> assemblyCharacter3DCompanions;

	[Tooltip("Animation played when a new merc is created")]
	public string createAnimation = "Attack";

	[Header("Scenario")]
	public Sprite scenarioPortrait;

	public Vector2 scenarioPortraitOffset;

	[Tooltip("Background for the right panel that appear when the character has active bonuses")]
	public Sprite initativeExtensionBackground;

	public CharacterTabSkin tabIconConfig;

	public Sprite activeAbility;

	[Header("Assembly party")]
	public Sprite assemblyPortrait;

	public Vector2 rosterPortraitOffset;

	public bool hasToReveal;

	[Header("Distribution panel")]
	public Sprite distributionPortrait;

	[HideInInspector]
	public string ID;

	[Header("Class Mechanics")]
	public Sprite ActiveToken;

	public Sprite InactiveToken;

	[Header("Cards")]
	public AbilityCardConfigUI[] Cards;

	public Sprite GetAdventurePortrait(bool highlighted = false)
	{
		if (!highlighted)
		{
			return newAdventurePortrait;
		}
		return newHighlightAdventurePortrait;
	}

	public Object3DPlacement GetAssemblyCharacter3DPlacement(bool isGuildmaster)
	{
		if (isGuildmaster)
		{
			return guildmasterAssemblyCharacterPlacement;
		}
		return campaignAssemblyCharacterPlacement;
	}

	public AbilityCardUI GetCardUI(int id)
	{
		return Cards?.FirstOrDefault((AbilityCardConfigUI c) => c.CardID == id).Prefab;
	}

	public Sprite GetCardHeader(int id)
	{
		return Cards?.FirstOrDefault((AbilityCardConfigUI c) => c.CardID == id).Header;
	}
}
