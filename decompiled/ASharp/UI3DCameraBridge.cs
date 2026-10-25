using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class UI3DCameraBridge : MonoBehaviour
{
	public RawImage TargetTexture;

	public Camera Camera;

	public UI3DCharacterDisplayEventsChannel DisplayEvents;

	private RenderTexture tex;

	private void Awake()
	{
		Rect rect = TargetTexture.rectTransform.rect;
		tex = new RenderTexture(Mathf.FloorToInt(rect.width * 1.5f), Mathf.FloorToInt(rect.height * 1.5f), 24, RenderTextureFormat.ARGBHalf);
		TargetTexture.texture = tex;
		Camera.targetTexture = tex;
		Camera.GetComponent<UniversalAdditionalCameraData>().antialiasing = AntialiasingMode.FastApproximateAntialiasing;
		if (DisplayEvents != null)
		{
			DisplayEvents.OnDisplay.AddListener((ECharacter _, string _, string _) =>
			{
				TargetTexture.enabled = true;
			});
			DisplayEvents.OnHide.AddListener(() =>
			{
				TargetTexture.enabled = false;
			});
		}
		else
		{
			TargetTexture.enabled = true;
		}
	}

	private void OnDestroy()
	{
		if (tex != null)
		{
			tex.Release();
			Object.Destroy(tex);
		}
	}
}
