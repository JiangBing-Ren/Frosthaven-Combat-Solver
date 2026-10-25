using GLOOM;
using MapRuleLibrary.Party;
using TMPro;
using UnityEngine;

public class UIPartyStats : MonoBehaviour
{
	[SerializeField]
	protected TextMeshProUGUI levelText;

	protected CMapParty mapParty;

	public virtual void Setup(CMapParty mapParty)
	{
		this.mapParty = mapParty;
		UpdateLevel();
		gameObject.SetActive(value: true);
	}

	public virtual void UpdateLevel()
	{
		levelText.text = string.Format("{0} {1}", LocalizationManager.GetTranslation("GUI_LEVEL"), mapParty.PartyLevel);
	}
}
