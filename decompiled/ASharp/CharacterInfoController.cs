using FHGUI.DTO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInfoController : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _gold;

	[SerializeField]
	private TMP_Text _level;

	[SerializeField]
	private TMP_Text _xp;

	[SerializeField]
	private GameObject _xpBar;

	[SerializeField]
	private Image _xpBarFill;

	[SerializeField]
	private TMP_Text _name;

	public void Init(CharacterDataDTO characterData)
	{
		gameObject.SetActive(value: true);
		_gold.text = characterData.Gold.ToString();
		_level.text = characterData.Level.ToString();
		_xp.text = characterData.Experience.ToDisplayString();
		_gold.text = characterData.Gold.ToString();
		_xpBarFill.fillAmount = characterData.Experience.Progress;
		_name.text = characterData.Name;
	}

	public void Init(PartySlotDTO characterData)
	{
		if (!characterData.ShowValues)
		{
			gameObject.SetActive(value: false);
			return;
		}
		gameObject.SetActive(value: true);
		_gold.text = characterData.Gold.ToString();
		_level.text = characterData.Level.ToString();
		_xp.text = characterData.Experience.ToDisplayString();
		_gold.text = characterData.Gold.ToString();
		_xpBarFill.fillAmount = characterData.Experience.Progress;
		_name.text = characterData.Name;
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
	}
}
