using GLOOM;
using TMPro;
using UnityEngine;

public class UIOptionEntry : LocalizedListener
{
	[SerializeField]
	private TMP_Text description;

	[SerializeField]
	private string m_DescriptionLoc;

	[SerializeField]
	private bool m_IsEnabled;

	[SerializeField]
	private Color m_DisabledColorText;

	private void Awake()
	{
		if (m_DescriptionLoc.IsNOTNullOrEmpty())
		{
			Init(m_DescriptionLoc);
		}
	}

	public void Init(string descriptionLoc)
	{
		m_DescriptionLoc = descriptionLoc;
		if (string.IsNullOrEmpty(descriptionLoc))
		{
			Enable(enable: false);
		}
		else
		{
			Enable(m_IsEnabled);
		}
	}

	public void Enable(bool enable)
	{
		m_IsEnabled = enable;
		if (string.IsNullOrEmpty(m_DescriptionLoc))
		{
			description.text = string.Empty;
			return;
		}
		string translation = LocalizationManager.GetTranslation(m_DescriptionLoc);
		int num = translation.IndexOf(":");
		if (num == -1)
		{
			description.text = (enable ? translation : ("<color=#" + m_DisabledColorText.ToHex() + ">" + translation + "</color>"));
		}
		else if (enable)
		{
			description.text = "<color=#" + UIInfoTools.DefInstance.mainColor.ToHex() + ">" + translation.SubstringFromXToY(0, num) + "</color>" + translation.Substring(num);
		}
		else
		{
			description.text = "<color=#" + m_DisabledColorText.ToHex() + ">" + translation.SubstringFromXToY(0, num) + translation.Substring(num) + "</color>";
		}
	}

	protected override void OnLanguageChanged()
	{
		if (m_DescriptionLoc.IsNOTNullOrEmpty())
		{
			Enable(m_IsEnabled);
		}
	}
}
