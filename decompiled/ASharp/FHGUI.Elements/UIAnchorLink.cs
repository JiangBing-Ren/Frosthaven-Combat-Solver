using UnityEngine;

namespace FHGUI.Elements;

public class UIAnchorLink : MonoBehaviour
{
	[SerializeField]
	private string m_ID;

	[SerializeField]
	private RectTransform m_Target;

	public RectTransform Content => m_Target ?? (transform as RectTransform);

	public string ID => m_ID;
}
