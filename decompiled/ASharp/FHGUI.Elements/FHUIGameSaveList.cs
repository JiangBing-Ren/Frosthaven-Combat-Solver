using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GLOOM.MainMenu;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class FHUIGameSaveList : MonoBehaviour
{
	public struct CampaignData
	{
		public string Name;

		public List<IGameSaveData> Saves;

		public bool Collapsed;

		public bool IsCurrentActiveCampaign;

		public DateTime LastSavedTimeStamp
		{
			get
			{
				if (Saves == null || Saves.Count <= 0)
				{
					return default;
				}
				return Saves[0].LastSavedTimeStamp;
			}
		}
	}

	[Header("Components")]
	[SerializeField]
	private Transform container;

	[SerializeField]
	private FHUIGameSaveCampaign campaignPrototype;

	[SerializeField]
	private ScrollRect scrollRect;

	public Action<IGameSaveData> OnSelected;

	public Action<string> OnDeleteCampaign;

	private List<FHUIGameSaveCampaign> _availableCampaigns = new List<FHUIGameSaveCampaign>();

	private Dictionary<string, bool> _campaignsCollapseState = new Dictionary<string, bool>();

	private bool _initialized;

	private void InitCache()
	{
		if (!_initialized)
		{
			_initialized = true;
			_availableCampaigns.Clear();
			_availableCampaigns.AddRange(container.GetComponentsInChildren<FHUIGameSaveCampaign>());
		}
	}

	public void Init(List<IGameSaveData> saves)
	{
		InitCache();
		StartCoroutine(Populate(saves));
	}

	private IEnumerator Populate(List<IGameSaveData> saves)
	{
		container.gameObject.SetActive(value: false);
		string activeCampaignName;
		if (SaveData.Instance.Global.CurrentAdventureData != null)
		{
			Task<PartyAdventureData> activeCampaign = SaveData.Instance.Global.GetResumeCampaign();
			yield return new WaitForTask(activeCampaign);
			activeCampaignName = activeCampaign.Result?.PartyName;
		}
		else
		{
			activeCampaignName = null;
		}
		List<CampaignData> list = ParseCampaigns(saves, activeCampaignName);
		HelperTools.NormalizePool(ref _availableCampaigns, campaignPrototype.gameObject, container, list.Count);
		for (int i = 0; i < _availableCampaigns.Count; i++)
		{
			if (list.Count > i)
			{
				_availableCampaigns[i].OnSelected = (IGameSaveData d) =>
				{
					OnSelected?.Invoke(d);
				};
				_availableCampaigns[i].OnCollapse = (string s, bool b) =>
				{
					_campaignsCollapseState[s] = b;
				};
				_availableCampaigns[i].OnDeleteCampaign = (string c) =>
				{
					OnDeleteCampaign?.Invoke(c);
				};
				_availableCampaigns[i].Init(list[i]);
			}
			else
			{
				_availableCampaigns[i].gameObject.SetActive(value: false);
			}
		}
		container.gameObject.SetActive(value: true);
		scrollRect.verticalNormalizedPosition = 1f;
	}

	private List<CampaignData> ParseCampaigns(List<IGameSaveData> saves, string activeCampaignName)
	{
		List<CampaignData> list = new List<CampaignData>();
		foreach (IGrouping<string, IGameSaveData> item2 in from data in saves
			group data by data.PartyAdventureData.PartyName)
		{
			CampaignData item = default;
			item.Name = item2.Key;
			item.Saves = new List<IGameSaveData>(item2);
			item.Saves.Sort((IGameSaveData x, IGameSaveData y) => y.LastSavedTimeStamp.CompareTo(x.LastSavedTimeStamp));
			item.Collapsed = _campaignsCollapseState.ContainsKey(item.Name) && _campaignsCollapseState[item.Name];
			item.IsCurrentActiveCampaign = activeCampaignName == item2.Key;
			list.Add(item);
		}
		list.Sort((CampaignData x, CampaignData y) => y.LastSavedTimeStamp.CompareTo(x.LastSavedTimeStamp));
		return list;
	}

	public void Select(IGameSaveData saveData)
	{
		foreach (FHUIGameSaveCampaign availableCampaign in _availableCampaigns)
		{
			if (!availableCampaign.HasSave(saveData))
			{
				availableCampaign.ClearSelection();
			}
			else
			{
				availableCampaign.Select(saveData);
			}
		}
	}
}
