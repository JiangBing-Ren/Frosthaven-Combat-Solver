using System.Linq;

namespace ScenarioRuleLibrary;

[StateDump(UseToString = true, NoMembers = true)]
public class CBaseAbilityCard : CBaseCard
{
	public int Initiative { get; private set; }

	public string ClassID { get; private set; }

	public string ClassModel
	{
		get
		{
			switch (CardType)
			{
			case ECardType.CharacterAbility:
				return CharacterClassManager.Classes.Single((CCharacterClass s) => s.CharacterID == ClassID).AbilityCardArt;
			case ECardType.HeroSummonAbility:
				return CharacterClassManager.HeroSummonClasses.Single((CHeroSummonClass s) => s.ID == ClassID).AbilityCardArt;
			case ECardType.MonsterAbility:
				return MonsterClassManager.MonsterAndObjectClassesWithInitiative.Single((CMonsterClass s) => s.ID == ClassID).AbilityCardArt;
			default:
				Log.ErrorWithReport("Invalid Card Type for CBaseAbilityCard " + CardType);
				return null;
			}
		}
	}

	public string ClassCharacterConfig
	{
		get
		{
			switch (CardType)
			{
			case ECardType.CharacterAbility:
				return CharacterClassManager.Classes.Single((CCharacterClass s) => s.CharacterID == ClassID).CharacterYML.CustomCharacterConfig;
			case ECardType.HeroSummonAbility:
			case ECardType.MonsterAbility:
				return null;
			default:
				Log.ErrorWithReport("Invalid Card Type for CBaseAbilityCard " + CardType);
				return null;
			}
		}
	}

	public CBaseAbilityCard(int initiative, int id, string classID, ECardType cardType, string stringID)
		: base(id, cardType, stringID)
	{
		Initiative = initiative;
		ClassID = classID;
		ActionHasHappened = false;
	}
}
