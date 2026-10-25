using ScenarioRuleLibrary;
using UnityEngine;

internal class ScenarioScriptFrontend_FH132 : ScenarioScriptFrontend
{
	private bool m_Invalidate = true;

	private ScenarioScript_FH132 _script132 => (ScenarioScript_FH132)_script;

	protected override void InitializeInternal()
	{
		_script132.OnAltarStateChanged = () =>
		{
			m_Invalidate = true;
		};
		_script132.OnAltarStateChanged();
	}

	protected override void TickInternal()
	{
		if (!m_Invalidate)
		{
			return;
		}
		m_Invalidate = false;
		CObjectObstacle cObjectObstacle = ScenarioManager.GetTile(_script132.AltarPosition).FindObstacleProp();
		if (cObjectObstacle == null)
		{
			return;
		}
		GameObject gameObject = Choreographer.FindPropGameObject(cObjectObstacle);
		gameObject.FindInChildren("EN_RH_Obstacle_1Hex_Cat", includeInactive: true)?.SetActive(!_script132.AltarLooted);
		ParticleSystem[] componentsInChildren;
		if (_script132.AltarLooted)
		{
			componentsInChildren = gameObject.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Stop(withChildren: true);
			}
			return;
		}
		componentsInChildren = gameObject.GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem particleSystem in componentsInChildren)
		{
			if (!particleSystem.isPlaying)
			{
				particleSystem.Play(withChildren: true);
			}
		}
	}
}
