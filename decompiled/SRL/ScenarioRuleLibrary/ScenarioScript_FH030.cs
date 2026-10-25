namespace ScenarioRuleLibrary;

public class ScenarioScript_FH030 : ScenarioScript
{
	public override string GetOverrideCharacterModelPrefabName(CActor actor)
	{
		return actor.GetPrefabName() switch
		{
			"AlgoxGuard" => "AlgoxGuardSnowspeaker", 
			"AlgoxGuard Elite" => "AlgoxGuardSnowspeaker Elite", 
			"AlgoxArcher" => "AlgoxArcherSnowspeaker", 
			"AlgoxArcher Elite" => "AlgoxArcherSnowspeaker Elite", 
			"AlgoxScout" => "AlgoxScoutSnowspeaker", 
			"AlgoxScout Elite" => "AlgoxScoutSnowspeaker Elite", 
			"AlgoxSnowspeaker" => "AlgoxSnowspeakerOrange", 
			"AlgoxSnowspeaker Elite" => "AlgoxSnowspeakerOrange Elite", 
			_ => null, 
		};
	}
}
