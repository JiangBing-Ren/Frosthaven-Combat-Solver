using ScenarioRuleLibrary;
using UnityEngine;

internal class ScenarioScriptFrontend_FH005 : ScenarioScriptFrontend
{
	private ScenarioScript_FH005 _script005 => (ScenarioScript_FH005)_script;

	protected override void InitializeInternal()
	{
	}

	protected override void TickInternal()
	{
		Vector3 vector = Vector3.zero;
		foreach (GameObject clientMonsterObject in Choreographer.s_Choreographer.ClientMonsterObjects)
		{
			CActor actor = ActorBehaviour.GetActorBehaviour(clientMonsterObject).Actor;
			if (_script005.IsAltar(actor))
			{
				vector = clientMonsterObject.transform.position;
				break;
			}
		}
		if (vector == Vector3.zero)
		{
			return;
		}
		foreach (GameObject clientMonsterObject2 in Choreographer.s_Choreographer.ClientMonsterObjects)
		{
			CActor actor2 = ActorBehaviour.GetActorBehaviour(clientMonsterObject2).Actor;
			if (_script005.IsCorpse(actor2))
			{
				Vector3 vector2 = vector;
				Vector3 position = clientMonsterObject2.transform.position;
				vector2.y = (position.y = 0f);
				Vector3 dir = position - vector2;
				float magnitude = dir.magnitude;
				float num = 0.2f / magnitude;
				_ = dir.magnitude / magnitude;
				for (float num2 = 0f; num2 <= 1f; num2 += num)
				{
					arrow(Vector3.Lerp(vector2, position, num2), dir);
				}
			}
		}
		static void arrow(Vector3 pos, Vector3 normalized)
		{
			normalized = normalized.normalized;
			float num3 = Mathf.Repeat(Time.unscaledTime, 0.2f);
			Vector3 vector3 = pos + normalized * num3;
			Vector3 b = vector3 - 0.2f * (normalized + new Vector3(normalized.z, 0f, 0f - normalized.x));
			Vector3 b2 = vector3 - 0.2f * (normalized - new Vector3(normalized.z, 0f, 0f - normalized.x));
			Color red = Color.red;
			red.a = 0.75f;
			QGL.LateDrawLineWorld(vector3, b, red);
			QGL.LateDrawLineWorld(vector3, b2, red);
		}
	}
}
