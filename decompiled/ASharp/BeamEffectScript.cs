using System.Collections;
using System.Linq;
using Chronos;
using SETUtil.Materials;
using UnityEngine;

public class BeamEffectScript : MonoBehaviour
{
	public int numPoints = 50;

	public Light beamLight;

	public AnimationCurve FadeCurve;

	public float StartTime;

	public float Lifetime;

	public string SecondAttachPoint;

	public Vector3 secondAttachPointOffset;

	private const string BeamFadePropName = "_BeamFade";

	private Vector3[] positions;

	private float orInten;

	private LineRenderer beamLineRenderer;

	private bool initComplete;

	private Coroutine fadeBeamCoroutine;

	private Coroutine adjustBeamIntensityCoroutine;

	private bool isFadeBeamRunning;

	private bool isAdjustBeamIntensityRunning;

	private float animTimeTaken;

	private Material beamMat;

	private Transform secondAttachPoint;

	private void Awake()
	{
		beamLineRenderer = gameObject.GetComponent<LineRenderer>();
		if (beamLineRenderer == null)
		{
			Debug.LogError("LineRenderer component is missing from " + gameObject.name + ". Beam effect will not be played.");
			return;
		}
		positions = new Vector3[numPoints];
		beamLineRenderer.positionCount = numPoints;
		beamMat = beamLineRenderer.GetMaterialClonesSlow().Single();
		if ((bool)beamLight)
		{
			beamLight.color = beamLineRenderer.colorGradient.Evaluate(0.5f);
			orInten = beamLight.intensity;
			initComplete = true;
		}
		else
		{
			Debug.LogError("No light has been attached to " + gameObject.name + ". Beam effect will not be played");
		}
	}

	private void StartEffect()
	{
		beamMat.SetFloat("_BeamFade", 0f);
		animTimeTaken = 0f;
		DrawLinearCurve();
		adjustBeamIntensityCoroutine = StartCoroutine(AdjustBeamIntensity());
		fadeBeamCoroutine = StartCoroutine(FadeBeam());
	}

	private void OnEnable()
	{
		if (!initComplete)
		{
			return;
		}
		CharacterManager componentInParent = base.gameObject.GetComponentInParent<CharacterManager>();
		if ((bool)componentInParent)
		{
			GameObject gameObject = componentInParent.gameObject.FindInChildren(SecondAttachPoint);
			if ((bool)gameObject)
			{
				gameObject.transform.localPosition += secondAttachPointOffset;
				secondAttachPoint = gameObject.transform;
				StartEffect();
				return;
			}
			Debug.LogError("Error: Attach point " + SecondAttachPoint + " could not be found in " + base.gameObject.name + ".  Unable to play Beam Effect");
		}
		else
		{
			Debug.LogError("Error: Unable to find Character Manager in parent of " + base.gameObject.name + ".  Unable to play Beam effect.");
		}
	}

	private void OnDisable()
	{
		if (initComplete)
		{
			beamLight.intensity = orInten;
			if (isAdjustBeamIntensityRunning)
			{
				StopCoroutine(adjustBeamIntensityCoroutine);
			}
			if (isFadeBeamRunning)
			{
				StopCoroutine(fadeBeamCoroutine);
			}
		}
	}

	private IEnumerator AdjustBeamIntensity()
	{
		isAdjustBeamIntensityRunning = true;
		while (animTimeTaken <= Lifetime)
		{
			DrawLinearCurve();
			beamLight.gameObject.transform.position = positions[numPoints / 2];
			float num = beamLineRenderer.sharedMaterial.GetFloat("_BeamFade");
			num = 1f - Mathf.Abs(2f * num - 1f);
			beamLight.intensity = num * orInten;
			yield return new WaitForEndOfFrame();
		}
		isAdjustBeamIntensityRunning = false;
	}

	private IEnumerator FadeBeam()
	{
		isFadeBeamRunning = true;
		if (StartTime > 0f)
		{
			yield return Timekeeper.instance.WaitForSeconds(StartTime);
		}
		while (animTimeTaken <= Lifetime)
		{
			animTimeTaken += Timekeeper.instance.m_GlobalClock.deltaTime;
			beamMat.SetFloat("_BeamFade", FadeCurve.Evaluate(animTimeTaken / Lifetime));
			yield return new WaitForEndOfFrame();
		}
		isFadeBeamRunning = false;
	}

	private void DrawLinearCurve()
	{
		for (int i = 1; i < numPoints; i++)
		{
			float t = (float)i / (float)numPoints;
			positions[i] = CalculateLinearBezierPoint(t, transform.position, secondAttachPoint.position);
		}
		positions[0] = transform.position;
		positions[numPoints - 1] = secondAttachPoint.position;
		beamLineRenderer.SetPositions(positions);
	}

	private Vector3 CalculateLinearBezierPoint(float t, Vector3 p0, Vector3 p1)
	{
		return p0 + t * (p1 - p0);
	}
}
