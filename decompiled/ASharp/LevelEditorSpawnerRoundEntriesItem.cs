using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class LevelEditorSpawnerRoundEntriesItem : MonoBehaviour
{
	public TMP_Dropdown EnemyDropDown1;

	public TMP_Dropdown EnemyDropDown2;

	public TMP_Dropdown EnemyDropDown3;

	public TMP_Dropdown EnemyDropDown4;

	public UnityAction<LevelEditorSpawnerRoundEntriesItem> DeleteButtonPressedAction;

	private TMP_Dropdown[] _enemyDropDownList;

	public SpawnRoundEntry SpawnRoundEntry { get; private set; }

	private TMP_Dropdown[] EnemyDropDownList => _enemyDropDownList ?? new TMP_Dropdown[4] { EnemyDropDown1, EnemyDropDown2, EnemyDropDown3, EnemyDropDown4 };

	public void SetupUI(SpawnRoundEntry item)
	{
		SpawnRoundEntry = item;
		List<string> allMonsters = MonsterClassManager.Classes.Select((CMonsterClass s) => s.ID).ToList();
		allMonsters.Sort();
		allMonsters.Insert(0, "DON'T SPAWN");
		allMonsters.Insert(1, "PROP_BearTrap");
		for (int num = 0; num < EnemyDropDownList.Length; num++)
		{
			TMP_Dropdown obj = EnemyDropDownList[num];
			obj.options.Clear();
			obj.AddOptions(allMonsters);
			int num2 = allMonsters.IndexOf(SpawnRoundEntry.SpawnClass[num]);
			obj.value = ((num2 != -1) ? num2 : 0);
			int enemyDropDownIndex = num;
			obj.onValueChanged.AddListener((int monsterIndex) =>
			{
				SpawnRoundEntry.SpawnClass[enemyDropDownIndex] = allMonsters[monsterIndex];
			});
		}
	}

	public void DeletePressed()
	{
		DeleteButtonPressedAction?.Invoke(this);
	}
}
