using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chronos;
using FHGUI.Common;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;

namespace WorldspaceUI;

public class AttackModBar : MonoBehaviour
{
	[Serializable]
	private class Config
	{
		[Header("Phase 1 - Label fade out & Icon scale in")]
		public float InitialLabelFadeAwayDuration = 1f;

		public AnimationCurve InitialLabelFadeAwayCurve;

		public float InitialIconScaleInDelay = 1f;

		public float InitialIconScaleInDuration = 1f;

		public AnimationCurve InitialIconScaleInCurve;

		[AudioEventName]
		public string InitialIconAppearAudioEvent;

		public float InitialIconAudioEventDelay;

		[Header("Phase 2 - Icon swivel & modifier reveal")]
		public float IconSwivelDelay = 1f;

		public float IconSwivelDuration = 1f;

		public AnimationCurve IconSwivelCurve;

		[Header("Phase 2 - Icon modifier replace")]
		public float ReplaceShowDelay;

		public float ReplaceShowDuration = 1f;

		public AnimationCurve ReplaceShowCurve;

		public float ReplaceGlowDuration = 1f;

		public AnimationCurve ReplaceGlowCurve;

		public float ReplaceShineDelay;

		public float ReplaceShineDuration = 1f;

		public AnimationCurve ReplaceShineCurve;

		[Header("Phase 2 - Rolling")]
		public float ModifierRollingDelay = 1f;

		internal Vector3 ModifierRollingScale = new Vector3(2f, 2f);

		public float ModifierRollingDuration = 1f;

		public AnimationCurve ModifierRollingDurationCurve;

		public float ModifierRollingMoveDuration = 1f;

		public AnimationCurve ModifierRollingMoveDurationCurve;

		public float ModifierRollingFinishDelay = 0.3f;

		[Header("Phase 3 - Attack has hit & Damage being displayed")]
		public float UnusedModifierFadeAwayDelay = 1f;

		public float UnusedModifierFadeAwayDuration = 1f;

		public AnimationCurve UnusedModifierFadeAwayCurve;

		public float DamageLabelDisplayDelay = 1f;

		public float LabelFadeAwayDelay = 1f;

		public float LabelFadeAwayDuration = 1f;

		public AnimationCurve LabelFadeAwayCurve;
	}

	public static bool s_AttackModifierBarFlowCanBegin;

	public static int s_LongestAttackModSequence;

	public static bool s_FrozenForAttackModifiers;

	[Space]
	[SerializeField]
	private InfoBar m_InfoBar;

	[SerializeField]
	private HealthBar m_HealthBar;

	[Space]
	[SerializeField]
	private RectTransform m_ContainerRect;

	[SerializeField]
	private RectTransform m_ContainerDrawnRect;

	[SerializeField]
	private RectTransform m_ContainerPosLowered;

	[SerializeField]
	private RectTransform m_ContainerPosRaised;

	[Space]
	[SerializeField]
	private RectTransform m_ModifierPosNormal;

	[SerializeField]
	private RectTransform m_ModifierPosAdvantage1;

	[SerializeField]
	private RectTransform m_ModifierPosAdvantage2;

	[Space]
	[SerializeField]
	private AttackModElementUI m_ModifierPrefab;

	private AttackModElementUI m_UsedModifier;

	private AttackModElementUI m_NotUsedModifier;

	private List<AttackModElementUI> m_RollingModifiers;

	[Space]
	[SerializeField]
	private CanvasGroup m_IconRolling;

	[Space]
	[SerializeField]
	private Color m_AdvantageColor;

	[SerializeField]
	private Color m_DisadvantageColor;

	[Space]
	[SerializeField]
	private Config m_Config;

	[HideInInspector]
	public Coroutine m_AttackModCoroutine;

	[HideInInspector]
	public Coroutine m_AttackModFinishCoroutine;

	[HideInInspector]
	public IEnumerator m_AttackModFinishEnumerator;

	public float testTimeScaler = 10f;

	private IEnumerator m_IconFadeRollingRoutine;

	private IEnumerator m_IconScaleRollingRoutine;

	private bool isDrawingModifiers;

	private Vector3 iconRollingPosition;

	private CAttackSummary.TargetSummary m_Summary;

	private bool m_ConfirmFinishedDrawingInLogicThread;

	public bool IsFlowActive { get; private set; }

	public GameObject Actor { get; set; }

