using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FH.LevelComponents.ProcGen;
using Libs.Penumbra;
using SETUtil.Types;
using UnityEngine;
using UnityEngine.Scripting;

namespace FH.Integrations.Penumbra;

[Preserve]
internal class PenumbraAction_PositionScenarioCamera : PenumbraAction
{
	public override int Priority => 10;

	public override Filtering Binding => new Filtering(Filtering.Type.FilterContains, "ScenarioProcessing");

	public override IEnumerator RunCrt(PenumbraActionParams parameters, PenumbraJob runningJob, PenumbraJobResults outResults)
	{
		ProceduralScenario proceduralScenario = null;
		int attemptToFindScenario = 10;
		while (proceduralScenario == null && attemptToFindScenario-- > 0)
		{
			proceduralScenario = UnityEngine.Object.FindObjectOfType<ProceduralScenario>();
			yield return new WaitForSeconds(1f);
		}
		GameObject gameObject = GameObject.Find("Apparance Engine");
		float safetyTimeStart = Time.timeSinceLevelLoad;
		ApparanceEngine component;
		if (!proceduralScenario || !gameObject || !(component = gameObject.GetComponent<ApparanceEngine>()) || !component.enabled)
		{
			yield break;
		}
		IEnumerable<ProceduralMapTile> proc = proceduralScenario.MapTiles.Where((ProceduralMapTile a) => a).SelectMany((ProceduralMapTile a) => a.GetComponentsInChildren<ProceduralMapTile>());
		while (proc.Any((ProceduralMapTile a) => !a.ContentComplete))
		{
			if (Time.timeSinceLevelLoad - safetyTimeStart > 30f)
			{
				parameters.LogActionError(this, "Error - Exceeded total allowed time to wait for level generation. Stopped by safety check.");
				yield break;
			}
			yield return null;
		}
		CameraController cameraController = null;
		int attemptToFindCamera = 10;
		while (cameraController == null && attemptToFindCamera-- > 0)
		{
			cameraController = UnityEngine.Object.FindObjectOfType<CameraController>();
			yield return new WaitForSeconds(1f);
		}
		if ((bool)cameraController)
		{
			SetCam(parameters.Logger, (from a in proceduralScenario.MapTiles
				where a
				select a.GetComponent<ProceduralMapTile>()).SelectMany((ProceduralMapTile a) => a.GetComponentsInChildren<Renderer>()), cameraController);
		}
	}

	private static void SetCam(ILogger logger, IEnumerable<Renderer> renderers, CameraController cameraController)
	{
		if (!cameraController)
		{
			logger.LogError("Error", "No camera controller");
			return;
		}
		Bounds bounds = default;
		foreach (Renderer renderer in renderers)
		{
			if ((bool)renderer)
			{
				Bounds bounds2 = renderer.bounds;
				if (bounds == default(Bounds))
				{
					bounds = bounds2;
				}
				else
				{
					bounds.Encapsulate(bounds2);
				}
			}
		}
		float num = (cameraController.m_Camera ? cameraController.m_Camera.fieldOfView : cameraController.m_DefaultFOV);
		logger.Log($"Camera FOV: {num}");
		float num2 = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z) / (2f * Mathf.Tan(0.5f * num * (MathF.PI / 180f)));
		Vector3 vector = bounds.center + num2 * (Vector3.up + Vector3.back).normalized * 0.5f;
		cameraController.SetFreeCamPosition(new TransformData(vector, Quaternion.LookRotation(bounds.center - vector)));
	}
}
