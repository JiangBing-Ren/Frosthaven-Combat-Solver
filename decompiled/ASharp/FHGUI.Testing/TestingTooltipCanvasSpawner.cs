using UnityEngine;

namespace FHGUI.Testing;

public class TestingTooltipCanvasSpawner : MonoBehaviour
{
	public GameObject TooltipCanvasPrefab;

	public GameObject UICameraPrefab;

	private void Awake()
	{
		Canvas component = Object.Instantiate(TooltipCanvasPrefab).GetComponent<Canvas>();
		Camera worldCamera = ((UIManager.Instance != null) ? UIManager.Instance.UICamera : Object.Instantiate(UICameraPrefab).GetComponent<Camera>());
		component.worldCamera = worldCamera;
	}
}