	private void Awake()
	{
		m_RollingModifiers = new List<AttackModElementUI>();
		iconRollingPosition = m_IconRolling.GetComponent<RectTransform>().anchoredPosition;
	}

	private void SetAdvantageColors(CAttackSummary.TargetSummary attackSummary = null)
	{
		if (attackSummary == null)
		{
			m_UsedModifier?.SetAdvantadgeColor(Color.white);
			m_NotUsedModifier?.SetAdvantadgeColor(Color.white);
			return;
		}
		try
		{
			switch (attackSummary.OverallAdvantage)
			{
			case EAdvantageStatuses.None:
				m_UsedModifier?.SetAdvantadgeColor(Color.white);
				m_NotUsedModifier?.SetAdvantadgeColor(Color.white);
				break;
			case EAdvantageStatuses.Advantage:
				m_UsedModifier?.SetAdvantadgeColor(m_AdvantageColor);
				m_NotUsedModifier?.SetAdvantadgeColor(m_AdvantageColor);
				break;
			case EAdvantageStatuses.Disadvantage:
				m_UsedModifier?.SetAdvantadgeColor(m_DisadvantageColor);
				m_NotUsedModifier?.SetAdvantadgeColor(m_DisadvantageColor);
				break;
			}
		}
		catch (Exception exception)
		{
			Log.GUIException(exception, "Error trying to Set Advantage Colors in attack modifier UI");
		}
	}

	private void SetAdvantageColors(AttackModElementUI modUI, CAttackSummary.TargetSummary attackSummary = null)
	{
		if (attackSummary == null)
		{
			modUI?.SetAdvantadgeColor(Color.white);
			return;
		}
		try
		{
			switch (attackSummary.OverallAdvantage)
			{
			case EAdvantageStatuses.None:
				modUI?.SetAdvantadgeColor(Color.white);
				break;
			case EAdvantageStatuses.Advantage:
				modUI?.SetAdvantadgeColor(m_AdvantageColor);
				break;
			case EAdvantageStatuses.Disadvantage:
				modUI?.SetAdvantadgeColor(m_DisadvantageColor);
				break;
			}
		}
		catch (Exception exception)
		{
			Log.GUIException(exception, "Error trying to Set Advantage Colors in attack modifier UI");
		}
	}

	private void SetTextStyleForModifiers(AttackModifierYMLData modifier1, AttackModifierYMLData modifier2, CAttackSummary.TargetSummary summary, bool useOriginalModifier = true)
	{
		if (modifier1 != null)
		{
			EAttackModifierDamageColorCode colorCodeForAttackModifier = GetColorCodeForAttackModifier((useOriginalModifier && modifier1.OriginalModifier.IsNOTNullOrEmpty()) ? modifier1.OriginalModifier : modifier1.MathModifier);
			m_UsedModifier?.SetTextStyleForModifiers(modifier1, m_InfoBar.GetColorForColorCode(colorCodeForAttackModifier), useOriginalModifier);
		}
		else
		{
			m_UsedModifier?.SetTextStyleForModifiers(null, Color.gray);
		}
		if (modifier2 != null)
		{
			EAttackModifierDamageColorCode colorCodeForAttackModifier2 = GetColorCodeForAttackModifier((useOriginalModifier && modifier2.OriginalModifier.IsNOTNullOrEmpty()) ? modifier2.OriginalModifier : modifier2.MathModifier);
			m_NotUsedModifier.SetTextStyleForModifiers(modifier2, m_InfoBar.GetColorForColorCode(colorCodeForAttackModifier2), useOriginalModifier);
		}
	}

	private void SetTextStyleForModifiers(AttackModElementUI modUI, CAttackSummary.TargetSummary summary, bool useOriginalModifier = true)
	{
		AttackModifierYMLData modifier = modUI.m_Modifier;
		if (modifier != null)
		{
			EAttackModifierDamageColorCode colorCodeForAttackModifier = GetColorCodeForAttackModifier((useOriginalModifier && modifier.OriginalModifier.IsNOTNullOrEmpty()) ? modifier.OriginalModifier : modifier.MathModifier);
			modUI?.SetTextStyleForModifiers(modifier, m_InfoBar.GetColorForColorCode(colorCodeForAttackModifier), useOriginalModifier);
		}
		else
		{
			modUI?.SetTextStyleForModifiers(null, Color.gray);
		}
	}

