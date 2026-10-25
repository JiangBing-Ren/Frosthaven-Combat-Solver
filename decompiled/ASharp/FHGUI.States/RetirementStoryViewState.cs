using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class RetirementStoryViewState : FHMapViewState
{
	private readonly RosterViewService _rosterService;

	private CharacterClassDef _classDef;

	private IList<CampaignEffect> _appliedEffects;

	private bool _readyCheckPassed;

	public override FHMapViewLayer.LayerType TargetLayerType { get; set; } = FHMapViewLayer.LayerType.Modal;

	[Preserve]
	private RetirementStoryModule StoryModule => View.Modules.Characters.RetirementStoryModule;

	[Preserve]
	private InputActionHintsModule InputActionHints => View.Modules.InputActionHints;

	public RetirementStoryViewState(CharacterClassDef def, IList<CampaignEffect> appliedEffects)
	{
		_classDef = def;
		_rosterService = View.GetService<RosterViewService>();
		_appliedEffects = appliedEffects;
	}

	public override void OnShow()
	{
		View.GetService<InputActionHintsService>().Setup(InputActionHints, this);
		View.OutpostSceneController.OpenWorldMap();
		CharacterConfigUI characterConfigUI = UIInfoTools.Instance.GetCharacterConfigUI(CrossReference.ClassDefToECharacterModel(_classDef));
		OutpostEventsService service = View.GetService<OutpostEventsService>();
		List<CampaignEffect> list = service.CombineCampaignEffects(_appliedEffects);
		list.Sort((CampaignEffect x, CampaignEffect y) => x.Type.CompareTo(y.Type));
		List<OutpostEventDTO.Effect> rewards = (from e in list
			select service.GetFormattedEffect(e) into effect
			where !effect.Hidden
			select effect).ToList();
		RetirementStoryModule.Params obj = new RetirementStoryModule.Params
		{
			Illustration = characterConfigUI.illustration,
			ClassIcon = characterConfigUI.iconClassGold,
			StoryKey = "Event_" + _classDef.RetirementCampaignSection + "_Text",
			OnDone = OnFinished,
			Rewards = rewards
		};
		StoryModule.Init(obj);
	}

	private void OnFinished()
	{
		PopThisState();
	}
}
