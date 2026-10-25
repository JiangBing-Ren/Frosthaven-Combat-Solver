using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using UnityEngine;
using UnityEngine.UI;

public class InteractabilityManager : MonoBehaviour
{
	public static InteractabilityManager s_Instance;

	private List<InteractabilityIsolatedUIControl> m_ControlsCurrentlyIsolatable = new List<InteractabilityIsolatedUIControl>();

	private List<InteractabilityIsolatedUIControl> m_CurrentlyIsolatedControls = new List<InteractabilityIsolatedUIControl>();

	private List<(InteractabilityIsolatedUIControl control, bool disable)> m_CurrentlyExplicitlyDisabledControls = new List<(InteractabilityIsolatedUIControl, bool)>();

	private CLevelUIInteractionProfile m_CurrentlyLoadedProfile;

	public bool CurrentlyLoadedProfileIsNullOrDefault;

	private bool m_Invalidate;

	private CLevelUIInteractionProfile DefaultAdditionalControlsToAllow = new CLevelUIInteractionProfile
	{
		ControlsToAllow = new List<CLevelUIInteractionProfile.CLevelUIInteractionSpecific>
		{
			new CLevelUIInteractionProfile.CLevelUIInteractionSpecific(CLevelUIInteractionProfile.EIsolatedControlType.EscapeMenu, CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.LimitInteractionOnly),
			new CLevelUIInteractionProfile.CLevelUIInteractionSpecific(CLevelUIInteractionProfile.EIsolatedControlType.PersistentUI, CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.LimitInteractionOnly),
			new CLevelUIInteractionProfile.CLevelUIInteractionSpecific(CLevelUIInteractionProfile.EIsolatedControlType.LevelMessageWindow, CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.LimitInteractionOnly),
			new CLevelUIInteractionProfile.CLevelUIInteractionSpecific(CLevelUIInteractionProfile.EIsolatedControlType.ResultsScreen, CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.LimitInteractionOnly),
			new CLevelUIInteractionProfile.CLevelUIInteractionSpecific(CLevelUIInteractionProfile.EIsolatedControlType.StoryBox, CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.LimitInteractionOnly),
			new CLevelUIInteractionProfile.CLevelUIInteractionSpecific(CLevelUIInteractionProfile.EIsolatedControlType.GeneralInfoModule, CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.LimitInteractionOnly)
		}
	};

	private CLevelUIInteractionProfile DefaultMessagelessControlsToAllow = new CLevelUIInteractionProfile
	{
		ControlsToAllow = new List<CLevelUIInteractionProfile.CLevelUIInteractionSpecific>
		{
			new CLevelUIInteractionProfile.CLevelUIInteractionSpecific(CLevelUIInteractionProfile.EIsolatedControlType.EscapeMenu, CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.LimitInteractionOnly),
			new CLevelUIInteractionProfile.CLevelUIInteractionSpecific(CLevelUIInteractionProfile.EIsolatedControlType.PersistentUI, CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.LimitInteractionOnly),
			new CLevelUIInteractionProfile.CLevelUIInteractionSpecific(CLevelUIInteractionProfile.EIsolatedControlType.ResultsScreen, CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.LimitInteractionOnly),
			new CLevelUIInteractionProfile.CLevelUIInteractionSpecific(CLevelUIInteractionProfile.EIsolatedControlType.BottomBarConfirmButton, CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.LimitInteractionOnly),
			new CLevelUIInteractionProfile.CLevelUIInteractionSpecific(CLevelUIInteractionProfile.EIsolatedControlType.GeneralInfoModule, CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.LimitInteractionOnly)
		}
	};

	public bool HasNonDefaultIsolatedControls
	{
		get
		{
			foreach (InteractabilityIsolatedUIControl control in m_CurrentlyIsolatedControls)
			{
				if (!DefaultMessagelessControlsToAllow.ControlsToAllow.Any((CLevelUIInteractionProfile.CLevelUIInteractionSpecific x) => x.ControlType == control.ControlType))
				{
					return true;
				}
			}
			return false;
		}
	}

	public List<InteractabilityIsolatedUIControl> CurrentlyIsolatedControls => m_CurrentlyIsolatedControls;

	public CLevelUIInteractionProfile CurrentlyLoadedProfile => m_CurrentlyLoadedProfile;

	public List<InteractabilityIsolatedUIControl> ControlsCurrentlyIsolatable => m_ControlsCurrentlyIsolatable;

	private void Awake()
	{
		if (s_Instance == null)
		{
			s_Instance = this;
		}
		if (!CurrentlyLoadedProfileIsNullOrDefault && m_CurrentlyLoadedProfile == null)
		{
			LoadDefaultMessagelessProfile();
		}
	}

	private void OnDestroy()
	{
		if (s_Instance == this)
		{
			s_Instance = null;
		}
	}

