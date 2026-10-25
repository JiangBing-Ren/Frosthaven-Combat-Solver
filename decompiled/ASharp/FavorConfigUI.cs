using FHGUI.Configs;
using FHRL;
using UnityEngine;

[CreateAssetMenu(menuName = "UI Config/FavorConfigUI", fileName = "FavorConfigUI")]
public class FavorConfigUI : ScriptableObject
{
	[PingInHierarchy]
	public ScenarioFavorType Id;

	[ConfigIcon(48, true)]
	public Sprite Icon;

	[Header("Localization")]
	public string Name;

	public string Description;

	public string NameLiteral;

	public string DescriptionLiteral;
}
