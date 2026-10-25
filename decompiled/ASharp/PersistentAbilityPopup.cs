using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class PersistentAbilityPopup : SlotPopup
{
	[SerializeField]
	public UIActiveAbility activeAbility;

	[SerializeField]
	private Image background;

	[SerializeField]
	private GameObject cancelationFooter;

	[SerializeField]
	private Sprite defaultBackground;

	[SerializeField]
	private Sprite footerBackground;

	[SerializeField]
	private TextLocalizedListener footerText;

	[SerializeField]
	private Color defaultFooterTextColor;

	public void Init(CActiveBonus activeBonus, CActor actor, Transform holder, int instances)
	{
		if (!(actor is CEnemyActor))
		{
			background.sprite = footerBackground;
			if (activeBonus.Ability.ActiveBonusData.CannotCancel)
			{
				footerText.SetTextKey("GUI_CANNOT_CANCEL_ACTIVE_BONUS");
				footerText.Text.color = UIInfoTools.Instance.warningColor;
			}
			else
			{
				footerText.SetTextKey("GUI_CANCEL_ACTIVE_BONUS");
				footerText.Text.color = defaultFooterTextColor;
			}
			cancelationFooter.SetActive(value: true);
		}
		else
		{
			cancelationFooter.SetActive(value: true);
			background.sprite = defaultBackground;
		}
		Init(holder);
		activeAbility.Initialize(activeBonus, actor, instances);
	}

	public void OnActiveBonusTriggered()
	{
		activeAbility.Tracker.OnActiveBonusTriggered();
	}
}
