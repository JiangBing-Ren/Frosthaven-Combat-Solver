using System.Collections.Generic;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;

public interface ICharacterService : ICharacterEnhancementService, ICharacter
{
	int Level { get; }

	List<int> HandAbilityCardIDs { get; }

	List<int> OwnedAbilityCardIDs { get; }

	int PerkPoints { get; }

	List<CharacterPerk> Perks { get; }

	int PerkChecks { get; }

	int PerkChecksToComplete { get; }

	List<AttackModifierYMLData> AttackModifierDeck { get; }

	List<CItem> EquippedItems { get; }

	ECharacter CharacterModel { get; }

	bool CanLevelup();

	int GetLevelsToLevelUp();

	void GainCard(CAbilityCard card);

	List<CAbilityCard> GetUnownedAbilityCards(int i, int i1);

	void AddCallbackOnPerkPointsChanged(BasicEventHandler callback);

	void RemoveCallbackOnPerkPointsChanged(BasicEventHandler callback);

	void ModifyGold(int gold, bool useGoldModifier = false);

	void AddCallbackOnGoldChanged(IGoldEventListener callback);

	void RemoveCallbackOnGoldChanged(IGoldEventListener callback);

	void AddCallbackOnLevelUpAvailable(BasicEventHandler callback);

	void UpdatePerkPoints(int points);

	int CalculatePriceResetLevel();

	void ResetLevels();

	bool HasFreeLevelReset();

	bool AddNewFreeResetLevel(string notificationId, int notificationOrder);

	void UseFreeResetLevel();

	bool IsNewPerk(AttackModifierYMLData perk);

	bool LevelUp();
}
