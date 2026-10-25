using UnityEngine;

namespace FHGUI.Elements;

public class UIBuildingEffectRowElement : MonoBehaviour
{
	[SerializeField]
	private TextLocalizedListener _text;

	public void Init(string effectText)
	{
		_text.SetTextKey(effectText);
	}
}
