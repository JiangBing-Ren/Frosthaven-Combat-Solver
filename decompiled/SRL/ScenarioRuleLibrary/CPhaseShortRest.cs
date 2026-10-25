using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CPhaseShortRest : CPhase
{
	public CPhaseShortRest()
	{
		m_PhaseType = PhaseType.ShortRest;
	}

	private static CPlayerActor GetNextPlayerToShortRest(CPlayerActor lastShortRestedPlayer)
	{
		List<CPlayerActor> playerActors = ScenarioManager.Scenario.PlayerActors;
		int num = -1;
		if (lastShortRestedPlayer != null)
		{
			num = playerActors.IndexOf(lastShortRestedPlayer);
		}
		for (num++; num < playerActors.Count; num++)
		{
			CPlayerActor cPlayerActor = playerActors[num];
			if (cPlayerActor.CharacterClass.ShortRest)
			{
				return cPlayerActor;
			}
		}
		return null;
	}

	protected override void OnNextStep(bool passing = false)
	{
		bool flag = GameState.s_ShortRestingPlayer?.IsExhausted() ?? false;
		if ((GameState.s_ShortRestingPlayer == null) | flag)
		{
			if (flag)
			{
				GameState.CheckExhaustAfterShortRest(GameState.s_ShortRestingPlayer, isShortRestingPlayer: true);
			}
			GameState.s_ShortRestingPlayer = GetNextPlayerToShortRest(GameState.s_ShortRestingPlayer);
			GameState.OverrideCurrentActor(GameState.s_ShortRestingPlayer);
			if (GameState.s_ShortRestingPlayer == null)
			{
				GameState.NextPhase();
				return;
			}
		}
		if (!GameState.s_ShortRestingPlayer.CharacterClass.HasShortRested)
		{
			GameState.OverrideCurrentActor(GameState.s_ShortRestingPlayer);
			CPlayerToShortRest_MessageData message = new CPlayerToShortRest_MessageData(GameState.s_ShortRestingPlayer);
			ScenarioRuleClient.MessageHandler(message);
			return;
		}
		GameState.s_ShortRestingPlayer = GetNextPlayerToShortRest(GameState.s_ShortRestingPlayer);
		GameState.OverrideCurrentActor(GameState.s_ShortRestingPlayer);
		if (GameState.s_ShortRestingPlayer == null)
		{
			GameState.NextPhase();
			return;
		}
		CPlayerToShortRest_MessageData message2 = new CPlayerToShortRest_MessageData(GameState.s_ShortRestingPlayer);
		ScenarioRuleClient.MessageHandler(message2);
	}

	protected override void OnStepComplete(bool passingStep = false)
	{
		NextStep();
	}
}
