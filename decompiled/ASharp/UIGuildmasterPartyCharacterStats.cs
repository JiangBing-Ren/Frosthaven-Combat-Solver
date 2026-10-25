using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIGuildmasterPartyCharacterStats : UICustomPartyCharacterStats
{
	[SerializeField]
	protected Button levelUpButton;

	[SerializeField]
	protected GameObject cardsButton;

	public UnityEvent OnClickLevelUp => levelUpButton.onClick;

	public override void EnableUIForLevelingUp()
	{
		if (character != null)
		{
			base.EnableUIForLevelingUp();
			levelUpButton.gameObject.SetActive(value: true);
			cardsButton.SetActive(value: false);
		}
		else
		{
			Log.GUIInfo("Trying to activate Level up button but characterData is null.");
		}
	}

	public override void DisableUIForLevelingUp()
	{
		base.DisableUIForLevelingUp();
		levelUpButton.gameObject.SetActive(value: false);
		cardsButton.SetActive(value: true);
	}
}
