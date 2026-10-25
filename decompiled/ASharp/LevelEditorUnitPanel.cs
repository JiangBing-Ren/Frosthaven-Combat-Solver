using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class LevelEditorUnitPanel : MonoBehaviour
{
	private delegate void OnSearchContextChange(string searchContext);

	public LevelEditorUnitsPanel.UnitType type;

	public InputField SearchField;

	public GameObject UnitButton;

	public GameObject ParentPanel;

	public RectTransform Container;

	private HashSet<GameObject> _initiallyAvailable = new HashSet<GameObject>();

	private void Awake()
	{
		if (SearchField != null)
		{
			SearchField.onValueChanged.AddListener((string s) =>
			{
				Init();
			});
		}
		foreach (Transform item in GetContainer())
		{
			_initiallyAvailable.Add(item.gameObject);
		}
		Init();
	}

	private void OnDestroy()
	{
		Clear();
		if (SearchField != null)
		{
			SearchField.onValueChanged.RemoveAllListeners();
		}
	}

	private void Init()
	{
		Clear();
		switch (type)
		{
		case LevelEditorUnitsPanel.UnitType.Player:
		{
			foreach (CCharacterClass charClass in from x in CharacterClassManager.Classes
				where x.CharacterYML.VerifyValidForLevel(ScenarioManager.CurrentScenarioState?.ID)
				orderby x.ID
				select x)
			{
				CharacterConfigUI characterConfigUI = UIInfoTools.Instance.characterConfigsUI.FirstOrDefault((CharacterConfigUI c) => c.character == charClass.CharacterModel);
				if (characterConfigUI != null && (GlobalData.CompletedCharacters.Contains(charClass.CharacterID) || !characterConfigUI.hasToReveal))
				{
					AddCharacterButton(charClass.CharacterID, charClass.LocKey);
				}
			}
			break;
		}
		case LevelEditorUnitsPanel.UnitType.Enemy:
		{
			foreach (CMonsterClass item in from x in MonsterClassManager.Classes
				where x.NonEliteVariant == null && x.MonsterYML.VerifyValidForLevel(ScenarioManager.CurrentScenarioState?.ID)
				orderby x.ID
				select x)
			{
				if (AssetBundleManager.Instance.BundleConfigForPrefab(CActor.EType.Enemy, item.DefaultModel) != null)
				{
					AddMonsterButton(item.ID, item.LocKey);
				}
			}
			break;
		}
		case LevelEditorUnitsPanel.UnitType.Objects:
		{
			foreach (CObjectClass objectClass in MonsterClassManager.ObjectClasses)
			{
				if (!(objectClass.ID == "PropDummyObject") && AssetBundleManager.Instance.BundleConfigForPrefab(CActor.EType.Enemy, objectClass.DefaultModel) != null)
				{
					AddMonsterButton(objectClass.ID, objectClass.LocKey);
				}
			}
			break;
		}
		case LevelEditorUnitsPanel.UnitType.Prop:
			foreach (EPropType item2 in CObjectProp.PropTypes.Where((EPropType x) => x != EPropType.None))
			{
				string text = item2.ToString();
				if (IsInCurrentSearchContext(text))
				{
					AddGenericPropButton(item2);
				}
			}
			{
				foreach (ESpecificPropType item3 in CObjectProp.SpecificPropTypes.Where((ESpecificPropType x) => x != ESpecificPropType.None))
				{
					string name = item3.ToString();
					if (!CObjectProp.VFXPropTypes.Any((EVFXPropType x) => x.ToString() == name) && IsInCurrentSearchContext(name))
					{
						AddSpecificPropButton(item3);
					}
				}
				break;
			}
		case LevelEditorUnitsPanel.UnitType.Spawner:
			AddSpawnerButton("Spawner");
			AddSpawnerButton("SewerPipeSpawner");
			AddSpawnerButton("GraveSingleSpawner");
			AddSpawnerButton("GraveDoubleSpawner");
			break;
		}
	}

	private void Clear()
	{
		foreach (Transform item in GetContainer())
		{
			if (!_initiallyAvailable.Contains(item.gameObject))
			{
				Object.Destroy(item.gameObject);
			}
		}
	}

	private bool IsInCurrentSearchContext(string name)
	{
		if (SearchField == null || SearchField.text == string.Empty)
		{
			return true;
		}
		return name.ToLower().Contains(SearchField.text.ToLower());
	}

	private void AddMonsterButton(string unitName, string locKey)
	{
		Object.Instantiate(UnitButton, GetContainer()).GetComponent<LevelEditorUnitButton>().SetUnitEnemy(unitName, locKey, ParentPanel.GetComponent<LevelEditorUnitsPanel>());
	}

	private Transform GetContainer()
	{
		if (!(Container == null))
		{
			return Container;
		}
		return transform;
	}

	private void AddSpecificPropButton(ESpecificPropType prop)
	{
		string propName = prop.ToString();
		Internal_AddPropButton(propName, CObjectProp.SpecificPropTypeToPropType(prop).ToString(), generic: false);
	}

	private void AddGenericPropButton(EPropType prop)
	{
		string text = prop.ToString();
		Internal_AddPropButton(text, text, generic: true);
	}

	private void Internal_AddPropButton(string propName, string propType, bool generic)
	{
		if (generic)
		{
			switch (propName)
			{
			case "Snowdrift":
				return;
			case "RockSingle":
				return;
			}
		}
		CAreaEffect areaEffect = CObjectProp.GetAreaEffect(propName, propType);
		Object.Instantiate(UnitButton, GetContainer()).GetComponent<LevelEditorUnitButton>().SetUnitApparanceProp(propName, ParentPanel.GetComponent<LevelEditorUnitsPanel>(), generic, areaEffect);
	}

	private void AddSpawnerButton(string unitName)
	{
		LevelEditorUnitButton component = Object.Instantiate(UnitButton, GetContainer()).GetComponent<LevelEditorUnitButton>();
		CAreaEffect areaEffect = null;
		if (unitName == "GraveDouble")
		{
			areaEffect = CardProcessingShared.CreateArea("0,0,R|0,1,R", "Editor", "Editor");
		}
		component.SetUnitSpawner(unitName, ParentPanel.GetComponent<LevelEditorUnitsPanel>(), areaEffect);
	}

	private void AddCharacterButton(string characterID, string locKey)
	{
		Object.Instantiate(UnitButton, (Container == null) ? transform : Container).GetComponent<LevelEditorUnitButton>().SetUnitCharacter(characterID, locKey, ParentPanel.GetComponent<LevelEditorUnitsPanel>());
	}
}
