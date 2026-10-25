using System;
using System.Collections;
using System.Collections.Generic;
using Chronos;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class UIFX_MaterialFX_Control : MonoBehaviour
{
	public enum UIFXDebugTheme
	{
		Consumable,
		Initiative,
		ButtonFX
	}

	public Image MainIcon;

	private float MainIconState;

	public Image MainIcon2;

	private float MainIcon2State;

	public List<Image> MainIconFX;

	private Image[] mainIconFX;

	private float[] mainIconFXState;

	public List<Image> MainIcon2FX;

	private Image[] mainIcon2FX;

	private float[] mainIcon2FXState;

	public List<Image> TextAndSubIconFX;

	private Image[] textAndSubIconFX;

	private float[] textAndSubIconFXState;

	public List<Image> ActivateFX;

	private Image[] activateFX;

	private string fxAnim = "_FXAnim";

	public bool customaActivateFxAnimation;

	[ConditionalField("customaActivateFxAnimation", "true", true)]
	public AnimationCurve activateFxAnimationCurve;

	private float transitionDelay;

	private float animTime;

	private float animTimeMult;

	private float mainIconFXTargetValue;

	private float textAndSubIconFXTargetValue;

	private float MainIconTargetValue;

	private float SubIconTargetValue;

	private float textColorStateTargetValue;

	private bool playSelectionFX;

	public UIFXDebugTheme UIFXDebugType;

	public bool debugIdleThroughStates = true;

	private int idleCount;

	private List<Action> pendingActionsFromTimeline = new List<Action>();

	private void Awake()
	{
		if (!debugIdleThroughStates)
		{
			ToggleEnable(active: false);
		}
		mainIconFX = MainIconFX.ToArray();
		mainIcon2FX = MainIcon2FX.ToArray();
		textAndSubIconFX = TextAndSubIconFX.ToArray();
		activateFX = ActivateFX.ToArray();
		Image[] array = mainIconFX;
		foreach (Image obj in array)
		{
			obj.material = new Material(obj.material);
		}
		array = mainIcon2FX;
		foreach (Image obj2 in array)
		{
			obj2.material = new Material(obj2.material);
		}
		array = textAndSubIconFX;
		foreach (Image obj3 in array)
		{
			obj3.material = new Material(obj3.material);
		}
		array = activateFX;
		foreach (Image obj4 in array)
		{
			obj4.material = new Material(obj4.material);
		}
		if (MainIcon != null)
		{
			MainIcon.material = new Material(MainIcon.material);
		}
		if (MainIcon2 != null)
		{
			MainIcon2.material = new Material(MainIcon2.material);
		}
		if (debugIdleThroughStates)
		{
			transitionDelay = 2f;
			IdleThroughStates();
		}
		else
		{
			transitionDelay = 0f;
		}
	}

	public void ApplyEffectType(ElementInfusionBoardManager.EElement element)
	{
		if (!(UIInfoTools.Instance == null))
		{
			ApplyEffectType(UIInfoTools.Instance.GetElementEffect(element));
		}
	}

	public void ApplyEffectType(EEnhancement enhancement)
	{
		if (!(UIInfoTools.Instance == null))
		{
			ApplyEffectType(UIInfoTools.Instance.GetEnhancementEffect(enhancement));
		}
	}

	public void ApplyEffectType(EffectDataBase effectData)
	{
		if (ActivateFX != null)
		{
			foreach (Image item in ActivateFX)
			{
				item.color = effectData.fxColor;
				item.material = new Material(effectData.waveMaterial);
			}
		}
		if (MainIconFX != null && MainIconFX.Count > 0)
		{
			MainIconFX[0].color = effectData.fxColor;
			MainIconFX[0].material = new Material(effectData.sparksMaterial);
			MainIconFX[1].color = effectData.bloomColor;
			MainIconFX[1].material = new Material(effectData.bloomMaterial);
		}
		if (MainIcon2FX != null && MainIcon2FX.Count > 0)
		{
			MainIcon2FX[0].color = effectData.fxColor;
			MainIcon2FX[0].material = new Material(effectData.sparksMaterial);
			MainIcon2FX[1].color = effectData.bloomColor;
			MainIcon2FX[1].material = new Material(effectData.bloomMaterial);
		}
		if (TextAndSubIconFX != null && TextAndSubIconFX.Count > 0)
		{
			TextAndSubIconFX[0].color = effectData.fxColor;
			TextAndSubIconFX[0].material = new Material(effectData.sparksMaterial);
			TextAndSubIconFX[1].color = effectData.bloomColor;
			TextAndSubIconFX[1].material = new Material(effectData.bloomMaterial);
		}
	}

	public void ToggleEnable(bool active, bool enableTextFX = false)
	{
		if (mainIconFX != null)
		{
			Image[] array = mainIconFX;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(active);
			}
		}
		if (mainIcon2FX != null)
		{
			Image[] array = mainIcon2FX;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(active);
			}
		}
		if (activateFX != null)
		{
			Image[] array = activateFX;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(active);
			}
		}
		if (TextAndSubIconFX == null)
		{
			return;
		}
		foreach (Image item in TextAndSubIconFX)
		{
			item.gameObject.SetActive(active & enableTextFX);
		}
	}

	private void StoreCurrentState()
	{
		StopAllCoroutines();
		foreach (Action item in pendingActionsFromTimeline)
		{
			Debug.Log("Material Control FX: Action from stop all");
			item();
		}
		pendingActionsFromTimeline.Clear();
		mainIconFXState = new float[mainIconFX.Length];
		for (int i = 0; i < mainIconFX.Length; i++)
		{
			if (mainIconFX[i].material.HasProperty("_FXAnim"))
			{
				mainIconFXState[i] = mainIconFX[i].material.GetFloat(fxAnim);
			}
			else
			{
				Log.Info("Material needs to have property '_FXAnim'!");
			}
		}
		mainIcon2FXState = new float[mainIcon2FX.Length];
		for (int j = 0; j < mainIcon2FX.Length; j++)
		{
			if (mainIcon2FX[j].material.HasProperty("_FXAnim"))
			{
				mainIcon2FXState[j] = mainIcon2FX[j].material.GetFloat(fxAnim);
			}
			else
			{
				Log.Info("Material needs to have property '_FXAnim'!");
			}
		}
		textAndSubIconFXState = new float[textAndSubIconFX.Length];
		for (int k = 0; k < textAndSubIconFX.Length; k++)
		{
			if (textAndSubIconFX[k].material.HasProperty("_FXAnim"))
			{
				textAndSubIconFXState[k] = textAndSubIconFX[k].material.GetFloat(fxAnim);
			}
			else
			{
				Log.Info("Material needs to have property '_FXAnim'!");
			}
		}
		if (MainIcon != null && MainIcon.material.HasProperty(fxAnim))
		{
			MainIconState = MainIcon.material.GetFloat(fxAnim);
		}
		if (MainIcon2 != null && MainIcon2.material.HasProperty(fxAnim))
		{
			MainIcon2State = MainIcon2.material.GetFloat(fxAnim);
		}
		Image[] array = activateFX;
		for (int l = 0; l < array.Length; l++)
		{
			array[l].material.SetFloat(fxAnim, 0f);
		}
	}

	private void IdleThroughStates()
	{
		if (UIFXDebugType == UIFXDebugTheme.Consumable)
		{
			if (idleCount == 4)
			{
				idleCount = 0;
			}
			if (idleCount == 0)
			{
				NonConsumable();
			}
			if (idleCount == 1)
			{
				Consumable();
			}
			if (idleCount == 2)
			{
				ConsumableHover();
			}
			if (idleCount == 3)
			{
				ConsumableSelect();
			}
		}
		if (UIFXDebugType == UIFXDebugTheme.Initiative)
		{
			if (idleCount == 4)
			{
				idleCount = 0;
			}
			if (idleCount == 0)
			{
				InitiativeNone();
			}
			if (idleCount == 1)
			{
				InitiativeHover();
			}
			if (idleCount == 2)
			{
				InitiativeSelect();
			}
			if (idleCount == 3)
			{
				InitiativeActive();
			}
		}
		if (UIFXDebugType == UIFXDebugTheme.ButtonFX)
		{
			if (idleCount == 3)
			{
				idleCount = 0;
			}
			if (idleCount == 0)
			{
				ButtonFXActive();
			}
			if (idleCount == 1)
			{
				ButtonFXEndTurn();
			}
			if (idleCount == 2)
			{
				ButtonFXOff();
			}
		}
	}

	private void NonConsumable()
	{
		StoreCurrentState();
		animTime = 1.5f;
		animTimeMult = 1f;
		mainIconFXTargetValue = 0f;
		textAndSubIconFXTargetValue = 0f;
		MainIconTargetValue = 0.167f;
		SubIconTargetValue = 0.167f;
		textColorStateTargetValue = 0.33f;
		playSelectionFX = false;
		StartCoroutine(ConsumableStateTimeline());
	}

	private void Consumable()
	{
		StoreCurrentState();
		animTime = 1.5f;
		animTimeMult = 1f;
		mainIconFXTargetValue = 0.25f;
		textAndSubIconFXTargetValue = 0f;
		MainIconTargetValue = 0.5f;
		SubIconTargetValue = 0.167f;
		textColorStateTargetValue = 0.33f;
		playSelectionFX = false;
		StartCoroutine(ConsumableStateTimeline());
	}

	private void ConsumableHover()
	{
		StoreCurrentState();
		animTime = 1f;
		animTimeMult = 1f;
		mainIconFXTargetValue = 0.5f;
		textAndSubIconFXTargetValue = 0.5f;
		MainIconTargetValue = 0.5f;
		SubIconTargetValue = 0.5f;
		textColorStateTargetValue = 1f;
		playSelectionFX = false;
		StartCoroutine(ConsumableStateTimeline());
	}

	public void ConsumableSelect(Action onfinished = null)
	{
		StoreCurrentState();
		animTime = 2f;
		animTimeMult = 1f;
		mainIconFXTargetValue = 0.5f;
		textAndSubIconFXTargetValue = 0f;
		MainIconTargetValue = 0.5f;
		SubIconTargetValue = 0.5f;
		textColorStateTargetValue = 1f;
		playSelectionFX = true;
		StartCoroutine(ConsumableStateTimeline(onfinished));
	}

	public void InitiativeNone()
	{
		StoreCurrentState();
		animTime = 0.5f;
		animTimeMult = 1f;
		mainIconFXTargetValue = 0f;
		textAndSubIconFXTargetValue = 0f;
		MainIconTargetValue = 0.167f;
		SubIconTargetValue = 0.167f;
		textColorStateTargetValue = 0.33f;
		playSelectionFX = false;
		StartCoroutine(ConsumableStateTimeline());
	}

	public void InitiativeActive()
	{
		StoreCurrentState();
		animTime = 0.5f;
		animTimeMult = 1f;
		mainIconFXTargetValue = 0.5f;
		textAndSubIconFXTargetValue = 0f;
		MainIconTargetValue = 0.5f;
		SubIconTargetValue = 0.167f;
		textColorStateTargetValue = 0.33f;
		playSelectionFX = false;
		StartCoroutine(ConsumableStateTimeline());
	}

	public void InitiativeHover()
	{
		StoreCurrentState();
		animTime = 0.5f;
		animTimeMult = 1f;
		mainIconFXTargetValue = 0.25f;
		textAndSubIconFXTargetValue = 0.5f;
		MainIconTargetValue = 0.5f;
		SubIconTargetValue = 0.5f;
		textColorStateTargetValue = 1f;
		playSelectionFX = false;
		StartCoroutine(ConsumableStateTimeline());
	}

	public void InitiativeSelect()
	{
		StoreCurrentState();
		animTime = 2f;
		animTimeMult = 1f;
		mainIconFXTargetValue = 0.5f;
		textAndSubIconFXTargetValue = 0f;
		MainIconTargetValue = 0.5f;
		SubIconTargetValue = 0.5f;
		textColorStateTargetValue = 1f;
		playSelectionFX = true;
		StartCoroutine(ConsumableStateTimeline());
	}

	private void ButtonFXOff()
	{
		StoreCurrentState();
		animTime = 0.6f;
		animTimeMult = 1f;
		mainIconFXTargetValue = 0f;
		textAndSubIconFXTargetValue = 0f;
		MainIconTargetValue = 0.167f;
		SubIconTargetValue = 0.167f;
		textColorStateTargetValue = 0.33f;
		playSelectionFX = false;
		StartCoroutine(ConsumableStateTimeline());
	}

	public void ButtonFXEndTurn()
	{
		StoreCurrentState();
		animTime = 0.5f;
		animTimeMult = 1f;
		mainIconFXTargetValue = 0.5f;
		textAndSubIconFXTargetValue = 0f;
		MainIconTargetValue = 0.5f;
		SubIconTargetValue = 0.5f;
		textColorStateTargetValue = 1f;
		playSelectionFX = false;
		if (gameObject.activeInHierarchy)
		{
			StartCoroutine(ConsumableStateTimeline());
		}
	}

	public void ButtonFXActive(Action onfinished = null)
	{
		StoreCurrentState();
		animTime = 0.2f;
		animTimeMult = 1f;
		mainIconFXTargetValue = 0.5f;
		textAndSubIconFXTargetValue = 0f;
		MainIconTargetValue = 0.5f;
		SubIconTargetValue = 0.5f;
		textColorStateTargetValue = 1f;
		playSelectionFX = true;
		if (gameObject.activeInHierarchy)
		{
			StartCoroutine(ConsumableStateTimeline(onfinished));
		}
		else
		{
			onfinished?.Invoke();
		}
	}

	public IEnumerator ConsumableStateTimeline(Action onfinished = null)
	{
		if (onfinished != null)
		{
			pendingActionsFromTimeline.Add(onfinished);
		}
		yield return Timekeeper.instance.WaitForSeconds(transitionDelay);
		float dTime = 0f;
		float startTime = Timekeeper.instance.m_GlobalClock.unscaledTime;
		while (Timekeeper.instance.m_GlobalClock.unscaledTime - startTime < animTime)
		{
			dTime += Timekeeper.instance.m_GlobalClock.unscaledDeltaTime / animTime;
			for (int i = 0; i < mainIconFX.Length; i++)
			{
				mainIconFX[i].material.SetFloat(fxAnim, Mathf.Lerp(mainIconFXState[i], mainIconFXTargetValue, Mathf.Clamp01(dTime * animTimeMult)));
			}
			for (int j = 0; j < mainIcon2FX.Length; j++)
			{
				mainIcon2FX[j].material.SetFloat(fxAnim, Mathf.Lerp(mainIcon2FXState[j], mainIconFXTargetValue, Mathf.Clamp01(dTime * animTimeMult)));
			}
			for (int k = 0; k < textAndSubIconFX.Length; k++)
			{
				textAndSubIconFX[k].material.SetFloat(fxAnim, Mathf.Lerp(textAndSubIconFXState[k], textAndSubIconFXTargetValue, Mathf.Clamp01(dTime * animTimeMult)));
			}
			if (MainIcon != null)
			{
				MainIcon.material.SetFloat(fxAnim, Mathf.Lerp(MainIconState, MainIconTargetValue, Mathf.Clamp01(dTime * animTimeMult)));
			}
			if (MainIcon2 != null)
			{
				MainIcon2.material.SetFloat(fxAnim, Mathf.Lerp(MainIcon2State, MainIconTargetValue, Mathf.Clamp01(dTime * animTimeMult)));
			}
			if (playSelectionFX)
			{
				Image[] array = activateFX;
				foreach (Image image in array)
				{
					if (customaActivateFxAnimation)
					{
						image.material.SetFloat(fxAnim, Mathf.Clamp01(activateFxAnimationCurve.Evaluate(dTime)));
					}
					else
					{
						image.material.SetFloat(fxAnim, 1f - (1f - dTime) * (1f - dTime) * (1f - dTime));
					}
				}
			}
			yield return new WaitForEndOfFrame();
		}
		if (onfinished != null)
		{
			onfinished();
			pendingActionsFromTimeline.Remove(onfinished);
		}
		if (debugIdleThroughStates)
		{
			idleCount++;
			IdleThroughStates();
		}
	}
}
