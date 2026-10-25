using FHGUI.Configs;
using UnityEngine;

[CreateAssetMenu(menuName = "UI Config/Perk", fileName = "PerkConfig")]
public class PerkConfigUI : ScriptableObject
{
	[PingInHierarchy]
	public string Id;

	[ConfigIcon(48, true)]
	public Sprite Icon;
}
