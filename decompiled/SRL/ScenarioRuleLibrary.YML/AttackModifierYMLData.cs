using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SharedLibrary.Client;
using SharedLibrary.YML;

namespace ScenarioRuleLibrary.YML;

public class AttackModifierYMLData
{
	public const string BlessName = "Bless";

	public const string CurseName = "Curse";

	public const string ReducedRandomBlessName = "Reduced_Randomness_Bless";

	public const string ReducedRandomCurseName = "Reduced_Randomness_Curse";

	private int? m_ID;

	[NonSerialized]
	[JsonIgnore]
	[StateDump(Skip = true)]
	public bool FinishedBeingDrawn;

	public AttackModifierCard ModifiedFromModifier;

	private AttackModifierCard m_Card;

	public int ID
	{
		get
		{
			if (!m_ID.HasValue)
			{
				m_ID = YMLShared.GetDeterministicHashCode(Name);
			}
			return m_ID.Value;
		}
	}

	public CAbilityFilterContainer AlternateFilter { get; set; }

	public string Name { get; set; }

	public string MathModifier { get; set; }

	public string AlternateMathModifier { get; set; }

	public string DescriptionForToggleAlternate { get; set; }

	public string OriginalModifier { get; set; }

	public bool? ShuffleNullable { get; set; }

	public bool? RollingNullable { get; set; }

	public bool? AddTargetNullable { get; set; }

	public List<CAbility> Abilities { get; set; }

	public List<CAbilityOverride> Overrides { get; set; }

	public bool? IsCurseNullable { get; set; }

	public bool? IsBlessNullable { get; set; }

	public bool? IsConditionalModifierNullable { get; set; }

	public bool NullAbilities { get; set; }

	public bool NullOverrides { get; set; }

	public bool UseGenericIcon { get; set; }

	public int? DamageIfTargetMoves { get; set; }

	public bool TryToPerformAsSummoner { get; set; }

	public bool ControlTargetingActor { get; set; }

	public string FileName { get; private set; }

	public bool Shuffle
	{
		get
		{
			if (!ShuffleNullable.HasValue)
			{
				return false;
			}
			return ShuffleNullable.Value;
		}
		set
		{
			ShuffleNullable = value;
		}
	}

	public bool Rolling
	{
		get
		{
			if (!RollingNullable.HasValue)
			{
				return false;
			}
			return RollingNullable.Value;
		}
		set
		{
			RollingNullable = value;
		}
	}

	public bool AddTarget
	{
		get
		{
			if (!AddTargetNullable.HasValue)
			{
				return false;
			}
			return AddTargetNullable.Value;
		}
		set
		{
			AddTargetNullable = value;
		}
	}

	public bool IsCurse
	{
		get
		{
			if (!IsCurseNullable.HasValue)
			{
				return false;
			}
			return IsCurseNullable.Value;
		}
		set
		{
			IsCurseNullable = value;
		}
	}

	public bool IsBless
	{
		get
		{
			if (!IsBlessNullable.HasValue)
			{
				return false;
			}
			return IsBlessNullable.Value;
		}
		set
		{
			IsBlessNullable = value;
		}
	}

	public bool IsConditionalModifier
	{
		get
		{
			if (!IsConditionalModifierNullable.HasValue)
			{
				return false;
			}
			return IsConditionalModifierNullable.Value;
		}
		set
		{
			IsConditionalModifierNullable = value;
		}
	}

	public bool IsPersistent => Abilities.Any((CAbility x) => x.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.Persistent);

	public string CustomCombatLogDescriptionLocKey { get; set; }

	public AttackModifierCard Card
	{
		get
		{
			if (m_Card == null)
			{
				m_Card = new AttackModifierCard(Name);
			}
			return m_Card;
		}
	}

	public static bool TryParseMathModifier(string mathModifier, out char operatorSymbol, out int mathVal)
	{
		if (mathModifier.Length <= 1)
		{
			operatorSymbol = '\0';
			mathVal = 0;
			return false;
		}
		operatorSymbol = mathModifier[0];
		if (int.TryParse(mathModifier.Substring(1), out mathVal))
		{
			return true;
		}
		return false;
	}

	public AttackModifierYMLData(string fileName)
	{
		FileName = fileName;
		Name = null;
		MathModifier = null;
		ShuffleNullable = null;
		RollingNullable = null;
		AddTargetNullable = null;
		Abilities = new List<CAbility>();
		Overrides = new List<CAbilityOverride>();
		IsConditionalModifierNullable = null;
		IsBlessNullable = null;
		IsCurseNullable = null;
		NullAbilities = false;
		NullOverrides = false;
		UseGenericIcon = false;
		DamageIfTargetMoves = null;
		TryToPerformAsSummoner = false;
		ControlTargetingActor = false;
		CustomCombatLogDescriptionLocKey = null;
	}

