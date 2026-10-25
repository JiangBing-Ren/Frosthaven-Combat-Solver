using UnityEngine;

namespace FHGUI.Configs;

[CreateAssetMenu(menuName = "UI Config/InputActionConfigUI", fileName = "InputActionConfigUI")]
public class InputActionConfigUI : ScriptableObject
{
	[PingInHierarchy]
	public FHInputEventElement.UserInteractionType Action;

	[ConfigIcon(48, true)]
	public Sprite Icon;
}
