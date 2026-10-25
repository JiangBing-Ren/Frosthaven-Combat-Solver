using System.Linq;
using FHGUI.DTO;
using Libs.Utils;

namespace FHGUI.Elements;

public class StategyFavorMonsterContainer : ListContainerBase<StrategyFavorMonsterBannerSlot, MonsterDataAction, StrategyFavorMonsterBannerSlot.MonsterSlotData>
{
	private MonsterDataAction _onClick;

	public StrategyFavorMonsterBannerSlot Selected { get; private set; }

	public int SelectedIndex => Elements.IndexOf(Selected);

	protected override void InitElement(StrategyFavorMonsterBannerSlot listElement, StrategyFavorMonsterBannerSlot.MonsterSlotData elementParams, MonsterDataAction onClick)
	{
		listElement.Init(elementParams, OnMonsterClick);
	}

	protected override void AfterInit_Impl(MonsterDataAction onClick)
	{
		base.AfterInit_Impl(onClick);
		_onClick = onClick;
	}

	private void OnMonsterClick(StrategyFavorMonsterBannerSlot monsterBanner)
	{
		if (!(Selected == monsterBanner))
		{
			SetSelected(monsterBanner);
			_onClick?.Invoke(monsterBanner.MonsterID);
		}
	}

	public void SetSelected(int index)
	{
		if (index < 0 || index >= Elements.Count())
		{
			index = 0;
		}
		SetSelected(Elements.ElementAt(index));
	}

	private void SetSelected(StrategyFavorMonsterBannerSlot monsterBanner)
	{
		if ((bool)Selected)
		{
			Selected.Deselect();
		}
		Selected = monsterBanner;
		Selected.Select();
	}

	private void OnDisable()
	{
		Selected = null;
		_onClick = null;
	}
}
