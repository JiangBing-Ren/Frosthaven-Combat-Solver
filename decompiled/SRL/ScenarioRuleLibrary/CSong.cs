using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

[JsonObject(MemberSerialization.OptIn)]
public class CSong
{
	[JsonObject(MemberSerialization.OptIn)]
	public class SongEffect
	{
		[JsonProperty]
		public ESongEffectType SongEffectType;

		[JsonProperty]
		public ESongActivationType SongActivationType;

		[JsonProperty]
		public CAbility.EAbilityType AbilityType;

		[JsonProperty]
		public CAbility.EAttackType AttackType;

		[JsonProperty]
		public List<CAbility> Abilities;

		[JsonProperty]
		public List<CAbilityOverride> AbilityOverrides;

		public SongEffect()
		{
		}

		public SongEffect(List<CAbility> abilities, ESongActivationType activationType, CAbility.EAttackType type, CAbility.EAbilityType abilityType)
		{
			SongEffectType = ESongEffectType.Ability;
			SongActivationType = activationType;
			Abilities = abilities;
			AttackType = type;
			AbilityType = abilityType;
		}

		public SongEffect(List<CAbilityOverride> abilityOverrides, ESongActivationType activationType, CAbility.EAttackType type, CAbility.EAbilityType abilityType)
		{
			SongEffectType = ESongEffectType.Override;
			SongActivationType = activationType;
			AbilityOverrides = abilityOverrides;
			AttackType = type;
			AbilityType = abilityType;
		}

		public void OnSongEffectToggled(CAbility currentAbility, bool toggledOn)
		{
			if (IsValidAbilityType(currentAbility) & toggledOn)
			{
				if (AbilityOverrides != null && AbilityOverrides.Count > 0)
				{
					foreach (CAbilityOverride abilityOverride in AbilityOverrides)
					{
						CAbilityOverride cAbilityOverride = currentAbility.OverrideAbilityValues(abilityOverride, perform: false);
						if (cAbilityOverride.AbilityType.HasValue && cAbilityOverride.AbilityType != cAbilityOverride.OriginalAbility.AbilityType && PhaseManager.CurrentPhase is CPhaseAction cPhaseAction)
						{
							cPhaseAction.FinalizeOverrideAbilityType(currentAbility, cAbilityOverride);
						}
					}
				}
				if (Abilities == null || Abilities.Count <= 0)
				{
					return;
				}
				List<CAbility> list = new List<CAbility>();
				foreach (CAbility ability in Abilities)
				{
					CAbility item = CAbility.CopyAbility(ability, generateNewID: false);
					list.Add(item);
				}
				(PhaseManager.CurrentPhase as CPhaseAction).StackInlineSubAbilities(list);
			}
			else
			{
				if (toggledOn)
				{
					return;
				}
				if (AbilityOverrides != null && AbilityOverrides.Count > 0)
				{
					foreach (CAbilityOverride abilityOverride2 in AbilityOverrides)
					{
						bool num = abilityOverride2.AbilityType.HasValue && abilityOverride2.AbilityType != abilityOverride2.OriginalAbility.AbilityType;
						currentAbility.UndoOverride(abilityOverride2, perform: false);
						if (num && PhaseManager.CurrentPhase is CPhaseAction cPhaseAction2)
						{
							cPhaseAction2.FinalizeOverrideAbilityType(currentAbility);
						}
					}
				}
				if (Abilities == null || Abilities.Count <= 0)
				{
					return;
				}
				foreach (CAbility ability2 in Abilities)
				{
					(PhaseManager.CurrentPhase as CPhaseAction).UnstackAbility(ability2);
				}
			}
		}

		public bool IsValidAbilityType(CAbility conditionAbility)
		{
			if (AbilityType != CAbility.EAbilityType.None)
			{
				return AbilityType == conditionAbility.AbilityType;
			}
			return true;
		}

		public bool IsValidAbilityType(CAbility.EAbilityType abilityType)
		{
			if (AbilityType != CAbility.EAbilityType.None)
			{
				return AbilityType == abilityType;
			}
			return true;
		}
	}

	public enum ESongEffectType
	{
		None,
		Ability,
		Override
	}

	public enum ESongActivationType
	{
		None,
		AbilityStart,
		ActionStart
	}

	public static readonly ESongEffectType[] SongEffectTypes = (ESongEffectType[])Enum.GetValues(typeof(ESongEffectType));

	public static readonly ESongActivationType[] SongActivationTypes = (ESongActivationType[])Enum.GetValues(typeof(ESongActivationType));

	public Guid ID;

	[JsonProperty]
	public List<SongEffect> SongEffects;

	public string ActiveBonusLayout;

	[JsonProperty]
	public CAbilityFilterContainer Filter;

	public CBaseCard RegisteredBaseCard { get; set; }

	[JsonConstructor]
	public CSong(List<SongEffect> songEffects, CAbilityFilterContainer filter)
	{
		ID = Guid.NewGuid();
		SongEffects = songEffects;
		Filter = filter;
	}
}