	public AttackModifierYMLData(string name, string mathModifier, bool isCurse, bool isBless)
	{
		FileName = "Generated In Code";
		Name = name;
		MathModifier = mathModifier;
		IsCurse = isCurse;
		IsBless = isBless;
		IsConditionalModifier = true;
		Shuffle = false;
		Rolling = false;
		AddTarget = false;
		Abilities = new List<CAbility>();
		Overrides = new List<CAbilityOverride>();
		UseGenericIcon = false;
		DamageIfTargetMoves = null;
		TryToPerformAsSummoner = false;
		ControlTargetingActor = false;
		CustomCombatLogDescriptionLocKey = null;
	}

	public AttackModifierYMLData(string name, string mathModifier)
	{
		FileName = "Generated In Code";
		Name = name;
		MathModifier = mathModifier;
		IsCurse = false;
		IsBless = false;
		IsConditionalModifier = true;
		Shuffle = false;
		Rolling = false;
		AddTarget = false;
		Abilities = new List<CAbility>();
		Overrides = new List<CAbilityOverride>();
		UseGenericIcon = false;
		DamageIfTargetMoves = null;
		TryToPerformAsSummoner = false;
		ControlTargetingActor = false;
		CustomCombatLogDescriptionLocKey = null;
	}

	public AttackModifierYMLData Copy(bool createCard = false)
	{
		AttackModifierYMLData attackModifierYMLData = new AttackModifierYMLData(FileName)
		{
			Name = Name,
			MathModifier = MathModifier,
			AlternateMathModifier = AlternateMathModifier,
			OriginalModifier = OriginalModifier,
			Shuffle = Shuffle,
			Rolling = Rolling,
			AddTarget = AddTarget,
			Abilities = Abilities,
			Overrides = Overrides?.Select((CAbilityOverride x) => CAbilityOverride.CopyAbilityOverride(x))?.ToList(),
			IsCurse = IsCurse,
			IsBless = IsBless,
			IsConditionalModifier = IsConditionalModifier,
			UseGenericIcon = UseGenericIcon,
			DescriptionForToggleAlternate = DescriptionForToggleAlternate,
			AlternateFilter = AlternateFilter?.Copy(),
			DamageIfTargetMoves = DamageIfTargetMoves,
			TryToPerformAsSummoner = TryToPerformAsSummoner,
			ControlTargetingActor = ControlTargetingActor,
			CustomCombatLogDescriptionLocKey = CustomCombatLogDescriptionLocKey
		};
		if (createCard)
		{
			attackModifierYMLData.m_Card = new AttackModifierCard(Name);
			attackModifierYMLData.m_Card.YMLData = attackModifierYMLData;
		}
		return attackModifierYMLData;
	}

	public bool Validate()
	{
		bool result = true;
		if (Name == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Name specified for atack modifier card in file " + FileName);
			result = false;
		}
		if (MathModifier == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No MathModifier specified for atack modifier card in file " + FileName);
			result = false;
		}
		if (IsBless && Name != "Bless" && Name != "Reduced_Randomness_Bless")
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "The Attack Modifier Card with IsBless set to True must be named 'Bless' or 'Reduced_Randomness_Bless'.  File: " + FileName);
			result = false;
		}
		if (IsCurse && Name != "Curse" && Name != "Reduced_Randomness_Curse")
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "The Attack Modifier Card with IsCurse set to True must be named 'Curse' or 'Reduced_Randomness_Curse'.  File: " + FileName);
			result = false;
		}
		return result;
	}

	public void UpdateData(AttackModifierYMLData data)
	{
		if (data.MathModifier != null)
		{
			MathModifier = data.MathModifier;
		}
		if (data.ShuffleNullable.HasValue)
		{
			ShuffleNullable = data.ShuffleNullable;
		}
		if (data.RollingNullable.HasValue)
		{
			RollingNullable = data.RollingNullable;
		}
		if (data.AddTargetNullable.HasValue)
		{
			AddTargetNullable = data.AddTargetNullable;
		}
		if (data.Abilities != null)
		{
			Abilities = data.Abilities;
		}
		if (data.Overrides != null)
		{
			Overrides = data.Overrides;
		}
		if (data.IsConditionalModifierNullable.HasValue)
		{
			IsConditionalModifierNullable = data.IsConditionalModifierNullable;
		}
		if (data.IsBlessNullable.HasValue)
		{
			IsBlessNullable = data.IsBlessNullable;
		}
		if (data.IsCurseNullable.HasValue)
		{
			IsCurseNullable = data.IsCurseNullable;
		}
		if (NullAbilities)
		{
			Abilities = null;
		}
		if (NullOverrides)
		{
			Overrides = null;
		}
		if (data.DamageIfTargetMoves.HasValue)
		{
			DamageIfTargetMoves = data.DamageIfTargetMoves;
		}
		if (data.CustomCombatLogDescriptionLocKey != null)
		{
			CustomCombatLogDescriptionLocKey = data.CustomCombatLogDescriptionLocKey;
		}
		TryToPerformAsSummoner = data.TryToPerformAsSummoner;
		ControlTargetingActor = data.ControlTargetingActor;
		UseGenericIcon = data.UseGenericIcon;
	}

	public override string ToString()
	{
		return Name;
	}
}
