using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ScenarioRuleLibrary.CustomLevels;
using SharedLibrary.Client;

namespace ScenarioRuleLibrary.YML;

[Serializable]
public class HeroSummonYMLData : BinarySerializableYMLData
{
	public string StatIsBasedOnXEntriesRaw;

	[BinarySerialize]
	public string ID { get; set; }

	[BinarySerialize]
	public string LocKey { get; set; }

	[BinarySerialize]
	public string Model { get; set; }

	[BinarySerialize]
	public int[] HealthTable { get; set; }

	public int? MoveNullable { get; set; }

	public int? AttackNullable { get; set; }

	public int? RangeNullable { get; set; }

	public int? ShieldNullable { get; set; }

	public int? RetaliateNullable { get; set; }

	public int? RetaliateRangeNullable { get; set; }

	public int? PierceNullable { get; set; }

	public bool? FlyingNullable { get; set; }

	public bool? InvulnerableNullable { get; private set; }

	public bool? PierceInvulnerabilityNullable { get; private set; }

	public bool? UntargetableNullable { get; private set; }

	public bool? DoesNotBlockNullable { get; private set; }

	public bool? IgnoreActorCollisionNullable { get; private set; }

	public bool? TreatAsTrapNullable { get; private set; }

	public bool? PlayerControlledNullable { get; private set; }

	[BinarySerialize]
	public ElementInfusionBoardManager.EElement? AttackInfuse { get; set; }

	[BinarySerialize]
	public string AttackAnimOverload { get; set; }

	[BinarySerialize]
	public string AttackOnDeathAnimOverload { get; set; }

	[BinarySerialize]
	public int? AdjacentAttackOnDeathNullable { get; set; }

	[BinarySerialize]
	public List<CCondition.ENegativeCondition> OnAttackConditions { get; set; }

	public List<CAbility> OnSummonAbilities { get; set; }

	public List<CAbility> OnDeathAbilities { get; set; }

	public List<CAbility> SpecialAbilities { get; set; }

	public CAbilityOverride MoveOverride { get; set; }

	public CAbilityOverride AttackOverride { get; set; }

	public CAIFocusOverrideDetails AIFocusOverrideDetails { get; set; }

	public List<AbilityData.StatIsBasedOnXData> StatIsBasedOnXEntries { get; set; }

	public bool NullAttackAnimOverload { get; set; }

	public bool NullAttackOnDeathAnimOverload { get; set; }

	public bool NullAdjacentAttackOnDeathNullable { get; set; }

	public bool NullOnAttackConditions { get; set; }

	public bool NullOnSummonAbilities { get; set; }

	public bool NullOnDeathAbilities { get; set; }

	public bool NullSpecialAbilities { get; set; }

	public bool NullMoveOverride { get; set; }

	public bool NullAttackOverride { get; set; }

	public bool NullStatIsBasedOnXEntries { get; set; }

	[BinarySerialize]
	public string ColourHTML { get; set; }

	[BinarySerialize]
	public float Fatness { get; set; }

	[BinarySerialize]
	public float VertexAnimIntensity { get; set; }

	[BinarySerialize]
	public string CustomConfig { get; set; }

	[BinarySerialize]
	public string CanMoveOnActorClassID { get; set; }

	public List<AbilityData.StatIsBasedOnXData> AttackStatIsBasedOnXEntries => StatIsBasedOnXEntries?.Where((AbilityData.StatIsBasedOnXData x) => x.BaseStatType == EMonsterBaseStats.Attack || x.BaseStatType == EMonsterBaseStats.Range || x.BaseStatType == EMonsterBaseStats.Target).ToList();

	public List<AbilityData.StatIsBasedOnXData> MoveStatIsBasedOnXEntries => StatIsBasedOnXEntries?.Where((AbilityData.StatIsBasedOnXData x) => x.BaseStatType == EMonsterBaseStats.Move).ToList();

	[BinarySerialize]
	public List<EEnhancementMarkType> HealthEnhancements { get; set; }

	[BinarySerialize]
	public int Move
	{
		get
		{
			if (!MoveNullable.HasValue)
			{
				return 0;
			}
			return MoveNullable.Value;
		}
		set
		{
			MoveNullable = value;
		}
	}

