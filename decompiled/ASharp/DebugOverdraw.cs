using UnityEngine;

public class DebugOverdraw : MonoBehaviour
{
	[SerializeField]
	private Shader _overdrawShader;

	private Camera _camera;

	private bool _originalFogSettings;

	private void OnEnable()
	{
		hideFlags = HideFlags.DontSave;
		_overdrawShader = Shader.Find("DebugOverdraw");
		if (_overdrawShader == null)
		{
			Debug.LogError("Can't debug overdraw! Shader not found!");
			return;
		}
		_camera = GetComponent<Camera>();
		if (_camera == null)
		{
			Debug.LogError("Can't debug overdraw! DebugOverdraw not added to a GameObject with Camera!");
			return;
		}
		RenderSettings.fog = false;
		_camera.SetReplacementShader(_overdrawShader, "");
	}

	private void OnDisable()
	{
		if (_camera != null)
		{
			RenderSettings.fog = _originalFogSettings;
			_camera.SetReplacementShader(null, "");
		}
	}
}
