using System.Collections.Generic;
using FHGUI.DTO;
using FHGUI.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class PerksInventoryModule : FHMapViewModule
{
	public struct Params
	{
		public List<PerkDTO> Perks;

		public UIPerkInteractionEvents Interactions;

		public uint Marks;

		public uint Checks;

		public Sprite Illustration;

		public string CaptionKey;

		public string PerksTooltipKey;

		public string PerkCostKey;
	}

	[Header("Components")]
	[SerializeField]
	private TextLocalizedListener m_Caption;

	[SerializeField]
	private Image m_Illustration;

	[SerializeField]
	private UIPerkInventoryContainer m_InventoryContainer;

	[SerializeField]
	private UIPerkPointsElement m_PerkPointsElement;

	private bool m_Initialized;

	private void InitCache()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
		}
	}

	public void Init(Params @params)
	{
		InitCache();
		m_InventoryContainer.Init(@params.Perks, @params.Interactions);
		if ((bool)m_PerkPointsElement)
		{
			m_PerkPointsElement.Init(@params.Marks, @params.Checks, @params.PerksTooltipKey);
		}
		if ((bool)m_Illustration)
		{
			m_Illustration.overrideSprite = @params.Illustration;
		}
		if ((bool)m_Caption)
		{
			m_Caption.SetTextKey(@params.CaptionKey);
		}
	}
}
