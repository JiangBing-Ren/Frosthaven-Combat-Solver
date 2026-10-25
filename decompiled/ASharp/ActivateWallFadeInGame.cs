using UnityEngine;

[ExecuteInEditMode]
public class ActivateWallFadeInGame : MonoBehaviour
{
	public bool manualToggleOn;

	public bool manualToggleOff;

	private bool t = true;

	private void Start()
	{
		Shader.SetGlobalInt("ToggleWallFade", 1);
	}

	private void Update()
	{
		if (manualToggleOn)
		{
			Shader.SetGlobalInt("ToggleWallFade", 1);
			manualToggleOn = false;
		}
		if (manualToggleOff)
		{
			Shader.SetGlobalInt("ToggleWallFade", 0);
			manualToggleOff = false;
		}
		if (t && !Application.isPlaying)
		{
			Shader.SetGlobalInt("ToggleWallFade", 0);
			t = false;
		}
	}
}
