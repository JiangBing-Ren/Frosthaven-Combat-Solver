using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class GenericModalViewModule : FHMapViewModule
{
	[Serializable]
	public class SizeSettings
	{
		public int EligibleCharacterCount;

		public int EligibleLinesCount;

		public Sprite Background;

		public Rect PreferredSize;
	}

	public struct Params
	{
		public string TitleLocalised;

		public string DescriptionLocalised;

		public List<UIMultiplayerUser.Data> SuggestingUsers;

		public bool ShowCancel;

		public Action<bool> OnResult;

		public string ConfirmButton;

		public string CancelButton;

		public bool ReadyCheckPassed;
	}

	[Header("Components")]
	[SerializeField]
	private Image Background;

	[SerializeField]
	private GameObject TitleContainer;

	[SerializeField]
	private TMP_Text Title;

	[SerializeField]
	private TMP_Text Description;

	[SerializeField]
	private LayoutElement ContentLayoutElement;

	[SerializeField]
	private FHButton OkButton;

	[SerializeField]
	private FHButton CancelButton;

	[Header("Settings")]
	[SerializeField]
	private SizeSettings[] Settings;

	[Header("Multiplayer")]
	[SerializeField]
	private UIUsersListContainer SyncUserList;

	public void Show(Params @params)
	{
		Title.text = @params.TitleLocalised;
		Description.text = @params.DescriptionLocalised;
		OkButton.onClick.RemoveAllListeners();
		OkButton.onClick.AddListener(() =>
		{
			@params.OnResult?.Invoke(obj: true);
		});
		if (!string.IsNullOrEmpty(@params.ConfirmButton))
		{
			OkButton.SetNonLocalizedText(@params.ConfirmButton);
		}
		else
		{
			OkButton.SetTextKey("GUI_OK");
		}
		OkButton.interactable = !@params.ReadyCheckPassed;
		CancelButton.gameObject.SetActive(@params.ShowCancel);
		if (!string.IsNullOrEmpty(@params.CancelButton))
		{
			CancelButton.SetNonLocalizedText(@params.CancelButton);
		}
		else
		{
			CancelButton.SetTextKey("GUI_CANCEL");
		}
		if (@params.ShowCancel)
		{
			CancelButton.onClick.RemoveAllListeners();
			CancelButton.onClick.AddListener(() =>
			{
				@params.OnResult?.Invoke(obj: false);
				SetActive(active: false);
			});
		}
		if (TitleContainer != null)
		{
			TitleContainer.SetActive(!string.IsNullOrEmpty(@params.DescriptionLocalised));
		}
		SyncUserList.Init(@params.SuggestingUsers);
		ApplySkin();
		SetActive(active: true);
	}

	private void ApplySkin()
	{
		if (Settings != null && Settings.Length != 0)
		{
			TMP_TextInfo textInfo = Description.GetTextInfo(Description.text);
			int charCount = textInfo.characterCount;
			int lineCount = textInfo.lineCount;
			SizeSettings sizeSettings = Settings.FirstOrDefault((SizeSettings t) => t.EligibleCharacterCount >= charCount && t.EligibleLinesCount >= lineCount);
			if (sizeSettings != null)
			{
				Background.sprite = sizeSettings.Background;
				ContentLayoutElement.preferredWidth = sizeSettings.PreferredSize.width;
				ContentLayoutElement.preferredHeight = sizeSettings.PreferredSize.height;
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
