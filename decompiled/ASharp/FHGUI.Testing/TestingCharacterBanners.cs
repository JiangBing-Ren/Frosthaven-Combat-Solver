using System.Collections.Generic;
using System.Linq;
using FH.YML;
using FHGUI.DTO;
using FHRL;
using UnityEngine;

namespace FHGUI.Testing;

public class TestingCharacterBanners : MonoBehaviour
{
	public Transform ContainerGroup;

	public TestingBannersElement TemplateBannersElement;

	private List<MonsterDataDto> _allMonsters;

	private Dictionary<int, int> _normalToEliteIndexes = new Dictionary<int, int>();

	private List<TestingBannersElement> _bannersElements;

	private void Awake()
	{
		_bannersElements = ContainerGroup.GetComponentsInChildren<TestingBannersElement>().ToList();
	}

	private void Start()
	{
		_allMonsters = YMLDefRepository.Repo.DefRepository.Scenarios.Values.SelectMany((ScenarioDef s) => s.Levels[0].PartySettings.SelectMany((ScenarioDef.PartySetting ps) => ps.Monsters)).Distinct().Select(MonsterDataDto.GenerateMonsterDataDto)
			.ToList();
		foreach (MonsterDataDto normalMonster in _allMonsters.Where((MonsterDataDto m) => !m.IsElite))
		{
			string commonId = normalMonster.Id;
			if (commonId.EndsWith("ID"))
			{
				commonId = commonId.Remove(commonId.Length - 2);
			}
			int key = _allMonsters.IndexOf(normalMonster);
			int value = _allMonsters.FindIndex((MonsterDataDto m) => m.Id.StartsWith(commonId) && m.IsElite && m.IsAlly == normalMonster.IsAlly && m.IsSecondFaction == normalMonster.IsSecondFaction);
			_normalToEliteIndexes.Add(key, value);
		}
		List<KeyValuePair<int, int>> list = _normalToEliteIndexes.OrderBy((KeyValuePair<int, int> pair) => _allMonsters[pair.Key].NameLocalised).ToList();
		HelperTools.NormalizePool(ref _bannersElements, TemplateBannersElement.gameObject, ContainerGroup, list.Count);
		for (int num = 0; num < list.Count; num++)
		{
			MonsterDataDto normalDto = _allMonsters[list[num].Key];
			MonsterDataDto eliteDto = ((list[num].Value < 0) ? null : _allMonsters[list[num].Value]);
			_bannersElements[num].Init(normalDto, eliteDto);
		}
	}
}
