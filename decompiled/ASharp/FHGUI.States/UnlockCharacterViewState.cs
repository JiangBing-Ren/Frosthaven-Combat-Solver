using System.Linq;
using FHGUI.DTO;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using ScenarioRuleLibrary;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class UnlockCharacterViewState : FHMapViewState
{
	private CharacterClassDef _characterClassDef;

	public override FHMapViewLayer.LayerType TargetLayerType { get; set; } = FHMapViewLayer.LayerType.Modal;

	[Preserve]
	private UnlockCharacterModule UnlockCharacterModule => View.Modules.Characters.UnlockCharacterModule;

	[Preserve]
	private HeroClassStatsModule HeroClassStatsModule => View.Modules.Characters.UnlockedHeroClassStats;

	private PartyViewService PartyService => View.GetService<PartyViewService>();

	public UnlockCharacterViewState(CharacterClassDef classDef)
	{
		_characterClassDef = classDef;
	}

	public override void OnShow()
	{
		ECharacter eCharacter = CrossReference.ClassIdToECharacterModel(_characterClassDef.ID.ID);
		PartySlotDTO characterSlotData = PartyViewService.FromClassType(-1, _characterClassDef.ID.ID, eCharacter);
		ClassInfoDTO classInfoDTO = new ClassInfoDTO(1u, _characterClassDef, null);
		UnlockCharacterModule.Params obj = new UnlockCharacterModule.Params
		{
			CharacterSlotData = characterSlotData,
			ClassData = classInfoDTO,
			OnDone = PopThisState,
			Cards = (from cardDef in _characterClassDef.GetCardsAvailableAtLevel(View.GameController.DefRepository, 1)
				select CharacterClassManager.AllAbilityCards.First((CAbilityCard cCard) => cCard.ID == cardDef.ID.ID)).ToList()
		};
		UnlockCharacterModule.Init(obj);
		HeroClassStatsModule.Init(classInfoDTO, null, "GUI_CHARACTER_PREVIEW_CARDS", isReadOnly: true, showExp: false);
	}
}
