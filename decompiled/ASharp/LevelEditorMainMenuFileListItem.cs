using System.IO;
using ScenarioRuleLibrary.CustomLevels;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LevelEditorMainMenuFileListItem : MonoBehaviour
{
	public TextMeshProUGUI FileNameLabel;

	public TextMeshProUGUI DateModifiedLabel;

	public TextMeshProUGUI FileTypeLabel;

	public Color BGColor1;

	public Color BGColor2;

	public Image ItemBG;

	public UnityAction<LevelEditorMainMenuFileListItem> OnPressAction;

	public FileInfo DataFileInfo { get; private set; }

	public CCustomLevelData LevelData { get; private set; }

	public int Index { get; private set; }

	public void InitForFileInfo(FileInfo fileInfo, int index, UnityAction<LevelEditorMainMenuFileListItem> onItemPressedAction = null)
	{
		DataFileInfo = fileInfo;
		Index = index;
		ItemBG.color = ((Index % 2 == 0) ? BGColor1 : BGColor2);
		FileNameLabel.text = Path.GetFileNameWithoutExtension(DataFileInfo.Name);
		DateModifiedLabel.text = DataFileInfo.LastWriteTime.ToString("dd/MM/yyyy HH:mm");
		FileTypeLabel.text = DataFileInfo.Extension;
		OnPressAction = onItemPressedAction;
	}

	public async void OnListItemPressed()
	{
		LevelData = await SaveData.Instance.LevelEditorDataManager.GetLevelDataForFile(DataFileInfo);
		OnPressAction?.Invoke(this);
	}
}
