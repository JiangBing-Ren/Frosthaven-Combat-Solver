using System;
using System.Linq;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LevelEditorScenarioModifierListItem : MonoBehaviour
{
	public Image Background;

	[HideInInspector]
	public int modifierIndex;

	public TextMeshProUGUI ModifierNameText;

	public UnityAction<LevelEditorScenarioModifierListItem> ButtonPressedAction;

	private Color m_BackgroundOriginalColor;

	public CScenarioModifier Modifier => ScenarioManager.CurrentScenarioState.ScenarioModifiers[modifierIndex];

	public string ModifierEditorDisplayName
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(Modifier.EventIdentifier) && !Guid.TryParse(Modifier.EventIdentifier, out var _))
			{
				return Modifier.EventIdentifier;
			}
			string text = Modifier.ScenarioAbilityID ?? "";
			text = text.Replace("ScenarioAbility_", "").Replace("MoveScript", "MS").Replace("PhaseScript", "PS")
				.Replace("FHScenario", "FS");
			if (text.Equals("None", StringComparison.OrdinalIgnoreCase))
			{
				text = "";
			}
			if (string.IsNullOrWhiteSpace(text) && Modifier.ScenarioModifierType != EScenarioModifierType.None)
			{
				text = Modifier.ScenarioModifierType.ToString();
			}
			string text2 = ((Modifier.ScenarioModifierTriggerPhase != EScenarioModifierTriggerPhase.None) ? string.Join("", Modifier.ScenarioModifierTriggerPhase.ToString().Where(char.IsUpper)) : "");
			if (!string.IsNullOrEmpty(text2))
			{
				return text + "/" + text2;
			}
			return text;
		}
	}

	public void InitForScenarioModifier(CScenarioModifier scenarioModifier, int indexToUse)
	{
		modifierIndex = indexToUse;
		RefreshDisplayName();
	}

	public void RefreshDisplayName()
	{
		ModifierNameText.text = ModifierEditorDisplayName;
		if (string.IsNullOrWhiteSpace(ModifierNameText.text))
		{
			ModifierNameText.text = "Modifier #" + modifierIndex;
		}
	}

	public void SetSelectedState(bool selected)
	{
		Background.color = (selected ? new Color(0.85f, 0.745f, 0.541f) : m_BackgroundOriginalColor);
	}

	public void OnButtonPressItemSelected()
	{
		ButtonPressedAction?.Invoke(this);
	}

	private void Awake()
	{
		m_BackgroundOriginalColor = Background.color;
	}
}