	[BinarySerialize]
	public List<EEnhancementMarkType> MoveEnhancements { get; set; } = new List<EEnhancementMarkType>();

	[BinarySerialize]
	public int Attack
	{
		get
		{
			if (!AttackNullable.HasValue)
			{
				return 0;
			}
			return AttackNullable.Value;
		}
		set
		{
			AttackNullable = value;
		}
	}

	[BinarySerialize]
	public List<EEnhancementMarkType> AttackEnhancements { get; set; } = new List<EEnhancementMarkType>();

	[BinarySerialize]
	public int Range
	{
		get
		{
			if (!RangeNullable.HasValue)
			{
				return 1;
			}
			return RangeNullable.Value;
		}
		set
		{
			RangeNullable = value;
		}
	}

	[BinarySerialize]
	public List<EEnhancementMarkType> RangeEnhancements { get; set; } = new List<EEnhancementMarkType>();

	[BinarySerialize]
	public int Shield
	{
		get
		{
			if (!ShieldNullable.HasValue)
			{
				return 0;
			}
			return ShieldNullable.Value;
		}
		set
		{
			ShieldNullable = value;
		}
	}

	[BinarySerialize]
	public int Retaliate
	{
		get
		{
			if (!RetaliateNullable.HasValue)
			{
				return 0;
			}
			return RetaliateNullable.Value;
		}
		set
		{
			RetaliateNullable = value;
		}
	}

	[BinarySerialize]
	public int RetaliateRange
	{
		get
		{
			if (!RetaliateRangeNullable.HasValue)
			{
				return 1;
			}
			return RetaliateRangeNullable.Value;
		}
		set
		{
			RetaliateRangeNullable = value;
		}
	}

	[BinarySerialize]
	public int Pierce
	{
		get
		{
			if (!PierceNullable.HasValue)
			{
				return 0;
			}
			return PierceNullable.Value;
		}
		set
		{
			PierceNullable = value;
		}
	}

	[BinarySerialize]
	public bool Flying
	{
		get
		{
			if (!FlyingNullable.HasValue)
			{
				return false;
			}
			return FlyingNullable.Value;
		}
		set
		{
			FlyingNullable = value;
		}
	}

	[BinarySerialize]
	public bool Invulnerable
	{
		get
		{
			if (!InvulnerableNullable.HasValue)
			{
				return false;
			}
			return InvulnerableNullable.Value;
		}
		set
		{
			InvulnerableNullable = value;
		}
	}

	[BinarySerialize]
	public bool PierceInvulnerability
	{
		get
		{
			if (!PierceInvulnerabilityNullable.HasValue)
			{
				return false;
			}
			return PierceInvulnerabilityNullable.Value;
		}
		set
		{
			PierceInvulnerabilityNullable = value;
		}
	}

	[BinarySerialize]
	public bool Untargetable
	{
		get
		{
			if (!UntargetableNullable.HasValue)
			{
				return false;
			}
			return UntargetableNullable.Value;
		}
		set
		{
			UntargetableNullable = value;
		}
	}

	[BinarySerialize]
	public bool DoesNotBlock
	{
		get
		{
			if (!DoesNotBlockNullable.HasValue)
			{
				return false;
			}
			return DoesNotBlockNullable.Value;
		}
		set
		{
			DoesNotBlockNullable = value;
		}
	}

	[BinarySerialize]
	public bool PlayerControlled
	{
		get
		{
			if (!PlayerControlledNullable.HasValue)
			{
				return false;
			}
			return PlayerControlledNullable.Value;
		}
		set
		{
			PlayerControlledNullable = value;
		}
	}

	[BinarySerialize]
	public bool IgnoreActorCollision
	{
		get
		{
			if (!IgnoreActorCollisionNullable.HasValue)
			{
				return false;
			}
			return IgnoreActorCollisionNullable.Value;
		}
		set
		{
			IgnoreActorCollisionNullable = value;
		}
	}

	[BinarySerialize]
	public bool TreatAsTrap
	{
		get
		{
			if (!TreatAsTrapNullable.HasValue)
			{
				return false;
			}
			return TreatAsTrapNullable.Value;
		}
		set
		{
			TreatAsTrapNullable = value;
		}
	}

