using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RFX4_PerPlatformSettings : MonoBehaviour
{
	public bool RenderMobileDistortion;

	[Range(0.1f, 1f)]
	public float ParticleBudgetForMobiles = 0.5f;

	private bool startColorTex;

	private void Start()
	{
	}

	private void Awake()
	{
		ChangeParticlesBudget(ParticleBudgetForMobiles);
	}

	private void OnEnable()
	{
		Camera main = Camera.main;
		if (!(main == null))
		{
			UniversalAdditionalCameraData component = main.GetComponent<UniversalAdditionalCameraData>();
			if (component != null)
			{
				startColorTex = component.requiresColorTexture;
				component.requiresColorTexture = true;
			}
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
				component.requiresColorTexture = startColorTex;
			}
		}
	}

	private void ChangeParticlesBudget(float particlesMul)
	{
		ParticleSystem component = GetComponent<ParticleSystem>();
		if (component == null)
		{
			return;
		}
		ParticleSystem.MainModule main = component.main;
		main.maxParticles = Mathf.Max(1, (int)((float)main.maxParticles * particlesMul));
		ParticleSystem.EmissionModule emission = component.emission;
		if (!emission.enabled)
		{
			return;
		}
		ParticleSystem.MinMaxCurve rateOverTime = emission.rateOverTime;
		if (rateOverTime.constantMin > 1f)
		{
			rateOverTime.constantMin *= particlesMul;
		}
		if (rateOverTime.constantMax > 1f)
		{
			rateOverTime.constantMax *= particlesMul;
		}
		emission.rateOverTime = rateOverTime;
		ParticleSystem.MinMaxCurve rateOverDistance = emission.rateOverDistance;
		if (rateOverDistance.constantMin > 1f)
		{
			if (rateOverDistance.constantMin > 1f)
			{
				rateOverDistance.constantMin *= particlesMul;
			}
			if (rateOverDistance.constantMax > 1f)
			{
				rateOverDistance.constantMax *= particlesMul;
			}
			emission.rateOverDistance = rateOverDistance;
		}
		ParticleSystem.Burst[] array = new ParticleSystem.Burst[emission.burstCount];
		emission.GetBursts(array);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].minCount > 1)
			{
				array[i].minCount = (short)((float)array[i].minCount * particlesMul);
			}
			if (array[i].maxCount > 1)
			{
				array[i].maxCount = (short)((float)array[i].maxCount * particlesMul);
			}
		}
		emission.SetBursts(array);
	}
}
