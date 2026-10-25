using System;
using FHGUI.DTO;
using GLOOM;
using Libs.GameConsole;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIScenarioInfoSummary : MonoBehaviour
{
	[ConsoleVariable(Alias = "dbg_show_scenario_id", Persistent = true)]
	private static bool ShowScenarioID_kvar;

	[Header("Components")]
	public Image ImageIcon;

	public Image ImageIllustration;

	public Image ImageColorBanner;

	public Image ImageRegionIcon;

	public GameObject ContainerRegionIcon;

	public TMP_Text TextId;

	public TMP_Text TextLevel;

	public TMP_Text TextTitle;

	public TMP_Text TextDescription;

	public FHStacksCounter DifficultyMarks;

	public GameObject Completed;

	public Button Button;

	[Header("Settings")]
	[SerializeField]
	private Color TextColorNormal = new Color(1f, 1f, 1f);

	[SerializeField]
	private Color TextColorCompleted = new Color(1f, 1f, 1f);

	public UITextTooltipTarget RegionIconTooltip;

	private Action _onClick;

	private void Awake()
	{
		if (Button != null)
		{
			Button.onClick.AddListener(() =>
			{
				_onClick?.Invoke();
			});
		}
	}

	public void Set(ScenarioInfoDTO dto, Action onClick)
	{
		Set(dto.Icon, dto.Illustration, dto.RegionIcon, dto.Color, dto.Id.ToString(), dto.Level, dto.IsCompleted, dto.NameLiteral, dto.DescriptionLiteral, dto.LocationNameKey, dto.Difficulty, onClick);
	}

	public void Set(Sprite icon, Sprite illustration, Sprite regionIcon, Color color, string id, int level, bool completed, string titleLiteral, string descriptionLiteral, string locationName, uint difficulty, Action onClick)
	{
		if (ImageIcon != null)
		{
			ImageIcon.sprite = icon;
		}
		if (ImageRegionIcon != null)
		{
			ImageRegionIcon.sprite = regionIcon;
		}
		if (ContainerRegionIcon != null)
		{
			ContainerRegionIcon.gameObject.SetActive(regionIcon != null);
		}
		if (TextId != null)
		{
			TextId.enabled = ShowScenarioID_kvar;
			if (TextId.enabled)
			{
				TextId.text = id;
			}
		}
		if (TextLevel != null)
		{
			TextLevel.text = $"{level} lvl";
		}
		if (TextTitle != null)
		{
			TextTitle.text = titleLiteral;
			TextTitle.color = (completed ? TextColorCompleted : TextColorNormal);
		}
		if (TextDescription != null)
		{
			TextDescription.text = descriptionLiteral;
		}
		if (ImageIllustration != null)
		{
			ImageIllustration.overrideSprite = illustration;
		}
		if (ImageColorBanner != null)
		{
			ImageColorBanner.color = color;
		}
		if ((bool)Completed)
		{
			Completed.gameObject.SetActive(completed);
		}
		if ((bool)RegionIconTooltip)
		{
			RegionIconTooltip.SetText((locationName != null) ? LocalizationManager.GetTranslation(locationName, fixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: true) : string.Empty);
		}
		if ((bool)DifficultyMarks)
		{
			DifficultyMarks.Init((int)difficulty, 3);
		}
		_onClick = onClick;
	}

	public void Clear()
	{
		Set(null, null, null, Color.white, null, 0, completed: false, null, null, null, 0u, null);
	}
}