	[BinarySerialize]
	public int AdjacentAttackOnDeath
	{
		get
		{
			if (!AdjacentAttackOnDeathNullable.HasValue)
			{
				return 0;
			}
			return AdjacentAttackOnDeathNullable.Value;
		}
		set
		{
			AdjacentAttackOnDeathNullable = value;
		}
	}

	public int GetMoveFromActor(CHeroSummonActor actor)
	{
		if (actor == null)
		{
			return Move;
		}
		IEnumerable<CMoveActiveBonus_BuffMove> enumerable = from x in actor.FindApplicableActiveBonuses(CAbility.EAbilityType.Move, CActiveBonus.EActiveBonusBehaviourType.BuffMove)
			select x.GetBehaviour<CMoveActiveBonus_BuffMove>();
		int num = 0;
		foreach (CMoveActiveBonus_BuffMove item in enumerable)
		{
			if (item != null && item.ApplyToMonsterStats)
			{
				num += item.ReferenceStrength(null, actor);
			}
		}
		return Move + num;
	}

	public int GetAttackFromActor(CHeroSummonActor actor)
	{
		if (actor == null)
		{
			return Attack;
		}
		IEnumerable<CAttackActiveBonus_BuffAttack> enumerable = from x in actor.FindApplicableActiveBonuses(CAbility.EAbilityType.Attack, CActiveBonus.EActiveBonusBehaviourType.BuffAttack)
			select x.GetBehaviour<CAttackActiveBonus_BuffAttack>();
		int num = 0;
		foreach (CAttackActiveBonus_BuffAttack item in enumerable)
		{
			if (item != null && item.ApplyToMonsterStats)
			{
				num += item.ReferenceStrength(null, actor);
			}
		}
		return Attack + num;
	}

	public int GetPierceFromActor(CHeroSummonActor actor)
	{
		if (actor == null)
		{
			return Pierce;
		}
		IEnumerable<CAttackActiveBonus_BuffAttack> enumerable = from x in actor.FindApplicableActiveBonuses(CAbility.EAbilityType.Attack, CActiveBonus.EActiveBonusBehaviourType.BuffAttack)
			select x.GetBehaviour<CAttackActiveBonus_BuffAttack>();
		int num = 0;
		foreach (CAttackActiveBonus_BuffAttack item in enumerable)
		{
			if (item != null && item.ApplyToMonsterStats)
			{
				num += item.ReferencePierce(null, actor);
			}
		}
		return Pierce + num;
	}

	public HeroSummonYMLData()
	{
	}

	public HeroSummonYMLData(string fileName)
	{
		FileName = fileName;
		ID = null;
		LocKey = null;
		Model = null;
		HealthTable = null;
		MoveNullable = null;
		AttackNullable = null;
		RangeNullable = null;
		ShieldNullable = null;
		RetaliateNullable = null;
		PierceNullable = null;
		FlyingNullable = null;
		AttackInfuse = null;
		AttackAnimOverload = string.Empty;
		AttackOnDeathAnimOverload = string.Empty;
		AdjacentAttackOnDeathNullable = null;
		StatIsBasedOnXEntries = null;
		OnAttackConditions = null;
		OnSummonAbilities = null;
		OnDeathAbilities = null;
		SpecialAbilities = null;
		MoveOverride = null;
		AttackOverride = null;
		InvulnerableNullable = null;
		PierceInvulnerabilityNullable = null;
		UntargetableNullable = null;
		DoesNotBlockNullable = null;
		PlayerControlledNullable = null;
		IgnoreActorCollisionNullable = null;
		TreatAsTrapNullable = null;
		CanMoveOnActorClassID = null;
		NullAttackAnimOverload = false;
		NullAttackOnDeathAnimOverload = false;
		NullAdjacentAttackOnDeathNullable = false;
		NullOnAttackConditions = false;
		NullOnSummonAbilities = false;
		NullOnDeathAbilities = false;
		NullSpecialAbilities = false;
		NullMoveOverride = false;
		NullAttackOverride = false;
		NullStatIsBasedOnXEntries = false;
		ColourHTML = "#FFFFFF";
		Fatness = 0f;
		VertexAnimIntensity = 0f;
		CustomConfig = null;
	}

