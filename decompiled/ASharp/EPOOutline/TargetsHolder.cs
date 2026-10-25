using System.Collections.Generic;
using UnityEngine;

namespace EPOOutline;

[ExecuteAlways]
public class TargetsHolder : MonoBehaviour
{
	private static TargetsHolder instance;

	private Dictionary<string, RenderTexture> targets = new Dictionary<string, RenderTexture>();

	public static TargetsHolder Instance
	{
		get
		{
			if (instance == null)
			{
				GameObject gameObject = new GameObject("TargetsHolder");
				instance = gameObject.AddComponent<TargetsHolder>();
				gameObject.hideFlags = HideFlags.HideAndDontSave;
			}
			return instance;
		}
	}

	private void OnDestroy()
	{
		ReleaseTargets();
	}

	public RenderTexture GetAllocatedTarget(string name)
	{
		RenderTexture value = null;
		if (!targets.TryGetValue(name, out value))
		{
			return null;
		}
		return value;
	}

	public RenderTexture GetTarget(OutlineParameters parameters, string name)
	{
		RenderTexture value = null;
		if (!targets.TryGetValue(name, out value))
		{
			RenderTargetUtility.RenderTextureInfo targetInfo = RenderTargetUtility.GetTargetInfo(parameters, parameters.TargetWidth, parameters.TargetHeight, 24, forceNoAA: false, noFiltering: false);
			value = RenderTexture.GetTemporary(targetInfo.Descriptor);
			value.filterMode = targetInfo.FilterMode;
			targets.Add(name, value);
			return value;
		}
		Shader.SetGlobalTexture(name, value);
		return value;
	}

	private void ReleaseTargets()
	{
		RenderTexture.active = null;
		foreach (KeyValuePair<string, RenderTexture> target in targets)
		{
			if (!(target.Value == null))
			{
				Graphics.SetRenderTarget(target.Value);
				GL.Clear(clearDepth: true, clearColor: true, Color.clear);
				RenderTexture.ReleaseTemporary(target.Value);
			}
		}
		targets.Clear();
	}

	private void Update()
	{
		ReleaseTargets();
	}
}
