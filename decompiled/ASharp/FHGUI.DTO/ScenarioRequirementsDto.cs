using System.Collections.Generic;
using System.Linq;
using FHRL;

namespace FHGUI.DTO;

public class ScenarioRequirementsDto
{
	public bool RequireBoat;

	public bool RequireSled;

	public bool RequireClimbingGear;

	public bool MissingBoat;

	public bool MissingSled;

	public bool MissingClimbingGear;

	public bool InvalidParty;

	public bool PartyMemberIsReadyToRetire;

	public bool CantBeReplayed;

	public bool MissingPersonalQuest;

	public bool MissingLurkerCrownShards;

	public string MissingSticker;

	public ScenarioChainType MissingCompletedType;

	public bool HasSomethingMissing
	{
		get
		{
			if (!MissingBoat && !MissingSled && !MissingClimbingGear && MissingCompletedType == ScenarioChainType.None && !InvalidParty && !PartyMemberIsReadyToRetire && !CantBeReplayed && !MissingPersonalQuest && !MissingLurkerCrownShards)
			{
				return MissingSticker.IsNOTNullOrEmpty();
			}
			return true;
		}
	}

	public ScenarioRequirementsDto()
	{
	}

	public ScenarioRequirementsDto(ScenarioDef scenarioDef, bool hasBoat, bool hasSled, bool hasClimbingGear, bool invalidParty, bool partyMemberIsReadyToRetire, bool wasCompleted, bool missingPersonalQuest, uint lurkerCrownShardsCount, List<CampaignStickerID> stickers)
	{
		RequireBoat = scenarioDef.RequiresBoat;
		RequireSled = scenarioDef.RequiresSled;
		RequireClimbingGear = scenarioDef.RequiresClimbingGear;
		MissingBoat = scenarioDef.RequiresBoat && !hasBoat;
		MissingSled = scenarioDef.RequiresSled && !hasSled;
		MissingClimbingGear = scenarioDef.RequiresClimbingGear && !hasClimbingGear;
		InvalidParty = invalidParty;
		PartyMemberIsReadyToRetire = partyMemberIsReadyToRetire;
		CantBeReplayed = scenarioDef.CanNotBeReplayed & wasCompleted;
		MissingPersonalQuest = missingPersonalQuest;
		MissingLurkerCrownShards = scenarioDef.RequiredLurkerCrownShards != 0 && scenarioDef.RequiredLurkerCrownShards > lurkerCrownShardsCount;
		MissingSticker = ((scenarioDef.RequiresSticker.IsNOTNullOrEmpty() && !stickers.Contains(new CampaignStickerID(scenarioDef.RequiresSticker))) ? scenarioDef.RequiresSticker : null);
	}

	public ScenarioRequirementsDto(ScenarioState scenarioState, CampaignState campaignState, bool invalidParty, bool partyMemberIsReadyToRetire, bool missingPersonalQuest)
		: this(scenarioState.Def, campaignState.CanPlayBoatScenarios, campaignState.CanPlaySledScenarios, campaignState.CanPlayClimbingGearScenarios, invalidParty, partyMemberIsReadyToRetire, scenarioState.Completed, missingPersonalQuest, campaignState.LurkerCrownShards, campaignState.Stickers)
	{
		if (scenarioState.Def.RequiresCompletedType != ScenarioChainType.None && campaignState.UnlockedScenarios.Where((ScenarioState s) => s.Completed).All((ScenarioState s) => s.Def.Chain != scenarioState.Def.RequiresCompletedType))
		{
			MissingCompletedType = scenarioState.Def.RequiresCompletedType;
		}
	}

	public ScenarioRequirementsDto(ScenarioDef scenarioDef, CampaignState campaignState, bool invalidParty, bool partyMemberIsReadyToRetire, bool missingPersonalQuest)
		: this(scenarioDef, campaignState.CanPlayBoatScenarios, campaignState.CanPlaySledScenarios, campaignState.CanPlayClimbingGearScenarios, invalidParty, partyMemberIsReadyToRetire, wasCompleted: false, missingPersonalQuest, campaignState.LurkerCrownShards, campaignState.Stickers)
	{
		if (scenarioDef.RequiresCompletedType != ScenarioChainType.None && campaignState.UnlockedScenarios.Where((ScenarioState s) => s.Completed).All((ScenarioState s) => s.Def.Chain != scenarioDef.RequiresCompletedType))
		{
			MissingCompletedType = scenarioDef.RequiresCompletedType;
		}
	}
}
