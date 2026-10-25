using UnityEngine;
using UnityEngine.Rendering.Universal;

[ExecuteInEditMode]
public class RFX4_Decal : MonoBehaviour
{
	public bool IsScreenSpace = true;

	private ParticleSystem ps;

	private ParticleSystem.MainModule psMain;

	private MaterialPropertyBlock props;

	private MeshRenderer rend;

	private bool startDepthTex;

	private void OnEnable()
	{
		ps = GetComponent<ParticleSystem>();
		if (ps != null)
		{
			psMain = ps.main;
		}
		Camera main = Camera.main;
		if (main != null)
		{
			UniversalAdditionalCameraData component = main.GetComponent<UniversalAdditionalCameraData>();
			if (component != null)
			{
				startDepthTex = component.requiresDepthTexture;
				component.requiresDepthTexture = IsScreenSpace;
			}
		}
		if (!IsScreenSpace)
		{
			Material sharedMaterial = GetComponent<Renderer>().sharedMaterial;
			sharedMaterial.EnableKeyword("USE_QUAD_DECAL");
			sharedMaterial.SetInt("_ZTest1", 4);
			if (Application.isPlaying)
			{
				Vector3 localPosition = transform.localPosition;
				localPosition.z += 0.1f;
				transform.localPosition = localPosition;
				Vector3 localScale = transform.localScale;
				localScale.y = 0.001f;
				transform.localScale = localScale;
			}
		}
		else
		{
			Material sharedMaterial2 = GetComponent<Renderer>().sharedMaterial;
			sharedMaterial2.DisableKeyword("USE_QUAD_DECAL");
			sharedMaterial2.SetInt("_ZTest1", 5);
		}
	}

	private void OnDisable()
	{
		Camera main = Camera.main;
		if (!(main == null))
		{
			UniversalAdditionalCameraData component = main.GetComponent<UniversalAdditionalCameraData>();
			if (component != null)
			{
				component.requiresDepthTexture = startDepthTex;
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.matrix = Matrix4x4.TRS(transform.TransformPoint(Vector3.zero), transform.rotation, transform.lossyScale);
		Gizmos.color = new Color(1f, 1f, 1f, 1f);
		Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
	}
}
