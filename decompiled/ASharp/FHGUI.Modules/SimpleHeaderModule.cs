using I2.Loc;
using TMPro;
using UnityEngine;

namespace FHGUI.Modules;

public class SimpleHeaderModule : FHMapViewModule
{
	[SerializeField]
	private Localize _title;

	[Header("States")]
	[SerializeField]
	private GameObject _DefaultVisualState;

	[SerializeField]
	private GameObject _ManagePartyVisualState;

	protected override bool PlaySoundOnEnable => false;

	public void SetTitle(string title)
	{
		_title.SetTerm(title);
	}

	public void SetTitleNotLocalized(string title)
	{
		_title.GetComponent<TMP_Text>().text = title;
	}

	public void SetFunctionality(CharacterDetailsScreenModule.Functionality functionality)
	{
		_ManagePartyVisualState.SetActive(value: false);
		_DefaultVisualState.SetActive(functionality == CharacterDetailsScreenModule.Functionality.None);
	}
}
