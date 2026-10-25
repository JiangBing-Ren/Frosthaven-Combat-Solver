using System;
using System.Collections.Generic;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GLOOM.MainMenu;

[RequireComponent(typeof(UIWindow))]
public class UILoadGameWindow : Singleton<UILoadGameWindow>
{
	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private ScrollRect scroll;

	[SerializeField]
	private UILoadGameSlot slotPrefab;

	[SerializeField]
	private GUIAnimator showAnimator;

	[SerializeField]
	private UIEnableDLCAnimator enableDLCAnimator;

	[SerializeField]
	private CreateCustomSaveSlot createCustomPrefab;

	private List<UILoadGameSlot> pool = new List<UILoadGameSlot>();

	private Dictionary<IGameSaveData, UILoadGameSlot> slots = new Dictionary<IGameSaveData, UILoadGameSlot>();

	private ILoadGameService service;

	private Action onClosed;

	protected override void AwakeInit()
	{
		window.onTransitionComplete.AddListener((UIWindow _, UIWindow.VisualState state) =>
		{
			if (state == UIWindow.VisualState.Hidden)
			{
				onClosed?.Invoke();
			}
		});
		window.onHidden.AddListener(StopAnimations);
	}

	public async void Show(ILoadGameService service, Action onClosed)
	{
		this.onClosed = onClosed;
		this.service = service;
		showAnimator.Stop();
		LoadSavedGames(await service.GetSaves());
		scroll.ScrollToTop();
		window.Show();
		showAnimator.Play();
	}

	private void LoadSavedGames(List<IGameSaveData> saves)
	{
		slots.Clear();
		HelperTools.NormalizePool(ref pool, slotPrefab.gameObject, scroll.content, saves.Count, (UILoadGameSlot slot) =>
		{
			slot.OnLoadData.AddListener(LoadParty);
			slot.OnDeleteData.AddListener(AskDeleteParty);
			slot.OnEnableDLC.AddListener(EnableDLC);
			slot.OnHovered += () =>
			{
				if (Singleton<InputFHManager>.Instance.UsingGamepad && EventSystem.current.currentSelectedGameObject == slot.gameObject)
				{
					scroll.ScrollVerticallyToTop(slot.transform as RectTransform);
				}
			};
		});
		for (int num = 0; num < saves.Count; num++)
		{
			IGameSaveData gameSaveData = saves[num];
			slots[gameSaveData] = pool[num];
			pool[num].SetData(gameSaveData);
		}
		CreateCustomSaveSlot componentInChildren = scroll.content.GetComponentInChildren<CreateCustomSaveSlot>(includeInactive: true);
		if ((bool)componentInChildren)
		{
			UnityEngine.Object.Destroy(componentInChildren.gameObject);
		}
	}

	private async void CreateSave(CustomPartySetup.Configuration config)
	{
		await service.CreateCustomGame(config);
		LoadSavedGames(await service.GetSaves());
	}

	public void Hide()
	{
		Singleton<UIConfirmationBoxManager>.Instance.Hide();
		enableDLCAnimator.Stop();
		window.Hide();
	}

	private void AskDeleteParty(IGameSaveData data)
	{
		Singleton<UIConfirmationBoxManager>.Instance.ShowGenericConfirmation(LocalizationManager.GetTranslation("GUI_DELETE_PARTY_CONFIRMATION"), () =>
		{
			DeleteParty(data);
		});
	}

	private void DeleteParty(IGameSaveData data)
	{
		StopAllCoroutines();
		service.DeleteGame(data);
		UILoadGameSlot uILoadGameSlot = slots[data];
		slots.Remove(data);
		uILoadGameSlot.gameObject.SetActive(value: false);
	}

	private void EnableDLC(IGameSaveData data, DLCRegistry.EDLCKey dlcToAdd)
	{
		throw new NotImplementedException();
	}

	private void LoadParty(IGameSaveData data)
	{
		service.LoadGame(data);
	}

	private void OnDisable()
	{
		if (!GloomUtility.IsAppQuitting)
		{
			StopAnimations();
		}
	}

	private void StopAnimations()
	{
		StopAllCoroutines();
		showAnimator.Stop();
	}
}
