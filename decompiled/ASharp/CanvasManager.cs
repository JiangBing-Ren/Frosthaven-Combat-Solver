using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Canvas))]
public class CanvasManager : MonoBehaviour
{
	[SerializeField]
	private Canvas persistentUICanvas;

	[SerializeField]
	private Canvas tooltipCanvas;

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if ((bool)UIManager.Instance)
		{
			Canvas canvas = persistentUICanvas;
			Camera worldCamera = (tooltipCanvas.worldCamera = UIManager.Instance.UICamera);
			canvas.worldCamera = worldCamera;
		}
	}
}