	public void PlayPfxForModifiers(AttackModifierYMLData modifier1, AttackModifierYMLData modifier2, bool useOriginalModifier = true)
	{
		if (modifier1 != null)
		{
			EAttackModifierDamageColorCode colorCodeForAttackModifier = GetColorCodeForAttackModifier((useOriginalModifier && modifier1.OriginalModifier.IsNOTNullOrEmpty()) ? modifier1.OriginalModifier : modifier1.MathModifier);
			m_UsedModifier?.PlayPfxForModifiers(colorCodeForAttackModifier);
		}
		if (modifier2 != null)
		{
			EAttackModifierDamageColorCode colorCodeForAttackModifier2 = GetColorCodeForAttackModifier((useOriginalModifier && modifier2.OriginalModifier.IsNOTNullOrEmpty()) ? modifier2.OriginalModifier : modifier2.MathModifier);
			m_NotUsedModifier?.PlayPfxForModifiers(colorCodeForAttackModifier2);
		}
	}

	public void PlayPfxForModifiers(AttackModElementUI modUI, bool useOriginalModifier = true)
	{
		AttackModifierYMLData modifier = modUI.m_Modifier;
		if (modifier != null)
		{
			EAttackModifierDamageColorCode colorCodeForAttackModifier = GetColorCodeForAttackModifier((useOriginalModifier && modifier.OriginalModifier.IsNOTNullOrEmpty()) ? modifier.OriginalModifier : modifier.MathModifier);
			modUI?.PlayPfxForModifiers(colorCodeForAttackModifier);
		}
	}

	public EAttackModifierDamageColorCode GetColorCodeForAttackModifier(string mathModifier)
	{
		switch (mathModifier)
		{
		case "x2":
		case "*2":
			return EAttackModifierDamageColorCode.CriticalDamage;
		case "x0":
		case "*0":
			return EAttackModifierDamageColorCode.ZeroDamage;
		case "+1":
			return EAttackModifierDamageColorCode.PositiveDamage;
		case "-1":
		case "-2":
			return EAttackModifierDamageColorCode.negativeDamage;
		case "+0":
		case "-0":
			return EAttackModifierDamageColorCode.RegularDamage;
		default:
			return EAttackModifierDamageColorCode.RegularDamage;
		}
	}

	private void ResetUI()
	{
		Log.GUIInfo("[AttackModifiers]: ResetUI " + m_RollingModifiers.Count);
		try
		{
			foreach (AttackModElementUI rollingModifier in m_RollingModifiers)
			{
				ObjectPool.Recycle(rollingModifier.gameObject);
			}
			if (m_IconScaleRollingRoutine != null)
			{
				StopCoroutine(m_IconScaleRollingRoutine);
			}
			if (m_IconFadeRollingRoutine != null)
			{
				StopCoroutine(m_IconFadeRollingRoutine);
			}
			m_IconRolling.alpha = 0f;
			m_RollingModifiers.Clear();
			if (m_UsedModifier != null)
			{
				m_UsedModifier.Hide();
				ObjectPool.Recycle(m_UsedModifier.gameObject);
				m_UsedModifier = null;
				Log.GUIInfo("[AttackModifiers]: Reset Modifier");
			}
			if (m_NotUsedModifier != null)
			{
				m_NotUsedModifier.Hide();
				ObjectPool.Recycle(m_NotUsedModifier.gameObject);
				m_NotUsedModifier = null;
			}
			m_ContainerRect.anchoredPosition = m_ContainerPosLowered.anchoredPosition;
			m_ContainerDrawnRect.anchoredPosition = Vector2.zero;
		}
		catch (Exception exception)
		{
			Log.GUIException(exception, "Error trying to reset the attack modifier UI");
		}
	}

	public void ConfirmFinishedDrawing()
	{
		m_ConfirmFinishedDrawingInLogicThread = true;
	}

