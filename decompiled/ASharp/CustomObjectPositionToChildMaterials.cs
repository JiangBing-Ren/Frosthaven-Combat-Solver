using System;
using SETUtil.Materials;
using ScenarioRuleLibrary;
using UnityEngine;

[Obsolete]
public class CustomObjectPositionToChildMaterials : MonoBehaviour
{
	public GameObject fadeSourceObject;

	public bool enableFade;

	private Renderer[] childRenderers;

	private bool isFading;

	private bool toggleFadeProperty = true;

	public Renderer[] overrideChildRenderers;

	private void Start()
	{
		bool flag = true;
		if (overrideChildRenderers.Length >= 1)
		{
			flag = false;
		}
		if (flag)
		{
			childRenderers = GetComponentsInChildren<Renderer>();
		}
		else
		{
			childRenderers = overrideChildRenderers;
		}
		UnityGameEditorObject componentInParent = GetComponentInParent<UnityGameEditorObject>();
		if (componentInParent == null || componentInParent.m_ObjectType != ScenarioManager.ObjectImportType.Obstacle)
		{
			return;
		}
		CObjectObstacle cObjectObstacle = (CObjectObstacle)componentInParent.PropObject;
		if (!ScenarioManager.CurrentScenarioState.TransparentProps.Contains(cObjectObstacle))
		{
			return;
		}
		CActor cActor = null;
		foreach (TileIndex pathingBlocker in cObjectObstacle.PathingBlockers)
		{
			CTile cTile = ScenarioManager.Tiles[pathingBlocker.X, pathingBlocker.Y];
			CActor cActor2 = ScenarioManager.Scenario.FindActorAt(cTile.m_ArrayIndex);
			if (cActor2 != null)
			{
				cActor = cActor2;
			}
		}
		if (cActor == null)
		{
			ScenarioManager.CurrentScenarioState.TransparentProps.Remove(cObjectObstacle);
			fadeSourceObject = null;
			enableFade = false;
		}
		else
		{
			fadeSourceObject = Choreographer.s_Choreographer.FindClientActorGameObject(cActor);
			enableFade = true;
		}
	}

	private void Update()
	{
		if (enableFade && fadeSourceObject != null && childRenderers.Length != 0)
		{
			isFading = true;
			_ = fadeSourceObject.transform.position;
		}
		else
		{
			if (enableFade || !isFading)
			{
				return;
			}
			isFading = false;
			Renderer[] array = childRenderers;
			foreach (Renderer obj in array)
			{
				Material[] materialClonesSlow = obj.GetMaterialClonesSlow();
				if (!(obj == null) && materialClonesSlow.Length != 0)
				{
					materialClonesSlow[0].SetFloat("_ToggleFadeSourcePos", 0f);
				}
			}
			toggleFadeProperty = true;
		}
	}
}
