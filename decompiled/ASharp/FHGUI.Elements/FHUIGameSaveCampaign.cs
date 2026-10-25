using System;
using System.Collections.Generic;
using GLOOM.MainMenu;
using UnityEngine;

namespace FHGUI.Elements;

public class FHUIGameSaveCampaign : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private Transform container;

	[SerializeField]
	private FHUIGameSaveDataRow saveRowPrototype;

	[SerializeField]
	private FHUIGameSaveCampaignRow campaignHeader;

	[Header("Settings")]
	[SerializeField]
	private string formatDate = "dd/MM/y, HH:mm:ss";

	public Action<IGameSaveData> OnSelected;

	public Action<string> OnDeleteCampaign;

	public Action<string, bool> OnCollapse;

	private List<FHUIGameSaveDataRow> _availableSaveRows = new List<FHUIGameSaveDataRow>();

	private FHUIGameSaveList.CampaignData Data;

	private bool _initialized;

	private void InitCache()
	{
		if (!_initialized)
		{
			_initialized = true;
			_availableSaveRows.Clear();
			_availableSaveRows.AddRange(container.GetComponentsInChildren<FHUIGameSaveDataRow>());
			campaignHeader.OnCollapse = HandleOnCollapse;
			campaignHeader.OnDeleteCampaign = OnDeleteCampaign;
		}
	}

	public void Init(FHUIGameSaveList.CampaignData data)
	{
		InitCache();
		Data = data;
		gameObject.SetActive(value: true);
		int newLength = data.Saves?.Count ?? 0;
		campaignHeader.Init(data.Saves[0], data.Collapsed, data.IsCurrentActiveCampaign);
		container.gameObject.SetActive(!data.Collapsed);
		HelperTools.NormalizePool(ref _availableSaveRows, saveRowPrototype.gameObject, container, newLength);
		for (int i = 0; i < _availableSaveRows.Count; i++)
		{
			if (data.Saves.Count > i)
			{
				_availableSaveRows[i].OnSelect = (IGameSaveData s) =>
				{
					OnSelected?.Invoke(s);
				};
				_availableSaveRows[i].Init(data.Saves[i]);
			}
			else
			{
				_availableSaveRows[i].Hide();
			}
		}
	}

	public bool HasSave(IGameSaveData saveData)
	{
		if (saveData == null)
		{
			return false;
		}
		if (Data.Saves == null)
		{
			return false;
		}
		return Data.Saves.Contains(saveData);
	}

	public void ClearSelection()
	{
		foreach (FHUIGameSaveDataRow availableSaveRow in _availableSaveRows)
		{
			availableSaveRow.SetSelect(selected: false);
		}
	}

	public void Select(IGameSaveData saveData)
	{
		foreach (FHUIGameSaveDataRow availableSaveRow in _availableSaveRows)
		{
			availableSaveRow.SetSelect(availableSaveRow.SaveData == saveData);
		}
	}

	private void HandleOnCollapse(bool collapse)
	{
		container.gameObject.SetActive(!collapse);
		OnCollapse?.Invoke(Data.Name, collapse);
	}
}
