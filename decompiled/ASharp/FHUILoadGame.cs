using System.Collections.Generic;
using System.Threading.Tasks;
using FHGUI.Elements;
using GLOOM.MainMenu;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class FHUILoadGame : Singleton<FHUILoadGame>
{
	[Header("Components")]
	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private FHUIGameSaveList saveList;

	[SerializeField]
	private FHUIGameSaveData saveInfo;

	[SerializeField]
	private FHButton close;

	private ILoadGameService service;

	private bool _invalidate;

	public UnityEvent OnHide => window.onHidden;

	protected override void AwakeInit()
	{
		saveInfo.OnAction = LoadSave;
		saveInfo.OnDelete = DeleteSave;
		saveList.OnSelected = SelectSave;
		saveList.OnDeleteCampaign = DeleteCampaign;
		close.onClick.AddListener(Hide);
	}

	public void Show(ILoadGameService service)
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
		if (saveList != null)
		{
			saveList.Init(saves);
		}
		if ((saves?.Count ?? 0) > 0)
		{
			SelectSave(saves[0]);
		}
		else
		{
			SelectSave(null);
		}
	}

	private void SelectSave(IGameSaveData saveData)
	{
		if (saveData != null)
		{
			saveInfo.SetData(new FHUIGameSaveData.Params
			{
				Data = saveData,
				ActionLocKey = "GUI_LOAD"
			});
		}
		else
		{
			saveInfo.Hide();
		}
		saveList.Select(saveData);
	}

	private void LoadSave(IGameSaveData data)
	{
		if (data != null)
		{
			Hide();
			service.LoadGame(data);
		}
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

	public void Hide()
	{
		window.Hide();
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
