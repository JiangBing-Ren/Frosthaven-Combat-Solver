using ScenarioRuleLibrary;
using UnityEngine;

namespace FHGUI.Common;

public struct LootDTO
{
	public LootCard.EType LootType;

	public Sprite Icon;

	public string Name;

	public string Tooltip;

	public uint Quantity;
}
