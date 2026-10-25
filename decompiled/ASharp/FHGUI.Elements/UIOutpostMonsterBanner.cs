using FHGUI.DTO;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIOutpostMonsterBanner : MonoBehaviour
{
	[Header("Base Elements")]
	[SerializeField]
	private Image _bannerImage;

	[SerializeField]
	private Image _portraitImage;

	[SerializeField]
	private Image _portraitFullImage;

	[SerializeField]
	private Image _factionImage;

	[Space]
	[SerializeField]
	private Sprite _normalBannerSprite;

	[SerializeField]
	private Sprite _eliteBannerSprite;

	[SerializeField]
	private Sprite _allyBannerSprite;

	[Header("Tooltip")]
	[SerializeField]
	private UITextTooltipTarget _textTooltipTarget;

	public void Set(Sprite portrait, bool isFullPortrait, bool isElite, bool isAlly, bool isSecondFaction, string tooltipLocalised)
	{
		bool flag = portrait == null;
		_portraitImage.overrideSprite = portrait;
		_portraitImage.enabled = !isFullPortrait && !flag;
		_portraitFullImage.overrideSprite = portrait;
		_portraitFullImage.enabled = isFullPortrait && !flag;
		_bannerImage.sprite = (isAlly ? _allyBannerSprite : _normalBannerSprite);
		_bannerImage.enabled = !flag;
		_factionImage.enabled = isSecondFaction;
		if (_textTooltipTarget != null)
		{
			_textTooltipTarget.SetText(tooltipLocalised);
		}
	}

	public void Set(MonsterDataDto dto)
	{
		Set(dto.Portrait, dto.IsFullPortrait, dto.IsElite, dto.IsAlly, dto.IsSecondFaction, dto.NameLocalised);
	}
}
