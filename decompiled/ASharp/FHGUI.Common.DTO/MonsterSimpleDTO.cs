using GLOOM;
using ScenarioRuleLibrary;

namespace FHGUI.Common.DTO;

public struct MonsterSimpleDTO
{
	public string ID;

	public string Name;

	public BaseStatsDTO Stats;

	public MonsterSimpleDTO(CEnemyActor monster)
	{
		ID = monster.MonsterClass.ID;
		Name = UICharacterUtils.GetEnemyName(monster);
		Stats = new BaseStatsDTO(monster.MonsterClass);
	}

	public MonsterSimpleDTO(CMonsterClass monster)
	{
		ID = monster.ID;
		Name = LocalizationManager.GetTranslation(monster.LocKey);
		Stats = new BaseStatsDTO(monster);
	}
}
