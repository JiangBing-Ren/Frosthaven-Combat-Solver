using FH.YML;
using FHRL;
using GLOOM;
using UnityEngine;

namespace FHGUI.DTO;

public class MonsterDataDto
{
	public string Id;

	public string NameLocalised;

	public Sprite Portrait;

	public bool IsFullPortrait;

	public bool IsElite;

	public bool IsBoss;

	public bool IsMiniBoss;

	public bool IsAlly;

	public bool IsSecondFaction;

	public static MonsterDataDto GenerateMonsterDataDto(ScenarioDef.Monster monster)
	{
		Sprite monsterPortrait = UIInfoTools.GetMonsterPortrait(monster.ID.ID, monster.Model, out var isFromConfig);
		string translation = LocalizationManager.GetTranslation(YMLDefRepository.Repo.DefRepository.Monsters[monster.ID].Name);
		return new MonsterDataDto
		{
			Id = monster.ID.ID,
			Portrait = monsterPortrait,
			IsFullPortrait = isFromConfig,
			IsElite = monster.IsElite,
			IsAlly = monster.IsAlly,
			IsBoss = monster.IsBoss,
			IsMiniBoss = monster.IsMiniBoss,
			IsSecondFaction = monster.IsSecondFaction,
			NameLocalised = translation
		};
	}
}