	private void Update()
	{
		if (m_Invalidate)
		{
			m_Invalidate = false;
			RefreshInteractability(instant: true);
		}
	}

	public static void RegisterControlForInteractionLimiting(InteractabilityIsolatedUIControl controlToRegister)
	{
		if (s_Instance == null && controlToRegister.ControlType == CLevelUIInteractionProfile.EIsolatedControlType.PersistentUI)
		{
			s_Instance = controlToRegister.GetComponent<InteractabilityManager>();
		}
		if (s_Instance != null && !s_Instance.m_ControlsCurrentlyIsolatable.Contains(controlToRegister))
		{
			s_Instance.m_ControlsCurrentlyIsolatable.Add(controlToRegister);
			if (s_Instance.m_CurrentlyLoadedProfile != null)
			{
				if (!s_Instance.CurrentlyLoadedProfileIsNullOrDefault)
				{
					s_Instance.AddControlsFromProfileToCurrentState(s_Instance.DefaultAdditionalControlsToAllow);
				}
				s_Instance.AddControlsFromProfileToCurrentState(s_Instance.m_CurrentlyLoadedProfile);
				RefreshInteractability();
			}
		}
		DeregisterInvalidControls();
	}

	public static void DeregisterControlForInteractionLimiting(InteractabilityIsolatedUIControl controlToDeregister)
	{
		if (s_Instance != null)
		{
			RemoveIsolatedControl(controlToDeregister);
			RemoveExplicilyDisabledControl(controlToDeregister);
			DeregisterInvalidControls();
		}
	}

	public static void DeregisterInvalidControls()
	{
		if (!(s_Instance == null))
		{
			s_Instance.m_ControlsCurrentlyIsolatable.RemoveAll((InteractabilityIsolatedUIControl c) => c == null || c.gameObject == null);
			s_Instance.m_CurrentlyIsolatedControls.RemoveAll((InteractabilityIsolatedUIControl c) => c == null || c.gameObject == null);
			s_Instance.m_CurrentlyExplicitlyDisabledControls.RemoveAll(((InteractabilityIsolatedUIControl control, bool disable) c) => c.control == null || c.control?.gameObject == null);
		}
	}

	public static void AddIsolatedControl(InteractabilityIsolatedUIControl controlToRegister)
	{
		if (s_Instance != null && controlToRegister != null && controlToRegister.gameObject != null && !s_Instance.m_CurrentlyIsolatedControls.Contains(controlToRegister))
		{
			s_Instance.m_CurrentlyIsolatedControls.Add(controlToRegister);
			RefreshInteractability(controlToRegister);
		}
	}

	public static void AddExplicilyDisabledControl(InteractabilityIsolatedUIControl controlToRegister, bool disabled)
	{
		if (s_Instance != null && controlToRegister != null && controlToRegister.gameObject != null && !s_Instance.m_CurrentlyExplicitlyDisabledControls.Any(((InteractabilityIsolatedUIControl control, bool disable) x) => x.control == controlToRegister))
		{
			s_Instance.m_CurrentlyExplicitlyDisabledControls.Add((controlToRegister, disabled));
			RefreshInteractability(controlToRegister);
		}
	}

	public static void RemoveIsolatedControl(InteractabilityIsolatedUIControl control)
	{
		if (s_Instance != null && control != null && control.gameObject != null && s_Instance.m_CurrentlyIsolatedControls.Contains(control))
		{
			s_Instance.m_CurrentlyIsolatedControls.Remove(control);
			RefreshInteractability(control);
		}
	}

	public static void RemoveExplicilyDisabledControl(InteractabilityIsolatedUIControl control)
	{
		if (s_Instance != null && control != null && control.gameObject != null && s_Instance.m_CurrentlyExplicitlyDisabledControls.Any(((InteractabilityIsolatedUIControl control, bool disable) x) => x.control == control))
		{
			s_Instance.m_CurrentlyExplicitlyDisabledControls.RemoveAll(((InteractabilityIsolatedUIControl control, bool disable) x) => x.control == control);
			RefreshInteractability(control);
		}
	}

	public static void ClearIsolatedControls()
	{
		if (s_Instance?.m_CurrentlyIsolatedControls == null)
		{
			return;
		}
		foreach (InteractabilityIsolatedUIControl item in s_Instance.m_CurrentlyIsolatedControls.ToList())
		{
			RemoveIsolatedControl(item);
		}
	}

	public static void RefreshInteractability(bool instant = false)
	{
		if (!instant)
		{
			s_Instance.m_Invalidate = true;
		}
		else
		{
			if (s_Instance?.m_ControlsCurrentlyIsolatable == null)
			{
				return;
			}
			foreach (InteractabilityIsolatedUIControl item in s_Instance.m_ControlsCurrentlyIsolatable.ToList())
			{
				RefreshInteractability(item);
			}
		}
	}

