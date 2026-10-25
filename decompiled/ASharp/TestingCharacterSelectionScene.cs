using FHGUI.DTO;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class TestingCharacterSelectionScene : MonoBehaviour
{
	public Camera UICamera;

	public UIPartySlot Slot;

	public FHCharacterSelectionScene Scene;

	public EditorSceneDirectPlaySpawner Spawner;

	[SerializeField]
	private ECharacter _character = ECharacter.BannerSpear;

	private void OnEnable()
	{
		Scene.Show(_character);
	}

	private void Awake()
	{
		if (SceneManager.sceneCount > 1)
		{
			Cleanup();
			return;
		}
		UniversalAdditionalCameraData component = Scene.Camera.GetComponent<UniversalAdditionalCameraData>();
		component.cameraStack.Clear();
		component.cameraStack.Add(UICamera);
		Spawner.gameObject.SetActive(value: true);
		Scene.Show(_character);
		Slot.Init(new PartySlotDTO
		{
			Character = _character,
			Name = _character.ToString()
		});
	}

	private void Cleanup()
	{
		Object.Destroy(UICamera.gameObject);
		Object.Destroy(gameObject);
	}
}
