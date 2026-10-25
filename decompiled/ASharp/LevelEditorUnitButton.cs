using GLOOM;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class LevelEditorUnitButton : MonoBehaviour
{
	public enum UnitType
	{
		None,
		Monster,
		Character,
		ApparanceProp,
		SpecificProp,
		Spawner
	}

	private string m_UnitID;

	private UnitType m_UnitType;

	private LevelEditorUnitsPanel m_UnitsPanel;

	private CAreaEffect m_AreaEffect;

	public Image unitImage;

	public Text unitText;

	public void SetUnitEnemy(string unitID, string locKey, LevelEditorUnitsPanel unitsPanel)
	{
		m_UnitID = unitID;
		m_UnitType = UnitType.Monster;
		unitText.text = $"<i>{unitID}</i>\n<b>{LocalizationManager.GetTranslation(locKey)}</b>";
		m_UnitsPanel = unitsPanel;
	}

	public void SetUnitCharacter(string characterID, string locKey, LevelEditorUnitsPanel unitsPanel)
	{
		m_UnitID = characterID;
		m_UnitType = UnitType.Character;
		unitText.text = $"<i>{characterID}</i>\n<b>{LocalizationManager.GetTranslation(locKey)}</b>";
		m_UnitsPanel = unitsPanel;
	}

	public void SetUnitSpawner(string spawnerName, LevelEditorUnitsPanel unitsPanel, CAreaEffect areaEffect = null)
	{
		m_UnitID = spawnerName;
		m_UnitType = UnitType.Spawner;
		unitText.text = m_UnitID;
		m_UnitsPanel = unitsPanel;
		m_AreaEffect = areaEffect;
	}

	public void SetUnitApparanceProp(string unitName, LevelEditorUnitsPanel unitsPanel, bool generic, CAreaEffect areaEffect)
	{
		m_UnitID = unitName;
		m_UnitType = UnitType.ApparanceProp;
		unitText.text = (generic ? "GENERIC" : "SPECIFIC") + "\n" + m_UnitID;
		unitText.color = (generic ? Color.black : new Color(0f, 0.2f, 0.5f, 1f));
		unitText.fontStyle = FontStyle.Bold;
		m_UnitsPanel = unitsPanel;
		m_AreaEffect = areaEffect;
	}

	public void SetUnitSpecificProp(string unitName, LevelEditorUnitsPanel unitsPanel)
	{
		m_UnitID = unitName;
		m_UnitType = UnitType.SpecificProp;
		unitText.text = m_UnitID;
		m_UnitsPanel = unitsPanel;
	}

	public void OnSearchContextChange(string obj)
	{
		if (obj == string.Empty)
		{
			unitText.color = Color.white;
		}
		else
		{
			unitText.color = (m_UnitID.ToLower().Contains(obj.ToLower()) ? Color.green : Color.gray);
		}
	}

	public void OnClick()
	{
		m_UnitsPanel.TryPlaceUnitInHex(m_UnitID, m_AreaEffect);
	}
}
