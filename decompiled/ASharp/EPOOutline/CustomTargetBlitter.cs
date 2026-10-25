using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace EPOOutline;

[ExecuteAlways]
public class CustomTargetBlitter : MonoBehaviour
{
	[SerializeField]
	private string customTargetName;

	private CommandBuffer buffer;

	private Material blitMaterial;

	private Coroutine blitProcess;

	private void Awake()
	{
		buffer = new CommandBuffer();
	}

	private void OnEnable()
	{
		StopAllCoroutines();
		blitProcess = StartCoroutine(Blit());
	}

	private void Update()
	{
		if (blitProcess == null)
		{
			StopAllCoroutines();
			blitProcess = StartCoroutine(Blit());
		}
	}

	private void OnDestroy()
	{
		if (buffer != null)
		{
			buffer.Dispose();
		}
	}

	private void CheckMaterial()
	{
		if (blitMaterial == null)
		{
			blitMaterial = new Material(OutlineEffect.LoadMaterial("TransparentBlit"));
		}
	}

	private IEnumerator Blit()
	{
		WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
		if (buffer == null)
		{
			buffer = new CommandBuffer();
			buffer.name = "Custom target blitter";
		}
		while (enabled)
		{
			CheckMaterial();
			yield return waitForEndOfFrame;
			RenderTexture allocatedTarget = TargetsHolder.Instance.GetAllocatedTarget(customTargetName);
			if (!(allocatedTarget == null))
			{
				buffer.Clear();
				buffer.Blit(allocatedTarget, BuiltinRenderTextureType.None, blitMaterial);
				Graphics.ExecuteCommandBuffer(buffer);
			}
		}
	}
}
