using SETUtil.Materials;
using UnityEngine;

public class RFX4_EffectSettingVisible : MonoBehaviour
{
	public bool IsActive = true;

	public float FadeOutTime = 3f;

	private bool previousActiveStatus;

	private const string rendererAdditionalName = "Loop";

	private string[] colorProperties = new string[8] { "_TintColor", "_Color", "_EmissionColor", "_BorderColor", "_ReflectColor", "_RimColor", "_MainColor", "_CoreColor" };

	private float alpha;

	private void Start()
	{
	}

	private void Update()
	{
		if (IsActive)
		{
			alpha += Time.deltaTime;
		}
		else
		{
			alpha -= Time.deltaTime;
		}
		alpha = Mathf.Clamp01(alpha);
		if (!IsActive)
		{
			Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in componentsInChildren)
			{
				if (renderer.GetComponent<ParticleSystem>() != null || !renderer.name.Contains("Loop"))
				{
					continue;
				}
				Material material = renderer.GetMaterialClonesSlow()[0];
				RFX4_ShaderColorGradient component = renderer.GetComponent<RFX4_ShaderColorGradient>();
				if (component != null)
				{
					component.canUpdate = false;
				}
				string[] array = colorProperties;
				foreach (string text in array)
				{
					if (material.HasProperty(text))
					{
						Color color = material.GetColor(text);
						color.a = alpha;
						material.SetColor(text, color);
					}
				}
			}
			Projector[] componentsInChildren2 = GetComponentsInChildren<Projector>();
			foreach (Projector projector in componentsInChildren2)
			{
				if (!projector.name.Contains("Loop"))
				{
					continue;
				}
				if (!projector.material.name.EndsWith("(Instance)"))
				{
					projector.material = new Material(projector.material)
					{
						name = projector.material.name + " (Instance)"
					};
				}
				Material material2 = projector.material;
				RFX4_ShaderColorGradient component2 = projector.GetComponent<RFX4_ShaderColorGradient>();
				if (component2 != null)
				{
					component2.canUpdate = false;
				}
				string[] array = colorProperties;
				foreach (string text2 in array)
				{
					if (material2.HasProperty(text2))
					{
						Color color2 = material2.GetColor(text2);
						color2.a = alpha;
						material2.SetColor(text2, color2);
					}
				}
			}
			ParticleSystem[] componentsInChildren3 = GetComponentsInChildren<ParticleSystem>(includeInactive: true);
			foreach (ParticleSystem particleSystem in componentsInChildren3)
			{
				if (particleSystem != null)
				{
					particleSystem.Stop();
				}
			}
			Light[] componentsInChildren4 = GetComponentsInChildren<Light>(includeInactive: true);
			for (int k = 0; k < componentsInChildren4.Length; k++)
			{
				if (componentsInChildren4[k].isActiveAndEnabled)
				{
					RFX4_LightCurves component3 = componentsInChildren4[k].GetComponent<RFX4_LightCurves>();
					if (component3 != null)
					{
						componentsInChildren4[k].intensity = alpha * component3.GraphIntensityMultiplier;
						component3.canUpdate = false;
					}
					else
					{
						componentsInChildren4[k].intensity = alpha;
					}
				}
			}
		}
		if (IsActive && !previousActiveStatus)
		{
			Transform[] componentsInChildren5 = gameObject.GetComponentsInChildren<Transform>();
			foreach (Transform obj in componentsInChildren5)
			{
				obj.gameObject.SetActive(value: false);
				obj.gameObject.SetActive(value: true);
			}
		}
		previousActiveStatus = IsActive;
	}
}
