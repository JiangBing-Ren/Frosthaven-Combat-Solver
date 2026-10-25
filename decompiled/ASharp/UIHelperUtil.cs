using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DevLocker.GFrame.Input.UIInputDisplay;
using DevLocker.GFrame.Input.UIScope;
using FHGUI.Tweening;
using GLOOM;
using I2.Loc;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

public static class UIHelperUtil
{
	public static void UpdateCharacterSpecificUI(CPlayerActor playerActor)
	{
		if (Singleton<ActorStatPanel>.Instance != null && Singleton<ActorStatPanel>.Instance.ActorShown == playerActor)
		{
			Singleton<ActorStatPanel>.Instance.GetComponentInChildren<UICharacterSpecificElementsContainer>()?.Init(playerActor);
		}
	}

	public static void EnsureEnoughChildComponents<T>(int count, List<T> targetList, GameObject prototype, Transform parent) where T : MonoBehaviour
	{
		while (targetList.Count < count)
		{
			T component = UnityEngine.Object.Instantiate(prototype, parent).GetComponent<T>();
			targetList.Add(component);
		}
	}

	public static void UpdateUIList<T>(int count, List<T> targetList, T prototype, Transform parent, Action<int, T> onShow = null, Action<int, T> onHide = null, Action<int, T> onCreate = null) where T : MonoBehaviour
	{
		int count2 = targetList.Count;
		int num = Mathf.Max(count, count2);
		for (int i = 0; i < num; i++)
		{
			if (i < count)
			{
				T val = null;
				if (i < count2)
				{
					val = targetList[i];
					val.gameObject.SetActive(value: true);
				}
				else
				{
					val = UnityEngine.Object.Instantiate(prototype, parent);
					onCreate?.Invoke(i, val);
					targetList.Add(val);
				}
				onShow?.Invoke(i, val);
			}
			else
			{
				T val2 = targetList[i];
				val2.gameObject.SetActive(value: false);
				onHide?.Invoke(i, val2);
			}
		}
	}

	public static void UpdateUIList(int count, List<GameObject> targetList, GameObject prototype, Transform parent, Action<int, GameObject> onShow = null, Action<int, GameObject> onHide = null)
	{
		int count2 = targetList.Count;
		int num = Mathf.Max(count, count2);
		for (int i = 0; i < num; i++)
		{
			if (i < count)
			{
				GameObject gameObject = null;
				if (i < count2)
				{
					gameObject = targetList[i];
					gameObject.gameObject.SetActive(value: true);
				}
				else
				{
					gameObject = UnityEngine.Object.Instantiate(prototype, parent);
					targetList.Add(gameObject);
				}
				onShow?.Invoke(i, gameObject);
			}
			else
			{
				GameObject gameObject2 = targetList[i];
				gameObject2.gameObject.SetActive(value: false);
				onHide?.Invoke(i, gameObject2);
			}
		}
	}

