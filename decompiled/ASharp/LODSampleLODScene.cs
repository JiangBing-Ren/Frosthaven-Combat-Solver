using System;
using System.Collections.Generic;
using UnityEngine;

public class LODSampleLODScene : MonoBehaviour
{
	[Serializable]
	public class SceneCamera
	{
		public Camera m_camera;

		public float m_near;

		public float m_far;

		[HideInInspector]
		public Vector3 m_v3InitialCameraPosition;

		[HideInInspector]
		public Vector3 m_v3ViewDir;
	}

	public SceneCamera[] SceneCameras;

	public Material WireframeMaterial;

	private Dictionary<GameObject, Material[]> m_objectMaterials;

	private SceneCamera m_selectedCamera;

	private bool m_bWireframe;

	private List<AutomaticLOD> m_sceneLODObjects;

	private int m_nMaxLODLevels;

	private float m_fCurrentDistanceSlider;

	private int m_nCamMode;

	private void Start()
	{
		AutomaticLOD[] array = UnityEngine.Object.FindObjectsOfType<AutomaticLOD>();
		m_sceneLODObjects = new List<AutomaticLOD>();
		m_objectMaterials = new Dictionary<GameObject, Material[]>();
		m_nMaxLODLevels = 0;
		AutomaticLOD[] array2 = array;
		foreach (AutomaticLOD automaticLOD in array2)
		{
			if (automaticLOD.IsRootAutomaticLOD())
			{
				m_sceneLODObjects.Add(automaticLOD);
				if (automaticLOD.GetLODLevelCount() > m_nMaxLODLevels)
				{
					m_nMaxLODLevels = automaticLOD.GetLODLevelCount();
				}
				AddMaterials(automaticLOD.gameObject, m_objectMaterials);
			}
		}
		if (SceneCameras != null && SceneCameras.Length != 0)
		{
			SceneCamera[] sceneCameras = SceneCameras;
			foreach (SceneCamera obj in sceneCameras)
			{
				obj.m_v3InitialCameraPosition = obj.m_camera.transform.position;
				obj.m_v3ViewDir = obj.m_camera.transform.forward;
			}
			SetActiveCamera(0);
		}
		m_bWireframe = false;
	}

	private void Update()
	{
		m_nCamMode = 0;
		if (Input.GetKey(KeyCode.I))
		{
			m_nCamMode = 1;
		}
		else if (Input.GetKey(KeyCode.O))
		{
			m_nCamMode = -1;
		}
		if (m_nCamMode != 0)
		{
			m_fCurrentDistanceSlider -= Time.deltaTime * 0.1f * (float)m_nCamMode;
			m_fCurrentDistanceSlider = Mathf.Clamp01(m_fCurrentDistanceSlider);
			UpdateCamera(m_fCurrentDistanceSlider);
		}
		if (Input.GetKeyDown(KeyCode.W))
		{
			m_bWireframe = !m_bWireframe;
			SetWireframe(m_bWireframe);
		}
	}

