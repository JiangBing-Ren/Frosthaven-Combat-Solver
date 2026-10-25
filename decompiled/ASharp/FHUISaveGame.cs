using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FHGUI;
using FHGUI.Elements;
using GLOOM;
using GLOOM.MainMenu;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class FHUISaveGame : Singleton<FHUISaveGame>
{
	[SerializeField]
	private UIWindow window;

	[Header("Components")]
	[SerializeField]
	private FHUISaveGameCreateNew saveNameDialog;

	[SerializeField]
	private FHUIGameSaveList saveList;

	[SerializeField]
	private FHUIGameSaveData saveInfo;

	[SerializeField]
	private FHButton newSave;

	[SerializeField]
	private FHButton close;

	private List<IGameSaveData> _saves;

	private bool _invalidate;

	private IGameModeService service;

	private string ActiveCampaignName => SaveData.Instance?.Global?.CurrentAdventureData?.PartyName ?? string.Empty;

	public UnityEvent OnHide => window.onHidden;

	protected override void AwakeInit()
	{
		window.onShown.AddListener(OnShown);
		window.onHidden.AddListener(OnHidden);
		newSave.onClick.AddListener(OpenNewSaveDialog);
		saveInfo.OnAction = OverrideSave;
		saveInfo.OnDelete = DeleteSave;
		saveList.OnDeleteCampaign = DeleteCampaign;
		saveList.OnSelected = SelectSave;
		close.onClick.AddListener(Hide);
		FHUISaveGameCreateNew fHUISaveGameCreateNew = saveNameDialog;
		fHUISaveGameCreateNew.OnConfirm = (Action<string>)Delegate.Combine(fHUISaveGameCreateNew.OnConfirm, new Action<string>(Save));
		saveNameDialog.Hide();
	}

	private void OpenNewSaveDialog()
	{
		saveNameDialog.Init();
	}

	public void Show(IGameModeService service)
	{
		this.service = service;
		window.Show();
		Rebuild();
	}

	private async void Rebuild()
	{
		PopulateSavedGames(await service.GetSaves());
	}

	private void PopulateSavedGames(List<IGameSaveData> saves)
	{
		_saves = saves;
		if (saveList != null)
		{
			saveList.Init(saves);
		}
		SelectSave(null);
	}

	public void Show()
	{
		window.Show();
	}

	public void Hide()
	{
		_saves = null;
		window.Hide();
		saveNameDialog.Hide();
	}

	private void OnShown()
	{
	}

	private void OnHidden()
	{
	}

	private void SelectSave(IGameSaveData saveData)
	{
		if (saveData != null)
		{
			saveInfo.SetData(new FHUIGameSaveData.Params
			{
				Data = saveData,
				ActionLocKey = "GUI_OVERWRITE",
				HideAction = (saveData.PartyAdventureData.SaveFileType != PartyAdventureData.SaveType.Manual || saveData.CampaignName != ActiveCampaignName)
			});
		}
		else
		{
			saveInfo.Hide();
		}
		saveList.Select(saveData);
	}

	private void OverrideSave(IGameSaveData data)
	{
		if (data != null)
		{
			string translation = LocalizationManager.GetTranslation("GUI_SAVEGAME_OVERWRITE_TITLE");
			string explanation = FHUILocFormatting.PopulateParams("GUI_SAVEGAME_OVERWRITE_DESC", data.GameName);
			Singleton<UIConfirmationBoxManager>.Instance.ShowGenericConfirmation(translation, explanation, () =>
			{
				Hide();
				SaveData.Instance.ManualSaveCurrentAdventureData(data.PartyAdventureData.SaveFileName);
			});
		}
	}

	private void Save(string saveName)
	{
		if (!saveName.IsNullOrEmpty())
		{
			if (CheckExisting(saveName, out var saveData))
			{
				OverrideSave(saveData);
				return;
			}
			Hide();
			SaveData.Instance.ManualSaveCurrentAdventureData(saveName);
		}
	}

	private bool CheckExisting(string saveName, out IGameSaveData saveData)
	{
		if (saveName.IsNullOrEmpty())
		{
			saveData = null;
			return false;
		}
		saveData = _saves?.Where((IGameSaveData data) => data.CampaignName == ActiveCampaignName).FirstOrDefault((IGameSaveData data) => data.SaveName == saveName);
		return saveData != null;
	}

	private void DeleteSave(IGameSaveData data)
	{
		if (data != null)
		{
			service.DeleteGame(data);
			_invalidate = true;
		}
	}

	private void DeleteCampaign(string campaignName)
	{
		service.DeleteCampaign(campaignName).ContinueWith((Task task1) =>
		{
			_invalidate = true;
		});
	}

	private void Update()
	{
		if (_invalidate)
		{
			_invalidate = false;
			Rebuild();
		}
	}
}
