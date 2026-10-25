using System.Collections.Generic;
using FHGUI.Elements;
using FHGUI.Modules;
using Libs.Utils;
using UnityEngine;

namespace FHGUI.Common;

public class UIMonsterMiniCardContainer : ListContainerBase<MiniMonsterCard, UIMonsterCardInteractionEvents, MonsterCardUIContext>
{
	[SerializeField]
	private RectTransform _tooltipPos;

	private readonly IList<MonsterCardUIContext> _cards = new List<MonsterCardUIContext>(8);

	private UIMonsterCardInteractionEvents _events;

	protected override void InitElement(MiniMonsterCard miniMonsterCard, MonsterCardUIContext context, UIMonsterCardInteractionEvents events)
	{
		miniMonsterCard.Init(context, events, _tooltipPos);
	}

	protected override void InitList(UIMonsterCardInteractionEvents listParams, IList<MonsterCardUIContext> listElementParams)
	{
		base.InitList(listParams, listElementParams);
		_cards.Clear();
		_cards.AddRange(listElementParams);
		_events = listParams;
	}

	public void ShowForLevel(int level)
	{
		int num = 0;
		foreach (MiniMonsterCard element in Elements)
		{
			MonsterCardUIContext monsterCardUIContext = _cards[num];
			monsterCardUIContext.OverrideLevel = level;
			element.Init(monsterCardUIContext, _events, _tooltipPos);
			num++;
		}
	}
}
