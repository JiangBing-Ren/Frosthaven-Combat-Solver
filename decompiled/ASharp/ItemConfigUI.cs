using FHGUI.Configs;
using UnityEngine;

[CreateAssetMenu(menuName = "UI Config/Item", fileName = "ItemConfig")]
public class ItemConfigUI : ScriptableObject
{
	[PingInHierarchy]
	public string itemName;

	[Header("Card")]
	[ConfigIcon(48, "Item Background", false)]
	public Sprite backgroundImage;

	[Header("Use Item")]
	[ConfigIcon(48, "Mini Icon", true)]
	public Sprite miniIcon;

	public PreviewEffectInfo previewEffect;

	public string toggleAudioItem = "PlaySound_ScenarioUIEquipmentToggle_Normal";
}
