using System;
using System.Collections.Generic;
using System.Linq;
using MapRuleLibrary.YML.Quest;
using UnityEngine;

[Serializable]
public class QuestAreaConfigUI
{
	[Serializable]
	private class QuestAreaIconTypeConfigUI : QuestTypeConfigUI
	{
		public EQuestIconType iconType;

		public bool completed;
	}

	public EQuestAreaType locationArea;

	[SerializeField]
	private List<QuestAreaIconTypeConfigUI> icons;

	public QuestTypeConfigUI GetConfig(EQuestIconType iconType, bool completed)
	{
		return icons.FirstOrDefault((QuestAreaIconTypeConfigUI it) => it.iconType == iconType && it.completed == completed);
	}
}
