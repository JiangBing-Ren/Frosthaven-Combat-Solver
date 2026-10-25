using System.Collections;
using System.Collections.Generic;
using Chronos;
using UnityEngine;
using UnityEngine.UI;

public class UIFX_MaterialFX_AttackModifier : MonoBehaviour
{
	public Image MainIcon;

	private float MainIconState;

	public List<Image> MainIconFX;

	private Image[] mainIconFX;

	private float[] mainIconFXState;

	private string fxAnim = "_FXAnim";

	public float switchTime = 1f;

	public AnimationCurve SwitchVerticalOffset = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f));

	public AnimationCurve SwitchHorizontalStretch = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, -1f), new Keyframe(1f, 0f));

	public AnimationCurve SwitchVerticalStretch = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f));

	private AnimationCurve[] SwitchCurves;

	public float disappearUpTime = 1f;

	public AnimationCurve DisappearUpVerticalOffset = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	public AnimationCurve DisappearUpHorizontalStretch = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	public AnimationCurve DisappearUpVerticalStretch = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	private AnimationCurve[] DisappearUpCurves;

	public float appearDownTime = 1f;

	public AnimationCurve AppearDownVerticalOffset = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	public AnimationCurve AppearDownHorizontalStretch = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	public AnimationCurve AppearDownVerticalStretch = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	private AnimationCurve[] AppearDownCurves;

	private float transitionDelay;

	private float localDelay = 1f;

	private float animTime;

	private AnimationCurve[] animCurves;

	private float mainIconFXTargetValue;

	private float MainIconTargetValue;

	private float MainIconColourFadeTarget;

	private float CutOutFadeTargetA;

	private float CutOutFadeTargetB;

	public bool debugIdleThroughStates = true;

	private int idleCount;

	private void Start()
	{
		mainIconFX = MainIconFX.ToArray();
		Image[] array = mainIconFX;
		foreach (Image obj in array)
		{
			obj.material = new Material(obj.material);
		}
		MainIcon.material = new Material(MainIcon.material);
		if (debugIdleThroughStates)
		{
			transitionDelay = 1f;
			IdleThroughStates();
		}
		else
		{
			transitionDelay = 0f;
		}
		ActivateFX();
	}

	private void Update()
	{
	}

	private void StoreCurrentState()
	{
		StopAllCoroutines();
		mainIconFXState = new float[mainIconFX.Length];
		for (int i = 0; i < mainIconFX.Length; i++)
		{
			mainIconFXState[i] = mainIconFX[i].material.GetFloat(fxAnim);
		}
		if (debugIdleThroughStates)
		{
			transitionDelay = localDelay;
		}
		else
		{
			transitionDelay = 0f;
		}
	}

	private void IdleThroughStates()
	{
		if (idleCount == 5)
		{
			idleCount = 0;
		}
		if (idleCount == 0)
		{
			DisappearUp();
		}
		if (idleCount == 1)
		{
			AppearDown();
		}
		if (idleCount == 2)
		{
			SwitchIcon();
		}
		if (idleCount == 3)
		{
			ActivateFX();
		}
		if (idleCount == 4)
		{
			DeactivateFX();
		}
	}

	private void DisappearUp()
	{
		localDelay = 1f;
		StoreCurrentState();
		animTime = disappearUpTime;
		StartCoroutine(DisappearUpTimeline());
	}

	private void AppearDown()
	{
		localDelay = 1f;
		StoreCurrentState();
		animTime = appearDownTime;
		StartCoroutine(AppearDownTimeline());
	}

	private void SwitchIcon()
	{
		localDelay = 1f;
		StoreCurrentState();
		animTime = switchTime;
		StartCoroutine(SwitchIconTimeline());
	}

	private void ActivateFX()
	{
		localDelay = 0.01f;
		StoreCurrentState();
		animTime = 0.4f;
		mainIconFXTargetValue = 0.5f;
		MainIconTargetValue = 0.5f;
		MainIconColourFadeTarget = 1f;
		CutOutFadeTargetA = 0f;
		CutOutFadeTargetB = 0f;
		StartCoroutine(ActivateFXTimeline());
	}

	private void DeactivateFX()
	{
		localDelay = 1.5f;
		StoreCurrentState();
		animTime = 0.4f;
		mainIconFXTargetValue = 0f;
		MainIconTargetValue = 0.5f;
		MainIconColourFadeTarget = 0f;
		CutOutFadeTargetA = 0f;
		CutOutFadeTargetB = 1f;
		StartCoroutine(ActivateFXTimeline());
	}

	public IEnumerator DisappearUpTimeline()
	{
		yield return Timekeeper.instance.WaitForSeconds(transitionDelay);
		float dTime = 0f;
		float startTime = Timekeeper.instance.m_GlobalClock.time;
		while (Timekeeper.instance.m_GlobalClock.time - startTime < animTime)
		{
			dTime += Timekeeper.instance.m_GlobalClock.deltaTime / animTime;
			MainIcon.material.SetFloat("_OffsetVertical", DisappearUpVerticalOffset.Evaluate(dTime));
			MainIcon.material.SetFloat("_SquashStretchHor", DisappearUpHorizontalStretch.Evaluate(dTime));
			MainIcon.material.SetFloat("_SquashStretchVert", DisappearUpVerticalStretch.Evaluate(dTime));
			MainIcon.material.SetFloat("_CutOut", Mathf.Clamp01(dTime));
			yield return new WaitForEndOfFrame();
		}
		if (debugIdleThroughStates)
		{
			idleCount++;
			IdleThroughStates();
		}
	}

	public IEnumerator AppearDownTimeline()
	{
		yield return Timekeeper.instance.WaitForSeconds(transitionDelay);
		float dTime = 0f;
		float startTime = Timekeeper.instance.m_GlobalClock.time;
		while (Timekeeper.instance.m_GlobalClock.time - startTime < animTime)
		{
			dTime += Timekeeper.instance.m_GlobalClock.deltaTime / animTime;
			MainIcon.material.SetFloat("_OffsetVertical", AppearDownVerticalOffset.Evaluate(dTime));
			MainIcon.material.SetFloat("_SquashStretchHor", AppearDownHorizontalStretch.Evaluate(dTime));
			MainIcon.material.SetFloat("_SquashStretchVert", AppearDownVerticalStretch.Evaluate(dTime));
			MainIcon.material.SetFloat("_CutOut", Mathf.Clamp01(1f - dTime));
			yield return new WaitForEndOfFrame();
		}
		if (debugIdleThroughStates)
		{
			idleCount++;
			IdleThroughStates();
		}
	}

	public IEnumerator SwitchIconTimeline()
	{
		yield return Timekeeper.instance.WaitForSeconds(transitionDelay);
		float dTime = 0f;
		float startTime = Timekeeper.instance.m_GlobalClock.time;
		while (Timekeeper.instance.m_GlobalClock.time - startTime < animTime)
		{
			dTime += Timekeeper.instance.m_GlobalClock.deltaTime / animTime;
			MainIcon.material.SetFloat("_OffsetVertical", SwitchVerticalOffset.Evaluate(dTime));
			MainIcon.material.SetFloat("_SquashStretchHor", SwitchHorizontalStretch.Evaluate(dTime));
			MainIcon.material.SetFloat("_SquashStretchVert", SwitchVerticalStretch.Evaluate(dTime));
			yield return new WaitForEndOfFrame();
		}
		if (debugIdleThroughStates)
		{
			idleCount++;
			IdleThroughStates();
		}
	}

	public IEnumerator ActivateFXTimeline()
	{
		yield return Timekeeper.instance.WaitForSeconds(transitionDelay);
		float dTime = 0f;
		float startTime = Timekeeper.instance.m_GlobalClock.time;
		while (Timekeeper.instance.m_GlobalClock.time - startTime < animTime)
		{
			dTime += Timekeeper.instance.m_GlobalClock.deltaTime / animTime;
			for (int i = 0; i < mainIconFX.Length; i++)
			{
				mainIconFX[i].material.SetFloat(fxAnim, Mathf.Lerp(mainIconFXState[i], mainIconFXTargetValue, dTime));
			}
			MainIcon.material.SetFloat("_FadeColourIn", Mathf.Lerp(1f - MainIconColourFadeTarget, MainIconColourFadeTarget, dTime * dTime));
			yield return new WaitForEndOfFrame();
		}
		if (debugIdleThroughStates)
		{
			idleCount++;
			IdleThroughStates();
		}
	}
}
