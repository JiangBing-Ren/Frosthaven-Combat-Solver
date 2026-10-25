using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LODSceneSelection : MonoBehaviour
{
	[Serializable]
	public class SceneOption
	{
		public string m_sceneName;

		public string m_sceneDisplayName;
	}

	public int BoxWidth = 300;

	public int BoxHeight = 50;

	public int MarginH = 20;

	public int MarginV = 20;

	public SceneOption[] SceneOptions;

	private void OnGUI()
	{
		Rect position = new Rect(Screen.width / 2 - BoxWidth / 2, 0f, BoxWidth, BoxHeight);
		Rect screenRect = new Rect(position.x + (float)MarginH, position.y + (float)MarginV, BoxWidth - MarginH * 2, BoxHeight - MarginV * 2);
		GUI.Box(position, "");
		GUI.Box(position, "");
		GUILayout.BeginArea(screenRect);
		GUILayout.Label("Scene selection:");
		GUILayout.BeginHorizontal();
		SceneOption[] sceneOptions = SceneOptions;
		foreach (SceneOption sceneOption in sceneOptions)
		{
			if (GUILayout.Button(sceneOption.m_sceneDisplayName))
			{
				SceneManager.LoadScene(sceneOption.m_sceneName);
			}
		}
		if (GUILayout.Button("Exit"))
		{
			Application.Quit();
		}
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
}
