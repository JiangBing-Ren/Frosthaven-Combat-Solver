using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class StatusEffectPopup : SlotPopup
{
	[SerializeField]
	private UIStatusEffect statusEffect;

	public void Init(string title, string description, Sprite image, Transform holder)
	{
		Init(holder);
		statusEffect.Initialize(title, image, UIInfoTools.DefInstance.basicTextColor, description);
	}
}
