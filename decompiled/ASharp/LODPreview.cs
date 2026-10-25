using System;
using System.Collections;
using System.Collections.Generic;
using UltimateGameTools.MeshSimplifier;
using UnityEngine;

public class LODPreview : MonoBehaviour
{
	[Serializable]
	public class ShowcaseObject
	{
		public AutomaticLOD m_automaticLOD;

		public Vector3 m_position;

		public Vector3 m_angles;

		public Vector3 m_rotationAxis = Vector3.up;

		public string m_description;
	}

	public ShowcaseObject[] ShowcaseObjects;

	public Material WireframeMaterial;

	public float MouseSensitvity = 0.3f;

	public float MouseReleaseSpeed = 3f;

	private Dictionary<GameObject, Material[]> m_objectMaterials;

	private AutomaticLOD m_selectedAutomaticLOD;

	private int m_nSelectedIndex = -1;

	private bool m_bWireframe;

	private float m_fRotationSpeed = 10f;

	private float m_fLastMouseX;

	private Mesh m_newMesh;

	private int m_nLastProgress = -1;

	private string m_strLastTitle = "";

	private string m_strLastMessage = "";

	private float m_fVertexAmount = 1f;

	private void Start()
	{
		if (ShowcaseObjects != null && ShowcaseObjects.Length != 0)
		{
			for (int i = 0; i < ShowcaseObjects.Length; i++)
			{
				ShowcaseObjects[i].m_description = ShowcaseObjects[i].m_description.Replace("\\n", Environment.NewLine);
			}
			SetActiveObject(0);
		}
		Simplifier.CoroutineFrameMiliseconds = 20;
	}

