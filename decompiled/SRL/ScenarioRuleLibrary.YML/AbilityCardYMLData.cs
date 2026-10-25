using System;
using System.Collections.Generic;
using System.Linq;
using SharedLibrary.Client;

namespace ScenarioRuleLibrary.YML;

public class AbilityCardYMLData
{
	public string Name { get; set; }

	public string CharacterID { get; set; }

	public int ID { get; set; }

	public string Level { get; set; }

	public int Initiative { get; set; }

	public int ProgressionOrder { get; set; }

	public bool? SupplyCard { get; set; }

	public string FileName { get; private set; }

	public string Prefab { get; set; }

	public string CustomImage { get; set; }

	public string MiniCardImage { get; set; }

	public CPlayerActor.EForm RequiredForm { get; set; }

	public DiscardType TopDiscardType { get; set; }

	public bool TopIsLost
	{
		get
		{
			if (TopDiscardType != DiscardType.Lost)
			{
				return TopDiscardType == DiscardType.PermanentlyLost;
			}
			return true;
		}
	}

	public bool TopIsPermLost => TopDiscardType == DiscardType.PermanentlyLost;

	public CAction TopActionCardData { get; set; }

	public CardLayout TopActionFullLayout { get; set; }

	public DiscardType BottomDiscardType { get; set; }

	public bool BottomIsLost
	{
		get
		{
			if (BottomDiscardType != DiscardType.Lost)
			{
				return BottomDiscardType == DiscardType.PermanentlyLost;
			}
			return true;
		}
	}

	public bool BottomIsPermLost => BottomDiscardType == DiscardType.PermanentlyLost;

	public CAction BottomActionCardData { get; set; }

	public CardLayout BottomActionFullLayout { get; set; }

	public List<AbilityConsume> TopConsumes { get; private set; }

	public List<AbilityConsume> BottomConsumes { get; private set; }

	public ECharacter Art
	{
		get
		{
			CharacterYMLData characterYMLData = ScenarioRuleClient.SRLYML.Characters.SingleOrDefault((CharacterYMLData s) => s.ID == CharacterID);
			if (characterYMLData != null)
			{
				if (characterYMLData.OverrideAbilityCardArt != null && Enum.TryParse<ECharacter>(characterYMLData.OverrideAbilityCardArt, out var result))
				{
					return result;
				}
				return characterYMLData.Model;
			}
			return ECharacter.None;
		}
	}

	public AbilityConsume FindConsumeByName(string name)
	{
		return GetConsumes().FirstOrDefault((AbilityConsume consume) => consume.Name == name);
	}

	public IEnumerable<AbilityConsume> GetConsumes()
	{
		return TopConsumes.Concat(BottomConsumes).Concat(CAbilityCard.GetActionAbilities(TopActionCardData).OfType<CAbilityConsume>().SelectMany((CAbilityConsume ability) => ability.Consumes)).Concat(CAbilityCard.GetActionAbilities(BottomActionCardData).OfType<CAbilityConsume>().SelectMany((CAbilityConsume ability) => ability.Consumes));
	}

	public AbilityCardYMLData(string filename)
	{
		FileName = filename;
		TopConsumes = new List<AbilityConsume>();
		BottomConsumes = new List<AbilityConsume>();
		Name = null;
		CharacterID = null;
		ID = int.MaxValue;
		Level = null;
		SupplyCard = false;
		Initiative = int.MaxValue;
		ProgressionOrder = -1;
		TopActionCardData = null;
		TopActionFullLayout = null;
		TopDiscardType = DiscardType.None;
		BottomActionCardData = null;
		BottomActionFullLayout = null;
		BottomDiscardType = DiscardType.None;
		Prefab = null;
		CustomImage = null;
		MiniCardImage = null;
	}

	public bool Validate()
	{
		if (Name == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Name specified for ability card in file " + FileName);
			return false;
		}
		if (CharacterID == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Character specified for ability card in file " + FileName);
			return false;
		}
		if (ID == int.MaxValue)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No ID specified for ability card in file " + FileName);
			return false;
		}
		if (Level == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Level specified for ability card in file " + FileName);
			return false;
		}
		if (!SupplyCard.HasValue)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No SupplyCard specified for ability card in file " + FileName);
			return false;
		}
		if (Initiative == int.MaxValue)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Initiative specified for ability card in file " + FileName);
			return false;
		}
		if (ID != -1 && TopActionCardData == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid TopActionCardData for ability card in file " + FileName);
			return false;
		}
		if (ID != -1 && TopActionFullLayout == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid TopActionFullLayout for ability card in file " + FileName);
			return false;
		}
		if (ID != -1 && !TopActionCardData.Validate())
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid data found in TopActionCardData, check Action and Abilities for ability card in file " + FileName);
			return false;
		}
		if (ID != -1 && BottomActionCardData == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid BottomActionCardData for ability card in file " + FileName);
			return false;
		}
		if (ID != -1 && BottomActionFullLayout == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid BottomActionFullLayout for ability card in file " + FileName);
			return false;
		}
		if (ID != -1 && !BottomActionCardData.Validate())
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid data found in BottomActionCardData, check Action and Abilities for ability card in file " + FileName);
			return false;
		}
		return true;
	}

	public void UpdateData(AbilityCardYMLData cardData)
	{
		if (cardData.Name != null)
		{
			Name = cardData.Name;
		}
		if (cardData.CharacterID != null)
		{
			CharacterID = cardData.CharacterID;
		}
		if (cardData.Level != null)
		{
			Level = cardData.Level;
		}
		if (cardData.SupplyCard.HasValue)
		{
			SupplyCard = cardData.SupplyCard.Value;
		}
		if (cardData.Initiative != int.MaxValue)
		{
			Initiative = cardData.Initiative;
		}
		if (cardData.ProgressionOrder != -1)
		{
			ProgressionOrder = cardData.ProgressionOrder;
		}
		if (cardData.TopActionCardData != null)
		{
			TopActionCardData = cardData.TopActionCardData;
			TopConsumes = cardData.TopConsumes;
		}
		if (cardData.TopDiscardType != DiscardType.None)
		{
			TopDiscardType = cardData.TopDiscardType;
		}
		if (cardData.TopActionFullLayout != null)
		{
			TopActionFullLayout = cardData.TopActionFullLayout;
		}
		if (cardData.BottomActionCardData != null)
		{
			BottomActionCardData = cardData.BottomActionCardData;
			BottomConsumes = cardData.BottomConsumes;
		}
		if (cardData.BottomDiscardType != DiscardType.None)
		{
			BottomDiscardType = cardData.BottomDiscardType;
		}
		if (cardData.BottomActionFullLayout != null)
		{
			BottomActionFullLayout = cardData.BottomActionFullLayout;
		}
		if (cardData.Prefab != null)
		{
			Prefab = cardData.Prefab;
		}
		CustomImage = cardData.CustomImage;
		MiniCardImage = cardData.MiniCardImage;
	}
}
