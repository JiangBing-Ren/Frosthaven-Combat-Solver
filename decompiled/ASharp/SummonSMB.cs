using System.Collections.Generic;
using Chronos;
using ScenarioRuleLibrary;
using UnityEngine;

public class SummonSMB : StateMachineBehaviour
{
	[SerializeField]
	private float m_Delay;

	private float m_InstanceDelay;

	private bool m_IsSummoned;

	private float m_OriginalDelay;

	private void Awake()
	{
		m_OriginalDelay = m_Delay;
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		animator.speed = Timekeeper.instance.m_GlobalClock.timeScale;
		m_IsSummoned = false;
		m_InstanceDelay = m_Delay;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		if (m_InstanceDelay > 0f || m_IsSummoned)
		{
			m_InstanceDelay -= Timekeeper.instance.m_GlobalClock.deltaTime;
		}
		else
		{
			SummonCharacters();
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		if (!m_IsSummoned)
		{
			SummonCharacters();
		}
	}

	public static void SummonCharacters(List<CActor> summonedActors = null)
	{
		Choreographer s_Choreographer = Choreographer.s_Choreographer;
		foreach (CEnemyActor enemy in ScenarioManager.Scenario.Enemies)
		{
			if (!s_Choreographer.FindClientEnemy(enemy) && !enemy.PhasedOut && (summonedActors == null || summonedActors.Contains(enemy)))
			{
				CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[enemy.StartArrayIndex.X, enemy.StartArrayIndex.Y];
				if (cClientTile != null)
				{
					s_Choreographer.m_ClientEnemies.Add(s_Choreographer.CreateCharacterActor(cClientTile, enemy, isSummoned: true));
				}
			}
		}
		foreach (CEnemyActor allyMonster in ScenarioManager.Scenario.AllyMonsters)
		{
			if (!s_Choreographer.FindClientEnemy(allyMonster) && !allyMonster.PhasedOut && (summonedActors == null || summonedActors.Contains(allyMonster)))
			{
				CClientTile cClientTile2 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[allyMonster.StartArrayIndex.X, allyMonster.StartArrayIndex.Y];
				if (cClientTile2 != null)
				{
					s_Choreographer.m_ClientAllyMonsters.Add(s_Choreographer.CreateCharacterActor(cClientTile2, allyMonster, isSummoned: true));
				}
			}
		}
		foreach (CEnemyActor enemy2Monster in ScenarioManager.Scenario.Enemy2Monsters)
		{
			if (!s_Choreographer.FindClientEnemy(enemy2Monster) && !enemy2Monster.PhasedOut && (summonedActors == null || summonedActors.Contains(enemy2Monster)))
			{
				CClientTile cClientTile3 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[enemy2Monster.StartArrayIndex.X, enemy2Monster.StartArrayIndex.Y];
				if (cClientTile3 != null)
				{
					s_Choreographer.m_ClientEnemy2Monsters.Add(s_Choreographer.CreateCharacterActor(cClientTile3, enemy2Monster, isSummoned: true));
				}
			}
		}
		foreach (CEnemyActor neutralMonster in ScenarioManager.Scenario.NeutralMonsters)
		{
			if (!s_Choreographer.FindClientEnemy(neutralMonster) && !neutralMonster.PhasedOut && (summonedActors == null || summonedActors.Contains(neutralMonster)))
			{
				CClientTile cClientTile4 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[neutralMonster.StartArrayIndex.X, neutralMonster.StartArrayIndex.Y];
				if (cClientTile4 != null)
				{
					s_Choreographer.m_ClientNeutralMonsters.Add(s_Choreographer.CreateCharacterActor(cClientTile4, neutralMonster, isSummoned: true));
				}
			}
		}
		foreach (CHeroSummonActor heroSummon in ScenarioManager.Scenario.HeroSummons)
		{
			if (!s_Choreographer.FindClientHeroSummon(heroSummon) && !heroSummon.PhasedOut && (summonedActors == null || summonedActors.Contains(heroSummon)))
			{
				CClientTile cClientTile5 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[heroSummon.StartArrayIndex.X, heroSummon.StartArrayIndex.Y];
				if (cClientTile5 != null)
				{
					s_Choreographer.m_ClientHeroSummons.Add(s_Choreographer.CreateCharacterActor(cClientTile5, heroSummon, isSummoned: true));
				}
			}
		}
		foreach (CObjectActor @object in ScenarioManager.Scenario.Objects)
		{
			if (!s_Choreographer.FindClientObjectActor(@object) && !@object.PhasedOut && (summonedActors == null || summonedActors.Contains(@object)))
			{
				CClientTile cClientTile6 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[@object.StartArrayIndex.X, @object.StartArrayIndex.Y];
				if (cClientTile6 != null)
				{
					s_Choreographer.m_ClientObjects.Add(s_Choreographer.CreateCharacterActor(cClientTile6, @object, isSummoned: true));
				}
			}
		}
	}
}
