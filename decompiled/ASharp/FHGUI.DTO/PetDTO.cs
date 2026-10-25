using FHRL;
using UnityEngine;

namespace FHGUI.DTO;

public struct PetDTO
{
	public PetDef Def;

	public Sprite Icon;

	public string NameLocKey;

	public string EffectLocKey;

	public PetDTO(PetDef def)
	{
		ItemConfigUI itemConfig = UIInfoTools.Instance.GetItemConfig(def.ArtKey);
		Def = def;
		Icon = itemConfig.miniIcon;
		NameLocKey = def.Name;
		EffectLocKey = def.Description;
	}
}