	public HeroSummonYMLData Copy()
	{
		return new HeroSummonYMLData(FileName)
		{
			ID = ID,
			LocKey = LocKey,
			Model = Model,
			HealthTable = HealthTable,
			HealthEnhancements = HealthEnhancements,
			Move = Move,
			MoveEnhancements = MoveEnhancements,
			Attack = Attack,
			AttackEnhancements = AttackEnhancements,
			Range = Range,
			RangeEnhancements = RangeEnhancements,
			Shield = Shield,
			Retaliate = Retaliate,
			RetaliateRange = RetaliateRange,
			Pierce = Pierce,
			Flying = Flying,
			AttackInfuse = AttackInfuse,
			AttackAnimOverload = (AttackAnimOverload ?? string.Empty),
			AttackOnDeathAnimOverload = (AttackOnDeathAnimOverload ?? string.Empty),
			AdjacentAttackOnDeath = AdjacentAttackOnDeath,
			StatIsBasedOnXEntries = StatIsBasedOnXEntries?.Select((AbilityData.StatIsBasedOnXData x) => x.Copy()).ToList(),
			OnAttackConditions = OnAttackConditions,
			OnSummonAbilities = ((OnSummonAbilities != null) ? OnSummonAbilities.Select((CAbility x) => CAbility.CopyAbility(x, generateNewID: false)).ToList() : null),
			OnDeathAbilities = ((OnDeathAbilities != null) ? OnDeathAbilities.Select((CAbility x) => CAbility.CopyAbility(x, generateNewID: false)).ToList() : null),
			SpecialAbilities = ((SpecialAbilities != null) ? SpecialAbilities.Select((CAbility x) => CAbility.CopyAbility(x, generateNewID: false)).ToList() : null),
			MoveOverride = MoveOverride,
			AttackOverride = AttackOverride,
			Fatness = Fatness,
			ColourHTML = ColourHTML,
			VertexAnimIntensity = VertexAnimIntensity,
			CustomConfig = CustomConfig,
			Invulnerable = Invulnerable,
			PierceInvulnerability = PierceInvulnerability,
			Untargetable = Untargetable,
			DoesNotBlock = DoesNotBlock,
			PlayerControlled = PlayerControlled,
			IgnoreActorCollision = IgnoreActorCollision,
			CanMoveOnActorClassID = CanMoveOnActorClassID
		};
	}