	public static void RefreshInteractability(InteractabilityIsolatedUIControl control)
	{
		if (!(s_Instance != null) || !(control != null) || !(control.gameObject != null))
		{
			return;
		}
		bool flag = !s_Instance.m_CurrentlyExplicitlyDisabledControls.Any(((InteractabilityIsolatedUIControl control, bool disable) x) => x.control == control) && (s_Instance.CurrentlyLoadedProfileIsNullOrDefault || s_Instance.m_CurrentlyIsolatedControls.Contains(control));
		if (!flag && s_Instance.m_CurrentlyLoadedProfile != null && s_Instance.m_CurrentlyLoadedProfile.ControlsToAllow.Any((CLevelUIInteractionProfile.CLevelUIInteractionSpecific x) => s_Instance.MatchControlInProfileToControlInScene(x, control) && (x.ControlBehaviour == CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.TreatAsInteractableOnlyForVisuals || x.ControlBehaviour == CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.HighlightAndTreatAsInteractableForVisuals)))
		{
			flag = true;
		}
		bool flag2 = !s_Instance.m_CurrentlyExplicitlyDisabledControls.Any(((InteractabilityIsolatedUIControl control, bool disable) x) => x.control == control && x.disable);
		switch (control.ControlType)
		{
		case CLevelUIInteractionProfile.EIsolatedControlType.StoryBox:
		{
			UICharacterStoryBox componentInChildren = control.GetComponentInChildren<UICharacterStoryBox>();
			if (componentInChildren != null)
			{
				componentInChildren.SetupInteractability(control);
				componentInChildren.interactabilityRef.Toggle(flag, flag2);
			}
			break;
		}
		case CLevelUIInteractionProfile.EIsolatedControlType.CardSelection:
		{
			AbilityCardUI component7 = control.GetComponent<AbilityCardUI>();
			if (component7 != null)
			{
				if (component7.interactability == null)
				{
					component7.SetupInteractability(control);
				}
				component7.interactability.Toggle(flag, flag2);
			}
			break;
		}
		case CLevelUIInteractionProfile.EIsolatedControlType.CharacterTab:
		{
			CardsHandManager component3 = control.GetComponent<CardsHandManager>();
			if (component3 != null)
			{
				if (component3.interactability == null)
				{
					component3.SetupInteractability(control);
				}
				component3.interactability.Toggle(flag, flag2);
			}
			break;
		}
		case CLevelUIInteractionProfile.EIsolatedControlType.BottomBarConfirmButton:
		{
			ReadyButton component = control.GetComponent<ReadyButton>();
			if (component != null)
			{
				if (component.interactability == null)
				{
					component.SetupInteractability(control);
				}
				component.interactability.Toggle(flag, flag2);
			}
			break;
		}
		case CLevelUIInteractionProfile.EIsolatedControlType.BottomBarUndoButton:
		{
			UndoButton component5 = control.GetComponent<UndoButton>();
			if (component5 != null)
			{
				if (component5.interactability == null)
				{
					component5.SetupInteractability(control);
				}
				component5.interactability.Toggle(flag, flag2);
			}
			break;
		}
		case CLevelUIInteractionProfile.EIsolatedControlType.BottomBarSkipButton:
		{
			SkipButton component4 = control.GetComponent<SkipButton>();
			if (component4 != null)
			{
				if (component4.interactability == null)
				{
					component4.SetupInteractability(control);
				}
				component4.interactability.Toggle(flag, flag2);
			}
			break;
		}
		case CLevelUIInteractionProfile.EIsolatedControlType.ShortRestButton:
		{
			ShortRest component2 = control.GetComponent<ShortRest>();
			if (component2 != null)
			{
				if (component2.interactability == null)
				{
					component2.SetupInteractability(control);
				}
				component2.interactability.Toggle(flag, flag2);
			}
			break;
		}
		case CLevelUIInteractionProfile.EIsolatedControlType.CardTopHalfSelection:
		case CLevelUIInteractionProfile.EIsolatedControlType.CardBottomHalfSelection:
		{
			FullAbilityCardAction component6 = control.GetComponent<FullAbilityCardAction>();
			if (component6.actionInteractability == null)
			{
				component6.SetupActionInteractability(control);
			}
			component6.actionInteractability.Toggle(flag, flag2);
			break;
		}
		case CLevelUIInteractionProfile.EIsolatedControlType.DefaultMoveAbility:
		case CLevelUIInteractionProfile.EIsolatedControlType.DefaultAttackAbility:
		{
			FullAbilityCardAction fullAbilityCardAction = control.GetComponent<FullAbilityCardAction>() ?? control.transform.GetComponentInParent<FullAbilityCardAction>(includeInactive: true);
			if (fullAbilityCardAction.defaultActionInteractability == null)
			{
				fullAbilityCardAction.SetupDefaultActionInteractability(control);
			}
			fullAbilityCardAction.defaultActionInteractability.Toggle(flag, flag2);
			break;
		}
		case CLevelUIInteractionProfile.EIsolatedControlType.TakeDamageButton:
		{
			TakeDamagePanel takeDamagePanel3 = control.transform?.parent?.parent?.GetComponent<TakeDamagePanel>();
			if (takeDamagePanel3 != null)
			{
				if (takeDamagePanel3.takeDamageButtonInteractability == null)
				{
					takeDamagePanel3.SetupTakeDamageInteractability(control);
				}
				takeDamagePanel3.takeDamageButtonInteractability.Toggle(flag, flag2);
			}
			break;
		}
		case CLevelUIInteractionProfile.EIsolatedControlType.BurnAvailableButton:
		{
			TakeDamagePanel takeDamagePanel = control.transform?.parent?.parent?.GetComponent<TakeDamagePanel>();
			if (takeDamagePanel != null)
			{
				if (takeDamagePanel.burnAvailableCardsToggleInteractability == null)
				{
					takeDamagePanel.SetupBurnAvailableCardsToggleInteractability(control);
				}
				takeDamagePanel.burnAvailableCardsToggleInteractability.Toggle(flag, flag2);
			}
			break;
		}
		case CLevelUIInteractionProfile.EIsolatedControlType.BurnDiscardedButton:
		{
			TakeDamagePanel takeDamagePanel2 = control.transform?.parent?.parent?.GetComponent<TakeDamagePanel>();
			if (takeDamagePanel2 != null)
			{
				if (takeDamagePanel2.burnDiscardedCardsInteractability == null)
				{
					takeDamagePanel2.SetupBurnDiscardedCardsInteractability(control);
				}
				takeDamagePanel2.burnDiscardedCardsInteractability.Toggle(flag, flag2);
			}
			break;
		}
		case CLevelUIInteractionProfile.EIsolatedControlType.ScenarioObjectivesAndModifiers:
		case CLevelUIInteractionProfile.EIsolatedControlType.BottomBarHelpText:
		case CLevelUIInteractionProfile.EIsolatedControlType.ElementInfusionBoard:
		case CLevelUIInteractionProfile.EIsolatedControlType.CombatLog:
			if (control != null && control.gameObject.activeSelf != flag2 && !control.destroyCancellationToken.IsCancellationRequested)
			{
				control.gameObject.SetActive(flag2);
			}
			break;
		case CLevelUIInteractionProfile.EIsolatedControlType.AttackModifierHoverButton:
		case CLevelUIInteractionProfile.EIsolatedControlType.TileSelection:
		case CLevelUIInteractionProfile.EIsolatedControlType.ShortRestConfirmButton:
		case CLevelUIInteractionProfile.EIsolatedControlType.ShortRestCancelButton:
		case CLevelUIInteractionProfile.EIsolatedControlType.ShortRestDialogOption:
		case CLevelUIInteractionProfile.EIsolatedControlType.CharacterPortrait:
		case CLevelUIInteractionProfile.EIsolatedControlType.ConsumeElementOnCardTop:
		case CLevelUIInteractionProfile.EIsolatedControlType.ConsumeElementOnCardBottom:
		case CLevelUIInteractionProfile.EIsolatedControlType.ItemSlotButton:
		case CLevelUIInteractionProfile.EIsolatedControlType.CharacterTabIcon:
		case CLevelUIInteractionProfile.EIsolatedControlType.ReduceDamageShieldItemButton:
		case CLevelUIInteractionProfile.EIsolatedControlType.BottomBarCameraRoomButton:
		case CLevelUIInteractionProfile.EIsolatedControlType.BottomBarWarningConfirmButton:
		case CLevelUIInteractionProfile.EIsolatedControlType.SpeedUpButton:
		case CLevelUIInteractionProfile.EIsolatedControlType.CardSelectionInitiative:
		case CLevelUIInteractionProfile.EIsolatedControlType.DialogOptionButton:
		case CLevelUIInteractionProfile.EIsolatedControlType.GeneralInfoModule:
			break;
		}
	}

