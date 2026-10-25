using System.IO;
using GLOOM;
using ScenarioRuleLibrary.CustomLevels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelMessagePageUI : LocalizedListener
{
	[SerializeField]
	private TextMeshProUGUI information;

	[SerializeField]
	private Image image;

	private CLevelMessagePage page;

	public void Init(CLevelMessagePage page)
	{
		this.page = page;
		OnLanguageChanged();
		if (page.ImagePath.IsNOTNullOrEmpty())
		{
			string path = Path.Combine(Application.streamingAssetsPath, page.ImagePath + ".png");
			if (File.Exists(path))
			{
				byte[] data = File.ReadAllBytes(path);
				Texture2D texture2D = new Texture2D(2, 2);
				texture2D.LoadImage(data);
				Sprite sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
				image.sprite = sprite;
				image.gameObject.SetActive(value: true);
			}
		}
		else
		{
			image.gameObject.SetActive(value: false);
		}
	}

	protected override void OnLanguageChanged()
	{
		if (page != null)
		{
			information.text = ((Singleton<InputFHManager>.Instance.UsingGamepad && page.PageTextKeyController.IsNOTNullOrEmpty()) ? LocalizationManager.GetTranslation(page.PageTextKeyController) : LocalizationManager.GetTranslation(page.PageTextKey));
		}
		information.text = information.text.Replace("\\n", "\n");
	}

	public void RefreshText()
	{
		OnLanguageChanged();
	}
}
