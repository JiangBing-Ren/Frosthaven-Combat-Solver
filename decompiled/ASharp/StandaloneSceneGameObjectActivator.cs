using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1000)]
public class StandaloneSceneGameObjectActivator : MonoBehaviour
{
	[SerializeField]
	private List<GameObject> _gameObjects;

	private void Awake()
	{
		if (!Application.isEditor)
		{
			for (int num = _gameObjects.Count - 1; num >= 0; num--)
			{
				Object.Destroy(_gameObjects[num]);
			}
			Object.Destroy(gameObject);
			return;
		}
		if (SceneManager.sceneCount == 1)
		{
			foreach (GameObject gameObject in _gameObjects)
			{
				gameObject.SetActive(value: true);
			}
			return;
		}
		for (int num2 = _gameObjects.Count - 1; num2 >= 0; num2--)
		{
			Object.Destroy(_gameObjects[num2]);
		}
	}
}
