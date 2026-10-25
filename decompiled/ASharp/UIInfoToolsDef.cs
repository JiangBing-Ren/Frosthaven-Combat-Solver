using System.Collections.Generic;
using ScenarioRuleLibrary;
using UnityEngine;

[CreateAssetMenu(fileName = "UIInfoToolsDef", menuName = "Defs/UIInfoToolsDef")]
public class UIInfoToolsDef : ScriptableObject
{
	[Header("UI Text Colors")]
	[ColorUsage(true, true)]
	public Color mainColor = new Color(236f, 208f, 141f, 255f);

	[ColorUsage(true, true)]
	public Color basicTextColor = new Color(243f, 221f, 171f);

	[ColorUsage(true, true)]
	public Color descriptionTextColor;

	[ColorUsage(true, true)]
	public Color dialogueTextColor;

	[ColorUsage(true, true)]
	public Color neutralActionTextColor;

	[ColorUsage(true, true)]
	public Color positiveTextColor;

	[ColorUsage(true, true)]
	public Color negativeTextColor;

	[ColorUsage(true, true)]
	public Color greyedOutTextColor;

	[ColorUsage(true, true)]
	public Color positiveStatusEffectColor = new Color(255f, 164f, 20f);

	[ColorUsage(true, true)]
	public Color negativeStatusEffectColor = new Color(160f, 80f, 224f);

	[ColorUsage(true, true)]
	public Color greenDifficultyColor;

	[ColorUsage(true, true)]
	public Color yellowDifficultyColor;

	[ColorUsage(true, true)]
	public Color redDifficultyColor;

	[Header("Prop Outline Colors")]
	public List<UIInfoTools.PropOutlinableColourSetting> m_PropOutlinableColourSettings = new List<UIInfoTools.PropOutlinableColourSetting>();

	public Dictionary<ScenarioManager.ObjectImportType, Color> m_PropOutlineColourDictionary;

	[Header("Character Outline Colors")]
	public List<UIInfoTools.CharacterOutlinableColourSetting> m_CharacterOutlinableColourSettings = new List<UIInfoTools.CharacterOutlinableColourSetting>();

	public Dictionary<CActor.EType, Color> m_CharacterOutlineColourDictionary;

	private static UIInfoToolsDef s_instance;

	public static UIInfoToolsDef Instance
	{
		get
		{
			if (s_instance == null)
			{
				s_instance = Resources.Load("Settings/UIInfoToolsDef") as UIInfoToolsDef;
				if (s_instance != null)
				{
					s_instance.Init();
				}
			}
			return s_instance;
		}
	}

	private void Init()
	{
		m_CharacterOutlineColourDictionary = new Dictionary<CActor.EType, Color>();
		foreach (UIInfoTools.CharacterOutlinableColourSetting characterOutlinableColourSetting in m_CharacterOutlinableColourSettings)
		{
			m_CharacterOutlineColourDictionary.Add(characterOutlinableColourSetting.ActorType, characterOutlinableColourSetting.OutlineColor);
		}
		m_PropOutlineColourDictionary = new Dictionary<ScenarioManager.ObjectImportType, Color>();
		foreach (UIInfoTools.PropOutlinableColourSetting propOutlinableColourSetting in m_PropOutlinableColourSettings)
		{
			m_PropOutlineColourDictionary.Add(propOutlinableColourSetting.PropType, propOutlinableColourSetting.OutlineColor);
		}
	}

	private void CopyFromPrefab()
	{
	}
}
