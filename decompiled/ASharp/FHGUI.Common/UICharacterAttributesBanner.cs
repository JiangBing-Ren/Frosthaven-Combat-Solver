using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Common;

public class UICharacterAttributesBanner : MonoBehaviour
{
	[SerializeField]
	private Image _flying;

	[SerializeField]
	private Image _invulnerable;

	[SerializeField]
	private Image _untargetable;

	[SerializeField]
	private Image _summon;

	public void Init(UIGeneralInfoUtils.CharacterTokenAttributes attributes, bool isSummon = false)
	{
		_flying.gameObject.SetActive(attributes.IsFlying);
		_invulnerable.gameObject.SetActive(attributes.Invulnerable);
		_untargetable.gameObject.SetActive(attributes.Untargetable);
		_summon.gameObject.SetActive(isSummon);
	}
}
