using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using SharedLibrary.Client;

namespace ScenarioRuleLibrary.YML;

[Serializable]
public class MonsterYMLData : BinarySerializableYMLData
{
	public static readonly EMonsterBaseStats[] MonsterBaseStatsEnums = (EMonsterBaseStats[])Enum.GetValues(typeof(EMonsterBaseStats));

	public static readonly EMonsterType[] MonsterTypes = (EMonsterType[])Enum.GetValues(typeof(EMonsterType));

	public const int NumberOfBaseStatLevels = 8;

	[BinarySerialize]
	public string ID { get; set; }

	[BinarySerialize]
	public string LocKey { get; set; }

	[BinarySerialize]
	public List<string> Models { get; set; }

	[BinarySerialize]
	public EMonsterType MonsterType { get; set; }

	[BinarySerialize]
	public EMonsterFlag Flags { get; set; }

	[BinarySerialize]
	public string NonEliteVariant { get; set; }

	[BinarySerialize]
	public string CardsPoolClass { get; set; }

	[BinarySerialize]
	public int? StandeeLimit { get; set; }

	[BinarySerialize]
	public bool IgnoreCardPoolStandeeLimit { get; set; }

	[BinarySerialize]
	public bool MiniBoss { get; set; }

	public bool? PredominantlyMeleeNullable { get; set; }

	public List<MonsterCardYMLData> AbilityCards { get; set; }

	[BinarySerialize]
	public List<EMonsterBaseStats> AddNumberOfPlayersTo { get; set; }

	public List<BaseStats> MonsterBaseStats { get; set; }

	[BinarySerialize]
	public string MonsterClassIDToActImmediatelyBefore { get; set; }

	[BinarySerialize]
	public string ColourHTML { get; set; }

	public float Fatness { get; set; }

	[BinarySerialize]
	public float VertexAnimIntensity { get; set; }

	[BinarySerialize]
	public bool UseNormalSizeAvatarForBoss { get; set; }

	[BinarySerialize]
	public string CustomConfig { get; set; }

	[BinarySerialize]
	public int? FocusingInitiative { get; set; }

	[BinarySerialize]
	public bool PredominantlyMelee
	{
		get
		{
			if (!PredominantlyMeleeNullable.HasValue)
			{
				return false;
			}
			return PredominantlyMeleeNullable.Value;
		}
		set
		{
			PredominantlyMeleeNullable = value;
		}
	}

	[BinarySerialize]
	public bool AlwaysShowInnateImmunities { get; set; }

	[BinarySerialize]
	public bool TreatAsNonMonster { get; set; }

	public List<string> OnlyValidInLevels { get; set; }

	[BinarySerialize]
	public List<string> CanMoveOnActorClassID { get; set; }

	public MonsterYMLData()
	{
	}

	protected MonsterYMLData(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public MonsterYMLData(string fileName)
	{
		FileName = fileName;
		ID = null;
		LocKey = null;
		Models = null;
		MonsterType = EMonsterType.None;
		NonEliteVariant = null;
		CardsPoolClass = null;
		StandeeLimit = null;
		PredominantlyMeleeNullable = null;
		AlwaysShowInnateImmunities = false;
		AbilityCards = null;
		AddNumberOfPlayersTo = null;
		MonsterBaseStats = null;
		MonsterClassIDToActImmediatelyBefore = null;
		ColourHTML = "#FFFFFF";
		Fatness = 0f;
		VertexAnimIntensity = 0f;
		UseNormalSizeAvatarForBoss = false;
		CustomConfig = null;
		FocusingInitiative = null;
		OnlyValidInLevels = null;
		CanMoveOnActorClassID = null;
	}

	public bool Validate()
	{
		if (ID == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No ID specified for monster in file " + FileName);
			return false;
		}
		if (LocKey == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No LocKey specified for monster in file " + FileName);
			return false;
		}
		if (Models == null || Models.Count <= 0)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Model specified for monster in file " + FileName);
			return false;
		}
		if (MonsterBaseStats == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No MonsterBaseStats specified for monster in file " + FileName);
			return false;
		}
		if (MonsterBaseStats.Count != 8)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No MonsterBaseStats does not have " + 8 + " levels specified." + FileName);
			return false;
		}
		if (MonsterType == EMonsterType.None)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No MonsterType specified for monster in file " + FileName);
			return false;
		}
		if (!StandeeLimit.HasValue)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No StandeeLimit specified for monster in file " + FileName);
			return false;
		}
		if (MonsterType != EMonsterType.Object && !PredominantlyMeleeNullable.HasValue)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No PredominantlyMelee value specified for monster in file " + FileName);
			return false;
		}
		return true;
	}

	public void UpdateData(MonsterYMLData data)
	{
		if (data.LocKey != null)
		{
			LocKey = data.LocKey;
		}
		if (data.Models != null)
		{
			Models = data.Models;
		}
		if (data.MonsterType != EMonsterType.None)
		{
			MonsterType = data.MonsterType;
		}
		if (data.NonEliteVariant != null)
		{
			NonEliteVariant = data.NonEliteVariant;
		}
		if (data.CardsPoolClass != null)
		{
			CardsPoolClass = data.CardsPoolClass;
		}
		if (data.PredominantlyMeleeNullable.HasValue)
		{
			PredominantlyMeleeNullable = data.PredominantlyMeleeNullable;
		}
		AlwaysShowInnateImmunities = data.AlwaysShowInnateImmunities;
		if (data.AbilityCards != null)
		{
			AbilityCards = data.AbilityCards;
		}
		if (data.AddNumberOfPlayersTo != null)
		{
			AddNumberOfPlayersTo = data.AddNumberOfPlayersTo;
		}
		if (data.MonsterBaseStats != null)
		{
			MonsterBaseStats = data.MonsterBaseStats;
		}
		if (data.StandeeLimit.HasValue)
		{
			StandeeLimit = data.StandeeLimit;
		}
		IgnoreCardPoolStandeeLimit = data.IgnoreCardPoolStandeeLimit;
		MiniBoss = data.MiniBoss;
		TreatAsNonMonster = data.TreatAsNonMonster;
		if (data.ColourHTML != "#FFFFFF")
		{
			ColourHTML = data.ColourHTML;
		}
		if (data.Fatness != 0f)
		{
			Fatness = data.Fatness;
		}
		if (data.VertexAnimIntensity != 0f)
		{
			VertexAnimIntensity = data.VertexAnimIntensity;
		}
		if (data.CustomConfig != null)
		{
			CustomConfig = data.CustomConfig;
		}
		if (data.FocusingInitiative.HasValue)
		{
			FocusingInitiative = data.FocusingInitiative;
		}
		if (data.OnlyValidInLevels != null)
		{
			OnlyValidInLevels = data.OnlyValidInLevels.ToList();
		}
		if (data.CanMoveOnActorClassID != null)
		{
			CanMoveOnActorClassID = data.CanMoveOnActorClassID.ToList();
		}
		UseNormalSizeAvatarForBoss = data.UseNormalSizeAvatarForBoss;
	}

	public bool VerifyValidForLevel(string levelName)
	{
		if (OnlyValidInLevels == null)
		{
			return true;
		}
		return OnlyValidInLevels.Contains(levelName);
	}
}