	public static void SetTextKeyFormatted(MonoBehaviour behaviour, string key, params string[] parameters)
	{
		ControllerMouseButtonScopeElement componentInChildren = behaviour.GetComponentInChildren<ControllerMouseButtonScopeElement>(includeInactive: true);
		if ((bool)componentInChildren)
		{
			foreach (GameObject item in componentInChildren.MouseButtonParts.Concat(componentInChildren.ControllerHotkeyParts))
			{
				TextMeshProUGUI[] componentsInChildren = item.GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true);
				foreach (TextMeshProUGUI textMeshProUGUI in componentsInChildren)
				{
					if (!textMeshProUGUI.GetComponent<HotkeyDisplayUI>())
					{
						SetTextKeyFormatted(textMeshProUGUI, key, parameters);
					}
				}
			}
			return;
		}
		TextLocalizedListener componentInChildren2 = behaviour.GetComponentInChildren<TextLocalizedListener>(includeInactive: true);
		if ((bool)componentInChildren2)
		{
			componentInChildren2.SetTextKey(key, parameters.Select((string s, int num) => new LocalizationParam($"{{{num}}}", s)).ToArray());
			return;
		}
		Localize componentInChildren3 = behaviour.GetComponentInChildren<Localize>(includeInactive: true);
		if ((bool)componentInChildren3)
		{
			componentInChildren3.SetTerm(key);
			if (parameters.Length != 0)
			{
				Log.GUIError(behaviour.name + " localizing via I2 doesn't support numbered parameters.", behaviour);
			}
			return;
		}
		TextMeshProUGUI componentInChildren4 = behaviour.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
		if ((bool)componentInChildren4)
		{
			string text = GLOOM.LocalizationManager.GetTranslation(key, fixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, behaviour.gameObject).Replace("\\n", "\n");
			string text2;
			if (parameters.Length == 0)
			{
				text2 = text;
			}
			else
			{
				text2 = string.Format(text, parameters);
			}
			componentInChildren4.text = text2;
			return;
		}
		throw new InvalidOperationException(behaviour.name + " doesn't have text to be localized");
	}

	public static void SetTextKey(MonoBehaviour behaviour, string key, Dictionary<string, string> parameters)
	{
		SetTextKey(behaviour, key, parameters.Select((KeyValuePair<string, string> p) => new LocalizationParam(p)).ToArray());
	}

	public static void SetTextKey(MonoBehaviour behaviour, string key, params LocalizationParam[] parameters)
	{
		ControllerMouseButtonScopeElement componentInChildren = behaviour.GetComponentInChildren<ControllerMouseButtonScopeElement>(includeInactive: true);
		if ((bool)componentInChildren)
		{
			foreach (GameObject item in componentInChildren.MouseButtonParts.Concat(componentInChildren.ControllerHotkeyParts))
			{
				TextMeshProUGUI[] componentsInChildren = item.GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true);
				foreach (TextMeshProUGUI textMeshProUGUI in componentsInChildren)
				{
					if (!textMeshProUGUI.GetComponent<HotkeyDisplayUI>())
					{
						SetTextKey(textMeshProUGUI, key, parameters);
					}
				}
			}
			return;
		}
		TextLocalizedListener componentInChildren2 = behaviour.GetComponentInChildren<TextLocalizedListener>(includeInactive: true);
		if ((bool)componentInChildren2)
		{
			componentInChildren2.SetTextKey(key, parameters);
			return;
		}
		LocalizationParamsManager componentInChildren3 = behaviour.GetComponentInChildren<LocalizationParamsManager>(includeInactive: true);
		if ((bool)componentInChildren3)
		{
			LocalizationParam[] array = parameters;
			for (int i = 0; i < array.Length; i++)
			{
				LocalizationParam localizationParam = array[i];
				componentInChildren3.SetParameterValue(localizationParam.Name, localizationParam.Value, localize: false);
			}
			Localize componentInChildren4 = behaviour.GetComponentInChildren<Localize>(includeInactive: true);
			if ((bool)componentInChildren4)
			{
				componentInChildren4.SetTerm(key);
				return;
			}
		}
		TextMeshProUGUI componentInChildren5 = behaviour.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
		if ((bool)componentInChildren5)
		{
			string text = GLOOM.LocalizationManager.GetTranslation(key, fixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, behaviour.gameObject);
			LocalizationParam[] array = parameters;
			foreach (LocalizationParam localizationParam2 in array)
			{
				text = localizationParam2.Apply(text);
			}
			componentInChildren5.text = text.Replace("\\n", "\n");
			return;
		}
		throw new InvalidOperationException(behaviour.name + " doesn't have text to be localized");
	}

	public static void SetNonLocalizedText(MonoBehaviour behaviour, string nonlocalizedText)
	{
		TextMeshProUGUI[] componentsInChildren = behaviour.GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true);
		foreach (TextMeshProUGUI textMeshProUGUI in componentsInChildren)
		{
			if (!textMeshProUGUI.GetComponent<HotkeyDisplayUI>())
			{
				textMeshProUGUI.text = nonlocalizedText;
			}
		}
	}

	public static void SetHoverReparenting(GameObject gameObject, RectTransform reparentTargetTransform, RectTransform followTransform = null, RectTransform overrideTransform = null)
	{
		bool flag = gameObject.TryGetComponent<UIHoverReparentEffects>(out var component);
		if (!flag)
		{
			component = gameObject.AddComponent<UIHoverReparentEffects>();
		}
		component.ReparentTargetTransform = reparentTargetTransform;
		component.FollowTransform = followTransform;
		component.OverrideTransform = overrideTransform;
		if (!flag && gameObject.activeInHierarchy)
		{
			component.enabled = false;
			component.enabled = true;
		}
	}

	public static bool IsEmoji(char c)
	{
		if (char.GetUnicodeCategory(c) == UnicodeCategory.Surrogate)
		{
			return true;
		}
		int num = char.ConvertToUtf32(c.ToString(), 0);
		if ((num < 128512 || num > 128591) && (num < 127744 || num > 128511) && (num < 128640 || num > 128767) && (num < 9728 || num > 9983))
		{
			if (num >= 9984)
			{
				return num <= 10175;
			}
			return false;
		}
		return true;
	}

	public static void SetSpriteAndTweenIt(Image image, Sprite newSprite, bool newEnabledState)
	{
		bool num = image.sprite != newSprite || image.enabled != newEnabledState;
		image.enabled = newEnabledState;
		image.sprite = newSprite;
		if (num & newEnabledState)
		{
			UITweenEffects.PlayAllOnTransform(image.rectTransform);
		}
	}

	public static bool ClearPositionConstraint(GameObject gameObjectToBeConstrained)
	{
		return TrySetPositionConstraint(gameObjectToBeConstrained, null);
	}

	public static bool TrySetPositionConstraint(GameObject gameObjectToBeConstrained, Transform constraintSource, float constraintWeight = 1f, bool createConstraintIfMissing = false)
	{
		if (!gameObjectToBeConstrained.TryGetComponent<PositionConstraint>(out var component))
		{
			if (!createConstraintIfMissing)
			{
				return false;
			}
			component = gameObjectToBeConstrained.AddComponent<PositionConstraint>();
		}
		component.constraintActive = false;
		while (component.sourceCount > 0)
		{
			component.RemoveSource(0);
		}
		if ((object)constraintSource == null)
		{
			return true;
		}
		component.AddSource(new ConstraintSource
		{
			sourceTransform = constraintSource,
			weight = constraintWeight
		});
		component.weight = 1f;
		component.constraintActive = true;
		return true;
	}
}
