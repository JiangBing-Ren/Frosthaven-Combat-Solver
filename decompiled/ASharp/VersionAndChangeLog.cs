using System.Text.RegularExpressions;
using I2.Loc;
using Libs.Build;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VersionAndChangeLog : MonoBehaviour
{
	public TextMeshProUGUI VersionText;

	public GameObject ChangeLogPanel;

	public TextMeshProUGUI ChangeLogText;

	public Button OpenButton;

	public Button CloseButton;

	public TMP_InputField SearchField;

	public Button PrevButton;

	public Button NextButton;

	public TextMeshProUGUI NumResultsText;

	public ScrollRect ChangeLogScrollRect;

	private string _changeLog;

	private string _searchString = string.Empty;

	private MatchCollection _searchResults;

	private int _currentResultIndex;

	private void Awake()
	{
		VersionText.text = string.Format(Bootstrap.GetVersionLoc(), RuntimeBuildInfo.UserVersion);
		_changeLog = RuntimeBuildInfo.BuildChangeLog;
		ChangeLogText.text = _changeLog;
		ChangeLogPanel.SetActive(value: false);
		CloseButton.onClick.AddListener(() =>
		{
			ChangeLogPanel.SetActive(value: false);
		});
		OpenButton.enabled = false;
		PrevButton.onClick.AddListener(OnPrevButtonClicked);
		NextButton.onClick.AddListener(OnNextButtonClicked);
		UpdateSearch();
		LocalizationManager.OnLocalizeEvent += HandleOnLanguageChanged;
	}

	private void OnDestroy()
	{
		LocalizationManager.OnLocalizeEvent -= HandleOnLanguageChanged;
	}

	public void OpenChangelog()
	{
	}

	private void Update()
	{
		if (ChangeLogPanel.activeSelf && !(SearchField.text == _searchString))
		{
			UpdateSearch();
		}
	}

	private void UpdateSearch()
	{
		_searchString = SearchField.text;
		bool flag = _searchString.Length != 0;
		PrevButton.gameObject.SetActive(flag);
		NextButton.gameObject.SetActive(flag);
		NumResultsText.gameObject.SetActive(flag);
		if (flag)
		{
			Regex regex = new Regex(_searchString, RegexOptions.IgnoreCase);
			_searchResults = regex.Matches(_changeLog);
			_currentResultIndex = 0;
			PrevButton.interactable = _searchResults.Count > 0;
			NextButton.interactable = PrevButton.interactable;
		}
		else
		{
			_searchResults = null;
		}
		UpdateSearchVisuals();
	}

	private void OnPrevButtonClicked()
	{
		_currentResultIndex--;
		if (_currentResultIndex < 0)
		{
			_currentResultIndex = _searchResults.Count - 1;
		}
		UpdateSearchVisuals();
	}

	private void OnNextButtonClicked()
	{
		_currentResultIndex++;
		if (_currentResultIndex == _searchResults.Count)
		{
			_currentResultIndex = 0;
		}
		UpdateSearchVisuals();
	}

	private void HandleOnLanguageChanged()
	{
		VersionText.text = string.Format(Bootstrap.GetVersionLoc(), RuntimeBuildInfo.UserVersion);
	}

	private void UpdateSearchVisuals()
	{
		int num = _searchResults?.Count ?? 0;
		int num2 = ((num > 0) ? (_currentResultIndex + 1) : 0);
		NumResultsText.text = $"{num2}/{num}";
		if (num == 0)
		{
			ChangeLogText.text = _changeLog;
			return;
		}
		Regex regex = new Regex(_searchString, RegexOptions.IgnoreCase);
		int resultSearchIndex = -1;
		int resultStringIndex = -1;
		ChangeLogText.text = regex.Replace(_changeLog, (Match match) =>
		{
			int num4 = resultSearchIndex + 1;
			resultSearchIndex = num4;
			if (resultSearchIndex == _currentResultIndex)
			{
				resultStringIndex = match.Index;
				return "<color=orange>" + match.Value + "</color>";
			}
			return "<color=yellow>" + match.Value + "</color>";
		});
		TMP_CharacterInfo tMP_CharacterInfo = ChangeLogText.textInfo.characterInfo[resultStringIndex];
		float t = Mathf.Abs((tMP_CharacterInfo.topLeft.y + (tMP_CharacterInfo.bottomLeft.y - tMP_CharacterInfo.topLeft.y)) / ChangeLogScrollRect.content.rect.height);
		float num3 = ChangeLogScrollRect.viewport.rect.height / ChangeLogScrollRect.content.rect.height / 2f;
		t = Mathf.Lerp(0f - num3, 1f + num3, t);
		ChangeLogScrollRect.verticalNormalizedPosition = Mathf.Clamp01(1f - t);
	}
}
