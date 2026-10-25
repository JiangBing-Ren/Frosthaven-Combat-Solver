using ScenarioRuleLibrary;
using UnityEngine;

[CreateAssetMenu(menuName = "UI Config/Element", fileName = "Element Config")]
public class ElementConfigUI : ScriptableObject
{
	public ElementInfusionBoardManager.EElement element;

	public Sprite icon;

	public EffectDataBase effectData;

	public string selectAudioItem;

	public Color highlightColor;

	[Header("Reward")]
	public Sprite rewardIcon;

	[Header("Picker")]
	public Sprite pickerIcon;

	public Sprite selectedIcon;

	[Header("Use Bar")]
	public Sprite useIcon;
}
