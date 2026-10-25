using System;
using System.Collections.Generic;
using System.Linq;
using FH.YML;
using FHRL;
using GLOOM.MainMenu;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class FHUIGameSaveDataRow : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private TextMeshProUGUI Name;

	[SerializeField]
	private TextMeshProUGUI Date;

	[SerializeField]
	private Image Icon;

	[SerializeField]
	private FHButton Button;

	[SerializeField]
	private GameObject Selected;

	[Header("Settings")]
	[SerializeField]
	private string formatDate = "dd/MM/y, HH:mm:ss";

	[SerializeField]
	private Color textColorNormal;

	[SerializeField]
	private Color textColorSelected;

	public Action<IGameSaveData> OnSelect;

	private bool _selected;

	public IGameSaveData SaveData { get; private set; }

	private void Awake()
	{
		Button.onClick.AddListener(() =>
		{
			OnSelect?.Invoke(SaveData);
		});
	}

	public void Init(IGameSaveData data)
	{
		SaveData = data;
		Name.text = data.SaveName;
		if ((bool)Date)
		{
			Date.text = data.LastSavedTimeStamp.ToString(formatDate);
		}
		Icon.overrideSprite = GetLocationSprite(data);
		_selected = false;
		UpdateHighlight();
	}

	public void Hide()
	{
		SaveData = null;
		gameObject.SetActive(value: false);
	}

	public void SetSelect(bool selected)
	{
		Button.Select();
		_selected = selected;
		UpdateHighlight();
	}

	private void UpdateHighlight()
	{
		Selected.SetActive(_selected);
		Name.color = (_selected ? textColorSelected : textColorNormal);
		Date.color = (_selected ? textColorSelected : textColorNormal);
	}

	private Sprite GetLocationSprite(IGameSaveData gameSaveData)
	{
		if (!gameSaveData.ScenarioID.HasValue)
		{
			return UIInfoTools.Instance.GetQuestChainIcon(ScenarioChainType.None);
		}
		ScenarioDef value = YMLDefRepository.Repo.DefRepository.Scenarios.FirstOrDefault((KeyValuePair<ScenarioID, ScenarioDef> pair) =>
		{
			ScenarioID key = pair.Key;
			ScenarioID? scenarioID = gameSaveData.ScenarioID;
			return key == scenarioID;
		}).Value;
		return UIInfoTools.Instance.GetQuestChainIcon(value.Chain);
	}
}
