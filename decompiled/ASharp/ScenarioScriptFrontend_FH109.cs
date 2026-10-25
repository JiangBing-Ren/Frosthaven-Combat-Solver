using System.Collections.Generic;
using AStar;
using ClockStone;
using ScenarioRuleLibrary;
using UnityEngine;

internal class ScenarioScriptFrontend_FH109 : ScenarioScriptFrontend
{
	private Dictionary<GameObject, AudioObject> _trackRotationAudio = new Dictionary<GameObject, AudioObject>();

	private ScenarioScript_FH109 _script109 => (ScenarioScript_FH109)_script;

	protected override void InitializeInternal()
	{
	}

	protected override void TickInternal()
	{
		foreach (KeyValuePair<CObjectActor, ScenarioScript_FH109.LaserInfo> laserInfo in _script109.LaserInfos)
		{
			CObjectActor key = laserInfo.Key;
			Point interactingTile = key.InteractingTile;
			GameObject gameObject = Choreographer.s_Choreographer.FindClientActorGameObject(key);
			ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(gameObject);
			if (actorBehaviour == null)
			{
				continue;
			}
			if (laserInfo.Value.startedRotating)
			{
				Vector3 vector = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[interactingTile.X, interactingTile.Y].m_GameObject.transform.position - actorBehaviour.m_RootGameObject.transform.position;
				Quaternion quaternion = Quaternion.LookRotation(new Vector3(vector.x, 0f, vector.z), Vector3.up);
				Quaternion quaternion2 = Quaternion.RotateTowards(actorBehaviour.m_RootGameObject.transform.rotation, quaternion, Time.deltaTime * 120f);
				bool flag = Quaternion.Angle(quaternion2, quaternion) >= 0.01f;
				actorBehaviour.m_RootGameObject.transform.rotation = quaternion2;
				if (!flag)
				{
					laserInfo.Value.finishedRotating = true;
				}
				else if (!_trackRotationAudio.ContainsKey(gameObject) || _trackRotationAudio[gameObject] == null || !_trackRotationAudio[gameObject].IsPlaying())
				{
					_trackRotationAudio[gameObject] = AudioController.Play("Laser_TurnAttacked", gameObject);
				}
			}
			else
			{
				actorBehaviour.RotateTowardsInteractTile();
			}
		}
	}
}