	public bool Validate()
	{
		if (LocKey == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "HeroSummon is missing LocKey value. " + FileName);
			return false;
		}
		if (Model == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "HeroSummon is missing Model value. " + FileName);
			return false;
		}
		if (HealthTable == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "HeroSummon is missing HealthTable value. " + FileName);
			return false;
		}
		return true;
	}

	public void UpdateData(HeroSummonYMLData summonData)
	{
		if (summonData.LocKey != null)
		{
			LocKey = summonData.LocKey;
		}
		if (summonData.Model != null)
		{
			Model = summonData.Model;
		}
		if (summonData.HealthTable != null)
		{
			HealthTable = summonData.HealthTable;
		}
		HealthEnhancements = summonData.HealthEnhancements.ToList();
		if (summonData.MoveNullable.HasValue)
		{
			MoveNullable = summonData.MoveNullable;
		}
		MoveEnhancements = summonData.MoveEnhancements.ToList();
		if (summonData.AttackNullable.HasValue)
		{
			AttackNullable = summonData.AttackNullable;
		}
		AttackEnhancements = summonData.AttackEnhancements.ToList();
		if (summonData.RangeNullable.HasValue)
		{
			RangeNullable = summonData.RangeNullable;
		}
		RangeEnhancements = summonData.RangeEnhancements.ToList();
		if (summonData.ShieldNullable.HasValue)
		{
			ShieldNullable = summonData.ShieldNullable;
		}
		if (summonData.RetaliateNullable.HasValue)
		{
			RetaliateNullable = summonData.RetaliateNullable;
		}
		if (summonData.RetaliateRangeNullable.HasValue)
		{
			RetaliateRangeNullable = summonData.RetaliateRangeNullable;
		}
		if (summonData.PierceNullable.HasValue)
		{
			PierceNullable = summonData.PierceNullable;
		}
		if (summonData.FlyingNullable.HasValue)
		{
			FlyingNullable = summonData.FlyingNullable;
		}
		if (summonData.AttackInfuse.HasValue)
		{
			AttackInfuse = summonData.AttackInfuse;
		}
		if (summonData.AttackAnimOverload != null)
		{
			AttackAnimOverload = summonData.AttackAnimOverload;
		}
		if (summonData.AttackOnDeathAnimOverload != null)
		{
			AttackOnDeathAnimOverload = summonData.AttackOnDeathAnimOverload;
		}
		if (summonData.AdjacentAttackOnDeathNullable.HasValue)
		{
			AdjacentAttackOnDeathNullable = summonData.AdjacentAttackOnDeath;
		}
		if (summonData.StatIsBasedOnXEntries != null)
		{
			StatIsBasedOnXEntries = summonData.StatIsBasedOnXEntries;
		}
		if (summonData.OnAttackConditions != null)
		{
			OnAttackConditions = summonData.OnAttackConditions;
		}
		if (summonData.OnSummonAbilities != null)
		{
			OnSummonAbilities = summonData.OnSummonAbilities;
		}
		if (summonData.OnDeathAbilities != null)
		{
			OnDeathAbilities = summonData.OnDeathAbilities;
		}
		if (summonData.SpecialAbilities != null)
		{
			SpecialAbilities = summonData.SpecialAbilities;
		}
		if (summonData.MoveOverride != null)
		{
			MoveOverride = summonData.MoveOverride;
		}
		if (summonData.AttackOverride != null)
		{
			AttackOverride = summonData.AttackOverride;
		}
		if (summonData.InvulnerableNullable.HasValue)
		{
			InvulnerableNullable = summonData.InvulnerableNullable;
		}
		if (summonData.DoesNotBlockNullable.HasValue)
		{
			DoesNotBlockNullable = summonData.DoesNotBlockNullable;
		}
		if (summonData.PierceInvulnerabilityNullable.HasValue)
		{
			PierceInvulnerabilityNullable = summonData.PierceInvulnerabilityNullable;
		}
		if (summonData.UntargetableNullable.HasValue)
		{
			UntargetableNullable = summonData.UntargetableNullable;
		}
		if (summonData.PlayerControlledNullable.HasValue)
		{
			PlayerControlledNullable = summonData.PlayerControlledNullable;
		}
		if (summonData.IgnoreActorCollisionNullable.HasValue)
		{
			IgnoreActorCollisionNullable = summonData.IgnoreActorCollisionNullable;
		}
		if (summonData.TreatAsTrapNullable.HasValue)
		{
			TreatAsTrapNullable = summonData.TreatAsTrapNullable;
		}
		if (summonData.CanMoveOnActorClassID != null)
		{
			CanMoveOnActorClassID = summonData.CanMoveOnActorClassID;
		}
		if (NullAttackAnimOverload)
		{
			AttackAnimOverload = string.Empty;
		}
		if (NullAttackOnDeathAnimOverload)
		{
			AttackOnDeathAnimOverload = string.Empty;
		}
		if (NullAdjacentAttackOnDeathNullable)
		{
			AdjacentAttackOnDeathNullable = null;
		}
		if (NullOnAttackConditions)
		{
			OnAttackConditions = null;
		}
		if (NullOnSummonAbilities)
		{
			OnSummonAbilities = null;
		}
		if (NullOnDeathAbilities)
		{
			OnDeathAbilities = null;
		}
		if (NullSpecialAbilities)
		{
			SpecialAbilities = null;
		}
		if (NullMoveOverride)
		{
			MoveOverride = null;
		}
		if (NullAttackOverride)
		{
			AttackOverride = null;
		}
		if (NullStatIsBasedOnXEntries)
		{
			StatIsBasedOnXEntries = null;
		}
		if (summonData.ColourHTML != "#FFFFFF")
		{
			ColourHTML = summonData.ColourHTML;
		}
		if (summonData.Fatness != 0f)
		{
			Fatness = summonData.Fatness;
		}
		if (summonData.VertexAnimIntensity != 0f)
		{
			VertexAnimIntensity = summonData.VertexAnimIntensity;
		}
		if (summonData.CustomConfig != null)
		{
			CustomConfig = summonData.CustomConfig;
		}
	}

	protected HeroSummonYMLData(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public int GetHealth(int level, float baseHealthModifier = 1f)
	{
		return (int)Math.Floor(baseHealthModifier * (float)HealthTable[level]);
	}
}
