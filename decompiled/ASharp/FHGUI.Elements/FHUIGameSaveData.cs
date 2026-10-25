using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FH.YML;
using FHRL;
using GLOOM;
using GLOOM.MainMenu;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class FHUIGameSaveData : MonoBehaviour
{
	public struct Params
	{
		public IGameSaveData Data;

		public string ActionLocKey;

		public bool HideAction;
	}

	[Header("Components")]
	[SerializeField]
	private Image LocationIcon;

	[SerializeField]
	private Image Illustration;

	[SerializeField]
	private TextMeshProUGUI partyName;

	[SerializeField]
	private UIMultiplayerSaveOwner saveOwner;

	[SerializeField]
	private TextMeshProUGUI date;

	[SerializeField]
	private TextMeshProUGUI LocationName;

	[SerializeField]
	private TextMeshProUGUI Week;

	[SerializeField]
	private TextMeshProUGUI Difficulty;

	[SerializeField]
	private UILoadGameCharacter[] characterSlots;

	[SerializeField]
	private Button actionButton;

	[SerializeField]
	private Button deleteButton;

	public Action<IGameSaveData> OnAction;

	public Action<IGameSaveData> OnDelete;

	private IGameSaveData saveData;

	private Texture2D _saveTexture;

	private void Awake()
	{
		actionButton.onClick.AddListener(HandleOnAction);
		deleteButton.onClick.AddListener(Delete);
	}

	public void SetData(Params @params)
	{
		gameObject.SetActive(value: true);
		saveData = @params.Data;
		if ((bool)LocationIcon)
		{
			LocationIcon.overrideSprite = GetLocationSprite(@params.Data);
		}
		partyName.text = @params.Data.GameName;
		saveOwner.SetPlayer(@params.Data.Owner);
		if (Week != null)
		{
			string translation = LocalizationManager.GetTranslation((new Calendar((uint)@params.Data.Week).Season == CalendarSeason.Summer) ? "GUI_SUMMER" : "GUI_Winter");
			Week.text = $"{@params.Data.Week}, {translation}";
		}
		if (Difficulty != null)
		{
			Difficulty.text = LocalizationManager.GetTranslation("GUI_DIFFICULTY_" + @params.Data.PartyAdventureData.Difficulty.ToString().ToUpper());
		}
		actionButton.gameObject.SetActive(!@params.HideAction);
		actionButton.SetTextKey(@params.ActionLocKey);
		SetCharacters(@params.Data.SelectedCharacters);
		if (@params.Data.ScenarioID.HasValue)
		{
			ScenarioDef value = YMLDefRepository.Repo.DefRepository.Scenarios.FirstOrDefault((KeyValuePair<ScenarioID, ScenarioDef> pair) =>
			{
				ScenarioID key = pair.Key;
				ScenarioID? scenarioID = @params.Data.ScenarioID;
				return key == scenarioID;
			}).Value;
			string translation2 = LocalizationManager.GetTranslation($"FS_{value.ID}_NAME");
			if (@params.Data.CurrentScenarioRound > 0)
			{
				LocationName.text = string.Format("{0}, {1} {2}", translation2, LocalizationManager.GetTranslation("GUI_COMBATLOG_START_ROUND"), @params.Data.CurrentScenarioRound);
			}
			else
			{
				LocationName.text = translation2;
			}
			string id = value.LevelID + "_Intro_1";
			UIInfoTools.Instance.GetNarrativeIllustration(id, throwError: false);
			Illustration.overrideSprite = UIInfoTools.Instance.GetNarrativeIllustration(id, throwError: false);
			LocationIcon.overrideSprite = UIInfoTools.Instance.GetQuestChainIcon(value.Chain);
		}
		else
		{
			LocationName.text = LocalizationManager.GetTranslation("GUI_Outpost");
			Illustration.overrideSprite = null;
			LocationIcon.overrideSprite = null;
		}
		Illustration.gameObject.SetActive(value: false);
		StartCoroutine(LoadScreenshot(@params.Data.ScreenShotFilePath, (Sprite sprite) =>
		{
			Illustration.overrideSprite = sprite;
			Illustration.gameObject.SetActive(value: true);
		}, () =>
		{
			Illustration.gameObject.SetActive(value: true);
		}));
	}

	private Sprite GetLocationSprite(IGameSaveData gameSaveData)
	{
		if (!gameSaveData.ScenarioID.HasValue)
		{
			return UIInfoTools.Instance.GetQuestChainIcon(ScenarioChainType.Other);
		}
		ScenarioDef value = YMLDefRepository.Repo.DefRepository.Scenarios.FirstOrDefault((KeyValuePair<ScenarioID, ScenarioDef> pair) =>
		{
			ScenarioID key = pair.Key;
			ScenarioID? scenarioID = gameSaveData.ScenarioID;
			return key == scenarioID;
		}).Value;
		return UIInfoTools.Instance.GetQuestChainIcon(value.Chain);
	}

	private void SetCharacters(List<IGameSaveCharacterData> characters)
	{
		for (int i = 0; i < characterSlots.Length; i++)
		{
			if (i < characters.Count && characters[i] != null)
			{
				characterSlots[i].ShowCharacter(characters[i].CharacterModel, characters[i].Level, characters[i].CharacterName, characters[i].Gold);
			}
			else
			{
				characterSlots[i].ShowEmpty();
			}
		}
	}

	private void HandleOnAction()
	{
		if (saveData != null)
		{
			OnAction?.Invoke(saveData);
		}
	}

	private void Delete()
	{
		if (saveData != null)
		{
			Singleton<UIConfirmationBoxManager>.Instance.ShowGenericConfirmation(LocalizationManager.GetTranslation("GUI_DELETE_PARTY_CONFIRMATION"), () =>
			{
				OnDelete?.Invoke(saveData);
			});
		}
	}

	private void OnDisable()
	{
		if (_saveTexture != null)
		{
			UnityEngine.Object.Destroy(_saveTexture);
		}
	}

	private IEnumerator LoadScreenshot(Task<string> url, Action<Sprite> callback, Action failCallback)
	{
		while (!url.IsCompleted)
		{
			yield return null;
		}
		using UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url.Result);
		yield return uwr.SendWebRequest();
		if (uwr.result != UnityWebRequest.Result.Success)
		{
			Debug.Log(uwr.error);
			failCallback?.Invoke();
			yield break;
		}
		_saveTexture = DownloadHandlerTexture.GetContent(uwr);
		_saveTexture.ignoreMipmapLimit = true;
		_saveTexture.filterMode = FilterMode.Point;
		_saveTexture.Apply();
		Sprite sprite = Sprite.Create(_saveTexture, new Rect(0f, 0f, _saveTexture.width, _saveTexture.height), new Vector2(0.5f, 0.5f), 100f, 1u, SpriteMeshType.Tight);
		sprite.name = "ScreenShot";
		callback?.Invoke(sprite);
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
	}
}
