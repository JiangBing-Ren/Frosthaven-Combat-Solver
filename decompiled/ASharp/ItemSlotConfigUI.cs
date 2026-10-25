using FHGUI.Configs;
using UnityEngine;

[CreateAssetMenu(menuName = "UI Config/ItemSlot ConfigUI", fileName = "ItemSlotConfigUI")]
public class ItemSlotConfigUI : ScriptableObject
{
	[PingInHierarchy]
	public string Id;

	[ConfigIcon(48, true)]
	public Sprite Icon;

	[ConfigIcon(48, true)]
	public Sprite IconStylized;

	[ConfigIcon(48, true)]
	public Sprite IconTintable;
}
