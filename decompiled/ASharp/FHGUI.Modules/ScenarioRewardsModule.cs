using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Elements;
using GLOOM;
using TMPro;
using UnityEngine;

namespace FHGUI.Modules;

public class ScenarioRewardsModule : FHMapViewModule
{
	public struct Params
	{
		public string ScenarioNameKey;

		public List<OutpostEventDTO.Effect> Effects;

		public List<UIMultiplayerUser.Data> ReadyUsers;

		public bool ReadyCheckPassed;
	}

	[Header("Components")]
	[SerializeField]
	private TMP_Text m_Description;

	[SerializeField]
	private GameObject m_EffectPrototype;

	[SerializeField]
	private RectTransform m_EffectsContainer;

	[SerializeField]
	private FHButton OkButton;

	[Header("Multiplayer")]
	[SerializeField]
	private UIUsersListContainer SyncUserList;

	private List<GameObject> m_Effects = new List<GameObject>(5);

	public Action OnDone;

	private bool m_Initialized;

	public List<GameObject> Effects => m_Effects;

	public Params Data { get; private set; }

	private void InitCache()
	{
		if (m_Initialized)
		{
			return;
		}
		m_Initialized = true;
		foreach (RectTransform item2 in m_EffectsContainer)
		{
			GameObject item = item2.gameObject;
			m_Effects.Add(item);
		}
		OkButton.onClick.RemoveAllListeners();
		OkButton.onClick.AddListener(() =>
		{
			OnDone?.Invoke();
		});
	}

	public void Show(Params @params)
	{
		InitCache();
		Data = @params;
		if (m_Description != null)
		{
			m_Description.text = FHUILocFormatting.PopulateParams("GUI_QUEST_REWARDS_DESC", LocalizationManager.GetTranslation(Data.ScenarioNameKey));
		}
		OkButton.interactable = !@params.ReadyCheckPassed;
		PopulateEffects();
		SyncUserList.Init(@params.ReadyUsers);
		SetActive(active: true);
	}

	private void PopulateEffects()
	{
		List<OutpostEventDTO.Effect> list = Data.Effects.Where((OutpostEventDTO.Effect effect2) => !effect2.Hidden).ToList();
		int count = list.Count;
		m_EffectsContainer.gameObject.SetActive(count > 0);
		UIHelperUtil.UpdateUIList(count, m_Effects, m_EffectPrototype, m_EffectsContainer);
		for (int num = 0; num < m_Effects.Count; num++)
		{
			OutpostEventDTO.Effect effect = ((num < list.Count) ? list[num] : null);
			GameObject gameObject = m_Effects[num];
			gameObject.gameObject.SetActive(effect != null);
			if (effect != null)
			{
				TextMeshProUGUI componentInChildren = gameObject.GetComponentInChildren<TextMeshProUGUI>();
				if ((object)componentInChildren != null)
				{
					componentInChildren.text = effect.Text;
				}
			}
		}
	}

	public void Hide()
	{
		SetActive(active: false);
	}

	private void SetActive(bool active)
	{
		gameObject.SetActive(active);
	}
}
