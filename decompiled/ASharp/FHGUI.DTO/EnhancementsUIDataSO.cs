using System;
using System.Linq;
using FHGUI.Configs;
using FHRL;
using ScenarioRuleLibrary;
using UnityEngine;

namespace FHGUI.DTO;

[CreateAssetMenu(fileName = "EnhancementsUIDataSO", menuName = "Def/EnhancementsUIDataSO")]
public class EnhancementsUIDataSO : ScriptableObject
{
	[Serializable]
	public struct EnhancementIcon
	{
		public EEnhancement Enhancement;

		[ConfigIcon(48, true)]
		public Sprite Icon;

		[ConfigIcon(48, true)]
		public Sprite InventoryIcon;
	}

	[Serializable]
	public struct EnhancementSlotIcon
	{
		public EEnhancementMarkType EnhancementMarkType;

		[ConfigIcon(48, true)]
		public Sprite Icon;
	}

	[SerializeField]
	public EnhancementIcon[] Enhancements;

	[SerializeField]
	public EnhancementSlotIcon[] EnhancementSlots;

	public Sprite GetEnhancementIcon(CardEnhancementEffectType ceet)
	{
		return GetEnhancementIcon(CrossReference.CardEnhancementEffectTypeToEEnhancement(ceet));
	}

	public Sprite GetEnhancementIcon(EEnhancement enhancement)
	{
		EnhancementIcon enhancementIcon = Enhancements.FirstOrDefault((EnhancementIcon it) => it.Enhancement == enhancement);
		if (enhancementIcon.Icon == null)
		{
			Log.GUIError($"Not found enhancement icon for {enhancement}", this);
		}
		return enhancementIcon.Icon;
	}

	public Sprite GetEnhancementInventoryIcon(EEnhancement enhancement)
	{
		EnhancementIcon enhancementIcon = Enhancements.FirstOrDefault((EnhancementIcon it) => it.Enhancement == enhancement);
		if (enhancementIcon.InventoryIcon == null)
		{
			Log.GUIError($"Not found enhancement inventory icon for {enhancement}", this);
		}
		return enhancementIcon.InventoryIcon;
	}

	public Sprite GetEnhancementSlotIcon(CardEnhancementMarkType enhancementMark)
	{
		return GetEnhancementSlotIcon(CrossReference.CardEnhancementMarkTypeToEEnhancementMarkType(enhancementMark));
	}

	public Sprite GetEnhancementSlotIcon(EEnhancementMarkType enhancementMark)
	{
		EnhancementSlotIcon enhancementSlotIcon = EnhancementSlots.FirstOrDefault((EnhancementSlotIcon it) => it.EnhancementMarkType == enhancementMark);
		if (enhancementSlotIcon.Icon == null)
		{
			Log.GUIError($"Not found enhancement slot icon for {enhancementMark}", this);
		}
		return enhancementSlotIcon.Icon;
	}
}
