using GLOOM;
using TMPro;
using UnityEngine;

namespace FHGUI.Modules;

public class TextModule : FHMapViewModule
{
	[SerializeField]
	private TMP_Text m_Text;

	protected override bool PlaySoundOnEnable => false;

	public void SetText(string locKey)
	{
		SetTextLiteral(LocalizationManager.GetTranslation(locKey));
	}

	public void SetTextLiteral(string literalText)
	{
		m_Text.text = literalText;
	}
}
