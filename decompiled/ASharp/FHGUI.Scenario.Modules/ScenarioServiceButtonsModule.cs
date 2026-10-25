using System;
using FFSNet;
using FHGUI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Scenario.Modules;

public class ScenarioServiceButtonsModule : MonoBehaviour
{
	[SerializeField]
	private Button _buttonSettings;

	[SerializeField]
	private Button _buttonCodex;

	[SerializeField]
	private Button _buttonViewParty;

	[SerializeField]
	private Button _buttonSpeedUp;

	public float[] SpeedUpValues = new float[4] { 1f, 3f, 5f, 8f };

	public Sprite[] SpeedUpSprites;

	public GameObject GameObject => gameObject;

	private void Awake()
	{
		_buttonSettings.onClick.AddListener(() =>
		{
			Singleton<ESCMenu>.Instance?.Show();
		});
		_buttonCodex.onClick.AddListener(() =>
		{
			Singleton<FHUICodex>.Instance?.Show();
		});
		_buttonViewParty.onClick.AddListener(() =>
		{
			Singleton<CardsHandManager>.Instance?.ShowAllDecks(openByKey: false);
		});
		_buttonSpeedUp.onClick.AddListener(OnSpeedUpClicked);
		SaveData.Instance.GlobalDataChanged += OnGlobalDataChanged;
		SaveData.Instance.Global.GameSpeedChanged += UpdateSpeedUpButton;
		UpdateSpeedUpButton();
		_buttonSpeedUp.gameObject.SetActive(SaveData.Instance.Global.ShowSpeedUpButton);
		if (_buttonCodex != null)
		{
			UIContextualTutorialModule.OnEvent("Codex", _buttonCodex);
		}
	}

	private void OnDestroy()
	{
		if (SaveData.Instance != null)
		{
			SaveData.Instance.GlobalDataChanged -= OnGlobalDataChanged;
			if (SaveData.Instance.Global != null)
			{
				SaveData.Instance.Global.GameSpeedChanged -= UpdateSpeedUpButton;
			}
		}
	}

	private void OnSpeedUpClicked()
	{
		int num = Array.IndexOf(SpeedUpValues, SaveData.Instance.Global.RegularSpeedMultiplier);
		num = (num + 1) % SpeedUpValues.Length;
		SaveData.Instance.Global.RegularSpeedMultiplier = SpeedUpValues[num];
		if (FFSNetwork.IsOnline && FFSNetwork.IsHost)
		{
			Synchronizer.SendSideAction(GameActionType.SetRegularSpeedMultiplier, null, canBeUnreliable: false, sendToHostOnly: false, 0, (int)(SaveData.Instance.Global.RegularSpeedMultiplier * 100f));
		}
		UpdateSpeedUpButton();
	}

	private void UpdateSpeedUpButton()
	{
		float element = ((!FFSNetwork.IsOnline || FFSNetwork.IsHost) ? SaveData.Instance.Global.RegularSpeedMultiplier : SaveData.Instance.Global.MultiplayerRegularSpeedMultiplier);
		int num = Math.Max(SpeedUpValues.IndexOf(element), 0);
		_buttonSpeedUp.image.sprite = SpeedUpSprites[num];
	}

	private void OnGlobalDataChanged()
	{
		bool flag = _buttonSpeedUp.gameObject.activeSelf != SaveData.Instance.Global.ShowSpeedUpButton;
		_buttonSpeedUp.gameObject.SetActive(SaveData.Instance.Global.ShowSpeedUpButton);
		if (flag && !SaveData.Instance.Global.ShowSpeedUpButton)
		{
			SaveData.Instance.Global.RegularSpeedMultiplier = 1f;
			if (FFSNetwork.IsOnline && FFSNetwork.IsHost)
			{
				Synchronizer.SendSideAction(GameActionType.SetRegularSpeedMultiplier, null, canBeUnreliable: false, sendToHostOnly: false, 0, (int)(SaveData.Instance.Global.RegularSpeedMultiplier * 100f));
			}
		}
	}

	private void Update()
	{
		_buttonViewParty.interactable = Singleton<CardsHandManager>.Instance.IsFullCardPreviewAllowed;
		_buttonSpeedUp.interactable = !FFSNetwork.IsOnline || FFSNetwork.IsHost;
	}
}
