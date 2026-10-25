using System;
using System.Collections.Concurrent;
using ScenarioRuleLibrary.YML;

public class CSRLYMLModeData
{
	public HeroSummonYML HeroSummons { get; private set; }

	public CharacterYML Characters { get; private set; }

	public AbilityCardYML AbilityCards { get; private set; }

	public ItemCardYML ItemCards { get; private set; }

	public ItemCardYML PetCards { get; private set; }

	public PerksYML Perks { get; private set; }

	public EnhancementYML Enhancements { get; private set; }

	public IconGlossaryYML IconGlossary { get; private set; }

	public AttackModifiersYML AttackModifiers { get; private set; }

	public AttackModifierDecksYML AttackModifierDecks { get; private set; }

	public MonsterCardYML MonsterCards { get; private set; }

	public MonstersYML Monsters { get; private set; }

	public TreasureTablesYML TreasureTables { get; private set; }

	public ScenarioYML Scenarios { get; private set; }

	public MonsterDataYML MonsterData { get; private set; }

	public ScenarioRoomsYML ScenarioRooms { get; private set; }

	public ScenarioAbilitiesYML ScenarioAbilities { get; private set; }

	public CharacterResourceYML CharacterResources { get; private set; }

	public ItemConfigYML ItemConfigs { get; private set; }

	public AbilityCardConfigYML AbilityCardConfigs { get; private set; }

	public CharacterConfigYML CharacterConfigs { get; private set; }

	public MonsterConfigYML MonsterConfigs { get; private set; }

	public MapConfigYML MapConfigs { get; private set; }

	public ConcurrentQueue<Tuple<string, byte[]>> CustomLevels { get; private set; }

	public CSRLYMLModeData()
	{
		HeroSummons = new HeroSummonYML();
		Characters = new CharacterYML();
		AbilityCards = new AbilityCardYML();
		ItemCards = new ItemCardYML();
		PetCards = new ItemCardYML();
		Perks = new PerksYML();
		Enhancements = new EnhancementYML();
		IconGlossary = new IconGlossaryYML();
		AttackModifiers = new AttackModifiersYML();
		AttackModifierDecks = new AttackModifierDecksYML();
		MonsterCards = new MonsterCardYML();
		Monsters = new MonstersYML();
		TreasureTables = new TreasureTablesYML();
		Scenarios = new ScenarioYML();
		MonsterData = new MonsterDataYML();
		ScenarioRooms = new ScenarioRoomsYML();
		ScenarioAbilities = new ScenarioAbilitiesYML();
		CharacterResources = new CharacterResourceYML();
		ItemConfigs = new ItemConfigYML();
		AbilityCardConfigs = new AbilityCardConfigYML();
		CharacterConfigs = new CharacterConfigYML();
		MonsterConfigs = new MonsterConfigYML();
		MapConfigs = new MapConfigYML();
		CustomLevels = new ConcurrentQueue<Tuple<string, byte[]>>();
	}
}