	private void OnGUI()
	{
		int num = 400;
		if (SceneCameras == null || SceneCameras.Length == 0)
		{
			return;
		}
		GUI.Box(new Rect(0f, 0f, num + 10, 260f), "");
		GUILayout.Space(20f);
		GUILayout.Label("Select camera:", GUILayout.Width(num));
		GUILayout.BeginHorizontal();
		for (int i = 0; i < SceneCameras.Length; i++)
		{
			if (GUILayout.Button(SceneCameras[i].m_camera.name))
			{
				SetActiveCamera(i);
			}
		}
		GUILayout.EndHorizontal();
		GUILayout.Label("Camera distance:", GUILayout.Width(num));
		GUI.changed = false;
		m_fCurrentDistanceSlider = GUILayout.HorizontalSlider(m_fCurrentDistanceSlider, 0f, 1f);
		if (GUI.changed)
		{
			UpdateCamera(m_fCurrentDistanceSlider);
		}
		GUI.changed = false;
		m_bWireframe = GUILayout.Toggle(m_bWireframe, "Show wireframe");
		if (GUI.changed)
		{
			SetWireframe(m_bWireframe);
		}
		GUILayout.Space(20f);
		GUILayout.Label("Select LOD:");
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Automatic LOD"))
		{
			foreach (AutomaticLOD sceneLODObject in m_sceneLODObjects)
			{
				sceneLODObject.SetAutomaticCameraLODSwitch(bEnabled: true);
			}
		}
		for (int j = 0; j < m_nMaxLODLevels; j++)
		{
			if (!GUILayout.Button("LOD " + j))
			{
				continue;
			}
			foreach (AutomaticLOD sceneLODObject2 in m_sceneLODObjects)
			{
				sceneLODObject2.SetAutomaticCameraLODSwitch(bEnabled: false);
				sceneLODObject2.SwitchToLOD(j, bRecurseIntoChildren: true);
			}
		}
		GUILayout.EndHorizontal();
		GUILayout.Space(20f);
		int num2 = 0;
		int num3 = 0;
		foreach (AutomaticLOD sceneLODObject3 in m_sceneLODObjects)
		{
			num2 += sceneLODObject3.GetCurrentVertexCount(bRecurseIntoChildren: true);
			num3 += sceneLODObject3.GetOriginalVertexCount(bRecurseIntoChildren: true);
		}
		GUILayout.Label("Vertex count: " + num2 + "/" + num3 + " " + Mathf.RoundToInt(100f * ((float)num2 / (float)num3)) + "%");
		GUILayout.Space(20f);
	}

	private void SetActiveCamera(int index)
	{
		SceneCamera[] sceneCameras = SceneCameras;
		for (int i = 0; i < sceneCameras.Length; i++)
		{
			sceneCameras[i].m_camera.gameObject.SetActive(value: false);
		}
		m_selectedCamera = SceneCameras[index];
		m_selectedCamera.m_camera.gameObject.SetActive(value: true);
		m_selectedCamera.m_camera.transform.position = m_selectedCamera.m_v3InitialCameraPosition;
		m_fCurrentDistanceSlider = m_selectedCamera.m_near / (m_selectedCamera.m_near - m_selectedCamera.m_far);
	}

	private void UpdateCamera(float fPos)
	{
		Vector3 position = Vector3.Lerp(m_selectedCamera.m_v3InitialCameraPosition + m_selectedCamera.m_v3ViewDir * m_selectedCamera.m_near, m_selectedCamera.m_v3InitialCameraPosition + m_selectedCamera.m_v3ViewDir * m_selectedCamera.m_far, fPos);
		m_selectedCamera.m_camera.transform.position = position;
	}

	private void AddMaterials(GameObject theGameObject, Dictionary<GameObject, Material[]> dicMaterials)
	{
		Renderer component = theGameObject.GetComponent<Renderer>();
		AutomaticLOD component2 = theGameObject.GetComponent<AutomaticLOD>();
		if (component != null && component.sharedMaterials != null && component2 != null && component2 != null)
		{
			dicMaterials.Add(theGameObject, component.sharedMaterials);
		}
		for (int i = 0; i < theGameObject.transform.childCount; i++)
		{
			AddMaterials(theGameObject.transform.GetChild(i).gameObject, dicMaterials);
		}
	}

	private void SetWireframe(bool bEnabled)
	{
		m_bWireframe = bEnabled;
		foreach (KeyValuePair<GameObject, Material[]> objectMaterial in m_objectMaterials)
		{
			Renderer component = objectMaterial.Key.GetComponent<Renderer>();
			if (bEnabled)
			{
				Material[] array = new Material[objectMaterial.Value.Length];
				for (int i = 0; i < objectMaterial.Value.Length; i++)
				{
					array[i] = WireframeMaterial;
				}
				component.sharedMaterials = array;
			}
			else
			{
				component.sharedMaterials = objectMaterial.Value;
			}
		}
	}
}
