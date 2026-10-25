using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Elements;
using UnityEngine;

namespace FHGUI.Modules;

public class AttackModifiersModule : FHMapViewModule
{
	[Header("Components")]
	[SerializeField]
	private RectTransform m_BaseModifiersContainer;

	[SerializeField]
	private RectTransform m_SpecialModifiersContainer;

	[SerializeField]
	private RectTransform m_TacticalPerksContainer;

	[SerializeField]
	private RectTransform m_GroupBaseModifiers;

	[SerializeField]
	private RectTransform m_GroupSpecialModifiers;

	[SerializeField]
	private RectTransform m_GroupTacticalPerks;

	[SerializeField]
	private FHUIAttackModifier m_AttackModifierPrototype;

	[SerializeField]
	private UIPerkElement m_PerkPrototype;

	private List<FHUIAttackModifier> m_BaseModifiers = new List<FHUIAttackModifier>();

	private List<FHUIAttackModifier> m_SpecialModifiers = new List<FHUIAttackModifier>();

	private List<UIPerkElement> m_Perks = new List<UIPerkElement>();

	private bool m_Initialized;

	private void InitCache()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			if (m_BaseModifiersContainer != null)
			{
				m_BaseModifiersContainer.GetComponentsInChildren(includeInactive: true, m_BaseModifiers);
			}
			if (m_SpecialModifiersContainer != null)
			{
				m_SpecialModifiersContainer.GetComponentsInChildren(includeInactive: true, m_SpecialModifiers);
			}
		}
	}

	public void Init(List<AttackModifierDTO> attackModifiers, List<PerkDTO> perks, bool showTactical = true)
	{
		InitCache();
		PopulateModifierIcons(attackModifiers.Where((AttackModifierDTO dto) => dto.IsBasic).ToList(), m_BaseModifiers, m_BaseModifiersContainer, 8);
		PopulateModifierIcons(attackModifiers.Where((AttackModifierDTO dto) => !dto.IsBasic).ToList(), m_SpecialModifiers, m_SpecialModifiersContainer, 8);
		if (showTactical)
		{
			PopulatePerks(perks, m_Perks, m_TacticalPerksContainer);
		}
		m_GroupTacticalPerks.gameObject.SetActive(showTactical);
	}

	private void PopulateModifierIcons(List<AttackModifierDTO> attackModifiers, List<FHUIAttackModifier> icons, RectTransform container, int count)
	{
		if (!(container == null))
		{
			count = Mathf.Max(attackModifiers?.Count ?? 0, count);
			UIHelperUtil.UpdateUIList(count, icons, m_AttackModifierPrototype, container, onShow);
		}
		void onShow(int index, FHUIAttackModifier item)
		{
			AttackModifierDTO dto = ((attackModifiers != null && attackModifiers.Count > index) ? attackModifiers[index] : null);
			item.Init(dto);
		}
	}

	private void PopulatePerks(List<PerkDTO> perks, List<UIPerkElement> icons, RectTransform container)
	{
		if (!(container == null))
		{
			UIHelperUtil.UpdateUIList(perks?.Count ?? 0, icons, m_PerkPrototype, container, onShow);
		}
		void onShow(int index, UIPerkElement item)
		{
			PerkDTO data = ((perks != null && perks.Count > index) ? perks[index] : null);
			item.Init(data, null);
		}
	}
}
