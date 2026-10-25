using System.Linq;
using FHGUI.DTO;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class PotionDiscoveryViewState : FHMapViewState
{
	private ItemDataDTO m_Potion;

	private OutpostResources m_Herbs;

	private string m_TargetCharacter;

	public override FHMapViewLayer.LayerType TargetLayerType { get; set; } = FHMapViewLayer.LayerType.Modal;

	[Preserve]
	private PotionDiscoveryModule PotionDistributionModule => View.Modules.Characters.PotionDiscoveryModule;

	private PartyViewService PartyService => View.GetService<PartyViewService>();

	private ItemViewService ItemService => View.GetService<ItemViewService>();

	private AlchemistViewService AlchemistService => View.GetService<AlchemistViewService>();

	private CharacterDetailsViewService CharacterDetailsService => View.GetService<CharacterDetailsViewService>();

	public PotionDiscoveryViewState(ItemDataDTO potion, OutpostResources sourceHerbs)
	{
		m_Potion = potion;
		m_Herbs = sourceHerbs;
	}

	public override void OnShow()
	{
		if (!View.GameState.Outpost.Resources.Has(m_Herbs))
		{
			PopThisState();
			return;
		}
		bool flag = AlchemistService.CanResearch(m_Potion.ID, m_Herbs);
		bool flag2 = View.GameState.PartyCharacters.Any((CharacterState state) => AlchemistService.CanBrew(state, m_Herbs));
		if (!flag && !flag2)
		{
			PopThisState();
			return;
		}
		PotionDistributionModule.Init(new PotionDiscoveryModule.Params
		{
			OnConfirm = OnDone,
			Potion = m_Potion
		});
	}

	private void OnDone()
	{
		PopThisState();
	}
}
