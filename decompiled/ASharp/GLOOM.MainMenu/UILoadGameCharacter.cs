using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GLOOM.MainMenu;

public class UILoadGameCharacter : MonoBehaviour
{
	[SerializeField]
	private Image portrait;

	[SerializeField]
	private GameObject iconContainer;

	[SerializeField]
	private GameObject informationContainer;

	[SerializeField]
	private GameObject emptyContainer;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private TextMeshProUGUI levelText;

	[SerializeField]
	private TextMeshProUGUI goldText;

	[SerializeField]
	private string goldFormat = "<sprite name=\"Gold_White\">{0}";

	public void ShowCharacter(ECharacter character, int charLevel, string characterName = null, int? gold = null, bool isValid = true)
	{
		portrait.material = (isValid ? null : UIInfoTools.Instance.disabledGrayscaleMaterial);
		portrait.enabled = true;
		portrait.sprite = UIInfoTools.GetHeroPortrait(CrossReference.ECharacterModelToClassId(character));
		levelText.text = charLevel.ToString();
		if (characterName == null)
		{
			nameText.gameObject.SetActive(value: false);
		}
		else
		{
			nameText.text = characterName;
			nameText.gameObject.SetActive(value: true);
		}
		if (!gold.HasValue)
		{
			goldText.gameObject.SetActive(value: false);
		}
		else
		{
			goldText.text = string.Format(goldFormat, gold.Value);
			goldText.gameObject.SetActive(value: true);
		}
		iconContainer.SetActive(value: true);
		informationContainer.SetActive(value: true);
		emptyContainer.SetActive(value: false);
	}

	public void ShowEmpty()
	{
		portrait.enabled = false;
		informationContainer.SetActive(value: false);
		iconContainer.SetActive(value: false);
		emptyContainer.SetActive(value: true);
	}

	public void ShowMissing(DLCRegistry.EDLCKey dlc)
	{
		portrait.enabled = true;
		informationContainer.SetActive(value: false);
		portrait.sprite = UIInfoTools.Instance.GetMissingCharacterDLC(dlc);
	}
}
