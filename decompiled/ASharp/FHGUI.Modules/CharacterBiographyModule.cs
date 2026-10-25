using FHGUI.DTO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class CharacterBiographyModule : FHMapViewModule
{
	[SerializeField]
	private Image _classIcon;

	[SerializeField]
	private TMP_Text _biography;

	private ClassInfoDTO _classInfo;

	protected override bool PlaySoundOnEnable => false;

	public void Init(ClassInfoDTO classInfo)
	{
		_classInfo = classInfo;
		if (_classIcon != null)
		{
			_classIcon.sprite = classInfo.Icon;
		}
		UIHelperUtil.SetTextKey(_biography, _classInfo.Description);
	}

	public void Toggle()
	{
		if (isActiveAndEnabled)
		{
			Hide();
		}
		else
		{
			Show();
		}
	}

	public void Show()
	{
		if (_classInfo != null)
		{
			gameObject.SetActive(value: true);
			Init(_classInfo);
		}
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
	}
}
