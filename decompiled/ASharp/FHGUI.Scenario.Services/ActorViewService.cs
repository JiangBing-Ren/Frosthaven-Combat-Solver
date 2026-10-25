using System.Collections.Generic;
using System.Linq;
using FHGUI.Scenario.DTO;
using ScenarioRuleLibrary;

namespace FHGUI.Scenario.Services;

public class ActorViewService : FHScenarioViewService
{
	public ActorViewService(FHScenarioViewController view)
		: base(view)
	{
	}

	public List<TacItemDTO> GetUsableItems(CActor actor)
	{
		if (actor == null)
		{
			return new List<TacItemDTO>();
		}
		return (from item in actor.Inventory.AllItemsAndPets
			where IsItemUsabe(item)
			select new TacItemDTO(item)).ToList();
	}

	public void UseItem(CActor actor, CItem item)
	{
		new UseItemService(actor).UseItem(item);
		_view.RefreshView();
	}

	public void UseItem(CActor actor, TacItemDTO item)
	{
		UseItem(actor, item.Item);
	}

	public static bool IsItemUsabe(CItem item)
	{
		if (item.SlotState == CItem.EItemSlotState.Useable || item.SlotState == CItem.EItemSlotState.Selected)
		{
			return true;
		}
		return false;
	}
}
