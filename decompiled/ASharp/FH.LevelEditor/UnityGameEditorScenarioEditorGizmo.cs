using System.Collections;
using Libs.Utils;
using UnityEngine;

namespace FH.LevelEditor;

[ExecuteAlways]
public class UnityGameEditorScenarioEditorGizmo : MonoBehaviour
{
	public Renderer[] Gizmos;

	private bool shouldEnable;

	private void OnEnable()
	{
		bool flag = LevelEditorController.s_Instance?.IsEditing ?? false;
		string layer = (flag ? "Default" : "LevelEditorPlane");
		Gizmos.ForEach((Renderer g) =>
		{
			g.gameObject.layer = LayerMask.NameToLayer(layer);
		});
		if (flag)
		{
			StartCoroutine(EnsureGizmoState());
		}
	}

	private IEnumerator EnsureGizmoState()
	{
		while (enabled)
		{
			yield return new WaitForSecondsRealtime(2f);
			ApplyEnableStateToRenderers();
		}
	}

	private void ApplyEnableStateToRenderers()
	{
		Renderer[] gizmos = Gizmos;
		for (int i = 0; i < gizmos.Length; i++)
		{
			gizmos[i].enabled = shouldEnable && Application.isPlaying;
		}
	}

	[ContextMenu("Show")]
	public void Show()
	{
		shouldEnable = true;
		ApplyEnableStateToRenderers();
	}

	[ContextMenu("Hide")]
	public void Hide()
	{
		shouldEnable = false;
		ApplyEnableStateToRenderers();
	}
}
