using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class ImmunityPopup : SlotPopup
{
	[SerializeField]
	private UIMonsterImmunity monsterImmunity;

	public void Init(string immunity, Transform holder)
	{
		Init(holder);
		monsterImmunity.Initialize(immunity);
	}
}
