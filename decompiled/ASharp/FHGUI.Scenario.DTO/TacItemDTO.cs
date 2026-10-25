using ScenarioRuleLibrary;
using UnityEngine;

namespace FHGUI.Scenario.DTO;

public class TacItemDTO
{
	public readonly CItem Item;

	public Sprite Image => UIInfoTools.Instance.GetItemMiniSprite(Item.YMLData.Art);

	public TacItemDTO(CItem item)
	{
		Item = item;
	}
}