	private IEnumerator ShowModifierChange(List<AttackModElementUI> changedModifiers, CAttackSummary.TargetSummary targetSummary)
	{
		yield return Timekeeper.instance.WaitForSeconds(m_Config.ReplaceShowDelay * testTimeScaler);
		Log.GUIInfo("[AttackModifiers]: ShowModifiers - Play replace");
		foreach (AttackModElementUI modUI in changedModifiers)
		{
			bool finishedReveal = false;
			modUI.ShowReplaceValue(m_Config.ReplaceShowDuration * testTimeScaler, m_Config.ReplaceShowCurve, m_Config.ReplaceGlowDuration * testTimeScaler, m_Config.ReplaceGlowCurve, m_Config.ReplaceShineDelay * testTimeScaler, m_Config.ReplaceShineDuration * testTimeScaler, m_Config.ReplaceShineCurve, m_Config.IconSwivelDuration * testTimeScaler, m_Config.IconSwivelCurve, () =>
			{
				SetTextStyleForModifiers(modUI, targetSummary, useOriginalModifier: false);
			}, () =>
			{
				finishedReveal = true;
			});
			yield return new WaitUntil(() => finishedReveal);
			PlayPfxForModifiers(modUI, useOriginalModifier: false);
		}
	}

	public IEnumerator ShowModifiers(CAttackSummary.TargetSummary targetSummary, CAbilityAttack attackAbility)
	{
		try
		{
			Log.GUIInfo("[AttackModifiers]: ShowModifiers");
			ResetUI();
			m_Summary = targetSummary;
			m_ConfirmFinishedDrawingInLogicThread = false;
			s_AttackModifierBarFlowCanBegin = false;
			IsFlowActive = true;
			isDrawingModifiers = true;
			ActorEvents.GetActorEvents(Actor).RegisterEvent(ActorEvents.ActorEvent.StartDrawingModifiers);
		}
		catch (Exception arg)
		{
			Log.GUIError($"[AttackModifiers]: Init block exception: {arg}");
		}
		float initialSafetyTimeout = GlobalSettings.DefInstance.m_AttackModifierSettings.AttackModifierFlowBeginTimeout;
		while (!s_AttackModifierBarFlowCanBegin)
		{
			try
			{
				initialSafetyTimeout -= Timekeeper.instance.m_GlobalClock.unscaledDeltaTime;
				if (initialSafetyTimeout < 0f)
				{
					s_AttackModifierBarFlowCanBegin = true;
				}
			}
			catch (Exception arg2)
			{
				Log.GUIError($"[AttackModifiers]: Flow begin block exception: {arg2}");
			}
			yield return null;
		}
		try
		{
			Log.GUIInfo("[AttackModifiers]: ShowModifiers - Fade Attack Label");
			m_InfoBar.MoveToAttackMode(m_Config.InitialLabelFadeAwayDuration * testTimeScaler, m_Config.InitialLabelFadeAwayCurve);
		}
		catch (Exception arg3)
		{
			Log.GUIError($"[AttackModifiers]: Fade label block exception: {arg3}");
		}
		bool hasAdvantageStatus = false;
		bool hasRolling = false;
		List<AttackModElementUI> allDrawnModifierUIs = new List<AttackModElementUI>();
		bool hasRemainingModifiersToDraw = true;
		try
		{
			hasAdvantageStatus = targetSummary.OverallAdvantage != EAdvantageStatuses.None;
			hasRolling = targetSummary.DrawnAttackMods.Any((AttackModifierYMLData x) => x.Rolling);
		}
		catch (Exception arg4)
		{
			Log.GUIError($"[AttackModifiers]: Advantage setup block exception: {arg4}");
		}
		do
		{
			List<AttackModifierYMLData> remainingModifiers = new List<AttackModifierYMLData>();
			AttackModifierYMLData newModifier = null;
			AttackModElementUI modUI = null;
			int indexDrawnin = -1;
			float time = 0f;
			bool finishedBeingDrawn = false;
			List<AttackModifierYMLData> list;
			try
			{
				list = targetSummary.DrawnAttackMods.ToList();
				for (int num = allDrawnModifierUIs.Count; num < list.Count; num++)
				{
					remainingModifiers.Add(list[num]);
				}
				if (remainingModifiers.Any())
				{
					hasRemainingModifiersToDraw = true;
					newModifier = remainingModifiers.FirstOrDefault();
					if (newModifier != null)
					{
						indexDrawnin = list.IndexOf(newModifier);
						AttackModifierYMLData modifier = newModifier;
						Log.GUIInfo("[AttackModifiers]: ShowModifiers - Spawn Modifier");
						modUI = ObjectPool.Spawn(m_ModifierPrefab, m_ContainerRect);
						if (modUI == null)
						{
							Log.GUIError("[AttackModifiers]: Invalid object pooled from the ObjectPool");
							break;
						}
						modUI.transform.localScale = Vector3.one;
						modUI.m_Modifier = modifier;
						allDrawnModifierUIs.Add(modUI);
						List<AttackModifierYMLData> list2 = targetSummary.DrawnAttackMods.Where((AttackModifierYMLData x) => !x.Rolling).ToList();
						SetAdvantageColors(modUI, list2.Contains(newModifier) ? targetSummary : null);
						if (hasAdvantageStatus && !hasRolling)
						{
							if (list2.Count > 0 && list2[0] == newModifier)
							{
								modUI.transform.position = m_ModifierPosAdvantage1.position;
							}
							else
							{
								modUI.transform.position = m_ModifierPosAdvantage2.position;
							}
						}
						else
						{
							modUI.transform.position = m_ModifierPosNormal.position;
						}
						modUI.ShowIcon(m_Config.InitialIconScaleInDelay * testTimeScaler, m_Config.InitialIconScaleInDuration * testTimeScaler, m_Config.InitialIconScaleInCurve);
						AudioController.Play(m_Config.InitialIconAppearAudioEvent, 1f, m_Config.InitialIconAudioEventDelay);
						time = m_Config.InitialIconScaleInDelay * testTimeScaler + m_Config.InitialIconScaleInDuration * testTimeScaler + m_Config.IconSwivelDelay * testTimeScaler;
					}
				}
				else
				{
					hasRemainingModifiersToDraw = false;
				}
			}
			catch (Exception arg5)
			{
				Log.GUIError($"[AttackModifiers]: Determine remaining block exception: {arg5}");
			}
			if (!hasRemainingModifiersToDraw)
			{
				yield return new WaitForEndOfFrame();
				continue;
			}
			if (modUI == null)
			{
				yield return new WaitForEndOfFrame();
				continue;
			}
			yield return new WaitForSecondsFrozen(time);
			try
			{
				Log.GUIInfo("[AttackModifiers]: ShowModifiers - Show Modifier");
				SetTextStyleForModifiers(modUI, targetSummary);
				modUI.ShowValue(m_Config.IconSwivelDuration * testTimeScaler, m_Config.IconSwivelCurve);
			}
			catch (Exception arg6)
			{
				Log.GUIError($"[AttackModifiers]: Reveal modifier block exception: {arg6}");
			}
			yield return new WaitForSecondsFrozen(m_Config.IconSwivelDuration * testTimeScaler);
			try
			{
				UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.AttackModifierValueRevealed));
				PlayPfxForModifiers(modUI);
			}
			catch (Exception arg7)
			{
				Log.GUIError($"[AttackModifiers]: FinishedBeingDrawn pre-loop exception: {arg7}");
			}
			AttackModifierYMLData actualModifier;
			do
			{
				actualModifier = targetSummary.DrawnAttackMods[indexDrawnin];
				if (actualModifier.FinishedBeingDrawn)
				{
					finishedBeingDrawn = true;
				}
				yield return new WaitForEndOfFrame();
			}
			while (!finishedBeingDrawn);
			List<AttackModElementUI> list3 = new List<AttackModElementUI>();
			try
			{
				if (actualModifier.MathModifier != modUI.m_Modifier.MathModifier || (!string.IsNullOrEmpty(modUI.m_Modifier.OriginalModifier) && modUI.m_Modifier.MathModifier != modUI.m_Modifier.OriginalModifier))
				{
					list3.Add(modUI);
				}
				modUI.m_Modifier = actualModifier;
			}
			catch (Exception arg8)
			{
				Log.GUIError($"[AttackModifiers]: Modifier change detection exception: {arg8}");
			}
			if (list3.Count > 0)
			{
				yield return ShowModifierChange(list3, targetSummary);
			}
			list = targetSummary.DrawnAttackMods.ToList();
			for (int num2 = allDrawnModifierUIs.Count; num2 < list.Count; num2++)
			{
				remainingModifiers.Add(list[num2]);
			}
			if ((!newModifier.Rolling && !(hasAdvantageStatus & hasRolling)) || remainingModifiers.Count <= 1)
			{
				continue;
			}
			try
			{
				Log.GUIInfo("[AttackModifiers]: ShowModifiers - Make Space for Next Rolling Modifier");
				m_RollingModifiers.Add(modUI);
				m_IconRolling.transform.localScale = Vector2.one;
				m_IconRolling.alpha = 0f;
				(m_IconRolling.transform as RectTransform).anchoredPosition = iconRollingPosition;
			}
			catch (Exception arg9)
			{
				Log.GUIError($"[AttackModifiers]: Rolling prepare block exception: {arg9}");
			}
			yield return new WaitForSecondsFrozen(m_Config.ModifierRollingDelay * testTimeScaler);
			try
			{
				if (newModifier.Rolling)
				{
					RollModifier();
				}
			}
			catch (Exception arg10)
			{
				Log.GUIError($"[AttackModifiers]: Rolling execute block exception: {arg10}");
			}
			yield return new WaitForSecondsFrozen(m_Config.ModifierRollingDuration / 2f * testTimeScaler);
			try
			{
				modUI.transform.SetParent(m_ContainerDrawnRect);
			}
			catch (Exception arg11)
			{
				Log.GUIError($"[AttackModifiers]: Rolling reparent block exception: {arg11}");
			}
			yield return new WaitForSecondsFrozen(m_Config.ModifierRollingDuration / 2f * testTimeScaler);
			yield return CorrutineUtils.RectTransformMove(m_ContainerDrawnRect, 0f, m_Config.ModifierRollingMoveDuration * testTimeScaler, new Vector3(0f - m_ContainerDrawnRect.sizeDelta.x, 0f), m_Config.ModifierRollingMoveDurationCurve);
		}
		while (hasRemainingModifiersToDraw || !m_ConfirmFinishedDrawingInLogicThread);
		if (hasAdvantageStatus)
		{
			List<AttackModElementUI> modifiersToHide = new List<AttackModElementUI>();
			try
			{
				foreach (AttackModifierYMLData unusedModifier in targetSummary.NotUsedAttackMods)
				{
					AttackModElementUI attackModElementUI = allDrawnModifierUIs.SingleOrDefault((AttackModElementUI x) => x.m_Modifier == unusedModifier);
					attackModElementUI.HideValue(m_Config.UnusedModifierFadeAwayDelay * testTimeScaler, m_Config.UnusedModifierFadeAwayDuration * testTimeScaler, m_Config.UnusedModifierFadeAwayCurve);
					modifiersToHide.Add(attackModElementUI);
					m_RollingModifiers.Remove(attackModElementUI);
				}
			}
			catch (Exception arg12)
			{
				Log.GUIError($"[AttackModifiers]: Hide unused fade block exception: {arg12}");
			}
			yield return Timekeeper.instance.WaitForSeconds(m_Config.UnusedModifierFadeAwayDelay * testTimeScaler + m_Config.UnusedModifierFadeAwayDuration * testTimeScaler);
			try
			{
				foreach (AttackModElementUI item in modifiersToHide)
				{
					if (item != null)
					{
						ObjectPool.Recycle(item.gameObject);
					}
				}
			}
			catch (Exception arg13)
			{
				Log.GUIError($"[AttackModifiers]: Recycle unused block exception: {arg13}");
			}
		}
		try
		{
			m_UsedModifier = allDrawnModifierUIs.Where((AttackModElementUI x) => targetSummary.UsedAttackMods.Contains(x.m_Modifier)).LastOrDefault();
		}
		catch (Exception arg14)
		{
			Log.GUIError($"[AttackModifiers]: Determine used modifier block exception: {arg14}");
		}
		yield return new WaitForSecondsFrozen(m_Config.ModifierRollingFinishDelay * testTimeScaler);
		try
		{
			Log.GUIInfo("[AttackModifiers]: ShowModifiers - Finish");
			m_AttackModCoroutine = null;
			isDrawingModifiers = false;
			ActorEvents.GetActorEvents(Actor).RegisterEvent(ActorEvents.ActorEvent.FinishedDrawingModifiers);
		}
		catch (Exception arg15)
		{
			Log.GUIError($"[AttackModifiers]: Final cleanup block exception: {arg15}");
		}
	}

	private void RollModifier()
	{
		m_IconScaleRollingRoutine = CorrutineUtils.RectTransformLocalScale(m_IconRolling.transform as RectTransform, 0f, m_Config.ModifierRollingDuration * testTimeScaler, m_Config.ModifierRollingScale, m_Config.ModifierRollingDurationCurve);
		StartCoroutine(m_IconScaleRollingRoutine);
		m_IconFadeRollingRoutine = CorrutineUtils.FadeInOut(m_IconRolling, m_Config.ModifierRollingDuration * testTimeScaler, m_Config.ModifierRollingDurationCurve, 0.8f);
		StartCoroutine(m_IconFadeRollingRoutine);
	}

	public IEnumerator ShowAttackModifDamage(CAttackSummary.TargetSummary attackSummary, CAbilityAttack attackAbility, Action onFinished)
	{
		Log.GUIInfo("[AttackModifiers]: ShowAttackModifDamage " + (TimeManager.IsPaused || m_UsedModifier == null));
		float initialSafetyTimeout = GlobalSettings.DefInstance.m_AttackModifierSettings.AttackModifierFlowBeginTimeout;
		while (TimeManager.IsPaused || m_UsedModifier == null)
		{
			yield return null;
			if (!TimeManager.IsPaused)
			{
				initialSafetyTimeout -= Timekeeper.instance.m_GlobalClock.unscaledDeltaTime;
				if (initialSafetyTimeout < 0f)
				{
					break;
				}
			}
		}
		if (isDrawingModifiers)
		{
			isDrawingModifiers = false;
			ActorEvents.GetActorEvents(Actor).RegisterEvent(ActorEvents.ActorEvent.FinishedDrawingModifiers);
		}
		yield return Timekeeper.instance.WaitForSeconds(m_Config.DamageLabelDisplayDelay * testTimeScaler);
		Log.GUIInfo("[AttackModifiers]: ShowAttackModifDamage show label");
		m_ContainerRect.position = m_ContainerPosRaised.position;
		int num = attackSummary.CalcExpressionString(calcPostShieldScalars: true, calcShield: false);
		int num2 = attackSummary.CalcExpressionString(calcPostShieldScalars: true, calcShield: true);
		m_InfoBar.ShowDamage(num2, forceShow: true, m_InfoBar.GetCodeForDamage(num2 - num, attackAbility.Strength, attackSummary.OverallAdvantage == EAdvantageStatuses.Advantage, attackSummary.OverallAdvantage == EAdvantageStatuses.Disadvantage));
		m_InfoBar.FadeInfoLabel(0f, m_Config.LabelFadeAwayDelay * testTimeScaler, m_Config.LabelFadeAwayDuration * testTimeScaler, m_Config.LabelFadeAwayCurve);
		m_RollingModifiers.ForEach((AttackModElementUI it) =>
		{
			it.HideValue(m_Config.LabelFadeAwayDelay * testTimeScaler, m_Config.LabelFadeAwayDuration * testTimeScaler, m_Config.LabelFadeAwayCurve);
		});
		m_UsedModifier?.HideValue(m_Config.LabelFadeAwayDelay * testTimeScaler, m_Config.LabelFadeAwayDuration * testTimeScaler, m_Config.LabelFadeAwayCurve);
		yield return Timekeeper.instance.WaitForSeconds(Mathf.Max(m_Config.LabelFadeAwayDelay * testTimeScaler + m_Config.LabelFadeAwayDuration * testTimeScaler, 0f));
		Log.GUIInfo("[AttackModifiers]: ShowAttackModifDamage finish");
		m_AttackModFinishCoroutine = null;
		FinalizeFlow();
		onFinished?.Invoke();
	}

	public void FinalizeFlow()
	{
		Log.GUIInfo("[AttackModifiers]: FinalizeFlow " + m_RollingModifiers.Count);
		IsFlowActive = false;
		if (isDrawingModifiers)
		{
			isDrawingModifiers = false;
			ActorEvents.GetActorEvents(Actor).RegisterEvent(ActorEvents.ActorEvent.FinishedDrawingModifiers);
		}
		if (m_NotUsedModifier != null)
		{
			m_NotUsedModifier.Hide();
			ObjectPool.Recycle(m_NotUsedModifier.gameObject);
			m_NotUsedModifier = null;
		}
		m_InfoBar.FinalizeAttackFlow();
		m_HealthBar.FinalizeAttackFlow();
		if (m_UsedModifier != null)
		{
			m_UsedModifier.Hide();
			ObjectPool.Recycle(m_UsedModifier.gameObject);
			m_RollingModifiers.Remove(m_UsedModifier);
			m_UsedModifier = null;
		}
		m_RollingModifiers.ForEach((AttackModElementUI it) =>
		{
			ObjectPool.Recycle(it.gameObject);
		});
		m_RollingModifiers.Clear();
		UIContextualTutorialModule.OnEvent("ModifierDrawn", m_Summary);
	}
}
