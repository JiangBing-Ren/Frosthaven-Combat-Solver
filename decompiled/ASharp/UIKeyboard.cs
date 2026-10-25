using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Script.GUI.Configuration;
using Assets.Script.GUI.Controller.Keyboard;
using I2.Loc;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class UIKeyboard : LocalizedListener
{
	[Serializable]
	public class KeyCodeEvent : UnityEvent<KeyCode>
	{
	}

	[SerializeField]
	private List<UIKeyboardKey> keysPool;

	[SerializeField]
	private FlowLayoutGroup container;

	[SerializeField]
	private List<UIKeyboardKey> fixedKeys;

	[SerializeField]
	private KeyboardConfig defaultConfig;

	[SerializeField]
	private List<KeyboardConfig> languageConfigs;

	public KeyCodeEvent OnSelectedKeyCode;

	private KeyboardConfig loadedConfig;

	private void Start()
	{
		foreach (UIKeyboardKey fixedKey in fixedKeys)
		{
			fixedKey.OnClicked.AddListener(OnClickedKey);
		}
		foreach (UIKeyboardKey item in keysPool)
		{
			item.OnClicked.AddListener(OnClickedKey);
		}
		OnLanguageChanged();
	}

	protected override void OnLanguageChanged()
	{
		Setup(languageConfigs.FirstOrDefault((KeyboardConfig it) => it.Language == LocalizationManager.CurrentLanguage.ToLower()) ?? defaultConfig);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		SelectFirstElement();
	}

	private void SelectFirstElement()
	{
		StopAllCoroutines();
		if (keysPool[0].isActiveAndEnabled)
		{
			EventSystem.current.SetSelectedGameObject(keysPool[0].gameObject);
		}
		else
		{
			StartCoroutine(WaitToSelect(keysPool[0]));
		}
	}

	private IEnumerator WaitToSelect(UIKeyboardKey slot)
	{
		yield return new WaitUntil(() => slot.isActiveAndEnabled);
		EventSystem.current.SetSelectedGameObject(slot.gameObject);
	}

	private void Setup(KeyboardConfig config)
	{
		if (loadedConfig == config)
		{
			return;
		}
		loadedConfig = config;
		container.columnsCount = config.Rows.Select((KeyboardRow it) => it.Keys.Count).ToList();
		HelperTools.NormalizePool(ref keysPool, keysPool[0].gameObject, container.transform, config.Rows.Sum((KeyboardRow it) => it.Keys.Count), (UIKeyboardKey slot) =>
		{
			slot.OnClicked.AddListener(OnClickedKey);
		});
		int num = 0;
		for (int num2 = 0; num2 < config.Rows.Count; num2++)
		{
			for (int num3 = 0; num3 < config.Rows[num2].Keys.Count; num3++)
			{
				KeyCode keyCode = config.Rows[num2].Keys[num3];
				keysPool[num].name = keyCode.ToString();
				keysPool[num].SetKeyCode(keyCode);
				num++;
			}
		}
		for (int num4 = 0; num4 < fixedKeys.Count; num4++)
		{
			fixedKeys[num4].transform.SetAsLastSibling();
		}
		SelectFirstElement();
	}

	public void Show()
	{
		gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		EventSystem.current.SetSelectedGameObject(null);
		gameObject.SetActive(value: false);
	}

	protected override void OnDisable()
	{
		EventSystem.current.SetSelectedGameObject(null);
		base.OnDisable();
	}

	private void OnClickedKey(KeyCode keyCode)
	{
		OnSelectedKeyCode.Invoke(keyCode);
	}
}
