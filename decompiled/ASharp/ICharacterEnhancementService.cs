using System.Collections.Generic;
using ScenarioRuleLibrary;

public interface ICharacterEnhancementService : ICharacter
{
	int Gold { get; }

	bool HasFreeEnhancementSlots();

	int GetPaidPriceEnhancement(EnhancementButtonBase enhancementButton);

	int CalculateSellPriceEnhancement(EnhancementButtonBase enhancementButton);

	float GetEnhancementSellPricePercentage();

	int GetFreeEnhancementSlots();

	List<CAbilityCard> GetOwnedAbilityCards();

	bool IsCardEnhanced(int abilityCardID);

	int CountEnhancements(int abilityCardID);
}