	private void Progress(string strTitle, string strMessage, float fT)
	{
		int num = Mathf.RoundToInt(fT * 100f);
		if (num != m_nLastProgress || m_strLastTitle != strTitle || m_strLastMessage != strMessage)
		{
			m_strLastTitle = strTitle;
			m_strLastMessage = strMessage;
			m_nLastProgress = num;
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.W))
		{
			m_bWireframe = !m_bWireframe;
			SetWireframe(m_bWireframe);
		}
		if (m_selectedAutomaticLOD != null)
		{
			if (Input.GetMouseButton(0) && Input.mousePosition.y > 100f)
			{
				Vector3 eulers = ShowcaseObjects[m_nSelectedIndex].m_rotationAxis * (0f - (Input.mousePosition.x - m_fLastMouseX) * MouseSensitvity);
				m_selectedAutomaticLOD.transform.Rotate(eulers, Space.Self);
			}
			else if (Input.GetMouseButtonUp(0) && Input.mousePosition.y > 100f)
			{
				m_fRotationSpeed = (0f - (Input.mousePosition.x - m_fLastMouseX)) * MouseReleaseSpeed;
			}
			else
			{
				Vector3 eulers2 = ShowcaseObjects[m_nSelectedIndex].m_rotationAxis * (m_fRotationSpeed * Time.deltaTime);
				m_selectedAutomaticLOD.transform.Rotate(eulers2, Space.Self);
			}
		}
		m_fLastMouseX = Input.mousePosition.x;
	}

	private void OnGUI()
	{
		int num = 400;
		if (ShowcaseObjects == null)
		{
			return;
		}
		bool flag = true;
		if (!string.IsNullOrEmpty(m_strLastTitle) && !string.IsNullOrEmpty(m_strLastMessage))
		{
			flag = false;
		}
		GUI.Box(new Rect(0f, 0f, num + 10, 430f), "");
		GUILayout.Label("Select model:", GUILayout.Width(num));
		GUILayout.BeginHorizontal();
		for (int i = 0; i < ShowcaseObjects.Length; i++)
		{
			if (GUILayout.Button(ShowcaseObjects[i].m_automaticLOD.name) & flag)
			{
				if (m_selectedAutomaticLOD != null)
				{
					UnityEngine.Object.DestroyImmediate(m_selectedAutomaticLOD.gameObject);
				}
				SetActiveObject(i);
			}
		}
		GUILayout.EndHorizontal();
		if (!(m_selectedAutomaticLOD != null))
		{
			return;
		}
		GUILayout.Space(20f);
		GUILayout.Label(ShowcaseObjects[m_nSelectedIndex].m_description);
		GUILayout.Space(20f);
		GUI.changed = false;
		m_bWireframe = GUILayout.Toggle(m_bWireframe, "Show wireframe");
		if (GUI.changed && m_selectedAutomaticLOD != null)
		{
			SetWireframe(m_bWireframe);
		}
		GUILayout.Space(20f);
		GUILayout.Label("Select predefined LOD:");
		GUILayout.BeginHorizontal();
		for (int j = 0; j < m_selectedAutomaticLOD.GetLODLevelCount(); j++)
		{
			if (GUILayout.Button("LOD " + j) & flag)
			{
				m_selectedAutomaticLOD.SwitchToLOD(j, bRecurseIntoChildren: true);
			}
		}
		GUILayout.EndHorizontal();
		GUILayout.Space(20f);
		GUILayout.Label("Vertex count: " + m_selectedAutomaticLOD.GetCurrentVertexCount(bRecurseIntoChildren: true) + "/" + m_selectedAutomaticLOD.GetOriginalVertexCount(bRecurseIntoChildren: true));
		GUILayout.Space(20f);
		if (!string.IsNullOrEmpty(m_strLastTitle) && !string.IsNullOrEmpty(m_strLastMessage))
		{
			GUILayout.Label(m_strLastTitle + ": " + m_strLastMessage, GUILayout.MaxWidth(num));
			GUI.color = Color.blue;
			Rect lastRect = GUILayoutUtility.GetLastRect();
			GUI.Box(new Rect(10f, lastRect.yMax + 5f, 204f, 24f), "");
			GUI.Box(new Rect(12f, lastRect.yMax + 7f, m_nLastProgress * 2, 20f), "");
			return;
		}
		GUILayout.Label("Vertices: " + (m_fVertexAmount * 100f).ToString("0.00") + "%");
		m_fVertexAmount = GUILayout.HorizontalSlider(m_fVertexAmount, 0f, 1f, GUILayout.Width(200f));
		GUILayout.BeginHorizontal();
		GUILayout.Space(3f);
		if (GUILayout.Button("Compute custom LOD", GUILayout.Width(200f)))
		{
			StartCoroutine(ComputeLODWithVertices(m_fVertexAmount));
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}

	private void SetActiveObject(int index)
	{
		m_nSelectedIndex = index;
		AutomaticLOD automaticLOD = UnityEngine.Object.Instantiate(ShowcaseObjects[index].m_automaticLOD);
		automaticLOD.transform.position = ShowcaseObjects[index].m_position;
		automaticLOD.transform.rotation = Quaternion.Euler(ShowcaseObjects[index].m_angles);
		m_selectedAutomaticLOD = automaticLOD;
		automaticLOD.SetAutomaticCameraLODSwitch(bEnabled: false);
		m_objectMaterials = new Dictionary<GameObject, Material[]>();
		AddMaterials(automaticLOD.gameObject, m_objectMaterials);
		m_bWireframe = false;
	}

	private void AddMaterials(GameObject theGameObject, Dictionary<GameObject, Material[]> dicMaterials)
	{
		Renderer component = theGameObject.GetComponent<Renderer>();
		AutomaticLOD component2 = theGameObject.GetComponent<AutomaticLOD>();
		if (component != null && component.sharedMaterials != null && component2 != null)
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

	private IEnumerator ComputeLODWithVertices(float fAmount)
	{
		foreach (KeyValuePair<GameObject, Material[]> objectMaterial in m_objectMaterials)
		{
			AutomaticLOD automaticLOD = objectMaterial.Key.GetComponent<AutomaticLOD>();
			MeshFilter meshFilter = objectMaterial.Key.GetComponent<MeshFilter>();
			SkinnedMeshRenderer skin = objectMaterial.Key.GetComponent<SkinnedMeshRenderer>();
			if ((bool)automaticLOD && (meshFilter != null || skin != null))
			{
				Mesh newMesh = null;
				if (meshFilter != null)
				{
					newMesh = UnityEngine.Object.Instantiate(meshFilter.sharedMesh);
				}
				else if (skin != null)
				{
					newMesh = UnityEngine.Object.Instantiate(skin.sharedMesh);
				}
				automaticLOD.GetMeshSimplifier().CoroutineEnded = false;
				StartCoroutine(automaticLOD.GetMeshSimplifier().ComputeMeshWithVertexCount(objectMaterial.Key, newMesh, Mathf.RoundToInt(fAmount * (float)automaticLOD.GetMeshSimplifier().GetOriginalMeshUniqueVertexCount()), automaticLOD.name, Progress));
				while (!automaticLOD.GetMeshSimplifier().CoroutineEnded)
				{
					yield return null;
				}
				if (meshFilter != null)
				{
					meshFilter.mesh = newMesh;
				}
				else if (skin != null)
				{
					skin.sharedMesh = newMesh;
				}
			}
		}
		m_strLastTitle = "";
		m_strLastMessage = "";
		m_nLastProgress = 0;
	}
}