	public static bool ShouldTryPreventControl()
	{
		if (s_Instance != null && s_Instance.HasNonDefaultIsolatedControls)
		{
			if (LevelEventsController.s_EventsControllerActive)
			{
				return true;
			}
			if (Singleton<InteractabilityChecker>.Instance != null && Singleton<InteractabilityChecker>.Instance.IsActive)
			{
				return true;
			}
		}
		return false;
	}

	public static bool ShouldAllowClickForExtendedButton(ExtendedButton buttonToCheck)
	{
		if (!LevelEventsController.s_EventsControllerActive)
		{
			return true;
		}
		if (buttonToCheck == null)
		{
			return true;
		}
		foreach (var currentlyExplicitlyDisabledControl in s_Instance.m_CurrentlyExplicitlyDisabledControls)
		{
			if (currentlyExplicitlyDisabledControl.control.ExtendedButtonsToAllow.Any((ExtendedButton button) => button != null && button.gameObject == buttonToCheck.gameObject))
			{
				return false;
			}
		}
		if (ShouldTryPreventControl())
		{
			if (s_Instance?.m_CurrentlyLoadedProfile?.ControlsToAllow != null && s_Instance.m_CurrentlyLoadedProfile.ControlsToAllow.Any((CLevelUIInteractionProfile.CLevelUIInteractionSpecific c) => c.ControlBehaviour == CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.HighlightAndAllowAnyInteraction))
			{
				return true;
			}
			foreach (InteractabilityIsolatedUIControl currentlyIsolatedControl in s_Instance.m_CurrentlyIsolatedControls)
			{
				if (currentlyIsolatedControl.ExtendedButtonsToAllow.Any((ExtendedButton button) => button != null && button.gameObject == buttonToCheck.gameObject))
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	public static bool ShouldAllowClickForExtendedToggle(ExtendedToggle toggleToCheck)
	{
		if (!LevelEventsController.s_EventsControllerActive)
		{
			return true;
		}
		if (toggleToCheck == null)
		{
			return true;
		}
		foreach (var currentlyExplicitlyDisabledControl in s_Instance.m_CurrentlyExplicitlyDisabledControls)
		{
			if (currentlyExplicitlyDisabledControl.control.ExtendedTogglesToAllow.Any((ExtendedToggle button) => button != null && button.gameObject == toggleToCheck.gameObject))
			{
				return false;
			}
		}
		if (ShouldTryPreventControl())
		{
			if (s_Instance?.m_CurrentlyLoadedProfile?.ControlsToAllow != null && s_Instance.m_CurrentlyLoadedProfile.ControlsToAllow.Any((CLevelUIInteractionProfile.CLevelUIInteractionSpecific c) => c.ControlBehaviour == CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.HighlightAndAllowAnyInteraction))
			{
				return true;
			}
			foreach (InteractabilityIsolatedUIControl currentlyIsolatedControl in s_Instance.m_CurrentlyIsolatedControls)
			{
				if (currentlyIsolatedControl.ExtendedTogglesToAllow.Any((ExtendedToggle toggle) => toggle != null && toggle.gameObject == toggleToCheck.gameObject))
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	public static bool ShouldAllowClickForFHButton(FHButton buttonToCheck)
	{
		if (!LevelEventsController.s_EventsControllerActive)
		{
			return true;
		}
		if (buttonToCheck == null)
		{
			return true;
		}
		foreach (var currentlyExplicitlyDisabledControl in s_Instance.m_CurrentlyExplicitlyDisabledControls)
		{
			if (currentlyExplicitlyDisabledControl.control.FHButtonsToAllow.Any((FHButton button) => button != null && button.gameObject == buttonToCheck.gameObject))
			{
				return false;
			}
		}
		if (ShouldTryPreventControl())
		{
			if (s_Instance?.m_CurrentlyLoadedProfile?.ControlsToAllow != null && s_Instance.m_CurrentlyLoadedProfile.ControlsToAllow.Any((CLevelUIInteractionProfile.CLevelUIInteractionSpecific c) => c.ControlBehaviour == CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.HighlightAndAllowAnyInteraction))
			{
				return true;
			}
			foreach (InteractabilityIsolatedUIControl currentlyIsolatedControl in s_Instance.m_CurrentlyIsolatedControls)
			{
				if (currentlyIsolatedControl.FHButtonsToAllow.Any((FHButton button) => button != null && button.gameObject == buttonToCheck.gameObject))
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	public static bool ShouldAllowClickForTrackedButton(TrackedButton buttonToCheck)
	{
		if (!LevelEventsController.s_EventsControllerActive)
		{
			return true;
		}
		if (buttonToCheck == null)
		{
			return true;
		}
		foreach (var currentlyExplicitlyDisabledControl in s_Instance.m_CurrentlyExplicitlyDisabledControls)
		{
			if (currentlyExplicitlyDisabledControl.control.TrackedButtonsToAllow.Any((TrackedButton button) => button != null && button.gameObject == buttonToCheck.gameObject))
			{
				return false;
			}
		}
		if (ShouldTryPreventControl())
		{
			if (s_Instance?.m_CurrentlyLoadedProfile?.ControlsToAllow != null && s_Instance.m_CurrentlyLoadedProfile.ControlsToAllow.Any((CLevelUIInteractionProfile.CLevelUIInteractionSpecific c) => c.ControlBehaviour == CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.HighlightAndAllowAnyInteraction))
			{
				return true;
			}
			foreach (InteractabilityIsolatedUIControl currentlyIsolatedControl in s_Instance.m_CurrentlyIsolatedControls)
			{
				if (currentlyIsolatedControl.TrackedButtonsToAllow.Any((TrackedButton button) => button != null && button.gameObject == buttonToCheck.gameObject))
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	public static bool ShouldAllowClickForTrackedToggle(TrackedToggle toggleToCheck)
	{
		if (!LevelEventsController.s_EventsControllerActive)
		{
			return true;
		}
		if (toggleToCheck == null)
		{
			return true;
		}
		foreach (var currentlyExplicitlyDisabledControl in s_Instance.m_CurrentlyExplicitlyDisabledControls)
		{
			if (currentlyExplicitlyDisabledControl.control.TrackedTogglesToAllow.Any((TrackedToggle button) => button != null && button.gameObject == toggleToCheck.gameObject))
			{
				return false;
			}
		}
		if (ShouldTryPreventControl())
		{
			if (s_Instance?.m_CurrentlyLoadedProfile?.ControlsToAllow != null && s_Instance.m_CurrentlyLoadedProfile.ControlsToAllow.Any((CLevelUIInteractionProfile.CLevelUIInteractionSpecific c) => c.ControlBehaviour == CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.HighlightAndAllowAnyInteraction))
			{
				return true;
			}
			foreach (InteractabilityIsolatedUIControl currentlyIsolatedControl in s_Instance.m_CurrentlyIsolatedControls)
			{
				if (currentlyIsolatedControl.TrackedTogglesToAllow.Any((TrackedToggle toggle) => toggle != null && toggle.gameObject == toggleToCheck.gameObject))
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	public static bool ShouldAllowClickForFHToggle(FHToggle toggleToCheck)
	{
		if (!LevelEventsController.s_EventsControllerActive)
		{
			return true;
		}
		if (toggleToCheck == null)
		{
			return true;
		}
		foreach (var currentlyExplicitlyDisabledControl in s_Instance.m_CurrentlyExplicitlyDisabledControls)
		{
			if (currentlyExplicitlyDisabledControl.control.FHTogglesToAllow.Any((FHToggle button) => button != null && button.gameObject == toggleToCheck.gameObject))
			{
				return false;
			}
		}
		if (ShouldTryPreventControl())
		{
			if (s_Instance?.m_CurrentlyLoadedProfile?.ControlsToAllow != null && s_Instance.m_CurrentlyLoadedProfile.ControlsToAllow.Any((CLevelUIInteractionProfile.CLevelUIInteractionSpecific c) => c.ControlBehaviour == CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.HighlightAndAllowAnyInteraction))
			{
				return true;
			}
			foreach (InteractabilityIsolatedUIControl currentlyIsolatedControl in s_Instance.m_CurrentlyIsolatedControls)
			{
				if (currentlyIsolatedControl.FHTogglesToAllow.Any((FHToggle toggle) => toggle != null && toggle.gameObject == toggleToCheck.gameObject))
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	public static bool ShouldAllowClickForTab(UITab toggleToCheck)
	{
		if (!LevelEventsController.s_EventsControllerActive)
		{
			return true;
		}
		if (toggleToCheck == null)
		{
			return true;
		}
		foreach (var currentlyExplicitlyDisabledControl in s_Instance.m_CurrentlyExplicitlyDisabledControls)
		{
			if (currentlyExplicitlyDisabledControl.control.TabsToAllow.Any((UITab button) => button != null && button.gameObject == toggleToCheck.gameObject))
			{
				return false;
			}
		}
		if (ShouldTryPreventControl())
		{
			if (s_Instance?.m_CurrentlyLoadedProfile?.ControlsToAllow != null && s_Instance.m_CurrentlyLoadedProfile.ControlsToAllow.Any((CLevelUIInteractionProfile.CLevelUIInteractionSpecific c) => c.ControlBehaviour == CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.HighlightAndAllowAnyInteraction))
			{
				return true;
			}
			foreach (InteractabilityIsolatedUIControl currentlyIsolatedControl in s_Instance.m_CurrentlyIsolatedControls)
			{
				if (currentlyIsolatedControl.TabsToAllow.Any((UITab toggle) => toggle != null && toggle.gameObject == toggleToCheck.gameObject))
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	public static bool ShouldAllowSelectionForTileIndex(Point pointToCheck)
	{
		if (!LevelEventsController.s_EventsControllerActive)
		{
			return true;
		}
		if (ShouldTryPreventControl())
		{
			if (s_Instance?.m_CurrentlyLoadedProfile?.ControlsToAllow != null && s_Instance.m_CurrentlyLoadedProfile.ControlsToAllow.Any((CLevelUIInteractionProfile.CLevelUIInteractionSpecific c) => c.ControlBehaviour == CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.HighlightAndAllowAnyInteraction))
			{
				return true;
			}
			if (s_Instance?.m_CurrentlyLoadedProfile?.ControlsToAllow != null && s_Instance.m_CurrentlyLoadedProfile.ControlsToAllow.Any((CLevelUIInteractionProfile.CLevelUIInteractionSpecific c) => c.ControlType == CLevelUIInteractionProfile.EIsolatedControlType.TileSelection && c.ControlBehaviour != CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.HighlightOnly && c.ControlBehaviour != CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.HighlightOnlyObsolete && (c.ControlTileIndex == null || (c.ControlTileIndex.X == pointToCheck.X && c.ControlTileIndex.Y == pointToCheck.Y))))
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public void LoadProfile(CLevelUIInteractionProfile profileToLoad)
	{
		try
		{
			InteractabilityHighlightCanvas.s_Instance?.ClearHighlights();
			ClearIsolatedControls();
			m_CurrentlyLoadedProfile = profileToLoad;
			AddControlsFromProfileToCurrentState(DefaultAdditionalControlsToAllow);
			if (m_CurrentlyLoadedProfile != null)
			{
				CurrentlyLoadedProfileIsNullOrDefault = false;
				AddControlsFromProfileToCurrentState(profileToLoad);
			}
			else
			{
				CurrentlyLoadedProfileIsNullOrDefault = true;
			}
			RefreshInteractability();
		}
		catch (Exception ex)
		{
			m_CurrentlyLoadedProfile = null;
			CurrentlyLoadedProfileIsNullOrDefault = true;
			ClearIsolatedControls();
			Log.GUIInfo("Exception thrown trying to load interactibility profile:" + ex.Message + " | StackTrace:" + ex.StackTrace);
		}
	}

	public void LoadDefaultMessagelessProfile()
	{
		try
		{
			InteractabilityHighlightCanvas.s_Instance?.ClearHighlights();
			ClearIsolatedControls();
			m_CurrentlyLoadedProfile = DefaultMessagelessControlsToAllow;
			AddControlsFromProfileToCurrentState(DefaultMessagelessControlsToAllow);
			CurrentlyLoadedProfileIsNullOrDefault = true;
			RefreshInteractability();
		}
		catch (Exception ex)
		{
			m_CurrentlyLoadedProfile = null;
			CurrentlyLoadedProfileIsNullOrDefault = true;
			ClearIsolatedControls();
			RefreshInteractability();
			Log.GUIInfo("Exception thrown trying to load default messageless interactibility profile:" + ex.Message + " | StackTrace:" + ex.StackTrace);
		}
	}

	public void LoadDefaultDismissMessageProfile()
	{
		try
		{
			InteractabilityHighlightCanvas.s_Instance?.ClearHighlights();
			ClearIsolatedControls();
			m_CurrentlyLoadedProfile = DefaultAdditionalControlsToAllow;
			AddControlsFromProfileToCurrentState(DefaultAdditionalControlsToAllow);
			RefreshInteractability();
		}
		catch (Exception ex)
		{
			m_CurrentlyLoadedProfile = null;
			CurrentlyLoadedProfileIsNullOrDefault = true;
			ClearIsolatedControls();
			RefreshInteractability();
			Log.GUIInfo("Exception thrown trying to load default Dismiss button interactibility profile:" + ex.Message + " | StackTrace:" + ex.StackTrace);
		}
	}

	public void AddControlsFromProfileToCurrentState(CLevelUIInteractionProfile profileToAdd)
	{
		if (profileToAdd == null)
		{
			return;
		}
		foreach (CLevelUIInteractionProfile.CLevelUIInteractionSpecific item in profileToAdd.ControlsToAllow)
		{
			if (item.ControlBehaviour == CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.TreatAsInteractableOnlyForVisuals)
			{
				continue;
			}
			if (item.ControlType == CLevelUIInteractionProfile.EIsolatedControlType.TileSelection)
			{
				if (item.ControlTileIndex != null && item.ControlBehaviour != CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.LimitInteractionOnly)
				{
					InteractabilityHighlightCanvas.s_Instance?.AddHighlightToTile(item.ControlTileIndex, item.HighlightPrefab);
				}
				continue;
			}
			foreach (InteractabilityIsolatedUIControl item2 in m_ControlsCurrentlyIsolatable)
			{
				if (!MatchControlInProfileToControlInScene(item, item2))
				{
					continue;
				}
				if (item.ControlBehaviour != CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.HighlightOnly && item.ControlBehaviour != CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.HighlightAndTreatAsInteractableForVisuals)
				{
					AddIsolatedControl(item2);
				}
				if (item.ControlBehaviour != CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.LimitInteractionOnly)
				{
					InteractabilityHighlightCanvas interactabilityHighlightCanvas = InteractabilityHighlightCanvas.s_Instance;
					if ((object)interactabilityHighlightCanvas != null)
					{
						RectTransform highlightRectTransform = item2.GetHighlightRectTransform();
						string highlightPrefab = item.HighlightPrefab;
						interactabilityHighlightCanvas.AddHighlightForRectTransform(highlightRectTransform, item2, default, highlightPrefab);
					}
				}
			}
		}
	}

	public static bool AllowInteractabilityHighlight(InteractabilityIsolatedUIControl control)
	{
		if (s_Instance.m_CurrentlyLoadedProfile != null)
		{
			return s_Instance.m_CurrentlyLoadedProfile.ControlsToAllow.Any((CLevelUIInteractionProfile.CLevelUIInteractionSpecific x) => s_Instance.MatchControlInProfileToControlInScene(x, control) && (x.ControlBehaviour == CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.TreatAsInteractableOnlyForVisuals || x.ControlBehaviour == CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.HighlightAndTreatAsInteractableForVisuals));
		}
		return false;
	}

	public bool MatchControlInProfileToControlInScene(CLevelUIInteractionProfile.CLevelUIInteractionSpecific controlInProfile, InteractabilityIsolatedUIControl controlInScene)
	{
		if (controlInProfile == null || controlInScene == null || controlInScene.gameObject == null)
		{
			return false;
		}
		if (controlInProfile.ControlType == controlInScene.ControlType)
		{
			switch (controlInProfile.ControlType)
			{
			case CLevelUIInteractionProfile.EIsolatedControlType.CardSelection:
			case CLevelUIInteractionProfile.EIsolatedControlType.CardTopHalfSelection:
			case CLevelUIInteractionProfile.EIsolatedControlType.CardBottomHalfSelection:
			case CLevelUIInteractionProfile.EIsolatedControlType.ShortRestDialogOption:
			case CLevelUIInteractionProfile.EIsolatedControlType.DefaultMoveAbility:
			case CLevelUIInteractionProfile.EIsolatedControlType.DefaultAttackAbility:
			case CLevelUIInteractionProfile.EIsolatedControlType.CharacterPortrait:
			case CLevelUIInteractionProfile.EIsolatedControlType.CardSelectionInitiative:
			case CLevelUIInteractionProfile.EIsolatedControlType.DialogOptionButton:
				if (!controlInProfile.ControlIdentifier.Equals(controlInScene.ControlIdentifier))
				{
					return controlInScene.ControlIdentifier.Contains(controlInProfile.ControlIdentifier.Replace(" ", string.Empty).Replace("'", string.Empty));
				}
				return true;
			case CLevelUIInteractionProfile.EIsolatedControlType.ConsumeElementOnCardTop:
			case CLevelUIInteractionProfile.EIsolatedControlType.ConsumeElementOnCardBottom:
				if (controlInProfile.ControlIdentifier.Equals(controlInScene.ControlIdentifier))
				{
					return controlInProfile.ControlIndex == controlInScene.ControlIndex;
				}
				return false;
			case CLevelUIInteractionProfile.EIsolatedControlType.ItemSlotButton:
			case CLevelUIInteractionProfile.EIsolatedControlType.UseItemSlot:
				if (((CItem.EItemSlot)controlInProfile.ControlContextTypeInt/*cast due to constrained. prefix*/).ToString().Equals(controlInScene.ControlIdentifier) && (controlInProfile.ControlIndex < 0 || controlInProfile.ControlIndex == controlInScene.ControlIndex))
				{
					if (!string.IsNullOrEmpty(controlInProfile.ControlIdentifier2))
					{
						return controlInProfile.ControlIdentifier2 == controlInScene.ControlSecondIdentifier;
					}
					return true;
				}
				return false;
			case CLevelUIInteractionProfile.EIsolatedControlType.ShortRestButton:
			case CLevelUIInteractionProfile.EIsolatedControlType.ShortRestConfirmButton:
			case CLevelUIInteractionProfile.EIsolatedControlType.ShortRestCancelButton:
			case CLevelUIInteractionProfile.EIsolatedControlType.CharacterTabIcon:
				if (!string.IsNullOrEmpty(controlInProfile.ControlIdentifier2))
				{
					return controlInProfile.ControlIdentifier2 == controlInScene.ControlSecondIdentifier;
				}
				return true;
			default:
				return true;
			}
		}
		return false;
	}

	public void ConfigureDefaultControlsForMessage(CLevelMessage messageDisplayed)
	{
		if (messageDisplayed.LayoutType != CLevelMessage.ELevelMessageLayoutType.HelpText && messageDisplayed.DismissTrigger.IsTriggeredByDismiss)
		{
			DefaultAdditionalControlsToAllow.ControlsToAllow.Single((CLevelUIInteractionProfile.CLevelUIInteractionSpecific ic) => ic.ControlType == CLevelUIInteractionProfile.EIsolatedControlType.LevelMessageWindow).ControlBehaviour = CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.HighlightAndLimitInteraction;
		}
		else
		{
			DefaultAdditionalControlsToAllow.ControlsToAllow.Single((CLevelUIInteractionProfile.CLevelUIInteractionSpecific ic) => ic.ControlType == CLevelUIInteractionProfile.EIsolatedControlType.LevelMessageWindow).ControlBehaviour = CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.LimitInteractionOnly;
		}
	}

	[ContextMenu("Clear out loaded profile")]
	public void Debug_ClearLoadedProfile()
	{
		LoadProfile(null);
	}
}
