using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public sealed class UIItemCardTooltipTarget : UITooltipTarget
{
	private CItem item;

	public GameObject itemCard;

	public bool showModifiers;

	[ConditionalField("showModifiers", "true", true)]
	public bool showModifiersOnLeft;

	public void Initialize(CItem item)
	{
		this.item = item;
	}

	protected override void ShowTooltip(float delay = -1f)
	{
		if (item != null)
		{
			itemCard = ObjectPool.SpawnCard(item.ID, ObjectPool.ECardType.Item, UITooltipManager.GetTransform(), resetLocalScale: true, resetToMiddle: true);
			ItemCardUI component = itemCard.GetComponent<ItemCardUI>();
			component.item = item;
			component.EnableShowModifiers(showModifiers, showModifiersOnLeft);
			UITooltipManager.ShowBackground(show: false);
			base.ShowTooltip(delay);
		}
		else
		{
			Log.GUIError("Trying to show an item card but CItem is null.");
		}
	}

	public override void HideTooltip(float delay = -1f)
	{
		base.HideTooltip(delay);
		ObjectPool.RecycleCard(item.ID, ObjectPool.ECardType.Item, itemCard);
	}
}
